using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Neo4Net.cluster.com
{
	using ClientBootstrap = org.jboss.netty.bootstrap.ClientBootstrap;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelFuture = org.jboss.netty.channel.ChannelFuture;
	using ChannelHandlerContext = org.jboss.netty.channel.ChannelHandlerContext;
	using ChannelPipeline = org.jboss.netty.channel.ChannelPipeline;
	using ChannelPipelineFactory = org.jboss.netty.channel.ChannelPipelineFactory;
	using ChannelStateEvent = org.jboss.netty.channel.ChannelStateEvent;
	using Channels = org.jboss.netty.channel.Channels;
	using ExceptionEvent = org.jboss.netty.channel.ExceptionEvent;
	using SimpleChannelHandler = org.jboss.netty.channel.SimpleChannelHandler;
	using WriteCompletionEvent = org.jboss.netty.channel.WriteCompletionEvent;
	using ChannelGroup = org.jboss.netty.channel.group.ChannelGroup;
	using DefaultChannelGroup = org.jboss.netty.channel.group.DefaultChannelGroup;
	using NioClientSocketChannelFactory = org.jboss.netty.channel.socket.nio.NioClientSocketChannelFactory;
	using ObjectEncoder = org.jboss.netty.handler.codec.serialization.ObjectEncoder;
	using ThreadNameDeterminer = org.jboss.netty.util.ThreadNameDeterminer;
	using ThreadRenamingRunnable = org.jboss.netty.util.ThreadRenamingRunnable;


	using Neo4Net.cluster.com.message;
	using MessageSender = Neo4Net.cluster.com.message.MessageSender;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Neo4Net.Helpers;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.NetworkReceiver.CLUSTER_SCHEME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.NamedThreadFactory.daemon;

	/// <summary>
	/// TCP version of sending messages. This handles sending messages from state machines to other instances
	/// in the cluster.
	/// </summary>
	public class NetworkSender : MessageSender, Lifecycle
	{
		 public interface Monitor : NamedThreadFactory.Monitor
		 {
			  void QueuedMessage( Message message );

			  void SentMessage( Message message );
		 }

		 public interface Configuration
		 {
			  int DefaultPort(); // This is the default port to try to connect to

			  int Port(); // This is the port we are listening on
		 }

		 public interface NetworkChannelsListener
		 {
			  void ChannelOpened( URI to );

			  void ChannelClosed( URI to );
		 }

		 private ChannelGroup _channels;

		 // Sending
		 // One executor for each receiving instance, so that one blocking instance cannot block others receiving messages
		 private readonly IDictionary<URI, ExecutorService> _senderExecutors = new Dictionary<URI, ExecutorService>();
		 private readonly ISet<URI> _failedInstances = new HashSet<URI>(); // Keeps track of what instances we have failed to open
		 // connections to
		 private ClientBootstrap _clientBootstrap;

		 private readonly Monitor _monitor;
		 private readonly Configuration _config;
		 private readonly NetworkReceiver _receiver;
		 private readonly Log _msgLog;
		 private URI _me;

		 private readonly IDictionary<URI, Channel> _connections = new ConcurrentDictionary<URI, Channel>();
		 private readonly Listeners<NetworkChannelsListener> _listeners = new Listeners<NetworkChannelsListener>();

		 private volatile bool _paused;

		 public NetworkSender( Monitor monitor, Configuration config, NetworkReceiver receiver, LogProvider logProvider )
		 {
			  this._monitor = monitor;
			  this._config = config;
			  this._receiver = receiver;
			  this._msgLog = logProvider.getLog( this.GetType() );
			  _me = URI.create( CLUSTER_SCHEME + "://0.0.0.0:" + config.Port() );
			  receiver.AddNetworkChannelsListener( new NetworkChannelsListenerAnonymousInnerClass( this ) );
		 }

		 private class NetworkChannelsListenerAnonymousInnerClass : NetworkReceiver.NetworkChannelsListener
		 {
			 private readonly NetworkSender _outerInstance;

			 public NetworkChannelsListenerAnonymousInnerClass( NetworkSender outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void listeningAt( URI me )
			 {
				  _outerInstance.me = me;
			 }

			 public void channelOpened( URI to )
			 {
			 }

			 public void channelClosed( URI to )
			 {
			 }
		 }

		 public override void Init()
		 {
			  ThreadRenamingRunnable.ThreadNameDeterminer = ThreadNameDeterminer.CURRENT;
		 }

		 public override void Start()
		 {
			  _channels = new DefaultChannelGroup();

			  // Start client bootstrap
			  _clientBootstrap = new ClientBootstrap( new NioClientSocketChannelFactory( Executors.newSingleThreadExecutor( daemon( "Cluster client boss", _monitor ) ), Executors.newFixedThreadPool( 2, daemon( "Cluster client worker", _monitor ) ), 2 ) );
			  _clientBootstrap.setOption( "tcpNoDelay", true );
			  _clientBootstrap.PipelineFactory = new NetworkNodePipelineFactory( this );

			  _msgLog.debug( "Started NetworkSender for " + ToString( _config ) );
		 }

		 private string ToString( Configuration config )
		 {
			  return "defaultPort:" + config.DefaultPort() + ", port:" + config.Port();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _msgLog.debug( "Shutting down NetworkSender" );
			  foreach ( ExecutorService executorService in _senderExecutors.Values )
			  {
					executorService.shutdown();
			  }
			  long totalWaitTime = 0;
			  long maxWaitTime = SECONDS.toMillis( 5 );
			  foreach ( KeyValuePair<URI, ExecutorService> entry in _senderExecutors.SetOfKeyValuePairs() )
			  {
					URI targetAddress = entry.Key;
					ExecutorService executorService = entry.Value;

					long start = currentTimeMillis();
					if ( !executorService.awaitTermination( maxWaitTime - totalWaitTime, MILLISECONDS ) )
					{
						 _msgLog.warn( "Could not shut down send executor towards: " + targetAddress );
						 break;
					}
					totalWaitTime += currentTimeMillis() - start;
			  }
			  _senderExecutors.Clear();

			  _channels.close().awaitUninterruptibly();
			  _clientBootstrap.releaseExternalResources();
			  _msgLog.debug( "Shutting down NetworkSender for " + ToString( _config ) + " complete" );
		 }

		 public override void Shutdown()
		 {
		 }

		 // MessageSender implementation
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void process(final java.util.List<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>> messages)
		 public override void Process<T1>( IList<T1> messages ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> message : messages)
			  foreach ( Message<MessageType> message in messages )
			  {
					try
					{
						 Process( message );
					}
					catch ( Exception e )
					{
						 _msgLog.warn( "Error sending message " + message + "(" + e.Message + ")" );
					}
			  }
		 }

		 public override bool Process<T1>( Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
			  if ( !_paused )
			  {
					if ( message.HasHeader( Message.HEADER_TO ) )
					{
						 Send( message );
					}
					else
					{
						 // Internal message
						 _receiver.receive( message );
					}
			  }
			  return true;
		 }

		 public virtual bool Paused
		 {
			 set
			 {
				  this._paused = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URI getURI(java.net.InetSocketAddress address) throws java.net.URISyntaxException
		 private URI GetURI( InetSocketAddress address )
		 {
			  return new URI( CLUSTER_SCHEME + ":/" + address ); // Socket.toString() already prepends a /
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private synchronized void send(final org.neo4j.cluster.com.message.Message message)
		 private void Send( Message message )
		 {
			 lock ( this )
			 {
				  _monitor.queuedMessage( message );
      
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.URI to = java.net.URI.create(message.getHeader(org.neo4j.cluster.com.message.Message.HEADER_TO));
				  URI to = URI.create( message.getHeader( Message.HEADER_TO ) );
      
				  ExecutorService senderExecutor = _senderExecutors.computeIfAbsent( to, t => Executors.newSingleThreadExecutor( new NamedThreadFactory( "Cluster Sender " + t.toASCIIString(), _monitor ) ) );
      
				  senderExecutor.submit(() =>
				  {
					Channel channel = GetChannel( to );

					try
					{
						 if ( channel == null )
						 {
							  channel = OpenChannel( to );
							  OpenedChannel( to, channel );

							  // Instance could be connected to, remove any marker of it being failed
							  _failedInstances.remove( to );
						 }
					}
					catch ( Exception e )
					{
						 // Only print out failure message on first fail
						 if ( !_failedInstances.Contains( to ) )
						 {
							  _msgLog.warn( e.Message );
							  _failedInstances.Add( to );
						 }

						 return;
					}

					try
					{
						 // Set HEADER_FROM header
						 message.setHeader( Message.HEADER_FROM, _me.toASCIIString() );

						 _msgLog.debug( "Sending to " + to + ": " + message );

						 ChannelFuture future = channel.write( message );
						 future.addListener(future1 =>
						 {
							  _monitor.sentMessage( message );

							  if ( !future1.Success )
							  {
									_msgLog.debug( "Unable to write " + message + " to " + future1.Channel, future1.Cause );
									ClosedChannel( future1.Channel );

									// Try again
									Send( message );
							  }
						 });
					}
					catch ( Exception e )
					{
						 if ( Exceptions.contains( e, typeof( ClosedChannelException ) ) )
						 {
							  _msgLog.warn( "Could not send message, because the connection has been closed." );
						 }
						 else
						 {
							  _msgLog.warn( "Could not send message", e );
						 }
						 channel.close();
					}
				  });
			 }
		 }

		 protected internal virtual void OpenedChannel( URI uri, Channel ctxChannel )
		 {
			  _connections[uri] = ctxChannel;

			  _listeners.notify( listener => listener.channelOpened( uri ) );
		 }

		 protected internal virtual void ClosedChannel( Channel channelClosed )
		 {
			  /*
			   * Netty channels do not have the remote address set when closed (technically, when not connected). So
			   * we need to do a reverse lookup
			   */
			  URI to = null;
			  foreach ( KeyValuePair<URI, Channel> uriChannelEntry in _connections.SetOfKeyValuePairs() )
			  {
					if ( uriChannelEntry.Value.Equals( channelClosed ) )
					{
						 to = uriChannelEntry.Key;
						 break;
					}
			  }

			  if ( to == null )
			  {
					/*
					 * This is normal to happen if a channel fails to open - channelOpened() will not be called and the
					 * association with the URI will not exist, but channelClosed() will be called anyway.
					 */
					return;
			  }

			  _connections.Remove( to );

			  URI uri = to;

			  _listeners.notify( listener => listener.channelClosed( uri ) );
		 }

		 public virtual Channel GetChannel( URI uri )
		 {
			  return _connections[uri];
		 }

		 public virtual void AddNetworkChannelsListener( NetworkChannelsListener listener )
		 {
			  _listeners.add( listener );
		 }

		 private Channel OpenChannel( URI clusterUri )
		 {
			  SocketAddress destination = new InetSocketAddress( clusterUri.Host, clusterUri.Port == -1 ? _config.defaultPort() : clusterUri.Port );
			  // We must specify the origin address in case the server has multiple IPs per interface
			  SocketAddress origin = new InetSocketAddress( _me.Host, 0 );

			  _msgLog.info( "Attempting to connect from " + origin + " to " + destination );
			  ChannelFuture channelFuture = _clientBootstrap.connect( destination, origin );
			  channelFuture.awaitUninterruptibly( 5, TimeUnit.SECONDS );

			  if ( channelFuture.Success )
			  {
					Channel channel = channelFuture.Channel;
					_msgLog.info( "Connected from " + channel.LocalAddress + " to " + channel.RemoteAddress );
					return channel;

			  }

			  Exception cause = channelFuture.Cause;
			  _msgLog.info( "Failed to connect to " + destination + " due to: " + cause );

			  throw new ChannelOpenFailedException( cause );
		 }

		 private class NetworkNodePipelineFactory : ChannelPipelineFactory
		 {
			 private readonly NetworkSender _outerInstance;

			 public NetworkNodePipelineFactory( NetworkSender outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override ChannelPipeline Pipeline
			  {
				  get
				  {
						ChannelPipeline pipeline = Channels.pipeline();
						pipeline.addLast( "frameEncoder", new ObjectEncoder( 2048 ) );
						pipeline.addLast( "sender", new NetworkMessageSender( _outerInstance ) );
						return pipeline;
				  }
			  }
		 }

		 private class NetworkMessageSender : SimpleChannelHandler
		 {
			 private readonly NetworkSender _outerInstance;

			 public NetworkMessageSender( NetworkSender outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal Exception LastException;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void channelConnected(org.jboss.netty.channel.ChannelHandlerContext ctx, org.jboss.netty.channel.ChannelStateEvent e) throws Exception
			  public override void ChannelConnected( ChannelHandlerContext ctx, ChannelStateEvent e )
			  {
					Channel ctxChannel = ctx.Channel;
					outerInstance.OpenedChannel( outerInstance.getURI( ( InetSocketAddress ) ctxChannel.RemoteAddress ), ctxChannel );
					outerInstance.channels.add( ctxChannel );
			  }

			  public override void ChannelClosed( ChannelHandlerContext ctx, ChannelStateEvent e )
			  {
					outerInstance.ClosedChannel( ctx.Channel );
					outerInstance.channels.remove( ctx.Channel );
			  }

			  public override void ExceptionCaught( ChannelHandlerContext ctx, ExceptionEvent e )
			  {
					Exception cause = e.Cause;
					if ( !( cause is ConnectException || cause is RejectedExecutionException ) )
					{
						 // If we keep getting the same exception, only output the first one
						 if ( LastException != null && !LastException.GetType().Equals(cause.GetType()) )
						 {
							  outerInstance.msgLog.Error( "Receive exception:", cause );
							  LastException = cause;
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeComplete(org.jboss.netty.channel.ChannelHandlerContext ctx, org.jboss.netty.channel.WriteCompletionEvent e) throws Exception
			  public override void WriteComplete( ChannelHandlerContext ctx, WriteCompletionEvent e )
			  {
					if ( LastException != null )
					{
						 outerInstance.msgLog.Error( "Recovered from:", LastException );
						 LastException = null;
					}
					base.WriteComplete( ctx, e );
			  }
		 }
	}

}