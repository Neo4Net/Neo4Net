/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Bolt.transport.pipeline
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using After = org.junit.After;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.Unpooled.buffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.Unpooled.copyShort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.Unpooled.wrappedBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltTestUtil.assertByteBufEquals;

	public class ChunkDecoderTest
	{
		 private readonly EmbeddedChannel _channel = new EmbeddedChannel( new ChunkDecoder() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _channel.finishAndReleaseAll();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeFullChunk()
		 public virtual void ShouldDecodeFullChunk()
		 {
			  // whole chunk with header and body arrives at once
			  ByteBuf input = buffer();
			  input.writeShort( 7 );
			  input.writeByte( 1 );
			  input.writeByte( 11 );
			  input.writeByte( 2 );
			  input.writeByte( 22 );
			  input.writeByte( 3 );
			  input.writeByte( 33 );
			  input.writeByte( 4 );

			  // after buffer is written there should be something to read on the other side
			  assertTrue( _channel.writeInbound( input ) );
			  assertTrue( _channel.finish() );

			  // there should only be a single chunk available for reading
			  assertEquals( 1, _channel.inboundMessages().size() );
			  // it should have no size header and expected body
			  assertByteBufEquals( input.slice( 2, 7 ), _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeSplitChunk()
		 public virtual void ShouldDecodeSplitChunk()
		 {
			  // first part of the chunk contains size header and some bytes
			  ByteBuf input1 = buffer();
			  input1.writeShort( 9 );
			  input1.writeByte( 1 );
			  input1.writeByte( 11 );
			  input1.writeByte( 2 );
			  // nothing should be available for reading
			  assertFalse( _channel.writeInbound( input1 ) );

			  // second part contains just a single byte
			  ByteBuf input2 = buffer();
			  input2.writeByte( 22 );
			  // nothing should be available for reading
			  assertFalse( _channel.writeInbound( input2 ) );

			  // third part contains couple more bytes
			  ByteBuf input3 = buffer();
			  input3.writeByte( 3 );
			  input3.writeByte( 33 );
			  input3.writeByte( 4 );
			  // nothing should be available for reading
			  assertFalse( _channel.writeInbound( input3 ) );

			  // fourth part contains couple more bytes, and the chunk is now complete
			  ByteBuf input4 = buffer();
			  input4.writeByte( 44 );
			  input4.writeByte( 5 );
			  // there should be something to read now
			  assertTrue( _channel.writeInbound( input4 ) );

			  assertTrue( _channel.finish() );

			  // there should only be a single chunk available for reading
			  assertEquals( 1, _channel.inboundMessages().size() );
			  // it should have no size header and expected body
			  assertByteBufEquals( wrappedBuffer( new sbyte[]{ 1, 11, 2, 22, 3, 33, 4, 44, 5 } ), _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeEmptyChunk()
		 public virtual void ShouldDecodeEmptyChunk()
		 {
			  // chunk contains just the size header which is zero
			  ByteBuf input = copyShort( 0 );
			  assertTrue( _channel.writeInbound( input ) );
			  assertTrue( _channel.finish() );

			  // there should only be a single chunk available for reading
			  assertEquals( 1, _channel.inboundMessages().size() );
			  // it should have no size header and empty body
			  assertByteBufEquals( wrappedBuffer( new sbyte[0] ), _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeMaxSizeChunk()
		 public virtual void ShouldDecodeMaxSizeChunk()
		 {
			  sbyte[] message = new sbyte[0xFFFF];

			  ByteBuf input = buffer();
			  input.writeShort( message.Length );
			  input.writeBytes( message );

			  assertTrue( _channel.writeInbound( input ) );
			  assertTrue( _channel.finish() );

			  assertEquals( 1, _channel.inboundMessages().size() );
			  assertByteBufEquals( wrappedBuffer( message ), _channel.readInbound() );
		 }
	}

}