using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogVersionRepository_Fields.INITIAL_LOG_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

	public class CheckPointerIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private GraphDatabaseBuilder _builder;
		 private FileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fs = FsRule.get();
			  File storeDir = TestDirectory.databaseDir();
			  _builder = ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(_fs)).newImpermanentDatabaseBuilder(storeDir);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseShutdownDuringConstantCheckPointing() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabaseShutdownDuringConstantCheckPointing()
		 {
			  GraphDatabaseService db = _builder.setConfig( GraphDatabaseSettings.check_point_interval_time, 0 + "ms" ).setConfig( GraphDatabaseSettings.check_point_interval_tx, "1" ).setConfig( GraphDatabaseSettings.logical_log_rotation_threshold, "1g" ).newGraphDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
			  Thread.Sleep( 10 );
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckPointBasedOnTime() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckPointBasedOnTime()
		 {
			  // given
			  long millis = 200;
			  GraphDatabaseService db = _builder.setConfig( GraphDatabaseSettings.check_point_interval_time, millis + "ms" ).setConfig( GraphDatabaseSettings.check_point_interval_tx, "10000" ).setConfig( GraphDatabaseSettings.logical_log_rotation_threshold, "1g" ).newGraphDatabase();

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  // The scheduled job checking whether or not checkpoints are needed runs more frequently
			  // now that we've set the time interval so low, so we can simply wait for it here
			  long endTime = currentTimeMillis() + SECONDS.toMillis(30);
			  while ( !CheckPointInTxLog( db ) )
			  {
					Thread.Sleep( millis );
					assertTrue( "Took too long to produce a checkpoint", currentTimeMillis() < endTime );
			  }

			  Db.shutdown();

			  // then - 2 check points have been written in the log
			  IList<CheckPoint> checkPoints = ( new CheckPointCollector( TestDirectory.databaseDir(), _fs ) ).find(0);

			  assertTrue( "Expected at least two (at least one for time interval and one for shutdown), was " + checkPoints.ToString(), checkPoints.Count >= 2 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static boolean checkPointInTxLog(org.neo4j.graphdb.GraphDatabaseService db) throws java.io.IOException
		 private static bool CheckPointInTxLog( GraphDatabaseService db )
		 {
			  LogFiles logFiles = ( ( GraphDatabaseAPI )db ).DependencyResolver.resolveDependency( typeof( LogFiles ) );
			  LogFile logFile = logFiles.LogFile;
			  using ( ReadableLogChannel reader = logFile.GetReader( new LogPosition( 0, LOG_HEADER_SIZE ) ) )
			  {
					LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
					LogEntry entry;
					while ( ( entry = logEntryReader.ReadLogEntry( reader ) ) != null )
					{
						 if ( entry is CheckPoint )
						 {
							  return true;
						 }
					}
					return false;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckPointBasedOnTxCount() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckPointBasedOnTxCount()
		 {
			  // given
			  GraphDatabaseService db = _builder.setConfig( GraphDatabaseSettings.check_point_interval_time, "300m" ).setConfig( GraphDatabaseSettings.check_point_interval_tx, "1" ).setConfig( GraphDatabaseSettings.logical_log_rotation_threshold, "1g" ).newGraphDatabase();

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  // Instead of waiting 10s for the background job to do this check, perform the check right here
			  TriggerCheckPointAttempt( db );

			  assertTrue( CheckPointInTxLog( db ) );

			  Db.shutdown();

			  // then - 2 check points have been written in the log
			  IList<CheckPoint> checkPoints = ( new CheckPointCollector( TestDirectory.databaseDir(), _fs ) ).find(0);

			  assertEquals( 2, checkPoints.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCheckPointWhenThereAreNoCommits() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCheckPointWhenThereAreNoCommits()
		 {
			  // given
			  GraphDatabaseService db = _builder.setConfig( GraphDatabaseSettings.check_point_interval_time, "1s" ).setConfig( GraphDatabaseSettings.check_point_interval_tx, "10000" ).setConfig( GraphDatabaseSettings.logical_log_rotation_threshold, "1g" ).newGraphDatabase();

			  // when

			  // nothing happens

			  TriggerCheckPointAttempt( db );
			  assertFalse( CheckPointInTxLog( db ) );

			  Db.shutdown();

			  // then - 1 check point has been written in the log
			  IList<CheckPoint> checkPoints = ( new CheckPointCollector( TestDirectory.databaseDir(), _fs ) ).find(0);

			  assertEquals( 1, checkPoints.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToStartAndShutdownMultipleTimesTheDBWithoutCommittingTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToStartAndShutdownMultipleTimesTheDBWithoutCommittingTransactions()
		 {
			  // given
			  GraphDatabaseBuilder graphDatabaseBuilder = _builder.setConfig( GraphDatabaseSettings.check_point_interval_time, "300m" ).setConfig( GraphDatabaseSettings.check_point_interval_tx, "10000" ).setConfig( GraphDatabaseSettings.logical_log_rotation_threshold, "1g" );

			  // when
			  graphDatabaseBuilder.NewGraphDatabase().shutdown();
			  graphDatabaseBuilder.NewGraphDatabase().shutdown();

			  // then - 2 check points have been written in the log
			  IList<CheckPoint> checkPoints = ( new CheckPointCollector( TestDirectory.databaseDir(), _fs ) ).find(0);

			  assertEquals( 2, checkPoints.Count );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void triggerCheckPointAttempt(org.neo4j.graphdb.GraphDatabaseService db) throws Exception
		 private static void TriggerCheckPointAttempt( GraphDatabaseService db )
		 {
			  // Simulates triggering the checkpointer background job which runs now and then, checking whether
			  // or not there's a need to perform a checkpoint.
			  ( ( GraphDatabaseAPI )db ).DependencyResolver.resolveDependency( typeof( CheckPointer ) ).checkPointIfNeeded( new SimpleTriggerInfo( "Test" ) );
		 }

		 private class CheckPointCollector
		 {
			  internal readonly LogFiles LogFiles;
			  internal readonly LogEntryReader<ReadableClosablePositionAwareChannel> LogEntryReader;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CheckPointCollector(java.io.File directory, org.neo4j.io.fs.FileSystemAbstraction fileSystem) throws java.io.IOException
			  internal CheckPointCollector( File directory, FileSystemAbstraction fileSystem )
			  {
					this.LogEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
					this.LogFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( directory, fileSystem ).withLogEntryReader( LogEntryReader ).build();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<org.neo4j.kernel.impl.transaction.log.entry.CheckPoint> find(long version) throws java.io.IOException
			  public virtual IList<CheckPoint> Find( long version )
			  {
					IList<CheckPoint> checkPoints = new List<CheckPoint>();
					for ( ; version >= INITIAL_LOG_VERSION && LogFiles.versionExists( version ); version-- )
					{
						 LogVersionedStoreChannel channel = LogFiles.openForVersion( version );
						 ReadableClosablePositionAwareChannel recoveredDataChannel = new ReadAheadLogChannel( channel );

						 using ( LogEntryCursor cursor = new LogEntryCursor( LogEntryReader, recoveredDataChannel ) )
						 {
							  while ( cursor.Next() )
							  {
									LogEntry entry = cursor.Get();
									if ( entry is CheckPoint )
									{
										 checkPoints.Add( entry.As() );
									}
							  }
						 }
					}
					return checkPoints;
			  }
		 }
	}

}