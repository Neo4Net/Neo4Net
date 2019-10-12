using System;
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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{

	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using Neo4Net.cluster.protocol;
	using Neo4Net.cluster.statemachine;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class LearnerStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseLastKnownOnlineClusterMemberAndSetTimeoutForCatchup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseLastKnownOnlineClusterMemberAndSetTimeoutForCatchup()
		 {
			  // Given
			  LearnerState state = LearnerState.Learner;
			  LearnerContext ctx = mock( typeof( LearnerContext ) );
			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  Neo4Net.cluster.InstanceId upToDateClusterMember = new Neo4Net.cluster.InstanceId( 1 );

			  // What we know
			  when( ctx.LastLearnedInstanceId ).thenReturn( 0L );
			  when( ctx.GetPaxosInstance( new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 1L ) ) ).thenReturn( new PaxosInstance( null, new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 1L ) ) );
			  when( ctx.LastKnownAliveUpToDateInstance ).thenReturn( upToDateClusterMember );
			  when( ctx.GetUriForId( upToDateClusterMember ) ).thenReturn( new URI( "c:/1" ) );

			  // What we know the cluster knows
			  when( ctx.LastKnownLearnedInstanceInCluster ).thenReturn( 1L );

			  // When
			  Message<LearnerMessage> message = Message.to( LearnerMessage.CatchUp, new URI( "c:/2" ), 2L ).setHeader( Message.HEADER_FROM, "c:/2" ).setHeader( Message.HEADER_INSTANCE_ID, "2" );
			  State newState = state.handle( ctx, message, outgoing );

			  // Then

			  assertThat( newState, equalTo( LearnerState.Learner ) );
			  verify( outgoing ).offer( Message.to( LearnerMessage.LearnRequest, new URI( "c:/1" ), new LearnerMessage.LearnRequestState() ).setHeader(Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE, Convert.ToString(1L)) );
			  verify( ctx ).setTimeout( "learn", Message.timeout( LearnerMessage.LearnTimedout, message ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void learnerServingOldInstanceShouldNotLogErrorIfItDoesNotHaveIt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LearnerServingOldInstanceShouldNotLogErrorIfItDoesNotHaveIt()
		 {
			  // Given
			  LearnerState state = LearnerState.Learner;
			  LearnerContext ctx = mock( typeof( LearnerContext ) );
			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  // The instance will be asked for paxos instance 4...
			  InstanceId paxosInstanceIdIDontHave = new InstanceId( 4 );
			  Message<LearnerMessage> messageRequestingId = Message.to( LearnerMessage.LearnRequest, URI.create( "c:/1" ) ).setHeader( Message.HEADER_FROM, "c:/2" ).setHeader( InstanceId.INSTANCE, "4" );
			  // ...but it does not have it yet
			  when( ctx.GetPaxosInstance( paxosInstanceIdIDontHave ) ).thenReturn( new PaxosInstance( mock( typeof( PaxosInstanceStore ) ), paxosInstanceIdIDontHave ) );

			  // When
			  state.handle( ctx, messageRequestingId, outgoing );

			  // Then
			  // verify there is no logging of the failure
			  verify( ctx, never() ).notifyLearnMiss(paxosInstanceIdIDontHave);
			  // but the learn failed went out anyway
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: verify(outgoing, times(1)).offer(org.mockito.ArgumentMatchers.argThat<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>>(new org.neo4j.cluster.protocol.MessageArgumentMatcher()
			  verify( outgoing, times( 1 ) ).offer(ArgumentMatchers.argThat<Message<MessageType>>(new MessageArgumentMatcher()
									.onMessageType( LearnerMessage.LearnFailed ).to( URI.create( "c:/2" ) )));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void learnerReceivingLearnFailedShouldLogIt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LearnerReceivingLearnFailedShouldLogIt()
		 {
			  // Given
			  LearnerState state = LearnerState.Learner;
			  LearnerContext ctx = mock( typeof( LearnerContext ) );
			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  InstanceId paxosInstanceIdIAskedFor = new InstanceId( 4 );
			  Message<LearnerMessage> theLearnFailure = Message.to( LearnerMessage.LearnFailed, URI.create( "c:/1" ) ).setHeader( Message.HEADER_FROM, "c:/2" ).setHeader( InstanceId.INSTANCE, "4" );
			  when( ctx.GetPaxosInstance( paxosInstanceIdIAskedFor ) ).thenReturn( new PaxosInstance( mock( typeof( PaxosInstanceStore ) ), paxosInstanceIdIAskedFor ) );
			  when( ctx.MemberURIs ).thenReturn( Collections.singletonList( URI.create( "c:/2" ) ) );

			  // When
			  state.handle( ctx, theLearnFailure, outgoing );

			  // Then
			  // verify that the failure was logged
			  verify( ctx, times( 1 ) ).notifyLearnMiss( paxosInstanceIdIAskedFor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void learnerShouldAskAllAliveInstancesAndTheseOnlyForMissingValue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LearnerShouldAskAllAliveInstancesAndTheseOnlyForMissingValue()
		 {
			  // Given

			  IList<URI> allMembers = new List<URI>( 3 );
			  URI instance1 = URI.create( "c:/1" ); // this one is failed
			  URI instance2 = URI.create( "c:/2" ); // this one is ok and will respond
			  URI instance3 = URI.create( "c:/3" ); // this one is the requesting instance
			  URI instance4 = URI.create( "c:/4" ); // and this one is ok and will respond too
			  allMembers.Add( instance1 );
			  allMembers.Add( instance2 );
			  allMembers.Add( instance3 );
			  allMembers.Add( instance4 );

			  ISet<Neo4Net.cluster.InstanceId> aliveInstanceIds = new HashSet<Neo4Net.cluster.InstanceId>();
			  Neo4Net.cluster.InstanceId id2 = new Neo4Net.cluster.InstanceId( 2 );
			  Neo4Net.cluster.InstanceId id4 = new Neo4Net.cluster.InstanceId( 4 );
			  aliveInstanceIds.Add( id2 );
			  aliveInstanceIds.Add( id4 );

			  LearnerState state = LearnerState.Learner;
			  LearnerContext ctx = mock( typeof( LearnerContext ) );
			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  InstanceId paxosInstanceIdIAskedFor = new InstanceId( 4 );

			  when( ctx.LastDeliveredInstanceId ).thenReturn( 3L );
			  when( ctx.LastKnownLearnedInstanceInCluster ).thenReturn( 5L );
			  when( ctx.MemberURIs ).thenReturn( allMembers );
			  when( ctx.Alive ).thenReturn( aliveInstanceIds );
			  when( ctx.GetUriForId( id2 ) ).thenReturn( instance2 );
			  when( ctx.GetUriForId( id4 ) ).thenReturn( instance4 );
			  when( ctx.GetPaxosInstance( paxosInstanceIdIAskedFor ) ).thenReturn( new PaxosInstance( mock( typeof( PaxosInstanceStore ) ), paxosInstanceIdIAskedFor ) );

			  Message<LearnerMessage> theCause = Message.to( LearnerMessage.CatchUp, instance2 ); // could be anything, really

			  // When
			  state.handle( ctx, Message.timeout( LearnerMessage.LearnTimedout, theCause ), outgoing );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: verify(outgoing, times(1)).offer(org.mockito.ArgumentMatchers.argThat<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>>(new org.neo4j.cluster.protocol.MessageArgumentMatcher()
			  verify( outgoing, times( 1 ) ).offer(ArgumentMatchers.argThat<Message<MessageType>>(new MessageArgumentMatcher()
									.onMessageType( LearnerMessage.LearnRequest ).to( instance2 )));
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: verify(outgoing, times(1)).offer(org.mockito.ArgumentMatchers.argThat<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>>(new org.neo4j.cluster.protocol.MessageArgumentMatcher()
			  verify( outgoing, times( 1 ) ).offer(ArgumentMatchers.argThat<Message<MessageType>>(new MessageArgumentMatcher()
									.onMessageType( LearnerMessage.LearnRequest ).to( instance4 )));
			  verifyNoMoreInteractions( outgoing );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLocalLearnMessagesWithoutInstanceIdInTheMessageHeaderWhenCatchingUp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleLocalLearnMessagesWithoutInstanceIdInTheMessageHeaderWhenCatchingUp()
		 {
			  // Given
			  LearnerState learner = LearnerState.Learner;
			  Neo4Net.cluster.InstanceId instanceId = new Neo4Net.cluster.InstanceId( 42 );
			  long payload = 12L;

			  LearnerContext context = mock( typeof( LearnerContext ) );
			  when( context.MyId ).thenReturn( instanceId );
			  when( context.LastKnownLearnedInstanceInCluster ).thenReturn( 11L );
			  when( context.LastLearnedInstanceId ).thenReturn( payload );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.cluster.com.message.Message<LearnerMessage> message = mock(org.neo4j.cluster.com.message.Message.class);
			  Message<LearnerMessage> message = mock( typeof( Message ) );
			  when( message.MessageType ).thenReturn( LearnerMessage.CatchUp );
			  when( message.HasHeader( Message.HEADER_INSTANCE_ID ) ).thenReturn( false );
			  when( message.GetHeader( Message.HEADER_INSTANCE_ID ) ).thenThrow( new System.ArgumentException() );
			  when( message.Payload ).thenReturn( payload );

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.statemachine.State<?,?> state = learner.handle(context, message, mock(org.neo4j.cluster.com.message.MessageHolder.class));
			  State<object, ?> state = learner.handle( context, message, mock( typeof( MessageHolder ) ) );

			  // Then
			  assertSame( state, learner );
			  verify( context, times( 1 ) ).setLastKnownLearnedInstanceInCluster( payload, instanceId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTheGapIfItsTheCoordinator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseTheGapIfItsTheCoordinator()
		 {
			  // Given
			  // A coordinator that knows that the last Paxos instance delivered is 3
			  LearnerState learner = LearnerState.Learner;
			  Neo4Net.cluster.InstanceId memberId = new Neo4Net.cluster.InstanceId( 42 );
			  long lastDelivered = 3L;

			  LearnerContext context = mock( typeof( LearnerContext ) );
			  when( context.IsMe( any() ) ).thenReturn(true);
			  when( context.Coordinator ).thenReturn( memberId ); // so it's the coordinator
			  when( context.LastDeliveredInstanceId ).thenReturn( lastDelivered );
			  // and has this list of pending instances (up to id 14)
			  IList<PaxosInstance> pendingInstances = new LinkedList<PaxosInstance>();
			  for ( int i = 1; i < 12; i++ ) // start at 1 because instance 3 is already delivered
			  {
					InstanceId instanceId = new InstanceId( lastDelivered + i );
					PaxosInstance value = new PaxosInstance( mock( typeof( PaxosInstanceStore ) ), instanceId );
					value.Closed( "", "" );
					when( context.GetPaxosInstance( instanceId ) ).thenReturn( value );
					pendingInstances.Add( value );
			  }
			  when( context.GetLog( any() ) ).thenReturn(mock(typeof(Log)));

			  Message<LearnerMessage> incomingInstance = Message.to( LearnerMessage.Learn, URI.create( "c:/1" ), new LearnerMessage.LearnState( new object() ) ).setHeader(Message.HEADER_FROM, "c:/2").setHeader(Message.HEADER_CONVERSATION_ID, "conversation-id").setHeader(InstanceId.INSTANCE, "" + (lastDelivered + LearnerContext_Fields.LEARN_GAP_THRESHOLD + 1));

			  // When
			  // it receives a message with Paxos instance id 1 greater than the threshold
			  learner.handle( context, incomingInstance, mock( typeof( MessageHolder ) ) );

			  // Then
			  // it delivers everything pending and marks the context appropriately
			  foreach ( PaxosInstance pendingInstance in pendingInstances )
			  {
					assertTrue( pendingInstance.IsState( PaxosInstance.State.Delivered ) );
					verify( context, times( 1 ) ).LastDeliveredInstanceId = pendingInstance.Id.id;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCloseTheGapIfItsTheCoordinatorAndTheGapIsSmallerThanTheThreshold() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCloseTheGapIfItsTheCoordinatorAndTheGapIsSmallerThanTheThreshold()
		 {
			  // Given
			  // A coordinator that knows that the last Paxos instance delivered is 3
			  long lastDelivered = 3L;
			  LearnerState learner = LearnerState.Learner;
			  Neo4Net.cluster.InstanceId memberId = new Neo4Net.cluster.InstanceId( 42 );

			  LearnerContext context = mock( typeof( LearnerContext ) );
			  when( context.IsMe( any() ) ).thenReturn(true);
			  when( context.Coordinator ).thenReturn( memberId ); // so it's the coordinator
			  when( context.LastDeliveredInstanceId ).thenReturn( lastDelivered );
			  // and has this list of pending instances (up to id 14)
			  IList<PaxosInstance> pendingInstances = new LinkedList<PaxosInstance>();
			  for ( int i = 1; i < 12; i++ ) // start at 1 because instance 3 is already delivered
			  {
					InstanceId instanceId = new InstanceId( lastDelivered + i );
					PaxosInstance value = new PaxosInstance( mock( typeof( PaxosInstanceStore ) ), instanceId );
					value.Closed( "", "" );
					when( context.GetPaxosInstance( instanceId ) ).thenReturn( value );
					pendingInstances.Add( value );
			  }
			  when( context.GetLog( any() ) ).thenReturn(mock(typeof(Log)));

			  Message<LearnerMessage> incomingInstance = Message.to( LearnerMessage.Learn, URI.create( "c:/1" ), new LearnerMessage.LearnState( new object() ) ).setHeader(Message.HEADER_FROM, "c:/2").setHeader(Message.HEADER_CONVERSATION_ID, "conversation-id").setHeader(InstanceId.INSTANCE, "" + (lastDelivered + LearnerContext_Fields.LEARN_GAP_THRESHOLD));

			  // When
			  // it receives a message with Paxos instance id at the threshold
			  learner.handle( context, incomingInstance, mock( typeof( MessageHolder ) ) );

			  // Then
			  // it waits and doesn't deliver anything
			  foreach ( PaxosInstance pendingInstance in pendingInstances )
			  {
					assertFalse( pendingInstance.IsState( PaxosInstance.State.Delivered ) );
			  }
			  verify( context, times( 0 ) ).LastDeliveredInstanceId = anyLong();
		 }
	}

}