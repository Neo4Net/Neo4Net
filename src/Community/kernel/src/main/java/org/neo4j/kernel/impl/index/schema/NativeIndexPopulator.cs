using System.Collections.Generic;
using System.Text;

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

	using Neo4Net.Index.@internal.gbptree;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using Neo4Net.Index.@internal.gbptree;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using UniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.UniqueIndexSampler;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Preconditions = Neo4Net.Util.Preconditions;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_WRITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptor.Type.GENERAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptor.Type.UNIQUE;

	/// <summary>
	/// <seealso cref="IndexPopulator"/> backed by a <seealso cref="GBPTree"/>.
	/// </summary>
	/// @param <KEY> type of <seealso cref="NativeIndexSingleValueKey"/>. </param>
	/// @param <VALUE> type of <seealso cref="NativeIndexValue"/>. </param>
	public abstract class NativeIndexPopulator<KEY, VALUE> : NativeIndex<KEY, VALUE>, IndexPopulator, ConsistencyCheckable where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		public override abstract bool ConsistencyCheck( Neo4Net.Kernel.Impl.Annotations.ReporterFactory reporterFactory );
		public abstract void PutAllNoOverwrite( IDictionary<string, Value> target, IDictionary<string, Value> source );
		public abstract IDictionary<string, Value> IndexConfig();
		public abstract void ScanCompleted( Neo4Net.Kernel.Impl.Api.index.PhaseTracker phaseTracker );
		public abstract Neo4Net.Storageengine.Api.schema.PopulationProgress Progress( Neo4Net.Storageengine.Api.schema.PopulationProgress scanProgress );
		 public const sbyte BYTE_FAILED = 0;
		 internal const sbyte BYTE_ONLINE = 1;
		 internal const sbyte BYTE_POPULATING = 2;

		 private readonly KEY _treeKey;
		 private readonly VALUE _treeValue;
		 private readonly UniqueIndexSampler _uniqueSampler;
		 private readonly System.Action<PageCursor> _additionalHeaderWriter;

		 private ConflictDetectingValueMerger<KEY, VALUE, Value[]> _mainConflictDetector;
		 private ConflictDetectingValueMerger<KEY, VALUE, Value[]> _updatesConflictDetector;

		 private sbyte[] _failureBytes;
		 private bool _dropped;
		 private bool _closed;

		 internal NativeIndexPopulator( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<KEY, VALUE> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, System.Action<PageCursor> additionalHeaderWriter ) : base( pageCache, fs, storeFile, layout, monitor, descriptor, false )
		 {
			  this._treeKey = layout.newKey();
			  this._treeValue = layout.NewValue();
			  this._additionalHeaderWriter = additionalHeaderWriter;
			  switch ( descriptor.Type() )
			  {
			  case GENERAL:
					_uniqueSampler = null;
					break;
			  case UNIQUE:
					_uniqueSampler = new UniqueIndexSampler();
					break;
			  default:
					throw new System.ArgumentException( "Unexpected index type " + descriptor.Type() );
			  }
		 }

		 public virtual void Clear()
		 {
			  DeleteFileIfPresent( fileSystem, storeFile );
		 }

		 public override void Create()
		 {
			 lock ( this )
			 {
				  Create( new NativeIndexHeaderWriter( BYTE_POPULATING, _additionalHeaderWriter ) );
			 }
		 }

		 protected internal virtual void Create( System.Action<PageCursor> headerWriter )
		 {
			 lock ( this )
			 {
				  AssertNotDropped();
				  AssertNotClosed();
      
				  DeleteFileIfPresent( fileSystem, storeFile );
				  instantiateTree( RecoveryCleanupWorkCollector.immediate(), headerWriter );
      
				  // true:  tree uniqueness is (value,entityId)
				  // false: tree uniqueness is (value) <-- i.e. more strict
				  _mainConflictDetector = MainConflictDetector;
				  // for updates we have to have uniqueness on (value,entityId) to allow for intermediary violating updates.
				  // there are added conflict checks after updates have been applied.
				  _updatesConflictDetector = new ThrowingConflictDetector<KEY, VALUE, Value[]>( true );
			 }
		 }

		 internal virtual ConflictDetectingValueMerger<KEY, VALUE, Value[]> MainConflictDetector
		 {
			 get
			 {
				  return new ThrowingConflictDetector<KEY, VALUE, Value[]>( descriptor.type() == GENERAL );
			 }
		 }

		 public override void Drop()
		 {
			 lock ( this )
			 {
				  try
				  {
						closeTree();
						DeleteFileIfPresent( fileSystem, storeFile );
				  }
				  finally
				  {
						_dropped = true;
						_closed = true;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(java.util.Collection<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  ProcessUpdates( updates, _mainConflictDetector );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  // No-op, uniqueness is checked for each update in add(IndexEntryUpdate)
		 }

		 public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		 {
			  return NewPopulatingUpdater();
		 }

		 internal virtual IndexUpdater NewPopulatingUpdater()
		 {
			  IndexUpdater updater = new CollectingIndexUpdater( updates => processUpdates( updates, _updatesConflictDetector ) );
			  if ( descriptor.type() == UNIQUE && CanCheckConflictsWithoutStoreAccess() )
			  {
					// The index population detects conflicts on the fly, however for updates coming in we're in a position
					// where we cannot detect conflicts while applying, but instead afterwards.
					updater = new DeferredConflictCheckingIndexUpdater( updater, this.newReader, descriptor );
			  }
			  return updater;
		 }

		 internal virtual bool CanCheckConflictsWithoutStoreAccess()
		 {
			  return true;
		 }

		 internal abstract NativeIndexReader<KEY, VALUE> NewReader();

		 public override void Close( bool populationCompletedSuccessfully )
		 {
			 lock ( this )
			 {
				  if ( populationCompletedSuccessfully && _failureBytes != null )
				  {
						throw new System.InvalidOperationException( "Can't mark index as online after it has been marked as failure" );
				  }
      
				  try
				  {
						AssertNotDropped();
						if ( populationCompletedSuccessfully )
						{
							 // Successful and completed population
							 AssertPopulatorOpen();
							 MarkTreeAsOnline();
						}
						else if ( _failureBytes != null )
						{
							 // Failed population
							 EnsureTreeInstantiated();
							 MarkTreeAsFailed();
						}
						// else cancelled population. Here we simply close the tree w/o checkpointing it and it will look like POPULATING state on next open
				  }
				  finally
				  {
						closeTree();
						_closed = true;
				  }
			 }
		 }

		 private void AssertNotDropped()
		 {
			  if ( _dropped )
			  {
					throw new System.InvalidOperationException( "Populator has already been dropped." );
			  }
		 }

		 private void AssertNotClosed()
		 {
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "Populator has already been closed." );
			  }
		 }

		 public override void MarkAsFailed( string failure )
		 {
			  _failureBytes = failure.GetBytes( Encoding.UTF8 );
		 }

		 private void EnsureTreeInstantiated()
		 {
			  if ( tree == null )
			  {
					instantiateTree( RecoveryCleanupWorkCollector.ignore(), NO_HEADER_WRITER );
			  }
		 }

		 private void AssertPopulatorOpen()
		 {
			  if ( tree == null )
			  {
					throw new System.InvalidOperationException( "Populator has already been closed." );
			  }
		 }

		 private void MarkTreeAsFailed()
		 {
			  Preconditions.checkState( _failureBytes != null, "markAsFailed hasn't been called, populator not actually failed?" );
			  tree.checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited, new FailureHeaderWriter( _failureBytes ) );
		 }

		 internal virtual void MarkTreeAsOnline()
		 {
			  tree.checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited, new NativeIndexHeaderWriter( BYTE_ONLINE, _additionalHeaderWriter ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void processUpdates(Iterable<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> indexEntryUpdates, ConflictDetectingValueMerger<KEY,VALUE,org.neo4j.values.storable.Value[]> conflictDetector) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void ProcessUpdates<T1>( IEnumerable<T1> indexEntryUpdates, ConflictDetectingValueMerger<KEY, VALUE, Value[]> conflictDetector ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  try
			  {
					  using ( Writer<KEY, VALUE> writer = tree.writer() )
					  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> indexEntryUpdate : indexEntryUpdates)
						foreach ( IndexEntryUpdate<object> indexEntryUpdate in indexEntryUpdates )
						{
							 NativeIndexUpdater.ProcessUpdate( _treeKey, _treeValue, indexEntryUpdate, writer, conflictDetector );
						}
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  switch ( descriptor.type() )
			  {
			  case GENERAL:
					// Don't do anything here, we'll do a scan in the end instead
					break;
			  case UNIQUE:
					UpdateUniqueSample( update );
					break;
			  default:
					throw new System.ArgumentException( "Unexpected index type " + descriptor.type() );
			  }
		 }

		 private void UpdateUniqueSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  switch ( update.UpdateMode() )
			  {
			  case ADDED:
					_uniqueSampler.increment( 1 );
					break;
			  case REMOVED:
					_uniqueSampler.increment( -1 );
					break;
			  case CHANGED:
					break;
			  default:
					throw new System.ArgumentException( "Unsupported update mode type:" + update.UpdateMode() );
			  }
		 }

		 public override IndexSample SampleResult()
		 {
			  switch ( descriptor.type() )
			  {
			  case GENERAL:
					return ( new FullScanNonUniqueIndexSampler<>( tree, layout ) ).Result();
			  case UNIQUE:
					return _uniqueSampler.result();
			  default:
					throw new System.ArgumentException( "Unexpected index type " + descriptor.type() );
			  }
		 }

		 private static void DeleteFileIfPresent( FileSystemAbstraction fs, File storeFile )
		 {
			  try
			  {
					fs.DeleteFileOrThrow( storeFile );
			  }
			  catch ( NoSuchFileException )
			  {
					// File does not exist, we don't need to delete
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}