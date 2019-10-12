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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;


	using Org.Neo4j.causalclustering.core.state.storage;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using EndOfStreamException = Org.Neo4j.causalclustering.messaging.EndOfStreamException;
	using NetworkWritableChannel = Org.Neo4j.causalclustering.messaging.NetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Org.Neo4j.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using StoreCopyRequest = Org.Neo4j.causalclustering.messaging.StoreCopyRequest;
	using StoreIdMarshal = Org.Neo4j.causalclustering.messaging.marshalling.storeid.StoreIdMarshal;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

	public class GetIndexFilesRequest : StoreCopyRequest
	{
		 private readonly StoreId _expectedStoreId;
		 private readonly long _indexId;
		 private readonly long _requiredTransactionId;

		 public GetIndexFilesRequest( StoreId expectedStoreId, long indexId, long requiredTransactionId )
		 {
			  this._expectedStoreId = expectedStoreId;
			  this._indexId = indexId;
			  this._requiredTransactionId = requiredTransactionId;
		 }

		 public override StoreId ExpectedStoreId()
		 {
			  return _expectedStoreId;
		 }

		 public override long RequiredTransactionId()
		 {
			  return _requiredTransactionId;
		 }

		 public virtual long IndexId()
		 {
			  return _indexId;
		 }

		 public override RequestMessageType MessageType()
		 {
			  return RequestMessageType.INDEX_SNAPSHOT;
		 }

		 internal class IndexSnapshotRequestMarshall : SafeChannelMarshal<GetIndexFilesRequest>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected GetIndexFilesRequest unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
			  protected internal override GetIndexFilesRequest Unmarshal0( ReadableChannel channel )
			  {
					StoreId storeId = StoreIdMarshal.INSTANCE.unmarshal( channel );
					long requiredTransactionId = channel.Long;
					long indexId = channel.Long;
					return new GetIndexFilesRequest( storeId, indexId, requiredTransactionId );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(GetIndexFilesRequest getIndexFilesRequest, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Marshal( GetIndexFilesRequest getIndexFilesRequest, WritableChannel channel )
			  {
					StoreIdMarshal.INSTANCE.marshal( getIndexFilesRequest.ExpectedStoreId(), channel );
					channel.PutLong( getIndexFilesRequest.RequiredTransactionId() );
					channel.PutLong( getIndexFilesRequest.IndexId() );
			  }
		 }

		 public class Encoder : MessageToByteEncoder<GetIndexFilesRequest>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void encode(io.netty.channel.ChannelHandlerContext ctx, GetIndexFilesRequest msg, io.netty.buffer.ByteBuf out) throws Exception
			  protected internal override void Encode( ChannelHandlerContext ctx, GetIndexFilesRequest msg, ByteBuf @out )
			  {
					( new IndexSnapshotRequestMarshall() ).Marshal(msg, new NetworkWritableChannel(@out));
			  }
		 }

		 public class Decoder : ByteToMessageDecoder
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void decode(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf in, java.util.List<Object> out) throws Exception
			  protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf @in, IList<object> @out )
			  {
					GetIndexFilesRequest getIndexFilesRequest = ( new IndexSnapshotRequestMarshall() ).Unmarshal0(new NetworkReadableClosableChannelNetty4(@in));
					@out.Add( getIndexFilesRequest );
			  }
		 }

		 public override string ToString()
		 {
			  return "GetIndexFilesRequest{" + "expectedStoreId=" + _expectedStoreId + ", indexId=" + _indexId + ", requiredTransactionId=" + _requiredTransactionId + '}';
		 }
	}

}