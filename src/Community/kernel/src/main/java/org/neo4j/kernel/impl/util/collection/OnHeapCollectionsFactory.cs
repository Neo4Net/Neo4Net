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
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	using MutableLongDiffSetsImpl = Neo4Net.Kernel.impl.util.diffsets.MutableLongDiffSetsImpl;
	using MemoryTracker = Neo4Net.Memory.MemoryTracker;
	using Value = Neo4Net.Values.Storable.Value;

	public class OnHeapCollectionsFactory : CollectionsFactory
	{
		 public static readonly CollectionsFactory Instance = new OnHeapCollectionsFactory();

		 private OnHeapCollectionsFactory()
		 {
			  // nop
		 }

		 public override MutableLongSet NewLongSet()
		 {
			  return new LongHashSet();
		 }

		 public override MutableLongDiffSetsImpl NewLongDiffSets()
		 {
			  return new MutableLongDiffSetsImpl( this );
		 }

		 public override MutableLongObjectMap<Value> NewValuesMap()
		 {
			  return new LongObjectHashMap<Value>();
		 }

		 public virtual MemoryTracker MemoryTracker
		 {
			 get
			 {
				  return Neo4Net.Memory.MemoryTracker_Fields.None;
			 }
		 }

		 public override void Release()
		 {
			  // nop
		 }
	}

}