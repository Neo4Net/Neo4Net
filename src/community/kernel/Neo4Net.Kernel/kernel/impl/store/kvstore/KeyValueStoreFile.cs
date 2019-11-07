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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.AbstractKeyValueStore.MAX_LOOKUP_RETRY_COUNT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.BigEndianByteArrayBuffer.compare;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.BigEndianByteArrayBuffer.newBuffer;

	/// <summary>
	/// Stores Key/Value pairs sorted by the key in unsigned big-endian order.
	/// </summary>
	public class KeyValueStoreFile : System.IDisposable
	{
		 private readonly PagedFile _file;
		 private readonly int _keySize;
		 private readonly int _valueSize;
		 private readonly Headers _headers;
		 private readonly int _headerEntries;
		 /// <summary>
		 /// Includes header, data and trailer entries. </summary>
		 private readonly int _totalEntries;
		 /// <summary>
		 /// The page catalogue is used to find the appropriate (first) page without having to do I/O.
		 /// <para>
		 /// <b>Content:</b> {@code (minKey, maxKey)+}, one entry (at {@code 2 x} <seealso cref="keySize"/>) for each page.
		 /// </para>
		 /// </summary>
		 private readonly sbyte[] _pageCatalogue;

		 internal KeyValueStoreFile( PagedFile file, int keySize, int valueSize, Metadata metadata )
		 {
			  this._file = file;
			  this._keySize = keySize;
			  this._valueSize = valueSize;
			  this._headerEntries = metadata.HeaderEntries();
			  this._totalEntries = metadata.TotalEntries();
			  this._headers = metadata.Headers();
			  this._pageCatalogue = metadata.PageCatalogue();
		 }

		 public virtual Headers Headers()
		 {
			  return _headers;
		 }

		 /// <summary>
		 /// Visit key value pairs that are greater than or equal to the specified key. Visitation will continue as long as
		 /// the visitor <seealso cref="KeyValueVisitor.visit(ReadableBuffer, ReadableBuffer) returns true"/>.
		 /// </summary>
		 /// <returns> {@code true} if an exact match was found, meaning that the first visited key/value pair was a perfect
		 /// match for the specified key. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean scan(SearchKey search, KeyValueVisitor visitor) throws java.io.IOException
		 public virtual bool Scan( SearchKey search, KeyValueVisitor visitor )
		 {
			  BigEndianByteArrayBuffer searchKey = newBuffer( _keySize );
			  BigEndianByteArrayBuffer key = newBuffer( _keySize );
			  BigEndianByteArrayBuffer value = newBuffer( _valueSize );
			  search.SearchKeyConflict( searchKey );
			  int page = FindPage( searchKey, _pageCatalogue );
			  if ( page < 0 || ( page >= _pageCatalogue.Length / ( _keySize * 2 ) ) )
			  {
					return false;
			  }
			  using ( PageCursor cursor = _file.io( page, PF_SHARED_READ_LOCK ) )
			  {
					if ( !cursor.Next() )
					{
						 return false;
					}
					// finds and reads the first key/value pair
					int offset = FindByteOffset( cursor, searchKey, key, value );
					try
					{
						 return Arrays.Equals( searchKey.Buffer, key.Buffer );
					}
					finally
					{
						 VisitKeyValuePairs( _file.pageSize(), cursor, offset, visitor, false, key, value );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public DataProvider dataProvider() throws java.io.IOException
		 public virtual DataProvider DataProvider()
		 {
			  int pageId = _headerEntries * ( _keySize + _valueSize ) / _file.pageSize();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.io.pagecache.PageCursor cursor = file.io(pageId, PF_SHARED_READ_LOCK);
			  PageCursor cursor = _file.io( pageId, PF_SHARED_READ_LOCK );
			  return new DataProviderAnonymousInnerClass( this, cursor );
		 }

		 private class DataProviderAnonymousInnerClass : DataProvider
		 {
			 private readonly KeyValueStoreFile _outerInstance;

			 private PageCursor _cursor;

			 public DataProviderAnonymousInnerClass( KeyValueStoreFile outerInstance, PageCursor cursor )
			 {
				 this.outerInstance = outerInstance;
				 this._cursor = cursor;
				 offset = outerInstance.headerEntries * ( outerInstance.keySize + outerInstance.valueSize );
				 done = !cursor.Next();
			 }

			 internal int offset;
			 internal bool done;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(WritableBuffer key, WritableBuffer value) throws java.io.IOException
			 public bool visit( WritableBuffer key, WritableBuffer value )
			 {
				  if ( done )
				  {
						return false;
				  }
				  ReadKeyValuePair( _cursor, offset, key, value );
				  if ( key.AllZeroes() )
				  {
						done = true;
						return false;
				  }
				  offset += key.Size() + value.Size();
				  if ( offset >= _outerInstance.file.pageSize() )
				  {
						offset = 0;
						if ( !_cursor.next() )
						{
							 done = true;
						}
				  }
				  return true;
			 }

			 public void close()
			 {
				  _cursor.close();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void scan(KeyValueVisitor visitor) throws java.io.IOException
		 public virtual void Scan( KeyValueVisitor visitor )
		 {
			  ScanAll( _file, _headerEntries * ( _keySize + _valueSize ), visitor, new BigEndianByteArrayBuffer( new sbyte[_keySize] ), new BigEndianByteArrayBuffer( new sbyte[_valueSize] ) );
		 }

		 public virtual int EntryCount()
		 {
			  return _totalEntries - _headerEntries;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _file.close();
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + _file + "]";
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: static <Buffer extends BigEndianByteArrayBuffer> void scanAll(Neo4Net.io.pagecache.PagedFile file, int startOffset, EntryVisitor<? super Buffer> visitor, Buffer key, Buffer value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal static void ScanAll<Buffer, T1>( PagedFile file, int startOffset, EntryVisitor<T1> visitor, Buffer key, Buffer value ) where Buffer : BigEndianByteArrayBuffer
		 {
			  bool visitHeaders = !( visitor is KeyValueVisitor );
			  using ( PageCursor cursor = file.Io( startOffset / file.PageSize(), PF_SHARED_READ_LOCK ) )
			  {
					if ( !cursor.Next() )
					{
						 return;
					}
					ReadKeyValuePair( cursor, startOffset, key, value );
					VisitKeyValuePairs( file.PageSize(), cursor, startOffset, visitor, visitHeaders, key, value );
			  }
		 }

		 /// <summary>
		 /// Expects the first key/value-pair to be read into the buffers already, reads subsequent pairs (if requested). </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private static <Buffer extends BigEndianByteArrayBuffer> void visitKeyValuePairs(int pageSize, Neo4Net.io.pagecache.PageCursor cursor, int offset, EntryVisitor<? super Buffer> visitor, boolean visitHeaders, Buffer key, Buffer value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static void VisitKeyValuePairs<Buffer, T1>( int pageSize, PageCursor cursor, int offset, EntryVisitor<T1> visitor, bool visitHeaders, Buffer key, Buffer value ) where Buffer : BigEndianByteArrayBuffer
		 {
			  while ( Visitable( key, visitHeaders ) && visitor.Visit( key, value ) )
			  {
					offset += key.Size() + value.Size();
					if ( offset >= pageSize )
					{
						 offset = 0;
						 if ( !cursor.Next() )
						 {
							  return;
						 }
					}
					// reads the next key/value pair
					ReadKeyValuePair( cursor, offset, key, value );
			  }
		 }

		 private static bool Visitable( BigEndianByteArrayBuffer key, bool acceptZeroKey )
		 {
			  return acceptZeroKey || !key.AllZeroes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void readKeyValuePair(Neo4Net.io.pagecache.PageCursor cursor, int offset, WritableBuffer key, WritableBuffer value) throws java.io.IOException
		 internal static void ReadKeyValuePair( PageCursor cursor, int offset, WritableBuffer key, WritableBuffer value )
		 {
			  long retriesLeft = MAX_LOOKUP_RETRY_COUNT;
			  do
			  {
					cursor.Offset = offset;
					key.GetFrom( cursor );
					value.GetFrom( cursor );
			  } while ( cursor.ShouldRetry() && (--retriesLeft) > 0 );

			  if ( cursor.CheckAndClearBoundsFlag() )
			  {
					ThrowOutOfBounds( cursor, offset );
			  }

			  if ( retriesLeft == 0 )
			  {
					ThrowFailedRead( cursor, offset );
			  }
		 }

		 private static void ThrowFailedRead( PageCursor cursor, int offset )
		 {
			  ThrowReadError( cursor, offset, "Failed to read after " + MAX_LOOKUP_RETRY_COUNT + " retries" );
		 }

		 private static void ThrowOutOfBounds( PageCursor cursor, int offset )
		 {
			  ThrowReadError( cursor, offset, "Out of page bounds" );
		 }

		 private static void ThrowReadError( PageCursor cursor, int offset, string error )
		 {
			  long pageId = cursor.CurrentPageId;
			  int pageSize = cursor.CurrentPageSize;
			  string file = cursor.CurrentFile.AbsolutePath;
			  throw new UnderlyingStorageException( error + " when reading key-value pair from offset " + offset + " into page " + pageId + " (with a size of " + pageSize + " bytes) of file " + file );
		 }

		 /// <summary>
		 /// Finds the page that would contain the given key from the <seealso cref="pageCatalogue page catalogue"/>.
		 /// </summary>
		 /// <param name="key"> The key to look for. </param>
		 /// <returns> {@code -1} if the key is not contained in any page,
		 /// otherwise the id of the page that would contain the key is returned. </returns>
		 internal static int FindPage( BigEndianByteArrayBuffer key, sbyte[] catalogue )
		 {
			  int max = catalogue.Length / ( key.Size() * 2 ) - 1;
			  int min = 0;
			  for ( int mid; min <= max; )
			  {
					mid = min + ( max - min ) / 2;
					// look at the low mark for the page
					int cmp = compare( key.Buffer, catalogue, mid * key.Size() * 2 );
					if ( cmp == 0 )
					{
						 // this page starts with the key
						 // the previous page might also contain mid the key
						 max = mid;
					}
					if ( cmp > 0 )
					{
						 // look at the high mark for the page
						 cmp = compare( key.Buffer, catalogue, mid * key.Size() * 2 + key.Size() );
						 if ( cmp <= 0 )
						 {
							  return mid; // the key is within the range of this page
						 }
						 else // look at pages after 'mid'
						 {
							  min = mid + 1;
						 }
					}
					else // look at pages before 'mid'
					{
						 max = mid - 1;
					}
			  }
			  return min; // the first page after the value that was smaller than the key (mid + 1, you know...)
		 }

		 /// <param name="cursor"> the cursor for the page to search for the key in. </param>
		 /// <param name="searchKey"> the key to search for. </param>
		 /// <param name="key"> a buffer to write the key into. </param>
		 /// <param name="value"> a buffer to write the value into. </param>
		 /// <returns> the offset (in bytes within the given page) of the first entry with a key that is greater than or equal
		 /// to the given key. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int findByteOffset(Neo4Net.io.pagecache.PageCursor cursor, BigEndianByteArrayBuffer searchKey, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) throws java.io.IOException
		 private int FindByteOffset( PageCursor cursor, BigEndianByteArrayBuffer searchKey, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value )
		 {
			  int entrySize = searchKey.Size() + value.Size();
			  int last = MaxPage( _file.pageSize(), entrySize, _totalEntries );
			  int firstEntry = ( cursor.CurrentPageId == 0 ) ? _headerEntries : 0; // skip header in first page
			  int entryCount = _totalEntries % ( _file.pageSize() / entrySize );
			  // If the last page is full, 'entryCount' will be 0 at this point.
			  if ( cursor.CurrentPageId != last || entryCount == 0 )
			  { // The current page is a full page (either because it has pages after it, or the last page is actually full).
					entryCount = _file.pageSize() / entrySize;
			  }
			  int entryOffset = FindEntryOffset( cursor, searchKey, key, value, firstEntry, entryCount - 1 );
			  return entryOffset * entrySize;
		 }

		 internal static int MaxPage( int pageSize, int entrySize, int totalEntries )
		 {
			  int maxPage = totalEntries / ( pageSize / entrySize );
			  return maxPage * ( pageSize / entrySize ) == totalEntries ? maxPage - 1 : maxPage;
		 }

		 /// <param name="cursor"> the cursor for the page to search for the key in. </param>
		 /// <param name="searchKey"> the key to search for. </param>
		 /// <param name="key"> a buffer to write the key into. </param>
		 /// <param name="value"> a buffer to write the value into. </param>
		 /// <param name="min"> the offset (in number of entries within the page) of the first entry in the page. </param>
		 /// <param name="max"> the offset (in number of entries within the page) of the last entry in the page. </param>
		 /// <returns> the offset (in number of entries within the page) of the first entry with a key that is greater than or
		 /// equal to the given key. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static int findEntryOffset(Neo4Net.io.pagecache.PageCursor cursor, BigEndianByteArrayBuffer searchKey, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value, int min, int max) throws java.io.IOException
		 internal static int FindEntryOffset( PageCursor cursor, BigEndianByteArrayBuffer searchKey, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value, int min, int max )
		 {
			  int entrySize = key.Size() + value.Size();
			  for ( int mid; min <= max; )
			  {
					mid = min + ( max - min ) / 2;
					ReadKeyValuePair( cursor, mid * entrySize, key, value );
					if ( min == max )
					{
						 break; // break here instead of in the loop condition to ensure the right key is read
					}
					int cmp = compare( searchKey.Buffer, key.Buffer, 0 );
					if ( cmp > 0 ) // search key bigger than found key, continue after 'mid'
					{
						 min = mid + 1;
					}
					else // search key smaller (or equal to) than found key, continue before 'mid'
					{
						 max = mid; // don't add, greater than are to be included...
					}
			  }
			  return max;
		 }
	}

}