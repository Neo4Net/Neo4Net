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
namespace Neo4Net.com
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using ChannelBufferFactory = org.jboss.netty.buffer.ChannelBufferFactory;
	using ChannelBufferIndexFinder = org.jboss.netty.buffer.ChannelBufferIndexFinder;
	using ChannelBuffers = org.jboss.netty.buffer.ChannelBuffers;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelFuture = org.jboss.netty.channel.ChannelFuture;
	using ChannelFutureListener = org.jboss.netty.channel.ChannelFutureListener;


	/// <summary>
	/// A decorator around a <seealso cref="ChannelBuffer"/> which adds the ability to transfer
	/// chunks of it over a <seealso cref="Channel"/> when capacity is reached.
	/// <para>
	/// Instances of this class are created with an underlying buffer for holding
	/// content, a capacity and a channel over which to stream the buffer contents
	/// when that capacity is reached. When content addition would make the size of
	/// the buffer exceed its capacity, a 2-byte continuation header is added in the
	/// stream that contains flow information and protocol versions and it is
	/// streamed over the channel. It is expected that a
	/// <seealso cref="DechunkingChannelBuffer"/> sits on the other end waiting to deserialize
	/// this stream. A final serialization round happens when <code>done()</code> is
	/// called, if content has been added.
	/// </para>
	/// <para>
	/// Each chunk written is marked as pending and no more than
	/// MAX_WRITE_AHEAD_CHUNKS are left pending - in such a case the write process
	/// sleeps until some acknowledgment comes back from the other side that chunks
	/// have been read.
	/// </para>
	/// </summary>
	public class ChunkingChannelBuffer : ChannelBuffer, ChannelFutureListener
	{
		 internal const sbyte CONTINUATION_LAST = 0;
		 internal const sbyte CONTINUATION_MORE = 1;
		 internal const sbyte OUTCOME_SUCCESS = 0;
		 internal const sbyte OUTCOME_FAILURE = 1;

		 protected internal const int MAX_WRITE_AHEAD_CHUNKS = 5;

		 private ChannelBuffer _buffer;
		 private readonly Channel _channel;
		 private readonly int _capacity;
		 private int _continuationPosition;
		 private readonly AtomicInteger _writeAheadCounter = new AtomicInteger();
		 private volatile bool _failure;
		 private readonly sbyte _applicationProtocolVersion;
		 private readonly sbyte _internalProtocolVersion;

		 public ChunkingChannelBuffer( ChannelBuffer buffer, Channel channel, int capacity, sbyte internalProtocolVersion, sbyte applicationProtocolVersion )
		 {
			  this._buffer = buffer;
			  this._channel = channel;
			  this._capacity = capacity;
			  this._internalProtocolVersion = internalProtocolVersion;
			  this._applicationProtocolVersion = applicationProtocolVersion;
			  AddRoomForContinuationHeader();
		 }

		 private void AddRoomForContinuationHeader()
		 {
			  _continuationPosition = WriterIndex();
			  // byte 0: [pppp,ppoc] p: internal protocol version, o: outcome, c: continuation
			  // byte 1: [aaaa,aaaa] a: application protocol version
			  _buffer.writeBytes( Header( CONTINUATION_LAST ) );
		 }

		 private sbyte[] Header( sbyte continuation )
		 {
			  sbyte[] header = new sbyte[2];
			  header[0] = ( sbyte )( ( _internalProtocolVersion << 2 ) | ( ( _failure ? OUTCOME_FAILURE : OUTCOME_SUCCESS ) << 1 ) | continuation );
			  header[1] = _applicationProtocolVersion;
			  return header;
		 }

		 private sbyte Continuation
		 {
			 set
			 {
				  _buffer.setBytes( _continuationPosition, Header( value ) );
			 }
		 }

		 public override ChannelBufferFactory Factory()
		 {
			  return _buffer.factory();
		 }

		 public override int Capacity()
		 {
			  return _buffer.capacity();
		 }

		 public override ByteOrder Order()
		 {
			  return _buffer.order();
		 }

		 public override bool Direct
		 {
			 get
			 {
				  return _buffer.Direct;
			 }
		 }

		 public override int ReaderIndex()
		 {
			  return _buffer.readerIndex();
		 }

		 public override void ReaderIndex( int readerIndex )
		 {
			  _buffer.readerIndex( readerIndex );
		 }

		 public override int WriterIndex()
		 {
			  return _buffer.writerIndex();
		 }

		 public override void WriterIndex( int writerIndex )
		 {
			  _buffer.writerIndex( writerIndex );
		 }

		 public override void SetIndex( int readerIndex, int writerIndex )
		 {
			  _buffer.setIndex( readerIndex, writerIndex );
		 }

		 public override int ReadableBytes()
		 {
			  return _buffer.readableBytes();
		 }

		 public override int WritableBytes()
		 {
			  return _buffer.writableBytes();
		 }

		 public override bool Readable()
		 {
			  return _buffer.readable();
		 }

		 public override bool Writable()
		 {
			  return _buffer.writable();
		 }

		 public virtual void Clear( bool failure )
		 {
			  _buffer.clear();
			  this._failure = failure;
			  AddRoomForContinuationHeader();
		 }

		 public override void Clear()
		 {
			  Clear( false );
		 }

		 public override void MarkReaderIndex()
		 {
			  _buffer.markReaderIndex();
		 }

		 public override void ResetReaderIndex()
		 {
			  _buffer.resetReaderIndex();
		 }

		 public override void MarkWriterIndex()
		 {
			  _buffer.markWriterIndex();
		 }

		 public override void ResetWriterIndex()
		 {
			  _buffer.resetWriterIndex();
		 }

		 public override void DiscardReadBytes()
		 {
			  _buffer.discardReadBytes();
		 }

		 public override void EnsureWritableBytes( int writableBytes )
		 {
			  _buffer.ensureWritableBytes( writableBytes );
		 }

		 public override sbyte GetByte( int index )
		 {
			  return _buffer.getByte( index );
		 }

		 public override short GetUnsignedByte( int index )
		 {
			  return _buffer.getUnsignedByte( index );
		 }

		 public override short GetShort( int index )
		 {
			  return _buffer.getShort( index );
		 }

		 public override int GetUnsignedShort( int index )
		 {
			  return _buffer.getUnsignedShort( index );
		 }

		 public override int GetMedium( int index )
		 {
			  return _buffer.getMedium( index );
		 }

		 public override int GetUnsignedMedium( int index )
		 {
			  return _buffer.getUnsignedMedium( index );
		 }

		 public override int GetInt( int index )
		 {
			  return _buffer.getInt( index );
		 }

		 public override long GetUnsignedInt( int index )
		 {
			  return _buffer.getUnsignedInt( index );
		 }

		 public override long GetLong( int index )
		 {
			  return _buffer.getLong( index );
		 }

		 public override char GetChar( int index )
		 {
			  return _buffer.getChar( index );
		 }

		 public override float GetFloat( int index )
		 {
			  return _buffer.getFloat( index );
		 }

		 public override double GetDouble( int index )
		 {
			  return _buffer.getDouble( index );
		 }

		 public override void GetBytes( int index, ChannelBuffer dst )
		 {
			  _buffer.getBytes( index, dst );
		 }

		 public override void GetBytes( int index, ChannelBuffer dst, int length )
		 {
			  _buffer.getBytes( index, dst, length );
		 }

		 public override void GetBytes( int index, ChannelBuffer dst, int dstIndex, int length )
		 {
			  _buffer.getBytes( index, dst, dstIndex, length );
		 }

		 public override void GetBytes( int index, sbyte[] dst )
		 {
			  _buffer.getBytes( index, dst );
		 }

		 public override void GetBytes( int index, sbyte[] dst, int dstIndex, int length )
		 {
			  _buffer.getBytes( index, dst, dstIndex, length );
		 }

		 public override void GetBytes( int index, ByteBuffer dst )
		 {
			  _buffer.getBytes( index, dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void getBytes(int index, java.io.OutputStream out, int length) throws java.io.IOException
		 public override void GetBytes( int index, Stream @out, int length )
		 {
			  _buffer.getBytes( index, @out, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getBytes(int index, java.nio.channels.GatheringByteChannel out, int length) throws java.io.IOException
		 public override int GetBytes( int index, GatheringByteChannel @out, int length )
		 {
			  return _buffer.getBytes( index, @out, length );
		 }

		 public override void SetByte( int index, int value )
		 {
			  _buffer.setByte( index, value );
		 }

		 public override void SetShort( int index, int value )
		 {
			  _buffer.setShort( index, value );
		 }

		 public override void SetMedium( int index, int value )
		 {
			  _buffer.setMedium( index, value );
		 }

		 public override void SetInt( int index, int value )
		 {
			  _buffer.setInt( index, value );
		 }

		 public override void SetLong( int index, long value )
		 {
			  _buffer.setLong( index, value );
		 }

		 public override void SetChar( int index, int value )
		 {
			  _buffer.setChar( index, value );
		 }

		 public override void SetFloat( int index, float value )
		 {
			  _buffer.setFloat( index, value );
		 }

		 public override void SetDouble( int index, double value )
		 {
			  _buffer.setDouble( index, value );
		 }

		 public override void SetBytes( int index, ChannelBuffer src )
		 {
			  _buffer.setBytes( index, src );
		 }

		 public override void SetBytes( int index, ChannelBuffer src, int length )
		 {
			  _buffer.setBytes( index, src, length );
		 }

		 public override void SetBytes( int index, ChannelBuffer src, int srcIndex, int length )
		 {
			  _buffer.setBytes( index, src, srcIndex, length );
		 }

		 public override void SetBytes( int index, sbyte[] src )
		 {
			  _buffer.setBytes( index, src );
		 }

		 public override void SetBytes( int index, sbyte[] src, int srcIndex, int length )
		 {
			  _buffer.setBytes( index, src, srcIndex, length );
		 }

		 public override void SetBytes( int index, ByteBuffer src )
		 {
			  _buffer.setBytes( index, src );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int setBytes(int index, java.io.InputStream in, int length) throws java.io.IOException
		 public override int SetBytes( int index, Stream @in, int length )
		 {
			  return _buffer.setBytes( index, @in, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int setBytes(int index, java.nio.channels.ScatteringByteChannel in, int length) throws java.io.IOException
		 public override int SetBytes( int index, ScatteringByteChannel @in, int length )
		 {
			  return _buffer.setBytes( index, @in, length );
		 }

		 public override void SetZero( int index, int length )
		 {
			  _buffer.setZero( index, length );
		 }

		 public override sbyte ReadByte()
		 {
			  return _buffer.readByte();
		 }

		 public override short ReadUnsignedByte()
		 {
			  return _buffer.readUnsignedByte();
		 }

		 public override short ReadShort()
		 {
			  return _buffer.readShort();
		 }

		 public override int ReadUnsignedShort()
		 {
			  return _buffer.readUnsignedShort();
		 }

		 public override int ReadMedium()
		 {
			  return _buffer.readMedium();
		 }

		 public override int ReadUnsignedMedium()
		 {
			  return _buffer.readUnsignedMedium();
		 }

		 public override int ReadInt()
		 {
			  return _buffer.readInt();
		 }

		 public override long ReadUnsignedInt()
		 {
			  return _buffer.readUnsignedInt();
		 }

		 public override long ReadLong()
		 {
			  return _buffer.readLong();
		 }

		 public override char ReadChar()
		 {
			  return _buffer.readChar();
		 }

		 public override float ReadFloat()
		 {
			  return _buffer.readFloat();
		 }

		 public override double ReadDouble()
		 {
			  return _buffer.readDouble();
		 }

		 public override ChannelBuffer ReadBytes( int length )
		 {
			  return _buffer.readBytes( length );
		 }

		 public override ChannelBuffer ReadBytes( ChannelBufferIndexFinder indexFinder )
		 {
			  return _buffer.readBytes( indexFinder );
		 }

		 public override ChannelBuffer ReadSlice( int length )
		 {
			  return _buffer.readSlice( length );
		 }

		 public override ChannelBuffer ReadSlice( ChannelBufferIndexFinder indexFinder )
		 {
			  return _buffer.readSlice( indexFinder );
		 }

		 public override void ReadBytes( ChannelBuffer dst )
		 {
			  _buffer.readBytes( dst );
		 }

		 public override void ReadBytes( ChannelBuffer dst, int length )
		 {
			  _buffer.readBytes( dst, length );
		 }

		 public override void ReadBytes( ChannelBuffer dst, int dstIndex, int length )
		 {
			  _buffer.readBytes( dst, dstIndex, length );
		 }

		 public override void ReadBytes( sbyte[] dst )
		 {
			  _buffer.readBytes( dst );
		 }

		 public override void ReadBytes( sbyte[] dst, int dstIndex, int length )
		 {
			  _buffer.readBytes( dst, dstIndex, length );
		 }

		 public override void ReadBytes( ByteBuffer dst )
		 {
			  _buffer.readBytes( dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readBytes(java.io.OutputStream out, int length) throws java.io.IOException
		 public override void ReadBytes( Stream @out, int length )
		 {
			  _buffer.readBytes( @out, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readBytes(java.nio.channels.GatheringByteChannel out, int length) throws java.io.IOException
		 public override int ReadBytes( GatheringByteChannel @out, int length )
		 {
			  return _buffer.readBytes( @out, length );
		 }

		 public override void SkipBytes( int length )
		 {
			  _buffer.skipBytes( length );
		 }

		 public override int SkipBytes( ChannelBufferIndexFinder indexFinder )
		 {
			  return _buffer.skipBytes( indexFinder );
		 }

		 private void SendChunkIfNeeded( int bytesPlus )
		 {
			  // Note: This is wasteful, it should pack as much data as possible into the current chunk before sending it off.
			  // Refactor when there is time.
			  if ( WriterIndex() + bytesPlus >= _capacity )
			  {
					Continuation = CONTINUATION_MORE;
					WriteCurrentChunk();
					_buffer = NewChannelBuffer();
					AddRoomForContinuationHeader();
			  }
		 }

		 protected internal virtual ChannelBuffer NewChannelBuffer()
		 {
			  return ChannelBuffers.dynamicBuffer( _capacity );
		 }

		 private void WriteCurrentChunk()
		 {
			  if ( !_channel.Open || !_channel.Connected || !_channel.Bound )
			  {
					throw new ComException( "Channel has been closed, so no need to try to write to it anymore. Client closed it?" );
			  }

			  WaitForClientToCatchUpOnReadingChunks();
			  ChannelFuture future = _channel.write( _buffer );
			  future.addListener( NewChannelFutureListener( _buffer ) );
			  _writeAheadCounter.incrementAndGet();
		 }

		 protected internal virtual ChannelFutureListener NewChannelFutureListener( ChannelBuffer buffer )
		 {
			  return this;
		 }

		 private void WaitForClientToCatchUpOnReadingChunks()
		 {
			  // Wait until channel gets disconnected or client catches up.
			  // If channel has been disconnected we can exit and the next write
			  // will produce a decent exception out.
			  bool waited = false;
			  while ( _channel.Connected && _writeAheadCounter.get() >= MAX_WRITE_AHEAD_CHUNKS )
			  {
					waited = true;
					try
					{
						 Thread.Sleep( 200 );
					}
					catch ( InterruptedException )
					{ // OK
						 Thread.interrupted();
					}
			  }

			  if ( waited && ( !_channel.Connected || !_channel.Open ) )
			  {
					throw new ComException( "Channel has been closed" );
			  }
		 }

		 public override void OperationComplete( ChannelFuture future )
		 {
			  if ( !future.Done )
			  {
					throw new ComException( "This should not be possible because we waited for the future to be done" );
			  }

			  if ( !future.Success || future.Cancelled )
			  {
					future.Channel.close();
			  }
			  _writeAheadCounter.decrementAndGet();
		 }

		 public virtual void Done()
		 {
			  if ( Readable() )
			  {
					WriteCurrentChunk();
			  }
		 }

		 public override void WriteByte( int value )
		 {
			  SendChunkIfNeeded( 1 );
			  _buffer.writeByte( value );
		 }

		 public override void WriteShort( int value )
		 {
			  SendChunkIfNeeded( 2 );
			  _buffer.writeShort( value );
		 }

		 public override void WriteMedium( int value )
		 {
			  SendChunkIfNeeded( 4 );
			  _buffer.writeMedium( value );
		 }

		 public override void WriteInt( int value )
		 {
			  SendChunkIfNeeded( 4 );
			  _buffer.writeInt( value );
		 }

		 public override void WriteLong( long value )
		 {
			  SendChunkIfNeeded( 8 );
			  _buffer.writeLong( value );
		 }

		 public override void WriteChar( int value )
		 {
			  SendChunkIfNeeded( 2 );
			  _buffer.writeChar( value );
		 }

		 public override void WriteFloat( float value )
		 {
			  SendChunkIfNeeded( 8 );
			  _buffer.writeFloat( value );
		 }

		 public override void WriteDouble( double value )
		 {
			  SendChunkIfNeeded( 8 );
			  _buffer.writeDouble( value );
		 }

		 public override void WriteBytes( ChannelBuffer src )
		 {
			  SendChunkIfNeeded( src.capacity() );
			  _buffer.writeBytes( src );
		 }

		 public override void WriteBytes( ChannelBuffer src, int length )
		 {
			  SendChunkIfNeeded( length );
			  _buffer.writeBytes( src, length );
		 }

		 public override void WriteBytes( ChannelBuffer src, int srcIndex, int length )
		 {
			  SendChunkIfNeeded( length );
			  _buffer.writeBytes( src, srcIndex, length );
		 }

		 public override void WriteBytes( sbyte[] src )
		 {
			  SendChunkIfNeeded( src.Length );
			  _buffer.writeBytes( src );
		 }

		 public override void WriteBytes( sbyte[] src, int srcIndex, int length )
		 {
			  SendChunkIfNeeded( length );
			  _buffer.writeBytes( src, srcIndex, length );
		 }

		 public override void WriteBytes( ByteBuffer src )
		 {
			  SendChunkIfNeeded( src.limit() );
			  _buffer.writeBytes( src );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int writeBytes(java.io.InputStream in, int length) throws java.io.IOException
		 public override int WriteBytes( Stream @in, int length )
		 {
			  SendChunkIfNeeded( length );
			  return _buffer.writeBytes( @in, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int writeBytes(java.nio.channels.ScatteringByteChannel in, int length) throws java.io.IOException
		 public override int WriteBytes( ScatteringByteChannel @in, int length )
		 {
			  SendChunkIfNeeded( length );
			  return _buffer.writeBytes( @in, length );
		 }

		 public override void WriteZero( int length )
		 {
			  SendChunkIfNeeded( length );
			  _buffer.writeZero( length );
		 }

		 public override int IndexOf( int fromIndex, int toIndex, sbyte value )
		 {
			  return _buffer.indexOf( fromIndex, toIndex, value );
		 }

		 public override int IndexOf( int fromIndex, int toIndex, ChannelBufferIndexFinder indexFinder )
		 {
			  return _buffer.indexOf( fromIndex, toIndex, indexFinder );
		 }

		 public override int BytesBefore( sbyte value )
		 {
			  return _buffer.bytesBefore( value );
		 }

		 public override int BytesBefore( ChannelBufferIndexFinder indexFinder )
		 {
			  return _buffer.bytesBefore( indexFinder );
		 }

		 public override int BytesBefore( int length, sbyte value )
		 {
			  return _buffer.bytesBefore( length, value );
		 }

		 public override int BytesBefore( int length, ChannelBufferIndexFinder indexFinder )
		 {
			  return _buffer.bytesBefore( length, indexFinder );
		 }

		 public override int BytesBefore( int index, int length, sbyte value )
		 {
			  return _buffer.bytesBefore( index, length, value );
		 }

		 public override int BytesBefore( int index, int length, ChannelBufferIndexFinder indexFinder )
		 {
			  return _buffer.bytesBefore( index, length, indexFinder );
		 }

		 public override ChannelBuffer Copy()
		 {
			  return _buffer.copy();
		 }

		 public override ChannelBuffer Copy( int index, int length )
		 {
			  return _buffer.copy( index, length );
		 }

		 public override ChannelBuffer Slice()
		 {
			  return _buffer.slice();
		 }

		 public override ChannelBuffer Slice( int index, int length )
		 {
			  return _buffer.slice( index, length );
		 }

		 public override ChannelBuffer Duplicate()
		 {
			  return _buffer.duplicate();
		 }

		 public override ByteBuffer ToByteBuffer()
		 {
			  return _buffer.toByteBuffer();
		 }

		 public override ByteBuffer ToByteBuffer( int index, int length )
		 {
			  return _buffer.toByteBuffer( index, length );
		 }

		 public override ByteBuffer[] ToByteBuffers()
		 {
			  return _buffer.toByteBuffers();
		 }

		 public override ByteBuffer[] ToByteBuffers( int index, int length )
		 {
			  return _buffer.toByteBuffers( index, length );
		 }

		 public override bool HasArray()
		 {
			  return _buffer.hasArray();
		 }

		 public override sbyte[] Array()
		 {
			  return _buffer.array();
		 }

		 public override int ArrayOffset()
		 {
			  return _buffer.arrayOffset();
		 }

		 public override string ToString( Charset charset )
		 {
			  return _buffer.ToString( charset );
		 }

		 public override string ToString( int index, int length, Charset charset )
		 {
			  return _buffer.ToString( index, length, charset );
		 }

		 public override string ToString( string charsetName )
		 {
			  return _buffer.ToString( charsetName );
		 }

		 public override string ToString( string charsetName, ChannelBufferIndexFinder terminatorFinder )
		 {
			  return _buffer.ToString( charsetName, terminatorFinder );
		 }

		 public override string ToString( int index, int length, string charsetName )
		 {
			  return _buffer.ToString( index, length, charsetName );
		 }

		 public override string ToString( int index, int length, string charsetName, ChannelBufferIndexFinder terminatorFinder )
		 {
			  return _buffer.ToString( index, length, charsetName, terminatorFinder );
		 }

		 public override int CompareTo( ChannelBuffer buffer )
		 {
			  return this._buffer.compareTo( buffer );
		 }

		 public override string ToString()
		 {
			  return _buffer.ToString();
		 }
	}

}