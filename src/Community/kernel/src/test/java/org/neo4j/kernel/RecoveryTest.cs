using System;

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
namespace Neo4Net.Kernel
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using Neo4Net.Helpers.Collection;
	using Neo4Net.Helpers.Collection;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using SimpleLogVersionRepository = Neo4Net.Kernel.impl.transaction.SimpleLogVersionRepository;
	using SimpleTransactionIdStore = Neo4Net.Kernel.impl.transaction.SimpleTransactionIdStore;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogPositionMarker = Neo4Net.Kernel.impl.transaction.log.LogPositionMarker;
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;
	using LogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using PhysicalLogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using PhysicalLogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.PhysicalLogicalTransactionStore;
	using PositionAwarePhysicalFlushableChannel = Neo4Net.Kernel.impl.transaction.log.PositionAwarePhysicalFlushableChannel;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using TransactionMetadataCache = Neo4Net.Kernel.impl.transaction.log.TransactionMetadataCache;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using SilentProgressReporter = Neo4Net.Kernel.impl.util.monitoring.SilentProgressReporter;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using CorruptedLogsTruncator = Neo4Net.Kernel.recovery.CorruptedLogsTruncator;
	using DefaultRecoveryService = Neo4Net.Kernel.recovery.DefaultRecoveryService;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using Recovery = Neo4Net.Kernel.recovery.Recovery;
	using RecoveryApplier = Neo4Net.Kernel.recovery.RecoveryApplier;
	using RecoveryMonitor = Neo4Net.Kernel.recovery.RecoveryMonitor;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderWriter.writeLogHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogVersions.CURRENT_LOG_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.recovery.RecoveryStartInformationProvider.NO_MONITOR;

	public class RecoveryTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();
		 private readonly LogVersionRepository _logVersionRepository = new SimpleLogVersionRepository();
		 private readonly TransactionIdStore _transactionIdStore = new SimpleTransactionIdStore( 5L, 0, BASE_TX_COMMIT_TIMESTAMP, 0, 0 );
		 private readonly int _logVersion = 0;

		 private LogEntry _lastCommittedTxStartEntry;
		 private LogEntry _lastCommittedTxCommitEntry;
		 private LogEntry _expectedStartEntry;
		 private LogEntry _expectedCommitEntry;
		 private LogEntry _expectedCheckPointEntry;
		 private Monitors _monitors = new Monitors();
		 private readonly SimpleLogVersionRepository _versionRepository = new SimpleLogVersionRepository();
		 private LogFiles _logFiles;
		 private File _storeDir;
		 private Lifecycle _schemaLife;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _storeDir = Directory.storeDir();
			  _logFiles = LogFilesBuilder.builder( Directory.databaseLayout(), FileSystemRule.get() ).withLogVersionRepository(_logVersionRepository).withTransactionIdStore(_transactionIdStore).build();
			  _schemaLife = new LifecycleAdapter();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverExistingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverExistingData()
		 {
			  File file = _logFiles.getLogFileForVersion( _logVersion );

			  WriteSomeData(file, pair =>
			  {
				LogEntryWriter writer = pair.first();
				Consumer<LogPositionMarker> consumer = pair.other();
				LogPositionMarker marker = new LogPositionMarker();

				// last committed tx
				consumer.accept( marker );
				LogPosition lastCommittedTxPosition = marker.newPosition();
				writer.writeStartEntry( 0, 1, 2L, 3L, new sbyte[0] );
				_lastCommittedTxStartEntry = new LogEntryStart( 0, 1, 2L, 3L, new sbyte[0], lastCommittedTxPosition );
				writer.writeCommitEntry( 4L, 5L );
				_lastCommittedTxCommitEntry = new LogEntryCommit( 4L, 5L );

				// check point pointing to the previously committed transaction
				writer.writeCheckPointEntry( lastCommittedTxPosition );
				_expectedCheckPointEntry = new CheckPoint( lastCommittedTxPosition );

				// tx committed after checkpoint
				consumer.accept( marker );
				writer.writeStartEntry( 0, 1, 6L, 4L, new sbyte[0] );
				_expectedStartEntry = new LogEntryStart( 0, 1, 6L, 4L, new sbyte[0], marker.newPosition() );

				writer.writeCommitEntry( 5L, 7L );
				_expectedCommitEntry = new LogEntryCommit( 5L, 7L );

				return true;
			  });

			  LifeSupport life = new LifeSupport();
			  RecoveryMonitor monitor = mock( typeof( RecoveryMonitor ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean recoveryRequired = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean recoveryRequired = new AtomicBoolean();
			  try
			  {
					StorageEngine storageEngine = mock( typeof( StorageEngine ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.entry.LogEntryReader<org.neo4j.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel> reader = new org.neo4j.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>();
					LogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
					LogTailScanner tailScanner = GetTailScanner( _logFiles, reader );

					TransactionMetadataCache metadataCache = new TransactionMetadataCache();
					LogicalTransactionStore txStore = new PhysicalLogicalTransactionStore( _logFiles, metadataCache, reader, _monitors, false );
					CorruptedLogsTruncator logPruner = new CorruptedLogsTruncator( _storeDir, _logFiles, FileSystemRule.get() );
					life.add(new Recovery(new DefaultRecoveryServiceAnonymousInnerClass(this, storageEngine, tailScanner, _transactionIdStore, txStore, _versionRepository, NO_MONITOR, recoveryRequired)
				  , logPruner, _schemaLife, monitor, SilentProgressReporter.INSTANCE, false));

					life.Start();

					InOrder order = inOrder( monitor );
					order.verify( monitor, times( 1 ) ).recoveryRequired( any( typeof( LogPosition ) ) );
					order.verify( monitor, times( 1 ) ).recoveryCompleted( 2 );
					assertTrue( recoveryRequired.get() );
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

		 private class DefaultRecoveryServiceAnonymousInnerClass : DefaultRecoveryService
		 {
			 private readonly RecoveryTest _outerInstance;

			 private AtomicBoolean _recoveryRequired;

			 public DefaultRecoveryServiceAnonymousInnerClass( RecoveryTest outerInstance, StorageEngine storageEngine, LogTailScanner tailScanner, TransactionIdStore transactionIdStore, LogicalTransactionStore txStore, SimpleLogVersionRepository versionRepository, UnknownType noMonitor, AtomicBoolean recoveryRequired ) : base( storageEngine, tailScanner, transactionIdStore, txStore, versionRepository, noMonitor )
			 {
				 this.outerInstance = outerInstance;
				 this._recoveryRequired = recoveryRequired;
			 }

			 private int nr;

			 public override void startRecovery()
			 {
				  _recoveryRequired.set( true );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.recovery.RecoveryApplier getRecoveryApplier(org.neo4j.storageengine.api.TransactionApplicationMode mode) throws Exception
			 public override RecoveryApplier getRecoveryApplier( TransactionApplicationMode mode )
			 {
				  RecoveryApplier actual = base.getRecoveryApplier( mode );
				  if ( mode == TransactionApplicationMode.REVERSE_RECOVERY )
				  {
						return actual;
				  }

				  return new RecoveryApplierAnonymousInnerClass( this, actual );
			 }

			 private class RecoveryApplierAnonymousInnerClass : RecoveryApplier
			 {
				 private readonly DefaultRecoveryServiceAnonymousInnerClass _outerInstance;

				 private RecoveryApplier _actual;

				 public RecoveryApplierAnonymousInnerClass( DefaultRecoveryServiceAnonymousInnerClass outerInstance, RecoveryApplier actual )
				 {
					 this.outerInstance = outerInstance;
					 this._actual = actual;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
				 public void close()
				 {
					  _actual.close();
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation tx) throws Exception
				 public bool visit( CommittedTransactionRepresentation tx )
				 {
					  _actual.visit( tx );
					  switch ( nr++ )
					  {
					  case 0:
							assertEquals( _outerInstance.outerInstance.lastCommittedTxStartEntry, tx.StartEntry );
							assertEquals( _outerInstance.outerInstance.lastCommittedTxCommitEntry, tx.CommitEntry );
							break;
					  case 1:
							assertEquals( _outerInstance.outerInstance.expectedStartEntry, tx.StartEntry );
							assertEquals( _outerInstance.outerInstance.expectedCommitEntry, tx.CommitEntry );
							break;
					  default:
						  fail( "Too many recovered transactions" );
					  break;
					  }
					  return false;
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeThatACleanDatabaseShouldNotRequireRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeThatACleanDatabaseShouldNotRequireRecovery()
		 {
			  File file = _logFiles.getLogFileForVersion( _logVersion );

			  WriteSomeData(file, pair =>
			  {
				LogEntryWriter writer = pair.first();
				Consumer<LogPositionMarker> consumer = pair.other();
				LogPositionMarker marker = new LogPositionMarker();

				// last committed tx
				consumer.accept( marker );
				writer.writeStartEntry( 0, 1, 2L, 3L, new sbyte[0] );
				writer.writeCommitEntry( 4L, 5L );

				// check point
				consumer.accept( marker );
				writer.writeCheckPointEntry( marker.newPosition() );

				return true;
			  });

			  LifeSupport life = new LifeSupport();
			  RecoveryMonitor monitor = mock( typeof( RecoveryMonitor ) );
			  try
			  {
					StorageEngine storageEngine = mock( typeof( StorageEngine ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.entry.LogEntryReader<org.neo4j.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel> reader = new org.neo4j.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>();
					LogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
					LogTailScanner tailScanner = GetTailScanner( _logFiles, reader );

					TransactionMetadataCache metadataCache = new TransactionMetadataCache();
					LogicalTransactionStore txStore = new PhysicalLogicalTransactionStore( _logFiles, metadataCache, reader, _monitors, false );
					CorruptedLogsTruncator logPruner = new CorruptedLogsTruncator( _storeDir, _logFiles, FileSystemRule.get() );
					life.add(new Recovery(new DefaultRecoveryServiceAnonymousInnerClass2(this, storageEngine, tailScanner, _transactionIdStore, txStore, _versionRepository, NO_MONITOR)
				  , logPruner, _schemaLife, monitor, SilentProgressReporter.INSTANCE, false));

					life.Start();

					verifyZeroInteractions( monitor );
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

		 private class DefaultRecoveryServiceAnonymousInnerClass2 : DefaultRecoveryService
		 {
			 private readonly RecoveryTest _outerInstance;

			 public DefaultRecoveryServiceAnonymousInnerClass2( RecoveryTest outerInstance, StorageEngine storageEngine, LogTailScanner tailScanner, TransactionIdStore transactionIdStore, LogicalTransactionStore txStore, SimpleLogVersionRepository versionRepository, UnknownType noMonitor ) : base( storageEngine, tailScanner, transactionIdStore, txStore, versionRepository, noMonitor )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void startRecovery()
			 {
				  fail( "Recovery should not be required" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateLogAfterSinglePartialTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateLogAfterSinglePartialTransaction()
		 {
			  // GIVEN
			  File file = _logFiles.getLogFileForVersion( _logVersion );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPositionMarker marker = new org.neo4j.kernel.impl.transaction.log.LogPositionMarker();
			  LogPositionMarker marker = new LogPositionMarker();

			  WriteSomeData(file, pair =>
			  {
				LogEntryWriter writer = pair.first();
				Consumer<LogPositionMarker> consumer = pair.other();

				// incomplete tx
				consumer.accept( marker ); // <-- marker has the last good position
				writer.writeStartEntry( 0, 1, 5L, 4L, new sbyte[0] );

				return true;
			  });

			  // WHEN
			  bool recoveryRequired = Recover( _storeDir, _logFiles );

			  // THEN
			  assertTrue( recoveryRequired );
			  assertEquals( marker.ByteOffset, file.length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotTruncateCheckpointsAfterLastTransaction() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotTruncateCheckpointsAfterLastTransaction()
		 {
			  File file = _logFiles.getLogFileForVersion( _logVersion );
			  LogPositionMarker marker = new LogPositionMarker();
			  WriteSomeData(file, pair =>
			  {
				LogEntryWriter writer = pair.first();
				writer.writeStartEntry( 1, 1, 1L, 1L, ArrayUtils.EMPTY_BYTE_ARRAY );
				writer.writeCommitEntry( 1L, 2L );
				writer.writeCheckPointEntry( new LogPosition( _logVersion, LogHeader.LOG_HEADER_SIZE ) );
				writer.writeCheckPointEntry( new LogPosition( _logVersion, LogHeader.LOG_HEADER_SIZE ) );
				writer.writeCheckPointEntry( new LogPosition( _logVersion, LogHeader.LOG_HEADER_SIZE ) );
				writer.writeCheckPointEntry( new LogPosition( _logVersion, LogHeader.LOG_HEADER_SIZE ) );
				Consumer<LogPositionMarker> other = pair.other();
				other.accept( marker );
				return true;
			  });
			  assertTrue( Recover( _storeDir, _logFiles ) );

			  assertEquals( marker.ByteOffset, file.length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateLogAfterLastCompleteTransactionAfterSuccessfulRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateLogAfterLastCompleteTransactionAfterSuccessfulRecovery()
		 {
			  // GIVEN
			  File file = _logFiles.getLogFileForVersion( _logVersion );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPositionMarker marker = new org.neo4j.kernel.impl.transaction.log.LogPositionMarker();
			  LogPositionMarker marker = new LogPositionMarker();

			  WriteSomeData(file, pair =>
			  {
				LogEntryWriter writer = pair.first();
				Consumer<LogPositionMarker> consumer = pair.other();

				// last committed tx
				writer.writeStartEntry( 0, 1, 2L, 3L, new sbyte[0] );
				writer.writeCommitEntry( 4L, 5L );

				// incomplete tx
				consumer.accept( marker ); // <-- marker has the last good position
				writer.writeStartEntry( 0, 1, 5L, 4L, new sbyte[0] );

				return true;
			  });

			  // WHEN
			  bool recoveryRequired = Recover( _storeDir, _logFiles );

			  // THEN
			  assertTrue( recoveryRequired );
			  assertEquals( marker.ByteOffset, file.length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTellTransactionIdStoreAfterSuccessfulRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTellTransactionIdStoreAfterSuccessfulRecovery()
		 {
			  // GIVEN
			  File file = _logFiles.getLogFileForVersion( _logVersion );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPositionMarker marker = new org.neo4j.kernel.impl.transaction.log.LogPositionMarker();
			  LogPositionMarker marker = new LogPositionMarker();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] additionalHeaderData = new byte[0];
			  sbyte[] additionalHeaderData = new sbyte[0];
			  const int masterId = 0;
			  const int authorId = 1;
			  const long transactionId = 4;
			  const long commitTimestamp = 5;
			  WriteSomeData(file, pair =>
			  {
				LogEntryWriter writer = pair.first();
				Consumer<LogPositionMarker> consumer = pair.other();

				// last committed tx
				writer.writeStartEntry( masterId, authorId, 2L, 3L, additionalHeaderData );
				writer.writeCommitEntry( transactionId, commitTimestamp );
				consumer.accept( marker );

				return true;
			  });

			  // WHEN
			  bool recoveryRequired = Recover( _storeDir, _logFiles );

			  // THEN
			  assertTrue( recoveryRequired );
			  long[] lastClosedTransaction = _transactionIdStore.LastClosedTransaction;
			  assertEquals( transactionId, lastClosedTransaction[0] );
			  assertEquals( LogEntryStart.checksum( additionalHeaderData, masterId, authorId ), _transactionIdStore.LastCommittedTransaction.checksum() );
			  assertEquals( commitTimestamp, _transactionIdStore.LastCommittedTransaction.commitTimestamp() );
			  assertEquals( _logVersion, lastClosedTransaction[1] );
			  assertEquals( marker.ByteOffset, lastClosedTransaction[2] );
		 }

		 private bool Recover( File storeDir, LogFiles logFiles )
		 {
			  LifeSupport life = new LifeSupport();
			  RecoveryMonitor monitor = mock( typeof( RecoveryMonitor ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean recoveryRequired = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean recoveryRequired = new AtomicBoolean();
			  try
			  {
					StorageEngine storageEngine = mock( typeof( StorageEngine ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.entry.LogEntryReader<org.neo4j.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel> reader = new org.neo4j.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>();
					LogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
					LogTailScanner tailScanner = GetTailScanner( logFiles, reader );

					TransactionMetadataCache metadataCache = new TransactionMetadataCache();
					LogicalTransactionStore txStore = new PhysicalLogicalTransactionStore( logFiles, metadataCache, reader, _monitors, false );
					CorruptedLogsTruncator logPruner = new CorruptedLogsTruncator( storeDir, logFiles, FileSystemRule.get() );
					life.add(new Recovery(new DefaultRecoveryServiceAnonymousInnerClass3(this, storageEngine, tailScanner, _transactionIdStore, txStore, _versionRepository, NO_MONITOR, recoveryRequired)
				  , logPruner, _schemaLife, monitor, SilentProgressReporter.INSTANCE, false));

					life.Start();
			  }
			  finally
			  {
					life.Shutdown();
			  }
			  return recoveryRequired.get();
		 }

		 private class DefaultRecoveryServiceAnonymousInnerClass3 : DefaultRecoveryService
		 {
			 private readonly RecoveryTest _outerInstance;

			 private AtomicBoolean _recoveryRequired;

			 public DefaultRecoveryServiceAnonymousInnerClass3( RecoveryTest outerInstance, StorageEngine storageEngine, LogTailScanner tailScanner, TransactionIdStore transactionIdStore, LogicalTransactionStore txStore, SimpleLogVersionRepository versionRepository, UnknownType noMonitor, AtomicBoolean recoveryRequired ) : base( storageEngine, tailScanner, transactionIdStore, txStore, versionRepository, noMonitor )
			 {
				 this.outerInstance = outerInstance;
				 this._recoveryRequired = recoveryRequired;
			 }

			 public override void startRecovery()
			 {
				  _recoveryRequired.set( true );
			 }
		 }

		 private LogTailScanner GetTailScanner( LogFiles logFiles, LogEntryReader<ReadableClosablePositionAwareChannel> reader )
		 {
			  return new LogTailScanner( logFiles, reader, _monitors, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeSomeData(java.io.File file, org.neo4j.helpers.collection.Visitor<org.neo4j.helpers.collection.Pair<org.neo4j.kernel.impl.transaction.log.entry.LogEntryWriter,System.Action<org.neo4j.kernel.impl.transaction.log.LogPositionMarker>>,java.io.IOException> visitor) throws java.io.IOException
		 private void WriteSomeData( File file, Visitor<Pair<LogEntryWriter, System.Action<LogPositionMarker>>, IOException> visitor )
		 {

			  using ( LogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( FileSystemRule.get().open(file, OpenMode.READ_WRITE), _logVersion, CURRENT_LOG_VERSION ), PositionAwarePhysicalFlushableChannel writableLogChannel = new PositionAwarePhysicalFlushableChannel(versionedStoreChannel) )
			  {
					writeLogHeader( writableLogChannel, _logVersion, 2L );

					System.Action<LogPositionMarker> consumer = marker =>
					{
					 try
					 {
						  writableLogChannel.GetCurrentPosition( marker );
					 }
					 catch ( IOException e )
					 {
						  throw new Exception( e );
					 }
					};
					LogEntryWriter first = new LogEntryWriter( writableLogChannel );
					visitor.Visit( Pair.of( first, consumer ) );
			  }
		 }
	}

}