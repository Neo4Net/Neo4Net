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
namespace Neo4Net.causalclustering.core.state.machines.locks
{
	using Test = org.junit.Test;

	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using Neo4Net.causalclustering.core.replication;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using AcquireLockTimeoutException = Neo4Net.Kernel.Api.StorageEngine.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class LeaderOnlyLockManagerTest
	public class LeaderOnlyLockManagerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIssueLocksOnLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIssueLocksOnLeader()
		 {
			  // given
			  MemberId me = member( 0 );

			  ReplicatedLockTokenStateMachine replicatedLockStateMachine = new ReplicatedLockTokenStateMachine( new InMemoryStateStorage( new ReplicatedLockTokenState() ) );

			  DirectReplicator replicator = new DirectReplicator( replicatedLockStateMachine );

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenReturn( me );
			  Locks locks = mock( typeof( Locks ) );
			  Neo4Net.Kernel.impl.locking.Locks_Client client = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  when( locks.NewClient() ).thenReturn(client);

			  LeaderOnlyLockManager lockManager = new LeaderOnlyLockManager( me, replicator, leaderLocator, locks, replicatedLockStateMachine );

			  // when
			  lockManager.NewClient().acquireExclusive(LockTracer.NONE, ResourceTypes.NODE, 0L);

			  // then
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIssueLocksOnNonLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotIssueLocksOnNonLeader()
		 {
			  // given
			  MemberId me = member( 0 );
			  MemberId leader = member( 1 );

			  ReplicatedLockTokenStateMachine replicatedLockStateMachine = new ReplicatedLockTokenStateMachine( new InMemoryStateStorage( new ReplicatedLockTokenState() ) );
			  DirectReplicator replicator = new DirectReplicator( replicatedLockStateMachine );

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenReturn( leader );
			  Locks locks = mock( typeof( Locks ) );
			  Neo4Net.Kernel.impl.locking.Locks_Client client = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  when( locks.NewClient() ).thenReturn(client);

			  LeaderOnlyLockManager lockManager = new LeaderOnlyLockManager( me, replicator, leaderLocator, locks, replicatedLockStateMachine );

			  // when
			  Neo4Net.Kernel.impl.locking.Locks_Client lockClient = lockManager.NewClient();
			  try
			  {
					lockClient.AcquireExclusive( LockTracer.NONE, ResourceTypes.NODE, 0L );
					fail( "Should have thrown exception" );
			  }
			  catch ( AcquireLockTimeoutException )
			  {
					// expected
			  }
		 }
	}

}