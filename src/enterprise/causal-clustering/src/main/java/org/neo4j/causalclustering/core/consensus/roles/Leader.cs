using System.Collections.Generic;
using System.Diagnostics;

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
	using ShipCommand = Neo4Net.causalclustering.core.consensus.outcome.ShipCommand;
	using FollowerState = Neo4Net.causalclustering.core.consensus.roles.follower.FollowerState;
	using Neo4Net.causalclustering.core.consensus.roles.follower;
	using ReadableRaftState = Neo4Net.causalclustering.core.consensus.state.ReadableRaftState;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.Collections.Helpers;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.MajorityIncludingSelfQuorum.isQuorum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.roles.Role.FOLLOWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.roles.Role.LEADER;

	public class Leader : RaftMessageHandler
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Iterable<Neo4Net.causalclustering.identity.MemberId> replicationTargets(final Neo4Net.causalclustering.core.consensus.state.ReadableRaftState ctx)
		 private static IEnumerable<MemberId> ReplicationTargets( ReadableRaftState ctx )
		 {
			  return new FilteringIterable<MemberId>( ctx.ReplicationMembers(), member => !member.Equals(ctx.Myself()) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void sendHeartbeats(Neo4Net.causalclustering.core.consensus.state.ReadableRaftState ctx, Neo4Net.causalclustering.core.consensus.outcome.Outcome outcome) throws java.io.IOException
		 internal static void SendHeartbeats( ReadableRaftState ctx, Outcome outcome )
		 {
			  long commitIndex = ctx.CommitIndex();
			  long commitIndexTerm = ctx.EntryLog().readEntryTerm(commitIndex);
			  RaftMessages_Heartbeat heartbeat = new RaftMessages_Heartbeat( ctx.Myself(), ctx.Term(), commitIndex, commitIndexTerm );
			  foreach ( MemberId to in ReplicationTargets( ctx ) )
			  {
					outcome.AddOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( to, heartbeat ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage message, Neo4Net.causalclustering.core.consensus.state.ReadableRaftState ctx, Neo4Net.logging.Log log) throws java.io.IOException
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
					this.Outcome = new Outcome( LEADER, ctx );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat heartbeat) throws java.io.IOException
			  public override Outcome Handle( RaftMessages_Heartbeat heartbeat )
			  {
					if ( heartbeat.LeaderTerm() < Ctx.term() )
					{
						 return Outcome;
					}

					StepDownToFollower( Outcome, Ctx );
					Log.info( "Moving to FOLLOWER state after receiving heartbeat at term %d (my term is " + "%d) from %s", heartbeat.LeaderTerm(), Ctx.term(), heartbeat.From() );
					Heart.Beat( Ctx, Outcome, heartbeat, Log );
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Heartbeat heartbeat) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Heartbeat heartbeat )
			  {
					SendHeartbeats( Ctx, Outcome );
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse heartbeatResponse )
			  {
					Outcome.addHeartbeatResponse( heartbeatResponse.From() );
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election )
			  {
					if ( !isQuorum( Ctx.votingMembers().Count, Ctx.heartbeatResponses().Count ) )
					{
						 StepDownToFollower( Outcome, Ctx );
						 Log.info( "Moving to FOLLOWER state after not receiving heartbeat responses in this election timeout " + "period. Heartbeats received: %s", Ctx.heartbeatResponses() );
					}

					Outcome.HeartbeatResponses.Clear();
					return Outcome;

			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request req )
			  {
					if ( req.LeaderTerm() < Ctx.term() )
					{
						 Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response appendResponse = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( Ctx.myself(), Ctx.term(), false, -1, Ctx.entryLog().appendIndex() );

						 Outcome.addOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( req.From(), appendResponse ) );
						 return Outcome;
					}
					else if ( req.LeaderTerm() == Ctx.term() )
					{
						 throw new System.InvalidOperationException( "Two leaders in the same term." );
					}
					else
					{
						 // There is a new leader in a later term, we should revert to follower. (§5.1)
						 StepDownToFollower( Outcome, Ctx );
						 Log.info( "Moving to FOLLOWER state after receiving append request at term %d (my term is " + "%d) from %s", req.LeaderTerm(), Ctx.term(), req.From() );
						 Appending.HandleAppendEntriesRequest( Ctx, Outcome, req, Log );
						 return Outcome;
					}

			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response )
			  {
					if ( response.Term() < Ctx.term() )
					{
							  /* Ignore responses from old terms! */
						 return Outcome;
					}
					else if ( response.Term() > Ctx.term() )
					{
						 Outcome.NextTerm = response.Term();
						 StepDownToFollower( Outcome, Ctx );
						 Log.info( "Moving to FOLLOWER state after receiving append response at term %d (my term is " + "%d) from %s", response.Term(), Ctx.term(), response.From() );
						 Outcome.replaceFollowerStates( new FollowerStates<MemberId>() );
						 return Outcome;
					}

					FollowerState follower = Ctx.followerStates().get(response.From());

					if ( response.Success() )
					{
						 Debug.Assert( response.MatchIndex() <= Ctx.entryLog().appendIndex() );

						 bool followerProgressed = response.MatchIndex() > follower.MatchIndex;

						 Outcome.replaceFollowerStates( Outcome.FollowerStates.onSuccessResponse( response.From(), max(response.MatchIndex(), follower.MatchIndex) ) );

						 Outcome.addShipCommand( new ShipCommand.Match( response.MatchIndex(), response.From() ) );

							  /*
							   * Matches from older terms can in complicated leadership change / log truncation scenarios
							   * be overwritten, even if they were replicated to a majority of instances. Thus we must only
							   * consider matches from this leader's term when figuring out which have been safely replicated
							   * and are ready for commit.
							   * This is explained nicely in Figure 3.7 of the thesis
							   */
						 bool matchInCurrentTerm = Ctx.entryLog().readEntryTerm(response.MatchIndex()) == Ctx.term();

							  /*
							   * The quorum situation may have changed only if the follower actually progressed.
							   */
						 if ( followerProgressed && matchInCurrentTerm )
						 {
							  // TODO: Test that mismatch between voting and participating members affects commit outcome

							  long quorumAppendIndex = Followers.quorumAppendIndex( Ctx.votingMembers(), Outcome.FollowerStates );
							  if ( quorumAppendIndex > Ctx.commitIndex() )
							  {
									Outcome.LeaderCommit = quorumAppendIndex;
									Outcome.CommitIndex = quorumAppendIndex;
									Outcome.addShipCommand( new ShipCommand.CommitUpdate() );
							  }
						 }
					}
					else // Response indicated failure.
					{
						 if ( response.AppendIndex() > -1 && response.AppendIndex() >= Ctx.entryLog().prevIndex() )
						 {
							  // Signal a mismatch to the log shipper, which will serve an earlier entry.
							  Outcome.addShipCommand( new ShipCommand.Mismatch( response.AppendIndex(), response.From() ) );
						 }
						 else
						 {
							  // There are no earlier entries, message the follower that we have compacted so that
							  // it can take appropriate action.
							  RaftMessages_LogCompactionInfo compactionInfo = new RaftMessages_LogCompactionInfo( Ctx.myself(), Ctx.term(), Ctx.entryLog().prevIndex() );
							  Neo4Net.causalclustering.core.consensus.RaftMessages_Directed directedCompactionInfo = new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( response.From(), compactionInfo );

							  Outcome.addOutgoingMessage( directedCompactionInfo );
						 }
					}
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request req )
			  {
					if ( req.Term() > Ctx.term() )
					{
						 StepDownToFollower( Outcome, Ctx );
						 Log.info( "Moving to FOLLOWER state after receiving vote request at term %d (my term is " + "%d) from %s", req.Term(), Ctx.term(), req.From() );

						 Voting.HandleVoteRequest( Ctx, Outcome, req, Log );
						 return Outcome;
					}

					Outcome.addOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( req.From(), new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response(Ctx.myself(), Ctx.term(), false) ) );
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request req )
			  {
					ReplicatedContent content = req.Content();
					Appending.AppendNewEntry( Ctx, Outcome, content );
					return Outcome;

			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest req )
			  {
					ICollection<ReplicatedContent> contents = req.Contents();
					Appending.AppendNewEntries( Ctx, Outcome, contents );
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PruneRequest pruneRequest )
			  {
					Pruning.HandlePruneRequest( Outcome, pruneRequest );
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response response )
			  {
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.outcome.Outcome handle(Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request req) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request req )
			  {
					if ( Ctx.supportPreVoting() )
					{
						 if ( req.Term() > Ctx.term() )
						 {
							  StepDownToFollower( Outcome, Ctx );
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

			  public override Outcome Handle( RaftMessages_LogCompactionInfo logCompactionInfo )
			  {
					return Outcome;
			  }

			  internal virtual void StepDownToFollower( Outcome outcome, ReadableRaftState raftState )
			  {
					outcome.SteppingDown( raftState.Term() );
					outcome.NextRole = FOLLOWER;
					outcome.Leader = null;
			  }
		 }
	}

}