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
namespace Neo4Net.Bolt.v1.transport
{
	using Channel = io.netty.channel.Channel;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using NettyServer = Neo4Net.Bolt.transport.NettyServer;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using PortBindException = Neo4Net.Helpers.PortBindException;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.genericMap;

	public class NettyServerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGivePortConflictErrorWithPortNumberInIt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGivePortConflictErrorWithPortNumberInIt()
		 {
			  // Given an occupied port
			  int port = 16000;
			  using ( ServerSocketChannel ignore = ServerSocketChannel.open().bind(new InetSocketAddress("localhost", port)) )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.helpers.ListenSocketAddress address = new org.Neo4Net.helpers.ListenSocketAddress("localhost", port);
					ListenSocketAddress address = new ListenSocketAddress( "localhost", port );

					// Expect
					Exception.expect( typeof( PortBindException ) );

					// When
					IDictionary<BoltConnector, NettyServer.ProtocolInitializer> initializersMap = genericMap( new BoltConnector( "test" ), ProtocolOnAddress( address ) );
					( new NettyServer( new NamedThreadFactory( "mythreads" ), initializersMap, new ConnectorPortRegister(), NullLog.Instance ) ).start();

			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.bolt.transport.NettyServer.ProtocolInitializer protocolOnAddress(final org.Neo4Net.helpers.ListenSocketAddress address)
		 private NettyServer.ProtocolInitializer ProtocolOnAddress( ListenSocketAddress address )
		 {
			  return new ProtocolInitializerAnonymousInnerClass( this, address );
		 }

		 private class ProtocolInitializerAnonymousInnerClass : NettyServer.ProtocolInitializer
		 {
			 private readonly NettyServerTest _outerInstance;

			 private ListenSocketAddress _address;

			 public ProtocolInitializerAnonymousInnerClass( NettyServerTest outerInstance, ListenSocketAddress address )
			 {
				 this.outerInstance = outerInstance;
				 this._address = address;
			 }

			 public ChannelInitializer<Channel> channelInitializer()
			 {
				  return new ChannelInitializerAnonymousInnerClass( this );
			 }

			 private class ChannelInitializerAnonymousInnerClass : ChannelInitializer<Channel>
			 {
				 private readonly ProtocolInitializerAnonymousInnerClass _outerInstance;

				 public ChannelInitializerAnonymousInnerClass( ProtocolInitializerAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public void initChannel( Channel ch )
				 {
				 }
			 }

			 public ListenSocketAddress address()
			 {
				  return _address;
			 }
		 }
	}

}