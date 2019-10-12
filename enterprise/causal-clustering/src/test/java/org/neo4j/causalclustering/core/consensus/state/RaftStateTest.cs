using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.consensus.state
{
	using Test = org.junit.Test;


	using ConsecutiveInFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.InFlightCache;
	using Org.Neo4j.causalclustering.core.state.storage;
	using InMemoryRaftLog = Org.Neo4j.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using RaftMembership = Org.Neo4j.causalclustering.core.consensus.membership.RaftMembership;
	using AppendLogEntry = Org.Neo4j.causalclustering.core.consensus.outcome.AppendLogEntry;
	using RaftLogCommand = Org.Neo4j.causalclustering.core.consensus.outcome.RaftLogCommand;
	using Outcome = Org.Neo4j.causalclustering.core.consensus.outcome.Outcome;
	using TruncateLogCommand = Org.Neo4j.causalclustering.core.consensus.outcome.TruncateLogCommand;
	using FollowerState = Org.Neo4j.causalclustering.core.consensus.roles.follower.FollowerState;
	using Org.Neo4j.causalclustering.core.consensus.roles.follower;
	using TermState = Org.Neo4j.causalclustering.core.consensus.term.TermState;
	using VoteState = Org.Neo4j.causalclustering.core.consensus.vote.VoteState;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ReplicatedInteger.valueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.CANDIDATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

	public class RaftStateTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateCacheState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateCacheState()
		 {
			  //Test that updates applied to the raft state will be reflected in the entry cache.

			  //given
			  InFlightCache cache = new ConsecutiveInFlightCache();
			  RaftState raftState = new RaftState( member( 0 ), new InMemoryStateStorage<TermState>( new TermState() ), new FakeMembership(this), new InMemoryRaftLog(), new InMemoryStateStorage<VoteState>(new VoteState()), cache, NullLogProvider.Instance, false, false );

			  IList<RaftLogCommand> logCommands = new LinkedListAnonymousInnerClass( this );

			  Outcome raftTestMemberOutcome = new Outcome( CANDIDATE, 0, null, -1, null, emptySet(), emptySet(), -1, InitialFollowerStates(), true, logCommands, EmptyOutgoingMessages(), emptySet(), -1, emptySet(), false );

			  //when
			  raftState.Update( raftTestMemberOutcome );

			  //then
			  assertNotNull( cache.Get( 1L ) );
			  assertNotNull( cache.Get( 2L ) );
			  assertNotNull( cache.Get( 3L ) );
			  assertEquals( valueOf( 5 ), cache.Get( 3L ).content() );
			  assertNull( cache.Get( 4L ) );
		 }

		 private class LinkedListAnonymousInnerClass : LinkedList<RaftLogCommand>
		 {
			 private readonly RaftStateTest _outerInstance;

			 public LinkedListAnonymousInnerClass( RaftStateTest outerInstance )
			 {
				 this.outerInstance = outerInstance;

				 this.add( new AppendLogEntry( 1, new RaftLogEntry( 0L, valueOf( 0 ) ) ) );
				 this.add( new AppendLogEntry( 2, new RaftLogEntry( 0L, valueOf( 1 ) ) ) );
				 this.add( new AppendLogEntry( 3, new RaftLogEntry( 0L, valueOf( 2 ) ) ) );
				 this.add( new AppendLogEntry( 4, new RaftLogEntry( 0L, valueOf( 4 ) ) ) );
				 this.add( new TruncateLogCommand( 3 ) );
				 this.add( new AppendLogEntry( 3, new RaftLogEntry( 0L, valueOf( 5 ) ) ) );
			 }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveFollowerStateAfterBecomingLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveFollowerStateAfterBecomingLeader()
		 {
			  // given
			  RaftState raftState = new RaftState( member( 0 ), new InMemoryStateStorage<TermState>( new TermState() ), new FakeMembership(this), new InMemoryRaftLog(), new InMemoryStateStorage<VoteState>(new VoteState()), new ConsecutiveInFlightCache(), NullLogProvider.Instance, false, false );

			  raftState.Update( new Outcome( CANDIDATE, 1, null, -1, null, emptySet(), emptySet(), -1, InitialFollowerStates(), true, EmptyLogCommands(), EmptyOutgoingMessages(), emptySet(), -1, emptySet(), false ) );

			  // when
			  raftState.Update( new Outcome( CANDIDATE, 1, null, -1, null, emptySet(), emptySet(), -1, new FollowerStates<MemberId>(), true, EmptyLogCommands(), EmptyOutgoingMessages(), emptySet(), -1, emptySet(), false ) );

			  // then
			  assertEquals( 0, raftState.FollowerStates().size() );
		 }

		 private ICollection<Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed> EmptyOutgoingMessages()
		 {
			  return new List<Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed>();
		 }

		 private FollowerStates<MemberId> InitialFollowerStates()
		 {
			  return new FollowerStates<MemberId>( new FollowerStates<MemberId>(), member(1), new FollowerState() );
		 }

		 private ICollection<RaftLogCommand> EmptyLogCommands()
		 {
			  return Collections.emptyList();
		 }

		 private class FakeMembership : RaftMembership
		 {
			 private readonly RaftStateTest _outerInstance;

			 public FakeMembership( RaftStateTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override ISet<MemberId> VotingMembers()
			  {
					return emptySet();
			  }

			  public override ISet<MemberId> ReplicationMembers()
			  {
					return emptySet();
			  }

			  public override void RegisterListener( Org.Neo4j.causalclustering.core.consensus.membership.RaftMembership_Listener listener )
			  {
					throw new System.NotSupportedException();
			  }
		 }
	}

}