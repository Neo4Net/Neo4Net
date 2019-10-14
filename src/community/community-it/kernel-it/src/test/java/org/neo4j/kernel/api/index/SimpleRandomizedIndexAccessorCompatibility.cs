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


	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueType = Neo4Net.Values.Storable.ValueType;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.change;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.remove;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.add;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite that provides test cases for verifying" + " IndexProvider implementations. Each index provider that is to be tested by this suite" + " must create their own test class extending IndexProviderCompatibilityTestSuite." + " The @Ignore annotation doesn't prevent these tests to run, it rather removes some annoying" + " errors or warnings in some IDEs about test classes needing a public zero-arg constructor.") public class SimpleRandomizedIndexAccessorCompatibility extends IndexAccessorCompatibility
	public class SimpleRandomizedIndexAccessorCompatibility : IndexAccessorCompatibility
	{
		 public SimpleRandomizedIndexAccessorCompatibility( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.forLabel( 1000, 100 ) )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExactMatchOnRandomValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestExactMatchOnRandomValues()
		 {
			  // given
			  ValueType[] types = RandomSetOfSupportedTypes();
			  IList<Value> values = GenerateValuesFromType( types, new HashSet<Value>(), 30_000 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = generateUpdatesFromValues(values, new org.apache.commons.lang3.mutable.MutableLong());
			  IList<IndexEntryUpdate<object>> updates = GenerateUpdatesFromValues( values, new MutableLong() );
			  UpdateAndCommit( updates );

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (IndexEntryUpdate<?> update : updates)
			  foreach ( IndexEntryUpdate<object> update in updates )
			  {
					// then
					IList<long> hits = Query( IndexQuery.exact( 0, update.Values()[0] ) );
					assertEquals( hits.ToString(), 1, hits.Count );
					assertThat( single( hits ), equalTo( update.EntityId ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRangeMatchInOrderOnRandomValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRangeMatchInOrderOnRandomValues()
		 {
			  Assume.assumeTrue( "Assume support for granular composite queries", TestSuite.supportsGranularCompositeQueries() );
			  // given
			  ValueType[] types = RandomSetOfSupportedAndSortableTypes();
			  ISet<Value> uniqueValues = new HashSet<Value>();
			  SortedSet<ValueAndId> sortedValues = new SortedSet<ValueAndId>( ( v1, v2 ) => Values.COMPARATOR.Compare( v1.value, v2.value ) );
			  MutableLong nextId = new MutableLong();

			  // A couple of rounds of updates followed by lots of range verifications
			  for ( int i = 0; i < 5; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
					IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
					if ( i == 0 )
					{
						 // The initial batch of data can simply be additions
						 updates = GenerateUpdatesFromValues( GenerateValuesFromType( types, uniqueValues, 20_000 ), nextId );
						 sortedValues.addAll( updates.Select( u => new ValueAndId( u.values()[0], u.EntityId ) ).ToList() );
					}
					else
					{
						 // Then do all sorts of updates
						 for ( int j = 0; j < 1_000; j++ )
						 {
							  int type = Random.intBetween( 0, 2 );
							  if ( type == 0 )
							  { // add
									Value value = GenerateUniqueRandomValue( types, uniqueValues );
									long id = nextId.AndIncrement;
									sortedValues.Add( new ValueAndId( value, id ) );
									updates.Add( add( id, Descriptor.schema(), value ) );
							  }
							  else if ( type == 1 )
							  { // update
									ValueAndId existing = Random.among( sortedValues.toArray( new ValueAndId[0] ) );
									sortedValues.remove( existing );
									Value newValue = GenerateUniqueRandomValue( types, uniqueValues );
									uniqueValues.remove( existing.Value );
									sortedValues.Add( new ValueAndId( newValue, existing.Id ) );
									updates.Add( change( existing.Id, Descriptor.schema(), existing.Value, newValue ) );
							  }
							  else
							  { // remove
									ValueAndId existing = Random.among( sortedValues.toArray( new ValueAndId[0] ) );
									sortedValues.remove( existing );
									uniqueValues.remove( existing.Value );
									updates.Add( remove( existing.Id, Descriptor.schema(), existing.Value ) );
							  }
						 }
					}
					UpdateAndCommit( updates );
					VerifyRandomRanges( types, sortedValues );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRandomRanges(org.neo4j.values.storable.ValueType[] types, java.util.TreeSet<ValueAndId> sortedValues) throws Exception
		 private void VerifyRandomRanges( ValueType[] types, SortedSet<ValueAndId> sortedValues )
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					// Construct a random range query of random value type
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

					// Expected result based on query
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> predicate = org.neo4j.internal.kernel.api.IndexQuery.range(0, from, fromInclusive, to, toInclusive);
					IndexQuery.RangePredicate<object> predicate = IndexQuery.range( 0, from, fromInclusive, to, toInclusive );
					IList<long> expectedIds = expectedIds( sortedValues, from, to, fromInclusive, toInclusive );

					// Depending on order capabilities we verify ids or order and ids.
					IndexOrder[] indexOrders = IndexProvider.getCapability( Descriptor ).orderCapability( predicate.ValueGroup().category() );
					foreach ( IndexOrder order in indexOrders )
					{
						 IList<long> actualIds = AssertInOrder( order, predicate );
						 actualIds.sort( long?.compare );
						 // then
						 assertThat( actualIds, equalTo( expectedIds ) );
					}
			  }
		 }

		 private IList<long> ExpectedIds( SortedSet<ValueAndId> sortedValues, Value from, Value to, bool fromInclusive, bool toInclusive )
		 {
			  return sortedValues.subSet( new ValueAndId( from, 0L ), fromInclusive, new ValueAndId( to, 0L ), toInclusive ).Select( v => v.id ).OrderBy( long?.compare ).ToList();
		 }

		 private IList<Value> GenerateValuesFromType( ValueType[] types, ISet<Value> duplicateChecker, int count )
		 {
			  IList<Value> values = new List<Value>();
			  for ( long i = 0; i < count; i++ )
			  {
					Value value = GenerateUniqueRandomValue( types, duplicateChecker );
					values.Add( value );
			  }
			  return values;
		 }

		 private Value GenerateUniqueRandomValue( ValueType[] types, ISet<Value> duplicateChecker )
		 {
			  Value value;
			  do
			  {
					value = Random.randomValues().nextValueOfTypes(types);
			  } while ( !duplicateChecker.Add( value ) );
			  return value;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<IndexEntryUpdate<?>> generateUpdatesFromValues(java.util.List<org.neo4j.values.storable.Value> values, org.apache.commons.lang3.mutable.MutableLong nextId)
		 private IList<IndexEntryUpdate<object>> GenerateUpdatesFromValues( IList<Value> values, MutableLong nextId )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
			  IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
			  foreach ( Value value in values )
			  {
					IndexEntryUpdate<SchemaDescriptor> update = add( nextId.AndIncrement, Descriptor.schema(), value );
					updates.Add( update );
			  }
			  return updates;
		 }

		 private class ValueAndId
		 {
			  internal readonly Value Value;
			  internal readonly long Id;

			  internal ValueAndId( Value value, long id )
			  {
					this.Value = value;
					this.Id = id;
			  }
		 }
	}

}