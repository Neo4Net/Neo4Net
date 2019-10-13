using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using UTF8 = Neo4Net.@string.UTF8;

	public class GetStoreFileRequest : StoreCopyRequest
	{
		 private readonly StoreId _expectedStoreId;
		 private readonly File _file;
		 private readonly long _requiredTransactionId;

		 public GetStoreFileRequest( StoreId expectedStoreId, File file, long requiredTransactionId )
		 {
			  this._expectedStoreId = expectedStoreId;
			  this._file = file;
			  this._requiredTransactionId = requiredTransactionId;
		 }

		 public override long RequiredTransactionId()
		 {
			  return _requiredTransactionId;
		 }

		 public override StoreId ExpectedStoreId()
		 {
			  return _expectedStoreId;
		 }

		 internal virtual File File()
		 {
			  return _file;
		 }

		 public override RequestMessageType MessageType()
		 {
			  return RequestMessageType.STORE_FILE;
		 }

		 internal class StoreFileRequestMarshall : SafeChannelMarshal<GetStoreFileRequest>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected GetStoreFileRequest unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
			  protected internal override GetStoreFileRequest Unmarshal0( ReadableChannel channel )
			  {
					StoreId storeId = StoreIdMarshal.INSTANCE.unmarshal( channel );
					long requiredTransactionId = channel.Long;
					int fileNameLength = channel.Int;
					sbyte[] fileNameBytes = new sbyte[fileNameLength];
					channel.Get( fileNameBytes, fileNameLength );
					return new GetStoreFileRequest( storeId, new File( UTF8.decode( fileNameBytes ) ), requiredTransactionId );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(GetStoreFileRequest getStoreFileRequest, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Marshal( GetStoreFileRequest getStoreFileRequest, WritableChannel channel )
			  {
					StoreIdMarshal.INSTANCE.marshal( getStoreFileRequest.ExpectedStoreId(), channel );
					channel.PutLong( getStoreFileRequest.RequiredTransactionId() );
					string name = getStoreFileRequest.File().Name;
					channel.PutInt( name.Length );
					channel.Put( UTF8.encode( name ), name.Length );
			  }
		 }

		 public class Encoder : MessageToByteEncoder<GetStoreFileRequest>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void encode(io.netty.channel.ChannelHandlerContext ctx, GetStoreFileRequest msg, io.netty.buffer.ByteBuf out) throws Exception
			  protected internal override void Encode( ChannelHandlerContext ctx, GetStoreFileRequest msg, ByteBuf @out )
			  {
					( new GetStoreFileRequest.StoreFileRequestMarshall() ).Marshal(msg, new NetworkWritableChannel(@out));
			  }
		 }

		 public class Decoder : ByteToMessageDecoder
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void decode(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf in, java.util.List<Object> out) throws Exception
			  protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf @in, IList<object> @out )
			  {
					GetStoreFileRequest getStoreFileRequest = ( new GetStoreFileRequest.StoreFileRequestMarshall() ).Unmarshal0(new NetworkReadableClosableChannelNetty4(@in));
					@out.Add( getStoreFileRequest );
			  }
		 }

		 public override string ToString()
		 {
			  return "GetStoreFileRequest{" + "expectedStoreId=" + _expectedStoreId + ", file=" + _file.Name + ", requiredTransactionId=" + _requiredTransactionId + '}';
		 }
	}

}