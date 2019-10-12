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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ReplicatedIdAllocationRequest = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest;
	using ReplicatedLockTokenRequest = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedTokenRequest = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using TokenType = Neo4Net.causalclustering.core.state.machines.token.TokenType;
	using ReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using CoreReplicatedContentMarshal = Neo4Net.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

	/// <summary>
	/// This class tests that partially written entries at the end of the last raft log file (also known as Segment)
	/// do not cause a problem. This is guaranteed by rotating after recovery and making sure that half written
	/// entries at the end do not stop recovery from proceeding.
	/// </summary>
	public class SegmentedRaftLogPartialEntryRecoveryTest
	{
		private bool InstanceFieldsInitialized = false;

		public SegmentedRaftLogPartialEntryRecoveryTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Dir = TestDirectory.testDirectory( FsRule.get() );
			Chain = RuleChain.outerRule( FsRule ).around( Dir );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fsRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FsRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory(fsRule.get());
		 public TestDirectory Dir;
		 private File _logDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(fsRule).around(dir);
		 public RuleChain Chain;

		 private SegmentedRaftLog CreateRaftLog( long rotateAtSize )
		 {
			  File directory = new File( RAFT_LOG_DIRECTORY_NAME );
			  _logDirectory = Dir.directory( directory.Name );

			  LogProvider logProvider = Instance;
			  CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( "100 entries", logProvider ) ).NewInstance();
			  return new SegmentedRaftLog( FsRule.get(), _logDirectory, rotateAtSize, CoreReplicatedContentMarshal.marshaller(), logProvider, 8, Clocks.fakeClock(), new OnDemandJobScheduler(), pruningStrategy );
		 }

		 private RecoveryProtocol CreateRecoveryProtocol()
		 {
			  FileNames fileNames = new FileNames( _logDirectory );
			  return new RecoveryProtocol( FsRule.get(), fileNames, new ReaderPool(8, Instance, fileNames, FsRule.get(), Clocks.fakeClock()), CoreReplicatedContentMarshal.marshaller(), Instance );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incompleteEntriesAtTheEndShouldNotCauseFailures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncompleteEntriesAtTheEndShouldNotCauseFailures()
		 {
			  // Given
			  // we use a RaftLog to create a raft log file and then we will start chopping bits off from the end
			  SegmentedRaftLog raftLog = CreateRaftLog( 100_000 );

			  raftLog.Start();

			  // Add a bunch of entries, preferably one of each available kind.
			  raftLog.Append( new RaftLogEntry( 4, new NewLeaderBarrier() ) );
			  raftLog.Append( new RaftLogEntry( 4, new ReplicatedIdAllocationRequest( new MemberId( System.Guid.randomUUID() ), IdType.RELATIONSHIP, 1, 1024 ) ) );
			  raftLog.Append( new RaftLogEntry( 4, new ReplicatedIdAllocationRequest( new MemberId( System.Guid.randomUUID() ), IdType.RELATIONSHIP, 1025, 1024 ) ) );
			  raftLog.Append( new RaftLogEntry( 4, new ReplicatedLockTokenRequest( new MemberId( System.Guid.randomUUID() ), 1 ) ) );
			  raftLog.Append( new RaftLogEntry( 4, new NewLeaderBarrier() ) );
			  raftLog.Append( new RaftLogEntry( 5, new ReplicatedTokenRequest( TokenType.LABEL, "labelToken", new sbyte[]{ 1, 2, 3 } ) ) );
			  raftLog.Append( new RaftLogEntry( 5, ReplicatedTransaction.from( new sbyte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } ) ) );

			  raftLog.Stop();

			  // We use a temporary RecoveryProtocol to get the file to chop
			  RecoveryProtocol recovery = CreateRecoveryProtocol();
			  State recoveryState = Recovery.run();
			  string logFilename = recoveryState.Segments.last().Filename;
			  recoveryState.Segments.close();
			  File logFile = new File( _logDirectory, logFilename );

			  // When
			  // We remove any number of bytes from the end (up to but not including the header) and try to recover
			  // Then
			  // No exceptions should be thrown
			  TruncateAndRecover( logFile, SegmentHeader.Size );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incompleteHeaderOfLastOfMoreThanOneLogFilesShouldNotCauseFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncompleteHeaderOfLastOfMoreThanOneLogFilesShouldNotCauseFailure()
		 {
			  // Given
			  // we use a RaftLog to create two log files, in order to chop the header of the second
			  SegmentedRaftLog raftLog = CreateRaftLog( 1 );

			  raftLog.Start();

			  raftLog.Append( new RaftLogEntry( 4, new NewLeaderBarrier() ) ); // will cause rotation

			  raftLog.Stop();

			  // We use a temporary RecoveryProtocol to get the file to chop
			  RecoveryProtocol recovery = CreateRecoveryProtocol();
			  State recoveryState = Recovery.run();
			  string logFilename = recoveryState.Segments.last().Filename;
			  recoveryState.Segments.close();
			  File logFile = new File( _logDirectory, logFilename );

			  // When
			  // We remove any number of bytes from the end of the second file and try to recover
			  // Then
			  // No exceptions should be thrown
			  TruncateAndRecover( logFile, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAppendAtTheEndOfLogFileWithIncompleteEntries() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAppendAtTheEndOfLogFileWithIncompleteEntries()
		 {
			  // Given
			  // we use a RaftLog to create a raft log file and then we will chop some bits off the end
			  SegmentedRaftLog raftLog = CreateRaftLog( 100_000 );

			  raftLog.Start();

			  raftLog.Append( new RaftLogEntry( 4, new NewLeaderBarrier() ) );

			  raftLog.Stop();

			  // We use a temporary RecoveryProtocol to get the file to chop
			  RecoveryProtocol recovery = CreateRecoveryProtocol();
			  State recoveryState = Recovery.run();
			  string logFilename = recoveryState.Segments.last().Filename;
			  recoveryState.Segments.close();
			  File logFile = new File( _logDirectory, logFilename );
			  StoreChannel lastFile = FsRule.get().open(logFile, OpenMode.READ_WRITE);
			  long currentSize = lastFile.size();
			  lastFile.close();

			  // When
			  // We induce an incomplete entry at the end of the last file
			  lastFile = FsRule.get().open(logFile, OpenMode.READ_WRITE);
			  lastFile.Truncate( currentSize - 1 );
			  lastFile.close();

			  // We start the raft log again, on the previous log file with truncated last entry.
			  raftLog = CreateRaftLog( 100_000 );

			  //  Recovery will run here
			  raftLog.Start();

			  // Append an entry
			  raftLog.Append( new RaftLogEntry( 4, new NewLeaderBarrier() ) );

			  // Then
			  // The log should report as containing only the last entry we've appended
			  using ( RaftLogCursor entryCursor = raftLog.GetEntryCursor( 0 ) )
			  {
					// There should be exactly one entry, of type NewLeaderBarrier
					assertTrue( entryCursor.Next() );
					RaftLogEntry raftLogEntry = entryCursor.get();
					assertEquals( typeof( NewLeaderBarrier ), raftLogEntry.Content().GetType() );
					assertFalse( entryCursor.Next() );
			  }
			  raftLog.Stop();
		 }

		 /// <summary>
		 /// Truncates and recovers the log file provided, one byte at a time until it reaches the header.
		 /// The reason the header is not truncated (and instead has its own test) is that if the log consists of
		 /// only one file (Segment) and the header is incomplete, that is correctly an exceptional circumstance and
		 /// is tested elsewhere.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void truncateAndRecover(java.io.File logFile, long truncateDownToSize) throws java.io.IOException, DamagedLogStorageException, DisposedException
		 private void TruncateAndRecover( File logFile, long truncateDownToSize )
		 {
			  StoreChannel lastFile = FsRule.get().open(logFile, OpenMode.READ_WRITE);
			  long currentSize = lastFile.size();
			  lastFile.close();
			  RecoveryProtocol recovery;
			  while ( currentSize-- > truncateDownToSize )
			  {
					lastFile = FsRule.get().open(logFile, OpenMode.READ_WRITE);
					lastFile.Truncate( currentSize );
					lastFile.close();
					recovery = CreateRecoveryProtocol();
					State state = Recovery.run();
					state.Segments.close();
			  }
		 }
	}

}