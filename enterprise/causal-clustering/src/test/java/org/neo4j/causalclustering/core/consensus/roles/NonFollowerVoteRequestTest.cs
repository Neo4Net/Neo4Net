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
namespace Org.Neo4j.causalclustering.core.consensus.roles
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Outcome = Org.Neo4j.causalclustering.core.consensus.outcome.Outcome;
	using RaftState = Org.Neo4j.causalclustering.core.consensus.state.RaftState;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Log = Org.Neo4j.Logging.Log;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.MessageUtils.messageFor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.voteRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.state.RaftStateBuilder.raftState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class NonFollowerVoteRequestTest
	public class NonFollowerVoteRequestTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection data()
		 public static System.Collections.ICollection Data()
		 {
			  return asList( Role.Candidate, Role.Leader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Role role;
		 public Role Role;

		 private MemberId _myself = member( 0 );
		 private MemberId _member1 = member( 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectVoteRequestFromCurrentTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectVoteRequestFromCurrentTerm()
		 {
			  RaftState state = NewState();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long candidateTerm = state.term();
			  long candidateTerm = state.Term();

			  Outcome outcome = Role.handler.handle( voteRequest().from(_member1).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  // then
			  assertFalse( ( ( Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response ) messageFor( outcome, _member1 ) ).voteGranted() );
			  assertEquals( Role, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectVoteRequestFromPreviousTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectVoteRequestFromPreviousTerm()
		 {
			  RaftState state = NewState();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long candidateTerm = state.term() - 1;
			  long candidateTerm = state.Term() - 1;

			  Outcome outcome = Role.handler.handle( voteRequest().from(_member1).term(candidateTerm).lastLogIndex(0).lastLogTerm(-1).build(), state, Log() );

			  // then
			  assertFalse( ( ( Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response ) messageFor( outcome, _member1 ) ).voteGranted() );
			  assertEquals( Role, outcome.Role );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.state.RaftState newState() throws java.io.IOException
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