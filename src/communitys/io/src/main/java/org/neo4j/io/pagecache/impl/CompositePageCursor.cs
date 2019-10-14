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
namespace Neo4Net.Io.pagecache.impl
{


	/// <summary>
	/// A CompositePageCursor is a seamless view over parts of two other PageCursors. </summary>
	/// <seealso cref= #compose(PageCursor, int, PageCursor, int) </seealso>
	public class CompositePageCursor : PageCursor
	{
		 private readonly PageCursor _first;
		 private readonly int _firstBaseOffset;
		 private readonly int _firstLength;
		 private readonly PageCursor _second;
		 private readonly int _secondBaseOffset;
		 private readonly int _secondLength;
		 private int _offset;
		 private PageCursor _byteAccessCursor;
		 private bool _outOfBounds;

		 // Constructed with static factory methods.
		 private CompositePageCursor( PageCursor first, int firstBaseOffset, int firstLength, PageCursor second, int secondBaseOffset, int secondLength )
		 {
			  this._first = first;
			  this._firstBaseOffset = firstBaseOffset;
			  this._firstLength = firstLength;
			  this._second = second;
			  this._secondBaseOffset = secondBaseOffset;
			  this._secondLength = secondLength;
			  _byteAccessCursor = new DelegatingPageCursorAnonymousInnerClass( this );
		 }

		 private class DelegatingPageCursorAnonymousInnerClass : DelegatingPageCursor
		 {
			 private readonly CompositePageCursor _outerInstance;

