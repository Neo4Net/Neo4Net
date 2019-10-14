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
namespace Neo4Net.Bolt.v1.messaging.util
{

	public class ArrayByteChannel : ReadableByteChannel
	{
		 private readonly ByteBuffer _data;

		 public ArrayByteChannel( sbyte[] bytes )
		 {
			  this._data = ByteBuffer.wrap( bytes );
		 }

		 public override int Read( ByteBuffer dst )
		 {
			  if ( _data.position() == _data.limit() )
			  {
					return -1;
			  }
			  int originalPosition = _data.position();
			  int originalLimit = _data.limit();
			  _data.limit( Math.Min( _data.limit(), dst.limit() - dst.position() + _data.position() ) );
			  dst.put( _data );
			  _data.limit( originalLimit );
			  return _data.position() - originalPosition;
		 }

		 public override bool Open
		 {
			 get
			 {
				  return _data.position() < _data.limit();
			 }
		 }

		 public override void Close()
		 {
		 }
	}

}