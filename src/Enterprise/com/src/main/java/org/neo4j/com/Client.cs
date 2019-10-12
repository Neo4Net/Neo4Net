using System;
using System.Diagnostics;
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
namespace Neo4Net.com
{
	using ClientBootstrap = org.jboss.netty.bootstrap.ClientBootstrap;
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using ChannelBuffers = org.jboss.netty.buffer.ChannelBuffers;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelFuture = org.jboss.netty.channel.ChannelFuture;
	using ChannelPipeline = org.jboss.netty.channel.ChannelPipeline;
	using ChannelPipelineFactory = org.jboss.netty.channel.ChannelPipelineFactory;
	using Channels = org.jboss.netty.channel.Channels;
	using NioClientSocketChannelFactory = org.jboss.netty.channel.socket.nio.NioClientSocketChannelFactory;
	using BlockingReadHandler = org.jboss.netty.handler.queue.BlockingReadHandler;


	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using ResponseUnpacker_TxHandler = Neo4Net.com.storecopy.ResponseUnpacker_TxHandler;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.addLengthFieldPipes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.Protocol.assertChunkSizeIsWithinFrameSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.ResourcePool.DEFAULT_CHECK_INTERVAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.ResponseUnpacker_TxHandler_Fields.NO_OP_TX_HANDLER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.throwIfUnchecked;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.NamedThreadFactory.daemon;

	/// <summary>
	/// A means for a client to communicate with a <seealso cref="Server"/>. It
	/// serializes requests and sends them to the server and waits for
	/// a response back.
	/// </summary>
	/// <seealso cref= Server </seealso>
	public abstract class Client<T> : LifecycleAdapter, ChannelPipelineFactory
	{
		 // Max number of concurrent channels that may exist. Needs to be high because we
		 // don't want to run into that limit, it will make some #acquire calls block and
		 // gets disastrous if that thread is holding monitors that is needed to communicate
		 // with the server in some way.
		 public const int DEFAULT_MAX_NUMBER_OF_CONCURRENT_CHANNELS_PER_CLIENT = 20;
		 public const int DEFAULT_READ_RESPONSE_TIMEOUT_SECONDS = 20;

		 private const string BLOCKING_CHANNEL_HANDLER_NAME = "blockingHandler";
		 private const string MONITORING_CHANNEL_HANDLER_NAME = "monitor";

		 private ClientBootstrap _bootstrap;
		 private readonly SocketAddress _destination;
		 private readonly SocketAddress _origin;
		 private readonly Log _msgLog;
		 private ResourcePool<ChannelContext> _channelPool;
		 private readonly Protocol _protocol;
		 private readonly int _frameLength;
		 private readonly long _readTimeout;
		 private readonly int _maxUnusedChannels;
		 private readonly StoreId _storeId;
		 private ResourceReleaser _resourcePoolReleaser;
		 private ComExceptionHandler _comExceptionHandler;
		 private readonly ResponseUnpacker _responseUnpacker;
		 private readonly ByteCounterMonitor _byteCounterMonitor;
		 private readonly RequestMonitor _requestMonitor;
		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _entryReader;