			 public DelegatingPageCursorAnonymousInnerClass( CompositePageCursor outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 private int outerInstance.offset;
			 public override int Int
			 {
				 get
				 {
					  int a = _outerInstance.getByte( _outerInstance.offset ) & 0xFF;
					  int b = _outerInstance.getByte( _outerInstance.offset + 1 ) & 0xFF;
					  int c = _outerInstance.getByte( _outerInstance.offset + 2 ) & 0xFF;
					  int d = _outerInstance.getByte( _outerInstance.offset + 3 ) & 0xFF;
					  int v = ( a << 24 ) | ( b << 16 ) | ( c << 8 ) | d;
					  return v;
				 }
			 }

			 public override int getInt( int offset )
			 {
				  this.offset = offset;
				  return outerInstance.Int;
			 }

			 public override short Short
			 {
				 get
				 {
					  int a = _outerInstance.getByte( _outerInstance.offset ) & 0xFF;
					  int b = _outerInstance.getByte( _outerInstance.offset + 1 ) & 0xFF;
					  int v = ( a << 8 ) | b;
					  return ( short ) v;
				 }
			 }

			 public override short getShort( int offset )
			 {
				  this.offset = offset;
				  return outerInstance.Short;
			 }

			 public override long Long
			 {
				 get
				 {
					  long a = _outerInstance.getByte( _outerInstance.offset ) & 0xFF;
					  long b = _outerInstance.getByte( _outerInstance.offset + 1 ) & 0xFF;
					  long c = _outerInstance.getByte( _outerInstance.offset + 2 ) & 0xFF;
					  long d = _outerInstance.getByte( _outerInstance.offset + 3 ) & 0xFF;
					  long e = _outerInstance.getByte( _outerInstance.offset + 4 ) & 0xFF;
					  long f = _outerInstance.getByte( _outerInstance.offset + 5 ) & 0xFF;
					  long g = _outerInstance.getByte( _outerInstance.offset + 6 ) & 0xFF;
					  long h = _outerInstance.getByte( _outerInstance.offset + 7 ) & 0xFF;
					  long v = ( a << 56 ) | ( b << 48 ) | ( c << 40 ) | ( d << 32 ) | ( e << 24 ) | ( f << 16 ) | ( g << 8 ) | h;
					  return v;
				 }
			 }

			 public override long getLong( int offset )
			 {
				  this.offset = offset;
				  return outerInstance.Long;
			 }

			 public override void getBytes( sbyte[] data )
			 {
				  for ( int i = 0; i < data.Length; i++ )
				  {
						data[i] = _outerInstance.getByte( _outerInstance.offset + i );
				  }
			 }

			 public override void putInt( int value )
			 {
				  _outerInstance.putByte( _outerInstance.offset, ( sbyte )( value >> 24 ) );
				  _outerInstance.putByte( _outerInstance.offset + 1, unchecked( ( sbyte )( ( value >> 16 ) & 0xFF ) ) );
				  _outerInstance.putByte( _outerInstance.offset + 2, unchecked( ( sbyte )( ( value >> 8 ) & 0xFF ) ) );
				  _outerInstance.putByte( _outerInstance.offset + 3, unchecked( ( sbyte )( value & 0xFF ) ) );
			 }

			 public override void putInt( int offset, int value )
			 {
				  this.offset = offset;
				  outerInstance.PutInt( value );
			 }

			 public override void putShort( short value )
			 {
				  _outerInstance.putByte( _outerInstance.offset, ( sbyte )( value >> 8 ) );
				  _outerInstance.putByte( _outerInstance.offset + 1, unchecked( ( sbyte )( value & 0xFF ) ) );
			 }

			 public override void putShort( int offset, short value )
			 {
				  this.offset = offset;
				  outerInstance.PutShort( value );
			 }

			 public override void putLong( long value )
			 {
				  _outerInstance.putByte( _outerInstance.offset, ( sbyte )( value >> 56 ) );
				  _outerInstance.putByte( _outerInstance.offset + 1, unchecked( ( sbyte )( ( value >> 48 ) & 0xFF ) ) );
				  _outerInstance.putByte( _outerInstance.offset + 2, unchecked( ( sbyte )( ( value >> 40 ) & 0xFF ) ) );
				  _outerInstance.putByte( _outerInstance.offset + 3, unchecked( ( sbyte )( ( value >> 32 ) & 0xFF ) ) );
				  _outerInstance.putByte( _outerInstance.offset + 4, unchecked( ( sbyte )( ( value >> 24 ) & 0xFF ) ) );
				  _outerInstance.putByte( _outerInstance.offset + 5, unchecked( ( sbyte )( ( value >> 16 ) & 0xFF ) ) );
				  _outerInstance.putByte( _outerInstance.offset + 6, unchecked( ( sbyte )( ( value >> 8 ) & 0xFF ) ) );
				  _outerInstance.putByte( _outerInstance.offset + 7, unchecked( ( sbyte )( value & 0xFF ) ) );
			 }

			 public override void putLong( int offset, long value )
			 {
				  this.offset = offset;
				  outerInstance.PutLong( value );
			 }

			 public override void putBytes( sbyte[] data )
			 {
				  for ( int i = 0; i < data.Length; i++ )
				  {
						_outerInstance.putByte( _outerInstance.offset + i, data[i] );
				  }
			 }

			 public override int Offset
			 {
				 set
				 {
					  this.offset = value;
				 }
			 }
		 }

		 private PageCursor Cursor( int width )
		 {
			  return Cursor( _offset, width );
		 }

		 private PageCursor Cursor( int offset, int width )
		 {
			  _outOfBounds |= offset + width > _firstLength + _secondLength;
			  if ( offset < _firstLength )
			  {
					return offset + width <= _firstLength ? _first : ByteCursor( offset );
			  }
			  return _second;

		 }

		 private PageCursor ByteCursor( int offset )
		 {
			  _byteAccessCursor.Offset = offset;
			  return _byteAccessCursor;
		 }

		 private int Relative( int offset )
		 {
			  return offset < _firstLength ? _firstBaseOffset + offset : _secondBaseOffset + ( offset - _firstLength );
		 }

		 public override sbyte Byte
		 {
			 get
			 {
				  PageCursor cursor = cursor( Byte.BYTES );
				  sbyte b = cursor.Byte;
				  _offset++;
				  return b;
			 }
		 }

		 public override sbyte getByte( int offset )
		 {
			  return Cursor( offset, Byte.BYTES ).getByte( Relative( offset ) );
		 }

		 public override void PutByte( sbyte value )
		 {
			  PageCursor cursor = cursor( Byte.BYTES );
			  cursor.PutByte( value );
			  _offset++;
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  Cursor( offset, Byte.BYTES ).putByte( Relative( offset ), value );
		 }

