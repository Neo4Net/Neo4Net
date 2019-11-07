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
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_NO_GROW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;

	/// <summary>
	/// Abstraction over page cache backed number arrays.
	/// </summary>
	/// <seealso cref= PageCachedNumberArrayFactory </seealso>
	public abstract class PageCacheNumberArray<N> : NumberArray<N> where N : NumberArray<N>
	{
		public abstract void Swap( long fromIndex, long toIndex );
		 protected internal readonly PagedFile PagedFile;
		 protected internal readonly int EntriesPerPage;
		 protected internal readonly int EntrySize;
		 private readonly long _length;
		 private readonly long _defaultValue;
		 private readonly long @base;
		 private bool _closed;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageCacheNumberArray(Neo4Net.io.pagecache.PagedFile pagedFile, int entrySize, long length, long super) throws java.io.IOException
		 internal PageCacheNumberArray( PagedFile pagedFile, int entrySize, long length, long @base ) : this( pagedFile, entrySize, length, 0, @base )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageCacheNumberArray(Neo4Net.io.pagecache.PagedFile pagedFile, int entrySize, long length, long defaultValue, long super) throws java.io.IOException
		 internal PageCacheNumberArray( PagedFile pagedFile, int entrySize, long length, long defaultValue, long @base )
		 {
			  this.PagedFile = pagedFile;
			  this.EntrySize = entrySize;
			  this.EntriesPerPage = pagedFile.PageSize() / entrySize;
			  this._length = length;
			  this._defaultValue = defaultValue;
			  this.@base = @base;

			  using ( PageCursor cursorToSetLength = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					SetLength( cursorToSetLength, length );
			  }

			  if ( defaultValue != 0 )
			  {
					DefaultValue = defaultValue;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setLength(Neo4Net.io.pagecache.PageCursor cursor, long length) throws java.io.IOException
		 private void SetLength( PageCursor cursor, long length )
		 {
			  if ( !cursor.Next( ( length - 1 ) / EntriesPerPage ) )
			  {
					throw new System.InvalidOperationException( string.Format( "Unable to extend the backing file {0} to desired size {1:D}.", PagedFile, length ) );
			  }
		 }

		 protected internal virtual long PageId( long index )
		 {
			  return Rebase( index ) / EntriesPerPage;
		 }

		 protected internal virtual int Offset( long index )
		 {
			  return toIntExact( Rebase( index ) % EntriesPerPage * EntrySize );
		 }

		 private long Rebase( long index )
		 {
			  return index - @base;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void setDefaultValue(long defaultValue) throws java.io.IOException
		 protected internal virtual long DefaultValue
		 {
			 set
			 {
				  using ( PageCursor writeCursor = PagedFile.io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
				  {
						writeCursor.Next();
						int pageSize = PagedFile.pageSize();
						FillPageWithDefaultValue( writeCursor, value, pageSize );
						if ( PageId( _length - 1 ) > 0 )
						{
							 using ( PageCursor cursor = PagedFile.io( 1, PF_NO_GROW | PF_SHARED_WRITE_LOCK ) )
							 {
								  while ( cursor.Next() )
								  {
										writeCursor.CopyTo( 0, cursor, 0, pageSize );
										CheckBounds( writeCursor );
								  }
							 }
						}
				  }
			 }
		 }

		 protected internal virtual void FillPageWithDefaultValue( PageCursor writeCursor, long defaultValue, int pageSize )
		 {
			  int longsInPage = pageSize / Long.BYTES;
			  for ( int i = 0; i < longsInPage; i++ )
			  {
					writeCursor.PutLong( defaultValue );
			  }
			  CheckBounds( writeCursor );
		 }

		 public override long Length()
		 {
			  return _length;
		 }

		 public override void Clear()
		 {
			  try
			  {
					DefaultValue = _defaultValue;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Close()
		 {
			  if ( _closed )
			  {
					return;
			  }
			  try
			  {
					PagedFile.close();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  finally
			  {
					_closed = true;
			  }
		 }

		 public override N At( long index )
		 {
			  return ( N ) this;
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  visitor.OffHeapUsage( Length() * EntrySize );
		 }

		 protected internal virtual void CheckBounds( PageCursor cursor )
		 {
			  if ( cursor.CheckAndClearBoundsFlag() )
			  {
					throw new System.InvalidOperationException( string.Format( "Cursor {0} access out of bounds, page id {1:D}, offset {2:D}", cursor.ToString(), cursor.CurrentPageId, cursor.Offset ) );
			  }
		 }
	}

}