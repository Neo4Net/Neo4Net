using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.transaction.log
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using TransactionMetadata = Org.Neo4j.Kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFile = Org.Neo4j.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LogAppendEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent;
	using SilentProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.SilentProgressReporter;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using CorruptedLogsTruncator = Org.Neo4j.Kernel.recovery.CorruptedLogsTruncator;
	using Recovery = Org.Neo4j.Kernel.recovery.Recovery;
	using RecoveryApplier = Org.Neo4j.Kernel.recovery.RecoveryApplier;
	using RecoveryMonitor = Org.Neo4j.Kernel.recovery.RecoveryMonitor;
	using RecoveryService = Org.Neo4j.Kernel.recovery.RecoveryService;
	using RecoveryStartInformation = Org.Neo4j.Kernel.recovery.RecoveryStartInformation;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.rotation.LogRotation.NO_ROTATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IdOrderingQueue.BYPASS;

	public class PhysicalLogicalTransactionStoreTest
	{
		 private static readonly DatabaseHealth _databaseHealth = mock( typeof( DatabaseHealth ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Dir = TestDirectory.testDirectory();
		 private File _databaseDirectory;
		 private Monitors _monitors = new Monitors();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _databaseDirectory = Dir.databaseDir();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extractTransactionFromLogFilesSkippingLastLogFileWithoutHeader() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExtractTransactionFromLogFilesSkippingLastLogFileWithoutHeader()
		 {
			  TransactionIdStore transactionIdStore = new SimpleTransactionIdStore();
			  TransactionMetadataCache positionCache = new TransactionMetadataCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] additionalHeader = new byte[]{1, 2, 5};
			  sbyte[] additionalHeader = new sbyte[]{ 1, 2, 5 };
			  const int masterId = 2;
			  int authorId = 1;
			  const long timeStarted = 12345;
			  long latestCommittedTxWhenStarted = 4545;
			  long timeCommitted = timeStarted + 10;
			  LifeSupport life = new LifeSupport();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles = org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.builder(dir.databaseLayout(), fileSystemRule.get()).withTransactionIdStore(transactionIdStore).withLogVersionRepository(mock(LogVersionRepository.class)).build();
			  LogFiles logFiles = LogFilesBuilder.builder( Dir.databaseLayout(), FileSystemRule.get() ).withTransactionIdStore(transactionIdStore).withLogVersionRepository(mock(typeof(LogVersionRepository))).build();
			  life.Add( logFiles );
			  life.Start();
			  try
			  {
					AddATransactionAndRewind( life, logFiles, positionCache, transactionIdStore, additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted );
			  }
			  finally
			  {
					life.Shutdown();
			  }

			  // create empty transaction log file and clear transaction cache to force re-read
			  FileSystemRule.get().create(logFiles.GetLogFileForVersion(logFiles.HighestLogVersion + 1)).close();
			  positionCache.Clear();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogicalTransactionStore store = new PhysicalLogicalTransactionStore(logFiles, positionCache, new org.neo4j.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>(), monitors, true);
			  LogicalTransactionStore store = new PhysicalLogicalTransactionStore( logFiles, positionCache, new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>(), _monitors, true );
			  VerifyTransaction( transactionIdStore, positionCache, additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted, store );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOpenCleanStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOpenCleanStore()
		 {
			  // GIVEN
			  TransactionIdStore transactionIdStore = new SimpleTransactionIdStore();
			  TransactionMetadataCache positionCache = new TransactionMetadataCache();

			  LifeSupport life = new LifeSupport();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles = org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.builder(dir.databaseLayout(), fileSystemRule.get()).withTransactionIdStore(transactionIdStore).withLogVersionRepository(mock(LogVersionRepository.class)).build();
			  LogFiles logFiles = LogFilesBuilder.builder( Dir.databaseLayout(), FileSystemRule.get() ).withTransactionIdStore(transactionIdStore).withLogVersionRepository(mock(typeof(LogVersionRepository))).build();
			  life.Add( logFiles );

			  life.Add( new BatchingTransactionAppender( logFiles, NO_ROTATION, positionCache, transactionIdStore, BYPASS, _databaseHealth ) );

			  try
			  {
					// WHEN
					life.Start();
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOpenAndRecoverExistingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOpenAndRecoverExistingData()
		 {
			  // GIVEN
			  TransactionIdStore transactionIdStore = new SimpleTransactionIdStore();
			  TransactionMetadataCache positionCache = new TransactionMetadataCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] additionalHeader = new byte[]{1, 2, 5};
			  sbyte[] additionalHeader = new sbyte[]{ 1, 2, 5 };
			  const int masterId = 2;
			  int authorId = 1;
			  const long timeStarted = 12345;
			  long latestCommittedTxWhenStarted = 4545;
			  long timeCommitted = timeStarted + 10;
			  LifeSupport life = new LifeSupport();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles = org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.builder(dir.databaseLayout(), fileSystemRule.get()).withTransactionIdStore(transactionIdStore).withLogVersionRepository(mock(LogVersionRepository.class)).build();
			  LogFiles logFiles = LogFilesBuilder.builder( Dir.databaseLayout(), FileSystemRule.get() ).withTransactionIdStore(transactionIdStore).withLogVersionRepository(mock(typeof(LogVersionRepository))).build();

			  life.Start();
			  life.Add( logFiles );
			  try
			  {
					AddATransactionAndRewind( life, logFiles, positionCache, transactionIdStore, additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted );
			  }
			  finally
			  {
					life.Shutdown();
			  }

			  life = new LifeSupport();
			  life.Add( logFiles );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean recoveryRequired = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean recoveryRequired = new AtomicBoolean();
			  FakeRecoveryVisitor visitor = new FakeRecoveryVisitor( additionalHeader, masterId, authorId, timeStarted, timeCommitted, latestCommittedTxWhenStarted );

			  LogicalTransactionStore txStore = new PhysicalLogicalTransactionStore( logFiles, positionCache, new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>(), _monitors, true );

			  life.Add( new BatchingTransactionAppender( logFiles, NO_ROTATION, positionCache, transactionIdStore, BYPASS, _databaseHealth ) );
			  CorruptedLogsTruncator logPruner = new CorruptedLogsTruncator( _databaseDirectory, logFiles, FileSystemRule.get() );
			  life.add(new Recovery(new RecoveryServiceAnonymousInnerClass(this, recoveryRequired, visitor, txStore)
			 , logPruner, new LifecycleAdapter(), mock(typeof(RecoveryMonitor)), SilentProgressReporter.INSTANCE, false));

			  // WHEN
			  try
			  {
					life.Start();
			  }
			  finally
			  {
					life.Shutdown();
			  }

			  // THEN
			  assertEquals( 1, visitor.VisitedTransactions );
			  assertTrue( recoveryRequired.get() );
		 }

		 private class RecoveryServiceAnonymousInnerClass : RecoveryService
		 {
			 private readonly PhysicalLogicalTransactionStoreTest _outerInstance;

			 private AtomicBoolean _recoveryRequired;
			 private Org.Neo4j.Kernel.impl.transaction.log.PhysicalLogicalTransactionStoreTest.FakeRecoveryVisitor _visitor;
			 private Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore _txStore;

			 public RecoveryServiceAnonymousInnerClass( PhysicalLogicalTransactionStoreTest outerInstance, AtomicBoolean recoveryRequired, Org.Neo4j.Kernel.impl.transaction.log.PhysicalLogicalTransactionStoreTest.FakeRecoveryVisitor visitor, Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore txStore )
			 {
				 this.outerInstance = outerInstance;
				 this._recoveryRequired = recoveryRequired;
				 this._visitor = visitor;
				 this._txStore = txStore;
			 }

			 public void startRecovery()
			 {
				  _recoveryRequired.set( true );
			 }

			 public RecoveryApplier getRecoveryApplier( TransactionApplicationMode mode )
			 {
				  return mode == TransactionApplicationMode.REVERSE_RECOVERY ? mock( typeof( RecoveryApplier ) ) : _visitor;
			 }

			 public RecoveryStartInformation RecoveryStartInformation
			 {
				 get
				 {
					  return new RecoveryStartInformation( LogPosition.Start( 0 ), 1 );
				 }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionCursor getTransactions(LogPosition position) throws java.io.IOException
			 public TransactionCursor getTransactions( LogPosition position )
			 {
				  return _txStore.getTransactions( position );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionCursor getTransactionsInReverseOrder(LogPosition position) throws java.io.IOException
			 public TransactionCursor getTransactionsInReverseOrder( LogPosition position )
			 {
				  return _txStore.getTransactionsInReverseOrder( position );
			 }

			 public void transactionsRecovered( CommittedTransactionRepresentation lastRecoveredTransaction, LogPosition positionAfterLastRecoveredTransaction )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractMetadataFromExistingTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractMetadataFromExistingTransaction()
		 {
			  // GIVEN
			  TransactionIdStore transactionIdStore = new SimpleTransactionIdStore();
			  TransactionMetadataCache positionCache = new TransactionMetadataCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] additionalHeader = new byte[]{1, 2, 5};
			  sbyte[] additionalHeader = new sbyte[]{ 1, 2, 5 };
			  const int masterId = 2;
			  int authorId = 1;
			  const long timeStarted = 12345;
			  long latestCommittedTxWhenStarted = 4545;
			  long timeCommitted = timeStarted + 10;
			  LifeSupport life = new LifeSupport();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles = org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.builder(dir.databaseLayout(), fileSystemRule.get()).withTransactionIdStore(transactionIdStore).withLogVersionRepository(mock(LogVersionRepository.class)).build();
			  LogFiles logFiles = LogFilesBuilder.builder( Dir.databaseLayout(), FileSystemRule.get() ).withTransactionIdStore(transactionIdStore).withLogVersionRepository(mock(typeof(LogVersionRepository))).build();
			  life.Start();
			  life.Add( logFiles );
			  try
			  {
					AddATransactionAndRewind( life, logFiles, positionCache, transactionIdStore, additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted );
			  }
			  finally
			  {
					life.Shutdown();
			  }

			  life = new LifeSupport();
			  life.Add( logFiles );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogicalTransactionStore store = new PhysicalLogicalTransactionStore(logFiles, positionCache, new org.neo4j.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>(), monitors, true);
			  LogicalTransactionStore store = new PhysicalLogicalTransactionStore( logFiles, positionCache, new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>(), _monitors, true );

			  // WHEN
			  life.Start();
			  try
			  {
					VerifyTransaction( transactionIdStore, positionCache, additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted, store );
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowNoSuchTransactionExceptionIfMetadataNotFound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowNoSuchTransactionExceptionIfMetadataNotFound()
		 {
			  // GIVEN
			  LogFiles logFiles = mock( typeof( LogFiles ) );
			  TransactionMetadataCache cache = new TransactionMetadataCache();

			  LifeSupport life = new LifeSupport();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogicalTransactionStore txStore = new PhysicalLogicalTransactionStore(logFiles, cache, new org.neo4j.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>(), monitors, true);
			  LogicalTransactionStore txStore = new PhysicalLogicalTransactionStore( logFiles, cache, new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>(), _monitors, true );

			  try
			  {
					life.Start();
					// WHEN
					try
					{
						 txStore.GetMetadataFor( 10 );
						 fail( "Should have thrown" );
					}
					catch ( NoSuchTransactionException )
					{ // THEN Good
					}
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowNoSuchTransactionExceptionIfLogFileIsMissing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowNoSuchTransactionExceptionIfLogFileIsMissing()
		 {
			  // GIVEN
			  LogFile logFile = mock( typeof( LogFile ) );
			  LogFiles logFiles = mock( typeof( LogFiles ) );
			  // a missing file
			  when( logFiles.LogFile ).thenReturn( logFile );
			  when( logFile.GetReader( any( typeof( LogPosition ) ) ) ).thenThrow( new FileNotFoundException() );
			  // Which is nevertheless in the metadata cache
			  TransactionMetadataCache cache = new TransactionMetadataCache();
			  cache.CacheTransactionMetadata( 10, new LogPosition( 2, 130 ), 1, 1, 100, DateTimeHelper.CurrentUnixTimeMillis() );

			  LifeSupport life = new LifeSupport();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogicalTransactionStore txStore = new PhysicalLogicalTransactionStore(logFiles, cache, new org.neo4j.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>(), monitors, true);
			  LogicalTransactionStore txStore = new PhysicalLogicalTransactionStore( logFiles, cache, new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>(), _monitors, true );

			  try
			  {
					life.Start();

					// WHEN
					// we ask for that transaction and forward
					try
					{
						 txStore.getTransactions( 10 );
						 fail();
					}
					catch ( NoSuchTransactionException )
					{
						 // THEN
						 // We don't get a FileNotFoundException but a NoSuchTransactionException instead
					}
			  }
			  finally
			  {
					life.Shutdown();
			  }

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addATransactionAndRewind(org.neo4j.kernel.lifecycle.LifeSupport life, org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles, TransactionMetadataCache positionCache, TransactionIdStore transactionIdStore, byte[] additionalHeader, int masterId, int authorId, long timeStarted, long latestCommittedTxWhenStarted, long timeCommitted) throws java.io.IOException
		 private void AddATransactionAndRewind( LifeSupport life, LogFiles logFiles, TransactionMetadataCache positionCache, TransactionIdStore transactionIdStore, sbyte[] additionalHeader, int masterId, int authorId, long timeStarted, long latestCommittedTxWhenStarted, long timeCommitted )
		 {
			  TransactionAppender appender = life.Add( new BatchingTransactionAppender( logFiles, NO_ROTATION, positionCache, transactionIdStore, BYPASS, _databaseHealth ) );
			  PhysicalTransactionRepresentation transaction = new PhysicalTransactionRepresentation( SingleCreateNodeCommand() );
			  transaction.SetHeader( additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted, -1 );
			  appender.Append( new TransactionToApply( transaction ), Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent_Fields.Null );
		 }

		 private ICollection<StorageCommand> SingleCreateNodeCommand()
		 {
			  ICollection<StorageCommand> commands = new List<StorageCommand>();

			  long id = 0;
			  NodeRecord before = new NodeRecord( id );
			  NodeRecord after = new NodeRecord( id );
			  after.InUse = true;
			  commands.Add( new Command.NodeCommand( before, after ) );
			  return commands;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyTransaction(TransactionIdStore transactionIdStore, TransactionMetadataCache positionCache, byte[] additionalHeader, int masterId, int authorId, long timeStarted, long latestCommittedTxWhenStarted, long timeCommitted, LogicalTransactionStore store) throws java.io.IOException
		 private void VerifyTransaction( TransactionIdStore transactionIdStore, TransactionMetadataCache positionCache, sbyte[] additionalHeader, int masterId, int authorId, long timeStarted, long latestCommittedTxWhenStarted, long timeCommitted, LogicalTransactionStore store )
		 {
			  TransactionMetadata expectedMetadata;
			  using ( TransactionCursor cursor = store.GetTransactions( TransactionIdStore_Fields.BASE_TX_ID + 1 ) )
			  {
					bool hasNext = cursor.next();
					assertTrue( hasNext );
					CommittedTransactionRepresentation tx = cursor.get();
					TransactionRepresentation transaction = tx.TransactionRepresentation;
					assertArrayEquals( additionalHeader, transaction.AdditionalHeader() );
					assertEquals( masterId, transaction.MasterId );
					assertEquals( authorId, transaction.AuthorId );
					assertEquals( timeStarted, transaction.TimeStarted );
					assertEquals( timeCommitted, transaction.TimeCommitted );
					assertEquals( latestCommittedTxWhenStarted, transaction.LatestCommittedTxWhenStarted );
					expectedMetadata = new TransactionMetadata( masterId, authorId, tx.StartEntry.StartPosition, tx.StartEntry.checksum(), timeCommitted );
			  }

			  positionCache.Clear();

			  TransactionMetadata actualMetadata = store.GetMetadataFor( transactionIdStore.LastCommittedTransactionId );
			  assertEquals( expectedMetadata, actualMetadata );
		 }

		 private class FakeRecoveryVisitor : RecoveryApplier
		 {
			  internal readonly sbyte[] AdditionalHeader;
			  internal readonly int MasterId;
			  internal readonly int AuthorId;
			  internal readonly long TimeStarted;
			  internal readonly long TimeCommitted;
			  internal readonly long LatestCommittedTxWhenStarted;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int VisitedTransactionsConflict;

			  internal FakeRecoveryVisitor( sbyte[] additionalHeader, int masterId, int authorId, long timeStarted, long timeCommitted, long latestCommittedTxWhenStarted )
			  {
					this.AdditionalHeader = additionalHeader;
					this.MasterId = masterId;
					this.AuthorId = authorId;
					this.TimeStarted = timeStarted;
					this.TimeCommitted = timeCommitted;
					this.LatestCommittedTxWhenStarted = latestCommittedTxWhenStarted;
			  }

			  public override bool Visit( CommittedTransactionRepresentation tx )
			  {
					TransactionRepresentation transaction = tx.TransactionRepresentation;
					assertArrayEquals( AdditionalHeader, transaction.AdditionalHeader() );
					assertEquals( MasterId, transaction.MasterId );
					assertEquals( AuthorId, transaction.AuthorId );
					assertEquals( TimeStarted, transaction.TimeStarted );
					assertEquals( TimeCommitted, transaction.TimeCommitted );
					assertEquals( LatestCommittedTxWhenStarted, transaction.LatestCommittedTxWhenStarted );
					VisitedTransactionsConflict++;
					return false;
			  }

			  internal virtual int VisitedTransactions
			  {
				  get
				  {
						return VisitedTransactionsConflict;
				  }
			  }

			  public override void Close()
			  {
			  }
		 }
	}

}