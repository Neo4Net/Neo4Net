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
namespace Neo4Net.Kernel.Api.Index
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Point = Neo4Net.GraphDb.Spatial.Point;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Race = Neo4Net.Test.Race;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ArrayEncoderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.ThreadingRule threads = new org.Neo4Net.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threads = new ThreadingRule();

		 private static readonly char?[] _base64chars = new char?[]{ 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/' };
		 private const char ARRAY_ENTRY_SEPARATOR = '|';
		 private const char PADDING = '=';

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void encodingShouldContainOnlyBase64EncodingChars()
		 public virtual void EncodingShouldContainOnlyBase64EncodingChars()
		 {
			  string[] array = new string[] { "This string is long enough for BASE64 to emit a line break, making the encoding platform dependant.", "Something else to trigger padding." };
			  string encoded = ArrayEncoder.Encode( Values.of( array ) );

			  int separators = 0;
			  bool padding = false;
			  for ( int i = 0; i < encoded.Length; i++ )
			  {
					char character = encoded[i];
					if ( character == ARRAY_ENTRY_SEPARATOR )
					{
						 padding = false;
						 separators++;
					}
					else if ( padding )
					{
						 assertEquals( PADDING, character );
					}
					else if ( character == PADDING )
					{
						 padding = true;
					}
					else
					{
						 assertTrue( "Char " + character + " at position " + i + " is not a valid Base64 encoded char", ArrayUtil.contains( _base64chars, character ) );
					}
			  }
			  assertEquals( array.Length, separators );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeArrays()
		 public virtual void ShouldEncodeArrays()
		 {
			  AssertEncoding( "D1.0|2.0|3.0|", new int[]{ 1, 2, 3 } );
			  AssertEncoding( "Ztrue|false|", new bool[]{ true, false } );
			  AssertEncoding( "LYWxp|YXJl|eW91|b2s=|", new string[]{ "ali", "are", "you", "ok" } );
			  AssertEncoding( "", new string[]{} );
			  AssertEncoding( "P1:4326:1.234;2.567|1:4326:2.345;5.678|2:9157:3.0;4.0;5.0|", new Point[]{ Values.pointValue( CoordinateReferenceSystem.WGS84, 1.234, 2.567 ), Values.pointValue( CoordinateReferenceSystem.WGS84, 2.345, 5.678 ), Values.pointValue( CoordinateReferenceSystem.Cartesian_3D, 3, 4, 5 ) } );
			  AssertEncoding( "T1991-03-05|1992-04-06|", new LocalDate[]{ DateValue.date( 1991, 3, 5 ).asObjectCopy(), DateValue.date(1992, 4, 6).asObjectCopy() } );
			  AssertEncoding( "T12:45:13.000008676|05:04:50.000000076|", new LocalTime[]{ LocalTimeValue.localTime( 12, 45, 13, 8676 ).asObjectCopy(), LocalTimeValue.localTime(5, 4, 50, 76).asObjectCopy() } );
			  AssertEncoding( "T1991-03-05T12:45:13.000008676|1992-04-06T05:04:50.000000076|", new DateTime[]{ LocalDateTimeValue.localDateTime( 1991, 3, 5, 12, 45, 13, 8676 ).asObjectCopy(), LocalDateTimeValue.localDateTime(1992, 4, 6, 5, 4, 50, 76).asObjectCopy() } );
			  AssertEncoding( "T02:45:13.000008676Z|01:05:00.0000003+01:00|05:04:50.000000076+05:00|", new OffsetTime[]{ TimeValue.time( 2, 45, 13, 8676, UTC ).asObjectCopy(), TimeValue.time(OffsetTime.ofInstant(Instant.ofEpochSecond(300, 300), ZoneId.of("Europe/Stockholm"))).asObjectCopy(), TimeValue.time(5, 4, 50, 76, "+05:00").asObjectCopy() } );
			  AssertEncoding( "T1991-03-05T02:45:13.000008676Z|1991-03-05T02:45:13.000008676+01:00[Europe/Stockholm]|1992-04-06T05:04:50.000000076+05:00|", new ZonedDateTime[]{ DateTimeValue.datetime( 1991, 3, 5, 2, 45, 13, 8676, UTC ).asObjectCopy(), DateTimeValue.datetime(1991, 3, 5, 2, 45, 13, 8676, ZoneId.of("Europe/Stockholm")).asObjectCopy(), DateTimeValue.datetime(1992, 4, 6, 5, 4, 50, 76, "+05:00").asObjectCopy() } );
			  AssertEncoding( "AP165Y11M3DT5.000000012S|P166Y4DT6.000000005S|", new TemporalAmount[]{ DurationValue.duration( 1991, 3, 5, 12 ).asObjectCopy(), DurationValue.duration(1992, 4, 6, 5).asObjectCopy() } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeProperlyWithMultipleThreadsRacing() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEncodeProperlyWithMultipleThreadsRacing()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] INPUT = { "These strings need to be longer than 57 bytes, because that is the line wrapping length of BASE64.", "This next line is also long. The number of strings in this array is the number of threads to use.", "Each thread will get a different string as input to encode, and ensure the result is always the same.", "Should the result of an encoding differ even once, the thread will yield a negative overall result.", "If any of the threads yields a negative result, the test will fail, since that should not happen.", "All threads are allowed to run together for a predetermined amount of time, to try to get contention.", "This predetermined time is the minimum runtime of the test, since the timer starts after all threads.", "The idea to use the input data as documentation for the test was just a cute thing I came up with.", "Since my imagination for coming up with test data is usually poor, I figured I'd do something useful.", "Hopefully this isn't just nonsensical drivel, and maybe, just maybe someone might actually read it." };
			  string[] input = new string[] { "These strings need to be longer than 57 bytes, because that is the line wrapping length of BASE64.", "This next line is also long. The number of strings in this array is the number of threads to use.", "Each thread will get a different string as input to encode, and ensure the result is always the same.", "Should the result of an encoding differ even once, the thread will yield a negative overall result.", "If any of the threads yields a negative result, the test will fail, since that should not happen.", "All threads are allowed to run together for a predetermined amount of time, to try to get contention.", "This predetermined time is the minimum runtime of the test, since the timer starts after all threads.", "The idea to use the input data as documentation for the test was just a cute thing I came up with.", "Since my imagination for coming up with test data is usually poor, I figured I'd do something useful.", "Hopefully this isn't just nonsensical drivel, and maybe, just maybe someone might actually read it." };

			  RaceEncode( input, ArrayEncoder.encode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void raceEncode(String[] INPUT, System.Func<org.Neo4Net.values.storable.Value, String> encodeFunction) throws Throwable
		 private void RaceEncode( string[] input, System.Func<Value, string> encodeFunction )
		 {
			  Race race = new Race();
			  foreach ( string input in input )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.values.storable.Value inputValue = org.Neo4Net.values.storable.Values.of(new String[]{input});
					Value inputValue = Values.of( new string[]{ input } );
					race.AddContestant(() =>
					{
					 string first = encodeFunction( inputValue );
					 for ( int i = 0; i < 1000; i++ )
					 {
						  string encoded = encodeFunction( inputValue );
						  assertEquals( "Each attempt at encoding should yield the same result. Turns out that first one was '" + first + "', yet another one was '" + encoded + "'", first, encoded );
					 }
					});
			  }
			  race.Go();
		 }

		 private void AssertEncoding( string expected, object toEncode )
		 {
			  assertEquals( expected, ArrayEncoder.Encode( Values.of( toEncode ) ) );
		 }
	}

}