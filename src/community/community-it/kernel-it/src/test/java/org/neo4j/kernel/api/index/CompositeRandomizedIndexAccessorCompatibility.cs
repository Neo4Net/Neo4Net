using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Api.Index
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Assume = org.junit.Assume;
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using ValueType = Neo4Net.Values.Storable.ValueType;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.exact;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite that provides test cases for verifying" + " IndexProvider implementations. Each index provider that is to be tested by this suite" + " must create their own test class extending IndexProviderCompatibilityTestSuite." + " The @Ignore annotation doesn't prevent these tests to run, it rather removes some annoying" + " errors or warnings in some IDEs about test classes needing a public zero-arg constructor.") public class CompositeRandomizedIndexAccessorCompatibility extends IndexAccessorCompatibility
	public class CompositeRandomizedIndexAccessorCompatibility : IndexAccessorCompatibility
	{
		 public CompositeRandomizedIndexAccessorCompatibility( IndexProviderCompatibilityTestSuite testSuite, IndexDescriptor descriptor ) : base( testSuite, descriptor )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class Exact extends CompositeRandomizedIndexAccessorCompatibility
		 public class Exact : CompositeRandomizedIndexAccessorCompatibility
		 {
			  public Exact( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.forLabel( 1000, 100, 101, 102, 103 ) )
			  {
					// composite index of 4 properties
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExactMatchOnRandomCompositeValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestExactMatchOnRandomCompositeValues()
			  {
					// given
					ValueType[] types = RandomSetOfSupportedTypes();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
					IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
					ISet<ValueTuple> duplicateChecker = new HashSet<ValueTuple>();
					for ( long id = 0; id < 30_000; id++ )
					{
						 IndexEntryUpdate<SchemaDescriptor> update;
						 do
						 {
							  update = IndexQueryHelper.Add( id, Descriptor.schema(), Random.randomValues().nextValueOfTypes(types), Random.randomValues().nextValueOfTypes(types), Random.randomValues().nextValueOfTypes(types), Random.randomValues().nextValueOfTypes(types) );
						 } while ( !duplicateChecker.Add( ValueTuple.of( update.Values() ) ) );
						 updates.Add( update );
					}
					UpdateAndCommit( updates );

					// when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (IndexEntryUpdate<?> update : updates)
					foreach ( IndexEntryUpdate<object> update in updates )
					{
						 // then
						 IList<long> hits = Query( exact( 100, update.Values()[0] ), exact(101, update.Values()[1]), exact(102, update.Values()[2]), exact(103, update.Values()[3]) );
						 assertEquals( update + " " + hits.ToString(), 1, hits.Count );
						 assertThat( single( hits ), equalTo( update.EntityId ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class Range extends CompositeRandomizedIndexAccessorCompatibility
		 public class Range : CompositeRandomizedIndexAccessorCompatibility
		 {
			  public Range( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.forLabel( 1000, 100, 101 ) )
			  {
					// composite index of 2 properties
			  }

			  /// <summary>
			  /// All entries in composite index look like (booleanValue, randomValue ).
			  /// Range queries in composite only work if all predicates before it is exact.
			  /// We use boolean values for exact part so that we get some real ranges to work
			  /// on in second composite slot where the random values are.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRangeMatchOnRandomValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestRangeMatchOnRandomValues()
			  {
					Assume.assumeTrue( "Assume support for granular composite queries", TestSuite.supportsGranularCompositeQueries() );
					// given
					ValueType[] types = RandomSetOfSupportedAndSortableTypes();
					ISet<ValueTuple> uniqueValues = new HashSet<ValueTuple>();
					SortedSet<ValueAndId> sortedValues = new SortedSet<ValueAndId>( ( v1, v2 ) => ValueTuple.COMPARATOR.Compare( v1.value, v2.value ) );
					MutableLong nextId = new MutableLong();

					for ( int i = 0; i < 5; i++ )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
						 IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
						 if ( i == 0 )
						 {
							  // The initial batch of data can simply be additions
							  updates = GenerateUpdatesFromValues( GenerateValuesFromType( types, uniqueValues, 20_000 ), nextId );
							  sortedValues.addAll( updates.Select( u => new ValueAndId( ValueTuple.of( u.values() ), u.EntityId ) ).ToList() );
						 }
						 else
						 {
							  // Then do all sorts of updates
							  for ( int j = 0; j < 1_000; j++ )
							  {
									int type = Random.intBetween( 0, 2 );
									if ( type == 0 )
									{ // add
										 ValueTuple value = GenerateUniqueRandomValue( types, uniqueValues );
										 long id = nextId.AndIncrement;
										 sortedValues.Add( new ValueAndId( value, id ) );
										 updates.Add( IndexEntryUpdate.Add( id, Descriptor.schema(), value.Values ) );
									}
									else if ( type == 1 )
									{ // update
										 ValueAndId existing = Random.among( sortedValues.toArray( new ValueAndId[0] ) );
										 sortedValues.remove( existing );
										 ValueTuple newValue = GenerateUniqueRandomValue( types, uniqueValues );
										 uniqueValues.remove( existing.Value );
										 sortedValues.Add( new ValueAndId( newValue, existing.Id ) );
										 updates.Add( IndexEntryUpdate.Change( existing.Id, Descriptor.schema(), existing.Value.Values, newValue.Values ) );
									}
									else
									{ // remove
										 ValueAndId existing = Random.among( sortedValues.toArray( new ValueAndId[0] ) );
										 sortedValues.remove( existing );
										 uniqueValues.remove( existing.Value );
										 updates.Add( IndexEntryUpdate.Remove( existing.Id, Descriptor.schema(), existing.Value.Values ) );
									}
							  }
						 }
						 UpdateAndCommit( updates );
						 VerifyRandomRanges( types, sortedValues );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRandomRanges(org.Neo4Net.values.storable.ValueType[] types, java.util.TreeSet<ValueAndId> sortedValues) throws Exception
			  internal virtual void VerifyRandomRanges( ValueType[] types, SortedSet<ValueAndId> sortedValues )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 Value booleanValue = Random.randomValues().nextBooleanValue();
						 ValueType type = Random.among( types );
						 Value from = Random.randomValues().nextValueOfType(type);
						 Value to = Random.randomValues().nextValueOfType(type);
						 if ( Values.COMPARATOR.Compare( from, to ) > 0 )
						 {
							  Value tmp = from;
							  from = to;
							  to = tmp;
						 }
						 bool fromInclusive = Random.nextBoolean();
						 bool toInclusive = Random.nextBoolean();

						 // when
						 IList<long> expectedIds = expectedIds( sortedValues, booleanValue, from, to, fromInclusive, toInclusive );

						 // Depending on order capabilities we verify ids or order and ids.
						 IndexQuery[] predicates = new IndexQuery[]{ IndexQuery.exact( 100, booleanValue ), IndexQuery.range( 101, from, fromInclusive, to, toInclusive ) };
						 ValueCategory[] valueCategories = GetValueCategories( predicates );
						 IndexOrder[] indexOrders = IndexProvider.getCapability( Descriptor ).orderCapability( valueCategories );
						 foreach ( IndexOrder order in indexOrders )
						 {
							  IList<long> actualIds = AssertInOrder( order, predicates );
							  actualIds.sort( long?.compare );
							  // then
							  assertThat( actualIds, equalTo( expectedIds ) );
						 }
					}
			  }

			  public virtual ValueCategory[] GetValueCategories( IndexQuery[] predicates )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return java.util.predicates.Select( iq => iq.valueGroup().category() ).ToArray(ValueCategory[]::new);
			  }

			  public virtual IList<long> ExpectedIds( SortedSet<ValueAndId> sortedValues, Value booleanValue, Value from, Value to, bool fromInclusive, bool toInclusive )
			  {
					return sortedValues.subSet( new ValueAndId( ValueTuple.of( booleanValue, from ), 0 ), fromInclusive, new ValueAndId( ValueTuple.of( booleanValue, to ), 0 ), toInclusive ).Select( v => v.id ).OrderBy( long?.compare ).ToList();
			  }

			  internal virtual IList<ValueTuple> GenerateValuesFromType( ValueType[] types, ISet<ValueTuple> duplicateChecker, int count )
			  {
					IList<ValueTuple> values = new List<ValueTuple>();
					for ( long i = 0; i < count; i++ )
					{
						 ValueTuple value = GenerateUniqueRandomValue( types, duplicateChecker );
						 values.Add( value );
					}
					return values;
			  }

			  internal virtual ValueTuple GenerateUniqueRandomValue( ValueType[] types, ISet<ValueTuple> duplicateChecker )
			  {
					ValueTuple value;
					do
					{
						 value = ValueTuple.of( Random.randomValues().nextBooleanValue(), Random.randomValues().nextValueOfTypes(types) );
					} while ( !duplicateChecker.Add( value ) );
					return value;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<IndexEntryUpdate<?>> generateUpdatesFromValues(java.util.List<org.Neo4Net.values.storable.ValueTuple> values, org.apache.commons.lang3.mutable.MutableLong nextId)
			  internal virtual IList<IndexEntryUpdate<object>> GenerateUpdatesFromValues( IList<ValueTuple> values, MutableLong nextId )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
					IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
					foreach ( ValueTuple value in values )
					{
						 updates.Add( add( nextId.AndIncrement, Descriptor.schema(), (object[]) value.Values ) );
					}
					return updates;
			  }
		 }

		 private class ValueAndId
		 {
			  internal readonly ValueTuple Value;
			  internal readonly long Id;

			  internal ValueAndId( ValueTuple value, long id )
			  {
					this.Value = value;
					this.Id = id;
			  }
		 }
	}

}