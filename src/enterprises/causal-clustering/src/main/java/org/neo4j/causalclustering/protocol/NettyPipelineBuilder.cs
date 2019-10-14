using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.causalclustering.protocol
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelDuplexHandler = io.netty.channel.ChannelDuplexHandler;
	using ChannelFutureListener = io.netty.channel.ChannelFutureListener;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelOutboundHandlerAdapter = io.netty.channel.ChannelOutboundHandlerAdapter;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using ChannelPromise = io.netty.channel.ChannelPromise;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;


	using MessageGate = Neo4Net.causalclustering.messaging.MessageGate;
	using Neo4Net.Functions;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	/// <summary>
	/// Builder and installer of pipelines.
	/// <para>
	/// Makes sures to install sane last-resort error handling and
	/// handles the construction of common patterns, like framing.
	/// </para>
	/// <para>
	/// Do not modify the names of handlers you install.
	/// </para>
	/// </summary>
	public abstract class NettyPipelineBuilder<O, BUILDER> where O : ProtocolInstaller_Orientation where BUILDER : NettyPipelineBuilder<O, BUILDER>
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_self = ( BUILDER ) this;
		}

		 internal const string MESSAGE_GATE_NAME = "message_gate";
		 internal const string ERROR_HANDLER_TAIL = "error_handler_tail";
		 internal const string ERROR_HANDLER_HEAD = "error_handler_head";

		 private readonly ChannelPipeline _pipeline;
		 private readonly Log _log;
		 private readonly IList<HandlerInfo> _handlerInfos = new List<HandlerInfo>();

		 private System.Predicate<object> _gatePredicate;
		 private ThreadStart _closeHandler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private BUILDER self = (BUILDER) this;
		 private BUILDER _self;

		 internal NettyPipelineBuilder( ChannelPipeline pipeline, Log log )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._pipeline = pipeline;
			  this._log = log;
		 }

		 /// <summary>
		 /// Entry point for the client builder.
		 /// </summary>
		 /// <param name="pipeline"> The pipeline to build for. </param>
		 /// <param name="log"> The log used for last-resort errors occurring in the pipeline. </param>
		 /// <returns> The client builder. </returns>
		 public static ClientNettyPipelineBuilder Client( ChannelPipeline pipeline, Log log )
		 {
			  return new ClientNettyPipelineBuilder( pipeline, log );
		 }

		 /// <summary>
		 /// Entry point for the server builder.
		 /// </summary>
		 /// <param name="pipeline"> The pipeline to build for. </param>
		 /// <param name="log"> The log used for last-resort errors occurring in the pipeline. </param>
		 /// <returns> The server builder. </returns>
		 public static ServerNettyPipelineBuilder Server( ChannelPipeline pipeline, Log log )
		 {
			  return new ServerNettyPipelineBuilder( pipeline, log );
		 }

		 /// <summary>
		 /// Adds buffer framing to the pipeline. Useful for pipelines marshalling
		 /// complete POJOs as an example using <seealso cref="MessageToByteEncoder"/> and
		 /// <seealso cref="ByteToMessageDecoder"/>.
		 /// </summary>
		 public abstract BUILDER AddFraming();

		 public virtual BUILDER Modify( ModifierProtocolInstaller<O> modifier )
		 {
			  modifier.Apply( this );
			  return _self;
		 }

		 public virtual BUILDER Modify( IList<ModifierProtocolInstaller<O>> modifiers )
		 {
			  modifiers.ForEach( this.modify );
			  return _self;
		 }

		 /// <summary>
		 /// Adds handlers to the pipeline.
		 /// <para>
		 /// The pipeline builder controls the internal names of the handlers in the
		 /// pipeline and external actors are forbidden from manipulating them.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="name"> The name of the handler, which must be unique. </param>
		 /// <param name="newHandlers"> The new handlers. </param>
		 /// <returns> The builder. </returns>
		 public virtual BUILDER Add( string name, IList<ChannelHandler> newHandlers )
		 {
			  newHandlers.Select( handler => new HandlerInfo( name, handler ) ).forEachOrdered( _handlerInfos.add );
			  return _self;
		 }

		 /// <seealso cref= #add(String, List) </seealso>
		 public virtual BUILDER Add( string name, params ChannelHandler[] newHandlers )
		 {
			  return Add( name, new IList<ChannelHandler> { newHandlers } );
		 }

		 public virtual BUILDER AddGate( System.Predicate<object> gatePredicate )
		 {
			  if ( this._gatePredicate != null )
			  {
					throw new System.InvalidOperationException( "Cannot have more than one gate." );
			  }
			  this._gatePredicate = gatePredicate;
			  return _self;
		 }

		 public virtual BUILDER OnClose( ThreadStart closeHandler )
		 {
			  if ( this._closeHandler != null )
			  {
					throw new System.InvalidOperationException( "Cannot have more than one close handler." );
			  }
			  this._closeHandler = closeHandler;
			  return _self;
		 }

		 /// <summary>
		 /// Installs the built pipeline and removes any old pipeline.
		 /// </summary>
		 public virtual void Install()
		 {
			  EnsureErrorHandling();
			  InstallGate();
			  ClearUserHandlers();

			  string userHead = ERROR_HANDLER_HEAD;
			  foreach ( HandlerInfo info in _handlerInfos )
			  {
					_pipeline.addAfter( userHead, info.Name, info.Handler );
					userHead = info.Name;
			  }
		 }

		 private void InstallGate()
		 {
			  if ( _pipeline.get( MESSAGE_GATE_NAME ) != null && _gatePredicate != null )
			  {
					throw new System.InvalidOperationException( "Cannot have more than one gate." );
			  }
			  else if ( _gatePredicate != null )
			  {
					_pipeline.addBefore( ERROR_HANDLER_TAIL, MESSAGE_GATE_NAME, new MessageGate( _gatePredicate ) );
			  }
		 }

		 private void ClearUserHandlers()
		 {
			  _pipeline.names().Where(this.isNotDefault).Where(this.isNotErrorHandler).Where(this.isNotGate).ForEach(_pipeline.remove);
		 }

		 private bool IsNotDefault( string name )
		 {
			  // these are netty internal handlers for head and tail
			  return _pipeline.get( name ) != null;
		 }

		 private bool IsNotErrorHandler( string name )
		 {
			  return !name.Equals( ERROR_HANDLER_HEAD ) && !name.Equals( ERROR_HANDLER_TAIL );
		 }

		 private bool IsNotGate( string name )
		 {
			  return !name.Equals( MESSAGE_GATE_NAME );
		 }

		 private void EnsureErrorHandling()
		 {
			  int size = _pipeline.names().size();

			  if ( _pipeline.names().get(0).Equals(ERROR_HANDLER_HEAD) )
			  {
					if ( !_pipeline.names().get(size - 2).Equals(ERROR_HANDLER_TAIL) ) // last position before netty's tail sentinel
					{
						 throw new System.InvalidOperationException( "Both error handlers must exist." );
					}
					return;
			  }

			  // inbound goes in the direction from first->last
			  _pipeline.addLast( ERROR_HANDLER_TAIL, new ChannelDuplexHandlerAnonymousInnerClass( this ) );

			  _pipeline.addFirst( ERROR_HANDLER_HEAD, new ChannelOutboundHandlerAdapterAnonymousInnerClass( this ) );
		 }

		 private class ChannelDuplexHandlerAnonymousInnerClass : ChannelDuplexHandler
		 {
			 private readonly NettyPipelineBuilder<O, BUILDER> _outerInstance;

			 public ChannelDuplexHandlerAnonymousInnerClass( NettyPipelineBuilder<O, BUILDER> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void exceptionCaught( ChannelHandlerContext ctx, Exception cause )
			 {
				  _outerInstance.swallow( () => _outerInstance.log.error(format("Exception in inbound for channel: %s", ctx.channel()), cause) );
				  outerInstance.swallow( ctx.close );
			 }

			 public override void channelRead( ChannelHandlerContext ctx, object msg )
			 {
				  _outerInstance.log.error( "Unhandled inbound message: %s for channel: %s", msg, ctx.channel() );
				  ctx.close();
			 }

			 // this is the first handler for an outbound message, and attaches a listener to its promise if possible
			 public override void write( ChannelHandlerContext ctx, object msg, ChannelPromise promise )
			 {
				  // if the promise is a void-promise, then exceptions will instead propagate to the
				  // exceptionCaught handler on the outbound handler further below

				  if ( !promise.Void )
				  {
						promise.addListener((ChannelFutureListener) future =>
						{
						if ( !future.Success )
						{
							_outerInstance.swallow( () => _outerInstance.log.error(format("Exception in outbound for channel: %s", future.channel()), future.cause()) );
							outerInstance.swallow( ctx.close );
						}
						});
				  }
				  ctx.write( msg, promise );
			 }

			 public override void channelInactive( ChannelHandlerContext ctx )
			 {
				  if ( _outerInstance.closeHandler != null )
				  {
						_outerInstance.closeHandler.run();
				  }
				  ctx.fireChannelInactive();
			 }
		 }

		 private class ChannelOutboundHandlerAdapterAnonymousInnerClass : ChannelOutboundHandlerAdapter
		 {
			 private readonly NettyPipelineBuilder<O, BUILDER> _outerInstance;

			 public ChannelOutboundHandlerAdapterAnonymousInnerClass( NettyPipelineBuilder<O, BUILDER> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

						 // exceptions which did not get fulfilled on the promise of a write, etc.
			 public override void exceptionCaught( ChannelHandlerContext ctx, Exception cause )
			 {
				  _outerInstance.swallow( () => _outerInstance.log.error(format("Exception in outbound for channel: %s", ctx.channel()), cause) );
				  outerInstance.swallow( ctx.close );
			 }

			 // netty can only handle bytes in the form of ByteBuf, so if you reach this then you are
			 // perhaps trying to send a POJO without having a suitable encoder
			 public override void write( ChannelHandlerContext ctx, object msg, ChannelPromise promise )
			 {
				  if ( !( msg is ByteBuf ) )
				  {
						_outerInstance.log.error( "Unhandled outbound message: %s for channel: %s", msg, ctx.channel() );
						ctx.close();
				  }
				  else
				  {
						ctx.write( msg, promise );
				  }
			 }
		 }

		 /// <summary>
		 /// An throwable-swallowing execution of an action. Used in last-resort exception handlers.
		 /// </summary>
		 private void Swallow( ThrowingAction<Exception> action )
		 {
			  try
			  {
					action.Apply();
			  }
			  catch ( Exception )
			  {
			  }
		 }

		 private class HandlerInfo
		 {
			  internal readonly string Name;
			  internal readonly ChannelHandler Handler;

			  internal HandlerInfo( string name, ChannelHandler handler )
			  {
					this.Name = name;
					this.Handler = handler;
			  }
		 }
	}

}