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
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using Neo4Net.causalclustering.core.state.storage;
	using BoundedNetworkWritableChannel = Neo4Net.causalclustering.messaging.BoundedNetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Neo4Net.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;
	using UTF8 = Neo4Net.Strings.UTF8;

	public class PrepareStoreCopyResponse
	{
		 private readonly File[] _files;
		 private readonly LongSet _indexIds;
		 private readonly long? _transactionId;
		 private readonly Status _status;

		 public static PrepareStoreCopyResponse Error( Status errorStatus )
		 {
			  if ( errorStatus == Status.Success )
			  {
					throw new System.InvalidOperationException( "Cannot create error result from state: " + errorStatus );
			  }
			  return new PrepareStoreCopyResponse( new File[0], LongSets.immutable.empty(), 0L, errorStatus );
		 }

		 public static PrepareStoreCopyResponse Success( File[] storeFiles, LongSet indexIds, long lastTransactionId )
		 {
			  return new PrepareStoreCopyResponse( storeFiles, indexIds, lastTransactionId, Status.Success );
		 }

		 internal virtual LongSet IndexIds
		 {
			 get
			 {
				  return _indexIds;
			 }
		 }

		 internal enum Status
		 {
			  Success,
			  EStoreIdMismatch,
			  EListingStore
		 }

		 private PrepareStoreCopyResponse( File[] files, LongSet indexIds, long? transactionId, Status status )
		 {
			  this._files = files;
			  this._indexIds = indexIds;
			  this._transactionId = transactionId;
			  this._status = status;
		 }

		 public virtual File[] Files
		 {
			 get
			 {
				  return _files;
			 }
		 }

		 internal virtual long LastTransactionId()
		 {
			  return _transactionId.Value;
		 }

		 public virtual Status Status()
		 {
			  return _status;
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
			  PrepareStoreCopyResponse that = ( PrepareStoreCopyResponse ) o;
			  return Arrays.Equals( _files, that._files ) && _indexIds.Equals( that._indexIds ) && Objects.Equals( _transactionId, that._transactionId ) && _status == that._status;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( Arrays.GetHashCode( _files ), _indexIds, _transactionId, _status );
		 }

		 public class StoreListingMarshal : SafeChannelMarshal<PrepareStoreCopyResponse>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(PrepareStoreCopyResponse prepareStoreCopyResponse, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel buffer) throws java.io.IOException
			  public override void Marshal( PrepareStoreCopyResponse prepareStoreCopyResponse, WritableChannel buffer )
			  {
					buffer.PutInt( ( int )prepareStoreCopyResponse._status );
					buffer.PutLong( prepareStoreCopyResponse._transactionId.Value );
					MarshalFiles( buffer, prepareStoreCopyResponse._files );
					MarshalIndexIds( buffer, prepareStoreCopyResponse._indexIds );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected PrepareStoreCopyResponse unmarshal0(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  protected internal override PrepareStoreCopyResponse Unmarshal0( ReadableChannel channel )
			  {
					int ordinal = channel.Int;
					Status status = Enum.GetValues( typeof( Status ) )[ordinal];
					long? transactionId = channel.Long;
					File[] files = UnmarshalFiles( channel );
					LongSet indexIds = UnmarshalIndexIds( channel );
					return new PrepareStoreCopyResponse( files, indexIds, transactionId, status );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void marshalFiles(org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel buffer, java.io.File[] files) throws java.io.IOException
			  internal static void MarshalFiles( WritableChannel buffer, File[] files )
			  {
					buffer.PutInt( Files.Length );
					foreach ( File file in files )
					{
						 PutBytes( buffer, file.Name );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void marshalIndexIds(org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel buffer, org.eclipse.collections.api.set.primitive.LongSet indexIds) throws java.io.IOException
			  internal virtual void MarshalIndexIds( WritableChannel buffer, LongSet indexIds )
			  {
					buffer.PutInt( indexIds.size() );
					LongIterator itr = indexIds.longIterator();
					while ( itr.hasNext() )
					{
						 long indexId = itr.next();
						 buffer.PutLong( indexId );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File[] unmarshalFiles(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  internal static File[] UnmarshalFiles( ReadableChannel channel )
			  {
					int numberOfFiles = channel.Int;
					File[] files = new File[numberOfFiles];
					for ( int i = 0; i < numberOfFiles; i++ )
					{
						 files[i] = UnmarshalFile( channel );
					}
					return files;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File unmarshalFile(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  internal static File UnmarshalFile( ReadableChannel channel )
			  {
					sbyte[] name = ReadBytes( channel );
					return new File( UTF8.decode( name ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.eclipse.collections.api.set.primitive.LongSet unmarshalIndexIds(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  internal virtual LongSet UnmarshalIndexIds( ReadableChannel channel )
			  {
					int numberOfIndexIds = channel.Int;
					MutableLongSet indexIds = new LongHashSet( numberOfIndexIds );
					for ( int i = 0; i < numberOfIndexIds; i++ )
					{
						 indexIds.add( channel.Long );
					}
					return indexIds;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void putBytes(org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel buffer, String value) throws java.io.IOException
			  internal static void PutBytes( WritableChannel buffer, string value )
			  {
					sbyte[] bytes = UTF8.encode( value );
					buffer.PutInt( bytes.Length );
					buffer.Put( bytes, bytes.Length );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static byte[] readBytes(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  internal static sbyte[] ReadBytes( ReadableChannel channel )
			  {
					int bytesLength = channel.Int;
					sbyte[] bytes = new sbyte[bytesLength];
					channel.Get( bytes, bytesLength );
					return bytes;
			  }
		 }

		 public class Encoder : MessageToByteEncoder<PrepareStoreCopyResponse>
		 {

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void encode(io.netty.channel.ChannelHandlerContext channelHandlerContext, PrepareStoreCopyResponse prepareStoreCopyResponse, io.netty.buffer.ByteBuf byteBuf) throws Exception
			  protected internal override void Encode( ChannelHandlerContext channelHandlerContext, PrepareStoreCopyResponse prepareStoreCopyResponse, ByteBuf byteBuf )
			  {
					( new PrepareStoreCopyResponse.StoreListingMarshal() ).Marshal(prepareStoreCopyResponse, new BoundedNetworkWritableChannel(byteBuf));
			  }
		 }

		 public class Decoder : ByteToMessageDecoder
		 {

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void decode(io.netty.channel.ChannelHandlerContext channelHandlerContext, io.netty.buffer.ByteBuf byteBuf, java.util.List<Object> list) throws Exception
			  protected internal override void Decode( ChannelHandlerContext channelHandlerContext, ByteBuf byteBuf, IList<object> list )
			  {
					list.Add( ( new PrepareStoreCopyResponse.StoreListingMarshal() ).Unmarshal(new NetworkReadableClosableChannelNetty4(byteBuf)) );
			  }
		 }
	}

}