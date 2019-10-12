using System;
using System.Threading;

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

	using Preconditions = Neo4Net.Util.Preconditions;

	/// <summary>
	/// Implementation of <seealso cref="RecentBuffer"/> using ring buffer.
	/// </summary>
	public class RingRecentBuffer<T> : RecentBuffer<T>
	{
		 private readonly int _size;
		 private readonly int _mask;
		 private readonly VolatileRef<T>[] _data;

		 private readonly AtomicLong _produceCount;
		 private readonly AtomicLong _consumeCount;
		 private readonly AtomicLong _dropEvents;

		 public RingRecentBuffer( int size )
		 {
			  if ( size > 0 )
			  {
					Preconditions.requirePowerOfTwo( size );
			  }

			  this._size = size;
			  _mask = size - 1;

			  //noinspection unchecked
			  _data = new VolatileRef[size];
			  for ( int i = 0; i < size; i++ )
			  {
					_data[i] = new VolatileRef<T>();
					_data[i].produceNumber = i - size;
			  }

			  _produceCount = new AtomicLong( 0 );
			  _consumeCount = new AtomicLong( 0 );
			  _dropEvents = new AtomicLong( 0 );
		 }

		 internal virtual long NumSilentQueryDrops()
		 {
			  return _dropEvents.get();
		 }

		 /* ---- many producers ---- */

		 public override void Produce( T t )
		 {
			  if ( _size == 0 )
			  {
					return;
			  }

			  long produceNumber = _produceCount.AndIncrement;
			  int offset = ( int )( produceNumber & _mask );
			  VolatileRef<T> volatileRef = _data[offset];
			  if ( AssertPreviousCompleted( produceNumber, volatileRef ) )
			  {
					volatileRef.Ref = t;
					volatileRef.ProduceNumber = produceNumber;
			  }
			  else
			  {
					// If we don't manage to wait for the previous produce to complete even after
					// all the yields in `assertPreviousCompleted`, we drop `t` to avoid causing
					// a problem in db operation. We increment dropEvents to so the RecentBuffer
					// consumer can detect that there has been a drop.
					_dropEvents.incrementAndGet();
			  }
		 }

		 private bool AssertPreviousCompleted( long produceNumber, VolatileRef<T> volatileRef )
		 {
			  int attempts = 100;
			  long prevProduceNumber = volatileRef.ProduceNumber;
			  while ( prevProduceNumber != produceNumber - _size && attempts > 0 )
			  {
					// Coming in here is expected to be very rare, because it means that producers have
					// circled around the ring buffer, and the producer `size` elements ago hasn't finished
					// writing to the buffer. We yield and hope the previous produce is done when we get back.
					try
					{
						 Thread.Sleep( 0, 1000 );
					}
					catch ( InterruptedException )
					{
						 // continue
					}
					prevProduceNumber = volatileRef.ProduceNumber;
					attempts--;
			  }
			  return attempts > 0;
		 }

		 /* ---- single consumer ---- */

		 public override void Clear()
		 {
			  if ( _size == 0 )
			  {
					return;
			  }

			  foreach ( VolatileRef<T> volatileRef in _data )
			  {
					volatileRef.Ref = default( T );
			  }
			  long snapshotProduce = _produceCount.get();
			  _consumeCount.set( snapshotProduce );
		 }

		 public override void Foreach( System.Action<T> consumer )
		 {
			  if ( _size == 0 )
			  {
					return;
			  }

			  long snapshotProduce = _produceCount.get();
			  long snapshotConsume = Math.Max( _consumeCount.get(), snapshotProduce - _size );
			  for ( long i = snapshotConsume; i < snapshotProduce; i++ )
			  {
					int offset = ( int )( i & _mask );
					VolatileRef<T> volatileRef = _data[offset];
					if ( volatileRef.ProduceNumber < i )
					{
						 return;
					}
					consumer( volatileRef.Ref );
			  }
		 }

		 private class VolatileRef<T>
		 {
			  internal volatile T Ref;
			  internal volatile long ProduceNumber;
		 }
	}

}