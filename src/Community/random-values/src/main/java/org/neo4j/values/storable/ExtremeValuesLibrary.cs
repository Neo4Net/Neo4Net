using System;

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
namespace Neo4Net.Values.Storable
{

	internal class ExtremeValuesLibrary
	{
		 private ExtremeValuesLibrary()
		 {
		 }

		 private const int MAX_ALPHA_NUMERIC_CODE_POINT = 0x7a; // small z
		 private static readonly string _maxCodePointString = new string( new int[]{ Character.MAX_CODE_POINT }, 0, 1 );
		 private static readonly string _maxAlphaNumericCodePointString = new string( new int[]{ MAX_ALPHA_NUMERIC_CODE_POINT }, 0, 1 );
		 private static readonly string _maxAsciiCodePointString = new string( new int[]{ RandomValues.MAX_ASCII_CODE_POINT }, 0, 1 );
		 private static readonly string _maxBmpCodePointString = new string( new int[]{ RandomValues.MAX_BMP_CODE_POINT }, 0, 1 );

		 internal static readonly Value[] ExtremeBoolean = new Value[]{ Values.BooleanValue( false ), Values.BooleanValue( true ) };
		 internal static readonly Value[] ExtremeByte = new Value[]{ Values.ByteValue( sbyte.MinValue ), Values.ByteValue( sbyte.MaxValue ), Values.ByteValue( ( sbyte ) 0 ) };
		 internal static readonly Value[] ExtremeShort = new Value[]{ Values.ShortValue( short.MinValue ), Values.ShortValue( short.MaxValue ), Values.ShortValue( ( short ) 0 ) };
		 internal static readonly Value[] ExtremeInt = new Value[]{ Values.IntValue( int.MinValue ), Values.IntValue( int.MaxValue ), Values.IntValue( 0 ) };
		 internal static readonly Value[] ExtremeLong = new Value[]{ Values.LongValue( long.MinValue ), Values.LongValue( long.MaxValue ), Values.LongValue( 0 ) };
		 internal static readonly Value[] ExtremeFloat = new Value[]{ Values.FloatValue( float.Epsilon ), Values.FloatValue( float.MaxValue ), Values.FloatValue( -float.Epsilon ), Values.FloatValue( -float.MaxValue ), Values.FloatValue( 0f ) };
		 internal static readonly Value[] ExtremeDouble = new Value[]{ Values.DoubleValue( double.Epsilon ), Values.DoubleValue( double.MaxValue ), Values.DoubleValue( -double.Epsilon ), Values.DoubleValue( -double.MaxValue ), Values.DoubleValue( 0d ) };
		 internal static readonly Value[] ExtremeChar = new Value[]{ Values.CharValue( char.MaxValue ), Values.CharValue( char.MinValue ) };
		 internal static readonly Value[] ExtremeString = new Value[]{ Values.StringValue( _maxCodePointString ), Values.StringValue( "" ) };
		 internal static readonly Value[] ExtremeStringAlphanumeric = new Value[]{ Values.StringValue( _maxAlphaNumericCodePointString ), Values.StringValue( "" ) };
		 internal static readonly Value[] ExtremeStringAscii = new Value[]{ Values.StringValue( _maxAsciiCodePointString ), Values.StringValue( "" ) };
		 internal static readonly Value[] ExtremeStringBmp = new Value[]{ Values.StringValue( _maxBmpCodePointString ), Values.StringValue( "" ) };
		 internal static readonly Value[] ExtremeLocalDateTime = new Value[]{ LocalDateTimeValue.MinValue, LocalDateTimeValue.MaxValue };
		 internal static readonly Value[] ExtremeDate = new Value[]{ DateValue.MinValue, DateValue.MaxValue };
		 internal static readonly Value[] ExtremeLocalTime = new Value[]{ LocalTimeValue.MinValue, LocalTimeValue.MaxValue };
		 internal static readonly Value[] ExtremePeriod = new Value[]{ DurationValue.MinValue, DurationValue.MaxValue };
		 internal static readonly Value[] ExtremeDuration = new Value[]{ DurationValue.MinValue, DurationValue.MaxValue };
		 internal static readonly Value[] ExtremeTime = new Value[]{ TimeValue.MinValue, TimeValue.MaxValue };
		 internal static readonly Value[] ExtremeDateTime = new Value[]{ DateTimeValue.MinValue, DateTimeValue.MaxValue };
		 internal static readonly Value[] ExtremeCartesianPoint = new Value[]{ PointValue.MinValueCartesian, PointValue.MaxValueCartesian };
		 internal static readonly Value[] ExtremeCartesianPoint_3d = new Value[]{ PointValue.MinValueCartesian_3d, PointValue.MaxValueCartesian_3d };
		 internal static readonly Value[] ExtremeGeographicPoint = new Value[]{ PointValue.MinValueWgs84, PointValue.MaxValueWgs84 };
		 internal static readonly Value[] ExtremeGeographicPoint_3d = new Value[]{ PointValue.MinValueWgs84_3d, PointValue.MaxValueWgs84_3d };
		 internal static readonly Value[] ExtremeBooleanArray = new Value[]{ Values.Of( new bool[0] ), Values.Of( new bool[]{ true } ) };
		 internal static readonly Value[] ExtremeByteArray = new Value[]{ Values.Of( new sbyte[0] ), Values.Of( new sbyte[]{ sbyte.MaxValue } ) };
		 internal static readonly Value[] ExtremeShortArray = new Value[]{ Values.Of( new short[0] ), Values.Of( new short[]{ short.MaxValue } ) };
		 internal static readonly Value[] ExtremeIntArray = new Value[]{ Values.Of( new int[0] ), Values.Of( new int[]{ int.MaxValue } ) };
		 internal static readonly Value[] ExtremeLongArray = new Value[]{ Values.Of( new long[0] ), Values.Of( new long[]{ long.MaxValue } ) };
		 internal static readonly Value[] ExtremeFloatArray = new Value[]{ Values.Of( new float[0] ), Values.Of( new float[]{ float.MaxValue } ) };
		 internal static readonly Value[] ExtremeDoubleArray = new Value[]{ Values.Of( new double[0] ), Values.Of( new double[]{ double.MaxValue } ) };
		 internal static readonly Value[] ExtremeCharArray = new Value[]{ Values.Of( new char[0] ), Values.Of( new char[]{ char.MaxValue } ) };
		 internal static readonly Value[] ExtremeStringArray = new Value[]{ Values.Of( new string[0] ), Values.Of( new string[]{ _maxCodePointString } ) };
		 internal static readonly Value[] ExtremeStringAlphanumericArray = new Value[]{ Values.Of( new string[0] ), Values.Of( new string[]{ _maxAlphaNumericCodePointString } ) };
		 internal static readonly Value[] ExtremeStringAsciiArray = new Value[]{ Values.Of( new string[0] ), Values.Of( new string[]{ _maxAsciiCodePointString } ) };
		 internal static readonly Value[] ExtremeStringBmpArray = new Value[]{ Values.Of( new string[0] ), Values.Of( new string[]{ _maxBmpCodePointString } ) };
		 internal static readonly Value[] ExtremeLocalDateTimeArray = new Value[]{ Values.Of( new DateTime[0] ), Values.Of( new DateTime[]{ DateTime.MaxValue } ) };
		 internal static readonly Value[] ExtremeDateArray = new Value[]{ Values.Of( new LocalDate[0] ), Values.Of( new LocalDate[]{ LocalDate.MAX } ) };
		 internal static readonly Value[] ExtremeLocalTimeArray = new Value[]{ Values.Of( new LocalTime[0] ), Values.Of( new LocalTime[]{ LocalTime.MAX } ) };
		 internal static readonly Value[] ExtremePeriodArray = new Value[]{ Values.Of( new DurationValue[0] ), Values.Of( new DurationValue[]{ DurationValue.MaxValue } ) };
		 internal static readonly Value[] ExtremeDurationArray = new Value[]{ Values.Of( new DurationValue[0] ), Values.Of( new DurationValue[]{ DurationValue.MaxValue } ) };
		 internal static readonly Value[] ExtremeTimeArray = new Value[]{ Values.Of( new OffsetTime[0] ), Values.Of( new OffsetTime[]{ OffsetTime.MAX } ) };
		 internal static readonly Value[] ExtremeDateTimeArray = new Value[]{ Values.Of( new ZonedDateTime[0] ), Values.Of( new ZonedDateTime[]{ ZonedDateTime.of( DateTime.MaxValue, ZoneOffset.MAX ) } ) };
		 internal static readonly Value[] ExtremeCartesianPointArray = new Value[]{ Values.Of( new PointValue[0] ), Values.Of( new PointValue[]{ PointValue.MaxValueCartesian } ) };
		 internal static readonly Value[] ExtremeCartesianPoint_3dArray = new Value[]{ Values.Of( new PointValue[0] ), Values.Of( new PointValue[]{ PointValue.MaxValueCartesian_3d } ) };
		 internal static readonly Value[] ExtremeGeographicPointArray = new Value[]{ Values.Of( new PointValue[0] ), Values.Of( new PointValue[]{ PointValue.MaxValueWgs84 } ) };
		 internal static readonly Value[] ExtremeGeographicPoint_3dArray = new Value[]{ Values.Of( new PointValue[0] ), Values.Of( new PointValue[]{ PointValue.MaxValueWgs84_3d } ) };
	}

}