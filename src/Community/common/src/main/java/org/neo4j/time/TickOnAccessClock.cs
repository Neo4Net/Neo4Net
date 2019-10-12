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
namespace Neo4Net.Time
{

	/// <summary>
	/// A <seealso cref="java.time.Clock"/> that ticks every time it is accessed.
	/// </summary>
	public class TickOnAccessClock : Clock
	{
		 private Instant _currentInstant;
		 private readonly Duration _tickDuration;

		 internal TickOnAccessClock( Instant initialTime, Duration tickDuration )
		 {
			  this._currentInstant = initialTime;
			  this._tickDuration = tickDuration;
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
			  throw new System.NotSupportedException();
		 }

		 public override Instant Instant()
		 {
			  Instant instant = _currentInstant;
			  Tick();
			  return instant;
		 }

		 private void Tick()
		 {
			  _currentInstant = _currentInstant.plus( _tickDuration );
		 }
	}

}