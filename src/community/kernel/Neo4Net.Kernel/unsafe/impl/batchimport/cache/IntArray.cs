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
	/// <summary>
	/// Abstraction of a {@code int[]} so that different implementations can be plugged in, for example
	/// off-heap, dynamically growing, or other implementations.
	/// </summary>
	/// <seealso cref= NumberArrayFactory </seealso>
	public interface IntArray : NumberArray<IntArray>
	{
		 int Get( long index );

		 void Set( long index, int value );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void swap(long fromIndex, long toIndex)
	//	 {
	//		  int intermediary = get(fromIndex);
	//		  set(fromIndex, get(toIndex));
	//		  set(toIndex, intermediary);
	//	 }
	}

}