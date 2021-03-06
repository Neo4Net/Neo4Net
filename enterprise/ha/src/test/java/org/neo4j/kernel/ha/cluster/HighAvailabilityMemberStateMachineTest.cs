﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.ha.cluster
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;


	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using ClusterClient = Org.Neo4j.cluster.client.ClusterClient;
	using ClusterMemberAvailability = Org.Neo4j.cluster.member.ClusterMemberAvailability;
	using ClusterMemberEvents = Org.Neo4j.cluster.member.ClusterMemberEvents;
	using ClusterMemberListener = Org.Neo4j.cluster.member.ClusterMemberListener;
	using Election = Org.Neo4j.cluster.protocol.election.Election;
	using ResourceReleaser = Org.Neo4j.com.ResourceReleaser;
	using Org.Neo4j.com;
	using StoreCopyClientMonitor = Org.Neo4j.com.storecopy.StoreCopyClientMonitor;
	using Suppliers = Org.Neo4j.Function.Suppliers;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using AvailabilityRequirement = Org.Neo4j.Kernel.availability.AvailabilityRequirement;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.ha;
	using ClusterMember = Org.Neo4j.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ClusterMembers;
	using ObservedClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ObservedClusterMembers;
	using Org.Neo4j.Kernel.ha.cluster.modeswitch;
	using ComponentSwitcherContainer = Org.Neo4j.Kernel.ha.cluster.modeswitch.ComponentSwitcherContainer;
	using HighAvailabilityModeSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using HandshakeResult = Org.Neo4j.Kernel.ha.com.master.HandshakeResult;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using MasterClient = Org.Neo4j.Kernel.ha.com.slave.MasterClient;
	using MasterClientResolver = Org.Neo4j.Kernel.ha.com.slave.MasterClientResolver;
	using SlaveServer = Org.Neo4j.Kernel.ha.com.slave.SlaveServer;
	using HaIdGeneratorFactory = Org.Neo4j.Kernel.ha.id.HaIdGeneratorFactory;
	using TransactionId = Org.Neo4j.Kernel.impl.store.TransactionId;
	using SimpleTransactionIdStore = Org.Neo4j.Kernel.impl.transaction.SimpleTransactionIdStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseTransactionStats = Org.Neo4j.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.StoreIdTestFactory.newStoreIdForCurrentVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcherTest.storeSupplierMock;

	public class HighAvailabilityMemberStateMachineTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartFromPending()
		 public virtual void ShouldStartFromPending()
		 {
			  // Given
			  HighAvailabilityMemberStateMachine memberStateMachine = BuildMockedStateMachine();
			  // Then
			  assertThat( memberStateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMoveToToMasterFromPendingOnMasterElectedForItself()
		 public virtual void ShouldMoveToToMasterFromPendingOnMasterElectedForItself()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );
			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events );
			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();

			  // When
			  memberListener.CoordinatorIsElected( me );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToMaster ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemainToPendingOnMasterElectedForSomeoneElse()
		 public virtual void ShouldRemainToPendingOnMasterElectedForSomeoneElse()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );
			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events );
			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();

			  // When
			  memberListener.CoordinatorIsElected( new InstanceId( 2 ) );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSwitchToToSlaveOnMasterAvailableForSomeoneElse()
		 public virtual void ShouldSwitchToToSlaveOnMasterAvailableForSomeoneElse()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );
			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events );

			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();
			  HAStateChangeListener probe = new HAStateChangeListener();
			  stateMachine.AddHighAvailabilityMemberListener( probe );

			  // When
			  memberListener.MemberIsAvailable( MASTER, new InstanceId( 2 ), URI.create( "ha://whatever" ), StoreId.DEFAULT );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToSlave ) );
			  assertThat( probe.MasterIsAvailableConflict, @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenInMasterStateLosingQuorumFromTwoInstancesShouldRemainMaster()
		 public virtual void WhenInMasterStateLosingQuorumFromTwoInstancesShouldRemainMaster()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId other = new InstanceId( 2 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );

			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  ObservedClusterMembers members = MockClusterMembers( me, emptyList(), singletonList(other) );

			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events, members, guard );

			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();
			  HAStateChangeListener probe = new HAStateChangeListener();
			  stateMachine.AddHighAvailabilityMemberListener( probe );

			  // Send it to MASTER
			  memberListener.CoordinatorIsElected( me );
			  memberListener.MemberIsAvailable( MASTER, me, URI.create( "ha://whatever" ), StoreId.DEFAULT );

			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Master ) );

			  // When
			  memberListener.MemberIsFailed( new InstanceId( 2 ) );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Master ) );
			  assertThat( probe.InstanceStopsConflict, @is( false ) );
			  assertThat( probe.InstanceDetachedConflict, @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenInMasterStateLosingQuorumFromThreeInstancesShouldGoToPending()
		 public virtual void WhenInMasterStateLosingQuorumFromThreeInstancesShouldGoToPending()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId other1 = new InstanceId( 2 );
			  InstanceId other2 = new InstanceId( 3 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );

			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  IList<InstanceId> otherInstances = new LinkedList();
			  otherInstances.Add( other1 );
			  otherInstances.Add( other2 );
			  ObservedClusterMembers members = MockClusterMembers( me, emptyList(), otherInstances );

			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events, members, guard );

			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();
			  HAStateChangeListener probe = new HAStateChangeListener();
			  stateMachine.AddHighAvailabilityMemberListener( probe );

			  // Send it to MASTER
			  memberListener.CoordinatorIsElected( me );
			  memberListener.MemberIsAvailable( MASTER, me, URI.create( "ha://whatever" ), StoreId.DEFAULT );

			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Master ) );

			  // When
			  memberListener.MemberIsFailed( new InstanceId( 2 ) );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
			  assertThat( probe.InstanceStopsConflict, @is( false ) );
			  assertThat( probe.InstanceDetachedConflict, @is( true ) );
			  verify( guard, times( 1 ) ).require( any( typeof( AvailabilityRequirement ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenInSlaveStateLosingOtherSlaveShouldNotPutInPending()
		 public virtual void WhenInSlaveStateLosingOtherSlaveShouldNotPutInPending()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId master = new InstanceId( 2 );
			  InstanceId otherSlave = new InstanceId( 3 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );
			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  ObservedClusterMembers members = MockClusterMembers( me, singletonList( master ), singletonList( otherSlave ) );

			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events, members, guard );

			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();
			  HAStateChangeListener probe = new HAStateChangeListener();
			  stateMachine.AddHighAvailabilityMemberListener( probe );

			  // Send it to MASTER
			  memberListener.MemberIsAvailable( MASTER, master, URI.create( "ha://whatever" ), StoreId.DEFAULT );
			  memberListener.MemberIsAvailable( SLAVE, me, URI.create( "ha://whatever3" ), StoreId.DEFAULT );
			  memberListener.MemberIsAvailable( SLAVE, otherSlave, URI.create( "ha://whatever2" ), StoreId.DEFAULT );

			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Slave ) );

			  // When
			  memberListener.MemberIsFailed( otherSlave );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Slave ) );
			  assertThat( probe.InstanceStopsConflict, @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenInSlaveStateWith3MemberClusterLosingMasterShouldPutInPending()
		 public virtual void WhenInSlaveStateWith3MemberClusterLosingMasterShouldPutInPending()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId master = new InstanceId( 2 );
			  InstanceId otherSlave = new InstanceId( 3 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );
			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  ObservedClusterMembers members = MockClusterMembers( me, singletonList( otherSlave ), singletonList( master ) );

			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events, members, guard );

			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();
			  HAStateChangeListener probe = new HAStateChangeListener();
			  stateMachine.AddHighAvailabilityMemberListener( probe );

			  // Send it to MASTER
			  memberListener.CoordinatorIsElected( master );
			  memberListener.MemberIsAvailable( MASTER, master, URI.create( "ha://whatever" ), StoreId.DEFAULT );
			  memberListener.MemberIsAvailable( SLAVE, me, URI.create( "ha://whatever3" ), StoreId.DEFAULT );
			  memberListener.MemberIsAvailable( SLAVE, otherSlave, URI.create( "ha://whatever2" ), StoreId.DEFAULT );

			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Slave ) );

			  // When
			  memberListener.MemberIsFailed( master );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
			  assertThat( probe.InstanceStopsConflict, @is( false ) );
			  assertThat( probe.InstanceDetachedConflict, @is( true ) );
			  verify( guard, times( 1 ) ).require( any( typeof( AvailabilityRequirement ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenInSlaveStateWith2MemberClusterLosingMasterShouldPutInPending()
		 public virtual void WhenInSlaveStateWith2MemberClusterLosingMasterShouldPutInPending()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId master = new InstanceId( 2 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );
			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  ObservedClusterMembers members = MockClusterMembers( me, emptyList(), singletonList(master) );

			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events, members, guard );

			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();
			  HAStateChangeListener probe = new HAStateChangeListener();
			  stateMachine.AddHighAvailabilityMemberListener( probe );

			  // Send it to MASTER
			  memberListener.CoordinatorIsElected( master );
			  memberListener.MemberIsAvailable( MASTER, master, URI.create( "ha://whatever" ), StoreId.DEFAULT );
			  memberListener.MemberIsAvailable( SLAVE, me, URI.create( "ha://whatever3" ), StoreId.DEFAULT );

			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Slave ) );

			  // When
			  memberListener.MemberIsFailed( master );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
			  assertThat( probe.InstanceStopsConflict, @is( false ) );
			  assertThat( probe.InstanceDetachedConflict, @is( true ) );
			  verify( guard, times( 1 ) ).require( any( typeof( AvailabilityRequirement ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenInToMasterStateLosingQuorumShouldPutInPending()
		 public virtual void WhenInToMasterStateLosingQuorumShouldPutInPending()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId other = new InstanceId( 2 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );
			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  ObservedClusterMembers members = MockClusterMembers( me, emptyList(), singletonList(other) );

			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events, members, guard );

			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();
			  HAStateChangeListener probe = new HAStateChangeListener();
			  stateMachine.AddHighAvailabilityMemberListener( probe );

			  // Send it to MASTER
			  memberListener.CoordinatorIsElected( me );

			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToMaster ) );

			  // When
			  memberListener.MemberIsFailed( new InstanceId( 2 ) );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
			  assertThat( probe.InstanceStopsConflict, @is( false ) );
			  assertThat( probe.InstanceDetachedConflict, @is( true ) );
			  verify( guard, times( 1 ) ).require( any( typeof( AvailabilityRequirement ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenInToSlaveStateLosingQuorumShouldPutInPending()
		 public virtual void WhenInToSlaveStateLosingQuorumShouldPutInPending()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId other = new InstanceId( 2 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, false );
			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  ObservedClusterMembers members = MockClusterMembers( me, emptyList(), singletonList(other) );

			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events, members, guard );
			  stateMachine.Init();
			  ClusterMemberListener memberListener = memberListenerContainer.Get();
			  HAStateChangeListener probe = new HAStateChangeListener();
			  stateMachine.AddHighAvailabilityMemberListener( probe );

			  // Send it to MASTER
			  memberListener.MemberIsAvailable( MASTER, other, URI.create( "ha://whatever" ), StoreId.DEFAULT );

			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToSlave ) );

			  // When
			  memberListener.MemberIsFailed( new InstanceId( 2 ) );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
			  assertThat( probe.InstanceStopsConflict, @is( false ) );
			  assertThat( probe.InstanceDetachedConflict, @is( true ) );
			  verify( guard, times( 1 ) ).require( any( typeof( AvailabilityRequirement ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenSlaveOnlyIsElectedStayInPending()
		 public virtual void WhenSlaveOnlyIsElectedStayInPending()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( me, true );
			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  ClusterMemberListenerContainer memberListenerContainer = MockAddClusterMemberListener( events );

			  HighAvailabilityMemberStateMachine stateMachine = BuildMockedStateMachine( context, events );

			  stateMachine.Init();

			  ClusterMemberListener memberListener = memberListenerContainer.Get();

			  // When
			  memberListener.CoordinatorIsElected( me );

			  // Then
			  assertThat( stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenHAModeSwitcherSwitchesToSlaveTheOtherModeSwitcherDoNotGetTheOldMasterClient() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WhenHAModeSwitcherSwitchesToSlaveTheOtherModeSwitcherDoNotGetTheOldMasterClient()
		 {
			  InstanceId me = new InstanceId( 1 );
			  StoreId storeId = newStoreIdForCurrentVersion();
			  HighAvailabilityMemberContext context = mock( typeof( HighAvailabilityMemberContext ) );
			  when( context.MyId ).thenReturn( me );
			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  ObservedClusterMembers members = mock( typeof( ObservedClusterMembers ) );
			  ClusterMember masterMember = mock( typeof( ClusterMember ) );
			  when( masterMember.HARole ).thenReturn( "master" );
			  when( masterMember.HasRole( "master" ) ).thenReturn( true );
			  when( masterMember.InstanceId ).thenReturn( new InstanceId( 2 ) );
			  when( masterMember.StoreId ).thenReturn( storeId );
			  ClusterMember self = new ClusterMember( me );
			  when( members.Members ).thenReturn( Arrays.asList( self, masterMember ) );
			  when( members.CurrentMember ).thenReturn( self );
			  DependencyResolver dependencyResolver = mock( typeof( DependencyResolver ) );
			  FileSystemAbstraction fs = mock( typeof( FileSystemAbstraction ) );
			  when( fs.FileExists( any( typeof( File ) ) ) ).thenReturn( true );
			  when( dependencyResolver.ResolveDependency( typeof( FileSystemAbstraction ) ) ).thenReturn( fs );
			  when( dependencyResolver.ResolveDependency( typeof( Monitors ) ) ).thenReturn( new Monitors() );
			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );
			  when( dataSource.DependencyResolver ).thenReturn( dependencyResolver );
			  when( dataSource.StoreId ).thenReturn( storeId );
			  when( dependencyResolver.ResolveDependency( typeof( NeoStoreDataSource ) ) ).thenReturn( dataSource );
			  when( dependencyResolver.ResolveDependency( typeof( TransactionIdStore ) ) ).thenReturn( new SimpleTransactionIdStore() );
			  when( dependencyResolver.ResolveDependency( typeof( ObservedClusterMembers ) ) ).thenReturn( members );
			  UpdatePuller updatePuller = mock( typeof( UpdatePuller ) );
			  when( updatePuller.TryPullUpdates() ).thenReturn(true);
			  when( dependencyResolver.ResolveDependency( typeof( UpdatePuller ) ) ).thenReturn( updatePuller );

			  ClusterMemberAvailability clusterMemberAvailability = mock( typeof( ClusterMemberAvailability ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TriggerableClusterMemberEvents events = new TriggerableClusterMemberEvents();
			  TriggerableClusterMemberEvents events = new TriggerableClusterMemberEvents();

			  Election election = mock( typeof( Election ) );
			  HighAvailabilityMemberStateMachine stateMachine = new HighAvailabilityMemberStateMachine( context, guard, members, events, election, NullLogProvider.Instance );

			  ClusterMembers clusterMembers = new ClusterMembers( members, stateMachine );
			  when( dependencyResolver.ResolveDependency( typeof( ClusterMembers ) ) ).thenReturn( clusterMembers );

			  stateMachine.Init();
			  stateMachine.Start();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.ha.DelegateInvocationHandler<org.neo4j.kernel.ha.com.master.Master> handler = new org.neo4j.kernel.ha.DelegateInvocationHandler<>(org.neo4j.kernel.ha.com.master.Master.class);
			  DelegateInvocationHandler<Master> handler = new DelegateInvocationHandler<Master>( typeof( Master ) );

			  MasterClientResolver masterClientResolver = mock( typeof( MasterClientResolver ) );
			  MasterClient masterClient = mock( typeof( MasterClient ) );
			  when( masterClient.ProtocolVersion ).thenReturn( MasterClient214.PROTOCOL_VERSION );
			  when( masterClient.Handshake( anyLong(), any(typeof(StoreId)) ) ).thenReturn(new ResponseAnonymousInnerClass(this, storeId, mock(typeof(ResourceReleaser)), handler));
			  when( masterClient.ToString() ).thenReturn("TheExpectedMasterClient!");
			  when( masterClientResolver.Instantiate( anyString(), anyInt(), anyString(), any(typeof(Monitors)), any(typeof(StoreId)), any(typeof(LifeSupport)) ) ).thenReturn(masterClient);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(2);
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean switchedSuccessfully = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean switchedSuccessfully = new AtomicBoolean();

			  SwitchToSlave.Monitor monitor = new MonitorAnonymousInnerClass( this, latch, switchedSuccessfully );

			  Config config = Config.defaults( ClusterSettings.server_id, me.ToString() );

			  DatabaseTransactionStats transactionCounters = mock( typeof( DatabaseTransactionStats ) );
			  when( transactionCounters.NumberOfActiveTransactions ).thenReturn( 0L );

			  PageCache pageCacheMock = mock( typeof( PageCache ) );
			  PagedFile pagedFileMock = mock( typeof( PagedFile ) );
			  when( pagedFileMock.LastPageId ).thenReturn( 1L );
			  when( pageCacheMock.Map( any( typeof( File ) ), anyInt() ) ).thenReturn(pagedFileMock);

			  TransactionIdStore transactionIdStoreMock = mock( typeof( TransactionIdStore ) );
			  when( transactionIdStoreMock.LastCommittedTransaction ).thenReturn( new TransactionId( 0, 0, 0 ) );
			  SwitchToSlaveCopyThenBranch switchToSlave = new SwitchToSlaveCopyThenBranch(DatabaseLayout.of(new File("")), NullLogService.Instance, mock(typeof(FileSystemAbstraction)), config, mock(typeof(HaIdGeneratorFactory)), handler, mock(typeof(ClusterMemberAvailability)), mock(typeof(RequestContextFactory)), mock(typeof(PullerFactory), RETURNS_MOCKS), Iterables.empty(), masterClientResolver, monitor, new Org.Neo4j.com.storecopy.StoreCopyClientMonitor_Adapter(), Suppliers.singleton(dataSource), Suppliers.singleton(transactionIdStoreMock), slave =>
			  {
						  SlaveServer mock = mock( typeof( SlaveServer ) );
						  when( mock.SocketAddress ).thenReturn( new InetSocketAddress( "localhost", 123 ) );
						  return mock;
			  }, updatePuller, pageCacheMock, mock( typeof( Monitors ) ), () => transactionCounters);

			  ComponentSwitcherContainer switcherContainer = new ComponentSwitcherContainer();
			  HighAvailabilityModeSwitcher haModeSwitcher = new HighAvailabilityModeSwitcher( switchToSlave, mock( typeof( SwitchToMaster ) ), election, clusterMemberAvailability, mock( typeof( ClusterClient ) ), storeSupplierMock(), me, switcherContainer, NeoStoreDataSourceSupplierMock(), NullLogService.Instance );
			  haModeSwitcher.Init();
			  haModeSwitcher.Start();
			  haModeSwitcher.ListeningAt( URI.create( "http://localhost:12345" ) );

			  stateMachine.AddHighAvailabilityMemberListener( haModeSwitcher );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.neo4j.kernel.ha.com.master.Master> ref = new java.util.concurrent.atomic.AtomicReference<>(null);
			  AtomicReference<Master> @ref = new AtomicReference<Master>( null );

			  //noinspection unchecked
			  AbstractComponentSwitcher<object> otherModeSwitcher = new AbstractComponentSwitcherAnonymousInnerClass( this, mock( typeof( DelegateInvocationHandler ) ), handler, latch, @ref );
			  switcherContainer.Add( otherModeSwitcher );
			  // When
			  events.SwitchToSlave( me );

			  // Then
			  latch.await();
			  assertTrue( "mode switch failed", switchedSuccessfully.get() );
			  Master actual = @ref.get();
			  // let's test the toString()s since there are too many wrappers of proxies
			  assertEquals( masterClient.ToString(), actual.ToString() );

			  stateMachine.Stop();
			  stateMachine.Shutdown();
			  haModeSwitcher.Stop();
			  haModeSwitcher.Shutdown();
		 }

		 private class ResponseAnonymousInnerClass : Response<HandshakeResult>
		 {
			 private readonly HighAvailabilityMemberStateMachineTest _outerInstance;

			 private DelegateInvocationHandler<Master> _handler;

			 public ResponseAnonymousInnerClass( HighAvailabilityMemberStateMachineTest outerInstance, StoreId storeId, UnknownType mock, DelegateInvocationHandler<Master> handler ) : base( new HandshakeResult( 0, 42 ), storeId, mock )
			 {
				 this.outerInstance = outerInstance;
				 this._handler = handler;
			 }

			 public override void accept( Handler handler )
			 {
			 }

			 public override bool hasTransactionsToBeApplied()
			 {
				  return false;
			 }
		 }

		 private class MonitorAnonymousInnerClass : SwitchToSlave.Monitor
		 {
			 private readonly HighAvailabilityMemberStateMachineTest _outerInstance;

			 private System.Threading.CountdownEvent _latch;
			 private AtomicBoolean _switchedSuccessfully;

			 public MonitorAnonymousInnerClass( HighAvailabilityMemberStateMachineTest outerInstance, System.Threading.CountdownEvent latch, AtomicBoolean switchedSuccessfully )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
				 this._switchedSuccessfully = switchedSuccessfully;
			 }

			 public void switchToSlaveCompleted( bool wasSuccessful )
			 {
				  _switchedSuccessfully.set( wasSuccessful );
				  _latch.Signal();
			 }
		 }

		 private class AbstractComponentSwitcherAnonymousInnerClass : AbstractComponentSwitcher<object>
		 {
			 private readonly HighAvailabilityMemberStateMachineTest _outerInstance;

			 private DelegateInvocationHandler<Master> _handler;
			 private System.Threading.CountdownEvent _latch;
			 private AtomicReference<Master> @ref;

			 public AbstractComponentSwitcherAnonymousInnerClass( HighAvailabilityMemberStateMachineTest outerInstance, UnknownType mock, DelegateInvocationHandler<Master> handler, System.Threading.CountdownEvent latch, AtomicReference<Master> @ref ) : base( mock )
			 {
				 this.outerInstance = outerInstance;
				 this._handler = handler;
				 this._latch = latch;
				 this.@ref = @ref;
			 }

			 protected internal override object SlaveImpl
			 {
				 get
				 {
					  Master master = _handler.cement();
					  @ref.set( master );
					  _latch.Signal();
					  return null;
				 }
			 }

			 protected internal override object MasterImpl
			 {
				 get
				 {
					  return null;
				 }
			 }
		 }

		 private ObservedClusterMembers MockClusterMembers( InstanceId me, IList<InstanceId> alive, IList<InstanceId> failed )
		 {
			  ObservedClusterMembers members = mock( typeof( ObservedClusterMembers ) );

			  // we cannot set outside of the package the isAlive to return false. So do it with a mock
			  IList<ClusterMember> aliveMembers = new List<ClusterMember>( alive.Count );
			  IList<ClusterMember> failedMembers = new List<ClusterMember>( failed.Count );
			  foreach ( InstanceId instanceId in alive )
			  {
					ClusterMember otherMember = mock( typeof( ClusterMember ) );
					when( otherMember.InstanceId ).thenReturn( instanceId );
					// the failed argument tells us which instance should be marked as failed
					when( otherMember.Alive ).thenReturn( true );
					aliveMembers.Add( otherMember );
			  }

			  foreach ( InstanceId instanceId in failed )
			  {
					ClusterMember otherMember = mock( typeof( ClusterMember ) );
					when( otherMember.InstanceId ).thenReturn( instanceId );
					// the failed argument tells us which instance should be marked as failed
					when( otherMember.Alive ).thenReturn( false );
					failedMembers.Add( otherMember );
			  }

			  ClusterMember thisMember = new ClusterMember( me );
			  aliveMembers.Add( thisMember );

			  IList<ClusterMember> allMembers = new List<ClusterMember>();
			  ( ( IList<ClusterMember> )allMembers ).AddRange( aliveMembers ); // thisMember is in aliveMembers
			  ( ( IList<ClusterMember> )allMembers ).AddRange( failedMembers );
			  when( members.Members ).thenReturn( allMembers );
			  when( members.AliveMembers ).thenReturn( aliveMembers );

			  return members;
		 }

		 private static DataSourceManager NeoStoreDataSourceSupplierMock()
		 {
			  DataSourceManager dataSourceManager = new DataSourceManager( Config.defaults() );
			  dataSourceManager.Register( mock( typeof( NeoStoreDataSource ) ) );
			  return dataSourceManager;
		 }

		 internal static ClusterMemberListenerContainer MockAddClusterMemberListener( ClusterMemberEvents events )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ClusterMemberListenerContainer listenerContainer = new ClusterMemberListenerContainer();
			  ClusterMemberListenerContainer listenerContainer = new ClusterMemberListenerContainer();
			  doAnswer(invocation =>
			  {
				listenerContainer.Set( invocation.getArgument( 0 ) );
				return null;
			  }).when( events ).addClusterMemberListener( ArgumentMatchers.any() );
			  return listenerContainer;
		 }

		 private HighAvailabilityMemberStateMachine BuildMockedStateMachine()
		 {
			  return ( new StateMachineBuilder() ).Build();
		 }

		 private HighAvailabilityMemberStateMachine BuildMockedStateMachine( HighAvailabilityMemberContext context, ClusterMemberEvents events )
		 {
			  return ( new StateMachineBuilder() ).WithContext(context).withEvents(events).build();
		 }

		 private HighAvailabilityMemberStateMachine BuildMockedStateMachine( HighAvailabilityMemberContext context, ClusterMemberEvents events, ObservedClusterMembers clusterMembers, AvailabilityGuard guard )
		 {
			  return ( new StateMachineBuilder() ).WithContext(context).withEvents(events).withClusterMembers(clusterMembers).withGuard(guard).build();
		 }

		 internal class StateMachineBuilder
		 {
			  internal HighAvailabilityMemberContext Context = mock( typeof( HighAvailabilityMemberContext ) );
			  internal ClusterMemberEvents Events = mock( typeof( ClusterMemberEvents ) );
			  internal ObservedClusterMembers ClusterMembers = mock( typeof( ObservedClusterMembers ) );
			  internal AvailabilityGuard Guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  internal Election Election = mock( typeof( Election ) );

			  internal virtual StateMachineBuilder WithContext( HighAvailabilityMemberContext context )
			  {
					this.Context = context;
					return this;
			  }

			  internal virtual StateMachineBuilder WithEvents( ClusterMemberEvents events )
			  {
					this.Events = events;
					return this;
			  }

			  internal virtual StateMachineBuilder WithClusterMembers( ObservedClusterMembers clusterMember )
			  {
					this.ClusterMembers = clusterMember;
					return this;
			  }

			  internal virtual StateMachineBuilder WithGuard( AvailabilityGuard guard )
			  {
					this.Guard = guard;
					return this;
			  }

			  internal virtual StateMachineBuilder WithElection( Election election )
			  {
					this.Election = election;
					return this;
			  }

			  public virtual HighAvailabilityMemberStateMachine Build()
			  {
					return new HighAvailabilityMemberStateMachine( Context, Guard, ClusterMembers, Events, Election, NullLogProvider.Instance );
			  }
		 }

		 internal class ClusterMemberListenerContainer
		 {
			  internal ClusterMemberListener ClusterMemberListener;

			  public virtual ClusterMemberListener Get()
			  {
					return ClusterMemberListener;
			  }

			  public virtual void Set( ClusterMemberListener clusterMemberListener )
			  {
					if ( this.ClusterMemberListener != null )
					{
						 throw new System.InvalidOperationException( "Expected to have only 1 listener, but have more. " + "Defined listener: " + this.ClusterMemberListener + ". Newly added listener:" + clusterMemberListener );
					}
					this.ClusterMemberListener = clusterMemberListener;
			  }
		 }

		 internal sealed class HAStateChangeListener : HighAvailabilityMemberListener
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool MasterIsElectedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool MasterIsAvailableConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool SlaveIsAvailableConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool InstanceStopsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool InstanceDetachedConflict;
			  internal HighAvailabilityMemberChangeEvent LastEvent;

			  public override void MasterIsElected( HighAvailabilityMemberChangeEvent @event )
			  {
					MasterIsElectedConflict = true;
					MasterIsAvailableConflict = false;
					SlaveIsAvailableConflict = false;
					InstanceStopsConflict = false;
					InstanceDetachedConflict = false;
					LastEvent = @event;
			  }

			  public override void MasterIsAvailable( HighAvailabilityMemberChangeEvent @event )
			  {
					MasterIsElectedConflict = false;
					MasterIsAvailableConflict = true;
					SlaveIsAvailableConflict = false;
					InstanceStopsConflict = false;
					InstanceDetachedConflict = false;
					LastEvent = @event;
			  }

			  public override void SlaveIsAvailable( HighAvailabilityMemberChangeEvent @event )
			  {
					MasterIsElectedConflict = false;
					MasterIsAvailableConflict = false;
					SlaveIsAvailableConflict = true;
					InstanceStopsConflict = false;
					InstanceDetachedConflict = false;
					LastEvent = @event;
			  }

			  public override void InstanceStops( HighAvailabilityMemberChangeEvent @event )
			  {
					MasterIsElectedConflict = false;
					MasterIsAvailableConflict = false;
					SlaveIsAvailableConflict = false;
					InstanceStopsConflict = true;
					InstanceDetachedConflict = false;
					LastEvent = @event;
			  }

			  public override void InstanceDetached( HighAvailabilityMemberChangeEvent @event )
			  {
					MasterIsElectedConflict = false;
					MasterIsAvailableConflict = false;
					SlaveIsAvailableConflict = false;
					InstanceStopsConflict = false;
					InstanceDetachedConflict = true;
					LastEvent = @event;
			  }
		 }

		 private class TriggerableClusterMemberEvents : ClusterMemberEvents
		 {
			  internal ClusterMemberListener Listener;

			  public override void AddClusterMemberListener( ClusterMemberListener listener )
			  {
					this.Listener = listener;
			  }

			  public override void RemoveClusterMemberListener( ClusterMemberListener listener )
			  {
			  }

			  internal virtual void SwitchToSlave( InstanceId me )
			  {
					InstanceId someOneElseThanMyself = new InstanceId( me.ToIntegerIndex() + 1 );
					Listener.memberIsAvailable( "master", someOneElseThanMyself, URI.create( "cluster://127.0.0.1:2390?serverId=2" ), null );
					Listener.memberIsAvailable( "slave", me, null, null );
			  }
		 }
	}

}