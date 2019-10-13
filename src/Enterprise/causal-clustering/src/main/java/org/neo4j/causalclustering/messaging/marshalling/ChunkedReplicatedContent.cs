using System;
using System.Diagnostics;

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
namespace Neo4Net.causalclustering.messaging.marshalling
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using CompositeByteBuf = io.netty.buffer.CompositeByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;

	using Neo4Net.Functions;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class ChunkedReplicatedContent : ChunkedInput<ByteBuf>
	{
		 private const int METADATA_SIZE = 1;

		 internal static ChunkedInput<ByteBuf> Single( sbyte contentType, ThrowingConsumer<WritableChannel, IOException> marshaller )
		 {
			  return Chunked( contentType, new Single( marshaller ) );
		 }

		 internal static ChunkedInput<ByteBuf> Chunked( sbyte contentType, ChunkedInput<ByteBuf> chunkedInput )
		 {
			  return new ChunkedReplicatedContent( contentType, chunkedInput );
		 }

		 private static int MetadataSize( bool isFirstChunk )
		 {
			  return METADATA_SIZE + ( isFirstChunk ? 1 : 0 );
		 }

		 private static ByteBuf WriteMetadata( bool isFirstChunk, bool isLastChunk, sbyte contentType, ByteBuf buffer )
		 {
			  buffer.writeBoolean( isLastChunk );
			  if ( isFirstChunk )
			  {
					buffer.writeByte( contentType );
			  }
			  return buffer;
		 }

		 private readonly sbyte _contentType;
		 private readonly ChunkedInput<ByteBuf> _byteBufAwareMarshal;
		 private bool _endOfInput;
		 private int _progress;

		 private ChunkedReplicatedContent( sbyte contentType, ChunkedInput<ByteBuf> byteBufAwareMarshal )
		 {
			  this._byteBufAwareMarshal = byteBufAwareMarshal;
			  this._contentType = contentType;
		 }

		 public override bool EndOfInput
		 {
			 get
			 {
				  return _endOfInput;
			 }
		 }

		 public override void Close()
		 {
			  // do nothing
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public io.netty.buffer.ByteBuf readChunk(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		 public override ByteBuf ReadChunk( ChannelHandlerContext ctx )
		 {
			  return ReadChunk( ctx.alloc() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public io.netty.buffer.ByteBuf readChunk(io.netty.buffer.ByteBufAllocator allocator) throws Exception
		 public override ByteBuf ReadChunk( ByteBufAllocator allocator )
		 {
			  if ( _endOfInput )
			  {
					return null;
			  }
			  ByteBuf data = _byteBufAwareMarshal.readChunk( allocator );
			  if ( data == null )
			  {
					return null;
			  }
			  _endOfInput = _byteBufAwareMarshal.EndOfInput;
			  CompositeByteBuf allData = new CompositeByteBuf( allocator, false, 2 );
			  allData.addComponent( true, data );
			  try
			  {
					bool isFirstChunk = Progress() == 0;
					int metaDataCapacity = MetadataSize( isFirstChunk );
					ByteBuf metaDataBuffer = allocator.buffer( metaDataCapacity, metaDataCapacity );
					allData.addComponent( true, 0, WriteMetadata( isFirstChunk, _byteBufAwareMarshal.EndOfInput, _contentType, metaDataBuffer ) );
					_progress += allData.readableBytes();
					Debug.Assert( _progress > 0 ); // logic relies on this
					return allData;
			  }
			  catch ( Exception e )
			  {
					allData.release();
					throw e;
			  }
		 }

		 public override long Length()
		 {
			  return -1;
		 }

		 public override long Progress()
		 {
			  return _progress;
		 }

		 private class Single : ChunkedInput<ByteBuf>
		 {
			  internal readonly ThrowingConsumer<WritableChannel, IOException> Marshaller;
			  internal bool IsEndOfInput;
			  internal int Offset;

			  internal Single( ThrowingConsumer<WritableChannel, IOException> marshaller )
			  {
					this.Marshaller = marshaller;
			  }

			  public override bool EndOfInput
			  {
				  get
				  {
						return IsEndOfInput;
				  }
			  }

			  public override void Close()
			  {
					IsEndOfInput = true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public io.netty.buffer.ByteBuf readChunk(io.netty.channel.ChannelHandlerContext ctx) throws Exception
			  public override ByteBuf ReadChunk( ChannelHandlerContext ctx )
			  {
					return ReadChunk( ctx.alloc() );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public io.netty.buffer.ByteBuf readChunk(io.netty.buffer.ByteBufAllocator allocator) throws Exception
			  public override ByteBuf ReadChunk( ByteBufAllocator allocator )
			  {
					if ( IsEndOfInput )
					{
						 return null;
					}
					ByteBuf buffer = allocator.buffer();
					Marshaller.accept( new BoundedNetworkWritableChannel( buffer ) );
					IsEndOfInput = true;
					Offset = buffer.readableBytes();
					return buffer;
			  }

			  public override long Length()
			  {
					return -1;
			  }

			  public override long Progress()
			  {
					return Offset;
			  }
		 }
	}

}