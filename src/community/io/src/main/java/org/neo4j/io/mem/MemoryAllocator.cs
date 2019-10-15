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
namespace Neo4Net.Io.mem
{
	using IMemoryAllocationTracker = Neo4Net.Memory.IMemoryAllocationTracker;

	/// <summary>
	/// A MemoryAllocator is simple: it only allocates memory, until it is closed and frees it all in one go.
	/// </summary>
	public interface MemoryAllocator
	{
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static MemoryAllocator createAllocator(String expectedMemory, org.neo4j.memory.IMemoryAllocationTracker memoryTracker)
	//	 {
	//		  return new GrabAllocator(ByteUnit.parse(expectedMemory), memoryTracker);
	//	 }

		 /// <returns> The sum, in bytes, of all the memory currently allocating through this allocator. </returns>
		 long UsedMemory();

		 /// <returns> The amount of available memory, in bytes. </returns>
		 long AvailableMemory();

		 /// <summary>
		 /// Allocate a contiguous, aligned region of memory of the given size in bytes. </summary>
		 /// <param name="bytes"> the number of bytes to allocate. </param>
		 /// <param name="alignment"> The byte multiple that the allocated pointers have to be aligned at. </param>
		 /// <returns> A pointer to the allocated memory. </returns>
		 /// <exception cref="OutOfMemoryError"> if the requested memory could not be allocated. </exception>
		 long AllocateAligned( long bytes, long alignment );

		 /// <summary>
		 /// Close all allocated resources and free all allocated memory.
		 /// Closing can happen by calling close explicitly or by GC as soon as allocator will become phantom reachable.
		 /// It's up to implementations to guarantee correctness in scenario when multiple attempts will be made to release allocator resources.
		 /// As soon as allocated resources will be cleaned any code that will try to access previously available memory will not gonna be able to do so.
		 /// </summary>
		 void Close();
	}

}