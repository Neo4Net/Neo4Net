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
namespace Neo4Net.Collection.primitive.hopscotch
{
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;

	public class LongKeyLongValueUnsafeTable : UnsafeTable<long[]>
	{
		 public LongKeyLongValueUnsafeTable( int capacity, MemoryAllocationTracker allocationTracker ) : base( capacity, 16, new long[1], allocationTracker )
		 {
		 }

		 protected internal override long InternalKey( long keyAddress )
		 {
			  return AlignmentSafeGetLongAsTwoInts( keyAddress );
		 }

		 protected internal override void InternalPut( long keyAddress, long key, long[] value )
		 {
			  AlignmentSafePutLongAsTwoInts( keyAddress, key );
			  AlignmentSafePutLongAsTwoInts( keyAddress + 8, value[0] );
		 }

		 protected internal override long[] InternalRemove( long keyAddress )
		 {
			  ValueMarker[0] = AlignmentSafeGetLongAsTwoInts( keyAddress + 8 );
			  AlignmentSafePutLongAsTwoInts( keyAddress, -1 );
			  return ValueMarker;
		 }

		 public override long[] PutValue( int index, long[] value )
		 {
			  long valueAddress = valueAddress( index );
			  long oldValue = AlignmentSafeGetLongAsTwoInts( valueAddress );
			  AlignmentSafePutLongAsTwoInts( valueAddress, value[0] );
			  return Pack( oldValue );
		 }

		 private long[] Pack( long value )
		 {
			  ValueMarker[0] = value;
			  return ValueMarker;
		 }

		 private long ValueAddress( int index )
		 {
			  return KeyAddress( index ) + 8;
		 }

		 public override long[] Value( int index )
		 {
			  long value = AlignmentSafeGetLongAsTwoInts( ValueAddress( index ) );
			  return Pack( value );
		 }

		 protected internal override Table<long[]> NewInstance( int newCapacity )
		 {
			  return new LongKeyLongValueUnsafeTable( newCapacity, AllocationTracker );
		 }
	}

}