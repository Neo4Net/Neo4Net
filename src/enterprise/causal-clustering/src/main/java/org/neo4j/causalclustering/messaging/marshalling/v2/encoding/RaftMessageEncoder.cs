using System;

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
namespace Neo4Net.causalclustering.messaging.marshalling.v2.encoding
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;

	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class RaftMessageEncoder : MessageToByteEncoder<Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void encode(io.netty.channel.ChannelHandlerContext ctx, Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage decoratedMessage, io.netty.buffer.ByteBuf out) throws Exception
		 protected internal override void Encode( ChannelHandlerContext ctx, Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage decoratedMessage, ByteBuf @out )
		 {
			  Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage message = decoratedMessage.message();
			  ClusterId clusterId = decoratedMessage.clusterId();
			  MemberId.Marshal memberMarshal = new MemberId.Marshal();

			  NetworkWritableChannel channel = new NetworkWritableChannel( @out );
			  channel.Put( ContentType.Message.get() );
			  ClusterId.Marshal.INSTANCE.marshal( clusterId, channel );
			  channel.PutInt( message.Type().ordinal() );
			  memberMarshal.MarshalConflict( message.From(), channel );

			  message.Dispatch( new Handler( memberMarshal, channel ) );
		 }

		 private class Handler : Neo4Net.causalclustering.core.consensus.RaftMessages_Handler<Void, Exception>
		 {
			  internal readonly MemberId.Marshal MemberMarshal;
			  internal readonly NetworkWritableChannel Channel;

			  internal Handler( MemberId.Marshal memberMarshal, NetworkWritableChannel channel )
			  {
					this.MemberMarshal = memberMarshal;
					this.Channel = channel;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request voteRequest) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request voteRequest )
			  {
					MemberMarshal.marshal( voteRequest.Candidate(), Channel );
					Channel.putLong( voteRequest.Term() );
					Channel.putLong( voteRequest.LastLogIndex() );
					Channel.putLong( voteRequest.LastLogTerm() );

					return null;
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response voteResponse )
			  {
					Channel.putLong( voteResponse.Term() );
					Channel.put( ( sbyte )( voteResponse.VoteGranted() ? 1 : 0 ) );

					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request preVoteRequest) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request preVoteRequest )
			  {
					MemberMarshal.marshal( preVoteRequest.Candidate(), Channel );
					Channel.putLong( preVoteRequest.Term() );
					Channel.putLong( preVoteRequest.LastLogIndex() );
					Channel.putLong( preVoteRequest.LastLogTerm() );

					return null;
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response preVoteResponse )
			  {
					Channel.putLong( preVoteResponse.Term() );
					Channel.put( ( sbyte )( preVoteResponse.VoteGranted() ? 1 : 0 ) );

					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request appendRequest) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request appendRequest )
			  {
					Channel.putLong( appendRequest.LeaderTerm() );
					Channel.putLong( appendRequest.PrevLogIndex() );
					Channel.putLong( appendRequest.PrevLogTerm() );
					Channel.putLong( appendRequest.LeaderCommit() );
					Channel.putInt( appendRequest.Entries().Length );

					return null;
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response appendResponse )
			  {
					Channel.putLong( appendResponse.Term() );
					Channel.put( ( sbyte )( appendResponse.Success() ? 1 : 0 ) );
					Channel.putLong( appendResponse.MatchIndex() );
					Channel.putLong( appendResponse.AppendIndex() );

					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request newEntryRequest) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request newEntryRequest )
			  {
					return null;
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat heartbeat )
			  {
					Channel.putLong( heartbeat.LeaderTerm() );
					Channel.putLong( heartbeat.CommitIndexTerm() );
					Channel.putLong( heartbeat.CommitIndex() );

					return null;
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse heartbeatResponse )
			  {
					// Heartbeat Response does not have any data attached to it.
					return null;
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo logCompactionInfo )
			  {
					Channel.putLong( logCompactionInfo.LeaderTerm() );
					Channel.putLong( logCompactionInfo.PrevIndex() );
					return null;
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election )
			  {
					return null; // Not network
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Heartbeat heartbeat )
			  {
					return null; // Not network
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest batchRequest )
			  {
					return null; // Not network
			  }

			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PruneRequest pruneRequest )
			  {
					return null; // Not network
			  }
		 }
	}

}