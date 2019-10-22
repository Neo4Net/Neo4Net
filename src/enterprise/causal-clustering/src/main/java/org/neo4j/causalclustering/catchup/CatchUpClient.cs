using System;

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
namespace Neo4Net.causalclustering.catchup
{
	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using Channel = io.netty.channel.Channel;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelFutureListener = io.netty.channel.ChannelFutureListener;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using NioSocketChannel = io.netty.channel.socket.nio.NioSocketChannel;


	using CatchUpRequest = Neo4Net.causalclustering.messaging.CatchUpRequest;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.catchup.TimeoutLoop.waitForCompletion;

	public class CatchUpClient : LifecycleAdapter
	{
		 private readonly Log _log;
		 private readonly Clock _clock;
		 private readonly long _inactivityTimeoutMillis;
		 private readonly System.Func<CatchUpResponseHandler, ChannelInitializer<SocketChannel>> _channelInitializer;

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private readonly CatchUpChannelPool<CatchUpChannel> _pool = new CatchUpChannelPool<CatchUpChannel>( CatchUpChannel::new );

		 private NioEventLoopGroup _eventLoopGroup;

		 public CatchUpClient( LogProvider logProvider, Clock clock, long inactivityTimeoutMillis, System.Func<CatchUpResponseHandler, ChannelInitializer<SocketChannel>> channelInitializer )
		 {
			  this._log = logProvider.getLog( this.GetType() );
			  this._clock = clock;
			  this._inactivityTimeoutMillis = inactivityTimeoutMillis;
			  this._channelInitializer = channelInitializer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> T makeBlockingRequest(org.Neo4Net.helpers.AdvertisedSocketAddress upstream, org.Neo4Net.causalclustering.messaging.CatchUpRequest request, CatchUpResponseCallback<T> responseHandler) throws CatchUpClientException
		 public virtual T MakeBlockingRequest<T>( AdvertisedSocketAddress upstream, CatchUpRequest request, CatchUpResponseCallback<T> responseHandler )
		 {
			  CompletableFuture<T> future = new CompletableFuture<T>();

			  CatchUpChannel channel = null;
			  try
			  {
					channel = _pool.acquire( upstream );
					channel.SetResponseHandler( responseHandler, future );
					future.whenComplete( new ReleaseOnComplete( this, channel ) );
					channel.Send( request );
			  }
			  catch ( Exception e )
			  {
					if ( channel != null )
					{
						 _pool.dispose( channel );
					}
					throw new CatchUpClientException( "Failed to send request", e );
			  }
			  string operation = format( "Completed exceptionally when executing operation %s on %s ", request, upstream );
			  return waitForCompletion( future, operation, channel.millisSinceLastResponse, _inactivityTimeoutMillis, _log );
		 }

		 private class ReleaseOnComplete : System.Action<object, Exception>
		 {
			 private readonly CatchUpClient _outerInstance;

			  internal CatchUpChannel CatchUpChannel;

			  internal ReleaseOnComplete( CatchUpClient outerInstance, CatchUpChannel catchUpChannel )
			  {
				  this._outerInstance = outerInstance;
					this.CatchUpChannel = catchUpChannel;
			  }

			  public override void Accept( object o, Exception throwable )
			  {
					if ( throwable == null )
					{
						 outerInstance.pool.Release( CatchUpChannel );
					}
					else
					{
						 outerInstance.pool.Dispose( CatchUpChannel );
					}
			  }
		 }

		 private class CatchUpChannel : CatchUpChannelPool.Channel
		 {
			 private readonly CatchUpClient _outerInstance;

			  internal readonly TrackingResponseHandler Handler;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AdvertisedSocketAddress DestinationConflict;
			  internal Channel NettyChannel;
			  internal readonly Bootstrap Bootstrap;

			  internal CatchUpChannel( CatchUpClient outerInstance, AdvertisedSocketAddress destination )
			  {
				  this._outerInstance = outerInstance;
					this.DestinationConflict = destination;
					Handler = new TrackingResponseHandler( new CatchUpResponseAdaptor(), outerInstance.clock );
					Bootstrap = ( new Bootstrap() ).group(outerInstance.eventLoopGroup).channel(typeof(NioSocketChannel)).handler(outerInstance.channelInitializer.apply(Handler));
			  }

			  internal virtual void SetResponseHandler<T1>( CatchUpResponseCallback responseHandler, CompletableFuture<T1> requestOutcomeSignal )
			  {
					Handler.setResponseHandler( responseHandler, requestOutcomeSignal );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void send(org.Neo4Net.causalclustering.messaging.CatchUpRequest request) throws java.net.ConnectException
			  internal virtual void Send( CatchUpRequest request )
			  {
					if ( !Active )
					{
						 throw new ConnectException( "Channel is not connected" );
					}
					NettyChannel.write( request.MessageType() );
					NettyChannel.writeAndFlush( request );
			  }

			  internal virtual long? MillisSinceLastResponse()
			  {
					return Handler.lastResponseTime().map(lastResponseMillis => outerInstance.clock.millis() - lastResponseMillis);
			  }

			  public override AdvertisedSocketAddress Destination()
			  {
					return DestinationConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void connect() throws Exception
			  public override void Connect()
			  {
					ChannelFuture channelFuture = Bootstrap.connect( DestinationConflict.socketAddress() );
					NettyChannel = channelFuture.sync().channel();
					NettyChannel.closeFuture().addListener((ChannelFutureListener) future => Handler.onClose());
			  }

			  public virtual bool Active
			  {
				  get
				  {
						return NettyChannel.Active;
				  }
			  }

			  public override void Close()
			  {
					if ( NettyChannel != null )
					{
						 NettyChannel.close();
					}
			  }
		 }

		 public override void Start()
		 {
			  _eventLoopGroup = new NioEventLoopGroup( 0, new NamedThreadFactory( "catch-up-client" ) );
		 }

		 public override void Stop()
		 {
			  _log.info( "CatchUpClient stopping" );
			  try
			  {
					_pool.close();
					_eventLoopGroup.shutdownGracefully( 0, 0, MICROSECONDS ).sync();
			  }
			  catch ( InterruptedException )
			  {
					_log.warn( "Interrupted while stopping catch up client." );
			  }
		 }
	}

}