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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using IMemoryAllocationTracker = Neo4Net.Memory.IMemoryAllocationTracker;

	/// <summary>
	/// Base class for common functionality for any <seealso cref="NumberArray"/> where the data lives off-heap.
	/// </summary>
	internal abstract class OffHeapRegularNumberArray<N> : OffHeapNumberArray<N> where N : NumberArray<N>
	{
		 protected internal readonly int Shift;

		 protected internal OffHeapRegularNumberArray( long length, int shift, long @base, IMemoryAllocationTracker allocationTracker ) : base( length, 1 << shift, @base, allocationTracker )
		 {
			  this.Shift = shift;
		 }

		 protected internal virtual long AddressOf( long index )
		 {
			  index = rebase( index );
			  if ( index < 0 || index >= length )
			  {
					throw new System.IndexOutOfRangeException( "Requested index " + index + ", but length is " + length );
			  }
			  return address + ( index << Shift );
		 }

		 protected internal virtual bool IsByteUniform( long value )
		 {
			  sbyte any = ( sbyte )value;
			  for ( int i = 1; i < itemSize; i++ )
			  {
					sbyte test = ( sbyte )( ( long )( ( ulong )value >> ( i << 3 ) ) );
					if ( test != any )
					{
						 return false;
					}
			  }
			  return true;
		 }
	}

}