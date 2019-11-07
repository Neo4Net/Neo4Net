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
namespace Neo4Net.Bolt.transport
{
	using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;
	using Channel = io.netty.channel.Channel;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using SslContext = io.netty.handler.ssl.SslContext;

	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using NetworkConnectionTracker = Neo4Net.Kernel.Api.net.NetworkConnectionTracker;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Implements a transport for the Neo4Net Messaging Protocol that uses good old regular sockets.
	/// </summary>
	public class SocketTransport : NettyServer.ProtocolInitializer
	{
		 private readonly string _connector;
		 private readonly ListenSocketAddress _address;
		 private readonly SslContext _sslCtx;
		 private readonly bool _encryptionRequired;
		 private readonly LogProvider _logging;
		 private readonly TransportThrottleGroup _throttleGroup;
		 private readonly BoltProtocolFactory _boltProtocolFactory;
		 private readonly NetworkConnectionTracker _connectionTracker;

		 public SocketTransport( string connector, ListenSocketAddress address, SslContext sslCtx, bool encryptionRequired, LogProvider logging, TransportThrottleGroup throttleGroup, BoltProtocolFactory boltProtocolFactory, NetworkConnectionTracker connectionTracker )
		 {
			  this._connector = connector;
			  this._address = address;
			  this._sslCtx = sslCtx;
			  this._encryptionRequired = encryptionRequired;
			  this._logging = logging;
			  this._throttleGroup = throttleGroup;
			  this._boltProtocolFactory = boltProtocolFactory;
			  this._connectionTracker = connectionTracker;
		 }

		 public override ChannelInitializer<Channel> ChannelInitializer()
		 {
			  return new ChannelInitializerAnonymousInnerClass( this );
		 }

		 private class ChannelInitializerAnonymousInnerClass : ChannelInitializer<Channel>
		 {
			 private readonly SocketTransport _outerInstance;

			 public ChannelInitializerAnonymousInnerClass( SocketTransport outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void initChannel( Channel ch )
			 {
				  ch.config().Allocator = PooledByteBufAllocator.DEFAULT;

				  BoltChannel boltChannel = outerInstance.newBoltChannel( ch );
				  _outerInstance.connectionTracker.add( boltChannel );
				  ch.closeFuture().addListener(future => _outerInstance.connectionTracker.remove(boltChannel));

				  // install throttles
				  _outerInstance.throttleGroup.install( ch );

				  // add a close listener that will uninstall throttles
				  ch.closeFuture().addListener(future => _outerInstance.throttleGroup.uninstall(ch));

				  TransportSelectionHandler transportSelectionHandler = new TransportSelectionHandler( boltChannel, _outerInstance.sslCtx, _outerInstance.encryptionRequired, false, _outerInstance.logging, _outerInstance.boltProtocolFactory );

				  ch.pipeline().addLast(transportSelectionHandler);
			 }
		 }

		 public override ListenSocketAddress Address()
		 {
			  return _address;
		 }

		 private BoltChannel NewBoltChannel( Channel ch )
		 {
			  return new BoltChannel( _connectionTracker.newConnectionId( _connector ), _connector, ch );
		 }
	}

}