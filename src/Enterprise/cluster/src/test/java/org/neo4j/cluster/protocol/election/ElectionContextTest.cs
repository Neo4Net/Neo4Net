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
namespace Neo4Net.cluster.protocol.election
{

	using Test = org.junit.Test;

	using ObjectInputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using AcceptorInstanceStore = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using MultiPaxosContext = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;
	using HeartbeatContext = Neo4Net.cluster.protocol.heartbeat.HeartbeatContext;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ElectionContextTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionOkNoFailed()
		 public virtual void TestElectionOkNoFailed()
		 {
			  ISet<InstanceId> failed = new HashSet<InstanceId>();

			  BaseTestForElectionOk( failed, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionOkLessThanQuorumFailed()
		 public virtual void TestElectionOkLessThanQuorumFailed()
		 {
			  ISet<InstanceId> failed = new HashSet<InstanceId>();
			  failed.Add( new InstanceId( 1 ) );

			  BaseTestForElectionOk( failed, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionNotOkMoreThanQuorumFailed()
		 public virtual void TestElectionNotOkMoreThanQuorumFailed()
		 {
			  ISet<InstanceId> failed = new HashSet<InstanceId>();
			  failed.Add( new InstanceId( 1 ) );
			  failed.Add( new InstanceId( 2 ) );

			  BaseTestForElectionOk( failed, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionNotOkQuorumFailedTwoInstances()
		 public virtual void TestElectionNotOkQuorumFailedTwoInstances()
		 {
			  ISet<InstanceId> failed = new HashSet<InstanceId>();
			  failed.Add( new InstanceId( 2 ) );

			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  members[new InstanceId( 1 )] = URI.create( "server1" );
			  members[new InstanceId( 2 )] = URI.create( "server2" );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.Members ).thenReturn( members );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );
			  when( clusterContext.Configuration ).thenReturn( clusterConfiguration );

			  MultiPaxosContext context = new MultiPaxosContext( new InstanceId( 1 ), Iterables.iterable( new ElectionRole( "coordinator" ) ), clusterConfiguration, mock( typeof( Executor ) ), NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  context.HeartbeatContext.Failed.addAll( failed );

			  ElectionContext toTest = context.ElectionContext;

			  assertFalse( toTest.ElectionOk() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionNotOkQuorumFailedFourInstances()
		 public virtual void TestElectionNotOkQuorumFailedFourInstances()
		 {
			  ISet<InstanceId> failed = new HashSet<InstanceId>();
			  failed.Add( new InstanceId( 2 ) );
			  failed.Add( new InstanceId( 3 ) );

			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  members[new InstanceId( 1 )] = URI.create( "server1" );
			  members[new InstanceId( 2 )] = URI.create( "server2" );
			  members[new InstanceId( 3 )] = URI.create( "server3" );
			  members[new InstanceId( 4 )] = URI.create( "server4" );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.Members ).thenReturn( members );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );
			  when( clusterContext.Configuration ).thenReturn( clusterConfiguration );

			  MultiPaxosContext context = new MultiPaxosContext( new InstanceId( 1 ), Iterables.iterable( new ElectionRole( "coordinator" ) ), clusterConfiguration, mock( typeof( Executor ) ), NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  context.HeartbeatContext.Failed.addAll( failed );

			  ElectionContext toTest = context.ElectionContext;

			  assertFalse( toTest.ElectionOk() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionNotOkQuorumFailedFiveInstances()
		 public virtual void TestElectionNotOkQuorumFailedFiveInstances()
		 {
			  ISet<InstanceId> failed = new HashSet<InstanceId>();
			  failed.Add( new InstanceId( 2 ) );
			  failed.Add( new InstanceId( 3 ) );
			  failed.Add( new InstanceId( 4 ) );

			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  members[new InstanceId( 1 )] = URI.create( "server1" );
			  members[new InstanceId( 2 )] = URI.create( "server2" );
			  members[new InstanceId( 3 )] = URI.create( "server3" );
			  members[new InstanceId( 4 )] = URI.create( "server4" );
			  members[new InstanceId( 5 )] = URI.create( "server5" );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.Members ).thenReturn( members );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );
			  when( clusterContext.Configuration ).thenReturn( clusterConfiguration );

			  MultiPaxosContext context = new MultiPaxosContext( new InstanceId( 1 ), Iterables.iterable( new ElectionRole( "coordinator" ) ), clusterConfiguration, mock( typeof( Executor ) ), NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  context.HeartbeatContext.Failed.addAll( failed );

			  ElectionContext toTest = context.ElectionContext;

			  assertFalse( toTest.ElectionOk() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInstanceWithLowestIdFailedIsNotConsideredTheElector()
		 public virtual void TestInstanceWithLowestIdFailedIsNotConsideredTheElector()
		 {
			  // Given
			  // A cluster of 5 of which the two lowest instances are failed
			  ISet<InstanceId> failed = new HashSet<InstanceId>();
			  failed.Add( new InstanceId( 1 ) );
			  failed.Add( new InstanceId( 2 ) );

			  // This is the instance that must discover that it is the elector and whose state machine we'll test
			  InstanceId lowestNonFailedInstanceId = new InstanceId( 3 );

			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  members[new InstanceId( 1 )] = URI.create( "server1" );
			  members[new InstanceId( 2 )] = URI.create( "server2" );
			  members[lowestNonFailedInstanceId] = URI.create( "server3" );
			  members[new InstanceId( 4 )] = URI.create( "server4" );
			  members[new InstanceId( 5 )] = URI.create( "server5" );

			  Config config = Config.defaults();
			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.Members ).thenReturn( members );
			  when( clusterConfiguration.MemberIds ).thenReturn( members.Keys );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );
			  when( clusterContext.Configuration ).thenReturn( clusterConfiguration );

			  MultiPaxosContext context = new MultiPaxosContext( lowestNonFailedInstanceId, Iterables.iterable( new ElectionRole( "coordinator" ) ), clusterConfiguration, mock( typeof( Executor ) ), NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  ElectionContext toTest = context.ElectionContext;
			  // Sanity check that before learning about lowest failed ids it does not consider itself the elector
			  assertFalse( toTest.Elector );

			  // When
			  // The lowest numbered alive instance receives word about other failed instances
			  context.HeartbeatContext.Failed.addAll( failed );

			  // Then
			  // It should realise it is the elector (lowest instance id alive)
			  assertTrue( toTest.Elector );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoVotesFromSameInstanceForSameRoleShouldBeConsolidated()
		 public virtual void TwoVotesFromSameInstanceForSameRoleShouldBeConsolidated()
		 {
			  // Given
			  const string coordinatorRole = "coordinator";
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  when( heartbeatContext.Failed ).thenReturn( Collections.emptySet() );

			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  members[new InstanceId( 1 )] = URI.create( "server1" );
			  members[new InstanceId( 2 )] = URI.create( "server2" );
			  members[new InstanceId( 3 )] = URI.create( "server3" );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.Members ).thenReturn( members );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );
			  when( clusterContext.Configuration ).thenReturn( clusterConfiguration );

			  MultiPaxosContext context = new MultiPaxosContext( new InstanceId( 1 ), Iterables.iterable( new ElectionRole( coordinatorRole ) ), clusterConfiguration, mock( typeof( Executor ) ), NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  ElectionContext toTest = context.ElectionContext;

			  // When
			  toTest.StartElectionProcess( coordinatorRole );
			  toTest.Voted( coordinatorRole, new InstanceId( 1 ), new IntegerElectionCredentials( 100 ), Neo4Net.cluster.protocol.cluster.ClusterContext_Fields.NO_ELECTOR_VERSION );
			  toTest.Voted( coordinatorRole, new InstanceId( 2 ), new IntegerElectionCredentials( 100 ), Neo4Net.cluster.protocol.cluster.ClusterContext_Fields.NO_ELECTOR_VERSION );
			  toTest.Voted( coordinatorRole, new InstanceId( 2 ), new IntegerElectionCredentials( 101 ), Neo4Net.cluster.protocol.cluster.ClusterContext_Fields.NO_ELECTOR_VERSION );

			  // Then
			  assertNull( toTest.GetElectionWinner( coordinatorRole ) );
			  assertEquals( 2, toTest.GetVoteCount( coordinatorRole ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void electionBeingForgottenMustIncreaseElectionId()
		 public virtual void ElectionBeingForgottenMustIncreaseElectionId()
		 {
			  // Given
			  const string coordinatorRole = "coordinator";
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  when( heartbeatContext.Failed ).thenReturn( Collections.emptySet() );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ElectionContext context = ( new MultiPaxosContext( new InstanceId( 1 ), Iterables.iterable( new ElectionRole( coordinatorRole ) ), mock( typeof( ClusterConfiguration ) ), mock( typeof( Executor ) ), NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config ) ).ElectionContext;

			  ElectionContext_VoteRequest voteRequestBefore = context.VoteRequestForRole( new ElectionRole( coordinatorRole ) );
			  context.ForgetElection( coordinatorRole );
			  ElectionContext_VoteRequest voteRequestAfter = context.VoteRequestForRole( new ElectionRole( coordinatorRole ) );
			  assertEquals( voteRequestBefore.Version + 1, voteRequestAfter.Version );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void voteFromPreviousSuccessfulElectionMustNotBeCounted()
		 public virtual void VoteFromPreviousSuccessfulElectionMustNotBeCounted()
		 {
			  // Given
			  const string coordinatorRole = "coordinator";
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  when( heartbeatContext.Failed ).thenReturn( Collections.emptySet() );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ElectionContext context = ( new MultiPaxosContext( new InstanceId( 1 ), Iterables.iterable( new ElectionRole( coordinatorRole ) ), mock( typeof( ClusterConfiguration ) ), mock( typeof( Executor ) ), NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config ) ).ElectionContext;

			  // When
			  ElectionContext_VoteRequest voteRequestBefore = context.VoteRequestForRole( new ElectionRole( coordinatorRole ) );
			  context.ForgetElection( coordinatorRole );

			  // Then
			  assertFalse( context.Voted( coordinatorRole, new InstanceId( 2 ), null, voteRequestBefore.Version - 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void instanceFailingShouldHaveItsVotesInvalidated()
		 public virtual void InstanceFailingShouldHaveItsVotesInvalidated()
		 {
			  // Given
			  const string role1 = "coordinator1";
			  const string role2 = "coordinator2";
			  InstanceId me = new InstanceId( 1 );
			  InstanceId failingInstance = new InstanceId( 2 );
			  InstanceId otherInstance = new InstanceId( 3 );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  IList<InstanceId> clusterMemberIds = new LinkedList<InstanceId>();
			  clusterMemberIds.Add( failingInstance );
			  clusterMemberIds.Add( otherInstance );
			  clusterMemberIds.Add( me );
			  when( clusterConfiguration.MemberIds ).thenReturn( clusterMemberIds );

			  MultiPaxosContext context = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( role1 ), new ElectionRole( role2 ) ), clusterConfiguration, ThreadStart.run, NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  HeartbeatContext heartbeatContext = context.HeartbeatContext;
			  ElectionContext electionContext = context.ElectionContext;

			  electionContext.StartElectionProcess( role1 );
			  electionContext.StartElectionProcess( role2 );

			  electionContext.Voted( role1, failingInstance, mock( typeof( ElectionCredentials ) ), 2 );
			  electionContext.Voted( role2, failingInstance, mock( typeof( ElectionCredentials ) ), 2 );

			  electionContext.Voted( role1, otherInstance, mock( typeof( ElectionCredentials ) ), 2 );
			  electionContext.Voted( role2, otherInstance, mock( typeof( ElectionCredentials ) ), 2 );

			  heartbeatContext.Suspect( failingInstance );

			  assertEquals( 1, electionContext.GetVoteCount( role1 ) );
			  assertEquals( 1, electionContext.GetVoteCount( role2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedElectorRejoiningMustHaveItsVersionFromVoteRequestsSetTheElectorVersion()
		 public virtual void FailedElectorRejoiningMustHaveItsVersionFromVoteRequestsSetTheElectorVersion()
		 {
			  // Given
			  const string role1 = "coordinator1";
			  InstanceId me = new InstanceId( 1 );
			  InstanceId failingInstance = new InstanceId( 2 );
			  InstanceId forQuorum = new InstanceId( 3 );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  IList<InstanceId> clusterMemberIds = new LinkedList<InstanceId>();
			  clusterMemberIds.Add( failingInstance );
			  clusterMemberIds.Add( me );
			  clusterMemberIds.Add( forQuorum );
			  when( clusterConfiguration.MemberIds ).thenReturn( clusterMemberIds );

			  MultiPaxosContext context = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( role1 ) ), clusterConfiguration, ThreadStart.run, NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  HeartbeatContext heartbeatContext = context.HeartbeatContext;
			  ClusterContext clusterContext = context.ClusterContext;

			  clusterContext.LastElector = failingInstance;
			  clusterContext.LastElectorVersion = 8;

			  // When the elector fails
			  heartbeatContext.Suspicions( forQuorum, Collections.singleton( failingInstance ) );
			  heartbeatContext.Suspect( failingInstance );

			  // Then the elector is reset to defaults
			  assertEquals( clusterContext.LastElector, InstanceId.NONE );
			  assertEquals( clusterContext.LastElectorVersion, Neo4Net.cluster.protocol.cluster.ClusterContext_Fields.NO_ELECTOR_VERSION );

			  // When the elector comes back with an election result
			  clusterContext.Elected( role1, forQuorum, failingInstance, 9 );

			  // Then the result is actually respected
			  assertEquals( clusterContext.LastElector, failingInstance );
			  assertEquals( clusterContext.LastElectorVersion, 9 );
		 }

		 /*
		  * This assumes an instance leaves the cluster normally and then rejoins, without any elections in between. The
		  * expected result is that it will succeed in sending election results.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void electorLeavingAndRejoiningWithNoElectionsInBetweenMustStillHaveElectionsGoThrough()
		 public virtual void ElectorLeavingAndRejoiningWithNoElectionsInBetweenMustStillHaveElectionsGoThrough()
		 {
			  // Given
			  const string role1 = "coordinator1";
			  InstanceId me = new InstanceId( 1 );
			  InstanceId leavingInstance = new InstanceId( 2 );
			  InstanceId forQuorum = new InstanceId( 3 );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  IList<InstanceId> clusterMemberIds = new LinkedList<InstanceId>();
			  clusterMemberIds.Add( leavingInstance );
			  clusterMemberIds.Add( me );
			  clusterMemberIds.Add( forQuorum );
			  when( clusterConfiguration.MemberIds ).thenReturn( clusterMemberIds );

			  MultiPaxosContext context = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( role1 ) ), clusterConfiguration, ThreadStart.run, NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  ClusterContext clusterContext = context.ClusterContext;

			  clusterContext.LastElector = leavingInstance;
			  clusterContext.LastElectorVersion = 8;

			  // When the elector leaves the cluster
			  clusterContext.Left( leavingInstance );

			  // Then the elector is reset to defaults
			  assertEquals( clusterContext.LastElector, InstanceId.NONE );
			  assertEquals( clusterContext.LastElectorVersion, Neo4Net.cluster.protocol.cluster.ClusterContext_Fields.NO_ELECTOR_VERSION );

			  // When the elector comes back with an election result
				 // We don't need to join, election results do not check for elector membership
			  clusterContext.Elected( role1, forQuorum, leavingInstance, 9 );

			  // Then the result is actually respected
			  assertEquals( clusterContext.LastElector, leavingInstance );
			  assertEquals( clusterContext.LastElectorVersion, 9 );
		 }

		 private void BaseTestForElectionOk( ISet<InstanceId> failed, bool moreThanQuorum )
		 {
			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  members[new InstanceId( 1 )] = URI.create( "server1" );
			  members[new InstanceId( 2 )] = URI.create( "server2" );
			  members[new InstanceId( 3 )] = URI.create( "server3" );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.Members ).thenReturn( members );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );
			  when( clusterContext.Configuration ).thenReturn( clusterConfiguration );

			  MultiPaxosContext context = new MultiPaxosContext( new InstanceId( 1 ), Iterables.iterable( new ElectionRole( "coordinator" ) ), clusterConfiguration, mock( typeof( Executor ) ), NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );

			  context.HeartbeatContext.Failed.addAll( failed );

			  ElectionContext toTest = context.ElectionContext;

			  assertEquals( moreThanQuorum, !toTest.ElectionOk() );
		 }
	}

}