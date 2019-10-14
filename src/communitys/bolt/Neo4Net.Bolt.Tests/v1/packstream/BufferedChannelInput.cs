using System;

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
namespace Neo4Net.Bolt.v1.packstream
{

	/// <summary>
	/// An <seealso cref="PackInput"/> implementation that reads from an input channel into an internal buffer.
	/// </summary>
	public class BufferedChannelInput : PackInput
	{
		 private readonly ByteBuffer _buffer;
		 private ReadableByteChannel _channel;

		 public BufferedChannelInput( int bufferCapacity )
		 {
			  this._buffer = ByteBuffer.allocate( bufferCapacity ).order( ByteOrder.BIG_ENDIAN );
		 }

		 public virtual BufferedChannelInput Reset( ReadableByteChannel ch )
		 {
			  this._channel = ch;
			  this._buffer.position( 0 );
			  this._buffer.limit( 0 );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean attempt(int numBytes) throws java.io.IOException
		 private bool Attempt( int numBytes )
		 {
			  if ( _buffer.remaining() >= numBytes )
			  {
					return true;
			  }

			  if ( _buffer.remaining() > 0 )
			  {
					// If there is data remaining in the buffer, shift that remaining data to the beginning of the buffer.
					_buffer.compact();
			  }
			  else
			  {
					_buffer.clear();
			  }

			  int count;
			  do
			  {
					count = _channel.read( _buffer );
			  } while ( count >= 0 && ( _buffer.position() < numBytes && _buffer.remaining() != 0 ) );

			  _buffer.flip();
			  return _buffer.remaining() >= numBytes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte readByte() throws java.io.IOException
		 public override sbyte ReadByte()
		 {
			  Ensure( 1 );
			  return _buffer.get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public short readShort() throws java.io.IOException
		 public override short ReadShort()
		 {
			  Ensure( 2 );
			  return _buffer.Short;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readInt() throws java.io.IOException
		 public override int ReadInt()
		 {
			  Ensure( 4 );
			  return _buffer.Int;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readLong() throws java.io.IOException
		 public override long ReadLong()
		 {
			  Ensure( 8 );
			  return _buffer.Long;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double readDouble() throws java.io.IOException
		 public override double ReadDouble()
		 {
			  Ensure( 8 );
			  return _buffer.Double;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackInput readBytes(byte[] into, int index, int toRead) throws java.io.IOException
		 public override PackInput ReadBytes( sbyte[] into, int index, int toRead )
		 {
			  int endIndex = index + toRead;
			  while ( index < endIndex )
			  {
					toRead = Math.Min( _buffer.remaining(), endIndex - index );
					_buffer.get( into, index, toRead );
					index += toRead;
					if ( _buffer.remaining() == 0 && index < endIndex )
					{
						 Attempt( endIndex - index );
						 if ( _buffer.remaining() == 0 )
						 {
							  throw new PackStream.EndOfStream( "Expected " + ( endIndex - index ) + " bytes available, " + "but no more bytes accessible from underlying stream." );
						 }
					}
			  }
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte peekByte() throws java.io.IOException
		 public override sbyte PeekByte()
		 {
			  Ensure( 1 );
			  return _buffer.get( _buffer.position() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensure(int numBytes) throws java.io.IOException
		 private void Ensure( int numBytes )
		 {
			  if ( !Attempt( numBytes ) )
			  {
					throw new PackStream.EndOfStream( "Unexpected end of stream while trying to read " + numBytes + " bytes." );
			  }
		 }
	}

}