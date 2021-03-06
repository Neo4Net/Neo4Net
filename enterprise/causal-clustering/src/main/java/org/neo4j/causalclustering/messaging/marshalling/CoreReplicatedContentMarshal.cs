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
namespace Org.Neo4j.causalclustering.messaging.marshalling
{
	using ByteBuf = io.netty.buffer.ByteBuf;


	using NewLeaderBarrier = Org.Neo4j.causalclustering.core.consensus.NewLeaderBarrier;
	using MemberIdSet = Org.Neo4j.causalclustering.core.consensus.membership.MemberIdSet;
	using MemberIdSetSerializer = Org.Neo4j.causalclustering.core.consensus.membership.MemberIdSetSerializer;
	using DistributedOperation = Org.Neo4j.causalclustering.core.replication.DistributedOperation;
	using ReplicatedContent = Org.Neo4j.causalclustering.core.replication.ReplicatedContent;
	using DummyRequest = Org.Neo4j.causalclustering.core.state.machines.dummy.DummyRequest;
	using ReplicatedIdAllocationRequest = Org.Neo4j.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest;
	using ReplicatedIdAllocationRequestSerializer = Org.Neo4j.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequestSerializer;
	using ReplicatedLockTokenRequest = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedLockTokenSerializer = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenSerializer;
	using ReplicatedTokenRequest = Org.Neo4j.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using ReplicatedTokenRequestSerializer = Org.Neo4j.causalclustering.core.state.machines.token.ReplicatedTokenRequestSerializer;
	using ReplicatedTransaction = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using ReplicatedTransactionSerializer = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransactionSerializer;
	using Org.Neo4j.causalclustering.core.state.storage;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

	public class CoreReplicatedContentMarshal
	{
		 private const sbyte TX_CONTENT_TYPE = 0;
		 private const sbyte RAFT_MEMBER_SET_TYPE = 1;
		 private const sbyte ID_RANGE_REQUEST_TYPE = 2;
		 private const sbyte TOKEN_REQUEST_TYPE = 4;
		 private const sbyte NEW_LEADER_BARRIER_TYPE = 5;
		 private const sbyte LOCK_TOKEN_REQUEST = 6;
		 private const sbyte DISTRIBUTED_OPERATION = 7;
		 private const sbyte DUMMY_REQUEST = 8;

		 public static Codec<ReplicatedContent> Codec()
		 {
			  return new ReplicatedContentCodec( new CoreReplicatedContentMarshal() );
		 }

		 public static SafeChannelMarshal<ReplicatedContent> Marshaller()
		 {
			  return new ReplicatedContentMarshaller( new CoreReplicatedContentMarshal() );
		 }

