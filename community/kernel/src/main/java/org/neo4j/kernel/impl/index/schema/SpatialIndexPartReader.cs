﻿using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Cursor;
	using SpaceFillingCurve = Org.Neo4j.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using SpaceFillingCurveConfiguration = Org.Neo4j.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using ExactPredicate = Org.Neo4j.@internal.Kernel.Api.IndexQuery.ExactPredicate;
	using GeometryRangePredicate = Org.Neo4j.@internal.Kernel.Api.IndexQuery.GeometryRangePredicate;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using BridgingIndexProgressor = Org.Neo4j.Kernel.Impl.Api.schema.BridgingIndexProgressor;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

	public class SpatialIndexPartReader<VALUE> : NativeIndexReader<SpatialIndexKey, VALUE> where VALUE : NativeIndexValue
	{
		 private readonly SpatialLayout _spatial;
		 private readonly SpaceFillingCurveConfiguration _configuration;

		 internal SpatialIndexPartReader( GBPTree<SpatialIndexKey, VALUE> tree, IndexLayout<SpatialIndexKey, VALUE> layout, IndexDescriptor descriptor, SpaceFillingCurveConfiguration configuration ) : base( tree, layout, descriptor )
		 {
			  _spatial = ( SpatialLayout ) layout;
			  this._configuration = configuration;
		 }

		 internal override void ValidateQuery( IndexOrder indexOrder, IndexQuery[] predicates )
		 {
			  if ( predicates.Length != 1 )
			  {
					throw new System.NotSupportedException( "Spatial index doesn't handle composite queries" );
			  }

			  CapabilityValidator.ValidateQuery( SpatialIndexProvider.Capability, indexOrder, predicates );
		 }

		 internal override bool InitializeRangeForQuery( SpatialIndexKey treeKeyFrom, SpatialIndexKey treeKeyTo, IndexQuery[] predicates )
		 {
			  throw new System.NotSupportedException( "Cannot initialize 1D range in multidimensional spatial index reader" );
		 }

		 public override void Query( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] predicates )
		 {
			  // Spatial does not support providing values
			  if ( needsValues )
			  {
					throw new System.InvalidOperationException( "Spatial index does not support providing values" );
			  }

			  ValidateQuery( indexOrder, predicates );
			  IndexQuery predicate = predicates[0];

			  SpatialIndexKey treeKeyFrom = layout.newKey();
			  SpatialIndexKey treeKeyTo = layout.newKey();
			  InitializeKeys( treeKeyFrom, treeKeyTo );

			  switch ( predicate.Type() )
			  {
			  case exists:
					StartSeekForExists( treeKeyFrom, treeKeyTo, cursor, predicate );
					break;
			  case exact:
					StartSeekForExact( treeKeyFrom, treeKeyTo, cursor, ( ( IndexQuery.ExactPredicate ) predicate ).value(), predicate );
					break;
			  case range:
					IndexQuery.GeometryRangePredicate rangePredicate = ( IndexQuery.GeometryRangePredicate ) predicate;
					if ( !rangePredicate.Crs().Equals(_spatial.crs) )
					{
						 throw new System.ArgumentException( "IndexQuery on spatial index with mismatching CoordinateReferenceSystem: " + rangePredicate.Crs() + " != " + _spatial.crs );
					}
					StartSeekForRange( cursor, rangePredicate, predicates );
					break;
			  default:
					throw new System.ArgumentException( "IndexQuery of type " + predicate.Type() + " is not supported." );
			  }
		 }

		 public override void DistinctValues( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  // This is basically a version of the basic implementation, but with added consulting of the PropertyAccessor
			  // since these are lossy spatial values.
			  SpatialIndexKey lowest = layout.newKey();
			  lowest.initialize( long.MinValue );
			  lowest.initValuesAsLowest();
			  SpatialIndexKey highest = layout.newKey();
			  highest.initialize( long.MaxValue );
			  highest.initValuesAsHighest();
			  try
			  {
					RawCursor<Hit<SpatialIndexKey, VALUE>, IOException> seeker = tree.seek( lowest, highest );
					IComparer<SpatialIndexKey> comparator = new PropertyLookupFallbackComparator<SpatialIndexKey>( layout, propertyAccessor, descriptor.schema().PropertyId );
					NativeDistinctValuesProgressor<SpatialIndexKey, VALUE> progressor = new NativeDistinctValuesProgressorAnonymousInnerClass( this, seeker, client, openSeekers, layout, comparator, propertyAccessor );
					client.Initialize( descriptor, progressor, new IndexQuery[0], IndexOrder.NONE, false );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private class NativeDistinctValuesProgressorAnonymousInnerClass : NativeDistinctValuesProgressor<SpatialIndexKey, VALUE>
		 {
			 private readonly SpatialIndexPartReader<VALUE> _outerInstance;

			 private NodePropertyAccessor _propertyAccessor;

			 public NativeDistinctValuesProgressorAnonymousInnerClass( SpatialIndexPartReader<VALUE> outerInstance, RawCursor<Hit<SpatialIndexKey, VALUE>, IOException> seeker, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, UnknownType openSeekers, UnknownType layout, IComparer<SpatialIndexKey> comparator, NodePropertyAccessor propertyAccessor ) : base( seeker, client, openSeekers, layout, comparator )
			 {
				 this.outerInstance = outerInstance;
				 this._propertyAccessor = propertyAccessor;
			 }

			 internal override Value[] extractValues( SpatialIndexKey key )
			 {
				  try
				  {
						return new Value[]{ _propertyAccessor.getNodePropertyValue( key.EntityId, descriptor.schema().PropertyId ) };
				  }
				  catch ( EntityNotFoundException )
				  {
						// We couldn't get the value due to the entity not being there. Concurrently deleted?
						return null;
				  }
			 }
		 }

		 private void StartSeekForExists( SpatialIndexKey treeKeyFrom, SpatialIndexKey treeKeyTo, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, params IndexQuery[] predicates )
		 {
			  treeKeyFrom.InitValueAsLowest( ValueGroup.GEOMETRY );
			  treeKeyTo.InitValueAsHighest( ValueGroup.GEOMETRY );
			  StartSeekForInitializedRange( client, treeKeyFrom, treeKeyTo, predicates, IndexOrder.NONE, false, false );
		 }

		 private void StartSeekForExact( SpatialIndexKey treeKeyFrom, SpatialIndexKey treeKeyTo, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, Value value, params IndexQuery[] predicates )
		 {
			  treeKeyFrom.From( value );
			  treeKeyTo.From( value );
			  StartSeekForInitializedRange( client, treeKeyFrom, treeKeyTo, predicates, IndexOrder.NONE, false, false );
		 }

		 private void StartSeekForRange( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, IndexQuery.GeometryRangePredicate rangePredicate, IndexQuery[] query )
		 {
			  try
			  {
					BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( client, descriptor.schema().PropertyIds );
					client.Initialize( descriptor, multiProgressor, query, IndexOrder.NONE, false );
					SpaceFillingCurve curve = _spatial.SpaceFillingCurve;
					double[] from = rangePredicate.From() == null ? null : rangePredicate.From().coordinate();
					double[] to = rangePredicate.To() == null ? null : rangePredicate.To().coordinate();
					IList<SpaceFillingCurve.LongRange> ranges = curve.GetTilesIntersectingEnvelope( from, to, _configuration );
					foreach ( SpaceFillingCurve.LongRange range in ranges )
					{
						 SpatialIndexKey treeKeyFrom = layout.newKey();
						 SpatialIndexKey treeKeyTo = layout.newKey();
						 InitializeKeys( treeKeyFrom, treeKeyTo );
						 treeKeyFrom.FromDerivedValue( long.MinValue, range.Min );
						 treeKeyTo.FromDerivedValue( long.MaxValue, range.Max + 1 );
						 RawCursor<Hit<SpatialIndexKey, VALUE>, IOException> seeker = makeIndexSeeker( treeKeyFrom, treeKeyTo, IndexOrder.NONE );
						 IndexProgressor hitProgressor = new NativeHitIndexProgressor<>( seeker, client, openSeekers );
						 multiProgressor.Initialize( descriptor, hitProgressor, query, IndexOrder.NONE, false );
					}
			  }
			  catch ( System.ArgumentException )
			  {
					// Invalid query ranges will cause this state (eg. min>max)
					client.Initialize( descriptor, IndexProgressor.EMPTY, query, IndexOrder.NONE, false );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 internal override void StartSeekForInitializedRange( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, SpatialIndexKey treeKeyFrom, SpatialIndexKey treeKeyTo, IndexQuery[] query, IndexOrder indexOrder, bool needFilter, bool needsValues )
		 {
			  // Spatial does not support providing values
			  Debug.Assert( !needsValues );

			  if ( layout.compare( treeKeyFrom, treeKeyTo ) > 0 )
			  {
					client.Initialize( descriptor, IndexProgressor.EMPTY, query, IndexOrder.NONE, false );
					return;
			  }
			  try
			  {
					RawCursor<Hit<SpatialIndexKey, VALUE>, IOException> seeker = makeIndexSeeker( treeKeyFrom, treeKeyTo, indexOrder );
					IndexProgressor hitProgressor = new NativeHitIndexProgressor<>( seeker, client, openSeekers );
					client.Initialize( descriptor, hitProgressor, query, IndexOrder.NONE, false );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return false;
		 }

		 private void InitializeKeys( SpatialIndexKey treeKeyFrom, SpatialIndexKey treeKeyTo )
		 {
			  treeKeyFrom.initialize( long.MinValue );
			  treeKeyTo.initialize( long.MaxValue );
		 }
	}

}