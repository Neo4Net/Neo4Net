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
namespace Neo4Net.causalclustering.messaging.marshalling.v2.decoding
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;


	using Neo4Net.causalclustering.catchup;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using Neo4Net.causalclustering.core.consensus;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.APPEND_ENTRIES_REQUEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.APPEND_ENTRIES_RESPONSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.HEARTBEAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.HEARTBEAT_RESPONSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.LOG_COMPACTION_INFO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.NEW_ENTRY_REQUEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.PRE_VOTE_REQUEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.PRE_VOTE_RESPONSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.VOTE_REQUEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMessages_Type.VOTE_RESPONSE;

	public class RaftMessageDecoder : ByteToMessageDecoder
	{
		 private readonly Protocol<ContentType> _protocol;

		 internal RaftMessageDecoder( Protocol<ContentType> protocol )
		 {
			  this._protocol = protocol;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void decode(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf buffer, java.util.List<Object> list) throws Exception
		 public override void Decode( ChannelHandlerContext ctx, ByteBuf buffer, IList<object> list )
		 {
			  ReadableChannel channel = new NetworkReadableClosableChannelNetty4( buffer );
			  ClusterId clusterId = ClusterId.Marshal.INSTANCE.unmarshal( channel );

			  int messageTypeWire = channel.Int;
			  Neo4Net.causalclustering.core.consensus.RaftMessages_Type[] values = Enum.GetValues( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_Type ) );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_Type messageType = values[messageTypeWire];

			  MemberId from = RetrieveMember( channel );
			  LazyComposer composer;

			  if ( messageType.Equals( VOTE_REQUEST ) )
			  {
					MemberId candidate = RetrieveMember( channel );

					long term = channel.Long;
					long lastLogIndex = channel.Long;
					long lastLogTerm = channel.Long;

					composer = new SimpleMessageComposer( new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request( from, term, candidate, lastLogIndex, lastLogTerm ) );
			  }
			  else if ( messageType.Equals( VOTE_RESPONSE ) )
			  {
					long term = channel.Long;
					bool voteGranted = channel.Get() == 1;

					composer = new SimpleMessageComposer( new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response( from, term, voteGranted ) );
			  }
			  else if ( messageType.Equals( PRE_VOTE_REQUEST ) )
			  {
					MemberId candidate = RetrieveMember( channel );

					long term = channel.Long;
					long lastLogIndex = channel.Long;
					long lastLogTerm = channel.Long;

					composer = new SimpleMessageComposer( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request( from, term, candidate, lastLogIndex, lastLogTerm ) );
			  }
			  else if ( messageType.Equals( PRE_VOTE_RESPONSE ) )
			  {
					long term = channel.Long;
					bool voteGranted = channel.Get() == 1;

					composer = new SimpleMessageComposer( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( from, term, voteGranted ) );
			  }
			  else if ( messageType.Equals( APPEND_ENTRIES_REQUEST ) )
			  {
					// how many
					long term = channel.Long;
					long prevLogIndex = channel.Long;
					long prevLogTerm = channel.Long;
					long leaderCommit = channel.Long;
					int entryCount = channel.Int;

					composer = new AppendEntriesComposer( entryCount, from, term, prevLogIndex, prevLogTerm, leaderCommit );
			  }
			  else if ( messageType.Equals( APPEND_ENTRIES_RESPONSE ) )
			  {
					long term = channel.Long;
					bool success = channel.Get() == 1;
					long matchIndex = channel.Long;
					long appendIndex = channel.Long;

					composer = new SimpleMessageComposer( new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( from, term, success, matchIndex, appendIndex ) );
			  }
			  else if ( messageType.Equals( NEW_ENTRY_REQUEST ) )
			  {
					composer = new NewEntryRequestComposer( from );
			  }
			  else if ( messageType.Equals( HEARTBEAT ) )
			  {
					long leaderTerm = channel.Long;
					long commitIndexTerm = channel.Long;
					long commitIndex = channel.Long;

					composer = new SimpleMessageComposer( new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat( from, leaderTerm, commitIndex, commitIndexTerm ) );
			  }
			  else if ( messageType.Equals( HEARTBEAT_RESPONSE ) )
			  {
					composer = new SimpleMessageComposer( new Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse( from ) );
			  }
			  else if ( messageType.Equals( LOG_COMPACTION_INFO ) )
			  {
					long leaderTerm = channel.Long;
					long prevIndex = channel.Long;

					composer = new SimpleMessageComposer( new Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo( from, leaderTerm, prevIndex ) );
			  }
			  else
			  {
					throw new System.ArgumentException( "Unknown message type" );
			  }

			  list.Add( new ClusterIdAwareMessageComposer( composer, clusterId ) );
			  _protocol.expect( ContentType.ContentType );
		 }

		 internal class ClusterIdAwareMessageComposer
		 {
			  internal readonly LazyComposer Composer;
			  internal readonly ClusterId ClusterId;

			  internal ClusterIdAwareMessageComposer( LazyComposer composer, ClusterId clusterId )
			  {
					this.Composer = composer;
					this.ClusterId = clusterId;
			  }

			  internal virtual Optional<Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage> MaybeCompose( Clock clock, LinkedList<long> terms, LinkedList<ReplicatedContent> contents )
			  {
					return Composer.maybeComplete( terms, contents ).map( m => RaftMessages_ReceivedInstantClusterIdAwareMessage.of( clock.instant(), ClusterId, m ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.identity.MemberId retrieveMember(org.neo4j.storageengine.api.ReadableChannel buffer) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 private MemberId RetrieveMember( ReadableChannel buffer )
		 {
			  MemberId.Marshal memberIdMarshal = new MemberId.Marshal();
			  return memberIdMarshal.Unmarshal( buffer );
		 }

		 internal interface LazyComposer
		 {
			  /// <summary>
			  /// Builds the complete raft message if provided collections contain enough data for building the complete message.
			  /// </summary>
			  Optional<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> MaybeComplete( LinkedList<long> terms, LinkedList<ReplicatedContent> contents );
		 }

		 /// <summary>
		 /// A plain message without any more internal content.
		 /// </summary>
		 private class SimpleMessageComposer : LazyComposer
		 {
			  internal readonly Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage Message;

			  internal SimpleMessageComposer( Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage message )
			  {
					this.Message = message;
			  }

			  public override Optional<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> MaybeComplete( LinkedList<long> terms, LinkedList<ReplicatedContent> contents )
			  {
					return Message;
			  }
		 }

		 private class AppendEntriesComposer : LazyComposer
		 {
			  internal readonly int EntryCount;
			  internal readonly MemberId From;
			  internal readonly long Term;
			  internal readonly long PrevLogIndex;
			  internal readonly long PrevLogTerm;
			  internal readonly long LeaderCommit;

			  internal AppendEntriesComposer( int entryCount, MemberId from, long term, long prevLogIndex, long prevLogTerm, long leaderCommit )
			  {
					this.EntryCount = entryCount;
					this.From = from;
					this.Term = term;
					this.PrevLogIndex = prevLogIndex;
					this.PrevLogTerm = prevLogTerm;
					this.LeaderCommit = leaderCommit;
			  }

			  public override Optional<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> MaybeComplete( LinkedList<long> terms, LinkedList<ReplicatedContent> contents )
			  {
					if ( terms.Count < EntryCount || contents.Count < EntryCount )
					{
						 return null;
					}

					RaftLogEntry[] entries = new RaftLogEntry[EntryCount];
					for ( int i = 0; i < EntryCount; i++ )
					{
						 long term = terms.RemoveFirst();
						 ReplicatedContent content = contents.RemoveFirst();
						 entries[i] = new RaftLogEntry( term, content );
					}
					return ( new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( From, Term, PrevLogIndex, PrevLogTerm, entries, LeaderCommit ) );
			  }
		 }

		 private class NewEntryRequestComposer : LazyComposer
		 {
			  internal readonly MemberId From;

			  internal NewEntryRequestComposer( MemberId from )
			  {
					this.From = from;
			  }

			  public override Optional<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> MaybeComplete( LinkedList<long> terms, LinkedList<ReplicatedContent> contents )
			  {
					if ( contents.Count == 0 )
					{
						 return null;
					}
					else
					{
						 return ( new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( From, contents.RemoveFirst() ) );
					}
			  }
		 }
	}

}