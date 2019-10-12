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
namespace Org.Neo4j.Kernel.impl.store
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using DelegatingPageCache = Org.Neo4j.Io.pagecache.DelegatingPageCache;
	using DelegatingPagedFile = Org.Neo4j.Io.pagecache.DelegatingPagedFile;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using DelegatingPageCursor = Org.Neo4j.Io.pagecache.impl.DelegatingPageCursor;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using MetaDataRecord = Org.Neo4j.Kernel.impl.store.record.MetaDataRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using NullLogger = Org.Neo4j.Logging.NullLogger;
	using Race = Org.Neo4j.Test.Race;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.versionStringToLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Race.throwing;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public class MetaDataStoreTest
	{
		private bool InstanceFieldsInitialized = false;

		public MetaDataStoreTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fsRule );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _testDirectory ).around( _pageCacheRule );
		}

		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule( config().withInconsistentReads(false) );
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(testDirectory).around(pageCacheRule);
		 public RuleChain RuleChain;

		 private EphemeralFileSystemAbstraction _fs;
		 private PageCache _pageCache;
		 private bool _fakePageCursorOverflow;
		 private PageCache _pageCacheWithFakeOverflow;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _fs = _fsRule.get();
			  _pageCache = _pageCacheRule.getPageCache( _fs );
			  _fakePageCursorOverflow = false;
			  _pageCacheWithFakeOverflow = new DelegatingPageCacheAnonymousInnerClass( this, _pageCache );
		 }

		 private class DelegatingPageCacheAnonymousInnerClass : DelegatingPageCache
		 {
			 private readonly MetaDataStoreTest _outerInstance;

			 public DelegatingPageCacheAnonymousInnerClass( MetaDataStoreTest outerInstance, PageCache pageCache ) : base( pageCache )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
			 public override PagedFile map( File file, int pageSize, params OpenOption[] openOptions )
			 {
				  return new DelegatingPagedFileAnonymousInnerClass( this, base.map( file, pageSize, openOptions ) );
			 }

			 private class DelegatingPagedFileAnonymousInnerClass : DelegatingPagedFile
			 {
				 private readonly DelegatingPageCacheAnonymousInnerClass _outerInstance;

				 public DelegatingPagedFileAnonymousInnerClass( DelegatingPageCacheAnonymousInnerClass outerInstance, UnknownType map ) : base( map )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageCursor io(long pageId, int pf_flags) throws java.io.IOException
				 public override PageCursor io( long pageId, int pfFlags )
				 {
					  return new DelegatingPageCursorAnonymousInnerClass( this, base.io( pageId, pfFlags ) );
				 }

				 private class DelegatingPageCursorAnonymousInnerClass : DelegatingPageCursor
				 {
					 private readonly DelegatingPagedFileAnonymousInnerClass _outerInstance;

					 public DelegatingPageCursorAnonymousInnerClass( DelegatingPagedFileAnonymousInnerClass outerInstance, UnknownType io ) : base( io )
					 {
						 this.outerInstance = outerInstance;
					 }

					 public override bool checkAndClearBoundsFlag()
					 {
						  return _outerInstance.outerInstance.outerInstance.fakePageCursorOverflow | base.checkAndClearBoundsFlag();
					 }
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getCreationTimeShouldFailWhenStoreIsClosed()
		 public virtual void getCreationTimeShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.CreationTime;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getCurrentLogVersionShouldFailWhenStoreIsClosed()
		 public virtual void getCurrentLogVersionShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.CurrentLogVersion;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getGraphNextPropShouldFailWhenStoreIsClosed()
		 public virtual void getGraphNextPropShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.GraphNextProp;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLastClosedTransactionIdShouldFailWhenStoreIsClosed()
		 public virtual void getLastClosedTransactionIdShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.LastClosedTransactionId;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLastClosedTransactionShouldFailWhenStoreIsClosed()
		 public virtual void getLastClosedTransactionShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.LastClosedTransaction;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLastCommittedTransactionShouldFailWhenStoreIsClosed()
		 public virtual void getLastCommittedTransactionShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.LastCommittedTransaction;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLastCommittedTransactionIdShouldFailWhenStoreIsClosed()
		 public virtual void getLastCommittedTransactionIdShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.LastCommittedTransactionId;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLatestConstraintIntroducingTxShouldFailWhenStoreIsClosed()
		 public virtual void getLatestConstraintIntroducingTxShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.LatestConstraintIntroducingTx;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getRandomNumberShouldFailWhenStoreIsClosed()
		 public virtual void getRandomNumberShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.RandomNumber;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getStoreVersionShouldFailWhenStoreIsClosed()
		 public virtual void getStoreVersionShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.StoreVersion;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getUpgradeTimeShouldFailWhenStoreIsClosed()
		 public virtual void getUpgradeTimeShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.UpgradeTime;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getUpgradeTransactionShouldFailWhenStoreIsClosed()
		 public virtual void getUpgradeTransactionShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.UpgradeTransaction;
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextCommittingTransactionIdShouldFailWhenStoreIsClosed()
		 public virtual void NextCommittingTransactionIdShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.NextCommittingTransactionId();
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void currentCommittingTransactionId()
		 public virtual void CurrentCommittingTransactionId()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.NextCommittingTransactionId();
			  long lastCommittingTxId = metaDataStore.NextCommittingTransactionId();
			  assertEquals( lastCommittingTxId, metaDataStore.CommittingTransactionId() );

			  metaDataStore.NextCommittingTransactionId();
			  metaDataStore.NextCommittingTransactionId();

			  lastCommittingTxId = metaDataStore.NextCommittingTransactionId();
			  assertEquals( lastCommittingTxId, metaDataStore.CommittingTransactionId() );
			  metaDataStore.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setLastCommittedAndClosedTransactionIdShouldFailWhenStoreIsClosed()
		 public virtual void SetLastCommittedAndClosedTransactionIdShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.SetLastCommittedAndClosedTransactionId( 1, 2, BASE_TX_COMMIT_TIMESTAMP, 3, 4 );
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionCommittedShouldFailWhenStoreIsClosed()
		 public virtual void TransactionCommittedShouldFailWhenStoreIsClosed()
		 {
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  metaDataStore.Close();
			  try
			  {
					metaDataStore.TransactionCommitted( 1, 1, BASE_TX_COMMIT_TIMESTAMP );
					fail( "Expected exception reading from MetaDataStore after being closed." );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRecordTransactionClosed()
		 public virtual void TestRecordTransactionClosed()
		 {
			  // GIVEN
			  MetaDataStore metaDataStore = NewMetaDataStore();
			  long[] originalClosedTransaction = metaDataStore.LastClosedTransaction;
			  long transactionId = originalClosedTransaction[0] + 1;
			  long version = 1L;
			  long byteOffset = 777L;

			  // WHEN
			  metaDataStore.TransactionClosed( transactionId, version, byteOffset );
			  // long[] with the highest offered gap-free number and its meta data.
			  long[] closedTransactionFlags = metaDataStore.LastClosedTransaction;

			  //EXPECT
			  assertEquals( version, closedTransactionFlags[1] );
			  assertEquals( byteOffset, closedTransactionFlags[2] );

			  // WHEN
			  metaDataStore.Close();
			  metaDataStore = NewMetaDataStore();

			  // EXPECT
			  long[] lastClosedTransactionFlags = metaDataStore.LastClosedTransaction;
			  assertEquals( version, lastClosedTransactionFlags[1] );
			  assertEquals( byteOffset, lastClosedTransactionFlags[2] );

			  metaDataStore.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setUpgradeTransactionMustBeAtomic() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUpgradeTransactionMustBeAtomic()
		 {
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					PagedFile pf = store.PagedFile;
					store.SetUpgradeTransaction( 0, 0, 0 );
					AtomicLong writeCount = new AtomicLong();
					AtomicLong fileReadCount = new AtomicLong();
					AtomicLong apiReadCount = new AtomicLong();
					int upperLimit = 10_000;
					int lowerLimit = 100;
					long endTime = currentTimeMillis() + SECONDS.toMillis(10);

					Race race = new Race();
					race.WithEndCondition( () => writeCount.get() >= upperLimit && fileReadCount.get() >= upperLimit && apiReadCount.get() >= upperLimit );
					race.WithEndCondition( () => writeCount.get() >= lowerLimit && fileReadCount.get() >= lowerLimit && apiReadCount.get() >= lowerLimit && currentTimeMillis() >= endTime );
					// writers
					race.AddContestants(3, () =>
					{
					 long count = writeCount.incrementAndGet();
					 store.SetUpgradeTransaction( count, count, count );
					});

					// file readers
					race.AddContestants(3, throwing(() =>
					{
					 using ( PageCursor cursor = pf.Io( 0, PagedFile.PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.next() );
						  long id;
						  long checksum;
						  do
						  {
								id = store.GetRecordValue( cursor, MetaDataStore.Position.UpgradeTransactionId );
								checksum = store.GetRecordValue( cursor, MetaDataStore.Position.UpgradeTransactionChecksum );
						  } while ( cursor.shouldRetry() );
						  AssertIdEqualsChecksum( id, checksum, "file" );
						  fileReadCount.incrementAndGet();
					 }
					}));

					race.AddContestants(3, () =>
					{
					 TransactionId transaction = store.UpgradeTransaction;
					 AssertIdEqualsChecksum( transaction.TransactionIdConflict(), transaction.Checksum(), "API" );
					 apiReadCount.incrementAndGet();
					});
					race.Go();
			  }
		 }

		 private static void AssertIdEqualsChecksum( long id, long checksum, string source )
		 {
			  if ( id != checksum )
			  {
					throw new AssertionError( "id (" + id + ") and checksum (" + checksum + ") from " + source + " should be identical" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementAndGetVersionMustBeAtomic() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncrementAndGetVersionMustBeAtomic()
		 {
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					long initialVersion = store.IncrementAndGetVersion();
					int threads = Runtime.Runtime.availableProcessors();
					int iterations = 500;
					Race race = new Race();
					race.AddContestants(threads, () =>
					{
					 for ( int i = 0; i < iterations; i++ )
					 {
						  store.IncrementAndGetVersion();
					 }
					});
					race.Go();
					assertThat( store.IncrementAndGetVersion(), @is(initialVersion + (threads * iterations) + 1) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionCommittedMustBeAtomic() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionCommittedMustBeAtomic()
		 {
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					PagedFile pf = store.PagedFile;
					store.TransactionCommitted( 2, 2, 2 );
					AtomicLong writeCount = new AtomicLong();
					AtomicLong fileReadCount = new AtomicLong();
					AtomicLong apiReadCount = new AtomicLong();
					int upperLimit = 10_000;
					int lowerLimit = 100;
					long endTime = currentTimeMillis() + SECONDS.toMillis(10);

					Race race = new Race();
					race.WithEndCondition( () => writeCount.get() >= upperLimit && fileReadCount.get() >= upperLimit && apiReadCount.get() >= upperLimit );
					race.WithEndCondition( () => writeCount.get() >= lowerLimit && fileReadCount.get() >= lowerLimit && apiReadCount.get() >= lowerLimit && currentTimeMillis() >= endTime );
					race.AddContestants(3, () =>
					{
					 long count = writeCount.incrementAndGet();
					 store.TransactionCommitted( count, count, count );
					});

					race.AddContestants(3, throwing(() =>
					{
					 using ( PageCursor cursor = pf.Io( 0, PagedFile.PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.next() );
						  long id;
						  long checksum;
						  do
						  {
								id = store.GetRecordValue( cursor, MetaDataStore.Position.LastTransactionId );
								checksum = store.GetRecordValue( cursor, MetaDataStore.Position.LastTransactionChecksum );
						  } while ( cursor.shouldRetry() );
						  AssertIdEqualsChecksum( id, checksum, "file" );
						  fileReadCount.incrementAndGet();
					 }
					}));

					race.AddContestants(3, () =>
					{
					 TransactionId transaction = store.LastCommittedTransaction;
					 AssertIdEqualsChecksum( transaction.TransactionIdConflict(), transaction.Checksum(), "API" );
					 apiReadCount.incrementAndGet();
					});

					race.Go();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionClosedMustBeAtomic() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionClosedMustBeAtomic()
		 {
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					PagedFile pf = store.PagedFile;
					int initialValue = 2;
					store.TransactionClosed( initialValue, initialValue, initialValue );
					AtomicLong writeCount = new AtomicLong();
					AtomicLong fileReadCount = new AtomicLong();
					AtomicLong apiReadCount = new AtomicLong();
					int upperLimit = 10_000;
					int lowerLimit = 100;
					long endTime = currentTimeMillis() + SECONDS.toMillis(10);

					Race race = new Race();
					race.WithEndCondition( () => writeCount.get() >= upperLimit && fileReadCount.get() >= upperLimit && apiReadCount.get() >= upperLimit );
					race.WithEndCondition( () => writeCount.get() >= lowerLimit && fileReadCount.get() >= lowerLimit && apiReadCount.get() >= lowerLimit && currentTimeMillis() >= endTime );
					race.AddContestants(3, () =>
					{
					 long count = writeCount.incrementAndGet();
					 store.TransactionCommitted( count, count, count );
					});

					race.AddContestants(3, throwing(() =>
					{
					 using ( PageCursor cursor = pf.Io( 0, PagedFile.PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.next() );
						  long logVersion;
						  long byteOffset;
						  do
						  {
								logVersion = store.GetRecordValue( cursor, MetaDataStore.Position.LastClosedTransactionLogVersion );
								byteOffset = store.GetRecordValue( cursor, MetaDataStore.Position.LastClosedTransactionLogByteOffset );
						  } while ( cursor.shouldRetry() );
						  AssertLogVersionEqualsByteOffset( logVersion, byteOffset, "file" );
						  fileReadCount.incrementAndGet();
					 }
					}));

					race.AddContestants(3, () =>
					{
					 long[] transaction = store.LastClosedTransaction;
					 AssertLogVersionEqualsByteOffset( transaction[0], transaction[1], "API" );
					 apiReadCount.incrementAndGet();
					});
					race.Go();
			  }
		 }

		 private static void AssertLogVersionEqualsByteOffset( long logVersion, long byteOffset, string source )
		 {
			  if ( logVersion != byteOffset )
			  {
					throw new AssertionError( "logVersion (" + logVersion + ") and byteOffset (" + byteOffset + ") from " + source + " should be identical" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSupportScanningAllRecords() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSupportScanningAllRecords()
		 {
			  File file = CreateMetaDataFile();
			  MetaDataStore.Position[] positions = MetaDataStore.Position.values();
			  long storeVersion = versionStringToLong( Standard.LATEST_RECORD_FORMATS.storeVersion() );
			  WriteCorrectMetaDataRecord( file, positions, storeVersion );

			  IList<long> actualValues = new List<long>();
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					store.ScanAllRecords(record =>
					{
					 actualValues.Add( record.Value );
					 return false;
					});
			  }

			  IList<long> expectedValues = java.util.positions.Select(p =>
			  {
				if ( p == MetaDataStore.Position.StoreVersion )
				{
					 return storeVersion;
				}
				else
				{
					 return p.ordinal() + 1L;
				}
			  }).ToList();

			  assertThat( actualValues, @is( expectedValues ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createMetaDataFile() throws java.io.IOException
		 private File CreateMetaDataFile()
		 {
			  File file = _testDirectory.databaseLayout().metadataStore();
			  _fs.create( file ).close();
			  return file;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSupportScanningAllRecordsWithRecordCursor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSupportScanningAllRecordsWithRecordCursor()
		 {
			  File file = CreateMetaDataFile();
			  MetaDataStore.Position[] positions = MetaDataStore.Position.values();
			  long storeVersion = versionStringToLong( Standard.LATEST_RECORD_FORMATS.storeVersion() );
			  WriteCorrectMetaDataRecord( file, positions, storeVersion );

			  IList<long> actualValues = new List<long>();
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					MetaDataRecord record = store.NewRecord();
					using ( PageCursor cursor = store.OpenPageCursorForReading( 0 ) )
					{
						 long highId = store.HighId;
						 for ( long id = 0; id < highId; id++ )
						 {
							  store.GetRecordByCursor( id, record, RecordLoad.NORMAL, cursor );
							  if ( record.InUse() )
							  {
									actualValues.Add( record.Value );
							  }
						 }
					}
			  }

			  IList<long> expectedValues = java.util.positions.Select(p =>
			  {
				if ( p == MetaDataStore.Position.StoreVersion )
				{
					 return storeVersion;
				}
				else
				{
					 return p.ordinal() + 1L;
				}
			  }).ToList();

			  assertThat( actualValues, @is( expectedValues ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeCorrectMetaDataRecord(java.io.File file, MetaDataStore.Position[] positions, long storeVersion) throws java.io.IOException
		 private void WriteCorrectMetaDataRecord( File file, MetaDataStore.Position[] positions, long storeVersion )
		 {
			  foreach ( MetaDataStore.Position position in positions )
			  {
					if ( position == MetaDataStore.Position.StoreVersion )
					{
						 MetaDataStore.SetRecord( _pageCache, file, position, storeVersion );
					}
					else
					{
						 MetaDataStore.SetRecord( _pageCache, file, position, position.ordinal() + 1 );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnderlyingStorageException.class) public void staticSetRecordMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StaticSetRecordMustThrowOnPageOverflow()
		 {
			  _fakePageCursorOverflow = true;
			  MetaDataStore.SetRecord( _pageCacheWithFakeOverflow, CreateMetaDataFile(), MetaDataStore.Position.FirstGraphProperty, 4242 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnderlyingStorageException.class) public void staticGetRecordMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StaticGetRecordMustThrowOnPageOverflow()
		 {
			  File metaDataFile = CreateMetaDataFile();
			  MetaDataStore.SetRecord( _pageCacheWithFakeOverflow, metaDataFile, MetaDataStore.Position.FirstGraphProperty, 4242 );
			  _fakePageCursorOverflow = true;
			  MetaDataStore.GetRecord( _pageCacheWithFakeOverflow, metaDataFile, MetaDataStore.Position.FirstGraphProperty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnderlyingStorageException.class) public void incrementVersionMustThrowOnPageOverflow()
		 public virtual void IncrementVersionMustThrowOnPageOverflow()
		 {
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					_fakePageCursorOverflow = true;
					store.IncrementAndGetVersion();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lastTxCommitTimestampShouldBeBaseInNewStore()
		 public virtual void LastTxCommitTimestampShouldBeBaseInNewStore()
		 {
			  using ( MetaDataStore metaDataStore = NewMetaDataStore() )
			  {
					long timestamp = metaDataStore.LastCommittedTransaction.commitTimestamp();
					assertThat( timestamp, equalTo( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnderlyingStorageException.class) public void readAllFieldsMustThrowOnPageOverflow()
		 public virtual void ReadAllFieldsMustThrowOnPageOverflow()
		 {
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					// Apparently this is possible, and will trick MetaDataStore into thinking the field is not initialised.
					// Thus it will reload all fields from the file, even though this ends up being the actual value in the
					// file. We do this because creating a proper MetaDataStore automatically initialises all fields.
					store.UpgradeTime = MetaDataStore.FieldNotInitialized;
					_fakePageCursorOverflow = true;
					store.UpgradeTime;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnderlyingStorageException.class) public void setRecordMustThrowOnPageOverflow()
		 public virtual void SetRecordMustThrowOnPageOverflow()
		 {
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					_fakePageCursorOverflow = true;
					store.SetUpgradeTransaction( 13, 42, 42 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logRecordsMustIgnorePageOverflow()
		 public virtual void LogRecordsMustIgnorePageOverflow()
		 {
			  using ( MetaDataStore store = NewMetaDataStore() )
			  {
					_fakePageCursorOverflow = true;
					store.LogRecords( NullLogger.Instance );
			  }
		 }

		 private MetaDataStore NewMetaDataStore()
		 {
			  LogProvider logProvider = NullLogProvider.Instance;
			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(_fs), _pageCacheWithFakeOverflow, _fs, logProvider, EmptyVersionContextSupplier.EMPTY );
			  return storeFactory.OpenNeoStores( true, StoreType.MetaData ).MetaDataStore;
		 }

	}

}