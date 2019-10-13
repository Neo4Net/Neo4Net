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
	public class LongKeyIntValueTable : IntArrayBasedKeyTable<int[]>
	{
		 public const int NULL = -1;

		 public LongKeyIntValueTable( int capacity ) : base( capacity, 3 + 1, 32, new int[]{ NULL } )
		 {
		 }

		 public override long Key( int index )
		 {
			  return GetLong( Address( index ) );
		 }

		 protected internal override void InternalPut( int actualIndex, long key, int[] value )
		 {
			  PutLong( actualIndex, key );
			  Table[actualIndex + 2] = value[0];
		 }

		 public override int[] PutValue( int index, int[] value )
		 {
			  int actualIndex = Address( index ) + 2;
			  int previous = Table[actualIndex];
			  Table[actualIndex] = value[0];
			  return Pack( previous );
		 }

		 public override int[] Value( int index )
		 {
			  return Pack( Table[Address( index ) + 2] );
		 }

		 protected internal override Table<int[]> NewInstance( int newCapacity )
		 {
			  return new LongKeyIntValueTable( newCapacity );
		 }

		 private int[] Pack( int value )
		 {
			  SingleValue[0] = value;
			  return SingleValue;
		 }
	}

}