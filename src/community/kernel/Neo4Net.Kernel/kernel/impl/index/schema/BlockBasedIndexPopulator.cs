using System;
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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Index.Internal.gbptree;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using IOUtils = Neo4Net.Io.IOUtils;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using BatchingMultipleIndexPopulator = Neo4Net.Kernel.Impl.Api.index.BatchingMultipleIndexPopulator;
	using PhaseTracker = Neo4Net.Kernel.Impl.Api.index.PhaseTracker;
	using Allocator = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory.Allocator;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettingsWriter = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsWriter;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;
	using Preconditions = Neo4Net.Utils.Preconditions;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.first;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.kibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.BlockStorage.Monitor_Fields.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.NativeIndexUpdater.initializeKeyFromUpdate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.NativeIndexes.deleteIndex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.concurrent.Runnables.runAll;

	/// <summary>
	/// <seealso cref="IndexPopulator"/> for native indexes that stores scan updates in parallel append-only files. When all scan updates have been collected
	/// each file is sorted and then all of them merged together into the resulting index.
	/// 
	/// Note on buffers: basically each thread adding scan updates will make use of a <seealso cref="ByteBufferFactory.acquireThreadLocalBuffer() thread-local buffer"/>.
	/// This together with <seealso cref="ByteBufferFactory.globalAllocator() a global buffer for external updates"/> and carefully reused
	/// <seealso cref="ByteBufferFactory.newLocalAllocator() local buffers"/> for merging allows memory consumption to stay virtually the same regardless
	/// how many indexes are being built concurrently by the same job and regardless of index sizes. Formula for peak number of buffers in use is roughly
	/// {@code 10 * numberOfPopulationWorkers} where numberOfPopulationWorkers is currently capped to 8. So given a buffer size of 1 MiB then maximum memory
	/// usage for one population job (which can populate multiple index) is ~80 MiB.
	/// </summary>
	/// @param <KEY> </param>
	/// @param <VALUE> </param>
	public abstract class BlockBasedIndexPopulator<KEY, VALUE> : NativeIndexPopulator<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 public const string BLOCK_SIZE_NAME = "blockSize";

		 private readonly IndexDirectoryStructure _directoryStructure;
		 private readonly IndexDropAction _dropAction;
		 private readonly bool _archiveFailedIndex;
		 /// <summary>
		 /// When merging all blocks together the algorithm does multiple passes over the block storage, until the number of blocks reaches 1.
		 /// Every pass does one or more merges and every merge merges up to <seealso cref="mergeFactor"/> number of blocks into one block,
		 /// i.e. the number of blocks shrinks by a factor <seealso cref="mergeFactor"/> every pass, until one block is left.
		 /// </summary>
		 private readonly int _mergeFactor;
		 private readonly BlockStorage.Monitor _blockStorageMonitor;
		 // written to in a synchronized method when creating new thread-local instances, read from when population completes
		 private readonly IList<ThreadLocalBlockStorage> _allScanUpdates = new CopyOnWriteArrayList<ThreadLocalBlockStorage>();
		 private readonly ThreadLocal<ThreadLocalBlockStorage> _scanUpdates;
		 private readonly ByteBufferFactory _bufferFactory;
		 private IndexUpdateStorage<KEY, VALUE> _externalUpdates;
		 // written in a synchronized method when creating new thread-local instances, read when processing external updates
		 private volatile bool _scanCompleted;
		 private readonly CloseCancellation _cancellation = new CloseCancellation();
		 // Will be instantiated right before merging and can be used to neatly await merge to complete
		 private volatile System.Threading.CountdownEvent _mergeOngoingLatch;

		 // progress state
		 private volatile long _numberOfAppliedScanUpdates;
		 private volatile long _numberOfAppliedExternalUpdates;

		 internal BlockBasedIndexPopulator( PageCache pageCache, FileSystemAbstraction fs, File file, IndexLayout<KEY, VALUE> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, IndexSpecificSpaceFillingCurveSettingsCache spatialSettings, IndexDirectoryStructure directoryStructure, IndexDropAction dropAction, bool archiveFailedIndex, ByteBufferFactory bufferFactory ) : this( pageCache, fs, file, layout, monitor, descriptor, spatialSettings, directoryStructure, dropAction, archiveFailedIndex, bufferFactory, FeatureToggles.getInteger( typeof( BlockBasedIndexPopulator ), "mergeFactor", 8 ), NO_MONITOR )
		 {
		 }

		 internal BlockBasedIndexPopulator( PageCache pageCache, FileSystemAbstraction fs, File file, IndexLayout<KEY, VALUE> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, IndexSpecificSpaceFillingCurveSettingsCache spatialSettings, IndexDirectoryStructure directoryStructure, IndexDropAction dropAction, bool archiveFailedIndex, ByteBufferFactory bufferFactory, int mergeFactor, BlockStorage.Monitor blockStorageMonitor ) : base( pageCache, fs, file, layout, monitor, descriptor, new SpaceFillingCurveSettingsWriter( spatialSettings ) )
		 {
			  this._directoryStructure = directoryStructure;
			  this._dropAction = dropAction;
			  this._archiveFailedIndex = archiveFailedIndex;
			  this._mergeFactor = mergeFactor;
			  this._blockStorageMonitor = blockStorageMonitor;
			  this._scanUpdates = ThreadLocal.withInitial( this.newThreadLocalBlockStorage );
			  this._bufferFactory = bufferFactory;
		 }

		 private ThreadLocalBlockStorage NewThreadLocalBlockStorage()
		 {
			 lock ( this )
			 {
				  Preconditions.checkState( !_cancellation.cancelled(), "Already closed" );
				  Preconditions.checkState( !_scanCompleted, "Scan has already been completed" );
				  try
				  {
						int id = _allScanUpdates.Count;
						ThreadLocalBlockStorage blockStorage = new ThreadLocalBlockStorage( this, id );
						_allScanUpdates.Add( blockStorage );
						return blockStorage;
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }
		 }

		 /// <summary>
		 /// Base size of blocks of entries. As entries gets written to a BlockStorage, they are buffered up to this size, then sorted and written out.
		 /// As blocks gets merged into bigger blocks, this is still the size of the read buffer for each block no matter its size.
		 /// Each thread has its own buffer when writing and each thread has <seealso cref="mergeFactor"/> buffers when merging.
		 /// The memory usage will be at its biggest during merge and a total memory usage sum can be calculated like so:
		 /// 
		 /// blockSize * numberOfPopulationWorkers * <seealso cref="mergeFactor"/>
		 /// 
		 /// where typically <seealso cref="BatchingMultipleIndexPopulator"/> controls the number of population workers. The setting
		 /// `unsupported.dbms.multi_threaded_schema_index_population_enabled` controls whether or not the multi-threaded <seealso cref="BatchingMultipleIndexPopulator"/>
		 /// is used, otherwise a single-threaded populator is used instead.
		 /// </summary>
		 public static int ParseBlockSize()
		 {
			  long blockSize = ByteUnit.parse( FeatureToggles.getString( typeof( BlockBasedIndexPopulator ), BLOCK_SIZE_NAME, "1M" ) );
			  Preconditions.checkArgument( blockSize >= 20 && blockSize < int.MaxValue, "Block size need to fit in int. Was " + blockSize );
			  return ( int ) blockSize;
		 }

		 public override void Create()
		 {
			  try
			  {
					deleteIndex( fileSystem, _directoryStructure, descriptor.Id, _archiveFailedIndex );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  base.Create();
			  try
			  {
					File externalUpdatesFile = new File( storeFile.Parent, storeFile.Name + ".ext" );
					_externalUpdates = new IndexUpdateStorage<KEY, VALUE>( fileSystem, externalUpdatesFile, _bufferFactory.globalAllocator(), SmallerBufferSize(), layout );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private int SmallerBufferSize()
		 {
			  return _bufferFactory.bufferSize() / 2;
		 }

		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  if ( updates.Count > 0 )
			  {
					BlockStorage<KEY, VALUE> blockStorage = _scanUpdates.get().blockStorage;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Neo4Net.kernel.api.index.IndexEntryUpdate<?> update : updates)
					foreach ( IndexEntryUpdate<object> update in updates )
					{
						 StoreUpdate( update, blockStorage );
					}
			  }
		 }

		 private void StoreUpdate( long IEntityId, Value[] values, BlockStorage<KEY, VALUE> blockStorage )
		 {
			  try
			  {
					KEY key = layout.newKey();
					VALUE value = layout.newValue();
					initializeKeyFromUpdate( key, IEntityId, values );
					value.From( values );
					blockStorage.Add( key, value );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void StoreUpdate<T1>( IndexEntryUpdate<T1> update, BlockStorage<KEY, VALUE> blockStorage )
		 {
			  StoreUpdate( update.EntityId, update.Values(), blockStorage );
		 }

		 private bool MarkMergeStarted()
		 {
			 lock ( this )
			 {
				  _scanCompleted = true;
				  if ( _cancellation.cancelled() )
				  {
						return false;
				  }
				  _mergeOngoingLatch = new System.Threading.CountdownEvent( 1 );
				  return true;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void scanCompleted(Neo4Net.kernel.impl.api.index.PhaseTracker phaseTracker) throws Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void ScanCompleted( PhaseTracker phaseTracker )
		 {
			  if ( !MarkMergeStarted() )
			  {
					// This populator has already been closed, either from an external cancel or drop call.
					// Either way we're not supposed to do this merge.
					return;
			  }

			  try
			  {
					phaseTracker.EnterPhase( Neo4Net.Kernel.Impl.Api.index.PhaseTracker_Phase.Merge );
					if ( _allScanUpdates.Count > 0 )
					{
						 MergeScanUpdates();
					}

					_externalUpdates.doneAdding();
					// don't merge and sort the external updates

					// Build the tree from the scan updates
					if ( _cancellation.cancelled() )
					{
						 // Do one additional check before starting to write to the tree
						 return;
					}
					phaseTracker.EnterPhase( Neo4Net.Kernel.Impl.Api.index.PhaseTracker_Phase.Build );
					File duplicatesFile = new File( storeFile.ParentFile, storeFile.Name + ".dup" );
					int readBufferSize = SmallerBufferSize();
					using ( Allocator allocator = _bufferFactory.newLocalAllocator(), IndexKeyStorage<KEY> indexKeyStorage = new IndexKeyStorage<KEY>(fileSystem, duplicatesFile, allocator, readBufferSize, layout) )
					{
						 RecordingConflictDetector<KEY, VALUE> recordingConflictDetector = new RecordingConflictDetector<KEY, VALUE>( !descriptor.Unique, indexKeyStorage );
						 WriteScanUpdatesToTree( recordingConflictDetector, allocator, readBufferSize );

						 // Apply the external updates
						 phaseTracker.EnterPhase( Neo4Net.Kernel.Impl.Api.index.PhaseTracker_Phase.ApplyExternal );
						 WriteExternalUpdatesToTree( recordingConflictDetector );

						 // Verify uniqueness
						 if ( descriptor.Unique )
						 {
							  using ( IndexKeyStorage.KeyEntryCursor<KEY> allConflictingKeys = recordingConflictDetector.AllConflicts() )
							  {
									VerifyUniqueKeys( allConflictingKeys );
							  }
						 }
					}
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  catch ( InterruptedException e )
			  {
					Thread.CurrentThread.Interrupt();
					throw new Exception( "Got interrupted, so merge not completed", e );
			  }
			  catch ( ExecutionException e )
			  {
					// Propagating merge exception from other thread
					Exception executionException = e.InnerException;
					if ( executionException is Exception )
					{
						 throw ( Exception ) executionException;
					}
					throw new Exception( executionException );
			  }
			  finally
			  {
					_mergeOngoingLatch.Signal();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void mergeScanUpdates() throws InterruptedException, java.util.concurrent.ExecutionException, java.io.IOException
		 private void MergeScanUpdates()
		 {
			  ExecutorService executorService = Executors.newFixedThreadPool( _allScanUpdates.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> mergeFutures = new java.util.ArrayList<>();
			  IList<Future<object>> mergeFutures = new List<Future<object>>();
			  foreach ( ThreadLocalBlockStorage part in _allScanUpdates )
			  {
					BlockStorage<KEY, VALUE> scanUpdates = part.BlockStorage;
					// Call doneAdding here so that the buffer it allocates if it needs to flush something will be shared with other indexes
					scanUpdates.DoneAdding();
					mergeFutures.Add(executorService.submit(() =>
					{
					 scanUpdates.Merge( _mergeFactor, _cancellation );
					 return null;
					}));
			  }
			  executorService.shutdown();
			  while ( !executorService.awaitTermination( 1, TimeUnit.SECONDS ) )
			  {
					// just wait longer
			  }
			  // Let potential exceptions in the merge threads have a chance to propagate
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> mergeFuture : mergeFutures)
			  foreach ( Future<object> mergeFuture in mergeFutures )
			  {
					mergeFuture.get();
			  }
		 }

		 /// <summary>
		 /// We will loop over all external updates once to add them to the tree. This is done without checking any uniqueness.
		 /// If index is a uniqueness index we will then loop over external updates again and for each ADD or CHANGED update
		 /// we will verify that those entries are unique in the tree and throw as soon as we find a duplicate. </summary>
		 /// <exception cref="IOException"> If something goes wrong while reading from index. </exception>
		 /// <exception cref="IndexEntryConflictException"> If a duplicate is found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeExternalUpdatesToTree(RecordingConflictDetector<KEY,VALUE> recordingConflictDetector) throws java.io.IOException, Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void WriteExternalUpdatesToTree( RecordingConflictDetector<KEY, VALUE> recordingConflictDetector )
		 {
			  using ( Writer<KEY, VALUE> writer = tree.writer(), IndexUpdateCursor<KEY, VALUE> updates = _externalUpdates.reader() )
			  {
					while ( updates.Next() && !_cancellation.cancelled() )
					{
						 switch ( updates.UpdateMode() )
						 {
						 case ADDED:
							  WriteToTree( writer, recordingConflictDetector, updates.Key(), updates.Value() );
							  break;
						 case REMOVED:
							  writer.Remove( updates.Key() );
							  break;
						 case CHANGED:
							  writer.Remove( updates.Key() );
							  WriteToTree( writer, recordingConflictDetector, updates.Key2(), updates.Value() );
							  break;
						 default:
							  throw new System.ArgumentException( "Unknown update mode " + updates.UpdateMode() );
						 }
						 _numberOfAppliedExternalUpdates++;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyUniqueKeys(IndexKeyStorage.KeyEntryCursor<KEY> allConflictingKeys) throws java.io.IOException, Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void VerifyUniqueKeys( IndexKeyStorage.KeyEntryCursor<KEY> allConflictingKeys )
		 {
			  while ( allConflictingKeys.Next() && !_cancellation.cancelled() )
			  {
					KEY key = allConflictingKeys.Key();
					key.CompareId = false;
					VerifyUniqueSeek( tree.seek( key, key ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyUniqueSeek(Neo4Net.cursor.RawCursor<Neo4Net.index.internal.gbptree.Hit<KEY,VALUE>,java.io.IOException> seek) throws java.io.IOException, Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void VerifyUniqueSeek( IRawCursor<Hit<KEY, VALUE>, IOException> seek )
		 {
			  if ( seek != null )
			  {
					if ( seek.Next() )
					{
						 long firstEntityId = seek.get().key().EntityId;
						 if ( seek.Next() )
						 {
							  long secondEntityId = seek.get().key().EntityId;
							  throw new IndexEntryConflictException( firstEntityId, secondEntityId, seek.get().key().asValues() );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeScanUpdatesToTree(RecordingConflictDetector<KEY,VALUE> recordingConflictDetector, Neo4Net.kernel.impl.index.schema.ByteBufferFactory.Allocator allocator, int bufferSize) throws java.io.IOException, Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void WriteScanUpdatesToTree( RecordingConflictDetector<KEY, VALUE> recordingConflictDetector, Allocator allocator, int bufferSize )
		 {
			  using ( MergingBlockEntryReader<KEY, VALUE> allEntries = new MergingBlockEntryReader<KEY, VALUE>( layout ) )
			  {
					ByteBuffer singleBlockAssertionBuffer = allocator.Allocate( ( int ) kibiBytes( 8 ) );
					foreach ( ThreadLocalBlockStorage part in _allScanUpdates )
					{
						 using ( BlockReader<KEY, VALUE> reader = part.BlockStorage.reader() )
						 {
							  BlockEntryReader<KEY, VALUE> singleMergedBlock = reader.NextBlock( allocator.Allocate( bufferSize ) );
							  if ( singleMergedBlock != null )
							  {
									allEntries.AddSource( singleMergedBlock );
									// Pass in some sort of ByteBuffer here. The point is that there should be no more data to read,
									// if there is then it's due to a bug in the code and must be fixed.
									if ( reader.NextBlock( singleBlockAssertionBuffer ) != null )
									{
										 throw new System.InvalidOperationException( "Final BlockStorage had multiple blocks" );
									}
							  }
						 }
					}

					int asMuchAsPossibleToTheLeft = 1;
					using ( Writer<KEY, VALUE> writer = tree.writer( asMuchAsPossibleToTheLeft ) )
					{
						 while ( allEntries.Next() && !_cancellation.cancelled() )
						 {
							  WriteToTree( writer, recordingConflictDetector, allEntries.Key(), allEntries.Value() );
							  _numberOfAppliedScanUpdates++;
						 }
					}
			  }
		 }

		 public override IndexUpdater NewPopulatingUpdater()
		 {
			  if ( _scanCompleted )
			  {
					// Will need the reader from newReader, which a sub-class of this class implements
					return base.NewPopulatingUpdater();
			  }

			  return new IndexUpdaterAnonymousInnerClass( this );
		 }

		 private class IndexUpdaterAnonymousInnerClass : IndexUpdater
		 {
			 private readonly BlockBasedIndexPopulator<KEY, VALUE> _outerInstance;

			 public IndexUpdaterAnonymousInnerClass( BlockBasedIndexPopulator<KEY, VALUE> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 private volatile bool closed;

			 public void process<T1>( IndexEntryUpdate<T1> update )
			 {
				  assertOpen();
				  try
				  {
						_outerInstance.externalUpdates.add( update );
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }

			 public void close()
			 {
				  closed = true;
			 }

			 private void assertOpen()
			 {
				  if ( closed )
				  {
						throw new System.InvalidOperationException( "Updater has been closed" );
				  }
			 }
		 }

		 public override void Drop()
		 {
			 lock ( this )
			 {
				  runAll( "Failed while trying to drop index", this.closeBlockStorage, base.drop, () => _dropAction.drop(descriptor.Id, _archiveFailedIndex) );
			 }
		 }

		 public override void Close( bool populationCompletedSuccessfully )
		 {
			 lock ( this )
			 {
				  runAll( "Failed while trying to close index", this.closeBlockStorage, () => base.close(populationCompletedSuccessfully) );
			 }
		 }

		 // Always called from synchronized method
		 private void CloseBlockStorage()
		 {
			  // This method may be called while scanCompleted is running. This could be a drop or shutdown(?) which happens when this population
			  // is in its final stages. scanCompleted merges things in multiple threads. Those threads will abort when they see that setCancel
			  // has been called.
			  _cancellation.setCancel();

			  // If there's a merge concurrently running it will very soon notice the cancel request and abort whatever it's doing as soon as it can.
			  // Let's wait for that merge to be fully aborted by simply waiting for the merge latch.
			  if ( _mergeOngoingLatch != null )
			  {
					try
					{
						 // We want to await any ongoing merge because it becomes problematic to close the channels otherwise
						 _mergeOngoingLatch.await();
					}
					catch ( InterruptedException )
					{
						 Thread.CurrentThread.Interrupt();
						 // We still want to go ahead and try to close things properly, so get by only restoring the interrupt flag on the thread
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  IList<System.IDisposable> toClose = _allScanUpdates.Select( local => local.blockStorage ).collect( Collectors.toCollection( List<object>::new ) );
			  toClose.Add( _externalUpdates );
			  IOUtils.closeAllUnchecked( toClose );
		 }

		 public override PopulationProgress Progress( PopulationProgress scanProgress )
		 {
			  // A general note on scanProgress.getTotal(). Before the scan is completed most progress parts will base their estimates on that value.
			  // It is known that it may be slightly higher since it'll be based on store high-id, not the actual count.
			  // This is fine, but it creates this small "jump" in the progress in the middle somewhere when it switches from scan to merge.
			  // This also exists in the most basic population progress reports, but there it will be less visible since it will jump from
			  // some close-to-100 percentage to 100% and ONLINE.

			  // This progress report will consist of a couple of smaller parts, weighted differently based on empirically collected values.
			  // The weights will not be absolutely correct in all environments, but they don't have to be either, it will just result in some
			  // slices of the percentage progression range progressing at slightly different paces. However, progression of progress reporting
			  // naturally fluctuates anyway due to data set and I/O etc. so this is not an actual problem.
			  Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress_MultiBuilder builder = PopulationProgress.multiple();

			  // Add scan progress (this one weights a bit heavier than the others)
			  builder.Add( scanProgress, 4 );

			  // Add merge progress
			  if ( _allScanUpdates.Count > 0 )
			  {
					// The parts are merged in parallel so just take the first one and it will represent the whole merge progress.
					// It will be fairly accurate, but slightly off sometimes if other threads gets scheduling problems, i.e. if this part
					// finish far apart from others.
					long completed = 0;
					long total = 0;
					if ( _scanCompleted )
					{
						 // We know the actual entry count to write during merge since we have been monitoring those values
						 ThreadLocalBlockStorage part = first( _allScanUpdates );
						 completed = part.EntriesMergedConflict;
						 total = part.TotalEntriesToMerge;
					}
					builder.Add( PopulationProgress.single( completed, total ), 1 );
			  }

			  // Add tree building incl. external updates
			  PopulationProgress treeBuildProgress;
			  if ( _allScanUpdates.All( part => part.mergeStarted ) )
			  {
					long entryCount = _allScanUpdates.Select( part => part.count ).Sum() + _externalUpdates.count();
					treeBuildProgress = PopulationProgress.single( _numberOfAppliedScanUpdates + _numberOfAppliedExternalUpdates, entryCount );
			  }
			  else
			  {
					treeBuildProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress_Fields.None;
			  }
			  builder.Add( treeBuildProgress, 2 );

			  return builder.Build();
		 }

		 /// <summary>
		 /// Write key and value to tree and record duplicates if any.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void WriteToTree(Neo4Net.index.internal.gbptree.Writer<KEY,VALUE> writer, RecordingConflictDetector<KEY,VALUE> recordingConflictDetector, KEY key, VALUE value) throws Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void WriteToTree( Writer<KEY, VALUE> writer, RecordingConflictDetector<KEY, VALUE> recordingConflictDetector, KEY key, VALUE value )
		 {
			  recordingConflictDetector.controlConflictDetection( key );
			  writer.Merge( key, value, recordingConflictDetector );
			  HandleMergeConflict( writer, recordingConflictDetector, key, value );
		 }

		 /// <summary>
		 /// Will check if recording conflict detector saw a conflict. If it did, that conflict has been recorded and we will verify uniqueness for this
		 /// value later on. But for now we try and insert conflicting value again but with a relaxed uniqueness constraint. Insert is done with a throwing
		 /// conflict checker which means it will throw if we see same value AND same id in one key.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void handleMergeConflict(Neo4Net.index.internal.gbptree.Writer<KEY,VALUE> writer, RecordingConflictDetector<KEY,VALUE> recordingConflictDetector, KEY key, VALUE value) throws Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void HandleMergeConflict( Writer<KEY, VALUE> writer, RecordingConflictDetector<KEY, VALUE> recordingConflictDetector, KEY key, VALUE value )
		 {
			  if ( recordingConflictDetector.wasConflicting() )
			  {
					// Report conflict
					KEY copy = layout.newKey();
					layout.copyKey( key, copy );
					recordingConflictDetector.reportConflict( copy );

					// Insert and overwrite with relaxed uniqueness constraint
					recordingConflictDetector.RelaxUniqueness( key );
					writer.Put( key, value );
			  }
		 }

		 /// <summary>
		 /// Keeps track of a <seealso cref="BlockStorage"/> instance as well as monitoring some aspects of it to be able to provide a fairly accurate
		 /// progress report from <seealso cref="BlockBasedIndexPopulator.progress(PopulationProgress)"/>.
		 /// </summary>
		 private class ThreadLocalBlockStorage : BlockStorage.Monitor_Delegate
		 {
			 private readonly BlockBasedIndexPopulator<KEY, VALUE> _outerInstance;

			  internal readonly BlockStorage<KEY, VALUE> BlockStorage;
			  internal volatile long Count;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool MergeStartedConflict;
			  internal volatile long TotalEntriesToMerge;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile long EntriesMergedConflict;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ThreadLocalBlockStorage(int id) throws java.io.IOException
			  internal ThreadLocalBlockStorage( BlockBasedIndexPopulator<KEY, VALUE> outerInstance, int id ) : base( outerInstance.blockStorageMonitor )
			  {
				  this._outerInstance = outerInstance;
					File blockFile = new File( storeFile.ParentFile, storeFile.Name + ".scan-" + id );
					this.BlockStorage = new BlockStorage<KEY, VALUE>( layout, outerInstance.bufferFactory, fileSystem, blockFile, this );
			  }

			  public override void MergeStarted( long entryCount, long totalEntriesToWriteDuringMerge )
			  {
					base.MergeStarted( entryCount, totalEntriesToWriteDuringMerge );
					this.Count = entryCount;
					this.TotalEntriesToMerge = totalEntriesToWriteDuringMerge;
					this.MergeStartedConflict = true;
			  }

			  public override void EntriesMerged( int entries )
			  {
					base.EntriesMerged( entries );
					EntriesMergedConflict += entries;
			  }
		 }

		 private class CloseCancellation : BlockStorage.Cancellation
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool CancelledConflict;

			  internal virtual void SetCancel()
			  {
					this.CancelledConflict = true;
			  }

			  public override bool Cancelled()
			  {
					return CancelledConflict;
			  }
		 }

		 private class RecordingConflictDetector<KEY, VALUE> : ConflictDetectingValueMerger<KEY, VALUE, KEY> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
			  internal readonly IndexKeyStorage<KEY> AllConflictingKeys;

			  internal RecordingConflictDetector( bool compareEntityIds, IndexKeyStorage<KEY> indexKeyStorage ) : base( compareEntityIds )
			  {
					AllConflictingKeys = indexKeyStorage;
			  }

			  internal override void DoReportConflict( long existingNodeId, long addedNodeId, KEY conflictingKey )
			  {
					try
					{
						 AllConflictingKeys.add( conflictingKey );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexKeyStorage.KeyEntryCursor<KEY> allConflicts() throws java.io.IOException
			  internal virtual IndexKeyStorage.KeyEntryCursor<KEY> AllConflicts()
			  {
					AllConflictingKeys.doneAdding();
					return AllConflictingKeys.reader();
			  }

			  internal virtual void RelaxUniqueness( KEY key )
			  {
					key.CompareId = true;
			  }
		 }
	}

}