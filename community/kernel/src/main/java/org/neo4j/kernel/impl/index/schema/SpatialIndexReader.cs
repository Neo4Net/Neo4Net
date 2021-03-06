﻿using System.Collections.Generic;

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

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Resource = Org.Neo4j.Graphdb.Resource;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using ExistsPredicate = Org.Neo4j.@internal.Kernel.Api.IndexQuery.ExistsPredicate;
	using BridgingIndexProgressor = Org.Neo4j.Kernel.Impl.Api.schema.BridgingIndexProgressor;
	using FusionIndexSampler = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.FusionIndexSampler;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexBase.forAll;

	internal class SpatialIndexReader : SpatialIndexCache<SpatialIndexPartReader<NativeIndexValue>>, IndexReader
	{
		 private readonly IndexDescriptor _descriptor;

		 internal SpatialIndexReader( IndexDescriptor descriptor, SpatialIndexAccessor accessor ) : base( new PartFactory( accessor ) )
		 {
			  this._descriptor = descriptor;
		 }

		 public override void Close()
		 {
			  forAll( Resource.close, this );
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  NativeIndexReader<SpatialIndexKey, NativeIndexValue> partReader = UncheckedSelect( ( ( PointValue ) propertyValues[0] ).CoordinateReferenceSystem );
			  return partReader == null ? 0L : partReader.CountIndexedNodes( nodeId, propertyKeyIds, propertyValues );
		 }

		 public override IndexSampler CreateSampler()
		 {
			  IList<IndexSampler> samplers = new List<IndexSampler>();
			  foreach ( SpatialIndexPartReader<NativeIndexValue> partReader in this )
			  {
					samplers.Add( partReader.createSampler() );
			  }
			  return new FusionIndexSampler( samplers );
		 }

		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  NodeValueIterator nodeValueIterator = new NodeValueIterator();
			  Query( nodeValueIterator, IndexOrder.NONE, nodeValueIterator.NeedsValues(), predicates );
			  return nodeValueIterator;
		 }

		 public override void Query( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] predicates )
		 {
			  // Spatial does not support providing values
			  if ( needsValues )
			  {
					throw new System.InvalidOperationException( "Spatial index does not support providing values" );
			  }

			  if ( predicates.Length != 1 )
			  {
					throw new System.ArgumentException( "Only single property spatial indexes are supported." );
			  }
			  IndexQuery predicate = predicates[0];
			  if ( predicate is IndexQuery.ExistsPredicate )
			  {
					LoadAll();
					BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( cursor, _descriptor.schema().PropertyIds );
					cursor.Initialize( _descriptor, multiProgressor, predicates, indexOrder, false );
					foreach ( NativeIndexReader<SpatialIndexKey, NativeIndexValue> reader in this )
					{
						 reader.Query( multiProgressor, indexOrder, false, predicates );
					}
			  }
			  else
			  {
					if ( ValidPredicate( predicate ) )
					{
						 CoordinateReferenceSystem crs;
						 if ( predicate is IndexQuery.ExactPredicate )
						 {
							  crs = ( ( PointValue )( ( IndexQuery.ExactPredicate ) predicate ).value() ).CoordinateReferenceSystem;
						 }
						 else if ( predicate is IndexQuery.GeometryRangePredicate )
						 {
							  crs = ( ( IndexQuery.GeometryRangePredicate ) predicate ).crs();
						 }
						 else
						 {
							  throw new System.ArgumentException( "Wrong type of predicate, couldn't get CoordinateReferenceSystem" );
						 }
						 SpatialIndexPartReader<NativeIndexValue> part = UncheckedSelect( crs );
						 if ( part != null )
						 {
							  part.Query( cursor, indexOrder, false, predicates );
						 }
						 else
						 {
							  cursor.Initialize( _descriptor, IndexProgressor.EMPTY, predicates, indexOrder, false );
						 }
					}
					else
					{
						 cursor.Initialize( _descriptor, IndexProgressor.EMPTY, predicates, indexOrder, false );
					}
			  }
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return false;
		 }

		 public override void DistinctValues( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  LoadAll();
			  BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( cursor, _descriptor.schema().PropertyIds );
			  cursor.Initialize( _descriptor, multiProgressor, new IndexQuery[0], IndexOrder.NONE, false );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (NativeIndexReader<?,NativeIndexValue> reader : this)
			  foreach ( NativeIndexReader<object, NativeIndexValue> reader in this )
			  {
					reader.DistinctValues( multiProgressor, propertyAccessor, needsValues );
			  }
		 }

		 private bool ValidPredicate( IndexQuery predicate )
		 {
			  return predicate is IndexQuery.ExactPredicate || predicate is IndexQuery.RangePredicate;
		 }

		 /// <summary>
		 /// To create TemporalIndexPartReaders on demand, the PartFactory maintains a reference to the parent TemporalIndexAccessor.
		 /// The creation of a part reader can then be delegated to the correct PartAccessor.
		 /// </summary>
		 internal class PartFactory : Factory<SpatialIndexPartReader<NativeIndexValue>>
		 {
			  internal readonly SpatialIndexAccessor Accessor;

			  internal PartFactory( SpatialIndexAccessor accessor )
			  {
					this.Accessor = accessor;
			  }

			  public override SpatialIndexPartReader<NativeIndexValue> NewSpatial( CoordinateReferenceSystem crs )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return Accessor.selectOrElse( crs, SpatialIndexAccessor.PartAccessor::newReader, null );
			  }
		 }
	}

}