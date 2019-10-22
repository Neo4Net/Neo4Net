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
	using Test = org.junit.jupiter.api.Test;


	using TemporalParseException = Neo4Net.Values.utils.TemporalParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.TimeValue.parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.AnyValueTestUtil.assertNotEqual;

	internal class TimeValueTest
	{
		 internal static readonly System.Func<ZoneId> InUTC = () => UTC;
		 internal static readonly System.Func<ZoneId> OrFail = () =>
		 {
		  throw new AssertionError( "should not request timezone" );
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTimeWithOnlyHour()
		 internal virtual void ShouldParseTimeWithOnlyHour()
		 {
			  assertEquals( time( 14, 0, 0, 0, UTC ), parse( "14", InUTC ) );
			  assertEquals( time( 4, 0, 0, 0, UTC ), parse( "4", InUTC ) );
			  assertEquals( time( 4, 0, 0, 0, UTC ), parse( "04", InUTC ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTimeWithHourAndMinute()
		 internal virtual void ShouldParseTimeWithHourAndMinute()
		 {
			  assertEquals( time( 14, 5, 0, 0, UTC ), parse( "1405", InUTC ) );
			  assertEquals( time( 14, 5, 0, 0, UTC ), parse( "14:5", InUTC ) );
			  assertEquals( time( 4, 15, 0, 0, UTC ), parse( "4:15", InUTC ) );
			  assertEquals( time( 9, 7, 0, 0, UTC ), parse( "9:7", InUTC ) );
			  assertEquals( time( 3, 4, 0, 0, UTC ), parse( "03:04", InUTC ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTimeWithHourMinuteAndSecond()
		 internal virtual void ShouldParseTimeWithHourMinuteAndSecond()
		 {
			  assertEquals( time( 14, 5, 17, 0, UTC ), parse( "140517", InUTC ) );
			  assertEquals( time( 14, 5, 17, 0, UTC ), parse( "14:5:17", InUTC ) );
			  assertEquals( time( 4, 15, 4, 0, UTC ), parse( "4:15:4", InUTC ) );
			  assertEquals( time( 9, 7, 19, 0, UTC ), parse( "9:7:19", InUTC ) );
			  assertEquals( time( 3, 4, 1, 0, UTC ), parse( "03:04:01", InUTC ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTimeWithHourMinuteSecondAndFractions()
		 internal virtual void ShouldParseTimeWithHourMinuteSecondAndFractions()
		 {
			  assertEquals( time( 14, 5, 17, 123000000, UTC ), parse( "140517.123", InUTC ) );
			  assertEquals( time( 14, 5, 17, 1, UTC ), parse( "14:5:17.000000001", InUTC ) );
			  assertEquals( time( 4, 15, 4, 0, UTC ), parse( "4:15:4.000", InUTC ) );
			  assertEquals( time( 9, 7, 19, 999999999, UTC ), parse( "9:7:19.999999999", InUTC ) );
			  assertEquals( time( 3, 4, 1, 123456789, UTC ), parse( "03:04:01.123456789", InUTC ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("ThrowableNotThrown") void shouldFailToParseTimeOutOfRange()
		 internal virtual void ShouldFailToParseTimeOutOfRange()
		 {
			  assertThrows( typeof( TemporalParseException ), () => parse("24", InUTC) );
			  assertThrows( typeof( TemporalParseException ), () => parse("1760", InUTC) );
			  assertThrows( typeof( TemporalParseException ), () => parse("173260", InUTC) );
			  assertThrows( typeof( TemporalParseException ), () => parse("173250.0000000001", InUTC) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteTime()
		 internal virtual void ShouldWriteTime()
		 {
			  // given
			  foreach ( TimeValue time in new TimeValue[] { time( 11, 30, 4, 112233440, ofHours( 3 ) ), time( 23, 59, 59, 999999999, ofHours( 18 ) ), time( 23, 59, 59, 999999999, ofHours( -18 ) ), time( 0, 0, 0, 0, ofHours( -18 ) ), time( 0, 0, 0, 0, ofHours( 18 ) ) } )
			  {
					IList<TimeValue> values = new List<TimeValue>( 1 );
					ValueWriter<Exception> writer = new AssertOnlyAnonymousInnerClass( this, values );

					// when
					time.WriteTo( writer );

					// then
					assertEquals( singletonList( time ), values );
			  }
		 }

		 private class AssertOnlyAnonymousInnerClass : ThrowingValueWriter.AssertOnly
		 {
			 private readonly TimeValueTest _outerInstance;

			 private IList<TimeValue> _values;

			 public AssertOnlyAnonymousInnerClass( TimeValueTest outerInstance, IList<TimeValue> values )
			 {
				 this.outerInstance = outerInstance;
				 this._values = values;
			 }

			 public override void writeTime( OffsetTime offsetTime )
			 {
				  _values.Add( time( offsetTime ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddDurationToTimes()
		 internal virtual void ShouldAddDurationToTimes()
		 {
			  assertEquals( time( 12, 15, 0, 0, UTC ), time( 12, 0, 0, 0, UTC ).add( DurationValue.Duration( 1, 1, 900, 0 ) ) );
			  assertEquals( time( 12, 0, 2, 0, UTC ), time( 12, 0, 0, 0, UTC ).add( DurationValue.Duration( 0, 0, 1, 1_000_000_000 ) ) );
			  assertEquals( time( 12, 0, 0, 0, UTC ), time( 12, 0, 0, 0, UTC ).add( DurationValue.Duration( 0, 0, 1, -1_000_000_000 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReuseInstanceInArithmetics()
		 internal virtual void ShouldReuseInstanceInArithmetics()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TimeValue noon = time(12, 0, 0, 0, UTC);
			  TimeValue noon = time( 12, 0, 0, 0, UTC );
			  assertSame( noon, noon.Add( DurationValue.Duration( 0, 0, 0, 0 ) ) );
			  assertSame( noon, noon.Add( DurationValue.Duration( 1, 1, 0, 0 ) ) );
			  assertSame( noon, noon.Add( DurationValue.Duration( -1, 1, 0, -0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSubtractDurationFromTimes()
		 internal virtual void ShouldSubtractDurationFromTimes()
		 {
			  assertEquals( time( 12, 0, 0, 0, UTC ), time( 12, 15, 0, 0, UTC ).sub( DurationValue.Duration( 1, 1, 900, 0 ) ) );
			  assertEquals( time( 12, 0, 0, 0, UTC ), time( 12, 0, 2, 0, UTC ).sub( DurationValue.Duration( 0, 0, 1, 1_000_000_000 ) ) );
			  assertEquals( time( 12, 0, 0, 0, UTC ), time( 12, 0, 0, 0, UTC ).sub( DurationValue.Duration( 0, 0, 1, -1_000_000_000 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEqualItself()
		 internal virtual void ShouldEqualItself()
		 {
			  assertEqual( time( 10, 52, 5, 6, UTC ), time( 10, 52, 5, 6, UTC ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotEqualSameInstantButDifferentTimezone()
		 internal virtual void ShouldNotEqualSameInstantButDifferentTimezone()
		 {
			  assertNotEqual( time( 10000, UTC ), time( 10000, ZoneOffset.of( "+01:00" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotEqualSameInstantInSameLocalTimeButDifferentTimezone()
		 internal virtual void ShouldNotEqualSameInstantInSameLocalTimeButDifferentTimezone()
		 {
			  assertNotEqual( time( 10, 52, 5, 6, UTC ), time( 11, 52, 5, 6, "+01:00" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToParseTimeThatOverridesHeaderInformation()
		 internal virtual void ShouldBeAbleToParseTimeThatOverridesHeaderInformation()
		 {
			  string headerInformation = "{timezone:-01:00}";
			  string data = "14:05:17Z";

			  TimeValue expected = TimeValue.Parse( data, OrFail );
			  TimeValue actual = TimeValue.Parse( data, OrFail, TemporalValue.ParseHeaderInformation( headerInformation ) );

			  assertEqual( expected, actual );
			  assertEquals( UTC, actual.ZoneOffset );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToParseTimeWithoutTimeZoneWithHeaderInformation()
		 internal virtual void ShouldBeAbleToParseTimeWithoutTimeZoneWithHeaderInformation()
		 {
			  string headerInformation = "{timezone:-01:00}";
			  string data = "14:05:17";

			  TimeValue expected = TimeValue.Parse( data, () => ZoneId.of("-01:00") );
			  TimeValue unexpected = TimeValue.Parse( data, InUTC );
			  TimeValue actual = TimeValue.Parse( data, OrFail, TemporalValue.ParseHeaderInformation( headerInformation ) );

			  assertEqual( expected, actual );
			  assertNotEquals( unexpected, actual );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteDerivedValueThatIsEqual()
		 internal virtual void ShouldWriteDerivedValueThatIsEqual()
		 {
			  TimeValue value1 = time( 42, ZoneOffset.of( "-18:00" ) );
			  TimeValue value2 = time( value1.Temporal() );

			  OffsetTime offsetTime1 = Write( value1 );
			  OffsetTime offsetTime2 = Write( value2 );

			  assertEquals( offsetTime1, offsetTime2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompareDerivedValue()
		 internal virtual void ShouldCompareDerivedValue()
		 {
			  TimeValue value1 = time( 4242, ZoneOffset.of( "-12:00" ) );
			  TimeValue value2 = time( value1.Temporal() );

			  assertEquals( 0, value1.UnsafeCompareTo( value2 ) );
		 }

		 private static OffsetTime Write( TimeValue value )
		 {
			  AtomicReference<OffsetTime> result = new AtomicReference<OffsetTime>();
			  value.WriteTo( new AssertOnlyAnonymousInnerClass2( result ) );
			  return result.get();
		 }

		 private class AssertOnlyAnonymousInnerClass2 : ThrowingValueWriter.AssertOnly
		 {
			 private AtomicReference<OffsetTime> _result;

			 public AssertOnlyAnonymousInnerClass2( AtomicReference<OffsetTime> result )
			 {
				 this._result = result;
			 }

			 public override void writeTime( OffsetTime offsetTime )
			 {
				  _result.set( offsetTime );
			 }
		 }
	}

}