		 public Client( string destinationHostNameOrIp, int destinationPort, string originHostNameOrIp, LogProvider logProvider, StoreId storeId, int frameLength, long readTimeout, int maxConcurrentChannels, int chunkSize, ResponseUnpacker responseUnpacker, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor, LogEntryReader<ReadableClosablePositionAwareChannel> entryReader )
		 {
			  this._entryReader = entryReader;
			  Debug.Assert( byteCounterMonitor != null );
			  Debug.Assert( requestMonitor != null );

			  this._byteCounterMonitor = byteCounterMonitor;
			  this._requestMonitor = requestMonitor;
			  assertChunkSizeIsWithinFrameSize( chunkSize, frameLength );

			  this._msgLog = logProvider.getLog( this.GetType() );
			  this._storeId = storeId;
			  this._frameLength = frameLength;
			  this._readTimeout = readTimeout;
			  // ResourcePool no longer controls max concurrent channels. Use this value for the pool size
			  this._maxUnusedChannels = maxConcurrentChannels;
			  this._comExceptionHandler = NoOpComExceptionHandler;

			  if ( destinationHostNameOrIp.Equals( "0.0.0.0" ) )
			  {
					// So it turns out that on Windows, connecting to 0.0.0.0 when specifying
					// an origin address will not succeed. But since we know we are
					// connecting to ourselves, and that we are listening on everything,
					// replacing with localhost is the proper thing to do.
					this._destination = new InetSocketAddress( LocalAddress, destinationPort );
			  }
			  else
			  {
					// An explicit destination address is always correct
					this._destination = new InetSocketAddress( destinationHostNameOrIp, destinationPort );
			  }

			  if ( string.ReferenceEquals( originHostNameOrIp, null ) || originHostNameOrIp.Equals( "0.0.0.0" ) )
			  {
					_origin = null;
			  }
			  else
			  {
					_origin = new InetSocketAddress( originHostNameOrIp, 0 );
			  }

			  ProtocolVersion protocolVersion = ProtocolVersion;
			  this._protocol = CreateProtocol( chunkSize, protocolVersion.ApplicationProtocol );
			  this._responseUnpacker = responseUnpacker;

			  _msgLog.info( this.GetType().Name + " communication channel created towards " + _destination );
		 }

		 private string LocalAddress
		 {
			 get
			 {
				  try
				  {
						// Null corresponds to localhost
						return InetAddress.getByName( null ).HostAddress;
				  }
				  catch ( UnknownHostException e )
				  {
						// Fetching the localhost address won't throw this exception, so this should never happen, but if it
						// were, then the computer doesn't even have a loopback interface, so crash now rather than later
						throw new AssertionError( e );
				  }
			 }
		 }

		 private ComExceptionHandler NoOpComExceptionHandler
		 {
			 get
			 {
				  return exception =>
				  {
					if ( ComException.TraceHaConnectivity )
					{
						 string noOpComExceptionHandler = "NoOpComExceptionHandler";
						 //noinspection ThrowableResultOfMethodCallIgnored
						 TraceComException( exception, noOpComExceptionHandler );
					}
				  };
			 }
		 }

		 private ComException TraceComException( ComException exception, string tracePoint )
		 {
			  return exception.TraceComException( _msgLog, tracePoint );
		 }

		 protected internal virtual Protocol CreateProtocol( int chunkSize, sbyte applicationProtocolVersion )
		 {
			  return new Protocol320( chunkSize, applicationProtocolVersion, InternalProtocolVersion );
		 }

		 public abstract ProtocolVersion ProtocolVersion { get; }

		 public override void Start()
		 {
			  _bootstrap = new ClientBootstrap( new NioClientSocketChannelFactory( newCachedThreadPool( daemon( this.GetType().Name + "-boss@" + _destination ) ), newCachedThreadPool(daemon(this.GetType().Name + "-worker@" + _destination)) ) );
			  _bootstrap.PipelineFactory = this;

			  _channelPool = new ResourcePoolAnonymousInnerClass( this, _maxUnusedChannels );
			  /*
			   * This is here to couple the channel releasing to Response.close() itself and not
			   * to TransactionStream.close() as it is implemented here. The reason is that a Response
			   * that is returned without a TransactionStream will still hold the channel and should
			   * release it eventually. Also, logically, closing the channel is not dependent on the
			   * TransactionStream.
			   */
			  _resourcePoolReleaser = () =>
			  {
				if ( _channelPool != null )
				{
					 _channelPool.release();
				}
			  };
		 }

		 private class ResourcePoolAnonymousInnerClass : ResourcePool<ChannelContext>
		 {
			 private readonly Client<T> _outerInstance;

