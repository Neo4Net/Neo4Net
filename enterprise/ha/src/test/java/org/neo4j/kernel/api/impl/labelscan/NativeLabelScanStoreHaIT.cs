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
namespace Org.Neo4j.Kernel.api.impl.labelscan
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TestHighlyAvailableGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using ManagedCluster = Org.Neo4j.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allAvailabilityGuardsReleased;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;

	public class NativeLabelScanStoreHaIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private readonly LifeSupport _life = new LifeSupport();
		 private ClusterManager.ManagedCluster _cluster;
		 private readonly TestMonitor _monitor = new TestMonitor();

		 private enum Labels
		 {
			  First,
			  Second
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  TestHighlyAvailableGraphDatabaseFactory factory = new TestHighlyAvailableGraphDatabaseFactory();
			  Monitors monitors = new Monitors();
			  monitors.AddMonitorListener( _monitor );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  factory.RemoveKernelExtensions( extension => extension.GetType().FullName.Contains("LabelScan") );
			  ClusterManager clusterManager = ( new ClusterManager.Builder( TestDirectory.directory( "root" ) ) ).withDbFactory( factory ).withMonitors( monitors ).withStoreDirInitializer((serverId, storeDir) =>
			  {
						  if ( serverId == 1 )
						  {
								GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
								try
								{
									 CreateSomeLabeledNodes( db, new Label[]{ Labels.First }, new Label[]{ Labels.First, Labels.Second }, new Label[]{ Labels.Second } );
								}
								finally
								{
									 Db.shutdown();
								}
						  }
			  }).build();
			  _life.add( clusterManager );
			  _life.start();
			  _cluster = clusterManager.Cluster;
			  _cluster.await( allSeesAllAsAvailable() );
			  _cluster.await( allAvailabilityGuardsReleased() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _life.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopyLabelScanStoreToNewSlaves()
		 public virtual void ShouldCopyLabelScanStoreToNewSlaves()
		 {
			  // This check is here o check so that the extension provided by this test is selected.
			  // It can be higher than 3 (number of cluster members) since some members may restart
			  // some services to switch role.
			  assertTrue( "Expected initial calls to init to be at least one per cluster member (>= 3), but was " + _monitor.initCalls.get(), _monitor.initCalls.get() >= 3 );

			  // GIVEN
			  // An HA cluster where the master started with initial data consisting
			  // of a couple of nodes, each having one or more properties.
			  // The cluster starting up should have the slaves copy their stores from the master
			  // and get the label scan store with it.

			  // THEN
			  assertEquals( "Expected none to build their label scan store index.", 0, _monitor.timesRebuiltWithData.get() );
			  foreach ( GraphDatabaseService db in _cluster.AllMembers )
			  {
					assertEquals( 2, NumberOfNodesHavingLabel( db, Labels.First ) );
					assertEquals( 2, NumberOfNodesHavingLabel( db, Labels.First ) );
			  }
		 }

		 private static long NumberOfNodesHavingLabel( GraphDatabaseService db, Label label )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					long count = count( Db.findNodes( label ) );
					tx.Success();
					return count;
			  }
		 }

		 private static void CreateSomeLabeledNodes( GraphDatabaseService db, params Label[][] labelArrays )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Label[] labels in labelArrays )
					{
						 Db.createNode( labels );
					}
					tx.Success();
			  }
		 }

		 private class TestMonitor : Org.Neo4j.Kernel.api.labelscan.LabelScanStore_Monitor_Adaptor
		 {
			  internal readonly AtomicInteger InitCalls = new AtomicInteger();
			  internal readonly AtomicInteger TimesRebuiltWithData = new AtomicInteger();

			  public override void Init()
			  {
					InitCalls.incrementAndGet();
			  }

			  public override void NoIndex()
			  {
			  }

			  public override void NotValidIndex()
			  {
			  }

			  public override void Rebuilding()
			  {
			  }

			  public override void Rebuilt( long roughNodeCount )
			  {
					// In HA each slave database will startup with an empty database before realizing that
					// it needs to copy a store from its master, let alone find its master.
					// So we're expecting one call to this method from each slave with node count == 0. Ignore those.
					// We're tracking number of times we're rebuilding the index where there's data to rebuild,
					// i.e. after the db has been copied from the master.
					if ( roughNodeCount > 0 )
					{
						 TimesRebuiltWithData.incrementAndGet();
					}
			  }
		 }
	}

}