using System.Collections.Generic;

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
namespace Neo4Net.Io.pagecache
{

	/// <summary>
	/// A page caching mechanism that allows caching multiple files and accessing their data
	/// in pages via a re-usable cursor.
	/// <para>
	/// This interface does not specify the cache eviction and allocation behavior, it may be
	/// backed by implementations that map entire files into RAM, or implementations with smart
	/// eviction strategies, trying to keep "hot" pages in RAM.
	/// </para>
	/// </summary>
	public interface PageCache : AutoCloseable
	{
		 /// <summary>
		 /// The default <seealso cref="pageSize()"/>.
		 /// </summary>

		 /// <summary>
		 /// Ask for a handle to a paged file, backed by this page cache.
		 /// <para>
		 /// Note that this currently asks for the pageSize to use, which is an artifact or records being
		 /// of varying size in the stores. This should be consolidated to use a standard page size for the
		 /// whole cache, with records aligning on those page boundaries.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="file"> The file to map. </param>
		 /// <param name="pageSize"> The file page size to use for this mapping. If the file is already mapped with a different page
		 /// size, an exception will be thrown. </param>
		 /// <param name="openOptions"> The set of open options to use for mapping this file.
		 /// The <seealso cref="StandardOpenOption.READ"/> and <seealso cref="StandardOpenOption.WRITE"/> options always implicitly specified.
		 /// The <seealso cref="StandardOpenOption.CREATE"/> open option will create the given file if it does not already exist, and
		 /// the <seealso cref="StandardOpenOption.TRUNCATE_EXISTING"/> will truncate any existing file <em>iff</em> it has not already
		 /// been mapped.
		 /// The <seealso cref="StandardOpenOption.DELETE_ON_CLOSE"/> will cause the file to be deleted after the last unmapping.
		 /// All other options are either silently ignored, or will cause an exception to be thrown. </param>
		 /// <exception cref="java.nio.file.NoSuchFileException"> if the given file does not exist, and the
		 /// <seealso cref="StandardOpenOption.CREATE"/> option was not specified. </exception>
		 /// <exception cref="IOException"> if the file could otherwise not be mapped. Causes include the file being locked. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException;
		 PagedFile Map( File file, int pageSize, params OpenOption[] openOptions );

		 /// <summary>
		 /// Ask for an already mapped paged file, backed by this page cache.
		 /// <para>
		 /// If mapping exist, the returned <seealso cref="Optional"/> will report <seealso cref="Optional.isPresent()"/> true and
		 /// <seealso cref="Optional.get()"/> will return the same <seealso cref="PagedFile"/> instance that was initially returned my
		 /// <seealso cref="map(File, int, OpenOption...)"/>.
		 /// If no mapping exist for this file, then returned <seealso cref="Optional"/> will report <seealso cref="Optional.isPresent()"/>
		 /// false.
		 /// </para>
		 /// <para>
		 /// <strong>NOTE:</strong> The calling code is responsible for closing the returned paged file, if any.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="file"> The file to try to get the mapped paged file for. </param>
		 /// <returns> <seealso cref="Optional"/> containing the <seealso cref="PagedFile"/> mapped by this <seealso cref="PageCache"/> for given file, or an
		 /// empty <seealso cref="Optional"/> if no mapping exist. </returns>
		 /// <exception cref="IOException"> if page cache has been closed or page eviction problems occur. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Optional<PagedFile> getExistingMapping(java.io.File file) throws java.io.IOException;
		 Optional<PagedFile> GetExistingMapping( File file );

		 /// <summary>
		 /// List a snapshot of the current file mappings.
		 /// <para>
		 /// The mappings can change as soon as this method returns.
		 /// </para>
		 /// <para>
		 /// <strong>NOTE:</strong> The calling code should <em>not</em> close the returned paged files, unless it does so
		 /// in collaboration with the code that originally mapped the file. Any reference count in the mapping will
		 /// <em>not</em> be incremented by this method, so calling code must be prepared for that the returned
		 /// <seealso cref="PagedFile"/>s can be asynchronously closed elsewhere.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IOException"> if page cache has been closed or page eviction problems occur. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<PagedFile> listExistingMappings() throws java.io.IOException;
		 IList<PagedFile> ListExistingMappings();

		 /// <summary>
		 /// Flush all dirty pages.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flushAndForce() throws java.io.IOException;
		 void FlushAndForce();

		 /// <summary>
		 /// Flush all dirty pages, but limit the rate of IO as advised by the given IOPSLimiter.
		 /// </summary>
		 /// <param name="limiter"> The <seealso cref="IOLimiter"/> that determines if pauses or sleeps should be injected into the flushing
		 /// process to keep the IO rate down. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flushAndForce(IOLimiter limiter) throws java.io.IOException;
		 void FlushAndForce( IOLimiter limiter );

		 /// <summary>
		 /// Close the page cache to prevent any future mapping of files.
		 /// This also releases any internal resources, including the <seealso cref="PageSwapperFactory"/> through its
		 /// <seealso cref="PageSwapperFactory.close() close"/> method.
		 /// </summary>
		 /// <exception cref="IllegalStateException"> if not all files have been unmapped, with <seealso cref="PagedFile.close()"/>, prior to
		 /// closing the page cache. In this case, the page cache <em>WILL NOT</em> be considered to be successfully closed. </exception>
		 /// <exception cref="RuntimeException"> if the <seealso cref="PageSwapperFactory.close()"/> method throws. In this case the page cache
		 /// <em>WILL BE</em> considered to have been closed successfully. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws IllegalStateException;
		 void Close();

		 /// <summary>
		 /// The size in bytes of the pages managed by this cache.
		 /// </summary>
		 int PageSize();

		 /// <summary>
		 /// The max number of cached pages.
		 /// </summary>
		 long MaxCachedPages();

		 /// <summary>
		 /// Report any thread-local events to the global page cache tracer, as if acquiring a thread-specific page cursor
		 /// tracer, and reporting the events collected within it.
		 /// </summary>
		 void ReportEvents();
	}

	public static class PageCache_Fields
	{
		 public const int PAGE_SIZE = 8192;
	}

}