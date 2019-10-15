using System;
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
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using Assume = org.junit.Assume;
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using SimpleNodeValueClient = Neo4Net.Storageengine.Api.schema.SimpleNodeValueClient;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using PointArray = Neo4Net.Values.Storable.PointArray;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using ValueType = Neo4Net.Values.Storable.ValueType;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.IndexQuery.exists;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.IndexQuery.range;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.epochDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.GEOMETRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.GEOMETRY_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.booleanArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite that provides test cases for verifying" + " IndexProvider implementations. Each index provider that is to be tested by this suite" + " must create their own test class extending IndexProviderCompatibilityTestSuite." + " The @Ignore annotation doesn't prevent these tests to run, it rather removes some annoying" + " errors or warnings in some IDEs about test classes needing a public zero-arg constructor.") public abstract class CompositeIndexAccessorCompatibility extends IndexAccessorCompatibility
	public abstract class CompositeIndexAccessorCompatibility : IndexAccessorCompatibility
	{
		 public CompositeIndexAccessorCompatibility( IndexProviderCompatibilityTestSuite testSuite, IndexDescriptor descriptor ) : base( testSuite, descriptor )
		 {
		 }

		 /* testIndexSeekAndScan */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByString()
		 {
			  testIndexScanAndSeekExactWithExact( "a", "b" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByNumber()
		 {
			  testIndexScanAndSeekExactWithExact( 333, 101 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByBoolean() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByBoolean()
		 {
			  testIndexScanAndSeekExactWithExact( true, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByTemporal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByTemporal()
		 {
			  testIndexScanAndSeekExactWithExact( epochDate( 303 ), epochDate( 101 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByStringArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByStringArray()
		 {
			  testIndexScanAndSeekExactWithExact( new string[]{ "a", "c" }, new string[]{ "b", "c" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByNumberArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByNumberArray()
		 {
			  testIndexScanAndSeekExactWithExact( new int[]{ 333, 900 }, new int[]{ 101, 900 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByBooleanArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByBooleanArray()
		 {
			  testIndexScanAndSeekExactWithExact( new bool[]{ true, true }, new bool[]{ false, true } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByTemporalArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByTemporalArray()
		 {
			  testIndexScanAndSeekExactWithExact( DateArray( 333, 900 ), DateArray( 101, 900 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexScanAndSeekExactWithExact(Object a, Object b) throws Exception
		 private void TestIndexScanAndSeekExactWithExact( object a, object b )
		 {
			  TestIndexScanAndSeekExactWithExact( Values.of( a ), Values.of( b ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexScanAndSeekExactWithExact(org.neo4j.values.storable.Value a, org.neo4j.values.storable.Value b) throws Exception
		 private void TestIndexScanAndSeekExactWithExact( Value a, Value b )
		 {
			  UpdateAndCommit( asList( Add( 1L, Descriptor.schema(), a, a ), Add(2L, Descriptor.schema(), b, b), Add(3L, Descriptor.schema(), a, b) ) );

			  assertThat( Query( exact( 0, a ), exact( 1, a ) ), equalTo( singletonList( 1L ) ) );
			  assertThat( Query( exact( 0, b ), exact( 1, b ) ), equalTo( singletonList( 2L ) ) );
			  assertThat( Query( exact( 0, a ), exact( 1, b ) ), equalTo( singletonList( 3L ) ) );
			  assertThat( Query( exists( 1 ) ), equalTo( asList( 1L, 2L, 3L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexScanAndSeekExactWithExactByPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexScanAndSeekExactWithExactByPoint()
		 {
			  Assume.assumeTrue( "Assume support for spatial", TestSuite.supportsSpatial() );

			  PointValue gps = pointValue( WGS84, 12.6, 56.7 );
			  PointValue car = pointValue( Cartesian, 12.6, 56.7 );
			  PointValue gps3d = pointValue( CoordinateReferenceSystem.WGS84_3D, 12.6, 56.7, 100.0 );
			  PointValue car3d = pointValue( CoordinateReferenceSystem.Cartesian_3D, 12.6, 56.7, 100.0 );

			  UpdateAndCommit( asList( add( 1L, Descriptor.schema(), gps, gps ), add(2L, Descriptor.schema(), car, car), add(3L, Descriptor.schema(), gps, car), add(4L, Descriptor.schema(), gps3d, gps3d), add(5L, Descriptor.schema(), car3d, car3d), add(6L, Descriptor.schema(), gps, car3d) ) );

			  assertThat( Query( exact( 0, gps ), exact( 1, gps ) ), equalTo( singletonList( 1L ) ) );
			  assertThat( Query( exact( 0, car ), exact( 1, car ) ), equalTo( singletonList( 2L ) ) );
			  assertThat( Query( exact( 0, gps ), exact( 1, car ) ), equalTo( singletonList( 3L ) ) );
			  assertThat( Query( exact( 0, gps3d ), exact( 1, gps3d ) ), equalTo( singletonList( 4L ) ) );
			  assertThat( Query( exact( 0, car3d ), exact( 1, car3d ) ), equalTo( singletonList( 5L ) ) );
			  assertThat( Query( exact( 0, gps ), exact( 1, car3d ) ), equalTo( singletonList( 6L ) ) );
			  assertThat( Query( exists( 1 ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L, 6L ) ) );
		 }

		 /* testIndexExactAndRangeExact_Range */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeByString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeByString()
		 {
			  TestIndexSeekExactWithRange( Values.of( "a" ), Values.of( "b" ), Values.of( "Anabelle" ), Values.of( "Anna" ), Values.of( "Bob" ), Values.of( "Harriet" ), Values.of( "William" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeByNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeByNumber()
		 {
			  TestIndexSeekExactWithRange( Values.of( 303 ), Values.of( 101 ), Values.of( 111 ), Values.of( 222 ), Values.of( 333 ), Values.of( 444 ), Values.of( 555 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeByTemporal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeByTemporal()
		 {
			  TestIndexSeekExactWithRange( epochDate( 303 ), epochDate( 101 ), epochDate( 111 ), epochDate( 222 ), epochDate( 333 ), epochDate( 444 ), epochDate( 555 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeByBoolean() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeByBoolean()
		 {
			  Assume.assumeTrue( "Assume support for boolean range queries", TestSuite.supportsBooleanRangeQueries() );

			  TestIndexSeekExactWithRangeByBooleanType( BooleanValue.TRUE, BooleanValue.FALSE, BooleanValue.FALSE, BooleanValue.TRUE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeByStringArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeByStringArray()
		 {
			  TestIndexSeekExactWithRange( stringArray( "a", "c" ), stringArray( "b", "c" ), stringArray( "Anabelle", "c" ), stringArray( "Anna", "c" ), stringArray( "Bob", "c" ), stringArray( "Harriet", "c" ), stringArray( "William", "c" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeByNumberArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeByNumberArray()
		 {
			  TestIndexSeekExactWithRange( longArray( new long[]{ 333, 9000 } ), longArray( new long[]{ 101, 900 } ), longArray( new long[]{ 111, 900 } ), longArray( new long[]{ 222, 900 } ), longArray( new long[]{ 333, 900 } ), longArray( new long[]{ 444, 900 } ), longArray( new long[]{ 555, 900 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeByBooleanArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeByBooleanArray()
		 {
			  TestIndexSeekExactWithRange( booleanArray( new bool[]{ true, true } ), booleanArray( new bool[]{ false, false } ), booleanArray( new bool[]{ false, false } ), booleanArray( new bool[]{ false, true } ), booleanArray( new bool[]{ true, false } ), booleanArray( new bool[]{ true, true } ), booleanArray( new bool[]{ true, true, true } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeByTemporalArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeByTemporalArray()
		 {
			  TestIndexSeekExactWithRange( DateArray( 303, 900 ), DateArray( 101, 900 ), DateArray( 111, 900 ), DateArray( 222, 900 ), DateArray( 333, 900 ), DateArray( 444, 900 ), DateArray( 555, 900 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithRangeBySpatial() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithRangeBySpatial()
		 {
			  TestIndexSeekExactWithRange( intValue( 100 ), intValue( 10 ), pointValue( WGS84, -10D, -10D ), pointValue( WGS84, -1D, -1D ), pointValue( WGS84, 0D, 0D ), pointValue( WGS84, 1D, 1D ), pointValue( WGS84, 10D, 10D ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexSeekExactWithRange(org.neo4j.values.storable.Value base1, org.neo4j.values.storable.Value base2, org.neo4j.values.storable.Value obj1, org.neo4j.values.storable.Value obj2, org.neo4j.values.storable.Value obj3, org.neo4j.values.storable.Value obj4, org.neo4j.values.storable.Value obj5) throws Exception
		 private void TestIndexSeekExactWithRange( Value base1, Value base2, Value obj1, Value obj2, Value obj3, Value obj4, Value obj5 )
		 {
			  Assume.assumeTrue( "Assume support for granular composite queries", TestSuite.supportsGranularCompositeQueries() );

			  UpdateAndCommit( asList( Add( 1L, Descriptor.schema(), base1, obj1 ), Add(2L, Descriptor.schema(), base1, obj2), Add(3L, Descriptor.schema(), base1, obj3), Add(4L, Descriptor.schema(), base1, obj4), Add(5L, Descriptor.schema(), base1, obj5), Add(6L, Descriptor.schema(), base2, obj1), Add(7L, Descriptor.schema(), base2, obj2), Add(8L, Descriptor.schema(), base2, obj3), Add(9L, Descriptor.schema(), base2, obj4), Add(10L, Descriptor.schema(), base2, obj5) ) );

			  assertThat( Query( exact( 0, base1 ), range( 1, obj2, true, obj4, false ) ), equalTo( asList( 2L, 3L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj4, true, null, false ) ), equalTo( asList( 4L, 5L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj4, false, null, true ) ), equalTo( singletonList( 5L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj5, false, obj2, true ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, null, false, obj3, false ) ), equalTo( asList( 1L, 2L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, null, true, obj3, true ) ), equalTo( asList( 1L, 2L, 3L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj1, false, obj2, true ) ), equalTo( singletonList( 2L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj1, false, obj3, false ) ), equalTo( singletonList( 2L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj2, true, obj4, false ) ), equalTo( asList( 7L, 8L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj4, true, null, false ) ), equalTo( asList( 9L, 10L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj4, false, null, true ) ), equalTo( singletonList( 10L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj5, false, obj2, true ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, null, false, obj3, false ) ), equalTo( asList( 6L, 7L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, null, true, obj3, true ) ), equalTo( asList( 6L, 7L, 8L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj1, false, obj2, true ) ), equalTo( singletonList( 7L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj1, false, obj3, false ) ), equalTo( singletonList( 7L ) ) );

			  ValueGroup valueGroup = obj1.ValueGroup();
			  if ( valueGroup != GEOMETRY && valueGroup != GEOMETRY_ARRAY )
			  {
					assertThat( Query( exact( 0, base1 ), range( 1, valueGroup ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L ) ) );
					assertThat( Query( exact( 0, base2 ), range( 1, valueGroup ) ), equalTo( asList( 6L, 7L, 8L, 9L, 10L ) ) );
			  }
			  else
			  {
					CoordinateReferenceSystem crs = GetCrs( obj1 );
					assertThat( Query( exact( 0, base1 ), range( 1, crs ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L ) ) );
					assertThat( Query( exact( 0, base2 ), range( 1, crs ) ), equalTo( asList( 6L, 7L, 8L, 9L, 10L ) ) );
			  }
		 }

		 private CoordinateReferenceSystem GetCrs( Value value )
		 {
			  if ( Values.isGeometryValue( value ) )
			  {
					return ( ( PointValue ) value ).CoordinateReferenceSystem;
			  }
			  else if ( Values.isGeometryArray( value ) )
			  {
					PointArray array = ( PointArray ) value;
					return array.PointValue( 0 ).CoordinateReferenceSystem;
			  }
			  throw new System.ArgumentException( "Expected some geometry value to get CRS from, but got " + value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexSeekExactWithRangeByBooleanType(org.neo4j.values.storable.Value base1, org.neo4j.values.storable.Value base2, org.neo4j.values.storable.Value obj1, org.neo4j.values.storable.Value obj2) throws Exception
		 private void TestIndexSeekExactWithRangeByBooleanType( Value base1, Value base2, Value obj1, Value obj2 )
		 {
			  UpdateAndCommit( asList( Add( 1L, Descriptor.schema(), base1, obj1 ), Add(2L, Descriptor.schema(), base1, obj2), Add(3L, Descriptor.schema(), base2, obj1), Add(4L, Descriptor.schema(), base2, obj2) ) );

			  assertThat( Query( exact( 0, base1 ), range( 1, obj1, true, obj2, true ) ), equalTo( asList( 1L, 2L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj1, false, obj2, true ) ), equalTo( singletonList( 2L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj1, true, obj2, false ) ), equalTo( singletonList( 1L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj1, false, obj2, false ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, null, true, obj2, true ) ), equalTo( asList( 1L, 2L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj1, true, null, true ) ), equalTo( asList( 1L, 2L ) ) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj1.ValueGroup() ) ), equalTo(asList(1L, 2L)) );
			  assertThat( Query( exact( 0, base1 ), range( 1, obj2, true, obj1, true ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj1, true, obj2, true ) ), equalTo( asList( 3L, 4L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj1, false, obj2, true ) ), equalTo( singletonList( 4L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj1, true, obj2, false ) ), equalTo( singletonList( 3L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj1, false, obj2, false ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, null, true, obj2, true ) ), equalTo( asList( 3L, 4L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj1, true, null, true ) ), equalTo( asList( 3L, 4L ) ) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj1.ValueGroup() ) ), equalTo(asList(3L, 4L)) );
			  assertThat( Query( exact( 0, base2 ), range( 1, obj2, true, obj1, true ) ), equalTo( EMPTY_LIST ) );
		 }

		 /* stringPrefix */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithPrefixRangeByString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithPrefixRangeByString()
		 {
			  Assume.assumeTrue( "Assume support for granular composite queries", TestSuite.supportsGranularCompositeQueries() );

			  UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a", "a" ), add(2L, Descriptor.schema(), "a", "A"), add(3L, Descriptor.schema(), "a", "apa"), add(4L, Descriptor.schema(), "a", "apA"), add(5L, Descriptor.schema(), "a", "b"), add(6L, Descriptor.schema(), "b", "a"), add(7L, Descriptor.schema(), "b", "A"), add(8L, Descriptor.schema(), "b", "apa"), add(9L, Descriptor.schema(), "b", "apA"), add(10L, Descriptor.schema(), "b", "b") ) );

			  assertThat( Query( exact( 0, "a" ), IndexQuery.stringPrefix( 1, stringValue( "a" ) ) ), equalTo( asList( 1L, 3L, 4L ) ) );
			  assertThat( Query( exact( 0, "a" ), IndexQuery.stringPrefix( 1, stringValue( "A" ) ) ), equalTo( Collections.singletonList( 2L ) ) );
			  assertThat( Query( exact( 0, "a" ), IndexQuery.stringPrefix( 1, stringValue( "ba" ) ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( exact( 0, "a" ), IndexQuery.stringPrefix( 1, stringValue( "" ) ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L ) ) );
			  assertThat( Query( exact( 0, "b" ), IndexQuery.stringPrefix( 1, stringValue( "a" ) ) ), equalTo( asList( 6L, 8L, 9L ) ) );
			  assertThat( Query( exact( 0, "b" ), IndexQuery.stringPrefix( 1, stringValue( "A" ) ) ), equalTo( Collections.singletonList( 7L ) ) );
			  assertThat( Query( exact( 0, "b" ), IndexQuery.stringPrefix( 1, stringValue( "ba" ) ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( exact( 0, "b" ), IndexQuery.stringPrefix( 1, stringValue( "" ) ) ), equalTo( asList( 6L, 7L, 8L, 9L, 10L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekPrefixRangeWithExistsByString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekPrefixRangeWithExistsByString()
		 {
			  Assume.assumeTrue( "Assume support for granular composite queries", TestSuite.supportsGranularCompositeQueries() );

			  UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a", 1 ), add(2L, Descriptor.schema(), "A", epochDate(2)), add(3L, Descriptor.schema(), "apa", "..."), add(4L, Descriptor.schema(), "apA", "someString"), add(5L, Descriptor.schema(), "b", true), add(6L, Descriptor.schema(), "a", 100), add(7L, Descriptor.schema(), "A", epochDate(200)), add(8L, Descriptor.schema(), "apa", "!!!"), add(9L, Descriptor.schema(), "apA", "someOtherString"), add(10L, Descriptor.schema(), "b", false) ) );

			  assertThat( Query( IndexQuery.stringPrefix( 0, stringValue( "a" ) ), exists( 1 ) ), equalTo( asList( 1L, 3L, 4L, 6L, 8L, 9L ) ) );
			  assertThat( Query( IndexQuery.stringPrefix( 0, stringValue( "A" ) ), exists( 1 ) ), equalTo( asList( 2L, 7L ) ) );
			  assertThat( Query( IndexQuery.stringPrefix( 0, stringValue( "ba" ) ), exists( 1 ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( IndexQuery.stringPrefix( 0, stringValue( "" ) ), exists( 1 ) ), equalTo( asList( 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L, 10L ) ) );
		 }

		 /* testIndexSeekExactWithExists */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsByString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsByString()
		 {
			  testIndexSeekExactWithExists( "a", "b" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsByNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsByNumber()
		 {
			  testIndexSeekExactWithExists( 303, 101 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsByTemporal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsByTemporal()
		 {
			  testIndexSeekExactWithExists( epochDate( 303 ), epochDate( 101 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsByBoolean() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsByBoolean()
		 {
			  testIndexSeekExactWithExists( true, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsByStringArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsByStringArray()
		 {
			  testIndexSeekExactWithExists( new string[]{ "a", "c" }, new string[]{ "b", "c" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsByNumberArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsByNumberArray()
		 {
			  testIndexSeekExactWithExists( new long[]{ 303, 900 }, new long[]{ 101, 900 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsByBooleanArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsByBooleanArray()
		 {
			  testIndexSeekExactWithExists( new bool[]{ true, true }, new bool[]{ false, true } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsByTemporalArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsByTemporalArray()
		 {
			  testIndexSeekExactWithExists( DateArray( 303, 900 ), DateArray( 101, 900 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsBySpatial() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsBySpatial()
		 {
			  testIndexSeekExactWithExists( pointValue( WGS84, 100D, 100D ), pointValue( WGS84, 0D, 0D ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekExactWithExistsBySpatialArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekExactWithExistsBySpatialArray()
		 {
			  testIndexSeekExactWithExists( pointArray( new PointValue[] { pointValue( Cartesian, 100D, 100D ), pointValue( Cartesian, 101D, 101D ) } ), pointArray( new PointValue[] { pointValue( Cartesian, 0D, 0D ), pointValue( Cartesian, 1D, 1D ) } ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexSeekExactWithExists(Object a, Object b) throws Exception
		 private void TestIndexSeekExactWithExists( object a, object b )
		 {
			  TestIndexSeekExactWithExists( Values.of( a ), Values.of( b ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexSeekExactWithExists(org.neo4j.values.storable.Value a, org.neo4j.values.storable.Value b) throws Exception
		 private void TestIndexSeekExactWithExists( Value a, Value b )
		 {
			  Assume.assumeTrue( "Assume support for granular composite queries", TestSuite.supportsGranularCompositeQueries() );
			  UpdateAndCommit( asList( Add( 1L, Descriptor.schema(), a, Values.of(1) ), Add(2L, Descriptor.schema(), b, Values.of("abv")), Add(3L, Descriptor.schema(), a, Values.of(false)) ) );

			  assertThat( Query( exact( 0, a ), exists( 1 ) ), equalTo( asList( 1L, 3L ) ) );
			  assertThat( Query( exact( 0, b ), exists( 1 ) ), equalTo( singletonList( 2L ) ) );
		 }

		 /* testIndexSeekRangeWithExists */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsByString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsByString()
		 {
			  testIndexSeekRangeWithExists( "Anabelle", "Anna", "Bob", "Harriet", "William" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsByNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsByNumber()
		 {
			  testIndexSeekRangeWithExists( -5, 0, 5.5, 10.0, 100.0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsByTemporal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsByTemporal()
		 {
			  DateTimeValue d1 = datetime( 9999, 100, ZoneId.of( "+18:00" ) );
			  DateTimeValue d2 = datetime( 10000, 100, ZoneId.of( "UTC" ) );
			  DateTimeValue d3 = datetime( 10000, 100, ZoneId.of( "+01:00" ) );
			  DateTimeValue d4 = datetime( 10000, 100, ZoneId.of( "Europe/Stockholm" ) );
			  DateTimeValue d5 = datetime( 10000, 100, ZoneId.of( "+03:00" ) );
			  testIndexSeekRangeWithExists( d1, d2, d3, d4, d5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsByBoolean() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsByBoolean()
		 {
			  Assume.assumeTrue( "Assume support for granular composite queries", TestSuite.supportsGranularCompositeQueries() );
			  Assume.assumeTrue( "Assume support for boolean range queries", TestSuite.supportsBooleanRangeQueries() );

			  UpdateAndCommit( asList( add( 1L, Descriptor.schema(), false, "someString" ), add(2L, Descriptor.schema(), true, 1000) ) );

			  assertThat( Query( range( 0, BooleanValue.FALSE, true, BooleanValue.TRUE, true ), exists( 1 ) ), equalTo( asList( 1L, 2L ) ) );
			  assertThat( Query( range( 0, BooleanValue.FALSE, false, BooleanValue.TRUE, true ), exists( 1 ) ), equalTo( singletonList( 2L ) ) );
			  assertThat( Query( range( 0, BooleanValue.FALSE, true, BooleanValue.TRUE, false ), exists( 1 ) ), equalTo( singletonList( 1L ) ) );
			  assertThat( Query( range( 0, BooleanValue.FALSE, false, BooleanValue.TRUE, false ), exists( 1 ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( range( 0, null, true, BooleanValue.TRUE, true ), exists( 1 ) ), equalTo( asList( 1L, 2L ) ) );
			  assertThat( Query( range( 0, BooleanValue.FALSE, true, null, true ), exists( 1 ) ), equalTo( asList( 1L, 2L ) ) );
			  assertThat( Query( range( 0, BooleanValue.TRUE, true, BooleanValue.FALSE, true ), exists( 1 ) ), equalTo( EMPTY_LIST ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsByStringArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsByStringArray()
		 {
			  testIndexSeekRangeWithExists( new string[]{ "Anabelle", "Anabelle" }, new string[]{ "Anabelle", "Anablo" }, new string[]{ "Anna", "Anabelle" }, new string[]{ "Anna", "Anablo" }, new string[]{ "Bob" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsByNumberArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsByNumberArray()
		 {
			  testIndexSeekRangeWithExists( new long[]{ 303, 303 }, new long[]{ 303, 404 }, new long[]{ 600, 303 }, new long[]{ 600, 404 }, new long[]{ 900 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsByBooleanArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsByBooleanArray()
		 {
			  testIndexSeekRangeWithExists( new bool[]{ false, false }, new bool[]{ false, true }, new bool[]{ true, false }, new bool[]{ true, true }, new bool[]{ true, true, false } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsByTemporalArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsByTemporalArray()
		 {
			  testIndexSeekRangeWithExists( DateArray( 303, 303 ), DateArray( 303, 404 ), DateArray( 404, 303 ), DateArray( 404, 404 ), DateArray( 404, 404, 303 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsBySpatial() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsBySpatial()
		 {
			  testIndexSeekRangeWithExists( pointValue( Cartesian, 0D, 0D ), pointValue( Cartesian, 1D, 1D ), pointValue( Cartesian, 2D, 2D ), pointValue( Cartesian, 3D, 3D ), pointValue( Cartesian, 4D, 4D ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexSeekRangeWithExistsBySpatialArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIndexSeekRangeWithExistsBySpatialArray()
		 {
			  testIndexSeekRangeWithExists( pointArray( new PointValue[] { pointValue( Cartesian, 0D, 0D ), pointValue( Cartesian, 0D, 1D ) } ), pointArray( new PointValue[] { pointValue( Cartesian, 10D, 1D ), pointValue( Cartesian, 10D, 2D ) } ), pointArray( new PointValue[] { pointValue( Cartesian, 20D, 2D ), pointValue( Cartesian, 20D, 3D ) } ), pointArray( new PointValue[] { pointValue( Cartesian, 30D, 3D ), pointValue( Cartesian, 30D, 4D ) } ), pointArray( new PointValue[] { pointValue( Cartesian, 40D, 4D ), pointValue( Cartesian, 40D, 5D ) } ) );
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
			  for ( long id = 0; id < 10_000; id++ )
			  {
					IndexEntryUpdate<SchemaDescriptor> update;
					do
					{
						 update = Add( id, Descriptor.schema(), Random.randomValues().nextValueOfTypes(types), Random.randomValues().nextValueOfTypes(types) );
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
					IList<long> hits = Query( exact( 0, update.Values()[0] ), exact(1, update.Values()[1]) );
					assertEquals( update + " " + hits.ToString(), 1, hits.Count );
					assertThat( single( hits ), equalTo( update.EntityId ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexSeekRangeWithExists(Object obj1, Object obj2, Object obj3, Object obj4, Object obj5) throws Exception
		 private void TestIndexSeekRangeWithExists( object obj1, object obj2, object obj3, object obj4, object obj5 )
		 {
			  TestIndexSeekRangeWithExists( Values.of( obj1 ), Values.of( obj2 ), Values.of( obj3 ), Values.of( obj4 ), Values.of( obj5 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testIndexSeekRangeWithExists(org.neo4j.values.storable.Value obj1, org.neo4j.values.storable.Value obj2, org.neo4j.values.storable.Value obj3, org.neo4j.values.storable.Value obj4, org.neo4j.values.storable.Value obj5) throws Exception
		 private void TestIndexSeekRangeWithExists( Value obj1, Value obj2, Value obj3, Value obj4, Value obj5 )
		 {
			  Assume.assumeTrue( "Assume support for granular composite queries", TestSuite.supportsGranularCompositeQueries() );
			  UpdateAndCommit( asList( Add( 1L, Descriptor.schema(), obj1, Values.of(100) ), Add(2L, Descriptor.schema(), obj2, Values.of("someString")), Add(3L, Descriptor.schema(), obj3, Values.of(epochDate(300))), Add(4L, Descriptor.schema(), obj4, Values.of(true)), Add(5L, Descriptor.schema(), obj5, Values.of(42)) ) );

			  assertThat( Query( range( 0, obj2, true, obj4, false ), exists( 1 ) ), equalTo( asList( 2L, 3L ) ) );
			  assertThat( Query( range( 0, obj4, true, null, false ), exists( 1 ) ), equalTo( asList( 4L, 5L ) ) );
			  assertThat( Query( range( 0, obj4, false, null, true ), exists( 1 ) ), equalTo( singletonList( 5L ) ) );
			  assertThat( Query( range( 0, obj5, false, obj2, true ), exists( 1 ) ), equalTo( EMPTY_LIST ) );
			  assertThat( Query( range( 0, null, false, obj3, false ), exists( 1 ) ), equalTo( asList( 1L, 2L ) ) );
			  assertThat( Query( range( 0, null, true, obj3, true ), exists( 1 ) ), equalTo( asList( 1L, 2L, 3L ) ) );
			  ValueGroup valueGroup = obj1.ValueGroup();
			  if ( valueGroup != GEOMETRY && valueGroup != GEOMETRY_ARRAY )
			  {
					// This cannot be done for spatial values because each bound in a spatial query needs a coordinate reference system,
					// and those are provided by Value instances, e.g. PointValue
					assertThat( Query( range( 0, obj1.ValueGroup() ), exists(1) ), equalTo(asList(1L, 2L, 3L, 4L, 5L)) );
			  }
			  assertThat( Query( range( 0, obj1, false, obj2, true ), exists( 1 ) ), equalTo( singletonList( 2L ) ) );
			  assertThat( Query( range( 0, obj1, false, obj3, false ), exists( 1 ) ), equalTo( singletonList( 2L ) ) );
		 }

		 /* IndexOrder */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderNumberAscending() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderNumberAscending()
		 {
			  object o0 = 0;
			  object o1 = 1;
			  object o2 = 2;
			  object o3 = 3;
			  object o4 = 4;
			  object o5 = 5;
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderNumberDescending() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderNumberDescending()
		 {
			  object o0 = 0;
			  object o1 = 1;
			  object o2 = 2;
			  object o3 = 3;
			  object o4 = 4;
			  object o5 = 5;
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderStringAscending() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderStringAscending()
		 {
			  object o0 = "0";
			  object o1 = "1";
			  object o2 = "2";
			  object o3 = "3";
			  object o4 = "4";
			  object o5 = "5";
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeSeekInOrderStringDescending() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeSeekInOrderStringDescending()
		 {
			  object o0 = "0";
			  object o1 = "1";
			  object o2 = "2";
			  object o3 = "3";
			  object o4 = "4";
			  object o5 = "5";
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.ASCENDING, o0, o1, o2, o3, o4, o5 );
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
			  ShouldSeekInOrderExactWithRange( IndexOrder.DESCENDING, o0, o1, o2, o3, o4, o5 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldSeekInOrderExactWithRange(org.neo4j.internal.kernel.api.IndexOrder order, Object o0, Object o1, Object o2, Object o3, Object o4, Object o5) throws Exception
		 private void ShouldSeekInOrderExactWithRange( IndexOrder order, object o0, object o1, object o2, object o3, object o4, object o5 )
		 {
			  object baseValue = 1; // Todo use random value instead
			  IndexQuery exact = exact( 100, baseValue );
			  IndexQuery range = range( 200, Values.of( o0 ), true, Values.of( o5 ), true );
			  IndexOrder[] indexOrders = OrderCapability( exact, range );
			  Assume.assumeTrue( "Assume support for order " + order, ArrayUtils.contains( indexOrders, order ) );

			  UpdateAndCommit( asList( Add( 1, Descriptor.schema(), baseValue, o0 ), Add(1, Descriptor.schema(), baseValue, o5), Add(1, Descriptor.schema(), baseValue, o1), Add(1, Descriptor.schema(), baseValue, o4), Add(1, Descriptor.schema(), baseValue, o2), Add(1, Descriptor.schema(), baseValue, o3) ) );

			  SimpleNodeValueClient client = new SimpleNodeValueClient();
			  using ( IDisposable ignored = Query( client, order, exact, range ) )
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
					Value[] value = new Value[]{ Random.nextValue( valueType ), Random.nextValue( valueType ) };
					UpdateAndCommit( singletonList( IndexEntryUpdate.Add( entityId, Descriptor.schema(), value ) ) );
					assertEquals( singletonList( entityId ), Query( ExactQuery( value ) ) );

					// when
					Value[] newValue;
					do
					{
						 newValue = new Value[]{ Random.nextValue( valueType ), Random.nextValue( valueType ) };
					} while ( ValueTuple.of( value ).Equals( ValueTuple.of( newValue ) ) );
					UpdateAndCommit( singletonList( IndexEntryUpdate.Change( entityId, Descriptor.schema(), value, newValue ) ) );

					// then
					assertEquals( emptyList(), Query(ExactQuery(value)) );
					assertEquals( singletonList( entityId ), Query( ExactQuery( newValue ) ) );
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
					Value[] value = new Value[]{ Random.nextValue( valueType ), Random.nextValue( valueType ) };
					UpdateAndCommit( singletonList( IndexEntryUpdate.Add( entityId, Descriptor.schema(), value ) ) );
					assertEquals( singletonList( entityId ), Query( ExactQuery( value ) ) );

					// when
					UpdateAndCommit( singletonList( IndexEntryUpdate.Remove( entityId, Descriptor.schema(), value ) ) );

					// then
					assertEquals( emptyList(), Query(ExactQuery(value)) );
			  }
		 }

		 private static IndexQuery[] ExactQuery( Value[] values )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.of( values ).map( v => IndexQuery.exact( 0, v ) ).toArray( IndexQuery[]::new );
		 }

		 // This behaviour is expected by General indexes

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class General extends CompositeIndexAccessorCompatibility
		 public class General : CompositeIndexAccessorCompatibility
		 {
			  public General( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.forLabel( 1000, 100, 200 ) )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByString()
			  {
					object value = "a";
					TestDuplicatesInIndexSeek( value );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByNumber()
			  {
					testDuplicatesInIndexSeek( 333 );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByPoint()
			  {
					Assume.assumeTrue( "Assume support for spatial", TestSuite.supportsSpatial() );
					testDuplicatesInIndexSeek( pointValue( WGS84, 12.6, 56.7 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByBoolean() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByBoolean()
			  {
					testDuplicatesInIndexSeek( true );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByTemporal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByTemporal()
			  {
					testDuplicatesInIndexSeek( ofEpochDay( 303 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByStringArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByStringArray()
			  {
					testDuplicatesInIndexSeek( new string[]{ "anabelle", "anabollo" } );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByNumberArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByNumberArray()
			  {
					testDuplicatesInIndexSeek( new long[]{ 303, 606 } );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByBooleanArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByBooleanArray()
			  {
					testDuplicatesInIndexSeek( new bool[]{ true, false } );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByTemporalArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByTemporalArray()
			  {
					testDuplicatesInIndexSeek( DateArray( 303, 606 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDuplicatesInIndexSeekByPointArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TestDuplicatesInIndexSeekByPointArray()
			  {
					Assume.assumeTrue( "Assume support for spatial", TestSuite.supportsSpatial() );
					testDuplicatesInIndexSeek( pointArray( new PointValue[]{ pointValue( WGS84, 12.6, 56.7 ), pointValue( WGS84, 12.6, 56.7 ) } ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testDuplicatesInIndexSeek(Object value) throws Exception
			  internal virtual void TestDuplicatesInIndexSeek( object value )
			  {
					TestDuplicatesInIndexSeek( Values.of( value ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testDuplicatesInIndexSeek(org.neo4j.values.storable.Value value) throws Exception
			  internal virtual void TestDuplicatesInIndexSeek( Value value )
			  {
					UpdateAndCommit( asList( Add( 1L, Descriptor.schema(), value, value ), Add(2L, Descriptor.schema(), value, value) ) );

					assertThat( Query( exact( 0, value ), exact( 1, value ) ), equalTo( asList( 1L, 2L ) ) );
			  }
		 }

		 // This behaviour is expected by Unique indexes

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class Unique extends CompositeIndexAccessorCompatibility
		 public class Unique : CompositeIndexAccessorCompatibility
		 {
			  public Unique( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.uniqueForLabel( 1000, 100, 200 ) )
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

					UpdateAndCommit( asList( add( 1L, Descriptor.schema(), "a", "a" ), add(2L, Descriptor.schema(), "a", "a") ) );

					assertThat( Query( exact( 0, "a" ), exact( 1, "a" ) ), equalTo( asList( 1L, 2L ) ) );
			  }
		 }

		 private static ArrayValue DateArray( params int[] epochDays )
		 {
			  LocalDate[] localDates = new LocalDate[epochDays.Length];
			  for ( int i = 0; i < epochDays.Length; i++ )
			  {
					localDates[i] = ofEpochDay( epochDays[i] );
			  }
			  return Values.dateArray( localDates );
		 }

		 private static IndexEntryUpdate<SchemaDescriptor> Add( long nodeId, SchemaDescriptor schema, object value1, object value2 )
		 {
			  return add( nodeId, schema, Values.of( value1 ), Values.of( value2 ) );
		 }

		 private static IndexEntryUpdate<SchemaDescriptor> Add( long nodeId, SchemaDescriptor schema, Value value1, Value value2 )
		 {
			  return IndexEntryUpdate.Add( nodeId, schema, value1, value2 );
		 }
	}

}