		 public override long Long
		 {
			 get
			 {
				  long l = Cursor( Long.BYTES ).Long;
				  _offset += Long.BYTES;
				  return l;
			 }
		 }

		 public override long getLong( int offset )
		 {
			  return Cursor( offset, Long.BYTES ).getLong( Relative( offset ) );
		 }

		 public override void PutLong( long value )
		 {
			  Cursor( Long.BYTES ).putLong( value );
			  _offset += Long.BYTES;
		 }

		 public override void PutLong( int offset, long value )
		 {
			  Cursor( offset, Long.BYTES ).putLong( Relative( offset ), value );
		 }

		 public override int Int
		 {
			 get
			 {
				  int i = Cursor( Integer.BYTES ).Int;
				  _offset += Integer.BYTES;
				  return i;
			 }
		 }

		 public override int getInt( int offset )
		 {
			  return Cursor( offset, Integer.BYTES ).getInt( Relative( offset ) );
		 }

		 public override void PutInt( int value )
		 {
			  PageCursor cursor = cursor( Integer.BYTES );
			  cursor.PutInt( value );
			  _offset += Integer.BYTES;
		 }

		 public override void PutInt( int offset, int value )
		 {
			  Cursor( offset, Integer.BYTES ).putInt( Relative( offset ), value );
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  Cursor( data.Length ).getBytes( data );
			  _offset += data.Length;
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  throw new System.NotSupportedException( "Composite page cursor does not yet support this operation" );
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  Cursor( data.Length ).putBytes( data );
			  _offset += data.Length;
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  throw new System.NotSupportedException( "Composite page cursor does not yet support this operation" );
		 }

		 public override void PutBytes( int bytes, sbyte value )
		 {
			  throw new System.NotSupportedException( "Composite page cursor does not yet support this operation" );
		 }

		 public override short Short
		 {
			 get
			 {
				  short s = Cursor( Short.BYTES ).Short;
				  _offset += Short.BYTES;
				  return s;
			 }
		 }

		 public override short getShort( int offset )
		 {
			  return Cursor( offset, Short.BYTES ).getShort( Relative( offset ) );
		 }

		 public override void PutShort( short value )
		 {
			  Cursor( Short.BYTES ).putShort( value );
			  _offset += Short.BYTES;
		 }

		 public override void PutShort( int offset, short value )
		 {
			  Cursor( offset, Short.BYTES ).putShort( Relative( offset ), value );
		 }

		 public override int Offset
		 {
			 set
			 {
				  if ( value < _firstLength )
				  {
						_first.Offset = _firstBaseOffset + value;
						_second.Offset = _secondBaseOffset;
				  }
				  else
				  {
						_first.Offset = _firstBaseOffset + _firstLength;
						_second.Offset = _secondBaseOffset + ( value - _firstLength );
				  }
				  this._offset = value;
			 }
			 get
			 {
				  return _offset;
			 }
		 }


		 public override void Mark()
		 {
			  _first.mark();
			  _second.mark();
		 }

		 public override void SetOffsetToMark()
		 {
			  _first.setOffsetToMark();
			  _second.setOffsetToMark();
		 }

		 public override long CurrentPageId
		 {
			 get
			 {
				  return Cursor( 0 ).CurrentPageId;
			 }
		 }

		 public override int CurrentPageSize
		 {
			 get
			 {
				  throw new System.NotSupportedException( "Getting current page size is not supported on compose PageCursor" );
			 }
		 }

		 public override File CurrentFile
		 {
			 get
			 {
				  return null;
			 }
		 }

		 public override void Rewind()
		 {
			  _first.Offset = _firstBaseOffset;
			  _second.Offset = _secondBaseOffset;
			  _offset = 0;
		 }

		 public override bool Next()
		 {
			  throw UnsupportedNext();
		 }

		 private System.NotSupportedException UnsupportedNext()
		 {
			  return new System.NotSupportedException( "Composite cursor does not support next operation. Please operate directly on underlying cursors." );
		 }

		 public override bool Next( long pageId )
		 {
			  throw UnsupportedNext();
		 }

