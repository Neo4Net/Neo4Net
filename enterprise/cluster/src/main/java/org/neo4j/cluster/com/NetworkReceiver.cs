using System;
using System.Collections.Concurrent;
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
namespace Org.Neo4j.cluster.com
{
	using ServerBootstrap = org.jboss.netty.bootstrap.ServerBootstrap;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelException = org.jboss.netty.channel.ChannelException;
	using ChannelHandlerContext = org.jboss.netty.channel.ChannelHandlerContext;
	using ChannelPipeline = org.jboss.netty.channel.ChannelPipeline;
	using ChannelPipelineFactory = org.jboss.netty.channel.ChannelPipelineFactory;
	using ChannelStateEvent = org.jboss.netty.channel.ChannelStateEvent;
	using Channels = org.jboss.netty.channel.Channels;
	using ExceptionEvent = org.jboss.netty.channel.ExceptionEvent;
	using MessageEvent = org.jboss.netty.channel.MessageEvent;
	using SimpleChannelHandler = org.jboss.netty.channel.SimpleChannelHandler;
	using ChannelGroup = org.jboss.netty.channel.group.ChannelGroup;
	using DefaultChannelGroup = org.jboss.netty.channel.group.DefaultChannelGroup;
	using NioServerSocketChannelFactory = org.jboss.netty.channel.socket.nio.NioServerSocketChannelFactory;
	using ObjectDecoder = org.jboss.netty.handler.codec.serialization.ObjectDecoder;
	using ThreadNameDeterminer = org.jboss.netty.util.ThreadNameDeterminer;
	using ThreadRenamingRunnable = org.jboss.netty.util.ThreadRenamingRunnable;


	using Org.Neo4j.cluster.com.message;
	using MessageProcessor = Org.Neo4j.cluster.com.message.MessageProcessor;
	using MessageSource = Org.Neo4j.cluster.com.message.MessageSource;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using Org.Neo4j.Helpers;
	using NamedThreadFactory = Org.Neo4j.Helpers.NamedThreadFactory;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.NamedThreadFactory.daemon;

	/// <summary>
	/// TCP version of a Networked Instance. This handles receiving messages to be consumed by local state-machines and
	/// sending outgoing messages
	/// </summary>
	public class NetworkReceiver : MessageSource, Lifecycle
	{
		 public interface Monitor : NamedThreadFactory.Monitor
		 {
			  void ReceivedMessage( Message message );

			  void ProcessedMessage( Message message );
		 }

		 public interface Configuration
		 {
			  HostnamePort ClusterServer();

			  int DefaultPort();

			  string Name(); // Name of this cluster instance. Null in most cases, but tools may use e.g. "Backup"
		 }

		 public interface NetworkChannelsListener
		 {
			  void ListeningAt( URI me );

			  void ChannelOpened( URI to );

			  void ChannelClosed( URI to );
		 }

		 public const string CLUSTER_SCHEME = "cluster";
		 public const string INADDR_ANY = "0.0.0.0";

		 private ChannelGroup _channels;

		 // Receiving
		 private NioServerSocketChannelFactory _nioChannelFactory;
		 private ServerBootstrap _serverBootstrap;
		 private readonly Listeners<MessageProcessor> _processors = new Listeners<MessageProcessor>();

		 private readonly Monitor _monitor;
		 private readonly Configuration _config;
		 private readonly Log _msgLog;

		 private readonly IDictionary<URI, Channel> _connections = new ConcurrentDictionary<URI, Channel>();
		 private readonly Listeners<NetworkChannelsListener> _listeners = new Listeners<NetworkChannelsListener>();

		 internal volatile bool BindingDetected;

		 private volatile bool _paused;
		 private int _port;

		 public NetworkReceiver( Monitor monitor, Configuration config, LogProvider logProvider )
		 {
			  this._monitor = monitor;
			  this._config = config;
			  this._msgLog = logProvider.getLog( this.GetType() );
		 }

		 public override void Init()
		 {
			  ThreadRenamingRunnable.ThreadNameDeterminer = ThreadNameDeterminer.CURRENT;
		 }

