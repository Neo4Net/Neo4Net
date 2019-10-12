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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_NO_GROW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;

	public class PageCacheIntArray : PageCacheNumberArray<IntArray>, IntArray
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageCacheIntArray(org.neo4j.io.pagecache.PagedFile pagedFile, long length, long defaultValue, long super) throws java.io.IOException
		 internal PageCacheIntArray( PagedFile pagedFile, long length, long defaultValue, long @base ) : base( pagedFile, Integer.BYTES, length, defaultValue | defaultValue << ( sizeof( int ) * 8 ), @base )
		 {
		 }

		 public override int Get( long index )
		 {
			  long pageId = pageId( index );
			  int offset = offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						cursor.Next();
						int result;
						do
						{
							 result = cursor.GetInt( offset );
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

		 public override void Set( long index, int value )
		 {
			  long pageId = pageId( index );
			  int offset = offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					  {
						cursor.Next();
						cursor.PutInt( offset, value );
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