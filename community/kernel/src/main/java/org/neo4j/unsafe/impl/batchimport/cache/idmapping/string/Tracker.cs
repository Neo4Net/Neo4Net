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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string
{

	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;
	using Collector = Org.Neo4j.@unsafe.Impl.Batchimport.input.Collector;
	using Group = Org.Neo4j.@unsafe.Impl.Batchimport.input.Group;

	/// <summary>
	/// <seealso cref="EncodingIdMapper"/> is an index where arbitrary ids, be it <seealso cref="string"/> or {@code long} or whatever
	/// can be added and mapped to an internal (node) {@code long} id. The order in which ids are added can be
	/// any order and so in the end when all ids have been added the index goes through a
	/// <seealso cref="IdMapper.prepare(LongFunction, Collector, ProgressListener) prepare phase"/> where these ids are sorted
	/// so that <seealso cref="IdMapper.get(object, Group)"/> can execute efficiently later on.
	/// <para>
	/// In that sorting the ids aren't moved, but instead a <seealso cref="Tracker"/> created where these moves are recorded
	/// and the initial data (in order of insertion) is kept intact to be able to track <seealso cref="Group"/> belonging among
	/// other things. Since a tracker is instantiated after all ids have been added there's an opportunity to create
	/// a smaller data structure for smaller datasets, for example those that fit inside {@code int} range.
	/// That's why this abstraction exists so that the best suited implementation can be picked for every import.
	/// </para>
	/// </summary>
	public interface Tracker : Org.Neo4j.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable, AutoCloseable
	{
		 /// <param name="index"> data index to get the value for. </param>
		 /// <returns> value previously <seealso cref="set(long, long)"/>. </returns>
		 long Get( long index );

		 /// <summary>
		 /// Swaps values from {@code fromIndex} to {@code toIndex}.
		 /// </summary>
		 /// <param name="fromIndex"> index to swap from. </param>
		 /// <param name="toIndex"> index to swap to. </param>
		 void Swap( long fromIndex, long toIndex );

		 /// <summary>
		 /// Sets {@code value} at the specified {@code index}.
		 /// </summary>
		 /// <param name="index"> data index to set value at. </param>
		 /// <param name="value"> value to set at that index. </param>
		 void Set( long index, long value );

		 void MarkAsDuplicate( long index );

		 bool IsMarkedAsDuplicate( long index );

		 void Close();
	}

}