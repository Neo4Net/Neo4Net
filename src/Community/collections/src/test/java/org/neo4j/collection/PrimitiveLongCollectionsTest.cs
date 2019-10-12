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
namespace Neo4Net.Collection
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.jupiter.api.Test;


	using PrimitiveLongBaseIterator = Neo4Net.Collection.PrimitiveLongCollections.PrimitiveLongBaseIterator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.set.mutable.primitive.LongHashSet.newSetWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.mergeToSet;

	internal class PrimitiveLongCollectionsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void arrayOfItemsAsIterator()
		 internal virtual void ArrayOfItemsAsIterator()
		 {
			  // GIVEN
			  long[] items = new long[] { 2, 5, 234 };

			  // WHEN
			  LongIterator iterator = PrimitiveLongCollections.Iterator( items );

			  // THEN
			  AssertItems( iterator, items );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void filter()
		 internal virtual void Filter()
		 {
			  // GIVEN
			  LongIterator items = PrimitiveLongCollections.Iterator( 1, 2, 3 );

			  // WHEN
			  LongIterator filtered = PrimitiveLongCollections.Filter( items, item => item != 2 );

			  // THEN
			  AssertItems( filtered, 1, 3 );
		 }

		 private sealed class CountingPrimitiveLongIteratorResource : LongIterator, AutoCloseable
		 {
			  internal readonly LongIterator Delegate;
			  internal readonly AtomicInteger CloseCounter;

			  internal CountingPrimitiveLongIteratorResource( LongIterator @delegate, AtomicInteger closeCounter )
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
//ORIGINAL LINE: @Test void indexOf()
		 internal virtual void IndexOf()
		 {
			  // GIVEN
			  System.Func<LongIterator> items = () => PrimitiveLongCollections.Iterator(10, 20, 30);

			  // THEN
			  assertEquals( -1, PrimitiveLongCollections.IndexOf( items(), 55 ) );
			  assertEquals( 0, PrimitiveLongCollections.IndexOf( items(), 10 ) );
			  assertEquals( 1, PrimitiveLongCollections.IndexOf( items(), 20 ) );
			  assertEquals( 2, PrimitiveLongCollections.IndexOf( items(), 30 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void count()
		 internal virtual void Count()
		 {
			  // GIVEN
			  LongIterator items = PrimitiveLongCollections.Iterator( 1, 2, 3 );

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
			  LongIterator items = PrimitiveLongCollections.Iterator( 1, 2, 3 );

			  // WHEN
			  long[] array = PrimitiveLongCollections.AsArray( items );

			  // THEN
			  assertTrue( Arrays.Equals( new long[] { 1, 2, 3 }, array ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeduplicate()
		 internal virtual void ShouldDeduplicate()
		 {
			  // GIVEN
			  long[] array = new long[] { 1L, 1L, 2L, 5L, 6L, 6L };

			  // WHEN
			  long[] deduped = PrimitiveLongCollections.Deduplicate( array );

			  // THEN
			  assertArrayEquals( new long[] { 1L, 2L, 5L, 6L }, deduped );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeduplicateWithRandomArrays()
		 internal virtual void ShouldDeduplicateWithRandomArrays()
		 {
			  int arrayLength = 5000;
			  int iterations = 10;
			  for ( int i = 0; i < iterations; i++ )
			  {
					long[] array = ThreadLocalRandom.current().longs(arrayLength, 0, arrayLength).sorted().toArray();
					long[] dedupedActual = PrimitiveLongCollections.Deduplicate( array );
					SortedSet<long> set = new SortedSet<long>();
					foreach ( long value in array )
					{
						 set.Add( value );
					}
					long[] dedupedExpected = new long[set.Count];
					IEnumerator<long> itr = set.GetEnumerator();
					for ( int j = 0; j < dedupedExpected.Length; j++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( itr.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 dedupedExpected[j] = itr.next();
					}
					assertArrayEquals( dedupedExpected, dedupedActual );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotContinueToCallNextOnHasNextFalse()
		 internal virtual void ShouldNotContinueToCallNextOnHasNextFalse()
		 {
			  // GIVEN
			  AtomicLong count = new AtomicLong( 2 );
			  LongIterator iterator = new PrimitiveLongBaseIteratorAnonymousInnerClass( this, count );

			  // WHEN/THEN
			  assertTrue( iterator.hasNext() );
			  assertTrue( iterator.hasNext() );
			  assertEquals( 1L, iterator.next() );
			  assertTrue( iterator.hasNext() );
			  assertTrue( iterator.hasNext() );
			  assertEquals( 0L, iterator.next() );
			  assertFalse( iterator.hasNext() );
			  assertFalse( iterator.hasNext() );
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
//ORIGINAL LINE: @Test void convertJavaCollectionToSetOfPrimitives()
		 internal virtual void ConvertJavaCollectionToSetOfPrimitives()
		 {
			  IList<long> longs = new IList<long> { 1L, 4L, 7L };
			  LongSet longSet = PrimitiveLongCollections.AsSet( longs );
			  assertTrue( longSet.contains( 1L ) );
			  assertTrue( longSet.contains( 4L ) );
			  assertTrue( longSet.contains( 7L ) );
			  assertEquals( 3, longSet.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void convertPrimitiveSetToJavaSet()
		 internal virtual void ConvertPrimitiveSetToJavaSet()
		 {
			  LongSet longSet = newSetWith( 1L, 3L, 5L );
			  ISet<long> longs = PrimitiveLongCollections.ToSet( longSet );
			  assertThat( longs, containsInAnyOrder( 1L, 3L, 5L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mergeLongIterableToSet()
		 internal virtual void MergeLongIterableToSet()
		 {
			  assertThat( mergeToSet( new LongHashSet(), new LongHashSet() ), equalTo(new LongHashSet()) );
			  assertThat( mergeToSet( newSetWith( 1, 2, 3 ), new LongHashSet() ), equalTo(newSetWith(1, 2, 3)) );
			  assertThat( mergeToSet( newSetWith( 1, 2, 3 ), newSetWith( 1, 2, 3, 4, 5, 6 ) ), equalTo( newSetWith( 1, 2, 3, 4, 5, 6 ) ) );
			  assertThat( mergeToSet( newSetWith( 1, 2, 3 ), newSetWith( 4, 5, 6 ) ), equalTo( newSetWith( 1, 2, 3, 4, 5, 6 ) ) );
		 }

		 private void AssertNoMoreItems( LongIterator iterator )
		 {
			  assertFalse( iterator.hasNext(), iterator + " should have no more items" );
			  try
			  {
					iterator.next();
					fail( "Invoking next() on " + iterator + " which has no items left should have thrown NoSuchElementException" );
			  }
			  catch ( NoSuchElementException )
			  { // Good
			  }
		 }

		 private void AssertNextEquals( long expected, LongIterator iterator )
		 {
			  assertTrue( iterator.hasNext(), iterator + " should have had more items" );
			  assertEquals( expected, iterator.next() );
		 }

		 private void AssertItems( LongIterator iterator, params long[] expectedItems )
		 {
			  foreach ( long expectedItem in expectedItems )
			  {
					AssertNextEquals( expectedItem, iterator );
			  }
			  AssertNoMoreItems( iterator );
		 }
	}

}