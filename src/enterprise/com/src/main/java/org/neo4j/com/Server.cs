using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.com
{
	using ServerBootstrap = org.jboss.netty.bootstrap.ServerBootstrap;
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using ChannelBuffers = org.jboss.netty.buffer.ChannelBuffers;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelHandlerContext = org.jboss.netty.channel.ChannelHandlerContext;
	using ChannelPipeline = org.jboss.netty.channel.ChannelPipeline;
	using ChannelPipelineFactory = org.jboss.netty.channel.ChannelPipelineFactory;
	using ChannelStateEvent = org.jboss.netty.channel.ChannelStateEvent;
	using Channels = org.jboss.netty.channel.Channels;
	using ExceptionEvent = org.jboss.netty.channel.ExceptionEvent;
	using MessageEvent = org.jboss.netty.channel.MessageEvent;
	using SimpleChannelHandler = org.jboss.netty.channel.SimpleChannelHandler;
	using WriteCompletionEvent = org.jboss.netty.channel.WriteCompletionEvent;
	using ChannelGroup = org.jboss.netty.channel.group.ChannelGroup;
	using DefaultChannelGroup = org.jboss.netty.channel.group.DefaultChannelGroup;
	using NioServerSocketChannelFactory = org.jboss.netty.channel.socket.nio.NioServerSocketChannelFactory;


	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Neo4Net.Helpers.Collections;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.DechunkingChannelBuffer.assertSameProtocolVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.addLengthFieldPipes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.Protocol.assertChunkSizeIsWithinFrameSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.NamedThreadFactory.daemon;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.NamedThreadFactory.named;

	/// <summary>
	/// Receives requests from <seealso cref="Client clients"/>. Delegates actual work to an instance
	/// of a specified communication interface, injected in the constructor.
	/// <para>
	/// frameLength vs. chunkSize: frameLength is the maximum and hardcoded size in each
	/// Netty buffer created by this server and handed off to a <seealso cref="Client"/>. If the
	/// client has got a smaller frameLength than this server it will fail on reading a frame
	/// that is bigger than what its frameLength.
	/// chunkSize is the max size a buffer will have before it's sent off and a new buffer
	/// allocated to continue writing to.
	/// frameLength should be a constant for an implementation and must have the same value
	/// on server as well as clients connecting to that server, whereas chunkSize very well
	/// can be configurable and vary between server and client.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= Client </seealso>
	public abstract class Server<T, R> : SimpleChannelHandler, ChannelPipelineFactory, Lifecycle, ChannelCloser
	{

		 private readonly LogProvider _logProvider;
		 private ScheduledExecutorService _silentChannelExecutor;

		 public interface Configuration
		 {
			  long OldChannelThreshold { get; }

			  int MaxConcurrentTransactions { get; }

			  int ChunkSize { get; }

			  HostnamePort ServerAddress { get; }
		 }

		 // It's ok if there are more transactions, since these worker threads doesn't
		 // do any actual work themselves, but spawn off other worker threads doing the
		 // actual work. So this is more like a core Netty I/O pool worker size.
		 public const int DEFAULT_MAX_NUMBER_OF_CONCURRENT_TRANSACTIONS = 200;
		 internal const sbyte INTERNAL_PROTOCOL_VERSION = 2;
		 private readonly T _requestTarget;
		 private readonly IdleChannelReaper _connectedSlaveChannels;
		 private readonly Log _msgLog;
		 private readonly IDictionary<Channel, PartialRequest> _partialRequests = new ConcurrentDictionary<Channel, PartialRequest>();
		 private readonly Configuration _config;
		 private readonly int _frameLength;
		 private readonly ByteCounterMonitor _byteCounterMonitor;
		 private readonly RequestMonitor _requestMonitor;
		 private readonly sbyte _applicationProtocolVersion;
		 private readonly TxChecksumVerifier _txVerifier;
		 private ServerBootstrap _bootstrap;
		 private ChannelGroup _channelGroup;
		 private ExecutorService _targetCallExecutor;
		 private volatile bool _shuttingDown;
		 private InetSocketAddress _socketAddress;
		 // Executor for channels that we know should be finished, but can't due to being
		 // active at the moment.
		 private ExecutorService _unfinishedTransactionExecutor;
		 private int _chunkSize;

		 public Server( T requestTarget, Configuration config, LogProvider logProvider, int frameLength, ProtocolVersion protocolVersion, TxChecksumVerifier txVerifier, Clock clock, ByteCounterMonitor byteCounterMonitor, RequestMonitor requestMonitor )
		 {
			  this._requestTarget = requestTarget;
			  this._config = config;
			  this._frameLength = frameLength;
			  this._applicationProtocolVersion = protocolVersion.ApplicationProtocol;
			  this._logProvider = logProvider;
			  this._msgLog = this._logProvider.getLog( this.GetType() );
			  this._txVerifier = txVerifier;
			  this._byteCounterMonitor = byteCounterMonitor;
			  this._requestMonitor = requestMonitor;
			  this._connectedSlaveChannels = new IdleChannelReaper( this, logProvider, clock, config.OldChannelThreshold );
			  this._chunkSize = config.ChunkSize;
			  assertChunkSizeIsWithinFrameSize( _chunkSize, frameLength );
		 }

		 private static void WriteStoreId( StoreId storeId, ChannelBuffer targetBuffer )
		 {
			  targetBuffer.writeLong( storeId.CreationTime );
			  targetBuffer.writeLong( storeId.RandomId );
			  targetBuffer.writeLong( storeId.StoreVersion );
			  targetBuffer.writeLong( storeId.UpgradeTime );
			  targetBuffer.writeLong( storeId.UpgradeId );
		 }

		 public override void Init()
		 {
			  _chunkSize = _config.ChunkSize;
			  assertChunkSizeIsWithinFrameSize( _chunkSize, _frameLength );

			  string className = this.GetType().Name;

			  _targetCallExecutor = newCachedThreadPool( named( className + ":" + _config.ServerAddress.Port ) );
			  _unfinishedTransactionExecutor = newScheduledThreadPool( 2, named( "Unfinished transactions" ) );
			  _silentChannelExecutor = newSingleThreadScheduledExecutor( named( "Silent channel reaper" ) );
			  _silentChannelExecutor.scheduleWithFixedDelay( _connectedSlaveChannels, 5, 5, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  string className = this.GetType().Name;
			  ExecutorService bossExecutor = newCachedThreadPool( daemon( "Boss-" + className ) );
			  ExecutorService workerExecutor = newCachedThreadPool( daemon( "Worker-" + className ) );
			  _bootstrap = new ServerBootstrap( new NioServerSocketChannelFactory( bossExecutor, workerExecutor, _config.MaxConcurrentTransactions ) );
			  _bootstrap.PipelineFactory = this;

			  PortRangeSocketBinder portRangeSocketBinder = new PortRangeSocketBinder( _bootstrap );
			  try
			  {
					Connection connection = portRangeSocketBinder.BindToFirstAvailablePortInRange( _config.ServerAddress );
					Channel channel = connection.Channel;
					_socketAddress = connection.SocketAddress;

					_channelGroup = new DefaultChannelGroup();
					_channelGroup.add( channel );
					_msgLog.info( className + " communication server started and bound to " + _socketAddress );
			  }
			  catch ( Exception ex )
			  {
					_msgLog.error( "Failed to bind server to " + _socketAddress, ex );
					_bootstrap.releaseExternalResources();
					_targetCallExecutor.shutdownNow();
					_unfinishedTransactionExecutor.shutdownNow();
					_silentChannelExecutor.shutdownNow();
					throw new IOException( ex );
			  }
		 }

		 public override void Stop()
		 {
			  string name = this.GetType().Name;
			  _msgLog.info( name + " communication server shutting down and unbinding from  " + _socketAddress );

			  _shuttingDown = true;
			  _channelGroup.close().awaitUninterruptibly();
			  _bootstrap.releaseExternalResources();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  _targetCallExecutor.shutdown();
			  _targetCallExecutor.awaitTermination( 10, TimeUnit.SECONDS );
			  _unfinishedTransactionExecutor.shutdown();
			  _unfinishedTransactionExecutor.awaitTermination( 10, TimeUnit.SECONDS );
			  _silentChannelExecutor.shutdown();
			  _silentChannelExecutor.awaitTermination( 10, TimeUnit.SECONDS );
		 }

		 public virtual InetSocketAddress SocketAddress
		 {
			 get
			 {
				  return _socketAddress;
			 }
		 }

		 /// <summary>
		 /// Only exposed so that tests can control it. It's not configurable really.
		 /// </summary>
		 protected internal virtual sbyte InternalProtocolVersion
		 {
			 get
			 {
				  return INTERNAL_PROTOCOL_VERSION;
			 }
		 }

		 public override ChannelPipeline Pipeline
		 {
			 get
			 {
				  ChannelPipeline pipeline = Channels.pipeline();
				  pipeline.addLast( "monitor", new MonitorChannelHandler( _byteCounterMonitor ) );
				  addLengthFieldPipes( pipeline, _frameLength );
				  pipeline.addLast( "serverHandler", this );
				  return pipeline;
			 }
		 }

		 public override void ChannelOpen( ChannelHandlerContext ctx, ChannelStateEvent e )
		 {
			  _channelGroup.add( e.Channel );
		 }

		 public override void MessageReceived( ChannelHandlerContext ctx, MessageEvent @event )
		 {
			  try
			  {
					ChannelBuffer message = ( ChannelBuffer ) @event.Message;
					HandleRequest( message, @event.Channel );
			  }
			  catch ( Exception e )
			  {
					_msgLog.error( "Error handling request", e );

					// Attempt to reply to the client
					ChunkingChannelBuffer buffer = NewChunkingBuffer( @event.Channel );
					buffer.Clear( true );
					WriteFailureResponse( e, buffer );

					ctx.Channel.close();
					TryToCloseChannel( ctx.Channel );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeComplete(org.jboss.netty.channel.ChannelHandlerContext ctx, org.jboss.netty.channel.WriteCompletionEvent e) throws Exception
		 public override void WriteComplete( ChannelHandlerContext ctx, WriteCompletionEvent e )
		 {
			  /*
			   * This is here to ensure that channels that have stuff written to them for a long time, long transaction
			   * pulls and store copies (mainly the latter), will not timeout and have their transactions rolled back.
			   * This is actually not a problem, since both mentioned above have no transaction associated with them
			   * but it is more sanitary and leaves less exceptions in the logs
			   * Each time a write completes, simply update the corresponding channel's timestamp.
			   */
			  if ( _connectedSlaveChannels.update( ctx.Channel ) )
			  {
					base.WriteComplete( ctx, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void channelClosed(org.jboss.netty.channel.ChannelHandlerContext ctx, org.jboss.netty.channel.ChannelStateEvent e) throws Exception
		 public override void ChannelClosed( ChannelHandlerContext ctx, ChannelStateEvent e )
		 {
			  base.ChannelClosed( ctx, e );

			  if ( !ctx.Channel.Open )
			  {
					TryToCloseChannel( ctx.Channel );
			  }

			  _channelGroup.remove( e.Channel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void channelDisconnected(org.jboss.netty.channel.ChannelHandlerContext ctx, org.jboss.netty.channel.ChannelStateEvent e) throws Exception
		 public override void ChannelDisconnected( ChannelHandlerContext ctx, ChannelStateEvent e )
		 {
			  base.ChannelDisconnected( ctx, e );

			  if ( !ctx.Channel.Connected )
			  {
					TryToCloseChannel( ctx.Channel );
			  }
		 }

		 public override void ExceptionCaught( ChannelHandlerContext ctx, ExceptionEvent e )
		 {
			  _msgLog.warn( "Exception from Netty", e.Cause );
		 }

		 public override void TryToCloseChannel( Channel channel )
		 {
			  IdleChannelReaper.Request request = UnmapSlave( channel );
			  if ( request == null )
			  {
					return;
			  }
			  TryToFinishOffChannel( channel, request.RequestContext );
		 }

		 protected internal virtual void TryToFinishOffChannel( Channel channel, RequestContext slave )
		 {
			  try
			  {
					StopConversation( slave );
					UnmapSlave( channel );
			  }
			  catch ( Exception failure ) // Unknown error trying to finish off the tx
			  {
					SubmitSilent( _unfinishedTransactionExecutor, NewTransactionFinisher( slave ) );
					_msgLog.warn( "Could not finish off dead channel", failure );
			  }
		 }

		 private void SubmitSilent( ExecutorService service, ThreadStart job )
		 {
			  try
			  {
					service.submit( job );
			  }
			  catch ( RejectedExecutionException e )
			  { // Don't scream and shout if we're shutting down, because a rejected execution
					// is expected at that time.
					if ( !_shuttingDown )
					{
						 throw e;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable newTransactionFinisher(final RequestContext slave)
		 private ThreadStart NewTransactionFinisher( RequestContext slave )
		 {
			  return new RunnableAnonymousInnerClass( this, slave );
		 }

		 private class RunnableAnonymousInnerClass : ThreadStart
		 {
			 private readonly Server<T, R> _outerInstance;

			 private Neo4Net.com.RequestContext _slave;

			 public RunnableAnonymousInnerClass( Server<T, R> outerInstance, Neo4Net.com.RequestContext slave )
			 {
				 this.outerInstance = outerInstance;
				 this._slave = slave;
			 }

			 public void run()
			 {
				  try
				  {
						outerInstance.StopConversation( _slave );
				  }
				  catch ( Exception )
				  {
						// Introduce some delay here. it becomes like a busy wait if it never succeeds
						sleepNicely( 200 );
						_outerInstance.unfinishedTransactionExecutor.submit( this );
				  }
			 }

			 private void sleepNicely( int millis )
			 {
				  try
				  {
						Thread.Sleep( millis );
				  }
				  catch ( InterruptedException )
				  {
						Thread.interrupted();
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected void handleRequest(org.jboss.netty.buffer.ChannelBuffer buffer, final org.jboss.netty.channel.Channel channel)
		 protected internal virtual void HandleRequest( ChannelBuffer buffer, Channel channel )
		 {
			  sbyte? continuation = ReadContinuationHeader( buffer, channel );
			  if ( continuation == null )
			  {
					return;
			  }
			  if ( continuation.Value == ChunkingChannelBuffer.CONTINUATION_MORE )
			  {
					PartialRequest partialRequest = _partialRequests[channel];
					if ( partialRequest == null )
					{
						 // This is the first chunk in a multi-chunk request
						 RequestType type = GetRequestContext( buffer.readByte() );
						 RequestContext context = ReadContext( buffer );
						 ChannelBuffer targetBuffer = MapSlave( channel, context );
						 partialRequest = new PartialRequest( this, type, context, targetBuffer );
						 _partialRequests[channel] = partialRequest;
					}
					partialRequest.Add( buffer );
			  }
			  else
			  {
					PartialRequest partialRequest = _partialRequests.Remove( channel );
					RequestType type;
					RequestContext context;
					ChannelBuffer targetBuffer;
					ChannelBuffer bufferToReadFrom;
					ChannelBuffer bufferToWriteTo;
					if ( partialRequest == null )
					{
						 // This is the one and single chunk in the request
						 type = GetRequestContext( buffer.readByte() );
						 context = ReadContext( buffer );
						 targetBuffer = MapSlave( channel, context );
						 bufferToReadFrom = buffer;
						 bufferToWriteTo = targetBuffer;
					}
					else
					{
						 // This is the last chunk in a multi-chunk request
						 type = partialRequest.Type;
						 context = partialRequest.Context;
						 targetBuffer = partialRequest.Buffer;
						 partialRequest.Add( buffer );
						 bufferToReadFrom = targetBuffer;
						 bufferToWriteTo = ChannelBuffers.dynamicBuffer();
					}

					bufferToWriteTo.clear();
					ChunkingChannelBuffer chunkingBuffer = NewChunkingBuffer( bufferToWriteTo, channel, _chunkSize, InternalProtocolVersion, _applicationProtocolVersion );
					SubmitSilent( _targetCallExecutor, new TargetCaller( this, type, channel, context, chunkingBuffer, bufferToReadFrom ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Nullable<sbyte> readContinuationHeader(org.jboss.netty.buffer.ChannelBuffer buffer, final org.jboss.netty.channel.Channel channel)
		 private sbyte? ReadContinuationHeader( ChannelBuffer buffer, Channel channel )
		 {
			  sbyte[] header = new sbyte[2];
			  buffer.readBytes( header );
			  try
			  { // Read request header and assert correct internal/application protocol version
					assertSameProtocolVersion( header, InternalProtocolVersion, _applicationProtocolVersion );
			  }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final IllegalProtocolVersionException e)
			  catch ( IllegalProtocolVersionException e )
			  { // Version mismatch, fail with a good exception back to the client
					SubmitSilent( _targetCallExecutor, () => writeFailureResponse(e, NewChunkingBuffer(channel)) );
					return null;
			  }
			  return ( sbyte )( header[0] & 0x1 );
		 }

		 protected internal virtual void WriteFailureResponse( Exception exception, ChunkingChannelBuffer buffer )
		 {
			  try
			  {
					MemoryStream bytes = new MemoryStream();
					ObjectOutputStream @out = new ObjectOutputStream( bytes );
					@out.writeObject( exception );
					@out.close();
					buffer.WriteBytes( bytes.toByteArray() );
					buffer.Done();
			  }
			  catch ( IOException )
			  {
					_msgLog.warn( "Couldn't send cause of error to client", exception );
			  }
		 }

		 protected internal virtual void ResponseWritten( RequestType type, Channel channel, RequestContext context )
		 {
		 }

		 protected internal virtual RequestContext ReadContext( ChannelBuffer buffer )
		 {
			  long sessionId = buffer.readLong();
			  int machineId = buffer.readInt();
			  int eventIdentifier = buffer.readInt();
			  long neoTx = buffer.readLong();
			  long checksum = buffer.readLong();

			  RequestContext readRequestContext = new RequestContext( sessionId, machineId, eventIdentifier, neoTx, checksum );

			  // verify checksum only if there are transactions committed in the store
			  if ( neoTx > Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID )
			  {
					_txVerifier.assertMatch( neoTx, checksum );
			  }
			  return readRequestContext;
		 }

		 protected internal abstract RequestType GetRequestContext( sbyte id );

		 protected internal virtual ChannelBuffer MapSlave( Channel channel, RequestContext slave )
		 {
			  // Checking for machineId -1 excludes the "empty" slave contexts
			  // which some communication points pass in as context.
			  if ( slave != null && slave.MachineId() != RequestContext.Empty.machineId() )
			  {
					_connectedSlaveChannels.add( channel, slave );
			  }
			  return ChannelBuffers.dynamicBuffer();
		 }

		 protected internal virtual IdleChannelReaper.Request UnmapSlave( Channel channel )
		 {
			  return _connectedSlaveChannels.remove( channel );
		 }

		 protected internal virtual T RequestTarget
		 {
			 get
			 {
				  return _requestTarget;
			 }
		 }

		 protected internal abstract void StopConversation( RequestContext context );

		 private ChunkingChannelBuffer NewChunkingBuffer( Channel channel )
		 {
			  return NewChunkingBuffer( ChannelBuffers.dynamicBuffer(), channel, _chunkSize, InternalProtocolVersion, _applicationProtocolVersion );
		 }

		 protected internal virtual ChunkingChannelBuffer NewChunkingBuffer( ChannelBuffer bufferToWriteTo, Channel channel, int capacity, sbyte internalProtocolVersion, sbyte applicationProtocolVersion )
		 {
			  return new ChunkingChannelBuffer( bufferToWriteTo, channel, capacity, internalProtocolVersion, applicationProtocolVersion );
		 }

		 private class TargetCaller : Response.Handler, ThreadStart
		 {
			 private readonly Server<T, R> _outerInstance;

			  internal readonly RequestType Type;
			  internal readonly Channel Channel;
			  internal readonly RequestContext Context;
			  internal readonly ChunkingChannelBuffer TargetBuffer;
			  internal readonly ChannelBuffer BufferToReadFrom;

			  internal TargetCaller( Server<T, R> outerInstance, RequestType type, Channel channel, RequestContext context, ChunkingChannelBuffer targetBuffer, ChannelBuffer bufferToReadFrom )
			  {
				  this._outerInstance = outerInstance;
					this.Type = type;
					this.Channel = channel;
					this.Context = context;
					this.TargetBuffer = targetBuffer;
					this.BufferToReadFrom = bufferToReadFrom;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public void run()
			  public override void Run()
			  {
					outerInstance.requestMonitor.BeginRequest( Channel.RemoteAddress, Type, Context );
					Response<R> response = null;
					Exception failure = null;
					try
					{
						 outerInstance.UnmapSlave( Channel );
						 response = Type.TargetCaller.call( outerInstance.requestTarget, Context, BufferToReadFrom, TargetBuffer );
						 Type.ObjectSerializer.write( response.ResponseConflict(), TargetBuffer );
						 WriteStoreId( response.StoreId, TargetBuffer );
						 response.Accept( this );
						 TargetBuffer.done();
						 outerInstance.ResponseWritten( Type, Channel, Context );
					}
					catch ( Exception e )
					{
						 failure = e;
						 TargetBuffer.clear( true );
						 outerInstance.WriteFailureResponse( e, TargetBuffer );
						 outerInstance.TryToFinishOffChannel( Channel, Context );
						 throw new Exception( e );
					}
					finally
					{
						 if ( response != null )
						 {
							  response.Close();
						 }
						 outerInstance.requestMonitor.EndRequest( failure );
					}
			  }

			  public override void Obligation( long txId )
			  {
					TargetBuffer.writeByte( -1 );
					TargetBuffer.writeLong( txId );
			  }

			  public override Visitor<CommittedTransactionRepresentation, Exception> Transactions()
			  {
					TargetBuffer.writeByte( 1 );
					return new CommittedTransactionSerializer( new NetworkFlushableChannel( TargetBuffer ) );
			  }
		 }

		 private class PartialRequest
		 {
			 private readonly Server<T, R> _outerInstance;

			  internal readonly RequestContext Context;
			  internal readonly ChannelBuffer Buffer;
			  internal readonly RequestType Type;

			  internal PartialRequest( Server<T, R> outerInstance, RequestType type, RequestContext context, ChannelBuffer buffer )
			  {
				  this._outerInstance = outerInstance;
					this.Type = type;
					this.Context = context;
					this.Buffer = buffer;
			  }

			  public virtual void Add( ChannelBuffer buffer )
			  {
					this.Buffer.writeBytes( buffer );
			  }
		 }
	}

}