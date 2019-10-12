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
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;
	using MutableIntList = org.eclipse.collections.api.list.primitive.MutableIntList;
	using IntLists = org.eclipse.collections.impl.factory.primitive.IntLists;
	using IntArrayList = org.eclipse.collections.impl.list.mutable.primitive.IntArrayList;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class SimpleBitSetTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void put()
		 public virtual void Put()
		 {
			  // Given
			  SimpleBitSet set = new SimpleBitSet( 16 );

			  // When
			  set.Put( 2 );
			  set.Put( 7 );
			  set.Put( 15 );

			  // Then
			  assertFalse( set.Contains( 1 ) );
			  assertFalse( set.Contains( 6 ) );
			  assertFalse( set.Contains( 14 ) );

			  assertTrue( set.Contains( 2 ) );
			  assertTrue( set.Contains( 7 ) );
			  assertTrue( set.Contains( 15 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void putAndRemove()
		 public virtual void PutAndRemove()
		 {
			  // Given
			  SimpleBitSet set = new SimpleBitSet( 16 );

			  // When
			  set.Put( 2 );
			  set.Put( 7 );
			  set.Remove( 2 );

			  // Then
			  assertFalse( set.Contains( 1 ) );
			  assertFalse( set.Contains( 6 ) );
			  assertFalse( set.Contains( 14 ) );
			  assertFalse( set.Contains( 2 ) );

			  assertTrue( set.Contains( 7 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void putOtherBitSet()
		 public virtual void PutOtherBitSet()
		 {
			  // Given
			  SimpleBitSet set = new SimpleBitSet( 16 );
			  SimpleBitSet otherSet = new SimpleBitSet( 16 );

			  otherSet.Put( 4 );
			  otherSet.Put( 14 );

			  set.Put( 3 );
			  set.Put( 4 );

			  // When
			  set.Put( otherSet );

			  // Then
			  assertFalse( set.Contains( 0 ) );
			  assertFalse( set.Contains( 1 ) );
			  assertFalse( set.Contains( 15 ) );
			  assertFalse( set.Contains( 7 ) );

			  assertTrue( set.Contains( 3 ) );
			  assertTrue( set.Contains( 4 ) );
			  assertTrue( set.Contains( 14 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeOtherBitSet()
		 public virtual void RemoveOtherBitSet()
		 {
			  // Given
			  SimpleBitSet set = new SimpleBitSet( 16 );
			  SimpleBitSet otherSet = new SimpleBitSet( 16 );

			  otherSet.Put( 4 );
			  otherSet.Put( 12 );
			  otherSet.Put( 14 );

			  set.Put( 3 );
			  set.Put( 4 );
			  set.Put( 12 );

			  // When
			  set.Remove( otherSet );

			  // Then
			  assertFalse( set.Contains( 0 ) );
			  assertFalse( set.Contains( 1 ) );
			  assertFalse( set.Contains( 4 ) );
			  assertFalse( set.Contains( 14 ) );

			  assertTrue( set.Contains( 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void resize()
		 public virtual void Resize()
		 {
			  // Given
			  SimpleBitSet set = new SimpleBitSet( 8 );

			  // When
			  set.Put( 128 );

			  // Then
			  assertTrue( set.Contains( 128 ) );

			  assertFalse( set.Contains( 126 ) );
			  assertFalse( set.Contains( 129 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowIterating()
		 public virtual void ShouldAllowIterating()
		 {
			  // Given
			  SimpleBitSet set = new SimpleBitSet( 64 );
			  set.Put( 4 );
			  set.Put( 7 );
			  set.Put( 63 );
			  set.Put( 78 );

			  // When
			  IntIterator iterator = set.GetEnumerator();
			  MutableIntList found = new IntArrayList();
			  while ( iterator.hasNext() )
			  {
					found.add( iterator.next() );
			  }

			  // Then
			  assertThat( found, equalTo( IntLists.immutable.of( 4, 7, 63, 78 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPointOnUnchangedSetMustDoNothing()
		 public virtual void CheckPointOnUnchangedSetMustDoNothing()
		 {
			  SimpleBitSet set = new SimpleBitSet( 16 );
			  int key = 10;
			  set.Put( key );
			  long checkpoint = 0;
			  checkpoint = set.CheckPointAndPut( checkpoint, key );
			  assertThat( set.CheckPointAndPut( checkpoint, key ), @is( checkpoint ) );
			  assertTrue( set.Contains( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPointOnUnchangedSetButWithDifferentKeyMustUpdateSet()
		 public virtual void CheckPointOnUnchangedSetButWithDifferentKeyMustUpdateSet()
		 {
			  SimpleBitSet set = new SimpleBitSet( 16 );
			  int key = 10;
			  set.Put( key );
			  long checkpoint = 0;
			  checkpoint = set.CheckPointAndPut( checkpoint, key );
			  assertThat( set.CheckPointAndPut( checkpoint, key + 1 ), @is( not( checkpoint ) ) );
			  assertTrue( set.Contains( key + 1 ) );
			  assertFalse( set.Contains( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPointOnChangedSetMustClearState()
		 public virtual void CheckPointOnChangedSetMustClearState()
		 {
			  SimpleBitSet set = new SimpleBitSet( 16 );
			  int key = 10;
			  set.Put( key );
			  long checkpoint = 0;
			  checkpoint = set.CheckPointAndPut( checkpoint, key );
			  set.Put( key + 1 );
			  assertThat( set.CheckPointAndPut( checkpoint, key ), @is( not( checkpoint ) ) );
			  assertTrue( set.Contains( key ) );
			  assertFalse( set.Contains( key + 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPointMustBeAbleToExpandCapacity()
		 public virtual void CheckPointMustBeAbleToExpandCapacity()
		 {
			  SimpleBitSet set = new SimpleBitSet( 16 );
			  int key = 10;
			  int key2 = 255;
			  set.Put( key );
			  long checkpoint = 0;
			  checkpoint = set.CheckPointAndPut( checkpoint, key );
			  assertThat( set.CheckPointAndPut( checkpoint, key2 ), @is( not( checkpoint ) ) );
			  assertTrue( set.Contains( key2 ) );
			  assertFalse( set.Contains( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modificationsMustTakeWriteLocks()
		 public virtual void ModificationsMustTakeWriteLocks()
		 {
			  // We can observe that a write lock was taken, by seeing that an optimistic read lock was invalidated.
			  SimpleBitSet set = new SimpleBitSet( 16 );
			  long stamp = set.tryOptimisticRead();

			  set.Put( 8 );
			  assertFalse( set.validate( stamp ) );
			  stamp = set.tryOptimisticRead();

			  set.Put( 8 );
			  assertFalse( set.validate( stamp ) );
			  stamp = set.tryOptimisticRead();

			  SimpleBitSet other = new SimpleBitSet( 16 );
			  other.Put( 3 );
			  set.Put( other );
			  assertFalse( set.validate( stamp ) );
			  stamp = set.tryOptimisticRead();

			  set.Remove( 3 );
			  assertFalse( set.validate( stamp ) );
			  stamp = set.tryOptimisticRead();

			  set.Remove( 3 );
			  assertFalse( set.validate( stamp ) );
			  stamp = set.tryOptimisticRead();

			  other.Put( 8 );
			  set.Remove( other );
			  assertFalse( set.validate( stamp ) );
			  stamp = set.tryOptimisticRead();

			  other.Put( 8 );
			  set.Remove( other );
			  assertFalse( set.validate( stamp ) );
		 }
	}

}