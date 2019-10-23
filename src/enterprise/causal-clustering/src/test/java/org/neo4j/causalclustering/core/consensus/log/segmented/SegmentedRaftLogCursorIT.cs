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
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.raft_log_pruning_strategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.NullLogProvider.getInstance;

	public class SegmentedRaftLogCursorIT
	{
		 private readonly LifeSupport _life = new LifeSupport();
		 private FileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _life.stop();
			  _life.shutdown();
			  _fileSystem.Dispose();
		 }

		 private SegmentedRaftLog CreateRaftLog( long rotateAtSize, string pruneStrategy )
		 {
			  if ( _fileSystem == null )
			  {
					_fileSystem = new EphemeralFileSystemAbstraction();
			  }

			  File directory = new File( RAFT_LOG_DIRECTORY_NAME );
			  _fileSystem.mkdir( directory );

			  LogProvider logProvider = Instance;
			  CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( pruneStrategy, logProvider ) ).NewInstance();
			  SegmentedRaftLog newRaftLog = new SegmentedRaftLog( _fileSystem, directory, rotateAtSize, new DummyRaftableContentSerializer(), logProvider, 8, Clocks.systemClock(), new OnDemandJobScheduler(), pruningStrategy );

			  _life.add( newRaftLog );
			  _life.init();
			  _life.start();

			  return newRaftLog;
		 }

		 private SegmentedRaftLog CreateRaftLog( long rotateAtSize )
		 {
			  return CreateRaftLog( rotateAtSize, raft_log_pruning_strategy.DefaultValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseOnCursorForEntryThatDoesntExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseOnCursorForEntryThatDoesntExist()
		 {
			  //given
			  SegmentedRaftLog segmentedRaftLog = CreateRaftLog( 1 );
			  segmentedRaftLog.Append( new RaftLogEntry( 1, ReplicatedInteger.ValueOf( 1 ) ) );
			  segmentedRaftLog.Append( new RaftLogEntry( 2, ReplicatedInteger.ValueOf( 2 ) ) );
			  long lastIndex = segmentedRaftLog.Append( new RaftLogEntry( 3, ReplicatedInteger.ValueOf( 3 ) ) );

			  //when
			  bool next;
			  using ( RaftLogCursor entryCursor = segmentedRaftLog.GetEntryCursor( lastIndex + 1 ) )
			  {
					next = entryCursor.Next();
			  }

			  //then
			  assertFalse( next );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTrueOnEntryThatExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTrueOnEntryThatExists()
		 {
			  //given
			  SegmentedRaftLog segmentedRaftLog = CreateRaftLog( 1 );
			  segmentedRaftLog.Append( new RaftLogEntry( 1, ReplicatedInteger.ValueOf( 1 ) ) );
			  segmentedRaftLog.Append( new RaftLogEntry( 2, ReplicatedInteger.ValueOf( 2 ) ) );
			  long lastIndex = segmentedRaftLog.Append( new RaftLogEntry( 3, ReplicatedInteger.ValueOf( 3 ) ) );

			  //when
			  bool next;
			  using ( RaftLogCursor entryCursor = segmentedRaftLog.GetEntryCursor( lastIndex ) )
			  {
					next = entryCursor.Next();
			  }

			  //then
			  assertTrue( next );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseOnCursorForEntryThatWasPruned() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseOnCursorForEntryThatWasPruned()
		 {
			  //given
			  SegmentedRaftLog segmentedRaftLog = CreateRaftLog( 1, "keep_none" );
			  long firstIndex = segmentedRaftLog.Append( new RaftLogEntry( 1, ReplicatedInteger.ValueOf( 1 ) ) );
			  segmentedRaftLog.Append( new RaftLogEntry( 2, ReplicatedInteger.ValueOf( 2 ) ) );
			  long lastIndex = segmentedRaftLog.Append( new RaftLogEntry( 3, ReplicatedInteger.ValueOf( 3 ) ) );

			  //when
			  segmentedRaftLog.Prune( firstIndex );
			  RaftLogCursor entryCursor = segmentedRaftLog.GetEntryCursor( firstIndex );
			  bool next = entryCursor.Next();

			  //then
			  assertFalse( next );
		 }
	}

}