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
namespace Neo4Net.causalclustering.messaging
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using ChannelPromise = io.netty.channel.ChannelPromise;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;

	using GateEvent = Neo4Net.causalclustering.protocol.handshake.GateEvent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class MessageGateTest
	{
		private bool InstanceFieldsInitialized = false;

		public MessageGateTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_gate = new MessageGate( m => m != _allowedMsg );
		}

		 private readonly string _allowedMsg = "allowed";
		 private MessageGate _gate;
		 private readonly ChannelHandlerContext _ctx = mock( typeof( ChannelHandlerContext ) );
		 private readonly Channel _channel = mock( typeof( Channel ) );
		 private readonly ChannelPipeline _pipeline = mock( typeof( ChannelPipeline ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( _channel.pipeline() ).thenReturn(_pipeline);
			  when( _ctx.channel() ).thenReturn(_channel);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLetAllowedMessagesPass() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLetAllowedMessagesPass()
		 {
			  // when
			  ChannelPromise promise = mock( typeof( ChannelPromise ) );
			  _gate.write( _ctx, _allowedMsg, promise );
			  _gate.write( _ctx, _allowedMsg, promise );
			  _gate.write( _ctx, _allowedMsg, promise );

			  // then
			  verify( _ctx, times( 3 ) ).write( _allowedMsg, promise );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGateMessages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGateMessages()
		 {
			  // when
			  ChannelPromise promise = mock( typeof( ChannelPromise ) );
			  _gate.write( _ctx, "A", promise );
			  _gate.write( _ctx, "B", promise );
			  _gate.write( _ctx, "C", promise );

			  // then
			  verify( _ctx, never() ).write(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLetGatedMessagesPassOnSuccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLetGatedMessagesPassOnSuccess()
		 {
			  // given
			  ChannelPromise promiseA = mock( typeof( ChannelPromise ) );
			  ChannelPromise promiseB = mock( typeof( ChannelPromise ) );
			  ChannelPromise promiseC = mock( typeof( ChannelPromise ) );

			  _gate.write( _ctx, "A", promiseA );
			  _gate.write( _ctx, "B", promiseB );
			  _gate.write( _ctx, "C", promiseC );
			  verify( _ctx, never() ).write(any(), any());

			  // when
			  _gate.userEventTriggered( _ctx, GateEvent.Success );

			  // then
			  InOrder inOrder = Mockito.inOrder( _ctx );
			  inOrder.verify( _ctx ).write( "A", promiseA );
			  inOrder.verify( _ctx ).write( "B", promiseB );
			  inOrder.verify( _ctx ).write( "C", promiseC );
			  inOrder.verify( _ctx, never() ).write(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveGateOnSuccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveGateOnSuccess()
		 {
			  // when
			  _gate.userEventTriggered( _ctx, GateEvent.Success );

			  // then
			  verify( _pipeline ).remove( _gate );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLetGatedMessagesPassAfterFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLetGatedMessagesPassAfterFailure()
		 {
			  // given
			  ChannelPromise promise = mock( typeof( ChannelPromise ) );
			  _gate.userEventTriggered( _ctx, GateEvent.Failure );

			  // when
			  _gate.write( _ctx, "A", promise );
			  _gate.write( _ctx, "B", promise );
			  _gate.write( _ctx, "C", promise );

			  // then
			  verify( _ctx, never() ).write(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStillLetAllowedMessagePassAfterFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStillLetAllowedMessagePassAfterFailure()
		 {
			  // given
			  ChannelPromise promise = mock( typeof( ChannelPromise ) );
			  _gate.userEventTriggered( _ctx, GateEvent.Failure );

			  // when
			  _gate.write( _ctx, _allowedMsg, promise );

			  // then
			  verify( _ctx ).write( _allowedMsg, promise );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLeaveGateOnFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLeaveGateOnFailure()
		 {
			  // when
			  _gate.userEventTriggered( _ctx, GateEvent.Failure );

			  // then
			  verify( _pipeline, never() ).remove(any(typeof(ChannelHandler)));
		 }
	}

}