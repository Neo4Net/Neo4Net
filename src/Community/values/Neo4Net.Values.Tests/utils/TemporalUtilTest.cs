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
namespace Neo4Net.Values.utils
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class TemporalUtilTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDoNothingForOffsetWithoutSeconds()
		 internal virtual void ShouldDoNothingForOffsetWithoutSeconds()
		 {
			  OffsetTime time = OffsetTime.of( 23, 30, 10, 0, ZoneOffset.ofHoursMinutes( -5, -30 ) );

			  OffsetTime truncatedTime = TemporalUtil.TruncateOffsetToMinutes( time );

			  assertEquals( time, truncatedTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTruncateOffsetSeconds()
		 internal virtual void ShouldTruncateOffsetSeconds()
		 {
			  OffsetTime time = OffsetTime.of( 14, 55, 50, 0, ZoneOffset.ofHoursMinutesSeconds( 2, 15, 45 ) );

			  OffsetTime truncatedTime = TemporalUtil.TruncateOffsetToMinutes( time );

			  assertEquals( OffsetTime.of( 14, 55, 5, 0, ZoneOffset.ofHoursMinutes( 2, 15 ) ), truncatedTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertNanosOfDayToUTCWhenOffsetIsZero()
		 internal virtual void ShouldConvertNanosOfDayToUTCWhenOffsetIsZero()
		 {
			  int nanosOfDayLocal = 42;

			  long nanosOfDayUTC = TemporalUtil.NanosOfDayToUTC( nanosOfDayLocal, 0 );

			  assertEquals( nanosOfDayLocal, nanosOfDayUTC );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertNanosOfDayToUTC()
		 internal virtual void ShouldConvertNanosOfDayToUTC()
		 {
			  int nanosOfDayLocal = 42;
			  Duration offsetDuration = Duration.ofMinutes( 35 );

			  long nanosOfDayUTC = TemporalUtil.NanosOfDayToUTC( nanosOfDayLocal, ( int ) offsetDuration.Seconds );

			  assertEquals( nanosOfDayLocal - offsetDuration.toNanos(), nanosOfDayUTC );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGetNanosOfDayUTC()
		 internal virtual void ShouldGetNanosOfDayUTC()
		 {
			  LocalTime localTime = LocalTime.of( 14, 19, 18, 123999 );
			  ZoneOffset offset = ZoneOffset.ofHours( -12 );
			  OffsetTime time = OffsetTime.of( localTime, offset );

			  long nanosOfDayUTC = TemporalUtil.GetNanosOfDayUTC( time );

			  long expectedNanosOfDayUTC = Duration.ofSeconds( localTime.toSecondOfDay() ).minus(offset.TotalSeconds, SECONDS).toNanos();

			  assertEquals( expectedNanosOfDayUTC + localTime.Nano, nanosOfDayUTC );
		 }
	}

}