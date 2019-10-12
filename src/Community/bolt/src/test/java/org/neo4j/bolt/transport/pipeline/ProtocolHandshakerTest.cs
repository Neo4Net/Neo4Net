/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
	using Unpooled = io.netty.buffer.Unpooled;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using DefaultFullHttpRequest = io.netty.handler.codec.http.DefaultFullHttpRequest;
	using FullHttpRequest = io.netty.handler.codec.http.FullHttpRequest;
	using HttpHeaderNames = io.netty.handler.codec.http.HttpHeaderNames;
	using HttpMethod = io.netty.handler.codec.http.HttpMethod;
	using HttpVersion = io.netty.handler.codec.http.HttpVersion;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltTestUtil.assertByteBufEquals;

	public class ProtocolHandshakerTest
	{
		 private readonly BoltChannel _boltChannel = NewBoltChannel();
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public ExpectedException Thrown = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _boltChannel.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChooseFirstAvailableProtocol()
		 public virtual void ShouldChooseFirstAvailableProtocol()
		 {
			  // Given
			  BoltProtocol protocol = NewBoltProtocol( 1 );
			  BoltProtocolFactory handlerFactory = NewProtocolFactory( 1, protocol );
			  EmbeddedChannel channel = new EmbeddedChannel( new ProtocolHandshaker( handlerFactory, _boltChannel, _logProvider, false, true ) );

			  // When
			  ByteBuf input = Unpooled.wrappedBuffer( new sbyte[]{ ( sbyte ) 0x60, ( sbyte ) 0x60, unchecked( ( sbyte ) 0xB0 ), ( sbyte ) 0x17 }, new sbyte[]{ 0, 0, 0, 0 }, new sbyte[]{ 0, 0, 0, 1 }, new sbyte[]{ 0, 0, 0, 0 }, new sbyte[]{ 0, 0, 0, 0 } ); // fourth choice - no protocol
			  channel.writeInbound( input );

			  // Then
			  assertEquals( 1, channel.outboundMessages().size() );
			  assertByteBufEquals( Unpooled.buffer().writeInt(1), channel.readOutbound() );

			  Thrown.expect( typeof( NoSuchElementException ) );
			  channel.pipeline().remove(typeof(ProtocolHandshaker));

			  assertTrue( channel.Active );
			  verify( protocol ).install();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFragmentedMessage()
		 public virtual void ShouldHandleFragmentedMessage()
		 {
			  // Given
			  BoltProtocol protocol = NewBoltProtocol( 1 );
			  BoltProtocolFactory handlerFactory = NewProtocolFactory( 1, protocol );
			  EmbeddedChannel channel = new EmbeddedChannel( new ProtocolHandshaker( handlerFactory, _boltChannel, _logProvider, false, true ) );

			  // When
			  channel.writeInbound( Unpooled.wrappedBuffer( new sbyte[]{ ( sbyte ) 0x60, ( sbyte ) 0x60, unchecked( ( sbyte ) 0xB0 ) } ) );
			  assertEquals( 0, channel.outboundMessages().size() );
			  channel.writeInbound( Unpooled.wrappedBuffer( new sbyte[]{ ( sbyte ) 0x17, 0, 0, 0 } ) );
			  assertEquals( 0, channel.outboundMessages().size() );
			  channel.writeInbound( Unpooled.wrappedBuffer( new sbyte[]{ 0, 0, 0 } ) );
			  assertEquals( 0, channel.outboundMessages().size() );
			  channel.writeInbound( Unpooled.wrappedBuffer( new sbyte[]{ 0, 1, 0, 0, 0 } ) );
			  assertEquals( 0, channel.outboundMessages().size() );
			  channel.writeInbound( Unpooled.wrappedBuffer( new sbyte[]{ 0, 0, 0 } ) );
			  assertEquals( 0, channel.outboundMessages().size() );
			  channel.writeInbound( Unpooled.wrappedBuffer( new sbyte[]{ 0, 0 } ) );

			  // Then
			  assertEquals( 1, channel.outboundMessages().size() );
			  assertByteBufEquals( Unpooled.buffer().writeInt(1), channel.readOutbound() );

			  Thrown.expect( typeof( NoSuchElementException ) );
			  channel.pipeline().remove(typeof(ProtocolHandshaker));

			  assertTrue( channel.Active );
			  verify( protocol ).install();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleHandshakeFollowedImmediatelyByMessage()
		 public virtual void ShouldHandleHandshakeFollowedImmediatelyByMessage()
		 {
			  // Given
			  BoltProtocol protocol = NewBoltProtocol( 1 );
			  BoltProtocolFactory handlerFactory = NewProtocolFactory( 1, protocol );
			  EmbeddedChannel channel = new EmbeddedChannel( new ProtocolHandshaker( handlerFactory, _boltChannel, _logProvider, false, true ) );

			  // When
			  ByteBuf input = Unpooled.wrappedBuffer( new sbyte[]{ ( sbyte ) 0x60, ( sbyte ) 0x60, unchecked( ( sbyte ) 0xB0 ), ( sbyte ) 0x17 }, new sbyte[]{ 0, 0, 0, 0 }, new sbyte[]{ 0, 0, 0, 1 }, new sbyte[]{ 0, 0, 0, 0 }, new sbyte[]{ 0, 0, 0, 0 }, new sbyte[]{ 1, 2, 3, 4 } ); // this is a message
			  channel.writeInbound( input );

			  // Then
			  assertEquals( 1, channel.outboundMessages().size() );
			  assertByteBufEquals( Unpooled.buffer().writeInt(1), channel.readOutbound() );

			  assertEquals( 1, channel.inboundMessages().size() );
			  assertByteBufEquals( Unpooled.wrappedBuffer( new sbyte[]{ 1, 2, 3, 4 } ), channel.readInbound() );

			  Thrown.expect( typeof( NoSuchElementException ) );
			  channel.pipeline().remove(typeof(ProtocolHandshaker));

			  assertTrue( channel.Active );
			  verify( protocol ).install();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMaxVersionNumber()
		 public virtual void ShouldHandleMaxVersionNumber()
		 {
			  long maxVersionNumber = 4_294_967_295L;

			  // Given
			  BoltProtocol protocol = NewBoltProtocol( maxVersionNumber );
			  BoltProtocolFactory handlerFactory = NewProtocolFactory( maxVersionNumber, protocol );
			  EmbeddedChannel channel = new EmbeddedChannel( new ProtocolHandshaker( handlerFactory, _boltChannel, _logProvider, false, true ) );

			  // When
			  ByteBuf input = Unpooled.wrappedBuffer( new sbyte[]{ ( sbyte ) 0x60, ( sbyte ) 0x60, unchecked( ( sbyte ) 0xB0 ), ( sbyte ) 0x17 }, new sbyte[]{ unchecked( ( sbyte ) 0xFF ), unchecked( ( sbyte ) 0xFF ), unchecked( ( sbyte ) 0xFF ), unchecked( ( sbyte ) 0xFF ) }, new sbyte[]{ 0, 0, 0, 0 }, new sbyte[]{ 0, 0, 0, 0 }, new sbyte[]{ 0, 0, 0, 0 } ); // fourth choice - no protocol
			  channel.writeInbound( input );

			  // Then
			  assertEquals( 1, channel.outboundMessages().size() );
			  assertByteBufEquals( Unpooled.buffer().writeInt((int) maxVersionNumber), channel.readOutbound() );

			  Thrown.expect( typeof( NoSuchElementException ) );
			  channel.pipeline().remove(typeof(ProtocolHandshaker));

			  assertTrue( channel.Active );
			  verify( protocol ).install();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFallbackToNoProtocolIfNoMatch()
		 public virtual void ShouldFallbackToNoProtocolIfNoMatch()
		 {
			  // Given
			  BoltProtocol protocol = NewBoltProtocol( 1 );
			  BoltProtocolFactory handlerFactory = NewProtocolFactory( 1, protocol );
			  EmbeddedChannel channel = new EmbeddedChannel( new ProtocolHandshaker( handlerFactory, _boltChannel, _logProvider, false, true ) );

			  // When
			  ByteBuf input = Unpooled.wrappedBuffer( new sbyte[]{ ( sbyte ) 0x60, ( sbyte ) 0x60, unchecked( ( sbyte ) 0xB0 ), ( sbyte ) 0x17 }, new sbyte[]{ 0, 0, 0, 0 }, new sbyte[]{ 0, 0, 0, 2 }, new sbyte[]{ 0, 0, 0, 3 }, new sbyte[]{ 0, 0, 0, 4 } ); // fourth choice - no protocol
			  channel.writeInbound( input );

			  // Then
			  assertEquals( 1, channel.outboundMessages().size() );
			  assertByteBufEquals( Unpooled.buffer().writeInt(0), channel.readOutbound() );

			  assertFalse( channel.Active );
			  verify( protocol, never() ).install();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectIfWrongPreamble()
		 public virtual void ShouldRejectIfWrongPreamble()
		 {
			  // Given
			  BoltProtocol protocol = NewBoltProtocol( 1 );
			  BoltProtocolFactory handlerFactory = NewProtocolFactory( 1, protocol );
			  EmbeddedChannel channel = new EmbeddedChannel( new ProtocolHandshaker( handlerFactory, _boltChannel, _logProvider, false, true ) );

			  // When
			  ByteBuf input = Unpooled.wrappedBuffer( new sbyte[]{ unchecked( ( sbyte ) 0xDE ), unchecked( ( sbyte ) 0xAB ), unchecked( ( sbyte ) 0xCD ), unchecked( ( sbyte ) 0xEF ) }, new sbyte[]{ 0, 0, 0, 1 }, new sbyte[]{ 0, 0, 0, 2 }, new sbyte[]{ 0, 0, 0, 3 }, new sbyte[]{ 0, 0, 0, 4 } ); // fourth choice - no protocol
			  channel.writeInbound( input );

			  // Then
			  assertEquals( 0, channel.outboundMessages().size() );
			  assertFalse( channel.Active );
			  verify( protocol, never() ).install();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectIfInsecureWhenEncryptionRequired()
		 public virtual void ShouldRejectIfInsecureWhenEncryptionRequired()
		 {
			  // Given
			  BoltProtocol protocol = NewBoltProtocol( 1 );
			  BoltProtocolFactory handlerFactory = NewProtocolFactory( 1, protocol );
			  EmbeddedChannel channel = new EmbeddedChannel( new ProtocolHandshaker( handlerFactory, _boltChannel, _logProvider, true, false ) );

			  // When
			  ByteBuf input = Unpooled.wrappedBuffer( new sbyte[]{ ( sbyte ) 0x60, ( sbyte ) 0x60, unchecked( ( sbyte ) 0xB0 ), ( sbyte ) 0x17 }, new sbyte[]{ 0, 0, 0, 1 }, new sbyte[]{ 0, 0, 0, 2 }, new sbyte[]{ 0, 0, 0, 3 }, new sbyte[]{ 0, 0, 0, 4 } ); // fourth choice - no protocol
			  channel.writeInbound( input );

			  // Then
			  assertEquals( 0, channel.outboundMessages().size() );
			  assertFalse( channel.Active );
			  verify( protocol, never() ).install();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectIfHttp()
		 public virtual void ShouldRejectIfHttp()
		 {
			  // Given
			  BoltProtocol protocol = NewBoltProtocol( 1 );
			  BoltProtocolFactory handlerFactory = NewProtocolFactory( 1, protocol );
			  EmbeddedChannel channel = new EmbeddedChannel( new ProtocolHandshaker( handlerFactory, _boltChannel, _logProvider, false, true ) );

			  // When
			  FullHttpRequest request = new DefaultFullHttpRequest( HttpVersion.HTTP_1_1, HttpMethod.POST, "http://hello_world:10000" );
			  request.headers().setInt(HttpHeaderNames.CONTENT_LENGTH, 0);
			  channel.writeInbound( request );

			  // Then
			  assertEquals( 0, channel.outboundMessages().size() );
			  assertFalse( channel.Active );
			  verify( protocol, never() ).install();
			  _logProvider.assertExactly( AssertableLogProvider.inLog( typeof( ProtocolHandshaker ) ).warn( "Unsupported connection type: 'HTTP'. Bolt protocol only operates over a TCP connection or WebSocket." ) );
		 }

		 private static BoltChannel NewBoltChannel()
		 {
			  return new BoltChannel( "bolt-1", "bolt", new EmbeddedChannel() );
		 }

		 private static BoltProtocol NewBoltProtocol( long version )
		 {
			  BoltProtocol handler = mock( typeof( BoltProtocol ) );

			  when( handler.Version() ).thenReturn(version);

			  return handler;
		 }

		 private static BoltProtocolFactory NewProtocolFactory( long version, BoltProtocol protocol )
		 {
			  return ( givenVersion, channel ) => version == givenVersion ? protocol : null;
		 }
	}

}