			 public ResourcePoolAnonymousInnerClass( Client<T> outerInstance, int maxUnusedChannels ) : base( maxUnusedChannels, new ResourcePool.CheckStrategy_TimeoutCheckStrategy( DEFAULT_CHECK_INTERVAL, Clocks.systemClock() ), new LoggingResourcePoolMonitor(outerInstance.msgLog) )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override ChannelContext create()
			 {
				  _outerInstance.msgLog.info( threadInfo() + "Trying to open a new channel from " + _outerInstance.origin + " to " + _outerInstance.destination );
				  // We must specify the origin address in case the server has multiple IPs per interface
				  ChannelFuture channelFuture = _outerInstance.bootstrap.connect( _outerInstance.destination, _outerInstance.origin );
				  channelFuture.awaitUninterruptibly( 5, TimeUnit.SECONDS );
				  if ( channelFuture.Success )
				  {
						_outerInstance.msgLog.info( threadInfo() + "Opened a new channel from " + channelFuture.Channel.LocalAddress + " to " + channelFuture.Channel.RemoteAddress );

						return new ChannelContext( channelFuture.Channel, ChannelBuffers.dynamicBuffer(), ByteBuffer.allocate(1024 * 1024) );
				  }

				  Exception cause = channelFuture.Cause;
				  string msg = _outerInstance.GetType().Name + " could not connect from " + _outerInstance.origin + " to " + _outerInstance.destination;
				  _outerInstance.msgLog.debug( msg );
				  throw outerInstance.traceComException( new ComException( msg, cause ), "Client.start" );
			 }

			 protected internal override bool isAlive( ChannelContext context )
			 {
				  return context.Channel().Connected;
			 }

			 protected internal override void dispose( ChannelContext context )
			 {
				  Channel channel = context.Channel();
				  if ( channel.Connected )
				  {
						_outerInstance.msgLog.info( threadInfo() + "Closing: " + context + ". " + "Channel pool size is now " + currentSize() );
						channel.close();
				  }
			 }

			 private string threadInfo()
			 {
				  return "Thread[" + Thread.CurrentThread.Id + ", " + Thread.CurrentThread.Name + "] ";
			 }
		 }

		 public override void Stop()
		 {
			  if ( _channelPool != null )
			  {
					_channelPool.close( true );
					_bootstrap.releaseExternalResources();
					_channelPool = null;
			  }

			  _comExceptionHandler = NoOpComExceptionHandler;
			  _msgLog.info( ToString() + " shutdown" );
		 }

		 protected internal virtual Response<R> SendRequest<R>( RequestType type, RequestContext context, Serializer serializer, Deserializer<R> deserializer )
		 {
			  return SendRequest( type, context, serializer, deserializer, null, NO_OP_TX_HANDLER );
		 }

		 protected internal virtual Response<R> SendRequest<R>( RequestType type, RequestContext context, Serializer serializer, Deserializer<R> deserializer, StoreId specificStoreId, ResponseUnpacker_TxHandler txHandler )
		 {
			  ChannelContext channelContext = AcquireChannelContext( type );

			  Exception failure = null;
			  try
			  {
					_requestMonitor.beginRequest( channelContext.Channel().RemoteAddress, type, context );

					// Request
					_protocol.serializeRequest( channelContext.Channel(), channelContext.Output(), type, context, serializer );

					// Response
					Response<R> response = _protocol.deserializeResponse( ExtractBlockingReadHandler( channelContext ), channelContext.Input(), GetReadTimeout(type, _readTimeout), deserializer, _resourcePoolReleaser, _entryReader );

					if ( type.ResponseShouldBeUnpacked() )
					{
						 _responseUnpacker.unpackResponse( response, txHandler );
					}

					if ( ShouldCheckStoreId( type ) )
					{
						 // specificStoreId is there as a workaround for then the graphDb isn't initialized yet
						 if ( specificStoreId != null )
						 {
							  AssertCorrectStoreId( response.StoreId, specificStoreId );
						 }
						 else
						 {
							  AssertCorrectStoreId( response.StoreId, _storeId );
						 }
					}

					return response;
			  }
			  catch ( ComException e )
			  {
					failure = e;
					_comExceptionHandler.handle( e );
					throw TraceComException( e, "Client.sendRequest" );
			  }
			  catch ( Exception e )
			  {
					failure = e;
					throwIfUnchecked( e );
					throw new Exception( e );
			  }
			  finally
			  {
					/*
					 * Otherwise the user must call response.close() to prevent resource leaks.
					 */
					if ( failure != null )
					{
						 Dispose( channelContext );
					}
					_requestMonitor.endRequest( failure );
			  }
		 }

