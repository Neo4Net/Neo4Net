using System;
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
namespace Org.Neo4j.Kernel.Api.Index
{
	using After = org.junit.After;
	using Assert = org.junit.Assert;
	using Before = org.junit.Before;


	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ReporterFactories = Org.Neo4j.Kernel.Impl.Annotations.ReporterFactories;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using SimpleNodeValueClient = Org.Neo4j.Storageengine.Api.schema.SimpleNodeValueClient;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueCategory = Org.Neo4j.Values.Storable.ValueCategory;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using ValueType = Org.Neo4j.Values.Storable.ValueType;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;

	public abstract class IndexAccessorCompatibility : IndexProviderCompatibilityTestSuite.Compatibility
	{
		 protected internal IndexAccessor Accessor;
		 // This map is for spatial values, so that the #query method can lookup the values for the results and filter properly
		 private IDictionary<long, Value[]> _committedValues = new Dictionary<long, Value[]>();

		 public IndexAccessorCompatibility( IndexProviderCompatibilityTestSuite testSuite, IndexDescriptor descriptor ) : base( testSuite, descriptor )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  IndexSamplingConfig indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );
			  IndexPopulator populator = IndexProvider.getPopulator( Descriptor, indexSamplingConfig, heapBufferFactory( 1024 ) );
			  populator.Create();
			  populator.Close( true );
			  Accessor = IndexProvider.getOnlineAccessor( Descriptor, indexSamplingConfig );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  try
			  {
					Accessor.consistencyCheck( ReporterFactories.throwingReporterFactory() );
			  }
			  finally
			  {
					Accessor.drop();
					Accessor.Dispose();
			  }
		 }

		 internal virtual ValueType[] RandomSetOfSupportedTypes()
		 {
			  ValueType[] supportedTypes = TestSuite.supportedValueTypes();
			  return Random.randomValues().selection(supportedTypes, 2, supportedTypes.Length, false);
		 }

		 internal virtual ValueType[] RandomSetOfSupportedAndSortableTypes()
		 {
			  ValueType[] types = TestSuite.supportedValueTypes();
			  types = RemoveSpatialTypes( types ); // <- don't use spatial values
			  types = RandomValues.excluding( types, ValueType.STRING, ValueType.STRING_ARRAY ); // <- don't use strings outside of BMP
			  types = Random.randomValues().selection(types, 2, types.Length, false);
			  return types;
		 }

		 private ValueType[] RemoveSpatialTypes( ValueType[] types )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return java.util.types.Where( t => !t.name().contains("POINT") ).ToArray(ValueType[]::new);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected java.util.List<long> query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws Exception
		 protected internal virtual IList<long> Query( params IndexQuery[] predicates )
		 {
			  using ( IndexReader reader = Accessor.newReader(), )
			  {
					SimpleNodeValueClient nodeValueClient = new SimpleNodeValueClient();
					reader.Query( nodeValueClient, IndexOrder.NONE, false, predicates );
					IList<long> list = new LinkedList<long>();
					while ( nodeValueClient.Next() )
					{
						 long entityId = nodeValueClient.Reference;
						 if ( PassesFilter( entityId, predicates ) )
						 {
							  list.Add( entityId );
						 }
					}
					list.Sort();
					return list;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected AutoCloseable query(org.neo4j.storageengine.api.schema.SimpleNodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder order, org.neo4j.internal.kernel.api.IndexQuery... predicates) throws Exception
		 protected internal virtual AutoCloseable Query( SimpleNodeValueClient client, IndexOrder order, params IndexQuery[] predicates )
		 {
			  IndexReader reader = Accessor.newReader();
			  reader.Query( client, order, false, predicates );
			  return reader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<long> assertInOrder(org.neo4j.internal.kernel.api.IndexOrder order, org.neo4j.internal.kernel.api.IndexQuery... predicates) throws Exception
		 internal virtual IList<long> AssertInOrder( IndexOrder order, params IndexQuery[] predicates )
		 {
			  IList<long> actualIds;
			  if ( order == IndexOrder.NONE )
			  {
					actualIds = Query( predicates );
			  }
			  else
			  {
					SimpleNodeValueClient client = new SimpleNodeValueClient();
					using ( AutoCloseable ignore = Query( client, order, predicates ) )
					{
						 actualIds = AssertClientReturnValuesInOrder( client, order );
					}
			  }
			  return actualIds;
		 }

		 internal virtual IList<long> AssertClientReturnValuesInOrder( SimpleNodeValueClient client, IndexOrder order )
		 {
			  IList<long> seenIds = new List<long>();
			  Value[] prevValues = null;
			  Value[] values;
			  int count = 0;
			  while ( client.Next() )
			  {
					count++;
					seenIds.Add( client.Reference );
					values = client.Values;
					if ( order == IndexOrder.ASCENDING )
					{
						 AssertLessThanOrEqualTo( prevValues, values );
					}
					else if ( order == IndexOrder.DESCENDING )
					{
						 AssertLessThanOrEqualTo( values, prevValues );
					}
					else
					{
						 Assert.fail( "Unexpected order " + order );
					}
					prevValues = values;
			  }
			  return seenIds;
		 }

		 internal virtual IndexOrder[] OrderCapability( params IndexQuery[] predicates )
		 {
			  ValueCategory[] categories = new ValueCategory[predicates.Length];
			  for ( int i = 0; i < predicates.Length; i++ )
			  {
					categories[i] = predicates[i].ValueGroup().category();
			  }
			  return IndexProvider.getCapability( Descriptor ).orderCapability( categories );
		 }

		 private void AssertLessThanOrEqualTo( Value[] o1, Value[] o2 )
		 {
			  if ( o1 == null || o2 == null )
			  {
					return;
			  }
			  int length = Math.Min( o1.Length, o2.Length );
			  for ( int i = 0; i < length; i++ )
			  {
					int compare = Values.COMPARATOR.Compare( o1[i], o2[i] );
					assertThat( "expected less than or equal to but was " + Arrays.ToString( o1 ) + " and " + Arrays.ToString( o2 ), compare, lessThanOrEqualTo( 0 ) );
					if ( compare != 0 )
					{
						 return;
					}
			  }
		 }

		 /// <summary>
		 /// Run the Value[] from a particular entityId through the list of IndexQuery[] predicates to see if they all accept the value.
		 /// </summary>
		 private bool PassesFilter( long entityId, IndexQuery[] predicates )
		 {
			  if ( predicates.Length == 1 && predicates[0] is IndexQuery.ExistsPredicate )
			  {
					return true;
			  }

			  Value[] values = _committedValues[entityId];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					IndexQuery predicate = predicates[i];
					if ( predicate.ValueGroup() == ValueGroup.GEOMETRY || predicate.ValueGroup() == ValueGroup.GEOMETRY_ARRAY || (predicate.ValueGroup() == ValueGroup.NUMBER && !TestSuite.supportFullValuePrecisionForNumbers()) )
					{
						 if ( !predicates[i].AcceptsValue( values[i] ) )
						 {
							  return false;
						 }
					}
					// else there's no functional need to let values, other than those of GEOMETRY type, to pass through the IndexQuery filtering
					// avoiding this filtering will have testing be more strict in what index readers returns.
			  }
			  return true;
		 }

		 /// <summary>
		 /// Commit these updates to the index. Also store the values, which currently are stored for all types except geometry,
		 /// so therefore it's done explicitly here so that we can filter on them later.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateAndCommit(java.util.Collection<IndexEntryUpdate<?>> updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 internal virtual void UpdateAndCommit<T1>( ICollection<T1> updates )
		 {
			  using ( IndexUpdater updater = Accessor.newUpdater( IndexUpdateMode.ONLINE ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (IndexEntryUpdate<?> update : updates)
					foreach ( IndexEntryUpdate<object> update in updates )
					{
						 updater.Process( update );
						 switch ( update.UpdateMode() )
						 {
						 case ADDED:
						 case CHANGED:
							  _committedValues[update.EntityId] = update.Values();
							  break;
						 case REMOVED:
							  _committedValues.Remove( update.EntityId );
							  break;
						 default:
							  throw new System.ArgumentException( "Unknown update mode of " + update );
						 }
					}
			  }
		 }
	}

}