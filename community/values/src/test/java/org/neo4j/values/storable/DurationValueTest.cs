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
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.jupiter.api.Test;


	using Org.Neo4j.Helpers.Collection;
	using InvalidValuesArgumentException = Org.Neo4j.Values.utils.InvalidValuesArgumentException;
	using TemporalParseException = Org.Neo4j.Values.utils.TemporalParseException;
	using TemporalUtil = Org.Neo4j.Values.utils.TemporalUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Pair.pair;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DurationValue.between;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DurationValue.durationBetween;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DurationValue.parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertNotEqual;

	internal class DurationValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNormalizeNanoseconds()
		 internal virtual void ShouldNormalizeNanoseconds()
		 {
			  // given
			  DurationValue evenPos = duration( 0, 0, 0, 1_000_000_000 );
			  DurationValue evenNeg = duration( 0, 0, 0, -1_000_000_000 );
			  DurationValue pos = duration( 0, 0, 0, 1_500_000_000 );
			  DurationValue neg = duration( 0, 0, 0, -1_400_000_000 );

			  // then
			  assertEquals( 500_000_000, pos.get( NANOS ), "+nanos" );
			  assertEquals( 1, pos.get( SECONDS ), "+seconds" );
			  assertEquals( 600_000_000, neg.get( NANOS ), "+nanos" );
			  assertEquals( -2, neg.get( SECONDS ), "-seconds" );

			  assertEquals( 0, evenPos.get( NANOS ), "+nanos" );
			  assertEquals( 1, evenPos.get( SECONDS ), "+seconds" );
			  assertEquals( 0, evenNeg.get( NANOS ), "+nanos" );
			  assertEquals( -1, evenNeg.get( SECONDS ), "-seconds" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFormatDurationToString()
		 internal virtual void ShouldFormatDurationToString()
		 {
			  TestDurationToString( 1, 0, "PT1S" );
			  TestDurationToString( -1, 0, "PT-1S" );

			  TestDurationToString( 59, -500_000_000, "PT58.5S" );
			  TestDurationToString( 59, 500_000_000, "PT59.5S" );
			  TestDurationToString( 60, -500_000_000, "PT59.5S" );
			  TestDurationToString( 60, 500_000_000, "PT1M0.5S" );
			  TestDurationToString( 61, -500_000_000, "PT1M0.5S" );

			  TestDurationToString( -59, 500_000_000, "PT-58.5S" );
			  TestDurationToString( -59, -500_000_000, "PT-59.5S" );
			  TestDurationToString( -60, 500_000_000, "PT-59.5S" );
			  TestDurationToString( -60, -500_000_000, "PT-1M-0.5S" );
			  TestDurationToString( -61, 500_000_000, "PT-1M-0.5S" );
			  TestDurationToString( -61, -500_000_000, "PT-1M-1.5S" );

			  TestDurationToString( 0, 5, "PT0.000000005S" );
			  TestDurationToString( 0, -5, "PT-0.000000005S" );
			  TestDurationToString( 0, 999_999_999, "PT0.999999999S" );
			  TestDurationToString( 0, -999_999_999, "PT-0.999999999S" );

			  TestDurationToString( 1, 5, "PT1.000000005S" );
			  TestDurationToString( -1, -5, "PT-1.000000005S" );
			  TestDurationToString( 1, -5, "PT0.999999995S" );
			  TestDurationToString( -1, 5, "PT-0.999999995S" );
			  TestDurationToString( 1, 999999999, "PT1.999999999S" );
			  TestDurationToString( -1, -999999999, "PT-1.999999999S" );
			  TestDurationToString( 1, -999999999, "PT0.000000001S" );
			  TestDurationToString( -1, 999999999, "PT-0.000000001S" );

			  TestDurationToString( -78036, -143000000, "PT-21H-40M-36.143S" );
		 }

		 private void TestDurationToString( long seconds, int nanos, string expectedValue )
		 {
			  assertEquals( expectedValue, duration( 0, 0, seconds, nanos ).prettyPrint() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNormalizeSecondsAndNanos()
		 internal virtual void ShouldNormalizeSecondsAndNanos()
		 {
			  // given
			  DurationValue pos = duration( 0, 0, 5, -1_400_000_000 );
			  DurationValue neg = duration( 0, 0, -5, 1_500_000_000 );
			  DurationValue x = duration( 0, 0, 1, -1_400_000_000 );

			  DurationValue y = duration( 0, 0, -59, -500_000_000 );
			  DurationValue y2 = duration( 0, 0, -60, 500_000_000 );

			  // then
			  assertEquals( 600_000_000, pos.get( NANOS ), "+nanos" );
			  assertEquals( 3, pos.get( SECONDS ), "+seconds" );
			  assertEquals( 500_000_000, neg.get( NANOS ), "+nanos" );
			  assertEquals( -4, neg.get( SECONDS ), "-seconds" );
			  assertEquals( 600_000_000, x.get( NANOS ), "+nanos" );
			  assertEquals( -1, x.get( SECONDS ), "-seconds" );
			  assertEquals( 500_000_000, y.get( NANOS ), "+nanos" );
			  assertEquals( -60, y.get( SECONDS ), "-seconds" );
			  assertEquals( 500_000_000, y2.get( NANOS ), "+nanos" );
			  assertEquals( -60, y2.get( SECONDS ), "-seconds" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFormatAsPrettyString()
		 internal virtual void ShouldFormatAsPrettyString()
		 {
			  assertEquals( "P1Y", PrettyPrint( 12, 0, 0, 0 ) );
			  assertEquals( "P5M", PrettyPrint( 5, 0, 0, 0 ) );
			  assertEquals( "P84D", PrettyPrint( 0, 84, 0, 0 ) );
			  assertEquals( "P2Y4M11D", PrettyPrint( 28, 11, 0, 0 ) );
			  assertEquals( "PT5S", PrettyPrint( 0, 0, 5, 0 ) );
			  assertEquals( "PT30H22M8S", PrettyPrint( 0, 0, 109328, 0 ) );
			  assertEquals( "PT7.123456789S", PrettyPrint( 0, 0, 7, 123_456_789 ) );
			  assertEquals( "PT0.000000001S", PrettyPrint( 0, 0, 0, 1 ) );
			  assertEquals( "PT0.1S", PrettyPrint( 0, 0, 0, 100_000_000 ) );
			  assertEquals( "PT0S", PrettyPrint( 0, 0, 0, 0 ) );
			  assertEquals( "PT1S", PrettyPrint( 0, 0, 0, 1_000_000_000 ) );
			  assertEquals( "PT-1S", PrettyPrint( 0, 0, 0, -1_000_000_000 ) );
			  assertEquals( "PT1.5S", PrettyPrint( 0, 0, 1, 500_000_000 ) );
			  assertEquals( "PT-1.4S", PrettyPrint( 0, 0, -1, -400_000_000 ) );
		 }

		 private static string PrettyPrint( long months, long days, long seconds, int nanos )
		 {
			  return duration( months, days, seconds, nanos ).prettyPrint();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleLargeNanos()
		 internal virtual void ShouldHandleLargeNanos()
		 {
			  DurationValue duration = DurationValue.Duration( 0L, 0L, 0L, long.MaxValue );
			  assertEquals( long.MaxValue, duration.Get( "nanoseconds" ).value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseDuration()
		 internal virtual void ShouldParseDuration()
		 {
			  assertEquals( duration( 14, 25, 18367, 800_000_000 ), parse( "+P1Y2M3W4DT5H6M7.8S" ) );

			  assertEquals( duration( 0, 0, 0, -100000000 ), parse( "PT-0.1S" ) );
			  assertEquals( duration( 0, 0, 0, -20000000 ), parse( "PT-0.02S" ) );
			  assertEquals( duration( 0, 0, 0, -3000000 ), parse( "PT-0.003S" ) );
			  assertEquals( duration( 0, 0, 0, -400000 ), parse( "PT-0.0004S" ) );
			  assertEquals( duration( 0, 0, 0, -50000 ), parse( "PT-0.00005S" ) );
			  assertEquals( duration( 0, 0, 0, -6000 ), parse( "PT-0.000006S" ) );
			  assertEquals( duration( 0, 0, 0, -700 ), parse( "PT-0.0000007S" ) );
			  assertEquals( duration( 0, 0, 0, -80 ), parse( "PT-0.00000008S" ) );
			  assertEquals( duration( 0, 0, 0, -9 ), parse( "PT-0.000000009S" ) );

			  assertEquals( duration( 0, 0, 0, 900_000_000 ), parse( "PT0.900000000S" ) );
			  assertEquals( duration( 0, 0, 0, 800_000_000 ), parse( "PT0.80000000S" ) );
			  assertEquals( duration( 0, 0, 0, 700_000_000 ), parse( "PT0.7000000S" ) );
			  assertEquals( duration( 0, 0, 0, 600_000_000 ), parse( "PT0.600000S" ) );
			  assertEquals( duration( 0, 0, 0, 500_000_000 ), parse( "PT0.50000S" ) );
			  assertEquals( duration( 0, 0, 0, 400_000_000 ), parse( "PT0.4000S" ) );
			  assertEquals( duration( 0, 0, 0, 300_000_000 ), parse( "PT0.300S" ) );
			  assertEquals( duration( 0, 0, 0, 200_000_000 ), parse( "PT0.20S" ) );
			  assertEquals( duration( 0, 0, 0, 100_000_000 ), parse( "PT0.1S" ) );

			  AssertParsesOne( "P", "Y", 12, 0, 0 );
			  AssertParsesOne( "P", "M", 1, 0, 0 );
			  AssertParsesOne( "P", "W", 0, 7, 0 );
			  AssertParsesOne( "P", "D", 0, 1, 0 );
			  AssertParsesOne( "PT", "H", 0, 0, 3600 );
			  AssertParsesOne( "PT", "M", 0, 0, 60 );
			  AssertParsesOne( "PT", "S", 0, 0, 1 );

			  assertEquals( duration( 0, 0, -1, -100_000_000 ), parse( "PT-1,1S" ) );

			  assertEquals( duration( 10, 0, 0, 0 ), parse( "P1Y-2M" ) );
			  assertEquals( duration( 0, 20, 0, 0 ), parse( "P3W-1D" ) );
			  assertEquals( duration( 0, 0, 3000, 0 ), parse( "PT1H-10M" ) );
			  assertEquals( duration( 0, 0, 3000, 0 ), parse( "PT1H-600S" ) );
			  assertEquals( duration( 0, 0, 50, 0 ), parse( "PT1M-10S" ) );
		 }

		 private void AssertParsesOne( string prefix, string suffix, int months, int days, int seconds )
		 {
			  assertEquals( duration( months, days, seconds, 0 ), parse( prefix + "1" + suffix ) );
			  assertEquals( duration( months, days, seconds, 0 ), parse( "+" + prefix + "1" + suffix ) );
			  assertEquals( duration( months, days, seconds, 0 ), parse( prefix + "+1" + suffix ) );
			  assertEquals( duration( months, days, seconds, 0 ), parse( "+" + prefix + "+1" + suffix ) );

			  assertEquals( duration( -months, -days, -seconds, 0 ), parse( "-" + prefix + "1" + suffix ) );
			  assertEquals( duration( -months, -days, -seconds, 0 ), parse( prefix + "-1" + suffix ) );
			  assertEquals( duration( -months, -days, -seconds, 0 ), parse( "+" + prefix + "-1" + suffix ) );
			  assertEquals( duration( -months, -days, -seconds, 0 ), parse( "-" + prefix + "+1" + suffix ) );

			  assertEquals( duration( months, days, seconds, 0 ), parse( "-" + prefix + "-1" + suffix ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseDateBasedDuration()
		 internal virtual void ShouldParseDateBasedDuration()
		 {
			  assertEquals( duration( 14, 17, 45252, 123400000 ), parse( "P0001-02-17T12:34:12.1234" ) );
			  assertEquals( duration( 14, 17, 45252, 123400000 ), parse( "P00010217T123412.1234" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotParseInvalidDurationStrings()
		 internal virtual void ShouldNotParseInvalidDurationStrings()
		 {
			  assertThrows( typeof( TemporalParseException ), () => parse("") );
			  assertThrows( typeof( TemporalParseException ), () => parse("P") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT.S") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT,S") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT.0S") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT,0S") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT0.S") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT0,S") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT1,-1S") );
			  assertThrows( typeof( TemporalParseException ), () => parse("PT1.-1S") );
			  foreach ( string s in new string[]{ "Y", "M", "W", "D" } )
			  {
					assertThrows( typeof( TemporalParseException ), () => parse("P-" + s) );
					assertThrows( typeof( TemporalParseException ), () => parse("P1" + s + "T") );
			  }
			  foreach ( string s in new string[]{ "H", "M", "S" } )
			  {
					assertThrows( typeof( TemporalParseException ), () => parse("PT-" + s) );
					assertThrows( typeof( TemporalParseException ), () => parse("T1" + s) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteDuration()
		 internal virtual void ShouldWriteDuration()
		 {
			  // given
			  foreach ( DurationValue duration in new DurationValue[]{ duration( 0, 0, 0, 0 ), duration( 1, 0, 0, 0 ), duration( 0, 1, 0, 0 ), duration( 0, 0, 1, 0 ), duration( 0, 0, 0, 1 ) } )
			  {
					IList<DurationValue> values = new List<DurationValue>( 1 );
					ValueWriter<Exception> writer = new AssertOnlyAnonymousInnerClass( this, values );

					// when
					duration.WriteTo( writer );

					// then
					assertEquals( singletonList( duration ), values );
			  }
		 }

		 private class AssertOnlyAnonymousInnerClass : ThrowingValueWriter.AssertOnly
		 {
			 private readonly DurationValueTest _outerInstance;

			 private IList<DurationValue> _values;

			 public AssertOnlyAnonymousInnerClass( DurationValueTest outerInstance, IList<DurationValue> values )
			 {
				 this.outerInstance = outerInstance;
				 this._values = values;
			 }

			 public override void writeDuration( long months, long days, long seconds, int nanos )
			 {
				  _values.Add( duration( months, days, seconds, nanos ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddToLocalDate()
		 internal virtual void ShouldAddToLocalDate()
		 {
			  assertEquals( LocalDate.of( 2017, 12, 5 ), LocalDate.of( 2017, 12, 4 ).plus( parse( "PT24H" ) ), "seconds" );
			  assertEquals( LocalDate.of( 2017, 12, 3 ), LocalDate.of( 2017, 12, 4 ).minus( parse( "PT24H" ) ), "seconds" );
			  assertEquals( LocalDate.of( 2017, 12, 4 ), LocalDate.of( 2017, 12, 4 ).plus( parse( "PT24H-1S" ) ), "seconds" );
			  assertEquals( LocalDate.of( 2017, 12, 4 ), LocalDate.of( 2017, 12, 4 ).minus( parse( "PT24H-1S" ) ), "seconds" );
			  assertEquals( LocalDate.of( 2017, 12, 5 ), LocalDate.of( 2017, 12, 4 ).plus( parse( "P1D" ) ), "days" );
			  assertEquals( LocalDate.of( 2017, 12, 3 ), LocalDate.of( 2017, 12, 4 ).minus( parse( "P1D" ) ), "days" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveSensibleHashCode()
		 internal virtual void ShouldHaveSensibleHashCode()
		 {
			  assertEquals( 0, duration( 0, 0, 0, 0 ).computeHash() );

			  assertNotEquals( duration( 0, 0, 0, 1 ).computeHash(), duration(0, 0, 0, 2).computeHash() );
			  assertNotEquals( duration( 0, 0, 0, 1 ).computeHash(), duration(0, 0, 1, 0).computeHash() );
			  assertNotEquals( duration( 0, 0, 0, 1 ).computeHash(), duration(0, 1, 0, 0).computeHash() );
			  assertNotEquals( duration( 0, 0, 0, 1 ).computeHash(), duration(1, 0, 0, 0).computeHash() );

			  assertNotEquals( duration( 0, 0, 1, 0 ).computeHash(), duration(0, 0, 2, 0).computeHash() );
			  assertNotEquals( duration( 0, 0, 1, 0 ).computeHash(), duration(0, 0, 0, 1).computeHash() );
			  assertNotEquals( duration( 0, 0, 1, 0 ).computeHash(), duration(0, 1, 0, 0).computeHash() );
			  assertNotEquals( duration( 0, 0, 1, 0 ).computeHash(), duration(1, 0, 0, 0).computeHash() );

			  assertNotEquals( duration( 0, 1, 0, 0 ).computeHash(), duration(0, 2, 0, 0).computeHash() );
			  assertNotEquals( duration( 0, 1, 0, 0 ).computeHash(), duration(0, 0, 0, 1).computeHash() );
			  assertNotEquals( duration( 0, 1, 0, 0 ).computeHash(), duration(0, 0, 1, 0).computeHash() );
			  assertNotEquals( duration( 0, 1, 0, 0 ).computeHash(), duration(1, 0, 0, 0).computeHash() );

			  assertNotEquals( duration( 1, 0, 0, 0 ).computeHash(), duration(2, 0, 0, 0).computeHash() );
			  assertNotEquals( duration( 1, 0, 0, 0 ).computeHash(), duration(0, 0, 0, 1).computeHash() );
			  assertNotEquals( duration( 1, 0, 0, 0 ).computeHash(), duration(0, 0, 1, 0).computeHash() );
			  assertNotEquals( duration( 1, 0, 0, 0 ).computeHash(), duration(0, 1, 0, 0).computeHash() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowExceptionOnAddOverflow()
		 internal virtual void ShouldThrowExceptionOnAddOverflow()
		 {
			  DurationValue duration1 = duration( 0, 0, long.MaxValue, 500_000_000 );
			  DurationValue duration2 = duration( 0, 0, 1, 0 );
			  DurationValue duration3 = duration( 0, 0, 0, 500_000_000 );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => duration1.Add(duration2) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => duration1.Add(duration3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowExceptionOnSubtractOverflow()
		 internal virtual void ShouldThrowExceptionOnSubtractOverflow()
		 {
			  DurationValue duration1 = duration( 0, 0, long.MinValue, 0 );
			  DurationValue duration2 = duration( 0, 0, 1, 0 );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => duration1.Sub(duration2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowExceptionOnMultiplyOverflow()
		 internal virtual void ShouldThrowExceptionOnMultiplyOverflow()
		 {
			  DurationValue duration = duration( 0, 0, long.MaxValue, 0 );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => duration.Mul(Values.IntValue(2)) );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => duration.Mul(Values.FloatValue(2)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowExceptionOnDivideOverflow()
		 internal virtual void ShouldThrowExceptionOnDivideOverflow()
		 {
			  DurationValue duration = duration( 0, 0, long.MaxValue, 0 );
			  assertThrows( typeof( InvalidValuesArgumentException ), () => duration.Div(Values.FloatValue(0.5f)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMultiplyDurationByInteger()
		 internal virtual void ShouldMultiplyDurationByInteger()
		 {
			  assertEquals( duration( 2, 0, 0, 0 ), duration( 1, 0, 0, 0 ).mul( longValue( 2 ) ) );
			  assertEquals( duration( 0, 2, 0, 0 ), duration( 0, 1, 0, 0 ).mul( longValue( 2 ) ) );
			  assertEquals( duration( 0, 0, 2, 0 ), duration( 0, 0, 1, 0 ).mul( longValue( 2 ) ) );
			  assertEquals( duration( 0, 0, 0, 2 ), duration( 0, 0, 0, 1 ).mul( longValue( 2 ) ) );

			  assertEquals( duration( 0, 40, 0, 0 ), duration( 0, 20, 0, 0 ).mul( longValue( 2 ) ) );
			  assertEquals( duration( 0, 0, 100_000, 0 ), duration( 0, 0, 50_000, 0 ).mul( longValue( 2 ) ) );
			  assertEquals( duration( 0, 0, 1, 0 ), duration( 0, 0, 0, 500_000_000 ).mul( longValue( 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMultiplyDurationByFloat()
		 internal virtual void ShouldMultiplyDurationByFloat()
		 {
			  assertEquals( duration( 0, 0, 0, 500_000_000 ), duration( 0, 0, 1, 0 ).mul( doubleValue( 0.5 ) ) );
			  assertEquals( duration( 0, 0, 43200, 0 ), duration( 0, 1, 0, 0 ).mul( doubleValue( 0.5 ) ) );
			  assertEquals( duration( 0, 15, 18873, 0 ), duration( 1, 0, 0, 0 ).mul( doubleValue( 0.5 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDivideDuration()
		 internal virtual void ShouldDivideDuration()
		 {
			  assertEquals( duration( 0, 0, 0, 500_000_000 ), duration( 0, 0, 1, 0 ).div( longValue( 2 ) ) );
			  assertEquals( duration( 0, 0, 43200, 0 ), duration( 0, 1, 0, 0 ).div( longValue( 2 ) ) );
			  assertEquals( duration( 0, 15, 18873, 0 ), duration( 1, 0, 0, 0 ).div( longValue( 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeDurationBetweenDates()
		 internal virtual void ShouldComputeDurationBetweenDates()
		 {
			  assertEquals( duration( 22, 23, 0, 0 ), durationBetween( date( 2016, 1, 27 ), date( 2017, 12, 20 ) ) );
			  assertEquals( duration( 0, 693, 0, 0 ), between( DAYS, date( 2016, 1, 27 ), date( 2017, 12, 20 ) ) );
			  assertEquals( duration( 22, 0, 0, 0 ), between( MONTHS, date( 2016, 1, 27 ), date( 2017, 12, 20 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeDurationBetweenLocalTimes()
		 internal virtual void ShouldComputeDurationBetweenLocalTimes()
		 {
			  assertEquals( duration( 0, 0, 10623, 0 ), durationBetween( localTime( 11, 30, 52, 0 ), localTime( 14, 27, 55, 0 ) ) );
			  assertEquals( duration( 0, 0, 10623, 0 ), between( SECONDS, localTime( 11, 30, 52, 0 ), localTime( 14, 27, 55, 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeDurationBetweenTimes()
		 internal virtual void ShouldComputeDurationBetweenTimes()
		 {
			  assertEquals( duration( 0, 0, 140223, 0 ), durationBetween( time( 11, 30, 52, 0, ofHours( 18 ) ), time( 14, 27, 55, 0, ofHours( -18 ) ) ) );
			  assertEquals( duration( 0, 0, 10623, 0 ), between( SECONDS, time( 11, 30, 52, 0, UTC ), time( 14, 27, 55, 0, UTC ) ) );

			  assertEquals( duration( 0, 0, 10623, 0 ), durationBetween( time( 11, 30, 52, 0, UTC ), localTime( 14, 27, 55, 0 ) ) );
			  assertEquals( duration( 0, 0, 10623, 0 ), durationBetween( time( 11, 30, 52, 0, ofHours( 17 ) ), localTime( 14, 27, 55, 0 ) ) );
			  assertEquals( duration( 0, 0, -10623, 0 ), durationBetween( localTime( 14, 27, 55, 0 ), time( 11, 30, 52, 0, UTC ) ) );
			  assertEquals( duration( 0, 0, -10623, 0 ), durationBetween( localTime( 14, 27, 55, 0 ), time( 11, 30, 52, 0, ofHours( 17 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeDurationBetweenDateAndTime()
		 internal virtual void ShouldComputeDurationBetweenDateAndTime()
		 {
			  assertEquals( parse( "PT14H32M11S" ), durationBetween( date( 2017, 12, 21 ), localTime( 14, 32, 11, 0 ) ) );
			  assertEquals( parse( "-PT14H32M11S" ), durationBetween( localTime( 14, 32, 11, 0 ), date( 2017, 12, 21 ) ) );
			  assertEquals( parse( "PT14H32M11S" ), durationBetween( date( 2017, 12, 21 ), time( 14, 32, 11, 0, UTC ) ) );
			  assertEquals( parse( "-PT14H32M11S" ), durationBetween( time( 14, 32, 11, 0, UTC ), date( 2017, 12, 21 ) ) );
			  assertEquals( parse( "PT14H32M11S" ), durationBetween( date( 2017, 12, 21 ), time( 14, 32, 11, 0, ofHours( -12 ) ) ) );
			  assertEquals( parse( "-PT14H32M11S" ), durationBetween( time( 14, 32, 11, 0, ofHours( -12 ) ), date( 2017, 12, 21 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeDurationBetweenDateTimeAndTime()
		 internal virtual void ShouldComputeDurationBetweenDateTimeAndTime()
		 {
			  assertEquals( parse( "PT8H-20M" ), durationBetween( datetime( date( 2017, 12, 21 ), time( 6, 52, 11, 0, UTC ) ), localTime( 14, 32, 11, 0 ) ) );
			  assertEquals( parse( "PT-8H+20M" ), durationBetween( localTime( 14, 32, 11, 0 ), datetime( date( 2017, 12, 21 ), time( 6, 52, 11, 0, UTC ) ) ) );

			  assertEquals( parse( "-PT14H32M11S" ), durationBetween( localTime( 14, 32, 11, 0 ), date( 2017, 12, 21 ) ) );
			  assertEquals( parse( "PT14H32M11S" ), durationBetween( date( 2017, 12, 21 ), time( 14, 32, 11, 0, UTC ) ) );
			  assertEquals( parse( "-PT14H32M11S" ), durationBetween( time( 14, 32, 11, 0, UTC ), date( 2017, 12, 21 ) ) );
			  assertEquals( parse( "PT14H32M11S" ), durationBetween( date( 2017, 12, 21 ), time( 14, 32, 11, 0, ofHours( -12 ) ) ) );
			  assertEquals( parse( "-PT14H32M11S" ), durationBetween( time( 14, 32, 11, 0, ofHours( -12 ) ), date( 2017, 12, 21 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeDurationBetweenDateTimeAndDateTime()
		 internal virtual void ShouldComputeDurationBetweenDateTimeAndDateTime()
		 {
			  assertEquals( parse( "PT1H" ), durationBetween( datetime( date( 2017, 12, 21 ), time( 6, 52, 11, 0, UTC ) ), datetime( date( 2017, 12, 21 ), time( 7, 52, 11, 0, UTC ) ) ) );
			  assertEquals( parse( "P1D" ), durationBetween( datetime( date( 2017, 12, 21 ), time( 6, 52, 11, 0, UTC ) ), datetime( date( 2017, 12, 22 ), time( 6, 52, 11, 0, UTC ) ) ) );
			  assertEquals( parse( "P1DT1H" ), durationBetween( datetime( date( 2017, 12, 21 ), time( 6, 52, 11, 0, UTC ) ), datetime( date( 2017, 12, 22 ), time( 7, 52, 11, 0, UTC ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGetSameInstantWhenAddingDurationBetweenToInstant()
		 internal virtual void ShouldGetSameInstantWhenAddingDurationBetweenToInstant()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.helpers.collection.Pair<java.time.temporal.Temporal, java.time.temporal.Temporal>[] input = new org.neo4j.helpers.collection.Pair[]{ pair(datetime(date(2017, 3, 20), localTime(13, 37, 0, 0), java.time.ZoneId.of("Europe/Stockholm")), datetime(date(2017, 3, 26), localTime(19, 40, 0, 0), java.time.ZoneId.of("Europe/Stockholm"))), pair(datetime(date(2017, 3, 20), localTime(13, 37, 0, 0), java.time.ZoneId.of("Europe/Stockholm")), datetime(date(2017, 3, 26), localTime(11, 40, 0, 0), java.time.ZoneId.of("Europe/Stockholm"))), pair(datetime(date(2017, 10, 20), localTime(13, 37, 0, 0), java.time.ZoneId.of("Europe/Stockholm")), datetime(date(2017, 10, 29), localTime(19, 40, 0, 0), java.time.ZoneId.of("Europe/Stockholm"))), pair(datetime(date(2017, 10, 20), localTime(13, 37, 0, 0), java.time.ZoneId.of("Europe/Stockholm")), datetime(date(2017, 10, 29), localTime(11, 40, 0, 0), java.time.ZoneId.of("Europe/Stockholm")))};
			  Pair<Temporal, Temporal>[] input = new Pair[]{ pair( datetime( date( 2017, 3, 20 ), localTime( 13, 37, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 3, 26 ), localTime( 19, 40, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ) ), pair( datetime( date( 2017, 3, 20 ), localTime( 13, 37, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 3, 26 ), localTime( 11, 40, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ) ), pair( datetime( date( 2017, 10, 20 ), localTime( 13, 37, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 10, 29 ), localTime( 19, 40, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ) ), pair( datetime( date( 2017, 10, 20 ), localTime( 13, 37, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ), datetime( date( 2017, 10, 29 ), localTime( 11, 40, 0, 0 ), ZoneId.of( "Europe/Stockholm" ) ) ) };
			  foreach ( Pair<Temporal, Temporal> pair in input )
			  {
					Temporal a = pair.First(), b = pair.Other();

					// when
					DurationValue diffAB = durationBetween( a, b );
					DurationValue diffBA = durationBetween( b, a );
					DurationValue diffABs = between( SECONDS, a, b );
					DurationValue diffBAs = between( SECONDS, b, a );

					// then
					assertEquals( b, a.plus( diffAB ), diffAB.PrettyPrint() );
					assertEquals( a, b.plus( diffBA ), diffBA.PrettyPrint() );
					assertEquals( b, a.plus( diffABs ), diffABs.PrettyPrint() );
					assertEquals( a, b.plus( diffBAs ), diffBAs.PrettyPrint() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEqualItself()
		 internal virtual void ShouldEqualItself()
		 {
			  assertEqual( duration( 40, 3, 13, 37 ), duration( 40, 3, 13, 37 ) );
			  assertEqual( duration( 40, 3, 14, 37 ), duration( 40, 3, 13, 1_000_000_037 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotEqualOther()
		 internal virtual void ShouldNotEqualOther()
		 {
			  assertNotEqual( duration( 40, 3, 13, 37 ), duration( 40, 3, 14, 37 ) );

			  // average nbr of seconds on a month doesn't imply equality
			  assertNotEqual( duration( 1, 0, 0, 0 ), duration( 0, 0, 2_629_800, 0 ) );

			  // not the same due to leap seconds
			  assertNotEqual( duration( 0, 1, 0, 0 ), duration( 0, 0, 60 * 60 * 24, 0 ) );

			  // average nbr of days in 400 years doesn't imply equality
			  assertNotEqual( duration( 400 * 12, 0, 0, 0 ), duration( 0, 146_097, 0, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApproximateWithoutAccumulatedRoundingErrors()
		 public virtual void ShouldApproximateWithoutAccumulatedRoundingErrors()
		 {
			  DurationValue result = DurationValue.Approximate( 10.8, 0, 0, 0 );
			  assertEqual( result, DurationValue.Duration( 10, 24, 30196, 800000000 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApproximateWithoutAccumulatedRoundingErrors2()
		 public virtual void ShouldApproximateWithoutAccumulatedRoundingErrors2()
		 {
			  double months = 1.9013243104086859E-16; // 0.5 ns
			  double nanos = 0.6; // with 1.1 ns we should be on the safe side to get rounded to 1 ns, even with rounding errors
			  DurationValue result = DurationValue.Approximate( months, 0, 0, nanos );
			  assertEqual( result, DurationValue.Duration( 0, 0, 0, 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotThrowWhenInsideOverflowLimit()
		 internal virtual void ShouldNotThrowWhenInsideOverflowLimit()
		 {
			  // when
			  duration( 0, 0, long.MaxValue, 999_999_999 );

			  // then should not throw
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotThrowWhenInsideNegativeOverflowLimit()
		 internal virtual void ShouldNotThrowWhenInsideNegativeOverflowLimit()
		 {
			  // when
			  duration( 0, 0, long.MinValue, -999_999_999 );

			  // then should not throw
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnOverflowOnNanos()
		 internal virtual void ShouldThrowOnOverflowOnNanos()
		 {
			  // when
			  int nanos = 1_000_000_000;
			  long seconds = long.MaxValue;
			  AssertConstructorThrows( 0, 0, seconds, nanos );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnNegativeOverflowOnNanos()
		 internal virtual void ShouldThrowOnNegativeOverflowOnNanos()
		 {
			  // when
			  int nanos = -1_000_000_000;
			  long seconds = long.MinValue;
			  AssertConstructorThrows( 0, 0, seconds, nanos );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnOverflowOnDays()
		 internal virtual void ShouldThrowOnOverflowOnDays()
		 {
			  // when
			  long days = long.MaxValue / TemporalUtil.SECONDS_PER_DAY;
			  long seconds = long.MaxValue - days * TemporalUtil.SECONDS_PER_DAY;
			  AssertConstructorThrows( 0, days, seconds + 1, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnNegativeOverflowOnDays()
		 internal virtual void ShouldThrowOnNegativeOverflowOnDays()
		 {
			  // when
			  long days = long.MinValue / TemporalUtil.SECONDS_PER_DAY;
			  long seconds = long.MinValue - days * TemporalUtil.SECONDS_PER_DAY;
			  AssertConstructorThrows( 0, days, seconds - 1, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnOverflowOnMonths()
		 internal virtual void ShouldThrowOnOverflowOnMonths()
		 {
			  // when
			  long months = long.MaxValue / TemporalUtil.AVG_SECONDS_PER_MONTH;
			  long seconds = long.MaxValue - months * TemporalUtil.AVG_SECONDS_PER_MONTH;
			  AssertConstructorThrows( months, 0, seconds + 1, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnNegativeOverflowOnMonths()
		 internal virtual void ShouldThrowOnNegativeOverflowOnMonths()
		 {
			  // when
			  long months = long.MinValue / TemporalUtil.AVG_SECONDS_PER_MONTH;
			  long seconds = long.MinValue - months * TemporalUtil.AVG_SECONDS_PER_MONTH;
			  AssertConstructorThrows( months, 0, seconds - 1, 0 );
		 }

		 private void AssertConstructorThrows( long months, long days, long seconds, int nanos )
		 {
			  InvalidValuesArgumentException e = assertThrows( typeof( InvalidValuesArgumentException ), () => duration(months, days, seconds, nanos) );

			  assertThat( e.Message, Matchers.allOf( Matchers.containsString( "Invalid value for duration" ), Matchers.containsString( "months=" + months ), Matchers.containsString( "days=" + days ), Matchers.containsString( "seconds=" + seconds ), Matchers.containsString( "nanos=" + nanos ) ) );
		 }
	}

}