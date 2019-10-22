/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.stresstests.transaction.checkpoint.tracers
{
	using Histogram = org.HdrHistogram.Histogram;


	using CheckPointTracer = Neo4Net.Kernel.impl.transaction.tracing.CheckPointTracer;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogCheckPointEvent = Neo4Net.Kernel.impl.transaction.tracing.LogCheckPointEvent;
	using LogForceEvent = Neo4Net.Kernel.impl.transaction.tracing.LogForceEvent;
	using LogForceWaitEvent = Neo4Net.Kernel.impl.transaction.tracing.LogForceWaitEvent;
	using LogRotateEvent = Neo4Net.Kernel.impl.transaction.tracing.LogRotateEvent;
	using SerializeTransactionEvent = Neo4Net.Kernel.impl.transaction.tracing.SerializeTransactionEvent;
	using StoreApplyEvent = Neo4Net.Kernel.impl.transaction.tracing.StoreApplyEvent;
	using TransactionEvent = Neo4Net.Kernel.impl.transaction.tracing.TransactionEvent;
	using TransactionTracer = Neo4Net.Kernel.impl.transaction.tracing.TransactionTracer;

	public class TimerTransactionTracer : TransactionTracer, CheckPointTracer
	{
		 private static volatile long _logForceBegin;
		 private static volatile long _logCheckPointBegin;
		 private static volatile long _logRotateBegin;
		 private static readonly Histogram _logForceTimes = new Histogram( 1000, TimeUnit.MINUTES.toNanos( 45 ), 0 );
		 private static readonly Histogram _logRotateTimes = new Histogram( 1000, TimeUnit.MINUTES.toNanos( 45 ), 0 );
		 private static readonly Histogram _logCheckPointTimes = new Histogram( 1000, TimeUnit.MINUTES.toNanos( 45 ), 0 );

		 public static void PrintStats( PrintStream @out )
		 {
			  PrintStat( @out, "Log force millisecond percentiles:", _logForceTimes );
			  PrintStat( @out, "Log rotate millisecond percentiles:", _logRotateTimes );
			  PrintStat( @out, "Log check point millisecond percentiles:", _logCheckPointTimes );
		 }

		 private static void PrintStat( PrintStream @out, string message, Histogram histogram )
		 {
			  @out.println( message );
			  histogram.outputPercentileDistribution( @out, 1000000.0 );
			  @out.println();
		 }

		 private static readonly LogForceEvent LOG_FORCE_EVENT = new LogForceEventAnonymousInnerClass();

		 private class LogForceEventAnonymousInnerClass : LogForceEvent
		 {
			 public void close()
			 {
				  long elapsedNanos = System.nanoTime() - _logForceBegin;
				  _logForceTimes.recordValue( elapsedNanos );
			 }
		 }

		 private static readonly LogCheckPointEvent LOG_CHECK_POINT_EVENT = new LogCheckPointEventAnonymousInnerClass();

		 private class LogCheckPointEventAnonymousInnerClass : LogCheckPointEvent
		 {
			 public LogForceWaitEvent beginLogForceWait()
			 {
				  return Neo4Net.Kernel.impl.transaction.tracing.LogForceWaitEvent_Fields.Null;
			 }

			 public LogForceEvent beginLogForce()
			 {
				  _logForceBegin = System.nanoTime();
				  return LOG_FORCE_EVENT;
			 }

			 public void close()
			 {
				  long elapsedNanos = System.nanoTime() - _logCheckPointBegin;
				  _logCheckPointTimes.recordValue( elapsedNanos );
			 }
		 }

		 private static readonly LogRotateEvent LOG_ROTATE_EVENT = new LogRotateEventAnonymousInnerClass();

		 private class LogRotateEventAnonymousInnerClass : LogRotateEvent
		 {
			 public void close()
			 {
				  long elapsedNanos = System.nanoTime() - _logRotateBegin;
				  _logRotateTimes.recordValue( elapsedNanos );
			 }
		 }

		 private static readonly LogAppendEvent LOG_APPEND_EVENT = new LogAppendEventAnonymousInnerClass();

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
				  _logRotateBegin = System.nanoTime();
				  return LOG_ROTATE_EVENT;
			 }

			 public SerializeTransactionEvent beginSerializeTransaction()
			 {
				  return Neo4Net.Kernel.impl.transaction.tracing.SerializeTransactionEvent_Fields.Null;
			 }

			 public LogForceWaitEvent beginLogForceWait()
			 {
				  return Neo4Net.Kernel.impl.transaction.tracing.LogForceWaitEvent_Fields.Null;
			 }

			 public LogForceEvent beginLogForce()
			 {
				  _logForceBegin = System.nanoTime();
				  return LOG_FORCE_EVENT;
			 }
		 }

		 private static readonly CommitEvent COMMIT_EVENT = new CommitEventAnonymousInnerClass();

		 private class CommitEventAnonymousInnerClass : CommitEvent
		 {
			 public void close()
			 {
			 }

			 public LogAppendEvent beginLogAppend()
			 {
				  return LOG_APPEND_EVENT;
			 }

			 public StoreApplyEvent beginStoreApply()
			 {
				  return Neo4Net.Kernel.impl.transaction.tracing.StoreApplyEvent_Fields.Null;
			 }
		 }

		 private static readonly TransactionEvent TRANSACTION_EVENT = new TransactionEventAnonymousInnerClass();

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
				  return COMMIT_EVENT;
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

		 public override TransactionEvent BeginTransaction()
		 {
			  return TRANSACTION_EVENT;
		 }

		 public override LogCheckPointEvent BeginCheckPoint()
		 {
			  _logCheckPointBegin = System.nanoTime();
			  return LOG_CHECK_POINT_EVENT;
		 }
	}

}