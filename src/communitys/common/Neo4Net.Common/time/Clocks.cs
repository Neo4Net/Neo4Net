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
namespace Neo4Net.Time
{

	/// <summary>
	/// This class consists of {@code static} utility methods for operating
	/// on clocks. These utilities include factory methods for different type of clocks.
	/// </summary>
	public class Clocks
	{
		 private static readonly Clock _systemClock = Clock.systemUTC();

		 private Clocks()
		 {
			  // non-instantiable
		 }

		 /// <summary>
		 /// Returns system clock. </summary>
		 /// <returns> system clock </returns>
		 public static Clock SystemClock()
		 {
			  return _systemClock;
		 }

		 /// <summary>
		 /// Returns clock that allow to get current nanos. </summary>
		 /// <returns> clock with nano time support </returns>
		 public static SystemNanoClock NanoClock()
		 {
			  return SystemNanoClock.Instance;
		 }

		 /// <summary>
		 /// Return new fake clock instance. </summary>
		 /// <returns> fake clock </returns>
		 public static FakeClock FakeClock()
		 {
			  return new FakeClock();
		 }

		 /// <summary>
		 /// Return new fake clock instance. </summary>
		 /// <param name="initialTime"> initial fake clock time </param>
		 /// <param name="unit"> initialTime fake clock time unit </param>
		 /// <returns> fake clock </returns>
		 public static FakeClock FakeClock( long initialTime, TimeUnit unit )
		 {
			  return new FakeClock( initialTime, unit );
		 }

		 public static FakeClock FakeClock( TemporalAccessor initialTime )
		 {
			  return ( new FakeClock( initialTime.getLong( ChronoField.INSTANT_SECONDS ), TimeUnit.SECONDS ) ).forward( initialTime.getLong( ChronoField.NANO_OF_SECOND ), TimeUnit.NANOSECONDS );
		 }

		 /// <summary>
		 /// Returns a clock that ticks every time it is accessed </summary>
		 /// <param name="initialInstant"> initial time for clock </param>
		 /// <param name="tickDuration"> amount of time of each tick </param>
		 /// <returns> access tick clock </returns>
		 public static TickOnAccessClock TickOnAccessClock( Instant initialInstant, Duration tickDuration )
		 {
			  return new TickOnAccessClock( initialInstant, tickDuration );
		 }
	}

}