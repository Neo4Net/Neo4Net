/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.impl.storemigration.participant
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using TransactionId = Org.Neo4j.Kernel.impl.store.TransactionId;
	using MetaDataRecordFormat = Org.Neo4j.Kernel.impl.store.format.standard.MetaDataRecordFormat;
	using StandardV3_0 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_0;
	using StandardV3_2 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_2;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logical_logs_location;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_CHECKSUM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.getRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.setRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.MetaDataRecordFormat.FIELD_NOT_PRESENT;

	public class StoreMigratorTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreMigratorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _pageCacheRule ).around( _random );
		}

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly RandomRule _random = new RandomRule();
		 private PageCache _pageCache;
		 private JobScheduler _jobScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(pageCacheRule).around(random);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _jobScheduler = new ThreadPoolJobScheduler();
			  _pageCache = _pageCacheRule.getPageCache( _fileSystemRule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _jobScheduler.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractTransactionInformationFromMetaDataStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractTransactionInformationFromMetaDataStore()
		 {
			  // given
			  // ... variables
			  long txId = 42;
			  long checksum = 123456789123456789L;
			  long timestamp = 919191919191919191L;
			  TransactionId expected = new TransactionId( txId, checksum, timestamp );

			  // ... and files
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File neoStore = databaseLayout.MetadataStore();
			  neoStore.createNewFile();

			  // ... and mocks
			  Config config = mock( typeof( Config ) );
			  LogService logService = mock( typeof( LogService ) );

			  // when
			  // ... data in record
			  setRecord( _pageCache, neoStore, LAST_TRANSACTION_ID, txId );
			  setRecord( _pageCache, neoStore, LAST_TRANSACTION_CHECKSUM, checksum );
			  setRecord( _pageCache, neoStore, LAST_TRANSACTION_COMMIT_TIMESTAMP, timestamp );

			  // ... and with migrator
			  StoreMigrator migrator = new StoreMigrator( _fileSystemRule.get(), _pageCache, config, logService, _jobScheduler );
			  TransactionId actual = migrator.ExtractTransactionIdInformation( neoStore, txId );

			  // then
			  assertEquals( expected, actual );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateTransactionInformationWhenLogsNotPresent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateTransactionInformationWhenLogsNotPresent()
		 {
			  // given
			  long txId = 42;
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File neoStore = databaseLayout.MetadataStore();
			  neoStore.createNewFile();
			  Config config = mock( typeof( Config ) );
			  LogService logService = new SimpleLogService( NullLogProvider.Instance, NullLogProvider.Instance );

			  // when
			  // ... transaction info not in neo store
			  assertEquals( FIELD_NOT_PRESENT, getRecord( _pageCache, neoStore, LAST_TRANSACTION_ID ) );
			  assertEquals( FIELD_NOT_PRESENT, getRecord( _pageCache, neoStore, LAST_TRANSACTION_CHECKSUM ) );
			  assertEquals( FIELD_NOT_PRESENT, getRecord( _pageCache, neoStore, LAST_TRANSACTION_COMMIT_TIMESTAMP ) );
			  // ... and with migrator
			  StoreMigrator migrator = new StoreMigrator( _fileSystemRule.get(), _pageCache, config, logService, _jobScheduler );
			  TransactionId actual = migrator.ExtractTransactionIdInformation( neoStore, txId );

			  // then
			  assertEquals( txId, actual.TransactionIdConflict() );
			  assertEquals( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_CHECKSUM, actual.Checksum() );
			  assertEquals( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP, actual.CommitTimestamp() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extractTransactionInformationFromLogsInCustomRelativeLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExtractTransactionInformationFromLogsInCustomRelativeLocation()
		 {
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File customLogLocation = databaseLayout.File( "customLogLocation" );
			  ExtractTransactionalInformationFromLogs( customLogLocation.Name, customLogLocation, databaseLayout, _directory.databaseDir() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extractTransactionInformationFromLogsInCustomAbsoluteLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExtractTransactionInformationFromLogsInCustomAbsoluteLocation()
		 {
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File customLogLocation = databaseLayout.File( "customLogLocation" );
			  ExtractTransactionalInformationFromLogs( customLogLocation.AbsolutePath, customLogLocation, databaseLayout, _directory.databaseDir() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void extractTransactionalInformationFromLogs(String path, java.io.File customLogLocation, org.neo4j.io.layout.DatabaseLayout databaseLayout, java.io.File storeDir) throws java.io.IOException
		 private void ExtractTransactionalInformationFromLogs( string path, File customLogLocation, DatabaseLayout databaseLayout, File storeDir )
		 {
			  LogService logService = new SimpleLogService( NullLogProvider.Instance, NullLogProvider.Instance );
			  File neoStore = databaseLayout.MetadataStore();

			  GraphDatabaseService database = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(logical_logs_location, path).newGraphDatabase();
			  for ( int i = 0; i < 10; i++ )
			  {
					using ( Transaction transaction = database.BeginTx() )
					{
						 Node node = database.CreateNode();
						 transaction.Success();
					}
			  }
			  database.Shutdown();

			  MetaDataStore.setRecord( _pageCache, neoStore, MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_VERSION, MetaDataRecordFormat.FIELD_NOT_PRESENT );
			  Config config = Config.defaults( logical_logs_location, path );
			  StoreMigrator migrator = new StoreMigrator( _fileSystemRule.get(), _pageCache, config, logService, _jobScheduler );
			  LogPosition logPosition = migrator.ExtractTransactionLogPosition( neoStore, databaseLayout, 100 );

			  File[] logFiles = customLogLocation.listFiles();
			  assertNotNull( logFiles );
			  assertEquals( 0, logPosition.LogVersion );
			  assertEquals( logFiles[0].length(), logPosition.ByteOffset );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateTransactionInformationWhenLogsAreEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateTransactionInformationWhenLogsAreEmpty()
		 {
			  // given
			  long txId = 1;
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File neoStore = databaseLayout.MetadataStore();
			  neoStore.createNewFile();
			  Config config = mock( typeof( Config ) );
			  LogService logService = new SimpleLogService( NullLogProvider.Instance, NullLogProvider.Instance );

			  // when
			  // ... transaction info not in neo store
			  assertEquals( FIELD_NOT_PRESENT, getRecord( _pageCache, neoStore, LAST_TRANSACTION_ID ) );
			  assertEquals( FIELD_NOT_PRESENT, getRecord( _pageCache, neoStore, LAST_TRANSACTION_CHECKSUM ) );
			  assertEquals( FIELD_NOT_PRESENT, getRecord( _pageCache, neoStore, LAST_TRANSACTION_COMMIT_TIMESTAMP ) );
			  // ... and with migrator
			  StoreMigrator migrator = new StoreMigrator( _fileSystemRule.get(), _pageCache, config, logService, _jobScheduler );
			  TransactionId actual = migrator.ExtractTransactionIdInformation( neoStore, txId );

			  // then
			  assertEquals( txId, actual.TransactionIdConflict() );
			  assertEquals( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_CHECKSUM, actual.Checksum() );
			  assertEquals( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP, actual.CommitTimestamp() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeAndReadLastTxInformation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WriteAndReadLastTxInformation()
		 {
			  StoreMigrator migrator = NewStoreMigrator();
			  TransactionId writtenTxId = new TransactionId( _random.nextLong(), _random.nextLong(), _random.nextLong() );

			  migrator.WriteLastTxInformation( _directory.databaseLayout(), writtenTxId );

			  TransactionId readTxId = migrator.ReadLastTxInformation( _directory.databaseLayout() );

			  assertEquals( writtenTxId, readTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeAndReadLastTxLogPosition() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WriteAndReadLastTxLogPosition()
		 {
			  StoreMigrator migrator = NewStoreMigrator();
			  LogPosition writtenLogPosition = new LogPosition( _random.nextLong(), _random.nextLong() );

			  migrator.WriteLastTxLogPosition( _directory.databaseLayout(), writtenLogPosition );

			  LogPosition readLogPosition = migrator.ReadLastTxLogPosition( _directory.databaseLayout() );

			  assertEquals( writtenLogPosition, readLogPosition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMigrateFilesForVersionsWithSameCapability() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotMigrateFilesForVersionsWithSameCapability()
		 {
			  // Prepare migrator and file
			  StoreMigrator migrator = NewStoreMigrator();
			  DatabaseLayout dbLayout = _directory.databaseLayout();
			  File neoStore = dbLayout.MetadataStore();
			  neoStore.createNewFile();

			  // Monitor what happens
			  MyProgressReporter progressReporter = new MyProgressReporter();
			  // Migrate with two storeversions that have the same FORMAT capabilities
			  migrator.Migrate( dbLayout, _directory.databaseLayout( "migrationDir" ), progressReporter, StandardV3_0.STORE_VERSION, StandardV3_2.STORE_VERSION );

			  // Should not have started any migration
			  assertFalse( progressReporter.Started );
		 }

		 private StoreMigrator NewStoreMigrator()
		 {
			  return new StoreMigrator( _fileSystemRule, _pageCache, Config.defaults(), NullLogService.Instance, _jobScheduler );
		 }

		 private class MyProgressReporter : ProgressReporter
		 {
			  public bool Started;

			  public override void Start( long max )
			  {
					Started = true;
			  }

			  public override void Progress( long add )
			  {

			  }

			  public override void Completed()
			  {

			  }
		 }
	}

}