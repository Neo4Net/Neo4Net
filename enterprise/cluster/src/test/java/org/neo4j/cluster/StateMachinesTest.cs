using System.Collections.Generic;
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
namespace Org.Neo4j.cluster
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using Mockito = org.mockito.Mockito;
	using Answer = org.mockito.stubbing.Answer;


	using Org.Neo4j.cluster.com.message;
	using MessageHolder = Org.Neo4j.cluster.com.message.MessageHolder;
	using MessageSender = Org.Neo4j.cluster.com.message.MessageSender;
	using MessageSource = Org.Neo4j.cluster.com.message.MessageSource;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;
	using Org.Neo4j.cluster.statemachine;
	using Org.Neo4j.cluster.statemachine;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.@internal;

	public class StateMachinesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenMessageHandlingCausesNewMessagesThenEnsureCorrectOrder()
		 public virtual void WhenMessageHandlingCausesNewMessagesThenEnsureCorrectOrder()
		 {
			  // Given
			  StateMachines stateMachines = new StateMachines( NullLogProvider.Instance, mock( typeof( StateMachines.Monitor ) ), mock( typeof( MessageSource ) ), Mockito.mock( typeof( MessageSender ) ), Mockito.mock( typeof( Timeouts ) ), Mockito.mock( typeof( DelayedDirectExecutor ) ), ThreadStart.run, mock( typeof( InstanceId ) ) );

			  List<TestMessage> handleOrder = new List<TestMessage>();
			  StateMachine stateMachine = new StateMachine( handleOrder, typeof( TestMessage ), TestState.Test, NullLogProvider.Instance );

			  stateMachines.AddStateMachine( stateMachine );

			  // When
			  stateMachines.Process( @internal( TestMessage.Message1 ) );

			  // Then
			  assertThat( handleOrder.ToString(), equalTo("[message1, message2, message4, message5, message3]") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAlwaysAddItsInstanceIdToOutgoingMessages()
		 public virtual void ShouldAlwaysAddItsInstanceIdToOutgoingMessages()
		 {
			  InstanceId me = new InstanceId( 42 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.cluster.com.message.Message> sentOut = new java.util.LinkedList<>();
			  IList<Message> sentOut = new LinkedList<Message>();

			  /*
			   * Lots of setup required. Must have a sender that keeps messages so we can see what the machine sent out.
			   * We must have the StateMachines actually delegate the incoming message and retrieve the generated outgoing.
			   * That means we need an actual StateMachine with a registered MessageType. And most of those are void
			   * methods, which means lots of Answer objects.
			   */
			  // Given
			  MessageSender sender = mock( typeof( MessageSender ) );
			  // The sender, which adds messages outgoing to the list above.
			  doAnswer(invocation =>
			  {
				( ( IList<Org.Neo4j.cluster.com.message.Message> )sentOut ).AddRange( invocation.getArgument( 0 ) );
				return null;
			  }).when( sender ).process( ArgumentMatchers.any<IList<Message<? extends MessageType>>>() );

			  StateMachines stateMachines = new StateMachines( NullLogProvider.Instance, mock( typeof( StateMachines.Monitor ) ), mock( typeof( MessageSource ) ), sender, mock( typeof( Timeouts ) ), mock( typeof( DelayedDirectExecutor ) ), ThreadStart.run, me );

			  // The state machine, which has a TestMessage message type and simply adds a HEADER_TO header to the messages it
			  // is handed to handle.
			  StateMachine machine = mock( typeof( StateMachine ) );
			  when( machine.MessageType ).then( ( Answer<object> ) invocation => typeof( TestMessage ) );
			  doAnswer(invocation =>
			  {
				Message message = invocation.getArgument( 0 );
				MessageHolder holder = invocation.getArgument( 1 );
				message.setHeader( Message.HEADER_TO, "to://neverland" );
				holder.offer( message );
				return null;
			  }).when( machine ).handle( any( typeof( Message ) ), any( typeof( MessageHolder ) ) );
			  stateMachines.AddStateMachine( machine );

			  // When
			  stateMachines.Process( Message.@internal( TestMessage.Message1 ) );

			  // Then
			  assertEquals( "StateMachines should not make up messages from thin air", 1, sentOut.Count );
			  Message sent = sentOut[0];
			  assertTrue( "StateMachines should add the instance-id header", sent.hasHeader( Message.HEADER_INSTANCE_ID ) );
			  assertEquals( "StateMachines should add instance-id header that has the correct value", me.ToString(), sent.getHeader(Message.HEADER_INSTANCE_ID) );
		 }

		 public enum TestMessage
		 {
			  Message1,
			  Message2,
			  Message3,
			  Message4,
			  Message5
		 }

		 public enum TestState
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: test { @Override public TestState handle(java.util.List context, org.neo4j.cluster.com.message.Message<TestMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { context.add(message.getMessageType()); switch(message.getMessageType()) { case message1: { outgoing.offer(internal(TestMessage.message2)); outgoing.offer(internal(TestMessage.message3)); break; } case message2: { outgoing.offer(internal(TestMessage.message4)); outgoing.offer(internal(TestMessage.message5)); break; } default: break; } return this; } }
			  test
			  {
				  public TestState handle( System.Collections.IList context, Message<TestMessage> message, MessageHolder outgoing )
				  {
					  context.add( message.MessageType ); switch ( message.MessageType )
					  {
						  case message1: { outgoing.offer( @internal( TestMessage.Message2 ) ); outgoing.offer( @internal( TestMessage.Message3 ) ); break; } case message2: { outgoing.offer( @internal( TestMessage.Message4 ) ); outgoing.offer( @internal( TestMessage.Message5 ) ); break; } default: break;
					  }
					  return this;
				  }
			  }
		 }
	}

}