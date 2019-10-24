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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Cursors;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.Index;
	using Neo4Net.Collections.Helpers;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using IndexDefineCommand = Neo4Net.Kernel.impl.index.IndexDefineCommand;
	using LogEntryCursor = Neo4Net.Kernel.impl.transaction.log.LogEntryCursor;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;
	using ReadableLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadableLogChannel;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	/// <summary>
	/// Test for a problem where multiple threads getting an index for the first time
	/// and adding to or removing from it right there after. There was a race condition
	/// where the transaction which created the index came after the first one using it.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public class IndexCreationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private GraphDatabaseAPI _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexCreationConfigRaceCondition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexCreationConfigRaceCondition()
		 {
			  // Since this is a probability test and not a precise test run do the run
			  // a couple of times to be sure.
			  for ( int run = 0; run < 10; run++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int r = run;
					int r = run;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
					System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
					ExecutorService executor = newCachedThreadPool();
					for ( int thread = 0; thread < 10; thread++ )
					{
						 executor.submit(() =>
						 {
						  try
						  {
							  using ( Transaction tx = _db.beginTx() )
							  {
									latch.await();
									Index<Node> index = _db.index().forNodes("index" + r);
									Node node = _db.createNode();
									index.add( node, "name", "Name" );
									tx.success();
							  }
						  }
						  catch ( InterruptedException )
						  {
								Thread.interrupted();
						  }
						 });
					}
					latch.Signal();
					executor.shutdown();
					executor.awaitTermination( 10, TimeUnit.SECONDS );

					VerifyThatIndexCreationTransactionIsTheFirstOne();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyThatIndexCreationTransactionIsTheFirstOne() throws Exception
		 private void VerifyThatIndexCreationTransactionIsTheFirstOne()
		 {
			  LogFiles logFiles = _db.DependencyResolver.resolveDependency( typeof( LogFiles ) );
			  long version = _db.DependencyResolver.resolveDependency( typeof( LogVersionRepository ) ).CurrentLogVersion;
			  _db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
			  _db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint(new SimpleTriggerInfo("test")
			 );

			  ReadableLogChannel logChannel = logFiles.LogFile.getReader( LogPosition.start( version ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean success = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean success = new AtomicBoolean( false );

			  using ( IOCursor<LogEntry> cursor = new LogEntryCursor( new VersionAwareLogEntryReader<LogEntry>(), logChannel ) )
			  {
					IList<StorageCommand> commandsInFirstEntry = new List<StorageCommand>();
					bool startFound = false;

					while ( cursor.next() )
					{
						 LogEntry entry = cursor.get();

						 if ( entry is LogEntryStart )
						 {
							  if ( startFound )
							  {
									throw new System.ArgumentException( "More than one start entry" );
							  }
							  startFound = true;
						 }

						 if ( startFound && entry is LogEntryCommand )
						 {
							  commandsInFirstEntry.Add( entry.As<LogEntryCommand>().Command );
						 }

						 if ( entry is LogEntryCommit )
						 {
							  // The first COMMIT
							  assertTrue( startFound );
							  assertFalse( "Index creation transaction wasn't the first one", commandsInFirstEntry.Count == 0 );
							  IList<StorageCommand> createCommands = Iterators.asList( new FilteringIterator<StorageCommand>( commandsInFirstEntry.GetEnumerator(), item => item is IndexDefineCommand ) );
							  assertEquals( 1, createCommands.Count );
							  success.set( true );
							  break;
						 }
					}
			  }

			  assertTrue( "Didn't find any commit record in log " + version, success.get() );
		 }
	}

}