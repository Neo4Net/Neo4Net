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
	public class LongKeyLongValueTable : IntArrayBasedKeyTable<long[]>
	{
		 public const long NULL = -1;

		 public LongKeyLongValueTable( int capacity ) : base( capacity, 5, 32, new long[]{ NULL } )
		 {
		 }

		 public override long Key( int index )
		 {
			  return GetLong( Address( index ) );
		 }

		 protected internal override void InternalPut( int actualIndex, long key, long[] value )
		 {
			  PutLong( actualIndex, key );
			  PutLong( actualIndex + 2, value[0] );
		 }

		 public override long[] PutValue( int index, long[] value )
		 {
			  int actualValueIndex = Address( index ) + 2;
			  long previous = GetLong( actualValueIndex );
			  PutLong( actualValueIndex, value[0] );
			  return Pack( previous );
		 }

		 public override long[] Value( int index )
		 {
			  return Pack( GetLong( Address( index ) + 2 ) );
		 }

		 protected internal override LongKeyLongValueTable NewInstance( int newCapacity )
		 {
			  return new LongKeyLongValueTable( newCapacity );
		 }

		 private long[] Pack( long value )
		 {
			  SingleValue[0] = value;
			  return SingleValue;
		 }
	}

}