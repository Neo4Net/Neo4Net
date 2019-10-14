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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;

	internal class VictimPageReference
	{
		 private static int _victimPageSize = -1;
		 private static long _victimPagePointer;

		 private VictimPageReference()
		 {
			  // All state is static
		 }

		 internal static long GetVictimPage( int pageSize, MemoryAllocationTracker allocationTracker )
		 {
			 lock ( typeof( VictimPageReference ) )
			 {
				  if ( _victimPageSize < pageSize )
				  {
						// Note that we NEVER free any old victim pages. This is important because we cannot tell
						// when we are done using them. Therefor, victim pages are allocated and stay allocated
						// until our process terminates.
						_victimPagePointer = UnsafeUtil.allocateMemory( pageSize, allocationTracker );
						_victimPageSize = pageSize;
				  }
				  return _victimPagePointer;
			 }
		 }
	}

}