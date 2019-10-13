using System;

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
namespace Neo4Net.restore
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using CommandLocator = Neo4Net.CommandLine.Admin.CommandLocator;
	using Usage = Neo4Net.CommandLine.Admin.Usage;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using RelationshipIndex = Neo4Net.Graphdb.index.RelationshipIndex;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using StoreLocker = Neo4Net.Kernel.@internal.locker.StoreLocker;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayWithSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.emptyArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class RestoreDatabaseCommandIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void forceShouldRespectStoreLock()
		 public virtual void ForceShouldRespectStoreLock()
		 {
			  string databaseName = "to";
			  Config config = ConfigWith( databaseName, Directory.absolutePath().AbsolutePath );

			  File fromPath = new File( Directory.absolutePath(), "from" );
			  File toPath = config.Get( GraphDatabaseSettings.database_path );
			  int fromNodeCount = 10;
			  int toNodeCount = 20;

			  CreateDbAt( fromPath, fromNodeCount );
			  CreateDbAt( toPath, toNodeCount );

			  FileSystemAbstraction fs = FileSystemRule.get();
			  try
			  {
					  using ( StoreLocker storeLocker = new StoreLocker( fs, DatabaseLayout.of( toPath ).StoreLayout ) )
					  {
						storeLocker.CheckLock();
      
						( new RestoreDatabaseCommand( fs, fromPath, config, databaseName, true ) ).Execute();
						fail( "expected exception" );
					  }
			  }
			  catch ( Exception e )
			  {
					assertThat( e.Message, equalTo( "the database is in use -- stop Neo4j and try again" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCopyOverAndExistingDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCopyOverAndExistingDatabase()
		 {
			  // given
			  string databaseName = "to";
			  Config config = ConfigWith( databaseName, Directory.absolutePath().AbsolutePath );

			  File fromPath = new File( Directory.absolutePath(), "from" );
			  File toPath = config.Get( GraphDatabaseSettings.database_path );

			  CreateDbAt( fromPath, 0 );
			  CreateDbAt( toPath, 0 );

			  try
			  {
					// when

					( new RestoreDatabaseCommand( FileSystemRule.get(), fromPath, config, databaseName, false ) ).execute();
					fail( "Should have thrown exception" );
			  }
			  catch ( System.ArgumentException exception )
			  {
					// then
					assertTrue( exception.Message, exception.Message.contains( "Database with name [to] already exists" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionIfBackupDirectoryDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionIfBackupDirectoryDoesNotExist()
		 {
			  // given
			  string databaseName = "to";
			  Config config = ConfigWith( databaseName, Directory.absolutePath().AbsolutePath );

			  File fromPath = new File( Directory.absolutePath(), "from" );
			  File toPath = config.Get( GraphDatabaseSettings.database_path );

			  CreateDbAt( toPath, 0 );

			  try
			  {
					// when

					( new RestoreDatabaseCommand( FileSystemRule.get(), fromPath, config, databaseName, false ) ).execute();
					fail( "Should have thrown exception" );
			  }
			  catch ( System.ArgumentException exception )
			  {
					// then
					assertTrue( exception.Message, exception.Message.contains( "Source directory does not exist" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionIfBackupDirectoryDoesNotHaveStoreFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionIfBackupDirectoryDoesNotHaveStoreFiles()
		 {
			  // given
			  string databaseName = "to";
			  Config config = ConfigWith( databaseName, Directory.absolutePath().AbsolutePath );

			  File fromPath = new File( Directory.absolutePath(), "from" );
			  assertTrue( fromPath.mkdirs() );

			  try
			  {
					// when
					( new RestoreDatabaseCommand( FileSystemRule.get(), fromPath, config, databaseName, false ) ).execute();
					fail( "Should have thrown exception" );
			  }
			  catch ( System.ArgumentException exception )
			  {
					// then
					assertTrue( exception.Message, exception.Message.contains( "Source directory is not a database backup" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowForcedCopyOverAnExistingDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowForcedCopyOverAnExistingDatabase()
		 {
			  // given
			  string databaseName = "to";
			  Config config = ConfigWith( databaseName, Directory.absolutePath().AbsolutePath );

			  File fromPath = new File( Directory.absolutePath(), "from" );
			  File toPath = config.Get( GraphDatabaseSettings.database_path );
			  int fromNodeCount = 10;
			  int toNodeCount = 20;

			  CreateDbAt( fromPath, fromNodeCount );
			  CreateDbAt( toPath, toNodeCount );

			  // when
			  ( new RestoreDatabaseCommand( FileSystemRule.get(), fromPath, config, databaseName, true ) ).execute();

			  // then
			  GraphDatabaseService copiedDb = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(toPath).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

			  using ( Transaction ignored = copiedDb.BeginTx() )
			  {
					assertEquals( fromNodeCount, Iterables.count( copiedDb.AllNodes ) );
			  }

			  copiedDb.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void restoreExplicitIndexesFromBackup() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RestoreExplicitIndexesFromBackup()
		 {
			  string databaseName = "destination";
			  Config config = ConfigWith( databaseName, Directory.absolutePath().AbsolutePath );
			  File fromPath = new File( Directory.absolutePath(), "from" );
			  File toPath = config.Get( GraphDatabaseSettings.database_path );

			  CreateDbWithExplicitIndexAt( fromPath, 100 );

			  ( new RestoreDatabaseCommand( FileSystemRule.get(), fromPath, config, databaseName, true ) ).execute();

			  GraphDatabaseService restoredDatabase = CreateDatabase( toPath, toPath.AbsolutePath );

			  using ( Transaction transaction = restoredDatabase.BeginTx() )
			  {
					IndexManager indexManager = restoredDatabase.Index();
					string[] nodeIndexNames = indexManager.NodeIndexNames();
					string[] relationshipIndexNames = indexManager.RelationshipIndexNames();

					foreach ( string nodeIndexName in nodeIndexNames )
					{
						 CountNodesByKeyValue( indexManager, nodeIndexName, "a", "b" );
						 CountNodesByKeyValue( indexManager, nodeIndexName, "c", "d" );
					}

					foreach ( string relationshipIndexName in relationshipIndexNames )
					{
						 CountRelationshipByKeyValue( indexManager, relationshipIndexName, "x", "y" );
					}
			  }
			  restoredDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void restoreTransactionLogsInCustomDirectoryForTargetDatabaseWhenConfigured() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RestoreTransactionLogsInCustomDirectoryForTargetDatabaseWhenConfigured()
		 {
			  string databaseName = "to";
			  Config config = ConfigWith( databaseName, Directory.absolutePath().AbsolutePath );
			  File customTxLogDirectory = Directory.directory( "customLogicalLog" );
			  string customTransactionLogDirectory = customTxLogDirectory.AbsolutePath;
			  config.AugmentDefaults( GraphDatabaseSettings.logical_logs_location, customTransactionLogDirectory );

			  File fromPath = new File( Directory.absolutePath(), "from" );
			  File toPath = config.Get( GraphDatabaseSettings.database_path );
			  int fromNodeCount = 10;
			  int toNodeCount = 20;
			  CreateDbAt( fromPath, fromNodeCount );

			  GraphDatabaseService db = CreateDatabase( toPath, customTransactionLogDirectory );
			  CreateTestData( toNodeCount, db );
			  Db.shutdown();

			  // when
			  ( new RestoreDatabaseCommand( FileSystemRule.get(), fromPath, config, databaseName, true ) ).execute();

			  LogFiles fromStoreLogFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( fromPath, FileSystemRule.get() ).build();
			  LogFiles toStoreLogFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( toPath, FileSystemRule.get() ).build();
			  LogFiles customLogLocationLogFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( customTxLogDirectory, FileSystemRule.get() ).build();
			  assertThat( toStoreLogFiles.LogFilesConflict(), emptyArray() );
			  assertThat( customLogLocationLogFiles.LogFilesConflict(), arrayWithSize(1) );
			  assertEquals( fromStoreLogFiles.GetLogFileForVersion( 0 ).length(), customLogLocationLogFiles.GetLogFileForVersion(0).length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotRemoveRelativeTransactionDirectoryAgain() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotRemoveRelativeTransactionDirectoryAgain()
		 {
			  FileSystemAbstraction fileSystem = Mockito.spy( FileSystemRule.get() );
			  File fromPath = Directory.directory( "from" );
			  File databaseFile = Directory.directory();
			  File relativeLogDirectory = Directory.directory( "relativeDirectory" );

			  Config config = Config.defaults( GraphDatabaseSettings.database_path, databaseFile.AbsolutePath );
			  config.augment( GraphDatabaseSettings.logical_logs_location, relativeLogDirectory.AbsolutePath );

			  CreateDbAt( fromPath, 10 );

			  ( new RestoreDatabaseCommand( fileSystem, fromPath, config, "testDatabase", true ) ).Execute();

			  verify( fileSystem ).deleteRecursively( eq( databaseFile ) );
			  verify( fileSystem, never() ).deleteRecursively(eq(relativeLogDirectory));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintNiceHelp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintNiceHelp()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "neo4j-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new RestoreDatabaseCliProvider(), ps.println );

					assertEquals( string.Format( "usage: neo4j-admin restore --from=<backup-directory> [--database=<name>]%n" + "                           [--force[=<true|false>]]%n" + "%n" + "environment variables:%n" + "    NEO4J_CONF    Path to directory which contains neo4j.conf.%n" + "    NEO4J_DEBUG   Set to anything to enable debug output.%n" + "    NEO4J_HOME    Neo4j home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "Restore a backed up database.%n" + "%n" + "options:%n" + "  --from=<backup-directory>   Path to backup to restore from.%n" + "  --database=<name>           Name of database. [default:graph.db]%n" + "  --force=<true|false>        If an existing database should be replaced.%n" + "                              [default:false]%n" ), baos.ToString() );
			  }
		 }

		 private static void CountRelationshipByKeyValue( IndexManager indexManager, string indexName, string key, string value )
		 {
			  using ( IndexHits<Relationship> nodes = indexManager.ForRelationships( indexName ).get( key, value ) )
			  {
					assertEquals( 50, nodes.Size() );
			  }
		 }

		 private static void CountNodesByKeyValue( IndexManager indexManager, string indexName, string key, string value )
		 {
			  using ( IndexHits<Node> nodes = indexManager.ForNodes( indexName ).get( key, value ) )
			  {
					assertEquals( 50, nodes.Size() );
			  }
		 }

		 private static Config ConfigWith( string databaseName, string dataDirectory )
		 {
			  return Config.defaults( stringMap( GraphDatabaseSettings.active_database.name(), databaseName, GraphDatabaseSettings.data_directory.name(), dataDirectory ) );
		 }

		 private void CreateDbAt( File fromPath, int nodesToCreate )
		 {
			  GraphDatabaseService db = CreateDatabase( fromPath, fromPath.AbsolutePath );

			  CreateTestData( nodesToCreate, db );

			  Db.shutdown();
		 }

		 private GraphDatabaseService CreateDatabase( File fromPath, string absolutePath )
		 {
			  return ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(fromPath).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).setConfig(GraphDatabaseSettings.logical_logs_location, absolutePath).newGraphDatabase();
		 }

		 private void CreateDbWithExplicitIndexAt( File fromPath, int pairNumberOfNodesToCreate )
		 {
			  GraphDatabaseService db = CreateDatabase( fromPath, fromPath.AbsolutePath );

			  Index<Node> explicitNodeIndex;
			  RelationshipIndex explicitRelationshipIndex;
			  using ( Transaction transaction = Db.beginTx() )
			  {
					explicitNodeIndex = Db.index().forNodes("explicitNodeIndex");
					explicitRelationshipIndex = Db.index().forRelationships("explicitRelationshipIndex");
					transaction.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < pairNumberOfNodesToCreate; i += 2 )
					{
						 Node node = Db.createNode();
						 Node otherNode = Db.createNode();
						 Relationship relationship = node.CreateRelationshipTo( otherNode, RelationshipType.withName( "rel" ) );

						 explicitNodeIndex.Add( node, "a", "b" );
						 explicitNodeIndex.Add( otherNode, "c", "d" );
						 explicitRelationshipIndex.add( relationship, "x", "y" );
					}
					tx.Success();
			  }
			  Db.shutdown();
		 }

		 private static void CreateTestData( int nodesToCreate, GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < nodesToCreate; i++ )
					{
						 Db.createNode();
					}
					tx.Success();
			  }
		 }
	}

}