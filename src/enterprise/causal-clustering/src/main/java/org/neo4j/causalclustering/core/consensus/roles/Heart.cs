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

	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using ReadableRaftState = Neo4Net.causalclustering.core.consensus.state.ReadableRaftState;
	using Log = Neo4Net.Logging.Log;

	internal class Heart
	{
		 private Heart()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void beat(Neo4Net.causalclustering.core.consensus.state.ReadableRaftState state, Neo4Net.causalclustering.core.consensus.outcome.Outcome outcome, Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat request, Neo4Net.logging.Log log) throws java.io.IOException
		 internal static void Beat( ReadableRaftState state, Outcome outcome, Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat request, Log log )
		 {
			  if ( request.LeaderTerm() < state.Term() )
			  {
					return;
			  }

			  outcome.PreElection = false;
			  outcome.NextTerm = request.LeaderTerm();
			  outcome.Leader = request.From();
			  outcome.LeaderCommit = request.CommitIndex();
			  outcome.AddOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( request.From(), new Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse(state.Myself()) ) );

			  if ( !Follower.LogHistoryMatches( state, request.CommitIndex(), request.CommitIndexTerm() ) )
			  {
					return;
			  }

			  Follower.CommitToLogOnUpdate( state, request.CommitIndex(), request.CommitIndex(), outcome );
		 }
	}

}