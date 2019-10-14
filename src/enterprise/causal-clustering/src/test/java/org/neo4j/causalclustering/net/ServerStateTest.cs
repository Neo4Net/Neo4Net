/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.net
{
	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using Channel = io.netty.channel.Channel;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using NioSocketChannel = io.netty.channel.socket.nio.NioSocketChannel;
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;

	using SuspendableLifeCycleLifeStateChangeTest = Neo4Net.causalclustering.helper.SuspendableLifeCycleLifeStateChangeTest;
	using SuspendableLifeCycleSuspendedStateChangeTest = Neo4Net.causalclustering.helper.SuspendableLifeCycleSuspendedStateChangeTest;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Level = Neo4Net.Logging.Level;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	/// <summary>
	/// More generalized state tests of SuspendableLifeCycle can be found <seealso cref="SuspendableLifeCycleLifeStateChangeTest"/> and
	/// <seealso cref="SuspendableLifeCycleSuspendedStateChangeTest"/>
	/// </summary>
	public class ServerStateTest
	{
		 private static Bootstrap _bootstrap;
		 private static EventLoopGroup _clientGroup;
		 private Server _server;
		 private Channel _channel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void initialSetup()
		 public static void InitialSetup()
		 {
			  _clientGroup = new NioEventLoopGroup();
			  _bootstrap = ( new Bootstrap() ).group(_clientGroup).channel(typeof(NioSocketChannel)).handler(new ChannelInitializerAnonymousInnerClass());
		 }

		 private class ChannelInitializerAnonymousInnerClass : ChannelInitializer<NioSocketChannel>
		 {
			 protected internal override void initChannel( NioSocketChannel ch )
			 {

			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _server = CreateServer();
			  _server.init();
			  assertFalse( CanConnect() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  if ( _server != null )
			  {
					_server.stop();
					_server.shutdown();
			  }
			  if ( _channel != null )
			  {
					_channel.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void finalTearDown()
		 public static void FinalTearDown()
		 {
			  _clientGroup.shutdownGracefully();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartServerNormally() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartServerNormally()
		 {
			  _server.start();
			  assertTrue( CanConnect() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canDisableAndEnableServer() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanDisableAndEnableServer()
		 {
			  _server.start();
			  assertTrue( CanConnect() );

			  _server.disable();
			  assertFalse( CanConnect() );

			  _server.enable();
			  assertTrue( CanConnect() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverCannotBeEnabledIfLifeCycleHasNotStarted() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ServerCannotBeEnabledIfLifeCycleHasNotStarted()
		 {
			  _server.enable();
			  assertFalse( CanConnect() );

			  _server.start();
			  assertTrue( CanConnect() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverCannotStartIfDisabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ServerCannotStartIfDisabled()
		 {
			  _server.disable();

			  _server.start();
			  assertFalse( CanConnect() );

			  _server.enable();
			  assertTrue( CanConnect() );
		 }

		 private static Server CreateServer()
		 {
			  return new Server(_channel =>
			  {
			  }, FormattedLogProvider.withDefaultLogLevel( Level.DEBUG ).toOutputStream( System.out ), FormattedLogProvider.withDefaultLogLevel( Level.DEBUG ).toOutputStream( System.out ), new ListenSocketAddress( "localhost", PortAuthority.allocatePort() ), "serverName");
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean canConnect() throws InterruptedException
		 private bool CanConnect()
		 {
			  ListenSocketAddress socketAddress = _server.address();
			  ChannelFuture channelFuture = _bootstrap.connect( socketAddress.Hostname, socketAddress.Port );
			  _channel = channelFuture.channel();
			  return channelFuture.await().Success;
		 }
	}

}