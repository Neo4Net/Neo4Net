using System;
using System.Collections.Generic;
using System.Text;
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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{

	using MemoryAllocator = Org.Neo4j.Io.mem.MemoryAllocator;
	using EvictionRunEvent = Org.Neo4j.Io.pagecache.tracing.EvictionRunEvent;
	using FlushEventOpportunity = Org.Neo4j.Io.pagecache.tracing.FlushEventOpportunity;
	using MajorFlushEvent = Org.Neo4j.Io.pagecache.tracing.MajorFlushEvent;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageFaultEvent = Org.Neo4j.Io.pagecache.tracing.PageFaultEvent;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using GlobalMemoryTracker = Org.Neo4j.Memory.GlobalMemoryTracker;
	using MemoryAllocationTracker = Org.Neo4j.Memory.MemoryAllocationTracker;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.flag;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.getInteger;

	/// <summary>
	/// The Muninn <seealso cref="org.neo4j.io.pagecache.PageCache page cache"/> implementation.
	/// <pre>
	///                                                                      ....
	///                                                                .;okKNWMUWN0ko,
	///        O'er Mithgarth Hugin and Munin both                   ;0WMUNINNMUNINNMUNOdc:.
	///        Each day set forth to fly;                          .OWMUNINNMUNI  00WMUNINNXko;.
	///        For Hugin I fear lest he come not home,            .KMUNINNMUNINNMWKKWMUNINNMUNIN0l.
	///        But for Munin my care is more.                    .KMUNINNMUNINNMUNINNWKkdlc:::::::'
	///                                                        .lXMUNINNMUNINNMUNINXo'
	///                                                    .,lONMUNINNMUNINNMUNINNk'
	///                                              .,cox0NMUNINNMUNINNMUNINNMUNI:
	///                                         .;dONMUNINNMUNINNMUNINNMUNINNMUNIN'
	///                                   .';okKWMUNINNMUNINNMUNINNMUNINNMUNINNMUx
	///                              .:dkKNWMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNN'
	///                        .';lONMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNWl
	///                       .:okXWMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNM0'
	///                   .,oONMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNo
	///             .';lx0NMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUN0'
	///          ;kKWMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMWx'
	///        .,kWMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMXd'
	///   .;lkKNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMUNINNMNx;'
	///   .oNMUNINNMWNKOxoc;'';:cdkKNWMUNINNMUNINNMUNINNMUNINNMUNINWKx;'
	///    lkOkkdl:'´                `':lkWMUNINNMUNINNMUNINN0kdoc;'
	///                                  c0WMUNINNMUNINNMUWx'
	///                                   .;ccllllxNMUNIXo'
	///                                           lWMUWkK;   .
	///                                           OMUNK.dNdc,....
	///                                           cWMUNlkWWWO:cl;.
	///                                            ;kWO,....',,,.
	///                                              cNd
	///                                               :Nk.
	///                                                cWK,
	///                                             .,ccxXWd.
	///                                                   dWNkxkOdc::;.
	///                                                    cNNo:ldo:.
	///                                                     'xo.   ..
	/// </pre>
	/// <para>
	///     In Norse mythology, Huginn (from Old Norse "thought") and Muninn (Old Norse
	///     "memory" or "mind") are a pair of ravens that fly all over the world, Midgard,
	///     and bring information to the god Odin.
	/// </para>
	/// <para>
	///     This implementation of <seealso cref="org.neo4j.io.pagecache.PageCache"/> is optimised for
	///     configurations with large memory capacities and large stores, and uses sequence
	///     locks to make uncontended reads and writes fast.
	/// </para>
	/// </summary>
	public class MuninnPageCache : PageCache
	{
		 public static readonly sbyte ZeroByte = ( sbyte )( flag( typeof( MuninnPageCache ), "brandedZeroByte", false ) ? 0x0f : 0 );

		 // The amount of memory we need for every page, both its buffer and its meta-data.
		 private static readonly int _memoryUsePerPage = Org.Neo4j.Io.pagecache.PageCache_Fields.PAGE_SIZE + PageList.META_DATA_BYTES_PER_PAGE;

		 // Keep this many pages free and ready for use in faulting.
		 // This will be truncated to be no more than half of the number of pages
		 // in the cache.
		 private static readonly int _pagesToKeepFree = getInteger( typeof( MuninnPageCache ), "pagesToKeepFree", 30 );

		 // This is how many times that, during cooperative eviction, we'll iterate through the entire set of pages looking
		 // for a page to evict, before we give up and throw CacheLiveLockException. This MUST be greater than 1.
		 private static readonly int _cooperativeEvictionLiveLockThreshold = getInteger( typeof( MuninnPageCache ), "cooperativeEvictionLiveLockThreshold", 100 );

		 // This is a pre-allocated constant, so we can throw it without allocating any objects:
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ThrowableInstanceNeverThrown") private static final java.io.IOException oomException = new java.io.IOException("OutOfMemoryError encountered in the page cache background eviction thread");
		 private static readonly IOException _oomException = new IOException( "OutOfMemoryError encountered in the page cache background eviction thread" );

		 // The field offset to unsafely access the freelist field.
		 private static readonly long _freelistOffset = UnsafeUtil.getFieldOffset( typeof( MuninnPageCache ), "freelist" );

		 // This is used as a poison-pill signal in the freelist, to inform any
		 // page faulting thread that it is now no longer possible to queue up and
		 // wait for more pages to be evicted, because the page cache has been shut
		 // down.
		 private static readonly FreePage _shutdownSignal = new FreePage( 0 );

		 // A counter used to identify which background threads belong to which page cache.
		 private static readonly AtomicInteger _pageCacheIdCounter = new AtomicInteger();

		 // Scheduler that runs all the background jobs for page cache.
		 private readonly JobScheduler _scheduler;

		 private static readonly IList<OpenOption> _ignoredOpenOptions = Arrays.asList( StandardOpenOption.APPEND, StandardOpenOption.READ, StandardOpenOption.WRITE, StandardOpenOption.SPARSE );

		 // Used when trying to figure out number of available pages in a page cache. Could be returned from tryGetNumberOfAvailablePages.
		 private const int UNKNOWN_AVAILABLE_PAGES = -1;

		 private readonly int _pageCacheId;
		 private readonly PageSwapperFactory _swapperFactory;
		 private readonly int _cachePageSize;
		 private readonly int _keepFree;
		 private readonly PageCacheTracer _pageCacheTracer;
		 private readonly PageCursorTracerSupplier _pageCursorTracerSupplier;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 internal readonly PageList Pages;
		 // All PageCursors are initialised with their pointers pointing to the victim page. This way, we don't have to throw
		 // exceptions on bounds checking failures; we can instead return the victim page pointer, and permit the page
		 // accesses to take place without fear of segfaulting newly allocated cursors.
		 internal readonly long VictimPage;

		 // The freelist is a thread-safe linked-list of FreePage objects, or an AtomicInteger, or null.
		 // Initially, the field is an AtomicInteger that counts from zero to the max page count, at which point all of the
		 // pages have been put in use. Once this happens, the field is set to null to allow the background eviction thread
		 // to start its work. From that point on, the field will operate as a concurrent stack of FreePage objects. The
		 // eviction thread pushes newly freed FreePage objects onto the stack, and page faulting threads pops FreePage
		 // objects from the stack. The FreePage objects are single-use, to avoid running into the ABA-problem.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile Object freelist;
		 private volatile object _freelist;

		 // Linked list of mappings - guarded by synchronized(this)
		 private volatile FileMapping _mappedFiles;

		 // The thread that runs the eviction algorithm. We unpark this when we've run out of
		 // free pages to grab.
		 private volatile Thread _evictionThread;
		 // True if the eviction thread is currently parked, without someone having
		 // signalled it to wake up. This is used as a weak guard for unparking the
		 // eviction thread, because calling unpark too much (from many page
		 // faulting threads) can cause contention on the locks protecting that
		 // threads scheduling meta-data in the OS kernel.
		 private volatile bool _evictorParked;
		 private volatile IOException _evictorException;

		 // Flag for when page cache is closed - writes guarded by synchronized(this), reads can be unsynchronized
		 private volatile bool _closed;

		 // Only used by ensureThreadsInitialised while holding the monitor lock on this MuninnPageCache instance.
		 private bool _threadsInitialised;

		 // 'true' (the default) if we should print any exceptions we get when unmapping a file.
		 private bool _printExceptionsOnClose;

		 /// <summary>
		 /// Compute the amount of memory needed for a page cache with the given number of 8 KiB pages. </summary>
		 /// <param name="pageCount"> The number of pages </param>
		 /// <returns> The memory required for the buffers and meta-data of the given number of pages </returns>
		 public static long MemoryRequiredForPages( long pageCount )
		 {
			  return pageCount * _memoryUsePerPage;
		 }

		 /// <summary>
		 /// Create page cache. </summary>
		 /// <param name="swapperFactory"> page cache swapper factory </param>
		 /// <param name="maxPages"> maximum number of pages </param>
		 /// <param name="pageCacheTracer"> global page cache tracer </param>
		 /// <param name="pageCursorTracerSupplier"> supplier of thread local (transaction local) page cursor tracer that will provide
		 /// thread local page cache statistics </param>
		 /// <param name="versionContextSupplier"> supplier of thread local (transaction local) version context that will provide
		 /// access to thread local version context </param>
		 public MuninnPageCache( PageSwapperFactory swapperFactory, int maxPages, PageCacheTracer pageCacheTracer, PageCursorTracerSupplier pageCursorTracerSupplier, VersionContextSupplier versionContextSupplier, JobScheduler jobScheduler ) : this( swapperFactory, MemoryAllocator.createAllocator( "" + MemoryRequiredForPages( maxPages ), GlobalMemoryTracker.INSTANCE ), org.neo4j.io.pagecache.PageCache_Fields.PAGE_SIZE, pageCacheTracer, pageCursorTracerSupplier, versionContextSupplier, jobScheduler )
		 {
		 }

		 /// <summary>
		 /// Create page cache. </summary>
		 /// <param name="swapperFactory"> page cache swapper factory </param>
		 /// <param name="memoryAllocator"> the source of native memory the page cache should use </param>
		 /// <param name="pageCacheTracer"> global page cache tracer </param>
		 /// <param name="pageCursorTracerSupplier"> supplier of thread local (transaction local) page cursor tracer that will provide
		 /// thread local page cache statistics </param>
		 /// <param name="versionContextSupplier"> supplier of thread local (transaction local) version context that will provide
		 ///        access to thread local version context </param>
		 public MuninnPageCache( PageSwapperFactory swapperFactory, MemoryAllocator memoryAllocator, PageCacheTracer pageCacheTracer, PageCursorTracerSupplier pageCursorTracerSupplier, VersionContextSupplier versionContextSupplier, JobScheduler jobScheduler ) : this( swapperFactory, memoryAllocator, org.neo4j.io.pagecache.PageCache_Fields.PAGE_SIZE, pageCacheTracer, pageCursorTracerSupplier, versionContextSupplier, jobScheduler )
		 {
		 }

		 /// <summary>
		 /// Constructor variant that allows setting a non-standard cache page size.
		 /// Only ever use this for testing.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("DeprecatedIsStillUsed") @Deprecated public MuninnPageCache(org.neo4j.io.pagecache.PageSwapperFactory swapperFactory, org.neo4j.io.mem.MemoryAllocator memoryAllocator, int cachePageSize, org.neo4j.io.pagecache.tracing.PageCacheTracer pageCacheTracer, org.neo4j.io.pagecache.tracing.cursor.PageCursorTracerSupplier pageCursorTracerSupplier, org.neo4j.io.pagecache.tracing.cursor.context.VersionContextSupplier versionContextSupplier, org.neo4j.scheduler.JobScheduler jobScheduler)
		 [Obsolete]
		 public MuninnPageCache( PageSwapperFactory swapperFactory, MemoryAllocator memoryAllocator, int cachePageSize, PageCacheTracer pageCacheTracer, PageCursorTracerSupplier pageCursorTracerSupplier, VersionContextSupplier versionContextSupplier, JobScheduler jobScheduler )
		 {
			  VerifyHacks();
			  VerifyCachePageSizeIsPowerOfTwo( cachePageSize );
			  int maxPages = CalculatePageCount( memoryAllocator, cachePageSize );

			  // Expose the total number of pages
			  pageCacheTracer.MaxPages( maxPages );
			  MemoryAllocationTracker memoryTracker = GlobalMemoryTracker.INSTANCE;

			  this._pageCacheId = _pageCacheIdCounter.incrementAndGet();
			  this._swapperFactory = swapperFactory;
			  this._cachePageSize = cachePageSize;
			  this._keepFree = Math.Min( _pagesToKeepFree, maxPages / 2 );
			  this._pageCacheTracer = pageCacheTracer;
			  this._pageCursorTracerSupplier = pageCursorTracerSupplier;
			  this._versionContextSupplier = versionContextSupplier;
			  this._printExceptionsOnClose = true;
			  long alignment = swapperFactory.RequiredBufferAlignment;
			  this.VictimPage = VictimPageReference.GetVictimPage( cachePageSize, memoryTracker );
			  this.Pages = new PageList( maxPages, cachePageSize, memoryAllocator, new SwapperSet(), VictimPage, alignment );
			  this._scheduler = jobScheduler;

			  FreelistHead = new AtomicInteger();
		 }

		 private static void VerifyHacks()
		 {
			  // Make sure that we have access to theUnsafe.
			  UnsafeUtil.assertHasUnsafe();
		 }

		 private static void VerifyCachePageSizeIsPowerOfTwo( int cachePageSize )
		 {
			  int exponent = 31 - Integer.numberOfLeadingZeros( cachePageSize );
			  if ( 1 << exponent != cachePageSize )
			  {
					throw new System.ArgumentException( "Cache page size must be a power of two, but was " + cachePageSize );
			  }
		 }

		 private static int CalculatePageCount( MemoryAllocator memoryAllocator, int cachePageSize )
		 {
			  long memoryPerPage = cachePageSize + PageList.META_DATA_BYTES_PER_PAGE;
			  long maxPages = memoryAllocator.AvailableMemory() / memoryPerPage;
			  int minimumPageCount = 2;
			  if ( maxPages < minimumPageCount )
			  {
					throw new System.ArgumentException( format( "Page cache must have at least %s pages (%s bytes of memory), but was given %s pages.", minimumPageCount, minimumPageCount * memoryPerPage, maxPages ) );
			  }
			  maxPages = Math.Min( maxPages, PageList.MaxPages );
			  return Math.toIntExact( maxPages );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized org.neo4j.io.pagecache.PagedFile map(java.io.File file, int filePageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
		 public override PagedFile Map( File file, int filePageSize, params OpenOption[] openOptions )
		 {
			 lock ( this )
			 {
				  AssertHealthy();
				  EnsureThreadsInitialised();
				  if ( filePageSize > _cachePageSize )
				  {
						throw new System.ArgumentException( "Cannot map files with a filePageSize (" + filePageSize + ") that is greater than the " + "cachePageSize (" + _cachePageSize + ")" );
				  }
				  file = file.CanonicalFile;
				  bool createIfNotExists = false;
				  bool truncateExisting = false;
				  bool deleteOnClose = false;
				  bool anyPageSize = false;
				  bool noChannelStriping = false;
				  foreach ( OpenOption option in openOptions )
				  {
						if ( option.Equals( StandardOpenOption.CREATE ) )
						{
							 createIfNotExists = true;
						}
						else if ( option.Equals( StandardOpenOption.TRUNCATE_EXISTING ) )
						{
							 truncateExisting = true;
						}
						else if ( option.Equals( StandardOpenOption.DELETE_ON_CLOSE ) )
						{
							 deleteOnClose = true;
						}
						else if ( option.Equals( PageCacheOpenOptions.ANY_PAGE_SIZE ) )
						{
							 anyPageSize = true;
						}
						else if ( option.Equals( PageCacheOpenOptions.NO_CHANNEL_STRIPING ) )
						{
							 noChannelStriping = true;
						}
						else if ( !_ignoredOpenOptions.Contains( option ) )
						{
							 throw new System.NotSupportedException( "Unsupported OpenOption: " + option );
						}
				  }
      
				  FileMapping current = _mappedFiles;
      
				  // find an existing mapping
				  while ( current != null )
				  {
						if ( current.File.Equals( file ) )
						{
							 MuninnPagedFile pagedFile = current.PagedFile;
							 if ( pagedFile.PageSize() != filePageSize && !anyPageSize )
							 {
								  string msg = "Cannot map file " + file + " with " +
											 "filePageSize " + filePageSize + " bytes, " +
											 "because it has already been mapped with a " +
											 "filePageSize of " + pagedFile.PageSize() +
											 " bytes.";
								  throw new System.ArgumentException( msg );
							 }
							 if ( truncateExisting )
							 {
								  throw new System.NotSupportedException( "Cannot truncate a file that is already mapped" );
							 }
							 pagedFile.IncrementRefCount();
							 pagedFile.MarkDeleteOnClose( deleteOnClose );
							 return pagedFile;
						}
						current = current.Next;
				  }
      
				  if ( filePageSize < Long.BYTES )
				  {
						throw new System.ArgumentException( "Cannot map files with a filePageSize (" + filePageSize + ") that is less than " + Long.BYTES + " bytes" );
				  }
      
				  // there was no existing mapping
				  MuninnPagedFile pagedFile = new MuninnPagedFile( file, this, filePageSize, _swapperFactory, _pageCacheTracer, _pageCursorTracerSupplier, _versionContextSupplier, createIfNotExists, truncateExisting, noChannelStriping );
				  pagedFile.IncrementRefCount();
				  pagedFile.MarkDeleteOnClose( deleteOnClose );
				  current = new FileMapping( file, pagedFile );
				  current.Next = _mappedFiles;
				  _mappedFiles = current;
				  _pageCacheTracer.mappedFile( file );
				  return pagedFile;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized java.util.Optional<org.neo4j.io.pagecache.PagedFile> getExistingMapping(java.io.File file) throws java.io.IOException
		 public override Optional<PagedFile> GetExistingMapping( File file )
		 {
			 lock ( this )
			 {
				  AssertHealthy();
				  EnsureThreadsInitialised();
      
				  file = file.CanonicalFile;
				  MuninnPagedFile pagedFile = TryGetMappingOrNull( file );
				  if ( pagedFile != null )
				  {
						pagedFile.IncrementRefCount();
						return pagedFile;
				  }
				  return null;
			 }
		 }

		 private MuninnPagedFile TryGetMappingOrNull( File file )
		 {
			  FileMapping current = _mappedFiles;

			  // find an existing mapping
			  while ( current != null )
			  {
					if ( current.File.Equals( file ) )
					{
						 return current.PagedFile;
					}
					current = current.Next;
			  }

			  // no mapping exists
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized java.util.List<org.neo4j.io.pagecache.PagedFile> listExistingMappings() throws java.io.IOException
		 public override IList<PagedFile> ListExistingMappings()
		 {
			 lock ( this )
			 {
				  AssertNotClosed();
				  EnsureThreadsInitialised();
      
				  IList<PagedFile> list = new List<PagedFile>();
				  FileMapping current = _mappedFiles;
      
				  while ( current != null )
				  {
						// Note that we are NOT incrementing the reference count here.
						// Calling code is expected to be able to deal with asynchronously closed PagedFiles.
						MuninnPagedFile pagedFile = current.PagedFile;
						list.Add( pagedFile );
						current = current.Next;
				  }
				  return list;
			 }
		 }

		 /// <summary>
		 /// Note: Must be called while synchronizing on the MuninnPageCache instance.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureThreadsInitialised() throws java.io.IOException
		 private void EnsureThreadsInitialised()
		 {
			  if ( _threadsInitialised )
			  {
					return;
			  }
			  _threadsInitialised = true;

			  try
			  {
					_scheduler.schedule( Group.PAGE_CACHE, new EvictionTask( this ) );
			  }
			  catch ( Exception e )
			  {
					IOException exception = new IOException( e );
					try
					{
						 Close();
					}
					catch ( Exception closeException )
					{
						 exception.addSuppressed( closeException );
					}
					throw exception;
			  }
		 }

		 internal virtual void Unmap( MuninnPagedFile file )
		 {
			 lock ( this )
			 {
				  if ( file.DecrementRefCount() )
				  {
						// This was the last reference!
						// Find and remove the existing mapping:
						FileMapping prev = null;
						FileMapping current = _mappedFiles;
      
						while ( current != null )
						{
							 if ( current.PagedFile == file )
							 {
								  if ( prev == null )
								  {
										_mappedFiles = current.Next;
								  }
								  else
								  {
										prev.Next = current.Next;
								  }
								  _pageCacheTracer.unmappedFile( current.File );
								  FlushAndCloseWithoutFail( file );
								  break;
							 }
							 prev = current;
							 current = current.Next;
						}
				  }
			 }
		 }

		 private void FlushAndCloseWithoutFail( MuninnPagedFile file )
		 {
			  bool flushedAndClosed = false;
			  bool printedFirstException = false;
			  do
			  {
					try
					{
						 file.FlushAndForceForClose();
						 file.CloseSwapper();
						 flushedAndClosed = true;
					}
					catch ( IOException e )
					{
						 if ( _printExceptionsOnClose && !printedFirstException )
						 {
							  printedFirstException = true;
							  try
							  {
									Console.WriteLine( e.ToString() );
									Console.Write( e.StackTrace );
							  }
							  catch ( Exception )
							  {
							  }
						 }
					}
			  } while ( !flushedAndClosed );
		 }

		 public virtual bool PrintExceptionsOnClose
		 {
			 set
			 {
				  this._printExceptionsOnClose = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce() throws java.io.IOException
		 public override void FlushAndForce()
		 {
			  FlushAndForce( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 public override void FlushAndForce( IOLimiter limiter )
		 {
			  if ( limiter == null )
			  {
					throw new System.ArgumentException( "IOLimiter cannot be null" );
			  }
			  IList<PagedFile> files = ListExistingMappings();

			  using ( MajorFlushEvent ignored = _pageCacheTracer.beginCacheFlush() )
			  {
					if ( limiter.Limited )
					{
						 FlushAllPages( files, limiter );
					}
					else
					{
						 FlushAllPagesParallel( files, limiter );
					}
					SyncDevice();
			  }
			  ClearEvictorException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void flushAllPages(java.util.List<org.neo4j.io.pagecache.PagedFile> files, org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 private void FlushAllPages( IList<PagedFile> files, IOLimiter limiter )
		 {
			  foreach ( PagedFile file in files )
			  {
					FlushFile( ( MuninnPagedFile ) file, limiter );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void flushAllPagesParallel(java.util.List<org.neo4j.io.pagecache.PagedFile> files, org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 private void FlushAllPagesParallel( IList<PagedFile> files, IOLimiter limiter )
		 {
			  IList<JobHandle> flushes = new List<JobHandle>( Files.Count );

			  // Submit all flushes to the background thread
			  foreach ( PagedFile file in files )
			  {
					flushes.Add(_scheduler.schedule(Group.PAGE_CACHE, () =>
					{
					 try
					 {
						  FlushFile( ( MuninnPagedFile ) file, limiter );
					 }
					 catch ( IOException e )
					 {
						  throw new UncheckedIOException( e );
					 }
					}));
			  }

			  // Wait for all to complete
			  foreach ( JobHandle flush in flushes )
			  {
					try
					{
						 flush.WaitTermination();
					}
					catch ( Exception e ) when ( e is InterruptedException || e is ExecutionException )
					{
						 throw new IOException( e );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void flushFile(MuninnPagedFile muninnPagedFile, org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 private void FlushFile( MuninnPagedFile muninnPagedFile, IOLimiter limiter )
		 {
			  try
			  {
					  using ( MajorFlushEvent fileFlush = _pageCacheTracer.beginFileFlush( muninnPagedFile.Swapper ) )
					  {
						FlushEventOpportunity flushOpportunity = fileFlush.FlushEventOpportunity();
						muninnPagedFile.FlushAndForceInternal( flushOpportunity, false, limiter );
					  }
			  }
			  catch ( ClosedChannelException e )
			  {
					if ( muninnPagedFile.RefCount > 0 )
					{
						 // The file is not supposed to be closed, since we have a positive ref-count, yet we got a
						 // ClosedChannelException anyway? It's an odd situation, so let's tell the outside world about
						 // this failure.
						 throw e;
					}
					// Otherwise: The file was closed while we were trying to flush it. Since unmapping implies a flush
					// anyway, we can safely assume that this is not a problem. The file was flushed, and it doesn't
					// really matter how that happened. We'll ignore this exception.
			  }
		 }

		 internal virtual void SyncDevice()
		 {
			  _swapperFactory.syncDevice();
		 }

		 public override void Close()
		 {
			 lock ( this )
			 {
				  if ( _closed )
				  {
						return;
				  }
      
				  FileMapping files = _mappedFiles;
				  if ( files != null )
				  {
						StringBuilder msg = new StringBuilder( "Cannot close the PageCache while files are still mapped:" );
						while ( files != null )
						{
							 int refCount = Files.pagedFile.RefCount;
							 msg.Append( "\n\t" );
							 msg.Append( Files.file );
							 msg.Append( " (" ).Append( refCount );
							 msg.Append( refCount == 1 ? " mapping)" : " mappings)" );
							 files = Files.next;
						}
						throw new System.InvalidOperationException( msg.ToString() );
				  }
      
				  _closed = true;
      
				  Interrupt( _evictionThread );
				  _evictionThread = null;
      
				  // Close the page swapper factory last. If this fails then we will still consider ourselves closed.
				  _swapperFactory.close();
			 }
		 }

		 private static void Interrupt( Thread thread )
		 {
			  if ( thread != null )
			  {
					thread.Interrupt();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void finalize() throws Throwable
		 protected internal override void Finalize()
		 {
			  Close();
			  base.Finalize();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertHealthy() throws java.io.IOException
		 private void AssertHealthy()
		 {
			  AssertNotClosed();
			  IOException exception = _evictorException;
			  if ( exception != null )
			  {
					throw new IOException( "Exception in the page eviction thread", exception );
			  }
		 }

		 private void AssertNotClosed()
		 {
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "The PageCache has been shut down" );
			  }
		 }

		 public override int PageSize()
		 {
			  return _cachePageSize;
		 }

		 public override long MaxCachedPages()
		 {
			  return Pages.PageCount;
		 }

		 public override void ReportEvents()
		 {
			  _pageCursorTracerSupplier.get().reportEvents();
		 }

		 internal virtual int PageCacheId
		 {
			 get
			 {
				  return _pageCacheId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long grabFreeAndExclusivelyLockedPage(org.neo4j.io.pagecache.tracing.PageFaultEvent faultEvent) throws java.io.IOException
		 internal virtual long GrabFreeAndExclusivelyLockedPage( PageFaultEvent faultEvent )
		 {
			  // Review the comment on the freelist field before making changes to
			  // this part of the code.
			  // Whatever the case, we're going to the head-pointer of the freelist,
			  // and in doing so, we can discover a number of things.
			  // We can discover a MuninnPage object, in which case we can try to
			  // CAS the freelist pointer to the value of the MuninnPage.nextFree
			  // pointer, and if this succeeds then we've grabbed that page.
			  // We can discover a FreePage object, in which case we'll do a similar
			  // dance by attempting to CAS the freelist to the FreePage objects next
			  // pointer, and again, if we succeed then we've grabbed the MuninnPage
			  // given by the FreePage object.
			  // We can discover a null-pointer, in which case the freelist has just
			  // been emptied for whatever it contained before. New FreePage objects
			  // are eventually going to be added to the freelist, but we are not
			  // going to wait around for that to happen. If the freelist is empty,
			  // then we do our own eviction to get a free page.
			  // If we find a FreePage object on the freelist, then it is important
			  // to check and see if it is the shutdownSignal instance. If that's the
			  // case, then the page cache has been shut down, and we should throw an
			  // exception from our page fault routine.
			  object current;
			  for ( ;; )
			  {
					AssertHealthy();
					current = FreelistHead;
					if ( current == null )
					{
						 UnparkEvictor();
						 long pageRef = CooperativelyEvict( faultEvent );
						 if ( pageRef != 0 )
						 {
							  return pageRef;
						 }
					}
					else if ( current is AtomicInteger )
					{
						 int pageCount = Pages.PageCount;
						 AtomicInteger counter = ( AtomicInteger ) current;
						 int pageId = counter.get();
						 if ( pageId < pageCount && counter.compareAndSet( pageId, pageId + 1 ) )
						 {
							  return Pages.deref( pageId );
						 }
						 if ( pageId >= pageCount )
						 {
							  CompareAndSetFreelistHead( current, null );
						 }
					}
					else if ( current is FreePage )
					{
						 FreePage freePage = ( FreePage ) current;
						 if ( freePage == _shutdownSignal )
						 {
							  throw new System.InvalidOperationException( "The PageCache has been shut down." );
						 }

						 if ( CompareAndSetFreelistHead( freePage, freePage.NextConflict ) )
						 {
							  return freePage.PageRef;
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long cooperativelyEvict(org.neo4j.io.pagecache.tracing.PageFaultEvent faultEvent) throws java.io.IOException
		 private long CooperativelyEvict( PageFaultEvent faultEvent )
		 {
			  int iterations = 0;
			  int pageCount = Pages.PageCount;
			  int clockArm = ThreadLocalRandom.current().Next(pageCount);
			  bool evicted = false;
			  long pageRef;
			  do
			  {
					AssertHealthy();
					if ( FreelistHead != null )
					{
						 return 0;
					}

					if ( clockArm == pageCount )
					{
						 if ( iterations == _cooperativeEvictionLiveLockThreshold )
						 {
							  throw CooperativeEvictionLiveLock();
						 }
						 iterations++;
						 clockArm = 0;
					}

					pageRef = Pages.deref( clockArm );
					if ( Pages.isLoaded( pageRef ) && Pages.decrementUsage( pageRef ) )
					{
						 evicted = Pages.tryEvict( pageRef, faultEvent );
					}
					clockArm++;
			  } while ( !evicted );
			  return pageRef;
		 }

		 private CacheLiveLockException CooperativeEvictionLiveLock()
		 {
			  return new CacheLiveLockException( "Live-lock encountered when trying to cooperatively evict a page during page fault. " + "This happens when we want to access a page that is not in memory, so it has to be faulted in, but " + "there are no free memory pages available to accept the page fault, so we have to evict an existing " + "page, but all the in-memory pages are currently locked by other accesses. If those other access are " + "waiting for our page fault to make progress, then we have a live-lock, and the only way we can get " + "out of it is by throwing this exception. This should be extremely rare, but can happen if the page " + "cache size is tiny and the number of concurrently running transactions is very high. You should be " + "able to get around this problem by increasing the amount of memory allocated to the page cache " + "with the `dbms.memory.pagecache.size` setting. Please contact Neo4j support if you need help tuning " + "your database." );
		 }

		 private void UnparkEvictor()
		 {
			  if ( _evictorParked )
			  {
					_evictorParked = false;
					LockSupport.unpark( _evictionThread );
			  }
		 }

		 private void ParkEvictor( long parkNanos )
		 {
			  // Only called from the background eviction thread!
			  _evictorParked = true;
			  LockSupport.parkNanos( this, parkNanos );
			  _evictorParked = false;
		 }

		 private object FreelistHead
		 {
			 get
			 {
				  return UnsafeUtil.getObjectVolatile( this, _freelistOffset );
			 }
			 set
			 {
				  UnsafeUtil.putObjectVolatile( this, _freelistOffset, value );
			 }
		 }

		 private bool CompareAndSetFreelistHead( object expected, object update )
		 {
			  return UnsafeUtil.compareAndSwapObject( this, _freelistOffset, expected, update );
		 }


		 /// <summary>
		 /// Scan through all the pages, one by one, and decrement their usage stamps.
		 /// If a usage reaches zero, we try-write-locking it, and if we get that lock,
		 /// we evict the page. If we don't, we move on to the next page.
		 /// Once we have enough free pages, we park our thread. Page-faulting will
		 /// unpark our thread as needed.
		 /// </summary>
		 internal virtual void ContinuouslySweepPages()
		 {
			  _evictionThread = Thread.CurrentThread;
			  int clockArm = 0;

			  while ( !_closed )
			  {
					int pageCountToEvict = ParkUntilEvictionRequired( _keepFree );
					using ( EvictionRunEvent evictionRunEvent = _pageCacheTracer.beginPageEvictions( pageCountToEvict ) )
					{
						 clockArm = EvictPages( pageCountToEvict, clockArm, evictionRunEvent );
					}
			  }

			  // The last thing we do, is signalling the shutdown of the cache via
			  // the freelist. This signal is looked out for in grabFreePage.
			  FreelistHead = _shutdownSignal;
		 }

		 private int ParkUntilEvictionRequired( int keepFree )
		 {
			  // Park until we're either interrupted, or the number of free pages drops
			  // bellow keepFree.
			  long parkNanos = TimeUnit.MILLISECONDS.toNanos( 10 );
			  for ( ;; )
			  {
					ParkEvictor( parkNanos );
					if ( Thread.interrupted() || _closed )
					{
						 return 0;
					}

					int availablePages = TryGetNumberOfAvailablePages( keepFree );
					if ( availablePages != UNKNOWN_AVAILABLE_PAGES )
					{
						 return availablePages;
					}
			  }
		 }

		 private int TryGetNumberOfAvailablePages( int keepFree )
		 {
			  object freelistHead = FreelistHead;

			  if ( freelistHead == null )
			  {
					return keepFree;
			  }
			  else if ( freelistHead.GetType() == typeof(FreePage) )
			  {
					int availablePages = ( ( FreePage ) freelistHead ).Count;
					if ( availablePages < keepFree )
					{
						 return keepFree - availablePages;
					}
			  }
			  else if ( freelistHead.GetType() == typeof(AtomicInteger) )
			  {
					AtomicInteger counter = ( AtomicInteger ) freelistHead;
					long count = Pages.PageCount - counter.get();
					if ( count < keepFree )
					{
						 return count < 0 ? keepFree : ( int )( keepFree - count );
					}
			  }
			  return UNKNOWN_AVAILABLE_PAGES;
		 }

		 internal virtual int EvictPages( int pageCountToEvict, int clockArm, EvictionRunEvent evictionRunEvent )
		 {
			  while ( pageCountToEvict > 0 && !_closed )
			  {
					if ( clockArm == Pages.PageCount )
					{
						 clockArm = 0;
					}

					if ( _closed )
					{
						 // The page cache has been shut down.
						 return 0;
					}

					long pageRef = Pages.deref( clockArm );
					if ( Pages.isLoaded( pageRef ) && Pages.decrementUsage( pageRef ) )
					{
						 try
						 {
							  pageCountToEvict--;
							  if ( Pages.tryEvict( pageRef, evictionRunEvent ) )
							  {
									ClearEvictorException();
									AddFreePageToFreelist( pageRef );
							  }
						 }
						 catch ( IOException e )
						 {
							  _evictorException = e;
						 }
						 catch ( System.OutOfMemoryException )
						 {
							  _evictorException = _oomException;
						 }
						 catch ( Exception th )
						 {
							  _evictorException = new IOException( "Eviction thread encountered a problem", th );
						 }
					}

					clockArm++;
			  }

			  return clockArm;
		 }

		 internal virtual void AddFreePageToFreelist( long pageRef )
		 {
			  object current;
			  FreePage freePage = new FreePage( pageRef );
			  do
			  {
					current = FreelistHead;
					if ( current is AtomicInteger && ( ( AtomicInteger ) current ).get() > Pages.PageCount )
					{
						 current = null;
					}
					freePage.Next = current;
			  } while ( !CompareAndSetFreelistHead( current, freePage ) );
		 }

		 internal virtual void ClearEvictorException()
		 {
			  if ( _evictorException != null )
			  {
					_evictorException = null;
			  }
		 }

		 public override string ToString()
		 {
			  int availablePages = TryGetNumberOfAvailablePages( _keepFree );
			  return format( "%s[pageCacheId:%d, pageSize:%d, pages:%d, availablePages:%s]", this.GetType().Name, _pageCacheId, _cachePageSize, Pages.PageCount, availablePages != UNKNOWN_AVAILABLE_PAGES ? availablePages.ToString() : "N/A" );
		 }

		 internal virtual void Vacuum( SwapperSet swappers )
		 {
			  if ( FreelistHead is AtomicInteger && swappers.CountAvailableIds() > 200 )
			  {
					return; // We probably still have plenty of free pages left. Don't bother vacuuming just yet.
			  }
			  swappers.Vacuum(swapperIds =>
			  {
				int pageCount = Pages.PageCount;
				try
				{
					using ( EvictionRunEvent evictions = _pageCacheTracer.beginPageEvictions( 0 ) )
					{
						 for ( int i = 0; i < pageCount; i++ )
						 {
							  long pageRef = Pages.deref( i );
							  while ( swapperIds.contains( Pages.getSwapperId( pageRef ) ) )
							  {
									if ( Pages.tryEvict( pageRef, evictions ) )
									{
										 AddFreePageToFreelist( pageRef );
										 break;
									}
							  }
						 }
					}
				}
				catch ( IOException e )
				{
					 throw new UncheckedIOException( e );
				}
			  });
		 }
	}

}