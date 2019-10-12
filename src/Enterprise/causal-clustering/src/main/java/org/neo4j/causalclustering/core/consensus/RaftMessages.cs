using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.causalclustering.core.consensus
{

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Message = Neo4Net.causalclustering.messaging.Message;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.HEARTBEAT_RESPONSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.PRUNE_REQUEST;

	public interface RaftMessages
	{

		 // Position is used to identify messages. Changing order will break upgrade paths.
	}

	 public interface RaftMessages_Handler<T, E> where E : Exception
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_Vote_Request request) throws E;
		  T Handle( RaftMessages_Vote_Request request );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_Vote_Response response) throws E;
		  T Handle( RaftMessages_Vote_Response response );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_PreVote_Request request) throws E;
		  T Handle( RaftMessages_PreVote_Request request );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_PreVote_Response response) throws E;
		  T Handle( RaftMessages_PreVote_Response response );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_AppendEntries_Request request) throws E;
		  T Handle( RaftMessages_AppendEntries_Request request );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_AppendEntries_Response response) throws E;
		  T Handle( RaftMessages_AppendEntries_Response response );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_Heartbeat heartbeat) throws E;
		  T Handle( RaftMessages_Heartbeat heartbeat );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_HeartbeatResponse heartbeatResponse) throws E;
		  T Handle( RaftMessages_HeartbeatResponse heartbeatResponse );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_LogCompactionInfo logCompactionInfo) throws E;
		  T Handle( RaftMessages_LogCompactionInfo logCompactionInfo );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_Timeout_Election election) throws E;
		  T Handle( RaftMessages_Timeout_Election election );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_Timeout_Heartbeat heartbeat) throws E;
		  T Handle( RaftMessages_Timeout_Heartbeat heartbeat );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_NewEntry_Request request) throws E;
		  T Handle( RaftMessages_NewEntry_Request request );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_NewEntry_BatchRequest batchRequest) throws E;
		  T Handle( RaftMessages_NewEntry_BatchRequest batchRequest );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T handle(RaftMessages_PruneRequest pruneRequest) throws E;
		  T Handle( RaftMessages_PruneRequest pruneRequest );
	 }

	 public abstract class RaftMessages_HandlerAdaptor<T, E> : RaftMessages_Handler<T, E> where E : Exception
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_Vote_Request request) throws E
		  public override T Handle( RaftMessages_Vote_Request request )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_Vote_Response response) throws E
		  public override T Handle( RaftMessages_Vote_Response response )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_PreVote_Request request) throws E
		  public override T Handle( RaftMessages_PreVote_Request request )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_PreVote_Response response) throws E
		  public override T Handle( RaftMessages_PreVote_Response response )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_AppendEntries_Request request) throws E
		  public override T Handle( RaftMessages_AppendEntries_Request request )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_AppendEntries_Response response) throws E
		  public override T Handle( RaftMessages_AppendEntries_Response response )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_Heartbeat heartbeat) throws E
		  public override T Handle( RaftMessages_Heartbeat heartbeat )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_HeartbeatResponse heartbeatResponse) throws E
		  public override T Handle( RaftMessages_HeartbeatResponse heartbeatResponse )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_LogCompactionInfo logCompactionInfo) throws E
		  public override T Handle( RaftMessages_LogCompactionInfo logCompactionInfo )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_Timeout_Election election) throws E
		  public override T Handle( RaftMessages_Timeout_Election election )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_Timeout_Heartbeat heartbeat) throws E
		  public override T Handle( RaftMessages_Timeout_Heartbeat heartbeat )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_NewEntry_Request request) throws E
		  public override T Handle( RaftMessages_NewEntry_Request request )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_NewEntry_BatchRequest batchRequest) throws E
		  public override T Handle( RaftMessages_NewEntry_BatchRequest batchRequest )
		  {
				return default( T );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T handle(RaftMessages_PruneRequest pruneRequest) throws E
		  public override T Handle( RaftMessages_PruneRequest pruneRequest )
		  {
				return default( T );
		  }
	 }

	 public enum RaftMessages_Type
	 {
		  VoteRequest,
		  VoteResponse,

		  AppendEntriesRequest,
		  AppendEntriesResponse,

		  Heartbeat,
		  HeartbeatResponse,
		  LogCompactionInfo,

		  // Timeouts
		  ElectionTimeout,
		  HeartbeatTimeout,

		  // TODO: Refactor, these are client-facing messages / api. Perhaps not public and instantiated through an api
		  // TODO: method instead?
		  NewEntryRequest,
		  NewBatchRequest,

		  PruneRequest,

		  PreVoteRequest,
		  PreVoteResponse,
	 }

	 public interface RaftMessages_RaftMessage : Message
	 {
		  MemberId From();
		  RaftMessages_Type Type();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <T, E extends Exception> T dispatch(RaftMessages_Handler<T, E> handler) throws E;
		  T dispatch<T, E>( RaftMessages_Handler<T, E> handler );
	 }

	 public class RaftMessages_Directed
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal MemberId ToConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal RaftMessages_RaftMessage MessageConflict;

		  public RaftMessages_Directed( MemberId to, RaftMessages_RaftMessage message )
		  {
				this.ToConflict = to;
				this.MessageConflict = message;
		  }

		  public virtual MemberId To()
		  {
				return ToConflict;
		  }

		  public virtual RaftMessages_RaftMessage Message()
		  {
				return MessageConflict;
		  }

		  public override string ToString()
		  {
				return format( "Directed{to=%s, message=%s}", ToConflict, MessageConflict );
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
				RaftMessages_Directed directed = ( RaftMessages_Directed ) o;
				return Objects.Equals( ToConflict, directed.ToConflict ) && Objects.Equals( MessageConflict, directed.MessageConflict );
		  }

		  public override int GetHashCode()
		  {
				return Objects.hash( ToConflict, MessageConflict );
		  }
	 }

	 public interface RaftMessages_AnyVote
	 {
	 }

	  public interface RaftMessages_AnyVote_Request
	  {
			long Term();

			long LastLogTerm();

			long LastLogIndex();

			MemberId Candidate();
	  }

	  public interface RaftMessages_AnyVote_Response
	  {
			long Term();

			bool VoteGranted();
	  }

	 public interface RaftMessages_Vote
	 {
	 }

	  public class RaftMessages_Vote_Request : RaftMessages_BaseRaftMessage, RaftMessages_AnyVote_Request
	  {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long TermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal MemberId CandidateConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long LastLogIndexConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long LastLogTermConflict;

			public RaftMessages_Vote_Request( MemberId from, long term, MemberId candidate, long lastLogIndex, long lastLogTerm ) : base( from, RaftMessages_Type.VoteRequest )
			{
				 this.TermConflict = term;
				 this.CandidateConflict = candidate;
				 this.LastLogIndexConflict = lastLogIndex;
				 this.LastLogTermConflict = lastLogTerm;
			}

			public override long Term()
			{
				 return TermConflict;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override bool Equals( object o )
			{
				 if ( this == o )
				 {
					  return true;
				 }
				 if ( o == null || this.GetType() != o.GetType() )
				 {
					  return false;
				 }
				 RaftMessages_Vote_Request request = ( RaftMessages_Vote_Request ) o;
				 return LastLogIndexConflict == request.LastLogIndexConflict && LastLogTermConflict == request.LastLogTermConflict && TermConflict == request.TermConflict && CandidateConflict.Equals( request.CandidateConflict );
			}

			public override int GetHashCode()
			{
				 int result = ( int ) TermConflict;
				 result = 31 * result + CandidateConflict.GetHashCode();
				 result = 31 * result + ( int )( LastLogIndexConflict ^ ( ( long )( ( ulong )LastLogIndexConflict >> 32 ) ) );
				 result = 31 * result + ( int )( LastLogTermConflict ^ ( ( long )( ( ulong )LastLogTermConflict >> 32 ) ) );
				 return result;
			}

			public override string ToString()
			{
				 return format( "Vote.Request from %s {term=%d, candidate=%s, lastAppended=%d, lastLogTerm=%d}", FromConflict, TermConflict, CandidateConflict, LastLogIndexConflict, LastLogTermConflict );
			}

			public override long LastLogTerm()
			{
				 return LastLogTermConflict;
			}

			public override long LastLogIndex()
			{
				 return LastLogIndexConflict;
			}

			public override MemberId Candidate()
			{
				 return CandidateConflict;
			}
	  }

	  public class RaftMessages_Vote_Response : RaftMessages_BaseRaftMessage, RaftMessages_AnyVote_Response
	  {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long TermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool VoteGrantedConflict;

			public RaftMessages_Vote_Response( MemberId from, long term, bool voteGranted ) : base( from, RaftMessages_Type.VoteResponse )
			{
				 this.TermConflict = term;
				 this.VoteGrantedConflict = voteGranted;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override bool Equals( object o )
			{
				 if ( this == o )
				 {
					  return true;
				 }
				 if ( o == null || this.GetType() != o.GetType() )
				 {
					  return false;
				 }

				 RaftMessages_Vote_Response response = ( RaftMessages_Vote_Response ) o;

				 return TermConflict == response.TermConflict && VoteGrantedConflict == response.VoteGrantedConflict;

			}

			public override int GetHashCode()
			{
				 int result = ( int ) TermConflict;
				 result = 31 * result + ( VoteGrantedConflict ? 1 : 0 );
				 return result;
			}

			public override string ToString()
			{
				 return format( "Vote.Response from %s {term=%d, voteGranted=%s}", FromConflict, TermConflict, VoteGrantedConflict );
			}

			public override long Term()
			{
				 return TermConflict;
			}

			public override bool VoteGranted()
			{
				 return VoteGrantedConflict;
			}
	  }

	 public interface RaftMessages_PreVote
	 {
	 }

	  public class RaftMessages_PreVote_Request : RaftMessages_BaseRaftMessage, RaftMessages_AnyVote_Request
	  {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long TermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal MemberId CandidateConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long LastLogIndexConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long LastLogTermConflict;

			public RaftMessages_PreVote_Request( MemberId from, long term, MemberId candidate, long lastLogIndex, long lastLogTerm ) : base( from, RaftMessages_Type.PreVoteRequest )
			{
				 this.TermConflict = term;
				 this.CandidateConflict = candidate;
				 this.LastLogIndexConflict = lastLogIndex;
				 this.LastLogTermConflict = lastLogTerm;
			}

			public override long Term()
			{
				 return TermConflict;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override bool Equals( object o )
			{
				 if ( this == o )
				 {
					  return true;
				 }
				 if ( o == null || this.GetType() != o.GetType() )
				 {
					  return false;
				 }
				 RaftMessages_PreVote_Request request = ( RaftMessages_PreVote_Request ) o;
				 return LastLogIndexConflict == request.LastLogIndexConflict && LastLogTermConflict == request.LastLogTermConflict && TermConflict == request.TermConflict && CandidateConflict.Equals( request.CandidateConflict );
			}

			public override int GetHashCode()
			{
				 int result = ( int ) TermConflict;
				 result = 31 * result + CandidateConflict.GetHashCode();
				 result = 31 * result + ( int )( LastLogIndexConflict ^ ( ( long )( ( ulong )LastLogIndexConflict >> 32 ) ) );
				 result = 31 * result + ( int )( LastLogTermConflict ^ ( ( long )( ( ulong )LastLogTermConflict >> 32 ) ) );
				 return result;
			}

			public override string ToString()
			{
				 return format( "PreVote.Request from %s {term=%d, candidate=%s, lastAppended=%d, lastLogTerm=%d}", FromConflict, TermConflict, CandidateConflict, LastLogIndexConflict, LastLogTermConflict );
			}

			public override long LastLogTerm()
			{
				 return LastLogTermConflict;
			}

			public override long LastLogIndex()
			{
				 return LastLogIndexConflict;
			}

			public override MemberId Candidate()
			{
				 return CandidateConflict;
			}
	  }

	  public class RaftMessages_PreVote_Response : RaftMessages_BaseRaftMessage, RaftMessages_AnyVote_Response
	  {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long TermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool VoteGrantedConflict;

			public RaftMessages_PreVote_Response( MemberId from, long term, bool voteGranted ) : base( from, RaftMessages_Type.PreVoteResponse )
			{
				 this.TermConflict = term;
				 this.VoteGrantedConflict = voteGranted;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override bool Equals( object o )
			{
				 if ( this == o )
				 {
					  return true;
				 }
				 if ( o == null || this.GetType() != o.GetType() )
				 {
					  return false;
				 }

				 RaftMessages_PreVote_Response response = ( RaftMessages_PreVote_Response ) o;

				 return TermConflict == response.TermConflict && VoteGrantedConflict == response.VoteGrantedConflict;

			}

			public override int GetHashCode()
			{
				 int result = ( int ) TermConflict;
				 result = 31 * result + ( VoteGrantedConflict ? 1 : 0 );
				 return result;
			}

			public override string ToString()
			{
				 return format( "PreVote.Response from %s {term=%d, voteGranted=%s}", FromConflict, TermConflict, VoteGrantedConflict );
			}

			public override long Term()
			{
				 return TermConflict;
			}

			public override bool VoteGranted()
			{
				 return VoteGrantedConflict;
			}
	  }

	 public interface RaftMessages_AppendEntries
	 {
	 }

	  public class RaftMessages_AppendEntries_Request : RaftMessages_BaseRaftMessage
	  {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long LeaderTermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long PrevLogIndexConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long PrevLogTermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal RaftLogEntry[] EntriesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long LeaderCommitConflict;

			public RaftMessages_AppendEntries_Request( MemberId from, long leaderTerm, long prevLogIndex, long prevLogTerm, RaftLogEntry[] entries, long leaderCommit ) : base( from, RaftMessages_Type.AppendEntriesRequest )
			{
				 Objects.requireNonNull( entries );
				 Debug.Assert( !( ( prevLogIndex == -1 && prevLogTerm != -1 ) || ( prevLogTerm == -1 && prevLogIndex != -1 ) ), format( "prevLogIndex was %d and prevLogTerm was %d", prevLogIndex, prevLogTerm ) );
				 this.EntriesConflict = entries;
				 this.LeaderTermConflict = leaderTerm;
				 this.PrevLogIndexConflict = prevLogIndex;
				 this.PrevLogTermConflict = prevLogTerm;
				 this.LeaderCommitConflict = leaderCommit;
			}

			public virtual long LeaderTerm()
			{
				 return LeaderTermConflict;
			}

			public virtual long PrevLogIndex()
			{
				 return PrevLogIndexConflict;
			}

			public virtual long PrevLogTerm()
			{
				 return PrevLogTermConflict;
			}

			public virtual RaftLogEntry[] Entries()
			{
				 return EntriesConflict;
			}

			public virtual long LeaderCommit()
			{
				 return LeaderCommitConflict;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override bool Equals( object o )
			{
				 if ( this == o )
				 {
					  return true;
				 }
				 if ( o == null || this.GetType() != o.GetType() )
				 {
					  return false;
				 }
				 RaftMessages_AppendEntries_Request request = ( RaftMessages_AppendEntries_Request ) o;
				 return Objects.Equals( LeaderTermConflict, request.LeaderTermConflict ) && Objects.Equals( PrevLogIndexConflict, request.PrevLogIndexConflict ) && Objects.Equals( PrevLogTermConflict, request.PrevLogTermConflict ) && Objects.Equals( LeaderCommitConflict, request.LeaderCommitConflict ) && Arrays.Equals( EntriesConflict, request.EntriesConflict );
			}

			public override int GetHashCode()
			{
				 return Objects.hash( LeaderTermConflict, PrevLogIndexConflict, PrevLogTermConflict, Arrays.GetHashCode( EntriesConflict ), LeaderCommitConflict );
			}

			public override string ToString()
			{
				 return format( "AppendEntries.Request from %s {leaderTerm=%d, prevLogIndex=%d, " + "prevLogTerm=%d, entry=%s, leaderCommit=%d}", FromConflict, LeaderTermConflict, PrevLogIndexConflict, PrevLogTermConflict, Arrays.ToString( EntriesConflict ), LeaderCommitConflict );
			}
	  }

	  public class RaftMessages_AppendEntries_Response : RaftMessages_BaseRaftMessage
	  {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long TermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool SuccessConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long MatchIndexConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long AppendIndexConflict;

			public RaftMessages_AppendEntries_Response( MemberId from, long term, bool success, long matchIndex, long appendIndex ) : base( from, RaftMessages_Type.AppendEntriesResponse )
			{
				 this.TermConflict = term;
				 this.SuccessConflict = success;
				 this.MatchIndexConflict = matchIndex;
				 this.AppendIndexConflict = appendIndex;
			}

			public virtual long Term()
			{
				 return TermConflict;
			}

			public virtual bool Success()
			{
				 return SuccessConflict;
			}

			public virtual long MatchIndex()
			{
				 return MatchIndexConflict;
			}

			public virtual long AppendIndex()
			{
				 return AppendIndexConflict;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override bool Equals( object o )
			{
				 if ( this == o )
				 {
					  return true;
				 }
				 if ( o == null || this.GetType() != o.GetType() )
				 {
					  return false;
				 }
				 if ( !base.Equals( o ) )
				 {
					  return false;
				 }
				 RaftMessages_AppendEntries_Response response = ( RaftMessages_AppendEntries_Response ) o;
				 return TermConflict == response.TermConflict && SuccessConflict == response.SuccessConflict && MatchIndexConflict == response.MatchIndexConflict && AppendIndexConflict == response.AppendIndexConflict;
			}

			public override int GetHashCode()
			{
				 return Objects.hash( base.GetHashCode(), TermConflict, SuccessConflict, MatchIndexConflict, AppendIndexConflict );
			}

			public override string ToString()
			{
				 return format( "AppendEntries.Response from %s {term=%d, success=%s, matchIndex=%d, appendIndex=%d}", FromConflict, TermConflict, SuccessConflict, MatchIndexConflict, AppendIndexConflict );
			}
	  }

	 public class RaftMessages_Heartbeat : RaftMessages_BaseRaftMessage
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal long LeaderTermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal long CommitIndexConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal long CommitIndexTermConflict;

		  public RaftMessages_Heartbeat( MemberId from, long leaderTerm, long commitIndex, long commitIndexTerm ) : base( from, RaftMessages_Type.Heartbeat )
		  {
				this.LeaderTermConflict = leaderTerm;
				this.CommitIndexConflict = commitIndex;
				this.CommitIndexTermConflict = commitIndexTerm;
		  }

		  public virtual long LeaderTerm()
		  {
				return LeaderTermConflict;
		  }

		  public virtual long CommitIndex()
		  {
				return CommitIndexConflict;
		  }

		  public virtual long CommitIndexTerm()
		  {
				return CommitIndexTermConflict;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
		  public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
		  {
				return handler.Handle( this );
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
				if ( !base.Equals( o ) )
				{
					 return false;
				}

				RaftMessages_Heartbeat heartbeat = ( RaftMessages_Heartbeat ) o;

				return LeaderTermConflict == heartbeat.LeaderTermConflict && CommitIndexConflict == heartbeat.CommitIndexConflict && CommitIndexTermConflict == heartbeat.CommitIndexTermConflict;
		  }

		  public override int GetHashCode()
		  {
				int result = base.GetHashCode();
				result = 31 * result + ( int )( LeaderTermConflict ^ ( ( long )( ( ulong )LeaderTermConflict >> 32 ) ) );
				result = 31 * result + ( int )( CommitIndexConflict ^ ( ( long )( ( ulong )CommitIndexConflict >> 32 ) ) );
				result = 31 * result + ( int )( CommitIndexTermConflict ^ ( ( long )( ( ulong )CommitIndexTermConflict >> 32 ) ) );
				return result;
		  }

		  public override string ToString()
		  {
				return format( "Heartbeat from %s {leaderTerm=%d, commitIndex=%d, commitIndexTerm=%d}", FromConflict, LeaderTermConflict, CommitIndexConflict, CommitIndexTermConflict );
		  }
	 }

	 public class RaftMessages_HeartbeatResponse : RaftMessages_BaseRaftMessage
	 {

		  public RaftMessages_HeartbeatResponse( MemberId from ) : base( from, HEARTBEAT_RESPONSE )
		  {
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
		  public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
		  {
				return handler.Handle( this );
		  }

		  public override string ToString()
		  {
				return "HeartbeatResponse{from=" + FromConflict + "}";
		  }
	 }

	 public class RaftMessages_LogCompactionInfo : RaftMessages_BaseRaftMessage
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal long LeaderTermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal long PrevIndexConflict;

		  public RaftMessages_LogCompactionInfo( MemberId from, long leaderTerm, long prevIndex ) : base( from, RaftMessages_Type.LogCompactionInfo )
		  {
				this.LeaderTermConflict = leaderTerm;
				this.PrevIndexConflict = prevIndex;
		  }

		  public virtual long LeaderTerm()
		  {
				return LeaderTermConflict;
		  }

		  public virtual long PrevIndex()
		  {
				return PrevIndexConflict;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
		  public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
		  {
				return handler.Handle( this );
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
				if ( !base.Equals( o ) )
				{
					 return false;
				}

				RaftMessages_LogCompactionInfo other = ( RaftMessages_LogCompactionInfo ) o;

				return LeaderTermConflict == other.LeaderTermConflict && PrevIndexConflict == other.PrevIndexConflict;
		  }

		  public override int GetHashCode()
		  {
				int result = base.GetHashCode();
				result = 31 * result + ( int )( LeaderTermConflict ^ ( ( long )( ( ulong )LeaderTermConflict >> 32 ) ) );
				result = 31 * result + ( int )( PrevIndexConflict ^ ( ( long )( ( ulong )PrevIndexConflict >> 32 ) ) );
				return result;
		  }

		  public override string ToString()
		  {
				return format( "Log compaction from %s {leaderTerm=%d, prevIndex=%d}", FromConflict, LeaderTermConflict, PrevIndexConflict );
		  }
	 }

	 public interface RaftMessages_Timeout
	 {
	 }

	  public class RaftMessages_Timeout_Election : RaftMessages_BaseRaftMessage
	  {
			public RaftMessages_Timeout_Election( MemberId from ) : base( from, RaftMessages_Type.ElectionTimeout )
			{
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override string ToString()
			{
				 return "Timeout.Election{}";
			}
	  }

	  public class RaftMessages_Timeout_Heartbeat : RaftMessages_BaseRaftMessage
	  {
			public RaftMessages_Timeout_Heartbeat( MemberId from ) : base( from, RaftMessages_Type.HeartbeatTimeout )
			{
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override string ToString()
			{
				 return "Timeout.Heartbeat{}";
			}
	  }

	 public interface RaftMessages_NewEntry
	 {
	 }

	  public class RaftMessages_NewEntry_Request : RaftMessages_BaseRaftMessage
	  {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal ReplicatedContent ContentConflict;

			public RaftMessages_NewEntry_Request( MemberId from, ReplicatedContent content ) : base( from, RaftMessages_Type.NewEntryRequest )
			{
				 this.ContentConflict = content;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override string ToString()
			{
				 return format( "NewEntry.Request from %s {content=%s}", FromConflict, ContentConflict );
			}

			public override bool Equals( object o )
			{
				 if ( this == o )
				 {
					  return true;
				 }
				 if ( o == null || this.GetType() != o.GetType() )
				 {
					  return false;
				 }

				 RaftMessages_NewEntry_Request request = ( RaftMessages_NewEntry_Request ) o;

				 return !( ContentConflict != null ?!ContentConflict.Equals( request.ContentConflict ) : request.ContentConflict != null );
			}

			public override int GetHashCode()
			{
				 return ContentConflict != null ? ContentConflict.GetHashCode() : 0;
			}

			public virtual ReplicatedContent Content()
			{
				 return ContentConflict;
			}
	  }

	  public class RaftMessages_NewEntry_BatchRequest : RaftMessages_BaseRaftMessage
	  {
			internal readonly IList<ReplicatedContent> Batch;

			public RaftMessages_NewEntry_BatchRequest( IList<ReplicatedContent> batch ) : base( null, RaftMessages_Type.NewBatchRequest )
			{
				 this.Batch = batch;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
			public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
			{
				 return handler.Handle( this );
			}

			public override bool Equals( object o )
			{
				 if ( this == o )
				 {
					  return true;
				 }
				 if ( o == null || this.GetType() != o.GetType() )
				 {
					  return false;
				 }
				 if ( !base.Equals( o ) )
				 {
					  return false;
				 }
				 RaftMessages_NewEntry_BatchRequest batchRequest = ( RaftMessages_NewEntry_BatchRequest ) o;
				 return Objects.Equals( Batch, batchRequest.Batch );
			}

			public override int GetHashCode()
			{
				 return Objects.hash( base.GetHashCode(), Batch );
			}

			public override string ToString()
			{
				 return "BatchRequest{" +
						  "batch=" + Batch +
						  '}';
			}

			public virtual IList<ReplicatedContent> Contents()
			{
				 return Collections.unmodifiableList( Batch );
			}
	  }

	 public interface RaftMessages_EnrichedRaftMessage<RM> : RaftMessages_RaftMessage where RM : RaftMessages_RaftMessage
	 {
		  RM Message();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		  default org.neo4j.causalclustering.identity.MemberId from()
	//	  {
	//			return message().from();
	//	  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		  default RaftMessages_Type type()
	//	  {
	//			return message().type();
	//	  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		  default <T, E> T dispatch(RaftMessages_Handler<T, E> handler) throws E
	//	  {
	//			return message().dispatch(handler);
	//	  }
	 }

	 public interface RaftMessages_ClusterIdAwareMessage<RM> : RaftMessages_EnrichedRaftMessage<RM> where RM : RaftMessages_RaftMessage
	 {
		  ClusterId ClusterId();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		  static <RM> RaftMessages_ClusterIdAwareMessage<RM> of(org.neo4j.causalclustering.identity.ClusterId clusterId, RM message)
	//	  {
	//			return new RaftMessages.ClusterIdAwareMessageImpl<>(clusterId, message);
	//	  }
	 }

	 public interface RaftMessages_ReceivedInstantAwareMessage<RM> : RaftMessages_EnrichedRaftMessage<RM> where RM : RaftMessages_RaftMessage
	 {
		  Instant ReceivedAt();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		  static <RM> RaftMessages_ReceivedInstantAwareMessage<RM> of(java.time.Instant receivedAt, RM message)
	//	  {
	//			return new RaftMessages.ReceivedInstantAwareMessageImpl<>(receivedAt, message);
	//	  }
	 }

	 public interface RaftMessages_ReceivedInstantClusterIdAwareMessage<RM> : RaftMessages_ReceivedInstantAwareMessage<RM>, RaftMessages_ClusterIdAwareMessage<RM> where RM : RaftMessages_RaftMessage
	 {
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		  static <RM> RaftMessages_ReceivedInstantClusterIdAwareMessage<RM> of(java.time.Instant receivedAt, org.neo4j.causalclustering.identity.ClusterId clusterId, RM message)
	//	  {
	//			return new RaftMessages.ReceivedInstantClusterIdAwareMessageImpl<>(receivedAt, clusterId, message);
	//	  }
	 }

	 public class RaftMessages_ClusterIdAwareMessageImpl<RM> : RaftMessages_ClusterIdAwareMessage<RM> where RM : RaftMessages_RaftMessage
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly ClusterId ClusterIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly RM MessageConflict;

		  internal RaftMessages_ClusterIdAwareMessageImpl( ClusterId clusterId, RM message )
		  {
				Objects.requireNonNull( message );
				this.ClusterIdConflict = clusterId;
				this.MessageConflict = message;
		  }

		  public override ClusterId ClusterId()
		  {
				return ClusterIdConflict;
		  }

		  public override RM Message()
		  {
				return MessageConflict;
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: RaftMessages_ClusterIdAwareMessageImpl<?> that = (RaftMessages_ClusterIdAwareMessageImpl<?>) o;
				RaftMessages_ClusterIdAwareMessageImpl<object> that = ( RaftMessages_ClusterIdAwareMessageImpl<object> ) o;
				return Objects.Equals( ClusterIdConflict, that.ClusterIdConflict ) && Objects.Equals( Message(), that.Message() );
		  }

		  public override int GetHashCode()
		  {
				return Objects.hash( ClusterIdConflict, Message() );
		  }

		  public override string ToString()
		  {
				return format( "{clusterId: %s, message: %s}", ClusterIdConflict, Message() );
		  }
	 }

	 public class RaftMessages_ReceivedInstantAwareMessageImpl<RM> : RaftMessages_ReceivedInstantAwareMessage<RM> where RM : RaftMessages_RaftMessage
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly Instant ReceivedAtConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly RM MessageConflict;

		  internal RaftMessages_ReceivedInstantAwareMessageImpl( Instant receivedAt, RM message )
		  {
				Objects.requireNonNull( message );
				this.ReceivedAtConflict = receivedAt;
				this.MessageConflict = message;
		  }

		  public override Instant ReceivedAt()
		  {
				return ReceivedAtConflict;
		  }

		  public override RM Message()
		  {
				return MessageConflict;
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: RaftMessages_ReceivedInstantAwareMessageImpl<?> that = (RaftMessages_ReceivedInstantAwareMessageImpl<?>) o;
				RaftMessages_ReceivedInstantAwareMessageImpl<object> that = ( RaftMessages_ReceivedInstantAwareMessageImpl<object> ) o;
				return Objects.Equals( ReceivedAtConflict, that.ReceivedAtConflict ) && Objects.Equals( Message(), that.Message() );
		  }

		  public override int GetHashCode()
		  {
				return Objects.hash( ReceivedAtConflict, Message() );
		  }

		  public override string ToString()
		  {
				return format( "{receivedAt: %s, message: %s}", ReceivedAtConflict, Message() );
		  }
	 }

	 public class RaftMessages_ReceivedInstantClusterIdAwareMessageImpl<RM> : RaftMessages_ReceivedInstantClusterIdAwareMessage<RM> where RM : RaftMessages_RaftMessage
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly Instant ReceivedAtConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly ClusterId ClusterIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly RM MessageConflict;

		  internal RaftMessages_ReceivedInstantClusterIdAwareMessageImpl( Instant receivedAt, ClusterId clusterId, RM message )
		  {
				Objects.requireNonNull( message );
				this.ClusterIdConflict = clusterId;
				this.ReceivedAtConflict = receivedAt;
				this.MessageConflict = message;
		  }

		  public override Instant ReceivedAt()
		  {
				return ReceivedAtConflict;
		  }

		  public override ClusterId ClusterId()
		  {
				return ClusterIdConflict;
		  }

		  public override RM Message()
		  {
				return MessageConflict;
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: RaftMessages_ReceivedInstantClusterIdAwareMessageImpl<?> that = (RaftMessages_ReceivedInstantClusterIdAwareMessageImpl<?>) o;
				RaftMessages_ReceivedInstantClusterIdAwareMessageImpl<object> that = ( RaftMessages_ReceivedInstantClusterIdAwareMessageImpl<object> ) o;
				return Objects.Equals( ReceivedAtConflict, that.ReceivedAtConflict ) && Objects.Equals( ClusterIdConflict, that.ClusterIdConflict ) && Objects.Equals( Message(), that.Message() );
		  }

		  public override int GetHashCode()
		  {
				return Objects.hash( ReceivedAtConflict, ClusterIdConflict, Message() );
		  }

		  public override string ToString()
		  {
				return format( "{clusterId: %s, receivedAt: %s, message: %s}", ClusterIdConflict, ReceivedAtConflict, Message() );
		  }
	 }

	 public class RaftMessages_PruneRequest : RaftMessages_BaseRaftMessage
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly long PruneIndexConflict;

		  public RaftMessages_PruneRequest( long pruneIndex ) : base( null, PRUNE_REQUEST )
		  {
				this.PruneIndexConflict = pruneIndex;
		  }

		  public virtual long PruneIndex()
		  {
				return PruneIndexConflict;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T,E extends Exception> T dispatch(RaftMessages_Handler<T,E> handler) throws E
		  public override T Dispatch<T, E>( RaftMessages_Handler<T, E> handler ) where E : Exception
		  {
				return handler.Handle( this );
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
				if ( !base.Equals( o ) )
				{
					 return false;
				}
				RaftMessages_PruneRequest that = ( RaftMessages_PruneRequest ) o;
				return PruneIndexConflict == that.PruneIndexConflict;
		  }

		  public override int GetHashCode()
		  {
				return Objects.hash( base.GetHashCode(), PruneIndexConflict );
		  }
	 }

	 public abstract class RaftMessages_BaseRaftMessage : RaftMessages_RaftMessage
	 {
		 public abstract T Dispatch( RaftMessages_Handler<T, E> handler );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  protected internal readonly MemberId FromConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly RaftMessages_Type TypeConflict;

		  internal RaftMessages_BaseRaftMessage( MemberId from, RaftMessages_Type type )
		  {
				this.FromConflict = from;
				this.TypeConflict = type;
		  }

		  public override MemberId From()
		  {
				return FromConflict;
		  }

		  public override RaftMessages_Type Type()
		  {
				return TypeConflict;
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
				RaftMessages_BaseRaftMessage that = ( RaftMessages_BaseRaftMessage ) o;
				return Objects.Equals( FromConflict, that.FromConflict ) && TypeConflict == that.TypeConflict;
		  }

		  public override int GetHashCode()
		  {
				return Objects.hash( FromConflict, TypeConflict );
		  }
	 }

}