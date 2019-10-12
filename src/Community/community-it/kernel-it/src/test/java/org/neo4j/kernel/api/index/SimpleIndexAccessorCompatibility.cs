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
namespace Neo4Net.Kernel.Api.Index
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using Matchers = org.hamcrest.Matchers;
	using Assume = org.junit.Assume;
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using SimpleNodeValueClient = Neo4Net.Storageengine.Api.schema.SimpleNodeValueClient;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueType = Neo4Net.Values.Storable.ValueType;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.exists;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.range;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.stringContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.stringPrefix;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.stringSuffix;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.epochDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite that provides test cases for verifying" + " IndexProvider implementations. Each index provider that is to be tested by this suite" + " must create their own test class extending IndexProviderCompatibilityTestSuite." + " The @Ignore annotation doesn't prevent these tests to run, it rather removes some annoying" + " errors or warnings in some IDEs about test classes needing a public zero-arg constructor.") public abstract class SimpleIndexAccessorCompatibility extends IndexAccessorCompatibility
	public abstract class SimpleIndexAccessorCompatibility : IndexAccessorCompatibility
	{
		 public SimpleIndexAccessorCompatibility( IndexProviderCompatibilityTestSuite testSuite, IndexDescriptor descriptor ) : base( testSuite, descriptor )
		 {
		 }

		 // This behaviour is shared by General and Unique indexes

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekByPrefix() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekByPrefix()
		 {
			  UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a" ), add(2L, Descriptor.schema(), "A"), add(3L, Descriptor.schema(), "apa"), add(4L, Descriptor.schema(), "apA"), add(5L, Descriptor.schema(), "b") ) );

			  assertThat( Query( IndexQuery.stringPrefix( 1, stringValue( "a" ) ) ), equalTo( asList( 1L, 3L, 4L ) ) );
			  assertThat( Query( IndexQuery.stringPrefix( 1, stringValue( "A" ) ) ), equalTo( singletonList( 2L ) ) );
			  assertThat( Query( IndexQuery.stringPrefix( 1, stringValue( "ba" ) ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( IndexQuery.stringPrefix( 1, stringValue( "" ) ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekByPrefixOnNonStrings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekByPrefixOnNonStrings()
		 {
			  UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "2a" ), add(2L, Descriptor.schema(), 2L), add(2L, Descriptor.schema(), 20L) ) );
			  assertThat( Query( IndexQuery.stringPrefix( 1, stringValue( "2" ) ) ), equalTo( singletonList( 1L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDateTimeWithSneakyZones() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByDateTimeWithSneakyZones()
		 {
			  DateTimeValue d1 = datetime( 9999, 100, ZoneId.of( "+18:00" ) );
			  DateTimeValue d4 = datetime( 10000, 100, ZoneId.of( "UTC" ) );
			  DateTimeValue d5 = datetime( 10000, 100, ZoneId.of( "+01:00" ) );
			  DateTimeValue d6 = datetime( 10000, 100, ZoneId.of( "Europe/Stockholm" ) );
			  DateTimeValue d7 = datetime( 10000, 100, ZoneId.of( "+03:00" ) );
			  DateTimeValue d8 = datetime( 10000, 101, ZoneId.of( "UTC" ) );

			  UpdateAndCommit( asList( add( 1L, Descriptor.schema(), d1 ), add(4L, Descriptor.schema(), d4), add(5L, Descriptor.schema(), d5), add(6L, Descriptor.schema(), d6), add(7L, Descriptor.schema(), d7), add(8L, Descriptor.schema(), d8) ) );

			  assertThat( Query( range( 1, d4, true, d7, true ) ), Matchers.contains( 4L, 5L, 6L, 7L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekWithSpatial() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekWithSpatial()
		 {
			  Assume.assumeTrue( TestSuite.supportsSpatial() );

			  PointValue p1 = Values.pointValue( CoordinateReferenceSystem.WGS84, -180, -1 );
			  PointValue p2 = Values.pointValue( CoordinateReferenceSystem.WGS84, -180, 1 );
			  PointValue p3 = Values.pointValue( CoordinateReferenceSystem.WGS84, 0, 0 );

			  UpdateAndCommit( asList( add( 1L, Descriptor.schema(), p1 ), add(2L, Descriptor.schema(), p2), add(3L, Descriptor.schema(), p3) ) );

			  assertThat( Query( range( 1, p1, true, p2, true ) ), Matchers.contains( 1L, 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateWithAllValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateWithAllValues()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = updates(valueSet1);
			  IList<IndexEntryUpdate<object>> updates = updates( ValueSet1 );
			  UpdateAndCommit( updates );

			  // then
			  int propertyKeyId = Descriptor.schema().PropertyId;
			  foreach ( NodeAndValue entry in ValueSet1 )
			  {
					IList<long> result = Query( IndexQuery.exact( propertyKeyId, entry.Value ) );
					assertThat( result, equalTo( singletonList( entry.NodeId ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanAllValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldScanAllValues()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = updates(valueSet1);
			  IList<IndexEntryUpdate<object>> updates = updates( ValueSet1 );
			  UpdateAndCommit( updates );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  long?[] allNodes = ValueSet1.Select( x => x.nodeId ).ToArray( long?[]::new );

			  // THEN
			  int propertyKeyId = Descriptor.schema().PropertyId;
			  IList<long> result = Query( IndexQuery.exists( propertyKeyId ) );
			  assertThat( result, containsInAnyOrder( allNodes ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByNumber()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextNumberValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByText() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByText()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextTextValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByChar() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByChar()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextCharValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByDateTime()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextDateTimeValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByLocalDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByLocalDateTime()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextLocalDateTimeValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByDate()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextDateValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByTime()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextTimeValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByLocalTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByLocalTime()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextLocalTimeValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDuration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByDuration()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextDuration() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByPeriod() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByPeriod()
		 {
			  TestIndexRangeSeek( () => Random.randomValues().nextPeriod() );
		 }

		 // testIndexRangeSeekGeometry not present because geometry is not orderable
		 // testIndexRangeSeekBoolean not present because test needs more than two possible values

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByZonedDateTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByZonedDateTimeArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextDateTimeArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByLocalDateTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByLocalDateTimeArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextLocalDateTimeArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDateArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByDateArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextDateArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByZonedTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByZonedTimeArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextTimeArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByLocalTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByLocalTimeArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextLocalTimeArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDurationArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByDurationArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextDurationArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByTextArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByTextArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextBasicMultilingualPlaneTextArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByCharArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByCharArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextCharArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByBooleanArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByBooleanArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextBooleanArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByByteArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByByteArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextByteArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByShortArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByShortArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextShortArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByIntArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByIntArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextIntArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByLongArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByLongArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextLongArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByFloatArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByFloatArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextFloatArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDoubleArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexRangeSeekByDoubleArray()
		 {
			  TestIndexRangeSeekArray( () => Random.randomValues().nextDoubleArray() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexRangeSeekArray(System.Func<org.neo4j.values.storable.ArrayValue> generator) throws Exception
		 private void TestIndexRangeSeekArray( System.Func<ArrayValue> generator )
		 {
			  Assume.assumeTrue( TestSuite.supportsGranularCompositeQueries() );
			  TestIndexRangeSeek( generator );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexRangeSeek(System.Func<? extends org.neo4j.values.storable.Value> generator) throws Exception
		 private void TestIndexRangeSeek<T1>( System.Func<T1> generator ) where T1 : Neo4Net.Values.Storable.Value
		 {
			  int count = Random.Next( 5, 10 );
			  IList<Value> values = new List<Value>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
			  IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
			  ISet<Value> duplicateCheck = new HashSet<Value>();
			  for ( int i = 0; i < count; i++ )
			  {
					Value value;
					do
					{
						 value = generator();
					} while ( !duplicateCheck.Add( value ) );
					values.Add( value );
			  }
			  values.sort( Values.COMPARATOR );
			  for ( int i = 0; i < count; i++ )
			  {
					updates.Add( add( i + 1, Descriptor.schema(), values[i] ) );
			  }
			  Collections.shuffle( updates ); // <- Don't rely on insert order

			  UpdateAndCommit( updates );

			  for ( int f = 0; f < values.Count; f++ )
			  {
					for ( int t = f; t < values.Count; t++ )
					{
						 Value from = values[f];
						 Value to = values[t];
						 foreach ( bool fromInclusive in new bool[] { true, false } )
						 {
							  foreach ( bool toInclusive in new bool[] { true, false } )
							  {
									assertThat( Query( range( 1, from, fromInclusive, to, toInclusive ) ), equalTo( Ids( f, fromInclusive, t, toInclusive ) ) );
							  }
						 }
					}
			  }
		 }

		 private IList<long> Ids( int fromIndex, bool fromInclusive, int toIndex, bool toInclusive )
		 {
			  IList<long> ids = new List<long>();
			  int from = fromInclusive ? fromIndex : fromIndex + 1;
			  int to = toInclusive ? toIndex : toIndex - 1;
			  for ( int i = from; i <= to; i++ )
			  {
					ids.Add( ( long )( i + 1 ) );
			  }
			  return ids;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingNumber()
		 {
			  object o0 = 0;
			  object o1 = 1;
			  object o2 = 2;
			  object o3 = 3;
			  object o4 = 4;
			  object o5 = 5;
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingNumber()
		 {
			  object o0 = 0;
			  object o1 = 1;
			  object o2 = 2;
			  object o3 = 3;
			  object o4 = 4;
			  object o5 = 5;
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingString()
		 {
			  object o0 = "0";
			  object o1 = "1";
			  object o2 = "2";
			  object o3 = "3";
			  object o4 = "4";
			  object o5 = "5";
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingString()
		 {
			  object o0 = "0";
			  object o1 = "1";
			  object o2 = "2";
			  object o3 = "3";
			  object o4 = "4";
			  object o5 = "5";
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingDate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingDate()
		 {
			  object o0 = DateValue.epochDateRaw( 0 );
			  object o1 = DateValue.epochDateRaw( 1 );
			  object o2 = DateValue.epochDateRaw( 2 );
			  object o3 = DateValue.epochDateRaw( 3 );
			  object o4 = DateValue.epochDateRaw( 4 );
			  object o5 = DateValue.epochDateRaw( 5 );
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingDate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingDate()
		 {
			  object o0 = DateValue.epochDateRaw( 0 );
			  object o1 = DateValue.epochDateRaw( 1 );
			  object o2 = DateValue.epochDateRaw( 2 );
			  object o3 = DateValue.epochDateRaw( 3 );
			  object o4 = DateValue.epochDateRaw( 4 );
			  object o5 = DateValue.epochDateRaw( 5 );
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingLocalTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingLocalTime()
		 {
			  object o0 = LocalTimeValue.localTimeRaw( 0 );
			  object o1 = LocalTimeValue.localTimeRaw( 1 );
			  object o2 = LocalTimeValue.localTimeRaw( 2 );
			  object o3 = LocalTimeValue.localTimeRaw( 3 );
			  object o4 = LocalTimeValue.localTimeRaw( 4 );
			  object o5 = LocalTimeValue.localTimeRaw( 5 );
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingLocalTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingLocalTime()
		 {
			  object o0 = LocalTimeValue.localTimeRaw( 0 );
			  object o1 = LocalTimeValue.localTimeRaw( 1 );
			  object o2 = LocalTimeValue.localTimeRaw( 2 );
			  object o3 = LocalTimeValue.localTimeRaw( 3 );
			  object o4 = LocalTimeValue.localTimeRaw( 4 );
			  object o5 = LocalTimeValue.localTimeRaw( 5 );
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingTime()
		 {
			  object o0 = TimeValue.timeRaw( 0, ZoneOffset.ofHours( 0 ) );
			  object o1 = TimeValue.timeRaw( 1, ZoneOffset.ofHours( 0 ) );
			  object o2 = TimeValue.timeRaw( 2, ZoneOffset.ofHours( 0 ) );
			  object o3 = TimeValue.timeRaw( 3, ZoneOffset.ofHours( 0 ) );
			  object o4 = TimeValue.timeRaw( 4, ZoneOffset.ofHours( 0 ) );
			  object o5 = TimeValue.timeRaw( 5, ZoneOffset.ofHours( 0 ) );
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingTime()
		 {
			  object o0 = TimeValue.timeRaw( 0, ZoneOffset.ofHours( 0 ) );
			  object o1 = TimeValue.timeRaw( 1, ZoneOffset.ofHours( 0 ) );
			  object o2 = TimeValue.timeRaw( 2, ZoneOffset.ofHours( 0 ) );
			  object o3 = TimeValue.timeRaw( 3, ZoneOffset.ofHours( 0 ) );
			  object o4 = TimeValue.timeRaw( 4, ZoneOffset.ofHours( 0 ) );
			  object o5 = TimeValue.timeRaw( 5, ZoneOffset.ofHours( 0 ) );
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingLocalDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingLocalDateTime()
		 {
			  object o0 = LocalDateTimeValue.localDateTimeRaw( 10, 0 );
			  object o1 = LocalDateTimeValue.localDateTimeRaw( 10, 1 );
			  object o2 = LocalDateTimeValue.localDateTimeRaw( 10, 2 );
			  object o3 = LocalDateTimeValue.localDateTimeRaw( 10, 3 );
			  object o4 = LocalDateTimeValue.localDateTimeRaw( 10, 4 );
			  object o5 = LocalDateTimeValue.localDateTimeRaw( 10, 5 );
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingLocalDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingLocalDateTime()
		 {
			  object o0 = LocalDateTimeValue.localDateTimeRaw( 10, 0 );
			  object o1 = LocalDateTimeValue.localDateTimeRaw( 10, 1 );
			  object o2 = LocalDateTimeValue.localDateTimeRaw( 10, 2 );
			  object o3 = LocalDateTimeValue.localDateTimeRaw( 10, 3 );
			  object o4 = LocalDateTimeValue.localDateTimeRaw( 10, 4 );
			  object o5 = LocalDateTimeValue.localDateTimeRaw( 10, 5 );
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingDateTime()
		 {
			  object o0 = DateTimeValue.datetimeRaw( 1, 0, ZoneId.of( "UTC" ) );
			  object o1 = DateTimeValue.datetimeRaw( 1, 1, ZoneId.of( "UTC" ) );
			  object o2 = DateTimeValue.datetimeRaw( 1, 2, ZoneId.of( "UTC" ) );
			  object o3 = DateTimeValue.datetimeRaw( 1, 3, ZoneId.of( "UTC" ) );
			  object o4 = DateTimeValue.datetimeRaw( 1, 4, ZoneId.of( "UTC" ) );
			  object o5 = DateTimeValue.datetimeRaw( 1, 5, ZoneId.of( "UTC" ) );
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingDateTime()
		 {
			  object o0 = DateTimeValue.datetimeRaw( 1, 0, ZoneId.of( "UTC" ) );
			  object o1 = DateTimeValue.datetimeRaw( 1, 1, ZoneId.of( "UTC" ) );
			  object o2 = DateTimeValue.datetimeRaw( 1, 2, ZoneId.of( "UTC" ) );
			  object o3 = DateTimeValue.datetimeRaw( 1, 3, ZoneId.of( "UTC" ) );
			  object o4 = DateTimeValue.datetimeRaw( 1, 4, ZoneId.of( "UTC" ) );
			  object o5 = DateTimeValue.datetimeRaw( 1, 5, ZoneId.of( "UTC" ) );
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingDuration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingDuration()
		 {
			  object o0 = Duration.ofMillis( 0 );
			  object o1 = Duration.ofMillis( 1 );
			  object o2 = Duration.ofMillis( 2 );
			  object o3 = Duration.ofMillis( 3 );
			  object o4 = Duration.ofMillis( 4 );
			  object o5 = Duration.ofMillis( 5 );
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingDuration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingDuration()
		 {
			  object o0 = Duration.ofMillis( 0 );
			  object o1 = Duration.ofMillis( 1 );
			  object o2 = Duration.ofMillis( 2 );
			  object o3 = Duration.ofMillis( 3 );
			  object o4 = Duration.ofMillis( 4 );
			  object o5 = Duration.ofMillis( 5 );
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingNumberArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingNumberArray()
		 {
			  object o0 = new int[]{ 0 };
			  object o1 = new int[]{ 1 };
			  object o2 = new int[]{ 2 };
			  object o3 = new int[]{ 3 };
			  object o4 = new int[]{ 4 };
			  object o5 = new int[]{ 5 };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingNumberArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingNumberArray()
		 {
			  object o0 = new int[]{ 0 };
			  object o1 = new int[]{ 1 };
			  object o2 = new int[]{ 2 };
			  object o3 = new int[]{ 3 };
			  object o4 = new int[]{ 4 };
			  object o5 = new int[]{ 5 };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingStringArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingStringArray()
		 {
			  object o0 = new string[]{ "0" };
			  object o1 = new string[]{ "1" };
			  object o2 = new string[]{ "2" };
			  object o3 = new string[]{ "3" };
			  object o4 = new string[]{ "4" };
			  object o5 = new string[]{ "5" };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingStringArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingStringArray()
		 {
			  object o0 = new string[]{ "0" };
			  object o1 = new string[]{ "1" };
			  object o2 = new string[]{ "2" };
			  object o3 = new string[]{ "3" };
			  object o4 = new string[]{ "4" };
			  object o5 = new string[]{ "5" };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingBooleanArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingBooleanArray()
		 {
			  object o0 = new bool[]{ false };
			  object o1 = new bool[]{ false, false };
			  object o2 = new bool[]{ false, true };
			  object o3 = new bool[]{ true };
			  object o4 = new bool[]{ true, false };
			  object o5 = new bool[]{ true, true };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingBooleanArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingBooleanArray()
		 {
			  object o0 = new bool[]{ false };
			  object o1 = new bool[]{ false, false };
			  object o2 = new bool[]{ false, true };
			  object o3 = new bool[]{ true };
			  object o4 = new bool[]{ true, false };
			  object o5 = new bool[]{ true, true };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingDateTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingDateTimeArray()
		 {
			  object o0 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 0, ZoneId.of( "UTC" ) ) };
			  object o1 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 1, ZoneId.of( "UTC" ) ) };
			  object o2 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 2, ZoneId.of( "UTC" ) ) };
			  object o3 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 3, ZoneId.of( "UTC" ) ) };
			  object o4 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 4, ZoneId.of( "UTC" ) ) };
			  object o5 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 5, ZoneId.of( "UTC" ) ) };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingDateTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingDateTimeArray()
		 {
			  object o0 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 0, ZoneId.of( "UTC" ) ) };
			  object o1 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 1, ZoneId.of( "UTC" ) ) };
			  object o2 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 2, ZoneId.of( "UTC" ) ) };
			  object o3 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 3, ZoneId.of( "UTC" ) ) };
			  object o4 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 4, ZoneId.of( "UTC" ) ) };
			  object o5 = new ZonedDateTime[]{ ZonedDateTime.of( 10, 10, 10, 10, 10, 10, 5, ZoneId.of( "UTC" ) ) };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingLocalDateTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingLocalDateTimeArray()
		 {
			  object o0 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 0 ) };
			  object o1 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 1 ) };
			  object o2 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 2 ) };
			  object o3 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 3 ) };
			  object o4 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 4 ) };
			  object o5 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 5 ) };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingLocalDateTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingLocalDateTimeArray()
		 {
			  object o0 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 0 ) };
			  object o1 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 1 ) };
			  object o2 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 2 ) };
			  object o3 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 3 ) };
			  object o4 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 4 ) };
			  object o5 = new DateTime[]{ new DateTime( 10, 10, 10, 10, 10, 10, 5 ) };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingTimeArray()
		 {
			  object o0 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 0, ZoneOffset.ofHours( 0 ) ) };
			  object o1 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 1, ZoneOffset.ofHours( 0 ) ) };
			  object o2 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 2, ZoneOffset.ofHours( 0 ) ) };
			  object o3 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 3, ZoneOffset.ofHours( 0 ) ) };
			  object o4 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 4, ZoneOffset.ofHours( 0 ) ) };
			  object o5 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 5, ZoneOffset.ofHours( 0 ) ) };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingTimeArray()
		 {
			  object o0 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 0, ZoneOffset.ofHours( 0 ) ) };
			  object o1 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 1, ZoneOffset.ofHours( 0 ) ) };
			  object o2 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 2, ZoneOffset.ofHours( 0 ) ) };
			  object o3 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 3, ZoneOffset.ofHours( 0 ) ) };
			  object o4 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 4, ZoneOffset.ofHours( 0 ) ) };
			  object o5 = new OffsetTime[]{ OffsetTime.of( 10, 10, 10, 5, ZoneOffset.ofHours( 0 ) ) };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingDateArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingDateArray()
		 {
			  object o0 = new LocalDate[]{ LocalDate.of( 10, 10, 1 ) };
			  object o1 = new LocalDate[]{ LocalDate.of( 10, 10, 2 ) };
			  object o2 = new LocalDate[]{ LocalDate.of( 10, 10, 3 ) };
			  object o3 = new LocalDate[]{ LocalDate.of( 10, 10, 4 ) };
			  object o4 = new LocalDate[]{ LocalDate.of( 10, 10, 5 ) };
			  object o5 = new LocalDate[]{ LocalDate.of( 10, 10, 6 ) };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingDateArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingDateArray()
		 {
			  object o0 = new LocalDate[]{ LocalDate.of( 10, 10, 1 ) };
			  object o1 = new LocalDate[]{ LocalDate.of( 10, 10, 2 ) };
			  object o2 = new LocalDate[]{ LocalDate.of( 10, 10, 3 ) };
			  object o3 = new LocalDate[]{ LocalDate.of( 10, 10, 4 ) };
			  object o4 = new LocalDate[]{ LocalDate.of( 10, 10, 5 ) };
			  object o5 = new LocalDate[]{ LocalDate.of( 10, 10, 6 ) };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingLocalTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingLocalTimeArray()
		 {
			  object o0 = new LocalTime[]{ LocalTime.of( 10, 0 ) };
			  object o1 = new LocalTime[]{ LocalTime.of( 10, 1 ) };
			  object o2 = new LocalTime[]{ LocalTime.of( 10, 2 ) };
			  object o3 = new LocalTime[]{ LocalTime.of( 10, 3 ) };
			  object o4 = new LocalTime[]{ LocalTime.of( 10, 4 ) };
			  object o5 = new LocalTime[]{ LocalTime.of( 10, 5 ) };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingLocalTimeArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingLocalTimeArray()
		 {
			  object o0 = new LocalTime[]{ LocalTime.of( 10, 0 ) };
			  object o1 = new LocalTime[]{ LocalTime.of( 10, 1 ) };
			  object o2 = new LocalTime[]{ LocalTime.of( 10, 2 ) };
			  object o3 = new LocalTime[]{ LocalTime.of( 10, 3 ) };
			  object o4 = new LocalTime[]{ LocalTime.of( 10, 4 ) };
			  object o5 = new LocalTime[]{ LocalTime.of( 10, 5 ) };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderAscendingDurationArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderAscendingDurationArray()
		 {
			  object o0 = new Duration[]{ Duration.of( 0, ChronoUnit.SECONDS ) };
			  object o1 = new Duration[]{ Duration.of( 1, ChronoUnit.SECONDS ) };
			  object o2 = new Duration[]{ Duration.of( 2, ChronoUnit.SECONDS ) };
			  object o3 = new Duration[]{ Duration.of( 3, ChronoUnit.SECONDS ) };
			  object o4 = new Duration[]{ Duration.of( 4, ChronoUnit.SECONDS ) };
			  object o5 = new Duration[]{ Duration.of( 5, ChronoUnit.SECONDS ) };
			  ShouldRangeSeekInOrder( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderDescendingDurationArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderDescendingDurationArray()
		 {
			  object o0 = new Duration[]{ Duration.of( 0, ChronoUnit.SECONDS ) };
			  object o1 = new Duration[]{ Duration.of( 1, ChronoUnit.SECONDS ) };
			  object o2 = new Duration[]{ Duration.of( 2, ChronoUnit.SECONDS ) };
			  object o3 = new Duration[]{ Duration.of( 3, ChronoUnit.SECONDS ) };
			  object o4 = new Duration[]{ Duration.of( 4, ChronoUnit.SECONDS ) };
			  object o5 = new Duration[]{ Duration.of( 5, ChronoUnit.SECONDS ) };
			  ShouldRangeSeekInOrder( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldRangeSeekInOrder(org.neo4j.internal.kernel.api.IndexOrder order, Object o0, Object o1, Object o2, Object o3, Object o4, Object o5) throws Exception
		 private void ShouldRangeSeekInOrder( IndexOrder order, object o0, object o1, object o2, object o3, object o4, object o5 )
		 {
			  IndexQuery range = range( 100, Values.of( o0 ), true, Values.of( o5 ), true );
			  IndexOrder[] indexOrders = OrderCapability( range );
			  Assume.assumeTrue( "Assume support for order " + order, ArrayUtils.contains( indexOrders, order ) );

			  UpdateAndCommit( asList( add( 1, Descriptor.schema(), o0 ), add(1, Descriptor.schema(), o5), add(1, Descriptor.schema(), o1), add(1, Descriptor.schema(), o4), add(1, Descriptor.schema(), o2), add(1, Descriptor.schema(), o3) ) );

			  SimpleNodeValueClient client = new SimpleNodeValueClient();
			  using ( AutoCloseable ignored = Query( client, order, range ) )
			  {
					IList<long> seenIds = AssertClientReturnValuesInOrder( client, order );
					assertThat( seenIds.Count, equalTo( 6 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateEntries()
		 {
			  ValueType[] valueTypes = TestSuite.supportedValueTypes();
			  long entityId = Random.nextLong( 1_000_000_000 );
			  foreach ( ValueType valueType in valueTypes )
			  {
					// given
					Value value = Random.nextValue( valueType );
					UpdateAndCommit( singletonList( IndexEntryUpdate.Add( entityId, Descriptor.schema(), value ) ) );
					assertEquals( singletonList( entityId ), Query( IndexQuery.exact( 0, value ) ) );

					// when
					Value newValue;
					do
					{
						 newValue = Random.nextValue( valueType );
					} while ( value.Equals( newValue ) );
					UpdateAndCommit( singletonList( IndexEntryUpdate.Change( entityId, Descriptor.schema(), value, newValue ) ) );

					// then
					assertEquals( emptyList(), Query(IndexQuery.exact(0, value)) );
					assertEquals( singletonList( entityId ), Query( IndexQuery.exact( 0, newValue ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveEntries()
		 {
			  ValueType[] valueTypes = TestSuite.supportedValueTypes();
			  long entityId = Random.nextLong( 1_000_000_000 );
			  foreach ( ValueType valueType in valueTypes )
			  {
					// given
					Value value = Random.nextValue( valueType );
					UpdateAndCommit( singletonList( IndexEntryUpdate.Add( entityId, Descriptor.schema(), value ) ) );
					assertEquals( singletonList( entityId ), Query( IndexQuery.exact( 0, value ) ) );

					// when
					UpdateAndCommit( singletonList( IndexEntryUpdate.Remove( entityId, Descriptor.schema(), value ) ) );

					// then
					assertTrue( Query( IndexQuery.exact( 0, value ) ).Count == 0 );
			  }
		 }

		 // This behaviour is expected by General indexes

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class General extends SimpleIndexAccessorCompatibility
		 public class General : SimpleIndexAccessorCompatibility
		 {
			  public General( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.forLabel( 1000, 100 ) )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closingAnOnlineIndexUpdaterMustNotThrowEvenIfItHasBeenFedConflictingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ClosingAnOnlineIndexUpdaterMustNotThrowEvenIfItHasBeenFedConflictingData()
			  {
					// The reason is that we use and close IndexUpdaters in commit - not in prepare - and therefor
					// we cannot have them go around and throw exceptions, because that could potentially break
					// recovery.
					// Conflicting data can happen because of faulty data coercion. These faults are resolved by
					// the exact-match filtering we do on index seeks.

					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a" ), add(2L, Descriptor.schema(), "a") ) );

					assertThat( Query( exact( 1, "a" ) ), equalTo( asList( 1L, 2L ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekAndScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexSeekAndScan()
			  {
					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a" ), add(2L, Descriptor.schema(), "a"), add(3L, Descriptor.schema(), "b") ) );

					assertThat( Query( exact( 1, "a" ) ), equalTo( asList( 1L, 2L ) ) );
					assertThat( Query( exists( 1 ) ), equalTo( asList( 1L, 2L, 3L ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByNumberWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByNumberWithDuplicates()
			  {
					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), -5 ), add(2L, Descriptor.schema(), -5), add(3L, Descriptor.schema(), 0), add(4L, Descriptor.schema(), 5), add(5L, Descriptor.schema(), 5) ) );

					assertThat( Query( range( 1, -5, true, 5, true ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L ) ) );
					assertThat( Query( range( 1, -3, true, -1, true ) ), equalTo( EMPTY_LIST ) );
					assertThat( Query( range( 1, -5, true, 4, true ) ), equalTo( asList( 1L, 2L, 3L ) ) );
					assertThat( Query( range( 1, -4, true, 5, true ) ), equalTo( asList( 3L, 4L, 5L ) ) );
					assertThat( Query( range( 1, -5, true, 5, true ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByStringWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByStringWithDuplicates()
			  {
					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "Anna" ), add(2L, Descriptor.schema(), "Anna"), add(3L, Descriptor.schema(), "Bob"), add(4L, Descriptor.schema(), "William"), add(5L, Descriptor.schema(), "William") ) );

					assertThat( Query( range( 1, "Anna", false, "William", false ) ), equalTo( singletonList( 3L ) ) );
					assertThat( Query( range( 1, "Arabella", false, "Bob", false ) ), equalTo( EMPTY_LIST ) );
					assertThat( Query( range( 1, "Anna", true, "William", false ) ), equalTo( asList( 1L, 2L, 3L ) ) );
					assertThat( Query( range( 1, "Anna", false, "William", true ) ), equalTo( asList( 3L, 4L, 5L ) ) );
					assertThat( Query( range( 1, "Anna", true, "William", true ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDateWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByDateWithDuplicates()
			  {
					TestIndexRangeSeekWithDuplicates( epochDate( 100 ), epochDate( 101 ), epochDate( 200 ), epochDate( 300 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByLocalDateTimeWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByLocalDateTimeWithDuplicates()
			  {
					TestIndexRangeSeekWithDuplicates( localDateTime( 1000, 10 ), localDateTime( 1000, 11 ), localDateTime( 2000, 10 ), localDateTime( 3000, 10 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDateTimeWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByDateTimeWithDuplicates()
			  {
					TestIndexRangeSeekWithDuplicates( datetime( 1000, 10, UTC ), datetime( 1000, 11, UTC ), datetime( 2000, 10, UTC ), datetime( 3000, 10, UTC ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByLocalTimeWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByLocalTimeWithDuplicates()
			  {
					TestIndexRangeSeekWithDuplicates( localTime( 1000 ), localTime( 1001 ), localTime( 2000 ), localTime( 3000 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByTimeWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByTimeWithDuplicates()
			  {
					TestIndexRangeSeekWithDuplicates( time( 1000, UTC ), time( 1001, UTC ), time( 2000, UTC ), time( 3000, UTC ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByTimeWithZonesAndDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByTimeWithZonesAndDuplicates()
			  {
					TestIndexRangeSeekWithDuplicates( time( 20, 31, 53, 4, ZoneOffset.of( "+17:02" ) ), time( 20, 31, 54, 3, ZoneOffset.of( "+17:02" ) ), time( 19, 31, 54, 2, UTC ), time( 18, 23, 27, 1, ZoneOffset.of( "-18:00" ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByDurationWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByDurationWithDuplicates()
			  {
					TestIndexRangeSeekWithDuplicates( duration( 1, 1, 1, 1 ), duration( 1, 1, 1, 2 ), duration( 2, 1, 1, 1 ), duration( 3, 1, 1, 1 ) );
			  }

			  /// <summary>
			  /// Helper for testing range seeks. Takes 4 ordered sample values.
			  /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <VALUE extends org.neo4j.values.storable.Value> void testIndexRangeSeekWithDuplicates(VALUE v1, VALUE v2, VALUE v3, VALUE v4) throws Exception
			  internal virtual void TestIndexRangeSeekWithDuplicates<VALUE>( VALUE v1, VALUE v2, VALUE v3, VALUE v4 ) where VALUE : Neo4Net.Values.Storable.Value
			  {
					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), v1 ), add(2L, Descriptor.schema(), v1), add(3L, Descriptor.schema(), v3), add(4L, Descriptor.schema(), v4), add(5L, Descriptor.schema(), v4) ) );

					assertThat( Query( range( 1, v1, false, v4, false ) ), equalTo( singletonList( 3L ) ) );
					assertThat( Query( range( 1, v2, false, v3, false ) ), equalTo( EMPTY_LIST ) );
					assertThat( Query( range( 1, v1, true, v4, false ) ), equalTo( asList( 1L, 2L, 3L ) ) );
					assertThat( Query( range( 1, v1, false, v4, true ) ), equalTo( asList( 3L, 4L, 5L ) ) );
					assertThat( Query( range( 1, v1, true, v4, true ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexRangeSeekByPrefixWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexRangeSeekByPrefixWithDuplicates()
			  {
					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a" ), add(2L, Descriptor.schema(), "A"), add(3L, Descriptor.schema(), "apa"), add(4L, Descriptor.schema(), "apa"), add(5L, Descriptor.schema(), "apa") ) );

					assertThat( Query( stringPrefix( 1, stringValue( "a" ) ) ), equalTo( asList( 1L, 3L, 4L, 5L ) ) );
					assertThat( Query( stringPrefix( 1, stringValue( "apa" ) ) ), equalTo( asList( 3L, 4L, 5L ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexFullSearchWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexFullSearchWithDuplicates()
			  {
					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a" ), add(2L, Descriptor.schema(), "A"), add(3L, Descriptor.schema(), "apa"), add(4L, Descriptor.schema(), "apa"), add(5L, Descriptor.schema(), "apalong") ) );

					assertThat( Query( stringContains( 1, stringValue( "a" ) ) ), equalTo( asList( 1L, 3L, 4L, 5L ) ) );
					assertThat( Query( stringContains( 1, stringValue( "apa" ) ) ), equalTo( asList( 3L, 4L, 5L ) ) );
					assertThat( Query( stringContains( 1, stringValue( "apa*" ) ) ), equalTo( Collections.emptyList() ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexEndsWithWithDuplicated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexEndsWithWithDuplicated()
			  {
					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a" ), add(2L, Descriptor.schema(), "A"), add(3L, Descriptor.schema(), "apa"), add(4L, Descriptor.schema(), "apa"), add(5L, Descriptor.schema(), "longapa"), add(6L, Descriptor.schema(), "apalong") ) );

					assertThat( Query( stringSuffix( 1, stringValue( "a" ) ) ), equalTo( asList( 1L, 3L, 4L, 5L ) ) );
					assertThat( Query( stringSuffix( 1, stringValue( "apa" ) ) ), equalTo( asList( 3L, 4L, 5L ) ) );
					assertThat( Query( stringSuffix( 1, stringValue( "apa*" ) ) ), equalTo( Collections.emptyList() ) );
					assertThat( Query( stringSuffix( 1, stringValue( "" ) ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L, 6L ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexShouldHandleLargeAmountOfDuplicatesString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexShouldHandleLargeAmountOfDuplicatesString()
			  {
					DoTestShouldHandleLargeAmountOfDuplicates( "this is a semi-long string that will need to be split" );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexShouldHandleLargeAmountOfDuplicatesStringArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexShouldHandleLargeAmountOfDuplicatesStringArray()
			  {
					Value arrayValue = NextRandomValidArrayValue();
					DoTestShouldHandleLargeAmountOfDuplicates( arrayValue );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doTestShouldHandleLargeAmountOfDuplicates(Object value) throws Exception
			  internal virtual void DoTestShouldHandleLargeAmountOfDuplicates( object value )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
					IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
					IList<long> nodeIds = new List<long>();
					for ( long i = 0; i < 1000; i++ )
					{
						 nodeIds.Add( i );
						 updates.Add( add( i, Descriptor.schema(), value ) );
					}
					UpdateAndCommit( updates );

					assertThat( Query( exists( 1 ) ), equalTo( nodeIds ) );
			  }

			  internal virtual Value NextRandomValidArrayValue()
			  {
					Value value;
					do
					{
						 value = Random.randomValues().nextArray();
						 // todo remove when spatial is supported by all
					} while ( !TestSuite.supportsSpatial() && Values.isGeometryArray(value) );
					return value;
			  }
		 }

		 // This behaviour is expected by Unique indexes

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class Unique extends SimpleIndexAccessorCompatibility
		 public class Unique : SimpleIndexAccessorCompatibility
		 {
			  public Unique( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.uniqueForLabel( 1000, 100 ) )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closingAnOnlineIndexUpdaterMustNotThrowEvenIfItHasBeenFedConflictingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ClosingAnOnlineIndexUpdaterMustNotThrowEvenIfItHasBeenFedConflictingData()
			  {
					// The reason is that we use and close IndexUpdaters in commit - not in prepare - and therefor
					// we cannot have them go around and throw exceptions, because that could potentially break
					// recovery.
					// Conflicting data can happen because of faulty data coercion. These faults are resolved by
					// the exact-match filtering we do on index seeks.

					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a" ), add(2L, Descriptor.schema(), "a") ) );

					assertThat( Query( exact( 1, "a" ) ), equalTo( asList( 1L, 2L ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekAndScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestIndexSeekAndScan()
			  {
					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a" ), add(2L, Descriptor.schema(), "b"), add(3L, Descriptor.schema(), "c") ) );

					assertThat( Query( exact( 1, "a" ) ), equalTo( singletonList( 1L ) ) );
					assertThat( Query( IndexQuery.exists( 1 ) ), equalTo( asList( 1L, 2L, 3L ) ) );
			  }
		 }
	}

}