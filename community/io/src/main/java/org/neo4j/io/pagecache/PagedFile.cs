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
namespace Org.Neo4j.Io.pagecache
{

	/// <summary>
	/// The representation of a file that has been mapped into the associated page cache.
	/// </summary>
	public interface PagedFile : AutoCloseable
	{
		 /// <summary>
		 /// Pin the pages with a shared lock.
		 /// <para>
		 /// This implies <seealso cref="org.neo4j.io.pagecache.PagedFile.PF_NO_GROW"/>, since
		 /// pages under read locks cannot be safely written to anyway, so there's
		 /// no point in trying to go beyond the end of the file.
		 /// </para>
		 /// <para>
		 /// This cannot be combined with <seealso cref="PF_SHARED_WRITE_LOCK"/>.
		 /// </para>
		 /// </summary>
		 /// <summary>
		 /// Pin the pages with a shared write lock.
		 /// <para>
		 /// This will mark the pages as dirty, and caused them to be flushed, if they are evicted.
		 /// </para>
		 /// <para>
		 /// Note that write locks are <em>not</em> exclusive. You must use other means to coordinate access to the data on
		 /// the pages. The write lock only means that the page will not be concurrently evicted.
		 /// </para>
		 /// <para>
		 /// Note also that write locks exclude eviction. So since we can assume that write locks never make conflicting
		 /// modifications (higher level locks should ensure this), it is safe to perform page writes without a
		 /// <seealso cref="PageCursor.shouldRetry() shouldRetry"/> loop. The {@code shouldRetry} method on write locking cursors
		 /// always returns {@code false}.
		 /// </para>
		 /// <para>
		 /// This cannot be combined with <seealso cref="PF_SHARED_READ_LOCK"/>.
		 /// </para>
		 /// </summary>
		 /// <summary>
		 /// Disallow pinning and navigating to pages outside the range of the
		 /// underlying file.
		 /// </summary>
		 /// <summary>
		 /// Read-ahead hint for sequential forward scanning.
		 /// </summary>
		 /// <summary>
		 /// Do not load in the page if it is not loaded already. The methods <seealso cref="PageCursor.next()"/> and
		 /// <seealso cref="PageCursor.next(long)"/> will always return {@code true} for pages that are within the range of the file,
		 /// but the <seealso cref="PageCursor.getCurrentPageId()"/> will return <seealso cref="PageCursor.UNBOUND_PAGE_ID"/> for pages that are
		 /// not in-memory. The current page id <em>must</em> be checked on every <seealso cref="PageCursor.shouldRetry()"/> loop
		 /// iteration, in case it (for a read cursor) was evicted concurrently with the page access.
		 /// <para>
		 /// <seealso cref="PF_NO_FAULT"/> implies <seealso cref="PF_NO_GROW"/>, since a page fault is necessary to be able to extend a file.
		 /// </para>
		 /// </summary>
		 /// <summary>
		 /// Do not update page access statistics.
		 /// </summary>
		 /// <summary>
		 /// Flush pages more aggressively, after they have been dirtied by a write cursor.
		 /// </summary>