		 public override void Start()
		 {
			  _channels = new DefaultChannelGroup();

			  // Listen for incoming connections
			  _nioChannelFactory = new NioServerSocketChannelFactory( Executors.newCachedThreadPool( daemon( "Cluster boss", _monitor ) ), Executors.newFixedThreadPool( 2, daemon( "Cluster worker", _monitor ) ), 2 );
			  _serverBootstrap = new ServerBootstrap( _nioChannelFactory );
			  _serverBootstrap.setOption( "child.tcpNoDelay", true );
			  _serverBootstrap.PipelineFactory = new NetworkNodePipelineFactory( this );

			  int[] ports = _config.clusterServer().Ports;

			  int minPort = ports[0];
			  int maxPort = ports.Length == 2 ? ports[1] : minPort;

			  // Try all ports in the given range
			  _port = Listen( minPort, maxPort );

			  _msgLog.debug( "Started NetworkReceiver at " + _config.clusterServer().Host + ":" + _port );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _msgLog.debug( "Shutting down NetworkReceiver at " + _config.clusterServer().Host + ":" + _port );

			  _channels.close().awaitUninterruptibly();
			  _serverBootstrap.releaseExternalResources();
			  _msgLog.debug( "Shutting down NetworkReceiver complete" );
		 }

		 public override void Shutdown()
		 {
		 }

