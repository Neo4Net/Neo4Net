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
namespace Neo4Net.Kernel.impl.util.collection
{
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;

	using TxState = Neo4Net.Kernel.Impl.Api.state.TxState;
	using MutableLongDiffSetsImpl = Neo4Net.Kernel.impl.util.diffsets.MutableLongDiffSetsImpl;
	using MemoryTracker = Neo4Net.Memory.MemoryTracker;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// The purpose of this factory is the ability to switch between multiple collection implementations used in <seealso cref="TxState"/> (e.g. on- or off-heap),
	/// keeping track of underlying memory allocations.
	/// </summary>
	public interface CollectionsFactory
	{
		 MutableLongSet NewLongSet();

		 MutableLongDiffSetsImpl NewLongDiffSets();

		 MutableLongObjectMap<Value> NewValuesMap();

		 MemoryTracker MemoryTracker { get; }

		 /// <summary>
		 /// Release previously created collections. This method does not invalidate the factory.
		 /// </summary>
		 void Release();
	}

}