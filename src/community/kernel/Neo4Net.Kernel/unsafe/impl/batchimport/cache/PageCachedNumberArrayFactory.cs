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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;


	/// <summary>
	/// Factory of page cache backed number arrays. </summary>
	/// <seealso cref= NumberArrayFactory </seealso>
	public class PageCachedNumberArrayFactory : NumberArrayFactory_Adapter
	{
		 private readonly PageCache _pageCache;
		 private readonly File _storeDir;

		 public PageCachedNumberArrayFactory( PageCache pageCache, File storeDir )
		 {
			  Objects.requireNonNull( pageCache );
			  this._pageCache = pageCache;
			  this._storeDir = storeDir;
		 }

		 public override IntArray NewIntArray( long length, int defaultValue, long @base )
		 {
			  try
			  {
					File tempFile = File.createTempFile( "intArray", ".tmp", _storeDir );
					PagedFile pagedFile = _pageCache.map( tempFile, _pageCache.pageSize(), DELETE_ON_CLOSE, CREATE );
					return new PageCacheIntArray( pagedFile, length, defaultValue, @base );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override LongArray NewLongArray( long length, long defaultValue, long @base )
		 {
			  try
			  {
					File tempFile = File.createTempFile( "longArray", ".tmp", _storeDir );
					PagedFile pagedFile = _pageCache.map( tempFile, _pageCache.pageSize(), DELETE_ON_CLOSE, CREATE );
					return new PageCacheLongArray( pagedFile, length, defaultValue, @base );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override ByteArray NewByteArray( long length, sbyte[] defaultValue, long @base )
		 {
			  try
			  {
					File tempFile = File.createTempFile( "byteArray", ".tmp", _storeDir );
					PagedFile pagedFile = _pageCache.map( tempFile, _pageCache.pageSize(), DELETE_ON_CLOSE, CREATE );
					return new PageCacheByteArray( pagedFile, length, defaultValue, @base );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}