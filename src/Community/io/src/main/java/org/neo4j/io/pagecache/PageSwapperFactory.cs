﻿/*
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
namespace Neo4Net.Io.pagecache
{

	using Configuration = Neo4Net.Graphdb.config.Configuration;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	/// <summary>
	/// Creates PageSwappers for the given files.
	/// <para>
	/// A PageSwapper is responsible for swapping file pages in and out of memory.
	/// </para>
	/// <para>
	/// The PageSwapperFactory presumably knows about what file system to use.
	/// </para>
	/// <para>
	/// To be able to create PageSwapper factory need to be configured first using appropriate configure call.
	/// Note that this API is <em>only</em> intended to be used by a <seealso cref="PageCache"/> implementation.
	/// It should never be used directly by user code.
	/// </para>
	/// </summary>
	public interface PageSwapperFactory
	{
		 /// <summary>
		 /// Open page swapper factory with provided filesystem and config </summary>
		 /// <param name="fs"> file system to use in page swappers </param>
		 /// <param name="config"> custom page swapper configuration </param>
		 void Open( FileSystemAbstraction fs, Configuration config );

		 /// <summary>
		 /// Get the name of this PageSwapperFactory implementation, for configuration purpose.
		 /// </summary>
		 string ImplementationName();

		 /// <summary>
		 /// Get the unit of alignment that the swappers require of the memory buffers. For instance, if page alignment is
		 /// required for doing direct IO, then <seealso cref="org.neo4j.unsafe.impl.internal.dragons.UnsafeUtil.pageSize()"/> can be
		 /// returned.
		 /// </summary>
		 /// <returns> The required buffer alignment byte multiple. </returns>
		 long RequiredBufferAlignment { get; }

		 /// <summary>
		 /// Create a PageSwapper for the given file.
		 /// </summary>
		 /// <param name="file"> The file that the PageSwapper will move file pages in and
		 /// out of. </param>
		 /// <param name="filePageSize"> The size of the pages in the file. Presumably a
		 /// multiple of some record size. </param>
		 /// <param name="onEviction"> The PageSwapper will be told about evictions, and has
		 /// the responsibility of informing the PagedFile via this callback. </param>
		 /// <param name="createIfNotExist"> When true, creates the given file if it does not exist, instead of throwing an
		 /// exception. </param>
		 /// <param name="noChannelStriping"> When true, overrides channel striping behaviour,
		 /// setting it to a single channel per mapped file. </param>
		 /// <returns> A working PageSwapper instance for the given file. </returns>
		 /// <exception cref="IOException"> If the PageSwapper could not be created, for
		 /// instance if the underlying file could not be opened, or the given file does not exist and createIfNotExist is
		 /// false. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageSwapper createPageSwapper(java.io.File file, int filePageSize, PageEvictionCallback onEviction, boolean createIfNotExist, boolean noChannelStriping) throws java.io.IOException;
		 PageSwapper CreatePageSwapper( File file, int filePageSize, PageEvictionCallback onEviction, bool createIfNotExist, bool noChannelStriping );

		 /// <summary>
		 /// Forces all prior writes made through all non-closed PageSwappers that this factory has created, to all the
		 /// relevant devices, such that the writes are durable when this call returns.
		 /// <para>
		 /// This method has no effect if the <seealso cref="PageSwapper.force()"/> method forces the writes for the individual file.
		 /// The <seealso cref="PageCache.flushAndForce()"/> method will first call <code>force</code> on the PageSwappers for all
		 /// mapped files, then call <code>syncDevice</code> on the PageSwapperFactory. This way, the writes are always made
		 /// durable regardless of which method that does the forcing.
		 /// </para>
		 /// </summary>
		 void SyncDevice();

		 /// <summary>
		 /// Close and release any resources associated with this PageSwapperFactory, that it may have opened or acquired
		 /// during its construction or use.
		 /// <para>
		 /// This method cannot be called before all of the opened <seealso cref="PageSwapper PageSwappers"/> have been closed,
		 /// and it is guaranteed that no new page swappers will be created after this method has been called.
		 /// </para>
		 /// </summary>
		 void Close();
	}

}