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
namespace Neo4Net.Io.pagecache.impl
{


	/// <summary>
	/// A <seealso cref="PageCursor"/> implementation that delegates all calls to a given delegate PageCursor.
	/// </summary>
	public class DelegatingPageCursor : PageCursor
	{
		 protected internal readonly PageCursor Delegate;

		 public override sbyte Byte
		 {
			 get
			 {
				  return Delegate.Byte;
			 }
		 }

		 public override int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes )
		 {
			  return Delegate.copyTo( sourceOffset, targetCursor, targetOffset, lengthInBytes );
		 }

		 public override int CopyTo( int sourceOffset, ByteBuffer targetBuffer )
		 {
			  return Delegate.copyTo( sourceOffset, targetBuffer );
		 }

		 public override void ShiftBytes( int sourceOffset, int length, int shift )
		 {
			  Delegate.shiftBytes( sourceOffset, length, shift );
		 }

		 public override void PutInt( int value )
		 {
			  Delegate.putInt( value );
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  Delegate.getBytes( data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  return Delegate.next();
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  Delegate.putBytes( data );
		 }

		 public override short Short
		 {
			 get
			 {
				  return Delegate.Short;
			 }
		 }

		 public override File CurrentFile
		 {
			 get
			 {
				  return Delegate.CurrentFile;
			 }
		 }

		 public override void PutShort( short value )
		 {
			  Delegate.putShort( value );
		 }

		 public override short getShort( int offset )
		 {
			  return Delegate.getShort( offset );
		 }

		 public override int CurrentPageSize
		 {
			 get
			 {
				  return Delegate.CurrentPageSize;
			 }
		 }

		 public override long Long
		 {
			 get
			 {
				  return Delegate.Long;
			 }
		 }

		 public override void PutLong( long value )
		 {
			  Delegate.putLong( value );
		 }

		 public override int Offset
		 {
			 get
			 {
				  return Delegate.Offset;
			 }
			 set
			 {
				  Delegate.Offset = value;
			 }
		 }

		 public override void Mark()
		 {
			  Delegate.mark();
		 }

		 public override void SetOffsetToMark()
		 {
			  Delegate.setOffsetToMark();
		 }

		 public override void Close()
		 {
			  Delegate.close();
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  Delegate.putByte( offset, value );
		 }

		 public override void PutInt( int offset, int value )
		 {
			  Delegate.putInt( offset, value );
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  Delegate.putBytes( data, arrayOffset, length );
		 }

		 public override void PutBytes( int bytes, sbyte value )
		 {
			  Delegate.putBytes( bytes, value );
		 }

		 public override void Rewind()
		 {
			  Delegate.rewind();
		 }

		 public override void PutByte( sbyte value )
		 {
			  Delegate.putByte( value );
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  return Delegate.checkAndClearBoundsFlag();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws org.neo4j.io.pagecache.CursorException
		 public override void CheckAndClearCursorException()
		 {
			  Delegate.checkAndClearCursorException();
		 }

		 public override void RaiseOutOfBounds()
		 {
			  Delegate.raiseOutOfBounds();
		 }

		 public override string CursorException
		 {
			 set
			 {
				  Delegate.CursorException = value;
			 }
		 }

		 public override void ClearCursorException()
		 {
			  Delegate.clearCursorException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageCursor openLinkedCursor(long pageId) throws java.io.IOException
		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  return Delegate.openLinkedCursor( pageId );
		 }

		 public override long CurrentPageId
		 {
			 get
			 {
				  return Delegate.CurrentPageId;
			 }
		 }

		 public override void PutShort( int offset, short value )
		 {
			  Delegate.putShort( offset, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(long pageId) throws java.io.IOException
		 public override bool Next( long pageId )
		 {
			  return Delegate.next( pageId );
		 }

		 public override void PutLong( int offset, long value )
		 {
			  Delegate.putLong( offset, value );
		 }

		 public override long getLong( int offset )
		 {
			  return Delegate.getLong( offset );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  Delegate.getBytes( data, arrayOffset, length );
		 }

		 public override int GetInt( int offset )
		 {
			  return Delegate.getInt( offset );
		 }


		 public override sbyte getByte( int offset )
		 {
			  return Delegate.getByte( offset );
		 }

		 public override int GetInt()
		 {
			  return Delegate.Int;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean shouldRetry() throws java.io.IOException
		 public override bool ShouldRetry()
		 {
			  return Delegate.shouldRetry();
		 }

		 public override void ZapPage()
		 {
			  Delegate.zapPage();
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  return Delegate.WriteLocked;
			 }
		 }

		 public virtual PageCursor Unwrap()
		 {
			  return Delegate;
		 }

		 public DelegatingPageCursor( PageCursor @delegate )
		 {
			  this.Delegate = @delegate;
		 }
	}

}