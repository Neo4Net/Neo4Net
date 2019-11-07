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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using CursorException = Neo4Net.Io.pagecache.CursorException;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;

	internal class ReadableChannelPageCursor : PageCursor
	{
		 private readonly ReadableChannel _channel;
		 private CursorException _cursorException;

		 internal ReadableChannelPageCursor( ReadableChannel channel )
		 {
			  this._channel = channel;
		 }

		 public override sbyte Byte
		 {
			 get
			 {
				  try
				  {
						return _channel.get();
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }
		 }

		 public override sbyte getByte( int offset )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutByte( sbyte value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long Long
		 {
			 get
			 {
				  try
				  {
						return _channel.Long;
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }
		 }

		 public override long getLong( int offset )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutLong( long value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutLong( int offset, long value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int Int
		 {
			 get
			 {
				  try
				  {
						return _channel.Int;
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }
		 }

		 public override int getInt( int offset )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutInt( int value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutInt( int offset, int value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  GetBytes( data, 0, data.Length );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  if ( arrayOffset != 0 )
			  {
					throw new System.NotSupportedException();
			  }

			  try
			  {
					_channel.get( data, length );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutBytes( int bytes, sbyte value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override short Short
		 {
			 get
			 {
				  try
				  {
						return _channel.Short;
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }
		 }

		 public override short getShort( int offset )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutShort( short value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void PutShort( int offset, short value )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int Offset
		 {
			 set
			 {
				  throw new System.NotSupportedException();
			 }
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }


		 public override void Mark()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void SetOffsetToMark()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long CurrentPageId
		 {
			 get
			 {
				  return 0;
			 }
		 }

		 public override int CurrentPageSize
		 {
			 get
			 {
				  return 0;
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
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(long pageId) throws java.io.IOException
		 public override bool Next( long pageId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Close()
		 {
			  try
			  {
					_channel.Dispose();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
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
			  throw new System.NotSupportedException();
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws Neo4Net.io.pagecache.CursorException
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
						 ClearCursorException();
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
				  this._cursorException = new CursorException( value );
			 }
		 }

		 public override void ClearCursorException()
		 {
			  _cursorException = null;
		 }

		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void ZapPage()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  return false;
			 }
		 }
	}

}