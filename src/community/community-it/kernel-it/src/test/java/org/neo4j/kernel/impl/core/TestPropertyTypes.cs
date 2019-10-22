using System;

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
namespace Neo4Net.Kernel.impl.core
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Node = Neo4Net.GraphDb.Node;
	using Point = Neo4Net.GraphDb.spatial.Point;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Strings = Neo4Net.Helpers.Strings;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TestPropertyTypes : AbstractNeo4NetTestCase
	{
		 private Node _node1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public ExpectedException Thrown = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createInitialNode()
		 public virtual void CreateInitialNode()
		 {
			  _node1 = GraphDb.createNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void deleteInitialNode()
		 public virtual void DeleteInitialNode()
		 {
			  _node1.delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDoubleType()
		 public virtual void TestDoubleType()
		 {
			  double? dValue = 45.678d;
			  string key = "testdouble";
			  _node1.setProperty( key, dValue );
			  NewTransaction();
			  double? propertyValue;
			  propertyValue = ( double? ) _node1.getProperty( key );
			  assertEquals( dValue, propertyValue );
			  dValue = 56784.3243d;
			  _node1.setProperty( key, dValue );
			  NewTransaction();
			  propertyValue = ( double? ) _node1.getProperty( key );
			  assertEquals( dValue, propertyValue );

			  _node1.removeProperty( key );
			  NewTransaction();
			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFloatType()
		 public virtual void TestFloatType()
		 {
			  float? fValue = 45.678f;
			  string key = "testfloat";
			  _node1.setProperty( key, fValue );
			  NewTransaction();

			  float? propertyValue = null;
			  propertyValue = ( float? ) _node1.getProperty( key );
			  assertEquals( fValue, propertyValue );

			  fValue = 5684.3243f;
			  _node1.setProperty( key, fValue );
			  NewTransaction();

			  propertyValue = ( float? ) _node1.getProperty( key );
			  assertEquals( fValue, propertyValue );

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLongType()
		 public virtual void TestLongType()
		 {
			  long? lValue = DateTimeHelper.CurrentUnixTimeMillis();
			  string key = "testlong";
			  _node1.setProperty( key, lValue );
			  NewTransaction();

			  long? propertyValue = null;
			  propertyValue = ( long? ) _node1.getProperty( key );
			  assertEquals( lValue, propertyValue );

			  lValue = DateTimeHelper.CurrentUnixTimeMillis();
			  _node1.setProperty( key, lValue );
			  NewTransaction();

			  propertyValue = ( long? ) _node1.getProperty( key );
			  assertEquals( lValue, propertyValue );

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );

			  _node1.setProperty( "other", 123L );
			  assertEquals( 123L, _node1.getProperty( "other" ) );
			  NewTransaction();
			  assertEquals( 123L, _node1.getProperty( "other" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIntType()
		 public virtual void TestIntType()
		 {
			  int time = ( int )DateTimeHelper.CurrentUnixTimeMillis();
			  int? iValue = time;
			  string key = "testing";
			  _node1.setProperty( key, iValue );
			  NewTransaction();

			  int? propertyValue = null;
			  propertyValue = ( int? ) _node1.getProperty( key );
			  assertEquals( iValue, propertyValue );

			  iValue = ( int ) DateTimeHelper.CurrentUnixTimeMillis();
			  _node1.setProperty( key, iValue );
			  NewTransaction();

			  propertyValue = ( int? ) _node1.getProperty( key );
			  assertEquals( iValue, propertyValue );

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );

			  _node1.setProperty( "other", 123L );
			  assertEquals( 123L, _node1.getProperty( "other" ) );
			  NewTransaction();
			  assertEquals( 123L, _node1.getProperty( "other" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testByteType()
		 public virtual void TestByteType()
		 {
			  sbyte b = unchecked( ( sbyte ) 177 );
			  sbyte? bValue = b;
			  string key = "testbyte";
			  _node1.setProperty( key, bValue );
			  NewTransaction();

			  sbyte? propertyValue = null;
			  propertyValue = ( sbyte? ) _node1.getProperty( key );
			  assertEquals( bValue, propertyValue );

			  bValue = unchecked( ( sbyte ) 200 );
			  _node1.setProperty( key, bValue );
			  NewTransaction();

			  propertyValue = ( sbyte? ) _node1.getProperty( key );
			  assertEquals( bValue, propertyValue );

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testShortType()
		 public virtual void TestShortType()
		 {
			  short? sValue = ( short ) 453;
			  string key = "testshort";
			  _node1.setProperty( key, sValue );
			  NewTransaction();

			  short? propertyValue = null;
			  propertyValue = ( short? ) _node1.getProperty( key );
			  assertEquals( sValue, propertyValue );

			  sValue = ( short ) 5335;
			  _node1.setProperty( key, sValue );
			  NewTransaction();

			  propertyValue = ( short? ) _node1.getProperty( key );
			  assertEquals( sValue, propertyValue );

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCharType()
		 public virtual void TestCharType()
		 {
			  char c = 'c';
			  char? cValue = c;
			  string key = "testchar";
			  _node1.setProperty( key, cValue );
			  NewTransaction();

			  char? propertyValue = null;
			  propertyValue = ( char? ) _node1.getProperty( key );
			  assertEquals( cValue, propertyValue );

			  cValue = 'd';
			  _node1.setProperty( key, cValue );
			  NewTransaction();

			  propertyValue = ( char? ) _node1.getProperty( key );
			  assertEquals( cValue, propertyValue );

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBooleanType()
		 public virtual void TestBooleanType()
		 {
			  string key = "testbool";
			  _node1.setProperty( key, true );
			  NewTransaction();

			  bool? propertyValue = ( bool? ) _node1.getProperty( key );
			  assertEquals( true, propertyValue );

			  _node1.setProperty( key, false );
			  NewTransaction();

			  propertyValue = ( bool? ) _node1.getProperty( key );
			  assertEquals( false, propertyValue );

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPointType()
		 public virtual void TestPointType()
		 {
			  Point point = Values.pointValue( CoordinateReferenceSystem.Cartesian, 1, 1 );
			  string key = "location";
			  _node1.setProperty( key, point );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( point, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPointTypeWithOneOtherProperty()
		 public virtual void TestPointTypeWithOneOtherProperty()
		 {
			  Point point = Values.pointValue( CoordinateReferenceSystem.Cartesian, 1, 1 );
			  string key = "location";
			  _node1.setProperty( "prop1", 1 );
			  _node1.setProperty( key, point );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( point, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPointTypeWithTwoOtherProperties()
		 public virtual void TestPointTypeWithTwoOtherProperties()
		 {
			  Point point = Values.pointValue( CoordinateReferenceSystem.Cartesian, 1, 1 );
			  string key = "location";
			  _node1.setProperty( "prop1", 1 );
			  _node1.setProperty( "prop2", 2 );
			  _node1.setProperty( key, point );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( point, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test3DPointType()
		 public virtual void Test3DPointType()
		 {
			  Point point = Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 1, 1, 1 );
			  string key = "location";
			  _node1.setProperty( key, point );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( point, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test4DPointType()
		 public virtual void Test4DPointType()
		 {
			  Thrown.expect( typeof( Exception ) );
			  _node1.setProperty( "location", Values.unsafePointValue( CoordinateReferenceSystem.Cartesian, 1, 1, 1, 1 ) );
			  NewTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPointArray()
		 public virtual void TestPointArray()
		 {
			  Point[] array = new Point[]{ Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 1, 1, 1 ), Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 2, 1, 3 ) };
			  string key = "testpointarray";
			  _node1.setProperty( key, array );
			  NewTransaction();

			  Point[] propertyValue = null;
			  propertyValue = ( Point[] ) _node1.getProperty( key );
			  assertEquals( array.Length, propertyValue.Length );
			  for ( int i = 0; i < array.Length; i++ )
			  {
					assertEquals( array[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTypeSmallEpochDay()
		 public virtual void TestDateTypeSmallEpochDay()
		 {
			  LocalDate date = DateValue.date( 2018, 1, 31 ).asObjectCopy();
			  string key = "dt";
			  _node1.setProperty( key, date );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( date, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTypeLargeEpochDay()
		 public virtual void TestDateTypeLargeEpochDay()
		 {
			  LocalDate date = DateValue.epochDate( 2147483648L ).asObjectCopy();
			  string key = "dt";
			  _node1.setProperty( key, date );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( date, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateArray()
		 public virtual void TestDateArray()
		 {
			  LocalDate[] array = new LocalDate[]{ DateValue.date( 2018, 1, 31 ).asObjectCopy(), DateValue.epochDate(2147483648L).asObjectCopy() };
			  string key = "testarray";
			  _node1.setProperty( key, array );
			  NewTransaction();

			  LocalDate[] propertyValue = ( LocalDate[] ) _node1.getProperty( key );
			  assertEquals( array.Length, propertyValue.Length );
			  for ( int i = 0; i < array.Length; i++ )
			  {
					assertEquals( array[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLocalTimeTypeSmallNano()
		 public virtual void TestLocalTimeTypeSmallNano()
		 {
			  LocalTime time = LocalTimeValue.localTime( 0, 0, 0, 37 ).asObjectCopy();
			  string key = "dt";
			  _node1.setProperty( key, time );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( time, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLocalTimeTypeLargeNano()
		 public virtual void TestLocalTimeTypeLargeNano()
		 {
			  LocalTime time = LocalTimeValue.localTime( 0, 0, 13, 37 ).asObjectCopy();
			  string key = "dt";
			  _node1.setProperty( key, time );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( time, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLocalTimeArray()
		 public virtual void TestLocalTimeArray()
		 {
			  LocalTime[] array = new LocalTime[]{ LocalTimeValue.localTime( 0, 0, 0, 37 ).asObjectCopy(), LocalTimeValue.localTime(0, 0, 13, 37).asObjectCopy() };
			  string key = "testarray";
			  _node1.setProperty( key, array );
			  NewTransaction();

			  LocalTime[] propertyValue = ( LocalTime[] ) _node1.getProperty( key );
			  assertEquals( array.Length, propertyValue.Length );
			  for ( int i = 0; i < array.Length; i++ )
			  {
					assertEquals( array[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLocalDateTimeType()
		 public virtual void TestLocalDateTimeType()
		 {
			  DateTime dateTime = LocalDateTimeValue.localDateTime( 1991, 1, 1, 0, 0, 13, 37 ).asObjectCopy();
			  string key = "dt";
			  _node1.setProperty( key, dateTime );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( dateTime, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLocalDateTimeArray()
		 public virtual void TestLocalDateTimeArray()
		 {
			  DateTime[] array = new DateTime[]{ LocalDateTimeValue.localDateTime( 1991, 1, 1, 0, 0, 13, 37 ).asObjectCopy(), LocalDateTimeValue.localDateTime(1992, 2, 28, 1, 15, 0, 4000).asObjectCopy() };
			  string key = "testarray";
			  _node1.setProperty( key, array );
			  NewTransaction();

			  DateTime[] propertyValue = ( DateTime[] ) _node1.getProperty( key );
			  assertEquals( array.Length, propertyValue.Length );
			  for ( int i = 0; i < array.Length; i++ )
			  {
					assertEquals( array[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTimeType()
		 public virtual void TestTimeType()
		 {
			  OffsetTime time = TimeValue.time( 23, 11, 8, 0, "+17:59" ).asObjectCopy();
			  string key = "dt";
			  _node1.setProperty( key, time );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( time, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTimeArray()
		 public virtual void TestTimeArray()
		 {
			  string key = "testarray";

			  // array sizes 1 through 4
			  foreach (OffsetTime[] array in new OffsetTime[][]
			  {
				  new OffsetTime[]{ TimeValue.time( 23, 11, 8, 0, "+17:59" ).asObjectCopy() },
				  new OffsetTime[]{ TimeValue.time( 23, 11, 8, 0, "+17:59" ).asObjectCopy(), TimeValue.time(14, 34, 55, 3478, "+02:00").asObjectCopy() },
				  new OffsetTime[]{ TimeValue.time( 23, 11, 8, 0, "+17:59" ).asObjectCopy(), TimeValue.time(14, 34, 55, 3478, "+02:00").asObjectCopy(), TimeValue.time(0, 17, 20, 783478, "-03:00").asObjectCopy() },
				  new OffsetTime[]{ TimeValue.time( 23, 11, 8, 0, "+17:59" ).asObjectCopy(), TimeValue.time(14, 34, 55, 3478, "+02:00").asObjectCopy(), TimeValue.time(0, 17, 20, 783478, "-03:00").asObjectCopy(), TimeValue.time(1, 1, 1, 1, "-01:00").asObjectCopy() }
			  })
			  {
					_node1.setProperty( key, array );
					NewTransaction();

					OffsetTime[] propertyValue = ( OffsetTime[] ) _node1.getProperty( key );
					assertEquals( array.Length, propertyValue.Length );
					for ( int i = 0; i < array.Length; i++ )
					{
						 assertEquals( array[i], propertyValue[i] );
					}
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDurationType()
		 public virtual void TestDurationType()
		 {
			  TemporalAmount duration = DurationValue.duration( 57, 57, 57, 57 ).asObjectCopy();
			  string key = "dt";
			  _node1.setProperty( key, duration );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( duration, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDurationArray()
		 public virtual void TestDurationArray()
		 {
			  TemporalAmount[] array = new TemporalAmount[]{ DurationValue.duration( 57, 57, 57, 57 ).asObjectCopy(), DurationValue.duration(-40, -189, -6247, -1).asObjectCopy() };
			  string key = "testarray";
			  _node1.setProperty( key, array );
			  NewTransaction();

			  TemporalAmount[] propertyValue = ( TemporalAmount[] ) _node1.getProperty( key );
			  assertEquals( array.Length, propertyValue.Length );
			  for ( int i = 0; i < array.Length; i++ )
			  {
					assertEquals( array[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTimeTypeWithZoneOffset()
		 public virtual void TestDateTimeTypeWithZoneOffset()
		 {
			  DateTimeValue dateTime = DateTimeValue.datetime( 1991, 1, 1, 0, 0, 13, 37, "+01:00" );
			  string key = "dt";
			  _node1.setProperty( key, dateTime );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( dateTime.AsObjectCopy(), property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTimeArrayWithZoneOffset()
		 public virtual void TestDateTimeArrayWithZoneOffset()
		 {
			  ZonedDateTime[] array = new ZonedDateTime[]{ DateTimeValue.datetime( 1991, 1, 1, 0, 0, 13, 37, "-01:00" ).asObjectCopy(), DateTimeValue.datetime(1992, 2, 28, 1, 15, 0, 4000, "+11:00").asObjectCopy() };
			  string key = "testarray";
			  _node1.setProperty( key, array );
			  NewTransaction();

			  ZonedDateTime[] propertyValue = ( ZonedDateTime[] ) _node1.getProperty( key );
			  assertEquals( array.Length, propertyValue.Length );
			  for ( int i = 0; i < array.Length; i++ )
			  {
					assertEquals( array[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTimeTypeWithZoneId()
		 public virtual void TestDateTimeTypeWithZoneId()
		 {
			  DateTimeValue dateTime = DateTimeValue.datetime( 1991, 1, 1, 0, 0, 13, 37, ZoneId.of( "Europe/Stockholm" ) );
			  string key = "dt";
			  _node1.setProperty( key, dateTime );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( dateTime.AsObjectCopy(), property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTimeArrayWithZoneOffsetAndZoneID()
		 public virtual void TestDateTimeArrayWithZoneOffsetAndZoneID()
		 {
			  ZonedDateTime[] array = new ZonedDateTime[]{ DateTimeValue.datetime( 1991, 1, 1, 0, 0, 13, 37, "-01:00" ).asObjectCopy(), DateTimeValue.datetime(1992, 2, 28, 1, 15, 0, 4000, "+11:00").asObjectCopy(), DateTimeValue.datetime(1992, 2, 28, 1, 15, 0, 4000, ZoneId.of("Europe/Stockholm")).asObjectCopy() };
			  string key = "testarray";
			  _node1.setProperty( key, array );
			  NewTransaction();

			  ZonedDateTime[] propertyValue = ( ZonedDateTime[] ) _node1.getProperty( key );
			  assertEquals( array.Length, propertyValue.Length );
			  for ( int i = 0; i < array.Length; i++ )
			  {
					assertEquals( array[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIntArray()
		 public virtual void TestIntArray()
		 {
			  int[] array1 = new int[] { 1, 2, 3, 4, 5 };
			  int?[] array2 = new int?[] { 6, 7, 8 };
			  string key = "testintarray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  int[] propertyValue = null;
			  propertyValue = ( int[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i] );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( int[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], Convert.ToInt32( propertyValue[i] ) );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testShortArray()
		 public virtual void TestShortArray()
		 {
			  short[] array1 = new short[] { 1, 2, 3, 4, 5 };
			  short?[] array2 = new short?[] { 6, 7, 8 };
			  string key = "testintarray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  short[] propertyValue = null;
			  propertyValue = ( short[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i] );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( short[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], Convert.ToInt16( propertyValue[i] ) );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringArray()
		 public virtual void TestStringArray()
		 {
			  string[] array1 = new string[] { "a", "b", "c", "d", "e" };
			  string[] array2 = new string[] { "ff", "gg", "hh" };
			  string key = "teststringarray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  string[] propertyValue = null;
			  propertyValue = ( string[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i] );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( string[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBooleanArray()
		 public virtual void TestBooleanArray()
		 {
			  bool[] array1 = new bool[] { true, false, true, false, true };
			  bool?[] array2 = new bool?[] { false, true, false };
			  string key = "testboolarray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  bool[] propertyValue = null;
			  propertyValue = ( bool[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i] );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( bool[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], propertyValue[i] );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDoubleArray()
		 public virtual void TestDoubleArray()
		 {
			  double[] array1 = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
			  double?[] array2 = new double?[] { 6.0, 7.0, 8.0 };
			  string key = "testdoublearray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  double[] propertyValue = null;
			  propertyValue = ( double[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i], 0.0 );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( double[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], new double?( propertyValue[i] ) );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFloatArray()
		 public virtual void TestFloatArray()
		 {
			  float[] array1 = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
			  float?[] array2 = new float?[] { 6.0f, 7.0f, 8.0f };
			  string key = "testfloatarray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  float[] propertyValue = null;
			  propertyValue = ( float[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i], 0.0 );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( float[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], new float?( propertyValue[i] ) );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLongArray()
		 public virtual void TestLongArray()
		 {
			  long[] array1 = new long[] { 1, 2, 3, 4, 5 };
			  long?[] array2 = new long?[] { 6L, 7L, 8L };
			  string key = "testlongarray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  long[] propertyValue = null;
			  propertyValue = ( long[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i] );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( long[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], Convert.ToInt64( propertyValue[i] ) );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testByteArray()
		 public virtual void TestByteArray()
		 {
			  sbyte[] array1 = new sbyte[] { 1, 2, 3, 4, 5 };
			  sbyte?[] array2 = new sbyte?[] { 6, 7, 8 };
			  string key = "testbytearray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  sbyte[] propertyValue = null;
			  propertyValue = ( sbyte[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i] );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( sbyte[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], Convert.ToSByte( propertyValue[i] ) );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCharArray()
		 public virtual void TestCharArray()
		 {
			  char[] array1 = new char[] { '1', '2', '3', '4', '5' };
			  char?[] array2 = new char?[] { '6', '7', '8' };
			  string key = "testchararray";
			  _node1.setProperty( key, array1 );
			  NewTransaction();

			  char[] propertyValue = null;
			  propertyValue = ( char[] ) _node1.getProperty( key );
			  assertEquals( array1.Length, propertyValue.Length );
			  for ( int i = 0; i < array1.Length; i++ )
			  {
					assertEquals( array1[i], propertyValue[i] );
			  }

			  _node1.setProperty( key, array2 );
			  NewTransaction();

			  propertyValue = ( char[] ) _node1.getProperty( key );
			  assertEquals( array2.Length, propertyValue.Length );
			  for ( int i = 0; i < array2.Length; i++ )
			  {
					assertEquals( array2[i], new char?( propertyValue[i] ) );
			  }

			  _node1.removeProperty( key );
			  NewTransaction();

			  assertTrue( !_node1.hasProperty( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEmptyString()
		 public virtual void TestEmptyString()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "1", 2 );
			  node.SetProperty( "2", "" );
			  node.SetProperty( "3", "" );
			  NewTransaction();

			  assertEquals( 2, node.GetProperty( "1" ) );
			  assertEquals( "", node.GetProperty( "2" ) );
			  assertEquals( "", node.GetProperty( "3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonBooleanArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonBooleanArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new bool[] { false, false, false }, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonByteArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonByteArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new sbyte[] { 0, 0, 0 }, ( sbyte )1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonShortArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonShortArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new short[] { 0, 0, 0 }, ( short )1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonIntArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonIntArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new int[] { 0, 0, 0 }, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonLongArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonLongArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new long[] { 0, 0, 0 }, 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonFloatArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonFloatArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new float[] { 0F, 0F, 0F }, 1F );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonDoubleArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonDoubleArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new double[] { 0D, 0D, 0D }, 1D );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonCharArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonCharArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new char[] { '0', '0', '0' }, '1' );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonStringArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonStringArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( new string[] { "zero", "zero", "zero" }, "one" );
		 }

		 private object VeryLongArray( Type type )
		 {
			  object array = Array.CreateInstance( type, 1000 );
			  return array;
		 }

		 private string[] VeryLongStringArray()
		 {
			  string[] array = new string[100];
			  Arrays.fill( array, "zero" );
			  return array;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongBooleanArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongBooleanArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongArray( Boolean.TYPE ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongByteArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongByteArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongArray( Byte.TYPE ), ( sbyte )1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongShortArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongShortArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongArray( Short.TYPE ), ( short )1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongIntArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongIntArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongArray( Integer.TYPE ), 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongLongArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongLongArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongArray( Long.TYPE ), 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongFloatArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongFloatArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongArray( Float.TYPE ), 1F );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongDoubleArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongDoubleArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongArray( Double.TYPE ), 1D );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongCharArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongCharArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongArray( Character.TYPE ), '1' );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToPoisonVeryLongStringArrayProperty()
		 public virtual void ShouldNotBeAbleToPoisonVeryLongStringArrayProperty()
		 {
			  ShouldNotBeAbleToPoisonArrayProperty( VeryLongStringArray(), "one" );
		 }

		 private void ShouldNotBeAbleToPoisonArrayProperty( object value, object poison )
		 {
			  ShouldNotBeAbleToPoisonArrayPropertyInsideTransaction( value, poison );
			  ShouldNotBeAbleToPoisonArrayPropertyOutsideTransaction( value, poison );
		 }

		 private void ShouldNotBeAbleToPoisonArrayPropertyInsideTransaction( object value, object poison )
		 {
			  // GIVEN
			  string key = "key";
			  // setting a property, then reading it back
			  _node1.setProperty( key, value );
			  object readValue = _node1.getProperty( key );

			  // WHEN changing the value read back
			  ( ( Array )readValue ).SetValue( poison, 0 );

			  // THEN reading the value one more time should still yield the set property
			  assertTrue( format( "Expected %s, but was %s", Strings.prettyPrint( value ), Strings.prettyPrint( readValue ) ), ArrayUtil.Equals( value, _node1.getProperty( key ) ) );
		 }

		 private void ShouldNotBeAbleToPoisonArrayPropertyOutsideTransaction( object value, object poison )
		 {
			  // GIVEN
			  string key = "key";
			  // setting a property, then reading it back
			  _node1.setProperty( key, value );
			  NewTransaction();
			  object readValue = _node1.getProperty( key );

			  // WHEN changing the value read back
			  ( ( Array )readValue ).SetValue( poison, 0 );

			  // THEN reading the value one more time should still yield the set property
			  assertTrue( format( "Expected %s, but was %s", Strings.prettyPrint( value ), Strings.prettyPrint( readValue ) ), ArrayUtil.Equals( value, _node1.getProperty( key ) ) );
		 }
	}

}