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
namespace Neo4Net.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Exceptions = Neo4Net.Helpers.Exceptions;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;
	using LimitedFileSystemGraphDatabase = Neo4Net.Test.limited.LimitedFileSystemGraphDatabase;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class RunOutOfDiskSpaceIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateIOExceptions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateIOExceptions()
		 {
			  // Given
			  TransactionFailureException exceptionThrown = null;

			  File storeDir = TestDirectory.storeDir();
			  LimitedFileSystemGraphDatabase db = new LimitedFileSystemGraphDatabase( storeDir );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  long logVersion = Db.DependencyResolver.resolveDependency( typeof( LogVersionRepository ) ).CurrentLogVersion;

			  Db.runOutOfDiskSpaceNao();

			  // When
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.createNode();
						tx.Success();
					  }
			  }
			  catch ( TransactionFailureException e )
			  {
					exceptionThrown = e;
			  }
			  finally
			  {
					assertNotNull( "Expected tx finish to throw TransactionFailureException when filesystem is full.", exceptionThrown );
					assertTrue( Exceptions.contains( exceptionThrown, typeof( IOException ) ) );
			  }

			  Db.somehowGainMoreDiskSpace(); // to help shutting down the db
			  Db.shutdown();

			  PageCache pageCache = PageCacheRule.getPageCache( Db.FileSystem );
			  File neoStore = TestDirectory.databaseLayout().metadataStore();
			  assertEquals( logVersion, MetaDataStore.getRecord( pageCache, neoStore, MetaDataStore.Position.LOG_VERSION ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopDatabaseWhenOutOfDiskSpace() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopDatabaseWhenOutOfDiskSpace()
		 {
			  // Given
			  TransactionFailureException expectedCommitException = null;
			  TransactionFailureException expectedStartException = null;
			  File storeDir = TestDirectory.absolutePath();
			  LimitedFileSystemGraphDatabase db = Cleanup.add( new LimitedFileSystemGraphDatabase( storeDir ) );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  long logVersion = Db.DependencyResolver.resolveDependency( typeof( LogVersionRepository ) ).CurrentLogVersion;

			  Db.runOutOfDiskSpaceNao();

			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.createNode();
						tx.Success();
					  }
			  }
			  catch ( TransactionFailureException e )
			  {
					expectedCommitException = e;
			  }
			  finally
			  {
					assertNotNull( "Expected tx finish to throw TransactionFailureException when filesystem is full.", expectedCommitException );
			  }

			  // When
			  try
			  {
					  using ( Transaction transaction = Db.beginTx() )
					  {
						fail( "Expected tx begin to throw TransactionFailureException when tx manager breaks." );
					  }
			  }
			  catch ( TransactionFailureException e )
			  {
					expectedStartException = e;
			  }
			  finally
			  {
					assertNotNull( "Expected tx begin to throw TransactionFailureException when tx manager breaks.", expectedStartException );
			  }

			  // Then
			  Db.somehowGainMoreDiskSpace(); // to help shutting down the db
			  Db.shutdown();

			  PageCache pageCache = PageCacheRule.getPageCache( Db.FileSystem );
			  File neoStore = TestDirectory.databaseLayout().metadataStore();
			  assertEquals( logVersion, MetaDataStore.getRecord( pageCache, neoStore, MetaDataStore.Position.LOG_VERSION ) );
		 }

	}

}