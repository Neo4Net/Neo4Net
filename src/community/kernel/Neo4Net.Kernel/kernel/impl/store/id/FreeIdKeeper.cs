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

	using PrimitiveLongArrayQueue = Neo4Net.Collections.PrimitiveLongArrayQueue;
	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdContainer.NO_RESULT;

	/// <summary>
	/// Instances of this class maintain a list of free ids with the potential of overflowing to disk if the number
	/// of free ids becomes too large. This class has no expectations and makes no assertions as to the ids freed.
	/// Such consistency guarantees, for example uniqueness of values, should be imposed from users of this class.
	/// <para>
	/// There is no guarantee as to the ordering of the values returned (i.e. FIFO, LIFO or any other temporal strategy),
	/// primarily because the aggressiveMode argument influences exactly that behaviour.
	/// </para>
	/// <para>
	/// The <seealso cref="aggressiveMode"/> parameter controls whether or not IDs which are freed during this lifecycle will
	/// be allowed to be reused during the same lifecycle. The alternative non-aggressive behaviour is that the IDs
	/// will only be reused after a close/open cycle. This would generally correlate with a restart of the database.
	/// </para>
	/// </summary>
	public class FreeIdKeeper : System.IDisposable
	{
		 private static readonly int _idEntrySize = Long.BYTES;

		 private readonly PrimitiveLongArrayQueue _freeIds = new PrimitiveLongArrayQueue();
		 private readonly PrimitiveLongArrayQueue _readFromDisk = new PrimitiveLongArrayQueue();
		 private readonly StoreChannel _channel;
		 private readonly int _batchSize;
		 private readonly bool _aggressiveMode;

		 private long _freeIdCount;

		 /// <summary>
		 /// Keeps the position where batches of IDs will be flushed out to.
		 /// This can be viewed as being put on top of a stack.
		 /// </summary>
		 private long _stackPosition;

		 /// <summary>
		 /// The position before we started this run.
		 /// <para>
		 /// Useful to keep track of the gap that will form in non-aggressive mode
		 /// when IDs from old runs get reused and newly freed IDs are put on top
		 /// of the stack. During a clean shutdown the gap will be compacted away.
		 /// </para>
		 /// <para>
		 /// During an aggressive run a gap is never formed since batches of free
		 /// IDs are flushed on top of the stack (end of file) and also read in
		 /// from the top of the stack.
		 /// </para>
		 /// </summary>
		 private long _initialPosition;

		 /// <summary>
		 /// A keeper of freed IDs.
		 /// </summary>
		 /// <param name="channel"> a channel to the free ID file. </param>
		 /// <param name="batchSize"> the number of IDs which are read/written to disk in one go. </param>
		 /// <param name="aggressiveMode"> whether to reuse freed IDs during this lifecycle. </param>
		 /// <exception cref="IOException"> if an I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FreeIdKeeper(org.Neo4Net.io.fs.StoreChannel channel, int batchSize, boolean aggressiveMode) throws java.io.IOException
		 internal FreeIdKeeper( StoreChannel channel, int batchSize, bool aggressiveMode )
		 {
			  this._channel = channel;
			  this._batchSize = batchSize;
			  this._aggressiveMode = aggressiveMode;

			  this._initialPosition = channel.size();
			  this._stackPosition = _initialPosition;
			  this._freeIdCount = _stackPosition / _idEntrySize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static long countFreeIds(org.Neo4Net.io.fs.StoreChannel channel) throws java.io.IOException
		 internal static long CountFreeIds( StoreChannel channel )
		 {
			  return channel.size() / _idEntrySize;
		 }

		 public virtual void FreeId( long id )
		 {
			  _freeIds.enqueue( id );
			  _freeIdCount++;

			  if ( _freeIds.size() >= _batchSize )
			  {
					long endPosition = FlushFreeIds( ByteBuffer.allocate( _batchSize * _idEntrySize ) );
					if ( _aggressiveMode )
					{
						 _stackPosition = endPosition;
					}
			  }
		 }

		 private void Truncate( long position )
		 {
			  try
			  {
					_channel.truncate( position );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Failed to truncate", e );
			  }
		 }

		 public virtual long Id
		 {
			 get
			 {
				  long result;
				  if ( _freeIds.size() > 0 && _aggressiveMode )
				  {
						result = _freeIds.dequeue();
						_freeIdCount--;
				  }
				  else
				  {
						result = IdFromDisk;
						if ( result != NO_RESULT )
						{
							 _freeIdCount--;
						}
				  }
				  return result;
			 }
		 }

		 public virtual long[] GetIds( int numberOfIds )
		 {
			  if ( _freeIdCount == 0 )
			  {
					return PrimitiveLongCollections.EMPTY_LONG_ARRAY;
			  }
			  int reusableIds = ( int ) min( numberOfIds, _freeIdCount );
			  long[] ids = new long[reusableIds];
			  int cursor = 0;
			  while ( ( cursor < reusableIds ) && !_freeIds.Empty )
			  {
					ids[cursor++] = _freeIds.dequeue();
			  }
			  while ( cursor < reusableIds )
			  {
					ids[cursor++] = IdFromDisk;
			  }
			  _freeIdCount -= reusableIds;
			  return ids;
		 }

		 private long IdFromDisk
		 {
			 get
			 {
				  if ( _readFromDisk.Empty )
				  {
						ReadIdBatch();
				  }
				  if ( !_readFromDisk.Empty )
				  {
						return _readFromDisk.dequeue();
				  }
				  else
				  {
						return NO_RESULT;
				  }
			 }
		 }

		 public virtual long Count
		 {
			 get
			 {
				  return _freeIdCount;
			 }
		 }

		 /*
		  * After this method returns, if there were any entries found, they are placed in the readFromDisk list.
		  */
		 private void ReadIdBatch()
		 {
			  try
			  {
					ReadIdBatch0();
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Failed reading free id batch", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readIdBatch0() throws java.io.IOException
		 private void ReadIdBatch0()
		 {
			  if ( _stackPosition == 0 )
			  {
					return;
			  }

			  long startPosition = max( _stackPosition - _batchSize * _idEntrySize, 0 );
			  int bytesToRead = toIntExact( _stackPosition - startPosition );
			  ByteBuffer readBuffer = ByteBuffer.allocate( bytesToRead );

			  _channel.position( startPosition );
			  _channel.readAll( readBuffer );
			  _stackPosition = startPosition;

			  readBuffer.flip();
			  int idsRead = bytesToRead / _idEntrySize;
			  for ( int i = 0; i < idsRead; i++ )
			  {
					long id = readBuffer.Long;
					_readFromDisk.enqueue( id );
			  }
			  if ( _aggressiveMode )
			  {
					Truncate( startPosition );
			  }
		 }

		 /// <summary>
		 /// Flushes the currently collected in-memory freed IDs to the storage.
		 /// </summary>
		 private long FlushFreeIds( ByteBuffer writeBuffer )
		 {
			  try
			  {
					return FlushFreeIds0( writeBuffer );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Unable to write free id batch", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long flushFreeIds0(ByteBuffer writeBuffer) throws java.io.IOException
		 private long FlushFreeIds0( ByteBuffer writeBuffer )
		 {
			  _channel.position( _channel.size() );
			  writeBuffer.clear();
			  while ( !_freeIds.Empty )
			  {
					long id = _freeIds.dequeue();
					if ( id == NO_RESULT )
					{
						 continue;
					}
					writeBuffer.putLong( id );
					if ( writeBuffer.position() == writeBuffer.capacity() )
					{
						 writeBuffer.flip();
						 _channel.writeAll( writeBuffer );
						 writeBuffer.clear();
					}
			  }
			  writeBuffer.flip();
			  if ( writeBuffer.hasRemaining() )
			  {
					_channel.writeAll( writeBuffer );
			  }
			  return _channel.position();
		 }

		 /*
		  * Writes both freeIds and readFromDisk lists to disk and truncates the channel to size.
		  * It forces but does not close the channel.
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  ByteBuffer writeBuffer = ByteBuffer.allocate( _batchSize * _idEntrySize );
			  FlushFreeIds( writeBuffer );
			  _freeIds.addAll( _readFromDisk );
			  FlushFreeIds( writeBuffer );
			  if ( !_aggressiveMode )
			  {
					Compact( writeBuffer );
			  }
			  _channel.force( false );
		 }

		 /// <summary>
		 /// Compacts away the gap which will form in non-aggressive (regular) mode
		 /// when batches are read in from disk.
		 /// <para>
		 /// The gap will contain already used IDs so it is important to remove it
		 /// on a clean shutdown. The freed IDs will not be reused after an
		 /// unclean shutdown, as guaranteed by the external user.
		 /// <pre>
		 /// Below diagram tries to explain the situation
		 /// 
		 ///   S = old IDs which are still free (on the Stack)
		 ///   G = the Gap which has formed, due to consuming old IDs
		 ///   N = the New IDs which have been freed during this run (will be compacted to the left)
		 /// 
		 ///     stackPosition
		 ///          v
		 /// [ S S S S G G G N N N N N N N N ]
		 ///                ^
		 ///          initialPosition
		 /// </pre>
		 /// After compaction the state will be:
		 /// <pre>
		 /// [ S S S S N N N N N N N N ]
		 /// </pre>
		 /// and the last part of the file is truncated.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void compact(ByteBuffer writeBuffer) throws java.io.IOException
		 private void Compact( ByteBuffer writeBuffer )
		 {
			  Debug.Assert( _stackPosition <= _initialPosition ); // the stack can only be consumed in regular mode
			  if ( _initialPosition == _stackPosition )
			  {
					// there is no compaction to be done
					return;
			  }

			  long writePosition = _stackPosition;
			  long readPosition = _initialPosition; // readPosition to end of file contain new free IDs, to be compacted
			  int nBytes;
			  do
			  {
					writeBuffer.clear();
					_channel.position( readPosition );
					nBytes = _channel.read( writeBuffer );

					if ( nBytes > 0 )
					{
						 readPosition += nBytes;

						 writeBuffer.flip();
						 _channel.position( writePosition );
						 _channel.writeAll( writeBuffer );
						 writePosition += nBytes;
					}
			  } while ( nBytes > 0 );

			  _channel.truncate( writePosition );
		 }
	}

}