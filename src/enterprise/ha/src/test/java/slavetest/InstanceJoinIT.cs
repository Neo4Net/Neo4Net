using System.Collections.Generic;

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
namespace Slavetest
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.GraphDb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using UpdatePuller = Neo4Net.Kernel.ha.UpdatePuller;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.keep_logical_logs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;

	/*
	 * This test case ensures that instances with the same store id but very old txids
	 * will successfully join with a full version of the store.
	 */
	public class InstanceJoinIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureSlaveCanJoinEvenIfTooFarBackComparedToMaster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureSlaveCanJoinEvenIfTooFarBackComparedToMaster()
		 {
			  string key = "foo";
			  string value = "bar";

			  HighlyAvailableGraphDatabase master = null;
			  HighlyAvailableGraphDatabase slave = null;
			  File masterDir = TestDirectory.databaseDir( "master" );
			  File slaveDir = TestDirectory.databaseDir( "slave" );
			  try
			  {
					int masterClusterPort = PortAuthority.allocatePort();
					int masterHaPort = PortAuthority.allocatePort();
					master = Start( masterDir, 0, stringMap( keep_logical_logs.name(), "1 txs", ClusterSettings.initial_hosts.name(), "127.0.0.1:" + masterClusterPort ), masterClusterPort, masterHaPort );
					CreateNode( master, "something", "unimportant" );
					CheckPoint( master );
					// Need to start and shutdown the slave so when we start it up later it verifies instead of copying
					int slaveClusterPort = PortAuthority.allocatePort();
					int slaveHaPort = PortAuthority.allocatePort();
					slave = Start( slaveDir, 1, stringMap( ClusterSettings.initial_hosts.name(), "127.0.0.1:" + masterClusterPort ), slaveClusterPort, slaveHaPort );
					slave.Shutdown();

					CreateNode( master, key, value );
					CheckPoint( master );
					// Rotating, moving the above transactions away so they are removed on shutdown.
					RotateLog( master );

					/*
					 * We need to shutdown - rotating is not enough. The problem is that log positions are cached and they
					 * are not removed from the cache until we run into the cache limit. This means that the information
					 * contained in the log can actually be available even if the log is removed. So, to trigger the case
					 * of the master information missing from the master we need to also flush the log entry cache - hence,
					 * restart.
					 */
					master.Shutdown();
					master = Start( masterDir, 0, stringMap( keep_logical_logs.name(), "1 txs", ClusterSettings.initial_hosts.name(), "127.0.0.1:" + masterClusterPort ), masterClusterPort, masterHaPort );

					/*
					 * The new log on master needs to have at least one transaction, so here we go.
					 */
					int importantNodeCount = 10;
					for ( int i = 0; i < importantNodeCount; i++ )
					{
						 CreateNode( master, key, value );
						 CheckPoint( master );
						 RotateLog( master );
					}

					CheckPoint( master );

					slave = Start( slaveDir, 1, stringMap( ClusterSettings.initial_hosts.name(), "127.0.0.1:" + masterClusterPort ), slaveClusterPort, slaveHaPort );
					slave.DependencyResolver.resolveDependency( typeof( UpdatePuller ) ).pullUpdates();

					using ( Transaction ignore = slave.BeginTx() )
					{
						 assertEquals( "store contents differ", importantNodeCount + 1, NodesHavingProperty( slave, key, value ) );
					}
			  }
			  finally
			  {
					if ( slave != null )
					{
						 slave.Shutdown();
					}

					if ( master != null )
					{
						 master.Shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void rotateLog(Neo4Net.kernel.ha.HighlyAvailableGraphDatabase db) throws java.io.IOException
		 private static void RotateLog( HighlyAvailableGraphDatabase db )
		 {
			  Db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkPoint(Neo4Net.kernel.ha.HighlyAvailableGraphDatabase db) throws java.io.IOException
		 private static void CheckPoint( HighlyAvailableGraphDatabase db )
		 {
			  Db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint(new SimpleTriggerInfo("test")
			 );
		 }

		 private static int NodesHavingProperty( HighlyAvailableGraphDatabase slave, string key, string value )
		 {
			  using ( Transaction tx = slave.BeginTx() )
			  {
					int count = 0;
					foreach ( Node node in slave.AllNodes )
					{
						 if ( value.Equals( node.GetProperty( key, null ) ) )
						 {
							  count++;
						 }
					}
					tx.Success();
					return count;
			  }
		 }

		 private static void CreateNode( HighlyAvailableGraphDatabase db, string key, string value )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( key, value );
					tx.Success();
			  }
		 }

		 private static HighlyAvailableGraphDatabase Start( File storeDir, int i, IDictionary<string, string> additionalConfig, int clusterPort, int haPort )
		 {
			  HighlyAvailableGraphDatabase db = ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.server_id, i + "").setConfig(HaSettings.ha_server, "127.0.0.1:" + haPort).setConfig(HaSettings.pull_interval, "0ms").setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).setConfig(additionalConfig).newGraphDatabase();

			  AwaitStart( db );
			  return db;
		 }

		 private static void AwaitStart( HighlyAvailableGraphDatabase db )
		 {
			  Db.beginTx().close();
		 }
	}

}