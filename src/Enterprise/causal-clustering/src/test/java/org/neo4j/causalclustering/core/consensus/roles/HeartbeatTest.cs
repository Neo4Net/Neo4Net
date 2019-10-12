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
namespace Neo4Net.causalclustering.core.consensus.roles
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using RaftState = Neo4Net.causalclustering.core.consensus.state.RaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.heartbeat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.AppendEntriesRequestTest.ContentGenerator.content;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.state.RaftStateBuilder.raftState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class HeartbeatTest
	public class HeartbeatTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0} with leader {1} terms ahead.") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { Role.Follower, 0 },
				  new object[] { Role.Follower, 1 },
				  new object[] { Role.Leader, 1 },
				  new object[] { Role.Candidate, 1 }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(value = 0) public Role role;
		 public Role Role;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(value = 1) public int leaderTermDifference;
		 public int LeaderTermDifference;

		 private MemberId _myself = member( 0 );
		 private MemberId _leader = member( 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotResultInCommitIfReferringToFutureEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotResultInCommitIfReferringToFutureEntries()
		 {
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().myself(_myself).entryLog(raftLog).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  raftLog.Append( new RaftLogEntry( leaderTerm, content() ) );

			  Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat heartbeat = heartbeat().from(_leader).commitIndex(raftLog.AppendIndex() + 1).commitIndexTerm(leaderTerm).leaderTerm(leaderTerm).build();

			  Outcome outcome = Role.handler.handle( heartbeat, state, Log() );

			  assertThat( outcome.LogCommands, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotResultInCommitIfHistoryMismatches() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotResultInCommitIfHistoryMismatches()
		 {
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().myself(_myself).entryLog(raftLog).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  raftLog.Append( new RaftLogEntry( leaderTerm, content() ) );

			  Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat heartbeat = heartbeat().from(_leader).commitIndex(raftLog.AppendIndex()).commitIndexTerm(leaderTerm).leaderTerm(leaderTerm).build();

			  Outcome outcome = Role.handler.handle( heartbeat, state, Log() );

			  assertThat( outcome.CommitIndex, Matchers.equalTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResultInCommitIfHistoryMatches() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResultInCommitIfHistoryMatches()
		 {
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().myself(_myself).entryLog(raftLog).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  raftLog.Append( new RaftLogEntry( leaderTerm - 1, content() ) );

			  Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat heartbeat = heartbeat().from(_leader).commitIndex(raftLog.AppendIndex()).commitIndexTerm(leaderTerm).leaderTerm(leaderTerm).build();

			  Outcome outcome = Role.handler.handle( heartbeat, state, Log() );

			  assertThat( outcome.LogCommands, empty() );

		 }

		 private Log Log()
		 {
			  return NullLogProvider.Instance.getLog( this.GetType() );
		 }

	}

}