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
namespace Neo4Net.causalclustering.core.consensus.schedule
{
	using Test = org.junit.Test;

	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using FakeClockJobScheduler = Neo4Net.Test.FakeClockJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.schedule.TimeoutFactory.fixedTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.schedule.Timer.CancelMode.SYNC_WAIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.schedule.TimerServiceTest.Timers.TIMER_A;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.schedule.TimerServiceTest.Timers.TIMER_B;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;

	public class TimerServiceTest
	{
		private bool InstanceFieldsInitialized = false;

		public TimerServiceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_timerService = new TimerService( _scheduler, NullLogProvider.Instance );
			_timerA = _timerService.create( TIMER_A, _group, _handlerA );
			_timerB = _timerService.create( TIMER_B, _group, _handlerB );
		}

		 private readonly Group _group = Group.RAFT_TIMER;

		 private readonly TimeoutHandler _handlerA = mock( typeof( TimeoutHandler ) );
		 private readonly TimeoutHandler _handlerB = mock( typeof( TimeoutHandler ) );

		 private readonly FakeClockJobScheduler _scheduler = new FakeClockJobScheduler();
		 private TimerService _timerService;

		 private Timer _timerA;
		 private Timer _timerB;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotInvokeHandlerBeforeTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotInvokeHandlerBeforeTimeout()
		 {
			  // given
			  _timerA.set( fixedTimeout( 1000, MILLISECONDS ) );

			  // when
			  _scheduler.forward( 999, MILLISECONDS );

			  // then
			  verify( _handlerA, never() ).onTimeout(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeHandlerOnTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeHandlerOnTimeout()
		 {
			  // given
			  _timerA.set( fixedTimeout( 1000, MILLISECONDS ) );

			  // when
			  _scheduler.forward( 1000, MILLISECONDS );

			  // then
			  verify( _handlerA, times( 1 ) ).onTimeout( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeHandlerAfterTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeHandlerAfterTimeout()
		 {
			  // given
			  _timerA.set( fixedTimeout( 1, SECONDS ) );

			  // when
			  _scheduler.forward( 1001, MILLISECONDS );

			  // then
			  verify( _handlerA, times( 1 ) ).onTimeout( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeMultipleHandlersOnDifferentTimeouts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeMultipleHandlersOnDifferentTimeouts()
		 {
			  // given
			  _timerA.set( fixedTimeout( 1, SECONDS ) );
			  _timerB.set( fixedTimeout( 2, SECONDS ) );

			  // when
			  _scheduler.forward( 1, SECONDS );

			  // then
			  verify( _handlerA, times( 1 ) ).onTimeout( _timerA );
			  verify( _handlerB, never() ).onTimeout(any());

			  // given
			  reset( _handlerA );
			  reset( _handlerB );

			  // when
			  _scheduler.forward( 1, SECONDS );

			  // then
			  verify( _handlerA, never() ).onTimeout(any());
			  verify( _handlerB, times( 1 ) ).onTimeout( _timerB );

			  // given
			  reset( _handlerA );
			  reset( _handlerB );

			  // when
			  _scheduler.forward( 1, SECONDS );

			  // then
			  verify( _handlerA, never() ).onTimeout(any());
			  verify( _handlerB, never() ).onTimeout(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeMultipleHandlersOnSameTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeMultipleHandlersOnSameTimeout()
		 {
			  // given
			  _timerA.set( fixedTimeout( 1, SECONDS ) );
			  _timerB.set( fixedTimeout( 1, SECONDS ) );

			  // when
			  _scheduler.forward( 1, SECONDS );

			  // then
			  verify( _handlerA, times( 1 ) ).onTimeout( _timerA );
			  verify( _handlerB, times( 1 ) ).onTimeout( _timerB );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeTimersOnExplicitInvocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeTimersOnExplicitInvocation()
		 {
			  // when
			  _timerService.invoke( TIMER_A );

			  // then
			  verify( _handlerA, times( 1 ) ).onTimeout( _timerA );
			  verify( _handlerB, never() ).onTimeout(any());

			  // given
			  reset( _handlerA );
			  reset( _handlerB );

			  // when
			  _timerService.invoke( TIMER_B );

			  // then
			  verify( _handlerA, never() ).onTimeout(any());
			  verify( _handlerB, times( 1 ) ).onTimeout( _timerB );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutAfterReset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutAfterReset()
		 {
			  // given
			  _timerA.set( fixedTimeout( 1, SECONDS ) );

			  // when
			  _scheduler.forward( 900, MILLISECONDS );
			  _timerA.reset();
			  _scheduler.forward( 900, MILLISECONDS );

			  // then
			  verify( _handlerA, never() ).onTimeout(any());

			  // then
			  _scheduler.forward( 100, MILLISECONDS );

			  // when
			  verify( _handlerA, times( 1 ) ).onTimeout( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutSingleTimeAfterMultipleResets() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutSingleTimeAfterMultipleResets()
		 {
			  // given
			  _timerA.set( fixedTimeout( 1, SECONDS ) );

			  // when
			  _scheduler.forward( 900, MILLISECONDS );
			  _timerA.reset();
			  _scheduler.forward( 900, MILLISECONDS );
			  _timerA.reset();
			  _scheduler.forward( 900, MILLISECONDS );
			  _timerA.reset();
			  _scheduler.forward( 1000, MILLISECONDS );

			  // then
			  verify( _handlerA, times( 1 ) ).onTimeout( any() );

			  // when
			  reset( _handlerA );
			  _scheduler.forward( 5000, MILLISECONDS );

			  // then
			  verify( _handlerA, never() ).onTimeout(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotInvokeCancelledTimer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotInvokeCancelledTimer()
		 {
			  // given
			  _timerA.set( fixedTimeout( 1, SECONDS ) );
			  _scheduler.forward( 900, MILLISECONDS );

			  // when
			  _timerA.cancel( SYNC_WAIT );
			  _scheduler.forward( 100, MILLISECONDS );

			  // then
			  verify( _handlerA, never() ).onTimeout(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAwaitCancellationUnderRealScheduler() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAwaitCancellationUnderRealScheduler()
		 {
			  // given
			  IJobScheduler scheduler = createInitializedScheduler();
			  scheduler.Start();

			  TimerService timerService = new TimerService( scheduler, FormattedLogProvider.toOutputStream( System.out ) );

			  System.Threading.CountdownEvent started = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent finished = new System.Threading.CountdownEvent( 1 );

			  TimeoutHandler handlerA = timer =>
			  {
				started.Signal();
				finished.await();
			  };

			  TimeoutHandler handlerB = timer => finished.Signal();

			  Timer timerA = timerService.Create( Timers.TimerA, _group, handlerA );
			  timerA.Set( fixedTimeout( 0, SECONDS ) );
			  started.await();

			  Timer timerB = timerService.Create( Timers.TimerB, _group, handlerB );
			  timerB.Set( fixedTimeout( 2, SECONDS ) );

			  // when
			  timerA.Cancel( SYNC_WAIT );

			  // then
			  assertEquals( 0, finished.CurrentCount );

			  // cleanup
			  scheduler.Stop();
			  scheduler.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCancelBeforeHandlingWithRealScheduler() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCancelBeforeHandlingWithRealScheduler()
		 {
			  // given
			  IJobScheduler scheduler = createInitializedScheduler();
			  scheduler.Start();

			  TimerService timerService = new TimerService( scheduler, FormattedLogProvider.toOutputStream( System.out ) );

			  TimeoutHandler handlerA = timer =>
			  {
			  };

			  Timer timer = timerService.Create( Timers.TimerA, _group, handlerA );
			  timer.Set( fixedTimeout( 2, SECONDS ) );

			  // when
			  timer.Cancel( SYNC_WAIT );

			  // then: should not deadlock

			  // cleanup
			  scheduler.Stop();
			  scheduler.Shutdown();
		 }

		 internal enum Timers
		 {
			  TimerA,
			  TimerB
		 }
	}

}