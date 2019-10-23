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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_BOOLEAN_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_BYTE_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_CARTESIAN_POINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_CARTESIAN_POINT_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_CARTESIAN_POINT_3D_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_CARTESIAN_POINT_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_CHAR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_CHAR_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_DATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_DATE_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_DATE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_DATE_TIME_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_DOUBLE_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_DURATION_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_FLOAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_FLOAT_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_GEOGRAPHIC_POINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_GEOGRAPHIC_POINT_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_GEOGRAPHIC_POINT_3D_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_GEOGRAPHIC_POINT_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_INT_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_LOCAL_DATE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_LOCAL_DATE_TIME_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_LOCAL_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_LOCAL_TIME_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_PERIOD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_PERIOD_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_SHORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_SHORT_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_STRING_ALPHANUMERIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_STRING_ALPHANUMERIC_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_STRING_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_STRING_ASCII;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_STRING_ASCII_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_STRING_BMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_STRING_BMP_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ExtremeValuesLibrary.EXTREME_TIME_ARRAY;

	public sealed class ValueType
	{
		 public static readonly ValueType Boolean = new ValueType( "Boolean", InnerEnum.Boolean, ValueGroup.Boolean, typeof( BooleanValue ), EXTREME_BOOLEAN );
		 public static readonly ValueType Byte = new ValueType( "Byte", InnerEnum.Byte, ValueGroup.Number, typeof( ByteValue ), EXTREME_BYTE );
		 public static readonly ValueType Short = new ValueType( "Short", InnerEnum.Short, ValueGroup.Number, typeof( ShortValue ), EXTREME_SHORT );
		 public static readonly ValueType Int = new ValueType( "Int", InnerEnum.Int, ValueGroup.Number, typeof( IntValue ), EXTREME_INT );
		 public static readonly ValueType Long = new ValueType( "Long", InnerEnum.Long, ValueGroup.Number, typeof( LongValue ), EXTREME_LONG );
		 public static readonly ValueType Float = new ValueType( "Float", InnerEnum.Float, ValueGroup.Number, typeof( FloatValue ), EXTREME_FLOAT );
		 public static readonly ValueType Double = new ValueType( "Double", InnerEnum.Double, ValueGroup.Number, typeof( DoubleValue ), EXTREME_DOUBLE );
		 public static readonly ValueType Char = new ValueType( "Char", InnerEnum.Char, ValueGroup.Text, typeof( CharValue ), EXTREME_CHAR );
		 public static readonly ValueType String = new ValueType( "String", InnerEnum.String, ValueGroup.Text, typeof( TextValue ), EXTREME_STRING );
		 public static readonly ValueType StringAlphanumeric = new ValueType( "StringAlphanumeric", InnerEnum.StringAlphanumeric, ValueGroup.Text, typeof( TextValue ), EXTREME_STRING_ALPHANUMERIC );
		 public static readonly ValueType StringAscii = new ValueType( "StringAscii", InnerEnum.StringAscii, ValueGroup.Text, typeof( TextValue ), EXTREME_STRING_ASCII );
		 public static readonly ValueType StringBmp = new ValueType( "StringBmp", InnerEnum.StringBmp, ValueGroup.Text, typeof( TextValue ), EXTREME_STRING_BMP );
		 public static readonly ValueType LocalDateTime = new ValueType( "LocalDateTime", InnerEnum.LocalDateTime, ValueGroup.LocalDateTime, typeof( LocalDateTimeValue ), EXTREME_LOCAL_DATE_TIME );
		 public static readonly ValueType Date = new ValueType( "Date", InnerEnum.Date, ValueGroup.Date, typeof( DateValue ), EXTREME_DATE );
		 public static readonly ValueType LocalTime = new ValueType( "LocalTime", InnerEnum.LocalTime, ValueGroup.LocalTime, typeof( LocalTimeValue ), EXTREME_LOCAL_TIME );
		 public static readonly ValueType Period = new ValueType( "Period", InnerEnum.Period, ValueGroup.Duration, typeof( DurationValue ), EXTREME_PERIOD );
		 public static readonly ValueType Duration = new ValueType( "Duration", InnerEnum.Duration, ValueGroup.Duration, typeof( DurationValue ), EXTREME_DURATION );
		 public static readonly ValueType Time = new ValueType( "Time", InnerEnum.Time, ValueGroup.ZonedTime, typeof( TimeValue ), EXTREME_TIME );
		 public static readonly ValueType DateTime = new ValueType( "DateTime", InnerEnum.DateTime, ValueGroup.ZonedDateTime, typeof( DateTimeValue ), EXTREME_DATE_TIME );
		 public static readonly ValueType CartesianPoint = new ValueType( "CartesianPoint", InnerEnum.CartesianPoint, ValueGroup.Geometry, typeof( PointValue ), EXTREME_CARTESIAN_POINT );
		 public static readonly ValueType CartesianPoint_3d = new ValueType( "CartesianPoint_3d", InnerEnum.CartesianPoint_3d, ValueGroup.Geometry, typeof( PointValue ), EXTREME_CARTESIAN_POINT_3D );
		 public static readonly ValueType GeographicPoint = new ValueType( "GeographicPoint", InnerEnum.GeographicPoint, ValueGroup.Geometry, typeof( PointValue ), EXTREME_GEOGRAPHIC_POINT );
		 public static readonly ValueType GeographicPoint_3d = new ValueType( "GeographicPoint_3d", InnerEnum.GeographicPoint_3d, ValueGroup.Geometry, typeof( PointValue ), EXTREME_GEOGRAPHIC_POINT_3D );
		 public static readonly ValueType BooleanArray = new ValueType( "BooleanArray", InnerEnum.BooleanArray, ValueGroup.BooleanArray, typeof( BooleanArray ), true, EXTREME_BOOLEAN_ARRAY );
		 public static readonly ValueType ByteArray = new ValueType( "ByteArray", InnerEnum.ByteArray, ValueGroup.NumberArray, typeof( ByteArray ), true, EXTREME_BYTE_ARRAY );
		 public static readonly ValueType ShortArray = new ValueType( "ShortArray", InnerEnum.ShortArray, ValueGroup.NumberArray, typeof( ShortArray ), true, EXTREME_SHORT_ARRAY );
		 public static readonly ValueType IntArray = new ValueType( "IntArray", InnerEnum.IntArray, ValueGroup.NumberArray, typeof( IntArray ), true, EXTREME_INT_ARRAY );
		 public static readonly ValueType LongArray = new ValueType( "LongArray", InnerEnum.LongArray, ValueGroup.NumberArray, typeof( LongArray ), true, EXTREME_LONG_ARRAY );
		 public static readonly ValueType FloatArray = new ValueType( "FloatArray", InnerEnum.FloatArray, ValueGroup.NumberArray, typeof( FloatArray ), true, EXTREME_FLOAT_ARRAY );
		 public static readonly ValueType DoubleArray = new ValueType( "DoubleArray", InnerEnum.DoubleArray, ValueGroup.NumberArray, typeof( DoubleArray ), true, EXTREME_DOUBLE_ARRAY );
		 public static readonly ValueType CharArray = new ValueType( "CharArray", InnerEnum.CharArray, ValueGroup.TextArray, typeof( CharArray ), true, EXTREME_CHAR_ARRAY );
		 public static readonly ValueType StringArray = new ValueType( "StringArray", InnerEnum.StringArray, ValueGroup.TextArray, typeof( StringArray ), true, EXTREME_STRING_ARRAY );
		 public static readonly ValueType StringAlphanumericArray = new ValueType( "StringAlphanumericArray", InnerEnum.StringAlphanumericArray, ValueGroup.TextArray, typeof( StringArray ), true, EXTREME_STRING_ALPHANUMERIC_ARRAY );
		 public static readonly ValueType StringAsciiArray = new ValueType( "StringAsciiArray", InnerEnum.StringAsciiArray, ValueGroup.TextArray, typeof( StringArray ), true, EXTREME_STRING_ASCII_ARRAY );
		 public static readonly ValueType StringBmpArray = new ValueType( "StringBmpArray", InnerEnum.StringBmpArray, ValueGroup.TextArray, typeof( StringArray ), true, EXTREME_STRING_BMP_ARRAY );
		 public static readonly ValueType LocalDateTimeArray = new ValueType( "LocalDateTimeArray", InnerEnum.LocalDateTimeArray, ValueGroup.LocalDateTimeArray, typeof( LocalDateTimeArray ), true, EXTREME_LOCAL_DATE_TIME_ARRAY );
		 public static readonly ValueType DateArray = new ValueType( "DateArray", InnerEnum.DateArray, ValueGroup.DateArray, typeof( DateArray ), true, EXTREME_DATE_ARRAY );
		 public static readonly ValueType LocalTimeArray = new ValueType( "LocalTimeArray", InnerEnum.LocalTimeArray, ValueGroup.LocalTimeArray, typeof( LocalTimeArray ), true, EXTREME_LOCAL_TIME_ARRAY );
		 public static readonly ValueType PeriodArray = new ValueType( "PeriodArray", InnerEnum.PeriodArray, ValueGroup.DurationArray, typeof( DurationArray ), true, EXTREME_PERIOD_ARRAY );
		 public static readonly ValueType DurationArray = new ValueType( "DurationArray", InnerEnum.DurationArray, ValueGroup.DurationArray, typeof( DurationArray ), true, EXTREME_DURATION_ARRAY );
		 public static readonly ValueType TimeArray = new ValueType( "TimeArray", InnerEnum.TimeArray, ValueGroup.ZonedTimeArray, typeof( TimeArray ), true, EXTREME_TIME_ARRAY );
		 public static readonly ValueType DateTimeArray = new ValueType( "DateTimeArray", InnerEnum.DateTimeArray, ValueGroup.ZonedDateTimeArray, typeof( DateTimeArray ), true, EXTREME_DATE_TIME_ARRAY );
		 public static readonly ValueType CartesianPointArray = new ValueType( "CartesianPointArray", InnerEnum.CartesianPointArray, ValueGroup.GeometryArray, typeof( PointArray ), true, EXTREME_CARTESIAN_POINT_ARRAY );
		 public static readonly ValueType CartesianPoint_3dArray = new ValueType( "CartesianPoint_3dArray", InnerEnum.CartesianPoint_3dArray, ValueGroup.GeometryArray, typeof( PointArray ), true, EXTREME_CARTESIAN_POINT_3D_ARRAY );
		 public static readonly ValueType GeographicPointArray = new ValueType( "GeographicPointArray", InnerEnum.GeographicPointArray, ValueGroup.GeometryArray, typeof( PointArray ), true, EXTREME_GEOGRAPHIC_POINT_ARRAY );
		 public static readonly ValueType GeographicPoint_3dArray = new ValueType( "GeographicPoint_3dArray", InnerEnum.GeographicPoint_3dArray, ValueGroup.GeometryArray, typeof( PointArray ), true, EXTREME_GEOGRAPHIC_POINT_3D_ARRAY );

		 private static readonly IList<ValueType> valueList = new List<ValueType>();

		 static ValueType()
		 {
			 valueList.Add( Boolean );
			 valueList.Add( Byte );
			 valueList.Add( Short );
			 valueList.Add( Int );
			 valueList.Add( Long );
			 valueList.Add( Float );
			 valueList.Add( Double );
			 valueList.Add( Char );
			 valueList.Add( String );
			 valueList.Add( StringAlphanumeric );
			 valueList.Add( StringAscii );
			 valueList.Add( StringBmp );
			 valueList.Add( LocalDateTime );
			 valueList.Add( Date );
			 valueList.Add( LocalTime );
			 valueList.Add( Period );
			 valueList.Add( Duration );
			 valueList.Add( Time );
			 valueList.Add( DateTime );
			 valueList.Add( CartesianPoint );
			 valueList.Add( CartesianPoint_3d );
			 valueList.Add( GeographicPoint );
			 valueList.Add( GeographicPoint_3d );
			 valueList.Add( BooleanArray );
			 valueList.Add( ByteArray );
			 valueList.Add( ShortArray );
			 valueList.Add( IntArray );
			 valueList.Add( LongArray );
			 valueList.Add( FloatArray );
			 valueList.Add( DoubleArray );
			 valueList.Add( CharArray );
			 valueList.Add( StringArray );
			 valueList.Add( StringAlphanumericArray );
			 valueList.Add( StringAsciiArray );
			 valueList.Add( StringBmpArray );
			 valueList.Add( LocalDateTimeArray );
			 valueList.Add( DateArray );
			 valueList.Add( LocalTimeArray );
			 valueList.Add( PeriodArray );
			 valueList.Add( DurationArray );
			 valueList.Add( TimeArray );
			 valueList.Add( DateTimeArray );
			 valueList.Add( CartesianPointArray );
			 valueList.Add( CartesianPoint_3dArray );
			 valueList.Add( GeographicPointArray );
			 valueList.Add( GeographicPoint_3dArray );
		 }

		 public enum InnerEnum
		 {
			 Boolean,
			 Byte,
			 Short,
			 Int,
			 Long,
			 Float,
			 Double,
			 Char,
			 String,
			 StringAlphanumeric,
			 StringAscii,
			 StringBmp,
			 LocalDateTime,
			 Date,
			 LocalTime,
			 Period,
			 Duration,
			 Time,
			 DateTime,
			 CartesianPoint,
			 CartesianPoint_3d,
			 GeographicPoint,
			 GeographicPoint_3d,
			 BooleanArray,
			 ByteArray,
			 ShortArray,
			 IntArray,
			 LongArray,
			 FloatArray,
			 DoubleArray,
			 CharArray,
			 StringArray,
			 StringAlphanumericArray,
			 StringAsciiArray,
			 StringBmpArray,
			 LocalDateTimeArray,
			 DateArray,
			 LocalTimeArray,
			 PeriodArray,
			 DurationArray,
			 TimeArray,
			 DateTimeArray,
			 CartesianPointArray,
			 CartesianPoint_3dArray,
			 GeographicPointArray,
			 GeographicPoint_3dArray
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Public readonly;
		 internal Public readonly;
		 internal Public readonly;
		 internal Private readonly;

		 internal ValueType( string name, InnerEnum innerEnum, ValueGroup valueGroup, Type valueClass, params Value[] extremeValues ) : this( valueGroup, valueClass, false, extremeValues )
		 {

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal ValueType( string name, InnerEnum innerEnum, ValueGroup valueGroup, Type valueClass, bool arrayType, params Value[] extremeValues )
		 {
			  this.ValueGroup = valueGroup;
			  this.ValueClass = valueClass;
			  this.ArrayType = arrayType;
			  this._extremeValues = extremeValues;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public Value[] ExtremeValues()
		 {
			  return _extremeValues;
		 }

		 internal static ValueType[] ArrayTypes()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return java.util.ValueType.values().Where(t => t.arrayType).ToArray(ValueType[]::new);
		 }

		public static IList<ValueType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static ValueType ValueOf( string name )
		{
			foreach ( ValueType enumInstance in ValueType.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}