		 public virtual bool Paused
		 {
			 set
			 {
				  this._paused = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int listen(int minPort, int maxPort) throws org.jboss.netty.channel.ChannelException
		 private int Listen( int minPort, int maxPort )
		 {
			  ChannelException ex = null;
			  for ( int checkPort = minPort; checkPort <= maxPort; checkPort++ )
			  {
					try
					{
						 string address = _config.clusterServer().Host;
						 InetSocketAddress localAddress;
						 if ( string.ReferenceEquals( address, null ) || address.Equals( INADDR_ANY ) )
						 {
							  localAddress = new InetSocketAddress( checkPort );
						 }
						 else
						 {
							  localAddress = new InetSocketAddress( address, checkPort );
							  BindingDetected = true;
						 }

						 Channel listenChannel = _serverBootstrap.bind( localAddress );

						 ListeningAt( GetURI( localAddress ) );

						 _channels.add( listenChannel );
						 return checkPort;
					}
					catch ( ChannelException e )
					{
						 ex = e;
					}
			  }

			  _nioChannelFactory.releaseExternalResources();
			  throw ex;
		 }

		 // MessageSource implementation
		 public override void AddMessageProcessor( MessageProcessor processor )
		 {
			  _processors.add( processor );
		 }

		 public virtual void Receive( Message message )
		 {
			  if ( !_paused )
			  {
					foreach ( MessageProcessor processor in _processors )
					{
						 try
						 {
							  if ( !processor.Process( message ) )
							  {
									break;
							  }
						 }
						 catch ( Exception )
						 {
							  // Ignore
						 }
					}

					_monitor.processedMessage( message );
			  }
		 }

		 internal virtual URI GetURI( InetSocketAddress socketAddress )
		 {
			  string uri;

			  InetAddress address = socketAddress.Address;

			  if ( address is Inet6Address )
			  {
					uri = CLUSTER_SCHEME + "://" + WrapAddressForIPv6Uri( address.HostAddress ) + ":" + socketAddress.Port;
			  }
			  else if ( address is Inet4Address )
			  {
					uri = CLUSTER_SCHEME + "://" + address.HostAddress + ":" + socketAddress.Port;
			  }
			  else
			  {
					throw new System.ArgumentException( "Address type unknown" );
			  }

			  // Add name if given
			  if ( !string.ReferenceEquals( _config.name(), null ) )
			  {
					uri += "/?name=" + _config.name();
			  }

			  return URI.create( uri );
		 }

		 public virtual void ListeningAt( URI me )
		 {
			  _listeners.notify( listener => listener.listeningAt( me ) );
		 }

		 protected internal virtual void OpenedChannel( URI uri, Channel ctxChannel )
		 {
			  _connections[uri] = ctxChannel;

			  _listeners.notify( listener => listener.channelOpened( uri ) );
		 }

		 protected internal virtual void ClosedChannel( URI uri )
		 {
			  Channel channel = _connections.Remove( uri );
			  if ( channel != null )
			  {
					channel.close();
			  }

			  _listeners.notify( listener => listener.channelClosed( uri ) );
		 }

		 public virtual void AddNetworkChannelsListener( NetworkChannelsListener listener )
		 {
			  _listeners.add( listener );
		 }

		 private class NetworkNodePipelineFactory : ChannelPipelineFactory
		 {
			 private readonly NetworkReceiver _outerInstance;

			 public NetworkNodePipelineFactory( NetworkReceiver outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override ChannelPipeline Pipeline
			  {
				  get
				  {
						ChannelPipeline pipeline = Channels.pipeline();
						pipeline.addLast( "frameDecoder", new ObjectDecoder( 1024 * 1000, NetworkNodePipelineFactory.this.GetType().ClassLoader ) );
						pipeline.addLast( "serverHandler", new MessageReceiver( _outerInstance ) );
						return pipeline;
				  }
			  }
		 }

		 internal class MessageReceiver : SimpleChannelHandler
		 {
			 private readonly NetworkReceiver _outerInstance;

			 public MessageReceiver( NetworkReceiver outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void ChannelOpen( ChannelHandlerContext ctx, ChannelStateEvent e )
			  {
					Channel ctxChannel = ctx.Channel;
					outerInstance.OpenedChannel( outerInstance.GetURI( ( InetSocketAddress ) ctxChannel.RemoteAddress ), ctxChannel );
					outerInstance.channels.add( ctxChannel );
			  }

			  public override void MessageReceived( ChannelHandlerContext ctx, MessageEvent @event )
			  {
					if ( !outerInstance.BindingDetected )
					{
						 InetSocketAddress local = ( InetSocketAddress ) @event.Channel.LocalAddress;
						 outerInstance.BindingDetected = true;
						 outerInstance.ListeningAt( outerInstance.GetURI( local ) );
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.com.message.Message message = (org.neo4j.cluster.com.message.Message) event.getMessage();
					Message message = ( Message ) @event.Message;

					// Fix HEADER_FROM header since sender cannot know it's correct IP/hostname
					InetSocketAddress remote = ( InetSocketAddress ) ctx.Channel.RemoteAddress;
					string remoteAddress = remote.Address.HostAddress;
					URI fromHeader = URI.create( message.getHeader( Message.HEADER_FROM ) );
					if ( remote.Address is Inet6Address )
					{
						 remoteAddress = WrapAddressForIPv6Uri( remoteAddress );
					}
					fromHeader = URI.create( fromHeader.Scheme + "://" + remoteAddress + ":" + fromHeader.Port );
					message.setHeader( Message.HEADER_FROM, fromHeader.toASCIIString() );

					outerInstance.msgLog.Debug( "Received:" + message );
					outerInstance.monitor.ReceivedMessage( message );
					outerInstance.Receive( message );
			  }

			  public override void ChannelDisconnected( ChannelHandlerContext ctx, ChannelStateEvent e )
			  {
					outerInstance.ClosedChannel( outerInstance.GetURI( ( InetSocketAddress ) ctx.Channel.RemoteAddress ) );
			  }

			  public override void ChannelClosed( ChannelHandlerContext ctx, ChannelStateEvent e )
			  {
					outerInstance.ClosedChannel( outerInstance.GetURI( ( InetSocketAddress ) ctx.Channel.RemoteAddress ) );
					outerInstance.channels.remove( ctx.Channel );
			  }

			  public override void ExceptionCaught( ChannelHandlerContext ctx, ExceptionEvent e )
			  {
					if ( !( e.Cause is ConnectException ) )
					{
						 outerInstance.msgLog.Error( "Receive exception:", e.Cause );
					}
			  }
		 }

		 private static string WrapAddressForIPv6Uri( string address )
		 {
			  return "[" + address + "]";
		 }
	}

}