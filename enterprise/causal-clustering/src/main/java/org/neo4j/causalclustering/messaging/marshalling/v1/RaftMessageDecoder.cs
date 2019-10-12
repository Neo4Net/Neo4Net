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
namespace Org.Neo4j.causalclustering.messaging.marshalling.v1
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;


	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using ReplicatedContent = Org.Neo4j.causalclustering.core.replication.ReplicatedContent;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.messaging.marshalling;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;

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
		 private readonly ChannelMarshal<ReplicatedContent> _marshal;
		 private readonly Clock _clock;

		 public RaftMessageDecoder( ChannelMarshal<ReplicatedContent> marshal, Clock clock )
		 {
			  this._marshal = marshal;
			  this._clock = clock;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void decode(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf buffer, java.util.List<Object> list) throws Exception
		 public override void Decode( ChannelHandlerContext ctx, ByteBuf buffer, IList<object> list )
		 {
			  ReadableChannel channel = new NetworkReadableClosableChannelNetty4( buffer );
			  ClusterId clusterId = ClusterId.Marshal.INSTANCE.unmarshal( channel );

			  int messageTypeWire = channel.Int;
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Type[] values = Enum.GetValues( typeof( Org.Neo4j.causalclustering.core.consensus.RaftMessages_Type ) );
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Type messageType = values[messageTypeWire];

			  MemberId from = RetrieveMember( channel );
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage result;

			  if ( messageType.Equals( VOTE_REQUEST ) )
			  {
					MemberId candidate = RetrieveMember( channel );

					long term = channel.Long;
					long lastLogIndex = channel.Long;
					long lastLogTerm = channel.Long;

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request( from, term, candidate, lastLogIndex, lastLogTerm );
			  }
			  else if ( messageType.Equals( VOTE_RESPONSE ) )
			  {
					long term = channel.Long;
					bool voteGranted = channel.Get() == 1;

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response( from, term, voteGranted );
			  }
			  else if ( messageType.Equals( PRE_VOTE_REQUEST ) )
			  {
					MemberId candidate = RetrieveMember( channel );

					long term = channel.Long;
					long lastLogIndex = channel.Long;
					long lastLogTerm = channel.Long;

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Request( from, term, candidate, lastLogIndex, lastLogTerm );
			  }
			  else if ( messageType.Equals( PRE_VOTE_RESPONSE ) )
			  {
					long term = channel.Long;
					bool voteGranted = channel.Get() == 1;

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_PreVote_Response( from, term, voteGranted );
			  }
			  else if ( messageType.Equals( APPEND_ENTRIES_REQUEST ) )
			  {
					// how many
					long term = channel.Long;
					long prevLogIndex = channel.Long;
					long prevLogTerm = channel.Long;

					long leaderCommit = channel.Long;
					long count = channel.Long;

					RaftLogEntry[] entries = new RaftLogEntry[( int ) count];
					for ( int i = 0; i < count; i++ )
					{
						 long entryTerm = channel.Long;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.core.replication.ReplicatedContent content = marshal.unmarshal(channel);
						 ReplicatedContent content = _marshal.unmarshal( channel );
						 entries[i] = new RaftLogEntry( entryTerm, content );
					}

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( from, term, prevLogIndex, prevLogTerm, entries, leaderCommit );
			  }
			  else if ( messageType.Equals( APPEND_ENTRIES_RESPONSE ) )
			  {
					long term = channel.Long;
					bool success = channel.Get() == 1;
					long matchIndex = channel.Long;
					long appendIndex = channel.Long;

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( from, term, success, matchIndex, appendIndex );
			  }
			  else if ( messageType.Equals( NEW_ENTRY_REQUEST ) )
			  {
					ReplicatedContent content = _marshal.unmarshal( channel );

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_NewEntry_Request( from, content );
			  }
			  else if ( messageType.Equals( HEARTBEAT ) )
			  {
					long leaderTerm = channel.Long;
					long commitIndexTerm = channel.Long;
					long commitIndex = channel.Long;

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat( from, leaderTerm, commitIndex, commitIndexTerm );
			  }
			  else if ( messageType.Equals( HEARTBEAT_RESPONSE ) )
			  {
					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_HeartbeatResponse( from );
			  }
			  else if ( messageType.Equals( LOG_COMPACTION_INFO ) )
			  {
					long leaderTerm = channel.Long;
					long prevIndex = channel.Long;

					result = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_LogCompactionInfo( from, leaderTerm, prevIndex );
			  }
			  else
			  {
					throw new System.ArgumentException( "Unknown message type" );
			  }

			  list.Add( Org.Neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of( _clock.instant(), clusterId, result ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.identity.MemberId retrieveMember(org.neo4j.storageengine.api.ReadableChannel buffer) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 private MemberId RetrieveMember( ReadableChannel buffer )
		 {
			  MemberId.Marshal memberIdMarshal = new MemberId.Marshal();
			  return memberIdMarshal.Unmarshal( buffer );
		 }
	}

}