using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt.transport
{
	using ServerBootstrap = io.netty.bootstrap.ServerBootstrap;
	using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;
	using Channel = io.netty.channel.Channel;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using ChannelOption = io.netty.channel.ChannelOption;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using Epoll = io.netty.channel.epoll.Epoll;


	using EpollConfigurationProvider = Org.Neo4j.Bolt.transport.configuration.EpollConfigurationProvider;
	using NioConfigurationProvider = Org.Neo4j.Bolt.transport.configuration.NioConfigurationProvider;
	using ServerConfigurationProvider = Org.Neo4j.Bolt.transport.configuration.ServerConfigurationProvider;
	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using PortBindException = Org.Neo4j.Helpers.PortBindException;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using FeatureToggles = Org.Neo4j.Util.FeatureToggles;

	/// <summary>
	/// Simple wrapper around Netty boss and selector threads, which allows multiple ports and protocols to be handled
	/// by the same set of common worker threads.
	/// </summary>
	public class NettyServer : LifecycleAdapter
	{

		 private static readonly bool _useEpoll = FeatureToggles.flag( typeof( NettyServer ), "useEpoll", true );
		 // Not officially configurable, but leave it modifiable via system properties in case we find we need to
		 // change it.
		 private static readonly int _numSelectorThreads = Math.Max( 1, Integer.getInteger( "org.neo4j.selectorThreads", Runtime.Runtime.availableProcessors() * 2 ) );

		 private readonly IDictionary<BoltConnector, ProtocolInitializer> _bootstrappersMap;
		 private readonly ThreadFactory _tf;
		 private readonly ConnectorPortRegister _connectionRegister;
		 private readonly Log _log;
		 private EventLoopGroup _bossGroup;
		 private EventLoopGroup _selectorGroup;

		 /// <summary>
		 /// Describes how to initialize new channels for a protocol, and which address the protocol should be bolted into.
		 /// </summary>
		 public interface ProtocolInitializer
		 {
			  ChannelInitializer<Channel> ChannelInitializer();
			  ListenSocketAddress Address();
		 }

		 /// <param name="tf"> used to create IO threads to listen and handle network events </param>
		 /// <param name="initializersMap">  function per bolt connector map to bootstrap configured protocols </param>
		 /// <param name="connectorRegister"> register to keep local address information on all configured connectors </param>
		 public NettyServer( ThreadFactory tf, IDictionary<BoltConnector, ProtocolInitializer> initializersMap, ConnectorPortRegister connectorRegister, Log log )
		 {
			  this._bootstrappersMap = initializersMap;
			  this._tf = tf;
			  this._connectionRegister = connectorRegister;
			  this._log = log;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  bool useEpoll = _useEpoll && Epoll.Available;
			  ServerConfigurationProvider configurationProvider = useEpoll ? EpollConfigurationProvider.INSTANCE : NioConfigurationProvider.INSTANCE;
			  _bossGroup = configurationProvider.CreateEventLoopGroup( 1, _tf );

			  // These threads handle live channels. Each thread has a set of channels it is responsible for, and it will
			  // continuously run a #select() loop to react to new events on these channels.
			  _selectorGroup = configurationProvider.CreateEventLoopGroup( _numSelectorThreads, _tf );

			  // Bootstrap the various ports and protocols we want to handle

			  foreach ( KeyValuePair<BoltConnector, ProtocolInitializer> bootstrapEntry in _bootstrappersMap.SetOfKeyValuePairs() )
			  {
					try
					{
						 ProtocolInitializer protocolInitializer = bootstrapEntry.Value;
						 BoltConnector boltConnector = bootstrapEntry.Key;
						 ServerBootstrap serverBootstrap = CreateServerBootstrap( configurationProvider, protocolInitializer );
						 ChannelFuture channelFuture = serverBootstrap.bind( protocolInitializer.Address().socketAddress() ).sync();
						 InetSocketAddress localAddress = ( InetSocketAddress ) channelFuture.channel().localAddress();
						 _connectionRegister.register( boltConnector.Key(), localAddress );
						 string host = protocolInitializer.Address().Hostname;
						 int port = localAddress.Port;
						 if ( host.Contains( ":" ) )
						 {
							  // IPv6
							  _log.info( "Bolt enabled on [%s]:%s.", host, port );
						 }
						 else
						 {
							  // IPv4
							  _log.info( "Bolt enabled on %s:%s.", host, port );
						 }
					}
					catch ( Exception e )
					{
						 // We catch throwable here because netty uses clever tricks to have method signatures that look like they do not
						 // throw checked exceptions, but they actually do. The compiler won't let us catch them explicitly because in theory
						 // they shouldn't be possible, so we have to catch Throwable and do our own checks to grab them
						 throw new PortBindException( bootstrapEntry.Value.address(), e );
					}
			  }
		 }

		 public override void Stop()
		 {
			  _bossGroup.shutdownGracefully();
			  _selectorGroup.shutdownGracefully();
		 }

		 private ServerBootstrap CreateServerBootstrap( ServerConfigurationProvider configurationProvider, ProtocolInitializer protocolInitializer )
		 {
			  return ( new ServerBootstrap() ).group(_bossGroup, _selectorGroup).channel(configurationProvider.ChannelClass).option(ChannelOption.ALLOCATOR, PooledByteBufAllocator.DEFAULT).option(ChannelOption.SO_REUSEADDR, true).childOption(ChannelOption.SO_KEEPALIVE, true).childHandler(protocolInitializer.ChannelInitializer());
		 }
	}

}