		 protected internal virtual long GetReadTimeout( RequestType type, long readTimeout )
		 {
			  return readTimeout;
		 }

		 protected internal virtual bool ShouldCheckStoreId( RequestType type )
		 {
			  return true;
		 }

		 protected internal virtual StoreId StoreId
		 {
			 get
			 {
				  return _storeId;
			 }
		 }

		 private void AssertCorrectStoreId( StoreId storeId, StoreId myStoreId )
		 {
			  if ( !myStoreId.Equals( storeId ) )
			  {
					throw new MismatchingStoreIdException( myStoreId, storeId );
			  }
		 }

		 private ChannelContext AcquireChannelContext( RequestType type )
		 {
			  if ( _channelPool == null )
			  {
					throw new ComException( string.Format( "Client for {0} is stopped", _destination.ToString() ) );
			  }

			  // Calling acquire is dangerous since it may be a blocking call... and if this
			  // thread holds a lock which others may want to be able to communicate with
			  // the server things go stiff.
			  ChannelContext result = _channelPool.acquire();
			  if ( result == null )
			  {
					_msgLog.error( "Unable to acquire new channel for " + type );
					throw TraceComException( new ComException( "Unable to acquire new channel for " + type ), "Client.acquireChannelContext" );
			  }
			  return result;
		 }

		 private void Dispose( ChannelContext channelContext )
		 {
			  channelContext.Channel().close().awaitUninterruptibly();
			  if ( _channelPool != null )
			  {
					_channelPool.release();
			  }
		 }

		 public override ChannelPipeline Pipeline
		 {
			 get
			 {
				  ChannelPipeline pipeline = Channels.pipeline();
				  pipeline.addLast( MONITORING_CHANNEL_HANDLER_NAME, new MonitorChannelHandler( _byteCounterMonitor ) );
				  addLengthFieldPipes( pipeline, _frameLength );
				  BlockingReadHandler<ChannelBuffer> reader = new BlockingReadHandler<ChannelBuffer>( new ArrayBlockingQueue<>( 100, false ) );
				  pipeline.addLast( BLOCKING_CHANNEL_HANDLER_NAME, reader );
				  return pipeline;
			 }
		 }

		 public virtual ComExceptionHandler ComExceptionHandler
		 {
			 set
			 {
				  _comExceptionHandler = ( value == null ) ? NoOpComExceptionHandler : value;
			 }
		 }

		 protected internal virtual sbyte InternalProtocolVersion
		 {
			 get
			 {
				  return Server.INTERNAL_PROTOCOL_VERSION;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static org.jboss.netty.handler.queue.BlockingReadHandler<org.jboss.netty.buffer.ChannelBuffer> extractBlockingReadHandler(ChannelContext channelContext)
		 private static BlockingReadHandler<ChannelBuffer> ExtractBlockingReadHandler( ChannelContext channelContext )
		 {
			  ChannelPipeline pipeline = channelContext.Channel().Pipeline;
			  return ( BlockingReadHandler<ChannelBuffer> ) pipeline.get( BLOCKING_CHANNEL_HANDLER_NAME );
		 }

		 protected internal static string BeginningOfBufferAsHexString( ChannelBuffer buffer, int maxBytesToPrint )
		 {
			  // read buffer from pos 0 - writeIndex
			  int prevIndex = buffer.readerIndex();
			  buffer.readerIndex( 0 );
			  try
			  {
					MemoryStream byteArrayStream = new MemoryStream( buffer.readableBytes() );
					PrintStream stream = new PrintStream( byteArrayStream );
					HexPrinter printer = ( new HexPrinter( stream ) ).withLineNumberDigits( 4 );
					for ( int i = 0; buffer.readable() && i < maxBytesToPrint; i++ )
					{
						 printer.append( buffer.readByte() );
					}
					stream.flush();
					return byteArrayStream.ToString();
			  }
			  finally
			  {
					buffer.readerIndex( prevIndex );
			  }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + _destination + "]";
		 }
	}

}