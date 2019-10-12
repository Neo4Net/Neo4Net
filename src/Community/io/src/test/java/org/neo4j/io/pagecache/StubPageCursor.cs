using System;
using System.Diagnostics;

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
namespace Neo4Net.Io.pagecache
{

	/// <summary>
	/// Utility for testing code that depends on page cursors.
	/// </summary>
	public class StubPageCursor : PageCursor
	{
		 private readonly long _pageId;
		 private readonly int _pageSize;
		 protected internal ByteBuffer Page;
		 private int _currentOffset;
		 private bool _observedOverflow;
		 private string _cursorErrorMessage;
		 private bool _closed;
		 private bool _needsRetry;
		 protected internal StubPageCursor LinkedCursor;
		 private bool _writeLocked;
		 private int _mark;

		 public StubPageCursor( long initialPageId, int pageSize ) : this( initialPageId, ByteBuffer.allocate( pageSize ) )
		 {
		 }

		 public StubPageCursor( long initialPageId, ByteBuffer buffer )
		 {
			  this._pageId = initialPageId;
			  this._pageSize = buffer.capacity();
			  this.Page = buffer;
			  this._writeLocked = true;
		 }

		 public override long CurrentPageId
		 {
			 get
			 {
				  return _pageId;
			 }
		 }

		 public override int CurrentPageSize
		 {
			 get
			 {
				  return _pageSize;
			 }
		 }

		 public override File CurrentFile
		 {
			 get
			 {
				  return new File( "" );
			 }
		 }

		 public override void Rewind()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool Next()
		 {
			  return true;
		 }

		 public override bool Next( long pageId )
		 {
			  return true;
		 }

		 public override void Close()
		 {
			  _closed = true;
			  if ( LinkedCursor != null )
			  {
					LinkedCursor.close();
					LinkedCursor = null;
			  }
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return _closed;
			 }
		 }

		 public override bool ShouldRetry()
		 {
			  if ( _needsRetry )
			  {
					CheckAndClearBoundsFlag();
			  }
			  return _needsRetry || ( LinkedCursor != null && LinkedCursor.shouldRetry() );
		 }

		 public virtual bool NeedsRetry
		 {
			 set
			 {
				  this._needsRetry = value;
			 }
		 }

