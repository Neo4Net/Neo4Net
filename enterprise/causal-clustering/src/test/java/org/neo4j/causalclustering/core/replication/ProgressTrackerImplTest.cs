using System;
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
namespace Org.Neo4j.causalclustering.core.replication
{
	using Test = org.junit.Test;

	using ReplicatedInteger = Org.Neo4j.causalclustering.core.consensus.ReplicatedInteger;
	using GlobalSession = Org.Neo4j.causalclustering.core.replication.session.GlobalSession;
	using LocalOperationId = Org.Neo4j.causalclustering.core.replication.session.LocalOperationId;
	using Result = Org.Neo4j.causalclustering.core.state.Result;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ProgressTrackerImplTest
	{
		private bool InstanceFieldsInitialized = false;

		public ProgressTrackerImplTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_operationA = new DistributedOperation( ReplicatedInteger.valueOf( 0 ), _session, new LocalOperationId( 0, 0 ) );
			_operationB = new DistributedOperation( ReplicatedInteger.valueOf( 1 ), _session, new LocalOperationId( 1, 0 ) );
			_tracker = new ProgressTrackerImpl( _session );
		}

		 private readonly int _defaultTimeoutMs = 15_000;

		 private GlobalSession _session = new GlobalSession( System.Guid.randomUUID(), null );
		 private DistributedOperation _operationA;
		 private DistributedOperation _operationB;
		 private ProgressTrackerImpl _tracker;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportThatOperationIsNotReplicatedInitially()
		 public virtual void ShouldReportThatOperationIsNotReplicatedInitially()
		 {
			  // when
			  Progress progress = _tracker.start( _operationA );

			  // then
			  assertFalse( progress.Replicated );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWaitForReplication() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitForReplication()
		 {
			  // given
			  Progress progress = _tracker.start( _operationA );

			  // when
			  long time = DateTimeHelper.CurrentUnixTimeMillis();
			  progress.AwaitReplication( 10L );

			  // then
			  time = DateTimeHelper.CurrentUnixTimeMillis() - time;
			  assertThat( time, greaterThanOrEqualTo( 10L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopWaitingWhenReplicated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopWaitingWhenReplicated()
		 {
			  // given
			  Progress progress = _tracker.start( _operationA );

			  // when
			  Thread waiter = ReplicationEventWaiter( progress );

			  // then
			  assertTrue( waiter.IsAlive );
			  assertFalse( progress.Replicated );

			  // when
			  _tracker.trackReplication( _operationA );

			  // then
			  assertTrue( progress.Replicated );
			  waiter.Join( _defaultTimeoutMs );
			  assertFalse( waiter.IsAlive );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAbortTracking()
		 public virtual void ShouldBeAbleToAbortTracking()
		 {
			  // when
			  _tracker.start( _operationA );
			  // then
			  assertEquals( 1L, _tracker.inProgressCount() );

			  // when
			  _tracker.abort( _operationA );
			  // then
			  assertEquals( 0L, _tracker.inProgressCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckThatOneOperationDoesNotAffectProgressOfOther()
		 public virtual void ShouldCheckThatOneOperationDoesNotAffectProgressOfOther()
		 {
			  // given
			  Progress progressA = _tracker.start( _operationA );
			  Progress progressB = _tracker.start( _operationB );

			  // when
			  _tracker.trackReplication( _operationA );

			  // then
			  assertTrue( progressA.Replicated );
			  assertFalse( progressB.Replicated );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTriggerReplicationEvent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTriggerReplicationEvent()
		 {
			  // given
			  Progress progress = _tracker.start( _operationA );
			  Thread waiter = ReplicationEventWaiter( progress );

			  // when
			  _tracker.triggerReplicationEvent();

			  // then
			  assertFalse( progress.Replicated );
			  waiter.Join();
			  assertFalse( waiter.IsAlive );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetTrackedResult() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetTrackedResult()
		 {
			  // given
			  Progress progress = _tracker.start( _operationA );

			  // when
			  string result = "result";
			  _tracker.trackResult( _operationA, Result.of( result ) );

			  // then
			  assertEquals( result, progress.FutureResult().get(_defaultTimeoutMs, MILLISECONDS) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreOtherSessions()
		 public virtual void ShouldIgnoreOtherSessions()
		 {
			  // given
			  GlobalSession sessionB = new GlobalSession( System.Guid.randomUUID(), null );
			  DistributedOperation aliasUnderSessionB = new DistributedOperation( ReplicatedInteger.valueOf( 0 ), sessionB, new LocalOperationId( _operationA.operationId().localSessionId(), _operationA.operationId().sequenceNumber() ) );

			  Progress progressA = _tracker.start( _operationA );

			  // when
			  _tracker.trackReplication( aliasUnderSessionB );
			  _tracker.trackResult( aliasUnderSessionB, Result.of( "result" ) );

			  // then
			  assertFalse( progressA.Replicated );
			  assertFalse( progressA.FutureResult().Done );
		 }

		 private Thread ReplicationEventWaiter( Progress progress )
		 {
			  Thread waiter = new Thread(() =>
			  {
				try
				{
					 progress.AwaitReplication( _defaultTimeoutMs );
				}
				catch ( InterruptedException e )
				{
					 throw new Exception( e );
				}
			  });

			  waiter.Start();
			  return waiter;
		 }
	}

}