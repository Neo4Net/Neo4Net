using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.util.collection
{
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;


	using Resource = Org.Neo4j.Graphdb.Resource;
	using AppendOnlyValuesContainer = Org.Neo4j.Kernel.Impl.Api.state.AppendOnlyValuesContainer;
	using ValuesContainer = Org.Neo4j.Kernel.Impl.Api.state.ValuesContainer;
	using ValuesMap = Org.Neo4j.Kernel.Impl.Api.state.ValuesMap;
	using MutableLongDiffSetsImpl = Org.Neo4j.Kernel.impl.util.diffsets.MutableLongDiffSetsImpl;
	using LocalMemoryTracker = Org.Neo4j.Memory.LocalMemoryTracker;
	using MemoryAllocationTracker = Org.Neo4j.Memory.MemoryAllocationTracker;
	using MemoryTracker = Org.Neo4j.Memory.MemoryTracker;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class OffHeapCollectionsFactory : CollectionsFactory
	{
		 private readonly MemoryAllocationTracker _memoryTracker = new LocalMemoryTracker();
		 private readonly MemoryAllocator _allocator;

		 private readonly ICollection<Resource> _resources = new List<Resource>();
		 private ValuesContainer _valuesContainer;

		 public OffHeapCollectionsFactory( OffHeapBlockAllocator blockAllocator )
		 {
			  this._allocator = new OffHeapMemoryAllocator( _memoryTracker, blockAllocator );
		 }

		 public override MutableLongSet NewLongSet()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MutableLinearProbeLongHashSet set = new MutableLinearProbeLongHashSet(allocator);
			  MutableLinearProbeLongHashSet set = new MutableLinearProbeLongHashSet( _allocator );
			  _resources.Add( set );
			  return set;
		 }

		 public override MutableLongDiffSetsImpl NewLongDiffSets()
		 {
			  return new MutableLongDiffSetsImpl( this );
		 }

		 public override MutableLongObjectMap<Value> NewValuesMap()
		 {
			  if ( _valuesContainer == null )
			  {
					_valuesContainer = new AppendOnlyValuesContainer( _allocator );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LinearProbeLongLongHashMap refs = new LinearProbeLongLongHashMap(allocator);
			  LinearProbeLongLongHashMap refs = new LinearProbeLongLongHashMap( _allocator );
			  _resources.Add( refs );
			  return new ValuesMap( refs, _valuesContainer );
		 }

		 public virtual MemoryTracker MemoryTracker
		 {
			 get
			 {
				  return _memoryTracker;
			 }
		 }

		 public override void Release()
		 {
			  _resources.forEach( Resource.close );
			  _resources.Clear();
			  if ( _valuesContainer != null )
			  {
					_valuesContainer.close();
					_valuesContainer = null;
			  }
		 }
	}

}