using System;
using System.Collections.Generic;

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
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToMessageEncoder = io.netty.handler.codec.MessageToMessageEncoder;


	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.messaging.marshalling.v2.encoding.RaftLogEntryTermsSerializer.serializeTerms;

	/// <summary>
	/// Serializes a raft messages content in the order Message, RaftLogTerms, ReplicatedContent.
	/// </summary>
	public class RaftMessageContentEncoder : MessageToMessageEncoder<Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage>
	{

		 private readonly Codec<ReplicatedContent> _codec;

		 public RaftMessageContentEncoder( Codec<ReplicatedContent> replicatedContentCodec )
		 {
			  this._codec = replicatedContentCodec;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void encode(io.netty.channel.ChannelHandlerContext ctx, org.Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage msg, java.util.List<Object> out) throws Exception
		 protected internal override void Encode( ChannelHandlerContext ctx, Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage msg, IList<object> @out )
		 {
			  @out.Add( msg );
			  Handler replicatedContentHandler = new Handler( this, @out, ctx.alloc() );
			  msg.message().dispatch(replicatedContentHandler);
		 }

		 private class Handler : Neo4Net.causalclustering.core.consensus.RaftMessages_Handler<Void, Exception>
		 {
			 private readonly RaftMessageContentEncoder _outerInstance;

			  internal readonly IList<object> Out;
			  internal readonly ByteBufAllocator Alloc;

			  internal Handler( RaftMessageContentEncoder outerInstance, IList<object> @out, ByteBufAllocator alloc )
			  {
				  this._outerInstance = outerInstance;
					this.Out = @out;
					this.Alloc = alloc;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request request) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request request )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response response) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response response )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request request) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request request )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response response) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response response )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request )
			  {
					Out.Add( serializeTerms( request.Entries(), Alloc ) );
					foreach ( RaftLogEntry entry in request.Entries() )
					{
						 SerializableContents( entry.Content(), Out );
					}
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat heartbeat) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat heartbeat )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo logCompactionInfo) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo logCompactionInfo )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse heartbeatResponse) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse heartbeatResponse )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request )
			  {
					SerializableContents( request.Content(), Out );
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Election election )
			  {
					return IllegalOutbound( election );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Heartbeat heartbeat) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_Timeout_Heartbeat heartbeat )
			  {
					return IllegalOutbound( heartbeat );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest batchRequest) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest batchRequest )
			  {
					return IllegalOutbound( batchRequest );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void handle(org.Neo4Net.causalclustering.core.consensus.RaftMessages_PruneRequest pruneRequest) throws Exception
			  public override Void Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_PruneRequest pruneRequest )
			  {
					return IllegalOutbound( pruneRequest );
			  }

			  internal virtual Void IllegalOutbound( Neo4Net.causalclustering.core.consensus.RaftMessages_BaseRaftMessage raftMessage )
			  {
					// not network
					throw new System.InvalidOperationException( "Illegal outbound call: " + raftMessage.GetType() );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void serializableContents(org.Neo4Net.causalclustering.core.replication.ReplicatedContent content, java.util.List<Object> out) throws java.io.IOException
			  internal virtual void SerializableContents( ReplicatedContent content, IList<object> @out )
			  {
					@out.Add( ContentType.ReplicatedContent );
					outerInstance.codec.Encode( content, @out );
			  }
		 }
	}

}