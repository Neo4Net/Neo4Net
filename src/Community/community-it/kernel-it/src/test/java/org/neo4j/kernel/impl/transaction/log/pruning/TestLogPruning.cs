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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{
	using After = org.junit.After;
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Neo4Net.Kernel.impl.transaction.log;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using TriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.TriggerInfo;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.keep_logical_logs;

	public class TestLogPruning
	{
		 private interface Extractor
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int extract(long fromVersion) throws java.io.IOException;
			  int Extract( long fromVersion );
		 }

		 private GraphDatabaseAPI _db;
		 private FileSystemAbstraction _fs;
		 private LogFiles _files;
		 private int _rotateEveryNTransactions;
		 private int _performedTransactions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void After()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
			  _fs.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noPruning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NoPruning()
		 {
			  NewDb( "true", 2 );

			  for ( int i = 0; i < 100; i++ )
			  {
					DoTransaction();
			  }

			  long currentVersion = _files.HighestLogVersion;
			  for ( long version = 0; version < currentVersion; version++ )
			  {
					assertTrue( "Version " + version + " has been unexpectedly pruned", _fs.fileExists( _files.getLogFileForVersion( version ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pruneByFileSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PruneByFileSize()
		 {
			  // Given
			  int transactionByteSize = FigureOutSampleTransactionSizeBytes();
			  int transactionsPerFile = 3;
			  int logThreshold = transactionByteSize * transactionsPerFile;
			  NewDb( logThreshold + " size", 1 );

			  // When
			  for ( int i = 0; i < 100; i++ )
			  {
					DoTransaction();
			  }

			  int totalLogFileSize = LogFileSize();
			  double totalTransactions = ( double ) totalLogFileSize / transactionByteSize;
			  assertTrue( totalTransactions >= 3 && totalTransactions < 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pruneByFileCount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PruneByFileCount()
		 {
			  int logsToKeep = 5;
			  NewDb( logsToKeep + " files", 3 );

			  for ( int i = 0; i < 100; i++ )
			  {
					DoTransaction();
			  }

			  assertEquals( logsToKeep, LogCount() );
			  // TODO we could verify, after the db has been shut down, that the file count is n.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pruneByTransactionCount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PruneByTransactionCount()
		 {
			  int transactionsToKeep = 100;
			  int transactionsPerLog = 3;
			  NewDb( transactionsToKeep + " txs", 3 );

			  for ( int i = 0; i < 100; i++ )
			  {
					DoTransaction();
			  }

			  int transactionCount = transactionCount();
			  assertTrue( "Transaction count expected to be within " + transactionsToKeep + " <= txs <= " + ( transactionsToKeep + transactionsPerLog ) + ", but was " + transactionCount, transactionCount >= transactionsToKeep && transactionCount <= ( transactionsToKeep + transactionsPerLog ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepAtLeastOneTransactionAfterRotate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepAtLeastOneTransactionAfterRotate()
		 {
			  // Given
			  // a database configured to keep 1 byte worth of logs, which means prune everything on rotate
			  NewDb( 1 + " size", 1 );

			  // When
			  // some transactions go through, rotating and pruning everything after them
			  for ( int i = 0; i < 2; i++ )
			  {
					DoTransaction();
			  }
			  // and the log gets rotated, which means we have a new one with no txs in it
			  _db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
			  /*
			   * if we hadn't rotated after the txs went through, we would need to change the assertion to be at least 1 tx
			   * instead of exactly one.
			   */

			  // Then
			  // the database must have kept at least one tx (in our case exactly one, because we rotated the log)
			  assertThat( TransactionCount(), greaterThanOrEqualTo(1) );
		 }

		 private GraphDatabaseAPI NewDb( string logPruning, int rotateEveryNTransactions )
		 {
			  this._rotateEveryNTransactions = rotateEveryNTransactions;
			  _fs = new EphemeralFileSystemAbstraction();
			  TestGraphDatabaseFactory gdf = new TestGraphDatabaseFactory();
			  gdf.FileSystem = new UncloseableDelegatingFileSystemAbstraction( _fs );
			  GraphDatabaseBuilder builder = gdf.NewImpermanentDatabaseBuilder();
			  builder.setConfig( keep_logical_logs, logPruning );
			  this._db = ( GraphDatabaseAPI ) builder.NewGraphDatabase();
			  _files = _db.DependencyResolver.resolveDependency( typeof( LogFiles ) );
			  return _db;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doTransaction() throws java.io.IOException
		 private void DoTransaction()
		 {
			  if ( ++_performedTransactions >= _rotateEveryNTransactions )
			  {
					_db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
					_performedTransactions = 0;
			  }

			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode();
					node.SetProperty( "name", "a somewhat lengthy string of some sort, right?" );
					tx.Success();
			  }
			  CheckPoint();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkPoint() throws java.io.IOException
		 private void CheckPoint()
		 {
			  TriggerInfo triggerInfo = new SimpleTriggerInfo( "test" );
			  _db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint( triggerInfo );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int figureOutSampleTransactionSizeBytes() throws java.io.IOException
		 private int FigureOutSampleTransactionSizeBytes()
		 {
			  _db = NewDb( "true", 5 );
			  DoTransaction();
			  _db.shutdown();
			  return ( int ) _fs.getFileSize( _files.getLogFileForVersion( 0 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int aggregateLogData(Extractor extractor) throws java.io.IOException
		 private int AggregateLogData( Extractor extractor )
		 {
			  int total = 0;
			  for ( long i = _files.HighestLogVersion; i >= 0; i-- )
			  {
					if ( _files.versionExists( i ) )
					{
						 total += extractor.Extract( i );
					}
					else
					{
						 break;
					}
			  }
			  return total;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int logCount() throws java.io.IOException
		 private int LogCount()
		 {
			  return AggregateLogData( from => 1 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int logFileSize() throws java.io.IOException
		 private int LogFileSize()
		 {
			  return AggregateLogData( from => ( int ) _fs.getFileSize( _files.getLogFileForVersion( from ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int transactionCount() throws java.io.IOException
		 private int TransactionCount()
		 {
			  return AggregateLogData(version =>
			  {
				int counter = 0;
				LogVersionBridge bridge = channel => channel;
				LogVersionedStoreChannel versionedStoreChannel = _files.openForVersion( version );
				using ( ReadableLogChannel channel = new ReadAheadLogChannel( versionedStoreChannel, bridge, 1000 ) )
				{
					 using ( PhysicalTransactionCursor<ReadableLogChannel> physicalTransactionCursor = new PhysicalTransactionCursor<ReadableLogChannel>( channel, new VersionAwareLogEntryReader<>() ) )
					 {
						  while ( physicalTransactionCursor.Next() )
						  {
								counter++;
						  }
					 }
				}
				return counter;
			  });
		 }
	}

}