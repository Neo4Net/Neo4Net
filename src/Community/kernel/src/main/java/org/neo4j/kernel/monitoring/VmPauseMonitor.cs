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
namespace Neo4Net.Kernel.monitoring
{

	using Log = Neo4Net.Logging.Log;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.nanoTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.currentThread;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requireNonNegative;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requirePositive;

	public class VmPauseMonitor
	{
		 private readonly long _measurementDurationNs;
		 private readonly long _stallAlertThresholdNs;
		 private readonly Log _log;
		 private readonly JobScheduler _jobScheduler;
		 private readonly System.Action<VmPauseInfo> _listener;
		 private JobHandle _job;

		 public VmPauseMonitor( Duration measureInterval, Duration stallAlertThreshold, Log log, JobScheduler jobScheduler, System.Action<VmPauseInfo> listener )
		 {
			  this._measurementDurationNs = requirePositive( measureInterval.toNanos() );
			  this._stallAlertThresholdNs = requireNonNegative( stallAlertThreshold.toNanos() );
			  this._log = requireNonNull( log );
			  this._jobScheduler = requireNonNull( jobScheduler );
			  this._listener = requireNonNull( listener );
		 }

		 public virtual void Start()
		 {
			  _log.debug( "Starting VM pause monitor" );
			  checkState( _job == null, "VM pause monitor is already started" );
			  _job = requireNonNull( _jobScheduler.schedule( Group.VM_PAUSE_MONITOR, this.run ) );
		 }

		 public virtual void Stop()
		 {
			  _log.debug( "Stopping VM pause monitor" );
			  checkState( _job != null, "VM pause monitor is not started" );
			  _job.cancel( true );
			  _job = null;
		 }

		 private void Run()
		 {
			  try
			  {
					Monitor();
			  }
			  catch ( InterruptedException )
			  {
					_log.debug( "VM pause monitor stopped" );
			  }
			  catch ( Exception e )
			  {
					_log.debug( "VM pause monitor failed", e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting void monitor() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void Monitor()
		 {
			  GcStats lastGcStats = GetGcStats();
			  long nextCheckPoint = nanoTime() + _measurementDurationNs;

			  while ( !Stopped )
			  {
					NANOSECONDS.sleep( _measurementDurationNs );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long now = nanoTime();
					long now = nanoTime();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long pauseNs = max(0L, now - nextCheckPoint);
					long pauseNs = max( 0L, now - nextCheckPoint );
					nextCheckPoint = now + _measurementDurationNs;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final GcStats gcStats = getGcStats();
					GcStats gcStats = GetGcStats();
					if ( pauseNs >= _stallAlertThresholdNs )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final VmPauseInfo pauseInfo = new VmPauseInfo(NANOSECONDS.toMillis(pauseNs), gcStats.time - lastGcStats.time, gcStats.count - lastGcStats.count);
						 VmPauseInfo pauseInfo = new VmPauseInfo( NANOSECONDS.toMillis( pauseNs ), gcStats.Time - lastGcStats.Time, gcStats.Count - lastGcStats.Count );
						 _listener.accept( pauseInfo );
					}
					lastGcStats = gcStats;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("MethodMayBeStatic") @VisibleForTesting boolean isStopped()
		 internal virtual bool Stopped
		 {
			 get
			 {
				  return currentThread().Interrupted;
			 }
		 }

		 public class VmPauseInfo
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long PauseTimeConflict;
			  internal readonly long GcTime;
			  internal readonly long GcCount;

			  internal VmPauseInfo( long pauseTime, long gcTime, long gcCount )
			  {
					this.PauseTimeConflict = pauseTime;
					this.GcTime = gcTime;
					this.GcCount = gcCount;
			  }

			  public virtual long PauseTime
			  {
				  get
				  {
						return PauseTimeConflict;
				  }
			  }

			  public override string ToString()
			  {
					return format( "{pauseTime=%d, gcTime=%d, gcCount=%d}", PauseTimeConflict, GcTime, GcCount );
			  }
		 }

		 private static GcStats GetGcStats()
		 {
			  long time = 0;
			  long count = 0;
			  foreach ( GarbageCollectorMXBean gcBean in ManagementFactory.GarbageCollectorMXBeans )
			  {
					time += gcBean.CollectionTime;
					count += gcBean.CollectionCount;
			  }
			  return new GcStats( time, count );
		 }

		 private class GcStats
		 {
			  internal readonly long Time;
			  internal readonly long Count;

			  internal GcStats( long time, long count )
			  {
					this.Time = time;
					this.Count = count;
			  }
		 }
	}

}