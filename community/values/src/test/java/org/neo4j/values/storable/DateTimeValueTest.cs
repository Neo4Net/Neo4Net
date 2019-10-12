using System;
using System.Collections.Generic;

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
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using InvalidValuesArgumentException = Org.Neo4j.Values.utils.InvalidValuesArgumentException;
	using TemporalParseException = Org.Neo4j.Values.utils.TemporalParseException;
	using UnsupportedTemporalUnitException = Org.Neo4j.Values.utils.UnsupportedTemporalUnitException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.StringStartsWith.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.AssertingStructureBuilder.asserting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.builder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.FrozenClockRule.assertEqualTemporal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.InputMappingStructureBuilder.fromValues;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValueTest.inUTC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValueTest.orFail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertNotEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertThrows;

	public class DateTimeValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final FrozenClockRule clock = new FrozenClockRule();
		 public readonly FrozenClockRule Clock = new FrozenClockRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseDateTime()
		 public virtual void ShouldParseDateTime()
		 {
			  assertEquals( datetime( date( 2017, 12, 17 ), time( 17, 14, 35, 123456789, UTC ) ), parse( "2017-12-17T17:14:35.123456789", inUTC ) );
			  assertEquals( datetime( date( 2017, 12, 17 ), time( 17, 14, 35, 123456789, UTC ) ), parse( "2017-12-17T17:14:35.123456789Z", orFail ) );
			  assertEquals( datetime( date( 2017, 12, 17 ), time( 17, 14, 35, 123456789, UTC ) ), parse( "2017-12-17T17:14:35.123456789+0000", orFail ) );
			  assertEquals( datetime( date( 10000, 12, 17 ), time( 17, 14, 35, 123456789, UTC ) ), parse( "+10000-12-17T17:14:35.123456789+0000", orFail ) );
			  assertEquals( datetime( date( -1, 12, 17 ), time( 17, 14, 35, 123456789, UTC ) ), parse( "-1-12-17T17:14:35.123456789+0000", orFail ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldSupportLeapSeconds()
		 public virtual void ShouldSupportLeapSeconds()
		 {
			  // Leap second according to https://www.timeanddate.com/time/leap-seconds-future.html
			  assertEquals( datetime( 2016, 12, 31, 23, 59, 60, 0, UTC ), parse( "2016-12-31T23:59:60Z", orFail ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectInvalidDateTimeString()
		 public virtual void ShouldRejectInvalidDateTimeString()
		 {
			  // Wrong year
			  assertThrows( typeof( TemporalParseException ), () => parse("10000-12-17T17:14:35", inUTC) );
			  assertThrows( typeof( TemporalParseException ), () => parse("10000-12-17T17:14:35Z", orFail) );

			  // Wrong month
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-13-17T17:14:35", inUTC) ).Message, startsWith("Invalid value for MonthOfYear") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-00-17T17:14:35", inUTC) ).Message, startsWith("Invalid value for MonthOfYear") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-13-17T17:14:35Z", orFail) ).Message, startsWith("Invalid value for MonthOfYear") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-00-17T17:14:35Z", orFail) ).Message, startsWith("Invalid value for MonthOfYear") );

			  // Wrong day of month
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-32T17:14:35", inUTC) ).Message, startsWith("Invalid value for DayOfMonth") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-00T17:14:35", inUTC) ).Message, startsWith("Invalid value for DayOfMonth") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-32T17:14:35Z", orFail) ).Message, startsWith("Invalid value for DayOfMonth") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-00T17:14:35Z", orFail) ).Message, startsWith("Invalid value for DayOfMonth") );

			  // Wrong hour
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-17T24:14:35", inUTC) ).Message, startsWith("Invalid value for HourOfDay") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-17T24:14:35Z", orFail) ).Message, startsWith("Invalid value for HourOfDay") );

			  // Wrong minute
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-17T17:60:35", inUTC) ).Message, startsWith("Invalid value for MinuteOfHour") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-17T17:60:35Z", orFail) ).Message, startsWith("Invalid value for MinuteOfHour") );

			  // Wrong second
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-17T17:14:61", inUTC) ).Message, startsWith("Invalid value for SecondOfMinute") );
			  assertThat( assertThrows( typeof( TemporalParseException ), () => parse("2017-12-17T17:14:61Z", orFail) ).Message, startsWith("Invalid value for SecondOfMinute") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteDateTime()
		 public virtual void ShouldWriteDateTime()
		 {
			  // given
			  foreach ( DateTimeValue value in new DateTimeValue[] { datetime( date( 2017, 3, 26 ), localTime( 1, 0, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 3, 26 ), localTime( 2, 0, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 3, 26 ), localTime( 3, 0, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 10, 29 ), localTime( 2, 0, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 10, 29 ), localTime( 3, 0, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 10, 29 ), localTime( 4, 0, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ) } )
			  {
					IList<DateTimeValue> values = new List<DateTimeValue>( 1 );
					ValueWriter<Exception> writer = new AssertOnlyAnonymousInnerClass( this, values );

					// when
					value.WriteTo( writer );

					// then
					assertEquals( singletonList( value ), values );
			  }
		 }

		 private class AssertOnlyAnonymousInnerClass : ThrowingValueWriter.AssertOnly
		 {
			 private readonly DateTimeValueTest _outerInstance;

			 private IList<DateTimeValue> _values;

			 public AssertOnlyAnonymousInnerClass( DateTimeValueTest outerInstance, IList<DateTimeValue> values )
			 {
				 this.outerInstance = outerInstance;
				 this._values = values;
			 }

			 public override void writeDateTime( ZonedDateTime zonedDateTime )
			 {
				  _values.Add( datetime( zonedDateTime ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @FrozenClockRule.TimeZone("Europe/Stockholm") public void shouldAcquireCurrentDateTime()
		 public virtual void ShouldAcquireCurrentDateTime()
		 {
			  assertEqualTemporal( datetime( ZonedDateTime.now( Clock ) ), DateTimeValue.Now( Clock ) );

			  assertEqualTemporal( datetime( ZonedDateTime.now( Clock.withZone( "UTC" ) ) ), DateTimeValue.now( Clock, "UTC" ) );

			  assertEqualTemporal( datetime( ZonedDateTime.now( Clock.withZone( UTC ) ) ), DateTimeValue.now( Clock, "Z" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @FrozenClockRule.TimeZone({"Europe/Stockholm", "America/Los_Angeles"}) public void shouldCopyDateTime()
		 public virtual void ShouldCopyDateTime()
		 {
			  assertEqualTemporal( datetime( ZonedDateTime.now( Clock ) ), builder( Clock ).add( "datetime", datetime( ZonedDateTime.now( Clock ) ) ).build() );
			  assertEqualTemporal( datetime( ZonedDateTime.now( Clock ) ), builder( Clock ).add( "datetime", localDateTime( DateTime.now( Clock ) ) ).build() );
			  assertEqualTemporal( datetime( ZonedDateTime.now( Clock ).withZoneSameLocal( ZoneId.of( "America/New_York" ) ) ), builder( Clock ).add( "datetime", localDateTime( DateTime.now( Clock ) ) ).add( "timezone", stringValue( "America/New_York" ) ).build() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @FrozenClockRule.TimeZone("Europe/Stockholm") public void shouldConstructDateTimeFromComponents()
		 public virtual void ShouldConstructDateTimeFromComponents()
		 {
			  assertEqualTemporal( parse( "2018-01-10T10:35:57", Clock.getZone ), fromValues( builder( Clock ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).build() );
			  assertEqualTemporal( parse( "2018-01-10T10:35:57.999999999", Clock.getZone ), fromValues( builder( Clock ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "nanosecond", 999999999 ).build() );
			  assertEqualTemporal( parse( "2018-01-10T10:35:57.999999", Clock.getZone ), fromValues( builder( Clock ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "microsecond", 999999 ).build() );
			  assertEqualTemporal( parse( "2018-01-10T10:35:57.999", Clock.getZone ), fromValues( builder( Clock ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 999 ).build() );
			  assertEqualTemporal( parse( "2018-01-10T10:35:57.001999999", Clock.getZone ), fromValues( builder( Clock ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 1 ).add( "microsecond", 999 ).add( "nanosecond", 999 ).build() );
			  assertEqualTemporal( parse( "2018-01-10T10:35:57.000001999", Clock.getZone ), fromValues( builder( Clock ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "microsecond", 1 ).add( "nanosecond", 999 ).build() );
			  assertEqualTemporal( parse( "2018-01-10T10:35:57.001999", Clock.getZone ), fromValues( builder( Clock ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 1 ).add( "microsecond", 999 ).build() );
			  assertEqualTemporal( parse( "2018-01-10T10:35:57.001999999", Clock.getZone ), fromValues( builder( Clock ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 1 ).add( "microsecond", 999 ).add( "nanosecond", 999 ).build() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectInvalidFieldCombinations()
		 public virtual void ShouldRejectInvalidFieldCombinations()
		 {
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 12 ).add( "dayOfWeek", 5 ).assertThrows( typeof( UnsupportedTemporalUnitException ), "Cannot assign dayOfWeek to calendar date." );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "week", 12 ).add( "day", 12 ).assertThrows( typeof( UnsupportedTemporalUnitException ), "Cannot assign day to week date." );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "ordinalDay", 12 ).add( "dayOfWeek", 1 ).assertThrows( typeof( UnsupportedTemporalUnitException ), "Cannot assign dayOfWeek to ordinal date." );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "nanosecond", 1000000000 ).assertThrows( typeof( InvalidValuesArgumentException ), "Invalid value for Nanosecond: 1000000000" );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "microsecond", 1000000 ).assertThrows( typeof( InvalidValuesArgumentException ), "Invalid value for Microsecond: 1000000" );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 1000 ).assertThrows( typeof( InvalidValuesArgumentException ), "Invalid value for Millisecond: 1000" );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 1 ).add( "nanosecond", 1000000 ).assertThrows( typeof( InvalidValuesArgumentException ), "Invalid value for Nanosecond: 1000000" );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "microsecond", 1 ).add( "nanosecond", 1000 ).assertThrows( typeof( InvalidValuesArgumentException ), "Invalid value for Nanosecond: 1000" );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 1 ).add( "microsecond", 1000 ).assertThrows( typeof( InvalidValuesArgumentException ), "Invalid value for Microsecond: 1000" );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 1 ).add( "microsecond", 1000 ).add( "nanosecond", 999 ).assertThrows( typeof( InvalidValuesArgumentException ), "Invalid value for Microsecond: 1000" );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 1 ).add( "day", 10 ).add( "hour", 10 ).add( "minute", 35 ).add( "second", 57 ).add( "millisecond", 1 ).add( "microsecond", 999 ).add( "nanosecond", 1000 ).assertThrows( typeof( InvalidValuesArgumentException ), "Invalid value for Nanosecond: 1000" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectInvalidComponentValues()
		 public virtual void ShouldRejectInvalidComponentValues()
		 {
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "moment", 12 ).assertThrows( typeof( InvalidValuesArgumentException ), "No such field: moment" );
			  asserting( fromValues( builder( Clock ) ) ).add( "year", 2018 ).add( "month", 12 ).add( "day", 5 ).add( "hour", 5 ).add( "minute", 5 ).add( "second", 5 ).add( "picosecond", 12 ).assertThrows( typeof( InvalidValuesArgumentException ), "No such field: picosecond" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddDurationToDateTimes()
		 public virtual void ShouldAddDurationToDateTimes()
		 {
			  assertEquals( datetime( date( 2018, 2, 1 ), time( 1, 17, 3, 0, UTC ) ), datetime( date( 2018, 1, 1 ), time( 1, 2, 3, 0, UTC ) ).add( DurationValue.Duration( 1, 0, 900, 0 ) ) );
			  assertEquals( datetime( date( 2018, 2, 28 ), time( 0, 0, 0, 0, UTC ) ), datetime( date( 2018, 1, 31 ), time( 0, 0, 0, 0, UTC ) ).add( DurationValue.Duration( 1, 0, 0, 0 ) ) );
			  assertEquals( datetime( date( 2018, 1, 28 ), time( 0, 0, 0, 0, UTC ) ), datetime( date( 2018, 2, 28 ), time( 0, 0, 0, 0, UTC ) ).add( DurationValue.Duration( -1, 0, 0, 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseInstanceInArithmetics()
		 public virtual void ShouldReuseInstanceInArithmetics()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DateTimeValue datetime = datetime(date(2018, 2, 1), time(1, 17, 3, 0, UTC));
			  DateTimeValue datetime = datetime( date( 2018, 2, 1 ), time( 1, 17, 3, 0, UTC ) );
			  assertSame( datetime, datetime.Add( DurationValue.Duration( 0, 0, 0, 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSubtractDurationFromDateTimes()
		 public virtual void ShouldSubtractDurationFromDateTimes()
		 {
			  assertEquals( datetime( date( 2018, 1, 1 ), time( 1, 2, 3, 0, UTC ) ), datetime( date( 2018, 2, 1 ), time( 1, 17, 3, 0, UTC ) ).sub( DurationValue.Duration( 1, 0, 900, 0 ) ) );
			  assertEquals( datetime( date( 2018, 1, 28 ), time( 0, 0, 0, 0, UTC ) ), datetime( date( 2018, 2, 28 ), time( 0, 0, 0, 0, UTC ) ).sub( DurationValue.Duration( 1, 0, 0, 0 ) ) );
			  assertEquals( datetime( date( 2018, 2, 28 ), time( 0, 0, 0, 0, UTC ) ), datetime( date( 2018, 1, 31 ), time( 0, 0, 0, 0, UTC ) ).sub( DurationValue.Duration( -1, 0, 0, 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEqualItself()
		 public virtual void ShouldEqualItself()
		 {
			  assertEqual( datetime( 10000, 100, UTC ), datetime( 10000, 100, UTC ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldEqualRenamedTimeZone()
		 public virtual void ShouldEqualRenamedTimeZone()
		 {
			  assertEqual( datetime( 10000, 100, ZoneId.of( "Canada/Saskatchewan" ) ), datetime( 10000, 100, ZoneId.of( "Canada/East-Saskatchewan" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotEqualSameInstantButDifferentTimezone()
		 public virtual void ShouldNotEqualSameInstantButDifferentTimezone()
		 {
			  assertNotEqual( datetime( 10000, 100, UTC ), datetime( 10000, 100, ZoneOffset.of( "+01:00" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotEqualSameInstantInSameLocalTimeButDifferentTimezone()
		 public virtual void ShouldNotEqualSameInstantInSameLocalTimeButDifferentTimezone()
		 {
			  assertNotEqual( datetime( 2018, 1, 31, 10, 52, 5, 6, UTC ), datetime( 2018, 1, 31, 11, 52, 5, 6, "+01:00" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotEqualSameInstantButDifferentTimezoneWithSameOffset()
		 public virtual void ShouldNotEqualSameInstantButDifferentTimezoneWithSameOffset()
		 {
			  assertNotEqual( datetime( 1969, 12, 31, 23, 59, 59, 0, UTC ), datetime( 1969, 12, 31, 23, 59, 59, 0, "Africa/Freetown" ) );
		 }
	}

}