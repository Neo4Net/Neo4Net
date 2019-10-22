using System;

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
namespace Neo4Net.Bolt.v1.transport
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Channel = io.netty.channel.Channel;


	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using TransportThrottleException = Neo4Net.Bolt.transport.TransportThrottleException;
	using TransportThrottleGroup = Neo4Net.Bolt.transport.TransportThrottleGroup;
	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using PackOutputClosedException = Neo4Net.Bolt.v1.packstream.PackOutputClosedException;
	using PackStream = Neo4Net.Bolt.v1.packstream.PackStream;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// A target output for <seealso cref="PackStream"/> which breaks the data into a continuous stream of chunks before pushing them into a netty
	/// channel.
	/// </summary>
	public class ChunkedOutput : PackOutput
	{
		 private const int DEFAULT_BUFFER_SIZE = 8192;

		 public const int CHUNK_HEADER_SIZE = 2;
		 public const int MESSAGE_BOUNDARY = 0;

		 private static readonly int _maxChunkSize = short.MaxValue / 2;
		 private const int NO_MESSAGE = -1;

		 private readonly Channel _channel;
		 private readonly int _maxBufferSize;
		 private readonly int _maxChunkSize;
		 private readonly TransportThrottleGroup _throttleGroup;

		 private ByteBuf _buffer;
		 private int _currentChunkStartIndex;
		 private bool _closed;

		 /// <summary>
		 /// Are currently in the middle of writing a chunk? </summary>
		 private bool _chunkOpen;
		 private int _currentMessageStartIndex = NO_MESSAGE;

		 public ChunkedOutput( Channel ch, TransportThrottleGroup throttleGroup ) : this( ch, DEFAULT_BUFFER_SIZE, throttleGroup )
		 {
		 }

		 public ChunkedOutput( Channel ch, int bufferSize, TransportThrottleGroup throttleGroup ) : this( ch, bufferSize, _maxChunkSize, throttleGroup )
		 {
		 }

		 public ChunkedOutput( Channel channel, int maxBufferSize, int maxChunkSize, TransportThrottleGroup throttleGroup )
		 {
			  this._channel = Objects.requireNonNull( channel );
			  this._maxBufferSize = maxBufferSize;
			  this._maxChunkSize = maxChunkSize;
			  this._buffer = AllocateBuffer();
			  this._throttleGroup = Objects.requireNonNull( throttleGroup );
		 }

		 public override void BeginMessage()
		 {
			  if ( _currentMessageStartIndex != NO_MESSAGE )
			  {
					throw new System.InvalidOperationException( "Message has already been started, index: " + _currentMessageStartIndex );
			  }

			  _currentMessageStartIndex = _buffer.writerIndex();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void messageSucceeded() throws java.io.IOException
		 public override void MessageSucceeded()
		 {
			  AssertMessageStarted();
			  _currentMessageStartIndex = NO_MESSAGE;

			  CloseChunkIfOpen();
			  _buffer.writeShort( MESSAGE_BOUNDARY );

			  if ( _buffer.readableBytes() >= _maxBufferSize )
			  {
					Flush();
			  }
			  _chunkOpen = false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void messageFailed() throws java.io.IOException
		 public override void MessageFailed()
		 {
			  AssertMessageStarted();
			  int writerIndex = _currentMessageStartIndex;
			  _currentMessageStartIndex = NO_MESSAGE;

			  // truncate the buffer to remove all data written by an unfinished message
			  _buffer.capacity( writerIndex );
			  _chunkOpen = false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.v1.packstream.PackOutput flush() throws java.io.IOException
		 public override PackOutput Flush()
		 {
			  if ( _buffer != null && _buffer.readableBytes() > 0 )
			  {
					CloseChunkIfOpen();

					// check for and apply write throttles
					try
					{
						 _throttleGroup.writeThrottle().acquire(_channel);
					}
					catch ( TransportThrottleException ex )
					{
						 throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidUsage, ex.Message, ex );
					}

					// Local copy and clear the buffer field. This ensures that the buffer is not re-released if the flush call fails
					ByteBuf @out = this._buffer;
					this._buffer = null;

					_channel.writeAndFlush( @out, _channel.voidPromise() );

					_buffer = AllocateBuffer();
			  }
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.v1.packstream.PackOutput writeByte(byte value) throws java.io.IOException
		 public override PackOutput WriteByte( sbyte value )
		 {
			  Ensure( Byte.BYTES );
			  _buffer.writeByte( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.v1.packstream.PackOutput writeShort(short value) throws java.io.IOException
		 public override PackOutput WriteShort( short value )
		 {
			  Ensure( Short.BYTES );
			  _buffer.writeShort( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.v1.packstream.PackOutput writeInt(int value) throws java.io.IOException
		 public override PackOutput WriteInt( int value )
		 {
			  Ensure( Integer.BYTES );
			  _buffer.writeInt( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.v1.packstream.PackOutput writeLong(long value) throws java.io.IOException
		 public override PackOutput WriteLong( long value )
		 {
			  Ensure( Long.BYTES );
			  _buffer.writeLong( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.v1.packstream.PackOutput writeDouble(double value) throws java.io.IOException
		 public override PackOutput WriteDouble( double value )
		 {
			  Ensure( Double.BYTES );
			  _buffer.writeDouble( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.v1.packstream.PackOutput writeBytes(ByteBuffer data) throws java.io.IOException
		 public override PackOutput WriteBytes( ByteBuffer data )
		 {
			  while ( data.remaining() > 0 )
			  {
					// Ensure there is an open chunk, and that it has at least one byte of space left
					Ensure( 1 );

					int oldLimit = data.limit();
					data.limit( data.position() + Math.Min(AvailableBytesInCurrentChunk(), data.remaining()) );
					_buffer.writeBytes( data );
					data.limit( oldLimit );
			  }
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.v1.packstream.PackOutput writeBytes(byte[] data, int offset, int length) throws java.io.IOException
		 public override PackOutput WriteBytes( sbyte[] data, int offset, int length )
		 {
			  if ( offset + length > data.Length )
			  {
					throw new IOException( "Asked to write " + length + " bytes, but there is only " + ( data.Length - offset ) + " bytes available in data provided." );
			  }
			  return WriteBytes( ByteBuffer.wrap( data, offset, length ) );
		 }

		 public override void Close()
		 {
			  try
			  {
					Flush();
			  }
			  catch ( IOException )
			  {
			  }
			  finally
			  {
					_closed = true;
					if ( _buffer != null )
					{
						 _buffer.release();
						 _buffer = null;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensure(int numberOfBytes) throws java.io.IOException
		 private void Ensure( int numberOfBytes )
		 {
			  AssertOpen();
			  AssertMessageStarted();

			  if ( _chunkOpen )
			  {
					int targetChunkSize = CurrentChunkBodySize() + numberOfBytes + CHUNK_HEADER_SIZE;
					if ( targetChunkSize > _maxChunkSize )
					{
						 CloseChunkIfOpen();
						 StartNewChunk();
					}
			  }
			  else
			  {
					StartNewChunk();
			  }
		 }

		 private void StartNewChunk()
		 {
			  _currentChunkStartIndex = _buffer.writerIndex();

			  // write empty chunk header
			  _buffer.writeShort( 0 );
			  _chunkOpen = true;
		 }

		 private void CloseChunkIfOpen()
		 {
			  if ( _chunkOpen )
			  {
					int chunkBodySize = CurrentChunkBodySize();
					_buffer.setShort( _currentChunkStartIndex, chunkBodySize );
					_chunkOpen = false;
			  }
		 }

		 private int AvailableBytesInCurrentChunk()
		 {
			  return _maxChunkSize - CurrentChunkBodySize() - CHUNK_HEADER_SIZE;
		 }

		 private int CurrentChunkBodySize()
		 {
			  return _buffer.writerIndex() - (_currentChunkStartIndex + CHUNK_HEADER_SIZE);
		 }

		 private ByteBuf AllocateBuffer()
		 {
			  return _channel.alloc().buffer(_maxBufferSize);
		 }

		 private void AssertMessageStarted()
		 {
			  if ( _currentMessageStartIndex == NO_MESSAGE )
			  {
					throw new System.InvalidOperationException( "Message has not been started" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertOpen() throws org.Neo4Net.bolt.v1.packstream.PackOutputClosedException
		 private void AssertOpen()
		 {
			  if ( _closed )
			  {
					throw new PackOutputClosedException( string.Format( "Network channel towards {0} is closed. Client has probably been stopped.", _channel.remoteAddress() ), string.Format("{0}", _channel.remoteAddress()) );
			  }
		 }
	}

}