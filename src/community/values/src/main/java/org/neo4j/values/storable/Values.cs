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

	using CRS = Neo4Net.GraphDb.spatial.CRS;
	using Point = Neo4Net.GraphDb.spatial.Point;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.TimeValue.time;

	/// <summary>
	/// Entry point to the values library.
	/// <para>
	/// The values library centers around the Value class, which represents a value in Neo4Net. Values can be correctly
	/// checked for equality over different primitive representations, including consistent hashCodes and sorting.
	/// </para>
	/// <para>
	/// To create Values use the factory methods in the Values class.
	/// </para>
	/// <para>
	/// Values come in two major categories: Storable and Virtual. Storable values are valid values for
	/// node, relationship and graph properties. Virtual values are not supported as property values, but might be created
	/// and returned as part of cypher execution. These include Node, Relationship and Path.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public final class Values
	public sealed class Values
	{
		 public static readonly Value MinNumber = Values.DoubleValue( double.NegativeInfinity );
		 public static readonly Value MaxNumber = Values.DoubleValue( Double.NaN );
		 public static readonly Value ZeroFloat = Values.DoubleValue( 0.0 );
		 public static readonly IntegralValue ZeroInt = Values.LongValue( 0 );
		 public static readonly Value MinString = StringValue.EMPTY;
		 public static readonly Value MaxString = Values.BooleanValue( false );
		 public static readonly BooleanValue True = Values.BooleanValue( true );
		 public static readonly BooleanValue False = Values.BooleanValue( false );
		 public static readonly TextValue EmptyString = StringValue.EMPTY;
		 public static readonly DoubleValue E = Values.DoubleValue( Math.E );
		 public static readonly DoubleValue Pi = Values.DoubleValue( Math.PI );
		 public static readonly ArrayValue EmptyShortArray = Values.ShortArray( new short[0] );
		 public static readonly ArrayValue EmptyBooleanArray = Values.BooleanArray( new bool[0] );
		 public static readonly ArrayValue EmptyByteArray = Values.ByteArray( new sbyte[0] );
		 public static readonly ArrayValue EmptyCharArray = Values.CharArray( new char[0] );
		 public static readonly ArrayValue EmptyIntArray = Values.IntArray( new int[0] );
		 public static readonly ArrayValue EmptyLongArray = Values.LongArray( new long[0] );
		 public static readonly ArrayValue EmptyFloatArray = Values.FloatArray( new float[0] );
		 public static readonly ArrayValue EmptyDoubleArray = Values.DoubleArray( new double[0] );
		 public static readonly TextArray EmptyTextArray = Values.StringArray();

		 private Values()
		 {
		 }

		 /// <summary>
		 /// Default value comparator. Will correctly compare all storable values and order the value groups according the
		 /// to orderability group.
		 /// 
		 /// To get Comparability semantics, use .ternaryCompare
		 /// </summary>
		 public static readonly ValueComparator Comparator = new ValueComparator( ValueGroup.compareTo );

		 public static bool IsNumberValue( object value )
		 {
			  return value is NumberValue;
		 }

		 public static bool IsBooleanValue( object value )
		 {
			  return value is BooleanValue;
		 }

		 public static bool IsTextValue( object value )
		 {
			  return value is TextValue;
		 }

		 public static bool IsArrayValue( Value value )
		 {
			  return value is ArrayValue;
		 }

		 public static bool IsGeometryValue( Value value )
		 {
			  return value is PointValue;
		 }

		 public static bool IsGeometryArray( Value value )
		 {
			  return value is PointArray;
		 }

		 public static bool IsTemporalValue( Value value )
		 {
			  return value is TemporalValue || value is DurationValue;
		 }

		 public static bool IsTemporalArray( Value value )
		 {
			  return value is TemporalArray || value is DurationArray;
		 }

		 public static double CoerceToDouble( Value value )
		 {
			  if ( value is IntegralValue )
			  {
					return ( ( IntegralValue ) value ).LongValue();
			  }
			  if ( value is FloatingPointValue )
			  {
					return ( ( FloatingPointValue ) value ).DoubleValue();
			  }
			  throw new System.NotSupportedException( format( "Cannot coerce %s to double", value ) );
		 }

		 // DIRECT FACTORY METHODS

		 public static readonly Value NoValue = NoValue.NoValueConflict;

		 public static TextValue Utf8Value( sbyte[] bytes )
		 {
			  if ( bytes.Length == 0 )
			  {
					return EmptyString;
			  }

			  return Utf8Value( bytes, 0, bytes.Length );
		 }

		 public static TextValue Utf8Value( sbyte[] bytes, int offset, int length )
		 {
			  if ( length == 0 )
			  {
					return EmptyString;
			  }

			  return new UTF8StringValue( bytes, offset, length );
		 }

		 public static TextValue StringValue( string value )
		 {
			  if ( value.Length == 0 )
			  {
					return EmptyString;
			  }
			  return new StringWrappingStringValue( value );
		 }

		 public static Value StringOrNoValue( string value )
		 {
			  if ( string.ReferenceEquals( value, null ) )
			  {
					return NoValue;
			  }
			  else
			  {
					return StringValue( value );
			  }
		 }

		 public static NumberValue NumberValue( Number number )
		 {
			  if ( number is long? )
			  {
					return LongValue( number.longValue() );
			  }
			  if ( number is int? )
			  {
					return IntValue( number.intValue() );
			  }
			  if ( number is double? )
			  {
					return DoubleValue( number.doubleValue() );
			  }
			  if ( number is sbyte? )
			  {
					return ByteValue( number.byteValue() );
			  }
			  if ( number is float? )
			  {
					return FloatValue( number.floatValue() );
			  }
			  if ( number is short? )
			  {
					return ShortValue( number.shortValue() );
			  }

			  throw new System.NotSupportedException( "Unsupported type of Number " + number.ToString() );
		 }

		 public static LongValue LongValue( long value )
		 {
			  return new LongValue( value );
		 }

		 public static IntValue IntValue( int value )
		 {
			  return new IntValue( value );
		 }

		 public static ShortValue ShortValue( short value )
		 {
			  return new ShortValue( value );
		 }

		 public static ByteValue ByteValue( sbyte value )
		 {
			  return new ByteValue( value );
		 }

		 public static BooleanValue BooleanValue( bool value )
		 {
			  return value ? BooleanValue.TRUE : BooleanValue.FALSE;
		 }

		 public static CharValue CharValue( char value )
		 {
			  return new CharValue( value );
		 }

		 public static DoubleValue DoubleValue( double value )
		 {
			  return new DoubleValue( value );
		 }

		 public static FloatValue FloatValue( float value )
		 {
			  return new FloatValue( value );
		 }

		 public static TextArray StringArray( params string[] value )
		 {
			  return new StringArray( value );
		 }

		 public static ByteArray ByteArray( sbyte[] value )
		 {
			  return new ByteArray( value );
		 }

		 public static LongArray LongArray( long[] value )
		 {
			  return new LongArray( value );
		 }

		 public static IntArray IntArray( int[] value )
		 {
			  return new IntArray( value );
		 }

		 public static DoubleArray DoubleArray( double[] value )
		 {
			  return new DoubleArray( value );
		 }

		 public static FloatArray FloatArray( float[] value )
		 {
			  return new FloatArray( value );
		 }

		 public static BooleanArray BooleanArray( bool[] value )
		 {
			  return new BooleanArray( value );
		 }

		 public static CharArray CharArray( char[] value )
		 {
			  return new CharArray( value );
		 }

		 public static ShortArray ShortArray( short[] value )
		 {
			  return new ShortArray( value );
		 }

		 /// <summary>
		 /// Unlike pointValue(), this method does not enforce consistency between the CRS and coordinate dimensions.
		 /// This can be useful for testing.
		 /// </summary>
		 public static PointValue UnsafePointValue( CoordinateReferenceSystem crs, params double[] coordinate )
		 {
			  return new PointValue( crs, coordinate );
		 }

		 /// <summary>
		 /// Creates a PointValue, and enforces consistency between the CRS and coordinate dimensions.
		 /// </summary>
		 public static PointValue PointValue( CoordinateReferenceSystem crs, params double[] coordinate )
		 {
			  if ( crs.Dimension != coordinate.Length )
			  {
					throw new System.ArgumentException( format( "Cannot create point, CRS %s expects %d dimensions, but got coordinates %s", crs, crs.Dimension, Arrays.ToString( coordinate ) ) );
			  }
			  return new PointValue( crs, coordinate );
		 }

		 public static PointValue Point( Point point )
		 {
			  // An optimization could be to do an instanceof PointValue check here
			  // and in that case just return the casted argument.
			  IList<double> coordinate = point.Coordinate.Coordinate;
			  double[] coords = new double[coordinate.Count];
			  for ( int i = 0; i < coords.Length; i++ )
			  {
					coords[i] = coordinate[i];
			  }
			  return new PointValue( Crs( point.CRS ), coords );
		 }

		 public static PointValue MinPointValue( PointValue reference )
		 {
			  double[] coordinates = new double[reference.Coordinate().Length];
			  Arrays.fill( coordinates, -double.MaxValue );
			  return PointValue( reference.CoordinateReferenceSystem, coordinates );
		 }

		 public static PointValue MaxPointValue( PointValue reference )
		 {
			  double[] coordinates = new double[reference.Coordinate().Length];
			  Arrays.fill( coordinates, double.MaxValue );
			  return PointValue( reference.CoordinateReferenceSystem, coordinates );
		 }

		 public static PointArray PointArray( Point[] points )
		 {
			  PointValue[] values = new PointValue[points.Length];
			  for ( int i = 0; i < points.Length; i++ )
			  {
					values[i] = Values.Point( points[i] );
			  }
			  return new PointArray( values );
		 }

		 public static PointArray PointArray( Value[] maybePoints )
		 {
			  PointValue[] values = new PointValue[maybePoints.Length];
			  for ( int i = 0; i < maybePoints.Length; i++ )
			  {
					Value maybePoint = maybePoints[i];
					if ( !( maybePoint is PointValue ) )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 throw new System.ArgumentException( format( "[%s:%s] is not a supported point value", maybePoint, maybePoint.GetType().FullName ) );
					}
					values[i] = Values.Point( ( PointValue ) maybePoint );
			  }
			  return PointArray( values );
		 }

		 public static PointArray PointArray( PointValue[] points )
		 {
			  return new PointArray( points );
		 }

		 public static CoordinateReferenceSystem Crs( CRS crs )
		 {
			  return CoordinateReferenceSystem.Get( crs );
		 }

		 public static Value TemporalValue( Temporal value )
		 {
			  if ( value is ZonedDateTime )
			  {
					return datetime( ( ZonedDateTime ) value );
			  }
			  if ( value is OffsetDateTime )
			  {
					return datetime( ( OffsetDateTime ) value );
			  }
			  if ( value is DateTime )
			  {
					return localDateTime( ( DateTime ) value );
			  }
			  if ( value is OffsetTime )
			  {
					return time( ( OffsetTime ) value );
			  }
			  if ( value is LocalDate )
			  {
					return date( ( LocalDate ) value );
			  }
			  if ( value is LocalTime )
			  {
					return localTime( ( LocalTime ) value );
			  }
			  if ( value is TemporalValue )
			  {
					return ( Value ) value;
			  }
			  if ( value == null )
			  {
					return NoValue;
			  }

			  throw new System.NotSupportedException( "Unsupported type of Temporal " + value.ToString() );
		 }

		 public static DurationValue DurationValue( TemporalAmount value )
		 {
			  if ( value is Duration )
			  {
					return duration( ( Duration ) value );
			  }
			  if ( value is Period )
			  {
					return duration( ( Period ) value );
			  }
			  if ( value is DurationValue )
			  {
					return ( DurationValue ) value;
			  }
			  DurationValue duration = duration( 0, 0, 0, 0 );
			  foreach ( TemporalUnit unit in value.Units )
			  {
					duration = duration.Plus( value.get( unit ), unit );
			  }
			  return duration;
		 }

		 public static DateTimeArray DateTimeArray( ZonedDateTime[] values )
		 {
			  return new DateTimeArray( values );
		 }

		 public static LocalDateTimeArray LocalDateTimeArray( DateTime[] values )
		 {
			  return new LocalDateTimeArray( values );
		 }

		 public static LocalTimeArray LocalTimeArray( LocalTime[] values )
		 {
			  return new LocalTimeArray( values );
		 }

		 public static TimeArray TimeArray( OffsetTime[] values )
		 {
			  return new TimeArray( values );
		 }

		 public static DateArray DateArray( LocalDate[] values )
		 {
			  return new DateArray( values );
		 }

		 public static DurationArray DurationArray( DurationValue[] values )
		 {
			  return new DurationArray( values );
		 }

		 public static DurationArray DurationArray( TemporalAmount[] values )
		 {
			  DurationValue[] durations = new DurationValue[values.Length];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					durations[i] = DurationValue( values[i] );
			  }
			  return new DurationArray( durations );
		 }

		 // BOXED FACTORY METHODS

		 /// <summary>
		 /// Generic value factory method.
		 /// <para>
		 /// Beware, this method is intended for converting externally supplied values to the internal Value type, and to
		 /// make testing convenient. Passing a Value as in parameter should never be needed, and will throw an
		 /// UnsupportedOperationException.
		 /// </para>
		 /// <para>
		 /// This method does defensive copying of arrays, while the explicit *Array() factory methods do not.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="value"> Object to convert to Value </param>
		 /// <returns> the created Value </returns>
		 public static Value Of( object value )
		 {
			  return Of( value, true );
		 }

		 public static Value Of( object value, bool allowNull )
		 {
			  Value of = UnsafeOf( value, allowNull );
			  if ( of != null )
			  {
					return of;
			  }
			  Objects.requireNonNull( value );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( format( "[%s:%s] is not a supported property value", value, value.GetType().FullName ) );
		 }

		 public static Value UnsafeOf( object value, bool allowNull )
		 {
			  if ( value is string )
			  {
					return StringValue( ( string ) value );
			  }
			  if ( value is object[] )
			  {
					return ArrayValue( ( object[] ) value );
			  }
			  if ( value is bool? )
			  {
					return BooleanValue( ( bool? ) value.Value );
			  }
			  if ( value is Number )
			  {
					return NumberValue( ( Number ) value );
			  }
			  if ( value is char? )
			  {
					return CharValue( ( char? ) value.Value );
			  }
			  if ( value is Temporal )
			  {
					return TemporalValue( ( Temporal ) value );
			  }
			  if ( value is TemporalAmount )
			  {
					return DurationValue( ( TemporalAmount ) value );
			  }
			  if ( value is sbyte[] )
			  {
					return ByteArray( ( ( sbyte[] ) value ).Clone() );
			  }
			  if ( value is long[] )
			  {
					return LongArray( ( ( long[] ) value ).Clone() );
			  }
			  if ( value is int[] )
			  {
					return IntArray( ( ( int[] ) value ).Clone() );
			  }
			  if ( value is double[] )
			  {
					return DoubleArray( ( ( double[] ) value ).Clone() );
			  }
			  if ( value is float[] )
			  {
					return FloatArray( ( ( float[] ) value ).Clone() );
			  }
			  if ( value is bool[] )
			  {
					return BooleanArray( ( ( bool[] ) value ).Clone() );
			  }
			  if ( value is char[] )
			  {
					return CharArray( ( ( char[] ) value ).Clone() );
			  }
			  if ( value is short[] )
			  {
					return ShortArray( ( ( short[] ) value ).Clone() );
			  }
			  if ( value == null )
			  {
					if ( allowNull )
					{
						 return NoValue.NoValueConflict;
					}
					throw new System.ArgumentException( "[null] is not a supported property value" );
			  }
			  if ( value is Point )
			  {
					return Values.Point( ( Point ) value );
			  }
			  if ( value is Value )
			  {
					throw new System.NotSupportedException( "Converting a Value to a Value using Values.of() is not supported." );
			  }

			  // otherwise fail
			 return null;
		 }

		 /// <summary>
		 /// Generic value factory method.
		 /// <para>
		 /// Converts an array of object values to the internal Value type. See <seealso cref="Values.of"/>.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Value[] ValuesConflict( params object[] objects )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return java.util.objects.Select( Values.of ).ToArray( Value[]::new );
		 }

		 [Obsolete]
		 public static object AsObject( Value value )
		 {
			  return value == null ? null : value.AsObject();
		 }

		 public static object[] AsObjects( Value[] propertyValues )
		 {
			  object[] legacy = new object[propertyValues.Length];

			  for ( int i = 0; i < propertyValues.Length; i++ )
			  {
					legacy[i] = propertyValues[i].AsObjectCopy();
			  }

			  return legacy;
		 }

		 private static Value ArrayValue( object[] value )
		 {
			  if ( value is string[] )
			  {
					return StringArray( Copy( value, new string[value.Length] ) );
			  }
			  if ( value is sbyte?[] )
			  {
					return ByteArray( Copy( value, new sbyte[value.Length] ) );
			  }
			  if ( value is long?[] )
			  {
					return LongArray( Copy( value, new long[value.Length] ) );
			  }
			  if ( value is int?[] )
			  {
					return IntArray( Copy( value, new int[value.Length] ) );
			  }
			  if ( value is double?[] )
			  {
					return DoubleArray( Copy( value, new double[value.Length] ) );
			  }
			  if ( value is float?[] )
			  {
					return FloatArray( Copy( value, new float[value.Length] ) );
			  }
			  if ( value is bool?[] )
			  {
					return BooleanArray( Copy( value, new bool[value.Length] ) );
			  }
			  if ( value is char?[] )
			  {
					return CharArray( Copy( value, new char[value.Length] ) );
			  }
			  if ( value is short?[] )
			  {
					return ShortArray( Copy( value, new short[value.Length] ) );
			  }
			  if ( value is PointValue[] )
			  {
					return PointArray( Copy( value, new PointValue[value.Length] ) );
			  }
			  if ( value is Point[] )
			  {
					// no need to copy here, since the pointArray(...) method will copy into a PointValue[]
					return PointArray( ( Point[] )value );
			  }
			  if ( value is ZonedDateTime[] )
			  {
					return DateTimeArray( Copy( value, new ZonedDateTime[value.Length] ) );
			  }
			  if ( value is DateTime[] )
			  {
					return LocalDateTimeArray( Copy( value, new DateTime[value.Length] ) );
			  }
			  if ( value is LocalTime[] )
			  {
					return LocalTimeArray( Copy( value, new LocalTime[value.Length] ) );
			  }
			  if ( value is OffsetTime[] )
			  {
					return TimeArray( Copy( value, new OffsetTime[value.Length] ) );
			  }
			  if ( value is LocalDate[] )
			  {
					return DateArray( Copy( value, new LocalDate[value.Length] ) );
			  }
			  if ( value is TemporalAmount[] )
			  {
					// no need to copy here, since the durationArray(...) method will perform copying as appropriate
					return DurationArray( ( TemporalAmount[] ) value );
			  }
			  return null;
		 }

		 private static T Copy<T>( object[] value, T target )
		 {
			  for ( int i = 0; i < value.Length; i++ )
			  {
					if ( value[i] == null )
					{
						 throw new System.ArgumentException( "Property array value elements may not be null." );
					}
					( ( Array )target ).SetValue( value[i], i );
			  }
			  return target;
		 }

		 public static Value MinValue( ValueGroup valueGroup, Value value )
		 {
			  switch ( valueGroup.innerEnumValue )
			  {
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.TEXT:
				  return MinString;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.NUMBER:
				  return MinNumber;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.GEOMETRY:
				  return MinPointValue( ( PointValue )value );
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.DATE:
				  return DateValue.MinValue;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.LOCAL_DATE_TIME:
				  return LocalDateTimeValue.MinValue;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.ZONED_DATE_TIME:
				  return DateTimeValue.MinValue;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.LOCAL_TIME:
				  return LocalTimeValue.MinValue;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.ZONED_TIME:
				  return TimeValue.MinValue;
			  default:
				  throw new System.InvalidOperationException( format( "The minValue for valueGroup %s is not defined yet", valueGroup ) );
			  }
		 }

		 public static Value MaxValue( ValueGroup valueGroup, Value value )
		 {
			  switch ( valueGroup.innerEnumValue )
			  {
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.TEXT:
				  return MaxString;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.NUMBER:
				  return MaxNumber;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.GEOMETRY:
				  return MaxPointValue( ( PointValue )value );
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.DATE:
				  return DateValue.MaxValue;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.LOCAL_DATE_TIME:
				  return LocalDateTimeValue.MaxValue;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.ZONED_DATE_TIME:
				  return DateTimeValue.MaxValue;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.LOCAL_TIME:
				  return LocalTimeValue.MaxValue;
			  case Neo4Net.Values.Storable.ValueGroup.InnerEnum.ZONED_TIME:
				  return TimeValue.MaxValue;
			  default:
				  throw new System.InvalidOperationException( format( "The maxValue for valueGroup %s is not defined yet", valueGroup ) );
			  }
		 }
	}

}