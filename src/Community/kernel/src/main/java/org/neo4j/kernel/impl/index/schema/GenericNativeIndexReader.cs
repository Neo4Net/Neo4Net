using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using SpaceFillingCurveConfiguration = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using Neo4Net.Index.@internal.gbptree;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using ExactPredicate = Neo4Net.@internal.Kernel.Api.IndexQuery.ExactPredicate;
	using Neo4Net.@internal.Kernel.Api.IndexQuery;
	using StringPrefixPredicate = Neo4Net.@internal.Kernel.Api.IndexQuery.StringPrefixPredicate;
	using BridgingIndexProgressor = Neo4Net.Kernel.Impl.Api.schema.BridgingIndexProgressor;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.HIGH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.LOW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;

	internal class GenericNativeIndexReader : NativeIndexReader<GenericKey, NativeIndexValue>
	{
		 private readonly IndexSpecificSpaceFillingCurveSettingsCache _spaceFillingCurveSettings;
		 private readonly SpaceFillingCurveConfiguration _configuration;

		 internal GenericNativeIndexReader( GBPTree<GenericKey, NativeIndexValue> tree, IndexLayout<GenericKey, NativeIndexValue> layout, IndexDescriptor descriptor, IndexSpecificSpaceFillingCurveSettingsCache spaceFillingCurveSettings, SpaceFillingCurveConfiguration configuration ) : base( tree, layout, descriptor )
		 {
			  this._spaceFillingCurveSettings = spaceFillingCurveSettings;
			  this._configuration = configuration;
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  foreach ( IndexQuery predicate in predicates )
			  {
					ValueGroup valueGroup = predicate.ValueGroup();
					if ( valueGroup == ValueGroup.GEOMETRY_ARRAY || valueGroup == ValueGroup.GEOMETRY )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 internal override void ValidateQuery( IndexOrder indexOrder, IndexQuery[] predicates )
		 {
			  CapabilityValidator.ValidateQuery( GenericNativeIndexProvider.Capability, indexOrder, predicates );
		 }

		 public override void Query( Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  IndexQuery.GeometryRangePredicate geometryRangePredicate = GetGeometryRangePredicateIfAny( query );
			  if ( geometryRangePredicate != null )
			  {
					ValidateQuery( indexOrder, query );
					try
					{
						 // If there's a GeometryRangeQuery among the predicates then this query changes from a straight-forward: build from/to and seek...
						 // into a query that is split into multiple sub-queries. Predicates both before and after will have to be accompanied each sub-query.
						 BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( client, Descriptor.schema().PropertyIds );
						 client.Initialize( Descriptor, multiProgressor, query, indexOrder, needsValues );
						 double[] from = geometryRangePredicate.From() == null ? null : geometryRangePredicate.From().coordinate();
						 double[] to = geometryRangePredicate.To() == null ? null : geometryRangePredicate.To().coordinate();
						 CoordinateReferenceSystem crs = geometryRangePredicate.Crs();
						 SpaceFillingCurve curve = _spaceFillingCurveSettings.forCrs( crs, false );
						 IList<SpaceFillingCurve.LongRange> ranges = curve.GetTilesIntersectingEnvelope( from, to, _configuration );
						 foreach ( SpaceFillingCurve.LongRange range in ranges )
						 {
							  // Here's a sub-query that we'll have to do for this geometry range. Build this query from all predicates
							  // and when getting to the geometry range predicate that sparked these sub-query chenanigans, swap in this sub-query in its place.
							  GenericKey treeKeyFrom = Layout.newKey();
							  GenericKey treeKeyTo = Layout.newKey();
							  InitializeFromToKeys( treeKeyFrom, treeKeyTo );
							  bool needFiltering = InitializeRangeForGeometrySubQuery( treeKeyFrom, treeKeyTo, query, crs, range );
							  StartSeekForInitializedRange( multiProgressor, treeKeyFrom, treeKeyTo, query, indexOrder, needFiltering, needsValues );
						 }
					}
					catch ( System.ArgumentException )
					{
						 // Invalid query ranges will cause this state (eg. min>max)
						 client.Initialize( Descriptor, IndexProgressor.EMPTY, query, indexOrder, needsValues );
					}
			  }
			  else
			  {
					base.Query( client, indexOrder, needsValues, query );
			  }
		 }

		 /// <summary>
		 /// Initializes {@code treeKeyFrom} and {@code treeKeyTo} from the <seealso cref="IndexQuery query"/>.
		 /// Geometry range queries makes an otherwise straight-forward key construction complex in that a geometry range internally is performed
		 /// by executing multiple sub-range queries to the index. Each of those sub-range queries still needs to construct the full composite key -
		 /// in the case of a composite index. Therefore this method can be called either with null or non-null {@code crs} and {@code range} and
		 /// constructing a key when coming across a <seealso cref="IndexQuery.GeometryRangePredicate"/> will use the provided crs/range instead
		 /// of the predicate, where the specific range is one out of many sub-ranges calculated from the <seealso cref="IndexQuery.GeometryRangePredicate"/>
		 /// by the caller.
		 /// </summary>
		 /// <param name="treeKeyFrom"> the "from" key to construct from the query. </param>
		 /// <param name="treeKeyTo"> the "to" key to construct from the query. </param>
		 /// <param name="query"> the query to construct keys from to later send to <seealso cref="GBPTree"/> when reading. </param>
		 /// <param name="crs"> <seealso cref="CoordinateReferenceSystem"/> for the specific {@code range}, if range is specified too. </param>
		 /// <param name="range"> sub-range of a larger <seealso cref="IndexQuery.GeometryRangePredicate"/> to use instead of <seealso cref="IndexQuery.GeometryRangePredicate"/>
		 /// in the query. </param>
		 /// <returns> {@code true} if filtering is needed for the results from the reader, otherwise {@code false}. </returns>
		 private bool InitializeRangeForGeometrySubQuery( GenericKey treeKeyFrom, GenericKey treeKeyTo, IndexQuery[] query, CoordinateReferenceSystem crs, SpaceFillingCurve.LongRange range )
		 {
			  bool needsFiltering = false;
			  for ( int i = 0; i < query.Length; i++ )
			  {
					IndexQuery predicate = query[i];
					switch ( predicate.Type() )
					{
					case exists:
						 treeKeyFrom.InitValueAsLowest( i, ValueGroup.UNKNOWN );
						 treeKeyTo.InitValueAsHighest( i, ValueGroup.UNKNOWN );
						 break;
					case exact:
						 IndexQuery.ExactPredicate exactPredicate = ( IndexQuery.ExactPredicate ) predicate;
						 treeKeyFrom.InitFromValue( i, exactPredicate.Value(), NEUTRAL );
						 treeKeyTo.InitFromValue( i, exactPredicate.Value(), NEUTRAL );
						 break;
					case range:
						 if ( IsGeometryRangeQuery( predicate ) )
						 {
							  // Use the supplied SpaceFillingCurve range instead of the GeometryRangePredicate because at the time of calling this method
							  // the original geometry range have been split up into multiple sub-ranges and this invocation is for one of those sub-ranges.
							  // We can not take query inclusion / exclusion into consideration here because then we risk missing border values. Always use
							  // Inclusion.LOW / HIGH respectively and filter out points later on.
							  treeKeyFrom.StateSlot( i ).writePointDerived( crs, range.Min, LOW );
							  treeKeyTo.StateSlot( i ).writePointDerived( crs, range.Max + 1, HIGH );
						 }
						 else
						 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> rangePredicate = (org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?>) predicate;
							  IndexQuery.RangePredicate<object> rangePredicate = ( IndexQuery.RangePredicate<object> ) predicate;
							  InitFromForRange( i, rangePredicate, treeKeyFrom );
							  InitToForRange( i, rangePredicate, treeKeyTo );
						 }
						 break;
					case stringPrefix:
						 IndexQuery.StringPrefixPredicate prefixPredicate = ( IndexQuery.StringPrefixPredicate ) predicate;
						 treeKeyFrom.StateSlot( i ).initAsPrefixLow( prefixPredicate.Prefix() );
						 treeKeyTo.StateSlot( i ).initAsPrefixHigh( prefixPredicate.Prefix() );
						 break;
					case stringSuffix:
					case stringContains:
						 treeKeyFrom.InitValueAsLowest( i, ValueGroup.TEXT );
						 treeKeyTo.InitValueAsHighest( i, ValueGroup.TEXT );
						 needsFiltering = true;
						 break;
					default:
						 throw new System.ArgumentException( "IndexQuery of type " + predicate.Type() + " is not supported." );
					}
			  }
			  return needsFiltering;
		 }

		 internal override bool InitializeRangeForQuery( GenericKey treeKeyFrom, GenericKey treeKeyTo, IndexQuery[] query )
		 {
			  return InitializeRangeForGeometrySubQuery( treeKeyFrom, treeKeyTo, query, null, null );
		 }

		 private static void InitFromForRange<T1>( int stateSlot, IndexQuery.RangePredicate<T1> rangePredicate, GenericKey treeKeyFrom )
		 {
			  Value fromValue = rangePredicate.FromValue();
			  if ( fromValue == Values.NO_VALUE )
			  {
					treeKeyFrom.InitValueAsLowest( stateSlot, rangePredicate.ValueGroup() );
			  }
			  else
			  {
					treeKeyFrom.InitFromValue( stateSlot, fromValue, FromInclusion( rangePredicate ) );
					treeKeyFrom.CompareId = true;
			  }
		 }

		 private static void InitToForRange<T1>( int stateSlot, IndexQuery.RangePredicate<T1> rangePredicate, GenericKey treeKeyTo )
		 {
			  Value toValue = rangePredicate.ToValue();
			  if ( toValue == Values.NO_VALUE )
			  {
					treeKeyTo.InitValueAsHighest( stateSlot, rangePredicate.ValueGroup() );
			  }
			  else
			  {
					treeKeyTo.InitFromValue( stateSlot, toValue, ToInclusion( rangePredicate ) );
					treeKeyTo.CompareId = true;
			  }
		 }

		 private static NativeIndexKey.Inclusion FromInclusion<T1>( IndexQuery.RangePredicate<T1> rangePredicate )
		 {
			  return rangePredicate.FromInclusive() ? LOW : HIGH;
		 }

		 private static NativeIndexKey.Inclusion ToInclusion<T1>( IndexQuery.RangePredicate<T1> rangePredicate )
		 {
			  return rangePredicate.ToInclusive() ? HIGH : LOW;
		 }

		 private IndexQuery.GeometryRangePredicate GetGeometryRangePredicateIfAny( IndexQuery[] predicates )
		 {
			  foreach ( IndexQuery predicate in predicates )
			  {
					if ( IsGeometryRangeQuery( predicate ) )
					{
						 return ( IndexQuery.GeometryRangePredicate ) predicate;
					}
			  }
			  return null;
		 }

		 private bool IsGeometryRangeQuery( IndexQuery predicate )
		 {
			  return predicate is IndexQuery.GeometryRangePredicate;
		 }

	}

}