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
namespace Neo4Net.Kernel.Api.Internal
{
	using Test = org.junit.Test;

	using ExactPredicate = Neo4Net.Kernel.Api.Internal.IndexQuery.ExactPredicate;
	using ExistsPredicate = Neo4Net.Kernel.Api.Internal.IndexQuery.ExistsPredicate;
	using Neo4Net.Kernel.Api.Internal.IndexQuery;
	using StringContainsPredicate = Neo4Net.Kernel.Api.Internal.IndexQuery.StringContainsPredicate;
	using StringPrefixPredicate = Neo4Net.Kernel.Api.Internal.IndexQuery.StringPrefixPredicate;
	using StringSuffixPredicate = Neo4Net.Kernel.Api.Internal.IndexQuery.StringSuffixPredicate;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;

	public class IndexQueryTest
	{
		 private readonly int _propId = 0;

		 // EXISTS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExists()
		 public virtual void TestExists()
		 {
			  ExistsPredicate p = IndexQuery.Exists( _propId );

			  assertTrue( Test( p, "string" ) );
			  assertTrue( Test( p, 1 ) );
			  assertTrue( Test( p, 1.0 ) );
			  assertTrue( Test( p, true ) );
			  assertTrue( Test( p, new long[]{ 1L } ) );
			  assertTrue( Test( p, Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) ) );

			  assertFalse( Test( p, null ) );
		 }

