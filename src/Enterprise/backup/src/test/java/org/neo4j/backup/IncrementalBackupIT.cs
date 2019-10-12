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
namespace Neo4Net.backup
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using TestName = org.junit.rules.TestName;

	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class IncrementalBackupIT
	{
		private bool InstanceFieldsInitialized = false;

		public IncrementalBackupIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _testName ).around( TestDirectory ).around( SuppressOutput );
		}

		 private readonly TestName _testName = new TestName();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(testName).around(testDirectory).around(suppressOutput);
		 public RuleChain Rules;

		 private File _serverPath;
		 private File _backupDatabase;
		 private ServerInterface _server;
		 private GraphDatabaseService _db;
		 private File _backupStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _serverPath = TestDirectory.storeDir( "server" );
			  _backupStore = TestDirectory.storeDir( "backupStore" );
			  _backupDatabase = TestDirectory.databaseDir( _backupStore );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutItDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShutItDown()
		 {
			  if ( _server != null )
			  {
					ShutdownServer( _server );
					_server = null;
			  }
			  if ( _db != null )
			  {
					_db.shutdown();
					_db = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoIncrementalBackup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDoIncrementalBackup()
		 {
			  DbRepresentation initialDataSetRepresentation = CreateInitialDataSet( _serverPath );
			  int port = PortAuthority.allocatePort();
			  _server = StartServer( _serverPath, "127.0.0.1:" + port );

			  OnlineBackup backup = OnlineBackup.From( "127.0.0.1", port );

			  backup.Full( _backupDatabase.Path );

			  assertEquals( initialDataSetRepresentation, BackupDbRepresentation );
			  ShutdownServer( _server );

			  DbRepresentation furtherRepresentation = AddMoreData2( _serverPath );
			  _server = StartServer( _serverPath, "127.0.0.1:" + port );
			  backup.incremental( _backupDatabase.Path );
			  assertEquals( furtherRepresentation, BackupDbRepresentation );
			  ShutdownServer( _server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotServeTransactionsWithInvalidHighIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotServeTransactionsWithInvalidHighIds()
		 {
			  /*
			   * This is in effect a high level test for an edge case that happens when a relationship group is
			   * created and deleted in the same tx. This can end up causing an IllegalArgumentException because
			   * the HighIdApplier used when applying incremental updates (batch transactions in general) will postpone
			   * processing of added/altered record ids but deleted ids will be processed on application. This can result
			   * in a deleted record causing an IllegalArgumentException even though it is not the highest id in the tx.
			   *
			   * The way we try to trigger this is:
			   * 0. In one tx, create a node with 49 relationships, belonging to two types.
			   * 1. In another tx, create another relationship on that node (making it dense) and then delete all
			   *    relationships of one type. This results in the tx state having a relationship group record that was
			   *    created in this tx and also set to not in use.
			   * 2. Receipt of this tx will have the offending rel group command apply its id before the groups that are
			   *    altered. This will try to update the high id with a value larger than what has been seen previously and
			   *    fail the update.
			   * The situation is resolved by a check added in TransactionRecordState which skips the creation of such
			   * commands.
			   * Note that this problem can also happen in HA slaves.
			   */
			  DbRepresentation initialDataSetRepresentation = CreateInitialDataSet( _serverPath );
			  int port = PortAuthority.allocatePort();
			  _server = StartServer( _serverPath, "127.0.0.1:" + port );

			  OnlineBackup backup = OnlineBackup.From( "127.0.0.1", port );

			  backup.Full( _backupDatabase.Path );

			  assertEquals( initialDataSetRepresentation, BackupDbRepresentation );
			  ShutdownServer( _server );

			  DbRepresentation furtherRepresentation = CreateTransactionWithWeirdRelationshipGroupRecord( _serverPath );
			  _server = StartServer( _serverPath, "127.0.0.1:" + port );
			  backup.incremental( _backupDatabase.Path );
			  assertEquals( furtherRepresentation, BackupDbRepresentation );
			  ShutdownServer( _server );
		 }

		 private DbRepresentation CreateInitialDataSet( File path )
		 {
			  _db = StartGraphDatabase( path );
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.createNode().setProperty("name", "Goofy");
					Node donald = _db.createNode();
					donald.SetProperty( "name", "Donald" );
					Node daisy = _db.createNode();
					daisy.SetProperty( "name", "Daisy" );
					Relationship knows = donald.CreateRelationshipTo( daisy, RelationshipType.withName( "LOVES" ) );
					knows.SetProperty( "since", 1940 );
					tx.Success();
			  }
			  DbRepresentation result = DbRepresentation.of( _db );
			  _db.shutdown();
			  return result;
		 }

		 private DbRepresentation AddMoreData2( File path )
		 {
			  _db = StartGraphDatabase( path );
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node donald = _db.getNodeById( 2 );
					Node gladstone = _db.createNode();
					gladstone.SetProperty( "name", "Gladstone" );
					Relationship hates = donald.CreateRelationshipTo( gladstone, RelationshipType.withName( "HATES" ) );
					hates.SetProperty( "since", 1948 );
					tx.Success();
			  }
			  DbRepresentation result = DbRepresentation.of( _db );
			  _db.shutdown();
			  return result;
		 }

		 private DbRepresentation CreateTransactionWithWeirdRelationshipGroupRecord( File path )
		 {
			  _db = StartGraphDatabase( path );
			  int i = 0;
			  Node node;
			  RelationshipType typeToDelete = RelationshipType.withName( "A" );
			  RelationshipType theOtherType = RelationshipType.withName( "B" );
			  int defaultDenseNodeThreshold = int.Parse( GraphDatabaseSettings.dense_node_threshold.DefaultValue );

			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					for ( ; i < defaultDenseNodeThreshold - 1; i++ )
					{
						 node.CreateRelationshipTo( _db.createNode(), theOtherType );
					}
					node.CreateRelationshipTo( _db.createNode(), typeToDelete );
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					node.CreateRelationshipTo( _db.createNode(), theOtherType );
					foreach ( Relationship relationship in node.GetRelationships( Direction.BOTH, typeToDelete ) )
					{
						 relationship.Delete();
					}
					tx.Success();
			  }
			  DbRepresentation result = DbRepresentation.of( _db );
			  _db.shutdown();
			  return result;
		 }

		 private static GraphDatabaseService StartGraphDatabase( File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).setConfig(GraphDatabaseSettings.keep_logical_logs, Settings.TRUE).newGraphDatabase();
		 }

		 private static ServerInterface StartServer( File storeDir, string serverAddress )
		 {
			  ServerInterface server = new EmbeddedServer( storeDir, serverAddress );
			  server.AwaitStarted();
			  return server;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void shutdownServer(ServerInterface server) throws Exception
		 private static void ShutdownServer( ServerInterface server )
		 {
			  server.Shutdown();
			  Thread.Sleep( 1000 );
		 }

		 private DbRepresentation BackupDbRepresentation
		 {
			 get
			 {
				  return DbRepresentation.of( _backupDatabase, Config.defaults( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ) );
			 }
		 }
	}

}