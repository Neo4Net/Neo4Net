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
namespace Neo4Net.causalclustering.messaging.marshalling
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;

	using ByteUnit = Neo4Net.Io.ByteUnit;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.Preconditions.requirePositive;

	public class MaxTotalSize : ChunkedInput<ByteBuf>
	{
		 private readonly ChunkedInput<ByteBuf> _chunkedInput;
		 private readonly int _maxSize;
		 private int _totalSize;
		 private static readonly int _defaultMaxSize = ( int ) ByteUnit.gibiBytes( 1 );

		 internal MaxTotalSize( ChunkedInput<ByteBuf> chunkedInput, int maxSize )
		 {
			  requirePositive( maxSize );
			  this._chunkedInput = chunkedInput;
			  this._maxSize = maxSize;
		 }

		 internal MaxTotalSize( ChunkedInput<ByteBuf> chunkedInput ) : this( chunkedInput, _defaultMaxSize )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean isEndOfInput() throws Exception
		 public override bool EndOfInput
		 {
			 get
			 {
				  return _chunkedInput.EndOfInput;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _chunkedInput.close();
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
			  ByteBuf byteBuf = _chunkedInput.readChunk( allocator );
			  if ( byteBuf != null )
			  {
					int additionalBytes = byteBuf.readableBytes();
					this._totalSize += additionalBytes;
					if ( this._totalSize > _maxSize )
					{
						 throw new MessageTooBigException( format( "Size limit exceeded. Limit is %d, wanted to write %d, written so far %d", _maxSize, additionalBytes, _totalSize - additionalBytes ) );
					}
			  }
			  return byteBuf;
		 }

		 public override long Length()
		 {
			  return _chunkedInput.length();
		 }

		 public override long Progress()
		 {
			  return _chunkedInput.progress();
		 }
	}

}