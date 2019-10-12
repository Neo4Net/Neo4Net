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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context
{
	using Test = org.junit.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterContext = Org.Neo4j.cluster.protocol.cluster.ClusterContext;
	using ClusterMessage = Org.Neo4j.cluster.protocol.cluster.ClusterMessage;
	using HeartbeatContext = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatContext;
	using HeartbeatListener = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatListener;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ClusterContextImplTest
	{
		 /*
		  * This test ensures that an instance that cleanly leaves the cluster is no longer assumed to be an elector. This
		  * has the effect that when it rejoins its elector version will be reset and its results will go through
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void electorLeavingTheClusterMustBeRemovedAsElector()
		 public virtual void ElectorLeavingTheClusterMustBeRemovedAsElector()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId elector = new InstanceId( 2 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.GetUriForId( elector ) ).thenReturn( URI.create( "cluster://instance2" ) );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ) );
			  when( commonContextState.Configuration() ).thenReturn(clusterConfiguration);

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, mock( typeof( Timeouts ) ), mock( typeof( Executor ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), mock( typeof( HeartbeatContext ) ), mock( typeof( Config ) ) );

				 // This means instance 2 was the elector at version 8
			  context.LastElector = elector;
			  context.LastElectorVersion = 8;

			  // When
			  context.Left( elector );

			  // Then
			  assertEquals( context.LastElector, InstanceId.NONE );
			  assertEquals( context.LastElectorVersion, -1 );
		 }

		 /*
		  * This test ensures that an instance that cleanly leaves the cluster but is not the elector has no effect on
		  * elector id and last version
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonElectorLeavingTheClusterMustNotAffectElectorInformation()
		 public virtual void NonElectorLeavingTheClusterMustNotAffectElectorInformation()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId elector = new InstanceId( 2 );
			  InstanceId other = new InstanceId( 3 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.GetUriForId( other ) ).thenReturn( URI.create( "cluster://instance2" ) );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ) );
			  when( commonContextState.Configuration() ).thenReturn(clusterConfiguration);

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, mock( typeof( Timeouts ) ), mock( typeof( Executor ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), mock( typeof( HeartbeatContext ) ),mock( typeof( Config ) ) );

				 // This means instance 2 was the elector at version 8
			  context.LastElector = elector;
			  context.LastElectorVersion = 8;

			  // When
			  context.Left( other );

			  // Then
			  assertEquals( context.LastElector, elector );
			  assertEquals( context.LastElectorVersion, 8 );
		 }

		 /*
		  * This test ensures that an instance that enters the cluster has its elector version reset. That means that
		  * if it was the elector before its version is now reset so results can be applied. This and the previous tests
		  * actually perform the same things at different events, one covering for the other.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void instanceEnteringTheClusterMustBeRemovedAsElector()
		 public virtual void InstanceEnteringTheClusterMustBeRemovedAsElector()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId elector = new InstanceId( 2 );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ), RETURNS_MOCKS );

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, mock( typeof( Timeouts ) ), mock( typeof( Executor ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), mock( typeof( HeartbeatContext ) ), mock( typeof( Config ) ) );

			  // This means instance 2 was the elector at version 8
			  context.LastElector = elector;
			  context.LastElectorVersion = 8;

			  // When
			  context.Joined( elector, URI.create( "cluster://elector" ) );

			  // Then
			  assertEquals( context.LastElector, InstanceId.NONE );
			  assertEquals( context.LastElectorVersion, -1 );
		 }

		 /*
		  * This test ensures that a joining instance that was not marked as elector before does not affect the
		  * current elector version. This is the complement of the previous test.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void instanceEnteringTheClusterMustBeNotAffectElectorStatusIfItWasNotElectorBefore()
		 public virtual void InstanceEnteringTheClusterMustBeNotAffectElectorStatusIfItWasNotElectorBefore()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId elector = new InstanceId( 2 );
			  InstanceId other = new InstanceId( 3 );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ), RETURNS_MOCKS );

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, mock( typeof( Timeouts ) ), mock( typeof( Executor ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), mock( typeof( HeartbeatContext ) ), mock( typeof( Config ) ) );

			  // This means instance 2 was the elector at version 8
			  context.LastElector = elector;
			  context.LastElectorVersion = 8;

			  // When
			  context.Joined( other, URI.create( "cluster://other" ) );

			  // Then
			  assertEquals( context.LastElector, elector );
			  assertEquals( context.LastElectorVersion, 8 );
		 }

		 /*
		  * This test ensures that an instance that is marked as failed has its elector version reset. This means that
		  * the instance, once it comes back, will still be able to do elections even if it lost state
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void electorFailingMustCauseElectorVersionToBeReset()
		 public virtual void ElectorFailingMustCauseElectorVersionToBeReset()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId elector = new InstanceId( 2 );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ), RETURNS_MOCKS );
			  Timeouts timeouts = mock( typeof( Timeouts ) );
			  Executor executor = mock( typeof( Executor ) );

			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );

			  ArgumentCaptor<HeartbeatListener> listenerCaptor = ArgumentCaptor.forClass( typeof( HeartbeatListener ) );

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, timeouts, executor, mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), heartbeatContext, mock( typeof( Config ) ) );

			  verify( heartbeatContext ).addHeartbeatListener( listenerCaptor.capture() );

			  HeartbeatListener theListener = listenerCaptor.Value;

			  // This means instance 2 was the elector at version 8
			  context.LastElector = elector;
			  context.LastElectorVersion = 8;

			  // When
			  theListener.Failed( elector );

			  // Then
			  assertEquals( context.LastElector, InstanceId.NONE );
			  assertEquals( context.LastElectorVersion, ClusterContextImpl.NO_ELECTOR_VERSION );
		 }

		 /*
		  * This test ensures that an instance that is marked as failed has its elector version reset. This means that
		  * the instance, once it comes back, will still be able to do elections even if it lost state
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonElectorFailingMustNotCauseElectorVersionToBeReset()
		 public virtual void NonElectorFailingMustNotCauseElectorVersionToBeReset()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId elector = new InstanceId( 2 );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ), RETURNS_MOCKS );
			  Timeouts timeouts = mock( typeof( Timeouts ) );
			  Executor executor = mock( typeof( Executor ) );

			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );

			  ArgumentCaptor<HeartbeatListener> listenerCaptor = ArgumentCaptor.forClass( typeof( HeartbeatListener ) );

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, timeouts, executor, mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), heartbeatContext, mock( typeof( Config ) ) );

			  verify( heartbeatContext ).addHeartbeatListener( listenerCaptor.capture() );

			  HeartbeatListener theListener = listenerCaptor.Value;

			  // This means instance 2 was the elector at version 8
			  context.LastElector = elector;
			  context.LastElectorVersion = 8;

			  // When
			  theListener.Failed( new InstanceId( 3 ) );

			  // Then
			  assertEquals( context.LastElector, elector );
			  assertEquals( context.LastElectorVersion, 8 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGracefullyHandleEmptyDiscoveryHeader()
		 public virtual void ShouldGracefullyHandleEmptyDiscoveryHeader()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId joining = new InstanceId( 2 );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ), RETURNS_MOCKS );
			  Timeouts timeouts = mock( typeof( Timeouts ) );
			  Executor executor = mock( typeof( Executor ) );

			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, timeouts, executor, mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), heartbeatContext, mock( typeof( Config ) ) );

			  ClusterMessage.ConfigurationRequestState request = mock( typeof( ClusterMessage.ConfigurationRequestState ) );
			  when( request.JoiningId ).thenReturn( joining );

			  // When
			  // Instance 2 contacts us with a request but it is empty
			  context.AddContactingInstance( request, "" );

			  // Then
			  // The discovery header we generate should still contain that instance
			  assertEquals( "2", context.GenerateDiscoveryHeader() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateDiscoveryHeaderWithContactingInstances()
		 public virtual void ShouldUpdateDiscoveryHeaderWithContactingInstances()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId joiningOne = new InstanceId( 2 );
			  InstanceId joiningTwo = new InstanceId( 3 );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ), RETURNS_MOCKS );
			  Timeouts timeouts = mock( typeof( Timeouts ) );
			  Executor executor = mock( typeof( Executor ) );

			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, timeouts, executor, mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), heartbeatContext, mock( typeof( Config ) ) );

			  ClusterMessage.ConfigurationRequestState requestOne = mock( typeof( ClusterMessage.ConfigurationRequestState ) );
			  when( requestOne.JoiningId ).thenReturn( joiningOne );

			  ClusterMessage.ConfigurationRequestState requestTwo = mock( typeof( ClusterMessage.ConfigurationRequestState ) );
			  when( requestTwo.JoiningId ).thenReturn( joiningTwo );

			  // When
			  // Instance 2 contacts us twice and Instance 3 contacts us once
			  context.AddContactingInstance( requestOne, "4, 5" ); // discovery headers are random here
			  context.AddContactingInstance( requestOne, "4, 5" );
			  context.AddContactingInstance( requestTwo, "2, 5" );

			  // Then
			  // The discovery header we generate should still contain one copy of each instance
			  assertEquals( "2,3", context.GenerateDiscoveryHeader() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepTrackOfInstancesWeHaveContacted()
		 public virtual void ShouldKeepTrackOfInstancesWeHaveContacted()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId joiningOne = new InstanceId( 2 );
			  InstanceId joiningTwo = new InstanceId( 3 );

			  CommonContextState commonContextState = mock( typeof( CommonContextState ), RETURNS_MOCKS );
			  Timeouts timeouts = mock( typeof( Timeouts ) );
			  Executor executor = mock( typeof( Executor ) );

			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );

			  ClusterContext context = new ClusterContextImpl( me, commonContextState, NullLogProvider.Instance, timeouts, executor, mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( LearnerContext ) ), heartbeatContext, mock( typeof( Config ) ) );

			  ClusterMessage.ConfigurationRequestState requestOne = mock( typeof( ClusterMessage.ConfigurationRequestState ) );
			  when( requestOne.JoiningId ).thenReturn( joiningOne );

			  ClusterMessage.ConfigurationRequestState requestTwo = mock( typeof( ClusterMessage.ConfigurationRequestState ) );
			  when( requestTwo.JoiningId ).thenReturn( joiningTwo );

			  // When
			  // Instance two contacts us but we are not in the header
			  context.AddContactingInstance( requestOne, "4, 5" );
			  // Then we haven't contacted instance 2
			  assertFalse( context.HaveWeContactedInstance( requestOne ) );

			  // When
			  // Instance 2 reports that we have contacted it after all
			  context.AddContactingInstance( requestOne, "4, 5, 1" );
			  // Then
			  assertTrue( context.HaveWeContactedInstance( requestOne ) );

			  // When
			  // Instance 3 says we have contacted it
			  context.AddContactingInstance( requestTwo, "2, 5, 1" );
			  // Then
			  assertTrue( context.HaveWeContactedInstance( requestTwo ) );

			  // When
			  // For some reason we are not in the header of 3 in subsequent responses (a delayed one, for example)
			  context.AddContactingInstance( requestTwo, "2, 5" );
			  // Then
			  // The state should still keep the fact we've contacted it already
			  assertTrue( context.HaveWeContactedInstance( requestTwo ) );
		 }
	}

}