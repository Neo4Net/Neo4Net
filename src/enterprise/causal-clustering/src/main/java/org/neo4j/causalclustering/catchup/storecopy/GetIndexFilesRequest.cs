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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;


	using Neo4Net.causalclustering.core.state.storage;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using NetworkWritableChannel = Neo4Net.causalclustering.messaging.NetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Neo4Net.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using StoreCopyRequest = Neo4Net.causalclustering.messaging.StoreCopyRequest;
	using StoreIdMarshal = Neo4Net.causalclustering.messaging.marshalling.storeid.StoreIdMarshal;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

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
//ORIGINAL LINE: protected GetIndexFilesRequest unmarshal0(org.Neo4Net.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
			  protected internal override GetIndexFilesRequest Unmarshal0( ReadableChannel channel )
			  {
					StoreId storeId = StoreIdMarshal.INSTANCE.unmarshal( channel );
					long requiredTransactionId = channel.Long;
					long indexId = channel.Long;
					return new GetIndexFilesRequest( storeId, indexId, requiredTransactionId );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(GetIndexFilesRequest getIndexFilesRequest, org.Neo4Net.storageengine.api.WritableChannel channel) throws java.io.IOException
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