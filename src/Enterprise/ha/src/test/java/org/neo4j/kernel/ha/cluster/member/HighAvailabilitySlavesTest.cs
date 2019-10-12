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
namespace Neo4Net.Kernel.ha.cluster.member
{
	using Test = org.junit.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using InstanceId = Neo4Net.cluster.InstanceId;
	using Cluster = Neo4Net.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using Suppliers = Neo4Net.Function.Suppliers;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using DefaultSlaveFactory = Neo4Net.Kernel.ha.com.master.DefaultSlaveFactory;
	using Slave = Neo4Net.Kernel.ha.com.master.Slave;
	using SlaveFactory = Neo4Net.Kernel.ha.com.master.SlaveFactory;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using ReflectionUtil = Neo4Net.Test.ReflectionUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;

	public class HighAvailabilitySlavesTest
	{
		 private static readonly InstanceId _instanceId = new InstanceId( 1 );
		 private static readonly URI _haUri = create( "ha://server1?serverId=" + _instanceId.toIntegerIndex() );
		 private static readonly URI _clusterUri = create( "cluster://server2" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRegisterItselfOnMonitors()
		 public virtual void ShouldRegisterItselfOnMonitors()
		 {
			  // given
			  ClusterMembers clusterMembers = mock( typeof( ClusterMembers ) );
			  Cluster cluster = mock( typeof( Cluster ) );
			  SlaveFactory slaveFactory = mock( typeof( SlaveFactory ) );

			  // when
			  ( new HighAvailabilitySlaves( clusterMembers, cluster, slaveFactory, new HostnamePort( null, 0 ) ) ).init();

			  // then
			  verify( cluster ).addClusterListener( any( typeof( ClusterListener ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnUnavailableSlaves()
		 public virtual void ShouldNotReturnUnavailableSlaves()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  ClusterMembers clusterMembers = mock( typeof( ClusterMembers ) );
			  when( clusterMembers.AliveMembers ).thenReturn( Iterables.option( new ClusterMember( _instanceId ) ) );

			  SlaveFactory slaveFactory = mock( typeof( SlaveFactory ) );

			  HighAvailabilitySlaves slaves = new HighAvailabilitySlaves( clusterMembers, cluster, slaveFactory, new HostnamePort( null, 0 ) );
			  slaves.Init();

			  // when
			  IEnumerable<Slave> memberSlaves = slaves.Slaves;

			  // then
			  assertThat( count( memberSlaves ), equalTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnAvailableAndAliveSlaves()
		 public virtual void ShouldReturnAvailableAndAliveSlaves()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  ClusterMembers clusterMembers = mock( typeof( ClusterMembers ) );
			  when( clusterMembers.AliveMembers ).thenReturn( Iterables.option( ( new ClusterMember( _instanceId ) ).availableAs( SLAVE, _haUri, StoreId.DEFAULT ) ) );

			  SlaveFactory slaveFactory = mock( typeof( SlaveFactory ) );
			  Slave slave = mock( typeof( Slave ) );
			  when( slaveFactory.NewSlave( any( typeof( LifeSupport ) ), any( typeof( ClusterMember ) ), any( typeof( string ) ), any( typeof( Integer ) ) ) ).thenReturn( slave );

			  HighAvailabilitySlaves slaves = new HighAvailabilitySlaves( clusterMembers, cluster, slaveFactory, new HostnamePort( null, 0 ) );
			  slaves.Init();

			  // when
			  IEnumerable<Slave> memberSlaves = slaves.Slaves;

			  // then
			  assertThat( count( memberSlaves ), equalTo( 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearSlavesWhenNewMasterElected()
		 public virtual void ShouldClearSlavesWhenNewMasterElected()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  ClusterMembers clusterMembers = mock( typeof( ClusterMembers ) );
			  when( clusterMembers.AliveMembers ).thenReturn( Iterables.option( ( new ClusterMember( _instanceId ) ).availableAs( SLAVE, _haUri, StoreId.DEFAULT ) ) );

			  SlaveFactory slaveFactory = mock( typeof( SlaveFactory ) );
			  Slave slave1 = mock( typeof( Slave ) );
			  Slave slave2 = mock( typeof( Slave ) );
			  when( slaveFactory.NewSlave( any( typeof( LifeSupport ) ), any( typeof( ClusterMember ) ), any( typeof( string ) ), any( typeof( Integer ) ) ) ).thenReturn( slave1, slave2 );

			  HighAvailabilitySlaves slaves = new HighAvailabilitySlaves( clusterMembers, cluster, slaveFactory, new HostnamePort( "localhost", 0 ) );
			  slaves.Init();

			  ArgumentCaptor<ClusterListener> listener = ArgumentCaptor.forClass( typeof( ClusterListener ) );
			  verify( cluster ).addClusterListener( listener.capture() );

			  // when
			  Slave actualSlave1 = slaves.Slaves.GetEnumerator().next();

			  listener.Value.elected( ClusterConfiguration.COORDINATOR, _instanceId, _clusterUri );

			  Slave actualSlave2 = slaves.Slaves.GetEnumerator().next();

			  // then
			  assertThat( actualSlave2, not( sameInstance( actualSlave1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportConcurrentConsumptionOfSlaves() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportConcurrentConsumptionOfSlaves()
		 {
			  // Given
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  HighAvailabilitySlaves haSlaves = new HighAvailabilitySlaves( ClusterMembersOfSize( 1000 ), mock( typeof( Cluster ) ), new DefaultSlaveFactory( NullLogProvider.Instance, new Monitors(), 42, Suppliers.singleton(logEntryReader) ), new HostnamePort(null, 0) );

			  // When
			  ExecutorService executor = Executors.newFixedThreadPool( 5 );
			  for ( int i = 0; i < 5; i++ )
			  {
					executor.submit( SlavesConsumingRunnable( haSlaves ) );
			  }
			  executor.shutdown();
			  executor.awaitTermination( 30, SECONDS );

			  // Then
			  int slavesCount = 0;
			  LifeSupport life = ReflectionUtil.getPrivateField( haSlaves, "life", typeof( LifeSupport ) );
			  foreach ( Lifecycle lifecycle in life.LifecycleInstances )
			  {
					if ( lifecycle is Slave )
					{
						 slavesCount++;
					}
			  }
			  assertEquals( "Unexpected number of slaves", 1000 - 1, slavesCount ); // One instance is master
		 }

		 private static ClusterMembers ClusterMembersOfSize( int size )
		 {
			  IList<ClusterMember> members = new List<ClusterMember>( size );
			  members.Add( MockClusterMemberWithRole( HighAvailabilityModeSwitcher.MASTER ) );
			  for ( int i = 0; i < size - 1; i++ )
			  {
					members.Add( MockClusterMemberWithRole( SLAVE ) );
			  }

			  ClusterMembers clusterMembers = mock( typeof( ClusterMembers ) );
			  when( clusterMembers.AliveMembers ).thenReturn( members );

			  return clusterMembers;
		 }

		 private static ClusterMember MockClusterMemberWithRole( string role )
		 {
			  ClusterMember member = mock( typeof( ClusterMember ) );
			  when( member.HAUri ).thenReturn( URI.create( "http://localhost:7474" ) );
			  when( member.Alive ).thenReturn( true );
			  when( member.HasRole( eq( role ) ) ).thenReturn( true );
			  return member;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Runnable slavesConsumingRunnable(final HighAvailabilitySlaves haSlaves)
		 private static ThreadStart SlavesConsumingRunnable( HighAvailabilitySlaves haSlaves )
		 {
			  return () =>
			  {
				foreach ( Slave slave in haSlaves.Slaves )
				{
					 assertNotNull( slave );
				}
			  };
		 }
	}

}