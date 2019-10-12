using System.Diagnostics;
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
namespace Neo4Net.Kernel.impl.util
{
	public class SynchronizedArrayIdOrderingQueue : IdOrderingQueue
	{
		 private long[] _queue;
		 private int _offerIndex;
		 private int _headIndex;

		 public SynchronizedArrayIdOrderingQueue() : this(20)
		 {
		 }

		 public SynchronizedArrayIdOrderingQueue( int initialMaxSize )
		 {
			  this._queue = new long[initialMaxSize];
		 }

		 public override void Offer( long value )
		 {
			 lock ( this )
			 {
				  if ( _offerIndex - _headIndex >= _queue.Length )
				  {
						ExtendArray();
				  }
				  Debug.Assert( _offerIndex == _headIndex || ( _offerIndex - 1 ) % _queue.Length < value, "Was offered ids out-of-order, " + value + " whereas last offered was " + ( ( _offerIndex - 1 ) % _queue.Length ) );
				  _queue[( _offerIndex++ ) % _queue.Length] = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void waitFor(long value) throws InterruptedException
		 public override void WaitFor( long value )
		 {
			 lock ( this )
			 {
				  while ( _offerIndex == _headIndex || _queue[_headIndex % _queue.Length] != value )
				  {
						Monitor.Wait( this );
				  }
			 }
		 }

		 public override void RemoveChecked( long expectedValue )
		 {
			 lock ( this )
			 {
				  if ( _queue[_headIndex % _queue.Length] != expectedValue )
				  {
						throw new System.InvalidOperationException( "Was about to remove head and expected it to be " + expectedValue + ", but it was " + _queue[_headIndex] );
				  }
				  _headIndex++;
				  Monitor.PulseAll( this );
			 }
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				 lock ( this )
				 {
					  return _offerIndex == _headIndex;
				 }
			 }
		 }

		 private void ExtendArray()
		 {
			  long[] newQueue = new long[_queue.Length << 1];
			  int length = _offerIndex - _headIndex;
			  for ( int i = 0; i < length; i++ )
			  {
					newQueue[i] = _queue[( _headIndex + i ) % _queue.Length];
			  }

			  _queue = newQueue;
			  _offerIndex = length;
			  _headIndex = 0;
		 }
	}

}