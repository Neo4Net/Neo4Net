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
namespace Neo4Net.causalclustering.core.consensus.roles
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using RaftState = Neo4Net.causalclustering.core.consensus.state.RaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.MessageUtils.messageFor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.TestMessageBuilders.voteRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.state.RaftStateBuilder.raftState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.identity.RaftTestMember.member;

	/// <summary>
	/// Most behaviour for handling vote requests is identical for all roles.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class VoteRequestTest
	public class VoteRequestTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection data()
		 public static System.Collections.ICollection Data()
		 {
			  return asList( Role.values() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Role role;
		 public Role Role;

		 private MemberId _myself = member( 0 );
		 private MemberId _member1 = member( 1 );
		 private MemberId _member2 = member( 2 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVoteForCandidateInLaterTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVoteForCandidateInLaterTerm()
		 {
			  // given
			  RaftState state = NewState();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long candidateTerm = state.term() + 1;
			  long candidateTerm = state.Term() + 1;

			  Outcome outcome = Role.handler.handle( voteRequest().from(_member1).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  // then
			  assertTrue( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response ) messageFor( outcome, _member1 ) ).voteGranted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDenyForCandidateInPreviousTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDenyForCandidateInPreviousTerm()
		 {
			  // given
			  RaftState state = NewState();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long candidateTerm = state.term() - 1;
			  long candidateTerm = state.Term() - 1;

			  Outcome outcome = Role.handler.handle( voteRequest().from(_member1).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  // then
			  assertFalse( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response ) messageFor( outcome, _member1 ) ).voteGranted() );
			  assertEquals( Role, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVoteForOnlyOneCandidatePerTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVoteForOnlyOneCandidatePerTerm()
		 {
			  // given
			  RaftState state = NewState();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long candidateTerm = state.term() + 1;
			  long candidateTerm = state.Term() + 1;

			  Outcome outcome1 = Role.handler.handle( voteRequest().from(_member1).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  state.Update( outcome1 );

			  Outcome outcome2 = Role.handler.handle( voteRequest().from(_member2).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  // then
			  assertFalse( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response ) messageFor( outcome2, _member2 ) ).voteGranted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStayInCurrentRoleOnRequestFromCurrentTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStayInCurrentRoleOnRequestFromCurrentTerm()
		 {
			  // given
			  RaftState state = NewState();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long candidateTerm = state.term();
			  long candidateTerm = state.Term();

			  Outcome outcome = Role.handler.handle( voteRequest().from(_member1).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  // then
			  assertEquals( Role, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMoveToFollowerIfRequestIsFromLaterTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMoveToFollowerIfRequestIsFromLaterTerm()
		 {
			  // given
			  RaftState state = NewState();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long candidateTerm = state.term() + 1;
			  long candidateTerm = state.Term() + 1;

			  Outcome outcome = Role.handler.handle( voteRequest().from(_member1).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  // then
			  assertEquals( Role.Follower, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateTermIfRequestIsFromLaterTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateTermIfRequestIsFromLaterTerm()
		 {
			  // given
			  RaftState state = NewState();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long candidateTerm = state.term() + 1;
			  long candidateTerm = state.Term() + 1;

			  Outcome outcome = Role.handler.handle( voteRequest().from(_member1).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  // then
			  assertEquals( candidateTerm, outcome.Term );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.state.RaftState newState() throws java.io.IOException
		 public virtual RaftState NewState()
		 {
			  return raftState().myself(_myself).build();
		 }

		 private Log Log()
		 {
			  return NullLogProvider.Instance.getLog( this.GetType() );
		 }

	}

}