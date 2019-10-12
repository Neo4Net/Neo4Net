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
namespace Org.Neo4j.Values.Storable
{

	using Org.Neo4j.Values;
	using Org.Neo4j.Values;
	using InvalidValuesArgumentException = Org.Neo4j.Values.utils.InvalidValuesArgumentException;
	using UnsupportedTemporalUnitException = Org.Neo4j.Values.utils.UnsupportedTemporalUnitException;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.parseZoneName;

	public sealed class LocalTimeValue : TemporalValue<LocalTime, LocalTimeValue>
	{
		 public static readonly LocalTimeValue MinValue = new LocalTimeValue( LocalTime.MIN );
		 public static readonly LocalTimeValue MaxValue = new LocalTimeValue( LocalTime.MAX );

		 public static LocalTimeValue LocalTime( LocalTime value )
		 {
			  return new LocalTimeValue( requireNonNull( value, "LocalTime" ) );
		 }

		 public static LocalTimeValue LocalTime( int hour, int minute, int second, int nanosOfSecond )
		 {
			  return new LocalTimeValue( AssertValidArgument( () => LocalTime.of(hour, minute, second, nanosOfSecond) ) );
		 }

		 public static LocalTimeValue LocalTime( long nanoOfDay )
		 {
			  return new LocalTimeValue( LocalTimeRaw( nanoOfDay ) );
		 }

		 public static LocalTime LocalTimeRaw( long nanoOfDay )
		 {
			  return AssertValidArgument( () => LocalTime.ofNanoOfDay(nanoOfDay) );
		 }

		 public static LocalTimeValue Parse( CharSequence text )
		 {
			  return Parse( typeof( LocalTimeValue ), _pattern, LocalTimeValue.parse, text );
		 }

		 public static LocalTimeValue Parse( TextValue text )
		 {
			  return Parse( typeof( LocalTimeValue ), _pattern, LocalTimeValue.parse, text );
		 }

		 public static LocalTimeValue Now( Clock clock )
		 {
			  return new LocalTimeValue( LocalTime.now( clock ) );
		 }

		 public static LocalTimeValue Now( Clock clock, string timezone )
		 {
			  return Now( clock.withZone( parseZoneName( timezone ) ) );
		 }

		 public static LocalTimeValue Now( Clock clock, System.Func<ZoneId> defaultZone )
		 {
			  return Now( clock.withZone( defaultZone() ) );
		 }

		 public static LocalTimeValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return StructureBuilder.build( Builder( defaultZone ), map );
		 }

		 public static LocalTimeValue Select( AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return Builder( defaultZone ).selectTime( from );
		 }

