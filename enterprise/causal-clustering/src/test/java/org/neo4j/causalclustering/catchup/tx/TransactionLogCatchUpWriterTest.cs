﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.catchup.tx
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordStorageCommandReaderFactory = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using Commands = Org.Neo4j.Kernel.impl.transaction.command.Commands;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using LogVersionBridge = Org.Neo4j.Kernel.impl.transaction.log.LogVersionBridge;
	using LogVersionedStoreChannel = Org.Neo4j.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using Org.Neo4j.Kernel.impl.transaction.log;
	using ReadAheadLogChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using ReadableLogChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableLogChannel;
	using InvalidLogEntryHandler = Org.Neo4j.Kernel.impl.transaction.log.entry.InvalidLogEntryHandler;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogTailScanner = Org.Neo4j.Kernel.recovery.LogTailScanner;
	using LogTailInformation = Org.Neo4j.Kernel.recovery.LogTailScanner.LogTailInformation;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using NeoStoreDataSourceRule = Org.Neo4j.Test.rule.NeoStoreDataSourceRule;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

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
//	import static org.neo4j.kernel.impl.transaction.command.Commands.createNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TransactionLogCatchUpWriterTest
	public class TransactionLogCatchUpWriterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Dir = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fsRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FsRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.NeoStoreDataSourceRule dsRule = new org.neo4j.test.rule.NeoStoreDataSourceRule();
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
			  Org.Neo4j.Storageengine.Api.StoreId storeId = SimulateStoreCopy();

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
			  Org.Neo4j.Storageengine.Api.StoreId storeId = SimulateStoreCopy();

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
//ORIGINAL LINE: private void createTransactionLogWithCheckpoint(org.neo4j.kernel.configuration.Config config, boolean logsInStoreDir) throws java.io.IOException
		 private void CreateTransactionLogWithCheckpoint( Config config, bool logsInStoreDir )
		 {
			  Org.Neo4j.Storageengine.Api.StoreId storeId = SimulateStoreCopy();

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
//ORIGINAL LINE: final org.neo4j.kernel.recovery.LogTailScanner logTailScanner = new org.neo4j.kernel.recovery.LogTailScanner(logFiles, logEntryReader, new org.neo4j.kernel.monitoring.Monitors());
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
//ORIGINAL LINE: private void verifyTransactionsInLog(org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles, long fromTxId, long endTxId) throws java.io.IOException
		 private void VerifyTransactionsInLog( LogFiles logFiles, long fromTxId, long endTxId )
		 {
			  long expectedTxId = fromTxId;
			  LogVersionedStoreChannel versionedStoreChannel = logFiles.OpenForVersion( 0 );
			  using ( ReadableLogChannel channel = new ReadAheadLogChannel( versionedStoreChannel, Org.Neo4j.Kernel.impl.transaction.log.LogVersionBridge_Fields.NoMoreChannels, 1024 ) )
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
//ORIGINAL LINE: private org.neo4j.storageengine.api.StoreId simulateStoreCopy() throws java.io.IOException
		 private Org.Neo4j.Storageengine.Api.StoreId SimulateStoreCopy()
		 {
			  // create an empty store
			  Org.Neo4j.Storageengine.Api.StoreId storeId;
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

		 private StoreId ToCasualStoreId( Org.Neo4j.Storageengine.Api.StoreId storeId )
		 {
			  return new StoreId( storeId.CreationTime, storeId.RandomId, storeId.UpgradeTime, storeId.UpgradeId );
		 }

		 private static CommittedTransactionRepresentation Tx( long txId )
		 {
			  return new CommittedTransactionRepresentation( new LogEntryStart( 0, 0, 0, txId - 1, new sbyte[]{}, LogPosition.UNSPECIFIED ), Commands.transactionRepresentation( createNode( 0 ) ), new LogEntryCommit( txId, 0 ) );
		 }
	}

}