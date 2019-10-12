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
	using LongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongProcedure;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using ImmutableLongList = org.eclipse.collections.api.list.primitive.ImmutableLongList;
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongLists = org.eclipse.collections.impl.factory.primitive.LongLists;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using Nested = org.junit.jupiter.api.Nested;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using LocalMemoryTracker = Org.Neo4j.Memory.LocalMemoryTracker;
	using MemoryAllocationTracker = Org.Neo4j.Memory.MemoryAllocationTracker;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using RandomExtension = Org.Neo4j.Test.extension.RandomExtension;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.shuffle;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.list.mutable.primitive.LongArrayList.newListWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.set.mutable.primitive.LongHashSet.newSetWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.collection.MutableLinearProbeLongHashSet.DEFAULT_CAPACITY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.collection.MutableLinearProbeLongHashSet.REMOVALS_RATIO;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class MutableLinearProbeLongHashSetTest
	internal class MutableLinearProbeLongHashSetTest
	{
		private bool InstanceFieldsInitialized = false;

		public MutableLinearProbeLongHashSetTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_memoryAllocator = new OffHeapMemoryAllocator( _memoryTracker, _blockAllocator );
			_set = new MutableLinearProbeLongHashSet( _memoryAllocator );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.RandomRule rnd;
		 private RandomRule _rnd;

		 private readonly CachingOffHeapBlockAllocator _blockAllocator = new CachingOffHeapBlockAllocator();
		 private readonly MemoryAllocationTracker _memoryTracker = new LocalMemoryTracker();
		 private MemoryAllocator _memoryAllocator;

		 private MutableLinearProbeLongHashSet _set;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void afterEach()
		 internal virtual void AfterEach()
		 {
			  _set.close();
			  assertEquals( 0, _memoryTracker.usedDirectMemory(), "Leaking memory" );
			  _blockAllocator.release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addRemoveContains()
		 internal virtual void AddRemoveContains()
		 {
			  _set = spy( _set );

			  assertFalse( _set.contains( 0 ) );
			  assertTrue( _set.add( 0 ) );
			  assertTrue( _set.contains( 0 ) );
			  assertFalse( _set.add( 0 ) );
			  assertEquals( 1, _set.size() );

			  assertFalse( _set.contains( 1 ) );
			  assertTrue( _set.add( 1 ) );
			  assertTrue( _set.contains( 1 ) );
			  assertFalse( _set.add( 1 ) );
			  assertEquals( 2, _set.size() );

			  assertFalse( _set.contains( 2 ) );
			  assertTrue( _set.add( 2 ) );
			  assertTrue( _set.contains( 2 ) );
			  assertFalse( _set.add( 2 ) );
			  assertEquals( 3, _set.size() );

			  assertFalse( _set.contains( 3 ) );
			  assertFalse( _set.remove( 3 ) );
			  assertEquals( 3, _set.size() );

			  assertEquals( newSetWith( 0, 1, 2 ), _set );

			  assertTrue( _set.remove( 0 ) );
			  assertFalse( _set.contains( 0 ) );
			  assertEquals( 2, _set.size() );

			  assertTrue( _set.remove( 1 ) );
			  assertFalse( _set.contains( 1 ) );
			  assertEquals( 1, _set.size() );

			  assertTrue( _set.remove( 2 ) );
			  assertFalse( _set.contains( 2 ) );
			  assertEquals( 0, _set.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addRemoveAll()
		 internal virtual void AddRemoveAll()
		 {
			  _set.addAll( 1, 2, 3, 1, 2, 3, 100, 200, 300 );
			  assertEquals( 6, _set.size() );
			  assertTrue( _set.containsAll( 100, 200, 300, 1, 2, 3 ) );

			  _set.removeAll( 1, 2, 100, 200 );
			  assertEquals( 2, _set.size() );
			  assertTrue( _set.containsAll( 300, 3 ) );

			  _set.removeAll( 3, 300 );
			  assertEquals( 0, _set.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void clear()
		 internal virtual void Clear()
		 {
			  _set.addAll( 1, 2, 3 );
			  assertEquals( 3, _set.size() );

			  _set.clear();
			  assertEquals( 0, _set.size() );

			  _set.clear();
			  assertEquals( 0, _set.size() );

			  _set.addAll( 4, 5, 6 );
			  assertEquals( 3, _set.size() );

			  _set.clear();
			  assertEquals( 0, _set.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void grow()
		 internal virtual void Grow()
		 {
			  _set = spy( _set );

			  for ( int i = 0; i < DEFAULT_CAPACITY; i++ )
			  {
					assertTrue( _set.add( 100 + i ) );
			  }
			  verify( _set ).growAndRehash();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rehashWhenTooManyRemovals()
		 internal virtual void RehashWhenTooManyRemovals()
		 {
			  _set = spy( _set );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numOfElements = DEFAULT_CAPACITY / 2;
			  int numOfElements = DEFAULT_CAPACITY / 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int removalsToTriggerRehashing = DEFAULT_CAPACITY / REMOVALS_RATIO;
			  int removalsToTriggerRehashing = DEFAULT_CAPACITY / REMOVALS_RATIO;

			  for ( int i = 0; i < numOfElements; i++ )
			  {
					assertTrue( _set.add( 100 + i ) );
			  }

			  assertEquals( numOfElements, _set.size() );
			  verify( _set, never() ).rehashWithoutGrow();
			  verify( _set, never() ).growAndRehash();

			  for ( int i = 0; i < removalsToTriggerRehashing; i++ )
			  {
					assertTrue( _set.remove( 100 + i ) );
			  }

			  assertEquals( numOfElements - removalsToTriggerRehashing, _set.size() );
			  verify( _set ).rehashWithoutGrow();
			  verify( _set, never() ).growAndRehash();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void forEach()
		 internal virtual void ForEach()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.block.procedure.primitive.LongProcedure consumer = mock(org.eclipse.collections.api.block.procedure.primitive.LongProcedure.class);
			  LongProcedure consumer = mock( typeof( LongProcedure ) );

			  _set.addAll( 1, 2, 100, 200 );
			  _set.forEach( consumer );

			  verify( consumer ).accept( eq( 1L ) );
			  verify( consumer ).accept( eq( 2L ) );
			  verify( consumer ).accept( eq( 100L ) );
			  verify( consumer ).accept( eq( 200L ) );
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allocateFreeMemory()
		 internal virtual void AllocateFreeMemory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.memory.MemoryAllocationTracker memoryTrackerSpy = spy(new org.neo4j.memory.LocalMemoryTracker());
			  MemoryAllocationTracker memoryTrackerSpy = spy( new LocalMemoryTracker() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MutableLinearProbeLongHashSet set2 = new MutableLinearProbeLongHashSet(new OffHeapMemoryAllocator(memoryTrackerSpy, blockAllocator));
			  MutableLinearProbeLongHashSet set2 = new MutableLinearProbeLongHashSet( new OffHeapMemoryAllocator( memoryTrackerSpy, _blockAllocator ) );

			  verify( memoryTrackerSpy ).allocated( anyLong() );

			  for ( int i = 0; i < DEFAULT_CAPACITY; i++ )
			  {
					set2.Add( 100 + i );
			  }
			  verify( memoryTrackerSpy ).deallocated( anyLong() );
			  verify( memoryTrackerSpy, times( 2 ) ).allocated( anyLong() );

			  set2.Close();
			  verify( memoryTrackerSpy, times( 2 ) ).deallocated( anyLong() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void freeFrozenMemory()
		 internal virtual void FreeFrozenMemory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.memory.MemoryAllocationTracker memoryTrackerSpy = spy(new org.neo4j.memory.LocalMemoryTracker());
			  MemoryAllocationTracker memoryTrackerSpy = spy( new LocalMemoryTracker() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MutableLinearProbeLongHashSet set2 = new MutableLinearProbeLongHashSet(new OffHeapMemoryAllocator(memoryTrackerSpy, blockAllocator));
			  MutableLinearProbeLongHashSet set2 = new MutableLinearProbeLongHashSet( new OffHeapMemoryAllocator( memoryTrackerSpy, _blockAllocator ) );

			  verify( memoryTrackerSpy ).allocated( anyLong() );

			  set2.AddAll( 100, 200, 300 );
			  set2.Freeze();
			  set2.Remove( 100 );
			  set2.Freeze();
			  set2.Clear();
			  set2.Close();
			  verify( memoryTrackerSpy, times( 3 ) ).deallocated( anyLong() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void toList()
		 internal virtual void ToList()
		 {
			  assertEquals( 0, _set.toList().size() );

			  _set.addAll( 1, 2, 3, 100, 200, 300 );
			  assertEquals( newListWith( 1, 2, 3, 100, 200, 300 ), _set.toList().sortThis() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void toArray()
		 internal virtual void ToArray()
		 {
			  assertEquals( 0, _set.toArray().Length );

			  _set.addAll( 1, 2, 3, 100, 200, 300 );
			  assertArrayEquals( new long[]{ 1, 2, 3, 100, 200, 300 }, _set.toSortedArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void emptyIterator()
		 internal virtual void EmptyIterator()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.MutableLongIterator iterator = set.longIterator();
			  MutableLongIterator iterator = _set.longIterator();
			  assertFalse( iterator.hasNext );
			  assertThrows( typeof( NoSuchElementException ), iterator.next );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iterator()
		 internal virtual void Iterator()
		 {
			  _set.addAll( 1, 2, 3, 100, 200, 300 );

			  MutableLongIterator iterator = _set.longIterator();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.impl.set.mutable.primitive.LongHashSet visited = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
			  LongHashSet visited = new LongHashSet();
			  while ( iterator.hasNext() )
			  {
					visited.add( iterator.next() );
			  }

			  assertEquals( 6, visited.size() );
			  assertEquals( _set, visited );

			  assertThrows( typeof( NoSuchElementException ), iterator.next );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void freeze()
		 internal virtual void Freeze()
		 {
			  _set.addAll( 1, 2, 3, 100, 200, 300 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.LongSet frozen = set.freeze();
			  LongSet frozen = _set.freeze();
			  assertEquals( _set, frozen );
			  assertEquals( 6, _set.size() );
			  assertEquals( 6, frozen.size() );

			  _set.removeAll( 1, 100 );
			  assertNotEquals( _set, frozen );
			  assertEquals( 4, _set.size() );
			  assertEquals( 6, frozen.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testEquals()
		 internal virtual void TestEquals()
		 {
			  _set.addAll( 1, 2, 3, 100, 200, 300 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MutableLinearProbeLongHashSet set2 = new MutableLinearProbeLongHashSet(memoryAllocator);
			  MutableLinearProbeLongHashSet set2 = new MutableLinearProbeLongHashSet( _memoryAllocator );
			  set2.AddAll( 300, 200, 100, 3, 2, 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.impl.set.mutable.primitive.LongHashSet set3 = newSetWith(300, 200, 100, 3, 2, 1);
			  LongHashSet set3 = newSetWith( 300, 200, 100, 3, 2, 1 );
			  assertEquals( _set, set2 );
			  assertEquals( _set, set3 );

			  _set.removeAll( 1, 100 );
			  assertNotEquals( _set, set2 );
			  assertNotEquals( _set, set3 );

			  set2.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void frozenIterator()
		 internal virtual void FrozenIterator()
		 {
			  _set.addAll( 1, 2, 3, 100, 200, 300 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator iter1 = set.freeze().longIterator();
			  LongIterator iter1 = _set.freeze().longIterator();
			  _set.removeAll( 1, 100 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator iter2 = set.freeze().longIterator();
			  LongIterator iter2 = _set.freeze().longIterator();
			  _set.removeAll( 2, 200 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.LongSet values1 = drain(iter1);
			  LongSet values1 = Drain( iter1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.LongSet values2 = drain(iter2);
			  LongSet values2 = Drain( iter2 );

			  assertEquals( newSetWith( 1, 2, 3, 100, 200, 300 ), values1 );
			  assertEquals( newSetWith( 2, 3, 200, 300 ), values2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void frozenIteratorFailsWhenParentSetIsClosed()
		 internal virtual void FrozenIteratorFailsWhenParentSetIsClosed()
		 {
			  _set.addAll( 1, 2, 3, 100, 200, 300 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator iter = set.freeze().longIterator();
			  LongIterator iter = _set.freeze().longIterator();

			  _set.close();

			  assertThrows( typeof( ConcurrentModificationException ), iter.hasNext );
			  assertThrows( typeof( ConcurrentModificationException ), iter.next );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allSatisfy()
		 internal virtual void AllSatisfy()
		 {
			  _set.addAll( 1, 2, 3, 100, 200, 300 );
			  assertTrue( _set.allSatisfy( x => x < 1000 ) );
			  assertFalse( _set.allSatisfy( x => x % 2 == 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noneSatisfy()
		 internal virtual void NoneSatisfy()
		 {
			  _set.addAll( 1, 2, 3, 100, 200, 300 );
			  assertTrue( _set.noneSatisfy( x => x < 0 ) );
			  assertFalse( _set.noneSatisfy( x => x % 2 == 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void anySatisfy()
		 internal virtual void AnySatisfy()
		 {
			  _set.addAll( 1, 2, 3, 100, 200, 300 );
			  assertTrue( _set.anySatisfy( x => x % 3 == 1 ) );
			  assertFalse( _set.anySatisfy( x => x < 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void randomizedTest()
		 internal virtual void RandomizedTest()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int count = 10000 + rnd.nextInt(1000);
			  int count = 10000 + _rnd.Next( 1000 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet uniqueValues = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
			  MutableLongSet uniqueValues = new LongHashSet();
			  while ( uniqueValues.size() < count )
			  {
					uniqueValues.add( _rnd.nextLong() );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] values = uniqueValues.toArray();
			  long[] values = uniqueValues.toArray();

			  foreach ( long v in values )
			  {
					assertTrue( _set.add( v ) );
			  }
			  shuffle( values );
			  foreach ( long v in values )
			  {
					assertTrue( _set.contains( v ) );
					assertFalse( _set.add( v ) );
			  }
			  assertTrue( _set.containsAll( values ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] toRemove = uniqueValues.select(v -> rnd.nextInt(100) < 75).toArray();
			  long[] toRemove = uniqueValues.select( v => _rnd.Next( 100 ) < 75 ).toArray();
			  shuffle( toRemove );

			  foreach ( long v in toRemove )
			  {
					assertTrue( _set.remove( v ) );
					assertFalse( _set.contains( v ) );
			  }

			  assertEquals( count - toRemove.Length, _set.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nested class Collisions
		 internal class Collisions
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 CollisionsConflict = GenerateCollisions( 5 );
				 A = CollisionsConflict.get( 0 );
				 B = CollisionsConflict.get( 1 );
				 C = CollisionsConflict.get( 2 );
				 D = CollisionsConflict.get( 3 );
				 E = CollisionsConflict.get( 4 );
			 }

			 private readonly MutableLinearProbeLongHashSetTest _outerInstance;

			 public Collisions( MutableLinearProbeLongHashSetTest outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal ImmutableLongList CollisionsConflict;
			  internal long A;
			  internal long B;
			  internal long C;
			  internal long D;
			  internal long E;

			  internal virtual ImmutableLongList GenerateCollisions( int n )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long seed = rnd.nextLong();
					long seed = outerInstance.rnd.NextLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.list.primitive.MutableLongList elements;
					MutableLongList elements;
					using ( MutableLinearProbeLongHashSet s = new MutableLinearProbeLongHashSet( outerInstance.memoryAllocator ) )
					{
						 long v = s.HashAndMask( seed );
						 while ( s.HashAndMask( v ) != 0 || v == 0 || v == 1 )
						 {
							  ++v;
						 }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int h = s.hashAndMask(v);
						 int h = s.HashAndMask( v );
						 elements = LongLists.mutable.with( v );

						 while ( elements.size() < n )
						 {
							  ++v;
							  if ( s.HashAndMask( v ) == h )
							  {
									elements.add( v );
							  }
						 }
					}
					return elements.toImmutable();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAll()
			  internal virtual void AddAll()
			  {
					outerInstance.set.AddAll( CollisionsConflict );
					assertEquals( CollisionsConflict, outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllReversed()
			  internal virtual void AddAllReversed()
			  {
					outerInstance.set.AddAll( CollisionsConflict.toReversed() );
					assertEquals( CollisionsConflict.toReversed(), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllRemoveLast()
			  internal virtual void AddAllRemoveLast()

			  {
					outerInstance.set.AddAll( CollisionsConflict );
					outerInstance.set.Remove( E );
					assertEquals( newListWith( A, B, C, D ), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllRemoveFirst()
			  internal virtual void AddAllRemoveFirst()

			  {
					outerInstance.set.AddAll( CollisionsConflict );
					outerInstance.set.Remove( A );
					assertEquals( newListWith( B, C, D, E ), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllRemoveMiddle()
			  internal virtual void AddAllRemoveMiddle()

			  {
					outerInstance.set.AddAll( CollisionsConflict );
					outerInstance.set.RemoveAll( B, D );
					assertEquals( newListWith( A, C, E ), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllRemoveMiddle2()
			  internal virtual void AddAllRemoveMiddle2()
			  {
					outerInstance.set.AddAll( CollisionsConflict );
					outerInstance.set.RemoveAll( A, C, E );
					assertEquals( newListWith( B, D ), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addReusesRemovedHead()
			  internal virtual void AddReusesRemovedHead()
			  {
					outerInstance.set.AddAll( A, B, C );

					outerInstance.set.Remove( A );
					assertEquals( newListWith( B, C ), outerInstance.set.toList() );

					outerInstance.set.Add( D );
					assertEquals( newListWith( D, B, C ), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addReusesRemovedTail()
			  internal virtual void AddReusesRemovedTail()
			  {
					outerInstance.set.AddAll( A, B, C );

					outerInstance.set.Remove( C );
					assertEquals( newListWith( A, B ), outerInstance.set.toList() );

					outerInstance.set.Add( D );
					assertEquals( newListWith( A, B, D ), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addReusesRemovedMiddle()
			  internal virtual void AddReusesRemovedMiddle()

			  {
					outerInstance.set.AddAll( A, B, C );

					outerInstance.set.Remove( B );
					assertEquals( newListWith( A, C ), outerInstance.set.toList() );

					outerInstance.set.Add( D );
					assertEquals( newListWith( A, D, C ), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addReusesRemovedMiddle2()
			  internal virtual void AddReusesRemovedMiddle2()
			  {
					outerInstance.set.AddAll( A, B, C, D, E );

					outerInstance.set.RemoveAll( B, C );
					assertEquals( newListWith( A, D, E ), outerInstance.set.toList() );

					outerInstance.set.AddAll( C, B );
					assertEquals( newListWith( A, C, B, D, E ), outerInstance.set.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rehashingCompactsSparseSentinels()
			  internal virtual void RehashingCompactsSparseSentinels()
			  {
					outerInstance.set.AddAll( A, B, C, D, E );

					outerInstance.set.RemoveAll( B, D, E );
					assertEquals( newListWith( A, C ), outerInstance.set.toList() );

					outerInstance.set.AddAll( B, D, E );
					assertEquals( newListWith( A, B, C, D, E ), outerInstance.set.toList() );

					outerInstance.set.RemoveAll( B, D, E );
					assertEquals( newListWith( A, C ), outerInstance.set.toList() );

					outerInstance.set.RehashWithoutGrow();
					outerInstance.set.AddAll( E, D, B );
					assertEquals( newListWith( A, C, E, D, B ), outerInstance.set.toList() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nested class IterationConcurrentModification
		 internal class IterationConcurrentModification
		 {
			 private readonly MutableLinearProbeLongHashSetTest _outerInstance;

			 public IterationConcurrentModification( MutableLinearProbeLongHashSetTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void add()
			  internal virtual void Add()
			  {
					TestIteratorFails( s => s.add( 0 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.add( 1 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.add( 0 ), 1, 2, 3 );
					TestIteratorFails( s => s.add( 1 ), 0, 2, 3 );
					TestIteratorFails( s => s.add( 2 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.add( 4 ), 0, 1, 2, 3 );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void remove()
			  internal virtual void Remove()
			  {
					TestIteratorFails( s => s.remove( 0 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.remove( 1 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.remove( 2 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.remove( 4 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.removeAll( LongSets.immutable.empty() ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.removeAll( EMPTY_LONG_ARRAY ), 0, 1, 2, 3 );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAll()
			  internal virtual void AddAll()
			  {
					TestIteratorFails( s => s.addAll( 0, 2 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.addAll( 4, 5 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.addAll( LongSets.immutable.of( 0, 2 ) ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.addAll( LongSets.immutable.of( 4, 5 ) ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.addAll( LongSets.immutable.empty() ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.addAll( EMPTY_LONG_ARRAY ), 0, 1, 2, 3 );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removeAll()
			  internal virtual void RemoveAll()
			  {
					TestIteratorFails( s => s.removeAll( 0, 2 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.removeAll( 4, 5 ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.removeAll( LongSets.immutable.of( 0, 2 ) ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.removeAll( LongSets.immutable.of( 4, 5 ) ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.removeAll( LongSets.immutable.empty() ), 0, 1, 2, 3 );
					TestIteratorFails( s => s.removeAll( EMPTY_LONG_ARRAY ), 0, 1, 2, 3 );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void clear()
			  internal virtual void Clear()
			  {
					TestIteratorFails( MutableLinearProbeLongHashSet.clear, 1, 2, 3 );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void close()
			  internal virtual void Close()
			  {
					TestIteratorFails( MutableLinearProbeLongHashSet.close, 1, 2, 3 );
			  }

			  internal virtual void TestIteratorFails( System.Action<MutableLinearProbeLongHashSet> mutator, params long[] initialValues )
			  {
					outerInstance.set.AddAll( initialValues );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.MutableLongIterator iterator = set.longIterator();
					MutableLongIterator iterator = outerInstance.set.LongIterator();
					assertTrue( iterator.hasNext() );
					assertDoesNotThrow( iterator.next );

					mutator( outerInstance.set );
					assertThrows( typeof( ConcurrentModificationException ), iterator.hasNext );
					assertThrows( typeof( ConcurrentModificationException ), iterator.next );
			  }
		 }

		 private static LongSet Drain( LongIterator iter )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet result = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
			  MutableLongSet result = new LongHashSet();
			  while ( iter.hasNext() )
			  {
					result.add( iter.next() );
			  }
			  return result;
		 }
	}

}