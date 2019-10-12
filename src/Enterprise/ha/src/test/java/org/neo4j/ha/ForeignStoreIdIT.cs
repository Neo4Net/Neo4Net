using System;

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
namespace Neo4Net.ha
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.cluster_server;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.initial_hosts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.server_id;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.HaSettings.ha_server;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.HaSettings.state_switch_timeout;

	public class ForeignStoreIdIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private GraphDatabaseService _firstInstance;
		 private GraphDatabaseService _foreignInstance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  if ( _foreignInstance != null )
			  {
					_foreignInstance.shutdown();
			  }
			  if ( _firstInstance != null )
			  {
					_firstInstance.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyForeignDbShouldJoinAfterHavingItsEmptyDbDeleted()
		 public virtual void EmptyForeignDbShouldJoinAfterHavingItsEmptyDbDeleted()
		 {
			  // GIVEN
			  // -- one instance running
			  int firstInstanceClusterPort = PortAuthority.allocatePort();
			  _firstInstance = ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.directory("1")).setConfig(server_id, "1").setConfig(cluster_server, "127.0.0.1:" + firstInstanceClusterPort).setConfig(ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(initial_hosts, "127.0.0.1:" + firstInstanceClusterPort).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
			  // -- another instance preparing to join with a store with a different store ID
			  File foreignDbStoreDir = CreateAnotherStore( TestDirectory.databaseDir( "2" ), 0 );

			  // WHEN
			  // -- the other joins
			  _foreignInstance = ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(foreignDbStoreDir).setConfig(server_id, "2").setConfig(initial_hosts, "127.0.0.1:" + firstInstanceClusterPort).setConfig(cluster_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
			  // -- and creates a node
			  long foreignNode = CreateNode( _foreignInstance, "foreigner" );

			  // THEN
			  // -- that node should arrive at the master
			  assertEquals( foreignNode, FindNode( _firstInstance, "foreigner" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonEmptyForeignDbShouldNotBeAbleToJoin()
		 public virtual void NonEmptyForeignDbShouldNotBeAbleToJoin()
		 {
			  // GIVEN
			  // -- one instance running
			  int firstInstanceClusterPort = PortAuthority.allocatePort();
			  _firstInstance = ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.directory("1")).setConfig(server_id, "1").setConfig(initial_hosts, "127.0.0.1:" + firstInstanceClusterPort).setConfig(cluster_server, "127.0.0.1:" + firstInstanceClusterPort).setConfig(ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
			  CreateNodes( _firstInstance, 3, "first" );
			  // -- another instance preparing to join with a store with a different store ID
			  File foreignDbStoreDir = CreateAnotherStore( TestDirectory.databaseDir( "2" ), 1 );

			  // WHEN
			  // -- the other joins
			  _foreignInstance = ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(foreignDbStoreDir).setConfig(server_id, "2").setConfig(initial_hosts, "127.0.0.1:" + firstInstanceClusterPort).setConfig(cluster_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(state_switch_timeout, "5s").setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

			  try
			  {
					// THEN
					// -- that node should arrive at the master
					CreateNode( _foreignInstance, "foreigner" );
					fail( "Shouldn't be able to create a node, since it shouldn't have joined" );
			  }
			  catch ( Exception )
			  {
					// Good
			  }
		 }

		 private long FindNode( GraphDatabaseService db, string name )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 if ( name.Equals( node.GetProperty( "name", null ) ) )
						 {
							  return node.Id;
						 }
					}
					fail( "Didn't find node '" + name + "' in " + db );
					return -1; // will never happen
			  }
		 }

		 private File CreateAnotherStore( File directory, int transactions )
		 {
			  GraphDatabaseService db = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(directory).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
			  CreateNodes( db, transactions, "node" );
			  Db.shutdown();
			  return directory;
		 }

		 private void CreateNodes( GraphDatabaseService db, int transactions, string prefix )
		 {
			  for ( int i = 0; i < transactions; i++ )
			  {
					CreateNode( db, prefix + i );
			  }
		 }

		 private long CreateNode( GraphDatabaseService db, string name )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( "name", name );
					tx.Success();
					return node.Id;
			  }
		 }
	}

}