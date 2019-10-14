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
namespace Neo4Net.causalclustering.catchup
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using Test = org.junit.Test;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class RequestDecoderDispatcherTest
	{
		 private readonly Protocol<State> protocol = new ProtocolAnonymousInnerClass();

		 private class ProtocolAnonymousInnerClass : Protocol<State>
		 {
			 public ProtocolAnonymousInnerClass() : base(State.Two)
			 {
			 }

		 }
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();

		 private enum State
		 {
			  One,
			  Two,
			  Three
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDispatchToRegisteredDecoder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDispatchToRegisteredDecoder()
		 {
			  // given
			  RequestDecoderDispatcher<State> dispatcher = new RequestDecoderDispatcher<State>( protocol, _logProvider );
			  ChannelInboundHandler delegateOne = mock( typeof( ChannelInboundHandler ) );
			  ChannelInboundHandler delegateTwo = mock( typeof( ChannelInboundHandler ) );
			  ChannelInboundHandler delegateThree = mock( typeof( ChannelInboundHandler ) );
			  dispatcher.Register( State.One, delegateOne );
			  dispatcher.Register( State.Two, delegateTwo );
			  dispatcher.Register( State.Three, delegateThree );

			  ChannelHandlerContext ctx = mock( typeof( ChannelHandlerContext ) );
			  object msg = new object();

			  // when
			  dispatcher.ChannelRead( ctx, msg );

			  // then
			  verify( delegateTwo ).channelRead( ctx, msg );
			  verifyNoMoreInteractions( delegateTwo );
			  verifyZeroInteractions( delegateOne, delegateThree );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogAWarningIfThereIsNoDecoderForTheMessageType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogAWarningIfThereIsNoDecoderForTheMessageType()
		 {
			  // given
			  RequestDecoderDispatcher<State> dispatcher = new RequestDecoderDispatcher<State>( protocol, _logProvider );
			  ChannelInboundHandler delegateOne = mock( typeof( ChannelInboundHandler ) );
			  ChannelInboundHandler delegateThree = mock( typeof( ChannelInboundHandler ) );
			  dispatcher.Register( State.One, delegateOne );
			  dispatcher.Register( State.Three, delegateThree );

			  // when
			  dispatcher.ChannelRead( mock( typeof( ChannelHandlerContext ) ), new object() );

			  // then
			  AssertableLogProvider.LogMatcher matcher = inLog( typeof( RequestDecoderDispatcher ) ).warn( "Unregistered handler for protocol %s", protocol );

			  _logProvider.assertExactly( matcher );
			  verifyZeroInteractions( delegateOne, delegateThree );
		 }
	}

}