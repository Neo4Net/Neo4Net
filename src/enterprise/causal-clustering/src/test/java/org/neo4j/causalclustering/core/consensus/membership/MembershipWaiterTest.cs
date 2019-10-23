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
namespace Neo4Net.causalclustering.core.consensus.membership
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ExposedRaftState = Neo4Net.causalclustering.core.consensus.state.ExposedRaftState;
	using RaftStateBuilder = Neo4Net.causalclustering.core.consensus.state.RaftStateBuilder;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.ValueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.identity.RaftTestMember.member;

	public class MembershipWaiterTest
	{
		 private DatabaseHealth _dbHealth = mock( typeof( DatabaseHealth ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void mocking()
		 public virtual void Mocking()
		 {
			  when( _dbHealth.Healthy ).thenReturn( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnImmediatelyIfMemberAndCaughtUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnImmediatelyIfMemberAndCaughtUp()
		 {
			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();
			  MembershipWaiter waiter = new MembershipWaiter( member( 0 ), jobScheduler, () => _dbHealth, 500, NullLogProvider.Instance, new Monitors() );

			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  raftLog.Append( new RaftLogEntry( 0, ValueOf( 0 ) ) );
			  ExposedRaftState raftState = RaftStateBuilder.raftState().votingMembers(member(0)).leaderCommit(0).entryLog(raftLog).commitIndex(0L).build().copy();

			  RaftMachine raft = mock( typeof( RaftMachine ) );
			  when( raft.State() ).thenReturn(raftState);

			  CompletableFuture<bool> future = waiter.WaitUntilCaughtUpMember( raft );
			  jobScheduler.RunJob();
			  jobScheduler.RunJob();

			  future.get( 0, NANOSECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWaitUntilLeaderCommitIsAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitUntilLeaderCommitIsAvailable()
		 {
			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();
			  MembershipWaiter waiter = new MembershipWaiter( member( 0 ), jobScheduler, () => _dbHealth, 500, NullLogProvider.Instance, new Monitors() );

			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  raftLog.Append( new RaftLogEntry( 0, ValueOf( 0 ) ) );
			  ExposedRaftState raftState = RaftStateBuilder.raftState().votingMembers(member(0)).leaderCommit(0).entryLog(raftLog).commitIndex(0L).build().copy();

			  RaftMachine raft = mock( typeof( RaftMachine ) );
			  when( raft.State() ).thenReturn(raftState);

			  CompletableFuture<bool> future = waiter.WaitUntilCaughtUpMember( raft );
			  jobScheduler.RunJob();

			  future.get( 1, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutIfCaughtUpButNotMember() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutIfCaughtUpButNotMember()
		 {
			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();
			  MembershipWaiter waiter = new MembershipWaiter( member( 0 ), jobScheduler, () => _dbHealth, 1, NullLogProvider.Instance, new Monitors() );

			  ExposedRaftState raftState = RaftStateBuilder.raftState().votingMembers(member(1)).leaderCommit(0).build().copy();

			  RaftMachine raft = mock( typeof( RaftMachine ) );
			  when( raft.State() ).thenReturn(raftState);

			  CompletableFuture<bool> future = waiter.WaitUntilCaughtUpMember( raft );
			  jobScheduler.RunJob();
			  jobScheduler.RunJob();

			  try
			  {
					future.get( 10, MILLISECONDS );
					fail( "Should have timed out." );
			  }
			  catch ( TimeoutException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutIfMemberButNotCaughtUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutIfMemberButNotCaughtUp()
		 {
			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();
			  MembershipWaiter waiter = new MembershipWaiter( member( 0 ), jobScheduler, () => _dbHealth, 1, NullLogProvider.Instance, new Monitors() );

			  ExposedRaftState raftState = RaftStateBuilder.raftState().votingMembers(member(0), member(1)).leaderCommit(0).build().copy();

			  RaftMachine raft = mock( typeof( RaftMachine ) );
			  when( raft.State() ).thenReturn(raftState);

			  CompletableFuture<bool> future = waiter.WaitUntilCaughtUpMember( raft );
			  jobScheduler.RunJob();
			  jobScheduler.RunJob();

			  try
			  {
					future.get( 10, MILLISECONDS );
					fail( "Should have timed out." );
			  }
			  catch ( TimeoutException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutIfLeaderCommitIsNeverKnown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutIfLeaderCommitIsNeverKnown()
		 {
			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();
			  MembershipWaiter waiter = new MembershipWaiter( member( 0 ), jobScheduler, () => _dbHealth, 1, NullLogProvider.Instance, new Monitors() );

			  ExposedRaftState raftState = RaftStateBuilder.raftState().leaderCommit(-1).build().copy();

			  RaftMachine raft = mock( typeof( RaftMachine ) );
			  when( raft.State() ).thenReturn(raftState);

			  CompletableFuture<bool> future = waiter.WaitUntilCaughtUpMember( raft );
			  jobScheduler.RunJob();

			  try
			  {
					future.get( 10, MILLISECONDS );
					fail( "Should have timed out." );
			  }
			  catch ( TimeoutException )
			  {
					// expected
			  }
		 }
	}

}