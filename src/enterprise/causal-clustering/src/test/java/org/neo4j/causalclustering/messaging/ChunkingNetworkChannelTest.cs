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
namespace Neo4Net.causalclustering.messaging
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Buffers = Neo4Net.causalclustering.helpers.Buffers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;

	public class ChunkingNetworkChannelTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.causalclustering.helpers.Buffers buffers = new Neo4Net.causalclustering.helpers.Buffers();
		 public readonly Buffers Buffers = new Buffers();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeIntoChunksOfGivenSize()
		 public virtual void ShouldSerializeIntoChunksOfGivenSize()
		 {
			  // given
			  int chunkSize = 8;
			  LinkedList<ByteBuf> byteBufs = new LinkedList<ByteBuf>();
			  ChunkingNetworkChannel channel = new ChunkingNetworkChannel( Buffers, chunkSize, byteBufs );

			  // and data is written
			  sbyte[] array = new sbyte[10];
			  channel.Put( ( sbyte ) 1 );
			  channel.PutInt( 1 );
			  channel.PutFloat( 1.0f );
			  channel.PutDouble( 1.0d );
			  channel.PutShort( ( short ) 1 );
			  channel.PutLong( 1 );
			  channel.Put( array, array.Length );
			  channel.Flush();

			  // when
			  ByteBuf combinedByteBuf = Buffers.buffer();
			  ByteBuf byteBuf;
			  while ( ( byteBuf = byteBufs.RemoveFirst() ) != null )
			  {
					assertEquals( chunkSize, byteBuf.capacity() );
					combinedByteBuf.writeBytes( byteBuf );
			  }

			  //then
			  assertEquals( ( sbyte ) 1, combinedByteBuf.readByte() );
			  assertEquals( 1, combinedByteBuf.readInt() );
			  assertEquals( 1.0f, combinedByteBuf.readFloat() );
			  assertEquals( 1.0d, combinedByteBuf.readDouble() );
			  assertEquals( ( short ) 1, combinedByteBuf.readShort() );
			  assertEquals( 1L, combinedByteBuf.readLong() );
			  sbyte[] bytes = new sbyte[array.Length];
			  combinedByteBuf.readBytes( bytes );
			  assertArrayEquals( array, bytes );
			  assertEquals( 0, combinedByteBuf.readableBytes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullIfQueueIsEmpty()
		 public virtual void ShouldReturnNullIfQueueIsEmpty()
		 {
			  // given
			  int chunkSize = 8;
			  LinkedList<ByteBuf> byteBufs = new LinkedList<ByteBuf>();

			  ChunkingNetworkChannel channel = new ChunkingNetworkChannel( Buffers, chunkSize, byteBufs );

			  // when
			  channel.PutLong( 1L );
			  channel.PutLong( 1L );

			  // then
			  assertNotNull( byteBufs.RemoveFirst() );
			  assertNull( byteBufs.RemoveFirst() );

			  // when
			  channel.PutLong( 2L );

			  // then
			  assertNotNull( byteBufs.RemoveFirst() );
			  assertNull( byteBufs.RemoveFirst() );

			  // when
			  channel.Flush();

			  // then
			  assertNotNull( byteBufs.RemoveFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIllegalStatAfterClosed()
		 public virtual void ShouldThrowIllegalStatAfterClosed()
		 {
			  int chunkSize = 8;
			  ChunkingNetworkChannel channel = new ChunkingNetworkChannel( Buffers, chunkSize, new LinkedList<ByteBuf>() );
			  channel.Close();
			  channel.PutInt( 1 );
		 }
	}

}