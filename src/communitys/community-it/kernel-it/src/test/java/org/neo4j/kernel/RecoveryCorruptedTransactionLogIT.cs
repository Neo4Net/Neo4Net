using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
	using MutableObjectLongMap = org.eclipse.collections.api.map.primitive.MutableObjectLongMap;
	using ObjectLongMap = org.eclipse.collections.api.map.primitive.ObjectLongMap;
	using ObjectLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectLongHashMap;
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using SimpleLogVersionRepository = Neo4Net.Kernel.impl.transaction.SimpleLogVersionRepository;
	using SimpleTransactionIdStore = Neo4Net.Kernel.impl.transaction.SimpleTransactionIdStore;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using FlushablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.FlushablePositionAwareChannel;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using ReadableLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadableLogChannel;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using TransactionLogWriter = Neo4Net.Kernel.impl.transaction.log.TransactionLogWriter;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using UnsupportedLogVersionException = Neo4Net.Kernel.impl.transaction.log.entry.UnsupportedLogVersionException;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using CorruptedLogsTruncator = Neo4Net.Kernel.recovery.CorruptedLogsTruncator;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using Neo4Net.Test.mockito.matcher;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.emptyArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_START;

	public class RecoveryCorruptedTransactionLogIT
	{
		private bool InstanceFieldsInitialized = false;

		public RecoveryCorruptedTransactionLogIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( _fileSystemRule );
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _directory ).around( _expectedException ).around( _random );
		}

		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly ExpectedException _expectedException = ExpectedException.none();
		 private readonly RandomRule _random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(directory).around(expectedException).around(random);
		 public RuleChain RuleChain;

		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider( true );
		 private readonly RecoveryMonitor _recoveryMonitor = new RecoveryMonitor();
		 private File _storeDir;
		 private Monitors _monitors = new Monitors();
		 private LogFiles _logFiles;
		 private TestGraphDatabaseFactory _databaseFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _storeDir = _directory.databaseDir();
			  _monitors.addMonitorListener( _recoveryMonitor );
			  _databaseFactory = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(_logProvider).setMonitors(_monitors);
			  _logFiles = BuildDefaultLogFiles();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void evenTruncateNewerTransactionLogFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EvenTruncateNewerTransactionLogFile()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  TransactionIdStore transactionIdStore = GetTransactionIdStore( database );
			  long lastClosedTransactionBeforeStart = transactionIdStore.LastClosedTransactionId;
			  for ( int i = 0; i < 10; i++ )
			  {
					GenerateTransaction( database );
			  }
			  long numberOfClosedTransactions = GetTransactionIdStore( database ).LastClosedTransactionId - lastClosedTransactionBeforeStart;
			  database.Shutdown();
			  RemoveLastCheckpointRecordFromLastLogFile();
			  AddRandomBytesToLastLogFile( this.randomBytes );

			  database = StartDbNoRecoveryOfCorruptedLogs();
			  database.Shutdown();

			  assertEquals( numberOfClosedTransactions, _recoveryMonitor.NumberOfRecoveredTransactions );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotTruncateNewerTransactionLogFileWhenFailOnError() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotTruncateNewerTransactionLogFileWhenFailOnError()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  for ( int i = 0; i < 10; i++ )
			  {
					GenerateTransaction( database );
			  }
			  database.Shutdown();
			  RemoveLastCheckpointRecordFromLastLogFile();
			  AddRandomBytesToLastLogFile( this.randomPositiveBytes );

			  _expectedException.expectCause( new RootCauseMatcher<>( typeof( UnsupportedLogVersionException ) ) );

			  database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  database.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void truncateNewerTransactionLogFileWhenForced() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TruncateNewerTransactionLogFileWhenForced()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  for ( int i = 0; i < 10; i++ )
			  {
					GenerateTransaction( database );
			  }
			  TransactionIdStore transactionIdStore = GetTransactionIdStore( database );
			  long numberOfClosedTransactions = transactionIdStore.LastClosedTransactionId - 1;
			  database.Shutdown();

			  RemoveLastCheckpointRecordFromLastLogFile();
			  AddRandomBytesToLastLogFile( this.randomBytes );

			  database = StartDbNoRecoveryOfCorruptedLogs();
			  database.Shutdown();

			  _logProvider.rawMessageMatcher().assertContains("Fail to read transaction log version 0.");
			  _logProvider.rawMessageMatcher().assertContains("Fail to read transaction log version 0. Last valid transaction start offset is: 5668.");
			  assertEquals( numberOfClosedTransactions, _recoveryMonitor.NumberOfRecoveredTransactions );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFirstCorruptedTransactionSingleFileNoCheckpoint() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFirstCorruptedTransactionSingleFileNoCheckpoint()
		 {
			  AddCorruptedCommandsToLastLogFile();

			  GraphDatabaseService recoveredDatabase = StartDbNoRecoveryOfCorruptedLogs();
			  recoveredDatabase.Shutdown();

			  _logProvider.rawMessageMatcher().assertContains("Fail to read transaction log version 0.");
			  _logProvider.rawMessageMatcher().assertContains("Fail to read first transaction of log version 0.");
			  _logProvider.rawMessageMatcher().assertContains("Recovery required from position LogPosition{logVersion=0, byteOffset=16}");
			  _logProvider.rawMessageMatcher().assertContains("Fail to recover all transactions. Any later transactions after" + " position LogPosition{logVersion=0, byteOffset=16} are unreadable and will be truncated.");

			  assertEquals( 0, _logFiles.HighestLogVersion );
			  ObjectLongMap<Type> logEntriesDistribution = GetLogEntriesDistribution( _logFiles );
			  assertEquals( 1, logEntriesDistribution.size() );
			  assertEquals( 1, logEntriesDistribution.get( typeof( CheckPoint ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failToRecoverFirstCorruptedTransactionSingleFileNoCheckpointIfFailOnCorruption() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailToRecoverFirstCorruptedTransactionSingleFileNoCheckpointIfFailOnCorruption()
		 {
			  AddCorruptedCommandsToLastLogFile();

			  _expectedException.expectCause( new RootCauseMatcher<>( typeof( NegativeArraySizeException ) ) );

			  GraphDatabaseService recoveredDatabase = _databaseFactory.newEmbeddedDatabase( _storeDir );
			  recoveredDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverNotAFirstCorruptedTransactionSingleFileNoCheckpoint() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverNotAFirstCorruptedTransactionSingleFileNoCheckpoint()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  TransactionIdStore transactionIdStore = GetTransactionIdStore( database );
			  long lastClosedTransactionBeforeStart = transactionIdStore.LastClosedTransactionId;
			  for ( int i = 0; i < 10; i++ )
			  {
					GenerateTransaction( database );
			  }
			  long numberOfTransactions = transactionIdStore.LastClosedTransactionId - lastClosedTransactionBeforeStart;
			  database.Shutdown();

			  File highestLogFile = _logFiles.HighestLogFile;
			  long originalFileLength = highestLogFile.length();
			  RemoveLastCheckpointRecordFromLastLogFile();

			  AddCorruptedCommandsToLastLogFile();
			  long modifiedFileLength = highestLogFile.length();

			  assertThat( modifiedFileLength, greaterThan( originalFileLength ) );

			  database = StartDbNoRecoveryOfCorruptedLogs();
			  database.Shutdown();

			  _logProvider.rawMessageMatcher().assertContains("Fail to read transaction log version 0.");
			  _logProvider.rawMessageMatcher().assertContains("Recovery required from position LogPosition{logVersion=0, byteOffset=16}");
			  _logProvider.rawMessageMatcher().assertContains("Fail to recover all transactions.");
			  _logProvider.rawMessageMatcher().assertContains("Any later transaction after LogPosition{logVersion=0, byteOffset=6245} are unreadable and will be truncated.");

			  assertEquals( 0, _logFiles.HighestLogVersion );
			  ObjectLongMap<Type> logEntriesDistribution = GetLogEntriesDistribution( _logFiles );
			  assertEquals( 1, logEntriesDistribution.get( typeof( CheckPoint ) ) );
			  assertEquals( numberOfTransactions, _recoveryMonitor.NumberOfRecoveredTransactions );
			  assertEquals( originalFileLength, highestLogFile.length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverNotAFirstCorruptedTransactionMultipleFilesNoCheckpoints() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverNotAFirstCorruptedTransactionMultipleFilesNoCheckpoints()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  TransactionIdStore transactionIdStore = GetTransactionIdStore( database );
			  long lastClosedTransactionBeforeStart = transactionIdStore.LastClosedTransactionId;
			  GenerateTransactionsAndRotate( database, 3 );
			  for ( int i = 0; i < 7; i++ )
			  {
					GenerateTransaction( database );
			  }
			  long numberOfTransactions = transactionIdStore.LastClosedTransactionId - lastClosedTransactionBeforeStart;
			  database.Shutdown();

			  LogFiles logFiles = BuildDefaultLogFiles();
			  File highestLogFile = logFiles.HighestLogFile;
			  long originalFileLength = highestLogFile.length();
			  RemoveLastCheckpointRecordFromLastLogFile();

			  AddCorruptedCommandsToLastLogFile();
			  long modifiedFileLength = highestLogFile.length();

			  assertThat( modifiedFileLength, greaterThan( originalFileLength ) );

			  database = StartDbNoRecoveryOfCorruptedLogs();
			  database.Shutdown();

			  _logProvider.rawMessageMatcher().assertContains("Fail to read transaction log version 3.");
			  _logProvider.rawMessageMatcher().assertContains("Recovery required from position LogPosition{logVersion=0, byteOffset=16}");
			  _logProvider.rawMessageMatcher().assertContains("Fail to recover all transactions.");
			  _logProvider.rawMessageMatcher().assertContains("Any later transaction after LogPosition{logVersion=3, byteOffset=4632} are unreadable and will be truncated.");

			  assertEquals( 3, logFiles.HighestLogVersion );
			  ObjectLongMap<Type> logEntriesDistribution = GetLogEntriesDistribution( logFiles );
			  assertEquals( 1, logEntriesDistribution.get( typeof( CheckPoint ) ) );
			  assertEquals( numberOfTransactions, _recoveryMonitor.NumberOfRecoveredTransactions );
			  assertEquals( originalFileLength, highestLogFile.length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverNotAFirstCorruptedTransactionMultipleFilesMultipleCheckpoints() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverNotAFirstCorruptedTransactionMultipleFilesMultipleCheckpoints()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  long transactionsToRecover = 7;
			  GenerateTransactionsAndRotateWithCheckpoint( database, 3 );
			  for ( int i = 0; i < transactionsToRecover; i++ )
			  {
					GenerateTransaction( database );
			  }
			  database.Shutdown();

			  File highestLogFile = _logFiles.HighestLogFile;
			  long originalFileLength = highestLogFile.length();
			  RemoveLastCheckpointRecordFromLastLogFile();

			  AddCorruptedCommandsToLastLogFile();
			  long modifiedFileLength = highestLogFile.length();

			  assertThat( modifiedFileLength, greaterThan( originalFileLength ) );

			  database = StartDbNoRecoveryOfCorruptedLogs();
			  database.Shutdown();

			  _logProvider.rawMessageMatcher().assertContains("Fail to read transaction log version 3.");
			  _logProvider.rawMessageMatcher().assertContains("Recovery required from position LogPosition{logVersion=3, byteOffset=593}");
			  _logProvider.rawMessageMatcher().assertContains("Fail to recover all transactions.");
			  _logProvider.rawMessageMatcher().assertContains("Any later transaction after LogPosition{logVersion=3, byteOffset=4650} are unreadable and will be truncated.");

			  assertEquals( 3, _logFiles.HighestLogVersion );
			  ObjectLongMap<Type> logEntriesDistribution = GetLogEntriesDistribution( _logFiles );
			  assertEquals( 4, logEntriesDistribution.get( typeof( CheckPoint ) ) );
			  assertEquals( transactionsToRecover, _recoveryMonitor.NumberOfRecoveredTransactions );
			  assertEquals( originalFileLength, highestLogFile.length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverFirstCorruptedTransactionAfterCheckpointInLastLogFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverFirstCorruptedTransactionAfterCheckpointInLastLogFile()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  GenerateTransactionsAndRotate( database, 5 );
			  database.Shutdown();

			  File highestLogFile = _logFiles.HighestLogFile;
			  long originalFileLength = highestLogFile.length();
			  AddCorruptedCommandsToLastLogFile();
			  long modifiedFileLength = highestLogFile.length();

			  assertThat( modifiedFileLength, greaterThan( originalFileLength ) );

			  database = StartDbNoRecoveryOfCorruptedLogs();
			  database.Shutdown();

			  _logProvider.rawMessageMatcher().assertContains("Fail to read transaction log version 5.");
			  _logProvider.rawMessageMatcher().assertContains("Fail to read first transaction of log version 5.");
			  _logProvider.rawMessageMatcher().assertContains("Recovery required from position LogPosition{logVersion=5, byteOffset=593}");
			  _logProvider.rawMessageMatcher().assertContains("Fail to recover all transactions. " + "Any later transactions after position LogPosition{logVersion=5, byteOffset=593} " + "are unreadable and will be truncated.");

			  assertEquals( 5, _logFiles.HighestLogVersion );
			  ObjectLongMap<Type> logEntriesDistribution = GetLogEntriesDistribution( _logFiles );
			  assertEquals( 1, logEntriesDistribution.get( typeof( CheckPoint ) ) );
			  assertEquals( originalFileLength, highestLogFile.length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void repetitiveRecoveryOfCorruptedLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RepetitiveRecoveryOfCorruptedLogs()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  GenerateTransactionsAndRotate( database, 4, false );
			  database.Shutdown();
			  RemoveLastCheckpointRecordFromLastLogFile();

			  int expectedRecoveredTransactions = 7;
			  while ( expectedRecoveredTransactions > 0 )
			  {
					TruncateBytesFromLastLogFile( 1 + _random.Next( 10 ) );
					_databaseFactory.newEmbeddedDatabase( _storeDir ).shutdown();
					int numberOfRecoveredTransactions = _recoveryMonitor.NumberOfRecoveredTransactions;
					assertEquals( expectedRecoveredTransactions, numberOfRecoveredTransactions );
					expectedRecoveredTransactions--;
					RemoveLastCheckpointRecordFromLastLogFile();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void repetitiveRecoveryIfCorruptedLogsWithCheckpoints() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RepetitiveRecoveryIfCorruptedLogsWithCheckpoints()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  GenerateTransactionsAndRotate( database, 4, true );
			  database.Shutdown();

			  while ( _logFiles.HighestLogVersion > 0 )
			  {
					int bytesToTrim = 1 + _random.Next( 100 );
					TruncateBytesFromLastLogFile( bytesToTrim );
					_databaseFactory.newEmbeddedDatabase( _storeDir ).shutdown();
					int numberOfRecoveredTransactions = _recoveryMonitor.NumberOfRecoveredTransactions;
					assertThat( numberOfRecoveredTransactions, Matchers.greaterThanOrEqualTo( 0 ) );
			  }

			  File corruptedLogArchives = new File( _storeDir, CorruptedLogsTruncator.CORRUPTED_TX_LOGS_BASE_NAME );
			  assertThat( corruptedLogArchives.listFiles(), not(emptyArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void repetitiveRecoveryIfCorruptedLogsSmallTailsWithCheckpoints() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RepetitiveRecoveryIfCorruptedLogsSmallTailsWithCheckpoints()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabase( _storeDir );
			  GenerateTransactionsAndRotate( database, 4, true );
			  database.Shutdown();

			  sbyte[] trimSizes = new sbyte[]{ 4, 22 };
			  int trimSize = 0;
			  while ( _logFiles.HighestLogVersion > 0 )
			  {
					sbyte bytesToTrim = trimSizes[trimSize++ % trimSizes.Length];
					TruncateBytesFromLastLogFile( bytesToTrim );
					_databaseFactory.newEmbeddedDatabase( _storeDir ).shutdown();
					int numberOfRecoveredTransactions = _recoveryMonitor.NumberOfRecoveredTransactions;
					assertThat( numberOfRecoveredTransactions, Matchers.greaterThanOrEqualTo( 0 ) );
			  }

			  File corruptedLogArchives = new File( _storeDir, CorruptedLogsTruncator.CORRUPTED_TX_LOGS_BASE_NAME );
			  assertThat( corruptedLogArchives.listFiles(), not(emptyArray()) );
		 }

		 private static TransactionIdStore GetTransactionIdStore( GraphDatabaseAPI database )
		 {
			  return database.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void removeLastCheckpointRecordFromLastLogFile() throws java.io.IOException
		 private void RemoveLastCheckpointRecordFromLastLogFile()
		 {
			  LogPosition checkpointPosition = null;

			  LogFile transactionLogFile = _logFiles.LogFile;
			  VersionAwareLogEntryReader<ReadableLogChannel> entryReader = new VersionAwareLogEntryReader<ReadableLogChannel>();
			  LogPosition startPosition = LogPosition.start( _logFiles.HighestLogVersion );
			  using ( ReadableLogChannel reader = transactionLogFile.GetReader( startPosition ) )
			  {
					LogEntry logEntry;
					do
					{
						 logEntry = entryReader.ReadLogEntry( reader );
						 if ( logEntry is CheckPoint )
						 {
							  checkpointPosition = ( ( CheckPoint ) logEntry ).LogPosition;
						 }
					} while ( logEntry != null );
			  }
			  if ( checkpointPosition != null )
			  {
					using ( StoreChannel storeChannel = _fileSystemRule.open( _logFiles.HighestLogFile, OpenMode.READ_WRITE ) )
					{
						 storeChannel.Truncate( checkpointPosition.ByteOffset );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void truncateBytesFromLastLogFile(long bytesToTrim) throws java.io.IOException
		 private void TruncateBytesFromLastLogFile( long bytesToTrim )
		 {
			  File highestLogFile = _logFiles.HighestLogFile;
			  long fileSize = _fileSystemRule.getFileSize( highestLogFile );
			  if ( bytesToTrim > fileSize )
			  {
					_fileSystemRule.deleteFile( highestLogFile );
			  }
			  else
			  {
					_fileSystemRule.truncate( highestLogFile, fileSize - bytesToTrim );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addRandomBytesToLastLogFile(System.Func<sbyte> byteSource) throws java.io.IOException
		 private void AddRandomBytesToLastLogFile( System.Func<sbyte> byteSource )
		 {
			  using ( Lifespan lifespan = new Lifespan() )
			  {
					LogFile transactionLogFile = _logFiles.LogFile;
					lifespan.Add( _logFiles );

					FlushablePositionAwareChannel logFileWriter = transactionLogFile.Writer;
					for ( int i = 0; i < 10; i++ )
					{
						 logFileWriter.Put( byteSource() );
					}
			  }
		 }

		 private sbyte RandomPositiveBytes()
		 {
			  return ( sbyte ) _random.Next( 0, sbyte.MaxValue );
		 }

		 private sbyte RandomBytes()
		 {
			  return ( sbyte ) _random.Next( sbyte.MinValue, sbyte.MaxValue );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addCorruptedCommandsToLastLogFile() throws java.io.IOException
		 private void AddCorruptedCommandsToLastLogFile()
		 {
			  PositiveLogFilesBasedLogVersionRepository versionRepository = new PositiveLogFilesBasedLogVersionRepository( _logFiles );
			  LogFiles internalLogFiles = LogFilesBuilder.builder( _directory.databaseLayout(), _fileSystemRule ).withLogVersionRepository(versionRepository).withTransactionIdStore(new SimpleTransactionIdStore()).build();
			  using ( Lifespan lifespan = new Lifespan( internalLogFiles ) )
			  {
					LogFile transactionLogFile = internalLogFiles.LogFile;

					FlushablePositionAwareChannel channel = transactionLogFile.Writer;
					TransactionLogWriter writer = new TransactionLogWriter( new CorruptedLogEntryWriter( channel ) );

					ICollection<StorageCommand> commands = new List<StorageCommand>();
					commands.Add( new Command.PropertyCommand( new PropertyRecord( 1 ), new PropertyRecord( 2 ) ) );
					commands.Add( new Command.NodeCommand( new NodeRecord( 2 ), new NodeRecord( 3 ) ) );
					PhysicalTransactionRepresentation transaction = new PhysicalTransactionRepresentation( commands );
					writer.Append( transaction, 1000 );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.eclipse.collections.api.map.primitive.ObjectLongMap<Class> getLogEntriesDistribution(org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles) throws java.io.IOException
		 private static ObjectLongMap<Type> GetLogEntriesDistribution( LogFiles logFiles )
		 {
			  LogFile transactionLogFile = logFiles.LogFile;

			  LogPosition fileStartPosition = new LogPosition( 0, LogHeader.LOG_HEADER_SIZE );
			  VersionAwareLogEntryReader<ReadableLogChannel> entryReader = new VersionAwareLogEntryReader<ReadableLogChannel>();

			  MutableObjectLongMap<Type> multiset = new ObjectLongHashMap<Type>();
			  using ( ReadableLogChannel fileReader = transactionLogFile.GetReader( fileStartPosition ) )
			  {
					LogEntry logEntry = entryReader.ReadLogEntry( fileReader );
					while ( logEntry != null )
					{
						 multiset.addToValue( logEntry.GetType(), 1 );
						 logEntry = entryReader.ReadLogEntry( fileReader );
					}
			  }
			  return multiset;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.log.files.LogFiles buildDefaultLogFiles() throws java.io.IOException
		 private LogFiles BuildDefaultLogFiles()
		 {
			  return LogFilesBuilder.builder( _directory.databaseLayout(), _fileSystemRule ).withLogVersionRepository(new SimpleLogVersionRepository()).withTransactionIdStore(new SimpleTransactionIdStore()).build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void generateTransactionsAndRotateWithCheckpoint(org.neo4j.kernel.internal.GraphDatabaseAPI database, int logFilesToGenerate) throws java.io.IOException
		 private static void GenerateTransactionsAndRotateWithCheckpoint( GraphDatabaseAPI database, int logFilesToGenerate )
		 {
			  GenerateTransactionsAndRotate( database, logFilesToGenerate, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void generateTransactionsAndRotate(org.neo4j.kernel.internal.GraphDatabaseAPI database, int logFilesToGenerate) throws java.io.IOException
		 private static void GenerateTransactionsAndRotate( GraphDatabaseAPI database, int logFilesToGenerate )
		 {
			  GenerateTransactionsAndRotate( database, logFilesToGenerate, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void generateTransactionsAndRotate(org.neo4j.kernel.internal.GraphDatabaseAPI database, int logFilesToGenerate, boolean checkpoint) throws java.io.IOException
		 private static void GenerateTransactionsAndRotate( GraphDatabaseAPI database, int logFilesToGenerate, bool checkpoint )
		 {
			  DependencyResolver resolver = database.DependencyResolver;
			  LogFiles logFiles = resolver.ResolveDependency( typeof( TransactionLogFiles ) );
			  CheckPointer checkpointer = resolver.ResolveDependency( typeof( CheckPointer ) );
			  while ( logFiles.HighestLogVersion < logFilesToGenerate )
			  {
					logFiles.LogFile.rotate();
					GenerateTransaction( database );
					if ( checkpoint )
					{
						 checkpointer.ForceCheckPoint( new SimpleTriggerInfo( "testForcedCheckpoint" ) );
					}
			  }
		 }

		 private static void GenerateTransaction( GraphDatabaseAPI database )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node startNode = database.CreateNode( Label.label( "startNode" ) );
					startNode.SetProperty( "key", "value" );
					Node endNode = database.CreateNode( Label.label( "endNode" ) );
					endNode.SetProperty( "key", "value" );
					startNode.CreateRelationshipTo( endNode, RelationshipType.withName( "connects" ) );
					transaction.Success();
			  }
		 }

		 private GraphDatabaseAPI StartDbNoRecoveryOfCorruptedLogs()
		 {
			  return ( GraphDatabaseAPI ) _databaseFactory.newEmbeddedDatabaseBuilder( _storeDir ).setConfig( GraphDatabaseSettings.fail_on_corrupted_log_files, Settings.FALSE ).newGraphDatabase();
		 }

		 private class CorruptedLogEntryWriter : LogEntryWriter
		 {

			  internal CorruptedLogEntryWriter( FlushableChannel channel ) : base( channel )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeStartEntry(int masterId, int authorId, long timeWritten, long latestCommittedTxWhenStarted, byte[] additionalHeaderData) throws java.io.IOException
			  public override void WriteStartEntry( int masterId, int authorId, long timeWritten, long latestCommittedTxWhenStarted, sbyte[] additionalHeaderData )
			  {
					WriteLogEntryHeader( TX_START, Channel );
			  }
		 }

		 private class RecoveryMonitor : Neo4Net.Kernel.recovery.RecoveryMonitor
		 {
			  internal IList<long> RecoveredTransactions = new List<long>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int NumberOfRecoveredTransactionsConflict;

			  public override void RecoveryRequired( LogPosition recoveryPosition )
			  {
			  }

			  public override void TransactionRecovered( long txId )
			  {
					RecoveredTransactions.Add( txId );
			  }

			  public override void RecoveryCompleted( int numberOfRecoveredTransactions )
			  {
					this.NumberOfRecoveredTransactionsConflict = numberOfRecoveredTransactions;
			  }

			  internal virtual int NumberOfRecoveredTransactions
			  {
				  get
				  {
						return NumberOfRecoveredTransactionsConflict;
				  }
			  }
		 }

		 private class PositiveLogFilesBasedLogVersionRepository : LogVersionRepository
		 {

			  internal long Version;

			  internal PositiveLogFilesBasedLogVersionRepository( LogFiles logFiles )
			  {
					this.Version = ( logFiles.HighestLogVersion == -1 ) ? 0 : logFiles.HighestLogVersion;
			  }

			  public virtual long CurrentLogVersion
			  {
				  get
				  {
						return Version;
				  }
				  set
				  {
						this.Version = value;
				  }
			  }


			  public override long IncrementAndGetVersion()
			  {
					Version++;
					return Version;
			  }
		 }
	}

}