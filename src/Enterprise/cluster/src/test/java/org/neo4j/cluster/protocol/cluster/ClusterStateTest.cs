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
namespace Neo4Net.cluster.protocol.cluster
{

	using Test = org.junit.Test;
	using ArgumentMatcher = org.mockito.ArgumentMatcher;

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using TrackingMessageHolder = Neo4Net.cluster.com.message.TrackingMessageHolder;
	using ConfigurationRequestState = Neo4Net.cluster.protocol.cluster.ClusterMessage.ConfigurationRequestState;
	using ConfigurationResponseState = Neo4Net.cluster.protocol.cluster.ClusterMessage.ConfigurationResponseState;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.DISCOVERED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.@internal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.to;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.cluster.ClusterMessage.configurationRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.cluster.ClusterMessage.configurationTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.cluster.ClusterMessage.joinDenied;

	public class ClusterStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void joinDeniedResponseShouldContainRespondersConfiguration() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void JoinDeniedResponseShouldContainRespondersConfiguration()
		 {
			  // GIVEN
			  ClusterContext context = mock( typeof( ClusterContext ) );
			  IDictionary<InstanceId, URI> existingMembers = Members( 1, 2 );
			  when( context.IsCurrentlyAlive( any( typeof( InstanceId ) ) ) ).thenReturn( true );
			  when( context.Members ).thenReturn( existingMembers );
			  when( context.Configuration ).thenReturn( ClusterConfiguration( existingMembers ) );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  TrackingMessageHolder outgoing = new TrackingMessageHolder();
			  Message<ClusterMessage> message = to( configurationRequest, Uri( 1 ), Configuration( 2 ) ).setHeader( Message.HEADER_FROM, Uri( 2 ).ToString() );

			  // WHEN an instance responds to a join request, responding that the joining instance cannot join
			  ClusterState.Entered.handle( context, message, outgoing );

