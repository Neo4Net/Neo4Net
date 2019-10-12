using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.recovery
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using SimpleLogVersionRepository = Neo4Net.Kernel.impl.transaction.SimpleLogVersionRepository;
	using FlushablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.FlushablePositionAwareChannel;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogPositionMarker = Neo4Net.Kernel.impl.transaction.log.LogPositionMarker;
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using LogEntryVersion = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogTailInformation = Neo4Net.Kernel.recovery.LogTailScanner.LogTailInformation;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.recovery.LogTailScanner.NO_TRANSACTION_ID;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class LogTailScannerTest
	public class LogTailScannerTest
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fsRule );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _testDirectory ).around( _pageCacheRule );
		}

		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(testDirectory).around(pageCacheRule);
		 public RuleChain RuleChain;

		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
		 private LogTailScanner _tailScanner;

		 private readonly Monitors _monitors = new Monitors();
		 private LogFiles _logFiles;
		 private readonly int _startLogVersion;
		 private readonly int _endLogVersion;
		 private readonly LogEntryVersion _latestLogEntryVersion = LogEntryVersion.CURRENT;
		 private LogVersionRepository _logVersionRepository;

		 public LogTailScannerTest( int? startLogVersion, int? endLogVersion )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._startLogVersion = startLogVersion.Value;
			  this._endLogVersion = endLogVersion.Value;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0},{1}") public static java.util.Collection<Object[]> params()
		 public static ICollection<object[]> Params()
		 {
			  return Arrays.asList( new object[]{ 1, 2 }, new object[]{ 42, 43 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _logVersionRepository = new SimpleLogVersionRepository();
			  _logFiles = LogFilesBuilder.activeFilesBuilder( _testDirectory.databaseLayout(), _fsRule, _pageCacheRule.getPageCache(_fsRule) ).withLogVersionRepository(_logVersionRepository).build();
			  _tailScanner = new LogTailScanner( _logFiles, _reader, _monitors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noLogFilesFound()
		 public virtual void NoLogFilesFound()
		 {
			  // given no files
			  SetupLogFiles();

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( false, false, NO_TRANSACTION_ID, -1, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneLogFileNoCheckPoints()
		 public virtual void OneLogFileNoCheckPoints()
		 {
			  // given
			  SetupLogFiles( LogFile() );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( false, true, NO_TRANSACTION_ID, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneLogFileNoCheckPointsOneStart()
		 public virtual void OneLogFileNoCheckPointsOneStart()
		 {
			  // given
			  long txId = 10;
			  SetupLogFiles( LogFile( Start(), Commit(txId) ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( false, true, txId, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoLogFilesNoCheckPoints()
		 public virtual void TwoLogFilesNoCheckPoints()
		 {
			  // given
			  SetupLogFiles( LogFile(), LogFile() );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( false, true, NO_TRANSACTION_ID, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoLogFilesNoCheckPointsOneStart()
		 public virtual void TwoLogFilesNoCheckPointsOneStart()
		 {
			  // given
			  long txId = 21;
			  SetupLogFiles( LogFile(), LogFile(Start(), Commit(txId)) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( false, true, txId, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoLogFilesNoCheckPointsOneStartWithoutCommit()
		 public virtual void TwoLogFilesNoCheckPointsOneStartWithoutCommit()
		 {
			  // given
			  SetupLogFiles( LogFile(), LogFile(Start()) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( false, true, NO_TRANSACTION_ID, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoLogFilesNoCheckPointsTwoCommits()
		 public virtual void TwoLogFilesNoCheckPointsTwoCommits()
		 {
			  // given
			  long txId = 21;
			  SetupLogFiles( LogFile(), LogFile(Start(), Commit(txId), Start(), Commit(txId + 1)) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( false, true, txId, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoLogFilesCheckPointTargetsPrevious()
		 public virtual void TwoLogFilesCheckPointTargetsPrevious()
		 {
			  // given
			  long txId = 6;
			  PositionEntry position = position();
			  SetupLogFiles( LogFile( Start(), Commit(txId - 1), position ), LogFile(Start(), Commit(txId)), LogFile(CheckPoint(position)) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, txId, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoLogFilesStartAndCommitInDifferentFiles()
		 public virtual void TwoLogFilesStartAndCommitInDifferentFiles()
		 {
			  // given
			  long txId = 6;
			  SetupLogFiles( LogFile( Start() ), LogFile(Commit(txId)) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( false, true, 6, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latestLogFileContainingACheckPointOnly()
		 public virtual void LatestLogFileContainingACheckPointOnly()
		 {
			  // given
			  SetupLogFiles( LogFile( CheckPoint() ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, false, NO_TRANSACTION_ID, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latestLogFileContainingACheckPointAndAStartBefore()
		 public virtual void LatestLogFileContainingACheckPointAndAStartBefore()
		 {
			  // given
			  SetupLogFiles( LogFile( Start(), CheckPoint() ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, false, NO_TRANSACTION_ID, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void bigFileLatestCheckpointFindsStartAfter() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BigFileLatestCheckpointFindsStartAfter()
		 {
			  long firstTxAfterCheckpoint = int.MaxValue + 4L;

			  LogTailScanner tailScanner = new FirstTxIdConfigurableTailScanner( firstTxAfterCheckpoint, _logFiles, _reader, _monitors );
			  LogEntryStart startEntry = new LogEntryStart( 1, 2, 3L, 4L, new sbyte[]{ 5, 6 }, new LogPosition( _endLogVersion, int.MaxValue + 17L ) );
			  CheckPoint checkPoint = new CheckPoint( new LogPosition( _endLogVersion, 16L ) );
			  LogTailInformation logTailInformation = tailScanner.CheckpointTailInformation( _endLogVersion, startEntry, _endLogVersion, _latestLogEntryVersion, checkPoint, false );

			  AssertLatestCheckPoint( true, true, firstTxAfterCheckpoint, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoLogFilesSecondIsCorruptedBeforeCommit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TwoLogFilesSecondIsCorruptedBeforeCommit()
		 {
			  SetupLogFiles( LogFile( CheckPoint() ), LogFile(Start(), Commit(2)) );

			  File highestLogFile = _logFiles.HighestLogFile;
			  _fsRule.truncate( highestLogFile, _fsRule.getFileSize( highestLogFile ) - 3 );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, NO_TRANSACTION_ID, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoLogFilesSecondIsCorruptedBeforeAfterCommit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TwoLogFilesSecondIsCorruptedBeforeAfterCommit()
		 {
			  int firstTxId = 2;
			  SetupLogFiles( LogFile( CheckPoint() ), LogFile(Start(), Commit(firstTxId), Start(), Commit(3)) );

			  File highestLogFile = _logFiles.HighestLogFile;
			  _fsRule.truncate( highestLogFile, _fsRule.getFileSize( highestLogFile ) - 3 );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, firstTxId, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latestLogFileContainingACheckPointAndAStartAfter()
		 public virtual void LatestLogFileContainingACheckPointAndAStartAfter()
		 {
			  // given
			  long txId = 35;
			  StartEntry start = start();
			  SetupLogFiles( LogFile( start, Commit( txId ), CheckPoint( start ) ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, txId, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latestLogFileContainingACheckPointAndAStartWithoutCommitAfter()
		 public virtual void LatestLogFileContainingACheckPointAndAStartWithoutCommitAfter()
		 {
			  // given
			  StartEntry start = start();
			  SetupLogFiles( LogFile( start, CheckPoint( start ) ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, NO_TRANSACTION_ID, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latestLogFileContainingMultipleCheckPointsOneStartInBetween()
		 public virtual void LatestLogFileContainingMultipleCheckPointsOneStartInBetween()
		 {
			  // given
			  SetupLogFiles( LogFile( CheckPoint(), Start(), CheckPoint() ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, false, NO_TRANSACTION_ID, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latestLogFileContainingMultipleCheckPointsOneStartAfterBoth()
		 public virtual void LatestLogFileContainingMultipleCheckPointsOneStartAfterBoth()
		 {
			  // given
			  long txId = 11;
			  SetupLogFiles( LogFile( CheckPoint(), CheckPoint(), Start(), Commit(txId) ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, txId, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void olderLogFileContainingACheckPointAndNewerFileContainingAStart()
		 public virtual void OlderLogFileContainingACheckPointAndNewerFileContainingAStart()
		 {
			  // given
			  long txId = 11;
			  StartEntry start = start();
			  SetupLogFiles( LogFile( CheckPoint() ), LogFile(start, Commit(txId)) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, txId, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void olderLogFileContainingACheckPointAndNewerFileIsEmpty()
		 public virtual void OlderLogFileContainingACheckPointAndNewerFileIsEmpty()
		 {
			  // given
			  StartEntry start = start();
			  SetupLogFiles( LogFile( start, CheckPoint() ), LogFile() );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, NO_TRANSACTION_ID, _startLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void olderLogFileContainingAStartAndNewerFileContainingACheckPointPointingToAPreviousPositionThanStart()
		 public virtual void OlderLogFileContainingAStartAndNewerFileContainingACheckPointPointingToAPreviousPositionThanStart()
		 {
			  // given
			  long txId = 123;
			  StartEntry start = start();
			  SetupLogFiles( LogFile( start, Commit( txId ) ), LogFile( CheckPoint( start ) ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, txId, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void olderLogFileContainingAStartAndNewerFileContainingACheckPointPointingToAPreviousPositionThanStartWithoutCommit()
		 public virtual void OlderLogFileContainingAStartAndNewerFileContainingACheckPointPointingToAPreviousPositionThanStartWithoutCommit()
		 {
			  // given
			  StartEntry start = start();
			  SetupLogFiles( LogFile( start ), LogFile( CheckPoint( start ) ) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, false, NO_TRANSACTION_ID, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void olderLogFileContainingAStartAndNewerFileContainingACheckPointPointingToALaterPositionThanStart()
		 public virtual void OlderLogFileContainingAStartAndNewerFileContainingACheckPointPointingToALaterPositionThanStart()
		 {
			  // given
			  PositionEntry position = position();
			  SetupLogFiles( LogFile( Start(), Commit(3), position ), LogFile(CheckPoint(position)) );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, false, NO_TRANSACTION_ID, _endLogVersion, logTailInformation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latestLogEmptyStartEntryBeforeAndAfterCheckPointInTheLastButOneLog()
		 public virtual void LatestLogEmptyStartEntryBeforeAndAfterCheckPointInTheLastButOneLog()
		 {
			  // given
			  long txId = 432;
			  SetupLogFiles( LogFile( Start(), CheckPoint(), Start(), Commit(txId) ), LogFile() );

			  // when
			  LogTailInformation logTailInformation = _tailScanner.TailInformation;

			  // then
			  AssertLatestCheckPoint( true, true, txId, _startLogVersion, logTailInformation );
		 }

		 // === Below is code for helping the tests above ===

		 private void SetupLogFiles( params LogCreator[] logFiles )
		 {
			  IDictionary<Entry, LogPosition> positions = new Dictionary<Entry, LogPosition>();
			  long version = _endLogVersion - logFiles.Length;
			  foreach ( LogCreator logFile in logFiles )
			  {
					logFile.Create( ++version, positions );
			  }
		 }

		 private LogCreator LogFile( params Entry[] entries )
		 {
			  return ( logVersion, positions ) =>
			  {
				try
				{
					 AtomicLong lastTxId = new AtomicLong();
					 _logVersionRepository.CurrentLogVersion = logVersion;
					 LifeSupport logFileLife = new LifeSupport();
					 logFileLife.start();
					 logFileLife.add( _logFiles );
					 LogFile logFile = _logFiles.LogFile;
					 try
					 {
						  FlushablePositionAwareChannel writeChannel = logFile.Writer;
						  LogPositionMarker positionMarker = new LogPositionMarker();
						  LogEntryWriter writer = new LogEntryWriter( writeChannel );
						  foreach ( Entry entry in entries )
						  {
								LogPosition currentPosition = writeChannel.getCurrentPosition( positionMarker ).newPosition();
								positions.put( entry, currentPosition );
								if ( entry is StartEntry )
								{
									 writer.writeStartEntry( 0, 0, 0, 0, new sbyte[0] );
								}
								else if ( entry is CommitEntry )
								{
									 CommitEntry commitEntry = ( CommitEntry ) entry;
									 writer.writeCommitEntry( commitEntry.TxId, 0 );
									 lastTxId.set( commitEntry.TxId );
								}
								else if ( entry is CheckPointEntry )
								{
									 CheckPointEntry checkPointEntry = ( CheckPointEntry ) entry;
									 Entry target = checkPointEntry.WithPositionOfEntry;
									 LogPosition logPosition = target != null ? positions.get( target ) : currentPosition;
									 Debug.Assert( logPosition != null, "No registered log position for " + target );
									 writer.writeCheckPointEntry( logPosition );
								}
								else if ( entry is PositionEntry )
								{
									 // Don't write anything, this entry is just for registering a position so that
									 // another CheckPointEntry can refer to it
								}
								else
								{
									 throw new System.ArgumentException( "Unknown entry " + entry );
								}

						  }
					 }
					 finally
					 {
						  logFileLife.shutdown();
					 }
				}
				catch ( IOException e )
				{
					 throw new UncheckedIOException( e );
				}
			  };
		 }

		 internal interface LogCreator
		 {
			  void Create( long version, IDictionary<Entry, LogPosition> positions );
		 }

		 // Marker interface, helping compilation/test creation
		 internal interface Entry
		 {
		 }

		 private static StartEntry Start()
		 {
			  return new StartEntry();
		 }

		 private static CommitEntry Commit( long txId )
		 {
			  return new CommitEntry( txId );
		 }

		 private static CheckPointEntry CheckPoint()
		 {
			  return CheckPoint( null );
		 }

		 private static CheckPointEntry CheckPoint( Entry forEntry )
		 {
			  return new CheckPointEntry( forEntry );
		 }

		 private static PositionEntry Position()
		 {
			  return new PositionEntry();
		 }

		 private class StartEntry : Entry
		 {
		 }

		 private class CommitEntry : Entry
		 {
			  internal readonly long TxId;

			  internal CommitEntry( long txId )
			  {
					this.TxId = txId;
			  }
		 }

		 private class CheckPointEntry : Entry
		 {
			  internal readonly Entry WithPositionOfEntry;

			  internal CheckPointEntry( Entry withPositionOfEntry )
			  {
					this.WithPositionOfEntry = withPositionOfEntry;
			  }
		 }

		 private class PositionEntry : Entry
		 {
		 }

		 private void AssertLatestCheckPoint( bool hasCheckPointEntry, bool commitsAfterLastCheckPoint, long firstTxIdAfterLastCheckPoint, long logVersion, LogTailInformation logTailInformation )
		 {
			  assertEquals( hasCheckPointEntry, logTailInformation.LastCheckPoint != null );
			  assertEquals( commitsAfterLastCheckPoint, logTailInformation.CommitsAfterLastCheckpoint() );
			  if ( commitsAfterLastCheckPoint )
			  {
					assertEquals( firstTxIdAfterLastCheckPoint, logTailInformation.FirstTxIdAfterLastCheckPoint );
			  }
			  assertEquals( logVersion, logTailInformation.OldestLogVersionFound );
		 }

		 private class FirstTxIdConfigurableTailScanner : LogTailScanner
		 {

			  internal readonly long TxId;

			  internal FirstTxIdConfigurableTailScanner( long txId, LogFiles logFiles, LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader, Monitors monitors ) : base( logFiles, logEntryReader, monitors )
			  {
					this.TxId = txId;
			  }

			  protected internal override ExtractedTransactionRecord ExtractFirstTxIdAfterPosition( LogPosition initialPosition, long maxLogVersion )
			  {
					return new ExtractedTransactionRecord( TxId );
			  }
		 }
	}

}