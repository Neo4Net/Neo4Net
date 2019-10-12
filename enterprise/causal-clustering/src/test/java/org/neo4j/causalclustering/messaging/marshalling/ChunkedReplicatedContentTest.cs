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
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using UnpooledByteBufAllocator = io.netty.buffer.UnpooledByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	public class ChunkedReplicatedContentTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideExpectedMetaData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideExpectedMetaData()
		 {
			  ChunkedInput<ByteBuf> replicatedContent = ChunkedReplicatedContent.Chunked( ( sbyte ) 1, new ThreeChunks( this, -1, 8 ) );

			  UnpooledByteBufAllocator allocator = UnpooledByteBufAllocator.DEFAULT;

			  ByteBuf byteBuf = replicatedContent.readChunk( allocator );

			  // is not last
			  assertFalse( byteBuf.readBoolean() );
			  // first chunk has content
			  assertEquals( ( sbyte ) 1, byteBuf.readByte() );
			  byteBuf.release();

			  byteBuf = replicatedContent.readChunk( allocator );
			  // is not last
			  assertFalse( byteBuf.readBoolean() );
			  byteBuf.release();

			  byteBuf = replicatedContent.readChunk( allocator );
			  // is last
			  assertTrue( byteBuf.readBoolean() );
			  byteBuf.release();

			  assertNull( replicatedContent.readChunk( allocator ) );
		 }

		 private class ThreeChunks : ChunkedInput<ByteBuf>
		 {
			 private readonly ChunkedReplicatedContentTest _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int LengthConflict;
			  internal int LeftTowWrite;
			  internal readonly int ChunkSize;
			  internal int Count;

			  internal ThreeChunks( ChunkedReplicatedContentTest outerInstance, int length, int chunkSize )
			  {
				  this._outerInstance = outerInstance;
					this.LengthConflict = length;
					this.LeftTowWrite = length == -1 ? int.MaxValue : length;
					this.ChunkSize = chunkSize;
			  }

			  public override bool EndOfInput
			  {
				  get
				  {
						return Count == 3;
				  }
			  }

			  public override void Close()
			  {

			  }

			  public override ByteBuf ReadChunk( ChannelHandlerContext ctx )
			  {
					return ReadChunk( ctx.alloc() );
			  }

			  public override ByteBuf ReadChunk( ByteBufAllocator allocator )
			  {
					if ( Count == 3 )
					{
						 return null;
					}
					ByteBuf buffer = allocator.buffer( ChunkSize, ChunkSize );
					Count++;
					int toWrite = min( LeftTowWrite, buffer.writableBytes() );
					LeftTowWrite -= toWrite;
					buffer.writerIndex( buffer.writerIndex() + toWrite );
					return buffer;
			  }

			  public override long Length()
			  {
					return LengthConflict;
			  }

			  public override long Progress()
			  {
					return 0;
			  }
		 }
	}

}