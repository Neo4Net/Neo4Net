using System.Collections.Generic;
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
namespace Neo4Net.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using StoreUtil = Neo4Net.com.storecopy.StoreUtil;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using Neo4Net.Graphdb.index;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using Monitor = Neo4Net.Kernel.ha.cluster.SwitchToSlave.Monitor;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using RepairKit = Neo4Net.Kernel.impl.ha.ClusterManager.RepairKit;
	using Neo4Net.Kernel.impl.util;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.UNKNOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.memberThinksItIsRole;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.RetryACoupleOfTimesHandler.ANY_EXCEPTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.RetryACoupleOfTimesHandler.retryACoupleOfTimesOn;

	public class BranchedDataIT
	{
		private bool InstanceFieldsInitialized = false;

		public BranchedDataIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _life );
		}

		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(life);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrationOfBranchedDataDirectories() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MigrationOfBranchedDataDirectories()
		 {
			  long[] timestamps = new long[3];
			  for ( int i = 0; i < timestamps.Length; i++ )
			  {
					StartDbAndCreateNode();
					timestamps[i] = MoveAwayToLookLikeOldBranchedDirectory();
					Thread.Sleep( 1 ); // To make sure we get different timestamps
			  }

			  File databaseDirectory = _directory.databaseDir();
			  int clusterPort = PortAuthority.allocatePort();
			  ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(databaseDirectory).setConfig(ClusterSettings.server_id, "1").setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.initial_hosts, "localhost:" + clusterPort).setConfig(HaSettings.ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase().shutdown();
			  // It should have migrated those to the new location. Verify that.
			  foreach ( long timestamp in timestamps )
			  {
					assertFalse( "directory branched-" + timestamp + " still exists.", ( new File( databaseDirectory, "branched-" + timestamp ) ).exists() );
					assertTrue( "directory " + timestamp + " is not there", StoreUtil.getBranchedDataDirectory( databaseDirectory, timestamp ).exists() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopyStoreFromMasterIfBranched() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCopyStoreFromMasterIfBranched()
		 {
			  // GIVEN
			  File dir = _directory.directory();
			  ClusterManager clusterManager = _life.add(new ClusterManager.Builder(dir)
						 .withCluster( clusterOfSize( 2 ) ).build());
			  ClusterManager.ManagedCluster cluster = clusterManager.Cluster;
			  cluster.Await( allSeesAllAsAvailable() );
			  CreateNode( cluster.Master, "A" );
			  cluster.Sync();

			  // WHEN
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;
			  File databaseDir = slave.DatabaseLayout().databaseDirectory();
			  ClusterManager.RepairKit starter = cluster.Shutdown( slave );
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  CreateNode( master, "B1" );
			  CreateNode( master, "C" );
			  CreateNodeOffline( databaseDir, "B2" );
			  slave = starter.Repair();

			  // THEN
			  cluster.Await( allSeesAllAsAvailable() );
			  slave.BeginTx().close();
		 }

		 /// <summary>
		 /// Main difference to <seealso cref="shouldCopyStoreFromMasterIfBranched()"/> is that no instances are shut down
		 /// during the course of the test. This to test functionality of some internal components being restarted.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldCopyStoreFromMasterIfBranchedInLiveScenario() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCopyStoreFromMasterIfBranchedInLiveScenario()
		 {
			  // GIVEN a cluster of 3, all having the same data (node A)
			  // thor is whoever is the master to begin with
			  // odin is whoever is picked as _the_ slave given thor as initial master
			  File storeDirectory = _directory.directory();
			  ClusterManager clusterManager = _life.add(new ClusterManager.Builder(storeDirectory)
						 .withSharedConfig( stringMap( HaSettings.tx_push_factor.name(), "0", HaSettings.pull_interval.name(), "0" ) ).build());
			  ClusterManager.ManagedCluster cluster = clusterManager.Cluster;
			  cluster.Await( allSeesAllAsAvailable() );
			  HighlyAvailableGraphDatabase thor = cluster.Master;
			  string indexName = "valhalla";
			  CreateNode( thor, "A", AndIndexInto( indexName ) );
			  cluster.Sync();

			  // WHEN creating a node B1 on thor (note the disabled cluster transaction propagation)
			  CreateNode( thor, "B1", AndIndexInto( indexName ) );
			  // and right after that failing the master so that it falls out of the cluster
			  HighlyAvailableGraphDatabase odin = cluster.AnySlave;
			  cluster.Info( format( "%n   ==== TAMPERING WITH " + thor + "'s CABLES ====%n" ) );
			  ClusterManager.RepairKit thorRepairKit = cluster.Fail( thor );
			  // try to create a transaction on odin until it succeeds
			  cluster.Await( ClusterManager.masterAvailable( thor ) );
			  cluster.Await( ClusterManager.memberThinksItIsRole( odin, HighAvailabilityModeSwitcher.MASTER ) );
			  assertTrue( odin.Master );
			  RetryOnTransactionFailure( odin, db => createNode( db, "B2", AndIndexInto( indexName ) ) );
			  // perform transactions so that index files changes under the hood
			  ISet<File> odinLuceneFilesBefore = Iterables.asSet( GatherLuceneFiles( odin, indexName ) );
			  for ( char prefix = 'C'; !Changed( odinLuceneFilesBefore, Iterables.asSet( GatherLuceneFiles( odin, indexName ) ) ); prefix++ )
			  {
					char fixedPrefix = prefix;
					RetryOnTransactionFailure( odin, db => createNodes( odin, fixedPrefix.ToString(), 10_000, AndIndexInto(indexName) ) );
					cluster.Force(); // Force will most likely cause lucene explicit indexes to commit and change file structure
			  }
			  // so anyways, when thor comes back into the cluster
			  cluster.Info( format( "%n   ==== REPAIRING CABLES ====%n" ) );
			  cluster.Await( memberThinksItIsRole( thor, UNKNOWN ) );
			  BranchMonitor thorHasBranched = InstallBranchedDataMonitor( cluster.GetMonitorsByDatabase( thor ) );
			  thorRepairKit.Repair();
			  cluster.Await( memberThinksItIsRole( thor, SLAVE ) );
			  cluster.Await( memberThinksItIsRole( odin, MASTER ) );
			  cluster.Await( allSeesAllAsAvailable() );
			  assertFalse( thor.Master );
			  assertTrue( "No store-copy performed", thorHasBranched.CopyCompleted );
			  assertTrue( "Store-copy unsuccessful", thorHasBranched.CopySuccessful );

			  // Now do some more transactions on current master (odin) and have thor pull those
			  for ( int i = 0; i < 3; i++ )
			  {
					int ii = i;
					RetryOnTransactionFailure( odin, db => createNodes( odin, ( "" + ii ).ToString(), 10, AndIndexInto(indexName) ) );
					cluster.Sync();
					cluster.Force();
			  }

			  // THEN thor should be a slave, having copied a store from master and good to go
			  assertFalse( HasNode( thor, "B1" ) );
			  assertTrue( HasNode( thor, "B2" ) );
			  assertTrue( HasNode( thor, "C-0" ) );
			  assertTrue( HasNode( thor, "0-0" ) );
			  assertTrue( HasNode( odin, "0-0" ) );
		 }

		 private static BranchMonitor InstallBranchedDataMonitor( Monitors monitors )
		 {
			  BranchMonitor monitor = new BranchMonitor();
			  monitors.AddMonitorListener( monitor );
			  return monitor;
		 }

		 private static void RetryOnTransactionFailure( GraphDatabaseService db, System.Action<GraphDatabaseService> tx )
		 {
			  DatabaseRule.tx( db, retryACoupleOfTimesOn( ANY_EXCEPTION ), tx );
		 }

		 private static bool Changed( ISet<File> before, ISet<File> after )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  return !before.containsAll( after ) && !after.containsAll( before );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.Collection<java.io.File> gatherLuceneFiles(org.neo4j.kernel.ha.HighlyAvailableGraphDatabase db, String indexName) throws java.io.IOException
		 private static ICollection<File> GatherLuceneFiles( HighlyAvailableGraphDatabase db, string indexName )
		 {
			  ICollection<File> result = new List<File>();
			  NeoStoreDataSource ds = Db.DependencyResolver.resolveDependency( typeof( NeoStoreDataSource ) );
			  using ( ResourceIterator<StoreFileMetadata> files = ds.ListStoreFiles( false ) )
			  {
					while ( Files.MoveNext() )
					{
						 File file = Files.Current.file();
						 if ( file.Path.contains( indexName ) )
						 {
							  result.Add( file );
						 }
					}
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.kernel.impl.util.Listener<org.neo4j.graphdb.Node> andIndexInto(final String indexName)
		 private static Listener<Node> AndIndexInto( string indexName )
		 {
			  return node =>
			  {
				Index<Node> index = node.GraphDatabase.index().forNodes(indexName);
				foreach ( string key in node.PropertyKeys )
				{
					 index.add( node, key, node.getProperty( key ) );
				}
			  };
		 }

		 private static bool HasNode( GraphDatabaseService db, string nodeName )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 if ( nodeName.Equals( node.GetProperty( "name", null ) ) )
						 {
							  return true;
						 }
					}
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static void createNodeOffline(java.io.File storeDir, String name)
		 private static void CreateNodeOffline( File storeDir, string name )
		 {
			  GraphDatabaseService db = StartGraphDatabaseService( storeDir );
			  try
			  {
					CreateNode( db, name );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static void createNode(org.neo4j.graphdb.GraphDatabaseService db, String name, org.neo4j.kernel.impl.util.Listener<org.neo4j.graphdb.Node>... additional)
		 private static void CreateNode( GraphDatabaseService db, string name, params Listener<Node>[] additional )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = CreateNamedNode( db, name );
					foreach ( Listener<Node> listener in additional )
					{
						 listener.Receive( node );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static void createNodes(org.neo4j.graphdb.GraphDatabaseService db, String namePrefix, int count, org.neo4j.kernel.impl.util.Listener<org.neo4j.graphdb.Node>... additional)
		 private static void CreateNodes( GraphDatabaseService db, string namePrefix, int count, params Listener<Node>[] additional )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < count; i++ )
					{
						 Node node = CreateNamedNode( db, namePrefix + "-" + i );
						 foreach ( Listener<Node> listener in additional )
						 {
							  listener.Receive( node );
						 }
					}
					tx.Success();
			  }
		 }

		 private static Node CreateNamedNode( GraphDatabaseService db, string name )
		 {
			  Node node = Db.createNode();
			  node.SetProperty( "name", name );
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long moveAwayToLookLikeOldBranchedDirectory() throws java.io.IOException
		 private long MoveAwayToLookLikeOldBranchedDirectory()
		 {
			  File dir = _directory.databaseDir();
			  long timestamp = DateTimeHelper.CurrentUnixTimeMillis();
			  File branchDir = new File( dir, "branched-" + timestamp );
			  assertTrue( "create directory: " + branchDir, branchDir.mkdirs() );
			  foreach ( File file in Objects.requireNonNull( dir.listFiles() ) )
			  {
					string fileName = file.Name;
					if ( !fileName.Equals( "debug.log" ) && !file.Name.StartsWith( "branched-" ) )
					{
						 FileUtils.renameFile( file, new File( branchDir, file.Name ) );
					}
			  }
			  return timestamp;
		 }

		 private void StartDbAndCreateNode()
		 {
			  GraphDatabaseService db = StartGraphDatabaseService( _directory.absolutePath() );
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.createNode();
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private static GraphDatabaseService StartGraphDatabaseService( File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
		 }

		 private class BranchMonitor : Monitor
		 {
			  internal volatile bool CopyCompleted;
			  internal volatile bool CopySuccessful;

			  public override void StoreCopyCompleted( bool wasSuccessful )
			  {
					CopyCompleted = true;
					CopySuccessful = wasSuccessful;
			  }
		 }
	}

}