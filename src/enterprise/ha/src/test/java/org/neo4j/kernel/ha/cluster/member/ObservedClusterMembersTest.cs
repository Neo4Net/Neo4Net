/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha.cluster.member
{
	using Test = org.junit.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;
	using Mockito = org.mockito.Mockito;

	using OnlineBackupKernelExtension = Neo4Net.backup.OnlineBackupKernelExtension;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterMemberEvents = Neo4Net.cluster.member.ClusterMemberEvents;
	using ClusterMemberListener = Neo4Net.cluster.member.ClusterMemberListener;
	using Cluster = Neo4Net.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using Heartbeat = Neo4Net.cluster.protocol.heartbeat.Heartbeat;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.cluster.member.ClusterMemberMatcher.sameMemberAs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;

	public class ObservedClusterMembersTest
	{
		 private static readonly LogProvider _logProvider = NullLogProvider.Instance;
		 private static readonly InstanceId _clusterId1 = new InstanceId( 1 );
		 private static readonly InstanceId _clusterId2 = new InstanceId( 2 );
		 private static readonly InstanceId _clusterId3 = new InstanceId( 3 );
		 private static readonly URI _clusterUri1 = create( "cluster://server1" );
		 private static readonly URI _clusterUri2 = create( "cluster://server2" );
		 private static readonly URI _clusterUri3 = create( "cluster://server3" );
		 private static readonly URI _haUri1 = create( "ha://server1?serverId=" + _clusterId1.toIntegerIndex() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRegisterItselfOnListeners()
		 public virtual void ShouldRegisterItselfOnListeners()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents clusterMemberEvents = mock( typeof( ClusterMemberEvents ) );

			  // when
			  new ObservedClusterMembers( _logProvider, cluster, heartbeat, clusterMemberEvents, null );

			  // then
			  verify( cluster ).addClusterListener( Mockito.any() );
			  verify( heartbeat ).addHeartbeatListener( Mockito.any() );
			  verify( clusterMemberEvents ).addClusterMemberListener( Mockito.any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainMemberListAfterEnteringCluster()
		 public virtual void ShouldContainMemberListAfterEnteringCluster()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, null );

			  // when
			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  // then
			  assertThat( members.Members, hasItems( sameMemberAs( new ClusterMember( _clusterId1 ) ), sameMemberAs( new ClusterMember( _clusterId2 ) ), sameMemberAs( new ClusterMember( _clusterId3 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void joinedMemberShowsInList()
		 public virtual void JoinedMemberShowsInList()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, null );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );

			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2 ) );

			  // when
			  listener.Value.joinedCluster( _clusterId3, _clusterUri3 );

			  // then
			  assertThat( members.Members, hasItems( sameMemberAs( new ClusterMember( _clusterId1 ) ), sameMemberAs( new ClusterMember( _clusterId2 ) ), sameMemberAs( new ClusterMember( _clusterId3 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void iCanGetToMyself()
		 public virtual void ICanGetToMyself()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, _clusterId1 );

			  // when

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );

			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2 ) );

			  ClusterMember me = members.CurrentMember;
			  assertNotNull( me );
			  assertEquals( 1, me.InstanceId.toIntegerIndex() );
			  assertEquals( _clusterId1, me.InstanceId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leftMemberDisappearsFromList()
		 public virtual void LeftMemberDisappearsFromList()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, null );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );

			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  // when
			  listener.Value.leftCluster( _clusterId3, _clusterUri3 );

			  // then
			  assertThat( members.Members, not( hasItems( sameMemberAs( new ClusterMember( _clusterId3 ) ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void availableMasterShowsProperInformation()
		 public virtual void AvailableMasterShowsProperInformation()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, null );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<ClusterMemberListener> memberListener = ArgumentCaptor.forClass( typeof( ClusterMemberListener ) );
			  verify( memberEvents ).addClusterMemberListener( memberListener.capture() );

			  // when
			  memberListener.Value.memberIsAvailable( MASTER, _clusterId1, _haUri1, StoreId.DEFAULT );

			  // then
			  assertThat( members.Members, hasItem( sameMemberAs( ( new ClusterMember( _clusterId1 ) ).availableAs( MASTER, _haUri1, StoreId.DEFAULT ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void availableSlaveShowsProperInformation()
		 public virtual void AvailableSlaveShowsProperInformation()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, null );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<ClusterMemberListener> memberListener = ArgumentCaptor.forClass( typeof( ClusterMemberListener ) );
			  verify( memberEvents ).addClusterMemberListener( memberListener.capture() );

			  // when
			  memberListener.Value.memberIsAvailable( SLAVE, _clusterId1, _haUri1, StoreId.DEFAULT );

			  // then
			  assertThat( members.Members, hasItem( sameMemberAs( ( new ClusterMember( _clusterId1 ) ).availableAs( SLAVE, _haUri1, StoreId.DEFAULT ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void membersShowsAsUnavailableWhenNewMasterElectedBeforeTheyBecomeAvailable()
		 public virtual void MembersShowsAsUnavailableWhenNewMasterElectedBeforeTheyBecomeAvailable()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, null );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<ClusterMemberListener> memberListener = ArgumentCaptor.forClass( typeof( ClusterMemberListener ) );
			  verify( memberEvents ).addClusterMemberListener( memberListener.capture() );
			  memberListener.Value.memberIsAvailable( SLAVE, _clusterId1, _haUri1, StoreId.DEFAULT );

			  // when
			  memberListener.Value.coordinatorIsElected( _clusterId2 );

			  // then
			  assertThat( members.Members, hasItem( sameMemberAs( new ClusterMember( _clusterId1 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedMemberShowsAsSuch()
		 public virtual void FailedMemberShowsAsSuch()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, null );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<HeartbeatListener> heartBeatListener = ArgumentCaptor.forClass( typeof( HeartbeatListener ) );
			  verify( heartbeat ).addHeartbeatListener( heartBeatListener.capture() );

			  // when
			  heartBeatListener.Value.failed( _clusterId1 );

			  // then
			  assertThat( members.Members, hasItem( sameMemberAs( ( new ClusterMember( _clusterId1 ) ).failed() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedThenAliveMemberShowsAsAlive()
		 public virtual void FailedThenAliveMemberShowsAsAlive()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, null );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<HeartbeatListener> heartBeatListener = ArgumentCaptor.forClass( typeof( HeartbeatListener ) );
			  verify( heartbeat ).addHeartbeatListener( heartBeatListener.capture() );

			  // when
			  heartBeatListener.Value.failed( _clusterId1 );
			  heartBeatListener.Value.alive( _clusterId1 );

			  // then
			  assertThat( members.Members, hasItem( sameMemberAs( new ClusterMember( _clusterId1 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingMasterUnavailabilityEventDoesNotClobberState()
		 public virtual void MissingMasterUnavailabilityEventDoesNotClobberState()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, _clusterId1 );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<ClusterMemberListener> memberListener = ArgumentCaptor.forClass( typeof( ClusterMemberListener ) );
			  verify( memberEvents ).addClusterMemberListener( memberListener.capture() );

			  // when
			  // first we are available as slaves
			  memberListener.Value.memberIsAvailable( SLAVE, _clusterId1, _haUri1, StoreId.DEFAULT );
			  // and then for some reason as master, without an unavailable message in between
			  memberListener.Value.memberIsAvailable( MASTER, _clusterId1, _haUri1, StoreId.DEFAULT );

			  // then
			  assertThat( members.CurrentMember.HARole, equalTo( MASTER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingSlaveUnavailabilityEventDoesNotClobberState()
		 public virtual void MissingSlaveUnavailabilityEventDoesNotClobberState()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, _clusterId1 );

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<ClusterMemberListener> memberListener = ArgumentCaptor.forClass( typeof( ClusterMemberListener ) );
			  verify( memberEvents ).addClusterMemberListener( memberListener.capture() );

			  // when
			  // first we are available as master
			  memberListener.Value.memberIsAvailable( MASTER, _clusterId1, _haUri1, StoreId.DEFAULT );
			  // and then for some reason as slave, without an unavailable message in between
			  memberListener.Value.memberIsAvailable( SLAVE, _clusterId1, _haUri1, StoreId.DEFAULT );

			  // then
			  assertThat( members.CurrentMember.HARole, equalTo( SLAVE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingMasterUnavailabilityEventForOtherInstanceStillRemovesBackupRole()
		 public virtual void MissingMasterUnavailabilityEventForOtherInstanceStillRemovesBackupRole()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, _clusterId1 );
			  // initialized with the members of the cluster
			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<ClusterMemberListener> memberListener = ArgumentCaptor.forClass( typeof( ClusterMemberListener ) );
			  verify( memberEvents ).addClusterMemberListener( memberListener.capture() );

			  // instance 2 is available as MASTER and BACKUP
			  memberListener.Value.memberIsAvailable( OnlineBackupKernelExtension.BACKUP, _clusterId2, _clusterUri2, StoreId.DEFAULT );
			  memberListener.Value.memberIsAvailable( MASTER, _clusterId2, _clusterUri2, StoreId.DEFAULT );

			  // when - instance 2 becomes available as SLAVE
			  memberListener.Value.memberIsAvailable( SLAVE, _clusterId2, _clusterUri2, StoreId.DEFAULT );

			  // then - instance 2 should be available ONLY as SLAVE
			  foreach ( ClusterMember clusterMember in members.Members )
			  {
					if ( clusterMember.InstanceId.Equals( _clusterId2 ) )
					{
						 assertThat( count( clusterMember.Roles ), equalTo( 1L ) );
						 assertThat( Iterables.single( clusterMember.Roles ), equalTo( SLAVE ) );
						 break; // that's the only member we care about
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void receivingInstanceFailureEventRemovesAllRolesForIt()
		 public virtual void ReceivingInstanceFailureEventRemovesAllRolesForIt()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  Heartbeat heartbeat = mock( typeof( Heartbeat ) );
			  ClusterMemberEvents memberEvents = mock( typeof( ClusterMemberEvents ) );

			  ObservedClusterMembers members = new ObservedClusterMembers( _logProvider, cluster, heartbeat, memberEvents, _clusterId1 );
			  // initialized with the members of the cluster
			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );
			  listener.Value.enteredCluster( ClusterConfiguration( _clusterUri1, _clusterUri2, _clusterUri3 ) );

			  ArgumentCaptor<ClusterMemberListener> memberListener = ArgumentCaptor.forClass( typeof( ClusterMemberListener ) );
			  verify( memberEvents ).addClusterMemberListener( memberListener.capture() );

			  // instance 2 is available as MASTER and BACKUP
			  memberListener.Value.memberIsAvailable( OnlineBackupKernelExtension.BACKUP, _clusterId2, _clusterUri2, StoreId.DEFAULT );
			  memberListener.Value.memberIsAvailable( MASTER, _clusterId2, _clusterUri2, StoreId.DEFAULT );

			  // when - instance 2 becomes failed
			  memberListener.Value.memberIsFailed( _clusterId2 );

			  // then - instance 2 should not be available as any roles
			  foreach ( ClusterMember clusterMember in members.Members )
			  {
					if ( clusterMember.InstanceId.Equals( _clusterId2 ) )
					{
						 assertThat( count( clusterMember.Roles ), equalTo( 0L ) );
						 break; // that's the only member we care about
					}
			  }
		 }

		 private ClusterConfiguration ClusterConfiguration( params URI[] uris )
		 {
			  LogProvider logProvider = FormattedLogProvider.toOutputStream( System.out );
			  ClusterConfiguration toReturn = new ClusterConfiguration( "Neo4Net.ha", logProvider, asList( uris ) );
			  toReturn.Joined( _clusterId1, _clusterUri1 );
			  toReturn.Joined( _clusterId2, _clusterUri2 );
			  if ( uris.Length == 3 )
			  {
					toReturn.Joined( _clusterId3, _clusterUri3 );
			  }
			  return toReturn;
		 }
	}

}