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
namespace Neo4Net.Test.impl
{

	using StoreChannel = Neo4Net.Io.fs.StoreChannel;

	public class ChannelInputStream : Stream
	{
		 private readonly StoreChannel _channel;
		 private readonly ByteBuffer _buffer = ByteBuffer.allocate( 8096 );
		 private int _position;

		 public ChannelInputStream( StoreChannel channel )
		 {
			  this._channel = channel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read() throws java.io.IOException
		 public override int Read()
		 {
			  _buffer.clear();
			  _buffer.limit( 1 );
			  while ( _buffer.hasRemaining() )
			  {
					int read = _channel.read( _buffer );

					if ( read == -1 )
					{
						 return -1;
					}
			  }
			  _buffer.flip();
			  _position++;
			  // Return the *unsigned* byte value as an integer
			  return _buffer.get() & 0x000000FF;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(byte[] b, int off, int len) throws java.io.IOException
		 public override int Read( sbyte[] b, int off, int len )
		 {
			  // TODO implement properly
			  return base.Read( b, off, len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int available() throws java.io.IOException
		 public override int Available()
		 {
			  return ( int )( _position - _channel.size() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _channel.close();
		 }
	}

}