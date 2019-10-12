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
namespace Neo4Net.@internal.Collector
{

	/// <summary>
	/// Implementation of <seealso cref="RecentBuffer"/> using <seealso cref="ConcurrentLinkedQueue"/>.
	/// </summary>
	public class ConcurrentLinkedQueueRecentBuffer<T> : RecentBuffer<T>
	{
		 private readonly ConcurrentLinkedQueue<T> _queue;
		 private readonly int _maxSize;
		 private readonly AtomicInteger _size;

		 public ConcurrentLinkedQueueRecentBuffer( int bitSize )
		 {
			  _maxSize = 1 << bitSize;
			  _queue = new ConcurrentLinkedQueue<T>();
			  _size = new AtomicInteger( 0 );
		 }

		 /* ---- many producers ---- */

		 public override void Produce( T t )
		 {
			  _queue.add( t );
			  int newSize = _size.incrementAndGet();
			  if ( newSize > _maxSize )
			  {
					_queue.poll();
					_size.decrementAndGet();
			  }
		 }

		 /* ---- single consumer ---- */

		 public override void Clear()
		 {
			  _queue.clear();
			  // might go out of sync with queue here, but should be minor slippage.
			  // Will not accumulate leaks either, but reset on every clear.
			  _size.set( 0 );
		 }

		 public override void Foreach( System.Action<T> consumer )
		 {
			  _queue.forEach( consumer );
		 }
	}

}