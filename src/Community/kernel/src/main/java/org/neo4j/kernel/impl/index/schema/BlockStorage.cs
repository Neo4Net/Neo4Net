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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using MutableList = org.eclipse.collections.api.list.MutableList;
	using Lists = org.eclipse.collections.impl.factory.Lists;


	using Neo4Net.Index.@internal.gbptree;
	using IOUtils = Neo4Net.Io.IOUtils;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using Allocator = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory.Allocator;
	using Preconditions = Neo4Net.Util.Preconditions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.ceil;

	/// <summary>
	/// Transforms an unordered stream of key-value pairs (<seealso cref="BlockEntry"/>) to an ordered one. It does so in two phases:
	/// 1. ADD: Entries are added through <seealso cref="add(KEY, VALUE)"/>. Those entries are buffered in memory until there are enough of them to fill up a Block.
	/// At that point they are sorted and flushed out to a file. This file will eventually contain multiple Blocks that each contain many entries in sorted order.
	/// When there are no more entries to add <seealso cref="doneAdding()"/> is called and the last Block is flushed to file.
	/// 2. MERGE: By calling <seealso cref="merge(int, Cancellation)"/> (after <seealso cref="doneAdding()"/> has been called) the multiple Blocks are merge joined into a new file
	/// resulting in larger blocks of sorted entries. Those larger blocks are then merge joined back to the original file. Merging continues in this ping pong
	/// fashion until there is only a single large block in the resulting file. The entries are now ready to be read in sorted order,
	/// call <seealso cref="reader()"/>.
	/// </summary>
	internal class BlockStorage<KEY, VALUE> : System.IDisposable
	{
		 internal static readonly int BlockHeaderSize = Long.BYTES + Long.BYTES; // entryCount

		 private readonly Layout<KEY, VALUE> _layout;
		 private readonly FileSystemAbstraction _fs;
		 private readonly MutableList<BlockEntry<KEY, VALUE>> _bufferedEntries;
		 private readonly IComparer<BlockEntry<KEY, VALUE>> _comparator;
		 private readonly StoreChannel _storeChannel;
		 private readonly Monitor _monitor;
		 private readonly int _blockSize;
		 private readonly ByteBufferFactory _bufferFactory;
		 private readonly File _blockFile;
		 private long _numberOfBlocksInCurrentFile;
		 private int _currentBufferSize;
		 private bool _doneAdding;
		 private long _entryCount;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: BlockStorage(org.neo4j.index.internal.gbptree.Layout<KEY,VALUE> layout, ByteBufferFactory bufferFactory, org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File blockFile, Monitor monitor) throws java.io.IOException
		 internal BlockStorage( Layout<KEY, VALUE> layout, ByteBufferFactory bufferFactory, FileSystemAbstraction fs, File blockFile, Monitor monitor )
		 {
			  this._layout = layout;
			  this._fs = fs;
			  this._blockFile = blockFile;
			  this._monitor = monitor;
			  this._blockSize = bufferFactory.BufferSize();
			  this._bufferedEntries = Lists.mutable.empty();
			  this._bufferFactory = bufferFactory;
			  this._comparator = ( e0, e1 ) => layout.Compare( e0.key(), e1.key() );
			  this._storeChannel = fs.Create( blockFile );
			  ResetBufferedEntries();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(KEY key, VALUE value) throws java.io.IOException
		 public virtual void Add( KEY key, VALUE value )
		 {
			  Preconditions.checkState( !_doneAdding, "Cannot add more after done adding" );

			  int entrySize = BlockEntry.EntrySize( _layout, key, value );

			  if ( _currentBufferSize + entrySize > _blockSize )
			  {
					// append buffer to file and clear buffers
					FlushAndResetBuffer();
					_numberOfBlocksInCurrentFile++;
			  }

			  _bufferedEntries.add( new BlockEntry<>( key, value ) );
			  _currentBufferSize += entrySize;
			  _monitor.entryAdded( entrySize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void doneAdding() throws java.io.IOException
		 internal virtual void DoneAdding()
		 {
			  if ( !_bufferedEntries.Empty )
			  {
					FlushAndResetBuffer();
					_numberOfBlocksInCurrentFile++;
			  }
			  _doneAdding = true;
			  _storeChannel.close();
		 }

		 private void ResetBufferedEntries()
		 {
			  _bufferedEntries.clear();
			  _currentBufferSize = BlockHeaderSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void flushAndResetBuffer() throws java.io.IOException
		 private void FlushAndResetBuffer()
		 {
			  _bufferedEntries.sortThis( _comparator );

			  ListBasedBlockEntryCursor<KEY, VALUE> entries = new ListBasedBlockEntryCursor<KEY, VALUE>( _bufferedEntries );
			  ByteBuffer byteBuffer = _bufferFactory.acquireThreadLocalBuffer();
			  try
			  {
					WriteBlock( _storeChannel, entries, _blockSize, _bufferedEntries.size(), NotCancellable, count => _entryCount += count, byteBuffer );
			  }
			  finally
			  {
					_bufferFactory.releaseThreadLocalBuffer();
			  }

			  // Append to file
			  _monitor.blockFlushed( _bufferedEntries.size(), _currentBufferSize, _storeChannel.position() );
			  ResetBufferedEntries();
		 }

		 /// <summary>
		 /// There are two files: sourceFile and targetFile. Blocks are merged, mergeFactor at the time, from source to target. When all blocks from source have
		 /// been merged into a larger block one merge iteration is done and source and target are flipped. As long as source contain more than a single block more
		 /// merge iterations are needed and we start over again.
		 /// When source only contain a single block we are finished and the extra file is deleted and <seealso cref="blockFile"/> contains the result with a single sorted
		 /// block.
		 /// 
		 /// See <seealso cref="performSingleMerge(int, BlockReader, StoreChannel, Cancellation, ByteBuffer[], ByteBuffer)"/> for further details.
		 /// </summary>
		 /// <param name="mergeFactor"> See <seealso cref="performSingleMerge(int, BlockReader, StoreChannel, Cancellation, ByteBuffer[], ByteBuffer)"/>. </param>
		 /// <param name="cancellation"> Injected so that this merge can be cancelled, if an external request to do that comes in.
		 /// A cancelled merge will leave the same end state file/channel-wise, just not quite completed, which is fine because the merge
		 /// was cancelled meaning that the result will not be used for anything other than deletion. </param>
		 /// <exception cref="IOException"> If something goes wrong when reading from file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void merge(int mergeFactor, Cancellation cancellation) throws java.io.IOException
		 public virtual void Merge( int mergeFactor, Cancellation cancellation )
		 {
			  _monitor.mergeStarted( _entryCount, CalculateNumberOfEntriesWrittenDuringMerges( _entryCount, _numberOfBlocksInCurrentFile, mergeFactor ) );
			  File sourceFile = _blockFile;
			  File tempFile = new File( _blockFile.Parent, _blockFile.Name + ".b" );
			  try
			  {
					  using ( Allocator mergeBufferAllocator = _bufferFactory.newLocalAllocator() )
					  {
						File targetFile = tempFile;
      
						// Allocate all buffers that will be used and reused for all merge iterations
						ByteBuffer writeBuffer = mergeBufferAllocator.Allocate( _bufferFactory.bufferSize() );
						ByteBuffer[] readBuffers = new ByteBuffer[mergeFactor];
						for ( int i = 0; i < readBuffers.Length; i++ )
						{
							 readBuffers[i] = mergeBufferAllocator.Allocate( _bufferFactory.bufferSize() );
						}
      
						while ( _numberOfBlocksInCurrentFile > 1 )
						{
							 // Perform one complete merge iteration, merging all blocks from source into target.
							 // After this step, target will contain fewer blocks than source, but may need another merge iteration.
							 using ( BlockReader<KEY, VALUE> reader = reader( sourceFile ), StoreChannel targetChannel = _fs.open( targetFile, OpenMode.READ_WRITE ) )
							 {
								  long blocksMergedSoFar = 0;
								  long blocksInMergedFile = 0;
								  while ( !cancellation.Cancelled() && blocksMergedSoFar < _numberOfBlocksInCurrentFile )
								  {
										blocksMergedSoFar += PerformSingleMerge( mergeFactor, reader, targetChannel, cancellation, readBuffers, writeBuffer );
										blocksInMergedFile++;
								  }
								  _numberOfBlocksInCurrentFile = blocksInMergedFile;
								  _monitor.mergeIterationFinished( blocksMergedSoFar, blocksInMergedFile );
							 }
      
							 // Flip and restore the channels
							 File tmpSourceFile = sourceFile;
							 sourceFile = targetFile;
							 targetFile = tmpSourceFile;
						}
					  }
			  }
			  finally
			  {
					if ( sourceFile == _blockFile )
					{
						 _fs.deleteFile( tempFile );
					}
					else
					{
						 _fs.deleteFile( _blockFile );
						 _fs.renameFile( tempFile, _blockFile );
					}
			  }
		 }

		 /// <summary>
		 /// Calculates number of entries that will be written, given an entry count, number of blocks and a merge factor.
		 /// During merge entries are merged and written, potentially multiple times depending on number of blocks and merge factor.
		 /// </summary>
		 /// <param name="entryCount"> number of entries to merge. </param>
		 /// <param name="numberOfBlocks"> number of blocks that these entries exist in. </param>
		 /// <param name="mergeFactor"> the merge factor to use when merging. </param>
		 /// <returns> number of entries written in total when merging these entries, which exists in the given number of blocks,
		 /// merged with the given merge factor. </returns>
		 internal static long CalculateNumberOfEntriesWrittenDuringMerges( long entryCount, long numberOfBlocks, int mergeFactor )
		 {
			  int singleMerges = 0;
			  for ( long blocks = numberOfBlocks; blocks > 1; blocks = ( long ) ceil( ( double ) blocks / mergeFactor ) )
			  {
					singleMerges++;
			  }
			  return singleMerges * entryCount;
		 }

		 /// <summary>
		 /// Merge some number of blocks, how many is decided by mergeFactor, into a single sorted block. This is done by opening <seealso cref="BlockEntryReader"/> on each
		 /// block that we want to merge and give them to a <seealso cref="MergingBlockEntryReader"/>. The <seealso cref="BlockEntryReader"/>s are pulled from a <seealso cref="BlockReader"/>
		 /// that iterate over Blocks in file in sequential order.
		 /// 
		 /// <seealso cref="MergingBlockEntryReader"/> pull head from each <seealso cref="BlockEntryReader"/> and hand them out in sorted order, making the multiple entry readers look
		 /// like a single large and sorted entry reader.
		 /// 
		 /// The large block resulting from the merge is written down to targetChannel by calling
		 /// <seealso cref="writeBlock(StoreChannel, BlockEntryCursor, long, long, Cancellation, IntConsumer, ByteBuffer)"/>.
		 /// </summary>
		 /// <param name="mergeFactor"> How many blocks to merge at the same time. Influence how much memory will be used because each merge block will have it's own
		 /// <seealso cref="ByteBuffer"/> that they read from. </param>
		 /// <param name="reader"> The <seealso cref="BlockReader"/> to pull blocks / <seealso cref="BlockEntryReader"/>s from. </param>
		 /// <param name="targetChannel"> The <seealso cref="StoreChannel"/> to write the merge result to. Result will be appended to current position. </param>
		 /// <param name="cancellation"> Injected so that this merge can be cancelled, if an external request to do that comes in. </param>
		 /// <param name="readBuffers"> buffers for all block readers. </param>
		 /// <param name="writeBuffer"> buffer for writing merged blocks. </param>
		 /// <returns> The number of blocks that where merged, most often this will be equal to mergeFactor but can be less if there are fewer blocks left in source. </returns>
		 /// <exception cref="IOException"> If something goes wrong when reading from file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int performSingleMerge(int mergeFactor, BlockReader<KEY,VALUE> reader, org.neo4j.io.fs.StoreChannel targetChannel, Cancellation cancellation, ByteBuffer[] readBuffers, ByteBuffer writeBuffer) throws java.io.IOException
		 private int PerformSingleMerge( int mergeFactor, BlockReader<KEY, VALUE> reader, StoreChannel targetChannel, Cancellation cancellation, ByteBuffer[] readBuffers, ByteBuffer writeBuffer )
		 {
			  using ( MergingBlockEntryReader<KEY, VALUE> merger = new MergingBlockEntryReader<KEY, VALUE>( _layout ) )
			  {
					long blockSize = 0;
					long entryCount = 0;
					int blocksMerged = 0;
					for ( int i = 0; i < mergeFactor; i++ )
					{
						 readBuffers[i].clear();
						 BlockEntryReader<KEY, VALUE> source = reader.NextBlock( readBuffers[i] );
						 if ( source != null )
						 {
							  blockSize += source.BlockSize();
							  entryCount += source.EntryCount();
							  blocksMerged++;
							  merger.AddSource( source );
						 }
						 else
						 {
							  break;
						 }
					}

					writeBuffer.clear();
					WriteBlock( targetChannel, merger, blockSize, entryCount, cancellation, _monitor.entriesMerged, writeBuffer );
					_monitor.mergedBlocks( blockSize, entryCount, blocksMerged );
					return blocksMerged;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeBlock(org.neo4j.io.fs.StoreChannel targetChannel, BlockEntryCursor<KEY,VALUE> blockEntryCursor, long blockSize, long entryCount, Cancellation cancellation, System.Action<int> entryCountReporter, ByteBuffer byteBuffer) throws java.io.IOException
		 private void WriteBlock( StoreChannel targetChannel, BlockEntryCursor<KEY, VALUE> blockEntryCursor, long blockSize, long entryCount, Cancellation cancellation, System.Action<int> entryCountReporter, ByteBuffer byteBuffer )
		 {
			  WriteHeader( byteBuffer, blockSize, entryCount );
			  long actualDataSize = WriteEntries( targetChannel, byteBuffer, _layout, blockEntryCursor, cancellation, entryCountReporter );
			  WriteLastEntriesWithPadding( targetChannel, byteBuffer, blockSize - actualDataSize );
		 }

		 private static void WriteHeader( ByteBuffer byteBuffer, long blockSize, long entryCount )
		 {
			  byteBuffer.putLong( blockSize );
			  byteBuffer.putLong( entryCount );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY, VALUE> long writeEntries(org.neo4j.io.fs.StoreChannel targetChannel, ByteBuffer byteBuffer, org.neo4j.index.internal.gbptree.Layout<KEY,VALUE> layout, BlockEntryCursor<KEY,VALUE> blockEntryCursor, Cancellation cancellation, System.Action<int> entryCountReporter) throws java.io.IOException
		 private static long WriteEntries<KEY, VALUE>( StoreChannel targetChannel, ByteBuffer byteBuffer, Layout<KEY, VALUE> layout, BlockEntryCursor<KEY, VALUE> blockEntryCursor, Cancellation cancellation, System.Action<int> entryCountReporter )
		 {
			  // Loop over block entries
			  long actualDataSize = BlockHeaderSize;
			  ByteArrayPageCursor pageCursor = new ByteArrayPageCursor( byteBuffer );
			  int entryCountToReport = 0;
			  while ( blockEntryCursor.Next() )
			  {
					KEY key = blockEntryCursor.Key();
					VALUE value = blockEntryCursor.Value();
					int entrySize = BlockEntry.EntrySize( layout, key, value );
					actualDataSize += entrySize;
					entryCountToReport++;

					if ( byteBuffer.remaining() < entrySize )
					{
						 // First check if this merge have been cancelled, if so just break here, it's fine.
						 if ( cancellation.Cancelled() )
						 {
							  break;
						 }

						 // flush and reset + DON'T PAD!!!
						 byteBuffer.flip();
						 targetChannel.WriteAll( byteBuffer );
						 byteBuffer.clear();
						 entryCountReporter( entryCountToReport );
						 entryCountToReport = 0;
					}

					BlockEntry.Write( pageCursor, layout, key, value );
			  }
			  if ( entryCountToReport > 0 )
			  {
					entryCountReporter( entryCountToReport );
			  }
			  return actualDataSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeLastEntriesWithPadding(org.neo4j.io.fs.StoreChannel channel, ByteBuffer byteBuffer, long padding) throws java.io.IOException
		 private static void WriteLastEntriesWithPadding( StoreChannel channel, ByteBuffer byteBuffer, long padding )
		 {
			  bool didWrite;
			  do
			  {
					int toPadThisTime = ( int ) Math.Min( byteBuffer.remaining(), padding );
					sbyte[] padArray = new sbyte[toPadThisTime];
					byteBuffer.put( padArray );
					padding -= toPadThisTime;
					didWrite = byteBuffer.position() > 0;
					if ( didWrite )
					{
						 byteBuffer.flip();
						 channel.WriteAll( byteBuffer );
						 byteBuffer.clear();
					}
			  } while ( didWrite );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _storeChannel );
			  _fs.deleteFile( _blockFile );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: BlockReader<KEY,VALUE> reader() throws java.io.IOException
		 internal virtual BlockReader<KEY, VALUE> Reader()
		 {
			  return Reader( _blockFile );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private BlockReader<KEY,VALUE> reader(java.io.File file) throws java.io.IOException
		 private BlockReader<KEY, VALUE> Reader( File file )
		 {
			  return new BlockReader<KEY, VALUE>( _fs, file, _layout );
		 }

		 public interface Monitor
		 {
			  void EntryAdded( int entrySize );

			  void BlockFlushed( long keyCount, int numberOfBytes, long positionAfterFlush );

			  /// <param name="entryCount"> number of entries there are in this block storage. </param>
			  /// <param name="totalEntriesToWriteDuringMerge"> total entries that will be written, even accounting for that entries may need to be
			  /// written multiple times back and forth. </param>
			  void MergeStarted( long entryCount, long totalEntriesToWriteDuringMerge );

			  /// <param name="entries"> number of entries merged since last call. The sum of this value from all calls to this method
			  /// will in the end match the value provided in <seealso cref="mergeStarted(long, long)"/>. </param>
			  void EntriesMerged( int entries );

			  void MergeIterationFinished( long numberOfBlocksBefore, long numberOfBlocksAfter );

			  void MergedBlocks( long resultingBlockSize, long resultingEntryCount, long numberOfBlocks );
		 }

		 public static class Monitor_Fields
		 {
			 private readonly BlockStorage<KEY, VALUE> _outerInstance;

			 public Monitor_Fields( BlockStorage<KEY, VALUE> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public static readonly Monitor NoMonitor = new Adapter();
		 }

		  public class Monitor_Adapter : Monitor
		  {
			  private readonly BlockStorage<KEY, VALUE> _outerInstance;

			  public Monitor_Adapter( BlockStorage<KEY, VALUE> outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

				public override void EntryAdded( int entrySize )
				{ // no-op
				}

				public override void BlockFlushed( long keyCount, int numberOfBytes, long positionAfterFlush )
				{ // no-op
				}

				public override void MergeStarted( long entryCount, long totalEntriesToWriteDuringMerge )
				{ // no-op
				}

				public override void EntriesMerged( int entries )
				{ // no-op
				}

				public override void MergeIterationFinished( long numberOfBlocksBefore, long numberOfBlocksAfter )
				{ // no-op
				}

				public override void MergedBlocks( long resultingBlockSize, long resultingEntryCount, long numberOfBlocks )
				{ // no-op
				}
		  }

		  public class Monitor_Delegate : Monitor
		  {
			  private readonly BlockStorage<KEY, VALUE> _outerInstance;

				internal readonly Monitor Actual;

				internal Monitor_Delegate( BlockStorage<KEY, VALUE> outerInstance, Monitor actual )
				{
					this._outerInstance = outerInstance;
					 this.Actual = actual;
				}

				public override void EntryAdded( int entrySize )
				{
					 Actual.entryAdded( entrySize );
				}

				public override void BlockFlushed( long keyCount, int numberOfBytes, long positionAfterFlush )
				{
					 Actual.blockFlushed( keyCount, numberOfBytes, positionAfterFlush );
				}

				public override void MergeStarted( long entryCount, long totalEntriesToWriteDuringMerge )
				{
					 Actual.mergeStarted( entryCount, totalEntriesToWriteDuringMerge );
				}

				public override void EntriesMerged( int entries )
				{
					 Actual.entriesMerged( entries );
				}

				public override void MergeIterationFinished( long numberOfBlocksBefore, long numberOfBlocksAfter )
				{
					 Actual.mergeIterationFinished( numberOfBlocksBefore, numberOfBlocksAfter );
				}

				public override void MergedBlocks( long resultingBlockSize, long resultingEntryCount, long numberOfBlocks )
				{
					 Actual.mergedBlocks( resultingBlockSize, resultingEntryCount, numberOfBlocks );
				}
		  }

		 public interface Cancellation
		 {
			  bool Cancelled();
		 }

		 internal static readonly Cancellation NotCancellable = () => false;
	}

}