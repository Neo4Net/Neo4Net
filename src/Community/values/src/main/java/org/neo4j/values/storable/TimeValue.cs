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

	using Neo4Net.Values;
	using Neo4Net.Values;
	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using TemporalUtil = Neo4Net.Values.utils.TemporalUtil;
	using UnsupportedTemporalUnitException = Neo4Net.Values.utils.UnsupportedTemporalUnitException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.parseZoneName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.optInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.parseTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	public sealed class TimeValue : TemporalValue<OffsetTime, TimeValue>
	{
		 public static readonly TimeValue MinValue = new TimeValue( OffsetTime.MIN );
		 public static readonly TimeValue MaxValue = new TimeValue( OffsetTime.MAX );

		 public static TimeValue Time( OffsetTime time )
		 {
			  return new TimeValue( requireNonNull( time, "OffsetTime" ) );
		 }

		 public static TimeValue Time( int hour, int minute, int second, int nanosOfSecond, string offset )
		 {
			  return Time( hour, minute, second, nanosOfSecond, ParseOffset( offset ) );
		 }

		 public static TimeValue Time( int hour, int minute, int second, int nanosOfSecond, ZoneOffset offset )
		 {
			  return new TimeValue( OffsetTime.of( AssertValidArgument( () => LocalTime.of(hour, minute, second, nanosOfSecond) ), offset ) );
		 }

		 public static TimeValue Time( long nanosOfDayUTC, ZoneOffset offset )
		 {
			  return new TimeValue( TimeRaw( nanosOfDayUTC, offset ) );
		 }

		 public static OffsetTime TimeRaw( long nanosOfDayUTC, ZoneOffset offset )
		 {
			  return OffsetTime.ofInstant( AssertValidArgument( () => Instant.ofEpochSecond(0, nanosOfDayUTC) ), offset );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Time";
			 }
		 }

		 public static TimeValue Parse( CharSequence text, System.Func<ZoneId> defaultZone, CSVHeaderInformation fieldsFromHeader )
		 {
			  if ( fieldsFromHeader != null )
			  {
					if ( !( fieldsFromHeader is TimeCSVHeaderInformation ) )
					{
						 throw new System.InvalidOperationException( "Wrong header information type: " + fieldsFromHeader );
					}
					// Override defaultZone
					defaultZone = ( ( TimeCSVHeaderInformation ) fieldsFromHeader ).ZoneSupplier( defaultZone );
			  }
			  return Parse( typeof( TimeValue ), _pattern, TimeValue.parse, text, defaultZone );
		 }

		 public static TimeValue Parse( CharSequence text, System.Func<ZoneId> defaultZone )
		 {
			  return Parse( typeof( TimeValue ), _pattern, TimeValue.parse, text, defaultZone );
		 }

		 public static TimeValue Parse( TextValue text, System.Func<ZoneId> defaultZone )
		 {
			  return Parse( typeof( TimeValue ), _pattern, TimeValue.parse, text, defaultZone );
		 }

		 public static TimeValue Now( Clock clock )
		 {
			  return new TimeValue( OffsetTime.now( clock ) );
		 }

		 public static TimeValue Now( Clock clock, string timezone )
		 {
			  return Now( clock.withZone( parseZoneName( timezone ) ) );
		 }

		 public static TimeValue Now( Clock clock, System.Func<ZoneId> defaultZone )
		 {
			  return Now( clock.withZone( defaultZone() ) );
		 }

		 public static TimeValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return StructureBuilder.build( Builder( defaultZone ), map );
		 }

		 public static TimeValue Select( AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return Builder( defaultZone ).selectTime( from );
		 }

		 internal override bool HasTime()
		 {
			  return true;
		 }

		 public static TimeValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  OffsetTime time = input.getTimePart( defaultZone );
			  OffsetTime truncatedOT = AssertValidUnit( () => time.truncatedTo(unit) );
			  if ( fields.Size() == 0 )
			  {
					return time( truncatedOT );
			  }
			  else
			  {
					// Timezone needs some special handling, since the builder will shift keeping the instant instead of the local time
					AnyValue timezone = fields.Get( "timezone" );
					if ( timezone != NO_VALUE )
					{
						 ZonedDateTime currentDT = AssertValidArgument( () => ZonedDateTime.ofInstant(Instant.now(), TimezoneOf(timezone)) );
						 ZoneOffset currentOffset = currentDT.Offset;
						 truncatedOT = truncatedOT.withOffsetSameLocal( currentOffset );
					}

					return UpdateFieldMapWithConflictingSubseconds(fields, unit, truncatedOT, (mapValue, offsetTime) =>
					{
					if ( mapValue.size() == 0 )
					{
						return time( offsetTime );
					}
					else
					{
						return Build( mapValue.updatedWith( "time", time( offsetTime ) ), defaultZone );
					}
					});

			  }
		 }

		 internal static OffsetTime DefaultTime( ZoneId zoneId )
		 {
			  return OffsetTime.of( TemporalFields.Hour.defaultValue, TemporalFields.Minute.defaultValue, TemporalFields.Second.defaultValue, TemporalFields.Nanosecond.defaultValue, AssertValidZone( () => ZoneOffset.of(zoneId.ToString()) ) );
		 }

		 internal static TimeBuilder<TimeValue> Builder( System.Func<ZoneId> defaultZone )
		 {
			  return new TimeBuilderAnonymousInnerClass( defaultZone );
		 }

		 private class TimeBuilderAnonymousInnerClass : TimeBuilder<TimeValue>
		 {
			 private System.Func<ZoneId> _defaultZone;

			 public TimeBuilderAnonymousInnerClass( System.Func<ZoneId> defaultZone ) : base( defaultZone )
			 {
				 this._defaultZone = defaultZone;
			 }

			 protected internal override bool supportsTimeZone()
			 {
				  return true;
			 }

			 public override TimeValue buildInternal()
			 {
				  bool selectingTime = fields.containsKey( TemporalFields.Time );
				  bool selectingTimeZone;
				  OffsetTime result;
				  if ( selectingTime )
				  {
						AnyValue time = fields.get( TemporalFields.Time );
						if ( !( time is TemporalValue ) )
						{
							 throw new InvalidValuesArgumentException( string.Format( "Cannot construct time from: {0}", time ) );
						}
						TemporalValue t = ( TemporalValue ) time;
						result = t.getTimePart( _defaultZone );
						selectingTimeZone = t.supportsTimeZone();
				  }
				  else
				  {
						ZoneId timezone = timezone();
						if ( !( timezone is ZoneOffset ) )
						{
							 timezone = AssertValidArgument( () => ZonedDateTime.ofInstant(Instant.now(), timezone()) ).Offset;
						}

						result = DefaultTime( timezone );
						selectingTimeZone = false;
				  }

				  result = assignAllFields( result );
				  if ( timezone != null )
				  {
						ZoneOffset currentOffset = AssertValidArgument( () => ZonedDateTime.ofInstant(Instant.now(), timezone()) ).Offset;
						if ( selectingTime && selectingTimeZone )
						{
							 result = result.withOffsetSameInstant( currentOffset );
						}
						else
						{
							 result = result.withOffsetSameLocal( currentOffset );
						}
				  }
				  return Time( result );
			 }
			 protected internal override TimeValue selectTime( AnyValue temporal )
			 {
				  if ( !( temporal is TemporalValue ) )
				  {
						throw new InvalidValuesArgumentException( string.Format( "Cannot construct time from: {0}", temporal ) );
				  }
				  if ( temporal is TimeValue && timezone == null )
				  {
						return ( TimeValue ) temporal;
				  }

				  TemporalValue v = ( TemporalValue ) temporal;
				  OffsetTime time = v.getTimePart( _defaultZone );
				  if ( timezone != null )
				  {
						ZoneOffset currentOffset = AssertValidArgument( () => ZonedDateTime.ofInstant(Instant.now(), timezone()) ).Offset;
						time = time.withOffsetSameInstant( currentOffset );
				  }
				  return time( time );
			 }
		 }

		 private readonly OffsetTime _value;
		 private readonly long _nanosOfDayUTC;

		 private TimeValue( OffsetTime value )
		 {
			  this._value = value;
			  this._nanosOfDayUTC = TemporalUtil.getNanosOfDayUTC( this._value );
		 }

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  TimeValue other = ( TimeValue ) otherValue;
			  int compare = Long.compare( _nanosOfDayUTC, other._nanosOfDayUTC );
			  if ( compare == 0 )
			  {
					compare = Integer.compare( _value.Offset.TotalSeconds, other._value.Offset.TotalSeconds );
			  }
			  return compare;
		 }

		 internal override OffsetTime Temporal()
		 {
			  return _value;
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
				  return _value.toLocalTime();
			 }
		 }

		 internal override OffsetTime GetTimePart( System.Func<ZoneId> defaultZone )
		 {
			  return _value;
		 }

		 internal override ZoneId GetZoneId( System.Func<ZoneId> defaultZone )
		 {
			  return _value.Offset;
		 }

		 internal override ZoneId GetZoneId()
		 {
			  throw new UnsupportedTemporalTypeException( "Cannot get the timezone of" + this );
		 }

		 internal override ZoneOffset ZoneOffset
		 {
			 get
			 {
				  return _value.Offset;
			 }
		 }

		 public override bool SupportsTimeZone()
		 {
			  return true;
		 }

		 public override bool Equals( Value other )
		 {
			  return other is TimeValue && _value.Equals( ( ( TimeValue ) other )._value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteTime( _value );
		 }

		 public override string PrettyPrint()
		 {
			  return AssertPrintable( () => _value.format(DateTimeFormatter.ISO_TIME) );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.ZonedTime;
		 }

		 protected internal override int ComputeHash()
		 {
			  return Long.GetHashCode( _value.toLocalTime().toNanoOfDay() - _value.Offset.TotalSeconds * 1000_000_000L );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapTime( this );
		 }

		 public override TimeValue Add( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.plusNanos(duration.NanosOfDay()) ) );
		 }

		 public override TimeValue Sub( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.minusNanos(duration.NanosOfDay()) ) );
		 }

		 internal override TimeValue Replacement( OffsetTime time )
		 {
			  return time == _value ? this : new TimeValue( time );
		 }

		 private const string OFFSET_PATTERN = "(?<zone>Z|[+-](?<zoneHour>[0-9]{2})(?::?(?<zoneMinute>[0-9]{2}))?)";
		 internal static readonly string TimePattern = LocalTimeValue.TIME_PATTERN + "(?:" + OFFSET_PATTERN + ")?";
		 private static readonly Pattern _pattern = Pattern.compile( "(?:T)?" + TimePattern );
		 internal static readonly Pattern Offset = Pattern.compile( OFFSET_PATTERN );

		 internal static ZoneOffset ParseOffset( string offset )
		 {
			  Matcher matcher = Offset.matcher( offset );
			  if ( matcher.matches() )
			  {
					return ParseOffset( matcher );
			  }
			  throw new InvalidValuesArgumentException( "Not a valid offset: " + offset );
		 }

		 internal static ZoneOffset ParseOffset( Matcher matcher )
		 {
			  string zone = matcher.group( "zone" );
			  if ( null == zone )
			  {
					return null;
			  }
			  if ( "Z".Equals( zone, StringComparison.OrdinalIgnoreCase ) )
			  {
					return UTC;
			  }
			  int factor = zone[0] == '+' ? 1 : -1;
			  int hours = parseInt( matcher.group( "zoneHour" ) );
			  int minutes = optInt( matcher.group( "zoneMinute" ) );
			  return AssertValidZone( () => ZoneOffset.ofHoursMinutes(factor * hours, factor * minutes) );
		 }

		 private static TimeValue Parse( Matcher matcher, System.Func<ZoneId> defaultZone )
		 {
			  return new TimeValue( OffsetTime.of( parseTime( matcher ), ParseOffset( matcher, defaultZone ) ) );
		 }

		 private static ZoneOffset ParseOffset( Matcher matcher, System.Func<ZoneId> defaultZone )
		 {
			  ZoneOffset offset = ParseOffset( matcher );
			  if ( offset == null )
			  {
					ZoneId zoneId = defaultZone();
					offset = zoneId is ZoneOffset ? ( ZoneOffset ) zoneId : zoneId.Rules.getOffset( Instant.now() );
			  }
			  return offset;
		 }

		 internal abstract class TimeBuilder<Result> : Builder<Result>
		 {
			  internal TimeBuilder( System.Func<ZoneId> defaultZone ) : base( defaultZone )
			  {
			  }

			  protected internal override bool SupportsDate()
			  {
					return false;
			  }

			  protected internal override bool SupportsTime()
			  {
					return true;
			  }

			  protected internal override bool SupportsEpoch()
			  {
					return false;
			  }

			  protected internal abstract Result SelectTime( AnyValue time );
		 }
	}

}