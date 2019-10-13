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

	public class BufferedChannelOutput : PackOutput
	{
		 private readonly ByteBuffer _buffer;
		 private WritableByteChannel _channel;

		 public BufferedChannelOutput( int bufferSize )
		 {
			  this._buffer = ByteBuffer.allocate( bufferSize ).order( ByteOrder.BIG_ENDIAN );
		 }

		 public BufferedChannelOutput( WritableByteChannel channel ) : this( channel, 1024 )
		 {
		 }

		 public BufferedChannelOutput( WritableByteChannel channel, int bufferSize ) : this( bufferSize )
		 {
			  Reset( channel );
		 }

		 public virtual void Reset( WritableByteChannel channel )
		 {
			  this._channel = channel;
		 }

		 public override void BeginMessage()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void messageSucceeded() throws java.io.IOException
		 public override void MessageSucceeded()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void messageFailed() throws java.io.IOException
		 public override void MessageFailed()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BufferedChannelOutput flush() throws java.io.IOException
		 public override BufferedChannelOutput Flush()
		 {
			  _buffer.flip();
			  do
			  {
					_channel.write( _buffer );
			  } while ( _buffer.remaining() > 0 );
			  _buffer.clear();
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeBytes(ByteBuffer data) throws java.io.IOException
		 public override PackOutput WriteBytes( ByteBuffer data )
		 {
			  while ( data.remaining() > 0 )
			  {
					if ( _buffer.remaining() == 0 )
					{
						 Flush();
					}

					int oldLimit = data.limit();
					data.limit( data.position() + Math.Min(_buffer.remaining(), data.remaining()) );

					_buffer.put( data );

					data.limit( oldLimit );
			  }
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeBytes(byte[] data, int offset, int length) throws java.io.IOException
		 public override PackOutput WriteBytes( sbyte[] data, int offset, int length )
		 {
			  if ( offset + length > data.Length )
			  {
					throw new IOException( "Asked to write " + length + " bytes, but there is only " + ( data.Length - offset ) + " bytes available in data provided." );
			  }
			  return WriteBytes( ByteBuffer.wrap( data, offset, length ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeByte(byte value) throws java.io.IOException
		 public override PackOutput WriteByte( sbyte value )
		 {
			  Ensure( 1 );
			  _buffer.put( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeShort(short value) throws java.io.IOException
		 public override PackOutput WriteShort( short value )
		 {
			  Ensure( 2 );
			  _buffer.putShort( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeInt(int value) throws java.io.IOException
		 public override PackOutput WriteInt( int value )
		 {
			  Ensure( 4 );
			  _buffer.putInt( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeLong(long value) throws java.io.IOException
		 public override PackOutput WriteLong( long value )
		 {
			  Ensure( 8 );
			  _buffer.putLong( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeDouble(double value) throws java.io.IOException
		 public override PackOutput WriteDouble( double value )
		 {
			  Ensure( 8 );
			  _buffer.putDouble( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _buffer.clear();

			  if ( _channel != null )
			  {
					_channel.close();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensure(int size) throws java.io.IOException
		 private void Ensure( int size )
		 {
			  if ( _buffer.remaining() < size )
			  {
					Flush();
			  }
		 }
	}

}