using System.Collections.Generic;

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


	using PrimitiveLongBaseIterator = Neo4Net.Collections.primitive.PrimitiveLongCollections.PrimitiveLongBaseIterator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
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

	internal class PrimitiveLongCollectionsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void arrayOfItemsAsIterator()
		 internal virtual void ArrayOfItemsAsIterator()
		 {
			  // GIVEN
			  long[] items = new long[]{ 2, 5, 234 };

			  // WHEN
			  PrimitiveLongIterator iterator = PrimitiveLongCollections.Iterator( items );

			  // THEN
			  AssertItems( iterator, items );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void filter()
		 internal virtual void Filter()
		 {
			  // GIVEN
			  PrimitiveLongIterator items = PrimitiveLongCollections.Iterator( 1, 2, 3 );

			  // WHEN
			  PrimitiveLongIterator filtered = PrimitiveLongCollections.Filter( items, item => item != 2 );

			  // THEN
			  AssertItems( filtered, 1, 3 );
		 }

		 private sealed class CountingPrimitiveLongIteratorResource : PrimitiveLongIterator, AutoCloseable
		 {
			  internal readonly PrimitiveLongIterator Delegate;
			  internal readonly AtomicInteger CloseCounter;

			  internal CountingPrimitiveLongIteratorResource( PrimitiveLongIterator @delegate, AtomicInteger closeCounter )
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

			  public override long Next()
			  {
					return Delegate.next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void singleWithDefaultMustAutoCloseIterator()
		 internal virtual void SingleWithDefaultMustAutoCloseIterator()
		 {
			  AtomicInteger counter = new AtomicInteger();
			  CountingPrimitiveLongIteratorResource itr = new CountingPrimitiveLongIteratorResource( PrimitiveLongCollections.Iterator( 13 ), counter );
			  assertEquals( PrimitiveLongCollections.Single( itr, 2 ), 13 );
			  assertEquals( 1, counter.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void singleWithDefaultMustAutoCloseEmptyIterator()
		 internal virtual void SingleWithDefaultMustAutoCloseEmptyIterator()
		 {
			  AtomicInteger counter = new AtomicInteger();
			  CountingPrimitiveLongIteratorResource itr = new CountingPrimitiveLongIteratorResource( PrimitiveLongCollections.EmptyIterator(), counter );
			  assertEquals( PrimitiveLongCollections.Single( itr, 2 ), 2 );
			  assertEquals( 1, counter.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void indexOf()
		 internal virtual void IndexOf()
		 {
			  // GIVEN
			  PrimitiveLongIterable items = () => PrimitiveLongCollections.Iterator(10, 20, 30);

			  // THEN
			  assertEquals( -1, PrimitiveLongCollections.IndexOf( items.GetEnumerator(), 55 ) );
			  assertEquals( 0, PrimitiveLongCollections.IndexOf( items.GetEnumerator(), 10 ) );
			  assertEquals( 1, PrimitiveLongCollections.IndexOf( items.GetEnumerator(), 20 ) );
			  assertEquals( 2, PrimitiveLongCollections.IndexOf( items.GetEnumerator(), 30 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iteratorAsSet()
		 internal virtual void IteratorAsSet()
		 {
			  // GIVEN
			  PrimitiveLongIterator items = PrimitiveLongCollections.Iterator( 1, 2, 3 );

			  // WHEN
			  PrimitiveLongSet set = PrimitiveLongCollections.AsSet( items );

			  // THEN
			  assertTrue( set.Contains( 1 ) );
			  assertTrue( set.Contains( 2 ) );
			  assertTrue( set.Contains( 3 ) );
			  assertFalse( set.Contains( 4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void count()
		 internal virtual void Count()
		 {
			  // GIVEN
			  PrimitiveLongIterator items = PrimitiveLongCollections.Iterator( 1, 2, 3 );

			  // WHEN
			  int count = PrimitiveLongCollections.Count( items );

			  // THEN
			  assertEquals( 3, count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void asArray()
		 internal virtual void AsArray()
		 {
			  // GIVEN
			  PrimitiveLongIterator items = PrimitiveLongCollections.Iterator( 1, 2, 3 );

			  // WHEN
			  long[] array = PrimitiveLongCollections.AsArray( items );

			  // THEN
			  assertTrue( Arrays.Equals( new long[]{ 1, 2, 3 }, array ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeduplicate()
		 internal virtual void ShouldDeduplicate()
		 {
			  // GIVEN
			  long[] array = new long[]{ 1L, 1L, 2L, 5L, 6L, 6L };

			  // WHEN
			  long[] deduped = PrimitiveLongCollections.Deduplicate( array );

			  // THEN
			  assertArrayEquals( new long[]{ 1L, 2L, 5L, 6L }, deduped );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotContinueToCallNextOnHasNextFalse()
		 internal virtual void ShouldNotContinueToCallNextOnHasNextFalse()
		 {
			  // GIVEN
			  AtomicLong count = new AtomicLong( 2 );
			  PrimitiveLongIterator iterator = new PrimitiveLongBaseIteratorAnonymousInnerClass( this, count );

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

		 private class PrimitiveLongBaseIteratorAnonymousInnerClass : PrimitiveLongBaseIterator
		 {
			 private readonly PrimitiveLongCollectionsTest _outerInstance;

			 private AtomicLong _count;

			 public PrimitiveLongBaseIteratorAnonymousInnerClass( PrimitiveLongCollectionsTest outerInstance, AtomicLong count )
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
//ORIGINAL LINE: @Test void copyPrimitiveSet()
		 internal virtual void CopyPrimitiveSet()
		 {
			  PrimitiveLongSet longSet = PrimitiveLongCollections.setOf( 1L, 3L, 5L );
			  PrimitiveLongSet copySet = PrimitiveLongCollections.AsSet( longSet );
			  assertNotSame( copySet, longSet );

			  assertTrue( copySet.Contains( 1L ) );
			  assertTrue( copySet.Contains( 3L ) );
			  assertTrue( copySet.Contains( 5L ) );
			  assertEquals( 3, copySet.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void convertJavaCollectionToSetOfPrimitives()
		 internal virtual void ConvertJavaCollectionToSetOfPrimitives()
		 {
			  IList<long> longs = new IList<long> { 1L, 4L, 7L };
			  PrimitiveLongSet longSet = PrimitiveLongCollections.AsSet( longs );
			  assertTrue( longSet.Contains( 1L ) );
			  assertTrue( longSet.Contains( 4L ) );
			  assertTrue( longSet.Contains( 7L ) );
			  assertEquals( 3, longSet.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void convertPrimitiveSetToJavaSet()
		 internal virtual void ConvertPrimitiveSetToJavaSet()
		 {
			  PrimitiveLongSet longSet = PrimitiveLongCollections.setOf( 1L, 3L, 5L );
			  ISet<long> longs = PrimitiveLongCollections.ToSet( longSet );
			  assertThat( longs, containsInAnyOrder( 1L, 3L, 5L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyMap()
		 internal virtual void CopyMap()
		 {
			  PrimitiveLongObjectMap<object> originalMap = Primitive.LongObjectMap();
			  originalMap.Put( 1L, "a" );
			  originalMap.Put( 2L, "b" );
			  originalMap.Put( 3L, "c" );
			  PrimitiveLongObjectMap<object> copyMap = PrimitiveLongCollections.Copy( originalMap );
			  assertNotSame( originalMap, copyMap );
			  assertEquals( 3, copyMap.Size() );
			  assertEquals( "a", copyMap.Get( 1L ) );
			  assertEquals( "b", copyMap.Get( 2L ) );
			  assertEquals( "c", copyMap.Get( 3L ) );
		 }

		 private static void AssertNoMoreItems( PrimitiveLongIterator iterator )
		 {
			  assertFalse( iterator.HasNext(), iterator + " should have no more items" );
			  assertThrows( typeof( NoSuchElementException ), iterator.next );
		 }

		 private static void AssertNextEquals( long expected, PrimitiveLongIterator iterator )
		 {
			  assertTrue( iterator.HasNext(), iterator + " should have had more items" );
			  assertEquals( expected, iterator.Next() );
		 }

		 private static void AssertItems( PrimitiveLongIterator iterator, params long[] expectedItems )
		 {
			  foreach ( long expectedItem in expectedItems )
			  {
					AssertNextEquals( expectedItem, iterator );
			  }
			  AssertNoMoreItems( iterator );
		 }
	}

}