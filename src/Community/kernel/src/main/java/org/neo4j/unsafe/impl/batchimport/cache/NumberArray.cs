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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	/// <summary>
	/// Abstraction over primitive arrays.
	/// </summary>
	/// <seealso cref= NumberArrayFactory </seealso>
	public interface NumberArray<N> : MemoryStatsVisitor_Visitable, AutoCloseable where N : NumberArray<N>
	{
		 /// <returns> length of the array, i.e. the capacity. </returns>
		 long Length();

		 /// <summary>
		 /// Swaps items from {@code fromIndex} to {@code toIndex}, such that
		 /// {@code fromIndex} and {@code toIndex}, {@code fromIndex+1} and {@code toIndex} a.s.o swaps places.
		 /// The number of items swapped is equal to the length of the default value of the array. </summary>
		 ///  <param name="fromIndex"> where to start swapping from. </param>
		 /// <param name="toIndex"> where to start swapping to. </param>
		 void Swap( long fromIndex, long toIndex );

		 /// <summary>
		 /// Sets all values to a default value.
		 /// </summary>
		 void Clear();

		 /// <summary>
		 /// Releases any resources that GC won't release automatically.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Part of the nature of <seealso cref="NumberArray"/> is that <seealso cref="length()"/> can be dynamically growing.
		 /// For that to work some implementations (those coming from e.g
		 /// <seealso cref="NumberArrayFactory.newDynamicIntArray(long, int)"/> and such dynamic calls) has an indirection,
		 /// one that is a bit costly when comparing to raw array access. In scenarios where there will be two or
		 /// more access to the same index in the array it will be more efficient to resolve this indirection once
		 /// and return the "raw" array for that given index so that it can be used directly in multiple calls,
		 /// knowing that the returned array holds the given index.
		 /// </summary>
		 /// <param name="index"> index into the array which the returned array will contain. </param>
		 /// <returns> array sure to hold the given index. </returns>
		 N At( long index );
	}

}