		 public override void Close()
		 {
			  _first.close();
			  _second.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean shouldRetry() throws java.io.IOException
		 public override bool ShouldRetry()
		 {
			  bool needsRetry = _first.shouldRetry() | _second.shouldRetry();
			  if ( needsRetry )
			  {
					Rewind();
					CheckAndClearBoundsFlag();
			  }
			  return needsRetry;
		 }

		 public override int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes )
		 {
			  throw new System.NotSupportedException( "Composite cursor does not support copyTo functionality." );
		 }

		 public override int CopyTo( int sourceOffset, ByteBuffer targetBuffer )
		 {
			  throw new System.NotSupportedException( "Composite cursor does not support copyTo functionality." );
		 }

		 public override void ShiftBytes( int sourceOffset, int length, int shift )
		 {
			  throw new System.NotSupportedException( "Composite cursor does not support shiftBytes functionality... yet." );
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  bool bounds = _outOfBounds | _first.checkAndClearBoundsFlag() | _second.checkAndClearBoundsFlag();
			  _outOfBounds = false;
			  return bounds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws org.neo4j.io.pagecache.CursorException
		 public override void CheckAndClearCursorException()
		 {
			  _first.checkAndClearCursorException();
			  _second.checkAndClearCursorException();
		 }

		 public override void RaiseOutOfBounds()
		 {
			  _outOfBounds = true;
		 }

		 public override string CursorException
		 {
			 set
			 {
				  Cursor( 0 ).CursorException = value;
			 }
		 }

		 public override void ClearCursorException()
		 {
			  _first.clearCursorException();
			  _second.clearCursorException();
		 }

		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  throw new System.NotSupportedException( "Linked cursors are not supported for composite cursors" );
		 }

		 public override void ZapPage()
		 {
			  _first.zapPage();
			  _second.zapPage();
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  return _first.WriteLocked && _second.WriteLocked;
			 }
		 }

		 /// <summary>
		 /// Build a CompositePageCursor that is a view of the first page cursor from its current offset through the given
		 /// length, concatenated with the second cursor from its current offset through the given length. The offsets are
		 /// changed as part of accessing the underlying cursors through the composite cursor. However, the size and position
		 /// of the view does NOT change if the offsets of the underlying cursors are changed after constructing the composite
		 /// cursor.
		 /// <para>
		 /// Not all cursor operations are supported on composite cursors, notably <seealso cref="next()"/> and <seealso cref="next(long)"/> are
		 /// not supported.
		 /// Most things work as you would expect, though. For instance, <seealso cref="shouldRetry()"/> will delegate the check to
		 /// both of the underlying cursors, as will <seealso cref="checkAndClearBoundsFlag()"/>.
		 /// </para>
		 /// <para>
		 /// The composite cursor also has its own bounds flag built in, which will be raised if an access is made outside of
		 /// the composite view.
		 /// 
		 /// <pre>
		 ///     offset      first length            offset     second length
		 ///        │              │                    │              │
		 /// ┌──────▼──────────────▼────────┐   ┌───────▼──────────────▼───────┐
		 /// │         first cursor         │   │        second cursor         │
		 /// └──────▼──────────────▼────────┘   └───────▼──────────────▼───────┘
		 ///        └──────────┐   └──────────┐┌────────┘     ┌────────┘
		 ///                   ▼──────────────▼▼──────────────▼
		 ///                   │       composite cursor       │
		 ///                   └──────────────────────────────┘
		 ///             offset = 0, page size = first length + second length
		 /// </pre>
		 /// </para>
		 /// </summary>
		 /// <param name="first"> The cursor that will form the first part of this composite cursor, from its current offset. </param>
		 /// <param name="firstLength"> The number of bytes from the first cursor that will participate in the composite view. </param>
		 /// <param name="second"> The cursor that will form the second part of this composite cursor, from its current offset. </param>
		 /// <param name="secondLength"> The number of bytes from the second cursor that will participate in the composite view. </param>
		 /// <returns> A cursor that is a composed view of the given parts of the two given cursors. </returns>
		 public static PageCursor Compose( PageCursor first, int firstLength, PageCursor second, int secondLength )
		 {
			  return new CompositePageCursor( first, first.Offset, firstLength, second, second.Offset, secondLength );
		 }
	}

}