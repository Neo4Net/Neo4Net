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
namespace Neo4Net.com
{
	using ByteBufferBackedChannelBuffer = org.jboss.netty.buffer.ByteBufferBackedChannelBuffer;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelHandlerContext = org.jboss.netty.channel.ChannelHandlerContext;
	using MessageEvent = org.jboss.netty.channel.MessageEvent;
	using Test = org.junit.Test;

	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.DEFAULT_FRAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.EMPTY_SERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.VOID_DESERIALIZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

	public class ServerTest
	{
		 private readonly Protocol _protocol = new Protocol214( 1024, ( sbyte ) 0, Server.INTERNAL_PROTOCOL_VERSION );
		 private readonly TxChecksumVerifier _checksumVerifier = mock( typeof( TxChecksumVerifier ) );
		 private readonly RequestType _reqType = mock( typeof( RequestType ) );
		 private readonly RecordingChannel _channel = new RecordingChannel();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendExceptionBackToClientOnInvalidChecksum() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendExceptionBackToClientOnInvalidChecksum()
		 {
			  // Given
			  Server<object, object> server = NewServer( _checksumVerifier );
			  RequestContext ctx = new RequestContext( 0, 1, 0, 2, 12 );

			  doThrow( new System.InvalidOperationException( "123" ) ).when( _checksumVerifier ).assertMatch( anyLong(), anyLong() );

			  // When
			  try
			  {
					server.MessageReceived( ChannelCtx( _channel ), Message( _reqType, ctx, _channel, EMPTY_SERIALIZER ) );
					fail( "Should have failed." );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// Expected
			  }

			  // Then
			  try
			  {
					_protocol.deserializeResponse( _channel.asBlockingReadHandler(), ByteBuffer.allocate((int) ByteUnit.kibiBytes(1)), 1, VOID_DESERIALIZER, mock(typeof(ResourceReleaser)), new VersionAwareLogEntryReader<Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>() );
					fail( "Should have failed." );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					assertThat( e.Message, equalTo( "123" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendExceptionBackToClientOnInvalidChecksumIfThereAreNoTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSendExceptionBackToClientOnInvalidChecksumIfThereAreNoTransactions()
		 {
			  // Given
			  Server<object, object> server = NewServer( _checksumVerifier );
			  RequestContext ctx = new RequestContext( 0, 1, 0, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_CHECKSUM );

			  // When
			  server.MessageReceived( ChannelCtx( _channel ), Message( _reqType, ctx, _channel, EMPTY_SERIALIZER ) );

			  // Then
			  verifyZeroInteractions( _checksumVerifier );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.jboss.netty.channel.MessageEvent message(RequestType reqType, RequestContext ctx, org.jboss.netty.channel.Channel serverToClientChannel, Serializer payloadSerializer) throws java.io.IOException
		 private MessageEvent Message( RequestType reqType, RequestContext ctx, Channel serverToClientChannel, Serializer payloadSerializer )
		 {
			  ByteBuffer backingBuffer = ByteBuffer.allocate( 1024 );

			  _protocol.serializeRequest( new RecordingChannel(), new ByteBufferBackedChannelBuffer(backingBuffer), reqType, ctx, payloadSerializer );

			  MessageEvent @event = mock( typeof( MessageEvent ) );
			  when( @event.Message ).thenReturn( new ByteBufferBackedChannelBuffer( backingBuffer ) );
			  when( @event.Channel ).thenReturn( serverToClientChannel );

			  return @event;
		 }

		 private ChannelHandlerContext ChannelCtx( Channel channel )
		 {
			  ChannelHandlerContext ctx = mock( typeof( ChannelHandlerContext ) );
			  when( ctx.Channel ).thenReturn( channel );
			  return ctx;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Server<Object,Object> newServer(final TxChecksumVerifier checksumVerifier)
		 private Server<object, object> NewServer( TxChecksumVerifier checksumVerifier )
		 {
			  Server.Configuration conf = mock( typeof( Server.Configuration ) );
			  when( conf.ServerAddress ).thenReturn( new HostnamePort( "aa", -1667 ) );
			  Server<object, object> server = new ServerAnonymousInnerClass( this, conf, Instance, DEFAULT_FRAME_LENGTH, checksumVerifier, Clocks.systemClock(), mock(typeof(ByteCounterMonitor)), mock(typeof(RequestMonitor)) );
			  server.Init();
			  return server;
		 }

		 private class ServerAnonymousInnerClass : Server<object, object>
		 {
			 private readonly ServerTest _outerInstance;

			 public ServerAnonymousInnerClass( ServerTest outerInstance, Neo4Net.com.Server.Configuration conf, UnknownType getInstance, UnknownType defaultFrameLength, Neo4Net.com.TxChecksumVerifier checksumVerifier, java.time.Clock systemClock, UnknownType mock, UnknownType mock ) : base( null, conf, getInstance, defaultFrameLength, new ProtocolVersion( ( sbyte ) 0, ProtocolVersion.INTERNAL_PROTOCOL_VERSION ), checksumVerifier, systemClock, mock, mock )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override RequestType getRequestContext( sbyte id )
			 {
				  return mock( typeof( RequestType ) );
			 }

			 protected internal override void stopConversation( RequestContext context )
			 {
			 }
		 }
	}

}