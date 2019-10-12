using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

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
namespace Org.Neo4j.Values.Storable
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;


	using Predicates = Org.Neo4j.Function.Predicates;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
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
//	import static org.neo4j.values.storable.Values.byteValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.floatValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.shortValue;

	/// <summary>
	/// Helper class that generates generator values of all supported types.
	/// <para>
	/// Generated values are always uniformly distributed in pseudorandom fashion.
	/// </para>
	/// <para>
	/// Can generate both <seealso cref="Value"/> and "raw" instances. The "raw" type of a value type means
	/// the corresponding Core API type if such type exists. For example, {@code String[]} is the raw type of <seealso cref="TextArray"/>.
	/// </para>
	/// <para>
	/// The length of strings will be governed by <seealso cref="RandomValues.Configuration.stringMinLength()"/> and
	/// <seealso cref="RandomValues.Configuration.stringMaxLength()"/> and
	/// the length of arrays will be governed by <seealso cref="RandomValues.Configuration.arrayMinLength()"/> and
	/// <seealso cref="RandomValues.Configuration.arrayMaxLength()"/>
	/// unless method provide explicit arguments for those configurations in which case the provided argument will be used instead.
	/// </para>
	/// </summary>
	public class RandomValues
	{
		 public interface Configuration
		 {
			  int StringMinLength();

			  int StringMaxLength();

			  int ArrayMinLength();

			  int ArrayMaxLength();

			  int MaxCodePoint();
		 }

		 public class Default : Configuration
		 {
			  public override int StringMinLength()
			  {
					return 5;
			  }

			  public override int StringMaxLength()
			  {
					return 20;
			  }

			  public override int ArrayMinLength()
			  {
					return 1;
			  }

			  public override int ArrayMaxLength()
			  {
					return 10;
			  }

			  public override int MaxCodePoint()
			  {
					return Character.MAX_CODE_POINT;
			  }
		 }

		 public const int MAX_BMP_CODE_POINT = 0xFFFF;
		 public static readonly Configuration DefaultConfiguration = new Default();
		 internal const int MAX_ASCII_CODE_POINT = 0x7F;
		 private static readonly ValueType[] _allTypes = ValueType.values();
		 private static readonly ValueType[] _arrayTypes = ValueType.arrayTypes();
		 private const long NANOS_PER_SECOND = 1_000_000_000L;

		 private readonly Generator _generator;
		 private readonly Configuration _configuration;

		 private RandomValues( Generator generator ) : this( generator, DefaultConfiguration )
		 {
		 }

		 private RandomValues( Generator generator, Configuration configuration )
		 {
			  this._generator = generator;
			  this._configuration = configuration;
		 }

		 /// <summary>
		 /// Create a {@code RandomValues} with default configuration
		 /// </summary>
		 /// <returns> a {@code RandomValues} instance </returns>
		 public static RandomValues Create()
		 {
			  return new RandomValues( new RandomGenerator( ThreadLocalRandom.current() ) );
		 }

		 /// <summary>
		 /// Create a {@code RandomValues} with the given configuration
		 /// </summary>
		 /// <returns> a {@code RandomValues} instance </returns>
		 public static RandomValues Create( Configuration configuration )
		 {
			  return new RandomValues( new RandomGenerator( ThreadLocalRandom.current() ), configuration );
		 }

		 /// <summary>
		 /// Create a {@code RandomValues} using the given <seealso cref="System.Random"/> with given configuration
		 /// </summary>
		 /// <returns> a {@code RandomValues} instance </returns>
		 public static RandomValues Create( Random random, Configuration configuration )
		 {
			  return new RandomValues( new RandomGenerator( random ), configuration );
		 }

		 /// <summary>
		 /// Create a {@code RandomValues} using the given <seealso cref="System.Random"/> with default configuration
		 /// </summary>
		 /// <returns> a {@code RandomValues} instance </returns>
		 public static RandomValues Create( Random random )
		 {
			  return new RandomValues( new RandomGenerator( random ) );
		 }

		 /// <summary>
		 /// Create a {@code RandomValues} using the given <seealso cref="SplittableRandom"/> with given configuration
		 /// </summary>
		 /// <returns> a {@code RandomValues} instance </returns>
		 public static RandomValues Create( SplittableRandom random, Configuration configuration )
		 {
			  return new RandomValues( new SplittableRandomGenerator( random ), configuration );
		 }

		 /// <summary>
		 /// Create a {@code RandomValues} using the given <seealso cref="SplittableRandom"/> with default configuration
		 /// </summary>
		 /// <returns> a {@code RandomValues} instance </returns>
		 public static RandomValues Create( SplittableRandom random )
		 {
			  return new RandomValues( new SplittableRandomGenerator( random ) );
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="Value"/>, distributed uniformly among the supported Value types.
		 /// </summary>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual Value NextValue()
		 {
			  return NextValueOfType( Among( _allTypes ) );
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="Value"/>, distributed uniformly among the provided value types.
		 /// </summary>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual Value NextValueOfTypes( params ValueType[] types )
		 {
			  return NextValueOfType( Among( types ) );
		 }

		 public static ValueType[] Including( System.Predicate<ValueType> include )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return java.util.ValueType.values().Where(include).ToArray(ValueType[]::new);
		 }

		 /// <summary>
		 /// Create an array containing all value types, excluding provided types.
		 /// </summary>
		 public static ValueType[] Excluding( params ValueType[] exclude )
		 {
			  return Excluding( ValueType.values(), exclude );
		 }

		 public static ValueType[] Excluding( ValueType[] among, params ValueType[] exclude )
		 {
			  return Excluding( among, t => ArrayUtils.contains( exclude, t ) );
		 }

		 public static ValueType[] Excluding( ValueType[] among, System.Predicate<ValueType> exclude )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return java.util.among.Where( Predicates.not( exclude ) ).ToArray( ValueType[]::new );
		 }

		 public static ValueType[] TypesOfGroup( ValueGroup valueGroup )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return java.util.ValueType.values().Where(t => t.valueGroup == valueGroup).ToArray(ValueType[]::new);
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="Value"/> of provided type.
		 /// </summary>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual Value NextValueOfType( ValueType type )
		 {
			  switch ( type.innerEnumValue )
			  {
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.BOOLEAN:
					return NextBooleanValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.BYTE:
					return NextByteValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.SHORT:
					return NextShortValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.STRING:
					return NextTextValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.INT:
					return NextIntValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.LONG:
					return NextLongValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.FLOAT:
					return NextFloatValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.DOUBLE:
					return NextDoubleValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.CHAR:
					return NextCharValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.STRING_ALPHANUMERIC:
					return NextAlphaNumericTextValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.STRING_ASCII:
					return NextAsciiTextValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.STRING_BMP:
					return NextBasicMultilingualPlaneTextValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.LOCAL_DATE_TIME:
					return NextLocalDateTimeValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.DATE:
					return NextDateValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.LOCAL_TIME:
					return NextLocalTimeValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.PERIOD:
					return NextPeriod();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.DURATION:
					return NextDuration();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.TIME:
					return NextTimeValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.DATE_TIME:
					return NextDateTimeValue();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.CARTESIAN_POINT:
					return NextCartesianPoint();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.CARTESIAN_POINT_3D:
					return NextCartesian3DPoint();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.GEOGRAPHIC_POINT:
					return NextGeographicPoint();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.GEOGRAPHIC_POINT_3D:
					return NextGeographic3DPoint();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.BOOLEAN_ARRAY:
					return NextBooleanArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.BYTE_ARRAY:
					return NextByteArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.SHORT_ARRAY:
					return NextShortArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.INT_ARRAY:
					return NextIntArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.LONG_ARRAY:
					return NextLongArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.FLOAT_ARRAY:
					return NextFloatArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.DOUBLE_ARRAY:
					return NextDoubleArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.CHAR_ARRAY:
					return NextCharArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.STRING_ARRAY:
					return NextTextArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.STRING_ALPHANUMERIC_ARRAY:
					return NextAlphaNumericTextArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.STRING_ASCII_ARRAY:
					return NextAsciiTextArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.STRING_BMP_ARRAY:
					return NextBasicMultilingualPlaneTextArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.LOCAL_DATE_TIME_ARRAY:
					return NextLocalDateTimeArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.DATE_ARRAY:
					return NextDateArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.LOCAL_TIME_ARRAY:
					return NextLocalTimeArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.PERIOD_ARRAY:
					return NextPeriodArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.DURATION_ARRAY:
					return NextDurationArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.TIME_ARRAY:
					return NextTimeArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.DATE_TIME_ARRAY:
					return NextDateTimeArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.CARTESIAN_POINT_ARRAY:
					return NextCartesianPointArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.CARTESIAN_POINT_3D_ARRAY:
					return NextCartesian3DPointArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.GEOGRAPHIC_POINT_ARRAY:
					return NextGeographicPointArray();
			  case Org.Neo4j.Values.Storable.ValueType.InnerEnum.GEOGRAPHIC_POINT_3D_ARRAY:
					return NextGeographic3DPointArray();
			  default:
					throw new System.ArgumentException( "Unknown value type: " + type );
			  }
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="ArrayValue"/>, distributed uniformly among all array types.
		 /// </summary>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual ArrayValue NextArray()
		 {
			  return ( ArrayValue ) NextValueOfType( Among( _arrayTypes ) );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual BooleanValue NextBooleanValue()
		 {
			  return Values.BooleanValue( _generator.nextBoolean() );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual bool NextBoolean()
		 {
			  return _generator.nextBoolean();
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual ByteValue NextByteValue()
		 {
			  return byteValue( ( sbyte ) _generator.Next() );
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="ByteValue"/> between 0 (inclusive) and the specified value (exclusive)
		 /// </summary>
		 /// <param name="bound"> the upper bound (exclusive).  Must be positive. </param>
		 /// <returns> <seealso cref="ByteValue"/> </returns>
		 public virtual ByteValue NextByteValue( sbyte bound )
		 {
			  return byteValue( ( sbyte ) _generator.Next( bound ) );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual ShortValue NextShortValue()
		 {
			  return shortValue( ( short ) _generator.Next() );
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="ShortValue"/> between 0 (inclusive) and the specified value (exclusive)
		 /// </summary>
		 /// <param name="bound"> the upper bound (exclusive).  Must be positive. </param>
		 /// <returns> <seealso cref="ShortValue"/> </returns>
		 public virtual ShortValue NextShortValue( short bound )
		 {
			  return shortValue( ( short ) _generator.Next( bound ) );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual IntValue NextIntValue()
		 {
			  return intValue( _generator.Next() );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual int NextInt()
		 {
			  return _generator.Next();
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="IntValue"/> between 0 (inclusive) and the specified value (exclusive)
		 /// </summary>
		 /// <param name="bound"> the upper bound (exclusive).  Must be positive. </param>
		 /// <returns> <seealso cref="IntValue"/> </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual IntValue NextIntValue( int bound )
		 {
			  return intValue( _generator.Next( bound ) );
		 }

		 /// <summary>
		 /// Returns the next {@code int} between 0 (inclusive) and the specified value (exclusive)
		 /// </summary>
		 /// <param name="bound"> the upper bound (exclusive).  Must be positive. </param>
		 /// <returns> {@code int} </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual int NextInt( int bound )
		 {
			  return _generator.Next( bound );
		 }

		 /// <summary>
		 /// Returns an {@code int} between the given lower bound (inclusive) and the upper bound (inclusive)
		 /// </summary>
		 /// <param name="min"> minimum value that can be chosen (inclusive) </param>
		 /// <param name="max"> maximum value that can be chosen (inclusive) </param>
		 /// <returns> an {@code int} in the given inclusive range. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual int IntBetween( int min, int max )
		 {
			  return min + _generator.Next( max - min + 1 );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual long NextLong()
		 {
			  return _generator.nextLong();
		 }

		 /// <summary>
		 /// Returns the next {@code long} between 0 (inclusive) and the specified value (exclusive)
		 /// </summary>
		 /// <param name="bound"> the upper bound (exclusive).  Must be positive. </param>
		 /// <returns> {@code long} </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual long NextLong( long bound )
		 {
			  return abs( _generator.nextLong() ) % bound;
		 }

		 /// <summary>
		 /// Returns a {@code long} between the given lower bound (inclusive) and the upper bound (inclusive)
		 /// </summary>
		 /// <param name="min"> minimum value that can be chosen (inclusive) </param>
		 /// <param name="max"> maximum value that can be chosen (inclusive) </param>
		 /// <returns> a {@code long} in the given inclusive range. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 private long LongBetween( long min, long max )
		 {
			  return NextLong( ( max - min ) + 1L ) + min;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual LongValue NextLongValue()
		 {
			  return longValue( _generator.nextLong() );
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="LongValue"/> between 0 (inclusive) and the specified value (exclusive)
		 /// </summary>
		 /// <param name="bound"> the upper bound (exclusive).  Must be positive. </param>
		 /// <returns> <seealso cref="LongValue"/> </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual LongValue NextLongValue( long bound )
		 {
			  return longValue( NextLong( bound ) );
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="LongValue"/> between the specified lower bound (inclusive) and the specified upper bound (inclusive)
		 /// </summary>
		 /// <param name="lower"> the lower bound (inclusive). </param>
		 /// <param name="upper"> the upper bound (inclusive). </param>
		 /// <returns> <seealso cref="LongValue"/> </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual LongValue NextLongValue( long lower, long upper )
		 {
			  return longValue( NextLong( ( upper - lower ) + 1L ) + lower );
		 }

		 /// <summary>
		 /// Returns the next <seealso cref="FloatValue"/> between 0 (inclusive) and 1.0 (exclusive)
		 /// </summary>
		 /// <returns> <seealso cref="FloatValue"/> </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual FloatValue NextFloatValue()
		 {
			  return floatValue( _generator.nextFloat() );
		 }

		 /// <summary>
		 /// Returns the next {@code float} between 0 (inclusive) and 1.0 (exclusive)
		 /// </summary>
		 /// <returns> {@code float} </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual float NextFloat()
		 {
			  return _generator.nextFloat();
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual DoubleValue NextDoubleValue()
		 {
			  return doubleValue( NextDouble() );
		 }

		 /// <summary>
		 /// Returns the next {@code double} between 0 (inclusive) and 1.0 (exclusive)
		 /// </summary>
		 /// <returns> {@code float} </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual double NextDouble()
		 {
			  return _generator.NextDouble();
		 }

		 private double DoubleBetween( double min, double max )
		 {
			  return NextDouble() * (max - min) + min;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual NumberValue NextNumberValue()
		 {
			  int type = _generator.Next( 6 );
			  switch ( type )
			  {
			  case 0:
					return NextByteValue();
			  case 1:
					return NextShortValue();
			  case 2:
					return NextIntValue();
			  case 3:
					return NextLongValue();
			  case 4:
					return NextFloatValue();
			  case 5:
					return NextDoubleValue();
			  default:
					throw new System.ArgumentException( "Unknown value type " + type );
			  }
		 }

		 public virtual CharValue NextCharValue()
		 {
			  return Values.CharValue( NextCharRaw() );
		 }

		 public virtual char NextCharRaw()
		 {
			  int codePoint = BmpCodePoint();
			  assert( codePoint & ~0xFFFF ) == 0;
			  return ( char ) codePoint;
		 }

		 /// <returns> a <seealso cref="TextValue"/> consisting only of ascii alphabetic and numerical characters. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextValue NextAlphaNumericTextValue()
		 {
			  return NextAlphaNumericTextValue( MinString(), MaxString() );
		 }

		 /// <param name="minLength"> the minimum length of the string </param>
		 /// <param name="maxLength"> the maximum length of the string </param>
		 /// <returns> a <seealso cref="TextValue"/> consisting only of ascii alphabetic and numerical characters. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextValue NextAlphaNumericTextValue( int minLength, int maxLength )
		 {
			  return NextTextValue( minLength, maxLength, this.alphaNumericCodePoint );
		 }

		 /// <returns> a <seealso cref="TextValue"/> consisting only of ascii characters. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextValue NextAsciiTextValue()
		 {
			  return NextAsciiTextValue( MinString(), MaxString() );
		 }

		 /// <param name="minLength"> the minimum length of the string </param>
		 /// <param name="maxLength"> the maximum length of the string </param>
		 /// <returns> a <seealso cref="TextValue"/> consisting only of ascii characters. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextValue NextAsciiTextValue( int minLength, int maxLength )
		 {
			  return NextTextValue( minLength, maxLength, this.asciiCodePoint );
		 }

		 /// <returns> a <seealso cref="TextValue"/> consisting only of characters in the Basic Multilingual Plane(BMP). </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextValue NextBasicMultilingualPlaneTextValue()
		 {
			  return NextTextValue( MinString(), MaxString(), this.bmpCodePoint );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextValue NextTextValue()
		 {
			  return NextTextValue( MinString(), MaxString() );
		 }

		 /// <param name="minLength"> the minimum length of the string </param>
		 /// <param name="maxLength"> the maximum length of the string </param>
		 /// <returns> <seealso cref="TextValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextValue NextTextValue( int minLength, int maxLength )
		 {
			  return NextTextValue( minLength, maxLength, this.nextValidCodePoint );
		 }

		 private TextValue NextTextValue( int minLength, int maxLength, CodePointFactory codePointFactory )
		 {
			  // todo should we generate UTF8StringValue or StringValue? Or maybe both? Randomly?
			  int length = IntBetween( minLength, maxLength );
			  UTF8StringValueBuilder builder = new UTF8StringValueBuilder( NextPowerOf2( length ) );

			  for ( int i = 0; i < length; i++ )
			  {
					builder.AddCodePoint( codePointFactory() );
			  }
			  return builder.Build();
		 }

		 /// <summary>
		 /// Generate next code point that is valid for composition of a string.
		 /// Additional limitation on code point range is given by configuration.
		 /// </summary>
		 /// <returns> A pseudorandom valid code point </returns>
		 private int NextValidCodePoint()
		 {
			  return NextValidCodePoint( _configuration.maxCodePoint() );
		 }

		 /// <summary>
		 /// Generate next code point that is valid for composition of a string.
		 /// Additional limitation on code point range is given by method argument.
		 /// </summary>
		 /// <param name="maxCodePoint"> the maximum code point to consider </param>
		 /// <returns> A pseudorandom valid code point </returns>
		 private int NextValidCodePoint( int maxCodePoint )
		 {
			  int codePoint;
			  int type;
			  do
			  {
					codePoint = IntBetween( Character.MIN_CODE_POINT, maxCodePoint );
					type = char.GetUnicodeCategory( codePoint );
			  } while ( type == UnicodeCategory.OtherNotAssigned || type == UnicodeCategory.PrivateUse || type == UnicodeCategory.Surrogate );
			  return codePoint;
		 }

		 /// <returns> next code point limited to the ascii characters. </returns>
		 private int AsciiCodePoint()
		 {
			  return NextValidCodePoint( MAX_ASCII_CODE_POINT );
		 }

		 /// <returns> next code point limited to the alpha numeric characters. </returns>
		 private int AlphaNumericCodePoint()
		 {
			  int nextInt = _generator.Next( 4 );
			  if ( nextInt == 0 )
			  {
					return IntBetween( 'A', 'Z' );
			  }
			  else if ( nextInt == 1 )
			  {
					return IntBetween( 'a', 'z' );
			  }
			  else
			  {
					//We want digits being roughly as frequent as letters
					return IntBetween( '0', '9' );
			  }
		 }

		 /// <returns> next code point limited to the Basic Multilingual Plane (BMP). </returns>
		 private int BmpCodePoint()
		 {
			  return NextValidCodePoint( MAX_BMP_CODE_POINT );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual TimeValue NextTimeValue()
		 {
			  return time( NextTimeRaw() );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual LocalDateTimeValue NextLocalDateTimeValue()
		 {
			  return localDateTime( NextLocalDateTimeRaw() );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual DateValue NextDateValue()
		 {
			  return date( NextDateRaw() );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual LocalTimeValue NextLocalTimeValue()
		 {
			  return localTime( NextLocalTimeRaw() );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual DateTimeValue NextDateTimeValue()
		 {
			  return NextDateTimeValue( UTC );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual DateTimeValue NextDateTimeValue( ZoneId zoneId )
		 {
			  return datetime( NextZonedDateTimeRaw( zoneId ) );
		 }

		 /// <returns> next <seealso cref="DurationValue"/> based on java <seealso cref="Period"/> (years, months and days). </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual DurationValue NextPeriod()
		 {
			  return duration( NextPeriodRaw() );
		 }

		 /// <returns> next <seealso cref="DurationValue"/> based on java <seealso cref="Duration"/> (seconds, nanos). </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual DurationValue NextDuration()
		 {
			  return duration( NextDurationRaw() );
		 }

		 /// <summary>
		 /// Returns a randomly selected temporal value spread uniformly over the supported types.
		 /// </summary>
		 /// <returns> a randomly selected temporal value </returns>
		 public virtual Value NextTemporalValue()
		 {
			  int nextInt = _generator.Next( 6 );
			  switch ( nextInt )
			  {
			  case 0:
					return NextDateValue();

			  case 1:
					return NextLocalDateTimeValue();

			  case 2:
					return NextDateTimeValue();

			  case 3:
					return NextLocalTimeValue();

			  case 4:
					return NextTimeValue();

			  case 5:
					return NextDuration();

			  default:
					throw new System.ArgumentException( nextInt + " not a valid temporal type" );
			  }
		 }

		 /// <returns> the next pseudorandom two-dimensional cartesian <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointValue NextCartesianPoint()
		 {
			  double x = RandomCartesianCoordinate();
			  double y = RandomCartesianCoordinate();
			  return Values.PointValue( CoordinateReferenceSystem.Cartesian, x, y );
		 }

		 /// <returns> the next pseudorandom three-dimensional cartesian <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointValue NextCartesian3DPoint()
		 {
			  double x = RandomCartesianCoordinate();
			  double y = RandomCartesianCoordinate();
			  double z = RandomCartesianCoordinate();
			  return Values.PointValue( CoordinateReferenceSystem.Cartesian_3D, x, y, z );
		 }

		 /// <returns> the next pseudorandom two-dimensional geographic <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointValue NextGeographicPoint()
		 {
			  double longitude = RandomLongitude();
			  double latitude = RandomLatitude();
			  return Values.PointValue( CoordinateReferenceSystem.Wgs84, longitude, latitude );
		 }

		 /// <returns> the next pseudorandom three-dimensional geographic <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointValue NextGeographic3DPoint()
		 {
			  double longitude = RandomLongitude();
			  double latitude = RandomLatitude();
			  double z = RandomCartesianCoordinate();
			  return Values.PointValue( CoordinateReferenceSystem.Wgs84_3d, longitude, latitude, z );
		 }

		 private double RandomLatitude()
		 {
			  double spatialDefaultMinLatitude = -90;
			  double spatialDefaultMaxLatitude = 90;
			  return DoubleBetween( spatialDefaultMinLatitude, spatialDefaultMaxLatitude );
		 }

		 private double RandomLongitude()
		 {
			  double spatialDefaultMinLongitude = -180;
			  double spatialDefaultMaxLongitude = 180;
			  return DoubleBetween( spatialDefaultMinLongitude, spatialDefaultMaxLongitude );
		 }

		 private double RandomCartesianCoordinate()
		 {
			  double spatialDefaultMinExtent = -1000000;
			  double spatialDefaultMaxExtent = 1000000;
			  return DoubleBetween( spatialDefaultMinExtent, spatialDefaultMaxExtent );
		 }

		 /// <summary>
		 /// Returns a randomly selected point value spread uniformly over the supported types of points.
		 /// </summary>
		 /// <returns> a randomly selected point value </returns>
		 public virtual PointValue NextPointValue()
		 {
			  int nextInt = _generator.Next( 4 );
			  switch ( nextInt )
			  {
			  case 0:
					return NextCartesianPoint();

			  case 1:
					return NextCartesian3DPoint();

			  case 2:
					return NextGeographicPoint();

			  case 3:
					return NextGeographic3DPoint();

			  default:
					throw new System.InvalidOperationException( nextInt + " not a valid point type" );
			  }
		 }

		 public virtual CharArray NextCharArray()
		 {
			  return Values.CharArray( NextCharArrayRaw( MinArray(), MaxArray() ) );
		 }

		 private char[] NextCharArrayRaw( int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  char[] array = new char[length];
			  for ( int i = 0; i < length; i++ )
			  {
					array[i] = NextCharRaw();
			  }
			  return array;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual DoubleArray NextDoubleArray()
		 {
			  double[] array = NextDoubleArrayRaw( MinArray(), MaxArray() );
			  return Values.DoubleArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual double[] NextDoubleArrayRaw( int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  double[] doubles = new double[length];
			  for ( int i = 0; i < length; i++ )
			  {
					doubles[i] = NextDouble();
			  }
			  return doubles;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual FloatArray NextFloatArray()
		 {
			  float[] array = NextFloatArrayRaw( MinArray(), MaxArray() );
			  return Values.FloatArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual float[] NextFloatArrayRaw( int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  float[] floats = new float[length];
			  for ( int i = 0; i < length; i++ )
			  {
					floats[i] = _generator.nextFloat();
			  }
			  return floats;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual LongArray NextLongArray()
		 {
			  long[] array = NextLongArrayRaw( MinArray(), MaxArray() );
			  return Values.LongArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual long[] NextLongArrayRaw( int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  long[] longs = new long[length];
			  for ( int i = 0; i < length; i++ )
			  {
					longs[i] = _generator.nextLong();
			  }
			  return longs;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual IntArray NextIntArray()
		 {
			  int[] array = NextIntArrayRaw( MinArray(), MaxArray() );
			  return Values.IntArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual int[] NextIntArrayRaw( int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  int[] ints = new int[length];
			  for ( int i = 0; i < length; i++ )
			  {
					ints[i] = _generator.Next();
			  }
			  return ints;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual ByteArray NextByteArray()
		 {
			  return NextByteArray( MinArray(), MaxArray() );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual ByteArray NextByteArray( int minLength, int maxLength )
		 {
			  sbyte[] array = NextByteArrayRaw( minLength, maxLength );
			  return Values.ByteArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual sbyte[] NextByteArrayRaw( int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  sbyte[] bytes = new sbyte[length];
			  int index = 0;
			  while ( index < length )
			  {
					//For each random int we get up to four random bytes
					int rand = NextInt();
					int numBytesToShift = Math.Min( length - index, Integer.BYTES );

					//byte 4   byte 3   byte 2   byte 1
					//aaaaaaaa bbbbbbbb cccccccc dddddddd
					while ( numBytesToShift > 0 )
					{
						 bytes[index++] = ( sbyte ) rand;
						 numBytesToShift--;
						 rand >>= ( sizeof( sbyte ) * 8 );
					}
			  }
			  return bytes;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual ShortArray NextShortArray()
		 {
			  short[] array = NextShortArrayRaw( MinArray(), MaxArray() );
			  return Values.ShortArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual short[] NextShortArrayRaw( int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  short[] shorts = new short[length];
			  for ( int i = 0; i < length; i++ )
			  {
					shorts[i] = ( short ) _generator.Next();
			  }
			  return shorts;
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual BooleanArray NextBooleanArray()
		 {
			  bool[] array = NextBooleanArrayRaw( MinArray(), MaxArray() );
			  return Values.BooleanArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual bool[] NextBooleanArrayRaw( int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  bool[] booleans = new bool[length];
			  for ( int i = 0; i < length; i++ )
			  {
					booleans[i] = _generator.nextBoolean();
			  }
			  return booleans;
		 }

		 /// <returns> the next <seealso cref="TextArray"/> containing strings with only alpha-numeric characters. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextArray NextAlphaNumericTextArray()
		 {
			  string[] array = NextAlphaNumericStringArrayRaw( MinArray(), MaxArray(), MinString(), MaxString() );
			  return Values.StringArray( array );
		 }

		 /// <returns> the next {@code String[]} containing strings with only alpha-numeric characters. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual string[] NextAlphaNumericStringArrayRaw( int minLength, int maxLength, int minStringLength, int maxStringLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( string[]::new, () => NextStringRaw(minStringLength, maxStringLength, this.alphaNumericCodePoint), minLength, maxLength );
		 }

		 /// <returns> the next <seealso cref="TextArray"/> containing strings with only ascii characters. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 private TextArray NextAsciiTextArray()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] array = NextArray( string[]::new, () => NextStringRaw(this.asciiCodePoint), MinArray(), MaxArray() );
			  return Values.StringArray( array );
		 }

		 /// <returns> the next <seealso cref="TextArray"/> containing strings with only characters in the Basic Multilingual Plane (BMP). </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextArray NextBasicMultilingualPlaneTextArray()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] array = NextArray( string[]::new, () => NextStringRaw(MinString(), MaxString(), this.bmpCodePoint), MinArray(), MaxArray() );
			  return Values.StringArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual TextArray NextTextArray()
		 {
			  string[] array = NextStringArrayRaw( MinArray(), MaxArray(), MinString(), MaxString() );
			  return Values.StringArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual string[] NextStringArrayRaw( int minLength, int maxLength, int minStringLength, int maxStringLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( string[]::new, () => NextStringRaw(minStringLength, maxStringLength, this.nextValidCodePoint), minLength, maxLength );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual LocalTimeArray NextLocalTimeArray()
		 {
			  LocalTime[] array = NextLocalTimeArrayRaw( MinArray(), MaxArray() );
			  return Values.LocalTimeArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual LocalTime[] NextLocalTimeArrayRaw( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( LocalTime[]::new, this.nextLocalTimeRaw, minLength, maxLength );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual TimeArray NextTimeArray()
		 {
			  OffsetTime[] array = NextTimeArrayRaw( MinArray(), MaxArray() );
			  return Values.TimeArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual OffsetTime[] NextTimeArrayRaw( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( OffsetTime[]::new, this.nextTimeRaw, minLength, maxLength );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual DateTimeArray NextDateTimeArray()
		 {
			  ZonedDateTime[] array = NextDateTimeArrayRaw( MinArray(), MaxArray() );
			  return Values.DateTimeArray( array );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual ZonedDateTime[] NextDateTimeArrayRaw( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( ZonedDateTime[]::new, () => NextZonedDateTimeRaw(UTC), minLength, maxLength );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual LocalDateTimeArray NextLocalDateTimeArray()
		 {
			  return Values.LocalDateTimeArray( NextLocalDateTimeArrayRaw( MinArray(), MaxArray() ) );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual DateTime[] NextLocalDateTimeArrayRaw( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( DateTime[]::new, this.nextLocalDateTimeRaw, minLength, maxLength );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual DateArray NextDateArray()
		 {
			  return Values.DateArray( NextDateArrayRaw( MinArray(), MaxArray() ) );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual LocalDate[] NextDateArrayRaw( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( LocalDate[]::new, this.nextDateRaw, minLength, maxLength );
		 }

		 /// <returns> next <seealso cref="DurationArray"/> based on java <seealso cref="Period"/> (years, months and days). </returns>
		 /// <seealso cref= RandomValues </seealso>
		 private DurationArray NextPeriodArray()
		 {
			  return Values.DurationArray( NextPeriodArrayRaw( MinArray(), MaxArray() ) );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual Period[] NextPeriodArrayRaw( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( Period[]::new, this.nextPeriodRaw, minLength, maxLength );
		 }

		 /// <returns> next <seealso cref="DurationValue"/> based on java <seealso cref="Duration"/> (seconds, nanos). </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual DurationArray NextDurationArray()
		 {
			  return Values.DurationArray( NextDurationArrayRaw( MinArray(), MaxArray() ) );
		 }

		 /// <seealso cref= RandomValues </seealso>
		 public virtual Duration[] NextDurationArrayRaw( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NextArray( Duration[]::new, this.nextDurationRaw, minLength, maxLength );
		 }

		 /// <returns> the next random <seealso cref="PointArray"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextPointArray()
		 {
			  int nextInt = _generator.Next( 4 );
			  switch ( nextInt )
			  {
			  case 0:
					return NextCartesianPointArray();

			  case 1:
					return NextCartesian3DPointArray();

			  case 2:
					return NextGeographicPointArray();

			  case 3:
					return NextGeographic3DPointArray();

			  default:
					throw new System.InvalidOperationException( nextInt + " not a valid point type" );
			  }
		 }

		 /// <returns> the next <seealso cref="PointArray"/> containing two-dimensional cartesian <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextCartesianPointArray()
		 {
			  return NextCartesianPointArray( MinArray(), MaxArray() );
		 }

		 /// <returns> the next <seealso cref="PointArray"/> containing two-dimensional cartesian <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextCartesianPointArray( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  PointValue[] array = NextArray( PointValue[]::new, this.nextCartesianPoint, minLength, maxLength );
			  return Values.PointArray( array );
		 }

		 /// <returns> the next <seealso cref="PointArray"/> containing three-dimensional cartesian <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextCartesian3DPointArray()
		 {
			  return NextCartesian3DPointArray( MinArray(), MaxArray() );
		 }

		 /// <returns> the next <seealso cref="PointArray"/> containing three-dimensional cartesian <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextCartesian3DPointArray( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  PointValue[] array = NextArray( PointValue[]::new, this.nextCartesian3DPoint, minLength, maxLength );
			  return Values.PointArray( array );
		 }

		 /// <returns> the next <seealso cref="PointArray"/> containing two-dimensional geographic <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextGeographicPointArray()
		 {
			  return NextGeographicPointArray( MinArray(), MaxArray() );
		 }

		 /// <returns> the next <seealso cref="PointArray"/> containing two-dimensional geographic <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextGeographicPointArray( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  PointValue[] array = NextArray( PointValue[]::new, this.nextGeographicPoint, minLength, maxLength );
			  return Values.PointArray( array );
		 }

		 /// <returns> the next <seealso cref="PointArray"/> containing three-dimensional geographic <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextGeographic3DPointArray()
		 {
			  return NextGeographic3DPointArray( MinArray(), MaxArray() );
		 }

		 /// <returns> the next <seealso cref="PointArray"/> containing three-dimensional geographic <seealso cref="PointValue"/>. </returns>
		 /// <seealso cref= RandomValues </seealso>
		 public virtual PointArray NextGeographic3DPointArray( int minLength, int maxLength )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  PointValue[] points = NextArray( PointValue[]::new, this.nextGeographic3DPoint, minLength, maxLength );
			  return Values.PointArray( points );
		 }

		 /// <summary>
		 /// Create an randomly sized array filled with elements provided by factory.
		 /// </summary>
		 /// <param name="arrayFactory"> creates array with length equal to provided argument. </param>
		 /// <param name="elementFactory"> generating random values of some type. </param>
		 /// <param name="minLength"> minimum length of array (inclusive). </param>
		 /// <param name="maxLength"> maximum length of array (inclusive). </param>
		 /// @param <T> Generic type of elements in array. </param>
		 /// <returns> a new array created by arrayFactory, filled with elements created by elementFactory. </returns>
		 private T[] NextArray<T>( System.Func<int, T[]> arrayFactory, ElementFactory<T> elementFactory, int minLength, int maxLength )
		 {
			  int length = IntBetween( minLength, maxLength );
			  T[] array = arrayFactory( length );
			  for ( int i = 0; i < length; i++ )
			  {
					array[i] = elementFactory();
			  }
			  return array;
		 }

		 /* Single raw element */

		 private string NextStringRaw( CodePointFactory codePointFactory )
		 {
			  return NextStringRaw( MinString(), MaxString(), codePointFactory );
		 }

		 private string NextStringRaw( int minStringLength, int maxStringLength, CodePointFactory codePointFactory )
		 {
			  int length = IntBetween( minStringLength, maxStringLength );
			  StringBuilder sb = new StringBuilder( length );
			  for ( int i = 0; i < length; i++ )
			  {
					sb.appendCodePoint( codePointFactory() );
			  }
			  return sb.ToString();
		 }

		 private LocalTime NextLocalTimeRaw()
		 {
			  return ofNanoOfDay( LongBetween( LocalTime.MIN.toNanoOfDay(), LocalTime.MAX.toNanoOfDay() ) );
		 }

		 private DateTime NextLocalDateTimeRaw()
		 {
			  return DateTime.ofInstant( NextInstantRaw(), UTC );
		 }

		 private OffsetTime NextTimeRaw()
		 {
			  return OffsetTime.ofInstant( NextInstantRaw(), UTC );
		 }

		 private ZonedDateTime NextZonedDateTimeRaw( ZoneId utc )
		 {
			  return ZonedDateTime.ofInstant( NextInstantRaw(), utc );
		 }

		 private LocalDate NextDateRaw()
		 {
			  return ofEpochDay( LongBetween( LocalDate.MIN.toEpochDay(), LocalDate.MAX.toEpochDay() ) );
		 }

		 private Instant NextInstantRaw()
		 {
			  return Instant.ofEpochSecond( LongBetween( DateTime.MinValue.toEpochSecond( UTC ), DateTime.MaxValue.toEpochSecond( UTC ) ), NextLong( NANOS_PER_SECOND ) );
		 }

		 private Period NextPeriodRaw()
		 {
			  return Period.of( _generator.Next(), _generator.Next(12), _generator.Next(28) );
		 }

		 private Duration NextDurationRaw()
		 {
			  return Duration.ofSeconds( NextLong( DAYS.Duration.Seconds ), NextLong( NANOS_PER_SECOND ) );
		 }

		 /// <summary>
		 /// Returns a random element from the provided array.
		 /// </summary>
		 /// <param name="among"> the array to choose a random element from. </param>
		 /// <returns> a random element of the provided array. </returns>
		 public virtual T Among<T>( T[] among )
		 {
			  return among[_generator.Next( among.Length )];
		 }

		 /// <summary>
		 /// Returns a random element of the provided list
		 /// </summary>
		 /// <param name="among"> the list to choose a random element from </param>
		 /// <returns> a random element of the provided list </returns>
		 public virtual T Among<T>( IList<T> among )
		 {
			  return among[_generator.Next( among.Count )];
		 }

		 /// <summary>
		 /// Picks a random element of the provided list and feeds it to the provided <seealso cref="Consumer"/>
		 /// </summary>
		 /// <param name="among"> the list to pick from </param>
		 /// <param name="action"> the consumer to feed values to </param>
		 public virtual void Among<T>( IList<T> among, System.Action<T> action )
		 {
			  if ( among.Count > 0 )
			  {
					T item = among( among );
					action( item );
			  }
		 }

		 /// <summary>
		 /// Returns a random selection of the provided array.
		 /// </summary>
		 /// <param name="among"> the array to pick elements from </param>
		 /// <param name="min"> the minimum number of elements to choose </param>
		 /// <param name="max"> the maximum number of elements to choose </param>
		 /// <param name="allowDuplicates"> if {@code true} the same element can be chosen multiple times </param>
		 /// <returns> a random selection of the provided array. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public <T> T[] selection(T[] among, int min, int max, boolean allowDuplicates)
		 public virtual T[] Selection<T>( T[] among, int min, int max, bool allowDuplicates )
		 {
			  Debug.Assert( min <= max );
			  int diff = min == max ? 0 : _generator.Next( max - min );
			  int length = min + diff;
			  T[] result = ( T[] ) Array.CreateInstance( among.GetType().GetElementType(), length );
			  for ( int i = 0; i < length; i++ )
			  {
					while ( true )
					{
						 T candidate = among( among );
						 if ( !allowDuplicates && ArrayUtils.contains( result, candidate ) )
						 { // Try again
							  continue;
						 }
						 result[i] = candidate;
						 break;
					}
			  }
			  return result;
		 }

		 private static int NextPowerOf2( int i )
		 {
			  return 1 << ( 32 - Integer.numberOfLeadingZeros( i ) );
		 }

		 private int MaxArray()
		 {
			  return _configuration.arrayMaxLength();
		 }

		 private int MinArray()
		 {
			  return _configuration.arrayMinLength();
		 }

		 private int MaxString()
		 {
			  return _configuration.stringMaxLength();
		 }

		 private int MinString()
		 {
			  return _configuration.stringMinLength();
		 }

		 private delegate T ElementFactory<T>();

		 private delegate int CodePointFactory();
	}

}