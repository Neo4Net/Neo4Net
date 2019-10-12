using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.com
{

	/// <summary>
	/// This will produce data like (bytes):
	/// 
	/// 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4... a.s.o.
	/// 
	/// Up until {@code size} number of bytes has been returned.
	/// 
	/// </summary>
	public class KnownDataByteChannel : ReadableByteChannel
	{
		 protected internal int Position;
		 private readonly int _size;

		 public KnownDataByteChannel( int size )
		 {
			  this._size = size;
		 }

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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
		 public override int Read( ByteBuffer dst )
		 {
			  int toRead = Math.Min( dst.limit() - dst.position(), Left() );
			  if ( toRead == 0 )
			  {
					return -1;
			  }

			  for ( int i = 0; i < toRead; i++ )
			  {
					dst.put( ( sbyte )( ( Position++ ) % 10 ) );
			  }
			  return toRead;
		 }

		 private int Left()
		 {
			  return _size - Position;
		 }
	}

}