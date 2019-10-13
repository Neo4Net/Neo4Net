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
namespace Neo4Net.Io.pagecache
{

	using Exceptions = Neo4Net.Helpers.Exceptions;

	/// <summary>
	/// Wraps a byte array and present it as a PageCursor.
	/// <para>
	/// All the accessor methods (getXXX, putXXX) are implemented and delegates calls to its internal <seealso cref="ByteBuffer"/>.
	/// <seealso cref="setOffset(int)"/>, <seealso cref="getOffset()"/> and <seealso cref="rewind()"/> positions the internal <seealso cref="ByteBuffer"/>.
	/// <seealso cref="shouldRetry()"/> always returns {@code false}.
	/// </para>
	/// </summary>
	public class ByteArrayPageCursor : PageCursor
	{
		 private readonly ByteBuffer _buffer;
		 private CursorException _cursorException;

		 public static PageCursor Wrap( sbyte[] array, int offset, int length )
		 {
			  return new ByteArrayPageCursor( array, offset, length );
		 }

		 public static PageCursor Wrap( sbyte[] array )
		 {
			  return Wrap( array, 0, array.Length );
		 }

		 public static PageCursor Wrap( int length )
		 {
			  return wrap( new sbyte[length] );
		 }

		 public ByteArrayPageCursor( sbyte[] array ) : this( array, 0, array.Length )
		 {
		 }

		 public ByteArrayPageCursor( sbyte[] array, int offset, int length ) : this( ByteBuffer.wrap( array, offset, length ) )
		 {
		 }

		 public ByteArrayPageCursor( ByteBuffer buffer )
		 {
			  this._buffer = buffer;
		 }

		 public override sbyte Byte
		 {
			 get
			 {
				  return _buffer.get();
			 }
		 }

		 public override sbyte getByte( int offset )
		 {
			  return _buffer.get( offset );
		 }

		 public override void PutByte( sbyte value )
		 {
			  _buffer.put( value );
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  _buffer.put( offset, value );
		 }

		 public override long Long
		 {
			 get
			 {
				  return _buffer.Long;
			 }
		 }

		 public override long getLong( int offset )
		 {
			  return _buffer.getLong( offset );
		 }

		 public override void PutLong( long value )
		 {
			  _buffer.putLong( value );
		 }

		 public override void PutLong( int offset, long value )
		 {
			  _buffer.putLong( offset, value );
		 }

		 public override int Int
		 {
			 get
			 {
				  return _buffer.Int;
			 }
		 }

		 public override int getInt( int offset )
		 {
			  return _buffer.getInt( offset );
		 }

		 public override void PutInt( int value )
		 {
			  _buffer.putInt( value );
		 }

		 public override void PutInt( int offset, int value )
		 {
			  _buffer.putInt( offset, value );
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  _buffer.get( data );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  _buffer.get( data, arrayOffset, length );
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  _buffer.put( data );
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  _buffer.put( data, arrayOffset, length );
		 }

		 public override void PutBytes( int bytes, sbyte value )
		 {
			  sbyte[] byteArray = new sbyte[bytes];
			  Arrays.fill( byteArray, value );
			  _buffer.put( byteArray );
		 }

		 public override short Short
		 {
			 get
			 {
				  return _buffer.Short;
			 }
		 }

		 public override short getShort( int offset )
		 {
			  return _buffer.getShort( offset );
		 }

		 public override void PutShort( short value )
		 {
			  _buffer.putShort( value );
		 }

		 public override void PutShort( int offset, short value )
		 {
			  _buffer.putShort( offset, value );
		 }

		 public override int Offset
		 {
			 set
			 {
				  _buffer.position( value );
			 }
			 get
			 {
				  return _buffer.position();
			 }
		 }


		 public override void Mark()
		 {
			  _buffer.mark();
		 }

		 public override void SetOffsetToMark()
		 {
			  _buffer.reset();
		 }

		 public override long CurrentPageId
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override int CurrentPageSize
		 {
			 get
			 {
				  return _buffer.capacity();
			 }
		 }

		 public override File CurrentFile
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override void Rewind()
		 {
			  Offset = 0;
		 }

		 public override bool Next()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool Next( long pageId )
		 {
			  return pageId == 0;
		 }

		 public override void Close()
		 { // Nothing to close
		 }

		 public override bool ShouldRetry()
		 {
			  return false;
		 }

		 public override int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int CopyTo( int sourceOffset, ByteBuffer targetBuffer )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void ShiftBytes( int sourceOffset, int length, int shift )
		 {
			  int currentOffset = Offset;
			  Offset = sourceOffset;
			  sbyte[] bytes = new sbyte[length];
			  GetBytes( bytes );
			  Offset = sourceOffset + shift;
			  PutBytes( bytes );
			  Offset = currentOffset;
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws CursorException
		 public override void CheckAndClearCursorException()
		 {
			  if ( _cursorException != null )
			  {
					try
					{
						 throw _cursorException;
					}
					finally
					{
						 _cursorException = null;
					}
			  }
		 }

		 public override void RaiseOutOfBounds()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override string CursorException
		 {
			 set
			 {
				  _cursorException = Exceptions.chain( _cursorException, new CursorException( value ) );
			 }
		 }

		 public override void ClearCursorException()
		 { // Don't check
		 }

		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void ZapPage()
		 {
			  Arrays.fill( _buffer.array(), (sbyte) 0 );
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  // Because we allow writes; they can't possibly conflict because this class is meant to be used by only one
				  // thread at a time anyway.
				  return true;
			 }
		 }
	}

}