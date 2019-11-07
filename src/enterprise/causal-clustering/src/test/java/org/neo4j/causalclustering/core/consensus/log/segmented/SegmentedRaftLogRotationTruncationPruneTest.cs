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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using After = org.junit.After;
	using Test = org.junit.Test;

	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.ReplicatedInteger.ValueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.NullLogProvider.getInstance;

	public class SegmentedRaftLogRotationTruncationPruneTest
	{

		 private FileSystemAbstraction _fileSystem = new EphemeralFileSystemAbstraction();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _fileSystem.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneAwaySingleEntriesIfRotationHappenedEveryEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneAwaySingleEntriesIfRotationHappenedEveryEntry()
		 {
			  /// <summary>
			  /// If you have a raft log which rotates after every append, therefore having a single entry in every segment,
			  /// we assert that every sequential prune attempt will result in the prevIndex incrementing by one.
			  /// </summary>

			  // given
			  RaftLog log = CreateRaftLog();

			  long term = 0;
			  for ( int i = 0; i < 10; i++ )
			  {
					log.Append( new RaftLogEntry( term, ValueOf( i ) ) );
			  }

			  assertEquals( -1, log.PrevIndex() );
			  for ( int i = 0; i < 9; i++ )
			  {
					log.Prune( i );
					assertEquals( i, log.PrevIndex() );
			  }
			  log.Prune( 9 );
			  assertEquals( 8, log.PrevIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneAwaySingleEntriesAfterTruncationIfRotationHappenedEveryEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneAwaySingleEntriesAfterTruncationIfRotationHappenedEveryEntry()
		 {
			  /// <summary>
			  /// Given a log with many single-entry segments, a series of truncations at descending values followed by
			  /// pruning at more previous segments will maintain the correct prevIndex for the log.
			  /// 
			  /// Initial Scenario:    [0][1][2][3][4][5][6][7][8][9]              prevIndex = 0
			  /// Truncate segment 9 : [0][1][2][3][4][5][6][7][8]   (9)           prevIndex = 0
			  /// Truncate segment 8 : [0][1][2][3][4][5][6][7]      (8)(9)        prevIndex = 0
			  /// Truncate segment 7 : [0][1][2][3][4][5][6]         (7)(8)(9)     prevIndex = 0
			  /// Prune segment 0    :    [1][2][3][4][5][6]         (7)(8)(9)     prevIndex = 1
			  /// Prune segment 1    :       [2][3][4][5][6]         (7)(8)(9)     prevIndex = 2
			  /// Prune segment 2    :          [3][4][5][6]         (7)(8)(9)     prevIndex = 3
			  /// Prune segment 3    :             [4][5][6]         (7)(8)(9)     prevIndex = 4
			  /// Prune segment 4    :                [5][6]         (7)(8)(9)     prevIndex = 5
			  /// Prune segment 5    :                [5][6]         (7)(8)(9)     prevIndex = 5
			  /// Prune segment 6    :                [5][6]         (7)(8)(9)     prevIndex = 5
			  /// Etc...
			  /// 
			  /// The prevIndex should not become corrupt and become greater than 5 in this example.
			  /// </summary>

			  // given
			  RaftLog log = CreateRaftLog();

			  long term = 0;
			  for ( int i = 0; i < 10; i++ )
			  {
					log.Append( new RaftLogEntry( term, ValueOf( i ) ) );
			  }

			  log.Truncate( 9 );
			  log.Truncate( 8 );
			  log.Truncate( 7 );

			  assertEquals( -1, log.PrevIndex() );
			  for ( int i = 0; i <= 5; i++ )
			  {
					log.Prune( i );
					assertEquals( i, log.PrevIndex() );
			  }
			  for ( int i = 5; i < 10; i++ )
			  {
					log.Prune( i );
					assertEquals( 5, log.PrevIndex() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.causalclustering.core.consensus.log.RaftLog createRaftLog() throws Exception
		 private RaftLog CreateRaftLog()
		 {
			  File directory = new File( RAFT_LOG_DIRECTORY_NAME );
			  FileSystemAbstraction fileSystem = new EphemeralFileSystemAbstraction();
			  fileSystem.Mkdir( directory );

			  LogProvider logProvider = Instance;
			  CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( "1 entries", logProvider ) ).NewInstance();
			  SegmentedRaftLog newRaftLog = new SegmentedRaftLog( fileSystem, directory, 1, new DummyRaftableContentSerializer(), logProvider, 8, Clocks.fakeClock(), new OnDemandJobScheduler(), pruningStrategy );

			  newRaftLog.Start();
			  return newRaftLog;
		 }
	}

}