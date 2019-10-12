using System;
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
namespace Neo4Net.Index.@internal.gbptree
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Neo4Net.Cursors;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	/// <summary>
	/// From a range of keys two disjunct sets are generated, "toAdd" and "toRemove".
	/// In each "iteration" writer will grab enough work from toAdd and toRemove to fill up one "batch".
	/// The batch will be applied to the GB+Tree during this iteration. The batch is also used to update
	/// a set of keys that all readers MUST see.
	/// 
	/// Readers are allowed to see more keys because they race with concurrent insertions, but they should
	/// at least see every key that has been inserted in previous iterations or not yet removed in current
	/// or previous iterations.
	/// 
	/// The <seealso cref="TestCoordinator"/> is responsible for "planning" the execution of the test. It generates
	/// toAdd and toRemove, prepare the GB+Tree with entries and serve readers and writer with information
	/// about what they should do next.
	/// </summary>
	public abstract class GBPTreeConcurrencyITBase<KEY, VALUE>
	{
		private bool InstanceFieldsInitialized = false;

		public GBPTreeConcurrencyITBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( this.GetType(), _fs.get() );
			Rules = outerRule( _fs ).around( _directory ).around( _pageCacheRule ).around( _random );
		}

		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly RandomRule _random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(fs).around(directory).around(pageCacheRule).around(random);
		 public RuleChain Rules;

		 private TestLayout<KEY, VALUE> _layout;
		 private GBPTree<KEY, VALUE> _index;
		 private readonly ExecutorService _threadPool = Executors.newFixedThreadPool( Runtime.Runtime.availableProcessors() );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private GBPTree<KEY,VALUE> createIndex() throws java.io.IOException
		 private GBPTree<KEY, VALUE> CreateIndex()
		 {
			  int pageSize = 512;
			  _layout = GetLayout( _random );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs.get(), config().withPageSize(pageSize).withAccessChecks(true) );
			  return _index = ( new GBPTreeBuilder<KEY, VALUE>( pageCache, _directory.file( "index" ), _layout ) ).build();
		 }

		 protected internal abstract TestLayout<KEY, VALUE> GetLayout( RandomRule random );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void consistencyCheckAndClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConsistencyCheckAndClose()
		 {
			  _threadPool.shutdownNow();
			  _index.consistencyCheck();
			  _index.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadForwardCorrectlyWithConcurrentInsert() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadForwardCorrectlyWithConcurrentInsert()
		 {
			  TestCoordinator testCoordinator = new TestCoordinator( this, _random.random(), true, 1 );
			  ShouldReadCorrectlyWithConcurrentUpdates( testCoordinator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadBackwardCorrectlyWithConcurrentInsert() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadBackwardCorrectlyWithConcurrentInsert()
		 {
			  TestCoordinator testCoordinator = new TestCoordinator( this, _random.random(), false, 1 );
			  ShouldReadCorrectlyWithConcurrentUpdates( testCoordinator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadForwardCorrectlyWithConcurrentRemove() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadForwardCorrectlyWithConcurrentRemove()
		 {
			  TestCoordinator testCoordinator = new TestCoordinator( this, _random.random(), true, 0 );
			  ShouldReadCorrectlyWithConcurrentUpdates( testCoordinator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadBackwardCorrectlyWithConcurrentRemove() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadBackwardCorrectlyWithConcurrentRemove()
		 {
			  TestCoordinator testCoordinator = new TestCoordinator( this, _random.random(), false, 0 );
			  ShouldReadCorrectlyWithConcurrentUpdates( testCoordinator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadForwardCorrectlyWithConcurrentUpdates() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadForwardCorrectlyWithConcurrentUpdates()
		 {
			  TestCoordinator testCoordinator = new TestCoordinator( this, _random.random(), true, 0.5 );
			  ShouldReadCorrectlyWithConcurrentUpdates( testCoordinator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadBackwardCorrectlyWithConcurrentUpdates() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadBackwardCorrectlyWithConcurrentUpdates()
		 {
			  TestCoordinator testCoordinator = new TestCoordinator( this, _random.random(), false, 0.5 );
			  ShouldReadCorrectlyWithConcurrentUpdates( testCoordinator );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldReadCorrectlyWithConcurrentUpdates(TestCoordinator testCoordinator) throws Throwable
		 private void ShouldReadCorrectlyWithConcurrentUpdates( TestCoordinator testCoordinator )
		 {
			  // Readers config
			  int readers = max( 1, Runtime.Runtime.availableProcessors() - 1 );

			  // Thread communication
			  System.Threading.CountdownEvent readerReadySignal = new System.Threading.CountdownEvent( readers );
			  System.Threading.CountdownEvent readerStartSignal = new System.Threading.CountdownEvent( 1 );
			  AtomicBoolean endSignal = testCoordinator.EndSignal();
			  AtomicBoolean failHalt = new AtomicBoolean(); // Readers signal to writer that there is a failure
			  AtomicReference<Exception> readerError = new AtomicReference<Exception>();

			  // GIVEN
			  _index = CreateIndex();
			  testCoordinator.Prepare( _index );

			  // WHEN starting the readers
			  RunnableReader readerTask = new RunnableReader( this, testCoordinator, readerReadySignal, readerStartSignal, endSignal, failHalt, readerError );
			  for ( int i = 0; i < readers; i++ )
			  {
					_threadPool.submit( readerTask );
			  }

			  // and starting the checkpointer
			  _threadPool.submit( CheckpointThread( endSignal, readerError, failHalt ) );

			  // and starting the writer
			  try
			  {
					Write( testCoordinator, readerReadySignal, readerStartSignal, endSignal, failHalt );
			  }
			  finally
			  {
					// THEN no reader should have failed by the time we have finished all the scheduled updates.
					// A successful read means that all results were ordered and we saw all inserted values and
					// none of the removed values at the point of making the seek call.
					endSignal.set( true );
					_threadPool.shutdown();
					_threadPool.awaitTermination( 10, TimeUnit.SECONDS );
					if ( readerError.get() != null )
					{
						 //noinspection ThrowFromFinallyBlock
						 throw readerError.get();
					}
			  }
		 }

		 private class TestCoordinator : System.Func<ReaderInstruction>
		 {
			 private readonly GBPTreeConcurrencyITBase<KEY, VALUE> _outerInstance;

			  internal readonly Random Random;

			  // Range
			  internal readonly long MinRange = 0;
			  internal readonly long MaxRange = 1 << 13; // 8192

			  // Instructions for writer
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int WriteBatchSizeConflict;

			  // Instructions for reader
			  internal readonly bool ForwardsSeek;
			  internal readonly double WritePercentage;
			  internal readonly AtomicReference<ReaderInstruction> CurrentReaderInstruction;
			  internal SortedSet<long> ReadersShouldSee;

			  // Progress
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicBoolean EndSignalConflict;

			  // Control for ADD and REMOVE
			  internal LinkedList<long> ToRemove = new LinkedList<long>();
			  internal LinkedList<long> ToAdd = new LinkedList<long>();
			  internal IList<UpdateOperation> UpdatesForNextIteration = new List<UpdateOperation>();

			  internal TestCoordinator( GBPTreeConcurrencyITBase<KEY, VALUE> outerInstance, Random random, bool forwardsSeek, double writePercentage )
			  {
				  this._outerInstance = outerInstance;
					this.EndSignalConflict = new AtomicBoolean();
					this.Random = random;
					this.ForwardsSeek = forwardsSeek;
					this.WritePercentage = writePercentage;
					this.WriteBatchSizeConflict = random.Next( 990 ) + 10; // 10-999
					CurrentReaderInstruction = new AtomicReference<ReaderInstruction>();
					IComparer<long> comparator = forwardsSeek ? System.Collections.IComparer.naturalOrder() : System.Collections.IComparer.reverseOrder();
					ReadersShouldSee = new SortedSet<long>( comparator );
			  }

			  internal virtual IList<long> ShuffleToNewList( IList<long> sourceList, Random random )
			  {
					IList<long> shuffledList = new List<long>( sourceList );
					Collections.shuffle( shuffledList, random );
					return shuffledList;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void prepare(GBPTree<KEY,VALUE> index) throws java.io.IOException
			  internal virtual void Prepare( GBPTree<KEY, VALUE> index )
			  {
					PrepareIndex( index, ReadersShouldSee, ToRemove, ToAdd, Random );
					IterationFinished();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void prepareIndex(GBPTree<KEY,VALUE> index, java.util.TreeSet<long> dataInIndex, java.util.Queue<long> toRemove, java.util.Queue<long> toAdd, java.util.Random random) throws java.io.IOException
			  internal virtual void PrepareIndex( GBPTree<KEY, VALUE> index, SortedSet<long> dataInIndex, LinkedList<long> toRemove, LinkedList<long> toAdd, Random random )
			  {
					IList<long> fullRange = LongStream.range( MinRange, MaxRange ).boxed().collect(Collectors.toList());
					IList<long> rangeOutOfOrder = ShuffleToNewList( fullRange, random );
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 foreach ( long? key in rangeOutOfOrder )
						 {
							  bool addForRemoval = random.NextDouble() > WritePercentage;
							  if ( addForRemoval )
							  {
									writer.Put( key( key ),outerInstance.value( key.Value ) );
									dataInIndex.Add( key );
									toRemove.AddLast( key );
							  }
							  else
							  {
									toAdd.AddLast( key );
							  }
						 }
					}
			  }

			  internal virtual void IterationFinished()
			  {
					// Create new set to not modify set that readers use concurrently
					ReadersShouldSee = new SortedSet<long>( ReadersShouldSee );
					UpdateRecentlyInsertedData( ReadersShouldSee, UpdatesForNextIteration );
					UpdatesForNextIteration = GenerateUpdatesForNextIteration();
					UpdateWithSoonToBeRemovedData( ReadersShouldSee, UpdatesForNextIteration );
					CurrentReaderInstruction.set( NewReaderInstruction( MinRange, MaxRange, ReadersShouldSee ) );
			  }

			  internal virtual void UpdateRecentlyInsertedData( SortedSet<long> readersShouldSee, IList<UpdateOperation> updateBatch )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					updateBatch.Where( UpdateOperation::isInsert ).ForEach( uo => uo.applyToSet( readersShouldSee ) );
			  }

			  internal virtual void UpdateWithSoonToBeRemovedData( SortedSet<long> readersShouldSee, IList<UpdateOperation> updateBatch )
			  {
					updateBatch.Where( uo => !uo.Insert ).ForEach( uo => uo.applyToSet( readersShouldSee ) );
			  }

			  internal virtual ReaderInstruction NewReaderInstruction( long minRange, long maxRange, SortedSet<long> readersShouldSee )
			  {
					return ForwardsSeek ? new ReaderInstruction( minRange, maxRange, readersShouldSee ) : new ReaderInstruction( maxRange - 1, minRange, readersShouldSee );
			  }

			  internal virtual IList<UpdateOperation> GenerateUpdatesForNextIteration()
			  {
					IList<UpdateOperation> updateOperations = new List<UpdateOperation>();
					if ( ToAdd.Count == 0 && ToRemove.Count == 0 )
					{
						 EndSignalConflict.set( true );
						 return updateOperations;
					}

					int operationsInIteration = ReadersShouldSee.Count < 1000 ? 100 : ReadersShouldSee.Count / 10;
					int count = 0;
					while ( count < operationsInIteration && ( ToAdd.Count > 0 || ToRemove.Count > 0 ) )
					{
						 UpdateOperation operation;
						 if ( ToAdd.Count == 0 )
						 {
							  operation = new RemoveOperation( _outerInstance, ToRemove.RemoveFirst() );
						 }
						 else if ( ToRemove.Count == 0 )
						 {
							  operation = new PutOperation( _outerInstance, ToAdd.RemoveFirst() );
						 }
						 else
						 {
							  bool remove = Random.NextDouble() > WritePercentage;
							  if ( remove )
							  {
									operation = new RemoveOperation( _outerInstance, ToRemove.RemoveFirst() );
							  }
							  else
							  {
									operation = new PutOperation( _outerInstance, ToAdd.RemoveFirst() );
							  }
						 }
						 updateOperations.Add( operation );
						 count++;
					}
					return updateOperations;
			  }

			  internal virtual IEnumerable<UpdateOperation> NextToWrite()
			  {
					return UpdatesForNextIteration;
			  }

			  public override ReaderInstruction Get()
			  {
					return CurrentReaderInstruction.get();
			  }

			  internal virtual AtomicBoolean EndSignal()
			  {
					return EndSignalConflict;
			  }

			  internal virtual int WriteBatchSize()
			  {
					return WriteBatchSizeConflict;
			  }

			  internal virtual bool IsReallyExpected( long nextToSee )
			  {
					return ReadersShouldSee.Contains( nextToSee );
			  }
		 }

		 private abstract class UpdateOperation
		 {
			 private readonly GBPTreeConcurrencyITBase<KEY, VALUE> _outerInstance;

			  internal readonly long Key;

			  internal UpdateOperation( GBPTreeConcurrencyITBase<KEY, VALUE> outerInstance, long key )
			  {
				  this._outerInstance = outerInstance;
					this.Key = key;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void apply(Writer<KEY,VALUE> writer) throws java.io.IOException;
			  internal abstract void Apply( Writer<KEY, VALUE> writer );

			  internal abstract void ApplyToSet( ISet<long> set );

			  internal abstract bool Insert { get; }
		 }

		 private class PutOperation : UpdateOperation
		 {
			 private readonly GBPTreeConcurrencyITBase<KEY, VALUE> _outerInstance;

			  internal PutOperation( GBPTreeConcurrencyITBase<KEY, VALUE> outerInstance, long key ) : base( outerInstance, key )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void apply(Writer<KEY,VALUE> writer) throws java.io.IOException
			  internal override void Apply( Writer<KEY, VALUE> writer )
			  {
					writer.Put( outerInstance.key( Key ), outerInstance.value( Key ) );
			  }

			  internal override void ApplyToSet( ISet<long> set )
			  {
					set.Add( Key );
			  }

			  internal override bool Insert
			  {
				  get
				  {
						return true;
				  }
			  }
		 }

		 private class RemoveOperation : UpdateOperation
		 {
			 private readonly GBPTreeConcurrencyITBase<KEY, VALUE> _outerInstance;

			  internal RemoveOperation( GBPTreeConcurrencyITBase<KEY, VALUE> outerInstance, long key ) : base( outerInstance, key )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void apply(Writer<KEY,VALUE> writer) throws java.io.IOException
			  internal override void Apply( Writer<KEY, VALUE> writer )
			  {
					writer.Remove( outerInstance.key( Key ) );
			  }

			  internal override void ApplyToSet( ISet<long> set )
			  {
					set.remove( Key );
			  }

			  internal override bool Insert
			  {
				  get
				  {
						return false;
				  }
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void write(TestCoordinator testCoordinator, java.util.concurrent.CountDownLatch readerReadySignal, java.util.concurrent.CountDownLatch readerStartSignal, java.util.concurrent.atomic.AtomicBoolean endSignal, java.util.concurrent.atomic.AtomicBoolean failHalt) throws InterruptedException, java.io.IOException
		 private void Write( TestCoordinator testCoordinator, System.Threading.CountdownEvent readerReadySignal, System.Threading.CountdownEvent readerStartSignal, AtomicBoolean endSignal, AtomicBoolean failHalt )
		 {
			  assertTrue( readerReadySignal.await( 10, SECONDS ) ); // Ready, set...
			  readerStartSignal.Signal(); // GO!

			  while ( !failHalt.get() && !endSignal.get() )
			  {
					WriteOneIteration( testCoordinator, failHalt );
					testCoordinator.IterationFinished();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeOneIteration(TestCoordinator testCoordinator, java.util.concurrent.atomic.AtomicBoolean failHalt) throws java.io.IOException, InterruptedException
		 private void WriteOneIteration( TestCoordinator testCoordinator, AtomicBoolean failHalt )
		 {
			  int batchSize = testCoordinator.WriteBatchSize();
			  IEnumerable<UpdateOperation> toWrite = testCoordinator.NextToWrite();
			  IEnumerator<UpdateOperation> toWriteIterator = toWrite.GetEnumerator();
			  while ( toWriteIterator.MoveNext() )
			  {
					using ( Writer<KEY, VALUE> writer = _index.writer() )
					{
						 int inBatch = 0;
						 while ( toWriteIterator.MoveNext() && inBatch < batchSize )
						 {
							  UpdateOperation operation = toWriteIterator.Current;
							  operation.Apply( writer );
							  if ( failHalt.get() )
							  {
									break;
							  }
							  inBatch++;
						 }
					}
					// Sleep to allow checkpointer to step in
					MILLISECONDS.sleep( 1 );
			  }
		 }

		 private class RunnableReader : ThreadStart
		 {
			 private readonly GBPTreeConcurrencyITBase<KEY, VALUE> _outerInstance;

			  internal readonly System.Threading.CountdownEvent ReaderReadySignal;
			  internal readonly System.Threading.CountdownEvent ReaderStartSignal;
			  internal readonly AtomicBoolean EndSignal;
			  internal readonly AtomicBoolean FailHalt;
			  internal readonly AtomicReference<Exception> ReaderError;
			  internal readonly TestCoordinator TestCoordinator;

			  internal RunnableReader( GBPTreeConcurrencyITBase<KEY, VALUE> outerInstance, TestCoordinator testCoordinator, System.Threading.CountdownEvent readerReadySignal, System.Threading.CountdownEvent readerStartSignal, AtomicBoolean endSignal, AtomicBoolean failHalt, AtomicReference<Exception> readerError )
			  {
				  this._outerInstance = outerInstance;
					this.ReaderReadySignal = readerReadySignal;
					this.ReaderStartSignal = readerStartSignal;
					this.EndSignal = endSignal;
					this.FailHalt = failHalt;
					this.ReaderError = readerError;
					this.TestCoordinator = testCoordinator;
			  }

			  public override void Run()
			  {
					try
					{
						 ReaderReadySignal.Signal(); // Ready, set...
						 ReaderStartSignal.await(); // GO!

						 while ( !EndSignal.get() && !FailHalt.get() )
						 {
							  DoRead();
						 }
					}
					catch ( Exception e )
					{
						 ReaderError.set( e );
						 FailHalt.set( true );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doRead() throws java.io.IOException
			  internal virtual void DoRead()
			  {
					ReaderInstruction readerInstruction = TestCoordinator.get();
					IEnumerator<long> expectToSee = readerInstruction.ExpectToSee().GetEnumerator();
					long start = readerInstruction.Start();
					long end = readerInstruction.End();
					bool forward = start <= end;
					using ( RawCursor<Hit<KEY, VALUE>, IOException> cursor = outerInstance.index.Seek( outerInstance.key( start ), outerInstance.key( end ) ) )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( expectToSee.hasNext() )
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  long nextToSee = expectToSee.next();
							  while ( cursor.Next() )
							  {
									// Actual
									long lastSeenKey = outerInstance.keySeed( cursor.get().key() );
									long lastSeenValue = outerInstance.valueSeed( cursor.get().value() );

									if ( lastSeenKey != lastSeenValue )
									{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: fail(String.format("Read mismatching key value pair, key=%d, value=%d%n", lastSeenKey, lastSeenValue));
										 fail( string.Format( "Read mismatching key value pair, key=%d, value=%d%n", lastSeenKey, lastSeenValue ) );
									}

									while ( ( forward && lastSeenKey > nextToSee ) || ( !forward && lastSeenKey < nextToSee ) )
									{
										 if ( TestCoordinator.isReallyExpected( nextToSee ) )
										 {
											  fail( string.Format( "Expected to see {0:D} but went straight to {1:D}. ", nextToSee, lastSeenKey ) );
										 }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
										 if ( expectToSee.hasNext() )
										 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
											  nextToSee = expectToSee.next();
										 }
										 else
										 {
											  break;
										 }
									}
									if ( nextToSee == lastSeenKey )
									{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
										 if ( expectToSee.hasNext() )
										 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
											  nextToSee = expectToSee.next();
										 }
										 else
										 {
											  break;
										 }
									}
							  }
						 }
					}
			  }
		 }

		 private ThreadStart CheckpointThread( AtomicBoolean endSignal, AtomicReference<Exception> readerError, AtomicBoolean failHalt )
		 {
			  return () =>
			  {
				while ( !endSignal.get() )
				{
					 try
					 {
						  _index.checkpoint( IOLimiter.UNLIMITED );
						  // Sleep a little in between checkpoints
						  MILLISECONDS.sleep( 20L );
					 }
					 catch ( Exception e )
					 {
						  readerError.set( e );
						  failHalt.set( true );
					 }
				}
			  };
		 }

		 private class ReaderInstruction
		 {
			  internal readonly long StartRange;
			  internal readonly long EndRange;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly SortedSet<long> ExpectToSeeConflict;

			  internal ReaderInstruction( long startRange, long endRange, SortedSet<long> expectToSee )
			  {
					this.StartRange = startRange;
					this.EndRange = endRange;
					this.ExpectToSeeConflict = expectToSee;
			  }

			  internal virtual long Start()
			  {
					return StartRange;
			  }

			  internal virtual long End()
			  {
					return EndRange;
			  }

			  internal virtual SortedSet<long> ExpectToSee()
			  {
					return ExpectToSeeConflict;
			  }
		 }

		 private KEY Key( long seed )
		 {
			  return _layout.key( seed );
		 }

		 private VALUE Value( long seed )
		 {
			  return _layout.value( seed );
		 }

		 private long KeySeed( KEY key )
		 {
			  return _layout.keySeed( key );
		 }

		 private long ValueSeed( VALUE value )
		 {
			  return _layout.valueSeed( value );
		 }
	}

}