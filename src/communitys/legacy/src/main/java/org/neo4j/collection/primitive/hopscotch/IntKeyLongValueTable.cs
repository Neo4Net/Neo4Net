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
namespace Neo4Net.Collections.primitive.hopscotch
{
	public class IntKeyLongValueTable : IntArrayBasedKeyTable<long[]>
	{
		 public const long NULL = -1L;

		 public IntKeyLongValueTable( int capacity ) : base( capacity, 3 + 1, 32, new long[]{ NULL } )
		 {
		 }

		 public override long Key( int index )
		 {
			  return Table[Address( index )];
		 }

		 protected internal override void InternalPut( int actualIndex, long key, long[] valueHolder )
		 {
			  Table[actualIndex] = ( int ) key; // we know that key is an int
			  PutLong( actualIndex + 1, valueHolder[0] );
		 }

		 public override long[] Value( int index )
		 {
			  SingleValue[0] = GetLong( Address( index ) + 1 );
			  return SingleValue;
		 }

		 public override long[] PutValue( int index, long[] value )
		 {
			  SingleValue[0] = PutLong( Address( index ) + 1, value[0] );
			  return SingleValue;
		 }

		 protected internal override Table<long[]> NewInstance( int newCapacity )
		 {
			  return new IntKeyLongValueTable( newCapacity );
		 }
	}

}