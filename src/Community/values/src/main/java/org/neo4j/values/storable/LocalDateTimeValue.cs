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
	using UnsupportedTemporalUnitException = Neo4Net.Values.utils.UnsupportedTemporalUnitException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.parseZoneName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.DATE_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.parseDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.IntegralValue.safeCastIntegral;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.TIME_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.parseTime;

	public sealed class LocalDateTimeValue : TemporalValue<DateTime, LocalDateTimeValue>
	{
		 public static readonly LocalDateTimeValue MinValue = new LocalDateTimeValue( DateTime.MinValue );
		 public static readonly LocalDateTimeValue MaxValue = new LocalDateTimeValue( DateTime.MaxValue );

		 public static LocalDateTimeValue LocalDateTime( DateValue date, LocalTimeValue time )
		 {
			  return new LocalDateTimeValue( new DateTime( date.Temporal(), time.Temporal() ) );
		 }

		 public static LocalDateTimeValue LocalDateTime( int year, int month, int day, int hour, int minute, int second, int nanoOfSecond )
		 {
			  return new LocalDateTimeValue( AssertValidArgument( () => new DateTime(year, month, day, hour, minute, second, nanoOfSecond) ) );
		 }

		 public static LocalDateTimeValue LocalDateTime( DateTime value )
		 {
			  return new LocalDateTimeValue( requireNonNull( value, "LocalDateTime" ) );
		 }

		 public static LocalDateTimeValue LocalDateTime( long epochSecond, long nano )
		 {
			  return new LocalDateTimeValue( LocalDateTimeRaw( epochSecond, nano ) );
		 }

		 public static DateTime LocalDateTimeRaw( long epochSecond, long nano )
		 {
			  return AssertValidArgument( () => ofInstant(ofEpochSecond(epochSecond, nano), UTC) );
		 }

		 public static LocalDateTimeValue Parse( CharSequence text )
		 {
			  return Parse( typeof( LocalDateTimeValue ), _pattern, LocalDateTimeValue.parse, text );
		 }

		 public static LocalDateTimeValue Parse( TextValue text )
		 {
			  return Parse( typeof( LocalDateTimeValue ), _pattern, LocalDateTimeValue.parse, text );
		 }

		 public static LocalDateTimeValue Now( Clock clock )
		 {
			  return new LocalDateTimeValue( DateTime.now( clock ) );
		 }

		 public static LocalDateTimeValue Now( Clock clock, string timezone )
		 {
			  return Now( clock.withZone( parseZoneName( timezone ) ) );
		 }

		 public static LocalDateTimeValue Now( Clock clock, System.Func<ZoneId> defaultZone )
		 {
			  return Now( clock.withZone( defaultZone() ) );
		 }

		 public static LocalDateTimeValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return StructureBuilder.build( Builder( defaultZone ), map );
		 }

		 public static LocalDateTimeValue Select( AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return Builder( defaultZone ).selectDateTime( from );
		 }

		 public static LocalDateTimeValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  Pair<LocalDate, LocalTime> pair = GetTruncatedDateAndTime( unit, input, "local date time" );

			  LocalDate truncatedDate = pair.First();
			  LocalTime truncatedTime = pair.Other();

			  DateTime truncatedLDT = new DateTime( truncatedDate, truncatedTime );

			  if ( fields.Size() == 0 )
			  {
					return LocalDateTime( truncatedLDT );
			  }
			  else
			  {
					return UpdateFieldMapWithConflictingSubseconds(fields, unit, truncatedLDT, (mapValue, localDateTime) =>
					{
					if ( mapValue.size() == 0 )
					{
						return LocalDateTime( LocalDateTime );
					}
					else
					{
						return Build( mapValue.updatedWith( "datetime", LocalDateTime( LocalDateTime ) ), defaultZone );
					}
					});
			  }
		 }

		 internal static readonly DateTime DefaultLocalDateTime = new DateTime( TemporalFields.Year.defaultValue, TemporalFields.Month.defaultValue, TemporalFields.Day.defaultValue, TemporalFields.Hour.defaultValue, TemporalFields.Minute.defaultValue );

		 internal static DateTimeValue.DateTimeBuilder<LocalDateTimeValue> Builder( System.Func<ZoneId> defaultZone )
		 {
			  return new DateTimeBuilderAnonymousInnerClass( defaultZone );
		 }

		 private class DateTimeBuilderAnonymousInnerClass : DateTimeValue.DateTimeBuilder<LocalDateTimeValue>
		 {
			 public DateTimeBuilderAnonymousInnerClass( System.Func<ZoneId> defaultZone ) : base( defaultZone )
			 {
			 }

			 protected internal override bool supportsTimeZone()
			 {
				  return false;
			 }

			 protected internal override bool supportsEpoch()
			 {
				  return false;
			 }

			 public override LocalDateTimeValue buildInternal()
			 {
				  bool selectingDate = fields.containsKey( TemporalFields.Date );
				  bool selectingTime = fields.containsKey( TemporalFields.Time );
				  bool selectingDateTime = fields.containsKey( TemporalFields.Datetime );
				  DateTime result;
				  if ( selectingDateTime )
				  {
						AnyValue dtField = fields.get( TemporalFields.Datetime );
						if ( !( dtField is TemporalValue ) )
						{
							 throw new InvalidValuesArgumentException( string.Format( "Cannot construct local date time from: {0}", dtField ) );
						}
						TemporalValue dt = ( TemporalValue ) dtField;
						result = new DateTime( dt.DatePart, dt.LocalTimePart );
				  }
				  else if ( selectingTime || selectingDate )
				  {
						LocalTime time;
						if ( selectingTime )
						{
							 AnyValue timeField = fields.get( TemporalFields.Time );
							 if ( !( timeField is TemporalValue ) )
							 {
								  throw new InvalidValuesArgumentException( string.Format( "Cannot construct local time from: {0}", timeField ) );
							 }
							 TemporalValue t = ( TemporalValue ) timeField;
							 time = t.LocalTimePart;
						}
						else
						{
							 time = LocalTimeValue.DefaultLocalTime;
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

						result = new DateTime( date, time );
				  }
				  else
				  {
						result = DefaultLocalDateTime;
				  }

				  if ( fields.containsKey( TemporalFields.Week ) && !selectingDate && !selectingDateTime )
				  {
						// Be sure to be in the start of the week based year (which can be later than 1st Jan)
						result = result.with( IsoFields.WEEK_BASED_YEAR, safeCastIntegral( TemporalFields.Year.name(), fields.get(TemporalFields.Year), TemporalFields.Year.defaultValue ) ).with(IsoFields.WEEK_OF_WEEK_BASED_YEAR, 1).with(ChronoField.DAY_OF_WEEK, 1);
				  }

				  result = assignAllFields( result );
				  return LocalDateTime( result );
			 }

			 private DateTime getLocalDateTimeOf( AnyValue temporal )
			 {
				  if ( temporal is TemporalValue )
				  {
						TemporalValue v = ( TemporalValue ) temporal;
						LocalDate datePart = v.DatePart;
						LocalTime timePart = v.LocalTimePart;
						return new DateTime( datePart, timePart );
				  }
				  throw new InvalidValuesArgumentException( string.Format( "Cannot construct date from: {0}", temporal ) );
			 }

			 protected internal override LocalDateTimeValue selectDateTime( AnyValue datetime )
			 {
				  if ( datetime is LocalDateTimeValue )
				  {
						return ( LocalDateTimeValue ) datetime;
				  }
				  return LocalDateTime( getLocalDateTimeOf( datetime ) );
			 }
		 }

		 private readonly DateTime _value;
		 private readonly long _epochSecondsInUTC;

		 private LocalDateTimeValue( DateTime value )
		 {
			  this._value = value;
			  this._epochSecondsInUTC = this._value.toEpochSecond( UTC );
		 }

		 internal override int UnsafeCompareTo( Value other )
		 {
			  LocalDateTimeValue that = ( LocalDateTimeValue ) other;
			  int cmp = Long.compare( _epochSecondsInUTC, that._epochSecondsInUTC );
			  if ( cmp == 0 )
			  {
					cmp = _value.Nano - that._value.Nano;
			  }
			  return cmp;
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "LocalDateTime";
			 }
		 }

		 internal override DateTime Temporal()
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
			  ZoneOffset currentOffset = AssertValidArgument( () => ZonedDateTime.ofInstant(Instant.now(), defaultZone()) ).Offset;
			  return OffsetTime.of( _value.toLocalTime(), currentOffset );
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
			  return other is LocalDateTimeValue && _value.Equals( ( ( LocalDateTimeValue ) other )._value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteLocalDateTime( _value );
		 }

		 public override string PrettyPrint()
		 {
			  return AssertPrintable( () => _value.format(DateTimeFormatter.ISO_LOCAL_DATE_TIME) );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.LocalDateTime;
		 }

		 protected internal override int ComputeHash()
		 {
			  return _value.toInstant( UTC ).GetHashCode();
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapLocalDateTime( this );
		 }

		 public override LocalDateTimeValue Add( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.plus(duration) ) );
		 }

		 public override LocalDateTimeValue Sub( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.minus(duration) ) );
		 }

		 internal override LocalDateTimeValue Replacement( DateTime dateTime )
		 {
			  return dateTime == _value ? this : new LocalDateTimeValue( dateTime );
		 }

		 private static readonly Pattern _pattern = Pattern.compile( DATE_PATTERN + "(?<time>T" + TIME_PATTERN + ")?", Pattern.CASE_INSENSITIVE );

		 private static LocalDateTimeValue Parse( Matcher matcher )
		 {
			  return LocalDateTime( new DateTime( parseDate( matcher ), OptTime( matcher ) ) );
		 }

		 internal static LocalTime OptTime( Matcher matcher )
		 {
			  return matcher.group( "time" ) != null ? parseTime( matcher ) : LocalTime.MIN;
		 }
	}

}