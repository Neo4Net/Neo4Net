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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;

	public class DataProducer : ReadableByteChannel
	{
		 private int _bytesLeftToProduce;
		 private bool _closed;

		 public DataProducer( int size )
		 {
			  this._bytesLeftToProduce = size;
		 }

		 public override bool Open
		 {
			 get
			 {
				  return !_closed;
			 }
		 }

		 public override void Close()
		 {
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "Already closed" );
			  }
			  _closed = true;
		 }

		 public override int Read( ByteBuffer dst )
		 {
			  int toFill = min( dst.remaining(), _bytesLeftToProduce );
			  int leftToFill = toFill;
			  if ( toFill <= 0 )
			  {
					return -1;
			  }

			  while ( leftToFill-- > 0 )
			  {
					dst.put( ( sbyte ) 5 );
			  }
			  _bytesLeftToProduce -= toFill;
			  return toFill;
		 }
	}

}