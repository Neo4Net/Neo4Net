using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Org.Neo4j.Cursor;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Graphdb.index;
	using Org.Neo4j.Helpers.Collection;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using IndexDefineCommand = Org.Neo4j.Kernel.impl.index.IndexDefineCommand;
	using LogEntryCursor = Org.Neo4j.Kernel.impl.transaction.log.LogEntryCursor;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using LogVersionRepository = Org.Neo4j.Kernel.impl.transaction.log.LogVersionRepository;
	using ReadableLogChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableLogChannel;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogEntry = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogRotation = Org.Neo4j.Kernel.impl.transaction.log.rotation.LogRotation;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

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
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
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