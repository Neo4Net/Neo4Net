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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	public class RotationTimerFactory
	{
		 private SystemNanoClock _clock;
		 private long _timeoutMillis;

		 public RotationTimerFactory( SystemNanoClock clock, long timeoutMillis )
		 {
			  this._clock = clock;
			  this._timeoutMillis = timeoutMillis;
		 }

		 public virtual RotationTimer CreateTimer()
		 {
			  long startTimeNanos = _clock.nanos();
			  long timeoutNanos = TimeUnit.MILLISECONDS.toNanos( _timeoutMillis );
			  return new RotationTimer( this, startTimeNanos, startTimeNanos + timeoutNanos );
		 }

		 internal class RotationTimer
		 {
			 private readonly RotationTimerFactory _outerInstance;

			  internal long StartTimeNanos;
			  internal long DeadlineNanos;

			  internal RotationTimer( RotationTimerFactory outerInstance, long startTimeNanos, long deadlineNanos )
			  {
				  this._outerInstance = outerInstance;
					this.StartTimeNanos = startTimeNanos;
					this.DeadlineNanos = deadlineNanos;
			  }

			  public virtual bool TimedOut
			  {
				  get
				  {
						return outerInstance.clock.Nanos() > DeadlineNanos;
				  }
			  }

			  public virtual long ElapsedTimeMillis
			  {
				  get
				  {
						long elapsedNanos = outerInstance.clock.Nanos() - StartTimeNanos;
						return TimeUnit.NANOSECONDS.toMillis( elapsedNanos );
				  }
			  }

		 }
	}

}