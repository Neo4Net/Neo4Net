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
namespace Neo4Net.Values.Storable
{

	using Neo4Net.Helpers.Collections;
	using Neo4Net.Values;
	using Neo4Net.Values;
	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using TemporalParseException = Neo4Net.Values.utils.TemporalParseException;
	using UnsupportedTemporalUnitException = Neo4Net.Values.utils.UnsupportedTemporalUnitException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.DATE_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.parseDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.IntegralValue.safeCastIntegral;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalDateTimeValue.optTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.TimeValue.OFFSET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.TimeValue.TIME_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.TimeValue.parseOffset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	public sealed class DateTimeValue : TemporalValue<ZonedDateTime, DateTimeValue>
	{
		 public static readonly DateTimeValue MinValue = new DateTimeValue( ZonedDateTime.of( DateTime.MinValue, ZoneOffset.MIN ) );
		 public static readonly DateTimeValue MaxValue = new DateTimeValue( ZonedDateTime.of( DateTime.MaxValue, ZoneOffset.MAX ) );

		 public static DateTimeValue Datetime( DateValue date, LocalTimeValue time, ZoneId zone )
		 {
			  return new DateTimeValue( ZonedDateTime.of( date.Temporal(), time.Temporal(), zone ) );
		 }

		 public static DateTimeValue Datetime( DateValue date, TimeValue time )
		 {
			  OffsetTime t = time.Temporal();
			  return new DateTimeValue( ZonedDateTime.of( date.Temporal(), t.toLocalTime(), t.Offset ) );
		 }

		 public static DateTimeValue Datetime( int year, int month, int day, int hour, int minute, int second, int nanoOfSecond, string zone )
		 {
			  return Datetime( year, month, day, hour, minute, second, nanoOfSecond, ParseZoneName( zone ) );
		 }

		 public static DateTimeValue Datetime( int year, int month, int day, int hour, int minute, int second, int nanoOfSecond, ZoneId zone )
		 {
			  return new DateTimeValue( AssertValidArgument( () => ZonedDateTime.of(year, month, day, hour, minute, second, nanoOfSecond, zone) ) );
		 }

		 public static DateTimeValue Datetime( long epochSecond, long nano, ZoneOffset zoneOffset )
		 {
			  return new DateTimeValue( DatetimeRaw( epochSecond, nano, zoneOffset ) );
		 }

		 public static ZonedDateTime DatetimeRaw( long epochSecond, long nano, ZoneOffset zoneOffset )
		 {
			  return DatetimeRaw( epochSecond, nano, ( ZoneId ) zoneOffset );
		 }

		 public static DateTimeValue Datetime( ZonedDateTime datetime )
		 {
			  return new DateTimeValue( requireNonNull( datetime, "ZonedDateTime" ) );
		 }

		 public static DateTimeValue Datetime( OffsetDateTime datetime )
		 {
			  return new DateTimeValue( requireNonNull( datetime, "OffsetDateTime" ).toZonedDateTime() );
		 }

		 public static DateTimeValue Datetime( long epochSecondUTC, long nano, ZoneId zone )
		 {
			  return new DateTimeValue( DatetimeRaw( epochSecondUTC, nano, zone ) );
		 }

		 public static ZonedDateTime DatetimeRaw( long epochSecondUTC, long nano, ZoneId zone )
		 {
			  return AssertValidArgument( () => ofInstant(ofEpochSecond(epochSecondUTC, nano), zone) );
		 }

		 public static DateTimeValue OfEpoch( IntegralValue epochSecondUTC, IntegralValue nano )
		 {
			  long ns = safeCastIntegral( "nanosecond", nano, 0 );
			  if ( ns < 0 || ns >= 1000_000_000 )
			  {
					throw new InvalidValuesArgumentException( "Invalid nanosecond: " + ns );
			  }
			  return new DateTimeValue( DatetimeRaw( epochSecondUTC.LongValue(), ns, UTC ) );
		 }

		 public static DateTimeValue OfEpochMillis( IntegralValue millisUTC )
		 {
			  return new DateTimeValue( AssertValidArgument( () => ofInstant(ofEpochMilli(millisUTC.LongValue()), UTC) ) );
		 }

		 public static DateTimeValue Parse( CharSequence text, System.Func<ZoneId> defaultZone, CSVHeaderInformation fieldsFromHeader )
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
			  return Parse( typeof( DateTimeValue ), _pattern, DateTimeValue.parse, text, defaultZone );
		 }

