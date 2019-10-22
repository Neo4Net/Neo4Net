using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using SimpleLongLayout = Neo4Net.Index.Internal.gbptree.SimpleLongLayout;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Barrier = Neo4Net.Test.Barrier;
	using Neo4Net.Test;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.BlockStorage.Monitor_Fields.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.BlockStorage.NOT_CANCELLABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ByteBufferFactory.HEAP_ALLOCATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.OtherThreadExecutor.command;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, RandomExtension.class}) class BlockStorageTest
	internal class BlockStorageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory directory;
		 internal TestDirectory Directory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject RandomRule random;
		 internal RandomRule Random;

		 private File _file;
		 private FileSystemAbstraction _fileSystem;
		 private SimpleLongLayout _layout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setup()
		 internal virtual void Setup()
		 {
			  _file = Directory.file( "block" );
			  _fileSystem = Directory.FileSystem;
			  _layout = SimpleLongLayout.longLayout().withFixedSize(Random.nextBoolean()).withKeyPadding(Random.Next(10)).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateAndCloseTheBlockFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreateAndCloseTheBlockFile()
		 {
			  // given
			  assertFalse( _fileSystem.fileExists( _file ) );
			  using ( BlockStorage<MutableLong, MutableLong> ignored = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( 100 ), _fileSystem, _file, NO_MONITOR ) )
			  {
					// then
					assertTrue( _fileSystem.fileExists( _file ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddSingleEntryInLastBlock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddSingleEntryInLastBlock()
		 {
			  // given
			  TrackingMonitor monitor = new TrackingMonitor();
			  int blockSize = 100;
			  MutableLong key = new MutableLong( 10 );
			  MutableLong value = new MutableLong( 20 );
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( blockSize ), _fileSystem, _file, monitor ) )
			  {
					// when
					storage.Add( key, value );
					storage.DoneAdding();

					// then
					assertEquals( 1, monitor.BlockFlushedCallCount );
					assertEquals( 1, monitor.LastKeyCount );
					assertEquals( BlockStorage.BlockHeaderSize + monitor.TotalEntrySize, monitor.LastNumberOfBytes );
					assertEquals( blockSize, monitor.LastPositionAfterFlush );
					assertThat( monitor.LastNumberOfBytes, lessThan( blockSize ) );
					AssertContents( _layout, storage, singletonList( singletonList( new BlockEntry<>( key, value ) ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSortAndAddMultipleEntriesInLastBlock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldSortAndAddMultipleEntriesInLastBlock()
		 {
			  // given
			  TrackingMonitor monitor = new TrackingMonitor();
			  int blockSize = 1_000;
			  IList<BlockEntry<MutableLong, MutableLong>> expected = new List<BlockEntry<MutableLong, MutableLong>>();
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( blockSize ), _fileSystem, _file, monitor ) )
			  {
					// when
					for ( int i = 0; i < 10; i++ )
					{
						 long keyNumber = Random.nextLong( 10_000_000 );
						 MutableLong key = new MutableLong( keyNumber );
						 MutableLong value = new MutableLong( i );
						 storage.Add( key, value );
						 expected.Add( new BlockEntry<>( key, value ) );
					}
					storage.DoneAdding();

					// then
					Sort( expected );
					AssertContents( _layout, storage, singletonList( expected ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSortAndAddMultipleEntriesInMultipleBlocks() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldSortAndAddMultipleEntriesInMultipleBlocks()
		 {
			  // given
			  TrackingMonitor monitor = new TrackingMonitor();
			  int blockSize = 1_000;
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( blockSize ), _fileSystem, _file, monitor ) )
			  {
					// when
					IList<IList<BlockEntry<MutableLong, MutableLong>>> expectedBlocks = AddACoupleOfBlocksOfEntries( monitor, storage, 3 );

					// then
					AssertContents( _layout, storage, expectedBlocks );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMergeWhenEmpty() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMergeWhenEmpty()
		 {
			  // given
			  TrackingMonitor monitor = new TrackingMonitor();
			  int blockSize = 1_000;
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( blockSize ), _fileSystem, _file, monitor ) )
			  {
					// when
					storage.Merge( RandomMergeFactor(), NOT_CANCELLABLE );

					// then
					assertEquals( 0, monitor.MergeIterationCallCount );
					AssertContents( _layout, storage, emptyList() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMergeSingleBlock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMergeSingleBlock()
		 {
			  // given
			  TrackingMonitor monitor = new TrackingMonitor();
			  int blockSize = 1_000;
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( blockSize ), _fileSystem, _file, monitor ) )
			  {
					IList<IList<BlockEntry<MutableLong, MutableLong>>> expectedBlocks = singletonList( AddEntries( storage, 4 ) );
					storage.DoneAdding();

					// when
					storage.Merge( RandomMergeFactor(), NOT_CANCELLABLE );

					// then
					assertEquals( 0, monitor.MergeIterationCallCount );
					AssertContents( _layout, storage, expectedBlocks );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMergeMultipleBlocks() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMergeMultipleBlocks()
		 {
			  // given
			  TrackingMonitor monitor = new TrackingMonitor();
			  int blockSize = 1_000;
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( blockSize ), _fileSystem, _file, monitor ) )
			  {
					int numberOfBlocks = Random.Next( 100 ) + 2;
					IList<IList<BlockEntry<MutableLong, MutableLong>>> expectedBlocks = AddACoupleOfBlocksOfEntries( monitor, storage, numberOfBlocks );
					storage.DoneAdding();

					// when
					storage.Merge( RandomMergeFactor(), NOT_CANCELLABLE );

					// then
					AssertContents( _layout, storage, AsOneBigBlock( expectedBlocks ) );
					assertThat( monitor.TotalEntriesToMerge, greaterThanOrEqualTo( monitor.EntryAddedCallCount ) );
					assertEquals( monitor.TotalEntriesToMerge, monitor.EntriesMergedConflict );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOnlyLeaveSingleFileAfterMerge() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldOnlyLeaveSingleFileAfterMerge()
		 {
			  TrackingMonitor monitor = new TrackingMonitor();
			  int blockSize = 1_000;
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( blockSize ), _fileSystem, _file, monitor ) )
			  {
					int numberOfBlocks = Random.Next( 100 ) + 2;
					AddACoupleOfBlocksOfEntries( monitor, storage, numberOfBlocks );
					storage.DoneAdding();

					// when
					storage.Merge( 2, NOT_CANCELLABLE );

					// then
					File[] files = _fileSystem.listFiles( Directory.directory() );
					assertEquals( 1, Files.Length, "Expected only a single file to exist after merge." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAcceptAddedEntriesAfterDoneAdding() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotAcceptAddedEntriesAfterDoneAdding()
		 {
			  // given
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( 100 ), _fileSystem, _file, NO_MONITOR ) )
			  {
					// when
					storage.DoneAdding();

					// then
					assertThrows( typeof( System.InvalidOperationException ), () => storage.add(new MutableLong(0), new MutableLong(1)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotFlushAnythingOnEmptyBufferInDoneAdding() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotFlushAnythingOnEmptyBufferInDoneAdding()
		 {
			  // given
			  TrackingMonitor monitor = new TrackingMonitor();
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( 100 ), _fileSystem, _file, monitor ) )
			  {
					// when
					storage.DoneAdding();

					// then
					assertEquals( 0, monitor.BlockFlushedCallCount );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNoticeCancelRequest() throws java.io.IOException, java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNoticeCancelRequest()
		 {
			  // given
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  TrackingMonitor monitor = new TrackingMonitorAnonymousInnerClass( this, barrier );
			  int blocks = 10;
			  int mergeFactor = 2;
			  MutableLongSet uniqueKeys = new LongHashSet();
			  AtomicBoolean cancelled = new AtomicBoolean();
			  using ( BlockStorage<MutableLong, MutableLong> storage = new BlockStorage<MutableLong, MutableLong>( _layout, heapBufferFactory( 100 ), _fileSystem, _file, monitor ), OtherThreadExecutor<Void> t2 = new OtherThreadExecutor<Void>( "T2", null ) )
			  {
					while ( monitor.BlockFlushedCallCount < blocks )
					{
						 storage.Add( UniqueKey( uniqueKeys ), new MutableLong() );
					}
					storage.DoneAdding();

					// when starting to merge
					Future<object> merge = t2.ExecuteDontWait( command( () => storage.merge(mergeFactor, cancelled.get) ) );
					barrier.AwaitUninterruptibly();
					// one merge iteration have now been done, set the cancellation flag
					cancelled.set( true );
					barrier.Release();
					merge.get();
			  }

			  // then there should not be any more merge iterations done, i.e. merge was cancelled
			  assertEquals( 1, monitor.MergeIterationCallCount );
		 }

		 private class TrackingMonitorAnonymousInnerClass : TrackingMonitor
		 {
			 private readonly BlockStorageTest _outerInstance;

			 private Neo4Net.Test.Barrier_Control _barrier;

			 public TrackingMonitorAnonymousInnerClass( BlockStorageTest outerInstance, Neo4Net.Test.Barrier_Control barrier )
			 {
				 this.outerInstance = outerInstance;
				 this._barrier = barrier;
			 }

			 public override void mergedBlocks( long resultingBlockSize, long resultingEntryCount, long numberOfBlocks )
			 {
				  base.mergedBlocks( resultingBlockSize, resultingEntryCount, numberOfBlocks );
				  _barrier.reached();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateCorrectNumberOfEntriesToWriteDuringMerge()
		 internal virtual void ShouldCalculateCorrectNumberOfEntriesToWriteDuringMerge()
		 {
			  // when
			  long entryCountForOneBlock = BlockStorage.CalculateNumberOfEntriesWrittenDuringMerges( 100, 1, 2 );
			  long entryCountForMergeFactorBlocks = BlockStorage.CalculateNumberOfEntriesWrittenDuringMerges( 100, 4, 4 );
			  long entryCountForMoreThanMergeFactorBlocks = BlockStorage.CalculateNumberOfEntriesWrittenDuringMerges( 100, 5, 4 );
			  long entryCountForThreeFactorsMergeFactorBlocks = BlockStorage.CalculateNumberOfEntriesWrittenDuringMerges( 100, 4 * 4 * 4 - 3, 4 );

			  // then
			  assertEquals( 0, entryCountForOneBlock );
			  assertEquals( 100, entryCountForMergeFactorBlocks );
			  assertEquals( 200, entryCountForMoreThanMergeFactorBlocks );
			  assertEquals( 300, entryCountForThreeFactorsMergeFactorBlocks );
		 }

		 private IEnumerable<IList<BlockEntry<MutableLong, MutableLong>>> AsOneBigBlock( IList<IList<BlockEntry<MutableLong, MutableLong>>> expectedBlocks )
		 {
			  IList<BlockEntry<MutableLong, MutableLong>> all = new List<BlockEntry<MutableLong, MutableLong>>();
			  foreach ( IList<BlockEntry<MutableLong, MutableLong>> expectedBlock in expectedBlocks )
			  {
					( ( IList<BlockEntry<MutableLong, MutableLong>> )all ).AddRange( expectedBlock );
			  }
			  Sort( all );
			  return singletonList( all );
		 }

		 private int RandomMergeFactor()
		 {
			  return Random.Next( 2, 8 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<BlockEntry<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong>> addEntries(BlockStorage<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong> storage, int numberOfEntries) throws java.io.IOException
		 private IList<BlockEntry<MutableLong, MutableLong>> AddEntries( BlockStorage<MutableLong, MutableLong> storage, int numberOfEntries )
		 {
			  MutableLongSet uniqueKeys = LongSets.mutable.empty();
			  IList<BlockEntry<MutableLong, MutableLong>> entries = new List<BlockEntry<MutableLong, MutableLong>>();
			  for ( int i = 0; i < numberOfEntries; i++ )
			  {
					MutableLong key = UniqueKey( uniqueKeys );
					MutableLong value = new MutableLong( Random.nextLong( 10_000_000 ) );
					storage.Add( key, value );
					entries.Add( new BlockEntry<>( key, value ) );
			  }
			  Sort( entries );
			  return entries;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<java.util.List<BlockEntry<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong>>> addACoupleOfBlocksOfEntries(TrackingMonitor monitor, BlockStorage<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong> storage, int numberOfBlocks) throws java.io.IOException
		 private IList<IList<BlockEntry<MutableLong, MutableLong>>> AddACoupleOfBlocksOfEntries( TrackingMonitor monitor, BlockStorage<MutableLong, MutableLong> storage, int numberOfBlocks )
		 {
			  Debug.Assert( numberOfBlocks != 1 );

			  MutableLongSet uniqueKeys = LongSets.mutable.empty();
			  IList<IList<BlockEntry<MutableLong, MutableLong>>> expected = new List<IList<BlockEntry<MutableLong, MutableLong>>>();
			  IList<BlockEntry<MutableLong, MutableLong>> currentExpected = new List<BlockEntry<MutableLong, MutableLong>>();
			  long currentBlock = 0;
			  while ( monitor.BlockFlushedCallCount < numberOfBlocks - 1 )
			  {
					MutableLong key = UniqueKey( uniqueKeys );
					MutableLong value = new MutableLong( Random.nextLong( 10_000_000 ) );

					storage.Add( key, value );
					if ( monitor.BlockFlushedCallCount > currentBlock )
					{
						 Sort( currentExpected );
						 expected.Add( currentExpected );
						 currentExpected = new List<BlockEntry<MutableLong, MutableLong>>();
						 currentBlock = monitor.BlockFlushedCallCount;
					}
					currentExpected.Add( new BlockEntry<>( key, value ) );
			  }
			  storage.DoneAdding();
			  if ( currentExpected.Count > 0 )
			  {
					expected.Add( currentExpected );
			  }
			  return expected;
		 }

		 private MutableLong UniqueKey( MutableLongSet uniqueKeys )
		 {
			  MutableLong key;
			  do
			  {
					key = new MutableLong( Random.nextLong( 10_000_000 ) );
			  } while ( !uniqueKeys.add( key.longValue() ) );
			  return key;
		 }

		 private void Sort( IList<BlockEntry<MutableLong, MutableLong>> entries )
		 {
			  entries.sort( comparingLong( p => p.key().longValue() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertContents(org.Neo4Net.index.internal.gbptree.SimpleLongLayout layout, BlockStorage<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong> storage, Iterable<java.util.List<BlockEntry<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong>>> expectedBlocks) throws java.io.IOException
		 private void AssertContents( SimpleLongLayout layout, BlockStorage<MutableLong, MutableLong> storage, IEnumerable<IList<BlockEntry<MutableLong, MutableLong>>> expectedBlocks )
		 {
			  using ( BlockReader<MutableLong, MutableLong> reader = storage.Reader() )
			  {
					foreach ( IList<BlockEntry<MutableLong, MutableLong>> expectedBlock in expectedBlocks )
					{
						 using ( BlockEntryReader<MutableLong, MutableLong> block = reader.NextBlock( HEAP_ALLOCATOR.allocate( 1024 ) ) )
						 {
							  assertNotNull( block );
							  assertEquals( expectedBlock.Count, block.EntryCount() );
							  foreach ( BlockEntry<MutableLong, MutableLong> expectedEntry in expectedBlock )
							  {
									assertTrue( block.Next() );
									assertEquals( 0, layout.Compare( expectedEntry.Key(), block.Key() ) );
									assertEquals( expectedEntry.Value(), block.Value() );
							  }
						 }
					}
			  }
		 }

		 private class TrackingMonitor : BlockStorage.Monitor
		 {
			  // For entryAdded
			  internal long EntryAddedCallCount;
			  internal int LastEntrySize;
			  internal long TotalEntrySize;

			  // For blockFlushed
			  internal int BlockFlushedCallCount;
			  internal long LastKeyCount;
			  internal int LastNumberOfBytes;
			  internal long LastPositionAfterFlush;

			  // For mergeIteration
			  internal int MergeIterationCallCount;
			  internal long LastNumberOfBlocksBefore;
			  internal long LastNumberOfBlocksAfter;

			  // For mergeStarted
			  internal long TotalEntriesToMerge;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long EntriesMergedConflict;

			  public override void EntryAdded( int entrySize )
			  {
					EntryAddedCallCount++;
					LastEntrySize = entrySize;
					TotalEntrySize += entrySize;
			  }

			  public override void BlockFlushed( long keyCount, int numberOfBytes, long positionAfterFlush )
			  {
					BlockFlushedCallCount++;
					LastKeyCount = keyCount;
					LastNumberOfBytes = numberOfBytes;
					LastPositionAfterFlush = positionAfterFlush;
			  }

			  public override void MergeIterationFinished( long numberOfBlocksBefore, long numberOfBlocksAfter )
			  {
					MergeIterationCallCount++;
					LastNumberOfBlocksBefore = numberOfBlocksBefore;
					LastNumberOfBlocksAfter = numberOfBlocksAfter;
			  }

			  public override void MergedBlocks( long resultingBlockSize, long resultingEntryCount, long numberOfBlocks )
			  { // no-op
			  }

			  public override void MergeStarted( long entryCount, long totalEntriesToWriteDuringMerge )
			  {
					this.TotalEntriesToMerge = totalEntriesToWriteDuringMerge;
			  }

			  public override void EntriesMerged( int entries )
			  {
					EntriesMergedConflict += entries;
			  }
		 }
	}

}