		 /// <summary>
		 /// Initiate an IO interaction with the contents of the paged file.
		 /// <para>
		 /// The basic structure of an interaction looks like this:
		 /// <pre><code>
		 ///     try ( PageCursor cursor = pagedFile.io( startingPageId, intentFlags ) )
		 ///     {
		 ///         if ( cursor.next() )
		 ///         {
		 ///             do
		 ///             {
		 ///                 // perform read or write operations on the page
		 ///             }
		 ///             while ( cursor.shouldRetry() );
		 ///         }
		 ///     }
		 /// </code></pre>
		 /// <seealso cref="org.neo4j.io.pagecache.PageCursor PageCursors"/> are <seealso cref="AutoCloseable"/>, so interacting with them
		 /// using <em>try-with-resources</em> is recommended.
		 /// </para>
		 /// <para>
		 /// The returned PageCursor is initially not bound, so <seealso cref="PageCursor.next() next"/> must be called on it before it
		 /// can be used.
		 /// </para>
		 /// <para>
		 /// The first {@code next} call will advance the cursor to the initial page, as given by the {@code pageId}
		 /// parameter. Until then, the cursor won't be bound to any page, the <seealso cref="PageCursor.getCurrentPageId()"/> method
		 /// will return the <seealso cref="org.neo4j.io.pagecache.PageCursor.UNBOUND_PAGE_ID"/> constant, and attempts at reading from
		 /// or writing to the cursor will throw a <seealso cref="System.NullReferenceException"/>.
		 /// </para>
		 /// <para>
		 /// After the {@code next} call, if it returns {@code true}, the cursor will be bound to a page, and the get and put
		 /// methods will access that page.
		 /// </para>
		 /// <para>
		 /// After a call to <seealso cref="PageCursor.rewind()"/>, the cursor will return to its initial state.
		 /// </para>
		 /// <para>
		 /// The {@code pf_flags} argument expresses the intent of the IO operation. It is a bitmap that combines various
		 /// {@code PF_*} constants. You must always specify your desired locking behaviour, with either
		 /// <seealso cref="org.neo4j.io.pagecache.PagedFile.PF_SHARED_WRITE_LOCK"/> or
		 /// <seealso cref="org.neo4j.io.pagecache.PagedFile.PF_SHARED_READ_LOCK"/>.
		 /// </para>
		 /// <para>
		 /// The two locking modes cannot be combined, but other intents can be combined with them. For instance, if you want
		 /// to write to a page, but also make sure that you don't write beyond the end of the file, then you can express
		 /// your intent with {@code PF_SHARED_WRITE_LOCK | PF_NO_GROW} – note how the flags are combined with a bitwise-OR
		 /// operator.
		 /// Arithmetic addition can also be used, but might not make it as clear that we are dealing with a bit-set.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="pageId"> The initial file-page-id, that the cursor will be bound to
		 /// after the first call to <code>next</code>. </param>
		 /// <param name="pf_flags"> A bitmap of <code>PF_*</code> constants composed with
		 /// the bitwise-OR operator, that expresses the desired
		 /// locking behaviour, and other hints. </param>
		 /// <returns> A PageCursor in its initial unbound state.
		 /// Never <code>null</code>. </returns>
		 /// <exception cref="IOException"> if there was an error accessing the underlying file. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageCursor io(long pageId, int pf_flags) throws java.io.IOException;
		 PageCursor Io( long pageId, int pfFlags );

		 /// <summary>
		 /// Get the size of the file-pages, in bytes.
		 /// </summary>
		 int PageSize();

		 /// <summary>
		 /// Size of file, in bytes.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long fileSize() throws java.io.IOException;
		 long FileSize();

		 /// <summary>
		 /// Get the filename that is mapped by this {@code PagedFile}.
		 /// </summary>
		 File File();

		 /// <summary>
		 /// Flush all dirty pages into the file channel, and force the file channel to disk.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flushAndForce() throws java.io.IOException;
		 void FlushAndForce();

		 /// <summary>
		 /// Flush all dirty pages into the file channel, and force the file channel to disk, but limit the rate of IO as
		 /// advised by the given IOPSLimiter.
		 /// </summary>
		 /// <param name="limiter"> The <seealso cref="IOLimiter"/> that determines if pauses or sleeps should be injected into the flushing
		 /// process to keep the IO rate down. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flushAndForce(IOLimiter limiter) throws java.io.IOException;
		 void FlushAndForce( IOLimiter limiter );

		 /// <summary>
		 /// Get the file-page-id of the last page in the file.
		 /// <para>
		 /// This will return <em>a negative number</em> (not necessarily -1) if the file is completely empty.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IllegalStateException"> if this file has been unmapped </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getLastPageId() throws java.io.IOException;
		 long LastPageId { get; }

		 /// <summary>
		 /// Release a handle to a paged file.
		 /// <para>
		 /// If this is the last handle to the file, it will be flushed and closed.
		 /// </para>
		 /// <para>
		 /// Note that this operation assumes that there are no write page cursors open on the paged file. If there are, then
		 /// their writes may be lost, as they might miss the last flush that can happen on their data.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IOException"> instead of the Exception superclass as defined in AutoCloseable, if . </exception>
		 /// <seealso cref= AutoCloseable#close() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws java.io.IOException;
		 void Close();

	}

	public static class PagedFile_Fields
	{
		 public const int PF_SHARED_READ_LOCK = 1;
		 public static readonly int PfSharedWriteLock = 1 << 1;
		 public static readonly int PfNoGrow = 1 << 2;
		 public static readonly int PfReadAhead = 1 << 3;
		 public static readonly int PfNoFault = 1 << 4;
		 public static readonly int PfTransient = 1 << 5;
		 public static readonly int PfEagerFlush = 1 << 6;
	}

}