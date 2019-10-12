using System.Collections.Generic;

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
namespace Org.Neo4j.tools.dump
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using PrimitiveLongArrayQueue = Org.Neo4j.Collection.PrimitiveLongArrayQueue;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using SimpleLogVersionRepository = Org.Neo4j.Kernel.impl.transaction.SimpleLogVersionRepository;
	using SimpleTransactionIdStore = Org.Neo4j.Kernel.impl.transaction.SimpleTransactionIdStore;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using FlushablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.FlushablePositionAwareChannel;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using LogPositionMarker = Org.Neo4j.Kernel.impl.transaction.log.LogPositionMarker;
	using LogVersionRepository = Org.Neo4j.Kernel.impl.transaction.log.LogVersionRepository;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using TransactionLogWriter = Org.Neo4j.Kernel.impl.transaction.log.TransactionLogWriter;
	using CheckPoint = Org.Neo4j.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;
	using LogEntryWriter = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogFile = Org.Neo4j.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using Org.Neo4j.Test.rule.fs;
	using Monitor = Org.Neo4j.tools.dump.TransactionLogAnalyzer.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_LOG_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.InvalidLogEntryHandler.STRICT;

	public class TransactionLogAnalyzerTest
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionLogAnalyzerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( _fs );
			Rules = RuleChain.outerRule( _random ).around( _fs ).around( _directory ).around( _life ).around( _expectedException );
		}

		 private readonly FileSystemRule<DefaultFileSystemAbstraction> _fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly RandomRule _random = new RandomRule();
		 private readonly ExpectedException _expectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(random).around(fs).around(directory).around(life).around(expectedException);
		 public RuleChain Rules;

		 private LogFile _logFile;
		 private FlushablePositionAwareChannel _writer;
		 private TransactionLogWriter _transactionLogWriter;
		 private AtomicLong _lastCommittedTxId;
		 private VerifyingMonitor _monitor;
		 private LogVersionRepository _logVersionRepository;
		 private LogFiles _logFiles;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _lastCommittedTxId = new AtomicLong( BASE_TX_ID );
			  _logVersionRepository = new SimpleLogVersionRepository();
			  _logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), _fs ).withLogVersionRepository(_logVersionRepository).withTransactionIdStore(new SimpleTransactionIdStore()).build();
			  _life.add( _logFiles );
			  _logFile = _logFiles.LogFile;
			  _writer = _logFile.Writer;
			  _transactionLogWriter = new TransactionLogWriter( new LogEntryWriter( _writer ) );
			  _monitor = new VerifyingMonitor();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _life.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeTransactionsInOneLogFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeTransactionsInOneLogFile()
		 {
			  // given
			  WriteTransactions( 5 );

			  // when
			  TransactionLogAnalyzer.Analyze( _fs, _directory.databaseDir(), STRICT, _monitor );

			  // then
			  assertEquals( 1, _monitor.logFiles );
			  assertEquals( 5, _monitor.transactions );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwExceptionWithErrorMessageIfLogFilesNotFound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowExceptionWithErrorMessageIfLogFilesNotFound()
		 {
			  File emptyDirectory = _directory.directory( "empty" );
			  _expectedException.expect( typeof( System.InvalidOperationException ) );
			  _expectedException.expectMessage( "not found." );
			  TransactionLogAnalyzer.Analyze( _fs, emptyDirectory, STRICT, _monitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeCheckpointsInBetweenTransactionsInOneLogFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeCheckpointsInBetweenTransactionsInOneLogFile()
		 {
			  // given
			  WriteTransactions( 3 ); // txs 2, 3, 4
			  WriteCheckpoint();
			  WriteTransactions( 2 ); // txs 5, 6
			  WriteCheckpoint();
			  WriteTransactions( 4 ); // txs 7, 8, 9, 10

			  // when
			  TransactionLogAnalyzer.Analyze( _fs, _directory.databaseDir(), STRICT, _monitor );

			  // then
			  assertEquals( 1, _monitor.logFiles );
			  assertEquals( 2, _monitor.checkpoints );
			  assertEquals( 9, _monitor.transactions );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeLogFileTransitions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeLogFileTransitions()
		 {
			  // given
			  WriteTransactions( 1 );
			  Rotate();
			  WriteTransactions( 1 );
			  Rotate();
			  WriteTransactions( 1 );

			  // when
			  TransactionLogAnalyzer.Analyze( _fs, _directory.databaseDir(), STRICT, _monitor );

			  // then
			  assertEquals( 3, _monitor.logFiles );
			  assertEquals( 0, _monitor.checkpoints );
			  assertEquals( 3, _monitor.transactions );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeLogFileTransitionsTransactionsAndCheckpointsInMultipleLogFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeLogFileTransitionsTransactionsAndCheckpointsInMultipleLogFiles()
		 {
			  // given
			  int expectedTransactions = 0;
			  int expectedCheckpoints = 0;
			  int expectedLogFiles = 1;
			  for ( int i = 0; i < 30; i++ )
			  {
					float chance = _random.nextFloat();
					if ( chance < 0.5 )
					{ // tx
						 int count = _random.Next( 1, 5 );
						 WriteTransactions( count );
						 expectedTransactions += count;
					}
					else if ( chance < 0.75 )
					{ // checkpoint
						 WriteCheckpoint();
						 expectedCheckpoints++;
					}
					else
					{ // rotate
						 Rotate();
						 expectedLogFiles++;
					}
			  }
			  _writer.prepareForFlush().flush();

			  // when
			  TransactionLogAnalyzer.Analyze( _fs, _directory.databaseDir(), STRICT, _monitor );

			  // then
			  assertEquals( expectedLogFiles, _monitor.logFiles );
			  assertEquals( expectedCheckpoints, _monitor.checkpoints );
			  assertEquals( expectedTransactions, _monitor.transactions );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAnalyzeSingleLogWhenExplicitlySelected() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAnalyzeSingleLogWhenExplicitlySelected()
		 {
			  // given
			  WriteTransactions( 2 ); // txs 2, 3
			  long version = Rotate();
			  WriteTransactions( 3 ); // txs 4, 5, 6
			  WriteCheckpoint();
			  WriteTransactions( 4 ); // txs 7, 8, 9, 10
			  Rotate();
			  WriteTransactions( 2 ); // txs 11, 12

			  // when
			  _monitor.nextExpectedTxId = 4;
			  _monitor.nextExpectedLogVersion = version;
			  TransactionLogAnalyzer.Analyze( _fs, _logFiles.getLogFileForVersion( version ), STRICT, _monitor );

			  // then
			  assertEquals( 1, _monitor.logFiles );
			  assertEquals( 1, _monitor.checkpoints );
			  assertEquals( 7, _monitor.transactions );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long rotate() throws java.io.IOException
		 private long Rotate()
		 {
			  _logFile.rotate();
			  return _logVersionRepository.CurrentLogVersion;
		 }

		 private static void AssertTransaction( LogEntry[] transactionEntries, long expectedId )
		 {
			  assertTrue( Arrays.ToString( transactionEntries ), transactionEntries[0] is LogEntryStart );
			  assertTrue( transactionEntries[1] is LogEntryCommand );
			  LogEntryCommand command = transactionEntries[1].As();
			  assertEquals( expectedId, ( ( Command.NodeCommand )command.Command ).Key );
			  assertTrue( transactionEntries[2] is LogEntryCommit );
			  LogEntryCommit commit = transactionEntries[2].As();
			  assertEquals( expectedId, commit.TxId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeCheckpoint() throws java.io.IOException
		 private void WriteCheckpoint()
		 {
			  _transactionLogWriter.checkPoint( _writer.getCurrentPosition( new LogPositionMarker() ).newPosition() );
			  _monitor.expectCheckpointAfter( _lastCommittedTxId.get() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeTransactions(int count) throws java.io.IOException
		 private void WriteTransactions( int count )
		 {
			  for ( int i = 0; i < count; i++ )
			  {
					long id = _lastCommittedTxId.incrementAndGet();
					_transactionLogWriter.append( Tx( id ), id );
			  }
			  _writer.prepareForFlush().flush();
		 }

		 private TransactionRepresentation Tx( long nodeId )
		 {
			  IList<StorageCommand> commands = new List<StorageCommand>();
			  commands.add(new Command.NodeCommand(new NodeRecord(nodeId), new NodeRecord(nodeId)
						 .initialize( true, nodeId, false, nodeId, 0 )));
			  PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( commands );
			  tx.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return tx;
		 }

		 private class VerifyingMonitor : Monitor
		 {
			  internal int Transactions;
			  internal int Checkpoints;
			  internal int LogFiles;
			  internal long NextExpectedTxId = BASE_TX_ID + 1;
			  internal readonly PrimitiveLongArrayQueue ExpectedCheckpointsAt = new PrimitiveLongArrayQueue();
			  internal long NextExpectedLogVersion = BASE_TX_LOG_VERSION;

			  internal virtual void ExpectCheckpointAfter( long txId )
			  {
					ExpectedCheckpointsAt.enqueue( txId );
			  }

			  public override void LogFile( File file, long logVersion )
			  {
					LogFiles++;
					assertEquals( NextExpectedLogVersion++, logVersion );
			  }

			  public override void Transaction( LogEntry[] transactionEntries )
			  {
					Transactions++;
					AssertTransaction( transactionEntries, NextExpectedTxId++ );
			  }

			  public override void Checkpoint( CheckPoint checkpoint, LogPosition checkpointEntryPosition )
			  {
					Checkpoints++;
					long? expected = ExpectedCheckpointsAt.dequeue();
					assertNotNull( "Unexpected checkpoint", expected );
					assertEquals( expected.Value, NextExpectedTxId - 1 );
			  }
		 }
	}

}