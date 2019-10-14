﻿using System;

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
namespace Neo4Net.Collections
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	/// <summary>
	/// Simple array based FIFO queue for primitive longs.
	/// Newly enqueued element is added into the end of the queue, and dequeue will return
	/// element from the head of the queue. (See CLRS 10.1 for more detailed description)
	/// 
	/// Queue capacity should always be power of two to be able to use
	/// '&' mask operation with <seealso cref="values"/> length.
	/// </summary>
	public class PrimitiveLongArrayQueue
	{
		 private const int DEFAULT_CAPACITY = 16;
		 private long[] _values;
		 private int _head;
		 private int _tail;

		 public PrimitiveLongArrayQueue() : this(DEFAULT_CAPACITY)
		 {
		 }

		 internal PrimitiveLongArrayQueue( int capacity )
		 {
			  if ( capacity == 0 || ( capacity & ( capacity - 1 ) ) != 0 )
			  {
					throw new System.ArgumentException( "Capacity should be power of 2. Requested capacity: " + capacity );
			  }
			  InitValues( capacity );
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return _head == _tail;
			 }
		 }

		 public virtual void Clear()
		 {
			  InitValues( DEFAULT_CAPACITY );
		 }

		 public virtual int Size()
		 {
			  return ( _tail - _head ) & ( _values.Length - 1 );
		 }

		 public virtual LongIterator LongIterator()
		 {
			  return new PrimitiveLongArrayQueueIterator( this );
		 }

		 public virtual long Dequeue()
		 {
			  if ( Empty )
			  {
					throw new System.InvalidOperationException( "Fail to poll first element. Queue is empty." );
			  }
			  long value = _values[_head];
			  _head = ( _head + 1 ) & ( _values.Length - 1 );
			  return value;
		 }

		 public virtual void Enqueue( long value )
		 {
			  _values[_tail] = value;
			  _tail = ( _tail + 1 ) & ( _values.Length - 1 );
			  if ( _tail == _head )
			  {
					EnsureCapacity();
			  }
		 }

		 public virtual void AddAll( PrimitiveLongArrayQueue otherQueue )
		 {
			  while ( !otherQueue.Empty )
			  {
					Enqueue( otherQueue.Dequeue() );
			  }
		 }

		 private void InitValues( int capacity )
		 {
			  _values = new long[capacity];
			  _head = 0;
			  _tail = 0;
		 }

		 private void EnsureCapacity()
		 {
			  int newCapacity = _values.Length << 1;
			  if ( newCapacity < 0 )
			  {
					throw new System.InvalidOperationException( "Fail to increase queue capacity." );
			  }
			  long[] newValues = new long[newCapacity];
			  int elementsFromHeadTillEnd = _values.Length - _head;
			  Array.Copy( _values, _head, newValues, 0, elementsFromHeadTillEnd );
			  Array.Copy( _values, 0, newValues, elementsFromHeadTillEnd, _head );
			  _tail = _values.Length;
			  _head = 0;
			  _values = newValues;
		 }

		 private class PrimitiveLongArrayQueueIterator : LongIterator
		 {
			 private readonly PrimitiveLongArrayQueue _outerInstance;

			  internal int Position;

			  internal PrimitiveLongArrayQueueIterator( PrimitiveLongArrayQueue outerInstance )
			  {
				  this._outerInstance = outerInstance;
					this.Position = outerInstance.head;
			  }

			  public override bool HasNext()
			  {
					return Position != outerInstance.tail;
			  }

			  public override long Next()
			  {
					if ( HasNext() )
					{
						 long value = outerInstance.values[Position];
						 Position = ( Position + 1 ) & ( outerInstance.values.Length - 1 );
						 return value;
					}
					throw new NoSuchElementException();
			  }
		 }
	}

}