		 public static DateTimeValue Parse( CharSequence text, System.Func<ZoneId> defaultZone )
		 {
			  return Parse( typeof( DateTimeValue ), _pattern, DateTimeValue.parse, text, defaultZone );
		 }

		 public static DateTimeValue Parse( TextValue text, System.Func<ZoneId> defaultZone )
		 {
			  return Parse( typeof( DateTimeValue ), _pattern, DateTimeValue.parse, text, defaultZone );
		 }

		 public static DateTimeValue Now( Clock clock )
		 {
			  return new DateTimeValue( ZonedDateTime.now( clock ) );
		 }

		 public static DateTimeValue Now( Clock clock, string timezone )
		 {
			  return Now( clock.withZone( ParseZoneName( timezone ) ) );
		 }

		 public static DateTimeValue Now( Clock clock, System.Func<ZoneId> defaultZone )
		 {
			  return Now( clock.withZone( defaultZone() ) );
		 }

		 public static DateTimeValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return StructureBuilder.build( Builder( defaultZone ), map );
		 }

		 public static DateTimeValue Select( AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return Builder( defaultZone ).selectDateTime( from );
		 }

		 public static DateTimeValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  Pair<LocalDate, LocalTime> pair = GetTruncatedDateAndTime( unit, input, "date time" );

			  LocalDate truncatedDate = pair.First();
			  LocalTime truncatedTime = pair.Other();

			  ZoneId zoneId = input.supportsTimeZone() ? input.getZoneId(defaultZone) : defaultZone();
			  ZonedDateTime truncatedZDT = ZonedDateTime.of( truncatedDate, truncatedTime, zoneId );

			  if ( fields.Size() == 0 )
			  {
					return Datetime( truncatedZDT );
			  }
			  else
			  {
					// Timezone needs some special handling, since the builder will shift keeping the instant instead of the local time
					AnyValue timezone = fields.Get( "timezone" );
					if ( timezone != NO_VALUE )
					{
						 truncatedZDT = truncatedZDT.withZoneSameLocal( TimezoneOf( timezone ) );
					}

					return UpdateFieldMapWithConflictingSubseconds(fields, unit, truncatedZDT, (mapValue, zonedDateTime) =>
					{
					if ( mapValue.size() == 0 )
					{
						return Datetime( zonedDateTime );
					}
					else
					{
						return Build( mapValue.updatedWith( "datetime", Datetime( zonedDateTime ) ), defaultZone );
					}
					});
			  }
		 }

		 internal static DateTimeBuilder<DateTimeValue> Builder( System.Func<ZoneId> defaultZone )
		 {
			  return new DateTimeBuilderAnonymousInnerClass( defaultZone );
		 }

		 private class DateTimeBuilderAnonymousInnerClass : DateTimeBuilder<DateTimeValue>
		 {
			 private System.Func<ZoneId> _defaultZone;

			 public DateTimeBuilderAnonymousInnerClass( System.Func<ZoneId> defaultZone ) : base( defaultZone )
			 {
				 this._defaultZone = defaultZone;
			 }

			 protected internal override bool supportsTimeZone()
			 {
				  return true;
			 }

			 protected internal override bool supportsEpoch()
			 {
				  return true;
			 }

			 private readonly ZonedDateTime defaultZonedDateTime = ZonedDateTime.of( TemporalFields.Year.defaultValue, TemporalFields.Month.defaultValue, TemporalFields.Day.defaultValue, TemporalFields.Hour.defaultValue, TemporalFields.Minute.defaultValue, TemporalFields.Second.defaultValue, TemporalFields.Nanosecond.defaultValue, timezone() );

			 public override DateTimeValue buildInternal()
			 {
				  bool selectingDate = fields.containsKey( TemporalFields.Date );
				  bool selectingTime = fields.containsKey( TemporalFields.Time );
				  bool selectingDateTime = fields.containsKey( TemporalFields.Datetime );
				  bool selectingEpoch = fields.containsKey( TemporalFields.EpochSeconds ) || fields.containsKey( TemporalFields.EpochMillis );
				  bool selectingTimeZone;
				  ZonedDateTime result;
				  if ( selectingDateTime )
				  {
						AnyValue dtField = fields.get( TemporalFields.Datetime );
						if ( !( dtField is TemporalValue ) )
						{
							 throw new InvalidValuesArgumentException( string.Format( "Cannot construct date time from: {0}", dtField ) );
						}
						TemporalValue dt = ( TemporalValue ) dtField;
						LocalTime timePart = dt.getTimePart( _defaultZone ).toLocalTime();
						ZoneId zoneId = dt.getZoneId( _defaultZone );
						result = ZonedDateTime.of( dt.DatePart, timePart, zoneId );
						selectingTimeZone = dt.supportsTimeZone();
				  }
				  else if ( selectingEpoch )
				  {
						if ( fields.containsKey( TemporalFields.EpochSeconds ) )
						{
							 AnyValue epochField = fields.get( TemporalFields.EpochSeconds );
							 if ( !( epochField is IntegralValue ) )
							 {
								  throw new InvalidValuesArgumentException( string.Format( "Cannot construct date time from: {0}", epochField ) );
							 }
							 IntegralValue epochSeconds = ( IntegralValue ) epochField;
							 result = AssertValidArgument( () => ZonedDateTime.ofInstant(Instant.ofEpochMilli(epochSeconds.LongValue() * 1000), timezone()) );
						}
						else
						{
							 AnyValue epochField = fields.get( TemporalFields.EpochMillis );
							 if ( !( epochField is IntegralValue ) )
							 {
								  throw new InvalidValuesArgumentException( string.Format( "Cannot construct date time from: {0}", epochField ) );
							 }
							 IntegralValue epochMillis = ( IntegralValue ) epochField;
							 result = AssertValidArgument( () => ZonedDateTime.ofInstant(Instant.ofEpochMilli(epochMillis.LongValue()), timezone()) );
						}
						selectingTimeZone = false;
				  }
				  else if ( selectingTime || selectingDate )
				  {

						LocalTime time;
						ZoneId zoneId;
						if ( selectingTime )
						{
							 AnyValue timeField = fields.get( TemporalFields.Time );
							 if ( !( timeField is TemporalValue ) )
							 {
								  throw new InvalidValuesArgumentException( string.Format( "Cannot construct time from: {0}", timeField ) );
							 }
							 TemporalValue t = ( TemporalValue ) timeField;
							 time = t.getTimePart( _defaultZone ).toLocalTime();
							 zoneId = t.getZoneId( _defaultZone );
							 selectingTimeZone = t.supportsTimeZone();
						}
						else
						{
							 time = LocalTimeValue.DefaultLocalTime;
							 zoneId = timezone();
							 selectingTimeZone = false;
						}
						LocalDate date;
						if ( selectingDate )
						{
							 AnyValue dateField = fields.get( TemporalFields.Date );
							 if ( !( dateField is TemporalValue ) )
							 {
								  throw new InvalidValuesArgumentException( string.Format( "Cannot construct date from: {0}", dateField ) );
							 }
							 TemporalValue t = ( TemporalValue ) dateField;
							 date = t.DatePart;
						}
						else
						{
							 date = DateValue.DefaultCalenderDate;
						}
						result = ZonedDateTime.of( date, time, zoneId );
				  }
				  else
				  {
						result = defaultZonedDateTime;
						selectingTimeZone = false;
				  }

				  if ( fields.containsKey( TemporalFields.Week ) && !selectingDate && !selectingDateTime && !selectingEpoch )
				  {
						// Be sure to be in the start of the week based year (which can be later than 1st Jan)
						result = result.with( IsoFields.WEEK_BASED_YEAR, safeCastIntegral( TemporalFields.Year.name(), fields.get(TemporalFields.Year), TemporalFields.Year.defaultValue ) ).with(IsoFields.WEEK_OF_WEEK_BASED_YEAR, 1).with(ChronoField.DAY_OF_WEEK, 1);
				  }

				  result = assignAllFields( result );
				  if ( timezone != null )
				  {
						if ( ( ( selectingTime || selectingDateTime ) && selectingTimeZone ) || selectingEpoch )
						{
							 try
							 {
								  result = result.withZoneSameInstant( timezone() );
							 }
							 catch ( DateTimeParseException e )
							 {
								  throw new InvalidValuesArgumentException( e.Message, e );
							 }
						}
						else
						{
							 result = result.withZoneSameLocal( timezone() );
						}
				  }
				  return Datetime( result );
			 }

			 protected internal override DateTimeValue selectDateTime( AnyValue datetime )
			 {
				  if ( datetime is DateTimeValue )
				  {
						DateTimeValue value = ( DateTimeValue ) datetime;
						ZoneId zone = optionalTimezone();
						return zone == null ? value : new DateTimeValue( ZonedDateTime.of( value.Temporal().toLocalDateTime(), zone ) );
				  }
				  if ( datetime is LocalDateTimeValue )
				  {
						return new DateTimeValue( ZonedDateTime.of( ( ( LocalDateTimeValue ) datetime ).Temporal(), timezone() ) );
				  }
				  throw new UnsupportedTemporalUnitException( "Cannot select datetime from: " + datetime );
			 }
		 }

		 private readonly ZonedDateTime _value;
		 private readonly long _epochSeconds;

		 private DateTimeValue( ZonedDateTime value )
		 {
			  ZoneId zone = value.Zone;
			  if ( zone is ZoneOffset )
			  {
					this._value = value;
			  }
			  else
			  {
					// Do a 2-way lookup of the zone to make sure we only use the new name of renamed zones
					ZoneId mappedZone = ZoneId.of( TimeZones.Map( TimeZones.Map( zone.Id ) ) );
					this._value = value.withZoneSameInstant( mappedZone );
			  }
			  this._epochSeconds = this._value.toEpochSecond();
		 }

		 internal override ZonedDateTime Temporal()
		 {
			  return _value;
		 }

		 internal override LocalDate DatePart
		 {
			 get
			 {
				  return _value.toLocalDate();
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
			  ZoneOffset offset = _value.Offset;
			  LocalTime localTime = _value.toLocalTime();
			  return OffsetTime.of( localTime, offset );
		 }

		 internal override ZoneId GetZoneId( System.Func<ZoneId> defaultZone )
		 {
			  return _value.Zone;
		 }

		 internal override ZoneId GetZoneId()
		 {
			  return _value.Zone;
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

		 internal override bool HasTime()
		 {
			  return true;
		 }

		 public override bool Equals( Value other )
		 {
			  if ( other is DateTimeValue )
			  {
					ZonedDateTime that = ( ( DateTimeValue ) other )._value;
					bool res = _value.toLocalDateTime().Equals(that.toLocalDateTime());
					if ( res )
					{
						 ZoneId thisZone = _value.Zone;
						 ZoneId thatZone = that.Zone;
						 bool thisIsOffset = thisZone is ZoneOffset;
						 bool thatIsOffset = thatZone is ZoneOffset;
						 if ( thisIsOffset && thatIsOffset )
						 {
							  res = thisZone.Equals( thatZone );
						 }
						 else if ( !thisIsOffset && !thatIsOffset )
						 {
							  res = string.ReferenceEquals( TimeZones.map( thisZone.Id ), TimeZones.map( thatZone.Id ) );
						 }
						 else
						 {
							  res = false;
						 }
					}
					return res;

			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteDateTime( _value );
		 }

		 internal override int UnsafeCompareTo( Value other )
		 {
			  DateTimeValue that = ( DateTimeValue ) other;
			  int cmp = Long.compare( _epochSeconds, that._epochSeconds );
			  if ( cmp == 0 )
			  {
					cmp = _value.toLocalTime().Nano - that._value.toLocalTime().Nano;
					if ( cmp == 0 )
					{
						 ZoneOffset thisOffset = _value.Offset;
						 ZoneOffset thatOffset = that._value.Offset;

						 cmp = Integer.compare( thisOffset.TotalSeconds, thatOffset.TotalSeconds );
						 if ( cmp == 0 )
						 {
							  ZoneId thisZone = _value.Zone;
							  ZoneId thatZone = that._value.Zone;
							  bool thisIsOffset = thisZone is ZoneOffset;
							  bool thatIsOffset = thatZone is ZoneOffset;
							  // non-named timezone (just offset) before named-time zones, alphabetically
							  cmp = Boolean.compare( thatIsOffset, thisIsOffset );
							  if ( cmp == 0 )
							  {
									if ( !thisIsOffset ) // => also means !thatIsOffset
									{
										 cmp = CompareNamedZonesWithMapping( thisZone, thatZone );
									}
							  }
							  if ( cmp == 0 )
							  {
									cmp = _value.Chronology.compareTo( that._value.Chronology );
							  }
						 }
					}
			  }
			  return cmp;
		 }

		 private int CompareNamedZonesWithMapping( ZoneId thisZone, ZoneId thatZone )
		 {
			  string thisZoneNormalized = TimeZones.Map( TimeZones.Map( thisZone.Id ) );
			  string thatZoneNormalized = TimeZones.Map( TimeZones.Map( thatZone.Id ) );
			  return thisZoneNormalized.CompareTo( thatZoneNormalized );
		 }

		 public override string PrettyPrint()
		 {
			  return AssertPrintable( () => _value.format(DateTimeFormatter.ISO_DATE_TIME) );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.ZonedDateTime;
		 }

		 protected internal override int ComputeHash()
		 {
			  return _value.toInstant().GetHashCode();
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "DateTime";
			 }
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapDateTime( this );
		 }

		 public override DateTimeValue Add( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.plus(duration) ) );
		 }

		 public override DateTimeValue Sub( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.minus(duration) ) );
		 }

		 internal override DateTimeValue Replacement( ZonedDateTime datetime )
		 {
			  return _value == datetime ? this : new DateTimeValue( datetime );
		 }

		 private const string ZONE_NAME = "(?<zoneName>[a-zA-Z0-9~._ /+-]+)";
		 private static readonly Pattern _pattern = Pattern.compile( DATE_PATTERN + "(?<time>T" + TIME_PATTERN + "(?:\\[" + ZONE_NAME + "\\])?" + ")?", Pattern.CASE_INSENSITIVE );
		 private static readonly DateTimeFormatter _zoneNameParser = new DateTimeFormatterBuilder().parseCaseInsensitive().appendZoneRegionId().toFormatter();

		 private static DateTimeValue Parse( Matcher matcher, System.Func<ZoneId> defaultZone )
		 {
			  DateTime local = new DateTime( parseDate( matcher ), optTime( matcher ) );
			  string zoneName = matcher.group( "zoneName" );
			  ZoneOffset offset = parseOffset( matcher );
			  ZoneId zone;
			  if ( !string.ReferenceEquals( zoneName, null ) )
			  {
					zone = ParseZoneName( zoneName );
					if ( offset != null )
					{
						 ZoneOffset expected;
						 try
						 {
							  expected = zone.Rules.getOffset( local );
						 }
						 catch ( ZoneRulesException e )
						 {
							  throw new TemporalParseException( e.Message, e );
						 }
						 if ( !expected.Equals( offset ) )
						 {
							  throw new InvalidValuesArgumentException( "Timezone and offset do not match: " + matcher.group() );
						 }
					}
			  }
			  else if ( offset != null )
			  {
					zone = offset;
			  }
			  else
			  {
					zone = defaultZone();
			  }
			  return new DateTimeValue( ZonedDateTime.of( local, zone ) );
		 }

		 internal static ZoneId ParseZoneName( string zoneName )
		 {
			  ZoneId parsedName;
			  try
			  {
					parsedName = _zoneNameParser.parse( zoneName.Replace( ' ', '_' ) ).query( TemporalQueries.zoneId() );
			  }
			  catch ( DateTimeParseException e )
			  {
					throw new TemporalParseException( "Invalid value for TimeZone: " + e.Message, e.ParsedString, e.ErrorIndex, e );
			  }
			  return parsedName;
		 }

		 public static ZoneId ParseZoneOffsetOrZoneName( string zoneName )
		 {
			  Matcher matcher = OFFSET.matcher( zoneName );
			  if ( matcher.matches() )
			  {
					return parseOffset( matcher );
			  }
			  try
			  {
					return _zoneNameParser.parse( zoneName.Replace( ' ', '_' ) ).query( TemporalQueries.zoneId() );
			  }
			  catch ( DateTimeParseException e )
			  {
					throw new TemporalParseException( "Invalid value for TimeZone: " + e.Message, e.ParsedString, e.ErrorIndex, e );
			  }
		 }

		 internal abstract class DateTimeBuilder<Result> : Builder<Result>
		 {
			  internal DateTimeBuilder( System.Func<ZoneId> defaultZone ) : base( defaultZone )
			  {
			  }

			  protected internal override bool SupportsDate()
			  {
					return true;
			  }

			  protected internal override bool SupportsTime()
			  {
					return true;
			  }

			  protected internal abstract Result SelectDateTime( AnyValue date );
		 }
	}

}