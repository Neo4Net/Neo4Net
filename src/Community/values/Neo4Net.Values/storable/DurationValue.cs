using System;
using System.Collections.Generic;
using System.Text;

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

	using IHashFunction = Neo4Net.Hashing.HashFunction;
	using Neo4Net.Values;
	using Neo4Net.Values;
	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using TemporalArithmeticException = Neo4Net.Values.utils.TemporalArithmeticException;
	using UnsupportedTemporalUnitException = Neo4Net.Values.utils.UnsupportedTemporalUnitException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static double.Parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.NumberType.NO_NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.NumberValue.safeCastFloatingPoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.TemporalUtil.AVG_NANOS_PER_MONTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.TemporalUtil.AVG_SECONDS_PER_MONTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.TemporalUtil.NANOS_PER_SECOND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.TemporalUtil.SECONDS_PER_DAY;

	/// <summary>
	/// We use our own implementation because neither <seealso cref="java.time.Duration"/> nor <seealso cref="java.time.Period"/> fits our needs.
	/// <seealso cref="java.time.Duration"/> only works with seconds, assumes 24H days, and is unable to handle larger units than days.
	/// <seealso cref="java.time.Period"/> only works with units from days or larger, and does not deal with time.
	/// </summary>
	public sealed class DurationValue : ScalarValue, TemporalAmount, IComparable<DurationValue>
	{
		 public static readonly DurationValue MinValue = Duration( 0, 0, long.MinValue, 0 );
		 public static readonly DurationValue MaxValue = Duration( 0, 0, long.MaxValue, 999_999_999 );

		 public static DurationValue Duration( Duration value )
		 {
			  requireNonNull( value, "Duration" );
			  return NewDuration( 0, 0, value.Seconds, value.Nano );
		 }

		 public static DurationValue Duration( Period value )
		 {
			  requireNonNull( value, "Period" );
			  return NewDuration( value.toTotalMonths(), value.Days, 0, 0 );
		 }

		 public static DurationValue Duration( long months, long days, long seconds, long nanos )
		 {
			  return NewDuration( months, days, seconds, nanos );
		 }

		 public static DurationValue Parse( CharSequence text )
		 {
			  return TemporalValue.Parse( typeof( DurationValue ), _pattern, DurationValue.parse, text );
		 }

		 public static DurationValue Parse( TextValue text )
		 {
			  return TemporalValue.Parse( typeof( DurationValue ), _pattern, DurationValue.parse, text );
		 }

		 internal static DurationValue Build<T1>( IDictionary<T1> input ) where T1 : Neo4Net.Values.AnyValue
		 {
			  StructureBuilder<AnyValue, DurationValue> builder = builder();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<String,? extends Neo4Net.values.AnyValue> entry : input.entrySet())
			  foreach ( KeyValuePair<string, ? extends AnyValue> entry in input.SetOfKeyValuePairs() )
			  {
					builder.Add( entry.Key, entry.Value );
			  }
			  return builder.Build();
		 }

		 public static DurationValue Build( MapValue map )
		 {
			  return StructureBuilder.build( Builder(), map );
		 }

		 public static DurationValue Between( TemporalUnit unit, Temporal from, Temporal to )
		 {
			  if ( unit == null )
			  {
					return DurationBetween( from, to );
			  }
			  else if ( unit is ChronoUnit )
			  {
					switch ( ( ChronoUnit ) unit )
					{
					case MONTHS:
						 return NewDuration( AssertValidUntil( from, to,unit ), 0, 0, 0 );
					case DAYS:
						 return NewDuration( 0, AssertValidUntil( from, to,unit ), 0, 0 );
					case SECONDS:
						 return DurationInSecondsAndNanos( from, to );
					default:
						 throw new UnsupportedTemporalUnitException( "Unsupported unit: " + unit );
					}
			  }
			  else
			  {
					throw new UnsupportedTemporalUnitException( "Unsupported unit: " + unit );
			  }
		 }

		 internal static StructureBuilder<AnyValue, DurationValue> Builder()
		 {
			  return new DurationBuilderAnonymousInnerClass();
		 }

		 private class DurationBuilderAnonymousInnerClass : DurationBuilder<AnyValue, DurationValue>
		 {
			 internal override DurationValue create( AnyValue years, AnyValue months, AnyValue weeks, AnyValue days, AnyValue hours, AnyValue minutes, AnyValue seconds, AnyValue milliseconds, AnyValue microseconds, AnyValue nanoseconds )
			 {
				  return Approximate( safeCastFloatingPoint( "years", years, 0 ) * 12 + safeCastFloatingPoint( "months", months, 0 ), safeCastFloatingPoint( "weeks", weeks, 0 ) * 7 + safeCastFloatingPoint( "days", days, 0 ), safeCastFloatingPoint( "hours", hours, 0 ) * 3600 + safeCastFloatingPoint( "minutes", minutes, 0 ) * 60 + safeCastFloatingPoint( "seconds", seconds, 0 ), safeCastFloatingPoint( "milliseconds", milliseconds, 0 ) * 1_000_000 + safeCastFloatingPoint( "microseconds", microseconds, 0 ) * 1_000 + safeCastFloatingPoint( "nanoseconds", nanoseconds, 0 ) );
			 }
		 }

		 public abstract class Compiler<Input> : DurationBuilder<Input, MethodHandle>
		 {
		 }

		 public static readonly DurationValue Zero = new DurationValue( 0, 0, 0, 0 );
		 private static readonly IList<TemporalUnit> _units = unmodifiableList( asList( MONTHS, DAYS, SECONDS, NANOS ) );
		 // This comparator is safe until 292,271,023,045 years. After that, we have an overflow.
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly IComparer<DurationValue> _comparator = System.Collections.IComparer.comparingLong( DurationValue::getAverageLengthInSeconds ).thenComparingLong( d => d.nanos ).thenComparingLong( d => d.months ).thenComparingLong( d => d.days ).thenComparingLong( d => d.seconds );
		 private readonly long _months;
		 private readonly long _days;
		 private readonly long _seconds;
		 private readonly int _nanos;

		 private static DurationValue NewDuration( long months, long days, long seconds, long nanos )
		 {
			  return seconds == 0 && days == 0 && months == 0 && nanos == 0 ? Zero : new DurationValue( months, days, seconds, nanos );
		 }

		 private DurationValue( long months, long days, long seconds, long nanos )
		 {
			  AssertNoOverflow( months, days, seconds, nanos );
			  seconds = SecondsWithNanos( seconds, nanos );
			  nanos %= NANOS_PER_SECOND;
			  // normalize nanos to be between 0 and NANOS_PER_SECOND-1
			  if ( nanos < 0 )
			  {
					seconds -= 1;
					nanos += NANOS_PER_SECOND;
			  }
			  this._months = months;
			  this._days = days;
			  this._seconds = seconds;
			  this._nanos = ( int ) nanos;
		 }

		 public override int CompareTo( DurationValue other )
		 {
			  return _comparator.Compare( this, other );
		 }

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  return CompareTo( ( DurationValue ) otherValue );
		 }

		 private long AverageLengthInSeconds
		 {
			 get
			 {
				  return CalcAverageLengthInSeconds( this._months, this._days, this._seconds );
			 }
		 }

		 private long CalcAverageLengthInSeconds( long months, long days, long seconds )
		 {
			  long daysInSeconds = Math.multiplyExact( days, SECONDS_PER_DAY );
			  long monthsInSeconds = Math.multiplyExact( months, AVG_SECONDS_PER_MONTH );
			  return Math.addExact( seconds, Math.addExact( daysInSeconds, monthsInSeconds ) );
		 }

		 private long SecondsWithNanos( long seconds, long nanos )
		 {
			  return Math.addExact( seconds, nanos / NANOS_PER_SECOND );
		 }

		 private void AssertNoOverflow( long months, long days, long seconds, long nanos )
		 {
			  try
			  {
					CalcAverageLengthInSeconds( months, days, seconds );
					SecondsWithNanos( seconds, nanos );
			  }
			  catch ( ArithmeticException e )
			  {
					throw InvalidDuration( months, days, seconds, nanos, e );
			  }
		 }

		 internal long NanosOfDay()
		 {
			  return ( _seconds % SECONDS_PER_DAY ) * NANOS_PER_SECOND + _nanos;
		 }

		 internal long TotalMonths()
		 {
			  return _months;
		 }

		 /// <summary>
		 /// The number of days of this duration, as computed by the days and the whole days made up of seconds. This
		 /// excludes the days contributed by the months.
		 /// </summary>
		 /// <returns> the total number of days of this duration. </returns>
		 internal long TotalDays()
		 {
			  return _days + ( _seconds / SECONDS_PER_DAY );
		 }

		 private const string UNIT_BASED_PATTERN = "(?:(?<years>[-+]?[0-9]+(?:[.,][0-9]+)?)Y)?"
					+ "(?:(?<months>[-+]?[0-9]+(?:[.,][0-9]+)?)M)?"
					+ "(?:(?<weeks>[-+]?[0-9]+(?:[.,][0-9]+)?)W)?"
					+ "(?:(?<days>[-+]?[0-9]+(?:[.,][0-9]+)?)D)?"
					+ "(?<T>T"
					+ "(?:(?<hours>[-+]?[0-9]+(?:[.,][0-9]+)?)H)?"
					+ "(?:(?<minutes>[-+]?[0-9]+(?:[.,][0-9]+)?)M)?"
					+ "(?:(?<seconds>[-+]?[0-9]+)(?:[.,](?<subseconds>[0-9]{1,9}))?S)?)?";
		 private const string DATE_BASED_PATTERN = "(?:"
					+ "(?<year>[0-9]{4})(?:"
					+ "-(?<longMonth>[0-9]{2})-(?<longDay>[0-9]{2})|"
					+ "(?<shortMonth>[0-9]{2})(?<shortDay>[0-9]{2}))"
					+ ")?(?<time>T"
					+ "(?:(?<shortHour>[0-9]{2})(?:(?<shortMinute>[0-9]{2})"
					+ "(?:(?<shortSecond>[0-9]{2})(?:[.,](?<shortSub>[0-9]{1,9}))?)?)?|"
					+ "(?<longHour>[0-9]{2}):(?<longMinute>[0-9]{2})"
					+ "(?::(?<longSecond>[0-9]{2})(?:[.,](?<longSub>[0-9]{1,9}))?)?))?";
		 private static readonly Pattern _pattern = Pattern.compile( "(?<sign>[-+]?)P(?:" + UNIT_BASED_PATTERN + "|" + DATE_BASED_PATTERN + ")", CASE_INSENSITIVE );

		 private static DurationValue Parse( Matcher matcher )
		 {
			  string year = matcher.group( "year" );
			  string time = matcher.group( "time" );
			  if ( !string.ReferenceEquals( year, null ) || !string.ReferenceEquals( time, null ) )
			  {
					return ParseDateDuration( year, matcher, !string.ReferenceEquals( time, null ) );
			  }
			  else
			  {
					return ParseDuration( matcher );
			  }
		 }

		 private static DurationValue ParseDuration( Matcher matcher )
		 {
			  int sign = "-".Equals( matcher.group( "sign" ) ) ? -1 : 1;
			  string y = matcher.group( "years" );
			  string m = matcher.group( "months" );
			  string w = matcher.group( "weeks" );
			  string d = matcher.group( "days" );
			  string t = matcher.group( "T" );
			  if ( ( string.ReferenceEquals( y, null ) && string.ReferenceEquals( m, null ) && string.ReferenceEquals( w, null ) && string.ReferenceEquals( d, null ) && string.ReferenceEquals( t, null ) ) || "T".Equals( t, StringComparison.OrdinalIgnoreCase ) )
			  {
					return null;
			  }
			  int pos;
			  if ( ( pos = FractionPoint( y ) ) >= 0 )
			  {
					if ( !string.ReferenceEquals( m, null ) || !string.ReferenceEquals( w, null ) || !string.ReferenceEquals( d, null ) || !string.ReferenceEquals( t, null ) )
					{
						 return null;
					}
					return Approximate( ParseFractional( y, pos ) * 12, 0, 0, 0 );
			  }
			  long months = OptLong( y ) * 12;
			  if ( ( pos = FractionPoint( m ) ) >= 0 )
			  {
					if ( !string.ReferenceEquals( w, null ) || !string.ReferenceEquals( d, null ) || !string.ReferenceEquals( t, null ) )
					{
						 return null;
					}
					return Approximate( months + ParseFractional( m, pos ), 0, 0, 0 );
			  }
			  months += OptLong( m );
			  if ( ( pos = FractionPoint( w ) ) >= 0 )
			  {
					if ( !string.ReferenceEquals( d, null ) || !string.ReferenceEquals( t, null ) )
					{
						 return null;
					}
					return Approximate( months, ParseFractional( w, pos ) * 7, 0, 0 );
			  }
			  long days = OptLong( w ) * 7;
			  if ( ( pos = FractionPoint( d ) ) >= 0 )
			  {
					if ( !string.ReferenceEquals( t, null ) )
					{
						 return null;
					}
					return Approximate( months, days + ParseFractional( d, pos ), 0, 0 );
			  }
			  days += OptLong( d );
			  return ParseDuration( sign, months, days, matcher, false, "hours", "minutes", "seconds", "subseconds" );
		 }

		 private static DurationValue ParseDateDuration( string year, Matcher matcher, bool time )
		 {
			  int sign = "-".Equals( matcher.group( "sign" ) ) ? -1 : 1;
			  long months = 0;
			  long days = 0;
			  if ( !string.ReferenceEquals( year, null ) )
			  {
					string month = matcher.group( "longMonth" );
					string day;
					if ( string.ReferenceEquals( month, null ) )
					{
						 month = matcher.group( "shortMonth" );
						 day = matcher.group( "shortDay" );
					}
					else
					{
						 day = matcher.group( "longDay" );
					}
					months = parseLong( month );
					if ( months > 12 )
					{
						 throw new InvalidValuesArgumentException( "months is out of range: " + month );
					}
					months += parseLong( year ) * 12;
					days = parseLong( day );
					if ( days > 31 )
					{
						 throw new InvalidValuesArgumentException( "days is out of range: " + day );
					}
			  }
			  if ( time )
			  {
					if ( matcher.group( "longHour" ) != null )
					{
						 return ParseDuration( sign, months, days, matcher, true, "longHour", "longMinute", "longSecond", "longSub" );
					}
					else
					{
						 return ParseDuration( sign, months, days, matcher, true, "shortHour", "shortMinute", "shortSecond", "shortSub" );
					}
			  }
			  else
			  {
					return Duration( sign * months, sign * days, 0, 0 );
			  }
		 }

		 private static DurationValue ParseDuration( int sign, long months, long days, Matcher matcher, bool strict, string hour, string min, string sec, string sub )
		 {
			  string h = matcher.group( hour );
			  string m = matcher.group( min );
			  string s = matcher.group( sec );
			  string n = matcher.group( sub );
			  if ( !strict )
			  {
					int pos;
					if ( ( pos = FractionPoint( h ) ) >= 0 )
					{
						 if ( !string.ReferenceEquals( m, null ) || !string.ReferenceEquals( s, null ) )
						 {
							  return null;
						 }
						 return Approximate( months, days, ParseFractional( h, pos ) * 3600, 0 );
					}
					if ( ( pos = FractionPoint( m ) ) >= 0 )
					{
						 if ( !string.ReferenceEquals( s, null ) )
						 {
							  return null;
						 }
						 return Approximate( months, days, ParseFractional( m, pos ) * 60, 0 );
					}
			  }
			  long hours = OptLong( h );
			  long minutes = OptLong( m );
			  long seconds = OptLong( s );
			  if ( strict )
			  {
					if ( hours > 24 )
					{
						 throw new InvalidValuesArgumentException( "hours out of range: " + hours );
					}
					if ( minutes > 60 )
					{
						 throw new InvalidValuesArgumentException( "minutes out of range: " + minutes );
					}
					if ( seconds > 60 )
					{
						 throw new InvalidValuesArgumentException( "seconds out of range: " + seconds );
					}
			  }
			  seconds += hours * 3600 + minutes * 60;
			  long nanos = OptLong( n );
			  if ( nanos != 0 )
			  {
					for ( int i = n.Length; i < 9; i++ )
					{
						 nanos *= 10;
					}
					if ( s.StartsWith( "-", StringComparison.Ordinal ) )
					{
						 nanos = -nanos;
					}
			  }
			  return Duration( sign * months, sign * days, sign * seconds, sign * nanos );
		 }

		 private static double ParseFractional( string input, int pos )
		 {
			  return parseDouble( input[pos] == '.' ? input : ( input.Substring( 0, pos ) + "." + input.Substring( pos + 1 ) ) );
		 }

		 private static int FractionPoint( string field )
		 {
			  if ( string.ReferenceEquals( field, null ) )
			  {
					return -1;
			  }
			  int fractionPoint = field.IndexOf( '.' );
			  if ( fractionPoint < 0 )
			  {
					fractionPoint = field.IndexOf( ',' );
			  }
			  return fractionPoint;
		 }

		 private static long OptLong( string value )
		 {
			  return string.ReferenceEquals( value, null ) ? 0 : parseLong( value );
		 }

		 internal static DurationValue DurationBetween( Temporal from, Temporal to )
		 {
			  long months = 0;
			  long days = 0;
			  if ( from.isSupported( EPOCH_DAY ) && to.isSupported( EPOCH_DAY ) )
			  {
					months = AssertValidUntil( from, to, ChronoUnit.MONTHS );
					try
					{
						 from = from.plus( months, ChronoUnit.MONTHS );
					}
					catch ( Exception e ) when ( e is DateTimeException || e is ArithmeticException )
					{
						 throw new TemporalArithmeticException( e.Message, e );
					}

					days = AssertValidUntil( from, to, ChronoUnit.DAYS );
					try
					{
						 from = from.plus( days, ChronoUnit.DAYS );
					}
					catch ( Exception e ) when ( e is DateTimeException || e is ArithmeticException )
					{
						 throw new TemporalArithmeticException( e.Message, e );
					}
			  }
			  long nanos = AssertValidUntil( from, to, NANOS );
			  return NewDuration( months, days, nanos / NANOS_PER_SECOND, nanos % NANOS_PER_SECOND );
		 }

		 private static DurationValue DurationInSecondsAndNanos( Temporal from, Temporal to )
		 {
			  long seconds;
			  long nanos;
			  bool negate = false;
			  if ( from.isSupported( OFFSET_SECONDS ) && !to.isSupported( OFFSET_SECONDS ) )
			  {
					negate = true;
					Temporal tmp = from;
					from = to;
					to = tmp;
			  }
			  seconds = AssertValidUntil( from, to, SECONDS );
			  int fromNanos = from.isSupported( NANO_OF_SECOND ) ? from.get( NANO_OF_SECOND ) : 0;
			  int toNanos = to.isSupported( NANO_OF_SECOND ) ? to.get( NANO_OF_SECOND ) : 0;
			  nanos = toNanos - fromNanos;

			  bool differenceIsLessThanOneSecond = seconds == 0 && from.isSupported( SECOND_OF_MINUTE ) && to.isSupported( SECOND_OF_MINUTE ) && from.get( SECOND_OF_MINUTE ) != to.get( SECOND_OF_MINUTE );

			  if ( nanos < 0 && ( seconds > 0 || differenceIsLessThanOneSecond ) )
			  {
					nanos = NANOS_PER_SECOND + nanos;
			  }
			  else if ( nanos > 0 && ( seconds < 0 || differenceIsLessThanOneSecond ) )
			  {
					nanos = nanos - NANOS_PER_SECOND;
			  }
			  if ( negate )
			  {
					seconds = -seconds;
					nanos = -nanos;
			  }
			  return Duration( 0, 0, seconds, nanos );
		 }

		 public override bool Equals( Value other )
		 {
			  if ( other is DurationValue )
			  {
					DurationValue that = ( DurationValue ) other;
					return that._months == this._months && that._days == this._days && that._seconds == this._seconds && that._nanos == this._nanos;
			  }
			  else
			  {
					return false;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void WriteTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteDuration( _months, _days, _seconds, _nanos );
		 }

		 public override TemporalAmount AsObjectCopy()
		 {
			  return this;
		 }

		 public override string ToString()
		 {
			  return PrettyPrint();
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Duration";
			 }
		 }

		 public override string PrettyPrint()
		 {
			  if ( this == Zero )
			  {
					return "PT0S"; // no need to allocate a string builder if we know the result
			  }
			  StringBuilder str = ( new StringBuilder() ).Append("P");
			  Append( str, _months / 12, 'Y' );
			  Append( str, _months % 12, 'M' );
			  Append( str, _days, 'D' );
			  if ( _seconds != 0 || _nanos != 0 )
			  {
					bool negative = _seconds < 0;
					long s = _seconds;
					int n = _nanos;
					if ( negative && _nanos != 0 )
					{
						 s++;
						 n -= NANOS_PER_SECOND;
					}
					str.Append( 'T' );
					Append( str, s / 3600, 'H' );
					s %= 3600;
					Append( str, s / 60, 'M' );
					s %= 60;
					if ( s != 0 )
					{
						 if ( negative && s >= 0 && n != 0 )
						 {
							  str.Append( '-' );
						 }
						 str.Append( s );
						 if ( n != 0 )
						 {
							  Nanos( str, n );
						 }
						 str.Append( 'S' );
					}
					else if ( n != 0 )
					{
						 if ( negative )
						 {
							  str.Append( '-' );
						 }
						 str.Append( '0' );
						 Nanos( str, n );
						 str.Append( 'S' );
					}
			  }
			  if ( str.Length == 1 )
			  { // this was all zeros (but not ZERO for some reason), ensure well formed output:
					str.Append( "T0S" );
			  }
			  return str.ToString();
		 }

		 private void Nanos( StringBuilder str, int nanos )
		 {
			  str.Append( '.' );
			  int n = nanos < 0 ? -nanos : nanos;
			  for ( int mod = ( int )NANOS_PER_SECOND; mod > 1 && n > 0; n %= mod )
			  {
					str.Append( n / ( mod /= 10 ) );
			  }
		 }

		 private static void Append( StringBuilder str, long quantity, char unit )
		 {
			  if ( quantity != 0 )
			  {
					str.Append( quantity ).Append( unit );
			  }
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.Duration;
		 }

		 public override NumberType NumberType()
		 {
			  return NO_NUMBER;
		 }

		 protected internal override int ComputeHash()
		 {
			  int result = ( int )( _months ^ ( ( long )( ( ulong )_months >> 32 ) ) );
			  result = 31 * result + ( int )( _days ^ ( ( long )( ( ulong )_days >> 32 ) ) );
			  result = 31 * result + ( int )( _seconds ^ ( ( long )( ( ulong )_seconds >> 32 ) ) );
			  result = 31 * result + _nanos;
			  return result;
		 }

		 public override long UpdateHash( IHashFunction hashFunction, long hash )
		 {
			  hash = hashFunction.Update( hash, _months );
			  hash = hashFunction.Update( hash, _days );
			  hash = hashFunction.Update( hash, _seconds );
			  hash = hashFunction.Update( hash, _nanos );
			  return hash;
		 }

		 public override T Map<T>( IValueMapper<T> mapper )
		 {
			  return mapper.MapDuration( this );
		 }

		 public override long Get( TemporalUnit unit )
		 {
			  if ( unit is ChronoUnit )
			  {
					switch ( ( ChronoUnit ) unit )
					{
					case MONTHS:
						 return _months;
					case DAYS:
						 return _days;
					case SECONDS:
						 return _seconds;
					case NANOS:
						 return _nanos;
					default:
						 break;
					}
			  }
			  throw new UnsupportedTemporalUnitException( "Unsupported unit: " + unit );
		 }

		 /// <summary>
		 /// In contrast to <seealso cref="get(TemporalUnit)"/>, this method supports more units, namely:
		 /// 
		 /// years, hours, minutes, milliseconds, microseconds,
		 /// monthsOfYear, minutesOfHour, secondsOfMinute, millisecondsOfSecond, microsecondsOfSecond, nanosecondsOfSecond
		 /// </summary>
		 public LongValue Get( string fieldName )
		 {
			  long val = DurationFields.fromName( fieldName ).asTimeStamp( _months, _days, _seconds, _nanos );
			  return Values.LongValue( val );
		 }

		 public override IList<TemporalUnit> Units
		 {
			 get
			 {
				  return _units;
			 }
		 }

		 public DurationValue Plus( long amount, TemporalUnit unit )
		 {
			  if ( unit is ChronoUnit )
			  {
					switch ( ( ChronoUnit ) unit )
					{
					case NANOS:
						 return Duration( _months, _days, _seconds, _nanos + amount );
					case MICROS:
						 return Duration( _months, _days, _seconds, _nanos + amount * 1000 );
					case MILLIS:
						 return Duration( _months, _days, _seconds, _nanos + amount * 1000_000 );
					case SECONDS:
						 return Duration( _months, _days, _seconds + amount, _nanos );
					case MINUTES:
						 return Duration( _months, _days, _seconds + amount * 60, _nanos );
					case HOURS:
						 return Duration( _months, _days, _seconds + amount * 3600, _nanos );
					case HALF_DAYS:
						 return Duration( _months, _days, _seconds + amount * 12 * 3600, _nanos );
					case DAYS:
						 return Duration( _months, _days + amount, _seconds, _nanos );
					case WEEKS:
						 return Duration( _months, _days + amount * 7, _seconds, _nanos );
					case MONTHS:
						 return Duration( _months + amount, _days, _seconds, _nanos );
					case YEARS:
						 return Duration( _months + amount * 12, _days, _seconds, _nanos );
					case DECADES:
						 return Duration( _months + amount * 120, _days, _seconds, _nanos );
					case CENTURIES:
						 return Duration( _months + amount * 1200, _days, _seconds, _nanos );
					case MILLENNIA:
						 return Duration( _months + amount * 12000, _days, _seconds, _nanos );
					default:
						 break;
					}
			  }
			  throw new System.NotSupportedException( "Unsupported unit: " + unit );
		 }

		 public override Temporal AddTo( Temporal temporal )
		 {
			  if ( _months != 0 && temporal.isSupported( MONTHS ) )
			  {
					temporal = AssertValidPlus( temporal, _months, MONTHS );
			  }
			  if ( _days != 0 && temporal.isSupported( DAYS ) )
			  {
					temporal = AssertValidPlus( temporal, _days, DAYS );
			  }
			  if ( _seconds != 0 )
			  {
					if ( temporal.isSupported( SECONDS ) )
					{
						 temporal = AssertValidPlus( temporal, _seconds, SECONDS );
					}
					else
					{
						 long asDays = _seconds / SECONDS_PER_DAY;
						 if ( asDays != 0 )
						 {
							  temporal = AssertValidPlus( temporal, asDays, DAYS );
						 }
					}
			  }
			  if ( _nanos != 0 && temporal.isSupported( NANOS ) )
			  {
					temporal = AssertValidPlus( temporal, _nanos, NANOS );
			  }
			  return temporal;
		 }

		 public override Temporal SubtractFrom( Temporal temporal )
		 {
			  if ( _months != 0 && temporal.isSupported( MONTHS ) )
			  {
					temporal = AssertValidMinus( temporal, _months, MONTHS );
			  }
			  if ( _days != 0 && temporal.isSupported( DAYS ) )
			  {
					temporal = AssertValidMinus( temporal, _days, DAYS );
			  }
			  if ( _seconds != 0 )
			  {
					if ( temporal.isSupported( SECONDS ) )
					{
						 temporal = AssertValidMinus( temporal, _seconds, SECONDS );
					}
					else if ( temporal.isSupported( DAYS ) )
					{
						 long asDays = _seconds / SECONDS_PER_DAY;
						 if ( asDays != 0 )
						 {
							  temporal = AssertValidMinus( temporal, asDays, DAYS );
						 }
					}
			  }
			  if ( _nanos != 0 && temporal.isSupported( NANOS ) )
			  {
					temporal = AssertValidMinus( temporal, _nanos, NANOS );
			  }
			  return temporal;
		 }

		 public DurationValue Add( DurationValue that )
		 {
			  try
			  {
					return Duration( Math.addExact( this._months, that._months ), Math.addExact( this._days, that._days ), Math.addExact( this._seconds, that._seconds ), Math.addExact( this._nanos, that._nanos ) );
			  }
			  catch ( ArithmeticException e )
			  {
					throw InvalidDurationAdd( this, that, e );
			  }
		 }

		 public DurationValue Sub( DurationValue that )
		 {
			  try
			  {
					return Duration( Math.subtractExact( this._months, that._months ), Math.subtractExact( this._days, that._days ), Math.subtractExact( this._seconds, that._seconds ), Math.subtractExact( this._nanos, that._nanos ) );
			  }
			  catch ( ArithmeticException e )
			  {
					throw InvalidDurationSubtract( this, that, e );
			  }
		 }

		 public DurationValue Mul( NumberValue number )
		 {
			  try
			  {
					if ( number is IntegralValue )
					{
						 long factor = number.LongValue();
						 return Duration( Math.multiplyExact( _months, factor ), Math.multiplyExact( _days, factor ), Math.multiplyExact( _seconds, factor ), Math.multiplyExact( _nanos, factor ) );
					}
					if ( number is FloatingPointValue )
					{
						 double factor = number.DoubleValue();
						 return Approximate( _months * factor, _days * factor, _seconds * factor, _nanos * factor );
					}
			  }
			  catch ( ArithmeticException e )
			  {
					throw InvalidDurationMultiply( this, number, e );
			  }
			  throw new InvalidValuesArgumentException( "Factor must be either integer of floating point number." );
		 }

		 public DurationValue Div( NumberValue number )
		 {
			  double divisor = number.DoubleValue();
			  try
			  {
					return Approximate( _months / divisor, _days / divisor, _seconds / divisor, _nanos / divisor );
			  }
			  catch ( ArithmeticException e )
			  {
					throw InvalidDurationDivision( this, number, e );
			  }
		 }

		 /// <summary>
		 /// Returns an approximation of the provided values by rounding to whole units and recalculating
		 /// the remainder into the smaller units.
		 /// </summary>
		 public static DurationValue Approximate( double months, double days, double seconds, double nanos )
		 {
			  long monthsAsLong = SafeDoubleToLong( months );

			  double monthDiffInNanos = AVG_NANOS_PER_MONTH * months - AVG_NANOS_PER_MONTH * monthsAsLong;
			  days += monthDiffInNanos / ( NANOS_PER_SECOND * SECONDS_PER_DAY );
			  long daysAsLong = SafeDoubleToLong( days );

			  double daysDiffInNanos = NANOS_PER_SECOND * SECONDS_PER_DAY * days - NANOS_PER_SECOND * SECONDS_PER_DAY * daysAsLong;
			  seconds += daysDiffInNanos / NANOS_PER_SECOND;
			  long secondsAsLong = SafeDoubleToLong( seconds );

			  double secondsDiffInNanos = NANOS_PER_SECOND * seconds - NANOS_PER_SECOND * secondsAsLong;
			  nanos += secondsDiffInNanos;
			  long nanosAsLong = SafeDoubleToLong( nanos );

			  return Duration( monthsAsLong, daysAsLong, secondsAsLong, nanosAsLong );
		 }

		 /// <summary>
		 /// Will cast a double to a long, but only if it is inside the limits of [Long.MIN_VALUE, LONG.MAX_VALUE]
		 /// We need this to detect overflow errors, whereas normal truncation is OK while approximating.
		 /// </summary>
		 private static long SafeDoubleToLong( double d )
		 {
			  if ( d > long.MaxValue || d < long.MinValue )
			  {
					throw new ArithmeticException( "long overflow" );
			  }
			  return ( long ) d;
		 }

		 private static Temporal AssertValidPlus( Temporal temporal, long amountToAdd, TemporalUnit unit )
		 {
			  try
			  {
					return temporal.plus( amountToAdd, unit );
			  }
			  catch ( Exception e ) when ( e is DateTimeException || e is ArithmeticException )
			  {
					throw new TemporalArithmeticException( e.Message, e );
			  }
		 }

		 private static Temporal AssertValidMinus( Temporal temporal, long amountToAdd, TemporalUnit unit )
		 {
			  try
			  {
					return temporal.minus( amountToAdd, unit );
			  }
			  catch ( Exception e ) when ( e is DateTimeException || e is ArithmeticException )
			  {
					throw new TemporalArithmeticException( e.Message, e );
			  }
		 }

		 private static long AssertValidUntil( Temporal from, Temporal to, TemporalUnit unit )
		 {
			  try
			  {
					return from.until( to, unit );
			  }
			  catch ( UnsupportedTemporalTypeException e )
			  {
					throw new UnsupportedTemporalUnitException( e.Message, e );
			  }
			  catch ( DateTimeException e )
			  {
					throw new InvalidValuesArgumentException( e.Message, e );
			  }
		 }

		 private InvalidValuesArgumentException InvalidDuration( long months, long days, long seconds, long nanos, ArithmeticException e )
		 {
			  return new InvalidValuesArgumentException( string.Format( "Invalid value for duration, will cause overflow. Value was months={0:D}, days={1:D}, seconds={2:D}, nanos={3:D}", months, days, seconds, nanos ), e );
		 }

		 private InvalidValuesArgumentException InvalidDurationAdd( DurationValue o1, DurationValue o2, ArithmeticException e )
		 {
			  return new InvalidValuesArgumentException( string.Format( "Can not add duration {0} and {1} without causing overflow.", o1.ToString(), o2.ToString() ), e );
		 }

		 private InvalidValuesArgumentException InvalidDurationSubtract( DurationValue o1, DurationValue o2, ArithmeticException e )
		 {
			  return new InvalidValuesArgumentException( string.Format( "Can not subtract duration {0} and {1} without causing overflow.", o1.ToString(), o2.ToString() ), e );
		 }

		 private InvalidValuesArgumentException InvalidDurationMultiply( DurationValue o1, NumberValue numberValue, ArithmeticException e )
		 {
			  return new InvalidValuesArgumentException( string.Format( "Can not multiply duration {0} with {1} without causing overflow.", o1.ToString(), numberValue.ToString() ), e );
		 }

		 private InvalidValuesArgumentException InvalidDurationDivision( DurationValue o1, NumberValue numberValue, ArithmeticException e )
		 {
			  return new InvalidValuesArgumentException( string.Format( "Can not divide duration {0} with {1} without causing overflow.", o1.ToString(), numberValue.ToString() ), e );
		 }
	}

}