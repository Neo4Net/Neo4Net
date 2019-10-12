using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.com
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using ChannelBufferFactory = org.jboss.netty.buffer.ChannelBufferFactory;
	using ChannelBufferIndexFinder = org.jboss.netty.buffer.ChannelBufferIndexFinder;
	using ChannelBuffers = org.jboss.netty.buffer.ChannelBuffers;
	using BlockingReadHandler = org.jboss.netty.handler.queue.BlockingReadHandler;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.numbersToBitString;

	internal class DechunkingChannelBuffer : ChannelBuffer
	{
		 private readonly BlockingReadHandler<ChannelBuffer> _reader;
		 private ChannelBuffer _buffer;
		 private bool _more;
		 private bool _hasMarkedReaderIndex;
		 private readonly long _timeoutMillis;
		 private bool _failure;
		 private readonly sbyte _applicationProtocolVersion;
		 private readonly sbyte _internalProtocolVersion;

		 internal DechunkingChannelBuffer( BlockingReadHandler<ChannelBuffer> reader, long timeoutMillis, sbyte internalProtocolVersion, sbyte applicationProtocolVersion )
		 {
			  this._reader = reader;
			  this._timeoutMillis = timeoutMillis;
			  this._internalProtocolVersion = internalProtocolVersion;
			  this._applicationProtocolVersion = applicationProtocolVersion;
			  ReadNextChunk();
		 }

		 private ChannelBuffer ReadNext()
		 {
			  try
			  {
					ChannelBuffer result = _reader.read( _timeoutMillis, TimeUnit.MILLISECONDS );
					if ( result == null )
					{
						 throw new ComException( "Channel has been closed" );
					}
					return result;
			  }
			  catch ( Exception e ) when ( e is IOException || e is InterruptedException )
			  {
					throw new ComException( e );
			  }
		 }

		 private void ReadNextChunkIfNeeded( int bytesPlus )
		 {
			  if ( _buffer.readableBytes() < bytesPlus && _more )
			  {
					ReadNextChunk();
			  }
		 }

		 private void ReadNextChunk()
		 {
			  ChannelBuffer readBuffer = ReadNext();

			  /* Header layout:
			   * [    ,    ][    ,   x] 0: last chunk in message, 1: there a more chunks after this one
			   * [    ,    ][    ,  x ] 0: success, 1: failure
			   * [    ,    ][xxxx,xx  ] internal protocol version
			   * [xxxx,xxxx][    ,    ] application protocol version */
			  sbyte[] header = new sbyte[2];
			  readBuffer.readBytes( header );
			  _more = ( header[0] & 0x1 ) != 0;
			  _failure = ( header[0] & 0x2 ) != 0;
			  AssertSameProtocolVersion( header, _internalProtocolVersion, _applicationProtocolVersion );

			  if ( !_more && _buffer == null )
			  {
					// Optimization: this is the first chunk and it'll be the only chunk
					// in this message.
					_buffer = readBuffer;
			  }
			  else
			  {
					_buffer = _buffer == null ? ChannelBuffers.dynamicBuffer() : _buffer;
					DiscardReadBytes();
					_buffer.writeBytes( readBuffer );
			  }

			  if ( _failure )
			  {
					ReadAndThrowFailureResponse();
			  }
		 }

		 internal static void AssertSameProtocolVersion( sbyte[] header, sbyte internalProtocolVersion, sbyte applicationProtocolVersion )
		 {
			  /* [aaaa,aaaa][pppp,ppoc]
			   * Only 6 bits for internal protocol version, yielding 64 values. It's ok to wrap around because
			   * It's highly unlikely that instances that are so far apart in versions will communicate
			   * with each other.
			   */
			  sbyte readInternalProtocolVersion = ( sbyte )( ( int )( ( uint )( header[0] & 0x7C ) >> 2 ) );
			  if ( readInternalProtocolVersion != internalProtocolVersion )
			  {
					throw new IllegalProtocolVersionException( internalProtocolVersion, readInternalProtocolVersion, "Unexpected internal protocol version " + readInternalProtocolVersion + ", expected " + internalProtocolVersion + ". Header:" + numbersToBitString( header ) );
			  }
			  if ( header[1] != applicationProtocolVersion )
			  {
					throw new IllegalProtocolVersionException( applicationProtocolVersion, header[1], "Unexpected application protocol version " + header[1] + ", expected " + applicationProtocolVersion + ". Header:" + numbersToBitString( header ) );
			  }
		 }

		 private void ReadAndThrowFailureResponse()
		 {
			  Exception cause;
			  try
			  {
					using ( ObjectInputStream input = new ObjectInputStream( AsInputStream() ) )
					{
						 cause = ( Exception ) input.readObject();
					}
			  }
			  catch ( Exception e )
			  {
					// Note: this is due to a problem with the streaming of exceptions, the ChunkingChannelBuffer will almost
					// always sends exceptions back as two chunks, the first one empty and the second with the exception.
					// We hit this when we try to read the exception of the first one, and in reading it hit the second
					// chunk with the "real" exception. This should be revisited to 1) clear up the chunking and 2) handle
					// serialized exceptions spanning multiple chunks.
					if ( e is Exception )
					{
						 throw ( Exception ) e;
					}
					if ( e is Exception )
					{
						 throw ( Exception ) e;
					}

					throw new ComException( e );
			  }

			  if ( cause is Exception )
			  {
					throw ( Exception ) cause;
			  }
			  if ( cause is Exception )
			  {
					throw ( Exception ) cause;
			  }
			  throw new ComException( cause );
		 }

		 public override ChannelBufferFactory Factory()
		 {
			  return _buffer.factory();
		 }

		 public virtual bool Failure()
		 {
			  return _failure;
		 }

		 /// <summary>
		 /// Will return the capacity of the current chunk only
		 /// </summary>
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

		 /// <summary>
		 /// Will return amount of readable bytes in this chunk only
		 /// </summary>
		 public override int ReadableBytes()
		 {
			  return _buffer.readableBytes();
		 }

		 public override int WritableBytes()
		 {
			  return 0;
		 }

		 /// <summary>
		 /// Can fetch the next chunk if needed
		 /// </summary>
		 public override bool Readable()
		 {
			  ReadNextChunkIfNeeded( 1 );
			  return _buffer.readable();
		 }

		 public override bool Writable()
		 {
			  return _buffer.writable();
		 }

		 public override void Clear()
		 {
			  _buffer.clear();
		 }

		 public override void MarkReaderIndex()
		 {
			  _buffer.markReaderIndex();
			  _hasMarkedReaderIndex = true;
		 }

		 public override void ResetReaderIndex()
		 {
			  _buffer.resetReaderIndex();
			  _hasMarkedReaderIndex = false;
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
			  int oldReaderIndex = _buffer.readerIndex();
			  if ( _hasMarkedReaderIndex )
			  {
					_buffer.resetReaderIndex();
			  }
			  int bytesToDiscard = _buffer.readerIndex();
			  _buffer.discardReadBytes();
			  if ( _hasMarkedReaderIndex )
			  {
					_buffer.readerIndex( oldReaderIndex - bytesToDiscard );
			  }
		 }

		 public override void EnsureWritableBytes( int writableBytes )
		 {
			  _buffer.ensureWritableBytes( writableBytes );
		 }

		 public override sbyte GetByte( int index )
		 {
			  ReadNextChunkIfNeeded( 1 );
			  return _buffer.getByte( index );
		 }

		 public override short GetUnsignedByte( int index )
		 {
			  ReadNextChunkIfNeeded( 1 );
			  return _buffer.getUnsignedByte( index );
		 }

		 public override short GetShort( int index )
		 {
			  ReadNextChunkIfNeeded( 2 );
			  return _buffer.getShort( index );
		 }

		 public override int GetUnsignedShort( int index )
		 {
			  ReadNextChunkIfNeeded( 2 );
			  return _buffer.getUnsignedShort( index );
		 }

		 public override int GetMedium( int index )
		 {
			  ReadNextChunkIfNeeded( 4 );
			  return _buffer.getMedium( index );
		 }

		 public override int GetUnsignedMedium( int index )
		 {
			  ReadNextChunkIfNeeded( 4 );
			  return _buffer.getUnsignedMedium( index );
		 }

		 public override int GetInt( int index )
		 {
			  ReadNextChunkIfNeeded( 4 );
			  return _buffer.getInt( index );
		 }

		 public override long GetUnsignedInt( int index )
		 {
			  ReadNextChunkIfNeeded( 4 );
			  return _buffer.getUnsignedInt( index );
		 }

		 public override long GetLong( int index )
		 {
			  ReadNextChunkIfNeeded( 8 );
			  return _buffer.getLong( index );
		 }

		 public override char GetChar( int index )
		 {
			  ReadNextChunkIfNeeded( 2 );
			  return _buffer.getChar( index );
		 }

		 public override float GetFloat( int index )
		 {
			  ReadNextChunkIfNeeded( 8 );
			  return _buffer.getFloat( index );
		 }

		 public override double GetDouble( int index )
		 {
			  ReadNextChunkIfNeeded( 8 );
			  return _buffer.getDouble( index );
		 }

		 public override void GetBytes( int index, ChannelBuffer dst )
		 {
			  // TODO We need a loop for this (if dst is bigger than chunk size)
			  ReadNextChunkIfNeeded( dst.writableBytes() );
			  _buffer.getBytes( index, dst );
		 }

		 public override void GetBytes( int index, ChannelBuffer dst, int length )
		 {
			  // TODO We need a loop for this (if dst is bigger than chunk size)
			  ReadNextChunkIfNeeded( length );
			  _buffer.getBytes( index, dst, length );
		 }

		 public override void GetBytes( int index, ChannelBuffer dst, int dstIndex, int length )
		 {
			  // TODO We need a loop for this (if dst is bigger than chunk size)
			  ReadNextChunkIfNeeded( length );
			  _buffer.getBytes( index, dst, dstIndex, length );
		 }

		 public override void GetBytes( int index, sbyte[] dst )
		 {
			  // TODO We need a loop for this (if dst is bigger than chunk size)
			  ReadNextChunkIfNeeded( dst.Length );
			  _buffer.getBytes( index, dst );
		 }

		 public override void GetBytes( int index, sbyte[] dst, int dstIndex, int length )
		 {
			  // TODO We need a loop for this (if dst is bigger than chunk size)
			  ReadNextChunkIfNeeded( length );
			  _buffer.getBytes( index, dst, dstIndex, length );
		 }

		 public override void GetBytes( int index, ByteBuffer dst )
		 {
			  // TODO We need a loop for this (if dst is bigger than chunk size)
			  ReadNextChunkIfNeeded( dst.limit() );
			  _buffer.getBytes( index, dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void getBytes(int index, java.io.OutputStream out, int length) throws java.io.IOException
		 public override void GetBytes( int index, Stream @out, int length )
		 {
			  // TODO We need a loop for this (if dst is bigger than chunk size)
			  ReadNextChunkIfNeeded( length );
			  _buffer.getBytes( index, @out, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getBytes(int index, java.nio.channels.GatheringByteChannel out, int length) throws java.io.IOException
		 public override int GetBytes( int index, GatheringByteChannel @out, int length )
		 {
			  // TODO We need a loop for this (if dst is bigger than chunk size)
			  ReadNextChunkIfNeeded( length );
			  return _buffer.getBytes( index, @out, length );
		 }

		 private System.NotSupportedException UnsupportedOperation()
		 {
			  return new System.NotSupportedException( "Not supported in a DechunkingChannelBuffer, it's used merely for reading" );
		 }

		 public override void SetByte( int index, int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetShort( int index, int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetMedium( int index, int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetInt( int index, int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetLong( int index, long value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetChar( int index, int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetFloat( int index, float value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetDouble( int index, double value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetBytes( int index, ChannelBuffer src )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetBytes( int index, ChannelBuffer src, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetBytes( int index, ChannelBuffer src, int srcIndex, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetBytes( int index, sbyte[] src )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetBytes( int index, sbyte[] src, int srcIndex, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetBytes( int index, ByteBuffer src )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int SetBytes( int index, Stream @in, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int SetBytes( int index, ScatteringByteChannel @in, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void SetZero( int index, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override sbyte ReadByte()
		 {
			  ReadNextChunkIfNeeded( 1 );
			  return _buffer.readByte();
		 }

		 public override short ReadUnsignedByte()
		 {
			  ReadNextChunkIfNeeded( 1 );
			  return _buffer.readUnsignedByte();
		 }

		 public override short ReadShort()
		 {
			  ReadNextChunkIfNeeded( 2 );
			  return _buffer.readShort();
		 }

		 public override int ReadUnsignedShort()
		 {
			  ReadNextChunkIfNeeded( 2 );
			  return _buffer.readUnsignedShort();
		 }

		 public override int ReadMedium()
		 {
			  ReadNextChunkIfNeeded( 4 );
			  return _buffer.readMedium();
		 }

		 public override int ReadUnsignedMedium()
		 {
			  ReadNextChunkIfNeeded( 4 );
			  return _buffer.readUnsignedMedium();
		 }

		 public override int ReadInt()
		 {
			  ReadNextChunkIfNeeded( 4 );
			  return _buffer.readInt();
		 }

		 public override long ReadUnsignedInt()
		 {
			  ReadNextChunkIfNeeded( 4 );
			  return _buffer.readUnsignedInt();
		 }

		 public override long ReadLong()
		 {
			  ReadNextChunkIfNeeded( 8 );
			  return _buffer.readLong();
		 }

		 public override char ReadChar()
		 {
			  ReadNextChunkIfNeeded( 2 );
			  return _buffer.readChar();
		 }

		 public override float ReadFloat()
		 {
			  ReadNextChunkIfNeeded( 8 );
			  return _buffer.readFloat();
		 }

		 public override double ReadDouble()
		 {
			  ReadNextChunkIfNeeded( 8 );
			  return _buffer.readDouble();
		 }

		 public override ChannelBuffer ReadBytes( int length )
		 {
			  ReadNextChunkIfNeeded( length );
			  return _buffer.readBytes( length );
		 }

		 public override ChannelBuffer ReadBytes( ChannelBufferIndexFinder indexFinder )
		 {
			  throw UnsupportedOperation();
		 }

		 public override ChannelBuffer ReadSlice( int length )
		 {
			  ReadNextChunkIfNeeded( length );
			  return _buffer.readSlice( length );
		 }

		 public override ChannelBuffer ReadSlice( ChannelBufferIndexFinder indexFinder )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void ReadBytes( ChannelBuffer dst )
		 {
			  ReadNextChunkIfNeeded( dst.writableBytes() );
			  _buffer.readBytes( dst );
		 }

		 public override void ReadBytes( ChannelBuffer dst, int length )
		 {
			  ReadNextChunkIfNeeded( length );
			  _buffer.readBytes( dst, length );
		 }

		 public override void ReadBytes( ChannelBuffer dst, int dstIndex, int length )
		 {
			  ReadNextChunkIfNeeded( length );
			  _buffer.readBytes( dst, dstIndex, length );
		 }

		 public override void ReadBytes( sbyte[] dst )
		 {
			  ReadNextChunkIfNeeded( dst.Length );
			  _buffer.readBytes( dst );
		 }

		 public override void ReadBytes( sbyte[] dst, int dstIndex, int length )
		 {
			  ReadNextChunkIfNeeded( length );
			  _buffer.readBytes( dst, dstIndex, length );
		 }

		 public override void ReadBytes( ByteBuffer dst )
		 {
			  ReadNextChunkIfNeeded( dst.limit() );
			  _buffer.readBytes( dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readBytes(java.io.OutputStream out, int length) throws java.io.IOException
		 public override void ReadBytes( Stream @out, int length )
		 {
			  ReadNextChunkIfNeeded( length );
			  _buffer.readBytes( @out, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readBytes(java.nio.channels.GatheringByteChannel out, int length) throws java.io.IOException
		 public override int ReadBytes( GatheringByteChannel @out, int length )
		 {
			  ReadNextChunkIfNeeded( length );
			  return _buffer.readBytes( @out, length );
		 }

		 public override void SkipBytes( int length )
		 {
			  ReadNextChunkIfNeeded( length );
			  _buffer.skipBytes( length );
		 }

		 public override int SkipBytes( ChannelBufferIndexFinder indexFinder )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteByte( int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteShort( int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteMedium( int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteInt( int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteLong( long value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteChar( int value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteFloat( float value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteDouble( double value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteBytes( ChannelBuffer src )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteBytes( ChannelBuffer src, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteBytes( ChannelBuffer src, int srcIndex, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteBytes( sbyte[] src )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteBytes( sbyte[] src, int srcIndex, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteBytes( ByteBuffer src )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int WriteBytes( Stream @in, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int WriteBytes( ScatteringByteChannel @in, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override void WriteZero( int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int IndexOf( int fromIndex, int toIndex, sbyte value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int IndexOf( int fromIndex, int toIndex, ChannelBufferIndexFinder indexFinder )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int BytesBefore( sbyte value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int BytesBefore( ChannelBufferIndexFinder indexFinder )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int BytesBefore( int length, sbyte value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int BytesBefore( int length, ChannelBufferIndexFinder indexFinder )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int BytesBefore( int index, int length, sbyte value )
		 {
			  throw UnsupportedOperation();
		 }

		 public override int BytesBefore( int index, int length, ChannelBufferIndexFinder indexFinder )
		 {
			  throw UnsupportedOperation();
		 }

		 public override ChannelBuffer Copy()
		 {
			  throw UnsupportedOperation();
		 }

		 public override ChannelBuffer Copy( int index, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override ChannelBuffer Slice()
		 {
			  throw UnsupportedOperation();
		 }

		 public override ChannelBuffer Slice( int index, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override ChannelBuffer Duplicate()
		 {
			  throw UnsupportedOperation();
		 }

		 public override ByteBuffer ToByteBuffer()
		 {
			  throw UnsupportedOperation();
		 }

		 public override ByteBuffer ToByteBuffer( int index, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override ByteBuffer[] ToByteBuffers()
		 {
			  throw UnsupportedOperation();
		 }

		 public override ByteBuffer[] ToByteBuffers( int index, int length )
		 {
			  throw UnsupportedOperation();
		 }

		 public override bool HasArray()
		 {
			  throw UnsupportedOperation();
		 }

		 public override sbyte[] Array()
		 {
			  throw UnsupportedOperation();
		 }

		 public override int ArrayOffset()
		 {
			  throw UnsupportedOperation();
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

		 public override int GetHashCode()
		 {
			  return _buffer.GetHashCode();
		 }

		 public override bool Equals( object obj )
		 {
			  return _buffer.Equals( obj );
		 }

		 public override int CompareTo( ChannelBuffer buffer )
		 {
			  return this._buffer.compareTo( buffer );
		 }

		 public override string ToString()
		 {
			  return _buffer.ToString();
		 }

		 private Stream AsInputStream()
		 {
			  return new InputStreamAnonymousInnerClass( this );
		 }

		 private class InputStreamAnonymousInnerClass : Stream
		 {
			 private readonly DechunkingChannelBuffer _outerInstance;

			 public InputStreamAnonymousInnerClass( DechunkingChannelBuffer outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override int read( sbyte[] b )
			 {
				  outerInstance.readBytes( b );
				  return b.Length;
			 }

			 public override int read( sbyte[] b, int off, int len )
			 {
				  outerInstance.ReadBytes( b, off, len );
				  return len;
			 }

			 public override long skip( long n )
			 {
				  outerInstance.SkipBytes( ( int )n );
				  return n;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int available() throws java.io.IOException
			 public override int available()
			 {
				  return base.available();
			 }

			 public override void close()
			 {
			 }

			 public override void mark( int readlimit )
			 {
				 lock ( this )
				 {
					  throw new System.NotSupportedException();
				 }
			 }

			 public override void reset()
			 {
				 lock ( this )
				 {
					  throw new System.NotSupportedException();
				 }
			 }

			 public override bool markSupported()
			 {
				  return false;
			 }

			 public override int read()
			 {
				  return outerInstance.ReadByte();
			 }
		 }
	}

}