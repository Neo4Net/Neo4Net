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
namespace Neo4Net.Values.utils
{

	public sealed class TemporalUtil
	{
		 public const long NANOS_PER_SECOND = 1_000_000_000L;
		 public const long AVG_NANOS_PER_MONTH = 2_629_746_000_000_000L;

		 public static readonly long SecondsPerDay = DAYS.Duration.Seconds;
		 public const long AVG_SECONDS_PER_MONTH = 2_629_746;

		 /// <summary>
		 /// 30.4375 days = 30 days, 10 hours, 30 minutes </summary>
		 public const double AVG_DAYS_PER_MONTH = 365.2425 / 12;

		 private TemporalUtil()
		 {
		 }

		 public static OffsetTime TruncateOffsetToMinutes( OffsetTime value )
		 {
			  int offsetMinutes = value.Offset.TotalSeconds / 60;
			  ZoneOffset truncatedOffset = ZoneOffset.ofTotalSeconds( offsetMinutes * 60 );
			  return value.withOffsetSameInstant( truncatedOffset );
		 }

		 public static long NanosOfDayToUTC( long nanosOfDayLocal, int offsetSeconds )
		 {
			  return nanosOfDayLocal - offsetSeconds * NANOS_PER_SECOND;
		 }

		 public static long NanosOfDayToLocal( long nanosOfDayUTC, int offsetSeconds )
		 {
			  return nanosOfDayUTC + ( long ) offsetSeconds * NANOS_PER_SECOND;
		 }

		 public static long GetNanosOfDayUTC( OffsetTime value )
		 {
			  long secondsOfDayLocal = value.toLocalTime().toSecondOfDay();
			  long secondsOffset = value.Offset.TotalSeconds;
			  return ( secondsOfDayLocal - secondsOffset ) * NANOS_PER_SECOND + value.Nano;
		 }
	}

}