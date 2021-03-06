﻿using System.Collections.Generic;

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


	using InMemoryRaftLog = Org.Neo4j.causalclustering.core.consensus.log.InMemoryRaftLog;
	using Outcome = Org.Neo4j.causalclustering.core.consensus.outcome.Outcome;
	using PruneLogCommand = Org.Neo4j.causalclustering.core.consensus.outcome.PruneLogCommand;
	using RaftState = Org.Neo4j.causalclustering.core.consensus.state.RaftState;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Log = Org.Neo4j.Logging.Log;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.state.RaftStateBuilder.raftState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class PruningTest
	public class PruningTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { Role.Follower },
				  new object[] { Role.Leader },
				  new object[] { Role.Candidate }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Role role;
		 public Role Role;

		 private MemberId _myself = member( 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGeneratePruneCommandsOnRequest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGeneratePruneCommandsOnRequest()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().myself(_myself).entryLog(raftLog).build();

			  // when
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_PruneRequest pruneRequest = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_PruneRequest( 1000 );
			  Outcome outcome = Role.handler.handle( pruneRequest, state, Log() );

			  // then
			  assertThat( outcome.LogCommands, hasItem( new PruneLogCommand( 1000 ) ) );
		 }

		 private Log Log()
		 {
			  return NullLogProvider.Instance.getLog( this.GetType() );
		 }
	}

}