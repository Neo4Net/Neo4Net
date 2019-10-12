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
namespace Org.Neo4j.Helpers
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class TestFormat
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void dateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DateTime()
		 {
			  // Date
			  long timeWithDate = DateTimeHelper.CurrentUnixTimeMillis();
			  string dateAsString = Format.Date( timeWithDate );
			  assertEquals( timeWithDate, ( new SimpleDateFormat( Format.DATE_FORMAT ) ).parse( dateAsString ).Time );

			  // Time
			  string timeAsString = Format.Time( timeWithDate );
			  assertEquals( timeWithDate, TranslateToDate( timeWithDate, ( new SimpleDateFormat( Format.TIME_FORMAT ) ).parse( timeAsString ).Time, Format.DefaultTimeZone ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void dateTimeWithTimeZone() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DateTimeWithTimeZone()
		 {
			  string zoneOffset = "+03:00";
			  TimeZone zone = TimeZone.getTimeZone( "GMT" + zoneOffset );

			  // Date
			  string asString = Format.Date( zone );
			  assertTrue( asString.EndsWith( WithoutColon( zoneOffset ), StringComparison.Ordinal ) );
			  long timeWithDate = ( new SimpleDateFormat( Format.DATE_FORMAT ) ).parse( asString ).Time;

			  asString = Format.Date( timeWithDate, zone );
			  assertTrue( asString.EndsWith( WithoutColon( zoneOffset ), StringComparison.Ordinal ) );
			  assertEquals( timeWithDate, ( new SimpleDateFormat( Format.DATE_FORMAT ) ).parse( asString ).Time );

			  asString = Format.Date( new DateTime( timeWithDate ), zone );
			  assertTrue( asString.EndsWith( WithoutColon( zoneOffset ), StringComparison.Ordinal ) );
			  assertEquals( timeWithDate, ( new SimpleDateFormat( Format.DATE_FORMAT ) ).parse( asString ).Time );

			  // Time
			  asString = Format.Time( timeWithDate, zone );
			  assertEquals( timeWithDate, TranslateToDate( timeWithDate, ( new SimpleDateFormat( Format.TIME_FORMAT ) ).parse( asString ).Time, zone ) );

			  asString = Format.Time( new DateTime( timeWithDate ), zone );
			  assertEquals( timeWithDate, TranslateToDate( timeWithDate, ( new SimpleDateFormat( Format.TIME_FORMAT ) ).parse( asString ).Time, zone ) );
		 }

		 private static long TranslateToDate( long timeWithDate, long time, TimeZone timeIsGivenInThisTimeZone )
		 {
			  DateTime calendar = DateTime.getInstance( timeIsGivenInThisTimeZone );
			  calendar.TimeInMillis = timeWithDate;

			  DateTime timeCalendar = new DateTime();
			  timeCalendar.TimeInMillis = time;
			  timeCalendar.TimeZone = timeIsGivenInThisTimeZone;
			  timeCalendar.set( DateTime.YEAR, calendar.Year );
			  timeCalendar.set( DateTime.MONTH, calendar.Month );
			  bool crossedDayBoundary = !timeIsGivenInThisTimeZone.Equals( Format.DefaultTimeZone ) && timeCalendar.Hour < calendar.Hour;
			  timeCalendar.set( DateTime.DAY_OF_MONTH, calendar.Day + ( crossedDayBoundary ? 1 : 0 ) );
			  return timeCalendar.Ticks;
		 }

		 private static string WithoutColon( string zoneOffset )
		 {
			  return zoneOffset.replaceAll( ":", "" );
		 }
	}

}