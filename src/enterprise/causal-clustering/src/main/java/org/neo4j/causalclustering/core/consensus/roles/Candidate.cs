/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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

	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using ReadableRaftState = Neo4Net.causalclustering.core.consensus.state.ReadableRaftState;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.MajorityIncludingSelfQuorum.isQuorum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.CANDIDATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.FOLLOWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.LEADER;

	internal class Candidate : RaftMessageHandler
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage message, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException
		 public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage message, ReadableRaftState ctx, Log log )
		 {
			  return message.Dispatch( new Handler( ctx, log ) );
		 }

		 private class Handler : Neo4Net.causalclustering.core.consensus.RaftMessages_Handler<Outcome, IOException>
		 {
			  internal readonly ReadableRaftState Ctx;
			  internal readonly Log Log;
			  internal readonly Outcome Outcome;

			  internal Handler( ReadableRaftState ctx, Log log )
			  {
					this.Ctx = ctx;
					this.Log = log;
					this.Outcome = new Outcome( CANDIDATE, ctx );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat req )
			  {
					if ( req.LeaderTerm() < Ctx.term() )
					{
						 return Outcome;
					}

					Outcome.NextRole = FOLLOWER;
					Log.info( "Moving to FOLLOWER state after receiving heartbeat from %s at term %d (I am at %d)", req.From(), req.LeaderTerm(), Ctx.term() );
					Heart.Beat( Ctx, Outcome, req, Log );
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request req )
			  {
					if ( req.LeaderTerm() < Ctx.term() )
					{
						 Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response appendResponse = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( Ctx.myself(), Ctx.term(), false, req.PrevLogIndex(), Ctx.entryLog().appendIndex() );

						 Outcome.addOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( req.From(), appendResponse ) );
						 return Outcome;
					}

					Outcome.NextRole = FOLLOWER;
					Log.info( "Moving to FOLLOWER state after receiving append entries request from %s at term %d (I am at %d)n", req.From(), req.LeaderTerm(), Ctx.term() );
					Appending.HandleAppendEntriesRequest( Ctx, Outcome, req, Log );
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response res) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response res )
			  {
					if ( res.Term() > Ctx.term() )
					{
						 Outcome.NextTerm = res.Term();
						 Outcome.NextRole = FOLLOWER;
						 Log.info( "Moving to FOLLOWER state after receiving vote response from %s at term %d (I am at %d)", res.From(), res.Term(), Ctx.term() );
						 return Outcome;
					}
					else if ( res.Term() < Ctx.term() || !res.VoteGranted() )
					{
						 return Outcome;
					}

					if ( !res.From().Equals(Ctx.myself()) )
					{
						 Outcome.addVoteForMe( res.From() );
					}

					if ( isQuorum( Ctx.votingMembers(), Outcome.VotesForMe ) )
					{
						 Outcome.Leader = Ctx.myself();
						 Appending.AppendNewEntry( Ctx, Outcome, new NewLeaderBarrier() );
						 Leader.SendHeartbeats( Ctx, Outcome );

						 Outcome.LastLogIndexBeforeWeBecameLeader = Ctx.entryLog().appendIndex();
						 Outcome.electedLeader();
						 Outcome.renewElectionTimeout();
						 Outcome.NextRole = LEADER;
						 Log.info( "Moving to LEADER state at term %d (I am %s), voted for by %s", Ctx.term(), Ctx.myself(), Outcome.VotesForMe );
					}
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request req )
			  {
					if ( req.Term() > Ctx.term() )
					{
						 Outcome.VotesForMe.Clear();
						 Outcome.NextRole = FOLLOWER;
						 Log.info( "Moving to FOLLOWER state after receiving vote request from %s at term %d (I am at %d)", req.From(), req.Term(), Ctx.term() );
						 Voting.HandleVoteRequest( Ctx, Outcome, req, Log );
						 return Outcome;
					}

					Outcome.addOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( req.From(), new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response(Ctx.myself(), Outcome.Term, false) ) );
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election election) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election )
			  {
					Log.info( "Failed to get elected. Got votes from: %s", Ctx.votesForMe() );
					if ( !Election.StartRealElection( Ctx, Outcome, Log ) )
					{
						 Log.info( "Moving to FOLLOWER state after failing to start election" );
						 Outcome.NextRole = FOLLOWER;
					}
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request req )
			  {
					if ( Ctx.supportPreVoting() )
					{
						 if ( req.Term() > Ctx.term() )
						 {
							  Outcome.VotesForMe.Clear();
							  Outcome.NextRole = FOLLOWER;
							  Log.info( "Moving to FOLLOWER state after receiving pre vote request from %s at term %d (I am at %d)", req.From(), req.Term(), Ctx.term() );
						 }
						 Voting.DeclinePreVoteRequest( Ctx, Outcome, req );
					}
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response response )
			  {
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response )
			  {
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo logCompactionInfo )
			  {
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse heartbeatResponse )
			  {
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Heartbeat heartbeat )
			  {
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request )
			  {
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest batchRequest )
			  {
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PruneRequest pruneRequest )
			  {
					Pruning.HandlePruneRequest( Outcome, pruneRequest );
					return Outcome;
			  }
		 }
	}

}