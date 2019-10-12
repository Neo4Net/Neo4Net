using System;
using System.Collections.Concurrent;
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
namespace Neo4Net.Util.concurrent
{

	/// <summary>
	/// Tracks an (approximate) set of recently seen unique elements in a stream, based on a concurrent LRU implementation.
	/// 
	/// This class is thread safe. For the common case of items that are recently seen being seen again, this class is
	/// lock free.
	/// </summary>
	/// @param <Type> the entry type stored </param>
	public class RecentK<Type> : IEnumerable<Type>
	{
		 private readonly int _maxItems;

		 /// <summary>
		 /// Source of truth - the keys in this map are the "recent set". For each value, we also track a counter for
		 /// the number of times we've seen it, which is used to evict older and less used values.
		 /// </summary>
		 private readonly ConcurrentDictionary<Type, AtomicLong> _recentItems = new ConcurrentDictionary<Type, AtomicLong>();

		 /// <param name="maxItems"> is the size of the item set to track </param>
		 public RecentK( int maxItems )
		 {
			  this._maxItems = maxItems;
		 }

		 /// <param name="item"> a new item to the tracked set. </param>
		 public virtual void Add( Type item )
		 {
			  AtomicLong counter = _recentItems[item];
			  if ( counter != null )
			  {
					counter.incrementAndGet();
			  }
			  else
			  {
					// Double-checked locking ahead: Check if there is space for our item
					if ( _recentItems.Count >= _maxItems )
					{
						 // If not, synchronize and check again (this will happen if there is > maxItems in the current set)
						 lock ( _recentItems )
						 {
							  // Proper check under lock, make space in the set for our new item
							  while ( _recentItems.Count >= _maxItems )
							  {
									RemoveItemWithLowestCount();
							  }

							  HalveCounts();
							  _recentItems.GetOrAdd( item, new AtomicLong( 1 ) );
						 }
					}
					else
					{
						 // There were < maxItems in the set. This is racy as multiple clients may have hit this branch
						 // simultaneously. We accept going above max items here. The set will recover next time an item
						 // is added, since the synchronized block above will bring the set to maxItems items again.
						 _recentItems.GetOrAdd( item, new AtomicLong( 1 ) );
					}
			  }

		 }

		 /// <summary>
		 /// In order to give lower-and-lower priority to keys we've seen a lot in the past, but don't see much anymore,
		 /// we cut all key counts in half after we've run an eviction cycle.
		 /// </summary>
		 private void HalveCounts()
		 {
			  foreach ( AtomicLong count in _recentItems.Values )
			  {
					long prev;
					long next;
					do
					{
						 prev = count.get();
						 next = Math.Max( prev / 2, 1 );
					} while ( !count.compareAndSet( prev, next ) );

			  }
		 }

		 private void RemoveItemWithLowestCount()
		 {
			  Type lowestCountKey = default( Type );
			  long lowestCount = long.MaxValue;
			  foreach ( KeyValuePair<Type, AtomicLong> entry in _recentItems.SetOfKeyValuePairs() )
			  {
					long currentCount = entry.Value.get();
					if ( currentCount < lowestCount )
					{
						 lowestCount = currentCount;
						 lowestCountKey = entry.Key;
					}
			  }

			  if ( lowestCountKey != default( Type ) )
			  {
					_recentItems.Remove( lowestCountKey );
			  }
		 }

		 public virtual ISet<Type> RecentItems()
		 {
			  return _recentItems.Keys;
		 }

		 public override IEnumerator<Type> Iterator()
		 {
			  return RecentItems().GetEnumerator();
		 }
	}

}