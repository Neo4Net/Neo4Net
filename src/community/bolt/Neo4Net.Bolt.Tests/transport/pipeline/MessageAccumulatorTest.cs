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
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using After = org.junit.After;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.Unpooled.wrappedBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltTestUtil.assertByteBufEquals;

	public class MessageAccumulatorTest
	{
		 private readonly EmbeddedChannel _channel = new EmbeddedChannel( new MessageAccumulator() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _channel.finishAndReleaseAll();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeMessageWithSingleChunk()
		 public virtual void ShouldDecodeMessageWithSingleChunk()
		 {
			  assertFalse( _channel.writeInbound( wrappedBuffer( new sbyte[]{ 1, 2, 3, 4, 5 } ) ) );
			  assertTrue( _channel.writeInbound( wrappedBuffer( new sbyte[0] ) ) );
			  assertTrue( _channel.finish() );

			  assertEquals( 1, _channel.inboundMessages().size() );
			  assertByteBufEquals( wrappedBuffer( new sbyte[]{ 1, 2, 3, 4, 5 } ), _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeMessageWithMultipleChunks()
		 public virtual void ShouldDecodeMessageWithMultipleChunks()
		 {
			  assertFalse( _channel.writeInbound( wrappedBuffer( new sbyte[]{ 1, 2, 3 } ) ) );
			  assertFalse( _channel.writeInbound( wrappedBuffer( new sbyte[]{ 4, 5 } ) ) );
			  assertFalse( _channel.writeInbound( wrappedBuffer( new sbyte[]{ 6, 7, 8 } ) ) );
			  assertTrue( _channel.writeInbound( wrappedBuffer( new sbyte[0] ) ) );
			  assertTrue( _channel.finish() );

			  assertEquals( 1, _channel.inboundMessages().size() );
			  assertByteBufEquals( wrappedBuffer( new sbyte[]{ 1, 2, 3, 4, 5, 6, 7, 8 } ), _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeMultipleConsecutiveMessages()
		 public virtual void ShouldDecodeMultipleConsecutiveMessages()
		 {
			  _channel.writeInbound( wrappedBuffer( new sbyte[]{ 1, 2, 3 } ) );
			  _channel.writeInbound( wrappedBuffer( new sbyte[0] ) );

			  _channel.writeInbound( wrappedBuffer( new sbyte[]{ 4, 5 } ) );
			  _channel.writeInbound( wrappedBuffer( new sbyte[]{ 6 } ) );
			  _channel.writeInbound( wrappedBuffer( new sbyte[0] ) );

			  _channel.writeInbound( wrappedBuffer( new sbyte[]{ 7, 8 } ) );
			  _channel.writeInbound( wrappedBuffer( new sbyte[]{ 9, 10 } ) );
			  _channel.writeInbound( wrappedBuffer( new sbyte[0] ) );

			  assertEquals( 3, _channel.inboundMessages().size() );
			  assertByteBufEquals( wrappedBuffer( new sbyte[]{ 1, 2, 3 } ), _channel.readInbound() );
			  assertByteBufEquals( wrappedBuffer( new sbyte[]{ 4, 5, 6 } ), _channel.readInbound() );
			  assertByteBufEquals( wrappedBuffer( new sbyte[]{ 7, 8, 9, 10 } ), _channel.readInbound() );
		 }

	}

}