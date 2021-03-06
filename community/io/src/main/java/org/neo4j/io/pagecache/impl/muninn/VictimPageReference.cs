﻿/*
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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{
	using MemoryAllocationTracker = Org.Neo4j.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

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