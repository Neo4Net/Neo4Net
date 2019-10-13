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
	/// <seealso cref="Clock"/> that support nano time resolution. </summary>
	/// <seealso cref= Clocks </seealso>
	public class SystemNanoClock : Clock
	{
		 internal static readonly SystemNanoClock Instance = new SystemNanoClock();

		 protected internal SystemNanoClock()
		 {
			  // please use shared instance
		 }

		 public override ZoneId Zone
		 {
			 get
			 {
				  return ZoneOffset.UTC;
			 }
		 }

		 public override Clock WithZone( ZoneId zone )
		 {
			  return Clock.system( zone ); // the users of this method do not need a NanoClock
		 }

		 public override Instant Instant()
		 {
			  return Instant.ofEpochMilli( Millis() );
		 }

		 public override long Millis()
		 {
			  return DateTimeHelper.CurrentUnixTimeMillis();
		 }

		 /// <summary>
		 /// It is <em>only</em> useful for comparing values returned from the same clock, as the wall clock time of this method is arbitrary.
		 /// </summary>
		 /// <returns> current nano time of the system. </returns>
		 public virtual long Nanos()
		 {
			  return System.nanoTime();
		 }
	}

}