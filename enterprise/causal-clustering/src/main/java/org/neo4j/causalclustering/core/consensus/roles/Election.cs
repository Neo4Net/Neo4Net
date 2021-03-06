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

	using Outcome = Org.Neo4j.causalclustering.core.consensus.outcome.Outcome;
	using ReadableRaftState = Org.Neo4j.causalclustering.core.consensus.state.ReadableRaftState;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Log = Org.Neo4j.Logging.Log;

	public class Election
	{
		 private Election()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean startRealElection(org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.logging.Log log) throws java.io.IOException
		 public static bool StartRealElection( ReadableRaftState ctx, Outcome outcome, Log log )
		 {
			  ISet<MemberId> currentMembers = ctx.VotingMembers();
			  if ( currentMembers == null || !currentMembers.Contains( ctx.Myself() ) )
			  {
					log.Info( "Election attempted but not started, current members are %s, I am %s", currentMembers, ctx.Myself() );
					return false;
			  }

			  outcome.NextTerm = ctx.Term() + 1;

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request voteForMe = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request( ctx.Myself(), outcome.Term, ctx.Myself(), ctx.EntryLog().appendIndex(), ctx.EntryLog().readEntryTerm(ctx.EntryLog().appendIndex()) );

			  currentMembers.Where( member => !member.Equals( ctx.Myself() ) ).ForEach(member => outcome.addOutgoingMessage(new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed(member, voteForMe)));

			  outcome.VotedFor = ctx.Myself();
			  log.Info( "Election started with vote request: %s and members: %s", voteForMe, currentMembers );
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean startPreElection(org.neo4j.causalclustering.core.consensus.state.ReadableRaftState ctx, org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, org.neo4j.logging.Log log) throws java.io.IOException
		 public static bool StartPreElection( ReadableRaftState ctx, Outcome outcome, Log log )
		 {
			  ISet<MemberId> currentMembers = ctx.VotingMembers();
			  if ( currentMembers == null || !currentMembers.Contains( ctx.Myself() ) )
			  {
					log.Info( "Pre-election attempted but not started, current members are %s, I am %s", currentMembers, ctx.Myself() );
					return false;
			  }

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request preVoteForMe = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request( ctx.Myself(), outcome.Term, ctx.Myself(), ctx.EntryLog().appendIndex(), ctx.EntryLog().readEntryTerm(ctx.EntryLog().appendIndex()) );

			  currentMembers.Where( member => !member.Equals( ctx.Myself() ) ).ForEach(member => outcome.addOutgoingMessage(new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed(member, preVoteForMe)));

			  log.Info( "Pre-election started with: %s and members: %s", preVoteForMe, currentMembers );
			  return true;
		 }
	}

}