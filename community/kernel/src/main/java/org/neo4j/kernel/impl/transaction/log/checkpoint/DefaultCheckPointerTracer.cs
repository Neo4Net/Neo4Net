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
namespace Org.Neo4j.Kernel.impl.transaction.log.checkpoint
{

	using CheckPointTracer = Org.Neo4j.Kernel.impl.transaction.tracing.CheckPointTracer;
	using LogCheckPointEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogCheckPointEvent;
	using LogForceEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogForceEvent;
	using LogForceWaitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogForceWaitEvent;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using Clocks = Org.Neo4j.Time.Clocks;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

	public class DefaultCheckPointerTracer : CheckPointTracer, CheckPointerMonitor
	{
		 public interface Monitor
		 {
			  void LastCheckPointEventDuration( long millis );
		 }

		 private readonly SystemNanoClock _clock;
		 private readonly Monitor _monitor;
		 private readonly JobScheduler _jobScheduler;

		 private readonly AtomicLong _counter = new AtomicLong();
		 private readonly AtomicLong _accumulatedTotalTimeNanos = new AtomicLong();

		 private volatile long _startTimeNanos;

		 private LogCheckPointEvent logCheckPointEvent = new LogCheckPointEventAnonymousInnerClass();

		 private class LogCheckPointEventAnonymousInnerClass : LogCheckPointEvent
		 {
			 public void close()
			 {
				  outerInstance.updateCountersAndNotifyListeners();
			 }

			 public LogForceWaitEvent beginLogForceWait()
			 {
				  return Org.Neo4j.Kernel.impl.transaction.tracing.LogForceWaitEvent_Fields.Null;
			 }

			 public LogForceEvent beginLogForce()
			 {
				  return Org.Neo4j.Kernel.impl.transaction.tracing.LogForceEvent_Fields.Null;
			 }
		 }

		 public DefaultCheckPointerTracer( Monitor monitor, JobScheduler jobScheduler ) : this( Clocks.nanoClock(), monitor, jobScheduler )
		 {
		 }

		 public DefaultCheckPointerTracer( SystemNanoClock clock, Monitor monitor, JobScheduler jobScheduler )
		 {
			  this._clock = clock;
			  this._monitor = monitor;
			  this._jobScheduler = jobScheduler;
		 }

		 public override LogCheckPointEvent BeginCheckPoint()
		 {
			  _startTimeNanos = _clock.nanos();
			  return logCheckPointEvent;
		 }

		 public override long NumberOfCheckPointEvents()
		 {
			  return _counter.get();
		 }

		 public override long CheckPointAccumulatedTotalTimeMillis()
		 {
			  return TimeUnit.NANOSECONDS.toMillis( _accumulatedTotalTimeNanos.get() );
		 }

		 private void UpdateCountersAndNotifyListeners()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long lastEventTime = clock.nanos() - startTimeNanos;
			  long lastEventTime = _clock.nanos() - _startTimeNanos;

			  // update counters
			  _counter.incrementAndGet();
			  _accumulatedTotalTimeNanos.addAndGet( lastEventTime );

			  // notify async
			  _jobScheduler.schedule(Group.METRICS_EVENT, () =>
			  {
				long millis = TimeUnit.NANOSECONDS.toMillis( lastEventTime );
				_monitor.lastCheckPointEventDuration( millis );
			  });
		 }
	}

}