		 private CoreReplicatedContentMarshal()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ContentBuilder<org.neo4j.causalclustering.core.replication.ReplicatedContent> unmarshal(byte contentType, io.netty.buffer.ByteBuf buffer) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 private ContentBuilder<ReplicatedContent> Unmarshal( sbyte contentType, ByteBuf buffer )
		 {
			  switch ( contentType )
			  {
			  case TX_CONTENT_TYPE:
			  {
					return ContentBuilder.Finished( ReplicatedTransactionSerializer.decode( buffer ) );
			  }
			  case DUMMY_REQUEST:
					return ContentBuilder.Finished( DummyRequest.decode( buffer ) );
			  default:
					return Unmarshal( contentType, new NetworkReadableClosableChannelNetty4( buffer ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ContentBuilder<org.neo4j.causalclustering.core.replication.ReplicatedContent> unmarshal(byte contentType, org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 private ContentBuilder<ReplicatedContent> Unmarshal( sbyte contentType, ReadableChannel channel )
		 {
			  switch ( contentType )
			  {
			  case TX_CONTENT_TYPE:
					return ContentBuilder.Finished( ReplicatedTransactionSerializer.unmarshal( channel ) );
			  case RAFT_MEMBER_SET_TYPE:
					return ContentBuilder.Finished( MemberIdSetSerializer.unmarshal( channel ) );
			  case ID_RANGE_REQUEST_TYPE:
					return ContentBuilder.Finished( ReplicatedIdAllocationRequestSerializer.unmarshal( channel ) );
			  case TOKEN_REQUEST_TYPE:
					return ContentBuilder.Finished( ReplicatedTokenRequestSerializer.unmarshal( channel ) );
			  case NEW_LEADER_BARRIER_TYPE:
					return ContentBuilder.Finished( new NewLeaderBarrier() );
			  case LOCK_TOKEN_REQUEST:
					return ContentBuilder.Finished( ReplicatedLockTokenSerializer.unmarshal( channel ) );
			  case DISTRIBUTED_OPERATION:
			  {
					return DistributedOperation.deserialize( channel );
			  }
			  case DUMMY_REQUEST:
					return ContentBuilder.Finished( DummyRequest.Marshal.INSTANCE.unmarshal( channel ) );
			  default:
					throw new System.InvalidOperationException( "Not a recognized content type: " + contentType );
			  }
		 }

		 private class ReplicatedContentCodec : Codec<ReplicatedContent>
		 {
			  internal readonly CoreReplicatedContentMarshal Serializer;

			  internal ReplicatedContentCodec( CoreReplicatedContentMarshal serializer )
			  {
					this.Serializer = serializer;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void encode(org.neo4j.causalclustering.core.replication.ReplicatedContent type, java.util.List<Object> output) throws java.io.IOException
			  public override void Encode( ReplicatedContent type, IList<object> output )
			  {
					type.Handle( new EncodingHandlerReplicated( output ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ContentBuilder<org.neo4j.causalclustering.core.replication.ReplicatedContent> decode(io.netty.buffer.ByteBuf byteBuf) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
			  public override ContentBuilder<ReplicatedContent> Decode( ByteBuf byteBuf )
			  {
					return Serializer.unmarshal( byteBuf.readByte(), byteBuf );
			  }
		 }

		 private class ReplicatedContentMarshaller : SafeChannelMarshal<ReplicatedContent>
		 {
			  internal readonly CoreReplicatedContentMarshal Serializer;

			  internal ReplicatedContentMarshaller( CoreReplicatedContentMarshal serializer )
			  {
					this.Serializer = serializer;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(org.neo4j.causalclustering.core.replication.ReplicatedContent replicatedContent, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Marshal( ReplicatedContent replicatedContent, WritableChannel channel )
			  {
					replicatedContent.Handle( new MarshallingHandlerReplicated( channel ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.causalclustering.core.replication.ReplicatedContent unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
			  protected internal override ReplicatedContent Unmarshal0( ReadableChannel channel )
			  {
					sbyte type = channel.Get();
					ContentBuilder<ReplicatedContent> contentBuilder = Serializer.unmarshal( type, channel );
					while ( !contentBuilder.Complete )
					{
						 type = channel.Get();
						 contentBuilder = contentBuilder.Combine( Serializer.unmarshal( type, channel ) );
					}
					return contentBuilder.Build();
			  }
		 }

		 private class EncodingHandlerReplicated : ReplicatedContentHandler
		 {

			  internal readonly IList<object> Output;

			  internal EncodingHandlerReplicated( IList<object> output )
			  {
					this.Output = output;
			  }

			  public override void Handle( ReplicatedTransaction replicatedTransaction )
			  {
					Output.Add( ChunkedReplicatedContent.Chunked( TX_CONTENT_TYPE, new MaxTotalSize( replicatedTransaction.Encode() ) ) );
			  }

			  public override void Handle( MemberIdSet memberIdSet )
			  {
					Output.Add( ChunkedReplicatedContent.Single( RAFT_MEMBER_SET_TYPE, channel => MemberIdSetSerializer.marshal( memberIdSet, channel ) ) );
			  }

			  public override void Handle( ReplicatedIdAllocationRequest replicatedIdAllocationRequest )
			  {
					Output.Add( ChunkedReplicatedContent.Single( ID_RANGE_REQUEST_TYPE, channel => ReplicatedIdAllocationRequestSerializer.marshal( replicatedIdAllocationRequest, channel ) ) );
			  }

			  public override void Handle( ReplicatedTokenRequest replicatedTokenRequest )
			  {
					Output.Add( ChunkedReplicatedContent.Single( TOKEN_REQUEST_TYPE, channel => ReplicatedTokenRequestSerializer.marshal( replicatedTokenRequest, channel ) ) );
			  }

			  public override void Handle( NewLeaderBarrier newLeaderBarrier )
			  {
					Output.Add(ChunkedReplicatedContent.Single(NEW_LEADER_BARRIER_TYPE, channel =>
					{
					}));
			  }

			  public override void Handle( ReplicatedLockTokenRequest replicatedLockTokenRequest )
			  {
					Output.Add( ChunkedReplicatedContent.Single( LOCK_TOKEN_REQUEST, channel => ReplicatedLockTokenSerializer.marshal( replicatedLockTokenRequest, channel ) ) );
			  }

			  public override void Handle( DistributedOperation distributedOperation )
			  {
					Output.Add( ChunkedReplicatedContent.Single( DISTRIBUTED_OPERATION, distributedOperation.marshalMetaData ) );
			  }

			  public override void Handle( DummyRequest dummyRequest )
			  {
					Output.Add( ChunkedReplicatedContent.Chunked( DUMMY_REQUEST, dummyRequest.Encoder() ) );
			  }
		 }

		 private class MarshallingHandlerReplicated : ReplicatedContentHandler
		 {

			  internal readonly WritableChannel WritableChannel;

			  internal MarshallingHandlerReplicated( WritableChannel writableChannel )
			  {
					this.WritableChannel = writableChannel;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.core.state.machines.tx.ReplicatedTransaction replicatedTransaction) throws java.io.IOException
			  public override void Handle( ReplicatedTransaction replicatedTransaction )
			  {
					WritableChannel.put( TX_CONTENT_TYPE );
					replicatedTransaction.Marshal( WritableChannel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.core.consensus.membership.MemberIdSet memberIdSet) throws java.io.IOException
			  public override void Handle( MemberIdSet memberIdSet )
			  {
					WritableChannel.put( RAFT_MEMBER_SET_TYPE );
					MemberIdSetSerializer.marshal( memberIdSet, WritableChannel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest replicatedIdAllocationRequest) throws java.io.IOException
			  public override void Handle( ReplicatedIdAllocationRequest replicatedIdAllocationRequest )
			  {
					WritableChannel.put( ID_RANGE_REQUEST_TYPE );
					ReplicatedIdAllocationRequestSerializer.marshal( replicatedIdAllocationRequest, WritableChannel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.core.state.machines.token.ReplicatedTokenRequest replicatedTokenRequest) throws java.io.IOException
			  public override void Handle( ReplicatedTokenRequest replicatedTokenRequest )
			  {
					WritableChannel.put( TOKEN_REQUEST_TYPE );
					ReplicatedTokenRequestSerializer.marshal( replicatedTokenRequest, WritableChannel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.core.consensus.NewLeaderBarrier newLeaderBarrier) throws java.io.IOException
			  public override void Handle( NewLeaderBarrier newLeaderBarrier )
			  {
					WritableChannel.put( NEW_LEADER_BARRIER_TYPE );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest replicatedLockTokenRequest) throws java.io.IOException
			  public override void Handle( ReplicatedLockTokenRequest replicatedLockTokenRequest )
			  {
					WritableChannel.put( LOCK_TOKEN_REQUEST );
					ReplicatedLockTokenSerializer.marshal( replicatedLockTokenRequest, WritableChannel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.core.replication.DistributedOperation distributedOperation) throws java.io.IOException
			  public override void Handle( DistributedOperation distributedOperation )
			  {
					WritableChannel.put( DISTRIBUTED_OPERATION );
					distributedOperation.MarshalMetaData( WritableChannel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.core.state.machines.dummy.DummyRequest dummyRequest) throws java.io.IOException
			  public override void Handle( DummyRequest dummyRequest )
			  {
					WritableChannel.put( DUMMY_REQUEST );
					DummyRequest.Marshal.INSTANCE.marshal( dummyRequest, WritableChannel );
			  }
		 }
	}

}