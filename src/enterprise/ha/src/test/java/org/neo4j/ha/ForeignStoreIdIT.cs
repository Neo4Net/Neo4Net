using System;

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
namespace Neo4Net.ha
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.GraphDb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.ClusterSettings.cluster_server;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.ClusterSettings.initial_hosts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.ClusterSettings.server_id;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.HaSettings.ha_server;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.HaSettings.state_switch_timeout;

	public class ForeignStoreIdIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private IGraphDatabaseService _firstInstance;
		 private IGraphDatabaseService _foreignInstance;

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

		 private long FindNode( IGraphDatabaseService db, string name )
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
			  IGraphDatabaseService db = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(directory).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
			  CreateNodes( db, transactions, "node" );
			  Db.shutdown();
			  return directory;
		 }

		 private void CreateNodes( IGraphDatabaseService db, int transactions, string prefix )
		 {
			  for ( int i = 0; i < transactions; i++ )
			  {
					CreateNode( db, prefix + i );
			  }
		 }

		 private long CreateNode( IGraphDatabaseService db, string name )
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