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

	using Neo4Net.Values;
	using Neo4Net.Values;
	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using UnsupportedTemporalUnitException = Neo4Net.Values.utils.UnsupportedTemporalUnitException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.FeatureToggles.flag;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateTimeValue.parseZoneName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.IntegralValue.safeCastIntegral;

	public sealed class DateValue : TemporalValue<LocalDate, DateValue>
	{
		 public static readonly DateValue MinValue = new DateValue( LocalDate.MIN );
		 public static readonly DateValue MaxValue = new DateValue( LocalDate.MAX );

		 public static DateValue Date( LocalDate value )
		 {
			  return new DateValue( requireNonNull( value, "LocalDate" ) );
		 }

		 public static DateValue Date( int year, int month, int day )
		 {
			  return new DateValue( AssertValidArgument( () => LocalDate.of(year, month, day) ) );
		 }

		 public static DateValue WeekDate( int year, int week, int dayOfWeek )
		 {
			  return new DateValue( AssertValidArgument( () => LocalWeekDate(year, week, dayOfWeek) ) );
		 }

		 public static DateValue QuarterDate( int year, int quarter, int dayOfQuarter )
		 {
			  return new DateValue( AssertValidArgument( () => LocalQuarterDate(year, quarter, dayOfQuarter) ) );
		 }

		 public static DateValue OrdinalDate( int year, int dayOfYear )
		 {
			  return new DateValue( AssertValidArgument( () => LocalDate.ofYearDay(year, dayOfYear) ) );
		 }

		 public static DateValue EpochDate( long epochDay )
		 {
			  return new DateValue( EpochDateRaw( epochDay ) );
		 }

		 public static LocalDate EpochDateRaw( long epochDay )
		 {
			  return AssertValidArgument( () => LocalDate.ofEpochDay(epochDay) );
		 }

		 public static DateValue Parse( CharSequence text )
		 {
			  return Parse( typeof( DateValue ), _pattern, DateValue.parse, text );
		 }

		 public static DateValue Parse( TextValue text )
		 {
			  return Parse( typeof( DateValue ), _pattern, DateValue.parse, text );
		 }

		 public static DateValue Now( Clock clock )
		 {
			  return new DateValue( LocalDate.now( clock ) );
		 }

		 public static DateValue Now( Clock clock, string timezone )
		 {
			  return Now( clock.withZone( parseZoneName( timezone ) ) );
		 }

		 public static DateValue Now( Clock clock, System.Func<ZoneId> defaultZone )
		 {
			  return Now( clock.withZone( defaultZone() ) );
		 }

		 public static DateValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return StructureBuilder.build( Builder( defaultZone ), map );
		 }

		 public static DateValue Select( Neo4Net.Values.AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return Builder( defaultZone ).selectDate( from );
		 }

		 public static DateValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  LocalDate localDate = input.DatePart;
			  DateValue truncated = Date( TruncateTo( localDate, unit ) );
			  if ( fields.Size() == 0 )
			  {
					return truncated;
			  }
			  else
			  {
					MapValue updatedFields = fields.UpdatedWith( "date", truncated );
					return Build( updatedFields, defaultZone );
			  }
		 }

		 internal static LocalDate TruncateTo( LocalDate value, TemporalUnit unit )
		 {
			  if ( unit == ChronoUnit.MILLENNIA )
			  {
					return value.with( Neo4NetTemporalField.YearOfMillennium, 0 );
			  }
			  else if ( unit == ChronoUnit.CENTURIES )
			  {
					return value.with( Neo4NetTemporalField.YearOfCentury, 0 );
			  }
			  else if ( unit == ChronoUnit.DECADES )
			  {
					return value.with( Neo4NetTemporalField.YearOfDecade, 0 );
			  }
			  else if ( unit == ChronoUnit.YEARS )
			  {
					return value.with( TemporalAdjusters.firstDayOfYear() );
			  }
			  else if ( unit == IsoFields.WEEK_BASED_YEARS )
			  {
					return value.with( IsoFields.WEEK_OF_WEEK_BASED_YEAR, 1 ).with( ChronoField.DAY_OF_WEEK, 1 );
			  }
			  else if ( unit == IsoFields.QUARTER_YEARS )
			  {
					return value.with( IsoFields.DAY_OF_QUARTER, 1 );
			  }
			  else if ( unit == ChronoUnit.MONTHS )
			  {
					return value.with( TemporalAdjusters.firstDayOfMonth() );
			  }
			  else if ( unit == ChronoUnit.WEEKS )
			  {
					return value.with( TemporalAdjusters.previousOrSame( DayOfWeek.Monday ) );
			  }
			  else if ( unit == ChronoUnit.DAYS )
			  {
					return value;
			  }
			  else
			  {
					throw new UnsupportedTemporalUnitException( "Unit too small for truncation: " + unit );
			  }
		 }

		 internal static DateBuilder Builder( System.Func<ZoneId> defaultZone )
		 {
			  return new DateBuilder( defaultZone );
		 }

		 private readonly LocalDate _value;

		 private DateValue( LocalDate value )
		 {
			  this._value = value;
		 }

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  DateValue other = ( DateValue ) otherValue;
			  return _value.compareTo( other._value );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Date";
			 }
		 }

		 internal override LocalDate Temporal()
		 {
			  return _value;
		 }

		 internal override LocalDate DatePart
		 {
			 get
			 {
				  return _value;
			 }
		 }

		 internal override LocalTime LocalTimePart
		 {
			 get
			 {
				  throw new UnsupportedTemporalUnitException( string.Format( "Cannot get the time of: {0}", this ) );
			 }
		 }

		 internal override OffsetTime GetTimePart( System.Func<ZoneId> defaultZone )
		 {
			  throw new UnsupportedTemporalUnitException( string.Format( "Cannot get the time of: {0}", this ) );
		 }

		 internal override ZoneId GetZoneId( System.Func<ZoneId> defaultZone )
		 {
			  throw new UnsupportedTemporalUnitException( string.Format( "Cannot get the time zone of: {0}", this ) );
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
			  return false;
		 }

		 public override bool Equals( Value other )
		 {
			  return other is DateValue && _value.Equals( ( ( DateValue ) other )._value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteDate( _value );
		 }

		 public override string PrettyPrint()
		 {
			  return AssertPrintable( () => _value.format(DateTimeFormatter.ISO_DATE) );
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.Date;
		 }

		 protected internal override int ComputeHash()
		 {
			  return Long.GetHashCode( _value.toEpochDay() );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapDate( this );
		 }

		 public override DateValue Add( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.plusMonths(duration.TotalMonths()).plusDays(duration.TotalDays()) ) );
		 }

		 public override DateValue Sub( DurationValue duration )
		 {
			  return Replacement( AssertValidArithmetic( () => _value.minusMonths(duration.TotalMonths()).minusDays(duration.TotalDays()) ) );
		 }

		 internal override DateValue Replacement( LocalDate date )
		 {
			  return date == _value ? this : new DateValue( date );
		 }

		 internal static readonly bool QuarterDates = flag( typeof( DateValue ), "QUARTER_DATES", true );
		 /// <summary>
		 /// The regular expression pattern for parsing dates. All fields come in two versions - long and short form, the
		 /// long form is for formats containing dashes, the short form is for formats without dashes. The long format is
		 /// the only one that handles signed years, since that is how the only case that supports years with other than 4
		 /// numbers, and not having dashes then would make the format ambiguous. In order to not have two cases that can
		 /// parse only the year, we let the long case handle that.
		 /// <p/>
		 /// Valid formats:
		 /// <ul>
		 /// <li>Year:<ul>
		 /// <li>{@code [0-9]{4}} - unique without dashes, since it is the only one 4 numbers long<br>
		 /// Parsing: {@code longYear}</li>
		 /// <li>{@code [+-] [0-9]{1,9}}<br>
		 /// Parsing: {@code longYear}</li>
		 /// </ul></li>
		 /// <li>Year & Month:<ul>
		 /// <li>{@code [0-9]{4} [0-9]{2}} - unique without dashes, since it is the only one 6 numbers long<br>
		 /// Parsing: {@code shortYear, shortMonth}</li>
		 /// <li>{@code [0-9]{4} - [0-9]{1,2}}<br>
		 /// Parsing: {@code longYear, longMonth}</li>
		 /// <li>{@code [+-] [0-9]{1,9} - [0-9]{1,2}}<br>
		 /// Parsing: {@code longYear, longMonth}</li>
		 /// </ul></li>
		 /// <li>Calendar date (Year & Month & Day):<ul>
		 /// <li>{@code [0-9]{4} [0-9]{2} [0-9]{2}} - unique without dashes, since it is the only one 8 numbers long<br>
		 /// Parsing: {@code shortYear, shortMonth, shortDay}</li>
		 /// <li>{@code [0-9]{4} - [0-9]{1,2} - [0-9]{1,2}}<br>
		 /// Parsing: {@code longYear, longMonth, longDay}</li>
		 /// <li>{@code [+-] [0-9]{1,9} - [0-9]{1,2} - [0-9]{1,2}}<br>
		 /// Parsing: {@code longYear, longMonth, longDay}</li>
		 /// </ul></li>
		 /// <li>Year & Week:<ul>
		 /// <li>{@code [0-9]{4} W [0-9]{2}}<br>
		 /// Parsing: {@code shortYear, shortWeek}</li>
		 /// <li>{@code [0-9]{4} - W [0-9]{2}}<br>
		 /// Parsing: {@code longYear, longWeek}</li>
		 /// <li>{@code [+-] [0-9]{1,9} - W [0-9]{2}}<br>
		 /// Parsing: {@code longYear, longWeek}</li>
		 /// </ul></li>
		 /// <li>Week date (year & week & day of week):<ul>
		 /// <li>{@code [0-9]{4} W [0-9]{2} [0-9]} - unique without dashes, contains W followed by 2 numbers<br>
		 /// Parsing: {@code shortYear, shortWeek, shortDOW}</li>
		 /// <li>{@code [0-9]{4} - W [0-9]{1,2} - [0-9]}<br>
		 /// Parsing: {@code longYear, longWeek, longDOW}</li>
		 /// <li>{@code [+-] [0-9]{1,9} - W [0-9]{2} - [0-9]}<br>
		 /// Parsing: {@code longYear, longWeek, longDOW}</li>
		 /// </ul></li>
		 /// <li>Ordinal date (year & day of year):<ul>
		 /// <li>{@code [0-9]{4} [0-9]{3}} - unique without dashes, since it is the only one 7 number long<br>
		 /// Parsing: {@code shortYear, shortDOY}</li>
		 /// <li>{@code [0-9]{4} - [0-9]{3}} - needs to be exactly 3 numbers long to distinguish from Year & Month<br>
		 /// Parsing: {@code longYear, longDOY}</li>
		 /// <li>{@code [+-] [0-9]{1,9} - [0-9]{3}} - needs to be exactly 3 numbers long to distinguish from Year & Month<br>
		 /// Parsing: {@code longYear, longDOY}</li>
		 /// </ul></li>
		 /// </ul>
		 /// </summary>
		 internal const string DATE_PATTERN = "(?:"
														// short formats - without dashes:
														+ "(?<shortYear>[0-9]{4})(?:"
														+ "(?<shortMonth>[0-9]{2})(?<shortDay>[0-9]{2})?|" // calendar date
														+ "W(?<shortWeek>[0-9]{2})(?<shortDOW>[0-9])?|" // week date
														+ ( QuarterDates ? "Q(?<shortQuarter>[0-9])(?<shortDOQ>[0-9]{2})?|" : "" ) + "(?<shortDOY>[0-9]{3}))" + "|" // ordinal date
														// long formats - includes dashes:
														+ "(?<longYear>(?:[0-9]{4}|[+-][0-9]{1,9}))(?:"
														+ "-(?<longMonth>[0-9]{1,2})(?:-(?<longDay>[0-9]{1,2}))?|" // calendar date
														+ "-W(?<longWeek>[0-9]{1,2})(?:-(?<longDOW>[0-9]))?|" // week date
														+ ( QuarterDates ? "-Q(?<longQuarter>[0-9])(?:-(?<longDOQ>[0-9]{1,2}))?|" : "" ) + "-(?<longDOY>[0-9]{3}))?" + ")"; // ordinal date
		 private static final Pattern _pattern = Pattern.compile( DATE_PATTERN );

		 /// <summary>
		 /// Creates a <seealso cref="LocalDate"/> from a <seealso cref="Matcher"/> that matches the regular expression defined by
		 /// <seealso cref="DATE_PATTERN"/>. The decision tree in the implementation of this method is guided by the parsing notes
		 /// for <seealso cref="DATE_PATTERN"/>.
		 /// </summary>
		 /// <param name="matcher"> a <seealso cref="Matcher"/> that matches the regular expression defined in <seealso cref="DATE_PATTERN"/>. </param>
		 /// <returns> a <seealso cref="LocalDate"/> parsed from the given <seealso cref="Matcher"/>. </returns>
		 static LocalDate ParseDate( Matcher matcher )
		 {
			  string longYear = matcher.group( "longYear" );
			  if ( !string.ReferenceEquals( longYear, null ) )
			  {
					return Parse( matcher, parseInt( longYear ), "longMonth", "longDay", "longWeek", "longDOW", "longQuarter", "longDOQ", "longDOY" );
			  }
			  else
			  {
					return Parse( matcher, parseInt( matcher.group( "shortYear" ) ), "shortMonth", "shortDay", "shortWeek", "shortDOW", "shortQuarter", "shortDOQ", "shortDOY" );
			  }
		 }

		 private static LocalDate Parse( Matcher matcher, int year, string MONTH, string DAY, string WEEK, string DOW, string QUARTER, string DOQ, string DOY )
		 {
			  string month = matcher.group( MONTH );
			  if ( !string.ReferenceEquals( month, null ) )
			  {
					return AssertParsable( () => LocalDate.of(year, parseInt(month), OptInt(matcher.group(DAY))) );
			  }
			  string week = matcher.group( WEEK );
			  if ( !string.ReferenceEquals( week, null ) )
			  {
					return AssertParsable( () => LocalWeekDate(year, parseInt(week), OptInt(matcher.group(DOW))) );
			  }
			  string quarter = matcher.group( QUARTER );
			  if ( !string.ReferenceEquals( quarter, null ) )
			  {
					return AssertParsable( () => LocalQuarterDate(year, parseInt(quarter), OptInt(matcher.group(DOQ))) );
			  }
			  string doy = matcher.group( DOY );
			  if ( !string.ReferenceEquals( doy, null ) )
			  {
					return AssertParsable( () => LocalDate.ofYearDay(year, parseInt(doy)) );
			  }
			  return AssertParsable( () => LocalDate.of(year, 1, 1) );
		 }

		 private static DateValue Parse( Matcher matcher )
		 {
			  return new DateValue( ParseDate( matcher ) );
		 }

		 private static int OptInt( string _value )
		 {
			  return _value == null ? 1 : parseInt( _value );
		 }

		 private static LocalDate LocalWeekDate( int year, int week, int dayOfWeek )
		 {
			  LocalDate weekOne = LocalDate.of( year, 1, 4 ); // the fourth is guaranteed to be in week 1 by definition
			  LocalDate withWeek = weekOne.with( IsoFields.WEEK_OF_WEEK_BASED_YEAR, week );
			  // the implementation of WEEK_OF_WEEK_BASED_YEAR uses addition to adjust the date, this means that it accepts
			  // week 53 of years that don't have 53 weeks, so we have to guard for this:
			  if ( week == 53 && withWeek.get( IsoFields.WEEK_BASED_YEAR ) != year )
			  {
					throw new InvalidValuesArgumentException( string.Format( "Year {0:D} does not contain {1:D} weeks.", year, week ) );
			  }
			  return withWeek.with( ChronoField.DAY_OF_WEEK, dayOfWeek );
		 }

		 private static LocalDate LocalQuarterDate( int year, int quarter, int dayOfQuarter )
		 {
			  // special handling for the range of Q1 and Q2, since they are shorter than Q3 and Q4
			  if ( quarter == 2 && dayOfQuarter == 92 )
			  {
					throw new InvalidValuesArgumentException( "Quarter 2 only has 91 days." );
			  }
			  // instantiate the yearDate now, because we use it to know if it is a leap year
			  LocalDate yearDate = LocalDate.ofYearDay( year, dayOfQuarter ); // guess on the day
			  if ( quarter == 1 && dayOfQuarter > 90 && ( !yearDate.LeapYear || dayOfQuarter == 92 ) )
			  {
					throw new InvalidValuesArgumentException( string.Format( "Quarter 1 of {0:D} only has {1:D} days.", year, yearDate.LeapYear ? 91 : 90 ) );
			  }
			  return yearDate.with( IsoFields.QUARTER_OF_YEAR, quarter ).with( IsoFields.DAY_OF_QUARTER, dayOfQuarter );
		 }

		 static final LocalDate DefaultCalenderDate = LocalDate.of( TemporalFields.Year.defaultValue, TemporalFields.Month.defaultValue, TemporalFields.Day.defaultValue );

		 private static class DateBuilder extends Builder<DateValue>
		 {
			  protected bool SupportsTimeZone()
			  {
					return false;
			  }

			  protected bool supportsEpoch()
			  {
					return false;
			  }

			  DateBuilder( System.Func<ZoneId> defaultZone )
			  {
					base( defaultZone );
			  }

			  protected final bool supportsDate()
			  {
					return true;
			  }

			  protected final bool supportsTime()
			  {
					return false;
			  }

			  private LocalDate getDateOf( Neo4Net.Values.AnyValue temporal )
			  {
					if ( temporal is TemporalValue )
					{
						 TemporalValue v = ( TemporalValue ) temporal;
						 return v.DatePart;
					}
					throw new InvalidValuesArgumentException( string.Format( "Cannot construct date from: {0}", temporal ) );
			  }

			  public DateValue buildInternal()
			  {
					LocalDate result;
					if ( fields.containsKey( TemporalFields.Date ) )
					{
						 result = getDateOf( fields.get( TemporalFields.Date ) );
					}
					else if ( fields.containsKey( TemporalFields.Week ) )
					{
						 // Be sure to be in the start of the week based year (which can be later than 1st Jan)
						 result = DefaultCalenderDate.with( IsoFields.WEEK_BASED_YEAR, safeCastIntegral( TemporalFields.Year.name(), fields.get(TemporalFields.Year), TemporalFields.Year.defaultValue ) ).with(IsoFields.WEEK_OF_WEEK_BASED_YEAR, 1).with(ChronoField.DAY_OF_WEEK, 1);
					}
					else
					{
						 result = DefaultCalenderDate;
					}
					result = assignAllFields( result );
					return Date( result );
			  }

			  DateValue selectDate( Neo4Net.Values.AnyValue date )
			  {
					if ( date is DateValue )
					{
						 return ( DateValue ) date;
					}
					return Date( getDateOf( Date ) );
			  }
		 }
	}

}