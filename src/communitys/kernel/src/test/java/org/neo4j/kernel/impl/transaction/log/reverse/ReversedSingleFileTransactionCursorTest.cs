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
namespace Neo4Net.Kernel.impl.transaction.log.reverse
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using UnsupportedLogVersionException = Neo4Net.Kernel.impl.transaction.log.entry.UnsupportedLogVersionException;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_LABELS_FIELD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.GivenTransactionCursor.exhaust;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogPosition.start;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogVersionBridge_Fields.NO_MORE_CHANNELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_START;

	public class ReversedSingleFileTransactionCursorTest
	{
		private bool InstanceFieldsInitialized = false;

		public ReversedSingleFileTransactionCursorTest()
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
			_monitor = new ReverseTransactionCursorLoggingMonitor( _logProvider.getLog( typeof( ReversedSingleFileTransactionCursor ) ) );
		}

		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly RandomRule _random = new RandomRule();
		 private readonly ExpectedException _expectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(random).around(fs).around(directory).around(life).around(expectedException);
		 public RuleChain Rules;

		 private long _txId = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
		 private LogProvider _logProvider = new AssertableLogProvider( true );
		 private ReverseTransactionCursorLoggingMonitor _monitor;
		 private LogFile _logFile;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  LogVersionRepository logVersionRepository = new SimpleLogVersionRepository();
			  SimpleTransactionIdStore transactionIdStore = new SimpleTransactionIdStore();
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), _fs ).withLogVersionRepository(logVersionRepository).withTransactionIdStore(transactionIdStore).build();
			  _life.add( logFiles );
			  _logFile = logFiles.LogFile;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleVerySmallTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleVerySmallTransactions()
		 {
			  // given
			  WriteTransactions( 10, 1, 1 );

			  // when
			  CommittedTransactionRepresentation[] readTransactions = ReadAllFromReversedCursor();

			  // then
			  AssertTransactionRange( readTransactions, _txId, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleManyVerySmallTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleManyVerySmallTransactions()
		 {
			  // given
			  WriteTransactions( 20_000, 1, 1 );

			  // when
			  CommittedTransactionRepresentation[] readTransactions = ReadAllFromReversedCursor();

			  // then
			  AssertTransactionRange( readTransactions, _txId, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLargeTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleLargeTransactions()
		 {
			  // given
			  WriteTransactions( 10, 1000, 1000 );

			  // when
			  CommittedTransactionRepresentation[] readTransactions = ReadAllFromReversedCursor();

			  // then
			  AssertTransactionRange( readTransactions, _txId, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptyLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleEmptyLog()
		 {
			  // given

			  // when
			  CommittedTransactionRepresentation[] readTransactions = ReadAllFromReversedCursor();

			  // then
			  assertEquals( 0, readTransactions.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectAndPreventChannelReadingMultipleLogVersions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectAndPreventChannelReadingMultipleLogVersions()
		 {
			  // given
			  WriteTransactions( 1, 1, 1 );
			  _logFile.rotate();
			  WriteTransactions( 1, 1, 1 );

			  // when
			  try
			  {
					  using ( ReadAheadLogChannel channel = ( ReadAheadLogChannel ) _logFile.getReader( start( 0 ) ) )
					  {
						new ReversedSingleFileTransactionCursor( channel, new VersionAwareLogEntryReader<Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>(), false, _monitor );
						fail( "Should've failed" );
					  }
			  }
			  catch ( System.ArgumentException e )
			  {
					// then good
					assertThat( e.Message, containsString( "multiple log versions" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readCorruptedTransactionLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadCorruptedTransactionLog()
		 {
			  int readableTransactions = 10;
			  WriteTransactions( readableTransactions, 1, 1 );
			  AppendCorruptedTransaction();
			  WriteTransactions( readableTransactions, 1, 1 );
			  CommittedTransactionRepresentation[] committedTransactionRepresentations = ReadAllFromReversedCursor();
			  AssertTransactionRange( committedTransactionRepresentations, readableTransactions + Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failToReadCorruptedTransactionLogWhenConfigured() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailToReadCorruptedTransactionLogWhenConfigured()
		 {
			  int readableTransactions = 10;
			  WriteTransactions( readableTransactions, 1, 1 );
			  AppendCorruptedTransaction();
			  WriteTransactions( readableTransactions, 1, 1 );

			  _expectedException.expect( typeof( UnsupportedLogVersionException ) );

			  ReadAllFromReversedCursorFailOnCorrupted();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation[] readAllFromReversedCursor() throws java.io.IOException
		 private CommittedTransactionRepresentation[] ReadAllFromReversedCursor()
		 {
			  using ( ReversedSingleFileTransactionCursor cursor = TxCursor( false ) )
			  {
					return exhaust( cursor );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation[] readAllFromReversedCursorFailOnCorrupted() throws java.io.IOException
		 private CommittedTransactionRepresentation[] ReadAllFromReversedCursorFailOnCorrupted()
		 {
			  using ( ReversedSingleFileTransactionCursor cursor = TxCursor( true ) )
			  {
					return exhaust( cursor );
			  }
		 }

		 private void AssertTransactionRange( CommittedTransactionRepresentation[] readTransactions, long highTxId, long lowTxId )
		 {
			  long expectedTxId = highTxId;
			  foreach ( CommittedTransactionRepresentation tx in readTransactions )
			  {
					assertEquals( expectedTxId, tx.CommitEntry.TxId );
					expectedTxId--;
			  }
			  assertEquals( expectedTxId, lowTxId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ReversedSingleFileTransactionCursor txCursor(boolean failOnCorruptedLogFiles) throws java.io.IOException
		 private ReversedSingleFileTransactionCursor TxCursor( bool failOnCorruptedLogFiles )
		 {
			  ReadAheadLogChannel fileReader = ( ReadAheadLogChannel ) _logFile.getReader( start( 0 ), NO_MORE_CHANNELS );
			  try
			  {
					return new ReversedSingleFileTransactionCursor( fileReader, new VersionAwareLogEntryReader<Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>(), failOnCorruptedLogFiles, _monitor );
			  }
			  catch ( UnsupportedLogVersionException e )
			  {
					fileReader.Dispose();
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeTransactions(int transactionCount, int minTransactionSize, int maxTransactionSize) throws java.io.IOException
		 private void WriteTransactions( int transactionCount, int minTransactionSize, int maxTransactionSize )
		 {
			  FlushablePositionAwareChannel channel = _logFile.Writer;
			  TransactionLogWriter writer = new TransactionLogWriter( new LogEntryWriter( channel ) );
			  for ( int i = 0; i < transactionCount; i++ )
			  {
					writer.Append( Tx( _random.intBetween( minTransactionSize, maxTransactionSize ) ), ++_txId );
			  }
			  channel.PrepareForFlush().flush();
			  // Don't close the channel, LogFile owns it
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void appendCorruptedTransaction() throws java.io.IOException
		 private void AppendCorruptedTransaction()
		 {
			  FlushablePositionAwareChannel channel = _logFile.Writer;
			  TransactionLogWriter writer = new TransactionLogWriter( new CorruptedLogEntryWriter( channel ) );
			  writer.Append( Tx( _random.intBetween( 100, 1000 ) ), ++_txId );
		 }

		 private TransactionRepresentation Tx( int size )
		 {
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  for ( int i = 0; i < size; i++ )
			  {
					// The type of command doesn't matter here
					commands.Add( new Command.NodeCommand( new NodeRecord( i ), ( new NodeRecord( i ) ).initialize( true, i, false, i, NO_LABELS_FIELD.longValue() ) ) );
			  }
			  PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( commands );
			  tx.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return tx;
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
	}

}