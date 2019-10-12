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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using Mockito = org.mockito.Mockito;

	using Org.Neo4j.cluster.com.message;
	using MessageHolder = Org.Neo4j.cluster.com.message.MessageHolder;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;
	using TrackingMessageHolder = Org.Neo4j.cluster.com.message.TrackingMessageHolder;
	using Org.Neo4j.cluster.protocol;
	using State = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.PaxosInstance.State;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using NullLog = Org.Neo4j.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.to;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage.phase1Timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage.promise;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage.propose;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage.rejectAccept;

	public class ProposerStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "unchecked", "rawtypes" }) @Test public void ifProposingWithClosedInstanceThenRetryWithNextInstance() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IfProposingWithClosedInstanceThenRetryWithNextInstance()
		 {
			  ProposerContext context = Mockito.mock( typeof( ProposerContext ) );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );

			  Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId instanceId = new Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 42 );
			  PaxosInstanceStore paxosInstanceStore = new PaxosInstanceStore();

			  // The instance is closed
			  PaxosInstance paxosInstance = new PaxosInstance( paxosInstanceStore, instanceId ); // the instance
			  paxosInstance.Closed( instanceId, "1/15#" ); // is closed for that conversation, not really important
			  when( context.UnbookInstance( instanceId ) ).thenReturn( Message.@internal( ProposerMessage.Accepted, "the closed payload" ) );

			  when( context.GetPaxosInstance( instanceId ) ).thenReturn( paxosInstance ); // required for

			  // But in the meantime it was reused and has now (of course) timed out
			  string theTimedoutPayload = "the timed out payload";
			  Message message = Message.@internal( ProposerMessage.Phase1Timeout, theTimedoutPayload );
			  message.setHeader( Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE, instanceId.ToString() );

			  // Handle it
			  MessageHolder mockHolder = mock( typeof( MessageHolder ) );
			  ProposerState.Proposer.handle( context, message, mockHolder );

			  // Verify it was resent as a propose with the same value
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: verify(mockHolder, times(1)).offer(org.mockito.ArgumentMatchers.argThat<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>>(new org.neo4j.cluster.protocol.MessageArgumentMatcher().onMessageType(ProposerMessage.propose).withPayload(theTimedoutPayload)));
			  verify( mockHolder, times( 1 ) ).offer( ArgumentMatchers.argThat<Message<MessageType>>( ( new MessageArgumentMatcher() ).onMessageType(ProposerMessage.Propose).withPayload(theTimedoutPayload) ) );
			  verify( context, times( 1 ) ).unbookInstance( instanceId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void something() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Something()
		 {
			  object acceptorValue = new object();
			  object bookedValue = new object();

			  Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId instanceId = new Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 42 );

			  PaxosInstanceStore paxosInstanceStore = new PaxosInstanceStore();

			  ProposerContext context = Mockito.mock( typeof( ProposerContext ) );
			  when( context.GetPaxosInstance( instanceId ) ).thenReturn( paxosInstanceStore.GetPaxosInstance( instanceId ) );
			  when( context.GetMinimumQuorumSize( Mockito.anyList() ) ).thenReturn(2);

			  // The instance is closed
			  PaxosInstance paxosInstance = new PaxosInstance( paxosInstanceStore, instanceId ); // the instance
			  paxosInstance.Propose( 2001, Iterables.asList( Iterables.iterable( create( "http://something1" ), create( "http://something2" ), create( "http://something3" ) ) ) );

			  Message message = Message.to( ProposerMessage.Promise, create( "http://something1" ), new ProposerMessage.PromiseState( 2001, acceptorValue ) );
			  message.setHeader( Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE, instanceId.ToString() );

			  MessageHolder mockHolder = mock( typeof( MessageHolder ) );
			  ProposerState.Proposer.handle( context, message, mockHolder );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void proposer_proposePhase1TimeoutShouldCarryOnPayload() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProposerProposePhase1TimeoutShouldCarryOnPayload()
		 {
			  // GIVEN
			  PaxosInstance instance = mock( typeof( PaxosInstance ) );
			  ProposerContext context = mock( typeof( ProposerContext ) );
			  when( context.GetPaxosInstance( any( typeof( Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId ) ) ) ).thenReturn( instance );
			  when( context.MyId ).thenReturn( new Org.Neo4j.cluster.InstanceId( 0 ) );
			  TrackingMessageHolder outgoing = new TrackingMessageHolder();
			  string instanceId = "1";
			  Serializable payload = "myPayload";
			  Message<ProposerMessage> message = to( propose, create( "http://something" ), payload ).setHeader( INSTANCE, instanceId );

			  // WHEN
			  ProposerState.Proposer.handle( context, message, outgoing );

			  // THEN
			  verify( context ).setTimeout( eq( new Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( instanceId ) ), argThat( ( new MessageArgumentMatcher<>() ).withPayload(payload) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void proposer_phase1TimeoutShouldCarryOnPayload() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProposerPhase1TimeoutShouldCarryOnPayload()
		 {
			  // GIVEN
			  PaxosInstance instance = mock( typeof( PaxosInstance ) );
			  when( instance.IsState( State.p1_pending ) ).thenReturn( true );
			  ProposerContext context = mock( typeof( ProposerContext ) );
			  when( context.GetPaxosInstance( any( typeof( Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId ) ) ) ).thenReturn( instance );
			  TrackingMessageHolder outgoing = new TrackingMessageHolder();
			  string instanceId = "1";
			  Serializable payload = "myPayload";
			  Message<ProposerMessage> message = to( phase1Timeout, create( "http://something" ), payload ).setHeader( INSTANCE, instanceId );

			  // WHEN
			  ProposerState.Proposer.handle( context, message, outgoing );

			  // THEN
			  verify( context ).setTimeout( eq( new Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( instanceId ) ), argThat( ( new MessageArgumentMatcher<>() ).withPayload(payload) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void proposer_rejectAcceptShouldCarryOnPayload() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProposerRejectAcceptShouldCarryOnPayload()
		 {
			  // GIVEN
			  string instanceId = "1";
			  PaxosInstance instance = new PaxosInstance( mock( typeof( PaxosInstanceStore ) ), new Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( instanceId ) );
			  Serializable payload = "myPayload";
			  instance.Propose( 1, new IList<java.net.URI> { create( "http://some-guy" ) } );
			  instance.Ready( payload, true );
			  instance.Pending();
			  ProposerContext context = mock( typeof( ProposerContext ) );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  when( context.GetPaxosInstance( any( typeof( Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId ) ) ) ).thenReturn( instance );
			  when( context.MyId ).thenReturn( new Org.Neo4j.cluster.InstanceId( parseInt( instanceId ) ) );
			  TrackingMessageHolder outgoing = new TrackingMessageHolder();
			  Message<ProposerMessage> message = to( rejectAccept, create( "http://something" ), new ProposerMessage.RejectAcceptState() ).setHeader(INSTANCE, instanceId);

			  // WHEN
			  ProposerState.Proposer.handle( context, message, outgoing );

			  // THEN
			  verify( context ).setTimeout( eq( new Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( instanceId ) ), argThat( ( new MessageArgumentMatcher<>() ).withPayload(payload) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void proposer_promiseShouldCarryOnPayloadToPhase2Timeout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProposerPromiseShouldCarryOnPayloadToPhase2Timeout()
		 {
			  // GIVEN
			  string instanceId = "1";
			  Serializable payload = "myPayload";
			  PaxosInstance instance = new PaxosInstance( mock( typeof( PaxosInstanceStore ) ), new Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( instanceId ) );
			  instance.Propose( 1, new IList<java.net.URI> { create( "http://some-guy" ) } );
			  instance.Value_2 = payload; // don't blame me for making it package access.
			  ProposerContext context = mock( typeof( ProposerContext ) );
			  when( context.GetPaxosInstance( any( typeof( Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId ) ) ) ).thenReturn( instance );
			  when( context.GetMinimumQuorumSize( anyList() ) ).thenReturn(1);
			  TrackingMessageHolder outgoing = new TrackingMessageHolder();
			  Message<ProposerMessage> message = to( promise, create( "http://something" ), new ProposerMessage.PromiseState( 1, payload ) ).setHeader( INSTANCE, instanceId );

			  // WHEN
			  ProposerState.Proposer.handle( context, message, outgoing );

			  // THEN
			  verify( context ).setTimeout( eq( new Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( instanceId ) ), argThat( ( new MessageArgumentMatcher<>() ).withPayload(payload) ) );
		 }
	}

}