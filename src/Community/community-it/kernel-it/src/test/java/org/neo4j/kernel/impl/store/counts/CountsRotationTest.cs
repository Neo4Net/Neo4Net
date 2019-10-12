using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store.counts
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ClassGuardedAdversary = Neo4Net.Adversaries.ClassGuardedAdversary;
	using CountingAdversary = Neo4Net.Adversaries.CountingAdversary;
	using Neo4Net.Function;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using Neo4Net.Helpers.Collection;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using CountsVisitor = Neo4Net.Kernel.Impl.Api.CountsVisitor;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using CountsKey = Neo4Net.Kernel.impl.store.counts.keys.CountsKey;
	using CountsKeyFactory = Neo4Net.Kernel.impl.store.counts.keys.CountsKeyFactory;
	using RotationTimeoutException = Neo4Net.Kernel.impl.store.kvstore.RotationTimeoutException;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using TriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.TriggerInfo;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Register = Neo4Net.Register.Register;
	using Registers = Neo4Net.Register.Registers;
	using AdversarialPageCacheGraphDatabaseFactory = Neo4Net.Test.AdversarialPageCacheGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.FileVersion.INITIAL_MINOR_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.register.Registers.newDoubleLongRegister;

	public class CountsRotationTest
	{
		private bool InstanceFieldsInitialized = false;

		public CountsRotationTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDir = TestDirectory.testDirectory( this.GetType(), _fsRule.get() );
			RuleChain = RuleChain.outerRule( _threadingRule ).around( _pcRule ).around( _fsRule ).around( _testDir );
		}

		 private readonly Label _a = Label.label( "A" );
		 private readonly Label _b = Label.label( "B" );
		 private readonly Label _c = Label.label( "C" );

		 private readonly PageCacheRule _pcRule = new PageCacheRule();
		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private TestDirectory _testDir;
		 private readonly ThreadingRule _threadingRule = new ThreadingRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(threadingRule).around(pcRule).around(fsRule).around(testDir);
		 public RuleChain RuleChain;

		 private FileSystemAbstraction _fs;
		 private GraphDatabaseBuilder _dbBuilder;
		 private PageCache _pageCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fs = _fsRule.get();
			  _dbBuilder = ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(_fs)).newImpermanentDatabaseBuilder(_testDir.databaseDir());
			  _pageCache = _pcRule.getPageCache( _fs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateEmptyCountsTrackerStoreWhenCreatingDatabase()
		 public virtual void ShouldCreateEmptyCountsTrackerStoreWhenCreatingDatabase()
		 {
			  // GIVEN
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();

			  // WHEN
			  Db.shutdown();

			  // THEN
			  assertTrue( _fs.fileExists( AlphaStoreFile() ) );
			  assertFalse( _fs.fileExists( BetaStoreFile() ) );

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker( _pageCache ) );

					assertEquals( BASE_TX_ID, store.TxId() );
					assertEquals( INITIAL_MINOR_VERSION, store.MinorVersion() );
					assertEquals( 0, store.TotalEntriesStored() );
					assertEquals( 0, AllRecords( store ).Count );
			  }

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker( _pageCache ) );
					assertEquals( BASE_TX_ID, store.TxId() );
					assertEquals( INITIAL_MINOR_VERSION, store.MinorVersion() );
					assertEquals( 0, store.TotalEntriesStored() );
					assertEquals( 0, AllRecords( store ).Count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUnMapThePrestateFileWhenTimingOutOnRotationAndAllowForShutdownInTheFailedRotationState() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUnMapThePrestateFileWhenTimingOutOnRotationAndAllowForShutdownInTheFailedRotationState()
		 {
			  // Given
			  _dbBuilder.newGraphDatabase().shutdown();
			  CountsTracker store = CreateCountsTracker( _pageCache, Config.defaults( GraphDatabaseSettings.counts_store_rotation_timeout, "100ms" ) );
			  using ( Lifespan lifespan = new Lifespan( store ) )
			  {
					using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater updater = store.Apply( 2 ).get() )
					{
						 updater.IncrementNodeCount( 0, 1 );
					}

					try
					{
						 // when
						 store.Rotate( 3 );
						 fail( "should have thrown" );
					}
					catch ( RotationTimeoutException )
					{
						 // good
					}
			  }

			  // and also no exceptions closing the page cache
			  _pageCache.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rotationShouldNotCauseUnmappedFileProblem() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RotationShouldNotCauseUnmappedFileProblem()
		 {
			  // GIVEN
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();

			  DependencyResolver resolver = Db.DependencyResolver;
			  RecordStorageEngine storageEngine = resolver.ResolveDependency( typeof( RecordStorageEngine ) );
			  CountsTracker countStore = storageEngine.TestAccessNeoStores().Counts;

			  AtomicBoolean workerContinueFlag = new AtomicBoolean( true );
			  AtomicLong lookupsCounter = new AtomicLong();
			  int rotations = 100;
			  for ( int i = 0; i < 5; i++ )
			  {
					_threadingRule.execute( CountStoreLookup( workerContinueFlag, lookupsCounter ), countStore );
			  }

			  long startTxId = countStore.TxId();
			  for ( int i = 1; ( i < rotations ) || ( lookupsCounter.get() == 0 ); i++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( _b );
						 tx.Success();
					}
					CheckPoint( db );
			  }
			  workerContinueFlag.set( false );

			  assertEquals( "Should perform at least 100 rotations.", rotations, Math.Min( rotations, countStore.TxId() - startTxId ) );
			  assertTrue( "Should perform more then 0 lookups without exceptions.", lookupsCounter.get() > 0 );

			  Db.shutdown();
		 }

		 private static ThrowingFunction<CountsTracker, Void, Exception> CountStoreLookup( AtomicBoolean workerContinueFlag, AtomicLong lookups )
		 {
			  return countsTracker =>
			  {
				while ( workerContinueFlag.get() )
				{
					 Register.DoubleLongRegister register = Registers.newDoubleLongRegister();
					 countsTracker.get( CountsKeyFactory.nodeKey( 0 ), register );
					 lookups.incrementAndGet();
				}
				return null;
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRotateCountsStoreWhenClosingTheDatabase()
		 public virtual void ShouldRotateCountsStoreWhenClosingTheDatabase()
		 {
			  // GIVEN
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( _a );
					tx.Success();
			  }

			  // WHEN
			  Db.shutdown();

			  // THEN
			  assertTrue( _fs.fileExists( AlphaStoreFile() ) );
			  assertTrue( _fs.fileExists( BetaStoreFile() ) );

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker( _pageCache ) );
					// a transaction for creating the label and a transaction for the node
					assertEquals( BASE_TX_ID + 1 + 1, store.TxId() );
					assertEquals( INITIAL_MINOR_VERSION, store.MinorVersion() );
					// one for all nodes and one for the created "A" label
					assertEquals( 1 + 1, store.TotalEntriesStored() );
					assertEquals( 1 + 1, AllRecords( store ).Count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRotateCountsStoreWhenRotatingLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRotateCountsStoreWhenRotatingLog()
		 {
			  // GIVEN
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _dbBuilder.newGraphDatabase();

			  // WHEN doing a transaction (actually two, the label-mini-tx also counts)
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( _b );
					tx.Success();
			  }
			  // and rotating the log (which implies flushing)
			  CheckPoint( db );
			  // and creating another node after it
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( _c );
					tx.Success();
			  }

			  // THEN
			  assertTrue( _fs.fileExists( AlphaStoreFile() ) );
			  assertTrue( _fs.fileExists( BetaStoreFile() ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.pagecache.PageCache pageCache = db.getDependencyResolver().resolveDependency(org.neo4j.io.pagecache.PageCache.class);
			  PageCache pageCache = Db.DependencyResolver.resolveDependency( typeof( PageCache ) );
			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker store = life.Add( CreateCountsTracker( pageCache ) );
					// NOTE since the rotation happens before the second transaction is committed we do not see those changes
					// in the stats
					// a transaction for creating the label and a transaction for the node
					assertEquals( BASE_TX_ID + 1 + 1, store.TxId() );
					assertEquals( INITIAL_MINOR_VERSION, store.MinorVersion() );
					// one for all nodes and one for the created "B" label
					assertEquals( 1 + 1, store.TotalEntriesStored() );
					assertEquals( 1 + 1, AllRecords( store ).Count );
			  }

			  // on the other hand the tracker should read the correct value by merging data on disk and data in memory
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CountsTracker tracker = db.getDependencyResolver().resolveDependency(org.neo4j.kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine.class).testAccessNeoStores().getCounts();
			  CountsTracker tracker = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores().Counts;
			  assertEquals( 1 + 1, tracker.NodeCount( -1, newDoubleLongRegister() ).readSecond() );

			  int labelId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction transaction = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
					labelId = transaction.TokenRead().nodeLabel(_c.name());
			  }
			  assertEquals( 1, tracker.NodeCount( labelId, newDoubleLongRegister() ).readSecond() );

			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000) public void possibleToShutdownDbWhenItIsNotHealthyAndNotAllTransactionsAreApplied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PossibleToShutdownDbWhenItIsNotHealthyAndNotAllTransactionsAreApplied()
		 {
			  // adversary that makes page cache throw exception when node store is used
			  ClassGuardedAdversary adversary = new ClassGuardedAdversary( new CountingAdversary( 1, true ), typeof( NodeStore ) );
			  adversary.Disable();

			  GraphDatabaseService db = AdversarialPageCacheGraphDatabaseFactory.create( _fs, adversary ).newEmbeddedDatabaseBuilder( _testDir.databaseDir() ).newGraphDatabase();

			  System.Threading.CountdownEvent txStartLatch = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent txCommitLatch = new System.Threading.CountdownEvent( 1 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> result = java.util.concurrent.ForkJoinPool.commonPool().submit(() ->
			  Future<object> result = ForkJoinPool.commonPool().submit(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 txStartLatch.Signal();
					 Db.createNode();
					 Await( txCommitLatch );
					 tx.success();
				}
			  });

			  Await( txStartLatch );

			  adversary.Enable();

			  txCommitLatch.Signal();

			  try
			  {
					result.get();
					fail( "Exception expected" );
			  }
			  catch ( ExecutionException ee )
			  {
					// transaction is expected to fail because write through the page cache fails
					assertThat( ee.InnerException, instanceOf( typeof( TransactionFailureException ) ) );
			  }
			  adversary.Disable();

			  // shutdown should complete without any problems
			  Db.shutdown();
		 }

		 private static void Await( System.Threading.CountdownEvent latch )
		 {
			  try
			  {
					bool result = latch.await( 30, TimeUnit.SECONDS );
					if ( !result )
					{
						 throw new Exception( "Count down did not happen. Current count: " + latch.CurrentCount );
					}
			  }
			  catch ( InterruptedException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private CountsTracker CreateCountsTracker( PageCache pageCache )
		 {
			  return CreateCountsTracker( pageCache, Config.defaults() );
		 }

		 private CountsTracker CreateCountsTracker( PageCache pageCache, Config config )
		 {
			  return new CountsTracker( NullLogProvider.Instance, _fs, pageCache, config, _testDir.databaseLayout(), EmptyVersionContextSupplier.EMPTY );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkPoint(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws java.io.IOException
		 private static void CheckPoint( GraphDatabaseAPI db )
		 {
			  TriggerInfo triggerInfo = new SimpleTriggerInfo( "test" );
			  Db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint( triggerInfo );
		 }

		 private File AlphaStoreFile()
		 {
			  return _testDir.databaseLayout().countStoreA();
		 }

		 private File BetaStoreFile()
		 {
			  return _testDir.databaseLayout().countStoreB();
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Collection<org.neo4j.helpers.collection.Pair<? extends org.neo4j.kernel.impl.store.counts.keys.CountsKey, long>> allRecords(org.neo4j.kernel.impl.api.CountsVisitor_Visitable store)
		 private ICollection<Pair<CountsKey, long>> AllRecords( Neo4Net.Kernel.Impl.Api.CountsVisitor_Visitable store )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.neo4j.helpers.collection.Pair<? extends org.neo4j.kernel.impl.store.counts.keys.CountsKey, long>> records = new java.util.ArrayList<>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  ICollection<Pair<CountsKey, long>> records = new List<Pair<CountsKey, long>>();
			  store.Accept( new CountsVisitorAnonymousInnerClass( this, records ) );
			  return records;
		 }

		 private class CountsVisitorAnonymousInnerClass : CountsVisitor
		 {
			 private readonly CountsRotationTest _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private System.Collections.Generic.ICollection<org.neo4j.helpers.collection.Pair<JavaToDotNetGenericWildcard extends org.neo4j.kernel.impl.store.counts.keys.CountsKey, long>> records;
			 private ICollection<Pair<CountsKey, long>> _records;

			 public CountsVisitorAnonymousInnerClass<T1>( CountsRotationTest outerInstance, ICollection<T1> records ) where T1 : Neo4Net.Kernel.impl.store.counts.keys.CountsKey
			 {
				 this.outerInstance = outerInstance;
				 this._records = records;
			 }

			 public void visitNodeCount( int labelId, long count )
			 {
				  _records.Add( Pair.of( CountsKeyFactory.nodeKey( labelId ), count ) );
			 }

			 public void visitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
			 {
				  _records.Add( Pair.of( CountsKeyFactory.relationshipKey( startLabelId, typeId, endLabelId ), count ) );
			 }

			 public void visitIndexStatistics( long indexId, long updates, long size )
			 {
				  _records.Add( Pair.of( CountsKeyFactory.indexStatisticsKey( indexId ), size ) );
			 }

			 public void visitIndexSample( long indexId, long unique, long size )
			 {
				  _records.Add( Pair.of( CountsKeyFactory.indexSampleKey( indexId ), size ) );
			 }
		 }
	}

}