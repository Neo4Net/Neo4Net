using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.messaging
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;


	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;

	/// <summary>
	/// Uses provided allocator to create <seealso cref="ByteBuf"/>. The buffers will be split if maximum size is reached. The full buffer is then added
	/// to the provided output and a new buffer is allocated. If the output queue is bounded then writing to this channel may block!
	/// </summary>
	public class ChunkingNetworkChannel : WritableChannel, IDisposable
	{
		 private const int DEFAULT_INIT_CHUNK_SIZE = 512;
		 private readonly ByteBufAllocator _allocator;
		 private readonly int _maxChunkSize;
		 private readonly int _initSize;
		 private readonly LinkedList<ByteBuf> _byteBufs;
		 private ByteBuf _current;
		 private bool _isClosed;

		 /// <param name="allocator"> used to allocated <seealso cref="ByteBuf"/> </param> </param>
		 /// <param name="maxChunkSize"> when reached the current buffer will be moved to the <param name="outputQueue"> and a new <seealso cref="ByteBuf"/> is allocated </param>
		 /// <param name="outputQueue"> full or flushed buffers are added here. If this queue is bounded then writing to this channel may block! </param>
		 public ChunkingNetworkChannel( ByteBufAllocator allocator, int maxChunkSize, LinkedList<ByteBuf> outputQueue )
		 {
			  Objects.requireNonNull( allocator, "allocator cannot be null" );
			  Objects.requireNonNull( outputQueue, "outputQueue cannot be null" );
			  this._allocator = allocator;
			  this._maxChunkSize = maxChunkSize;
			  this._initSize = min( DEFAULT_INIT_CHUNK_SIZE, maxChunkSize );
			  if ( maxChunkSize < Double.BYTES )
			  {
					throw new System.ArgumentException( "Chunk size must be at least 8. Got " + maxChunkSize );
			  }
			  this._byteBufs = outputQueue;
		 }

		 public override WritableChannel Put( sbyte value )
		 {
			  CheckState();
			  PrepareWrite( 1 );
			  _current.writeByte( value );
			  return this;
		 }

		 public override WritableChannel PutShort( short value )
		 {
			  CheckState();
			  PrepareWrite( Short.BYTES );
			  _current.writeShort( value );
			  return this;
		 }

		 public override WritableChannel PutInt( int value )
		 {
			  CheckState();
			  PrepareWrite( Integer.BYTES );
			  _current.writeInt( value );
			  return this;
		 }

		 public override WritableChannel PutLong( long value )
		 {
			  CheckState();
			  PrepareWrite( Long.BYTES );
			  _current.writeLong( value );
			  return this;
		 }

		 public override WritableChannel PutFloat( float value )
		 {
			  CheckState();
			  PrepareWrite( Float.BYTES );
			  _current.writeFloat( value );
			  return this;
		 }

		 public override WritableChannel PutDouble( double value )
		 {
			  CheckState();
			  PrepareWrite( Double.BYTES );
			  _current.writeDouble( value );
			  return this;
		 }

		 public override WritableChannel Put( sbyte[] value, int length )
		 {
			  CheckState();
			  int writeIndex = 0;
			  int remaining = length;
			  while ( remaining != 0 )
			  {
					int toWrite = PrepareGently( remaining );
					ByteBuf current = OrCreateCurrent;
					current.writeBytes( value, writeIndex, toWrite );
					writeIndex += toWrite;
					remaining = length - writeIndex;
			  }
			  return this;
		 }

		 /// <summary>
		 /// Move the current buffer to the output.
		 /// </summary>
		 public virtual WritableChannel Flush()
		 {
			  StoreCurrent();
			  return this;
		 }

		 private int PrepareGently( int size )
		 {
			  if ( OrCreateCurrent.writerIndex() == _maxChunkSize )
			  {
					PrepareWrite( size );
			  }
			  return min( _maxChunkSize - _current.writerIndex(), size );
		 }

		 private ByteBuf OrCreateCurrent
		 {
			 get
			 {
				  if ( _current == null )
				  {
						_current = AllocateNewBuffer();
				  }
				  return _current;
			 }
		 }

		 private void PrepareWrite( int size )
		 {
			  if ( ( OrCreateCurrent.writerIndex() + size ) > _maxChunkSize )
			  {
					StoreCurrent();
			  }
			  OrCreateCurrent;
		 }

		 private void StoreCurrent()
		 {
			  if ( _current == null )
			  {
					return;
			  }
			  try
			  {
					while ( !_byteBufs.AddLast( _current ) )
					{
						 Thread.Sleep( 10 );
					}
					_current = null;
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
					throw new System.InvalidOperationException( "Unable to flush. Thread interrupted" );
			  }
		 }

		 private void ReleaseCurrent()
		 {
			  if ( this._current != null )
			  {
					_current.release();
			  }
		 }

		 private ByteBuf AllocateNewBuffer()
		 {
			  return _allocator.buffer( _initSize, _maxChunkSize );
		 }

		 private void CheckState()
		 {
			  if ( _isClosed )
			  {
					throw new System.InvalidOperationException( "Channel has been closed already" );
			  }
		 }

		 /// <summary>
		 /// Flushes and closes the channel
		 /// </summary>
		 /// <seealso cref= #flush() </seealso>
		 public override void Close()
		 {
			  try
			  {
					Flush();
			  }
			  finally
			  {
					_isClosed = true;
					ReleaseCurrent();
			  }
		 }

		 public virtual bool Closed()
		 {
			  return _isClosed;
		 }
	}

}