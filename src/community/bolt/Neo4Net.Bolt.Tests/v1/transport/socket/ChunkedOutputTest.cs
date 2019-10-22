using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.transport.socket
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ByteBufUtil = io.netty.buffer.ByteBufUtil;
	using Unpooled = io.netty.buffer.Unpooled;
	using Channel = io.netty.channel.Channel;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using PackOutputClosedException = Neo4Net.Bolt.v1.packstream.PackOutputClosedException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.transport.TransportThrottleGroup.NO_THROTTLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.transport.ChunkedOutput.CHUNK_HEADER_SIZE;

	public class ChunkedOutputTest
	{
		 private const int DEFAULT_TEST_BUFFER_SIZE = 16;

		 private readonly EmbeddedChannel _channel = new EmbeddedChannel();
		 private ChunkedOutput @out;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  @out = new ChunkedOutput( _channel, DEFAULT_TEST_BUFFER_SIZE, DEFAULT_TEST_BUFFER_SIZE, NO_THROTTLE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  @out.Dispose();
			  _channel.finishAndReleaseAll();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlushNothingWhenEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFlushNothingWhenEmpty()
		 {
			  @out.Flush();
			  assertEquals( 0, _channel.outboundMessages().size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlushNothingWhenClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFlushNothingWhenClosed()
		 {
			  @out.Dispose();
			  @out.Flush();
			  assertEquals( 0, _channel.outboundMessages().size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndFlushByte() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndFlushByte()
		 {
			  @out.BeginMessage();
			  @out.WriteByte( ( sbyte ) 42 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( ( sbyte ) 42 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndFlushShort() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndFlushShort()
		 {
			  @out.BeginMessage();
			  @out.WriteShort( ( short ) 42 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( ( short ) 42 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndFlushInt() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndFlushInt()
		 {
			  @out.BeginMessage();
			  @out.WriteInt( 424242 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( 424242 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndFlushLong() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndFlushLong()
		 {
			  @out.BeginMessage();
			  @out.WriteLong( 42424242 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( 42424242L ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndFlushDouble() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndFlushDouble()
		 {
			  @out.BeginMessage();
			  @out.WriteDouble( 42.4224 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( 42.4224 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndFlushByteBuffer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndFlushByteBuffer()
		 {
			  @out.BeginMessage();
			  @out.WriteBytes( ByteBuffer.wrap( new sbyte[]{ 9, 8, 7, 6, 5, 4, 3, 2, 1 } ) );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( ( sbyte ) 9, ( sbyte ) 8, ( sbyte ) 7, ( sbyte ) 6, ( sbyte ) 5, ( sbyte ) 4, ( sbyte ) 3, ( sbyte ) 2, ( sbyte ) 1 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndFlushByteArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndFlushByteArray()
		 {
			  @out.BeginMessage();
			  @out.WriteBytes( new sbyte[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1, 5 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( ( sbyte ) 2, ( sbyte ) 3, ( sbyte ) 4, ( sbyte ) 5, ( sbyte ) 6 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenByteArrayContainsInsufficientBytes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowWhenByteArrayContainsInsufficientBytes()
		 {
			  try
			  {
					@out.WriteBytes( new sbyte[]{ 1, 2, 3 }, 1, 5 );
					fail( "Exception expected" );
			  }
			  catch ( IOException e )
			  {
					assertEquals( "Asked to write 5 bytes, but there is only 2 bytes available in data provided.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlushOnClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFlushOnClose()
		 {
			  @out.BeginMessage();
			  @out.WriteInt( 42 ).writeInt( 4242 ).writeInt( 424242 );
			  @out.MessageSucceeded();
			  @out.Dispose();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();
			  AssertByteBufEqual( outboundMessage, ChunkContaining( 42, 4242, 424242 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseNothingWhenAlreadyClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseNothingWhenAlreadyClosed()
		 {
			  @out.BeginMessage();
			  @out.WriteLong( 42 );
			  @out.MessageSucceeded();

			  @out.Dispose();
			  @out.Dispose();
			  @out.Dispose();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();
			  AssertByteBufEqual( outboundMessage, ChunkContaining( ( long ) 42 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChunkSingleMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChunkSingleMessage()
		 {
			  @out.BeginMessage();
			  @out.WriteByte( ( sbyte ) 1 );
			  @out.WriteShort( ( short ) 2 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();
			  AssertByteBufEqual( outboundMessage, ChunkContaining( ( sbyte ) 1, ( short ) 2 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChunkMessageSpanningMultipleChunks() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChunkMessageSpanningMultipleChunks()
		 {
			  @out.BeginMessage();
			  @out.WriteLong( 1 );
			  @out.WriteLong( 2 );
			  @out.WriteLong( 3 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( ( long ) 1 ) + ChunkContaining( ( long ) 2 ) + ChunkContaining( ( long ) 3 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChunkDataWhoseSizeIsGreaterThanOutputBufferCapacity() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChunkDataWhoseSizeIsGreaterThanOutputBufferCapacity()
		 {
			  @out.BeginMessage();
			  sbyte[] bytes = new sbyte[16];
			  Arrays.fill( bytes, ( sbyte ) 42 );
			  @out.WriteBytes( bytes, 0, 16 );
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  Number[] chunk1Body = new Number[14];
			  Arrays.fill( chunk1Body, ( sbyte ) 42 );

			  Number[] chunk2Body = new Number[2];
			  Arrays.fill( chunk2Body, ( sbyte ) 42 );

			  AssertByteBufEqual( outboundMessage, ChunkContaining( chunk1Body ) + ChunkContaining( chunk2Body ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowIfOutOfSyncFlush() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotThrowIfOutOfSyncFlush()
		 {
			  @out.BeginMessage();
			  @out.WriteLong( 1 );
			  @out.WriteLong( 2 );
			  @out.WriteLong( 3 );
			  @out.MessageSucceeded();

			  @out.Flush();
			  @out.Dispose();
			  //this flush comes in to late but should not cause ChunkedOutput to choke.
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( ( long ) 1 ) + ChunkContaining( ( long ) 2 ) + ChunkContaining( ( long ) 3 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToWriteAfterClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToWriteAfterClose()
		 {
			  @out.BeginMessage();
			  @out.WriteLong( 1 );
			  @out.WriteLong( 2 );
			  @out.WriteLong( 3 );
			  @out.MessageSucceeded();

			  @out.Flush();
			  @out.Dispose();

			  try
			  {
					@out.WriteShort( ( short ) 42 );
					fail( "Should have thrown IOException" );
			  }
			  catch ( IOException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowErrorWithRemoteAddressWhenClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowErrorWithRemoteAddressWhenClosed()
		 {
			  Channel channel = mock( typeof( Channel ) );
			  ByteBufAllocator allocator = mock( typeof( ByteBufAllocator ) );
			  when( allocator.buffer( anyInt() ) ).thenReturn(Unpooled.buffer());
			  when( channel.alloc() ).thenReturn(allocator);
			  SocketAddress remoteAddress = mock( typeof( SocketAddress ) );
			  string remoteAddressString = "client.server.com:7687";
			  when( remoteAddress.ToString() ).thenReturn(remoteAddressString);
			  when( channel.remoteAddress() ).thenReturn(remoteAddress);

			  ChunkedOutput output = new ChunkedOutput( channel, DEFAULT_TEST_BUFFER_SIZE, DEFAULT_TEST_BUFFER_SIZE, NO_THROTTLE );
			  output.Dispose();

			  try
			  {
					output.WriteInt( 42 );
					fail( "Exception expected" );
			  }
			  catch ( PackOutputClosedException e )
			  {
					assertThat( e.Message, containsString( remoteAddressString ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateFailedMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateFailedMessage()
		 {
			  @out.BeginMessage();
			  @out.WriteInt( 1 );
			  @out.WriteInt( 2 );
			  @out.MessageSucceeded();

			  @out.BeginMessage();
			  @out.WriteInt( 3 );
			  @out.WriteInt( 4 );
			  @out.MessageFailed();

			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( 1, 2 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowWritingAfterFailedMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowWritingAfterFailedMessage()
		 {
			  @out.BeginMessage();
			  @out.WriteInt( 1 );
			  @out.WriteInt( 2 );
			  @out.MessageSucceeded();

			  @out.BeginMessage();
			  @out.WriteByte( ( sbyte ) 3 );
			  @out.WriteByte( ( sbyte ) 4 );
			  @out.MessageFailed();

			  @out.BeginMessage();
			  @out.WriteInt( 33 );
			  @out.WriteLong( 44 );
			  @out.MessageSucceeded();

			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( 1, 2 ) + MessageBoundary() + ChunkContaining(33, (long) 44) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteOnlyMessageBoundaryWhenWriterIsEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteOnlyMessageBoundaryWhenWriterIsEmpty()
		 {
			  @out.BeginMessage();
			  // write nothing in the message body
			  @out.MessageSucceeded();
			  @out.Flush();

			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAutoFlushOnlyWhenMaxBufferSizeReachedAfterFullMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAutoFlushOnlyWhenMaxBufferSizeReachedAfterFullMessage()
		 {
			  @out.BeginMessage();
			  @out.WriteInt( 1 );
			  @out.WriteInt( 2 );
			  @out.WriteInt( 3 );
			  @out.WriteLong( 4 );

			  // nothing should be flushed because we are still in the middle of the message
			  assertEquals( 0, PeekAllOutboundMessages().Count );

			  @out.WriteByte( ( sbyte ) 5 );
			  @out.WriteByte( ( sbyte ) 6 );
			  @out.WriteLong( 7 );
			  @out.WriteInt( 8 );
			  @out.WriteByte( ( sbyte ) 9 );

			  // still nothing should be flushed
			  assertEquals( 0, PeekAllOutboundMessages().Count );

			  @out.MessageSucceeded();

			  // now the whole buffer should be flushed, it is larger than the maxBufferSize
			  ByteBuf outboundMessage = PeekSingleOutboundMessage();

			  AssertByteBufEqual( outboundMessage, ChunkContaining( 1, 2, 3 ) + ChunkContaining( ( long ) 4, ( sbyte ) 5, ( sbyte ) 6 ) + ChunkContaining( ( long ) 7, 8, ( sbyte ) 9 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAutoFlushMultipleMessages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAutoFlushMultipleMessages()
		 {
			  @out.BeginMessage();
			  @out.WriteLong( 1 );
			  @out.WriteLong( 2 );
			  @out.MessageSucceeded();

			  @out.BeginMessage();
			  @out.WriteLong( 3 );
			  @out.WriteLong( 4 );
			  @out.MessageSucceeded();

			  @out.BeginMessage();
			  @out.WriteLong( 5 );
			  @out.WriteLong( 6 );
			  @out.MessageSucceeded();

			  IList<ByteBuf> outboundMessages = PeekAllOutboundMessages();
			  assertEquals( 3, outboundMessages.Count );

			  AssertByteBufEqual( outboundMessages[0], ChunkContaining( ( long ) 1 ) + ChunkContaining( ( long ) 2 ) + MessageBoundary() );
			  AssertByteBufEqual( outboundMessages[1], ChunkContaining( ( long ) 3 ) + ChunkContaining( ( long ) 4 ) + MessageBoundary() );
			  AssertByteBufEqual( outboundMessages[2], ChunkContaining( ( long ) 5 ) + ChunkContaining( ( long ) 6 ) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToBeginMultipleMessages()
		 public virtual void ShouldFailToBeginMultipleMessages()
		 {
			  @out.BeginMessage();

			  try
			  {
					@out.BeginMessage();
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToMarkMessageAsSuccessfulWhenMessageNotStarted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToMarkMessageAsSuccessfulWhenMessageNotStarted()
		 {
			  try
			  {
					@out.MessageSucceeded();
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToMarkMessageAsFialedWhenMessageNotStarted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToMarkMessageAsFialedWhenMessageNotStarted()
		 {
			  try
			  {
					@out.MessageFailed();
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToWriteByteOutsideOfMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToWriteByteOutsideOfMessage()
		 {
			  try
			  {
					@out.WriteByte( ( sbyte ) 1 );
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToWriteShortOutsideOfMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToWriteShortOutsideOfMessage()
		 {
			  try
			  {
					@out.WriteShort( ( short ) 1 );
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToWriteIntOutsideOfMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToWriteIntOutsideOfMessage()
		 {
			  try
			  {
					@out.WriteInt( 1 );
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToWriteLongOutsideOfMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToWriteLongOutsideOfMessage()
		 {
			  try
			  {
					@out.WriteLong( 1 );
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToWriteDoubleOutsideOfMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToWriteDoubleOutsideOfMessage()
		 {
			  try
			  {
					@out.WriteDouble( 1.1 );
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToWriteBytesOutsideOfMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToWriteBytesOutsideOfMessage()
		 {
			  try
			  {
					@out.WriteBytes( ByteBuffer.wrap( new sbyte[10] ) );
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToMarkMessageAsSuccessfulAndThenAsFailed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToMarkMessageAsSuccessfulAndThenAsFailed()
		 {
			  @out.BeginMessage();
			  @out.WriteInt( 42 );
			  @out.MessageSucceeded();

			  try
			  {
					@out.MessageFailed();
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }

			  @out.Flush();

			  AssertByteBufEqual( PeekSingleOutboundMessage(), ChunkContaining(42) + MessageBoundary() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToMarkMessageAsFailedAndThenAsSuccessful() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToMarkMessageAsFailedAndThenAsSuccessful()
		 {
			  @out.BeginMessage();
			  @out.WriteInt( 42 );
			  @out.MessageFailed();

			  try
			  {
					@out.MessageSucceeded();
					fail( "Exception expected" );
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }

			  @out.Flush();

			  assertEquals( 0, PeekAllOutboundMessages().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowMultipleFailedMessages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowMultipleFailedMessages()
		 {
			  for ( int i = 0; i < 7; i++ )
			  {
					@out.BeginMessage();
					@out.WriteByte( ( sbyte ) i );
					@out.WriteShort( ( short ) i );
					@out.WriteInt( i );
					@out.MessageFailed();
			  }

			  @out.Flush();
			  assertEquals( 0, PeekAllOutboundMessages().Count );

			  // try to write a 2-chunk message which should auto-flush
			  @out.BeginMessage();
			  @out.WriteByte( ( sbyte ) 8 );
			  @out.WriteShort( ( short ) 9 );
			  @out.WriteInt( 10 );
			  @out.WriteDouble( 199.92 );
			  @out.MessageSucceeded();

			  AssertByteBufEqual( PeekSingleOutboundMessage(), ChunkContaining((sbyte) 8, (short) 9, 10) + ChunkContaining(199.92) + MessageBoundary() );
		 }

		 private ByteBuf PeekSingleOutboundMessage()
		 {
			  IList<ByteBuf> outboundMessages = PeekAllOutboundMessages();
			  assertEquals( 1, outboundMessages.Count );
			  return outboundMessages[0];
		 }

		 private IList<ByteBuf> PeekAllOutboundMessages()
		 {
			  return _channel.outboundMessages().Select(msg => (ByteBuf) msg).ToList();
		 }

		 private static void AssertByteBufEqual( ByteBuf buf, string hexContent )
		 {
			  assertEquals( ByteBufUtil.hexDump( buf ), hexContent );
		 }

		 private static string ChunkContaining( params Number[] values )
		 {
			  short chunkSize = 0;
			  foreach ( Number value in values )
			  {
					if ( value is sbyte? )
					{
						 chunkSize += Byte.BYTES;
					}
					else if ( value is short? )
					{
						 chunkSize += Short.BYTES;
					}
					else if ( value is int? )
					{
						 chunkSize += Integer.BYTES;
					}
					else if ( value is long? )
					{
						 chunkSize += Long.BYTES;
					}
					else if ( value is double? )
					{
						 chunkSize += Double.BYTES;
					}
					else
					{
						 throw new System.ArgumentException( "Unsupported number " + value.GetType() + ' ' + value );
					}
			  }

			  ByteBuffer buffer = ByteBuffer.allocate( chunkSize + CHUNK_HEADER_SIZE );
			  buffer.putShort( chunkSize );

			  foreach ( Number value in values )
			  {
					if ( value is sbyte? )
					{
						 buffer.put( value.byteValue() );
					}
					else if ( value is short? )
					{
						 buffer.putShort( value.shortValue() );
					}
					else if ( value is int? )
					{
						 buffer.putInt( value.intValue() );
					}
					else if ( value is long? )
					{
						 buffer.putLong( value.longValue() );
					}
					else if ( value is double? )
					{
						 buffer.putDouble( value.doubleValue() );
					}
					else
					{
						 throw new System.ArgumentException( "Unsupported number " + value.GetType() + ' ' + value );
					}
			  }
			  buffer.flip();

			  return ByteBufUtil.hexDump( buffer.array() );
		 }

		 private static string MessageBoundary()
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( Short.BYTES );
			  buffer.putShort( ( short ) 0 );
			  buffer.flip();
			  return ByteBufUtil.hexDump( buffer.array() );
		 }
	}

}