		 public override int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes )
		 {
			  return 0;
		 }

		 public override int CopyTo( int sourceOffset, ByteBuffer targetBuffer )
		 {
			  return 0;
		 }

		 public override void ShiftBytes( int sourceOffset, int length, int shift )
		 {
			  throw new System.NotSupportedException( "Stub cursor does not support this method... yet" );
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  bool overflow = _observedOverflow;
			  _observedOverflow = false;
			  return overflow || ( LinkedCursor != null && LinkedCursor.checkAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws CursorException
		 public override void CheckAndClearCursorException()
		 {
			  string message = this._cursorErrorMessage;
			  if ( !string.ReferenceEquals( message, null ) )
			  {
					throw new CursorException( message );
			  }
		 }

		 public override void RaiseOutOfBounds()
		 {
			  _observedOverflow = true;
		 }

		 public override string CursorException
		 {
			 set
			 {
				  this._cursorErrorMessage = value;
			 }
		 }

		 public override void ClearCursorException()
		 {
			  this._cursorErrorMessage = null;
		 }

		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  return LinkedCursor = new StubPageCursor( pageId, _pageSize );
		 }

		 public override sbyte Byte
		 {
			 get
			 {
				  sbyte value = GetByte( _currentOffset );
				  _currentOffset += 1;
				  return value;
			 }
		 }

		 public override sbyte getByte( int offset )
		 {
			  try
			  {
					return Page.get( offset );
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					return HandleOverflow();
			  }
		 }

		 private sbyte HandleOverflow()
		 {
			  _observedOverflow = true;
			  return ( sbyte ) ThreadLocalRandom.current().Next();
		 }

		 public override void PutByte( sbyte value )
		 {
			  PutByte( _currentOffset, value );
			  _currentOffset += 1;
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  try
			  {
					Page.put( offset, value );
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					HandleOverflow();
			  }
		 }

		 public override long Long
		 {
			 get
			 {
				  long value = GetLong( _currentOffset );
				  _currentOffset += 8;
				  return value;
			 }
		 }

		 public override long getLong( int offset )
		 {
			  try
			  {
					return Page.getLong( offset );
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					return HandleOverflow();
			  }
		 }

		 public override void PutLong( long value )
		 {
			  PutLong( _currentOffset, value );
			  _currentOffset += 8;
		 }

		 public override void PutLong( int offset, long value )
		 {
			  try
			  {
					Page.putLong( offset, value );
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					HandleOverflow();
			  }
		 }

		 public override int Int
		 {
			 get
			 {
				  int value = GetInt( _currentOffset );
				  _currentOffset += 4;
				  return value;
			 }
		 }

		 public override int getInt( int offset )
		 {
			  try
			  {
					return Page.getInt( offset );
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					return HandleOverflow();
			  }
		 }

		 public override void PutInt( int value )
		 {
			  PutInt( _currentOffset, value );
			  _currentOffset += 4;
		 }

		 public override void PutInt( int offset, int value )
		 {
			  try
			  {
					Page.putInt( offset, value );
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					HandleOverflow();
			  }
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  GetBytes( data, 0, data.Length );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  try
			  {
					Debug.Assert( arrayOffset == 0, "please implement support for arrayOffset" );
					Page.position( _currentOffset );
					Page.get( data, arrayOffset, length );
					_currentOffset += length;
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					HandleOverflow();
			  }
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  PutBytes( data, 0, data.Length );
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  try
			  {
					Debug.Assert( arrayOffset == 0, "please implement support for arrayOffset" );
					Page.position( _currentOffset );
					Page.put( data, arrayOffset, length );
					_currentOffset += length;
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					HandleOverflow();
			  }
		 }

		 public override void PutBytes( int bytes, sbyte value )
		 {
			  sbyte[] byteArray = new sbyte[bytes];
			  Arrays.fill( byteArray, value );
			  PutBytes( byteArray, 0, bytes );
		 }

		 public override short Short
		 {
			 get
			 {
				  short value = GetShort( _currentOffset );
				  _currentOffset += 2;
				  return value;
			 }
		 }

		 public override short getShort( int offset )
		 {
			  try
			  {
					return Page.getShort( offset );
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					return HandleOverflow();
			  }
		 }

		 public override void PutShort( short value )
		 {
			  PutShort( _currentOffset, value );
			  _currentOffset += 2;
		 }

		 public override void PutShort( int offset, short value )
		 {
			  try
			  {
					Page.putShort( offset, value );
			  }
			  catch ( Exception e ) when ( e is System.IndexOutOfRangeException || e is BufferOverflowException || e is BufferUnderflowException )
			  {
					HandleOverflow();
			  }
		 }

		 public override int Offset
		 {
			 get
			 {
				  return _currentOffset;
			 }
			 set
			 {
				  if ( value < 0 )
				  {
						throw new System.IndexOutOfRangeException();
				  }
				  _currentOffset = value;
			 }
		 }


		 public override void Mark()
		 {
			  this._mark = _currentOffset;
		 }

		 public override void SetOffsetToMark()
		 {
			  this._currentOffset = this._mark;
		 }

		 public override void ZapPage()
		 {
			  for ( int i = 0; i < _pageSize; i++ )
			  {
					PutByte( i, ( sbyte ) 0 );
			  }
		 }

		 public override string ToString()
		 {
			  return "PageCursor{" +
						"currentOffset=" + _currentOffset +
						", page=" + Page +
						'}';
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  return _writeLocked;
			 }
			 set
			 {
				  this._writeLocked = value;
			 }
		 }

	}

}