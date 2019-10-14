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
namespace Neo4Net.Values.Storable
{
	using Disabled = org.junit.jupiter.api.Disabled;
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.signum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;

	public class ValueComparisonTest
	{
		 private static readonly IComparer<Value> _comparator = Values.Comparator;

		 private object[] _objs = new object[]
		 {
			 new PointValue[] {},
			 new PointValue[] { pointValue( WGS84, -1.0, -1.0 ) },
			 new PointValue[] { pointValue( WGS84, -1.0, -1.0 ), pointValue( WGS84, -1.0, -1.0 ) },
			 new PointValue[] { pointValue( WGS84, -1.0, -1.0 ), pointValue( Cartesian, 1.0, 2.0 ) },
			 new ZonedDateTime[] { datetime( 2018, 2, 2, 0, 0, 0, 0, "+00:00" ).asObjectCopy(), datetime(1991, 2, 2, 1, 30, 0, 0, "+00:00").asObjectCopy() },
			 new ZonedDateTime[] { datetime( 2018, 2, 2, 0, 0, 0, 0, "+00:00" ).asObjectCopy(), datetime(1992, 2, 2, 1, 30, 0, 0, "+00:00").asObjectCopy() },
			 new ZonedDateTime[] { datetime( 2019, 2, 2, 0, 0, 0, 0, "+00:00" ).asObjectCopy(), datetime(1991, 2, 2, 1, 30, 0, 0, "+00:00").asObjectCopy() },
			 new DateTime[] {},
			 new DateTime[] { localDateTime( 2019, 2, 2, 0, 0, 0, 0 ).asObjectCopy(), localDateTime(1991, 2, 2, 1, 30, 0, 0).asObjectCopy() },
			 new LocalDate[]{ date( 2018, 2, 1 ).asObjectCopy() },
			 new LocalDate[]{ date( 2018, 2, 1 ).asObjectCopy(), date(2019, 2, 1).asObjectCopy() },
			 new OffsetTime[]{ time( 0, 0, 0, 1, "+00:00" ).asObjectCopy() },
			 new OffsetTime[]{ time( 0, 0, 1, 0, "+00:00" ).asObjectCopy() },
			 new OffsetTime[]{ time( 0, 0, 1, 0, "+00:00" ).asObjectCopy(), time(0, 0, 1, 0, "+00:00").asObjectCopy() },
			 new LocalTime[]{ localTime( 0, 0, 0, 1 ).asObjectCopy() },
			 new LocalTime[]{ localTime( 0, 0, 1, 0 ).asObjectCopy() },
			 new LocalTime[]{ localTime( 0, 0, 1, 0 ).asObjectCopy(), localTime(0, 0, 1, 0).asObjectCopy() },
			 new DurationValue[] { duration( 0, 0, 0, 0 ) },
			 new DurationValue[] { duration( 0, 0, 0, 1 ) },
			 new DurationValue[] { duration( 0, 0, 0, 1 ), duration( 0, 0, 1, 0 ) },
			 new string[]{},
			 new string[]{ "a" },
			 new string[]{ "a", "aa" },
			 new char[]{ 'a', 'b' },
			 new string[]{ "aa" },
			 new bool[]{},
			 new bool[]{ false },
			 new bool[]{ false, true },
			 new bool[]{ true },
			 new int[]{},
			 new double[]{ -1.0 },
			 new long[]{ -1, 44 },
			 new float[]{ 2 },
			 new short[]{ 2, 3 },
			 new sbyte[]{ 3, ( sbyte ) - 99, ( sbyte ) - 99 },
			 pointValue( WGS84, -1000.0, -1000.0 ),
			 pointValue( WGS84, -1.0, -1.0 ),
			 pointValue( WGS84, 0.0, 0.0 ),
			 pointValue( WGS84, 0.0, 1.0 ),
			 pointValue( WGS84, 1.0, 0.0 ),
			 pointValue( WGS84, 1.0, 1.0 ),
			 pointValue( WGS84, 1.0, 2.0 ),
			 pointValue( WGS84, 2.0, 1.0 ),
			 pointValue( WGS84, 1000.0, 1000.0 ),
			 pointValue( WGS84_3D, -1000.0, -1000.0, -1000.0 ),
			 pointValue( WGS84_3D, 0.0, 0.0, 0.0 ),
			 pointValue( WGS84_3D, 1000.0, 1000.0, 1000.0 ),
			 pointValue( Cartesian, -1000.0, -1000.0 ),
			 pointValue( Cartesian, -1.0, -1.0 ),
			 pointValue( Cartesian, 0.0, 0.0 ),
			 pointValue( Cartesian, 1.0, 1.0 ),
			 pointValue( Cartesian, 1.0, 2.0 ),
			 pointValue( Cartesian, 2.0, 1.0 ),
			 pointValue( Cartesian, 1000.0, 1000.0 ),
			 pointValue( Cartesian_3D, -1000.0, -1000.0, -1000.0 ),
			 pointValue( Cartesian_3D, 0.0, 0.0, 0.0 ),
			 pointValue( Cartesian_3D, 1000.0, 1000.0, 1000.0 ),
			 datetime( 2018, 2, 2, 0, 0, 0, 0, "+00:00" ),
			 datetime( 2018, 2, 1, 22, 30, 0, 0, "-02:00" ),
			 datetime( 2018, 2, 2, 0, 30, 0, 0, "Europe/London" ),
			 datetime( 2018, 2, 2, 1, 30, 0, 0, "+01:00" ),
			 datetime( 2018, 2, 2, 1, 30, 0, 0, "Europe/Berlin" ),
			 datetime( 2018, 2, 2, 1, 30, 0, 0, "Europe/Prague" ),
			 datetime( 2018, 2, 2, 1, 30, 0, 0, "Europe/Stockholm" ),
			 datetime( 2018, 2, 2, 1, 0, 0, 0, "+00:00" ),
			 datetime( 2018, 3, 2, 1, 0, 0, 0, "Europe/Berlin" ),
			 datetime( 2018, 3, 2, 1, 0, 0, 0, "Europe/Stockholm" ),
			 localDateTime( 2018, 2, 2, 0, 0, 0, 0 ),
			 localDateTime( 2018, 2, 2, 0, 0, 0, 1 ),
			 localDateTime( 2018, 2, 2, 0, 0, 1, 0 ),
			 localDateTime( 2018, 2, 2, 0, 1, 0, 0 ),
			 localDateTime( 2018, 2, 2, 1, 0, 0, 0 ),
			 date( 2018, 2, 1 ),
			 date( 2018, 2, 2 ),
			 time( 12, 0, 0, 0, "+00:00" ),
			 time( 13, 30, 0, 0, "+01:00" ),
			 time( 13, 0, 0, 0, "+00:00" ),
			 localTime( 0, 0, 0, 1 ),
			 localTime( 0, 0, 0, 3 ),
			 duration( 0, 0, 0, 0 ),
			 duration( 0, 0, 0, 1 ),
			 duration( 0, 0, 1, 0 ),
			 duration( 0, 0, 60, 0 ),
			 duration( 0, 0, 60 * 60, 0 ),
			 duration( 0, 0, 60 * 60 * 24, 0 ),
			 duration( 0, 1, 0, 0 ),
			 duration( 0, 0, 60 * 60 * 24, 1 ),
			 duration( 0, 1, 60 * 60 * 24, 0 ),
			 duration( 0, 2, 0, 0 ),
			 duration( 0, 1, 60 * 60 * 24, 1 ),
			 duration( 0, 10, 60 * 60 * 24, 2_000_000_500 ),
			 duration( 0, 11, 2, 500 ),
			 duration( 0, 10, 60 * 60 * 24, 2_000_000_501 ),
			 duration( 0, 27, 0, 0 ),
			 duration( 1, 0, 0, 0 ),
			 duration( 0, 31, 0, 0 ),
			 duration( 0, 59, 0, 0 ),
			 duration( 2, 0, 0, 0 ),
			 duration( 0, 62, 0, 0 ),
			 duration( 0, 89, 0, 0 ),
			 duration( 3, 0, 0, 0 ),
			 duration( 0, 92, 0, 0 ),
			 duration( 0, 120, 0, 0 ),
			 duration( 4, 0, 0, 0 ),
			 duration( 0, 123, 0, 0 ),
			 duration( 0, 150, 0, 0 ),
			 duration( 5, 0, 0, 0 ),
			 duration( 0, 153, 0, 0 ),
			 duration( 0, 181, 0, 0 ),
			 duration( 6, 0, 0, 0 ),
			 duration( 0, 184, 0, 0 ),
			 duration( 0, 212, 0, 0 ),
			 duration( 7, 0, 0, 0 ),
			 duration( 0, 215, 0, 0 ),
			 duration( 0, 242, 0, 0 ),
			 duration( 8, 0, 0, 0 ),
			 duration( 0, 245, 0, 0 ),
			 duration( 0, 273, 0, 0 ),
			 duration( 9, 0, 0, 0 ),
			 duration( 0, 276, 0, 0 ),
			 duration( 0, 303, 0, 0 ),
			 duration( 10, 0, 0, 0 ),
			 duration( 0, 306, 0, 0 ),
			 duration( 0, 334, 0, 0 ),
			 duration( 11, 0, 0, 0 ),
			 duration( 0, 337, 0, 0 ),
			 duration( 0, 365, 0, 0 ),
			 duration( 12, 0, 0, 0 ),
			 duration( 0, 366, 0, 0 ),
			 duration( 0, 1460, 0, 0 ),
			 duration( 12 * 4, 0, 0, 0 ),
			 duration( 0, 1461, 0, 0 ),
			 duration( 0, 36_524, 0, 0 ),
			 duration( 12 * 100, 0, 0, 0 ),
			 duration( 0, 36_525, 0, 0 ),
			 duration( 0, 146_097, 0, 0 ),
			 duration( 12 * 400, 0, 0, 0 ),
			 duration( 0, 146_097, 0, 1 ),
			 duration( 9999999999L * 12, 0, 0, 0 ),
			 duration( 9999999999L * 12, 0, 0, 1 ),
			 duration( 9999999999L * 12, 0, 0, 2 ),
			 "",
			 char.MinValue,
			 " ",
			 "20",
			 "x",
			 "y",
			 Character.MIN_HIGH_SURROGATE,
			 Character.MAX_HIGH_SURROGATE,
			 Character.MIN_LOW_SURROGATE,
			 Character.MAX_LOW_SURROGATE,
			 char.MaxValue,
			 false,
			 true,
			 double.NegativeInfinity,
			 -double.MaxValue,
			 long.MinValue,
			 long.MinValue + 1,
			 int.MinValue,
			 short.MinValue,
			 sbyte.MinValue,
			 0,
			 double.Epsilon,
			 Double.MIN_NORMAL,
			 float.Epsilon,
			 Float.MIN_NORMAL,
			 1L,
			 1.1d,
			 1.2f,
			 Math.E,
			 Math.PI,
			 ( sbyte ) 10,
			 ( short ) 20,
			 sbyte.MaxValue,
			 short.MaxValue,
			 int.MaxValue,
			 9007199254740992D,
			 9007199254740993L,
			 long.MaxValue,
			 float.MaxValue,
			 double.MaxValue,
			 double.PositiveInfinity,
			 Double.NaN,
			 null
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOrderValuesCorrectly()
		 internal virtual void ShouldOrderValuesCorrectly()
		 {
			  IList<Value> values = java.util.objs.Select( Values.of ).ToList();

			  for ( int i = 0; i < values.Count; i++ )
			  {
					for ( int j = 0; j < values.Count; j++ )
					{
						 Value left = values[i];
						 Value right = values[j];

						 int cmpPos = signum( i - j );
						 int cmpVal = signum( Compare( _comparator, left, right ) );

						 assertEquals( cmpPos, cmpVal, format( "Comparing %s against %s does not agree with their positions in the sorted list (%d and " + "%d)", left, right, i, j ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled public void shouldCompareRenamedTimeZonesByZoneNumber()
		 public virtual void ShouldCompareRenamedTimeZonesByZoneNumber()
		 {
			  int cmp = Values.Comparator.Compare( datetime( 10000, 100, ZoneId.of( "Canada/Saskatchewan" ) ), datetime( 10000, 100, ZoneId.of( "Canada/East-Saskatchewan" ) ) );
			  assertEquals( 0, cmp, "East-Saskatchewan and Saskatchewan are the same place" );
		 }

		 private int Compare<T>( IComparer<T> comparator, T left, T right )
		 {
			  int cmp1 = comparator.Compare( left, right );
			  int cmp2 = comparator.Compare( right, left );
			  assertEquals( signum( cmp1 ), -signum( cmp2 ), format( "%s is not symmetric on %s and %s", comparator, left, right ) );
			  return cmp1;
		 }
	}

}