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
namespace Neo4Net.causalclustering.catchup.tx
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordStorageCommandReaderFactory = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using Commands = Neo4Net.Kernel.impl.transaction.command.Commands;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogVersionBridge = Neo4Net.Kernel.impl.transaction.log.LogVersionBridge;
	using LogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using Neo4Net.Kernel.impl.transaction.log;
	using ReadAheadLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using ReadableLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadableLogChannel;
	using InvalidLogEntryHandler = Neo4Net.Kernel.impl.transaction.log.entry.InvalidLogEntryHandler;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using LogTailInformation = Neo4Net.Kernel.recovery.LogTailScanner.LogTailInformation;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using NeoStoreDataSourceRule = Neo4Net.Test.rule.NeoStoreDataSourceRule;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.command.Commands.createNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TransactionLogCatchUpWriterTest
	public class TransactionLogCatchUpWriterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory dir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Dir = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.DefaultFileSystemRule fsRule = new org.Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FsRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.PageCacheRule pageCacheRule = new org.Neo4Net.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.NeoStoreDataSourceRule dsRule = new org.Neo4Net.test.rule.NeoStoreDataSourceRule();
		 public NeoStoreDataSourceRule DsRule = new NeoStoreDataSourceRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public boolean partOfStoreCopy;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public bool PartOfStoreCopyConflict;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "Part of store copy: {0}") public static java.util.List<bool> partOfStoreCopy()
		 public static IList<bool> PartOfStoreCopy()
		 {
			  return Arrays.asList( true, false );
		 }

		 private readonly int _manyTransactions = 100_000; // should be somewhere above the rotation threshold

		 private PageCache _pageCache;
		 private FileSystemAbstraction _fs;
		 private DatabaseLayout _databaseLayout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _databaseLayout = Dir.databaseLayout();
			  _fs = FsRule.get();
			  _pageCache = PageCacheRule.getPageCache( _fs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateTransactionLogWithCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateTransactionLogWithCheckpoint()
		 {
			  CreateTransactionLogWithCheckpoint( Config.defaults(), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createTransactionLogWithCheckpointInCustomLocation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateTransactionLogWithCheckpointInCustomLocation()
		 {
			  CreateTransactionLogWithCheckpoint( Config.defaults( GraphDatabaseSettings.logical_logs_location, "custom-tx-logs" ), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pullRotatesWhenThresholdCrossedAndExplicitlySet() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PullRotatesWhenThresholdCrossedAndExplicitlySet()
		 {
			  // given
			  Config config = Config.defaults();
			  config.Augment( GraphDatabaseSettings.logical_log_rotation_threshold, "1M" ); // 1 mebibyte

			  // and
			  Neo4Net.Kernel.Api.StorageEngine.StoreId storeId = SimulateStoreCopy();

			  // and
			  long fromTxId = BASE_TX_ID;
			  TransactionLogCatchUpWriter subject = new TransactionLogCatchUpWriter( _databaseLayout, _fs, _pageCache, config, NullLogProvider.Instance, fromTxId, PartOfStoreCopyConflict, false, true );

			  // when a bunch of transactions received
			  LongStream.range( fromTxId, _manyTransactions ).mapToObj( TransactionLogCatchUpWriterTest.tx ).map( tx => new TxPullResponse( ToCasualStoreId( storeId ), tx ) ).forEach( subject.onTxReceived );
			  subject.Close();

			  // then there was a rotation
			  LogFilesBuilder logFilesBuilder = LogFilesBuilder.activeFilesBuilder( _databaseLayout, _fs, _pageCache );
			  LogFiles logFiles = logFilesBuilder.Build();
			  assertNotEquals( logFiles.LowestLogVersion, logFiles.HighestLogVersion );
			  VerifyTransactionsInLog( logFiles, fromTxId, _manyTransactions );
			  VerifyCheckpointInLog( logFiles, PartOfStoreCopyConflict );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pullDoesNotRotateWhenThresholdCrossedAndExplicitlyOff() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PullDoesNotRotateWhenThresholdCrossedAndExplicitlyOff()
		 {
			  // given
			  Config config = Config.defaults();
			  config.Augment( GraphDatabaseSettings.logical_log_rotation_threshold, "1M" ); // 1 mebibyte

			  // and
			  Neo4Net.Kernel.Api.StorageEngine.StoreId storeId = SimulateStoreCopy();

			  // and
			  long fromTxId = BASE_TX_ID;
			  TransactionLogCatchUpWriter subject = new TransactionLogCatchUpWriter( _databaseLayout, _fs, _pageCache, config, NullLogProvider.Instance, fromTxId, PartOfStoreCopyConflict, false, false );

			  // when 1M tx received
			  LongStream.range( fromTxId, _manyTransactions ).mapToObj( TransactionLogCatchUpWriterTest.tx ).map( tx => new TxPullResponse( ToCasualStoreId( storeId ), tx ) ).forEach( subject.onTxReceived );
			  subject.Close();

			  // then there was a rotation
			  LogFilesBuilder logFilesBuilder = LogFilesBuilder.activeFilesBuilder( _databaseLayout, _fs, _pageCache );
			  LogFiles logFiles = logFilesBuilder.Build();
			  assertEquals( logFiles.LowestLogVersion, logFiles.HighestLogVersion );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createTransactionLogWithCheckpoint(org.Neo4Net.kernel.configuration.Config config, boolean logsInStoreDir) throws java.io.IOException
		 private void CreateTransactionLogWithCheckpoint( Config config, bool logsInStoreDir )
		 {
			  Neo4Net.Kernel.Api.StorageEngine.StoreId storeId = SimulateStoreCopy();

			  int fromTxId = 37;
			  int endTxId = fromTxId + 5;

			  TransactionLogCatchUpWriter catchUpWriter = new TransactionLogCatchUpWriter( _databaseLayout, _fs, _pageCache, config, NullLogProvider.Instance, fromTxId, PartOfStoreCopyConflict, logsInStoreDir, true );

			  // when
			  for ( int i = fromTxId; i <= endTxId; i++ )
			  {
					catchUpWriter.OnTxReceived( new TxPullResponse( ToCasualStoreId( storeId ), Tx( i ) ) );
			  }

			  catchUpWriter.Close();

			  // then
			  LogFilesBuilder logFilesBuilder = LogFilesBuilder.activeFilesBuilder( _databaseLayout, _fs, _pageCache );
			  if ( !logsInStoreDir )
			  {
					logFilesBuilder.WithConfig( config );
			  }
			  LogFiles logFiles = logFilesBuilder.Build();

			  VerifyTransactionsInLog( logFiles, fromTxId, endTxId );
			  VerifyCheckpointInLog( logFiles, PartOfStoreCopyConflict );
		 }

		 private void VerifyCheckpointInLog( LogFiles logFiles, bool shouldExist )
		 {
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>( new RecordStorageCommandReaderFactory(), InvalidLogEntryHandler.STRICT );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.recovery.LogTailScanner logTailScanner = new org.Neo4Net.kernel.recovery.LogTailScanner(logFiles, logEntryReader, new org.Neo4Net.kernel.monitoring.Monitors());
			  LogTailScanner logTailScanner = new LogTailScanner( logFiles, logEntryReader, new Monitors() );

			  LogTailScanner.LogTailInformation tailInformation = logTailScanner.TailInformation;

			  if ( !shouldExist )
			  {
					assertNull( tailInformation.LastCheckPoint );
					return;
			  }

			  assertNotNull( tailInformation.LastCheckPoint );
			  assertEquals( 0, tailInformation.LastCheckPoint.LogPosition.LogVersion );
			  assertEquals( LOG_HEADER_SIZE, tailInformation.LastCheckPoint.LogPosition.ByteOffset );
			  assertTrue( tailInformation.CommitsAfterLastCheckpoint() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyTransactionsInLog(org.Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles, long fromTxId, long endTxId) throws java.io.IOException
		 private void VerifyTransactionsInLog( LogFiles logFiles, long fromTxId, long endTxId )
		 {
			  long expectedTxId = fromTxId;
			  LogVersionedStoreChannel versionedStoreChannel = logFiles.OpenForVersion( 0 );
			  using ( ReadableLogChannel channel = new ReadAheadLogChannel( versionedStoreChannel, Neo4Net.Kernel.impl.transaction.log.LogVersionBridge_Fields.NoMoreChannels, 1024 ) )
			  {
					using ( PhysicalTransactionCursor<ReadableLogChannel> txCursor = new PhysicalTransactionCursor<ReadableLogChannel>( channel, new VersionAwareLogEntryReader<>() ) )
					{
						 while ( txCursor.Next() )
						 {
							  CommittedTransactionRepresentation tx = txCursor.Get();
							  long txId = tx.CommitEntry.TxId;

							  assertThat( expectedTxId, lessThanOrEqualTo( endTxId ) );
							  assertEquals( expectedTxId, txId );
							  expectedTxId++;
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.Kernel.Api.StorageEngine.StoreId simulateStoreCopy() throws java.io.IOException
		 private Neo4Net.Kernel.Api.StorageEngine.StoreId SimulateStoreCopy()
		 {
			  // create an empty store
			  Neo4Net.Kernel.Api.StorageEngine.StoreId storeId;
			  NeoStoreDataSource ds = DsRule.getDataSource( _databaseLayout, _fs, _pageCache );
			  using ( Lifespan ignored = new Lifespan( ds ) )
			  {
					storeId = ds.StoreId;
			  }

			  // we don't have log files after a store copy
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _databaseLayout.databaseDirectory(), FsRule.get() ).build();
			  //noinspection ResultOfMethodCallIgnored
			  logFiles.Accept( ( file, version ) => file.delete() );

			  return storeId;
		 }

		 private StoreId ToCasualStoreId( Neo4Net.Kernel.Api.StorageEngine.StoreId storeId )
		 {
			  return new StoreId( storeId.CreationTime, storeId.RandomId, storeId.UpgradeTime, storeId.UpgradeId );
		 }

		 private static CommittedTransactionRepresentation Tx( long txId )
		 {
			  return new CommittedTransactionRepresentation( new LogEntryStart( 0, 0, 0, txId - 1, new sbyte[]{}, LogPosition.UNSPECIFIED ), Commands.transactionRepresentation( createNode( 0 ) ), new LogEntryCommit( txId, 0 ) );
		 }
	}

}