		 // EXACT

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExact()
		 public virtual void TestExact()
		 {
			  AssertExactPredicate( "string" );
			  AssertExactPredicate( 1 );
			  AssertExactPredicate( 1.0 );
			  AssertExactPredicate( true );
			  AssertExactPredicate( new long[]{ 1L } );
			  AssertExactPredicate( Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) );
		 }

		 private void AssertExactPredicate( object value )
		 {
			  ExactPredicate p = IndexQuery.Exact( _propId, value );

			  assertTrue( Test( p, value ) );

			  AssertFalseForOtherThings( p );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExact_ComparingBigDoublesAndLongs()
		 public virtual void TestExactComparingBigDoublesAndLongs()
		 {
			  ExactPredicate p = IndexQuery.Exact( _propId, 9007199254740993L );

			  assertFalse( Test( p, 9007199254740992D ) );
		 }

		 // NUMERIC RANGE

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumRange_FalseForIrrelevant()
		 public virtual void TestNumRangeFalseForIrrelevant()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, 11, true, 13, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, 11, true, 13, true );

			  AssertFalseForOtherThings( p );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumRange_InclusiveLowerInclusiveUpper()
		 public virtual void TestNumRangeInclusiveLowerInclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, 11, true, 13, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, 11, true, 13, true );

			  assertFalse( Test( p, 10 ) );
			  assertTrue( Test( p, 11 ) );
			  assertTrue( Test( p, 12 ) );
			  assertTrue( Test( p, 13 ) );
			  assertFalse( Test( p, 14 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumRange_ExclusiveLowerExclusiveLower()
		 public virtual void TestNumRangeExclusiveLowerExclusiveLower()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, 11, false, 13, false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, 11, false, 13, false );

			  assertFalse( Test( p, 11 ) );
			  assertTrue( Test( p, 12 ) );
			  assertFalse( Test( p, 13 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumRange_InclusiveLowerExclusiveUpper()
		 public virtual void TestNumRangeInclusiveLowerExclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, 11, true, 13, false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, 11, true, 13, false );

			  assertFalse( Test( p, 10 ) );
			  assertTrue( Test( p, 11 ) );
			  assertTrue( Test( p, 12 ) );
			  assertFalse( Test( p, 13 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumRange_ExclusiveLowerInclusiveUpper()
		 public virtual void TestNumRangeExclusiveLowerInclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, 11, false, 13, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, 11, false, 13, true );

			  assertFalse( Test( p, 11 ) );
			  assertTrue( Test( p, 12 ) );
			  assertTrue( Test( p, 13 ) );
			  assertFalse( Test( p, 14 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumRange_LowerNullValue()
		 public virtual void TestNumRangeLowerNullValue()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, null, true, 13, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, null, true, 13, true );

			  assertTrue( Test( p, 10 ) );
			  assertTrue( Test( p, 11 ) );
			  assertTrue( Test( p, 12 ) );
			  assertTrue( Test( p, 13 ) );
			  assertFalse( Test( p, 14 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumRange_UpperNullValue()
		 public virtual void TestNumRangeUpperNullValue()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, 11, true, null, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, 11, true, null, true );

			  assertFalse( Test( p, 10 ) );
			  assertTrue( Test( p, 11 ) );
			  assertTrue( Test( p, 12 ) );
			  assertTrue( Test( p, 13 ) );
			  assertTrue( Test( p, 14 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumRange_ComparingBigDoublesAndLongs()
		 public virtual void TestNumRangeComparingBigDoublesAndLongs()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, 9007199254740993L, true, null, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, 9007199254740993L, true, null, true );

			  assertFalse( Test( p, 9007199254740992D ) );
		 }

		 // STRING RANGE

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringRange_FalseForIrrelevant()
		 public virtual void TestStringRangeFalseForIrrelevant()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, "bbb", true, "bee", true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, "bbb", true, "bee", true );

			  AssertFalseForOtherThings( p );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringRange_InclusiveLowerInclusiveUpper()
		 public virtual void TestStringRangeInclusiveLowerInclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, "bbb", true, "bee", true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, "bbb", true, "bee", true );

			  assertFalse( Test( p, "bba" ) );
			  assertTrue( Test( p, "bbb" ) );
			  assertTrue( Test( p, "bee" ) );
			  assertFalse( Test( p, "beea" ) );
			  assertFalse( Test( p, "bef" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringRange_ExclusiveLowerInclusiveUpper()
		 public virtual void TestStringRangeExclusiveLowerInclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, "bbb", false, "bee", true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, "bbb", false, "bee", true );

			  assertFalse( Test( p, "bbb" ) );
			  assertTrue( Test( p, "bbba" ) );
			  assertTrue( Test( p, "bee" ) );
			  assertFalse( Test( p, "beea" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringRange_InclusiveLowerExclusiveUpper()
		 public virtual void TestStringRangeInclusiveLowerExclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, "bbb", true, "bee", false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, "bbb", true, "bee", false );

			  assertFalse( Test( p, "bba" ) );
			  assertTrue( Test( p, "bbb" ) );
			  assertTrue( Test( p, "bed" ) );
			  assertFalse( Test( p, "bee" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringRange_ExclusiveLowerExclusiveUpper()
		 public virtual void TestStringRangeExclusiveLowerExclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, "bbb", false, "bee", false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, "bbb", false, "bee", false );

			  assertFalse( Test( p, "bbb" ) );
			  assertTrue( Test( p, "bbba" ) );
			  assertTrue( Test( p, "bed" ) );
			  assertFalse( Test( p, "bee" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringRange_UpperUnbounded()
		 public virtual void TestStringRangeUpperUnbounded()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, "bbb", false, null, false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, "bbb", false, null, false );

			  assertFalse( Test( p, "bbb" ) );
			  assertTrue( Test( p, "bbba" ) );
			  assertTrue( Test( p, "xxxxx" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringRange_LowerUnbounded()
		 public virtual void TestStringRangeLowerUnbounded()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, null, false, "bee", false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, null, false, "bee", false );

			  assertTrue( Test( p, "" ) );
			  assertTrue( Test( p, "bed" ) );
			  assertFalse( Test( p, "bee" ) );
		 }

		 // GEOMETRY RANGE

		 private PointValue _gps1 = Values.pointValue( CoordinateReferenceSystem.WGS84, -12.6, -56.7 );
		 private PointValue _gps2 = Values.pointValue( CoordinateReferenceSystem.WGS84, -12.6, -55.7 );
		 private PointValue _gps3 = Values.pointValue( CoordinateReferenceSystem.WGS84, -11.0, -55 );
		 private PointValue _gps4 = Values.pointValue( CoordinateReferenceSystem.WGS84, 0, 0 );
		 private PointValue _gps5 = Values.pointValue( CoordinateReferenceSystem.WGS84, 14.6, 56.7 );
		 private PointValue _gps6 = Values.pointValue( CoordinateReferenceSystem.WGS84, 14.6, 58.7 );
		 private PointValue _gps7 = Values.pointValue( CoordinateReferenceSystem.WGS84, 15.6, 59.7 );
		 private PointValue _car1 = Values.pointValue( CoordinateReferenceSystem.Cartesian, 0, 0 );
		 private PointValue _car2 = Values.pointValue( CoordinateReferenceSystem.Cartesian, 2, 2 );
		 private PointValue _car3 = Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 1, 2, 3 );
		 private PointValue _car4 = Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 2, 3, 4 );
		 private PointValue _gps1_3d = Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 12.6, 56.8, 100.0 );
		 private PointValue _gps2_3d = Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 12.8, 56.9, 200.0 );

		 //TODO: Also insert points which can't be compared e.g. Cartesian and (-100, 100)

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_FalseForIrrelevant()
		 public virtual void TestGeometryRangeFalseForIrrelevant()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, gps2, true, gps5, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _gps2, true, _gps5, true );

			  AssertFalseForOtherThings( p );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_InclusiveLowerInclusiveUpper()
		 public virtual void TestGeometryRangeInclusiveLowerInclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, gps2, true, gps5, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _gps2, true, _gps5, true );

			  assertFalse( Test( p, _gps1 ) );
			  assertTrue( Test( p, _gps2 ) );
			  assertTrue( Test( p, _gps5 ) );
			  assertFalse( Test( p, _gps6 ) );
			  assertFalse( Test( p, _gps7 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car2 ) );
			  assertFalse( Test( p, _car3 ) );
			  assertFalse( Test( p, _gps1_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_ExclusiveLowerInclusiveUpper()
		 public virtual void TestGeometryRangeExclusiveLowerInclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, gps2, false, gps5, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _gps2, false, _gps5, true );

			  assertFalse( Test( p, _gps2 ) );
			  assertTrue( Test( p, _gps3 ) );
			  assertTrue( Test( p, _gps5 ) );
			  assertFalse( Test( p, _gps6 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car2 ) );
			  assertFalse( Test( p, _car3 ) );
			  assertFalse( Test( p, _gps1_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_InclusiveLowerExclusiveUpper()
		 public virtual void TestGeometryRangeInclusiveLowerExclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, gps2, true, gps5, false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _gps2, true, _gps5, false );

			  assertFalse( Test( p, _gps1 ) );
			  assertTrue( Test( p, _gps2 ) );
			  assertTrue( Test( p, _gps3 ) );
			  assertFalse( Test( p, _gps5 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car2 ) );
			  assertFalse( Test( p, _car3 ) );
			  assertFalse( Test( p, _gps1_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_ExclusiveLowerExclusiveUpper()
		 public virtual void TestGeometryRangeExclusiveLowerExclusiveUpper()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, gps2, false, gps5, false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _gps2, false, _gps5, false );

			  assertFalse( Test( p, _gps2 ) );
			  assertTrue( Test( p, _gps3 ) );
			  assertTrue( Test( p, _gps4 ) );
			  assertFalse( Test( p, _gps5 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car2 ) );
			  assertFalse( Test( p, _car3 ) );
			  assertFalse( Test( p, _gps1_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_UpperUnbounded()
		 public virtual void TestGeometryRangeUpperUnbounded()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, gps2, false, null, false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _gps2, false, null, false );

			  assertFalse( Test( p, _gps2 ) );
			  assertTrue( Test( p, _gps3 ) );
			  assertTrue( Test( p, _gps7 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car2 ) );
			  assertFalse( Test( p, _car3 ) );
			  assertFalse( Test( p, _gps1_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_LowerUnbounded()
		 public virtual void TestGeometryRangeLowerUnbounded()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, null, false, gps5, false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, null, false, _gps5, false );

			  assertTrue( Test( p, _gps1 ) );
			  assertTrue( Test( p, _gps3 ) );
			  assertFalse( Test( p, _gps5 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car2 ) );
			  assertFalse( Test( p, _car3 ) );
			  assertFalse( Test( p, _gps1_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_Cartesian()
		 public virtual void TestGeometryRangeCartesian()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, car1, false, car2, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _car1, false, _car2, true );

			  assertFalse( Test( p, _gps1 ) );
			  assertFalse( Test( p, _gps3 ) );
			  assertFalse( Test( p, _gps5 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertTrue( Test( p, _car2 ) );
			  assertFalse( Test( p, _car3 ) );
			  assertFalse( Test( p, _car4 ) );
			  assertFalse( Test( p, _gps1_3d ) );
			  assertFalse( Test( p, _gps2_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_Cartesian3D()
		 public virtual void TestGeometryRangeCartesian3D()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, car3, true, car4, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _car3, true, _car4, true );

			  assertFalse( Test( p, _gps1 ) );
			  assertFalse( Test( p, _gps3 ) );
			  assertFalse( Test( p, _gps5 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car2 ) );
			  assertTrue( Test( p, _car3 ) );
			  assertTrue( Test( p, _car4 ) );
			  assertFalse( Test( p, _gps1_3d ) );
			  assertFalse( Test( p, _gps2_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryRange_WGS84_3D()
		 public virtual void TestGeometryRangeWGS84_3D()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, gps1_3d, true, gps2_3d, true);
			  RangePredicate<object> p = IndexQuery.Range( _propId, _gps1_3d, true, _gps2_3d, true );

			  assertFalse( Test( p, _gps1 ) );
			  assertFalse( Test( p, _gps3 ) );
			  assertFalse( Test( p, _gps5 ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car2 ) );
			  assertFalse( Test( p, _car3 ) );
			  assertFalse( Test( p, _car4 ) );
			  assertTrue( Test( p, _gps1_3d ) );
			  assertTrue( Test( p, _gps2_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateRange()
		 public virtual void TestDateRange()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, Neo4Net.values.storable.DateValue.date(2014, 7, 7), true, Neo4Net.values.storable.DateValue.date(2017,3, 7), false);
			  RangePredicate<object> p = IndexQuery.Range( _propId, DateValue.date( 2014, 7, 7 ), true, DateValue.date( 2017,3, 7 ), false );

			  assertFalse( Test( p, DateValue.date( 2014, 6, 8 ) ) );
			  assertTrue( Test( p, DateValue.date( 2014, 7, 7 ) ) );
			  assertTrue( Test( p, DateValue.date( 2016, 6, 8 ) ) );
			  assertFalse( Test( p, DateValue.date( 2017, 3, 7 ) ) );
			  assertFalse( Test( p, DateValue.date( 2017, 3, 8 ) ) );
			  assertFalse( Test( p, LocalDateTimeValue.localDateTime( 2016, 3, 8, 0, 0, 0, 0 ) ) );
		 }

		 // VALUE GROUP SCAN
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testValueGroupRange()
		 public virtual void TestValueGroupRange()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, Neo4Net.values.storable.ValueGroup.DATE);
			  RangePredicate<object> p = IndexQuery.Range( _propId, ValueGroup.DATE );

			  assertTrue( Test( p, DateValue.date( -4000, 1, 31 ) ) );
			  assertTrue( Test( p, DateValue.date( 2018, 3, 7 ) ) );
			  assertFalse( Test( p, DateTimeValue.datetime( 2018, 3, 7, 0, 0, 0, 0, ZoneOffset.UTC ) ) );
			  assertFalse( Test( p, stringValue( "hej" ) ) );
			  assertFalse( Test( p, _gps2_3d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCRSRange()
		 public virtual void TestCRSRange()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> p = IndexQuery.range(propId, Neo4Net.values.storable.CoordinateReferenceSystem.WGS84);
			  RangePredicate<object> p = IndexQuery.Range( _propId, CoordinateReferenceSystem.WGS84 );

			  assertTrue( Test( p, _gps2 ) );
			  assertFalse( Test( p, DateValue.date( -4000, 1, 31 ) ) );
			  assertFalse( Test( p, stringValue( "hej" ) ) );
			  assertFalse( Test( p, _car1 ) );
			  assertFalse( Test( p, _car4 ) );
			  assertFalse( Test( p, _gps1_3d ) );
		 }

		 // STRING PREFIX

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringPrefix_FalseForIrrelevant()
		 public virtual void TestStringPrefixFalseForIrrelevant()
		 {
			  StringPrefixPredicate p = IndexQuery.StringPrefix( _propId, stringValue( "dog" ) );

			  AssertFalseForOtherThings( p );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringPrefix_SomeValues()
		 public virtual void TestStringPrefixSomeValues()
		 {
			  StringPrefixPredicate p = IndexQuery.StringPrefix( _propId, stringValue( "dog" ) );

			  assertFalse( Test( p, "doffington" ) );
			  assertFalse( Test( p, "doh, not this again!" ) );
			  assertTrue( Test( p, "dog" ) );
			  assertTrue( Test( p, "doggidog" ) );
			  assertTrue( Test( p, "doggidogdog" ) );
		 }

		 // STRING CONTAINS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringContains_FalseForIrrelevant()
		 public virtual void TestStringContainsFalseForIrrelevant()
		 {
			  StringContainsPredicate p = IndexQuery.StringContains( _propId, stringValue( "cat" ) );

			  AssertFalseForOtherThings( p );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringContains_SomeValues()
		 public virtual void TestStringContainsSomeValues()
		 {
			  StringContainsPredicate p = IndexQuery.StringContains( _propId, stringValue( "cat" ) );

			  assertFalse( Test( p, "dog" ) );
			  assertFalse( Test( p, "cameraman" ) );
			  assertFalse( Test( p, "Cat" ) );
			  assertTrue( Test( p, "cat" ) );
			  assertTrue( Test( p, "bobcat" ) );
			  assertTrue( Test( p, "scatman" ) );
		 }

		 // STRING SUFFIX

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringSuffix_FalseForIrrelevant()
		 public virtual void TestStringSuffixFalseForIrrelevant()
		 {
			  StringSuffixPredicate p = IndexQuery.StringSuffix( _propId, stringValue( "less" ) );

			  AssertFalseForOtherThings( p );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringSuffix_SomeValues()
		 public virtual void TestStringSuffixSomeValues()
		 {
			  StringSuffixPredicate p = IndexQuery.StringSuffix( _propId, stringValue( "less" ) );

			  assertFalse( Test( p, "lesser being" ) );
			  assertFalse( Test( p, "make less noise please..." ) );
			  assertTrue( Test( p, "less" ) );
			  assertTrue( Test( p, "clueless" ) );
			  assertTrue( Test( p, "cluelessly clueless" ) );
		 }

		 // HELPERS

		 private void AssertFalseForOtherThings( IndexQuery p )
		 {
			  assertFalse( Test( p, "other string" ) );
			  assertFalse( Test( p, "string1" ) );
			  assertFalse( Test( p, "" ) );
			  assertFalse( Test( p, -1 ) );
			  assertFalse( Test( p, -1.0 ) );
			  assertFalse( Test( p, false ) );
			  assertFalse( Test( p, new long[]{ -1L } ) );
			  assertFalse( Test( p, null ) );
		 }

		 private bool Test( IndexQuery p, object x )
		 {
			  return p.AcceptsValue( x is Value ? ( Value )x : Values.of( x ) );
		 }
	}

}