﻿using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.core.consensus.log.pruning
{
	using Test = org.junit.Test;


	using RaftLogPruner = Org.Neo4j.causalclustering.core.state.RaftLogPruner;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using Group = Org.Neo4j.Scheduler.Group;
	using DoubleLatch = Org.Neo4j.Test.DoubleLatch;
	using OnDemandJobScheduler = Org.Neo4j.Test.OnDemandJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class PruningSchedulerTest
	{
		 private readonly RaftLogPruner _logPruner = mock( typeof( RaftLogPruner ) );
		 private readonly OnDemandJobScheduler _jobScheduler = spy( new OnDemandJobScheduler() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScheduleTheCheckPointerJobOnStart()
		 public virtual void ShouldScheduleTheCheckPointerJobOnStart()
		 {
			  // given
			  PruningScheduler scheduler = new PruningScheduler( _logPruner, _jobScheduler, 20L, NullLogProvider.Instance );

			  assertNull( _jobScheduler.Job );

			  // when
			  scheduler.Start();

			  // then
			  assertNotNull( _jobScheduler.Job );
			  verify( _jobScheduler, times( 1 ) ).schedule( eq( Group.RAFT_LOG_PRUNING ), any( typeof( ThreadStart ) ), eq( 20L ), eq( TimeUnit.MILLISECONDS ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRescheduleTheJobAfterARun() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRescheduleTheJobAfterARun()
		 {
			  // given
			  PruningScheduler scheduler = new PruningScheduler( _logPruner, _jobScheduler, 20L, NullLogProvider.Instance );

			  assertNull( _jobScheduler.Job );

			  scheduler.Start();

			  ThreadStart scheduledJob = _jobScheduler.Job;
			  assertNotNull( scheduledJob );

			  // when
			  _jobScheduler.runJob();

			  // then
			  verify( _jobScheduler, times( 2 ) ).schedule( eq( Group.RAFT_LOG_PRUNING ), any( typeof( ThreadStart ) ), eq( 20L ), eq( TimeUnit.MILLISECONDS ) );
			  verify( _logPruner, times( 1 ) ).prune();
			  assertEquals( scheduledJob, _jobScheduler.Job );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRescheduleAJobWhenStopped()
		 public virtual void ShouldNotRescheduleAJobWhenStopped()
		 {
			  // given
			  PruningScheduler scheduler = new PruningScheduler( _logPruner, _jobScheduler, 20L, NullLogProvider.Instance );

			  assertNull( _jobScheduler.Job );

			  scheduler.Start();

			  assertNotNull( _jobScheduler.Job );

			  // when
			  scheduler.Stop();

			  // then
			  assertNull( _jobScheduler.Job );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stoppedJobCantBeInvoked() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoppedJobCantBeInvoked()
		 {
			  PruningScheduler scheduler = new PruningScheduler( _logPruner, _jobScheduler, 10L, NullLogProvider.Instance );
			  scheduler.Start();
			  _jobScheduler.runJob();

			  // verify checkpoint was triggered
			  verify( _logPruner ).prune();

			  // simulate scheduled run that was triggered just before stop
			  scheduler.Stop();
			  scheduler.Start();
			  _jobScheduler.runJob();

			  // logPruner should not be invoked now because job stopped
			  verifyNoMoreInteractions( _logPruner );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5000) public void shouldWaitOnStopUntilTheRunningCheckpointIsDone() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitOnStopUntilTheRunningCheckpointIsDone()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<Throwable> ex = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<Exception> ex = new AtomicReference<Exception>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch checkPointerLatch = new org.neo4j.test.DoubleLatch(1);
			  DoubleLatch checkPointerLatch = new DoubleLatch( 1 );
			  RaftLogPruner logPruner = new RaftLogPrunerAnonymousInnerClass( this, Clock.systemUTC(), checkPointerLatch );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PruningScheduler scheduler = new PruningScheduler(logPruner, jobScheduler, 20L, org.neo4j.logging.NullLogProvider.getInstance());
			  PruningScheduler scheduler = new PruningScheduler( logPruner, _jobScheduler, 20L, NullLogProvider.Instance );

			  // when
			  scheduler.Start();

			  Thread runCheckPointer = new Thread( _jobScheduler.runJob );
			  runCheckPointer.Start();

			  checkPointerLatch.WaitForAllToStart();

			  Thread stopper = new Thread(() =>
			  {
			  try
			  {
				  scheduler.Stop();
			  }
			  catch ( Exception throwable )
			  {
				  ex.set( throwable );
			  }
			  });

			  stopper.Start();

			  checkPointerLatch.Finish();
			  runCheckPointer.Join();

			  stopper.Join();

			  assertNull( ex.get() );
		 }

		 private class RaftLogPrunerAnonymousInnerClass : RaftLogPruner
		 {
			 private readonly PruningSchedulerTest _outerInstance;

			 private DoubleLatch _checkPointerLatch;

			 public RaftLogPrunerAnonymousInnerClass( PruningSchedulerTest outerInstance, UnknownType systemUTC, DoubleLatch checkPointerLatch ) : base( null, null, systemUTC )
			 {
				 this.outerInstance = outerInstance;
				 this._checkPointerLatch = checkPointerLatch;
			 }

			 public override void prune()
			 {
				  _checkPointerLatch.startAndWaitForAllToStart();
				  _checkPointerLatch.waitForAllToFinish();
			 }
		 }
	}

}