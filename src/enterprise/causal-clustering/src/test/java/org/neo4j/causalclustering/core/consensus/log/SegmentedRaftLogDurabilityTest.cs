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
namespace Neo4Net.causalclustering.core.consensus.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CoreLogPruningStrategyFactory = Neo4Net.causalclustering.core.consensus.log.segmented.CoreLogPruningStrategyFactory;
	using SegmentedRaftLog = Neo4Net.causalclustering.core.consensus.log.segmented.SegmentedRaftLog;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.valueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.log.RaftLogHelper.hasNoContent;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.log.RaftLogHelper.readLogEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.NullLogProvider.getInstance;

	public class SegmentedRaftLogDurabilityTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fsRule = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();

		 private readonly RaftLogFactory _logFactory = fileSystem =>
		 {
		  File directory = new File( RAFT_LOG_DIRECTORY_NAME );
		  fileSystem.mkdir( directory );

		  long rotateAtSizeBytes = 128;
		  int readerPoolSize = 8;

		  NullLogProvider logProvider = Instance;
		  SegmentedRaftLog log = new SegmentedRaftLog( fileSystem, directory, rotateAtSizeBytes, new DummyRaftableContentSerializer(), logProvider, readerPoolSize, Clocks.fakeClock(), new OnDemandJobScheduler(), (new CoreLogPruningStrategyFactory("1 size", logProvider)).newInstance() );
		  log.start();

		  return log;
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendDataAndNotCommitImmediately() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendDataAndNotCommitImmediately()
		 {
			  RaftLog log = _logFactory.createBasedOn( FsRule.get() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogEntry logEntry = new RaftLogEntry(1, org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.valueOf(1));
			  RaftLogEntry logEntry = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 1 ) );
			  log.Append( logEntry );

			  VerifyCurrentLogAndNewLogLoadedFromFileSystem(log, FsRule.get(), myLog =>
			  {
				assertThat( myLog.appendIndex(), @is(0L) );
				assertThat( readLogEntry( myLog, 0 ), equalTo( logEntry ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendAndCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendAndCommit()
		 {
			  RaftLog log = _logFactory.createBasedOn( FsRule.get() );

			  RaftLogEntry logEntry = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 1 ) );
			  log.Append( logEntry );

			  VerifyCurrentLogAndNewLogLoadedFromFileSystem( log, FsRule.get(), myLog => assertThat(myLog.appendIndex(), @is(0L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendAfterReloadingFromFileSystem() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendAfterReloadingFromFileSystem()
		 {
			  RaftLog log = _logFactory.createBasedOn( FsRule.get() );

			  RaftLogEntry logEntryA = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 1 ) );
			  log.Append( logEntryA );

			  FsRule.get().crash();
			  log = _logFactory.createBasedOn( FsRule.get() );

			  RaftLogEntry logEntryB = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 2 ) );
			  log.Append( logEntryB );

			  assertThat( log.AppendIndex(), @is(1L) );
			  assertThat( readLogEntry( log, 0 ), @is( logEntryA ) );
			  assertThat( readLogEntry( log, 1 ), @is( logEntryB ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncatePreviouslyAppendedEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncatePreviouslyAppendedEntries()
		 {
			  RaftLog log = _logFactory.createBasedOn( FsRule.get() );

			  RaftLogEntry logEntryA = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 1 ) );
			  RaftLogEntry logEntryB = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 2 ) );

			  log.Append( logEntryA );
			  log.Append( logEntryB );
			  log.Truncate( 1 );

			  VerifyCurrentLogAndNewLogLoadedFromFileSystem( log, FsRule.get(), myLog => assertThat(myLog.appendIndex(), @is(0L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplacePreviouslyAppendedEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplacePreviouslyAppendedEntries()
		 {
			  RaftLog log = _logFactory.createBasedOn( FsRule.get() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogEntry logEntryA = new RaftLogEntry(1, org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.valueOf(1));
			  RaftLogEntry logEntryA = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 1 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogEntry logEntryB = new RaftLogEntry(1, org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.valueOf(2));
			  RaftLogEntry logEntryB = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 2 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogEntry logEntryC = new RaftLogEntry(1, org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.valueOf(3));
			  RaftLogEntry logEntryC = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 3 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogEntry logEntryD = new RaftLogEntry(1, org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.valueOf(4));
			  RaftLogEntry logEntryD = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 4 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogEntry logEntryE = new RaftLogEntry(1, org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.valueOf(5));
			  RaftLogEntry logEntryE = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 5 ) );

			  log.Append( logEntryA );
			  log.Append( logEntryB );
			  log.Append( logEntryC );

			  log.Truncate( 1 );

			  log.Append( logEntryD );
			  log.Append( logEntryE );

			  VerifyCurrentLogAndNewLogLoadedFromFileSystem(log, FsRule.get(), myLog =>
			  {
				assertThat( myLog.appendIndex(), @is(2L) );
				assertThat( readLogEntry( myLog, 0 ), equalTo( logEntryA ) );
				assertThat( readLogEntry( myLog, 1 ), equalTo( logEntryD ) );
				assertThat( readLogEntry( myLog, 2 ), equalTo( logEntryE ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogDifferentContentTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogDifferentContentTypes()
		 {
			  RaftLog log = _logFactory.createBasedOn( FsRule.get() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogEntry logEntryA = new RaftLogEntry(1, org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.valueOf(1));
			  RaftLogEntry logEntryA = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 1 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogEntry logEntryB = new RaftLogEntry(1, org.Neo4Net.causalclustering.core.consensus.ReplicatedString.valueOf("hejzxcjkzhxcjkxz"));
			  RaftLogEntry logEntryB = new RaftLogEntry( 1, ReplicatedString.valueOf( "hejzxcjkzhxcjkxz" ) );

			  log.Append( logEntryA );
			  log.Append( logEntryB );

			  VerifyCurrentLogAndNewLogLoadedFromFileSystem(log, FsRule.get(), myLog =>
			  {
				assertThat( myLog.appendIndex(), @is(1L) );
				assertThat( readLogEntry( myLog, 0 ), equalTo( logEntryA ) );
				assertThat( readLogEntry( myLog, 1 ), equalTo( logEntryB ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverAfterEventuallyPruning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverAfterEventuallyPruning()
		 {
			  RaftLog log = _logFactory.createBasedOn( FsRule.get() );

			  long term = 0L;

			  long safeIndex;
			  long prunedIndex = -1;
			  int val = 0;

			  // this loop should eventually be able to prune something
			  while ( prunedIndex == -1 )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 log.Append( new RaftLogEntry( term, valueOf( val++ ) ) );
					}
					safeIndex = log.AppendIndex() - 50;
					// when
					prunedIndex = log.Prune( safeIndex );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long finalAppendIndex = log.appendIndex();
			  long finalAppendIndex = log.AppendIndex();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long finalPrunedIndex = prunedIndex;
			  long finalPrunedIndex = prunedIndex;
			  VerifyCurrentLogAndNewLogLoadedFromFileSystem(log, FsRule.get(), myLog =>
			  {
				assertThat( log, hasNoContent( 0 ) );
				assertThat( log, hasNoContent( finalPrunedIndex ) );
				assertThat( myLog.prevIndex(), equalTo(finalPrunedIndex) );
				assertThat( myLog.appendIndex(), @is(finalAppendIndex) );
				assertThat( readLogEntry( myLog, finalPrunedIndex + 1 ).content(), equalTo(valueOf((int) finalPrunedIndex + 1)) );
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyCurrentLogAndNewLogLoadedFromFileSystem(RaftLog log, org.Neo4Net.graphdb.mockfs.EphemeralFileSystemAbstraction fileSystem, LogVerifier logVerifier) throws Exception
		 private void VerifyCurrentLogAndNewLogLoadedFromFileSystem( RaftLog log, EphemeralFileSystemAbstraction fileSystem, LogVerifier logVerifier )
		 {
			  logVerifier.VerifyLog( log );
			  logVerifier.VerifyLog( _logFactory.createBasedOn( FsRule.get() ) );
			  fileSystem.Crash();
			  logVerifier.VerifyLog( _logFactory.createBasedOn( FsRule.get() ) );
		 }

		 private interface RaftLogFactory
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: RaftLog createBasedOn(org.Neo4Net.io.fs.FileSystemAbstraction fileSystem) throws Exception;
			  RaftLog CreateBasedOn( FileSystemAbstraction fileSystem );
		 }

		 private interface LogVerifier
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void verifyLog(RaftLog log) throws java.io.IOException;
			  void VerifyLog( RaftLog log );
		 }
	}

}