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
namespace Org.Neo4j.Helpers.Collection
{
	using Test = org.junit.jupiter.api.Test;


	using Resource = Org.Neo4j.Graphdb.Resource;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	internal class TestCommonIterators
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testNoDuplicatesFilteringIterator()
		 internal virtual void TestNoDuplicatesFilteringIterator()
		 {
			  IList<int> ints = new IList<int> { 1, 2, 2, 40, 100, 40, 101, 2, 3 };
			  IEnumerator<int> iterator = FilteringIterator.NoDuplicates( ints.GetEnumerator() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 1, iterator.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 2, iterator.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 40, iterator.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 100, iterator.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 101, iterator.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 3, iterator.next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testFirstElement()
		 internal virtual void TestFirstElement()
		 {
			  object @object = new object();
			  object object2 = new object();

			  // first Iterable
			  assertEquals( @object, Iterables.First( asList( @object, object2 ) ) );
			  assertEquals( @object, Iterables.First( asList( @object ) ) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterables.First(asList()) );

			  // first Iterator
			  assertEquals( @object, Iterators.First( asList( @object, object2 ).GetEnumerator() ) );
			  assertEquals( @object, Iterators.First( asList( @object ).GetEnumerator() ) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterators.First(asList().GetEnumerator()) );

			  // firstOrNull Iterable
			  assertEquals( @object, Iterables.FirstOrNull( asList( @object, object2 ) ) );
			  assertEquals( @object, Iterables.FirstOrNull( asList( @object ) ) );
			  assertNull( Iterables.FirstOrNull( asList() ) );

			  // firstOrNull Iterator
			  assertEquals( @object, Iterators.FirstOrNull( asList( @object, object2 ).GetEnumerator() ) );
			  assertEquals( @object, Iterators.FirstOrNull( asList( @object ).GetEnumerator() ) );
			  assertNull( Iterators.FirstOrNull( asList().GetEnumerator() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testLastElement()
		 internal virtual void TestLastElement()
		 {
			  object @object = new object();
			  object object2 = new object();

			  // last Iterable
			  assertEquals( object2, Iterables.Last( asList( @object, object2 ) ) );
			  assertEquals( @object, Iterables.Last( asList( @object ) ) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterables.Last(asList()) );

			  // last Iterator
			  assertEquals( object2, Iterators.Last( asList( @object, object2 ).GetEnumerator() ) );
			  assertEquals( @object, Iterators.Last( asList( @object ).GetEnumerator() ) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterators.Last(asList().GetEnumerator()) );

			  // lastOrNull Iterable
			  assertEquals( object2, Iterables.LastOrNull( asList( @object, object2 ) ) );
			  assertEquals( @object, Iterables.LastOrNull( asList( @object ) ) );
			  assertNull( Iterables.LastOrNull( asList() ) );

			  // lastOrNull Iterator
			  assertEquals( object2, Iterators.LastOrNull( asList( @object, object2 ).GetEnumerator() ) );
			  assertEquals( @object, Iterators.LastOrNull( asList( @object ).GetEnumerator() ) );
			  assertNull( Iterators.LastOrNull( asList().GetEnumerator() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testSingleElement()
		 internal virtual void TestSingleElement()
		 {
			  object @object = new object();
			  object object2 = new object();

			  // single Iterable
			  assertEquals( @object, Iterables.Single( asList( @object ) ) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterables.Single(asList()) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterables.Single(asList(@object, object2)) );

			  // single Iterator
			  assertEquals( @object, Iterators.Single( asList( @object ).GetEnumerator() ) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterators.Single(asList().GetEnumerator()) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterators.Single(asList(@object, object2).GetEnumerator()) );

			  // singleOrNull Iterable
			  assertEquals( @object, Iterables.SingleOrNull( asList( @object ) ) );
			  assertNull( Iterables.SingleOrNull( asList() ) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterables.SingleOrNull(asList(@object, object2)) );

			  // singleOrNull Iterator
			  assertEquals( @object, Iterators.SingleOrNull( asList( @object ).GetEnumerator() ) );
			  assertNull( Iterators.SingleOrNull( asList().GetEnumerator() ) );
			  assertThrows( typeof( NoSuchElementException ), () => Iterators.SingleOrNull(asList(@object, object2).GetEnumerator()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getItemFromEnd()
		 internal virtual void getItemFromEnd()
		 {
			  IEnumerable<int> ints = asList( 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 );
			  assertEquals( ( int? ) 9, Iterables.FromEnd( ints, 0 ) );
			  assertEquals( ( int? ) 8, Iterables.FromEnd( ints, 1 ) );
			  assertEquals( ( int? ) 7, Iterables.FromEnd( ints, 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iteratorsStreamForNull()
		 internal virtual void IteratorsStreamForNull()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => Iterators.Stream(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iteratorsStream()
		 internal virtual void IteratorsStream()
		 {
			  IList<object> list = new IList<object> { 1, 2, "3", '4', null, "abc", "56789" };

			  IEnumerator<object> iterator = list.GetEnumerator();

			  assertEquals( list, Iterators.Stream( iterator ).collect( toList() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iteratorsStreamClosesResourceIterator()
		 internal virtual void IteratorsStreamClosesResourceIterator()
		 {
			  IList<object> list = new IList<object> { "a", "b", "c", "def" };

			  Resource resource = mock( typeof( Resource ) );
			  ResourceIterator<object> iterator = Iterators.ResourceIterator( list.GetEnumerator(), resource );

			  using ( Stream<object> stream = Iterators.Stream( iterator ) )
			  {
					assertEquals( list, stream.collect( toList() ) );
			  }
			  verify( resource ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iteratorsStreamCharacteristics()
		 internal virtual void IteratorsStreamCharacteristics()
		 {
			  IEnumerator<int> iterator = asList( 1, 2, 3 ).GetEnumerator();
			  int characteristics = Spliterator.DISTINCT | Spliterator.ORDERED | Spliterator.SORTED;

			  Stream<int> stream = Iterators.Stream( iterator, characteristics );

			  assertEquals( characteristics, stream.spliterator().characteristics() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iterablesStreamForNull()
		 internal virtual void IterablesStreamForNull()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => Iterables.Stream(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iterablesStream()
		 internal virtual void IterablesStream()
		 {
			  IList<object> list = new IList<object> { 1, 2, "3", '4', null, "abc", "56789" };

			  assertEquals( list, Iterables.Stream( list ).collect( toList() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iterablesStreamClosesResourceIterator()
		 internal virtual void IterablesStreamClosesResourceIterator()
		 {
			  IList<object> list = new IList<object> { "a", "b", "c", "def" };

			  Resource resource = mock( typeof( Resource ) );
			  ResourceIterable<object> iterable = () => Iterators.ResourceIterator(list.GetEnumerator(), resource);

			  using ( Stream<object> stream = Iterables.Stream( iterable ) )
			  {
					assertEquals( list, stream.collect( toList() ) );
			  }
			  verify( resource ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iterablesStreamCharacteristics()
		 internal virtual void IterablesStreamCharacteristics()
		 {
			  IEnumerable<int> iterable = asList( 1, 2, 3 );
			  int characteristics = Spliterator.DISTINCT | Spliterator.ORDERED | Spliterator.NONNULL;

			  Stream<int> stream = Iterables.Stream( iterable, characteristics );

			  assertEquals( characteristics, stream.spliterator().characteristics() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCachingIterator()
		 internal virtual void TestCachingIterator()
		 {
			  IEnumerator<int> source = new RangeIterator( 8 );
			  CachingIterator<int> caching = new CachingIterator<int>( source );

			  assertThrows( typeof( NoSuchElementException ), caching.previous );
			  assertThrows( typeof( NoSuchElementException ), caching.current );

			  // Next and previous
			  assertEquals( 0, caching.Position() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( caching.HasNext() );
			  assertEquals( 0, caching.Position() );
			  assertFalse( caching.HasPrevious() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 0, caching.Next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( caching.HasNext() );
			  assertTrue( caching.HasPrevious() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 1, caching.Next() );
			  assertTrue( caching.HasPrevious() );
			  assertEquals( ( int? ) 1, caching.Current() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 2, caching.Next() );
			  assertEquals( ( int? ) 2, caching.Current() );
			  assertEquals( ( int? ) 3, ( int? ) caching.Position() );
			  assertEquals( ( int? ) 2, caching.Current() );
			  assertTrue( caching.HasPrevious() );
			  assertEquals( ( int? ) 2, caching.Previous() );
			  assertEquals( ( int? ) 2, caching.Current() );
			  assertEquals( ( int? ) 2, ( int? ) caching.Position() );
			  assertEquals( ( int? ) 1, caching.Previous() );
			  assertEquals( ( int? ) 1, caching.Current() );
			  assertEquals( ( int? ) 1, ( int? ) caching.Position() );
			  assertEquals( ( int? ) 0, caching.Previous() );
			  assertEquals( ( int? ) 0, ( int? ) caching.Position() );
			  assertFalse( caching.HasPrevious() );

			  assertThrows( typeof( System.ArgumentException ), () => caching.Position(-1), "Shouldn't be able to set a lower value than 0" );

			  assertEquals( ( int? ) 0, caching.Current() );
			  assertEquals( 0, caching.Position( 3 ) );

			  assertThrows( typeof( NoSuchElementException ), caching.current, "Shouldn't be able to call current() after a call to position(int)" );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( caching.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 3, caching.Next() );
			  assertEquals( ( int? ) 3, caching.Current() );
			  assertTrue( caching.HasPrevious() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 4, caching.Next() );
			  assertEquals( 5, caching.Position() );
			  assertEquals( ( int? ) 4, caching.Previous() );
			  assertEquals( ( int? ) 4, caching.Current() );
			  assertEquals( ( int? ) 4, caching.Current() );
			  assertEquals( 4, caching.Position() );
			  assertEquals( ( int? ) 3, caching.Previous() );
			  assertEquals( 3, caching.Position() );

			  assertThrows( typeof( NoSuchElementException ), () => caching.Position(9), "Shouldn't be able to set a position which is too big" );

			  assertEquals( 3, caching.Position( 8 ) );
			  assertTrue( caching.HasPrevious() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( caching.HasNext() );

			  assertThrows( typeof( NoSuchElementException ), caching.next, "Shouldn't be able to go beyond last item" );

			  assertEquals( 8, caching.Position() );
			  assertEquals( ( int? ) 7, caching.Previous() );
			  assertEquals( ( int? ) 6, caching.Previous() );
			  assertEquals( 6, caching.Position( 0 ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( ( int? ) 0, caching.Next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testPagingIterator()
		 internal virtual void TestPagingIterator()
		 {
			  IEnumerator<int> source = new RangeIterator( 24 );
			  PagingIterator<int> pager = new PagingIterator<int>( source, 10 );
			  assertEquals( 0, pager.Page() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( pager.HasNext() );
			  AssertPage( pager.NextPage(), 10, 0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( pager.HasNext() );

			  assertEquals( 1, pager.Page() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( pager.HasNext() );
			  AssertPage( pager.NextPage(), 10, 10 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( pager.HasNext() );

			  assertEquals( 2, pager.Page() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( pager.HasNext() );
			  AssertPage( pager.NextPage(), 4, 20 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( pager.HasNext() );

			  pager.Page( 1 );
			  assertEquals( 1, pager.Page() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( pager.HasNext() );
			  AssertPage( pager.NextPage(), 10, 10 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( pager.HasNext() );
		 }

		 private void AssertPage( IEnumerator<int> page, int size, int plus )
		 {
			  for ( int i = 0; i < size; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( page.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( ( int? )( i + plus ), page.next() );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( page.hasNext() );
		 }
	}

}