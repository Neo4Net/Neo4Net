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
namespace Neo4Net.Memory
{
	/// <summary>
	/// Tracker that capable to report number of allocated bytes. </summary>
	/// <seealso cref= MemoryAllocationTracker </seealso>
	public interface MemoryTracker
	{

		 /// <returns> number of bytes of direct memory that are used </returns>
		 long UsedDirectMemory();
	}

	public static class MemoryTracker_Fields
	{
		 public static readonly MemoryTracker None = () => 0;
	}

}