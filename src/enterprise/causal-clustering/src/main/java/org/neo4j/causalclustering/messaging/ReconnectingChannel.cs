/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.messaging
{
	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelFutureListener = io.netty.channel.ChannelFutureListener;
	using EventLoop = io.netty.channel.EventLoop;
	using AttributeKey = io.netty.util.AttributeKey;
	using Promise = io.netty.util.concurrent.Promise;


	using ExponentialBackoffStrategy = Neo4Net.causalclustering.helper.ExponentialBackoffStrategy;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using ProtocolStack = Neo4Net.causalclustering.protocol.handshake.ProtocolStack;
	using SocketAddress = Neo4Net.Helpers.SocketAddress;
	using Log = Neo4Net.Logging.Log;
	using CappedLogger = Neo4Net.Logging.Internal.CappedLogger;

	public class ReconnectingChannel : Channel
	{
		 public static readonly AttributeKey<ProtocolStack> ProtocolStackKey = AttributeKey.ValueOf( "PROTOCOL_STACK" );

		 private readonly Log _log;
		 private readonly Bootstrap _bootstrap;
		 private readonly EventLoop _eventLoop;
		 private readonly SocketAddress _destination;
		 private readonly TimeoutStrategy _connectionBackoffStrategy;

		 private volatile io.netty.channel.Channel _channel;
		 private volatile ChannelFuture _fChannel;

		 private volatile bool _disposed;

		 private Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout _connectionBackoff;
		 private CappedLogger _cappedLogger;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: ReconnectingChannel(io.netty.bootstrap.Bootstrap bootstrap, io.netty.channel.EventLoop eventLoop, org.Neo4Net.helpers.SocketAddress destination, final org.Neo4Net.logging.Log log)
		 internal ReconnectingChannel( Bootstrap bootstrap, EventLoop eventLoop, SocketAddress destination, Log log ) : this( bootstrap, eventLoop, destination, log, new ExponentialBackoffStrategy( 100, 1600, MILLISECONDS ) )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private ReconnectingChannel(io.netty.bootstrap.Bootstrap bootstrap, io.netty.channel.EventLoop eventLoop, org.Neo4Net.helpers.SocketAddress destination, final org.Neo4Net.logging.Log log, org.Neo4Net.causalclustering.helper.TimeoutStrategy connectionBackoffStrategy)
		 private ReconnectingChannel( Bootstrap bootstrap, EventLoop eventLoop, SocketAddress destination, Log log, TimeoutStrategy connectionBackoffStrategy )
		 {
			  this._bootstrap = bootstrap;
			  this._eventLoop = eventLoop;
			  this._destination = destination;
			  this._log = log;
			  this._cappedLogger = ( new CappedLogger( log ) ).setTimeLimit( 20, TimeUnit.SECONDS, Clock.systemUTC() );
			  this._connectionBackoffStrategy = connectionBackoffStrategy;
			  this._connectionBackoff = connectionBackoffStrategy.NewTimeout();
		 }

		 internal virtual void Start()
		 {
			  TryConnect();
		 }

		 private void TryConnect()
		 {
			 lock ( this )
			 {
				  if ( _disposed )
				  {
						return;
				  }
				  else if ( _fChannel != null && !_fChannel.Done )
				  {
						return;
				  }
      
				  _fChannel = _bootstrap.connect( _destination.socketAddress() );
				  _channel = _fChannel.channel();
      
				  _fChannel.addListener((ChannelFuture f) =>
				  {
					if ( !f.Success )
					{
						 long millis = _connectionBackoff.Millis;
						 _cappedLogger.warn( "Failed to connect to: " + _destination.socketAddress() + ". Retrying in " + millis + " ms" );
						 f.channel().eventLoop().schedule(this.tryConnect, millis, MILLISECONDS);
						 _connectionBackoff.increment();
					}
					else
					{
						 _log.info( "Connected: " + f.channel() );
						 f.channel().closeFuture().addListener(closed =>
						 {
							  _log.warn( string.Format( "Lost connection to: {0} ({1})", _destination, _channel.remoteAddress() ) );
							  _connectionBackoff = _connectionBackoffStrategy.newTimeout();
							  f.channel().eventLoop().schedule(this.tryConnect, 0, MILLISECONDS);
						 });
					}
				  });
			 }
		 }

		 public override void Dispose()
		 {
			 lock ( this )
			 {
				  _disposed = true;
				  _channel.close();
			 }
		 }

		 public virtual bool Disposed
		 {
			 get
			 {
				  return _disposed;
			 }
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return _channel.Open;
			 }
		 }

		 public override Future<Void> Write( object msg )
		 {
			  return Write( msg, false );
		 }

		 public override Future<Void> WriteAndFlush( object msg )
		 {
			  return Write( msg, true );
		 }

		 private Future<Void> Write( object msg, bool flush )
		 {
			  if ( _disposed )
			  {
					throw new System.InvalidOperationException( "sending on disposed channel" );
			  }

			  if ( _channel.Active )
			  {
					if ( flush )
					{
						 return _channel.writeAndFlush( msg );
					}
					else
					{
						 return _channel.write( msg );
					}
			  }
			  else
			  {
					Promise<Void> promise = _eventLoop.newPromise();
					System.Action<io.netty.channel.Channel, object> writer;

					if ( flush )
					{
						 writer = ( _channel, message ) => chain( _channel.writeAndFlush( msg ), promise );
					}
					else
					{
						 writer = ( _channel, message ) => chain( _channel.write( msg ), promise );
					}

					DeferredWrite( msg, _fChannel, promise, true, writer );
					return promise;
			  }
		 }

		 /// <summary>
		 /// Chains a channel future to a promise. Used when the returned promise
		 /// was not allocated through the channel and cannot be used as the
		 /// first-hand promise for the I/O operation.
		 /// </summary>
		 private static void Chain( ChannelFuture when, Promise<Void> then )
		 {
			  when.addListener(f =>
			  {
			  if ( f.Success )
			  {
				  then.Success = when.get();
			  }
			  else
			  {
				  then.Failure = when.cause();
			  }
			  });
		 }

		 /// <summary>
		 /// Will try to reconnect once before giving up on a send. The reconnection *must* happen
		 /// after write was scheduled. This is necessary to provide proper ordering when a message
		 /// is sent right after the non-blocking channel was setup and before the server is ready
		 /// to accept a connection. This happens frequently in tests.
		 /// </summary>
		 private void DeferredWrite( object msg, ChannelFuture channelFuture, Promise<Void> promise, bool firstAttempt, System.Action<io.netty.channel.Channel, object> writer )
		 {
			  channelFuture.addListener((ChannelFutureListener) f =>
			  {
				if ( f.Success )
				{
					 writer( f.channel(), msg );
				}
				else if ( firstAttempt )
				{
					 TryConnect();
					 DeferredWrite( msg, _fChannel, promise, false, writer );
				}
				else
				{
					 promise.Failure = f.cause();
				}
			  });
		 }

		 public virtual Optional<ProtocolStack> InstalledProtocolStack()
		 {
			  return Optional.ofNullable( _channel.attr( ProtocolStackKey ).get() );
		 }

		 public override string ToString()
		 {
			  return "ReconnectingChannel{" + "channel=" + _channel + ", disposed=" + _disposed + '}';
		 }
	}

}