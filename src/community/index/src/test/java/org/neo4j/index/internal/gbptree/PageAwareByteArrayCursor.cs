using System;
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
namespace Neo4Net.Index.Internal.gbptree
{

	using CursorException = Neo4Net.Io.pagecache.CursorException;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.ByteArrayPageCursor.wrap;

	internal class PageAwareByteArrayCursor : PageCursor
	{
		 private readonly int _pageSize;
		 private readonly IList<sbyte[]> _pages;

		 private PageCursor _current;
		 private long _currentPageId = UNBOUND_PAGE_ID;
		 private long _nextPageId;
		 private PageCursor _linkedCursor;
		 private bool _shouldRetry;

		 internal PageAwareByteArrayCursor( int pageSize ) : this( pageSize, 0 )
		 {
		 }

		 private PageAwareByteArrayCursor( int pageSize, long nextPageId ) : this( new List<sbyte[]>(), pageSize, nextPageId )
		 {
		 }

		 private PageAwareByteArrayCursor( IList<sbyte[]> pages, int pageSize, long nextPageId )
		 {
			  this._pages = pages;
			  this._pageSize = pageSize;
			  this._nextPageId = nextPageId;
			  Initialize();
		 }

		 private void Initialize()
		 {
			  _currentPageId = UNBOUND_PAGE_ID;
			  _current = null;
		 }

		 internal virtual PageAwareByteArrayCursor Duplicate()
		 {
			  return new PageAwareByteArrayCursor( _pages, _pageSize, _currentPageId );
		 }

		 internal virtual PageAwareByteArrayCursor Duplicate( long nextPageId )
		 {
			  return new PageAwareByteArrayCursor( _pages, _pageSize, nextPageId );
		 }

		 internal virtual void ForceRetry()
		 {
			  _shouldRetry = true;
		 }

		 public override long CurrentPageId
		 {
			 get
			 {
				  return _currentPageId;
			 }
		 }

		 public override int CurrentPageSize
		 {
			 get
			 {
				  if ( _currentPageId == UNBOUND_PAGE_ID )
				  {
						return UNBOUND_PAGE_SIZE;
				  }
				  else
				  {
						return Page( _currentPageId ).Length;
				  }
			 }
		 }

		 public override bool Next()
		 {
			  _currentPageId = _nextPageId;
			  _nextPageId++;
			  AssertPages();

			  sbyte[] page = page( _currentPageId );
			  _current = wrap( page, 0, page.Length );
			  return true;
		 }

		 public override bool Next( long pageId )
		 {
			  _currentPageId = pageId;
			  AssertPages();

			  sbyte[] page = page( _currentPageId );
			  _current = wrap( page, 0, page.Length );
			  return true;
		 }

		 public override int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes )
		 {
			  if ( sourceOffset < 0 || targetOffset < 0 || lengthInBytes < 0 )
			  {
					throw new System.ArgumentException( format( "sourceOffset=%d, targetOffset=%d, lengthInBytes=%d, currentPageId=%d", sourceOffset, targetOffset, lengthInBytes, _currentPageId ) );
			  }
			  int bytesToCopy = Math.Min( lengthInBytes, Math.Min( _current.CurrentPageSize - sourceOffset, targetCursor.CurrentPageSize - targetOffset ) );

			  for ( int i = 0; i < bytesToCopy; i++ )
			  {
					targetCursor.PutByte( targetOffset + i, GetByte( sourceOffset + i ) );
			  }
			  return bytesToCopy;
		 }

		 public override int CopyTo( int sourceOffset, ByteBuffer buf )
		 {
			  int bytesToCopy = Math.Min( buf.limit() - buf.position(), _pageSize - sourceOffset );
			  for ( int i = 0; i < bytesToCopy; i++ )
			  {
					sbyte b = GetByte( sourceOffset + i );
					buf.put( b );
			  }
			  return bytesToCopy;
		 }

		 public override void ShiftBytes( int sourceOffset, int length, int shift )
		 {
			  _current.shiftBytes( sourceOffset, length, shift );
		 }

		 private void AssertPages()
		 {
			  if ( _currentPageId >= _pages.Count )
			  {
					for ( int i = _pages.Count; i <= _currentPageId; i++ )
					{
						 _pages.Add( new sbyte[_pageSize] );
					}
			  }
		 }

