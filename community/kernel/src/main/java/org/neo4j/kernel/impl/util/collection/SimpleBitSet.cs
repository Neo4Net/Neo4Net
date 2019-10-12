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
namespace Org.Neo4j.Kernel.impl.util.collection
{
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;


	/// <summary>
	/// A basic bitset.
	/// <para>
	/// Represented using an automatically expanding long array, with one bit per key.
	/// </para>
	/// <para>
	/// Performance:
	/// * put, remove, contains, size: O(1)
	/// * clear: O(size/64)
	/// </para>
	/// <para>
	/// Concurrency semantics:
	/// * Concurrent writes synchronise and is thread-safe
	/// * Concurrent reads are thread-safe and will not observe torn writes, but may become
	/// out of date as soon as the operation returns
	/// * Concurrent reads during write is thread-safe
	/// * Bulk operations appear atomic to concurrent readers
	/// * Only caveat being that the iterator is not thread-safe
	/// </para>
	/// </summary>
	public class SimpleBitSet : StampedLock
	{
		 private long _lastCheckPointKey;
		 private long[] _data;

		 public SimpleBitSet( int size )
		 {
			  int initialCapacity = size / 64;
			  int capacity = 1;
			  while ( capacity < initialCapacity )
			  {
					capacity <<= 1;
			  }
			  long stamp = writeLock();
			  _data = new long[capacity];
			  unlockWrite( stamp );
		 }

		 public virtual bool Contains( int key )
		 {
			  int idx = ( int )( ( uint )key >> 6 );
			  bool result;
			  long stamp;
			  do
			  {
					stamp = tryOptimisticRead();
					result = _data.Length > idx && ( _data[idx] & ( ( 1L << ( key & 63 ) ) ) ) != 0;
			  } while ( !validate( stamp ) );
			  return result;
		 }

		 public virtual void Put( int key )
		 {
			  long stamp = writeLock();
			  int idx = ( int )( ( uint )key >> 6 );
			  EnsureCapacity( idx );
			  _data[idx] = _data[idx] | ( 1L << ( key & 63 ) );
			  unlockWrite( stamp );
		 }

		 public virtual void Put( SimpleBitSet other )
		 {
			  long stamp = writeLock();
			  EnsureCapacity( other._data.Length - 1 );
			  for ( int i = 0; i < _data.Length && i < other._data.Length; i++ )
			  {
					_data[i] = _data[i] | other._data[i];
			  }
			  unlockWrite( stamp );
		 }

		 public virtual void Remove( int key )
		 {
			  long stamp = writeLock();
			  int idx = ( int )( ( uint )key >> 6 );
			  if ( _data.Length > idx )
			  {
					_data[idx] = _data[idx] & ~( 1L << ( key & 63 ) );
			  }
			  unlockWrite( stamp );
		 }

		 public virtual void Remove( SimpleBitSet other )
		 {
			  long stamp = writeLock();
			  for ( int i = 0; i < _data.Length; i++ )
			  {
					_data[i] = _data[i] & ~other._data[i];
			  }
			  unlockWrite( stamp );
		 }

		 public virtual long CheckPointAndPut( long checkPoint, int key )
		 {
			  // We only need to clear the bit set if it was modified since the last check point
			  if ( !validate( checkPoint ) || key != _lastCheckPointKey )
			  {
					long stamp = writeLock();
					int idx = ( int )( ( uint )key >> 6 );
					if ( idx < _data.Length )
					{
						 Arrays.fill( _data, 0 );
					}
					else
					{
						 int len = _data.Length;
						 len = FindNewLength( idx, len );
						 _data = new long[len];
					}
					_data[idx] = _data[idx] | ( 1L << ( key & 63 ) );
					_lastCheckPointKey = key;
					checkPoint = tryConvertToOptimisticRead( stamp );
			  }
			  return checkPoint;
		 }

		 private int FindNewLength( int idx, int len )
		 {
			  while ( len <= idx )
			  {
					len *= 2;
			  }
			  return len;
		 }

		 public virtual int Size()
		 {
			  int size = 0;
			  foreach ( long s in _data )
			  {
					size += Long.bitCount( s );
			  }
			  return size;
		 }

		 private void EnsureCapacity( int arrayIndex )
		 {
			  _data = Arrays.copyOf( _data, FindNewLength( arrayIndex, _data.Length ) );
		 }

		 //
		 // Views
		 //

		 public virtual IntIterator Iterator()
		 {
			  return new IntIteratorAnonymousInnerClass( this );
		 }

		 private class IntIteratorAnonymousInnerClass : IntIterator
		 {
			 private readonly SimpleBitSet _outerInstance;

			 public IntIteratorAnonymousInnerClass( SimpleBitSet outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 size = outerInstance.data.Length * 64;

				 // Prefetch first
				 while ( next < size && !outerInstance.Contains( next ) )
				 {
				 next++;
				 }
			 }

			 private int next;
			 private readonly int size;

			 public override bool hasNext()
			 {
				  return next < size;
			 }

			 public override int next()
			 {
				  int current = next;
				  next++;
				  while ( next < size && !outerInstance.Contains( next ) )
				  {
						next++;
				  }
				  return current;
			 }
		 }
	}

}