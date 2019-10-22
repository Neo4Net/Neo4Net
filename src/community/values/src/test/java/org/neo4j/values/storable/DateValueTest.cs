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


	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using TemporalParseException = Neo4Net.Values.utils.TemporalParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assumptions.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.ordinalDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.quarterDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.weekDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.AnyValueTestUtil.assertNotEqual;

	internal class DateValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseYear()
		 internal virtual void ShouldParseYear()
		 {
			  assertEquals( date( 2015, 1, 1 ), parse( "2015" ) );
			  assertEquals( date( 2015, 1, 1 ), parse( "+2015" ) );
			  assertEquals( date( 2015, 1, 1 ), parse( "+0002015" ) );
			  AssertCannotParse( "10000" );
			  AssertCannotParse( "2K18" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseYearMonth()
		 internal virtual void ShouldParseYearMonth()
		 {
			  assertEquals( date( 2015, 3, 1 ), parse( "201503" ) );
			  assertEquals( date( 2015, 3, 1 ), parse( "2015-03" ) );
			  assertEquals( date( 2015, 3, 1 ), parse( "2015-3" ) );
			  assertEquals( date( 2015, 3, 1 ), parse( "+2015-03" ) );
			  AssertCannotParse( "2018-00" );
			  AssertCannotParse( "2018-13" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseYearWeek()
		 internal virtual void ShouldParseYearWeek()
		 {
			  assertEquals( weekDate( 2015, 5, 1 ), parse( "2015W05" ) );
			  assertEquals( weekDate( 2015, 53, 1 ), parse( "2015W53" ) ); // 2015 had 53 weeks
			  AssertCannotParse( "2015W5" );
			  assertEquals( weekDate( 2015, 5, 1 ), parse( "2015-W05" ) );
			  assertEquals( weekDate( 2015, 5, 1 ), parse( "2015-W5" ) );
			  assertEquals( weekDate( 2015, 5, 1 ), parse( "+2015-W05" ) );
			  AssertCannotParse( "+2015W05" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseYearQuarter()
		 internal virtual void ShouldParseYearQuarter()
		 {
			  assumeTrue( DateValue.QuarterDates );
			  assertEquals( quarterDate( 2017, 3, 1 ), parse( "2017Q3" ) );
			  assertEquals( quarterDate( 2017, 3, 1 ), parse( "2017-Q3" ) );
			  assertEquals( quarterDate( 2017, 3, 1 ), parse( "+2017-Q3" ) );
			  AssertCannotParse( "2015Q0" );
			  AssertCannotParse( "2015Q5" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseCalendarDate()
		 internal virtual void ShouldParseCalendarDate()
		 {
			  assertEquals( date( 2016, 1, 27 ), parse( "20160127" ) );
			  assertEquals( date( 2016, 1, 27 ), parse( "+2016-01-27" ) );
			  assertEquals( date( 2016, 1, 27 ), parse( "+2016-1-27" ) );
			  AssertCannotParse( "2015-01-32" );
			  AssertCannotParse( "2015-01-00" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseWeekDate()
		 internal virtual void ShouldParseWeekDate()
		 {
			  assertEquals( weekDate( 2015, 5, 6 ), parse( "2015W056" ) );
			  AssertCannotParse( "+2015W056" );
			  assertEquals( weekDate( 2015, 5, 6 ), parse( "2015-W05-6" ) );
			  assertEquals( weekDate( 2015, 5, 6 ), parse( "+2015-W05-6" ) );
			  assertEquals( weekDate( 2015, 5, 6 ), parse( "2015-W5-6" ) );
			  assertEquals( weekDate( 2015, 5, 6 ), parse( "+2015-W5-6" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseQuarterDate()
		 internal virtual void ShouldParseQuarterDate()
		 {
			  assumeTrue( DateValue.QuarterDates );
			  assertEquals( quarterDate( 2017, 3, 92 ), parse( "2017Q392" ) );
			  assertEquals( quarterDate( 2017, 3, 92 ), parse( "2017-Q3-92" ) );
			  assertEquals( quarterDate( 2017, 3, 92 ), parse( "+2017-Q3-92" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseOrdinalDate()
		 internal virtual void ShouldParseOrdinalDate()
		 {
			  assertEquals( ordinalDate( 2017, 3 ), parse( "2017003" ) );
			  AssertCannotParse( "20173" );
			  assertEquals( ordinalDate( 2017, 3 ), parse( "2017-003" ) );
			  assertEquals( ordinalDate( 2017, 3 ), parse( "+2017-003" ) );
			  AssertCannotParse( "2017-366" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEnforceStrictWeekRanges()
		 internal virtual void ShouldEnforceStrictWeekRanges()
		 {
			  LocalDate localDate = weekDate( 2017, 52, 7 ).temporal();
			  assertEquals( DayOfWeek.Sunday, localDate.DayOfWeek, "Sunday is the seventh day of the week." );
			  assertEquals( 52, localDate.get( IsoFields.WEEK_OF_WEEK_BASED_YEAR ) );
			  assertEquals( localDate, date( 2017, 12, 31 ).temporal() );
			  InvalidValuesArgumentException expected = assertThrows( typeof( InvalidValuesArgumentException ), () => weekDate(2017, 53, 1), "2017 does not have 53 weeks." );
			  assertEquals( "Year 2017 does not contain 53 weeks.", expected.Message );
			  assertEquals( date( 2016, 1, 1 ), weekDate( 2015, 53, 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEnforceStrictQuarterRanges()
		 internal virtual void ShouldEnforceStrictQuarterRanges()
		 {
			  assertEquals( date( 2017, 3, 31 ), quarterDate( 2017, 1, 90 ) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 1, 0) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 2, 0) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 3, 0) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 4, 0) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 4, 93) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 3, 93) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 2, 92) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 1, 92) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2017, 1, 91) );
			  assertEquals( date( 2016, 3, 31 ), quarterDate( 2016, 1, 91 ) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => quarterDate(2016, 1, 92) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotParseInvalidDates()
		 internal virtual void ShouldNotParseInvalidDates()
		 {
			  AssertCannotParse( "2015W54" ); // no year should have more than 53 weeks (2015 had 53 weeks)
			  assertThrows( typeof( InvalidValuesArgumentException ), () => parse("2017W53") ); // 2017 only has 52 weeks
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteDate()
		 internal virtual void ShouldWriteDate()
		 {
			  // given
			  foreach ( DateValue value in new DateValue[] { date( 2016, 2, 29 ), date( 2017, 12, 22 ) } )
			  {
					IList<DateValue> values = new List<DateValue>( 1 );
					ValueWriter<Exception> writer = new AssertOnlyAnonymousInnerClass( this, values );

					// when
					value.WriteTo( writer );

					// then
					assertEquals( singletonList( value ), values );
			  }
		 }

		 private class AssertOnlyAnonymousInnerClass : ThrowingValueWriter.AssertOnly
		 {
			 private readonly DateValueTest _outerInstance;

			 private IList<DateValue> _values;

			 public AssertOnlyAnonymousInnerClass( DateValueTest outerInstance, IList<DateValue> values )
			 {
				 this.outerInstance = outerInstance;
				 this._values = values;
			 }

			 public override void writeDate( LocalDate localDate )
			 {
				  _values.Add( date( localDate ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddDurationToDates()
		 internal virtual void ShouldAddDurationToDates()
		 {
			  assertEquals( date( 2018, 2, 1 ), date( 2018, 1, 1 ).add( DurationValue.Duration( 1, 0, 900, 0 ) ) );
			  assertEquals( date( 2018, 2, 28 ), date( 2018, 1, 31 ).add( DurationValue.Duration( 1, 0, 0, 0 ) ) );
			  assertEquals( date( 2018, 1, 28 ), date( 2018, 2, 28 ).add( DurationValue.Duration( -1, 0, 0, 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReuseInstanceInArithmetics()
		 internal virtual void ShouldReuseInstanceInArithmetics()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DateValue date = date(2018, 2, 1);
			  DateValue date = date( 2018, 2, 1 );
			  assertSame( date, date.Add( DurationValue.Duration( 0, 0, 0, 0 ) ) );
			  assertSame( date, date.Add( DurationValue.Duration( 0, 0, 1, 1 ) ) );
			  assertSame( date, date.Add( DurationValue.Duration( -0, 0, 1, -1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSubtractDurationFromDates()
		 internal virtual void ShouldSubtractDurationFromDates()
		 {
			  assertEquals( date( 2018, 1, 1 ), date( 2018, 2, 1 ).sub( DurationValue.Duration( 1, 0, 900, 0 ) ) );
			  assertEquals( date( 2018, 1, 28 ), date( 2018, 2, 28 ).sub( DurationValue.Duration( 1, 0, 0, 0 ) ) );
			  assertEquals( date( 2018, 2, 28 ), date( 2018, 1, 31 ).sub( DurationValue.Duration( -1, 0, 0, 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEqualItself()
		 internal virtual void ShouldEqualItself()
		 {
			  assertEqual( date( 2018, 1, 31 ), date( 2018, 1, 31 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotEqualOther()
		 internal virtual void ShouldNotEqualOther()
		 {
			  assertNotEqual( date( 2018, 1, 31 ), date( 2018, 1, 30 ) );
		 }

		 private static void AssertCannotParse( string text )
		 {
			  assertThrows( typeof( TemporalParseException ), () => parse(text), format("'%s' parsed to value", text) );
		 }
	}

}