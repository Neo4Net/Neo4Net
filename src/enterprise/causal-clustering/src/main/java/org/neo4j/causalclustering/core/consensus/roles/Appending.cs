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

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using AppendLogEntry = Neo4Net.causalclustering.core.consensus.outcome.AppendLogEntry;
	using BatchAppendLogEntries = Neo4Net.causalclustering.core.consensus.outcome.BatchAppendLogEntries;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using ShipCommand = Neo4Net.causalclustering.core.consensus.outcome.ShipCommand;
	using TruncateLogCommand = Neo4Net.causalclustering.core.consensus.outcome.TruncateLogCommand;
	using ReadableRaftState = Neo4Net.causalclustering.core.consensus.state.ReadableRaftState;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Log = Neo4Net.Logging.Log;

	internal class Appending
	{
		 private Appending()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void handleAppendEntriesRequest(org.Neo4Net.causalclustering.core.consensus.state.ReadableRaftState state, org.Neo4Net.causalclustering.core.consensus.outcome.Outcome outcome, org.Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request, org.Neo4Net.logging.Log log) throws java.io.IOException
		 internal static void HandleAppendEntriesRequest( ReadableRaftState state, Outcome outcome, Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request, Log log )
		 {
			  if ( request.LeaderTerm() < state.Term() )
			  {
					Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response appendResponse = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( state.Myself(), state.Term(), false, -1, state.EntryLog().appendIndex() );

					outcome.AddOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( request.From(), appendResponse ) );
					return;
			  }

			  outcome.PreElection = false;
			  outcome.NextTerm = request.LeaderTerm();
			  outcome.Leader = request.From();
			  outcome.LeaderCommit = request.LeaderCommit();

			  if ( !Follower.LogHistoryMatches( state, request.PrevLogIndex(), request.PrevLogTerm() ) )
			  {
					Debug.Assert( request.PrevLogIndex() > -1 && request.PrevLogTerm() > -1 );
					Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response appendResponse = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( state.Myself(), request.LeaderTerm(), false, -1, state.EntryLog().appendIndex() );

					outcome.AddOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( request.From(), appendResponse ) );
					return;
			  }

			  long baseIndex = request.PrevLogIndex() + 1;
			  int offset;

			  /* Find possible truncation point. */
			  for ( offset = 0; offset < request.Entries().Length; offset++ )
			  {
					long logIndex = baseIndex + offset;
					long logTerm = state.EntryLog().readEntryTerm(logIndex);

					if ( logIndex > state.EntryLog().appendIndex() )
					{
						 // entry doesn't exist because it's beyond the current log end, so we can go ahead and append
						 break;
					}
					else if ( logIndex < state.EntryLog().prevIndex() )
					{
						 // entry doesn't exist because it's before the earliest known entry, so continue with the next one
						 continue;
					}
					else if ( logTerm != request.Entries()[offset].term() )
					{
						 /*
						  * the entry's index falls within our current range and the term doesn't match what we know. We must
						  * truncate.
						  */
						 if ( logIndex <= state.CommitIndex() ) // first, assert that we haven't committed what we are about to truncate
						 {
							  throw new System.InvalidOperationException( format( "Cannot truncate entry at index %d with term %d when commit index is at %d", logIndex, logTerm, state.CommitIndex() ) );
						 }
						 outcome.AddLogCommand( new TruncateLogCommand( logIndex ) );
						 break;
					}
			  }

			  if ( offset < request.Entries().Length )
			  {
					outcome.AddLogCommand( new BatchAppendLogEntries( baseIndex, offset, request.Entries() ) );
			  }

			  Follower.CommitToLogOnUpdate( state, request.PrevLogIndex() + request.Entries().Length, request.LeaderCommit(), outcome );

			  long endMatchIndex = request.PrevLogIndex() + request.Entries().Length; // this is the index of the last incoming entry
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response appendResponse = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( state.Myself(), request.LeaderTerm(), true, endMatchIndex, endMatchIndex );
			  outcome.AddOutgoingMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( request.From(), appendResponse ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void appendNewEntry(org.Neo4Net.causalclustering.core.consensus.state.ReadableRaftState ctx, org.Neo4Net.causalclustering.core.consensus.outcome.Outcome outcome, org.Neo4Net.causalclustering.core.replication.ReplicatedContent content) throws java.io.IOException
		 internal static void AppendNewEntry( ReadableRaftState ctx, Outcome outcome, ReplicatedContent content )
		 {
			  long prevLogIndex = ctx.EntryLog().appendIndex();
			  long prevLogTerm = prevLogIndex == -1 ? -1 : prevLogIndex > ctx.LastLogIndexBeforeWeBecameLeader() ? ctx.Term() : ctx.EntryLog().readEntryTerm(prevLogIndex);

			  RaftLogEntry newLogEntry = new RaftLogEntry( ctx.Term(), content );

			  outcome.AddShipCommand( new ShipCommand.NewEntries( prevLogIndex, prevLogTerm, new RaftLogEntry[]{ newLogEntry } ) );
			  outcome.AddLogCommand( new AppendLogEntry( prevLogIndex + 1, newLogEntry ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void appendNewEntries(org.Neo4Net.causalclustering.core.consensus.state.ReadableRaftState ctx, org.Neo4Net.causalclustering.core.consensus.outcome.Outcome outcome, java.util.Collection<org.Neo4Net.causalclustering.core.replication.ReplicatedContent> contents) throws java.io.IOException
		 internal static void AppendNewEntries( ReadableRaftState ctx, Outcome outcome, ICollection<ReplicatedContent> contents )
		 {
			  long prevLogIndex = ctx.EntryLog().appendIndex();
			  long prevLogTerm = prevLogIndex == -1 ? -1 : prevLogIndex > ctx.LastLogIndexBeforeWeBecameLeader() ? ctx.Term() : ctx.EntryLog().readEntryTerm(prevLogIndex);

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  RaftLogEntry[] raftLogEntries = contents.Select( content => new RaftLogEntry( ctx.Term(), content ) ).ToArray(RaftLogEntry[]::new);

			  outcome.AddShipCommand( new ShipCommand.NewEntries( prevLogIndex, prevLogTerm, raftLogEntries ) );
			  outcome.AddLogCommand( new BatchAppendLogEntries( prevLogIndex + 1, 0, raftLogEntries ) );
		 }
	}

}