﻿/*
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
namespace Org.Neo4j.causalclustering.core.state.machines.locks
{
	using Test = org.junit.Test;

	using LeaderLocator = Org.Neo4j.causalclustering.core.consensus.LeaderLocator;
	using Org.Neo4j.causalclustering.core.replication;
	using Org.Neo4j.causalclustering.core.state.storage;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using AcquireLockTimeoutException = Org.Neo4j.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

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
			  Org.Neo4j.Kernel.impl.locking.Locks_Client client = mock( typeof( Org.Neo4j.Kernel.impl.locking.Locks_Client ) );
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
			  Org.Neo4j.Kernel.impl.locking.Locks_Client client = mock( typeof( Org.Neo4j.Kernel.impl.locking.Locks_Client ) );
			  when( locks.NewClient() ).thenReturn(client);

			  LeaderOnlyLockManager lockManager = new LeaderOnlyLockManager( me, replicator, leaderLocator, locks, replicatedLockStateMachine );

			  // when
			  Org.Neo4j.Kernel.impl.locking.Locks_Client lockClient = lockManager.NewClient();
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