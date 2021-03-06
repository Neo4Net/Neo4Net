﻿using System;
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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using Node = Org.Neo4j.Graphdb.Node;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Org.Neo4j.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using LifecycleException = Org.Neo4j.Kernel.Lifecycle.LifecycleException;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.TestStoreId.assertAllStoresHaveTheSameStoreId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.server.CoreServerModule.CLUSTER_ID_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.RANDOM_NUMBER;

	public class ClusterBindingIT
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterBindingIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _clusterRule );
		}

		 private readonly ClusterRule _clusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(CausalClusteringSettings.raft_log_pruning_strategy, "3 entries").withSharedCoreParam(CausalClusteringSettings.raft_log_rotation_size, "1K");
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(clusterRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private FileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _fs = _fileSystemRule.get();
			  _cluster = _clusterRule.startCluster();
			  _cluster.coreTx((db, tx) =>
			  {
				SampleData.CreateSchema( db );
				tx.success();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allServersShouldHaveTheSameStoreId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllServersShouldHaveTheSameStoreId()
		 {
			  // WHEN
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  IList<File> coreStoreDirs = DatabaseDirs( _cluster.coreMembers() );

			  _cluster.shutdown();

			  // THEN
			  assertAllStoresHaveTheSameStoreId( coreStoreDirs, _fs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenWeRestartTheClusterAllServersShouldStillHaveTheSameStoreId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WhenWeRestartTheClusterAllServersShouldStillHaveTheSameStoreId()
		 {
			  // GIVEN
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  _cluster.shutdown();
			  // WHEN
			  _cluster.start();

			  IList<File> coreStoreDirs = DatabaseDirs( _cluster.coreMembers() );

			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  _cluster.shutdown();

			  // THEN
			  assertAllStoresHaveTheSameStoreId( coreStoreDirs, _fs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Ignore("Fix this test by having the bootstrapper augment his store and bind it using store-id on disk.") public void shouldNotJoinClusterIfHasDataWithDifferentStoreId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotJoinClusterIfHasDataWithDifferentStoreId()
		 {
			  // GIVEN
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  File databaseDirectory = _cluster.getCoreMemberById( 0 ).databaseDirectory();

			  _cluster.removeCoreMemberWithServerId( 0 );
			  ChangeStoreId( DatabaseLayout.of( databaseDirectory ) );

			  // WHEN
			  try
			  {
					_cluster.addCoreMemberWithId( 0 ).start();
					fail( "Should not have joined the cluster" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( LifecycleException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void laggingFollowerShouldDownloadSnapshot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LaggingFollowerShouldDownloadSnapshot()
		 {
			  // GIVEN
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  //TODO: Work out if/why this won't potentially remove a leader?
			  _cluster.removeCoreMemberWithServerId( 0 );

			  SampleData.CreateSomeData( 100, _cluster );

			  foreach ( CoreClusterMember db in _cluster.coreMembers() )
			  {
					Db.raftLogPruner().prune();
			  }

			  // WHEN
			  _cluster.addCoreMemberWithId( 0 ).start();

			  _cluster.awaitLeader();

			  // THEN
			  assertEquals( 3, _cluster.healthyCoreMembers().Count );

			  IList<File> coreStoreDirs = DatabaseDirs( _cluster.coreMembers() );
			  _cluster.shutdown();
			  assertAllStoresHaveTheSameStoreId( coreStoreDirs, _fs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void badFollowerShouldNotJoinCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BadFollowerShouldNotJoinCluster()
		 {
			  // GIVEN
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  CoreClusterMember coreMember = _cluster.getCoreMemberById( 0 );
			  _cluster.removeCoreMemberWithServerId( 0 );
			  ChangeClusterId( coreMember );

			  SampleData.CreateSomeData( 100, _cluster );

			  foreach ( CoreClusterMember db in _cluster.coreMembers() )
			  {
					Db.raftLogPruner().prune();
			  }

			  // WHEN
			  try
			  {
					_cluster.addCoreMemberWithId( 0 ).start();
					fail( "Should not have joined the cluster" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( LifecycleException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aNewServerShouldJoinTheClusterByDownloadingASnapshot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ANewServerShouldJoinTheClusterByDownloadingASnapshot()
		 {
			  // GIVEN
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  SampleData.CreateSomeData( 100, _cluster );

			  foreach ( CoreClusterMember db in _cluster.coreMembers() )
			  {
					Db.raftLogPruner().prune();
			  }

			  // WHEN
			  _cluster.addCoreMemberWithId( 4 ).start();

			  _cluster.awaitLeader();

			  // THEN
			  assertEquals( 4, _cluster.healthyCoreMembers().Count );

			  IList<File> coreStoreDirs = DatabaseDirs( _cluster.coreMembers() );
			  _cluster.shutdown();
			  assertAllStoresHaveTheSameStoreId( coreStoreDirs, _fs );
		 }

		 private static IList<File> DatabaseDirs( ICollection<CoreClusterMember> dbs )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return dbs.Select( CoreClusterMember::databaseDirectory ).ToList();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void changeClusterId(org.neo4j.causalclustering.discovery.CoreClusterMember coreMember) throws java.io.IOException
		 private void ChangeClusterId( CoreClusterMember coreMember )
		 {
			  SimpleStorage<ClusterId> clusterIdStorage = new SimpleFileStorage<ClusterId>( _fs, coreMember.ClusterStateDirectory(), CLUSTER_ID_NAME, new ClusterId.Marshal(), NullLogProvider.Instance );
			  clusterIdStorage.WriteState( new ClusterId( System.Guid.randomUUID() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void changeStoreId(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws Exception
		 private void ChangeStoreId( DatabaseLayout databaseLayout )
		 {
			  File neoStoreFile = databaseLayout.MetadataStore();
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler(), PageCache pageCache = StandalonePageCacheFactory.createPageCache(_fs, jobScheduler) )
			  {
					MetaDataStore.setRecord( pageCache, neoStoreFile, RANDOM_NUMBER, DateTimeHelper.CurrentUnixTimeMillis() );
			  }
		 }
	}

}