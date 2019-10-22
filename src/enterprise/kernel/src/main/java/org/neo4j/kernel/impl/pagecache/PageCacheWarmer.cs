using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.pagecache
{

	using Resource = Neo4Net.GraphDb.Resource;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using FileIsNotMappedException = Neo4Net.Io.pagecache.impl.FileIsNotMappedException;
	using NeoStoreFileListing = Neo4Net.Kernel.impl.transaction.state.NeoStoreFileListing;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_NO_FAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;

	/// <summary>
	/// The page cache warmer profiles the page cache to figure out what data is in memory and what is not, and uses those
	/// profiles to load probably-desirable data into the page cache during startup.
	/// <para>
	/// The profiling data is stored in a "profiles" directory in the same directory the mapped files.
	/// The profile files have the same name as their corresponding mapped file, except they end with a dot-hexadecimal
	/// sequence number, and ".cacheprof".
	/// </para>
	/// <para>
	/// The profiles are collected in the "profiles" directory, so it is easy to get rid of all of them, on the off chance
	/// that something is wrong with them.
	/// </para>
	/// <para>
	/// These cacheprof files are compressed bitmaps where each raised bit indicates that the page identified by the
	/// bit-index was in memory.
	/// </para>
	/// </summary>
	public class PageCacheWarmer : NeoStoreFileListing.StoreFileProvider
	{
		 public const string SUFFIX_CACHEPROF = ".cacheprof";

		 private static readonly int _ioParallelism = Runtime.Runtime.availableProcessors();

		 private readonly FileSystemAbstraction _fs;
		 private readonly PageCache _pageCache;
		 private readonly IJobScheduler _scheduler;
		 private readonly File _databaseDirectory;
		 private readonly ProfileRefCounts _refCounts;
		 private volatile bool _stopped;
		 private ExecutorService _executor;
		 private PageLoaderFactory _pageLoaderFactory;

		 internal PageCacheWarmer( FileSystemAbstraction fs, PageCache pageCache, IJobScheduler scheduler, File databaseDirectory )
		 {
			  this._fs = fs;
			  this._pageCache = pageCache;
			  this._scheduler = scheduler;
			  this._databaseDirectory = databaseDirectory;
			  this._refCounts = new ProfileRefCounts();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized org.Neo4Net.graphdb.Resource addFilesTo(java.util.Collection<org.Neo4Net.storageengine.api.StoreFileMetadata> coll) throws java.io.IOException
		 public override Resource AddFilesTo( ICollection<StoreFileMetadata> coll )
		 {
			 lock ( this )
			 {
				  if ( _stopped )
				  {
						return Neo4Net.GraphDb.Resource_Fields.Empty;
				  }
				  IList<PagedFile> files = _pageCache.listExistingMappings();
				  Profile[] existingProfiles = FindExistingProfiles( files );
				  foreach ( Profile profile in existingProfiles )
				  {
						coll.Add( new StoreFileMetadata( profile.File(), 1, false ) );
				  }
				  _refCounts.incrementRefCounts( existingProfiles );
				  return () => _refCounts.decrementRefCounts(existingProfiles);
			 }
		 }

		 public virtual void Start()
		 {
			 lock ( this )
			 {
				  _stopped = false;
				  _executor = BuildExecutorService( _scheduler );
				  _pageLoaderFactory = new PageLoaderFactory( _executor, _pageCache );
			 }
		 }

		 public virtual void Stop()
		 {
			  _stopped = true;
			  StopWarmer();
		 }

		 /// <summary>
		 /// Stopping warmer process, needs to be synchronised to prevent racing with profiling and heating
		 /// </summary>
		 private void StopWarmer()
		 {
			 lock ( this )
			 {
				  if ( _executor != null )
				  {
						_executor.shutdown();
				  }
			 }
		 }

		 /// <summary>
		 /// Reheat the page cache based on existing profiling data, or do nothing if no profiling data is available.
		 /// </summary>
		 /// <returns> An <seealso cref="OptionalLong"/> of the number of pages loaded in, or <seealso cref="OptionalLong.empty()"/> if the
		 /// reheating was stopped early via <seealso cref="stop()"/>. </returns>
		 /// <exception cref="IOException"> if anything goes wrong while reading the profiled data back in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized java.util.OptionalLong reheat() throws java.io.IOException
		 internal virtual long? Reheat()
		 {
			 lock ( this )
			 {
				  if ( _stopped )
				  {
						return long?.empty();
				  }
				  long pagesLoaded = 0;
				  IList<PagedFile> files = _pageCache.listExistingMappings();
				  Profile[] existingProfiles = FindExistingProfiles( files );
				  foreach ( PagedFile file in files )
				  {
						try
						{
							 pagesLoaded += Reheat( file, existingProfiles );
						}
						catch ( FileIsNotMappedException )
						{
							 // The database is allowed to map and unmap files while we are trying to heat it up.
						}
				  }
				  return long?.of( pagesLoaded );
			 }
		 }

		 /// <summary>
		 /// Profile the in-memory data in the page cache, and write it to "cacheprof" file siblings of the mapped files.
		 /// </summary>
		 /// <returns> An <seealso cref="OptionalLong"/> of the number of pages that were found to be in memory, or
		 /// <seealso cref="OptionalLong.empty()"/> if the profiling was stopped early via <seealso cref="stop()"/>. </returns>
		 /// <exception cref="IOException"> If anything goes wrong while accessing the page cache or writing out the profile data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized java.util.OptionalLong profile() throws java.io.IOException
		 public virtual long? Profile()
		 {
			 lock ( this )
			 {
				  if ( _stopped )
				  {
						return long?.empty();
				  }
				  // Note that we could in principle profile the files in parallel. However, producing a profile is usually so
				  // fast, that it rivals the overhead of starting and stopping threads. Because of this, the complexity of
				  // profiling in parallel is just not worth it.
				  long pagesInMemory = 0;
				  IList<PagedFile> files = _pageCache.listExistingMappings();
				  Profile[] existingProfiles = FindExistingProfiles( files );
				  foreach ( PagedFile file in files )
				  {
						try
						{
							 pagesInMemory += Profile( file, existingProfiles );
						}
						catch ( FileIsNotMappedException )
						{
							 // The database is allowed to map and unmap files while we are profiling the page cache.
						}
						if ( _stopped )
						{
							 _pageCache.reportEvents();
							 return long?.empty();
						}
				  }
				  _pageCache.reportEvents();
				  return long?.of( pagesInMemory );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long reheat(org.Neo4Net.io.pagecache.PagedFile file, Profile[] existingProfiles) throws java.io.IOException
		 private long Reheat( PagedFile file, Profile[] existingProfiles )
		 {
			  Optional<Profile> savedProfile = FilterRelevant( existingProfiles, file ).sorted( System.Collections.IComparer.reverseOrder() ).filter(this.verifyChecksum).findFirst();

			  if ( !savedProfile.Present )
			  {
					return 0;
			  }

			  // The file contents checks out. Let's load it in.
			  long pagesLoaded = 0;
			  using ( Stream input = savedProfile.get().read(_fs), PageLoader loader = _pageLoaderFactory.getLoader(file) )
			  {
					long pageId = 0;
					int b;
					while ( ( b = input.Read() ) != -1 )
					{
						 for ( int i = 0; i < 8; i++ )
						 {
							  if ( _stopped )
							  {
									_pageCache.reportEvents();
									return pagesLoaded;
							  }
							  if ( ( b & 1 ) == 1 )
							  {
									loader.Load( pageId );
									pagesLoaded++;
							  }
							  b >>= 1;
							  pageId++;
						 }
					}
			  }
			  _pageCache.reportEvents();
			  return pagesLoaded;
		 }

		 private bool VerifyChecksum( Profile profile )
		 {
			  // Successfully reading through and closing the compressed file implies verifying the gzip checksum.
			  try
			  {
					  using ( Stream input = profile.Read( _fs ) )
					  {
						int b;
						do
						{
							 b = input.Read();
						} while ( b != -1 );
					  }
			  }
			  catch ( IOException )
			  {
					return false;
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long profile(org.Neo4Net.io.pagecache.PagedFile file, Profile[] existingProfiles) throws java.io.IOException
		 private long Profile( PagedFile file, Profile[] existingProfiles )
		 {
			  long pagesInMemory = 0;
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  Profile nextProfile = FilterRelevant( existingProfiles, file ).max( naturalOrder() ).map(Profile::next).orElse(Profile.First(file.File()));

			  using ( Stream output = nextProfile.Write( _fs ), PageCursor cursor = file.Io( 0, PF_SHARED_READ_LOCK | PF_NO_FAULT ) )
			  {
					int stepper = 0;
					int b = 0;
					while ( cursor.Next() )
					{
						 if ( cursor.CurrentPageId != PageCursor.UNBOUND_PAGE_ID )
						 {
							  pagesInMemory++;
							  b |= 1 << stepper;
						 }
						 stepper++;
						 if ( stepper == 8 )
						 {
							  output.WriteByte( b );
							  b = 0;
							  stepper = 0;
						 }
					}
					output.WriteByte( b );
					output.Flush();
			  }

			  // Delete previous profile files.
			  FilterRelevant( existingProfiles, file ).filter( profile => !_refCounts.contains( profile ) ).forEach( profile => profile.delete( _fs ) );

			  return pagesInMemory;
		 }

		 private static ExecutorService BuildExecutorService( IJobScheduler scheduler )
		 {
			  BlockingQueue<ThreadStart> workQueue = new LinkedBlockingQueue<ThreadStart>( _ioParallelism * 4 );
			  RejectedExecutionHandler rejectionPolicy = new ThreadPoolExecutor.CallerRunsPolicy();
			  ThreadFactory threadFactory = scheduler.ThreadFactory( Group.FILE_IO_HELPER );
			  return new ThreadPoolExecutor( 0, _ioParallelism, 10, TimeUnit.SECONDS, workQueue, threadFactory, rejectionPolicy );
		 }

		 private static Stream<Profile> FilterRelevant( Profile[] profiles, PagedFile pagedFile )
		 {
			  return Stream.of( profiles ).filter( Profile.RelevantTo( pagedFile ) );
		 }

		 private Profile[] FindExistingProfiles( IList<PagedFile> pagedFiles )
		 {
			  Path databasePath = _databaseDirectory.toPath();
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return pagedFiles.Where( pf => pf.file().toPath().StartsWith(databasePath) ).Select(pf => pf.file().ParentFile).Distinct().flatMap(dir => Profile.FindProfilesInDirectory(_fs, dir)).ToArray(Profile[]::new);
		 }
	}

}