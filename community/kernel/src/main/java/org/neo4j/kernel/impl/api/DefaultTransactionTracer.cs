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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using LogAppendEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogForceEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogForceEvent;
	using LogForceWaitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogForceWaitEvent;
	using LogRotateEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogRotateEvent;
	using SerializeTransactionEvent = Org.Neo4j.Kernel.impl.transaction.tracing.SerializeTransactionEvent;
	using StoreApplyEvent = Org.Neo4j.Kernel.impl.transaction.tracing.StoreApplyEvent;
	using TransactionEvent = Org.Neo4j.Kernel.impl.transaction.tracing.TransactionEvent;
	using TransactionTracer = Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using Clocks = Org.Neo4j.Time.Clocks;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

	public class DefaultTransactionTracer : TransactionTracer, LogRotationMonitor
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_logRotateEvent = this.updateCountersAndNotifyListeners;
		}

		 public interface Monitor
		 {
			  void LastLogRotationEventDuration( long millis );
		 }

		 private readonly SystemNanoClock _clock;
		 private readonly Monitor _monitor;
		 private readonly JobScheduler _jobScheduler;

		 private readonly AtomicLong _counter = new AtomicLong();
		 private readonly AtomicLong _accumulatedTotalTimeNanos = new AtomicLong();

		 private long _startTimeNanos;

		 private LogRotateEvent _logRotateEvent;

		 private readonly LogAppendEvent logAppendEvent = new LogAppendEventAnonymousInnerClass();

		 private class LogAppendEventAnonymousInnerClass : LogAppendEvent
		 {
			 public void close()
			 {
			 }

			 public bool LogRotated
			 {
				 set
				 {
   
				 }
			 }

			 public LogRotateEvent beginLogRotate()
			 {
				  outerInstance.startTimeNanos = outerInstance.clock.nanos();
				  return outerInstance.logRotateEvent;
			 }

			 public SerializeTransactionEvent beginSerializeTransaction()
			 {
				  return Org.Neo4j.Kernel.impl.transaction.tracing.SerializeTransactionEvent_Fields.Null;
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

		 private readonly CommitEvent commitEvent = new CommitEventAnonymousInnerClass();

		 private class CommitEventAnonymousInnerClass : CommitEvent
		 {
			 public void close()
			 {
			 }

			 public LogAppendEvent beginLogAppend()
			 {
				  return logAppendEvent;
			 }

			 public StoreApplyEvent beginStoreApply()
			 {
				  return Org.Neo4j.Kernel.impl.transaction.tracing.StoreApplyEvent_Fields.Null;
			 }
		 }

		 private readonly TransactionEvent transactionEvent = new TransactionEventAnonymousInnerClass();

		 private class TransactionEventAnonymousInnerClass : TransactionEvent
		 {

			 public bool Success
			 {
				 set
				 {
				 }
			 }

			 public bool Failure
			 {
				 set
				 {
				 }
			 }

			 public CommitEvent beginCommitEvent()
			 {
				  return commitEvent;
			 }

			 public void close()
			 {
			 }

			 public string TransactionWriteState
			 {
				 set
				 {
				 }
			 }

			 public bool ReadOnly
			 {
				 set
				 {
				 }
			 }
		 }

		 public DefaultTransactionTracer( Monitor monitor, JobScheduler jobScheduler ) : this( Clocks.nanoClock(), monitor, jobScheduler )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
		 }

		 public DefaultTransactionTracer( SystemNanoClock clock, Monitor monitor, JobScheduler jobScheduler )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._clock = clock;
			  this._monitor = monitor;
			  this._jobScheduler = jobScheduler;
		 }

		 public override TransactionEvent BeginTransaction()
		 {
			  return transactionEvent;
		 }

		 public override long NumberOfLogRotationEvents()
		 {
			  return _counter.get();
		 }

		 public override long LogRotationAccumulatedTotalTimeMillis()
		 {
			  return TimeUnit.NANOSECONDS.toMillis( _accumulatedTotalTimeNanos.get() );
		 }

		 private void UpdateCountersAndNotifyListeners()
		 {
			  _counter.incrementAndGet();
			  long lastEventTime = _clock.nanos() - _startTimeNanos;
			  _accumulatedTotalTimeNanos.addAndGet( lastEventTime );
			  _jobScheduler.schedule(Group.METRICS_EVENT, () =>
			  {
				long millis = TimeUnit.NANOSECONDS.toMillis( lastEventTime );
				_monitor.lastLogRotationEventDuration( millis );
			  });
		 }
	}

}