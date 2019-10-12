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
namespace Org.Neo4j.causalclustering.messaging
{

	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using NioSocketChannel = io.netty.channel.socket.nio.NioSocketChannel;

	using ProtocolStack = Org.Neo4j.causalclustering.protocol.handshake.ProtocolStack;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using NamedThreadFactory = Org.Neo4j.Helpers.NamedThreadFactory;
	using Org.Neo4j.Helpers.Collection;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class SenderService : LifecycleAdapter, Outbound<AdvertisedSocketAddress, Message>
	{
		 private ReconnectingChannels _channels;

		 private readonly ChannelInitializer _channelInitializer;
		 private readonly ReadWriteLock _serviceLock = new ReentrantReadWriteLock();
		 private readonly Log _log;

		 private JobHandle _jobHandle;
		 private bool _senderServiceRunning;
		 private Bootstrap _bootstrap;
		 private NioEventLoopGroup _eventLoopGroup;

		 public SenderService( ChannelInitializer channelInitializer, LogProvider logProvider )
		 {
			  this._channelInitializer = channelInitializer;
			  this._log = logProvider.getLog( this.GetType() );
			  this._channels = new ReconnectingChannels();
		 }

		 public override void Send( AdvertisedSocketAddress to, Message message, bool block )
		 {
			  Future<Void> future;
			  _serviceLock.readLock().@lock();
			  try
			  {
					if ( !_senderServiceRunning )
					{
						 return;
					}

					future = Channel( to ).writeAndFlush( message );
			  }
			  finally
			  {
					_serviceLock.readLock().unlock();
			  }

			  if ( block )
			  {
					try
					{
						 future.get();
					}
					catch ( ExecutionException e )
					{
						 _log.error( "Exception while sending to: " + to, e );
					}
					catch ( InterruptedException e )
					{
						 _log.info( "Interrupted while sending", e );
					}
			  }
		 }

		 private Channel Channel( AdvertisedSocketAddress destination )
		 {
			  ReconnectingChannel channel = _channels.get( destination );

			  if ( channel == null )
			  {
					channel = new ReconnectingChannel( _bootstrap, _eventLoopGroup.next(), destination, _log );
					channel.Start();
					ReconnectingChannel existingNonBlockingChannel = _channels.putIfAbsent( destination, channel );

					if ( existingNonBlockingChannel != null )
					{
						 channel.Dispose();
						 channel = existingNonBlockingChannel;
					}
					else
					{
						 _log.info( "Creating channel to: [%s] ", destination );
					}
			  }

			  return channel;
		 }

		 public override void Start()
		 {
			 lock ( this )
			 {
				  _serviceLock.writeLock().@lock();
				  try
				  {
						_eventLoopGroup = new NioEventLoopGroup( 0, new NamedThreadFactory( "sender-service" ) );
						_bootstrap = ( new Bootstrap() ).group(_eventLoopGroup).channel(typeof(NioSocketChannel)).handler(_channelInitializer);
      
						_senderServiceRunning = true;
				  }
				  finally
				  {
						_serviceLock.writeLock().unlock();
				  }
			 }
		 }

		 public override void Stop()
		 {
			 lock ( this )
			 {
				  _serviceLock.writeLock().@lock();
				  try
				  {
						_senderServiceRunning = false;
      
						if ( _jobHandle != null )
						{
							 _jobHandle.cancel( true );
							 _jobHandle = null;
						}
      
						IEnumerator<ReconnectingChannel> itr = _channels.values().GetEnumerator();
						while ( itr.MoveNext() )
						{
							 Channel timestampedChannel = itr.Current;
							 timestampedChannel.Dispose();
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							 itr.remove();
						}
      
						try
						{
							 _eventLoopGroup.shutdownGracefully( 0, 0, MICROSECONDS ).sync();
						}
						catch ( InterruptedException )
						{
							 _log.warn( "Interrupted while stopping sender service." );
						}
				  }
				  finally
				  {
						_serviceLock.writeLock().unlock();
				  }
			 }
		 }

		 public virtual Stream<Pair<AdvertisedSocketAddress, ProtocolStack>> InstalledProtocols()
		 {
			  return _channels.installedProtocols();
		 }
	}

}