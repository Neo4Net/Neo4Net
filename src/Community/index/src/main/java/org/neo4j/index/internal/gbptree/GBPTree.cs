using System;
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
namespace Neo4Net.Index.@internal.gbptree
{
	using Pair = org.apache.commons.lang3.tuple.Pair;


	using Neo4Net.Cursors;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using CursorException = Neo4Net.Io.pagecache.CursorException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.withMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.Generation.generation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.Generation.stableGeneration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.Generation.unstableGeneration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointer.MIN_GENERATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.Header.CARRY_OVER_PREVIOUS_HEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.Header.replace;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.checkOutOfBounds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PointerChecking.assertNoSuccessor;

	/// <summary>
	/// A generation-aware B+tree (GB+Tree) implementation directly atop a <seealso cref="PageCache"/> with no caching in between.
	/// Additionally internal and leaf nodes on same level are linked both left and right (sibling pointers),
	/// this to provide correct reading when concurrently <seealso cref="writer() modifying"/>
	/// the tree.
	/// <para>
	/// Generation is incremented on <seealso cref="checkpoint(IOLimiter) check-pointing"/>.
	/// Generation awareness allows for recovery from last <seealso cref="checkpoint(IOLimiter)"/>, provided the same updates
	/// will be replayed onto the index since that point in time.
	/// </para>
	/// <para>
	/// Changes to tree nodes are made so that stable nodes (i.e. nodes that have survived at least one checkpoint)
	/// are immutable w/ regards to keys values and child/sibling pointers.
	/// Making a change in a stable node will copy the node to an unstable generation first and then make the change
	/// in that unstable version. Further change in that node in the same generation will not require a copy since
	/// it's already unstable.
	/// </para>
	/// <para>
	/// Every pointer to another node (child/sibling pointer) consists of two pointers, one to a stable version and
	/// one to a potentially unstable version. A stable -&gt; unstable node copy will have its parent redirect one of its
	/// two pointers to the new unstable version, redirecting readers and writers to the new unstable version,
	/// while at the same time keeping one pointer to the stable version, in case there's a crash or non-clean
	/// shutdown, followed by recovery.
	/// </para>
	/// <para>
	/// A single writer w/ multiple concurrent readers is supported. Assuming usage adheres to this
	/// constraint neither writer nor readers are blocking. Readers are virtually garbage-free.
	/// </para>
	/// <para>
	/// An reader of GB+Tree is a <seealso cref="SeekCursor"/> that returns result as it finds them.
	/// As the cursor move over keys/values, returned results are considered "behind" it
	/// and likewise keys not yet returned "in front of".
	/// Readers will always read latest written changes in front of it but will not see changes that appear behind.
	/// The isolation level is thus read committed.
	/// The tree have no knowledge about transactions and apply updates as isolated units of work one entry at the time.
	/// Therefore, readers can see parts of transactions that are not fully applied yet.
	/// </para>
	/// <para>
	/// A note on recovery:
	/// </para>
	/// <para>
	/// <seealso cref="GBPTree"/> is designed to be able to handle non-clean shutdown / crash, but needs external help
	/// in order to do so.
	/// <seealso cref="writer() Writes"/> happen to the tree and are made durable and
	/// safe on next call to <seealso cref="checkpoint(IOLimiter)"/>. Writes which happens after the last
	/// <seealso cref="checkpoint(IOLimiter)"/> are not safe if there's a <seealso cref="close()"/> or JVM crash in between, i.e:
	/// 
	/// <pre>
	/// w: write
	/// c: checkpoint
	/// x: crash or <seealso cref="close()"/>
	/// 
	/// TIME |--w--w----w--c--ww--w-c-w--w-ww--w--w---x------|
	///         ^------ safe -----^   ^- unsafe --^
	/// </pre>
	/// 
	/// The writes that happened before the last checkpoint are durable and safe, but the writes after it are not.
	/// The tree can however get back to a consistent state by replaying all the writes since the last checkpoint
	/// all the way up to the crash ({@code x}). Even including writes before the last checkpoint is OK,
	/// important is that <strong>at least</strong> writes since last checkpoint are included. Note that the order
	/// in which the changes are applied is not important as long as they do not affect the same key. The order of
	/// updates targeting the same key needs to be preserved when replaying as only the last applied update will
	/// be visible.
	/// 
	/// If failing to replay missing writes, that data will simply be missing from the tree and most likely leave the
	/// database inconsistent.
	/// </para>
	/// <para>
	/// The reason as to why <seealso cref="close()"/> doesn't do a checkpoint is that checkpointing as a whole should
	/// be managed externally, keeping multiple resources in sync w/ regards to checkpoints. This is especially important
	/// since a it is impossible to recognize crashed pointers after a checkpoint.
	/// 
	/// </para>
	/// </summary>
	/// @param <KEY> type of keys </param>
	/// @param <VALUE> type of values </param>
	public class GBPTree<KEY, VALUE> : System.IDisposable
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_rootCatchupSupplier = () => new TripCountingRootCatchup(() => _root);
			_generationSupplier = () => _generation;
			_exceptionDecorator = this.appendTreeInformation;
		}

		 /// <summary>
		 /// For monitoring <seealso cref="GBPTree"/>.
		 /// </summary>
		 public interface Monitor
		 {
			  /// <summary>
			  /// Adapter for <seealso cref="Monitor"/>.
			  /// </summary>

			  /// <summary>
			  /// Called when a <seealso cref="GBPTree.checkpoint(IOLimiter)"/> has been completed, but right before
			  /// <seealso cref="GBPTree.writer() writers"/> are re-enabled.
			  /// </summary>
			  void CheckpointCompleted();

			  /// <summary>
			  /// Called when the tree was started on no existing store file and so will be created.
			  /// </summary>
			  void NoStoreFile();

			  /// <summary>
			  /// Called after cleanup job has been created
			  /// </summary>
			  void CleanupRegistered();

			  /// <summary>
			  /// Called after cleanup job has been started
			  /// </summary>
			  void CleanupStarted();

			  /// <summary>
			  /// Called after recovery has completed and cleaning has been done.
			  /// </summary>
			  /// <param name="numberOfPagesVisited"> number of pages visited by the cleaner. </param>
			  /// <param name="numberOfCleanedCrashPointers"> number of cleaned crashed pointers. </param>
			  /// <param name="durationMillis"> time spent cleaning. </param>
			  void CleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis );

			  /// <summary>
			  /// Called when cleanup job is closed and lock is released
			  /// </summary>
			  void CleanupClosed();

			  /// <summary>
			  /// Called when cleanup job catches a throwable </summary>
			  /// <param name="throwable"> cause of failure </param>
			  void CleanupFailed( Exception throwable );

			  /// <summary>
			  /// Report tree state on startup.
			  /// </summary>
			  /// <param name="clean"> true if tree was clean on startup. </param>
			  void StartupState( bool clean );

			  /// <summary>
			  /// Report tree growth, meaning split in root.
			  /// </summary>
			  void TreeGrowth();

			  /// <summary>
			  /// Report tree shrink, when root becomes empty.
			  /// </summary>
			  void TreeShrink();
		 }

		  public class Monitor_Adaptor : Monitor
		  {
			  private readonly GBPTree<KEY, VALUE> _outerInstance;

			  public Monitor_Adaptor( GBPTree<KEY, VALUE> outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

				public override void CheckpointCompleted()
				{ // no-op
				}

				public override void NoStoreFile()
				{ // no-op
				}

				public override void CleanupRegistered()
				{ // no-op
				}

				public override void CleanupStarted()
				{ // no-op
				}

				public override void CleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
				{ // no-op
				}

				public override void CleanupClosed()
				{ // no-op
				}

				public override void CleanupFailed( Exception throwable )
				{ // no-op
				}

				public override void StartupState( bool clean )
				{ // no-op
				}

				public override void TreeGrowth()
				{ // no-op
				}

				public override void TreeShrink()
				{ // no-op
				}
		  }

		 /// <summary>
		 /// No-op <seealso cref="Monitor"/>.
		 /// </summary>
		 public static readonly Monitor NoMonitor = new Monitor_Adaptor( this );

		 /// <summary>
		 /// No-op header reader.
		 /// </summary>
		 public static readonly Header.Reader NoHeaderReader = headerData =>
		 {
		 };

		 /// <summary>
		 /// No-op header writer.
		 /// </summary>
		 public static readonly System.Action<PageCursor> NoHeaderWriter = pc =>
		 {
		 };

		 /// <summary>
		 /// Paged file in a <seealso cref="PageCache"/> providing the means of storage.
		 /// </summary>
		 private readonly PagedFile _pagedFile;

		 /// <summary>
		 /// <seealso cref="File"/> to map in <seealso cref="PageCache"/> for storing this tree.
		 /// </summary>
		 private readonly File _indexFile;

		 /// <summary>
		 /// User-provided layout of key/value as well as custom additional meta information.
		 /// This allows for custom key/value and comparison representation. The layout provided during index
		 /// creation, i.e. the first time constructor is called for the given paged file, will be stored
		 /// in the meta page and it's asserted that the same layout is passed to the constructor when opening the tree.
		 /// </summary>
		 private readonly Layout<KEY, VALUE> _layout;

		 /// <summary>
		 /// Instance of <seealso cref="TreeNode"/> which handles reading/writing physical bytes from pages representing tree nodes.
		 /// </summary>
		 private readonly TreeNode<KEY, VALUE> _bTreeNode;

		 /// <summary>
		 /// A free-list of released ids. Acquiring new ids involves first trying out the free-list and then,
		 /// as a fall-back allocate a new id at the end of the store.
		 /// </summary>
		 private readonly FreeListIdProvider _freeList;

		 /// <summary>
		 /// A single instance <seealso cref="Writer"/> because tree only supports single writer.
		 /// </summary>
		 private readonly SingleWriter _writer;

		 /// <summary>
		 /// Tells whether or not there have been made changes (using <seealso cref="writer()"/>) to this tree
		 /// since last call to <seealso cref="checkpoint(IOLimiter)"/>. This variable is set when calling <seealso cref="writer()"/>
		 /// and cleared inside <seealso cref="checkpoint(IOLimiter)"/>.
		 /// </summary>
		 private volatile bool _changesSinceLastCheckpoint;

		 /// <summary>
		 /// Lock with two individual parts. Writer lock and cleaner lock.
		 /// <para>
		 /// There are a few different scenarios that involve writing or flushing that can not be happen concurrently:
		 /// <ul>
		 ///     <li>Checkpoint and writing</li>
		 ///     <li>Checkpoint and close</li>
		 ///     <li>Write and checkpoint</li>
		 /// </ul>
		 /// For those scenarios, writer lock is taken.
		 /// </para>
		 /// <para>
		 /// If cleaning of crash pointers is needed the tree can not be allowed to perform a checkpoint until that job
		 /// has finished. For this scenario, cleaner lock is taken.
		 /// </para>
		 /// </summary>
		 private readonly GBPTreeLock @lock = new GBPTreeLock();

		 /// <summary>
		 /// Page size, i.e. tree node size, of the tree nodes in this tree. The page size is determined on
		 /// tree creation, stored in meta page and read when opening tree later.
		 /// </summary>
		 private int _pageSize;

		 /// <summary>
		 /// Whether or not the tree was created this time it was instantiated.
		 /// </summary>
		 private bool _created;

		 /// <summary>
		 /// Generation of the tree. This variable contains both stable and unstable generation and is
		 /// represented as one long to get atomic updates of both stable and unstable generation for readers.
		 /// Both stable and unstable generation are unsigned ints, i.e. 32 bits each.
		 /// 
		 /// <ul>
		 /// <li>stable generation, generation which has survived the last <seealso cref="checkpoint(IOLimiter)"/></li>
		 /// <li>unstable generation, current generation under evolution. This generation will be the
		 /// <seealso cref="Generation.stableGeneration(long)"/> after the next <seealso cref="checkpoint(IOLimiter)"/></li>
		 /// </ul>
		 /// </summary>
		 private volatile long _generation;

		 /// <summary>
		 /// Current root (id and generation where it was assigned). In the rare event of creating a new root
		 /// a new <seealso cref="Root"/> instance will be created and assigned to this variable.
		 /// 
		 /// For reading id and generation atomically a reader can first grab a local reference to this variable
		 /// and then call <seealso cref="Root.id()"/> and <seealso cref="Root.generation()"/>, or use <seealso cref="Root.goTo(PageCursor)"/>
		 /// directly, which moves the page cursor to the id and returns the generation.
		 /// </summary>
		 private volatile Root _root;

		 /// <summary>
		 /// Catchup for <seealso cref="SeekCursor"/> to become aware of new roots since it started.
		 /// </summary>
		 private System.Func<RootCatchup> _rootCatchupSupplier;

		 /// <summary>
		 /// Supplier of generation to readers. This supplier will actually very rarely be used, because normally
		 /// a <seealso cref="SeekCursor"/> is bootstrapped from <seealso cref="generation"/>. The only time this supplier will be
		 /// used is so that a long-running <seealso cref="SeekCursor"/> can keep up with a generation change after
		 /// a checkpoint, if the cursor lives that long.
		 /// </summary>
		 private System.Func<long> _generationSupplier;

		 /// <summary>
		 /// Called on certain events.
		 /// </summary>
		 private readonly Monitor _monitor;

		 /// <summary>
		 /// If this tree is read only, no changes will be made to it. No generation bumping, no checkpointing, no nothing.
		 /// </summary>
		 private readonly bool _readOnly;

		 /// <summary>
		 /// Whether or not this tree has been closed. Accessed and changed solely in
		 /// <seealso cref="close()"/> to be able to close tree multiple times gracefully.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("UnusedAssignment") private boolean closed = true;
		 private bool _closed = true;

		 /// <summary>
		 /// True if tree is clean, false if dirty
		 /// </summary>
		 private bool _clean;

		 /// <summary>
		 /// True if initial tree state was dirty
		 /// </summary>
		 private bool _dirtyOnStartup;

		 /// <summary>
		 /// State of cleanup job.
		 /// </summary>
		 private readonly CleanupJob _cleaning;

		 /// <summary>
		 /// <seealso cref="Consumer"/> to hand out to others who want to decorate information about this tree
		 /// to exceptions thrown out from its surface.
		 /// </summary>
		 private System.Action<Exception> _exceptionDecorator;

		 /// <summary>
		 /// Opens an index {@code indexFile} in the {@code pageCache}, creating and initializing it if it doesn't exist.
		 /// If the index doesn't exist it will be created and the <seealso cref="Layout"/> and {@code pageSize} will
		 /// be written in index header.
		 /// If the index exists it will be opened and the <seealso cref="Layout"/> will be matched with the information
		 /// in the header. At the very least <seealso cref="Layout.identifier()"/> will be matched.
		 /// <para>
		 /// On start, tree can be in a clean or dirty state. If dirty, it will
		 /// <seealso cref="createCleanupJob(RecoveryCleanupWorkCollector, bool)"/> and clean crashed pointers as part of constructor. Tree is only clean if
		 /// since last time it was opened it was <seealso cref="close() closed"/> without any non-checkpointed changes present.
		 /// Correct usage pattern of the GBPTree is:
		 /// 
		 /// <pre>
		 ///     try ( GBPTree tree = new GBPTree(...) )
		 ///     {
		 ///         // Use the tree
		 ///         tree.checkpoint( ... );
		 ///     }
		 /// </pre>
		 /// 
		 /// Expected state after first time tree is opened, where initial state is created:
		 /// <ul>
		 /// <li>StateA
		 /// <ul>
		 /// <li>stableGeneration=2</li>
		 /// <li>unstableGeneration=3</li>
		 /// <li>rootId=3</li>
		 /// <li>rootGeneration=2</li>
		 /// <li>lastId=4</li>
		 /// <li>freeListWritePageId=4</li>
		 /// <li>freeListReadPageId=4</li>
		 /// <li>freeListWritePos=0</li>
		 /// <li>freeListReadPos=0</li>
		 /// <li>clean=false</li>
		 /// </ul>
		 /// <li>StateB
		 /// <ul>
		 /// <li>stableGeneration=2</li>
		 /// <li>unstableGeneration=4</li>
		 /// <li>rootId=3</li>
		 /// <li>rootGeneration=2</li>
		 /// <li>lastId=4</li>
		 /// <li>freeListWritePageId=4</li>
		 /// <li>freeListReadPageId=4</li>
		 /// <li>freeListWritePos=0</li>
		 /// <li>freeListReadPos=0</li>
		 /// <li>clean=false</li>
		 /// </ul>
		 /// </ul>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="pageCache"> <seealso cref="PageCache"/> to use to map index file </param>
		 /// <param name="indexFile"> <seealso cref="File"/> containing the actual index </param>
		 /// <param name="layout"> <seealso cref="Layout"/> to use in the tree, this must match the existing layout
		 /// we're just opening the index </param>
		 /// <param name="tentativePageSize"> page size, i.e. tree node size. Must be less than or equal to that of the page cache.
		 /// A pageSize of {@code 0} means to use whatever the page cache has (at creation) </param>
		 /// <param name="monitor"> <seealso cref="Monitor"/> for monitoring <seealso cref="GBPTree"/>. </param>
		 /// <param name="headerReader"> reads header data, previously written using <seealso cref="checkpoint(IOLimiter, Consumer)"/>
		 /// or <seealso cref="close()"/> </param>
		 /// <param name="headerWriter"> writes header data if indexFile is created as a result of this call. </param>
		 /// <param name="recoveryCleanupWorkCollector"> collects recovery cleanup jobs for execution after recovery. </param>
		 /// <param name="readOnly"> Opening tree in readOnly mode will prevent any modifications to it. </param>
		 /// <exception cref="UncheckedIOException"> on page cache error </exception>
		 /// <exception cref="MetadataMismatchException"> if meta information does not match constructor parameters or meta page is missing </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public GBPTree(org.neo4j.io.pagecache.PageCache pageCache, java.io.File indexFile, Layout<KEY,VALUE> layout, int tentativePageSize, Monitor monitor, Header.Reader headerReader, System.Action<org.neo4j.io.pagecache.PageCursor> headerWriter, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, boolean readOnly) throws MetadataMismatchException
		 public GBPTree( PageCache pageCache, File indexFile, Layout<KEY, VALUE> layout, int tentativePageSize, Monitor monitor, Header.Reader headerReader, System.Action<PageCursor> headerWriter, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._indexFile = indexFile;
			  this._monitor = monitor;
			  this._readOnly = readOnly;
			  this._generation = Generation.GenerationConflict( MIN_GENERATION, MIN_GENERATION + 1 );
			  long rootId = IdSpace.MIN_TREE_NODE_ID;
			  SetRoot( rootId, Generation.UnstableGeneration( _generation ) );
			  this._layout = layout;

			  try
			  {
					this._pagedFile = OpenOrCreate( pageCache, indexFile, tentativePageSize );
					this._pageSize = _pagedFile.pageSize();
					_closed = false;
					TreeNodeSelector.Factory format;
					if ( _created )
					{
						 format = TreeNodeSelector.SelectByLayout( layout );
						 WriteMeta( layout, format, _pagedFile );
					}
					else
					{
						 Meta meta = ReadMeta( layout, _pagedFile );
						 meta.Verify( layout );
						 format = TreeNodeSelector.SelectByFormat( meta.FormatIdentifier, meta.FormatVersion );
					}
					this._bTreeNode = format.Create( _pageSize, layout );
					this._freeList = new FreeListIdProvider( _pagedFile, _pageSize, rootId, FreeListIdProvider.NO_MONITOR );
					this._writer = new SingleWriter( this, new InternalTreeLogic<KEY, VALUE>( _freeList, _bTreeNode, layout, monitor ) );

					// Create or load state
					if ( _created )
					{
						 InitializeAfterCreation( headerWriter );
					}
					else
					{
						 LoadState( _pagedFile, headerReader );
					}
					this._monitor.startupState( _clean );

					// Prepare tree for action
					_dirtyOnStartup = !_clean;
					_clean = false;
					BumpUnstableGeneration();
					if ( !readOnly )
					{
						 ForceState();
						 _cleaning = CreateCleanupJob( recoveryCleanupWorkCollector, _dirtyOnStartup );
					}
					else
					{
						 _cleaning = CleanupJob.CLEAN;
					}
			  }
			  catch ( IOException e )
			  {
					throw ExitConstructor( new UncheckedIOException( e ) );
			  }
			  catch ( Exception e )
			  {
					throw ExitConstructor( e );
			  }
		 }

		 private Exception ExitConstructor( Exception throwable )
		 {
			  try
			  {
					Close();
			  }
			  catch ( IOException e )
			  {
					throwable = Exceptions.chain( new UncheckedIOException( e ), throwable );
			  }

			  AppendTreeInformation( throwable );
			  Exceptions.throwIfUnchecked( throwable );
			  return new Exception( throwable );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void initializeAfterCreation(System.Action<org.neo4j.io.pagecache.PageCursor> headerWriter) throws java.io.IOException
		 private void InitializeAfterCreation( System.Action<PageCursor> headerWriter )
		 {
			  // Initialize state
			  using ( PageCursor cursor = _pagedFile.io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					TreeStatePair.InitializeStatePages( cursor );
			  }

			  // Initialize index root node to a leaf node.
			  using ( PageCursor cursor = OpenRootCursor( Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					long stableGeneration = stableGeneration( _generation );
					long unstableGeneration = unstableGeneration( _generation );
					_bTreeNode.initializeLeaf( cursor, stableGeneration, unstableGeneration );
					checkOutOfBounds( cursor );
			  }

			  // Initialize free-list
			  _freeList.initializeAfterCreation();
			  _changesSinceLastCheckpoint = true;

			  // Checkpoint to make the created root node stable. Forcing tree state also piggy-backs on this.
			  Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited, headerWriter );
			  _clean = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.io.pagecache.PagedFile openOrCreate(org.neo4j.io.pagecache.PageCache pageCache, java.io.File indexFile, int pageSizeForCreation) throws java.io.IOException, MetadataMismatchException
		 private PagedFile OpenOrCreate( PageCache pageCache, File indexFile, int pageSizeForCreation )
		 {
			  try
			  {
					return OpenExistingIndexFile( pageCache, indexFile );
			  }
			  catch ( NoSuchFileException e )
			  {
					if ( !_readOnly )
					{
						 return CreateNewIndexFile( pageCache, indexFile, pageSizeForCreation );
					}
					throw new TreeFileNotFoundException( "Can not create new tree file in read only mode.", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.io.pagecache.PagedFile openExistingIndexFile(org.neo4j.io.pagecache.PageCache pageCache, java.io.File indexFile) throws java.io.IOException, MetadataMismatchException
		 private static PagedFile OpenExistingIndexFile( PageCache pageCache, File indexFile )
		 {
			  PagedFile pagedFile = pageCache.Map( indexFile, pageCache.PageSize() );
			  // This index already exists, verify meta data aligns with expectations

			  bool success = false;
			  try
			  {
					// We're only interested in the page size really, so don't involve layout at this point
					Meta meta = ReadMeta( null, pagedFile );
					pagedFile = MapWithCorrectPageSize( pageCache, indexFile, pagedFile, meta.PageSize );
					success = true;
					return pagedFile;
			  }
			  catch ( System.InvalidOperationException e )
			  {
					throw new MetadataMismatchException( "Index is not fully initialized since it's missing the meta page", e );
			  }
			  finally
			  {
					if ( !success )
					{
						 pagedFile.Close();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.io.pagecache.PagedFile createNewIndexFile(org.neo4j.io.pagecache.PageCache pageCache, java.io.File indexFile, int pageSizeForCreation) throws java.io.IOException
		 private PagedFile CreateNewIndexFile( PageCache pageCache, File indexFile, int pageSizeForCreation )
		 {
			  // First time
			  _monitor.noStoreFile();
			  int pageSize = pageSizeForCreation == 0 ? pageCache.PageSize() : pageSizeForCreation;
			  if ( pageSize > pageCache.PageSize() )
			  {
					throw new MetadataMismatchException( "Tried to create tree with page size %d" + ", but page cache used to create it has a smaller page size %d" + " so cannot be created", pageSize, pageCache.PageSize() );
			  }

			  // We need to create this index
			  PagedFile pagedFile = pageCache.Map( indexFile, pageSize, StandardOpenOption.CREATE );
			  _created = true;
			  return pagedFile;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void loadState(org.neo4j.io.pagecache.PagedFile pagedFile, Header.Reader headerReader) throws java.io.IOException
		 private void LoadState( PagedFile pagedFile, Header.Reader headerReader )
		 {
			  Pair<TreeState, TreeState> states = LoadStatePages( pagedFile );
			  TreeState state = TreeStatePair.SelectNewestValidState( states );
			  using ( PageCursor cursor = pagedFile.Io( state.PageId(), Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					PageCursorUtil.GoTo( cursor, "header data", state.PageId() );
					DoReadHeader( headerReader, cursor );
			  }
			  _generation = Generation.GenerationConflict( state.StableGeneration(), state.UnstableGeneration() );
			  SetRoot( state.RootId(), state.RootGeneration() );

			  long lastId = state.LastId();
			  long freeListWritePageId = state.FreeListWritePageId();
			  long freeListReadPageId = state.FreeListReadPageId();
			  int freeListWritePos = state.FreeListWritePos();
			  int freeListReadPos = state.FreeListReadPos();
			  _freeList.initialize( lastId, freeListWritePageId, freeListReadPageId, freeListWritePos, freeListReadPos );
			  _clean = state.Clean;
		 }

		 /// <summary>
		 /// Use when you are only interested in reading the header of existing index file without opening the index for writes.
		 /// Useful when reading header and the demands on matching layout can be relaxed a bit.
		 /// </summary>
		 /// <param name="pageCache"> <seealso cref="PageCache"/> to use to map index file </param>
		 /// <param name="indexFile"> <seealso cref="File"/> containing the actual index </param>
		 /// <param name="headerReader"> reads header data, previously written using <seealso cref="checkpoint(IOLimiter, Consumer)"/>
		 /// or <seealso cref="close()"/> </param>
		 /// <exception cref="IOException"> On page cache error </exception>
		 /// <exception cref="MetadataMismatchException"> if some meta page is missing (tree not fully initialized) </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void readHeader(org.neo4j.io.pagecache.PageCache pageCache, java.io.File indexFile, Header.Reader headerReader) throws java.io.IOException, MetadataMismatchException
		 public static void ReadHeader( PageCache pageCache, File indexFile, Header.Reader headerReader )
		 {
			  try
			  {
					  using ( PagedFile pagedFile = OpenExistingIndexFile( pageCache, indexFile ) )
					  {
						Pair<TreeState, TreeState> states = LoadStatePages( pagedFile );
						TreeState state = TreeStatePair.SelectNewestValidState( states );
						using ( PageCursor cursor = pagedFile.Io( state.PageId(), Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
						{
							 PageCursorUtil.GoTo( cursor, "header data", state.PageId() );
							 DoReadHeader( headerReader, cursor );
						}
					  }
			  }
			  catch ( Exception t )
			  {
					// Decorate outgoing exceptions with basic tree information. This is similar to how the constructor
					// appends its information, but the constructor has read more information at that point so this one
					// is a bit more sparse on information.
					withMessage( t, t.Message + " | " + format( "GBPTree[file:%s]", indexFile ) );
					throw t;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void doReadHeader(Header.Reader headerReader, org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private static void DoReadHeader( Header.Reader headerReader, PageCursor cursor )
		 {
			  int headerDataLength;
			  do
			  {
					TreeState.Read( cursor );
					headerDataLength = cursor.Int;
			  } while ( cursor.ShouldRetry() );

			  int headerDataOffset = cursor.Offset;
			  sbyte[] headerDataBytes = new sbyte[headerDataLength];
			  do
			  {
					cursor.Offset = headerDataOffset;
					cursor.GetBytes( headerDataBytes );
			  } while ( cursor.ShouldRetry() );

			  headerReader.Read( ByteBuffer.wrap( headerDataBytes ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeState(org.neo4j.io.pagecache.PagedFile pagedFile, Header.Writer headerWriter) throws java.io.IOException
		 private void WriteState( PagedFile pagedFile, Header.Writer headerWriter )
		 {
			  Pair<TreeState, TreeState> states = ReadStatePages( pagedFile );
			  TreeState oldestState = TreeStatePair.SelectOldestOrInvalid( states );
			  long pageToOverwrite = oldestState.PageId();
			  Root root = this._root;
			  using ( PageCursor cursor = pagedFile.Io( pageToOverwrite, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					PageCursorUtil.GoTo( cursor, "state page", pageToOverwrite );
					TreeState.Write( cursor, stableGeneration( _generation ), unstableGeneration( _generation ), root.Id(), root.Generation(), _freeList.lastId(), _freeList.writePageId(), _freeList.readPageId(), _freeList.writePos(), _freeList.readPos(), _clean );

					WriterHeader( pagedFile, headerWriter, Other( states, oldestState ), cursor );

					checkOutOfBounds( cursor );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writerHeader(org.neo4j.io.pagecache.PagedFile pagedFile, Header.Writer headerWriter, TreeState otherState, org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private static void WriterHeader( PagedFile pagedFile, Header.Writer headerWriter, TreeState otherState, PageCursor cursor )
		 {
			  // Write/carry over header
			  int headerOffset = cursor.Offset;
			  int headerDataOffset = headerOffset + Integer.BYTES; // will contain length of written header data (below)
			  if ( otherState.Valid || headerWriter != CARRY_OVER_PREVIOUS_HEADER )
			  {
					PageCursor previousCursor = pagedFile.Io( otherState.PageId(), Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK );
					PageCursorUtil.GoTo( previousCursor, "previous state page", otherState.PageId() );
					checkOutOfBounds( cursor );
					do
					{
						 // Clear any out-of-bounds from prior attempts
						 cursor.CheckAndClearBoundsFlag();
						 // Place the previous state cursor after state data
						 TreeState.Read( previousCursor );
						 // Read length of previous header
						 int previousLength = previousCursor.Int;
						 // Reserve space to store length
						 cursor.Offset = headerDataOffset;
						 // Write
						 headerWriter.Write( previousCursor, previousLength, cursor );
					} while ( previousCursor.ShouldRetry() );
					checkOutOfBounds( previousCursor );
					checkOutOfBounds( cursor );

					int length = cursor.Offset - headerDataOffset;
					cursor.PutInt( headerOffset, length );
			  }
		 }

		 private static TreeState Other( Pair<TreeState, TreeState> states, TreeState state )
		 {
			  return states.Left == state ? states.Right : states.Left;
		 }

		 /// <summary>
		 /// Basically <seealso cref="readStatePages(PagedFile)"/> with some more checks, suitable for when first opening an index file,
		 /// not while running it and check pointing.
		 /// </summary>
		 /// <param name="pagedFile"> <seealso cref="PagedFile"/> to read the state pages from. </param>
		 /// <returns> both read state pages. </returns>
		 /// <exception cref="MetadataMismatchException"> if state pages are missing (file is smaller than that) or if they are both empty. </exception>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.apache.commons.lang3.tuple.Pair<TreeState,TreeState> loadStatePages(org.neo4j.io.pagecache.PagedFile pagedFile) throws MetadataMismatchException, java.io.IOException
		 private static Pair<TreeState, TreeState> LoadStatePages( PagedFile pagedFile )
		 {
			  try
			  {
					Pair<TreeState, TreeState> states = ReadStatePages( pagedFile );
					if ( states.Left.Empty && states.Right.Empty )
					{
						 throw new MetadataMismatchException( "Index is not fully initialized since its state pages are empty" );
					}
					return states;
			  }
			  catch ( System.InvalidOperationException e )
			  {
					throw new MetadataMismatchException( "Index is not fully initialized since it's missing state pages", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.apache.commons.lang3.tuple.Pair<TreeState,TreeState> readStatePages(org.neo4j.io.pagecache.PagedFile pagedFile) throws java.io.IOException
		 private static Pair<TreeState, TreeState> ReadStatePages( PagedFile pagedFile )
		 {
			  Pair<TreeState, TreeState> states;
			  using ( PageCursor cursor = pagedFile.Io( 0L, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					states = TreeStatePair.ReadStatePages( cursor, IdSpace.STATE_PAGE_A, IdSpace.STATE_PAGE_B );
			  }
			  return states;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.io.pagecache.PageCursor openMetaPageCursor(org.neo4j.io.pagecache.PagedFile pagedFile, int pfFlags) throws java.io.IOException
		 private static PageCursor OpenMetaPageCursor( PagedFile pagedFile, int pfFlags )
		 {
			  PageCursor metaCursor = pagedFile.Io( IdSpace.META_PAGE_ID, pfFlags );
			  PageCursorUtil.GoTo( metaCursor, "meta page", IdSpace.META_PAGE_ID );
			  return metaCursor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> Meta readMeta(Layout<KEY,VALUE> layout, org.neo4j.io.pagecache.PagedFile pagedFile) throws java.io.IOException
		 private static Meta ReadMeta<KEY, VALUE>( Layout<KEY, VALUE> layout, PagedFile pagedFile )
		 {
			  using ( PageCursor metaCursor = OpenMetaPageCursor( pagedFile, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					return Meta.Read( metaCursor, layout );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeMeta(Layout<KEY,VALUE> layout, TreeNodeSelector.Factory format, org.neo4j.io.pagecache.PagedFile pagedFile) throws java.io.IOException
		 private void WriteMeta( Layout<KEY, VALUE> layout, TreeNodeSelector.Factory format, PagedFile pagedFile )
		 {
			  Meta meta = new Meta( format.FormatIdentifier(), format.FormatVersion(), _pageSize, layout );
			  using ( PageCursor metaCursor = OpenMetaPageCursor( pagedFile, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					meta.Write( metaCursor, layout );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.io.pagecache.PagedFile mapWithCorrectPageSize(org.neo4j.io.pagecache.PageCache pageCache, java.io.File indexFile, org.neo4j.io.pagecache.PagedFile pagedFile, int pageSize) throws java.io.IOException
		 private static PagedFile MapWithCorrectPageSize( PageCache pageCache, File indexFile, PagedFile pagedFile, int pageSize )
		 {
			  // This index was created with another page size, re-open with that actual page size
			  if ( pageSize != pageCache.PageSize() )
			  {
					if ( pageSize > pageCache.PageSize() || pageSize < 0 )
					{
						 throw new MetadataMismatchException( "Tried to create tree with page size %d, but page cache used to open it this time " + "has a smaller page size %d so cannot be opened", pageSize, pageCache.PageSize() );
					}
					pagedFile.Close();
					return pageCache.Map( indexFile, pageSize );
			  }
			  return pagedFile;
		 }

		 /// <summary>
		 /// Utility for <seealso cref="PagedFile.io(long, int) acquiring"/> a new <seealso cref="PageCursor"/>,
		 /// placed at the current root id and which have had its <seealso cref="PageCursor.next()"/> called-
		 /// </summary>
		 /// <param name="pfFlags"> flags sent into <seealso cref="PagedFile.io(long, int)"/>. </param>
		 /// <returns> <seealso cref="PageCursor"/> result from call to <seealso cref="PagedFile.io(long, int)"/> after it has been
		 /// placed at the current root and has had <seealso cref="PageCursor.next()"/> called. </returns>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.io.pagecache.PageCursor openRootCursor(int pfFlags) throws java.io.IOException
		 private PageCursor OpenRootCursor( int pfFlags )
		 {
			  PageCursor cursor = _pagedFile.io( 0L, pfFlags );
			  _root.goTo( cursor );
			  return cursor;
		 }

		 /// <summary>
		 /// Seeks hits in this tree, given a key range. Hits are iterated over using the returned <seealso cref="RawCursor"/>.
		 /// There's no guarantee that neither the <seealso cref="Hit"/> nor key/value instances are immutable and so
		 /// if caller wants to cache the results it's safest to copy the instances, or rather their contents,
		 /// into its own result cache.
		 /// <para>
		 /// Seeks can go either forwards or backwards depending on the values of the key arguments.
		 /// <ul>
		 /// <li>
		 /// A {@code fromInclusive} that is smaller than the {@code toExclusive} results in results in ascending order.
		 /// </li>
		 /// <li>
		 /// A {@code fromInclusive} that is bigger than the {@code toExclusive} results in results in descending order.
		 /// </li>
		 /// </ul>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="fromInclusive"> lower bound of the range to seek (inclusive). </param>
		 /// <param name="toExclusive"> higher bound of the range to seek (exclusive). </param>
		 /// <returns> a <seealso cref="RawCursor"/> used to iterate over the hits within the specified key range. </returns>
		 /// <exception cref="IOException"> on error reading from index. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.cursor.RawCursor<Hit<KEY,VALUE>,java.io.IOException> seek(KEY fromInclusive, KEY toExclusive) throws java.io.IOException
		 public virtual RawCursor<Hit<KEY, VALUE>, IOException> Seek( KEY fromInclusive, KEY toExclusive )
		 {
			  long generation = this._generation;
			  long stableGeneration = stableGeneration( generation );
			  long unstableGeneration = unstableGeneration( generation );

			  PageCursor cursor = _pagedFile.io( 0L, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK );
			  long rootGeneration = _root.goTo( cursor );

			  // Returns cursor which is now initiated with left-most leaf node for the specified range
			  return new SeekCursor<Hit<KEY, VALUE>, IOException>( cursor, _bTreeNode, fromInclusive, toExclusive, _layout, stableGeneration, unstableGeneration, _generationSupplier, _rootCatchupSupplier.get(), rootGeneration, _exceptionDecorator, SeekCursor.DEFAULT_MAX_READ_AHEAD );
		 }

		 /// <summary>
		 /// Checkpoints and flushes any pending changes to storage. After a successful call to this method
		 /// the data is durable and safe. <seealso cref="writer() Changes"/> made after this call and until crashing or
		 /// otherwise non-clean shutdown (by omitting calling checkpoint before <seealso cref="close()"/>) will need to be replayed
		 /// next time this tree is opened.
		 /// <para>
		 /// Header writer is expected to leave consumed <seealso cref="PageCursor"/> at end of written header for calculation of
		 /// header size.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="ioLimiter"> for controlling I/O usage. </param>
		 /// <param name="headerWriter"> hook for writing header data, must leave cursor at end of written header. </param>
		 /// <exception cref="UncheckedIOException"> on error flushing to storage. </exception>
		 public virtual void Checkpoint( IOLimiter ioLimiter, System.Action<PageCursor> headerWriter )
		 {
			  try
			  {
					Checkpoint( ioLimiter, replace( headerWriter ) );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 /// <summary>
		 /// Performs a <seealso cref="checkpoint(IOLimiter, Consumer) check point"/>, keeping any header information
		 /// written in previous check point.
		 /// </summary>
		 /// <param name="ioLimiter"> for controlling I/O usage. </param>
		 /// <exception cref="UncheckedIOException"> on error flushing to storage. </exception>
		 /// <seealso cref= #checkpoint(IOLimiter, Consumer) </seealso>
		 public virtual void Checkpoint( IOLimiter ioLimiter )
		 {
			  try
			  {
					Checkpoint( ioLimiter, CARRY_OVER_PREVIOUS_HEADER );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkpoint(org.neo4j.io.pagecache.IOLimiter ioLimiter, Header.Writer headerWriter) throws java.io.IOException
		 private void Checkpoint( IOLimiter ioLimiter, Header.Writer headerWriter )
		 {
			  if ( _readOnly )
			  {
					return;
			  }
			  // Flush dirty pages of the tree, do this before acquiring the lock so that writers won't be
			  // blocked while we do this
			  _pagedFile.flushAndForce( ioLimiter );

			  // Block writers, or if there's a current writer then wait for it to complete and then block
			  // From this point and till the lock is released we know that the tree won't change.
			  @lock.WriterAndCleanerLock();
			  try
			  {
					AssertRecoveryCleanSuccessful();
					// Flush dirty pages since that last flush above. This should be a very small set of pages
					// and should be rather fast. In here writers are blocked and we want to minimize this
					// windows of time as much as possible, that's why there's an initial flush outside this lock.
					_pagedFile.flushAndForce();

					// Increment generation, i.e. stable becomes current unstable and unstable increments by one
					// and write the tree state (rootId, lastId, generation a.s.o.) to state page.
					long unstableGeneration = unstableGeneration( _generation );
					_generation = Generation.GenerationConflict( unstableGeneration, unstableGeneration + 1 );
					WriteState( _pagedFile, headerWriter );

					// Flush the state page.
					_pagedFile.flushAndForce();

					// Expose this fact.
					_monitor.checkpointCompleted();

					// Clear flag so that until next change there's no need to do another checkpoint.
					_changesSinceLastCheckpoint = false;
			  }
			  finally
			  {
					// Unblock writers, any writes after this point and up until the next checkpoint will have
					// the new unstable generation.
					@lock.WriterAndCleanerUnlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertRecoveryCleanSuccessful() throws java.io.IOException
		 private void AssertRecoveryCleanSuccessful()
		 {
			  if ( _cleaning != null && _cleaning.hasFailed() )
			  {
					throw new IOException( "Pointer cleaning during recovery failed", _cleaning.Cause );
			  }
		 }

		 private void AssertNotReadOnly( string operationDescription )
		 {
			  if ( _readOnly )
			  {
					throw new System.NotSupportedException( "GBPTree was opened in read only mode and can not finish operation: " + operationDescription );
			  }
		 }

		 /// <summary>
		 /// Closes this tree and its associated resources.
		 /// <para>
		 /// NOTE: No <seealso cref="checkpoint(IOLimiter) checkpoint"/> is performed.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IOException"> on error closing resources. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  if ( _readOnly )
			  {
					// Close without forcing state
					DoClose();
					return;
			  }
			  @lock.WriterLock();
			  try
			  {
					if ( _closed )
					{
						 return;
					}

					MaybeForceCleanState();
					DoClose();
			  }
			  catch ( IOException ioe )
			  {
					try
					{
						 _pagedFile.flushAndForce();
						 MaybeForceCleanState();
						 DoClose();
					}
					catch ( IOException e )
					{
						 ioe.addSuppressed( e );
						 throw ioe;
					}
			  }
			  finally
			  {
					@lock.WriterUnlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void maybeForceCleanState() throws java.io.IOException
		 private void MaybeForceCleanState()
		 {
			  if ( _cleaning != null && !_changesSinceLastCheckpoint && !_cleaning.needed() )
			  {
					_clean = true;
					ForceState();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doClose() throws java.io.IOException
		 private void DoClose()
		 {
			  if ( _pagedFile != null )
			  {
					// Will be null if exception while mapping file
					_pagedFile.close();
			  }
			  _closed = true;
		 }

		 /// <summary>
		 /// Use default value for ratioToKeepInLeftOnSplit </summary>
		 /// <seealso cref= GBPTree#writer(double) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Writer<KEY,VALUE> writer() throws java.io.IOException
		 public virtual Writer<KEY, VALUE> Writer()
		 {
			  return Writer( InternalTreeLogic.DEFAULT_SPLIT_RATIO );
		 }

		 /// <summary>
		 /// Returns a <seealso cref="Writer"/> able to modify the index, i.e. insert and remove keys/values.
		 /// After usage the returned writer must be closed, typically by using try-with-resource clause.
		 /// </summary>
		 /// <param name="ratioToKeepInLeftOnSplit"> Decide how much to keep in left node on split, 0=keep nothing, 0.5=split 50-50, 1=keep everything. </param>
		 /// <returns> the single <seealso cref="Writer"/> for this index. The returned writer must be
		 /// <seealso cref="Writer.close() closed"/> before another caller can acquire this writer. </returns>
		 /// <exception cref="IOException"> on error accessing the index. </exception>
		 /// <exception cref="IllegalStateException"> for calls made between a successful call to this method and closing the
		 /// returned writer. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Writer<KEY,VALUE> writer(double ratioToKeepInLeftOnSplit) throws java.io.IOException
		 public virtual Writer<KEY, VALUE> Writer( double ratioToKeepInLeftOnSplit )
		 {
			  AssertNotReadOnly( "Open tree writer." );
			  _writer.initialize( ratioToKeepInLeftOnSplit );
			  _changesSinceLastCheckpoint = true;
			  return _writer;
		 }

		 private void SetRoot( long rootId, long rootGeneration )
		 {
			  this._root = new Root( rootId, rootGeneration );
		 }

		 /// <summary>
		 /// Bump unstable generation, increasing the gap between stable and unstable generation. All pointers and tree nodes
		 /// with generation in this gap are considered to be 'crashed' and will be cleaned up by <seealso cref="CleanupJob"/>
		 /// created in <seealso cref="createCleanupJob(RecoveryCleanupWorkCollector, bool)"/>.
		 /// </summary>
		 private void BumpUnstableGeneration()
		 {
			  _generation = _generation( stableGeneration( _generation ), unstableGeneration( _generation ) + 1 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void forceState() throws java.io.IOException
		 private void ForceState()
		 {
			  AssertNotReadOnly( "Force tree state." );
			  if ( _changesSinceLastCheckpoint )
			  {
					throw new System.InvalidOperationException( "It seems that this method has been called in the wrong state. " + "It's expected that this is called after opening this tree, but before any changes " + "have been made" );
			  }

			  WriteState( _pagedFile, CARRY_OVER_PREVIOUS_HEADER );
			  _pagedFile.flushAndForce();
		 }

		 /// <summary>
		 /// Called on start if tree was not clean.
		 /// </summary>
		 private CleanupJob CreateCleanupJob( RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool needsCleaning )
		 {
			  if ( !needsCleaning )
			  {
					return CleanupJob.CLEAN;
			  }
			  else
			  {
					@lock.CleanerLock();
					_monitor.cleanupRegistered();

					long generation = this._generation;
					long stableGeneration = stableGeneration( generation );
					long unstableGeneration = unstableGeneration( generation );
					long highTreeNodeId = _freeList.lastId() + 1;

					CrashGenerationCleaner crashGenerationCleaner = new CrashGenerationCleaner( _pagedFile, _bTreeNode, IdSpace.MIN_TREE_NODE_ID, highTreeNodeId, stableGeneration, unstableGeneration, _monitor );
					GBPTreeCleanupJob cleanupJob = new GBPTreeCleanupJob( crashGenerationCleaner, @lock, _monitor, _indexFile );
					recoveryCleanupWorkCollector.Add( cleanupJob );
					return cleanupJob;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <VISITOR extends GBPTreeVisitor<KEY,VALUE>> VISITOR visit(VISITOR visitor) throws java.io.IOException
		 public virtual VISITOR Visit<VISITOR>( VISITOR visitor ) where VISITOR : GBPTreeVisitor<KEY,VALUE>
		 {
			  using ( PageCursor cursor = OpenRootCursor( Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					( new GBPTreeStructure<>( _bTreeNode, _layout, stableGeneration( _generation ), unstableGeneration( _generation ) ) ).visitTree( cursor, _writer.cursor, visitor );
					_freeList.visitFreelist( visitor );
			  }
			  return visitor;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public void printTree() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PrintTree()
		 {
			  PrintTree( false, false, false, false, false );
		 }

		 // Utility method
		 /// <summary>
		 /// Prints the contents of the tree to System.out.
		 /// </summary>
		 /// <param name="printValues"> whether or not to print values in the leaf nodes. </param>
		 /// <param name="printPosition"> whether or not to print position for each key. </param>
		 /// <param name="printState"> whether or not to print the tree state. </param>
		 /// <param name="printHeader"> whether or not to print header of each tree node </param>
		 /// <param name="printFreelist"> whether or not to print freelist </param>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") void printTree(boolean printValues, boolean printPosition, boolean printState, boolean printHeader, boolean printFreelist) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PrintTree( bool printValues, bool printPosition, bool printState, bool printHeader, bool printFreelist )
		 {
			  PrintingGBPTreeVisitor<KEY, VALUE> printingVisitor = new PrintingGBPTreeVisitor<KEY, VALUE>( System.out, printValues, printPosition, printState, printHeader, printFreelist );
			  Visit( printingVisitor );
		 }

		 // Utility method
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void printState() throws java.io.IOException
		 public virtual void PrintState()
		 {
			  using ( PageCursor cursor = OpenRootCursor( Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					PrintingGBPTreeVisitor<KEY, VALUE> printingVisitor = new PrintingGBPTreeVisitor<KEY, VALUE>( System.out, false, false, true, false, false );
					GBPTreeStructure.VisitTreeState( cursor, printingVisitor );
			  }
		 }

		 // Utility method
		 /// <summary>
		 /// Print node with given id to System.out, if node with id exists. </summary>
		 /// <param name="id"> the page id of node to print </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void printNode(long id) throws java.io.IOException
		 internal virtual void PrintNode( long id )
		 {
			  if ( id < _freeList.lastId() )
			  {
					// Use write lock to avoid adversary interference
					using ( PageCursor cursor = _pagedFile.io( id, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
					{
						 cursor.Next();
						 sbyte nodeType = TreeNode.NodeType( cursor );
						 if ( nodeType == TreeNode.NODE_TYPE_TREE_NODE )
						 {
							  _bTreeNode.printNode( cursor, false, true, stableGeneration( _generation ), unstableGeneration( _generation ) );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean consistencyCheck() throws java.io.IOException
		 public virtual bool ConsistencyCheck()
		 {
			  ThrowingConsistencyCheckVisitor<KEY> reporter = new ThrowingConsistencyCheckVisitor<KEY>();
			  return ConsistencyCheck( reporter );
		 }

		 // Utility method
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean consistencyCheck(GBPTreeConsistencyCheckVisitor<KEY> visitor) throws java.io.IOException
		 public virtual bool ConsistencyCheck( GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  CleanTrackingConsistencyCheckVisitor<KEY> cleanTrackingVisitor = new CleanTrackingConsistencyCheckVisitor<KEY>( visitor );
			  try
			  {
					  using ( PageCursor cursor = _pagedFile.io( 0L, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
					  {
						long unstableGeneration = unstableGeneration( _generation );
						GBPTreeConsistencyChecker<KEY> consistencyChecker = new GBPTreeConsistencyChecker<KEY>( _bTreeNode, _layout, _freeList, stableGeneration( _generation ), unstableGeneration );
      
						consistencyChecker.Check( _indexFile, cursor, _root, cleanTrackingVisitor );
					  }
			  }
			  catch ( Exception e ) when ( e is TreeInconsistencyException || e is MetadataMismatchException || e is CursorException )
			  {
					cleanTrackingVisitor.Exception( e );
			  }
			  return cleanTrackingVisitor.Clean();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public void unsafe(GBPTreeUnsafe<KEY,VALUE> unsafe) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Unsafe( GBPTreeUnsafe<KEY, VALUE> @unsafe )
		 {
			  TreeState state;
			  using ( PageCursor cursor = _pagedFile.io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					// todo find better way of getting TreeState?
					Pair<TreeState, TreeState> states = TreeStatePair.ReadStatePages( cursor, IdSpace.STATE_PAGE_A, IdSpace.STATE_PAGE_B );
					state = TreeStatePair.SelectNewestValidState( states );
			  }
			  @unsafe.Access( _pagedFile, _layout, _bTreeNode, state );
		 }

		 public override string ToString()
		 {
			  long generation = this._generation;
			  return format( "GB+Tree[file:%s, layout:%s, generation:%d/%d]", _indexFile.AbsolutePath, _layout, stableGeneration( generation ), unstableGeneration( generation ) );
		 }

		 private void AppendTreeInformation<E>( E e ) where E : Exception
		 {
			  Exceptions.withMessage( e, e.Message + " | " + ToString() );
		 }

		 private class SingleWriter : Writer<KEY, VALUE>
		 {
			 private readonly GBPTree<KEY, VALUE> _outerInstance;

			  /// <summary>
			  /// Currently an index only supports one concurrent writer and so this boolean will act as
			  /// guard so that only one writer ever exist.
			  /// </summary>
			  internal readonly AtomicBoolean WriterTaken = new AtomicBoolean();
			  internal readonly InternalTreeLogic<KEY, VALUE> TreeLogic;
			  internal readonly StructurePropagation<KEY> StructurePropagation;
			  internal PageCursor Cursor;

			  // Writer can't live past a checkpoint because of the mutex with checkpoint,
			  // therefore safe to locally cache these generation fields from the volatile generation in the tree
			  internal long StableGeneration;
			  internal long UnstableGeneration;
			  internal double RatioToKeepInLeftOnSplit;

			  internal SingleWriter( GBPTree<KEY, VALUE> outerInstance, InternalTreeLogic<KEY, VALUE> treeLogic )
			  {
				  this._outerInstance = outerInstance;
					this.StructurePropagation = new StructurePropagation<KEY>( outerInstance.layout.NewKey(), outerInstance.layout.NewKey(), outerInstance.layout.NewKey() );
					this.TreeLogic = treeLogic;
			  }

			  /// <summary>
			  /// When leaving initialize, writer should be in a fully consistent state.
			  /// <para>
			  /// Either fully initialized:
			  /// <ul>
			  ///    <li><seealso cref="writerTaken"/> - true</li>
			  ///    <li><seealso cref="lock"/> - writerLock locked</li>
			  ///    <li><seealso cref="cursor"/> - not null</li>
			  /// </ul>
			  /// Of fully closed:
			  /// <ul>
			  ///    <li><seealso cref="writerTaken"/> - false</li>
			  ///    <li><seealso cref="lock"/> - writerLock unlocked</li>
			  ///    <li><seealso cref="cursor"/> - null</li>
			  /// </ul>
			  /// 
			  /// </para>
			  /// </summary>
			  /// <exception cref="IOException"> if fail to open <seealso cref="PageCursor"/> </exception>
			  /// <param name="ratioToKeepInLeftOnSplit"> Decide how much to keep in left node on split, 0=keep nothing, 0.5=split 50-50, 1=keep everything. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void initialize(double ratioToKeepInLeftOnSplit) throws java.io.IOException
			  internal virtual void Initialize( double ratioToKeepInLeftOnSplit )
			  {
					if ( !WriterTaken.compareAndSet( false, true ) )
					{
						 throw new System.InvalidOperationException( "Writer in " + this + " is already acquired by someone else. " + "Only a single writer is allowed. The writer will become available as soon as " + "acquired writer is closed" );
					}

					bool success = false;
					try
					{
						 // Block here until cleaning has completed, if cleaning was required
						 outerInstance.@lock.WriterAndCleanerLock();
						 outerInstance.assertRecoveryCleanSuccessful();
						 Cursor = outerInstance.openRootCursor( Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock );
						 StableGeneration = StableGeneration( outerInstance.generation );
						 UnstableGeneration = UnstableGeneration( outerInstance.generation );
						 this.RatioToKeepInLeftOnSplit = ratioToKeepInLeftOnSplit;
						 Debug.Assert( assertNoSuccessor( Cursor, StableGeneration, UnstableGeneration ) );
						 TreeLogic.initialize( Cursor, ratioToKeepInLeftOnSplit );
						 success = true;
					}
					catch ( Exception e )
					{
						 outerInstance.appendTreeInformation( e );
						 throw e;
					}
					finally
					{
						 if ( !success )
						 {
							  Close();
						 }
					}
			  }

			  public override void Put( KEY key, VALUE value )
			  {
					Merge( key, value, ValueMergers.Overwrite() );
			  }

			  public override void Merge( KEY key, VALUE value, ValueMerger<KEY, VALUE> valueMerger )
			  {
					try
					{
						 TreeLogic.insert( Cursor, StructurePropagation, key, value, valueMerger, StableGeneration, UnstableGeneration );

						 HandleStructureChanges();
					}
					catch ( IOException e )
					{
						 outerInstance.appendTreeInformation( e );
						 throw new UncheckedIOException( e );
					}
					catch ( Exception t )
					{
						 outerInstance.appendTreeInformation( t );
						 throw t;
					}

					checkOutOfBounds( Cursor );
			  }

			  internal virtual long Root
			  {
				  set
				  {
						long rootId = GenerationSafePointerPair.Pointer( value );
						_outerInstance.setRoot( rootId, UnstableGeneration );
						TreeLogic.initialize( Cursor, RatioToKeepInLeftOnSplit );
				  }
			  }

			  public override VALUE Remove( KEY key )
			  {
					VALUE result;
					try
					{
						 result = TreeLogic.remove( Cursor, StructurePropagation, key, outerInstance.layout.NewValue(), StableGeneration, UnstableGeneration );

						 HandleStructureChanges();
					}
					catch ( IOException e )
					{
						 outerInstance.appendTreeInformation( e );
						 throw new UncheckedIOException( e );
					}
					catch ( Exception e )
					{
						 outerInstance.appendTreeInformation( e );
						 throw e;
					}

					checkOutOfBounds( Cursor );
					return result;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void handleStructureChanges() throws java.io.IOException
			  internal virtual void HandleStructureChanges()
			  {
					if ( StructurePropagation.hasRightKeyInsert )
					{
						 // New root
						 long newRootId = outerInstance.freeList.AcquireNewId( StableGeneration, UnstableGeneration );
						 PageCursorUtil.GoTo( Cursor, "new root", newRootId );

						 outerInstance.bTreeNode.InitializeInternal( Cursor, StableGeneration, UnstableGeneration );
						 outerInstance.bTreeNode.SetChildAt( Cursor, StructurePropagation.midChild, 0, StableGeneration, UnstableGeneration );
						 outerInstance.bTreeNode.InsertKeyAndRightChildAt( Cursor, StructurePropagation.rightKey, StructurePropagation.rightChild, 0, 0, StableGeneration, UnstableGeneration );
						 TreeNode.SetKeyCount( Cursor, 1 );
						 Root = newRootId;
						 outerInstance.monitor.TreeGrowth();
					}
					else if ( StructurePropagation.hasMidChildUpdate )
					{
						 Root = StructurePropagation.midChild;
					}
					StructurePropagation.clear();
			  }

			  public override void Close()
			  {
					if ( !WriterTaken.compareAndSet( true, false ) )
					{
						 throw new System.InvalidOperationException( "Tried to close writer of " + _outerInstance + ", but writer is already closed." );
					}
					CloseCursor();
					outerInstance.@lock.WriterAndCleanerUnlock();
			  }

			  internal virtual void CloseCursor()
			  {
					if ( Cursor != null )
					{
						 Cursor.close();
						 Cursor = null;
					}
			  }
		 }

		 public virtual bool WasDirtyOnStartup()
		 {
			  return _dirtyOnStartup;
		 }

		 /// <summary>
		 /// Total size limit for key and value.
		 /// This limit includes storage overhead that is specific to key implementation for example entity id or meta data about type. </summary>
		 /// <returns> Total size limit for key and value or <seealso cref="TreeNode.NO_KEY_VALUE_SIZE_CAP"/> if no such value exists. </returns>
		 public virtual int KeyValueSizeCap()
		 {
			  return _bTreeNode.keyValueSizeCap();
		 }
	}

}