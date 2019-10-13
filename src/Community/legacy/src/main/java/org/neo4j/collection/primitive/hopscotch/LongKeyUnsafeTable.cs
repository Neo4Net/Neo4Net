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
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;

	public class LongKeyUnsafeTable<VALUE> : UnsafeTable<VALUE>
	{
		 public LongKeyUnsafeTable( int capacity, VALUE valueMarker, MemoryAllocationTracker allocationTracker ) : base( capacity, 8, valueMarker, allocationTracker )
		 {
		 }

		 protected internal override long InternalKey( long keyAddress )
		 {
			  return AlignmentSafeGetLongAsTwoInts( keyAddress );
		 }

		 protected internal override void InternalPut( long keyAddress, long key, VALUE value )
		 {
			  AlignmentSafePutLongAsTwoInts( keyAddress, key );
		 }

		 protected internal override Table<VALUE> NewInstance( int newCapacity )
		 {
			  return new LongKeyUnsafeTable<VALUE>( newCapacity, ValueMarker, AllocationTracker );
		 }
	}

}