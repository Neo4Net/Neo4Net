using System;

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
namespace Org.Neo4j.Kernel.impl.index.labelscan
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;


	using Org.Neo4j.Cursor;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;
	using Header = Org.Neo4j.Index.@internal.gbptree.Header;
	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;
	using MetadataMismatchException = Org.Neo4j.Index.@internal.gbptree.MetadataMismatchException;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using TreeFileNotFoundException = Org.Neo4j.Index.@internal.gbptree.TreeFileNotFoundException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using AllEntriesLabelScanReader = Org.Neo4j.Kernel.api.labelscan.AllEntriesLabelScanReader;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using LabelScanWriter = Org.Neo4j.Kernel.api.labelscan.LabelScanWriter;
	using ReporterFactory = Org.Neo4j.Kernel.Impl.Annotations.ReporterFactory;
	using FullStoreChangeStream = Org.Neo4j.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asResourceIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iterator;

	/// <summary>
	/// <seealso cref="LabelScanStore"/> which is implemented using <seealso cref="GBPTree"/> atop a <seealso cref="PageCache"/>.
	/// Only a single writer is allowed at any given point in time so synchronization or merging of updates
	/// need to be handled externally.
	/// <para>
	/// About the <seealso cref="Layout"/> used in this instance of <seealso cref="GBPTree"/>:
	/// <ul>
	/// <li>
	/// Each keys is a combination of {@code labelId} and {@code nodeIdRange} ({@code nodeId/64}).
	/// </li>
	/// <li>
	/// Each value is a 64-bit bit set (a primitive {@code long}) where each set bit in it represents
	/// a node with that label, such that {@code nodeId = nodeIdRange+bitOffset}. Range size (e.g. 64 bits)
	/// is configurable on initial creation of the store, 8, 16, 32 or 64.
	/// </li>
	/// </ul>
	/// </para>
	/// <para>
	/// <seealso cref="force(IOLimiter)"/> is vital for allowing this store to be recoverable, and must be called
	/// whenever Neo4j performs a checkpoint.
	/// </para>
	/// <para>
	/// This store is backed by a single store file "neostore.labelscanstore.db".
	/// </para>
	/// </summary>
	public class NativeLabelScanStore : LabelScanStore
	{
		 /// <summary>
		 /// Written in header to indicate native label scan store is clean
		 /// </summary>
		 private static readonly sbyte _clean = ( sbyte ) 0x00;

		 /// <summary>
		 /// Written in header to indicate native label scan store is rebuilding
		 /// </summary>
		 private static readonly sbyte _needsRebuilding = ( sbyte ) 0x01;

		 /// <summary>
		 /// Whether or not this label scan store is read-only.
		 /// </summary>
		 private readonly bool _readOnly;

		 /// <summary>
		 /// Monitoring internal events.
		 /// </summary>
		 private readonly Org.Neo4j.Kernel.api.labelscan.LabelScanStore_Monitor _monitor;

		 /// <summary>
		 /// Monitors used to pass down monitor to underlying <seealso cref="GBPTree"/>
		 /// </summary>
		 private readonly Monitors _monitors;

		 /// <summary>
		 /// <seealso cref="PageCache"/> to <seealso cref="PageCache.map(File, int, java.nio.file.OpenOption...)"/>
		 /// store file backing this label scan store. Passed to <seealso cref="GBPTree"/>.
		 /// </summary>
		 private readonly PageCache _pageCache;

		 /// <summary>
		 /// Store file <seealso cref="PageCache.map(File, int, java.nio.file.OpenOption...)"/>.
		 /// </summary>
		 private readonly File _storeFile;

		 /// <summary>
		 /// Used in <seealso cref="start()"/> if the store is empty, where this will provide all data for fully populating
		 /// this label scan store. This can be the case when changing label scan store provider on an existing database.
		 /// </summary>
		 private readonly FullStoreChangeStream _fullStoreChangeStream;

		 /// <summary>
		 /// <seealso cref="FileSystemAbstraction"/> the backing file lives on.
		 /// </summary>
		 private readonly FileSystemAbstraction _fs;

		 /// <summary>
		 /// Page size to use for each tree node in <seealso cref="GBPTree"/>. Passed to <seealso cref="GBPTree"/>.
		 /// </summary>
		 private readonly int _pageSize;

		 /// <summary>
		 /// Used for all file operations on the gbpTree file.
		 /// </summary>
		 private readonly FileSystemAbstraction _fileSystem;

		 /// <summary>
		 /// Layout of the database.
		 /// </summary>
		 private readonly DatabaseLayout _directoryStructure;

		 /// <summary>
		 /// The index which backs this label scan store. Instantiated in <seealso cref="init()"/> and considered
		 /// started after call to <seealso cref="start()"/>.
		 /// </summary>
		 private GBPTree<LabelScanKey, LabelScanValue> _index;

		 /// <summary>
		 /// Set during <seealso cref="init()"/> if <seealso cref="start()"/> will need to rebuild the whole label scan store from
		 /// <seealso cref="FullStoreChangeStream"/>.
		 /// </summary>
		 private bool _needsRebuild;

		 /// <summary>
		 /// Passed to underlying <seealso cref="GBPTree"/> which use it to submit recovery cleanup jobs.
		 /// </summary>
		 private readonly RecoveryCleanupWorkCollector _recoveryCleanupWorkCollector;

		 /// <summary>
		 /// The single instance of <seealso cref="NativeLabelScanWriter"/> used for updates.
		 /// </summary>
		 private NativeLabelScanWriter _singleWriter;

		 /// <summary>
		 /// Monitor for all writes going into this label scan store.
		 /// </summary>
		 private NativeLabelScanWriter.WriteMonitor _writeMonitor;

		 /// <summary>
		 /// Write rebuilding bit to header.
		 /// </summary>
		 private static readonly System.Action<PageCursor> _needsRebuildingWriter = pageCursor => pageCursor.putByte( _needsRebuilding );

		 /// <summary>
		 /// Write clean header.
		 /// </summary>
		 private static readonly System.Action<PageCursor> _writeClean = pageCursor => pageCursor.putByte( _clean );

		 public NativeLabelScanStore( PageCache pageCache, DatabaseLayout directoryStructure, FileSystemAbstraction fs, FullStoreChangeStream fullStoreChangeStream, bool readOnly, Monitors monitors, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector ) : this( pageCache, directoryStructure, fs, fullStoreChangeStream, readOnly, monitors, recoveryCleanupWorkCollector, 0 )
		 {
		 }

		 /*
		  * Test access to be able to control page size.
		  */
		 internal NativeLabelScanStore( PageCache pageCache, DatabaseLayout directoryStructure, FileSystemAbstraction fs, FullStoreChangeStream fullStoreChangeStream, bool readOnly, Monitors monitors, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, int pageSize )
		 {
			  this._pageCache = pageCache;
			  this._fs = fs;
			  this._pageSize = pageSize;
			  this._fullStoreChangeStream = fullStoreChangeStream;
			  this._directoryStructure = directoryStructure;
			  this._storeFile = GetLabelScanStoreFile( directoryStructure );
			  this._readOnly = readOnly;
			  this._monitors = monitors;
			  this._monitor = monitors.NewMonitor( typeof( Org.Neo4j.Kernel.api.labelscan.LabelScanStore_Monitor ) );
			  this._recoveryCleanupWorkCollector = recoveryCleanupWorkCollector;
			  this._fileSystem = fs;
		 }

		 /// <summary>
		 /// Returns the file backing the label scan store.
		 /// </summary>
		 /// <param name="directoryStructure"> The store directory to use. </param>
		 /// <returns> the file backing the label scan store </returns>
		 public static File GetLabelScanStoreFile( DatabaseLayout directoryStructure )
		 {
			  return directoryStructure.LabelScanStore();
		 }

		 /// <returns> <seealso cref="LabelScanReader"/> capable of finding node ids with given label ids.
		 /// Readers will immediately see updates made by <seealso cref="LabelScanWriter"/>, although <seealso cref="LabelScanWriter"/>
		 /// may internally batch updates so functionality isn't reliable. The only given is that readers will
		 /// see at least updates from closed <seealso cref="LabelScanWriter writers"/>. </returns>
		 public override LabelScanReader NewReader()
		 {
			  return new NativeLabelScanReader( _index );
		 }

		 /// <summary>
		 /// Returns <seealso cref="LabelScanWriter"/> capable of making changes to this <seealso cref="LabelScanStore"/>.
		 /// Only a single writer is allowed at any given point in time.
		 /// </summary>
		 /// <returns> <seealso cref="LabelScanWriter"/> capable of making changes to this <seealso cref="LabelScanStore"/>. </returns>
		 /// <exception cref="IllegalStateException"> if someone else has already acquired a writer and hasn't yet
		 /// called <seealso cref="LabelScanWriter.close()"/>. </exception>
		 public override LabelScanWriter NewWriter()
		 {
			  if ( _readOnly )
			  {
					throw new System.NotSupportedException( "Can't create index writer in read only mode." );
			  }

			  try
			  {
					return Writer();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 /// <summary>
		 /// Forces all changes to <seealso cref="PageCache"/> and creates a checkpoint so that the <seealso cref="LabelScanStore"/>
		 /// is recoverable from this point, given that the same transactions which will be applied after this point
		 /// and non-clean shutdown will be applied again on next startup.
		 /// </summary>
		 /// <param name="limiter"> <seealso cref="IOLimiter"/>. </param>
		 public override void Force( IOLimiter limiter )
		 {
			  _index.checkpoint( limiter );
			  _writeMonitor.force();
		 }

		 public override AllEntriesLabelScanReader AllNodeLabelRanges()
		 {
			  System.Func<int, RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> seekProvider = labelId =>
			  {
				try
				{
					 return _index.seek( ( new LabelScanKey() ).Set(labelId, 0), (new LabelScanKey()).Set(labelId, long.MaxValue) );
				}
				catch ( IOException e )
				{
					 throw new Exception( e );
				}
			  };

			  int highestLabelId = -1;
			  try
			  {
					  using ( RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor = _index.seek( ( new LabelScanKey() ).Set(int.MaxValue, long.MaxValue), (new LabelScanKey()).Set(0, -1) ) )
					  {
						if ( cursor.Next() )
						{
							 highestLabelId = cursor.get().key().labelId;
						}
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  return new NativeAllEntriesLabelScanReader( seekProvider, highestLabelId );
		 }

		 /// <returns> store files, namely the single "neostore.labelscanstore.db" store file. </returns>
		 public override ResourceIterator<File> SnapshotStoreFiles()
		 {
			  return asResourceIterator( iterator( _storeFile ) );
		 }

		 /// <summary>
		 /// Instantiates the underlying <seealso cref="GBPTree"/> and its resources.
		 /// </summary>
		 /// <exception cref="IOException"> on <seealso cref="PageCache"/> exceptions. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws java.io.IOException
		 public override void Init()
		 {
			  _monitor.init();

			  bool storeExists = HasStore();
			  bool isDirty;
			  try
			  {
					_needsRebuild = !storeExists;
					if ( !storeExists )
					{
						 _monitor.noIndex();
					}

					isDirty = InstantiateTree();
			  }
			  catch ( MetadataMismatchException )
			  {
					// GBPTree is corrupt. Try to rebuild.
					isDirty = true;
			  }

			  _writeMonitor = LabelScanWriteMonitor.Enabled ? new LabelScanWriteMonitor( _fs, _directoryStructure ) : NativeLabelScanWriter.EMPTY;
			  _singleWriter = new NativeLabelScanWriter( 1_000, _writeMonitor );

			  if ( isDirty )
			  {
					_monitor.notValidIndex();
					if ( !_readOnly )
					{
						 DropStrict();
						 InstantiateTree();
					}
					_needsRebuild = true;
			  }
		 }

		 public override bool HasStore()
		 {
			  return _fileSystem.fileExists( _storeFile );
		 }

		 public override File GetLabelScanStoreFile()
		 {
			  return _storeFile;
		 }

		 /// <returns> true if instantiated tree needs to be rebuilt. </returns>
		 private bool InstantiateTree()
		 {
			  _monitors.addMonitorListener( TreeMonitor() );
			  GBPTree.Monitor monitor = _monitors.newMonitor( typeof( GBPTree.Monitor ) );
			  MutableBoolean isRebuilding = new MutableBoolean();
			  Header.Reader readRebuilding = headerData => isRebuilding.setValue( headerData.get() == _needsRebuilding );
			  try
			  {
					_index = new GBPTree<LabelScanKey, LabelScanValue>( _pageCache, _storeFile, new LabelScanLayout(), _pageSize, monitor, readRebuilding, _needsRebuildingWriter, _recoveryCleanupWorkCollector, _readOnly );
					return isRebuilding.Value;
			  }
			  catch ( TreeFileNotFoundException e )
			  {
					throw new System.InvalidOperationException( "Label scan store file could not be found, most likely this database needs to be recovered, file:" + _storeFile, e );
			  }
		 }

		 private GBPTree.Monitor TreeMonitor()
		 {
			  return new LabelIndexTreeMonitor( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void drop() throws java.io.IOException
		 public override void Drop()
		 {
			  try
			  {
					DropStrict();
			  }
			  catch ( NoSuchFileException )
			  {
					// Even better, it didn't even exist
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dropStrict() throws java.io.IOException
		 private void DropStrict()
		 {
			  if ( _index != null )
			  {
					_index.Dispose();
					_index = null;
			  }
			  _fileSystem.deleteFileOrThrow( _storeFile );
		 }

		 /// <summary>
		 /// Starts the store and makes it available for queries and updates.
		 /// Any required recovery must take place before calling this method.
		 /// </summary>
		 /// <exception cref="IOException"> on <seealso cref="PageCache"/> exceptions. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws java.io.IOException
		 public override void Start()
		 {
			  if ( _needsRebuild && !_readOnly )
			  {
					_monitor.rebuilding();
					long numberOfNodes;

					// Intentionally ignore read-only flag here when rebuilding.
					using ( LabelScanWriter writer = writer() )
					{
						 numberOfNodes = _fullStoreChangeStream.applyTo( writer );
					}

					_index.checkpoint( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited, _writeClean );

					_monitor.rebuilt( numberOfNodes );
					_needsRebuild = false;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private NativeLabelScanWriter writer() throws java.io.IOException
		 private NativeLabelScanWriter Writer()
		 {
			  return _singleWriter.initialize( _index.writer() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean isEmpty() throws java.io.IOException
		 public virtual bool Empty
		 {
			 get
			 {
				  using ( RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor = _index.seek( new LabelScanKey( 0, 0 ), new LabelScanKey( int.MaxValue, long.MaxValue ) ) )
				  {
						return !cursor.Next();
				  }
			 }
		 }

		 public override void Stop()
		 { // Not needed
		 }

		 /// <summary>
		 /// Shuts down this store so that no more queries or updates can be accepted.
		 /// </summary>
		 /// <exception cref="IOException"> on <seealso cref="PageCache"/> exceptions. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws java.io.IOException
		 public override void Shutdown()
		 {
			  if ( _index != null )
			  {
					_index.Dispose();
					_index = null;
					_writeMonitor.close();
			  }
		 }

		 public virtual bool ReadOnly
		 {
			 get
			 {
				  return _readOnly;
			 }
		 }

		 public virtual bool Dirty
		 {
			 get
			 {
				  return _index == null || _index.wasDirtyOnStartup();
			 }
		 }

		 public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		 {
			  return ConsistencyCheck( reporterFactory.GetClass( typeof( GBPTreeConsistencyCheckVisitor ) ) );
		 }

		 private bool ConsistencyCheck( GBPTreeConsistencyCheckVisitor<LabelScanKey> visitor )
		 {
			  try
			  {
					return _index.consistencyCheck( visitor );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private class LabelIndexTreeMonitor : GBPTree.Monitor_Adaptor
		 {
			 private readonly NativeLabelScanStore _outerInstance;

			 public LabelIndexTreeMonitor( NativeLabelScanStore outerInstance ) : base( outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void CleanupRegistered()
			  {
					outerInstance.monitor.RecoveryCleanupRegistered();
			  }

			  public override void CleanupStarted()
			  {
					outerInstance.monitor.RecoveryCleanupStarted();
			  }

			  public override void CleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
			  {
					outerInstance.monitor.RecoveryCleanupFinished( numberOfPagesVisited, numberOfCleanedCrashPointers, durationMillis );
			  }

			  public override void CleanupClosed()
			  {
					outerInstance.monitor.RecoveryCleanupClosed();
			  }

			  public override void CleanupFailed( Exception throwable )
			  {
					outerInstance.monitor.RecoveryCleanupFailed( throwable );
			  }
		 }
	}

}