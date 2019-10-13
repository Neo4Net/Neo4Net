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
namespace Neo4Net.Collections.primitive
{
	using Test = org.junit.jupiter.api.Test;


	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class PrimitiveIntCollectionsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void arrayOfItemsAsIterator()
		 internal virtual void ArrayOfItemsAsIterator()
		 {
			  // GIVEN
			  int[] items = new int[]{ 2, 5, 234 };

			  // WHEN
			  PrimitiveIntIterator iterator = PrimitiveIntCollections.Iterator( items );

			  // THEN
			  AssertItems( iterator, items );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void convertCollectionToLongArray()
		 internal virtual void ConvertCollectionToLongArray()
		 {
			  PrimitiveIntSet heapSet = PrimitiveIntCollections.AsSet( new int[]{ 1, 2, 3 } );
			  PrimitiveIntSet offHeapIntSet = Primitive.OffHeapIntSet( GlobalMemoryTracker.INSTANCE );
			  offHeapIntSet.Add( 7 );
			  offHeapIntSet.Add( 8 );
			  assertArrayEquals( new long[]{ 1, 2, 3 }, PrimitiveIntCollections.AsLongArray( heapSet ) );
			  assertArrayEquals( new long[]{ 7, 8 }, PrimitiveIntCollections.AsLongArray( offHeapIntSet ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void concatenateTwoIterators()
		 internal virtual void ConcatenateTwoIterators()
		 {
			  // GIVEN
			  PrimitiveIntIterator firstItems = PrimitiveIntCollections.Iterator( 10, 3, 203, 32 );
			  PrimitiveIntIterator otherItems = PrimitiveIntCollections.Iterator( 1, 2, 5 );

			  // WHEN
			  PrimitiveIntIterator iterator = PrimitiveIntCollections.Concat( asList( firstItems, otherItems ).GetEnumerator() );

			  // THEN
			  AssertItems( iterator, 10, 3, 203, 32, 1, 2, 5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void filter()
		 internal virtual void Filter()
		 {
			  // GIVEN
			  PrimitiveIntIterator items = PrimitiveIntCollections.Iterator( 1, 2, 3 );

			  // WHEN
			  PrimitiveIntIterator filtered = PrimitiveIntCollections.Filter( items, item => item != 2 );

			  // THEN
			  AssertItems( filtered, 1, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void deduplicate()
		 internal virtual void Deduplicate()
		 {
			  // GIVEN
			  PrimitiveIntIterator items = PrimitiveIntCollections.Iterator( 1, 1, 2, 3, 2 );

			  // WHEN
			  PrimitiveIntIterator deduped = PrimitiveIntCollections.Deduplicate( items );

			  // THEN
			  AssertItems( deduped, 1, 2, 3 );
		 }

		 private sealed class CountingPrimitiveIntIteratorResource : PrimitiveIntIterator, AutoCloseable
		 {
			  internal readonly PrimitiveIntIterator Delegate;
			  internal readonly AtomicInteger CloseCounter;

			  internal CountingPrimitiveIntIteratorResource( PrimitiveIntIterator @delegate, AtomicInteger closeCounter )
			  {
					this.Delegate = @delegate;
					this.CloseCounter = closeCounter;
			  }

			  public override void Close()
			  {
					CloseCounter.incrementAndGet();
			  }

			  public override bool HasNext()
			  {
					return Delegate.hasNext();
			  }

			  public override int Next()
			  {
					return Delegate.next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iteratorAsSet()
		 internal virtual void IteratorAsSet()
		 {
			  // GIVEN
			  PrimitiveIntIterator items = PrimitiveIntCollections.Iterator( 1, 2, 3 );

			  // WHEN
			  PrimitiveIntSet set = PrimitiveIntCollections.AsSet( items );

			  // THEN
			  assertTrue( set.Contains( 1 ) );
			  assertTrue( set.Contains( 2 ) );
			  assertTrue( set.Contains( 3 ) );
			  assertFalse( set.Contains( 4 ) );
			  assertThrows( typeof( System.InvalidOperationException ), () => PrimitiveIntCollections.AsSet(PrimitiveIntCollections.Iterator(1, 2, 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotContinueToCallNextOnHasNextFalse()
		 internal virtual void ShouldNotContinueToCallNextOnHasNextFalse()
		 {
			  // GIVEN
			  AtomicInteger count = new AtomicInteger( 2 );
			  PrimitiveIntIterator iterator = new PrimitiveIntBaseIteratorAnonymousInnerClass( this, count );

			  // WHEN/THEN
			  assertTrue( iterator.HasNext() );
			  assertTrue( iterator.HasNext() );
			  assertEquals( 1L, iterator.Next() );
			  assertTrue( iterator.HasNext() );
			  assertTrue( iterator.HasNext() );
			  assertEquals( 0L, iterator.Next() );
			  assertFalse( iterator.HasNext() );
			  assertFalse( iterator.HasNext() );
			  assertEquals( -1L, count.get() );
		 }

		 private class PrimitiveIntBaseIteratorAnonymousInnerClass : PrimitiveIntCollections.PrimitiveIntBaseIterator
		 {
			 private readonly PrimitiveIntCollectionsTest _outerInstance;

			 private AtomicInteger _count;

			 public PrimitiveIntBaseIteratorAnonymousInnerClass( PrimitiveIntCollectionsTest outerInstance, AtomicInteger count )
			 {
				 this.outerInstance = outerInstance;
				 this._count = count;
			 }

			 protected internal override bool fetchNext()
			 {
				  return _count.decrementAndGet() >= 0 && next(_count.get());
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeduplicate()
		 internal virtual void ShouldDeduplicate()
		 {
			  // GIVEN
			  int[] array = new int[]{ 1, 6, 2, 5, 6, 1, 6 };

			  // WHEN
			  int[] deduped = PrimitiveIntCollections.Deduplicate( array );

			  // THEN
			  assertArrayEquals( new int[]{ 1, 6, 2, 5 }, deduped );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyMap()
		 internal virtual void CopyMap()
		 {
			  PrimitiveIntObjectMap<object> originalMap = Primitive.IntObjectMap();
			  originalMap.Put( 1, "a" );
			  originalMap.Put( 2, "b" );
			  originalMap.Put( 3, "c" );
			  PrimitiveIntObjectMap<object> copyMap = PrimitiveIntCollections.Copy( originalMap );
			  assertNotSame( originalMap, copyMap );
			  assertEquals( 3, copyMap.Size() );
			  assertEquals( "a", copyMap.Get( 1 ) );
			  assertEquals( "b", copyMap.Get( 2 ) );
			  assertEquals( "c", copyMap.Get( 3 ) );
		 }

		 private static void AssertNoMoreItems( PrimitiveIntIterator iterator )
		 {
			  assertFalse( iterator.HasNext(), iterator + " should have no more items" );
			  assertThrows( typeof( NoSuchElementException ), iterator.next );
		 }

		 private static void AssertNextEquals( long expected, PrimitiveIntIterator iterator )
		 {
			  assertTrue( iterator.HasNext(), iterator + " should have had more items" );
			  assertEquals( expected, iterator.Next() );
		 }

		 private static void AssertItems( PrimitiveIntIterator iterator, params int[] expectedItems )
		 {
			  foreach ( long expectedItem in expectedItems )
			  {
					AssertNextEquals( expectedItem, iterator );
			  }
			  AssertNoMoreItems( iterator );
		 }
	}

}