		 private sbyte[] Page( long pageId )
		 {
			  return _pages[( int ) pageId];
		 }

		 /* DELEGATE METHODS */

		 public override File CurrentFile
		 {
			 get
			 {
				  return _current.CurrentFile;
			 }
		 }

		 public override sbyte Byte
		 {
			 get
			 {
				  return _current.Byte;
			 }
		 }

		 public override sbyte getByte( int offset )
		 {
			  return _current.getByte( offset );
		 }

		 public override void PutByte( sbyte value )
		 {
			  _current.putByte( value );
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  _current.putByte( offset, value );
		 }

		 public override long Long
		 {
			 get
			 {
				  return _current.Long;
			 }
		 }

		 public override long getLong( int offset )
		 {
			  return _current.getLong( offset );
		 }

		 public override void PutLong( long value )
		 {
			  _current.putLong( value );
		 }

		 public override void PutLong( int offset, long value )
		 {
			  _current.putLong( offset, value );
		 }

		 public override int Int
		 {
			 get
			 {
				  return _current.Int;
			 }
		 }

		 public override int getInt( int offset )
		 {
			  return _current.getInt( offset );
		 }

		 public override void PutInt( int value )
		 {
			  _current.putInt( value );
		 }

		 public override void PutInt( int offset, int value )
		 {
			  _current.putInt( offset, value );
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  _current.getBytes( data );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  _current.getBytes( data, arrayOffset, length );
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  _current.putBytes( data );
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  _current.putBytes( data, arrayOffset, length );
		 }

		 public override void PutBytes( int bytes, sbyte value )
		 {
			  _current.putBytes( bytes, value );
		 }

		 public override short Short
		 {
			 get
			 {
				  return _current.Short;
			 }
		 }

		 public override short getShort( int offset )
		 {
			  return _current.getShort( offset );
		 }

		 public override void PutShort( short value )
		 {
			  _current.putShort( value );
		 }

		 public override void PutShort( int offset, short value )
		 {
			  _current.putShort( offset, value );
		 }

		 public override int Offset
		 {
			 set
			 {
				  _current.Offset = value;
			 }
			 get
			 {
				  return _current.Offset;
			 }
		 }


		 public override void Mark()
		 {
			  _current.mark();
		 }

		 public override void SetOffsetToMark()
		 {
			  _current.setOffsetToMark();
		 }

		 public override void Rewind()
		 {
			  _current.rewind();
		 }

		 public override void Close()
		 {
			  if ( _linkedCursor != null )
			  {
					_linkedCursor.close();
			  }
			  _current.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean shouldRetry() throws java.io.IOException
		 public override bool ShouldRetry()
		 {
			  if ( _shouldRetry )
			  {
					_shouldRetry = false;

					// To reset shouldRetry for linked cursor as well
					if ( _linkedCursor != null )
					{
						 _linkedCursor.shouldRetry();
					}
					return true;
			  }
			  return _linkedCursor != null && _linkedCursor.shouldRetry() || _current.shouldRetry();
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  bool result = false;
			  if ( _linkedCursor != null )
			  {
					result = _linkedCursor.checkAndClearBoundsFlag();
			  }
			  result |= _current.checkAndClearBoundsFlag();
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws org.neo4j.io.pagecache.CursorException
		 public override void CheckAndClearCursorException()
		 {
			  _current.checkAndClearCursorException();
		 }

		 public override void RaiseOutOfBounds()
		 {
			  _current.raiseOutOfBounds();
		 }

		 public override string CursorException
		 {
			 set
			 {
				  _current.CursorException = value;
			 }
		 }

		 public override void ClearCursorException()
		 {
			  _current.clearCursorException();
		 }

		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  PageCursor toReturn = new PageAwareByteArrayCursor( _pages, _pageSize, pageId );
			  if ( _linkedCursor != null )
			  {
					_linkedCursor.close();
			  }
			  _linkedCursor = toReturn;
			  return toReturn;
		 }

		 public override void ZapPage()
		 {
			  _current.zapPage();
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  return _current == null || _current.WriteLocked;
			 }
		 }
	}

}