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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Buffers = Org.Neo4j.causalclustering.helpers.Buffers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class ByteArrayChunkedEncoderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.causalclustering.helpers.Buffers buffers = new org.neo4j.causalclustering.helpers.Buffers();
		 public readonly Buffers Buffers = new Buffers();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteToBufferInChunks()
		 public virtual void ShouldWriteToBufferInChunks()
		 {
			  int chunkSize = 5;
			  sbyte[] data = new sbyte[]{ 1, 2, 3, 4, 5, 6 };
			  sbyte[] readData = new sbyte[6];
			  ByteArrayChunkedEncoder byteArraySerializer = new ByteArrayChunkedEncoder( data, chunkSize );

			  ByteBuf buffer = byteArraySerializer.ReadChunk( Buffers );
			  buffer.readBytes( readData, 0, chunkSize );
			  assertEquals( 0, buffer.readableBytes() );

			  buffer = byteArraySerializer.ReadChunk( Buffers );
			  buffer.readBytes( readData, chunkSize, 1 );
			  assertArrayEquals( data, readData );
			  assertEquals( 0, buffer.readableBytes() );

			  assertNull( byteArraySerializer.ReadChunk( Buffers ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldThrowOnTooSmallChunk()
		 public virtual void ShouldThrowOnTooSmallChunk()
		 {
			  new ByteArrayChunkedEncoder( new sbyte[1], 0 );
		 }
	}

}