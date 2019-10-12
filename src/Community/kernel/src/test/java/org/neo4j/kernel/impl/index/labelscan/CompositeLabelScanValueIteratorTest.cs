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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using Test = org.junit.Test;


	using PrimitiveLongCollections = Neo4Net.Collection.PrimitiveLongCollections;
	using PrimitiveLongResourceIterator = Neo4Net.Collection.PrimitiveLongResourceIterator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongResourceCollections.emptyIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongResourceCollections.iterator;

	public class CompositeLabelScanValueIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustHandleEmptyListOfIterators()
		 public virtual void MustHandleEmptyListOfIterators()
		 {
			  // given
			  IList<PrimitiveLongResourceIterator> iterators = emptyList();

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, false );

			  // then
			  assertFalse( iterator.HasNext() );
			  try
			  {
					iterator.Next();
					fail( "Expected iterator to throw" );
			  }
			  catch ( NoSuchElementException )
			  {
					// Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustHandleEmptyIterator()
		 public virtual void MustHandleEmptyIterator()
		 {
			  // given
			  IList<PrimitiveLongResourceIterator> iterators = singletonList( emptyIterator() );

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, false );

			  // then
			  assertFalse( iterator.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustHandleMultipleEmptyIterators()
		 public virtual void MustHandleMultipleEmptyIterators()
		 {
			  // given
			  IList<PrimitiveLongResourceIterator> iterators = AsMutableList( emptyIterator(), emptyIterator(), emptyIterator() );

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, false );

			  // then
			  assertFalse( iterator.HasNext() );
		 }

		 /* ALL = FALSE */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportAllFromSingleIterator()
		 public virtual void MustReportAllFromSingleIterator()
		 {
			  // given
			  long[] expected = new long[] { 0L, 1L, long.MaxValue };
			  IList<PrimitiveLongResourceIterator> iterators = Collections.singletonList( iterator( null, expected ) );

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, false );

			  // then
			  assertArrayEquals( expected, PrimitiveLongCollections.asArray( iterator ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportAllFromNonOverlappingMultipleIterators()
		 public virtual void MustReportAllFromNonOverlappingMultipleIterators()
		 {
			  // given
			  AtomicInteger closeCounter = new AtomicInteger();
			  long[] firstIter = new long[] { 0L, 2L, long.MaxValue };
			  long[] secondIter = new long[] { 1L, 3L };
			  long[] expected = new long[] { 0L, 1L, 2L, 3L, long.MaxValue };
			  IList<PrimitiveLongResourceIterator> iterators = AsMutableList( iterator( closeCounter.incrementAndGet, firstIter ), iterator( closeCounter.incrementAndGet, secondIter ) );

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, false );

			  // then
			  assertArrayEquals( expected, PrimitiveLongCollections.asArray( iterator ) );

			  // when
			  iterator.Close();

			  // then
			  assertEquals( "expected close count", 2, closeCounter.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportUniqueValuesFromOverlappingIterators()
		 public virtual void MustReportUniqueValuesFromOverlappingIterators()
		 {
			  // given
			  AtomicInteger closeCounter = new AtomicInteger();
			  long[] firstIter = new long[] { 0L, 2L, long.MaxValue };
			  long[] secondIter = new long[] { 1L, 3L };
			  long[] thirdIter = new long[] { 0L, 3L };
			  long[] expected = new long[] { 0L, 1L, 2L, 3L, long.MaxValue };
			  IList<PrimitiveLongResourceIterator> iterators = AsMutableList( iterator( closeCounter.incrementAndGet, firstIter ), iterator( closeCounter.incrementAndGet, secondIter ), iterator( closeCounter.incrementAndGet, thirdIter ) );

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, false );

			  // then
			  assertArrayEquals( expected, PrimitiveLongCollections.asArray( iterator ) );

			  // when
			  iterator.Close();

			  // then
			  assertEquals( "expected close count", 3, closeCounter.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportUniqueValuesFromOverlappingIteratorsWithOneEmpty()
		 public virtual void MustReportUniqueValuesFromOverlappingIteratorsWithOneEmpty()
		 {
			  // given
			  AtomicInteger closeCounter = new AtomicInteger();
			  long[] firstIter = new long[] { 0L, 2L, long.MaxValue };
			  long[] secondIter = new long[] { 1L, 3L };
			  long[] thirdIter = new long[] { 0L, 3L };
			  long[] fourthIter = new long[] { };
			  long[] expected = new long[] { 0L, 1L, 2L, 3L, long.MaxValue };
			  IList<PrimitiveLongResourceIterator> iterators = AsMutableList( iterator( closeCounter.incrementAndGet, firstIter ), iterator( closeCounter.incrementAndGet, secondIter ), iterator( closeCounter.incrementAndGet, thirdIter ), iterator( closeCounter.incrementAndGet, fourthIter ) );

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, false );

			  // then
			  assertArrayEquals( expected, PrimitiveLongCollections.asArray( iterator ) );

			  // when
			  iterator.Close();

			  // then
			  assertEquals( "expected close count", 4, closeCounter.get() );
		 }

		 /* ALL = TRUE */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustOnlyReportValuesReportedByAll()
		 public virtual void MustOnlyReportValuesReportedByAll()
		 {
			  // given
			  AtomicInteger closeCounter = new AtomicInteger();
			  long[] firstIter = new long[] { 0L, long.MaxValue };
			  long[] secondIter = new long[] { 0L, 1L, long.MaxValue };
			  long[] thirdIter = new long[] { 0L, 1L, 2L, long.MaxValue };
			  long[] expected = new long[] { 0L, long.MaxValue };
			  IList<PrimitiveLongResourceIterator> iterators = AsMutableList( iterator( closeCounter.incrementAndGet, firstIter ), iterator( closeCounter.incrementAndGet, secondIter ), iterator( closeCounter.incrementAndGet, thirdIter ) );

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, true );

			  // then
			  assertArrayEquals( expected, PrimitiveLongCollections.asArray( iterator ) );

			  // when
			  iterator.Close();

			  // then
			  assertEquals( "expected close count", 3, closeCounter.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustOnlyReportValuesReportedByAllWithOneEmpty()
		 public virtual void MustOnlyReportValuesReportedByAllWithOneEmpty()
		 {
			  // given
			  AtomicInteger closeCounter = new AtomicInteger();
			  long[] firstIter = new long[] { 0L, long.MaxValue };
			  long[] secondIter = new long[] { 0L, 1L, long.MaxValue };
			  long[] thirdIter = new long[] { 0L, 1L, 2L, long.MaxValue };
			  long[] fourthIter = new long[] { };
			  long[] expected = new long[] { };
			  IList<PrimitiveLongResourceIterator> iterators = AsMutableList( iterator( closeCounter.incrementAndGet, firstIter ), iterator( closeCounter.incrementAndGet, secondIter ), iterator( closeCounter.incrementAndGet, thirdIter ), iterator( closeCounter.incrementAndGet, fourthIter ) );

			  // when
			  CompositeLabelScanValueIterator iterator = new CompositeLabelScanValueIterator( iterators, true );

			  // then
			  assertArrayEquals( expected, PrimitiveLongCollections.asArray( iterator ) );

			  // when
			  iterator.Close();

			  // then
			  assertEquals( "expected close count", 4, closeCounter.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final <T> java.util.List<T> asMutableList(T... objects)
		 private IList<T> AsMutableList<T>( params T[] objects )
		 {
			  return new List<T>( Arrays.asList( objects ) );
		 }
	}

}