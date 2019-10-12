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
namespace Neo4Net.Collection.primitive
{
	using Test = org.junit.jupiter.api.Test;


	using Resource = Neo4Net.Graphdb.Resource;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class PrimitiveLongResourceCollectionsTest
	{
		 private static readonly System.Func<long, bool> _even = value => value % 2 == 0;

		 // ITERATOR

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void simpleIterator()
		 internal virtual void SimpleIterator()
		 {
			  // Given
			  CountingResource resource = new CountingResource();
			  PrimitiveLongResourceIterator iterator = PrimitiveLongResourceCollections.Iterator( resource, 1, 2, 3, 4 );

			  // Then
			  AssertContent( iterator, 1, 2, 3, 4 );

			  // When
			  iterator.Close();

			  // Then
			  assertEquals( 1, resource.CloseCount(), "exactly one call to close" );
		 }

		 // FILTER

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void filterItems()
		 internal virtual void FilterItems()
		 {
			  // Given
			  CountingResource resource = new CountingResource();
			  PrimitiveLongResourceIterator iterator = PrimitiveLongResourceCollections.Iterator( resource, 1, 2, 3, 4 );

			  // When
			  PrimitiveLongResourceIterator filtered = PrimitiveLongResourceCollections.Filter( iterator, _even );

			  // Then
			  AssertContent( filtered, 2, 4 );

			  // When
			  filtered.Close();

			  // Then
			  assertEquals( 1, resource.CloseCount(), "exactly one call to close" );
		 }

		 // CONCAT

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void concatIterators()
		 internal virtual void ConcatIterators()
		 {
			  // Given
			  CountingResource resource = new CountingResource();
			  PrimitiveLongResourceIterator first = PrimitiveLongResourceCollections.Iterator( resource, 1, 2 );
			  PrimitiveLongResourceIterator second = PrimitiveLongResourceCollections.Iterator( resource, 3, 4 );

			  // When
			  PrimitiveLongResourceIterator concat = PrimitiveLongResourceCollections.Concat( first, second );

			  // Then
			  AssertContent( concat, 1, 2, 3, 4 );

			  // When
			  concat.Close();

			  // Then
			  assertEquals( 2, resource.CloseCount(), "all concatenated iterators are closed" );
		 }

		 private static void AssertContent( PrimitiveLongResourceIterator iterator, params long[] expected )
		 {
			  int i = 0;
			  while ( iterator.hasNext() )
			  {
					assertEquals( expected[i++], iterator.next(), "has expected value" );
			  }
			  assertEquals( expected.Length, i, "has all expected values" );
		 }

		 private class CountingResource : Resource
		 {
			  internal AtomicInteger Closed = new AtomicInteger();

			  public override void Close()
			  {
					Closed.incrementAndGet();
			  }

			  internal virtual int CloseCount()
			  {
					return Closed.get();
			  }
		 }
	}

}