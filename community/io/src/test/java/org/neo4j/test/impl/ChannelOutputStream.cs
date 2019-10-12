using System;

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
namespace Org.Neo4j.Test.impl
{

	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;

	public class ChannelOutputStream : Stream
	{
		 private readonly StoreChannel _channel;
		 private readonly ByteBuffer _buffer = ByteBuffer.allocate( 8096 );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ChannelOutputStream(org.neo4j.io.fs.StoreChannel channel, boolean append) throws java.io.IOException
		 public ChannelOutputStream( StoreChannel channel, bool append )
		 {
			  this._channel = channel;
			  if ( append )
			  {
					this._channel.position( this._channel.size() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(int b) throws java.io.IOException
		 public override void Write( int b )
		 {
			  _buffer.clear();
			  _buffer.put( ( sbyte ) b );
			  _buffer.flip();
			  _channel.write( _buffer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b) throws java.io.IOException
		 public override void Write( sbyte[] b )
		 {
			  Write( b, 0, b.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b, int off, int len) throws java.io.IOException
		 public override void Write( sbyte[] b, int off, int len )
		 {
			  int written = 0;
			  int index = off;
			  while ( written < len )
			  {
					_buffer.clear();
					_buffer.put( b, index + written, Math.Min( len - written, _buffer.capacity() ) );
					_buffer.flip();
					written += _channel.write( _buffer );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _channel.close();
		 }
	}

}