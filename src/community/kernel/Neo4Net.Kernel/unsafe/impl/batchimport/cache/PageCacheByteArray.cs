using System.Diagnostics;

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
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_NO_GROW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;

	public class PageCacheByteArray : PageCacheNumberArray<ByteArray>, ByteArray
	{
		 private readonly sbyte[] _defaultValue;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageCacheByteArray(Neo4Net.io.pagecache.PagedFile pagedFile, long length, byte[] defaultValue, long super) throws java.io.IOException
		 internal PageCacheByteArray( PagedFile pagedFile, long length, sbyte[] defaultValue, long @base ) : base( pagedFile, defaultValue.Length, length, @base )
		 {
			  // Default value is handled locally in this class, in contrast to its siblings, which lets the superclass
			  // handle it.
			  this._defaultValue = defaultValue;
			  DefaultValue = -1;
		 }

		 protected internal override void FillPageWithDefaultValue( PageCursor writeCursor, long ignoredDefaultValue, int pageSize )
		 {
			  for ( int i = 0; i < EntriesPerPage; i++ )
			  {
					writeCursor.PutBytes( this._defaultValue );
			  }
		 }

		 public override void Swap( long fromIndex, long toIndex )
		 {
			  long fromPageId = PageId( fromIndex );
			  int fromOffset = Offset( fromIndex );
			  long toPageId = PageId( toIndex );
			  int toOffset = Offset( toIndex );
			  try
			  {
					  using ( PageCursor fromCursor = PagedFile.io( fromPageId, PF_SHARED_WRITE_LOCK ), PageCursor toCursor = PagedFile.io( toPageId, PF_SHARED_WRITE_LOCK ) )
					  {
						fromCursor.Next();
						toCursor.Next();
						for ( int i = 0; i < EntrySize; i++, fromOffset++, toOffset++ )
						{
							 sbyte intermediary = fromCursor.GetByte( fromOffset );
							 fromCursor.PutByte( fromOffset, toCursor.GetByte( toOffset ) );
							 toCursor.PutByte( toOffset, intermediary );
						}
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Get( long index, sbyte[] into )
		 {
			  long pageId = pageId( index );
			  int offset = offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						cursor.Next();
						do
						{
							 for ( int i = 0; i < into.Length; i++ )
							 {
								  into[i] = cursor.GetByte( offset + i );
							 }
						} while ( cursor.ShouldRetry() );
						CheckBounds( cursor );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override sbyte GetByte( long index, int offset )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						cursor.Next();
						sbyte result;
						do
						{
							 result = cursor.GetByte( offset );
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

		 public override short GetShort( long index, int offset )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						cursor.Next();
						short result;
						do
						{
							 result = cursor.GetShort( offset );
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

		 public override int GetInt( long index, int offset )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
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

		 public override long Get5ByteLong( long index, int offset )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						cursor.Next();
						long result;
						do
						{
							 long low4b = cursor.GetInt( offset ) & 0xFFFFFFFFL;
							 long high1b = cursor.GetByte( offset + Integer.BYTES ) & 0xFF;
							 result = low4b | ( high1b << ( sizeof( int ) * 8 ) );
						} while ( cursor.ShouldRetry() );
						CheckBounds( cursor );
						return result == 0xFFFFFFFFFFL ? -1 : result;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override long Get6ByteLong( long index, int offset )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						cursor.Next();
						long result;
						do
						{
							 long low4b = cursor.GetInt( offset ) & 0xFFFFFFFFL;
							 long high2b = cursor.GetShort( offset + Integer.BYTES ) & 0xFFFF;
							 result = low4b | ( high2b << ( sizeof( int ) * 8 ) );
						} while ( cursor.ShouldRetry() );
						CheckBounds( cursor );
						return result == 0xFFFFFFFFFFFFL ? -1 : result;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override long GetLong( long index, int offset )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
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

		 public override void Set( long index, sbyte[] value )
		 {
			  Debug.Assert( value.Length == EntrySize );
			  long pageId = pageId( index );
			  int offset = offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					  {
						cursor.Next();
						for ( int i = 0; i < value.Length; i++ )
						{
							 cursor.PutByte( offset + i, value[i] );
						}
						CheckBounds( cursor );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void SetByte( long index, int offset, sbyte value )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					  {
						cursor.Next();
						cursor.PutByte( offset, value );
						CheckBounds( cursor );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void SetShort( long index, int offset, short value )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					  {
						cursor.Next();
						cursor.PutShort( offset, value );
						CheckBounds( cursor );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void SetInt( long index, int offset, int value )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
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

		 public override void Set5ByteLong( long index, int offset, long value )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					  {
						cursor.Next();
						cursor.PutInt( offset, ( int ) value );
						cursor.PutByte( offset + Integer.BYTES, ( sbyte )( ( long )( ( ulong )value >> ( sizeof( int ) * 8 ) ) ) );
						CheckBounds( cursor );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Set6ByteLong( long index, int offset, long value )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					  {
						cursor.Next();
						cursor.PutInt( offset, ( int ) value );
						cursor.PutShort( offset + Integer.BYTES, ( short )( ( long )( ( ulong )value >> ( sizeof( int ) * 8 ) ) ) );
						CheckBounds( cursor );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void SetLong( long index, int offset, long value )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
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

		 public override int Get3ByteInt( long index, int offset )
		 {

			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						cursor.Next();
						int result;
						do
						{
							 int lowWord = cursor.GetShort( offset ) & 0xFFFF;
							 int highByte = cursor.GetByte( offset + Short.BYTES ) & 0xFF;
							 result = lowWord | ( highByte << ( sizeof( short ) * 8 ) );
						} while ( cursor.ShouldRetry() );
						CheckBounds( cursor );
						return result == 0xFFFFFF ? -1 : result;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Set3ByteInt( long index, int offset, int value )
		 {
			  long pageId = pageId( index );
			  offset += offset( index );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					  {
						cursor.Next();
						cursor.PutShort( offset, ( short ) value );
						cursor.PutByte( offset + Short.BYTES, ( sbyte )( ( int )( ( uint )value >> ( sizeof( short ) * 8 ) ) ) );
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