using System.Collections.Generic;

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
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.MajorityIncludingSelfQuorum.isQuorum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.CANDIDATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.FOLLOWER;

	internal class Follower : RaftMessageHandler
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static boolean logHistoryMatches(org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, long leaderSegmentPrevIndex, long leaderSegmentPrevTerm) throws java.io.IOException
		 internal static bool LogHistoryMatches( ReadableRaftState ctx, long leaderSegmentPrevIndex, long leaderSegmentPrevTerm )
		 {
			  // NOTE: A prevLogIndex before or at our log's prevIndex means that we
			  //       already have all history (in a compacted form), so we report that history matches

			  // NOTE: The entry term for a non existing log index is defined as -1,
			  //       so the history for a non existing log entry never matches.

			  long localLogPrevIndex = ctx.EntryLog().prevIndex();
			  long localSegmentPrevTerm = ctx.EntryLog().readEntryTerm(leaderSegmentPrevIndex);

			  return leaderSegmentPrevIndex > -1 && ( leaderSegmentPrevIndex <= localLogPrevIndex || localSegmentPrevTerm == leaderSegmentPrevTerm );
		 }

		 internal static void CommitToLogOnUpdate( ReadableRaftState ctx, long indexOfLastNewEntry, long leaderCommit, Outcome outcome )
		 {
			  long newCommitIndex = min( leaderCommit, indexOfLastNewEntry );

			  if ( newCommitIndex > ctx.CommitIndex() )
			  {
					outcome.CommitIndex = newCommitIndex;
			  }
		 }

		 private static void HandleLeaderLogCompaction( ReadableRaftState ctx, Outcome outcome, Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo compactionInfo )
		 {
			  if ( compactionInfo.LeaderTerm() < ctx.Term() )
			  {
					return;
			  }

			  if ( ctx.EntryLog().appendIndex() <= -1 || compactionInfo.PrevIndex() > ctx.EntryLog().appendIndex() )
			  {
					outcome.MarkNeedForFreshSnapshot();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage message, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException
		 public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage message, ReadableRaftState ctx, Log log )
		 {
			  return message.Dispatch( Visitor( ctx, log ) );
		 }

		 private class Handler : Neo4Net.causalclustering.core.consensus.RaftMessages_Handler<Outcome, IOException>
		 {
			  protected internal readonly ReadableRaftState Ctx;
			  protected internal readonly Log Log;
			  protected internal readonly Outcome Outcome;
			  internal readonly PreVoteRequestHandler PreVoteRequestHandler;
			  internal readonly PreVoteResponseHandler PreVoteResponseHandler;
			  internal readonly ElectionTimeoutHandler ElectionTimeoutHandler;

			  internal Handler( PreVoteRequestHandler preVoteRequestHandler, PreVoteResponseHandler preVoteResponseHandler, ElectionTimeoutHandler electionTimeoutHandler, ReadableRaftState ctx, Log log )
			  {
					this.Ctx = ctx;
					this.Log = log;
					this.Outcome = new Outcome( FOLLOWER, ctx );
					this.PreVoteRequestHandler = preVoteRequestHandler;
					this.PreVoteResponseHandler = preVoteResponseHandler;
					this.ElectionTimeoutHandler = electionTimeoutHandler;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat heartbeat) throws java.io.IOException
			  public override Outcome Handle( RaftMessages_Heartbeat heartbeat )
			  {
					Heart.Beat( Ctx, Outcome, heartbeat, Log );
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request )
			  {
					Appending.HandleAppendEntriesRequest( Ctx, Outcome, request, Log );
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request request) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request request )
			  {
					Voting.HandleVoteRequest( Ctx, Outcome, request, Log );
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo logCompactionInfo )
			  {
					HandleLeaderLogCompaction( Ctx, Outcome, logCompactionInfo );
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response response )
			  {
					Log.info( "Late vote response: %s", response );
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request request) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request request )
			  {
					return PreVoteRequestHandler.handle( request, Outcome, Ctx, Log );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Response response) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response response )
			  {
					return PreVoteResponseHandler.handle( response, Outcome, Ctx, Log );
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PruneRequest pruneRequest )
			  {
					Pruning.HandlePruneRequest( Outcome, pruneRequest );
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response )
			  {
					return Outcome;
			  }

			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse heartbeatResponse )
			  {
					return Outcome;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election election) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election )
			  {
					return ElectionTimeoutHandler.handle( election, Outcome, Ctx, Log );
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
		 }

		 private interface ElectionTimeoutHandler
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election election, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException;
			  Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election, Outcome outcome, ReadableRaftState ctx, Log log );
		 }

		 private interface PreVoteRequestHandler
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request request, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException;
			  Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request request, Outcome outcome, ReadableRaftState ctx, Log log );

		 }
		 private interface PreVoteResponseHandler
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Response response, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException;
			  Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response response, Outcome outcome, ReadableRaftState ctx, Log log );
		 }

		 private class PreVoteSupportedHandler : ElectionTimeoutHandler
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election election, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					log.Info( "Election timeout triggered" );
					if ( Election.StartPreElection( ctx, outcome, log ) )
					{
						 outcome.PreElection = true;
					}
					return outcome;
			  }

			  internal static ElectionTimeoutHandler Instance = new PreVoteSupportedHandler();
		 }

		 private class PreVoteUnsupportedHandler : ElectionTimeoutHandler
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election election, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					log.Info( "Election timeout triggered" );
					if ( Election.StartRealElection( ctx, outcome, log ) )
					{
						 outcome.NextRole = CANDIDATE;
						 log.Info( "Moving to CANDIDATE state after successfully starting election" );
					}
					return outcome;
			  }

			  internal static ElectionTimeoutHandler Instance = new PreVoteUnsupportedHandler();
		 }

		 private class PreVoteUnsupportedRefusesToLead : ElectionTimeoutHandler
		 {
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					log.Info( "Election timeout triggered but refusing to be leader" );
					return outcome;
			  }

			  internal static ElectionTimeoutHandler Instance = new PreVoteUnsupportedRefusesToLead();
		 }

		 private class PreVoteSupportedRefusesToLeadHandler : ElectionTimeoutHandler
		 {
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					log.Info( "Election timeout triggered but refusing to be leader" );
					ISet<MemberId> memberIds = ctx.VotingMembers();
					if ( memberIds != null && memberIds.Contains( ctx.Myself() ) )
					{
						 outcome.PreElection = true;
					}
					return outcome;
			  }

			  internal static ElectionTimeoutHandler Instance = new PreVoteSupportedRefusesToLeadHandler();
		 }

		 private class PreVoteRequestVotingHandler : PreVoteRequestHandler
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request request, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request request, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					Voting.HandlePreVoteRequest( ctx, outcome, request, log );
					return outcome;
			  }

			  internal static PreVoteRequestHandler Instance = new PreVoteRequestVotingHandler();
		 }

		 private class PreVoteRequestDecliningHandler : PreVoteRequestHandler
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request request, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request request, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					Voting.DeclinePreVoteRequest( ctx, outcome, request );
					return outcome;
			  }

			  internal static PreVoteRequestHandler Instance = new PreVoteRequestDecliningHandler();
		 }

		 private class PreVoteRequestNoOpHandler : PreVoteRequestHandler
		 {
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request request, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					return outcome;
			  }

			  internal static PreVoteRequestHandler Instance = new PreVoteRequestNoOpHandler();
		 }

		 private class PreVoteResponseSolicitingHandler : PreVoteResponseHandler
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.outcome.Outcome handle(org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Response res, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.logging.Log log) throws java.io.IOException
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response res, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					if ( res.Term() > ctx.Term() )
					{
						 outcome.NextTerm = res.Term();
						 outcome.PreElection = false;
						 log.Info( "Aborting pre-election after receiving pre-vote response from %s at term %d (I am at %d)", res.From(), res.Term(), ctx.Term() );
						 return outcome;
					}
					else if ( res.Term() < ctx.Term() || !res.VoteGranted() )
					{
						 return outcome;
					}

					if ( !res.From().Equals(ctx.Myself()) )
					{
						 outcome.AddPreVoteForMe( res.From() );
					}

					if ( isQuorum( ctx.VotingMembers(), outcome.PreVotesForMe ) )
					{
						 outcome.RenewElectionTimeout();
						 outcome.PreElection = false;
						 if ( Election.StartRealElection( ctx, outcome, log ) )
						 {
							  outcome.NextRole = CANDIDATE;
							  log.Info( "Moving to CANDIDATE state after successful pre-election stage" );
						 }
					}
					return outcome;
			  }
			  internal static PreVoteResponseHandler Instance = new PreVoteResponseSolicitingHandler();
		 }

		 private class PreVoteResponseNoOpHandler : PreVoteResponseHandler
		 {
			  public override Outcome Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response response, Outcome outcome, ReadableRaftState ctx, Log log )
			  {
					return outcome;
			  }

			  internal static PreVoteResponseHandler Instance = new PreVoteResponseNoOpHandler();
		 }

		 private Handler Visitor( ReadableRaftState ctx, Log log )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ElectionTimeoutHandler electionTimeoutHandler;
			  ElectionTimeoutHandler electionTimeoutHandler;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PreVoteRequestHandler preVoteRequestHandler;
			  PreVoteRequestHandler preVoteRequestHandler;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PreVoteResponseHandler preVoteResponseHandler;
			  PreVoteResponseHandler preVoteResponseHandler;

			  if ( ctx.RefusesToBeLeader() )
			  {
					preVoteResponseHandler = PreVoteResponseNoOpHandler.Instance;
					if ( ctx.SupportPreVoting() )
					{
						 electionTimeoutHandler = PreVoteSupportedRefusesToLeadHandler.Instance;
						 if ( ctx.PreElection )
						 {
							  preVoteRequestHandler = PreVoteRequestVotingHandler.Instance;
						 }
						 else
						 {
							  preVoteRequestHandler = PreVoteRequestDecliningHandler.Instance;
						 }
					}
					else
					{
						 preVoteRequestHandler = PreVoteRequestNoOpHandler.Instance;
						 electionTimeoutHandler = PreVoteUnsupportedRefusesToLead.Instance;
					}
			  }
			  else
			  {
					if ( ctx.SupportPreVoting() )
					{
						 electionTimeoutHandler = PreVoteSupportedHandler.Instance;
						 if ( ctx.PreElection )
						 {
							  preVoteRequestHandler = PreVoteRequestVotingHandler.Instance;
							  preVoteResponseHandler = PreVoteResponseSolicitingHandler.Instance;
						 }
						 else
						 {
							  preVoteRequestHandler = PreVoteRequestDecliningHandler.Instance;
							  preVoteResponseHandler = PreVoteResponseNoOpHandler.Instance;
						 }
					}
					else
					{
						 preVoteRequestHandler = PreVoteRequestNoOpHandler.Instance;
						 preVoteResponseHandler = PreVoteResponseNoOpHandler.Instance;
						 electionTimeoutHandler = PreVoteUnsupportedHandler.Instance;
					}
			  }
			  return new Handler( preVoteRequestHandler, preVoteResponseHandler, electionTimeoutHandler, ctx, log );
		 }
	}

}