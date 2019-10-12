using System;
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
namespace Neo4Net.Cypher.@internal
{

	/// <summary>
	/// The default implementation of a Top N table used by all runtimes
	/// 
	/// It accepts tuples as boxed objects that implements Comparable
	/// 
	/// Implements the following interface:
	/// (since the code is generated it does not actually need to declare it with implements)
	/// 
	/// public interface SortTable<T>
	/// {
	///     boolean add( T e );
	/// 
	///     void sort();
	/// 
	///     Iterator<T> iterator();
	/// }
	/// 
	/// Uses a max heap (Java's standard PriorityQueue) to collect a maximum of totalCount tuples in reverse order.
	/// When sort() is called it collects them in reverse sorted order into an array.
	/// The iterator() then traverses this array backwards.
	/// 
	/// </summary>
	public class DefaultComparatorTopTable<T> : IEnumerable<T> // implements SortTable<T>
	{
		 private readonly IComparer<T> _comparator;
		 private readonly int _totalCount;
		 private int _count = -1;
		 private PriorityQueue<T> _heap;
		 private object[] _array; // TODO: Use Guava's MinMaxPriorityQueue to avoid having this array

		 public DefaultComparatorTopTable( IComparer<T> comparator, int totalCount )
		 {
			  this._comparator = comparator;
			  if ( totalCount <= 0 )
			  {
					throw new System.ArgumentException( "Top table size must be greater than 0" );
			  }
			  this._totalCount = totalCount;

			  _heap = new PriorityQueue<T>( Math.Min( totalCount, 1024 ), comparator.reversed() );
		 }

		 public virtual bool Add( T e )
		 {
			  if ( _heap.size() < _totalCount )
			  {
					return _heap.offer( e );
			  }
			  else
			  {
					T head = _heap.peek();
					if ( _comparator.Compare( head, e ) > 0 )
					{
						 _heap.poll();
						 return _heap.offer( e );
					}
					else
					{
						 return false;
					}
			  }
		 }

		 public virtual void Sort()
		 {
			  _count = _heap.size();
			  _array = new object[_count];

			  // We keep the values in reverse order so that we can write from start to end
			  for ( int i = 0; i < _count; i++ )
			  {
					_array[i] = _heap.poll();
			  }
		 }

		 public override IEnumerator<T> Iterator()
		 {
			  if ( _count == -1 )
			  {
					// This should never happen in generated code but is here to simplify debugging if used incorrectly
					throw new System.InvalidOperationException( "sort() needs to be called before requesting an iterator" );
			  }
			  return new IteratorAnonymousInnerClass( this );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private readonly DefaultComparatorTopTable<T> _outerInstance;

			 public IteratorAnonymousInnerClass( DefaultComparatorTopTable<T> outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 cursor = outerInstance.count;
			 }

			 private int cursor;

			 public bool hasNext()
			 {
				  return cursor > 0;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public T next()
			 public T next()
			 {
				  if ( !hasNext() )
				  {
						throw new NoSuchElementException();
				  }

				  int offset = --cursor;
				  return ( T ) _outerInstance.array[offset];
			 }
		 }
	}

}