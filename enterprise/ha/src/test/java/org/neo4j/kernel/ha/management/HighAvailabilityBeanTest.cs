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
namespace Org.Neo4j.Kernel.ha.management
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using Format = Org.Neo4j.Helpers.Format;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using ManagementData = Org.Neo4j.Jmx.impl.ManagementData;
	using ManagementSupport = Org.Neo4j.Jmx.impl.ManagementSupport;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ClusterMember = Org.Neo4j.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ClusterMembers;
	using HighAvailabilityModeSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using LastTxIdGetter = Org.Neo4j.Kernel.impl.core.LastTxIdGetter;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using ClusterMemberInfo = Org.Neo4j.management.ClusterMemberInfo;
	using HighAvailability = Org.Neo4j.management.HighAvailability;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.firstOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.UNKNOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.StoreId.DEFAULT;

	public class HighAvailabilityBeanTest
	{
		private bool InstanceFieldsInitialized = false;

		public HighAvailabilityBeanTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_dbInfoProvider = new ClusterDatabaseInfoProvider( _clusterMembers, _lastTxIdGetter, _lastUpdateTime );
		}

		 private readonly Dependencies _dependencies = new Dependencies();
		 private readonly ClusterMembers _clusterMembers = mock( typeof( ClusterMembers ) );
		 private readonly HighAvailabilityBean _bean = new HighAvailabilityBean();
		 private readonly LastTxIdGetter _lastTxIdGetter = mock( typeof( LastTxIdGetter ) );
		 private readonly LastUpdateTime _lastUpdateTime = mock( typeof( LastUpdateTime ) );
		 private ClusterDatabaseInfoProvider _dbInfoProvider;
		 private DefaultFileSystemAbstraction _fileSystem;
		 private KernelData _kernelData;
		 private HighAvailability _haBean;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws javax.management.NotCompliantMBeanException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  DataSourceManager dataSourceManager = new DataSourceManager( Config.defaults() );
			  _fileSystem = new DefaultFileSystemAbstraction();
			  _kernelData = new TestHighlyAvailableKernelData( this, dataSourceManager );
			  ManagementData data = new ManagementData( _bean, _kernelData, ManagementSupport.load() );

			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );
			  when( dataSource.DatabaseLayout ).thenReturn( TestDirectory.databaseLayout() );
			  dataSourceManager.Register( dataSource );
			  when( dataSource.DependencyResolver ).thenReturn( _dependencies );
			  _dependencies.satisfyDependency( DatabaseInfo.HA );
			  _haBean = ( HighAvailability ) ( new HighAvailabilityBean() ).CreateMBean(data);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _kernelData.shutdown();
			  _fileSystem.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpOnLastCommittedTxId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPickUpOnLastCommittedTxId()
		 {
			  // GIVEN
			  long txId = 101L;
			  when( _lastTxIdGetter.LastTxId ).thenReturn( txId, txId + 1 );
			  when( _clusterMembers.CurrentMember ).thenReturn( ClusterMember( 1, MASTER, 1010 ) );

			  // WHEN
			  long reportedTxId = _haBean.LastCommittedTxId;

			  // THEN
			  assertEquals( txId, reportedTxId );

			  // and WHEN
			  long nextReportedTxId = _haBean.LastCommittedTxId;

			  // THEN
			  assertEquals( txId + 1, nextReportedTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpOnLastUpdateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPickUpOnLastUpdateTime()
		 {
			  // GIVEN
			  long updateTime = 123456789L;
			  when( _lastUpdateTime.getLastUpdateTime() ).thenReturn(0L, updateTime, updateTime + 1_000);
			  when( _clusterMembers.CurrentMember ).thenReturn( ClusterMember( 1, MASTER, 1010 ) );

			  // WHEN
			  string reportedUpdateTime = _haBean.LastUpdateTime;

			  // THEN
			  assertEquals( "N/A", reportedUpdateTime );

			  // and WHEN
			  string secondReportedUpdateTime = _haBean.LastUpdateTime;

			  // THEN
			  assertEquals( Format.date( updateTime ), secondReportedUpdateTime );

			  // and WHEN
			  string thirdReportedTxId = _haBean.LastUpdateTime;

			  // THEN
			  assertEquals( Format.date( updateTime + 1_000 ), thirdReportedTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeChangesInClusterMembers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeChangesInClusterMembers()
		 {
			  // GIVEN
			  when( _clusterMembers.Members ).thenReturn( asList( ClusterMember( 1, MASTER, 1137 ), ClusterMember( 2, SLAVE, 1138 ), ClusterMember( 3, SLAVE, 1139 ) ) );

			  // THEN
			  AssertMasterAndSlaveInformation( _haBean.InstancesInCluster );

			  // and WHEN
			  when( _clusterMembers.Members ).thenReturn( asList( ClusterMember( 1, SLAVE, 1137 ), ClusterMember( 2, MASTER, 1138 ), ClusterMember( 3, SLAVE, 1139 ) ) );

			  // THEN
			  foreach ( ClusterMemberInfo info in _haBean.InstancesInCluster )
			  {
					assertTrue( "every instance should be available", info.Available );
					assertTrue( "every instances should have at least one role", info.Roles.Length > 0 );
					if ( HighAvailabilityModeSwitcher.MASTER.Equals( info.Roles[0] ) )
					{
						 assertEquals( "coordinator should be master", HighAvailabilityModeSwitcher.MASTER, info.HaRole );
					}
					else
					{
						 assertEquals( "Either master or slave, no other way", HighAvailabilityModeSwitcher.SLAVE, info.Roles[0] );
						 assertEquals( "instance " + info.InstanceId + " is cluster slave but HA master", HighAvailabilityModeSwitcher.SLAVE, info.HaRole );
					}
					foreach ( string uri in info.Uris )
					{
						 assertTrue( "roles should contain URIs", uri.StartsWith( "ha://", StringComparison.Ordinal ) || uri.StartsWith( "backup://", StringComparison.Ordinal ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeLeavingMemberDisappear() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeLeavingMemberDisappear()
		 {
			  // GIVEN
			  when( _clusterMembers.Members ).thenReturn( asList( ClusterMember( 1, MASTER, 1137 ), ClusterMember( 2, SLAVE, 1138 ), ClusterMember( 3, SLAVE, 1139 ) ) );
			  AssertMasterAndSlaveInformation( _haBean.InstancesInCluster );

			  // WHEN
			  when( _clusterMembers.Members ).thenReturn( asList( ClusterMember( 1, MASTER, 1137 ), ClusterMember( 3, SLAVE, 1139 ) ) );

			  // THEN
			  assertEquals( 2, _haBean.InstancesInCluster.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeFailedMembersInMemberList() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeFailedMembersInMemberList()
		 {
			  // GIVEN
			  when( _clusterMembers.Members ).thenReturn( asList( ClusterMember( 1, MASTER, 1137 ), ClusterMember( 2, SLAVE, 1138 ), ClusterMember( 3, UNKNOWN, 1139, false ) ) );

			  // WHEN
			  ClusterMemberInfo[] instances = _haBean.InstancesInCluster;

			  // THEN
			  assertEquals( 3, instances.Length );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertEquals( 2, Count( instances, ClusterMemberInfo::isAlive ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertEquals( 2, Count( instances, ClusterMemberInfo::isAvailable ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPullUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPullUpdates()
		 {
			  // GIVEN
			  UpdatePuller updatePuller = mock( typeof( UpdatePuller ) );
			  _dependencies.satisfyDependency( updatePuller );

			  // WHEN
			  string result = _haBean.update();

			  // THEN
			  verify( updatePuller ).pullUpdates();
			  assertTrue( result, result.Contains( "Update completed in" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportFailedPullUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportFailedPullUpdates()
		 {
			  // GIVEN
			  UpdatePuller updatePuller = mock( typeof( UpdatePuller ) );
			  Exception myException = new Exception( "My test exception" );
			  Mockito.doThrow( myException ).when( updatePuller ).pullUpdates();
			  _dependencies.satisfyDependency( updatePuller );

			  // WHEN
			  string result = _haBean.update();

			  // THEN
			  verify( updatePuller ).pullUpdates();
			  assertTrue( result, result.Contains( myException.Message ) );
		 }

		 private int Count( ClusterMemberInfo[] instances, System.Predicate<ClusterMemberInfo> filter )
		 {
			  int count = 0;
			  foreach ( ClusterMemberInfo instance in instances )
			  {
					if ( filter( instance ) )
					{
						 count++;
					}
			  }
			  return count;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.ha.cluster.member.ClusterMember clusterMember(int serverId, String role, int port) throws java.net.URISyntaxException
		 private ClusterMember ClusterMember( int serverId, string role, int port )
		 {
			  return ClusterMember( serverId, role, port, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.ha.cluster.member.ClusterMember clusterMember(int serverId, String role, int port, boolean alive) throws java.net.URISyntaxException
		 private ClusterMember ClusterMember( int serverId, string role, int port, bool alive )
		 {
			  URI uri = HighAvailabilityModeSwitcher.UNKNOWN.Equals( role ) ? null : new URI( "ha://" + role + ":" + port );
			  return new ClusterMember( new InstanceId( serverId ), MapUtil.genericMap( role, uri ), DEFAULT, alive );
		 }

		 private void AssertMasterAndSlaveInformation( ClusterMemberInfo[] instancesInCluster )
		 {
			  ClusterMemberInfo master = Member( instancesInCluster, 1 );
			  assertEquals( 1137, GetUriForScheme( "ha", Iterables.map( URI.create, Arrays.asList( master.Uris ) ) ).Port );
			  assertEquals( HighAvailabilityModeSwitcher.MASTER, master.HaRole );
			  ClusterMemberInfo slave = Member( instancesInCluster, 2 );
			  assertEquals( 1138, GetUriForScheme( "ha", Iterables.map( URI.create, Arrays.asList( slave.Uris ) ) ).Port );
			  assertEquals( HighAvailabilityModeSwitcher.SLAVE, slave.HaRole );
			  assertTrue( "Slave not available", slave.Available );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static java.net.URI getUriForScheme(final String scheme, Iterable<java.net.URI> uris)
		 private static URI GetUriForScheme( string scheme, IEnumerable<URI> uris )
		 {
			  return firstOrNull( filter( item => item.Scheme.Equals( scheme ), uris ) );
		 }

		 private ClusterMemberInfo Member( ClusterMemberInfo[] members, int instanceId )
		 {
			  foreach ( ClusterMemberInfo member in members )
			  {
					if ( member.InstanceId.Equals( Convert.ToString( instanceId ) ) )
					{
						 return member;
					}
			  }
			  fail( "Couldn't find cluster member with cluster URI port " + instanceId + " among " + Arrays.ToString( members ) );
			  return null; // it will never get here.
		 }

		 private class TestHighlyAvailableKernelData : HighlyAvailableKernelData
		 {
			 private readonly HighAvailabilityBeanTest _outerInstance;

			  internal TestHighlyAvailableKernelData( HighAvailabilityBeanTest outerInstance, DataSourceManager dataSourceManager ) : base( dataSourceManager, outerInstance._clusterMembers, outerInstance._dbInfoProvider, outerInstance._fileSystem, null, new File( "storeDir" ), Config.defaults() )
			  {
				  this._outerInstance = outerInstance;
			  }
		 }
	}

}