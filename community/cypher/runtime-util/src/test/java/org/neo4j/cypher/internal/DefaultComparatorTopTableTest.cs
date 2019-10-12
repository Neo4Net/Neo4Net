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
namespace Org.Neo4j.Cypher.@internal
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class DefaultComparatorTopTableTest
	{
		 private static long?[] _testValues = new long?[]{ 7L, 4L, 5L, 0L, 3L, 4L, 8L, 6L, 1L, 9L, 2L };

		 private static long[] _expectedValues = new long[]{ 0L, 1L, 2L, 3L, 4L, 4L, 5L, 6L, 7L, 8L, 9L };

		 private static readonly IComparer<long> _comparator = long?.compare;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleAddingMoreValuesThanCapacity()
		 public virtual void ShouldHandleAddingMoreValuesThanCapacity()
		 {
			  DefaultComparatorTopTable<long> table = new DefaultComparatorTopTable<long>( _comparator, 7 );
			  foreach ( long? i in _testValues )
			  {
					table.Add( i.Value );
			  }

			  table.Sort();

			  IEnumerator<long> iterator = table.GetEnumerator();

			  for ( int i = 0; i < 7; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					long value = iterator.next();
					assertEquals( _expectedValues[i], value );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWhenNotCompletelyFilledToCapacity()
		 public virtual void ShouldHandleWhenNotCompletelyFilledToCapacity()
		 {
			  DefaultComparatorTopTable<long> table = new DefaultComparatorTopTable<long>( _comparator, 20 );
			  foreach ( long? i in _testValues )
			  {
					table.Add( i.Value );
			  }

			  table.Sort();

			  IEnumerator<long> iterator = table.GetEnumerator();

			  for ( int i = 0; i < _testValues.Length; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					long value = iterator.next();
					assertEquals( _expectedValues[i], value );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWhenEmpty()
		 public virtual void ShouldHandleWhenEmpty()
		 {
			  DefaultComparatorTopTable<long> table = new DefaultComparatorTopTable<long>( _comparator, 10 );

			  table.Sort();

			  IEnumerator<long> iterator = table.GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnInitializeToZeroCapacity()
		 public virtual void ShouldThrowOnInitializeToZeroCapacity()
		 {
			  Exception.expect( typeof( System.ArgumentException ) );
			  new DefaultComparatorTopTable<>( _comparator, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnInitializeToNegativeCapacity()
		 public virtual void ShouldThrowOnInitializeToNegativeCapacity()
		 {
			  Exception.expect( typeof( System.ArgumentException ) );
			  new DefaultComparatorTopTable<>( _comparator, -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnSortNotCalledBeforeIterator()
		 public virtual void ShouldThrowOnSortNotCalledBeforeIterator()
		 {
			  DefaultComparatorTopTable<long> table = new DefaultComparatorTopTable<long>( _comparator, 5 );
			  foreach ( long? i in _testValues )
			  {
					table.Add( i.Value );
			  }

			  // We forgot to call sort() here...

			  Exception.expect( typeof( System.InvalidOperationException ) );
			  table.GetEnumerator();
		 }
	}

}