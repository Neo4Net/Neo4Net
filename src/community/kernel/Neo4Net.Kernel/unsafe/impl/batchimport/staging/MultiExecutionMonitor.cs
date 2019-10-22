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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Clocks = Neo4Net.Time.Clocks;

	/// <summary>
	/// <seealso cref="ExecutionMonitor"/> that wraps several other monitors. Each wrapper monitor can still specify
	/// individual poll frequencies and this <seealso cref="MultiExecutionMonitor"/> will make that happen.
	/// </summary>
	public class MultiExecutionMonitor : ExecutionMonitor
	{
		 private readonly Clock _clock;
		 private readonly ExecutionMonitor[] _monitors;
		 private readonly long[] _endTimes;

		 public MultiExecutionMonitor( params ExecutionMonitor[] monitors ) : this( Clocks.systemClock(), monitors )
		 {
		 }

		 public MultiExecutionMonitor( Clock clock, params ExecutionMonitor[] monitors )
		 {
			  this._clock = clock;
			  this._monitors = monitors;
			  this._endTimes = new long[monitors.Length];
			  FillEndTimes();
		 }

		 public override void Initialize( DependencyResolver dependencyResolver )
		 {
			  foreach ( ExecutionMonitor monitor in _monitors )
			  {
					monitor.Initialize( dependencyResolver );
			  }
		 }

		 public override void Start( StageExecution execution )
		 {
			  foreach ( ExecutionMonitor monitor in _monitors )
			  {
					monitor.Start( execution );
			  }
		 }

		 public override void End( StageExecution execution, long totalTimeMillis )
		 {
			  foreach ( ExecutionMonitor monitor in _monitors )
			  {
					monitor.End( execution, totalTimeMillis );
			  }
		 }

		 public override void Done( bool successful, long totalTimeMillis, string additionalInformation )
		 {
			  foreach ( ExecutionMonitor monitor in _monitors )
			  {
					monitor.Done( successful, totalTimeMillis, additionalInformation );
			  }
		 }

		 public override long NextCheckTime()
		 {
			  // Find the lowest of all end times
			  long low = _endTimes[0];
			  for ( int i = 1; i < _monitors.Length; i++ )
			  {
					long thisLow = _endTimes[i];
					if ( thisLow < low )
					{
						 low = thisLow;
					}
			  }
			  return low;
		 }

		 private void FillEndTimes()
		 {
			  for ( int i = 0; i < _monitors.Length; i++ )
			  {
					_endTimes[i] = _monitors[i].nextCheckTime();
			  }
		 }

		 public override void Check( StageExecution execution )
		 {
			  long currentTimeMillis = _clock.millis();
			  for ( int i = 0; i < _monitors.Length; i++ )
			  {
					if ( currentTimeMillis >= _endTimes[i] )
					{
						 _monitors[i].check( execution );
						 _endTimes[i] = _monitors[i].nextCheckTime();
					}
			  }
		 }
	}

}