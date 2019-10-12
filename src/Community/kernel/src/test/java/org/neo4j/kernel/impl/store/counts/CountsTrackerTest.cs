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
namespace Neo4Net.Kernel.impl.store.counts
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Function;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using CountsVisitor = Neo4Net.Kernel.Impl.Api.CountsVisitor;
	using TransactionVersionContextSupplier = Neo4Net.Kernel.impl.context.TransactionVersionContextSupplier;
	using CountsKey = Neo4Net.Kernel.impl.store.counts.keys.CountsKey;
	using CountsKeyFactory = Neo4Net.Kernel.impl.store.counts.keys.CountsKeyFactory;
	using Neo4Net.Kernel.impl.store.kvstore;
	using ReadableBuffer = Neo4Net.Kernel.impl.store.kvstore.ReadableBuffer;
	using RotationTimeoutException = Neo4Net.Kernel.impl.store.kvstore.RotationTimeoutException;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Register = Neo4Net.Register.Register;
	using Registers = Neo4Net.Register.Registers;
	using Barrier = Neo4Net.Test.Barrier;
	using Resources = Neo4Net.Test.rule.Resources;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.all;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.DebugUtil.classNameContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.DebugUtil.methodIs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.DebugUtil.stackTraceContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.Resources.InitialLifecycle.STARTED;

	public class CountsTrackerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.Resources resourceManager = new org.neo4j.test.rule.Resources();
		 public readonly Resources ResourceManager = new Resources();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.ThreadingRule threading = new org.neo4j.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threading = new ThreadingRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToStartAndStopTheStore()
		 public virtual void ShouldBeAbleToStartAndStopTheStore()
		 {
			  // given
			  ResourceManager.managed( NewTracker() );

			  // when
			  ResourceManager.lifeStarts();
			  ResourceManager.lifeShutsDown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldBeAbleToWriteDataToCountsTracker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToWriteDataToCountsTracker()
		 {
			  // given
			  CountsTracker tracker = ResourceManager.managed( NewTracker() );
			  long indexId = 0;
			  CountsOracle oracle = new CountsOracle();
			  {
					CountsOracle.Node a = oracle.Node( 1 );
					CountsOracle.Node b = oracle.Node( 1 );
					oracle.Relationship( a, 1, b );
					oracle.IndexSampling( indexId, 2, 2 );
					oracle.IndexUpdatesAndSize( indexId, 10, 2 );
			  }

			  // when
			  oracle.Update( tracker, 2 );

			  // then
			  oracle.Verify( tracker );

			  // when
			  tracker.Rotate( 2 );

			  // then
			  oracle.Verify( tracker );

			  // when
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_IndexStatsUpdater updater = tracker.UpdateIndexCounts() )
			  {
					updater.IncrementIndexUpdates( indexId, 2 );
			  }

			  // then
			  oracle.IndexUpdatesAndSize( indexId, 12, 2 );
			  oracle.Verify( tracker );

			  // when
			  tracker.Rotate( 2 );

			  // then
			  oracle.Verify( tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreCounts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreCounts()
		 {
			  // given
			  CountsOracle oracle = SomeData();

			  // when
			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker tracker = life.Add( NewTracker() );
					oracle.Update( tracker, 2 );
					tracker.Rotate( 2 );
			  }

			  // then
			  using ( Lifespan life = new Lifespan() )
			  {
					oracle.Verify( life.Add( NewTracker() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateCountsOnExistingStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateCountsOnExistingStore()
		 {
			  // given
			  CountsOracle oracle = SomeData();
			  int firstTx = 2;
			  int secondTx = 3;
			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker tracker = life.Add( NewTracker() );
					oracle.Update( tracker, firstTx );
					tracker.Rotate( firstTx );

					oracle.Verify( tracker );

					// when
					CountsOracle delta = new CountsOracle();
					{
						 CountsOracle.Node n1 = delta.Node( 1 );
						 CountsOracle.Node n2 = delta.Node( 1, 4 ); // Label 4 has not been used before...
						 delta.Relationship( n1, 1, n2 );
						 delta.Relationship( n2, 2, n1 ); // relationshipType 2 has not been used before...
					}
					delta.Update( tracker, secondTx );
					delta.Update( oracle );

					// then
					oracle.Verify( tracker );

					// when
					tracker.Rotate( secondTx );
			  }

			  // then
			  using ( Lifespan life = new Lifespan() )
			  {
					oracle.Verify( life.Add( NewTracker() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detectInMemoryDirtyVersionRead()
		 public virtual void DetectInMemoryDirtyVersionRead()
		 {
			  int labelId = 1;
			  long lastClosedTransactionId = 11L;
			  long writeTransactionId = 22L;
			  TransactionVersionContextSupplier versionContextSupplier = new TransactionVersionContextSupplier();
			  versionContextSupplier.Init( () => lastClosedTransactionId );
			  VersionContext versionContext = versionContextSupplier.VersionContext;

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker tracker = life.Add( NewTracker( versionContextSupplier ) );
					using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater updater = tracker.Apply( writeTransactionId ).get() )
					{
						 updater.IncrementNodeCount( labelId, 1 );
					}

					versionContext.InitRead();
					tracker.NodeCount( labelId, Registers.newDoubleLongRegister() );
					assertTrue( versionContext.Dirty );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allowNonDirtyInMemoryDirtyVersionRead()
		 public virtual void AllowNonDirtyInMemoryDirtyVersionRead()
		 {
			  int labelId = 1;
			  long lastClosedTransactionId = 15L;
			  long writeTransactionId = 13L;
			  TransactionVersionContextSupplier versionContextSupplier = new TransactionVersionContextSupplier();
			  versionContextSupplier.Init( () => lastClosedTransactionId );
			  VersionContext versionContext = versionContextSupplier.VersionContext;

			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker tracker = life.Add( NewTracker( versionContextSupplier ) );
					using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater updater = tracker.Apply( writeTransactionId ).get() )
					{
						 updater.IncrementNodeCount( labelId, 1 );
					}

					versionContext.InitRead();
					tracker.NodeCount( labelId, Registers.newDoubleLongRegister() );
					assertFalse( versionContext.Dirty );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReadUpToDateValueWhileAnotherThreadIsPerformingRotation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReadUpToDateValueWhileAnotherThreadIsPerformingRotation()
		 {
			  // given
			  CountsOracle oracle = SomeData();
			  const int firstTransaction = 2;
			  int secondTransaction = 3;
			  using ( Lifespan life = new Lifespan() )
			  {
					CountsTracker tracker = life.Add( NewTracker() );
					oracle.Update( tracker, firstTransaction );
					tracker.Rotate( firstTransaction );
			  }

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.CountsOracle delta = new org.neo4j.kernel.impl.store.CountsOracle();
			  CountsOracle delta = new CountsOracle();
			  {
					CountsOracle.Node n1 = delta.Node( 1 );
					CountsOracle.Node n2 = delta.Node( 1, 4 ); // Label 4 has not been used before...
					delta.Relationship( n1, 1, n2 );
					delta.Relationship( n2, 2, n1 ); // relationshipType 2 has not been used before...
			  }
			  delta.Update( oracle );

			  using ( Lifespan life = new Lifespan() )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control barrier = new org.neo4j.test.Barrier_Control();
					Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
					CountsTracker tracker = life.Add( new CountsTrackerAnonymousInnerClass( this, ResourceManager.logProvider(), ResourceManager.fileSystem(), ResourceManager.pageCache(), Config.defaults(), EmptyVersionContextSupplier.EMPTY, barrier ) );
					Future<Void> task = Threading.execute(t =>
					{
					 try
					 {
						  delta.Update( t, secondTransaction );
						  t.rotate( secondTransaction );
					 }
					 catch ( IOException e )
					 {
						  throw new AssertionError( e );
					 }
					 return null;
					}, tracker);

					// then
					barrier.Await();
					oracle.Verify( tracker );
					barrier.Release();
					task.get();
					oracle.Verify( tracker );
			  }
		 }

		 private class CountsTrackerAnonymousInnerClass : CountsTracker
		 {
			 private readonly CountsTrackerTest _outerInstance;

			 private Neo4Net.Test.Barrier_Control _barrier;

			 public CountsTrackerAnonymousInnerClass( CountsTrackerTest outerInstance, Neo4Net.Logging.LogProvider logProvider, Neo4Net.Io.fs.FileSystemAbstraction fileSystem, Neo4Net.Io.pagecache.PageCache pageCache, Config defaults, VersionContextSupplier empty, Neo4Net.Test.Barrier_Control barrier ) : base( logProvider, fileSystem, pageCache, defaults, outerInstance.ResourceManager.testDirectory().databaseLayout(), empty )
			 {
				 this.outerInstance = outerInstance;
				 this._barrier = barrier;
			 }

			 protected internal override bool include( CountsKey countsKey, ReadableBuffer value )
			 {
				  _barrier.reached();
				  return base.include( countsKey, value );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOrderStoreByTxIdInHeaderThenMinorVersion()
		 public virtual void ShouldOrderStoreByTxIdInHeaderThenMinorVersion()
		 {
			  // given
			  FileVersion version = new FileVersion( 16, 5 );

			  // then
			  assertTrue( CountsTracker.Compare( version, new FileVersion( 5, 5 ) ) > 0 );
			  assertEquals( 0, CountsTracker.Compare( version, new FileVersion( 16, 5 ) ) );
			  assertTrue( CountsTracker.Compare( version, new FileVersion( 30, 1 ) ) < 0 );
			  assertTrue( CountsTracker.Compare( version, new FileVersion( 16, 1 ) ) > 0 );
			  assertTrue( CountsTracker.Compare( version, new FileVersion( 16, 7 ) ) < 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldNotRotateIfNoDataChanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRotateIfNoDataChanges()
		 {
			  // given
			  CountsTracker tracker = ResourceManager.managed( NewTracker() );
			  File before = tracker.CurrentFile();

			  // when
			  tracker.Rotate( tracker.TxId() );

			  // then
			  assertSame( "not rotated", before, tracker.CurrentFile() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldRotateOnDataChangesEvenIfTransactionIsUnchanged() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRotateOnDataChangesEvenIfTransactionIsUnchanged()
		 {
			  // given
			  CountsTracker tracker = ResourceManager.managed( NewTracker() );
			  File before = tracker.CurrentFile();
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_IndexStatsUpdater updater = tracker.UpdateIndexCounts() )
			  {
					updater.IncrementIndexUpdates( 7, 100 );
			  }

			  // when
			  tracker.Rotate( tracker.TxId() );

			  // then
			  assertNotEquals( "rotated", before, tracker.CurrentFile() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldSupportTransactionsAppliedOutOfOrderOnRotation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportTransactionsAppliedOutOfOrderOnRotation()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CountsTracker tracker = resourceManager.managed(newTracker());
			  CountsTracker tracker = ResourceManager.managed( NewTracker() );
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater tx = tracker.Apply( 2 ).get() )
			  {
					tx.IncrementNodeCount( 1, 1 );
			  }
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater tx = tracker.Apply( 4 ).get() )
			  {
					tx.IncrementNodeCount( 1, 1 );
			  }

			  // when
			  Future<long> rotated = Threading.executeAndAwait(new Rotation(2), tracker, thread =>
			  {
				switch ( thread.State )
				{
				case BLOCKED:
				case WAITING:
				case TIMED_WAITING:
				case TERMINATED:
					 return true;
				default:
					 return false;
				}
			  }, 10, SECONDS);
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater tx = tracker.Apply( 5 ).get() )
			  {
					tx.IncrementNodeCount( 1, 1 );
			  }
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater tx = tracker.Apply( 3 ).get() )
			  {
					tx.IncrementNodeCount( 1, 1 );
			  }

			  // then
			  assertEquals( "rotated transaction", 4, rotated.get().longValue() );
			  assertEquals( "stored transaction", 4, tracker.TxId() );

			  // the value in memory
			  assertEquals( "count", 4, tracker.NodeCount( 1, Registers.newDoubleLongRegister() ).readSecond() );

			  // the value in the store
			  CountsVisitor visitor = mock( typeof( CountsVisitor ) );
			  tracker.VisitFile( tracker.CurrentFile(), visitor );
			  verify( visitor ).visitNodeCount( 1, 3 );
			  verifyNoMoreInteractions( visitor );

			  assertEquals( "final rotation", 5, tracker.Rotate( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Resources.Life(STARTED) public void shouldNotEndUpInBrokenStateAfterRotationFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotEndUpInBrokenStateAfterRotationFailure()
		 {
			  // GIVEN
			  FakeClock clock = Clocks.fakeClock();
			  CallTrackingClock callTrackingClock = new CallTrackingClock( clock );
			  CountsTracker tracker = ResourceManager.managed( NewTracker( callTrackingClock, EmptyVersionContextSupplier.EMPTY ) );
			  int labelId = 1;
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater tx = tracker.Apply( 2 ).get() )
			  {
					tx.IncrementNodeCount( labelId, 1 ); // now at 1
			  }

			  // WHEN
			  System.Predicate<Thread> arrived = thread => stackTraceContains( thread, all( classNameContains( "Rotation" ), methodIs( "rotate" ) ) );
			  Future<object> rotation = Threading.executeAndAwait( t => t.rotate( 4 ), tracker, arrived, 1, SECONDS );
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater tx = tracker.Apply( 3 ).get() )
			  {
					tx.IncrementNodeCount( labelId, 1 ); // now at 2
			  }
			  while ( callTrackingClock.CallsToNanos() == 0 )
			  {
					Thread.Sleep( 10 );
			  }
			  clock.Forward( Config.defaults().get(GraphDatabaseSettings.counts_store_rotation_timeout).toMillis() * 2, MILLISECONDS );
			  try
			  {
					rotation.get();
					fail( "Should've failed rotation due to timeout" );
			  }
			  catch ( ExecutionException e )
			  {
					// good
					assertTrue( e.InnerException is RotationTimeoutException );
			  }

			  // THEN
			  Neo4Net.Register.Register_DoubleLongRegister register = Registers.newDoubleLongRegister();
			  tracker.Get( CountsKeyFactory.nodeKey( labelId ), register );
			  assertEquals( 2, register.ReadSecond() );

			  // and WHEN later attempting rotation again
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater tx = tracker.Apply( 4 ).get() )
			  {
					tx.IncrementNodeCount( labelId, 1 ); // now at 3
			  }
			  tracker.Rotate( 4 );

			  // THEN
			  tracker.Get( CountsKeyFactory.nodeKey( labelId ), register );
			  assertEquals( 3, register.ReadSecond() );
		 }

		 private CountsTracker NewTracker()
		 {
			  return NewTracker( Clocks.nanoClock(), EmptyVersionContextSupplier.EMPTY );
		 }

		 private CountsTracker NewTracker( VersionContextSupplier versionContextSupplier )
		 {
			  return NewTracker( Clocks.nanoClock(), versionContextSupplier );
		 }

		 private CountsTracker NewTracker( SystemNanoClock clock, VersionContextSupplier versionContextSupplier )
		 {
			  return ( new CountsTracker( ResourceManager.logProvider(), ResourceManager.fileSystem(), ResourceManager.pageCache(), Config.defaults(), ResourceManager.testDirectory().databaseLayout(), clock, versionContextSupplier ) ).setInitializer(new DataInitializerAnonymousInnerClass(this));
		 }

		 private class DataInitializerAnonymousInnerClass : DataInitializer<Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater>
		 {
			 private readonly CountsTrackerTest _outerInstance;

			 public DataInitializerAnonymousInnerClass( CountsTrackerTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void initialize( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater updater )
			 {
			 }

			 public long initialVersion()
			 {
				  return FileVersion.InitialTxId;
			 }
		 }

		 private static CountsOracle SomeData()
		 {
			  CountsOracle oracle = new CountsOracle();
			  CountsOracle.Node n0 = oracle.Node( 0, 1 );
			  CountsOracle.Node n1 = oracle.Node( 0, 3 );
			  CountsOracle.Node n2 = oracle.Node( 2, 3 );
			  CountsOracle.Node n3 = oracle.Node( 2 );
			  oracle.Relationship( n0, 1, n2 );
			  oracle.Relationship( n1, 1, n3 );
			  oracle.Relationship( n1, 1, n2 );
			  oracle.Relationship( n0, 1, n3 );
			  long indexId = 2;
			  oracle.IndexUpdatesAndSize( indexId, 0L, 50L );
			  oracle.IndexSampling( indexId, 25L, 50L );
			  return oracle;
		 }

		 private class Rotation : IOFunction<CountsTracker, long>
		 {
			  internal readonly long TxId;

			  internal Rotation( long txId )
			  {
					this.TxId = txId;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<long> apply(CountsTracker tracker) throws java.io.IOException
			  public override long? Apply( CountsTracker tracker )
			  {
					return tracker.Rotate( TxId );
			  }
		 }
	}

}