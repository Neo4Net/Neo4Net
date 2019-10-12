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
namespace Neo4Net.Bolt.v1.messaging
{

	public class RecordingByteChannel : WritableByteChannel, ReadableByteChannel
	{
		 private readonly ByteBuffer _buffer = ByteBuffer.allocate( 64 * 1024 );
		 private int _writePosition;
		 private int _readPosition;
		 private bool _eof;

		 public override bool Open
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override void Close()
		 {

		 }

		 public override int Write( ByteBuffer src )
		 {
			  _buffer.position( _writePosition );
			  int originalPosition = _writePosition;

			  _buffer.put( src );

			  _writePosition = _buffer.position();
			  return _writePosition - originalPosition;
		 }

		 public override int Read( ByteBuffer dst )
		 {
			  if ( _readPosition == _writePosition )
			  {
					return _eof ? -1 : 0;
			  }
			  _buffer.position( _readPosition );
			  int originalPosition = _readPosition;
			  int originalLimit = _buffer.limit();

			  _buffer.limit( Math.Min( _buffer.position() + (dst.limit() - dst.position()), _writePosition ) );
			  dst.put( _buffer );

			  _readPosition = _buffer.position();
			  _buffer.limit( originalLimit );
			  return _readPosition - originalPosition;
		 }

		 public virtual sbyte[] Bytes
		 {
			 get
			 {
				  sbyte[] bytes = new sbyte[_buffer.position()];
				  _buffer.position( 0 );
				  _buffer.get( bytes );
				  return bytes;
			 }
		 }

		 /// <summary>
		 /// Mark this buffer as ended. Once whatever is currently unread in it is consumed,
		 /// it will start yielding -1 responses.
		 /// </summary>
		 public virtual void Eof()
		 {
			  _eof = true;
		 }
	}

}