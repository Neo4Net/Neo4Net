using System;
using System.Collections.Generic;

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
namespace Neo4Net.backup
{
	using FileUtils = org.apache.commons.io.FileUtils;
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.Graphdb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Neo4Net.Graphdb.factory.module.edition.CommunityEditionModule;
	using Neo4Net.Graphdb.index;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StoreLockException = Neo4Net.Kernel.StoreLockException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using TransactionHeaderInformation = Neo4Net.Kernel.Impl.Api.TransactionHeaderInformation;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.rootCause;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.MyRelTypes.TEST;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BackupIT
	public class BackupIT
	{
		private bool InstanceFieldsInitialized = false;

		public BackupIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _testDir ).around( _pageCacheRule ).around( SuppressOutput.suppressAll() ).around(_random);
		}

		 private readonly TestDirectory _testDir = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly RandomRule _random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDir).around(pageCacheRule).around(org.neo4j.test.rule.SuppressOutput.suppressAll()).around(random);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public String recordFormatName;
		 public string RecordFormatName;

		 private File _serverStorePath;
		 private File _otherServerPath;
		 private File _backupDatabasePath;
		 private IList<ServerInterface> _servers;
		 private DatabaseLayout _backupDatabaseLayout;
		 private DatabaseLayout _serverStoreLayout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<String> recordFormatNames()
		 public static IList<string> RecordFormatNames()
		 {
			  return Arrays.asList( Standard.LATEST_NAME, HighLimit.NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _servers = new List<ServerInterface>();
			  _serverStoreLayout = _testDir.databaseLayout( "server" );
			  _serverStorePath = _serverStoreLayout.databaseDirectory();
			  _otherServerPath = _testDir.directory( "server2" );
			  _backupDatabaseLayout = DatabaseLayout.of( _testDir.storeDir( "backedup-serverdb" ), "backupDb" );
			  _backupDatabasePath = _backupDatabaseLayout.databaseDirectory();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutDownServers()
		 public virtual void ShutDownServers()
		 {
			  foreach ( ServerInterface server in _servers )
			  {
					server.Shutdown();
			  }
			  _servers.Clear();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureFullFailsWhenDbExists()
		 public virtual void MakeSureFullFailsWhenDbExists()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  CreateInitialDataSet( _serverStorePath );
			  ServerInterface server = StartServer( _serverStorePath, backupPort );
			  OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
			  CreateInitialDataSet( _backupDatabasePath );
			  try
			  {
					backup.Full( _backupDatabasePath.Path );
					fail( "Shouldn't be able to do full backup into existing db" );
			  }
			  catch ( Exception )
			  {
					// good
			  }
			  ShutdownServer( server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureIncrementalFailsWhenNoDb()
		 public virtual void MakeSureIncrementalFailsWhenNoDb()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  CreateInitialDataSet( _serverStorePath );
			  ServerInterface server = StartServer( _serverStorePath, backupPort );
			  OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
			  try
			  {
					backup.incremental( _backupDatabasePath.Path );
					fail( "Shouldn't be able to do incremental backup into non-existing db" );
			  }
			  catch ( Exception )
			  {
					// Good
			  }
			  ShutdownServer( server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backedUpDatabaseContainsChecksumOfLastTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackedUpDatabaseContainsChecksumOfLastTx()
		 {
			  ServerInterface server = null;
			  try
			  {
					CreateInitialDataSet( _serverStorePath );
					int backupPort = PortAuthority.allocatePort();
					server = StartServer( _serverStorePath, backupPort );
					OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
					backup.Full( _backupDatabasePath.Path );
					assertTrue( "Should be consistent", backup.Consistent );
					ShutdownServer( server );
					server = null;
					PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );

					long firstChecksum = LastTxChecksumOf( _serverStoreLayout, pageCache );
					assertEquals( firstChecksum, LastTxChecksumOf( _backupDatabaseLayout, pageCache ) );

					AddMoreData( _serverStorePath );
					server = StartServer( _serverStorePath, backupPort );
					backup.incremental( _backupDatabasePath.Path );
					assertTrue( "Should be consistent", backup.Consistent );
					ShutdownServer( server );
					server = null;

					long secondChecksum = LastTxChecksumOf( _serverStoreLayout, pageCache );
					assertEquals( secondChecksum, LastTxChecksumOf( _backupDatabaseLayout, pageCache ) );
					assertTrue( firstChecksum != secondChecksum );
			  }
			  finally
			  {
					if ( server != null )
					{
						 ShutdownServer( server );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullThenIncremental()
		 public virtual void FullThenIncremental()
		 {
			  DbRepresentation initialDataSetRepresentation = CreateInitialDataSet( _serverStorePath );
			  int backupPort = PortAuthority.allocatePort();
			  ServerInterface server = StartServer( _serverStorePath, backupPort );

			  OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
			  backup.Full( _backupDatabasePath.Path );
			  assertTrue( "Should be consistent", backup.Consistent );
			  assertEquals( initialDataSetRepresentation, DbRepresentation );
			  ShutdownServer( server );

			  DbRepresentation furtherRepresentation = AddMoreData( _serverStorePath );
			  server = StartServer( _serverStorePath, backupPort );
			  backup.incremental( _backupDatabasePath.Path );
			  assertTrue( "Should be consistent", backup.Consistent );
			  assertEquals( furtherRepresentation, DbRepresentation );
			  ShutdownServer( server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureNoLogFileRemains()
		 public virtual void MakeSureNoLogFileRemains()
		 {
			  CreateInitialDataSet( _serverStorePath );
			  int backupPort = PortAuthority.allocatePort();
			  ServerInterface server = StartServer( _serverStorePath, backupPort );
			  OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );

			  // First check full
			  backup.Full( _backupDatabasePath.Path );
			  assertTrue( "Should be consistent", backup.Consistent );
			  assertFalse( CheckLogFileExistence( _backupDatabasePath.Path ) );
			  // Then check empty incremental
			  backup.incremental( _backupDatabasePath.Path );
			  assertTrue( "Should be consistent", backup.Consistent );
			  assertFalse( CheckLogFileExistence( _backupDatabasePath.Path ) );
			  // Then check real incremental
			  ShutdownServer( server );
			  AddMoreData( _serverStorePath );
			  server = StartServer( _serverStorePath, backupPort );
			  backup.incremental( _backupDatabasePath.Path );
			  assertTrue( "Should be consistent", backup.Consistent );
			  assertFalse( CheckLogFileExistence( _backupDatabasePath.Path ) );
			  ShutdownServer( server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureStoreIdIsEnforced()
		 public virtual void MakeSureStoreIdIsEnforced()
		 {
			  // Create data set X on server A
			  DbRepresentation initialDataSetRepresentation = CreateInitialDataSet( _serverStorePath );
			  int backupPort = PortAuthority.allocatePort();
			  ServerInterface server = StartServer( _serverStorePath, backupPort );

			  // Grab initial backup from server A
			  OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
			  backup.Full( _backupDatabasePath.Path );
			  assertTrue( "Should be consistent", backup.Consistent );
			  assertEquals( initialDataSetRepresentation, DbRepresentation );
			  ShutdownServer( server );

			  // Create data set X+Y on server B
			  CreateInitialDataSet( _otherServerPath );
			  AddMoreData( _otherServerPath );
			  server = StartServer( _otherServerPath, backupPort );

			  // Try to grab incremental backup from server B.
			  // Data should be OK, but store id check should prevent that.
			  try
			  {
					backup.incremental( _backupDatabasePath.Path );
					fail( "Shouldn't work" );
			  }
			  catch ( Exception e )
			  {
					assertThat( rootCause( e ), instanceOf( typeof( MismatchingStoreIdException ) ) );
			  }
			  ShutdownServer( server );
			  // Just make sure incremental backup can be received properly from
			  // server A, even after a failed attempt from server B
			  DbRepresentation furtherRepresentation = AddMoreData( _serverStorePath );
			  server = StartServer( _serverStorePath, backupPort );
			  backup.incremental( _backupDatabasePath.Path );
			  assertTrue( "Should be consistent", backup.Consistent );
			  assertEquals( furtherRepresentation, DbRepresentation );
			  ShutdownServer( server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIncrementals() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIncrementals()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = null;
			  try
			  {
					db = GetEmbeddedTestDataBaseService( backupPort );

					Index<Node> index;
					using ( Transaction tx = Db.beginTx() )
					{
						 index = Db.index().forNodes("yo");
						 index.Add( Db.createNode(), "justTo", "commitATx" );
						 Db.createNode();
						 tx.Success();
					}

					OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
					backup.Full( _backupDatabasePath.Path );
					assertTrue( "Should be consistent", backup.Consistent );
					PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
					long lastCommittedTx = GetLastCommittedTx( _backupDatabaseLayout, pageCache );

					for ( int i = 0; i < 5; i++ )
					{
						 using ( Transaction tx = Db.beginTx() )
						 {
							  Node node = Db.createNode();
							  index.Add( node, "key", "value" + i );
							  tx.Success();
						 }
						 backup = backup.incremental( _backupDatabasePath.Path );
						 assertTrue( "Should be consistent", backup.Consistent );
						 assertEquals( lastCommittedTx + i + 1, GetLastCommittedTx( _backupDatabaseLayout, pageCache ) );
					}
			  }
			  finally
			  {
					if ( db != null )
					{
						 Db.shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupIndexWithNoCommits()
		 public virtual void BackupIndexWithNoCommits()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = null;
			  try
			  {
					db = GetEmbeddedTestDataBaseService( backupPort );

					using ( Transaction transaction = Db.beginTx() )
					{
						 Db.index().forNodes("created-no-commits");
						 transaction.Success();
					}

					OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
					backup.Full( _backupDatabasePath.Path );
					assertTrue( "Should be consistent", backup.Consistent );
					assertTrue( backup.Consistent );
			  }
			  finally
			  {
					if ( db != null )
					{
						 Db.shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long getLastCommittedTx(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private static long GetLastCommittedTx( DatabaseLayout databaseLayout, PageCache pageCache )
		 {
			  File neoStore = databaseLayout.MetadataStore();
			  return MetaDataStore.getRecord( pageCache, neoStore, MetaDataStore.Position.LAST_TRANSACTION_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupEmptyIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupEmptyIndex()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  string key = "name";
			  string value = "Neo";
			  GraphDatabaseService db = GetEmbeddedTestDataBaseService( backupPort );

			  try
			  {
					Index<Node> index;
					Node node;
					using ( Transaction tx = Db.beginTx() )
					{
						 index = Db.index().forNodes(key);
						 node = Db.createNode();
						 node.SetProperty( key, value );
						 tx.Success();
					}
					OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort ).full( _backupDatabasePath.Path );
					assertTrue( "Should be consistent", backup.Consistent );
					assertEquals( DbRepresentation.of( db ), DbRepresentation );
					FileUtils.deleteDirectory( _backupDatabasePath );
					backup = OnlineBackup.From( "127.0.0.1", backupPort ).full( _backupDatabasePath.Path );
					assertTrue( "Should be consistent", backup.Consistent );
					assertEquals( DbRepresentation.of( db ), DbRepresentation );

					using ( Transaction tx = Db.beginTx() )
					{
						 index.Add( node, key, value );
						 tx.Success();
					}
					FileUtils.deleteDirectory( _backupDatabasePath );
					backup = OnlineBackup.From( "127.0.0.1", backupPort ).full( _backupDatabasePath.Path );
					assertTrue( "Should be consistent", backup.Consistent );
					assertEquals( DbRepresentation.of( db ), DbRepresentation );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupMultipleSchemaIndexes() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupMultipleSchemaIndexes()
		 {
			  // given
			  ExecutorService executorService = Executors.newSingleThreadExecutor();
			  AtomicBoolean end = new AtomicBoolean();
			  int backupPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = GetEmbeddedTestDataBaseService( backupPort );
			  try
			  {
					int numberOfIndexedLabels = 10;
					IList<Label> indexedLabels = CreateIndexes( db, numberOfIndexedLabels );

					// start thread that continuously writes to indexes
					executorService.submit(() =>
					{
					 while ( !end.get() )
					 {
						  using ( Transaction tx = Db.beginTx() )
						  {
								Db.createNode( indexedLabels[_random.Next( numberOfIndexedLabels )] ).setProperty( "prop", _random.nextValue() );
								tx.success();
						  }
					 }
					});
					executorService.shutdown();

					// create backup
					OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort ).full( _backupDatabasePath.Path );
					assertTrue( "Should be consistent", backup.Consistent );
					end.set( true );
					executorService.awaitTermination( 1, TimeUnit.MINUTES );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private static IList<Label> CreateIndexes( GraphDatabaseService db, int indexCount )
		 {
			  List<Label> indexedLabels = new List<Label>( indexCount );
			  for ( int i = 0; i < indexCount; i++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Label label = Label.label( "label" + i );
						 indexedLabels.Add( label );
						 Db.schema().indexFor(label).on("prop").create();
						 tx.Success();
					}
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
			  return indexedLabels;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetainFileLocksAfterFullBackupOnLiveDatabase()
		 public virtual void ShouldRetainFileLocksAfterFullBackupOnLiveDatabase()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  File sourcePath = _testDir.directory( "serverdb-lock" );

			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(sourcePath).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.TRUE).setConfig(OnlineBackupSettings.online_backup_server, "127.0.0.1:" + backupPort).setConfig(GraphDatabaseSettings.record_format, RecordFormatName).newGraphDatabase();
			  try
			  {
					AssertStoreIsLocked( sourcePath );
					OnlineBackup.From( "127.0.0.1", backupPort ).full( _backupDatabasePath.Path );
					AssertStoreIsLocked( sourcePath );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncrementallyBackupDenseNodes()
		 public virtual void ShouldIncrementallyBackupDenseNodes()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = StartGraphDatabase( _serverStorePath, true, backupPort );
			  try
			  {
					CreateInitialDataSet( db );

					OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
					backup.Full( _backupDatabasePath.Path );

					DbRepresentation representation = AddLotsOfData( db );
					backup.incremental( _backupDatabasePath.Path );
					assertEquals( representation, DbRepresentation );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLeaveIdFilesAfterBackup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLeaveIdFilesAfterBackup()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = StartGraphDatabase( _serverStorePath, true, backupPort );
			  try
			  {
					CreateInitialDataSet( db );

					OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
					backup.Full( _backupDatabasePath.Path );
					EnsureStoresHaveIdFiles( _backupDatabaseLayout );

					DbRepresentation representation = AddLotsOfData( db );
					backup.incremental( _backupDatabasePath.Path );
					assertEquals( representation, DbRepresentation );
					EnsureStoresHaveIdFiles( _backupDatabaseLayout );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupDatabaseWithCustomTransactionLogsLocation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupDatabaseWithCustomTransactionLogsLocation()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = StartGraphDatabase( _serverStorePath, true, backupPort, "customLogLocation" );
			  try
			  {
					CreateInitialDataSet( db );

					OnlineBackup backup = OnlineBackup.From( "127.0.0.1", backupPort );
					string backupStore = _backupDatabasePath.Path;
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( new File( backupStore ), _fileSystemRule.get() ).build();

					backup.Full( backupStore );
					assertThat( logFiles.LogFilesConflict(), Matchers.arrayWithSize(1) );

					DbRepresentation representation = AddLotsOfData( db );
					backup.Incremental( backupStore );
					assertThat( logFiles.LogFilesConflict(), Matchers.arrayWithSize(1) );

					assertEquals( representation, DbRepresentation );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureStoresHaveIdFiles(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private void EnsureStoresHaveIdFiles( DatabaseLayout databaseLayout )
		 {
			  foreach ( File idFile in databaseLayout.IdFiles() )
			  {
					assertTrue( "Missing id file " + idFile, idFile.exists() );
					assertTrue( "Id file " + idFile + " had 0 highId", IdGeneratorImpl.readHighId( _fileSystemRule.get(), idFile ) > 0 );
			  }
		 }

		 private static DbRepresentation AddLotsOfData( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					int threshold = parseInt( dense_node_threshold.DefaultValue );
					for ( int i = 0; i < threshold * 2; i++ )
					{
						 node.CreateRelationshipTo( Db.createNode(), TEST );
					}
					tx.Success();
			  }
			  return DbRepresentation.of( db );
		 }

		 private static void AssertStoreIsLocked( File path )
		 {
			  try
			  {
					( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(path).shutdown();
					fail( "Could start up database in same process, store not locked" );
			  }
			  catch ( Exception ex )
			  {
					assertThat( ex.InnerException.InnerException, instanceOf( typeof( StoreLockException ) ) );
			  }
		 }

		 private static bool CheckLogFileExistence( string directory )
		 {
			  return Config.defaults( logs_directory, directory ).get( store_internal_log_path ).exists();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long lastTxChecksumOf(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private static long LastTxChecksumOf( DatabaseLayout databaseLayout, PageCache pageCache )
		 {
			  File neoStore = databaseLayout.MetadataStore();
			  return MetaDataStore.getRecord( pageCache, neoStore, MetaDataStore.Position.LAST_TRANSACTION_CHECKSUM );
		 }

		 private ServerInterface StartServer( File path, int backupPort )
		 {
			  ServerInterface server = new EmbeddedServer( path, "127.0.0.1:" + backupPort );
			  server.AwaitStarted();
			  _servers.Add( server );
			  return server;
		 }

		 private void ShutdownServer( ServerInterface server )
		 {
			  server.Shutdown();
			  _servers.Remove( server );
		 }

		 private DbRepresentation AddMoreData( File path )
		 {
			  GraphDatabaseService db = StartGraphDatabase( path, false, null );
			  DbRepresentation representation;
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Node node = Db.createNode();
						node.SetProperty( "backup", "Is great" );
						Db.createNode().createRelationshipTo(node, RelationshipType.withName("LOVES"));
						tx.Success();
					  }
			  }
			  finally
			  {
					representation = DbRepresentation.of( db );
					Db.shutdown();
			  }
			  return representation;
		 }

		 private GraphDatabaseService StartGraphDatabase( File storeDir, bool withOnlineBackup, int? backupPort )
		 {
			  return StartGraphDatabase( storeDir, withOnlineBackup, backupPort, "" );
		 }

		 private GraphDatabaseService StartGraphDatabase( File storeDir, bool withOnlineBackup, int? backupPort, string logLocation )
		 {
			  GraphDatabaseFactory dbFactory = new TestGraphDatabaseFactoryAnonymousInnerClass( this, storeDir );
			  GraphDatabaseBuilder graphDatabaseBuilder = dbFactory.NewEmbeddedDatabaseBuilder( storeDir ).setConfig( OnlineBackupSettings.online_backup_enabled, withOnlineBackup.ToString() ).setConfig(GraphDatabaseSettings.keep_logical_logs, Settings.TRUE).setConfig(GraphDatabaseSettings.record_format, RecordFormatName).setConfig(GraphDatabaseSettings.logical_logs_location, logLocation);

			  if ( backupPort != null )
			  {
					graphDatabaseBuilder.SetConfig( OnlineBackupSettings.online_backup_server, "127.0.0.1:" + backupPort );
			  }

			  return graphDatabaseBuilder.NewGraphDatabase();
		 }

		 private class TestGraphDatabaseFactoryAnonymousInnerClass : TestGraphDatabaseFactory
		 {
			 private readonly BackupIT _outerInstance;

			 private File _storeDir;

			 public TestGraphDatabaseFactoryAnonymousInnerClass( BackupIT outerInstance, File storeDir )
			 {
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
			 }

			 protected internal override GraphDatabaseService newDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
			 {
				  System.Func<PlatformModule, AbstractEditionModule> factory = platformModule => new CommunityEditionModuleAnonymousInnerClass( this, platformModule );
				  return ( new GraphDatabaseFacadeFactory( DatabaseInfo.COMMUNITY, factory ) ).newFacade( storeDir, config, dependencies );
			 }

			 private class CommunityEditionModuleAnonymousInnerClass : CommunityEditionModule
			 {
				 private readonly TestGraphDatabaseFactoryAnonymousInnerClass _outerInstance;

				 public CommunityEditionModuleAnonymousInnerClass( TestGraphDatabaseFactoryAnonymousInnerClass outerInstance, UnknownType platformModule ) : base( platformModule )
				 {
					 this.outerInstance = outerInstance;
				 }


				 protected internal override TransactionHeaderInformationFactory createHeaderInformationFactory()
				 {
					  return new TransactionHeaderInformationFactory_WithRandomBytesAnonymousInnerClass( this );
				 }

				 private class TransactionHeaderInformationFactory_WithRandomBytesAnonymousInnerClass : Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory_WithRandomBytes
				 {
					 private readonly CommunityEditionModuleAnonymousInnerClass _outerInstance;

					 public TransactionHeaderInformationFactory_WithRandomBytesAnonymousInnerClass( CommunityEditionModuleAnonymousInnerClass outerInstance )
					 {
						 this.outerInstance = outerInstance;
					 }

					 protected internal override TransactionHeaderInformation createUsing( sbyte[] additionalHeader )
					 {
						  return new TransactionHeaderInformation( 1, 2, additionalHeader );
					 }
				 }
			 }
		 }

		 private DbRepresentation CreateInitialDataSet( File path )
		 {
			  GraphDatabaseService db = StartGraphDatabase( path, false, null );
			  try
			  {
					CreateInitialDataSet( db );
					return DbRepresentation.of( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private static void CreateInitialDataSet( GraphDatabaseService db )
		 {
			  // 4 transactions: THE transaction, "mykey" property key, "db-index" index, "KNOWS" rel type.
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( Label.label( "Me" ) );
					node.SetProperty( "myKey", "myValue" );
					Index<Node> nodeIndex = Db.index().forNodes("db-index");
					nodeIndex.Add( node, "myKey", "myValue" );
					Db.createNode().createRelationshipTo(node, RelationshipType.withName("KNOWS"));
					tx.Success();
			  }
		 }

		 private GraphDatabaseService GetEmbeddedTestDataBaseService( int backupPort )
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_serverStorePath).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.TRUE).setConfig(OnlineBackupSettings.online_backup_server, "127.0.0.1:" + backupPort).setConfig(GraphDatabaseSettings.record_format, RecordFormatName).newGraphDatabase();
		 }

		 private DbRepresentation DbRepresentation
		 {
			 get
			 {
				  Config config = Config.builder().withSetting(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).withSetting(GraphDatabaseSettings.active_database, _backupDatabasePath.Name).build();
				  return DbRepresentation.of( _backupDatabasePath, config );
			 }
		 }
	}

}