		 public static LocalTimeValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  LocalTime localTime = input.LocalTimePart;
			  LocalTime truncatedLT = AssertValidUnit( () => localTime.truncatedTo(unit) );
			  if ( fields.Size() == 0 )
			  {
					return localTime( truncatedLT );
			  }
			  else
			  {
					return UpdateFieldMapWithConflictingSubseconds(fields, unit, truncatedLT, (mapValue, localTime1) =>
					{
					if ( mapValue.size() == 0 )
					{
						return localTime( localTime1 );
					}
					else
					{
						return Build( mapValue.updatedWith( "time", localTime( localTime1 ) ), defaultZone );
					}
					});
			  }
		 }

		 internal static readonly LocalTime DefaultLocalTime = LocalTime.of( TemporalFields.Hour.defaultValue, TemporalFields.Minute.defaultValue );

		 internal static TimeValue.TimeBuilder<LocalTimeValue> Builder( System.Func<ZoneId> defaultZone )
		 {
			  return new TimeBuilderAnonymousInnerClass( defaultZone );
		 }

		 private class TimeBuilderAnonymousInnerClass : TimeValue.TimeBuilder<LocalTimeValue>
		 {
			 public TimeBuilderAnonymousInnerClass( System.Func<ZoneId> defaultZone ) : base( defaultZone )
			 {
			 }

			 protected internal override bool supportsTimeZone()
			 {
				  return false;
			 }

			 public override LocalTimeValue buildInternal()
			 {
				  LocalTime result;
				  if ( fields.containsKey( TemporalFields.Time ) )
				  {
						AnyValue time = fields.get( TemporalFields.Time );
						if ( !( time is TemporalValue ) )
						{
							 throw new InvalidValuesArgumentException( string.Format( "Cannot construct local time from: {0}", time ) );
						}
						result = ( ( TemporalValue ) time ).LocalTimePart;
				  }
				  else
				  {
						result = DefaultLocalTime;
				  }

				  result = assignAllFields( result );
				  return LocalTime( result );
			 }

			 protected internal override LocalTimeValue selectTime( AnyValue time )
			 {

				  if ( !( time is TemporalValue ) )
				  {
						throw new InvalidValuesArgumentException( string.Format( "Cannot construct local time from: {0}", time ) );
				  }
				  TemporalValue v = ( TemporalValue ) time;
				  LocalTime lt = v.LocalTimePart;
				  return LocalTime( lt );
			 }
		 }

		 private readonly LocalTime _value;

		 private LocalTimeValue( LocalTime value )
		 {
			  this._value = value;
		 }

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  LocalTimeValue other = ( LocalTimeValue ) otherValue;
			  return _value.compareTo( other._value );
		 }

		 internal override LocalTime Temporal()
		 {
			  return _value;
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "LocalTime";
			 }
		 }

		 internal override LocalDate DatePart
		 {
			 get
			 {
				  throw new UnsupportedTemporalUnitException( string.Format( "Cannot get the date of: {0}", this ) );
			 }
		 }

		 internal override LocalTime LocalTimePart
		 {
			 get
			 {
				  return _value;
			 }
		 }

		 internal override OffsetTime GetTimePart( System.Func<ZoneId> defaultZone )
		 {
			  ZoneOffset currentOffset = AssertValidArgument( () => ZonedDateTime.ofInstant(Instant.now(), defaultZone()) ).Offset;
			  return OffsetTime.of( _value, currentOffset );
		 }

		 internal override ZoneId GetZoneId( System.Func<ZoneId> defaultZone )
		 {
			  return defaultZone();
		 }

		 internal override ZoneId GetZoneId()
		 {
			  throw new UnsupportedTemporalUnitException( string.Format( "Cannot get the timezone of: {0}", this ) );
		 }

		 internal override ZoneOffset ZoneOffset
		 {
			 get
			 {
				  throw new UnsupportedTemporalUnitException( string.Format( "Cannot get the offset of: {0}", this ) );
			 }
		 }

		 public override bool SupportsTimeZone()
		 {
			  return false;
		 }

		 internal override bool HasTime()
		 {
			  return true;
		 }

		 public override bool Equals( Value other )
		 {
			  return other is LocalTimeValue && _value.Equals( ( ( LocalTimeValue ) other )._value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteLocalTime( _value );
		 }

		 public override string PrettyPrint()
		 {
			  return AssertPrintable( () => _value.format(DateTimeFormatter.ISO_LOCAL_TIME) );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.LocalTime;
		 }

		 protected internal override int ComputeHash()
		 {
			  return _value.GetHashCode();
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapLocalTime( this );
		 }

		 public override LocalTimeValue Add( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.plusNanos(duration.NanosOfDay()) ) );
		 }

		 public override LocalTimeValue Sub( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.minusNanos(duration.NanosOfDay()) ) );
		 }

		 internal override LocalTimeValue Replacement( LocalTime time )
		 {
			  return time == _value ? this : new LocalTimeValue( time );
		 }

		 internal const string TIME_PATTERN = "(?:(?:(?<longHour>[0-9]{1,2})(?::(?<longMinute>[0-9]{1,2})"
					+ "(?::(?<longSecond>[0-9]{1,2})(?:\\.(?<longFraction>[0-9]{1,9}))?)?)?)|"
					+ "(?:(?<shortHour>[0-9]{2})(?:(?<shortMinute>[0-9]{2})"
					+ "(?:(?<shortSecond>[0-9]{2})(?:\\.(?<shortFraction>[0-9]{1,9}))?)?)?))";
		 private static readonly Pattern _pattern = Pattern.compile( "(?:T)?" + TIME_PATTERN );

		 private static LocalTimeValue Parse( Matcher matcher )
		 {
			  return new LocalTimeValue( ParseTime( matcher ) );
		 }

		 internal static LocalTime ParseTime( Matcher matcher )
		 {
			  int hour;
			  int minute;
			  int second;
			  int fraction;
			  string longHour = matcher.group( "longHour" );
			  if ( !string.ReferenceEquals( longHour, null ) )
			  {
					hour = parseInt( longHour );
					minute = OptInt( matcher.group( "longMinute" ) );
					second = OptInt( matcher.group( "longSecond" ) );
					fraction = ParseNanos( matcher.group( "longFraction" ) );
			  }
			  else
			  {
					string shortHour = matcher.group( "shortHour" );
					hour = parseInt( shortHour );
					minute = OptInt( matcher.group( "shortMinute" ) );
					second = OptInt( matcher.group( "shortSecond" ) );
					fraction = ParseNanos( matcher.group( "shortFraction" ) );
			  }
			  return AssertParsable( () => LocalTime.of(hour, minute, second, fraction) );
		 }

		 private static int ParseNanos( string value )
		 {
			  if ( string.ReferenceEquals( value, null ) )
			  {
					return 0;
			  }
			  int nanos = parseInt( value );
			  if ( nanos != 0 )
			  {
					for ( int i = value.Length; i < 9; i++ )
					{
						 nanos *= 10;
					}
			  }
			  return nanos;
		 }

		 internal static int OptInt( string value )
		 {
			  return string.ReferenceEquals( value, null ) ? 0 : parseInt( value );
		 }
	}

}