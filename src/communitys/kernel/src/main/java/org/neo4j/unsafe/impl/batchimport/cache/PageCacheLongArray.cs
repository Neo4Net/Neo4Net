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

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_NO_GROW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;

	public class PageCacheLongArray : PageCacheNumberArray<LongArray>, LongArray
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageCacheLongArray(org.neo4j.io.pagecache.PagedFile pagedFile, long length, long defaultValue, long super) throws java.io.IOException
		 internal PageCacheLongArray( PagedFile pagedFile, long length, long defaultValue, long @base ) : base( pagedFile, Long.BYTES, length, defaultValue, @base )
		 {
		 }

		 public override long Get( long index )
		 {
			  long pageId = pageId( index );
			  int offset = offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						cursor.Next();
						long result;
						do
						{
							 result = cursor.GetLong( offset );
						} while ( cursor.ShouldRetry() );
						CheckBounds( cursor );
						return result;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Set( long index, long value )
		 {
			  long pageId = pageId( index );
			  int offset = offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					  {
						cursor.Next();
						cursor.PutLong( offset, value );
						CheckBounds( cursor );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}