			  // THEN assert that the responding instance sends its configuration along with the response
			  Message<ClusterMessage> response = outgoing.Single();
			  assertTrue( response.Payload is ConfigurationResponseState );
			  ConfigurationResponseState responseState = response.Payload;
			  assertEquals( existingMembers, responseState.Members );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void joinDeniedHandlingShouldKeepResponseConfiguration() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void JoinDeniedHandlingShouldKeepResponseConfiguration()
		 {
			  // GIVEN
			  ClusterContext context = mock( typeof( ClusterContext ) );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  TrackingMessageHolder outgoing = new TrackingMessageHolder();
			  IDictionary<InstanceId, URI> members = members( 1, 2 );

			  // WHEN a joining instance receives a denial to join
			  ClusterState.Discovery.handle( context, to( joinDenied, Uri( 2 ), ConfigurationResponseState( members ) ), outgoing );

			  // THEN assert that the response contains the configuration
			  verify( context ).joinDenied( argThat( ( new ConfigurationResponseStateMatcher() ).WithMembers(members) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void joinDeniedTimeoutShouldBeHandledWithExceptionIncludingConfiguration() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void JoinDeniedTimeoutShouldBeHandledWithExceptionIncludingConfiguration()
		 {
			  // GIVEN
			  ClusterContext context = mock( typeof( ClusterContext ) );
			  IDictionary<InstanceId, URI> existingMembers = Members( 1, 2 );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  when( context.JoiningInstances ).thenReturn( Collections.emptyList() );
			  when( context.HasJoinBeenDenied() ).thenReturn(true);
			  when( context.JoinDeniedConfigurationResponseState ).thenReturn( ConfigurationResponseState( existingMembers ) );
			  TrackingMessageHolder outgoing = new TrackingMessageHolder();

			  // WHEN the join denial actually takes effect (signaled by a join timeout locally)
			  ClusterState.Joining.handle( context, to( ClusterMessage.JoiningTimeout, Uri( 2 ) ).setHeader( Message.HEADER_CONVERSATION_ID, "bla" ), outgoing );

			  // THEN assert that the failure contains the received configuration
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> response = outgoing.single();
			  Message<MessageType> response = outgoing.Single();
			  ClusterEntryDeniedException deniedException = response.Payload;
			  assertEquals( existingMembers, deniedException.ConfigurationResponseState.Members );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDenyJoinToInstanceThatRejoinsBeforeTimingOut() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDenyJoinToInstanceThatRejoinsBeforeTimingOut()
		 {
			  // GIVEN
			  ClusterContext context = mock( typeof( ClusterContext ) );
			  IDictionary<InstanceId, URI> existingMembers = Members( 1, 2 );
			  when( context.IsCurrentlyAlive( Id( 2 ) ) ).thenReturn( true );
			  when( context.Members ).thenReturn( existingMembers );
			  when( context.Configuration ).thenReturn( ClusterConfiguration( existingMembers ) );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  when( context.GetUriForId( Id( 2 ) ) ).thenReturn( Uri( 2 ) );
			  TrackingMessageHolder outgoing = new TrackingMessageHolder();
			  Message<ClusterMessage> message = to( configurationRequest, Uri( 1 ), Configuration( 2 ) ).setHeader( Message.HEADER_FROM, Uri( 2 ).ToString() );

			  // WHEN the join denial actually takes effect (signaled by a join timeout locally)
			  ClusterState.Entered.handle( context, message, outgoing );

			  // THEN assert that the failure contains the received configuration
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> response = outgoing.single();
			  Message<MessageType> response = outgoing.Single();
			  assertEquals( ClusterMessage.ConfigurationResponse, response.MessageType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void discoveredInstancesShouldBeOnlyOnesWeHaveContactedDirectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DiscoveredInstancesShouldBeOnlyOnesWeHaveContactedDirectly()
		 {
			  // GIVEN
			  ClusterContext context = mock( typeof( ClusterContext ) );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  when( context.GetUriForId( Id( 2 ) ) ).thenReturn( Uri( 2 ) );

			  IList<ConfigurationRequestState> discoveredInstances = new LinkedList<ConfigurationRequestState>();
			  when( context.DiscoveredInstances ).thenReturn( discoveredInstances );
			  when( context.ShouldFilterContactingInstances() ).thenReturn(true);

			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  ConfigurationRequestState configurationRequestFromTwo = Configuration( 2 );
			  Message<ClusterMessage> message = to( configurationRequest, Uri( 1 ), configurationRequestFromTwo ).setHeader( Message.HEADER_FROM, Uri( 2 ).ToString() );

			  // WHEN
			  // We receive a configuration request from an instance which we haven't contacted
			  ClusterState.Discovery.handle( context, message, outgoing );

			  // THEN
			  // It shouldn't be added to the discovered instances
			  assertTrue( discoveredInstances.Count == 0 );

			  // WHEN
			  // It subsequently contacts us
			  when( context.HaveWeContactedInstance( configurationRequestFromTwo ) ).thenReturn( true );
			  ClusterState.Discovery.handle( context, message, outgoing );

			  // Then
			  assertTrue( discoveredInstances.Contains( configurationRequestFromTwo ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void discoveredInstancesShouldNotFilterByDefault() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DiscoveredInstancesShouldNotFilterByDefault()
		 {
			  // GIVEN
			  ClusterContext context = mock( typeof( ClusterContext ) );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  when( context.GetUriForId( Id( 2 ) ) ).thenReturn( Uri( 2 ) );
			  when( context.GetUriForId( Id( 3 ) ) ).thenReturn( Uri( 3 ) );

			  IList<ConfigurationRequestState> discoveredInstances = new LinkedList<ConfigurationRequestState>();
			  when( context.DiscoveredInstances ).thenReturn( discoveredInstances );

			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  ConfigurationRequestState configurationRequestFromTwo = Configuration( 2 );
			  Message<ClusterMessage> messageFromTwo = to( configurationRequest, Uri( 1 ), configurationRequestFromTwo ).setHeader( Message.HEADER_FROM, Uri( 2 ).ToString() );
			  ConfigurationRequestState configurationRequestFromThree = Configuration( 3 );
			  Message<ClusterMessage> messageFromThree = to( configurationRequest, Uri( 1 ), configurationRequestFromThree ).setHeader( Message.HEADER_FROM, Uri( 3 ).ToString() );

			  // WHEN
			  // We receive a configuration request from an instance which we haven't contacted
			  ClusterState.Discovery.handle( context, messageFromTwo, outgoing );

			  // THEN
			  // Since the setting is on, it should be added to the list anyway
			  assertTrue( discoveredInstances.Contains( configurationRequestFromTwo ) );

			  // WHEN
			  // Another contacts us as well
			  ClusterState.Discovery.handle( context, messageFromThree, outgoing );

			  // Then
			  // That should be in as well
			  assertTrue( discoveredInstances.Contains( configurationRequestFromTwo ) );
			  assertTrue( discoveredInstances.Contains( configurationRequestFromThree ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetDiscoveryHeaderProperly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetDiscoveryHeaderProperly()
		 {
			  // GIVEN
			  ClusterContext context = mock( typeof( ClusterContext ) );
			  when( context.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  when( context.GetUriForId( Id( 2 ) ) ).thenReturn( Uri( 2 ) );
			  when( context.JoiningInstances ).thenReturn( singletonList( Uri( 2 ) ) );

			  IList<ConfigurationRequestState> discoveredInstances = new LinkedList<ConfigurationRequestState>();
			  when( context.DiscoveredInstances ).thenReturn( discoveredInstances );

			  TrackingMessageHolder outgoing = new TrackingMessageHolder();
			  ClusterMessage.ConfigurationTimeoutState timeoutState = new ClusterMessage.ConfigurationTimeoutState( 3 );
			  Message<ClusterMessage> message = @internal( configurationTimeout, timeoutState );
			  string discoveryHeader = "1,2,3";
			  when( context.GenerateDiscoveryHeader() ).thenReturn(discoveryHeader);

			  // WHEN
			  // We receive a configuration request from an instance which we haven't contacted
			  ClusterState.Discovery.handle( context, message, outgoing );

			  // THEN
			  // It shouldn't be added to the discovered instances
			  assertEquals( discoveryHeader, outgoing.First().getHeader(DISCOVERED) );
		 }

		 private ConfigurationResponseState ConfigurationResponseState( IDictionary<InstanceId, URI> existingMembers )
		 {
			  return new ConfigurationResponseState( Collections.emptyMap(), existingMembers, null, Collections.emptySet(), "ClusterStateTest" );
		 }

		 private ClusterConfiguration ClusterConfiguration( IDictionary<InstanceId, URI> members )
		 {
			  ClusterConfiguration config = new ClusterConfiguration( "ClusterStateTest", NullLogProvider.Instance );
			  config.Members = members;
			  return config;
		 }

		 private IDictionary<InstanceId, URI> Members( params int[] memberIds )
		 {
			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  foreach ( int memberId in memberIds )
			  {
					members[new InstanceId( memberId )] = Uri( memberId );
			  }
			  return members;
		 }

		 private ConfigurationRequestState Configuration( int joiningInstance )
		 {
			  return new ConfigurationRequestState( new InstanceId( joiningInstance ), Uri( joiningInstance ) );
		 }

		 private URI Uri( int i )
		 {
			  return URI.create( "http://localhost:" + ( 6000 + i ) + "?serverId=" + i );
		 }

		 private InstanceId Id( int i )
		 {
			  return new InstanceId( i );
		 }

		 private class ConfigurationResponseStateMatcher : ArgumentMatcher<ConfigurationResponseState>
		 {
			  internal IDictionary<InstanceId, URI> Members;

			  public virtual ConfigurationResponseStateMatcher WithMembers( IDictionary<InstanceId, URI> members )
			  {
					this.Members = members;
					return this;
			  }

			  public override bool Matches( ConfigurationResponseState argument )
			  {
					return argument.Members.Equals( this.Members );
			  }
		 }
	}

}