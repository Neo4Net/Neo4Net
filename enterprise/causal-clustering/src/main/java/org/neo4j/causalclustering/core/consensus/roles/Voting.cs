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

	using Outcome = Org.Neo4j.causalclustering.core.consensus.outcome.Outcome;
	using ReadableRaftState = Org.Neo4j.causalclustering.core.consensus.state.ReadableRaftState;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.Function;
	using Log = Org.Neo4j.Logging.Log;

	public class Voting
	{

		 private Voting()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void handleVoteRequest(org.neo4j.causalclustering.core.consensus.state.ReadableRaftState state, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request voteRequest, org.neo4j.logging.Log log) throws java.io.IOException
		 internal static void HandleVoteRequest( ReadableRaftState state, Outcome outcome, Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request voteRequest, Log log )
		 {
			  if ( voteRequest.Term() > state.Term() )
			  {
					outcome.NextTerm = voteRequest.Term();
					outcome.VotedFor = null;
			  }

			  bool votedForAnother = outcome.VotedFor != null && !outcome.VotedFor.Equals( voteRequest.Candidate() );
			  bool willVoteForCandidate = ShouldVoteFor( state, outcome, voteRequest, votedForAnother, log );

			  if ( willVoteForCandidate )
			  {
					outcome.VotedFor = voteRequest.From();
					outcome.RenewElectionTimeout();
			  }

			  outcome.AddOutgoingMessage( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed( voteRequest.From(), new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response(state.Myself(), outcome.Term, willVoteForCandidate) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void handlePreVoteRequest(org.neo4j.causalclustering.core.consensus.state.ReadableRaftState state, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request voteRequest, org.neo4j.logging.Log log) throws java.io.IOException
		 internal static void HandlePreVoteRequest( ReadableRaftState state, Outcome outcome, Org.Neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request voteRequest, Log log )
		 {
			  ThrowingBooleanSupplier<IOException> willVoteForCandidate = () => ShouldVoteFor(state, outcome, voteRequest, false, log);
			  RespondToPreVoteRequest( state, outcome, voteRequest, willVoteForCandidate );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void declinePreVoteRequest(org.neo4j.causalclustering.core.consensus.state.ReadableRaftState state, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request voteRequest) throws java.io.IOException
		 internal static void DeclinePreVoteRequest( ReadableRaftState state, Outcome outcome, Org.Neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request voteRequest )
		 {
			  RespondToPreVoteRequest( state, outcome, voteRequest, () => false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void respondToPreVoteRequest(org.neo4j.causalclustering.core.consensus.state.ReadableRaftState state, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request voteRequest, org.neo4j.function.ThrowingBooleanSupplier<java.io.IOException> willVoteFor) throws java.io.IOException
		 private static void RespondToPreVoteRequest( ReadableRaftState state, Outcome outcome, Org.Neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request voteRequest, ThrowingBooleanSupplier<IOException> willVoteFor )
		 {
			  if ( voteRequest.Term() > state.Term() )
			  {
					outcome.NextTerm = voteRequest.Term();
			  }

			  outcome.AddOutgoingMessage( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed( voteRequest.From(), new Org.Neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Response(state.Myself(), outcome.Term, willVoteFor.AsBoolean) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static boolean shouldVoteFor(org.neo4j.causalclustering.core.consensus.state.ReadableRaftState state, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.causalclustering.core.consensus.RaftMessages_AnyVote_Request voteRequest, boolean committedToVotingForAnother, org.neo4j.logging.Log log) throws java.io.IOException
		 private static bool ShouldVoteFor( ReadableRaftState state, Outcome outcome, Org.Neo4j.causalclustering.core.consensus.RaftMessages_AnyVote_Request voteRequest, bool committedToVotingForAnother, Log log )
		 {
			  long requestTerm = voteRequest.Term();
			  MemberId candidate = voteRequest.Candidate();
			  long requestLastLogTerm = voteRequest.LastLogTerm();
			  long requestLastLogIndex = voteRequest.LastLogIndex();
			  long contextTerm = outcome.Term;
			  long contextLastAppended = state.EntryLog().appendIndex();
			  long contextLastLogTerm = state.EntryLog().readEntryTerm(contextLastAppended);

			  return ShouldVoteFor( candidate, contextTerm, requestTerm, contextLastLogTerm, requestLastLogTerm, contextLastAppended, requestLastLogIndex, committedToVotingForAnother, log );
		 }

		 public static bool ShouldVoteFor( MemberId candidate, long contextTerm, long requestTerm, long contextLastLogTerm, long requestLastLogTerm, long contextLastAppended, long requestLastLogIndex, bool committedToVotingForAnother, Log log )
		 {
			  if ( requestTerm < contextTerm )
			  {
					log.Debug( "Will not vote for %s as vote request term %s was earlier than my term %s", candidate, requestTerm, contextTerm );
					return false;
			  }

			  bool requestLogEndsAtHigherTerm = requestLastLogTerm > contextLastLogTerm;
			  bool logsEndAtSameTerm = requestLastLogTerm == contextLastLogTerm;
			  bool requestLogAtLeastAsLongAsMyLog = requestLastLogIndex >= contextLastAppended;

			  bool requesterLogUpToDate = requestLogEndsAtHigherTerm || ( logsEndAtSameTerm && requestLogAtLeastAsLongAsMyLog );

			  bool votedForOtherInSameTerm = requestTerm == contextTerm && committedToVotingForAnother;

			  bool shouldVoteFor = requesterLogUpToDate && !votedForOtherInSameTerm;

			  log.Debug( "Should vote for raft candidate %s: " + "requester log up to date: %s " + "(request last log term: %s, context last log term: %s, request last log index: %s, context last append: %s) " + "voted for other in same term: %s " + "(request term: %s, context term: %s, voted for another: %s)", shouldVoteFor, requesterLogUpToDate, requestLastLogTerm, contextLastLogTerm, requestLastLogIndex, contextLastAppended, votedForOtherInSameTerm, requestTerm, contextTerm, committedToVotingForAnother );

			  return shouldVoteFor;
		 }
	}

}