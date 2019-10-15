using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.store.id
{

	/// <summary>
	/// Buffer of {@code long} values which can be held until a safe threshold is crossed, at which point
	/// they are released onto a <seealso cref="Consumer"/>. These values are held in chunk and each chunk knows
	/// which threshold is considered safe. Regular <seealso cref="maintenance()"/> is required to be called externally
	/// for values to be released.
	/// 
	/// This class is thread-safe for concurrent requests, but only a single thread should be responsible for
	/// calling <seealso cref="maintenance()"/>.
	/// </summary>
	public class DelayedBuffer<T>
	{
		 private class Chunk<T>
		 {
			  internal readonly T Threshold;
			  internal readonly long[] Values;

			  internal Chunk( T threshold, long[] values )
			  {
					this.Threshold = threshold;
					this.Values = values;
			  }

			  public override string ToString()
			  {
					return Arrays.ToString( Values );
			  }
		 }

		 private readonly System.Func<T> _thresholdSupplier;
		 private readonly System.Predicate<T> _safeThreshold;
		 private readonly System.Action<long[]> _chunkConsumer;
		 private readonly Deque<Chunk<T>> _chunks = new LinkedList<Chunk<T>>();
		 private readonly int _chunkSize;

		 private readonly long[] _chunk;
		 private int _chunkCursor;

		 public DelayedBuffer( System.Func<T> thresholdSupplier, System.Predicate<T> safeThreshold, int chunkSize, System.Action<long[]> chunkConsumer )
		 {
			  Debug.Assert( chunkSize > 0 );
			  this._thresholdSupplier = thresholdSupplier;
			  this._safeThreshold = safeThreshold;
			  this._chunkSize = chunkSize;
			  this._chunkConsumer = chunkConsumer;
			  this._chunk = new long[chunkSize];
		 }

		 /// <summary>
		 /// Should be called every now and then to check for safe thresholds of buffered chunks and potentially
		 /// release them onto the <seealso cref="Consumer"/>.
		 /// </summary>
		 public virtual void Maintenance()
		 {
			  lock ( this )
			  {
					Flush();
			  }

			  if ( !_chunks.Empty )
			  {
					lock ( _chunks )
					{
						 // Potentially hand over chunks to the consumer
						 while ( !_chunks.Empty )
						 {
							  Chunk<T> candidate = _chunks.peek();
							  if ( _safeThreshold.test( candidate.Threshold ) )
							  {
									_chunkConsumer.accept( candidate.Values );
									_chunks.remove();
							  }
							  else
							  {
									// The chunks are ordered by chunkThreshold, so we know that no more chunks will qualify anyway
									break;
							  }
						 }
					}
			  }
		 }

		 // Must be called under synchronized on this
		 private void Flush()
		 {
			  if ( _chunkCursor > 0 )
			  {
					lock ( _chunks )
					{
						 Chunk<T> chunkToAdd = new Chunk<T>( _thresholdSupplier.get(), copyOf(_chunk, _chunkCursor) );
						 _chunks.offer( chunkToAdd );
					}
					_chunkCursor = 0;
			  }
		 }

		 /// <summary>
		 /// Offers a value to this buffer. This value will at a later point be part of a buffered chunk,
		 /// released by a call to <seealso cref="maintenance()"/> when the safe threshold for the chunk, which is determined
		 /// when the chunk is full or otherwise queued.
		 /// </summary>
		 public virtual void Offer( long value )
		 {
			 lock ( this )
			 {
				  _chunk[_chunkCursor++] = value;
				  if ( _chunkCursor == _chunkSize )
				  {
						Flush();
				  }
			 }
		 }

		 /// <summary>
		 /// Closes this buffer, releasing all <seealso cref="offer(long)"/> values into the <seealso cref="Consumer"/>.
		 /// 
		 /// This class is typically not used in a scenario suitable for try-with-resource
		 /// and so having it implement IDisposable would be more annoying
		 /// </summary>
		 public virtual void Close()
		 {
			 lock ( this )
			 {
				  Flush();
				  while ( !_chunks.Empty )
				  {
						_chunkConsumer.accept( _chunks.poll().values );
				  }
			 }
		 }

		 public virtual void Clear()
		 {
			 lock ( this )
			 {
				  _chunks.clear();
				  _chunkCursor = 0;
			 }
		 }
	}

}