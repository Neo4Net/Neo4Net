using System;
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
namespace Neo4Net.cluster.protocol.heartbeat
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using Mockito = org.mockito.Mockito;


	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageSender = Neo4Net.cluster.com.message.MessageSender;
	using MessageSource = Neo4Net.cluster.com.message.MessageSource;
	using Neo4Net.cluster.protocol;
	using ObjectInputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using AcceptorInstanceStore = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using LearnerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage;
	using MultiPaxosContext = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ElectionCredentialsProvider = Neo4Net.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionRole = Neo4Net.cluster.protocol.election.ElectionRole;
	using Neo4Net.cluster.statemachine;
	using TimeoutStrategy = Neo4Net.cluster.timeout.TimeoutStrategy;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.iterable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class HeartbeatStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreSuspicionsForOurselves() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreSuspicionsForOurselves()
		 {
			  // Given
			  InstanceId instanceId = new InstanceId( 1 );
			  HeartbeatState heartbeat = HeartbeatState.Heartbeat;
			  ClusterConfiguration configuration = new ClusterConfiguration( "whatever", NullLogProvider.Instance, "cluster://1", "cluster://2" );
			  configuration.Joined( instanceId, URI.create( "cluster://1" ) );
			  configuration.Joined( new InstanceId( 2 ), URI.create( "cluster://2" ) );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext context = new MultiPaxosContext( instanceId, iterable( new ElectionRole( "coordinator" ) ), configuration, Mockito.mock( typeof( Executor ) ), NullLogProvider.Instance, Mockito.mock( typeof( ObjectInputStreamFactory ) ), Mockito.mock( typeof( ObjectOutputStreamFactory ) ), Mockito.mock( typeof( AcceptorInstanceStore ) ), Mockito.mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  HeartbeatContext heartbeatContext = context.HeartbeatContext;
			  Message received = Message.@internal( HeartbeatMessage.Suspicions, new HeartbeatMessage.SuspicionsState( asSet( iterable( instanceId ) ) ) );
			  received.setHeader( Message.HEADER_FROM, "cluster://2" ).SetHeader( Message.HEADER_INSTANCE_ID, "2" );

			  // When
			  heartbeat.handle( heartbeatContext, received, mock( typeof( MessageHolder ) ) );

			  // Then
			  assertThat( heartbeatContext.GetSuspicionsOf( instanceId ).Count, equalTo( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreSuspicionsForOurselvesButKeepTheRest() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreSuspicionsForOurselvesButKeepTheRest()
		 {
			  // Given
			  InstanceId myId = new InstanceId( 1 );
			  InstanceId foreignId = new InstanceId( 3 );
			  HeartbeatState heartbeat = HeartbeatState.Heartbeat;
			  ClusterConfiguration configuration = new ClusterConfiguration( "whatever", NullLogProvider.Instance, "cluster://1", "cluster://2" );
			  configuration.Joined( myId, URI.create( "cluster://1" ) );
			  configuration.Joined( new InstanceId( 2 ), URI.create( "cluster://2" ) );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext context = new MultiPaxosContext( myId, iterable( new ElectionRole( "coordinator" ) ), configuration, Mockito.mock( typeof( Executor ) ), NullLogProvider.Instance, Mockito.mock( typeof( ObjectInputStreamFactory ) ), Mockito.mock( typeof( ObjectOutputStreamFactory ) ), Mockito.mock( typeof( AcceptorInstanceStore ) ), Mockito.mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  HeartbeatContext heartbeatContext = context.HeartbeatContext;
			  Message received = Message.@internal( HeartbeatMessage.Suspicions, new HeartbeatMessage.SuspicionsState( asSet( iterable( myId, foreignId ) ) ) );
			  received.setHeader( Message.HEADER_FROM, "cluster://2" ).SetHeader( Message.HEADER_INSTANCE_ID, "2" );

			  // When
			  heartbeat.handle( heartbeatContext, received, mock( typeof( MessageHolder ) ) );

			  // Then
			  assertThat( heartbeatContext.GetSuspicionsOf( myId ).Count, equalTo( 0 ) );
			  assertThat( heartbeatContext.GetSuspicionsOf( foreignId ).Count, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddInstanceIdHeaderInCatchUpMessages() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddInstanceIdHeaderInCatchUpMessages()
		 {
			  // Given
			  InstanceId instanceId = new InstanceId( 1 );
			  HeartbeatState heartbeat = HeartbeatState.Heartbeat;
			  ClusterConfiguration configuration = new ClusterConfiguration( "whatever", NullLogProvider.Instance, "cluster://1", "cluster://2" );
			  configuration.Joined( instanceId, URI.create( "cluster://1" ) );
			  InstanceId otherInstance = new InstanceId( 2 );
			  configuration.Joined( otherInstance, URI.create( "cluster://2" ) );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext context = new MultiPaxosContext( instanceId, iterable( new ElectionRole( "coordinator" ) ), configuration, Mockito.mock( typeof( Executor ) ), NullLogProvider.Instance, Mockito.mock( typeof( ObjectInputStreamFactory ) ), Mockito.mock( typeof( ObjectOutputStreamFactory ) ), Mockito.mock( typeof( AcceptorInstanceStore ) ), Mockito.mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  int lastDeliveredInstanceId = 100;
			  context.LearnerContext.LastDeliveredInstanceId = lastDeliveredInstanceId;
			  // This gap will trigger the catchUp message that we'll test against
			  lastDeliveredInstanceId += 20;

			  HeartbeatContext heartbeatContext = context.HeartbeatContext;
			  Message received = Message.@internal( HeartbeatMessage.IAmAlive, new HeartbeatMessage.IAmAliveState( otherInstance ) );
			  received.setHeader( Message.HEADER_FROM, "cluster://2" ).SetHeader( Message.HEADER_INSTANCE_ID, "2" ).setHeader( "last-learned", Convert.ToString( lastDeliveredInstanceId ) );

			  // When
			  MessageHolder holder = mock( typeof( MessageHolder ) );
			  heartbeat.handle( heartbeatContext, received, holder );

			  // Then
			  verify( holder, times( 1 ) ).offer(ArgumentMatchers.argThat(new MessageArgumentMatcher<LearnerMessage>()
						 .onMessageType( LearnerMessage.catchUp ).withHeader( Message.HEADER_INSTANCE_ID, "2" )));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFirstHeartbeatAfterTimeout()
		 public virtual void ShouldLogFirstHeartbeatAfterTimeout()
		 {
			  // given
			  InstanceId instanceId = new InstanceId( 1 );
			  InstanceId otherInstance = new InstanceId( 2 );
			  ClusterConfiguration configuration = new ClusterConfiguration( "whatever", NullLogProvider.Instance, "cluster://1", "cluster://2" );
			  configuration.Members[otherInstance] = URI.create( "cluster://2" );
			  AssertableLogProvider internalLog = new AssertableLogProvider( true );
			  TimeoutStrategy timeoutStrategy = mock( typeof( TimeoutStrategy ) );
			  Timeouts timeouts = new Timeouts( timeoutStrategy );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext context = new MultiPaxosContext( instanceId, iterable( new ElectionRole( "coordinator" ) ), configuration, mock( typeof( Executor ) ), internalLog, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), timeouts, mock( typeof( ElectionCredentialsProvider ) ), config );

			  StateMachines stateMachines = new StateMachines( internalLog, mock( typeof( StateMachines.Monitor ) ), mock( typeof( MessageSource ) ), mock( typeof( MessageSender ) ), timeouts, mock( typeof( DelayedDirectExecutor ) ), ThreadStart.run, instanceId );
			  stateMachines.AddStateMachine( new StateMachine( context.HeartbeatContext, typeof( HeartbeatMessage ), HeartbeatState.Start, internalLog ) );

			  timeouts.Tick( 0 );
			  when( timeoutStrategy.TimeoutFor( any( typeof( Message ) ) ) ).thenReturn( 5L );

			  // when
			  stateMachines.Process( Message.@internal( HeartbeatMessage.Join ) );
			  stateMachines.Process( Message.@internal( HeartbeatMessage.IAmAlive, new HeartbeatMessage.IAmAliveState( otherInstance ) ).setHeader( Message.HEADER_CREATED_BY, otherInstance.ToString() ) );
			  for ( int i = 1; i <= 15; i++ )
			  {
					timeouts.Tick( i );
			  }

			  // then
			  verify( timeoutStrategy, times( 3 ) ).timeoutTriggered(argThat(new MessageArgumentMatcher<>()
						 .onMessageType( HeartbeatMessage.TimedOut )));
			  internalLog.AssertExactly( inLog( typeof( HeartbeatState ) ).debug( "Received timed out for server 2" ), inLog( typeof( HeartbeatContext ) ).info( "1(me) is now suspecting 2" ), inLog( typeof( HeartbeatState ) ).debug( "Received timed out for server 2" ), inLog( typeof( HeartbeatState ) ).debug( "Received timed out for server 2" ) );
			  internalLog.Clear();

			  // when
			  stateMachines.Process( Message.@internal( HeartbeatMessage.IAmAlive, new HeartbeatMessage.IAmAliveState( otherInstance ) ).setHeader( Message.HEADER_CREATED_BY, otherInstance.ToString() ) );

			  // then
			  internalLog.AssertExactly( inLog( typeof( HeartbeatState ) ).debug( "Received i_am_alive[2] after missing 3 (15ms)" ) );
		 }
	}

}