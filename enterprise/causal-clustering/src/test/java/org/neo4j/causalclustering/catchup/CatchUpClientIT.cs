using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.catchup
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


	using GetStoreIdRequest = Org.Neo4j.causalclustering.catchup.storecopy.GetStoreIdRequest;
	using Server = Org.Neo4j.causalclustering.net.Server;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LifecycleException = Org.Neo4j.Kernel.Lifecycle.LifecycleException;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class CatchUpClientIT
	{

		 private LifeSupport _lifeSupport;
		 private int _inactivityTimeoutMillis = 10000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void initLifeCycles()
		 internal virtual void InitLifeCycles()
		 {
			  _lifeSupport = new LifeSupport();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void shutdownLifeSupport()
		 internal virtual void ShutdownLifeSupport()
		 {
			  _lifeSupport.stop();
			  _lifeSupport.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseHandlerIfChannelIsClosedInClient() throws org.neo4j.kernel.lifecycle.LifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseHandlerIfChannelIsClosedInClient()
		 {
			  // given
			  string hostname = "localhost";
			  int port = PortAuthority.allocatePort();
			  ListenSocketAddress listenSocketAddress = new ListenSocketAddress( hostname, port );
			  AtomicBoolean wasClosedByClient = new AtomicBoolean( false );

			  Server emptyServer = CatchupServer( listenSocketAddress );
			  CatchUpClient closingClient = ClosingChannelCatchupClient( wasClosedByClient );

			  _lifeSupport.add( emptyServer );
			  _lifeSupport.add( closingClient );

			  // when
			  _lifeSupport.init();
			  _lifeSupport.start();

			  // then
			  AssertClosedChannelException( hostname, port, closingClient );
			  assertTrue( wasClosedByClient.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseHandlerIfChannelIsClosedOnServer()
		 internal virtual void ShouldCloseHandlerIfChannelIsClosedOnServer()
		 {
			  // given
			  string hostname = "localhost";
			  int port = PortAuthority.allocatePort();
			  ListenSocketAddress listenSocketAddress = new ListenSocketAddress( hostname, port );
			  AtomicBoolean wasClosedByServer = new AtomicBoolean( false );

			  Server closingChannelServer = ClosingChannelCatchupServer( listenSocketAddress, wasClosedByServer );
			  CatchUpClient emptyClient = emptyClient();

			  _lifeSupport.add( closingChannelServer );
			  _lifeSupport.add( emptyClient );

			  // when
			  _lifeSupport.init();
			  _lifeSupport.start();

			  // then
			  CatchUpClientException catchUpClientException = assertThrows( typeof( CatchUpClientException ), () => emptyClient.MakeBlockingRequest(new AdvertisedSocketAddress(hostname, port), new GetStoreIdRequest(), NeverCompletingAdaptor()) );
			  assertEquals( typeof( ClosedChannelException ), Exceptions.rootCause( catchUpClientException ).GetType() );
			  assertTrue( wasClosedByServer.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTimeoutDueToInactivity()
		 internal virtual void ShouldTimeoutDueToInactivity()
		 {
			  // given
			  string hostname = "localhost";
			  int port = PortAuthority.allocatePort();
			  ListenSocketAddress listenSocketAddress = new ListenSocketAddress( hostname, port );
			  _inactivityTimeoutMillis = 0;

			  Server closingChannelServer = CatchupServer( listenSocketAddress );
			  CatchUpClient emptyClient = emptyClient();

			  _lifeSupport.add( closingChannelServer );
			  _lifeSupport.add( emptyClient );

			  // when
			  _lifeSupport.init();
			  _lifeSupport.start();

			  // then
			  CatchUpClientException catchUpClientException = assertThrows( typeof( CatchUpClientException ), () => emptyClient.MakeBlockingRequest(new AdvertisedSocketAddress(hostname, port), new GetStoreIdRequest(), NeverCompletingAdaptor()) );
			  assertEquals( typeof( TimeoutException ), Exceptions.rootCause( catchUpClientException ).GetType() );
		 }

		 private CatchUpClient EmptyClient()
		 {
			  return CatchupClient( new MessageToByteEncoderAnonymousInnerClass( this ) );
		 }

		 private class MessageToByteEncoderAnonymousInnerClass : MessageToByteEncoder<GetStoreIdRequest>
		 {
			 private readonly CatchUpClientIT _outerInstance;

			 public MessageToByteEncoderAnonymousInnerClass( CatchUpClientIT outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void encode( ChannelHandlerContext channelHandlerContext, GetStoreIdRequest getStoreIdRequest, ByteBuf byteBuf )
			 {
				  byteBuf.writeByte( ( sbyte ) 1 );
			 }
		 }

		 private void AssertClosedChannelException( string hostname, int port, CatchUpClient closingClient )
		 {
			  try
			  {
					closingClient.MakeBlockingRequest( new AdvertisedSocketAddress( hostname, port ), new GetStoreIdRequest(), NeverCompletingAdaptor() );
					fail();
			  }
			  catch ( CatchUpClientException e )
			  {
					Exception cause = e.InnerException;
					assertEquals( cause.GetType(), typeof(ExecutionException) );
					Exception actualCause = cause.InnerException;
					assertEquals( actualCause.GetType(), typeof(ClosedChannelException) );
			  }
		 }

		 private CatchUpResponseAdaptor<object> NeverCompletingAdaptor()
		 {
			  return new CatchUpResponseAdaptor<object>();
		 }

		 private CatchUpClient ClosingChannelCatchupClient( AtomicBoolean wasClosedByClient )
		 {
			  return CatchupClient( new MessageToByteEncoderAnonymousInnerClass2( this, wasClosedByClient ) );
		 }

		 private class MessageToByteEncoderAnonymousInnerClass2 : MessageToByteEncoder
		 {
			 private readonly CatchUpClientIT _outerInstance;

			 private AtomicBoolean _wasClosedByClient;

			 public MessageToByteEncoderAnonymousInnerClass2( CatchUpClientIT outerInstance, AtomicBoolean wasClosedByClient )
			 {
				 this.outerInstance = outerInstance;
				 this._wasClosedByClient = wasClosedByClient;
			 }

			 protected internal override void encode( ChannelHandlerContext ctx, object msg, ByteBuf @out )
			 {
				  _wasClosedByClient.set( true );
				  ctx.channel().close();
			 }
		 }

		 private Server ClosingChannelCatchupServer( ListenSocketAddress listenSocketAddress, AtomicBoolean wasClosedByServer )
		 {
			  return CatchupServer( listenSocketAddress, new ByteToMessageDecoderAnonymousInnerClass( this, wasClosedByServer ) );
		 }

		 private class ByteToMessageDecoderAnonymousInnerClass : ByteToMessageDecoder
		 {
			 private readonly CatchUpClientIT _outerInstance;

			 private AtomicBoolean _wasClosedByServer;

			 public ByteToMessageDecoderAnonymousInnerClass( CatchUpClientIT outerInstance, AtomicBoolean wasClosedByServer )
			 {
				 this.outerInstance = outerInstance;
				 this._wasClosedByServer = wasClosedByServer;
			 }

			 protected internal override void decode( ChannelHandlerContext ctx, ByteBuf byteBuf, IList<object> list )
			 {
				  _wasClosedByServer.set( true );
				  ctx.channel().close();
			 }
		 }

		 private CatchUpClient CatchupClient( params ChannelHandler[] channelHandlers )
		 {
			  return new CatchUpClient( NullLogProvider.Instance, Clock.systemUTC(), _inactivityTimeoutMillis, catchUpResponseHandler => new ChannelInitializerAnonymousInnerClass(this, channelHandlers) );
		 }

		 private class ChannelInitializerAnonymousInnerClass : ChannelInitializer<SocketChannel>
		 {
			 private readonly CatchUpClientIT _outerInstance;

			 private ChannelHandler[] _channelHandlers;

			 public ChannelInitializerAnonymousInnerClass( CatchUpClientIT outerInstance, ChannelHandler[] channelHandlers )
			 {
				 this.outerInstance = outerInstance;
				 this._channelHandlers = channelHandlers;
			 }

			 protected internal override void initChannel( SocketChannel ch )
			 {
				  ch.pipeline().addLast(_channelHandlers);
			 }
		 }

		 private Server CatchupServer( ListenSocketAddress listenSocketAddress, params ChannelHandler[] channelHandlers )
		 {
			  return new Server( channel => channel.pipeline().addLast(channelHandlers), listenSocketAddress, "empty-test-server" );
		 }
	}

}