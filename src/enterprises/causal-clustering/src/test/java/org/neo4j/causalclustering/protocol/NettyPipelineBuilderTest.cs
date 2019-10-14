using System;
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
namespace Neo4Net.causalclustering.protocol
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerAdapter = io.netty.channel.ChannelHandlerAdapter;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ChannelOutboundHandlerAdapter = io.netty.channel.ChannelOutboundHandlerAdapter;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using ChannelPromise = io.netty.channel.ChannelPromise;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Test = org.junit.Test;


	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class NettyPipelineBuilderTest
	{
		private bool InstanceFieldsInitialized = false;

		public NettyPipelineBuilderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_log = _logProvider.getLog( this.GetType() );
		}

		 private AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private Log _log;
		 private EmbeddedChannel _channel = new EmbeddedChannel();
		 private ChannelHandlerAdapter EMPTY_HANDLER = new ChannelHandlerAdapterAnonymousInnerClass();

		 private class ChannelHandlerAdapterAnonymousInnerClass : ChannelHandlerAdapter
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogExceptionInbound()
		 public virtual void ShouldLogExceptionInbound()
		 {
			  // given
			  Exception ex = new Exception();
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).add("read_handler", new ChannelInboundHandlerAdapterAnonymousInnerClass(this, ex)) // whenchannel.writeOneInbound(new object());
			 .install();

			  // then
			  _logProvider.assertExactly( inLog( this.GetType() ).error(startsWith("Exception in inbound"), equalTo(ex)) );
			  assertFalse( _channel.Open );
		 }

		 private class ChannelInboundHandlerAdapterAnonymousInnerClass : ChannelInboundHandlerAdapter
		 {
			 private readonly NettyPipelineBuilderTest _outerInstance;

			 private Exception _ex;

			 public ChannelInboundHandlerAdapterAnonymousInnerClass( NettyPipelineBuilderTest outerInstance, Exception ex )
			 {
				 this.outerInstance = outerInstance;
				 this._ex = ex;
			 }

			 public override void channelRead( ChannelHandlerContext ctx, object msg )
			 {
				  throw _ex;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnhandledMessageInbound()
		 public virtual void ShouldLogUnhandledMessageInbound()
		 {
			  // given
			  object msg = new object();
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).install();

			  // when
			  _channel.writeOneInbound( msg );

			  // then
			  _logProvider.assertExactly( inLog( this.GetType() ).error(equalTo("Unhandled inbound message: %s for channel: %s"), equalTo(msg), any(typeof(Channel))) );
			  assertFalse( _channel.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnhandledMessageOutbound()
		 public virtual void ShouldLogUnhandledMessageOutbound()
		 {
			  // given
			  object msg = new object();
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).install();

			  // when
			  _channel.writeAndFlush( msg );

			  // then
			  _logProvider.assertExactly( inLog( this.GetType() ).error(equalTo("Unhandled outbound message: %s for channel: %s"), equalTo(msg), any(typeof(Channel))) );
			  assertFalse( _channel.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogExceptionOutbound()
		 public virtual void ShouldLogExceptionOutbound()
		 {
			  Exception ex = new Exception();
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).add("write_handler", new ChannelOutboundHandlerAdapterAnonymousInnerClass(this, ex)) // whenchannel.writeAndFlush(new object());
			 .install();

			  // then
			  _logProvider.assertExactly( inLog( this.GetType() ).error(startsWith("Exception in outbound"), equalTo(ex)) );
			  assertFalse( _channel.Open );
		 }

		 private class ChannelOutboundHandlerAdapterAnonymousInnerClass : ChannelOutboundHandlerAdapter
		 {
			 private readonly NettyPipelineBuilderTest _outerInstance;

			 private Exception _ex;

			 public ChannelOutboundHandlerAdapterAnonymousInnerClass( NettyPipelineBuilderTest outerInstance, Exception ex )
			 {
				 this.outerInstance = outerInstance;
				 this._ex = ex;
			 }

			 public override void write( ChannelHandlerContext ctx, object msg, ChannelPromise promise )
			 {
				  throw _ex;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogExceptionOutboundWithVoidPromise()
		 public virtual void ShouldLogExceptionOutboundWithVoidPromise()
		 {
			  Exception ex = new Exception();
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).add("write_handler", new ChannelOutboundHandlerAdapterAnonymousInnerClass2(this, ex)) // whenchannel.writeAndFlush(new object(), _channel.voidPromise());
			 .install();

			  // then
			  _logProvider.assertExactly( inLog( this.GetType() ).error(startsWith("Exception in outbound"), equalTo(ex)) );
			  assertFalse( _channel.Open );
		 }

		 private class ChannelOutboundHandlerAdapterAnonymousInnerClass2 : ChannelOutboundHandlerAdapter
		 {
			 private readonly NettyPipelineBuilderTest _outerInstance;

			 private Exception _ex;

			 public ChannelOutboundHandlerAdapterAnonymousInnerClass2( NettyPipelineBuilderTest outerInstance, Exception ex )
			 {
				 this.outerInstance = outerInstance;
				 this._ex = ex;
			 }

			 public override void write( ChannelHandlerContext ctx, object msg, ChannelPromise promise )
			 {
				  throw _ex;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogAnythingForHandledInbound()
		 public virtual void ShouldNotLogAnythingForHandledInbound()
		 {
			  // given
			  object msg = new object();
			  ChannelInboundHandlerAdapter handler = new ChannelInboundHandlerAdapterAnonymousInnerClass2( this, msg );
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).add("read_handler", handler).install();

			  // when
			  _channel.writeOneInbound( msg );

			  // then
			  _logProvider.assertNoLoggingOccurred();
		 }

		 private class ChannelInboundHandlerAdapterAnonymousInnerClass2 : ChannelInboundHandlerAdapter
		 {
			 private readonly NettyPipelineBuilderTest _outerInstance;

			 private object _msg;

			 public ChannelInboundHandlerAdapterAnonymousInnerClass2( NettyPipelineBuilderTest outerInstance, object msg )
			 {
				 this.outerInstance = outerInstance;
				 this._msg = msg;
			 }

			 public override void channelRead( ChannelHandlerContext ctx, object msg )
			 {
				  // handled
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogAnythingForHandledOutbound()
		 public virtual void ShouldNotLogAnythingForHandledOutbound()
		 {
			  // given
			  object msg = new object();
			  ChannelOutboundHandlerAdapter encoder = new ChannelOutboundHandlerAdapterAnonymousInnerClass3( this, msg );
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).add("write_handler", encoder).install();

			  // when
			  _channel.writeAndFlush( msg );

			  // then
			  _logProvider.assertNoLoggingOccurred();
		 }

		 private class ChannelOutboundHandlerAdapterAnonymousInnerClass3 : ChannelOutboundHandlerAdapter
		 {
			 private readonly NettyPipelineBuilderTest _outerInstance;

			 private object _msg;

			 public ChannelOutboundHandlerAdapterAnonymousInnerClass3( NettyPipelineBuilderTest outerInstance, object msg )
			 {
				 this.outerInstance = outerInstance;
				 this._msg = msg;
			 }

			 public override void write( ChannelHandlerContext ctx, object msg, ChannelPromise promise )
			 {
				  ctx.write( ctx.alloc().buffer() );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReInstallWithPreviousGate()
		 public virtual void ShouldReInstallWithPreviousGate()
		 {
			  // given
			  object gatedMessage = new object();

			  ServerNettyPipelineBuilder builderA = NettyPipelineBuilder.Server( _channel.pipeline(), _log );
			  builderA.AddGate( p => p == gatedMessage );
			  builderA.Install();

			  assertEquals( 3, GetHandlers( _channel.pipeline() ).Count ); // head/tail error handlers also counted
			  assertThat( _channel.pipeline().names(), hasItems(NettyPipelineBuilder.ERROR_HANDLER_HEAD, NettyPipelineBuilder.MESSAGE_GATE_NAME, NettyPipelineBuilder.ERROR_HANDLER_TAIL) );

			  // when
			  ServerNettyPipelineBuilder builderB = NettyPipelineBuilder.Server( _channel.pipeline(), _log );
			  builderB.Add( "my_handler", EMPTY_HANDLER );
			  builderB.Install();

			  // then
			  assertEquals( 4, GetHandlers( _channel.pipeline() ).Count ); // head/tail error handlers also counted
			  assertThat( _channel.pipeline().names(), hasItems(NettyPipelineBuilder.ERROR_HANDLER_HEAD, "my_handler", NettyPipelineBuilder.MESSAGE_GATE_NAME, NettyPipelineBuilder.ERROR_HANDLER_TAIL) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeCloseHandlerOnClose() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeCloseHandlerOnClose()
		 {
			  Semaphore semaphore = new Semaphore( 0 );
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).onClose(semaphore.release).install();

			  // when
			  _channel.close();

			  // then
			  assertTrue( semaphore.tryAcquire( 1, TimeUnit.MINUTES ) );
			  assertFalse( _channel.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeCloseHandlerOnPeerDisconnect() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeCloseHandlerOnPeerDisconnect()
		 {
			  Semaphore semaphore = new Semaphore( 0 );
			  NettyPipelineBuilder.Server( _channel.pipeline(), _log ).onClose(semaphore.release).install();

			  // when
			  _channel.disconnect();

			  // then
			  assertTrue( semaphore.tryAcquire( 1, TimeUnit.MINUTES ) );
			  assertFalse( _channel.Open );
		 }

		 private IList<ChannelHandler> GetHandlers( ChannelPipeline pipeline )
		 {
			  return pipeline.names().Select(pipeline.get).Where(Objects.nonNull).ToList();
		 }
	}

}