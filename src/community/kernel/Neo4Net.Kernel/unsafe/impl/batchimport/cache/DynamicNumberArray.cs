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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

	/// <summary>
	/// Base class for common functionality for any <seealso cref="NumberArray"/> where the data is dynamically growing,
	/// where parts can live inside and parts off-heap.
	/// </summary>
	/// <seealso cref= NumberArrayFactory#newDynamicLongArray(long, long) </seealso>
	/// <seealso cref= NumberArrayFactory#newDynamicIntArray(long, int) </seealso>
	internal abstract class DynamicNumberArray<N> : NumberArray<N> where N : NumberArray<N>
	{
		public abstract void Swap( long fromIndex, long toIndex );
		 protected internal readonly NumberArrayFactory Factory;
		 protected internal readonly long ChunkSize;
		 protected internal N[] Chunks;

		 internal DynamicNumberArray( NumberArrayFactory factory, long chunkSize, N[] initialChunks )
		 {
			  this.Factory = factory;
			  this.ChunkSize = chunkSize;
			  this.Chunks = initialChunks;
		 }

		 public override long Length()
		 {
			  return Chunks.Length * ChunkSize;
		 }

		 public override void Clear()
		 {
			  foreach ( N chunk in Chunks )
			  {
					chunk.clear();
			  }
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  foreach ( N chunk in Chunks )
			  {
					chunk.acceptMemoryStatsVisitor( visitor );
			  }
		 }

		 protected internal virtual N ChunkOrNullAt( long index )
		 {
			  int chunkIndex = chunkIndex( index );
			  return chunkIndex < Chunks.Length ? Chunks[chunkIndex] : default( N );
		 }

		 protected internal virtual int ChunkIndex( long index )
		 {
			  return ( int )( index / ChunkSize );
		 }

		 public override N At( long index )
		 {
			  if ( index >= Length() )
			  {
					SynchronizedAddChunk( index );
			  }

			  int chunkIndex = chunkIndex( index );
			  return Chunks[chunkIndex];
		 }

		 private void SynchronizedAddChunk( long index )
		 {
			 lock ( this )
			 {
				  if ( index >= Length() )
				  {
						N[] newChunks = Arrays.copyOf( Chunks, ChunkIndex( index ) + 1 );
						for ( int i = Chunks.Length; i < newChunks.Length; i++ )
						{
							 newChunks[i] = AddChunk( ChunkSize, ChunkSize * i );
						}
						Chunks = newChunks;
				  }
			 }
		 }

		 protected internal abstract N AddChunk( long chunkSize, long @base );

		 public override void Close()
		 {
			  foreach ( N chunk in Chunks )
			  {
					chunk.close();
			  }
		 }
	}

}