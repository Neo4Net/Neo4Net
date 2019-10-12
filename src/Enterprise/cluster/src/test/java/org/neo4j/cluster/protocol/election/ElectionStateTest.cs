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
namespace Neo4Net.cluster.protocol.election
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using Mockito = org.mockito.Mockito;


	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using Neo4Net.cluster.protocol;
	using AtomicBroadcastMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;
	using VersionedConfigurationStateChange = Neo4Net.cluster.protocol.cluster.ClusterMessage.VersionedConfigurationStateChange;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
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
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.cluster.ClusterConfiguration.COORDINATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.election.ElectionMessage.demote;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.election.ElectionMessage.performRoleElections;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.election.ElectionMessage.voted;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.election.ElectionState.election;

	public class ElectionStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionRequestIsRejectedIfNoQuorum() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestElectionRequestIsRejectedIfNoQuorum()
		 {
			  ElectionContext context = mock( typeof( ElectionContext ) );
			  ClusterContext clusterContextMock = mock( typeof( ClusterContext ) );
			  when( context.GetLog( ArgumentMatchers.any() ) ).thenReturn(NullLog.Instance);

			  when( context.ElectionOk() ).thenReturn(false);
			  when( clusterContextMock.GetLog( ArgumentMatchers.any() ) ).thenReturn(NullLog.Instance);

			  MessageHolder holder = mock( typeof( MessageHolder ) );

			  election.handle( context, Message.@internal( performRoleElections ), holder );

			  verifyZeroInteractions( holder );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionFromDemoteIsRejectedIfNoQuorum() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestElectionFromDemoteIsRejectedIfNoQuorum()
		 {
			  ElectionContext context = mock( typeof( ElectionContext ) );
			  ClusterContext clusterContextMock = mock( typeof( ClusterContext ) );

			  when( context.ElectionOk() ).thenReturn(false);
			  when( clusterContextMock.GetLog( ArgumentMatchers.any() ) ).thenReturn(NullLog.Instance);
			  when( context.GetLog( ArgumentMatchers.any() ) ).thenReturn(NullLog.Instance);

			  MessageHolder holder = mock( typeof( MessageHolder ) );

			  election.handle( context, Message.@internal( demote ), holder );

			  verifyZeroInteractions( holder );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void electionShouldRemainLocalIfStartedBySingleInstanceWhichIsTheRoleHolder() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ElectionShouldRemainLocalIfStartedBySingleInstanceWhichIsTheRoleHolder()
		 {
			  /*
			   * Ensures that when an instance is alone in the cluster, elections for roles that it holds do not set
			   * timeouts or try to reach other instances.
			   */

			  // Given
			  ElectionContext context = mock( typeof( ElectionContext ) );
			  ClusterContext clusterContextMock = mock( typeof( ClusterContext ) );

			  when( clusterContextMock.GetLog( ArgumentMatchers.any() ) ).thenReturn(NullLog.Instance);
			  MessageHolder holder = mock( typeof( MessageHolder ) );

				 // These mean the election can proceed normally, by us
			  when( context.ElectionOk() ).thenReturn(true);
			  when( context.InCluster ).thenReturn( true );
			  when( context.Elector ).thenReturn( true );

				 // Like it says on the box, we are the only instance
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId myInstanceId = new org.neo4j.cluster.InstanceId(1);
			  InstanceId myInstanceId = new InstanceId( 1 );
			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  members[myInstanceId] = URI.create( "ha://me" );
			  when( context.Members ).thenReturn( members );

				 // Any role would do, just make sure we have it
			  const string role = "master";
			  ElectionContext_VoteRequest voteRequest = new ElectionContext_VoteRequest( role, 13 );
			  when( context.PossibleRoles ).thenReturn( Collections.singletonList( new ElectionRole( role ) ) );
			  when( context.GetElected( role ) ).thenReturn( myInstanceId );
			  when( context.VoteRequestForRole( new ElectionRole( role ) ) ).thenReturn( voteRequest );

				 // Required for logging
			  when( context.GetLog( Mockito.any() ) ).thenReturn(NullLog.Instance);

			  // When
			  election.handle( context, Message.@internal( performRoleElections ), holder );

			  // Then
				 // Make sure that we asked ourselves to vote for that role and that no timer was set
			  verify( holder, times( 1 ) ).offer(ArgumentMatchers.argThat(new MessageArgumentMatcher<ElectionMessage>()
						 .onMessageType( ElectionMessage.Vote ).withPayload( voteRequest )));
			  verify( context, never() ).setTimeout(ArgumentMatchers.any(), ArgumentMatchers.any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void delayedVoteFromPreviousElectionMustNotCauseCurrentElectionToComplete() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DelayedVoteFromPreviousElectionMustNotCauseCurrentElectionToComplete()
		 {
			  // Given
			  ElectionContext context = mock( typeof( ElectionContext ) );
			  MessageHolder holder = mock( typeof( MessageHolder ) );

			  when( context.GetLog( Mockito.any() ) ).thenReturn(NullLog.Instance);

			  const string role = "master";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId voter = new org.neo4j.cluster.InstanceId(2);
			  InstanceId voter = new InstanceId( 2 );

			  ElectionCredentials voteCredentialComparable = mock( typeof( ElectionCredentials ) );
			  Message<ElectionMessage> vote = Message.@internal( voted, new ElectionMessage.VersionedVotedData( role, voter, voteCredentialComparable, 4 ) );

			  when( context.Voted( role, voter, voteCredentialComparable, 4 ) ).thenReturn( false );

			  // When
			  election.handle( context, vote, holder );

			  verify( context ).getLog( ArgumentMatchers.any() );
			  verify( context ).voted( role, voter, voteCredentialComparable, 4 );

			  // Then
			  verifyNoMoreInteractions( context, holder );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void timeoutMakesElectionBeForgotten() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TimeoutMakesElectionBeForgotten()
		 {
			  // Given
			  string coordinatorRole = "coordinator";

			  ElectionContext context = mock( typeof( ElectionContext ) );
			  when( context.GetLog( Mockito.any() ) ).thenReturn(NullLog.Instance);

			  MessageHolder holder = mock( typeof( MessageHolder ) );

			  Message<ElectionMessage> timeout = Message.timeout( ElectionMessage.ElectionTimeout, Message.@internal( performRoleElections ), new ElectionState.ElectionTimeoutData( coordinatorRole, null ) );

			  // When
			  election.handle( context, timeout, holder );

			  // Then
			  verify( context, times( 1 ) ).forgetElection( coordinatorRole );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void electionCompletingMakesItBeForgotten() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ElectionCompletingMakesItBeForgotten()
		 {
			  // Given
			  string coordinatorRole = "coordinator";
			  InstanceId votingInstance = new InstanceId( 2 );
			  ElectionCredentials voteCredentialComparable = mock( typeof( ElectionCredentials ) );

			  ElectionContext context = mock( typeof( ElectionContext ) );
			  when( context.GetLog( Mockito.any() ) ).thenReturn(NullLog.Instance);
			  when( context.NeededVoteCount ).thenReturn( 3 );
			  when( context.GetVoteCount( coordinatorRole ) ).thenReturn( 3 );
			  when( context.Voted( coordinatorRole, votingInstance, voteCredentialComparable, 4 ) ).thenReturn( true );
			  MessageHolder holder = mock( typeof( MessageHolder ) );

			  Message<ElectionMessage> vote = Message.to( ElectionMessage.Voted, URI.create( "cluster://elector" ), new ElectionMessage.VersionedVotedData( coordinatorRole, votingInstance, voteCredentialComparable, 4 ) );

			  // When
			  election.handle( context, vote, holder );

			  // Then
			  verify( context, times( 1 ) ).forgetElection( coordinatorRole );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void voteResponseShouldHaveSameVersionAsVoteRequest() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VoteResponseShouldHaveSameVersionAsVoteRequest()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.cluster.com.message.Message<?>> messages = new java.util.ArrayList<>(1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  IList<Message<object>> messages = new List<Message<object>>( 1 );
			  MessageHolder holder = messages.add;

			  ElectionContext context = mock( typeof( ElectionContext ) );

			  const int version = 14;
			  Message<ElectionMessage> voteRequest = Message.to(ElectionMessage.Vote, URI.create("some://instance"), new ElectionContext_VoteRequest("coordinator", version)
			 );
			  voteRequest.SetHeader( Message.HEADER_FROM, "some://other" );

			  election.handle( context, voteRequest, holder );

			  assertEquals( 1, messages.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.com.message.Message<?> response = messages.get(0);
			  Message<object> response = messages[0];
			  assertEquals( ElectionMessage.Voted, response.MessageType );
			  ElectionMessage.VersionedVotedData payload = response.Payload;
			  assertEquals( version, payload.Version );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAtomicBroadcastOnJoiningAClusterWithAnEstablishedCoordinator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAtomicBroadcastOnJoiningAClusterWithAnEstablishedCoordinator()
		 {
			  // Given
			  string winnerURI = "some://winner";
			  InstanceId winner = new InstanceId( 2 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.cluster.com.message.Message<?>> messages = new java.util.ArrayList<>(1);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  IList<Message<object>> messages = new List<Message<object>>( 1 );
			  MessageHolder holder = messages.add;
			  ElectionCredentials voteCredentialComparable = mock( typeof( ElectionCredentials ) );

			  ElectionContext electionContext = mock( typeof( ElectionContext ) );
			  when( electionContext.Voted( eq( COORDINATOR ), eq( new InstanceId( 1 ) ), eq( voteCredentialComparable ), anyLong() ) ).thenReturn(true);
			  when( electionContext.GetVoteCount( COORDINATOR ) ).thenReturn( 3 );
			  when( electionContext.NeededVoteCount ).thenReturn( 3 );
			  when( electionContext.GetElectionWinner( COORDINATOR ) ).thenReturn( winner );

			  when( electionContext.GetLog( any( typeof( Type ) ) ) ).thenReturn( NullLog.Instance );
			  VersionedConfigurationStateChange stateChange = mock( typeof( VersionedConfigurationStateChange ) );
			  when( electionContext.NewConfigurationStateChange() ).thenReturn(stateChange);

			  when( electionContext.GetUriForId( winner ) ).thenReturn( URI.create( winnerURI ) );

			  // When
			  Message<ElectionMessage> votedMessage = Message.to( ElectionMessage.Voted, URI.create( "some://instance" ), new ElectionMessage.VotedData( COORDINATOR, new InstanceId( 1 ), voteCredentialComparable ) );
			  votedMessage.SetHeader( Message.HEADER_FROM, "some://other" );

			  election.handle( electionContext, votedMessage, holder );

			  // Then
			  assertEquals( 1, messages.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.cluster.com.message.Message<?> message = messages.get(0);
			  Message<object> message = messages[0];
			  assertEquals( AtomicBroadcastMessage.broadcast, message.MessageType );
		 }
	}

}