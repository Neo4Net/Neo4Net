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
namespace Neo4Net.Kernel.impl.store.counts
{

	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	public class CallTrackingClock : SystemNanoClock
	{
		 private readonly SystemNanoClock _actual;
		 private volatile int _nanosCalls;

		 public CallTrackingClock( SystemNanoClock actual )
		 {
			  this._actual = actual;
		 }

		 public override ZoneId Zone
		 {
			 get
			 {
				  return _actual.Zone;
			 }
		 }

		 public override Clock WithZone( ZoneId zone )
		 {
			  return _actual.withZone( zone );
		 }

		 public override Instant Instant()
		 {
			  return _actual.instant();
		 }

		 public override long Millis()
		 {
			  return _actual.millis();
		 }

		 public override long Nanos()
		 {
			  try
			  {
					return _actual.nanos();
			  }
			  finally
			  {
					_nanosCalls++;
			  }
		 }

		 public virtual int CallsToNanos()
		 {
			  return _nanosCalls;
		 }
	}

}