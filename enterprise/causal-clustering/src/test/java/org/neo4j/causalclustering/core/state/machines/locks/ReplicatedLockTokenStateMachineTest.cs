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
namespace Org.Neo4j.causalclustering.core.state.machines.locks
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.core.state.storage;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

	public class ReplicatedLockTokenStateMachineTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithInvalidTokenId()
		 public virtual void ShouldStartWithInvalidTokenId()
		 {
			  // given
			  ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( new InMemoryStateStorage<ReplicatedLockTokenState>( new ReplicatedLockTokenState() ) );

			  // when
			  int initialTokenId = stateMachine.CurrentToken().id();

			  // then
			  assertEquals( initialTokenId, LockToken_Fields.INVALID_LOCK_TOKEN_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIssueNextLockTokenCandidateId()
		 public virtual void ShouldIssueNextLockTokenCandidateId()
		 {
			  // given
			  ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( new InMemoryStateStorage<ReplicatedLockTokenState>( new ReplicatedLockTokenState() ) );
			  int firstCandidateId = LockToken.nextCandidateId( stateMachine.CurrentToken().id() );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId), 0, r =>
			  {
			  });

			  // then
			  assertEquals( firstCandidateId + 1, LockToken.nextCandidateId( stateMachine.CurrentToken().id() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepTrackOfCurrentLockTokenId()
		 public virtual void ShouldKeepTrackOfCurrentLockTokenId()
		 {
			  // given
			  ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( new InMemoryStateStorage<ReplicatedLockTokenState>( new ReplicatedLockTokenState() ) );
			  int firstCandidateId = LockToken.nextCandidateId( stateMachine.CurrentToken().id() );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId), 1, r =>
			  {
			  });

			  // then
			  assertEquals( firstCandidateId, stateMachine.CurrentToken().id() );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId + 1), 2, r =>
			  {
			  });

			  // then
			  assertEquals( firstCandidateId + 1, stateMachine.CurrentToken().id() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepTrackOfLockTokenOwner()
		 public virtual void ShouldKeepTrackOfLockTokenOwner()
		 {
			  // given
			  ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( new InMemoryStateStorage<ReplicatedLockTokenState>( new ReplicatedLockTokenState() ) );
			  int firstCandidateId = LockToken.nextCandidateId( stateMachine.CurrentToken().id() );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId), 1, r =>
			  {
			  });

			  // then
			  assertEquals( member( 0 ), stateMachine.CurrentToken().owner() );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(1), firstCandidateId + 1), 2, r =>
			  {
			  });

			  // then
			  assertEquals( member( 1 ), stateMachine.CurrentToken().owner() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptOnlyFirstRequestWithSameId()
		 public virtual void ShouldAcceptOnlyFirstRequestWithSameId()
		 {
			  // given
			  ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( new InMemoryStateStorage<ReplicatedLockTokenState>( new ReplicatedLockTokenState() ) );
			  int firstCandidateId = LockToken.nextCandidateId( stateMachine.CurrentToken().id() );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId), 1, r =>
			  {
			  });
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(1), firstCandidateId), 2, r =>
			  {
			  });

			  // then
			  assertEquals( 0, stateMachine.CurrentToken().id() );
			  assertEquals( member( 0 ), stateMachine.CurrentToken().owner() );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(1), firstCandidateId + 1), 3, r =>
			  {
			  });
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId + 1), 4, r =>
			  {
			  });

			  // then
			  assertEquals( 1, stateMachine.CurrentToken().id() );
			  assertEquals( member( 1 ), stateMachine.CurrentToken().owner() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyAcceptNextImmediateId()
		 public virtual void ShouldOnlyAcceptNextImmediateId()
		 {
			  // given
			  ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( new InMemoryStateStorage<ReplicatedLockTokenState>( new ReplicatedLockTokenState() ) );
			  int firstCandidateId = LockToken.nextCandidateId( stateMachine.CurrentToken().id() );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId + 1), 1, r =>
			  {
			  }); // not accepted

			  // then
			  assertEquals( stateMachine.CurrentToken().id(), LockToken_Fields.INVALID_LOCK_TOKEN_ID );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId), 2, r =>
			  {
			  }); // accepted

			  // then
			  assertEquals( stateMachine.CurrentToken().id(), firstCandidateId );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId + 1), 3, r =>
			  {
			  }); // accepted

			  // then
			  assertEquals( stateMachine.CurrentToken().id(), firstCandidateId + 1 );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId), 4, r =>
			  {
			  }); // not accepted

			  // then
			  assertEquals( stateMachine.CurrentToken().id(), firstCandidateId + 1 );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(member(0), firstCandidateId + 3), 5, r =>
			  {
			  }); // not accepted

			  // then
			  assertEquals( stateMachine.CurrentToken().id(), firstCandidateId + 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPersistAndRecoverState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPersistAndRecoverState()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = FileSystemRule.get();
			  fsa.Mkdir( TestDir.directory() );

			  StateMarshal<ReplicatedLockTokenState> marshal = new ReplicatedLockTokenState.Marshal( new MemberId.Marshal() );

			  MemberId memberA = member( 0 );
			  MemberId memberB = member( 1 );
			  int candidateId;

			  DurableStateStorage<ReplicatedLockTokenState> storage = new DurableStateStorage<ReplicatedLockTokenState>( fsa, TestDir.directory(), "state", marshal, 100, NullLogProvider.Instance );
			  using ( Lifespan lifespan = new Lifespan( storage ) )
			  {
					ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( storage );

					// when
					candidateId = 0;
					stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(memberA, candidateId), 0, r =>
					{
					});
					candidateId = 1;
					stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(memberB, candidateId), 1, r =>
					{
					});

					stateMachine.Flush();
					fsa.Crash();
			  }

			  // then
			  DurableStateStorage<ReplicatedLockTokenState> storage2 = new DurableStateStorage<ReplicatedLockTokenState>( fsa, TestDir.directory(), "state", marshal, 100, NullLogProvider.Instance );
			  using ( Lifespan lifespan = new Lifespan( storage2 ) )
			  {
					ReplicatedLockTokenState initialState = storage2.InitialState;

					assertEquals( memberB, initialState.Get().owner() );
					assertEquals( candidateId, initialState.Get().id() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeIdempotent()
		 public virtual void ShouldBeIdempotent()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = FileSystemRule.get();
			  fsa.Mkdir( TestDir.directory() );

			  StateMarshal<ReplicatedLockTokenState> marshal = new ReplicatedLockTokenState.Marshal( new MemberId.Marshal() );

			  DurableStateStorage<ReplicatedLockTokenState> storage = new DurableStateStorage<ReplicatedLockTokenState>( fsa, TestDir.directory(), "state", marshal, 100, NullLogProvider.Instance );

			  using ( Lifespan lifespan = new Lifespan( storage ) )
			  {
					ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( storage );

					MemberId memberA = member( 0 );
					MemberId memberB = member( 1 );

					stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(memberA, 0), 3, r =>
					{
					});

					// when
					stateMachine.ApplyCommand(new ReplicatedLockTokenRequest(memberB, 1), 2, r =>
					{
					});

					// then
					assertEquals( memberA, stateMachine.CurrentToken().owner() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetInitialPendingRequestToInitialState()
		 public virtual void ShouldSetInitialPendingRequestToInitialState()
		 {
			  // Given
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.causalclustering.core.state.storage.StateStorage<ReplicatedLockTokenState> storage = mock(org.neo4j.causalclustering.core.state.storage.StateStorage.class);
			  StateStorage<ReplicatedLockTokenState> storage = mock( typeof( StateStorage ) );
			  MemberId initialHoldingCoreMember = member( 0 );
			  ReplicatedLockTokenState initialState = new ReplicatedLockTokenState( 123, new ReplicatedLockTokenRequest( initialHoldingCoreMember, 3 ) );
			  when( storage.InitialState ).thenReturn( initialState );

			  // When
			  ReplicatedLockTokenStateMachine stateMachine = new ReplicatedLockTokenStateMachine( storage );

			  // Then
			  LockToken initialToken = stateMachine.CurrentToken();
			  assertEquals( initialState.Get().owner(), initialToken.Owner() );
			  assertEquals( initialState.Get().id(), initialToken.Id() );
		 }
	}

}