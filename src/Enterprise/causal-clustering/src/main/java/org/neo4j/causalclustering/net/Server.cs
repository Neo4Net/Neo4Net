using System;
using System.Threading;

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
namespace Neo4Net.causalclustering.net
{
	using ServerBootstrap = io.netty.bootstrap.ServerBootstrap;
	using Channel = io.netty.channel.Channel;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using ChannelOption = io.netty.channel.ChannelOption;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using NioServerSocketChannel = io.netty.channel.socket.nio.NioServerSocketChannel;


	using SuspendableLifeCycle = Neo4Net.causalclustering.helper.SuspendableLifeCycle;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

	public class Server : SuspendableLifeCycle
	{
		 private readonly Log _debugLog;
		 private readonly Log _userLog;
		 private readonly string _serverName;

		 private readonly NamedThreadFactory _threadFactory;
		 private readonly ChildInitializer _childInitializer;
		 private readonly ChannelInboundHandler _parentHandler;
		 private readonly ListenSocketAddress _listenAddress;

		 private EventLoopGroup _workerGroup;
		 private Channel _channel;

		 public Server( ChildInitializer childInitializer, LogProvider debugLogProvider, LogProvider userLogProvider, ListenSocketAddress listenAddress, string serverName ) : this( childInitializer, null, debugLogProvider, userLogProvider, listenAddress, serverName )
		 {
		 }

		 public Server( ChildInitializer childInitializer, ChannelInboundHandler parentHandler, LogProvider debugLogProvider, LogProvider userLogProvider, ListenSocketAddress listenAddress, string serverName ) : base( debugLogProvider.GetLog( typeof( Server ) ) )
		 {
			  this._childInitializer = childInitializer;
			  this._parentHandler = parentHandler;
			  this._listenAddress = listenAddress;
			  this._debugLog = debugLogProvider.getLog( this.GetType() );
			  this._userLog = userLogProvider.getLog( this.GetType() );
			  this._serverName = serverName;
			  this._threadFactory = new NamedThreadFactory( serverName );
		 }

		 public Server( ChildInitializer childInitializer, ListenSocketAddress listenAddress, string serverName ) : this( childInitializer, null, NullLogProvider.Instance, NullLogProvider.Instance, listenAddress, serverName )
		 {
		 }

		 protected internal override void Init0()
		 {
			  // do nothing
		 }

		 protected internal override void Start0()
		 {
			  if ( _channel != null )
			  {
					return;
			  }

			  _workerGroup = new NioEventLoopGroup( 0, _threadFactory );

			  ServerBootstrap bootstrap = ( new ServerBootstrap() ).group(_workerGroup).channel(typeof(NioServerSocketChannel)).option(ChannelOption.SO_REUSEADDR, true).localAddress(_listenAddress.socketAddress()).childHandler(_childInitializer.asChannelInitializer());

			  if ( _parentHandler != null )
			  {
					bootstrap.handler( _parentHandler );
			  }

			  try
			  {
					_channel = bootstrap.bind().syncUninterruptibly().channel();
					_debugLog.info( _serverName + ": bound to " + _listenAddress );
			  }
			  catch ( Exception e )
			  {
					//noinspection ConstantConditions netty sneaky throw
					if ( e is BindException )
					{
						 string message = _serverName + ": address is already bound: " + _listenAddress;
						 _userLog.error( message );
						 _debugLog.error( message, e );
					}
					throw e;
			  }
		 }

		 protected internal override void Stop0()
		 {
			  if ( _channel == null )
			  {
					return;
			  }

			  _debugLog.info( _serverName + ": stopping and unbinding from: " + _listenAddress );
			  try
			  {
					_channel.close().sync();
					_channel = null;
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
					_debugLog.warn( "Interrupted while closing channel." );
			  }

			  if ( _workerGroup != null && _workerGroup.shutdownGracefully( 2, 5, TimeUnit.SECONDS ).awaitUninterruptibly( 10, TimeUnit.SECONDS ) )
			  {
					_debugLog.warn( "Worker group not shutdown within 10 seconds." );
			  }
			  _workerGroup = null;
		 }

		 protected internal override void Shutdown0()
		 {
			  // do nothing
		 }

		 public virtual ListenSocketAddress Address()
		 {
			  return _listenAddress;
		 }

		 public override string ToString()
		 {
			  return format( "Server[%s]", _serverName );
		 }
	}

}