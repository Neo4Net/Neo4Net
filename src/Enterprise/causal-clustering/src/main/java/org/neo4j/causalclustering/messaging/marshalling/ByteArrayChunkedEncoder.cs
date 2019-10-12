using System;

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
namespace Neo4Net.causalclustering.messaging.marshalling
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requireNonNegative;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requirePositive;

	public class ByteArrayChunkedEncoder : ChunkedInput<ByteBuf>
	{
		 private const int DEFAULT_CHUNK_SIZE = 32 * 1024;
		 private readonly sbyte[] _content;
		 private int _chunkSize;
		 private int _pos;
		 private bool _hasRead;

		 internal ByteArrayChunkedEncoder( sbyte[] content, int chunkSize )
		 {
			  requireNonNull( content, "content cannot be null" );
			  requireNonNegative( content.Length );
			  requirePositive( chunkSize );
			  this._content = content;
			  this._chunkSize = chunkSize;
		 }

		 public ByteArrayChunkedEncoder( sbyte[] content ) : this( content, DEFAULT_CHUNK_SIZE )
		 {
		 }

		 private int Available()
		 {
			  return _content.Length - _pos;
		 }

		 public override bool EndOfInput
		 {
			 get
			 {
				  return _pos == _content.Length && _hasRead;
			 }
		 }

		 public override void Close()
		 {
			  _pos = _content.Length;
		 }

		 public override ByteBuf ReadChunk( ChannelHandlerContext ctx )
		 {
			  return ReadChunk( ctx.alloc() );
		 }

		 public override ByteBuf ReadChunk( ByteBufAllocator allocator )
		 {
			  _hasRead = true;
			  if ( EndOfInput )
			  {
					return null;
			  }
			  int toWrite = Math.Min( Available(), _chunkSize );
			  ByteBuf buffer = allocator.buffer( toWrite );
			  try
			  {
					buffer.writeBytes( _content, _pos, toWrite );
					_pos += toWrite;
					return buffer;
			  }
			  catch ( Exception t )
			  {
					buffer.release();
					throw t;
			  }
		 }

		 public override long Length()
		 {
			  return _content.Length;
		 }

		 public override long Progress()
		 {
			  return _pos;
		 }
	}

}