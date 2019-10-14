using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using ByteBufferBackedChannelBuffer = org.jboss.netty.buffer.ByteBufferBackedChannelBuffer;
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using ChannelBufferFactory = org.jboss.netty.buffer.ChannelBufferFactory;
	using ChannelBufferIndexFinder = org.jboss.netty.buffer.ChannelBufferIndexFinder;


	using InMemoryClosableChannel = Neo4Net.Kernel.impl.transaction.log.InMemoryClosableChannel;
	using ReadPastEndException = Neo4Net.Storageengine.Api.ReadPastEndException;

	/// <summary>
	/// Wraps an <seealso cref="InMemoryClosableChannel"/>, making it look like one <seealso cref="ChannelBuffer"/>.
	/// </summary>
	public class ChannelBufferWrapper : ChannelBuffer
	{
		 private readonly InMemoryClosableChannel @delegate;

		 public ChannelBufferWrapper( InMemoryClosableChannel @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override ChannelBufferFactory Factory()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int Capacity()
		 {
			  return @delegate.Capacity();
		 }

		 public override ByteOrder Order()
		 {
			  return ByteOrder.BIG_ENDIAN;
		 }

		 public override bool Direct
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override int ReaderIndex()
		 {
			  return @delegate.ReaderPosition();
		 }

		 public override void ReaderIndex( int readerIndex )
		 {
			  @delegate.PositionReader( readerIndex );
		 }

		 public override int WriterIndex()
		 {
			  return @delegate.WriterPosition();
		 }

		 public override void WriterIndex( int writerIndex )
		 {
			  @delegate.PositionWriter( writerIndex );
		 }

		 public override void SetIndex( int readerIndex, int writerIndex )
		 {
			  @delegate.PositionReader( readerIndex );
			  @delegate.PositionWriter( writerIndex );
		 }

		 public override int ReadableBytes()
		 {
			  return @delegate.AvailableBytesToRead();
		 }

		 public override int WritableBytes()
		 {
			  return @delegate.AvailableBytesToWrite();
		 }

		 public override bool Readable()
		 {
			  return @delegate.WriterPosition() > @delegate.ReaderPosition();
		 }

		 public override bool Writable()
		 {
			  return @delegate.WriterPosition() < @delegate.Capacity();
		 }

		 public override void Clear()
		 {
			  @delegate.Reset();
		 }

		 public override void MarkReaderIndex()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void ResetReaderIndex()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void MarkWriterIndex()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void ResetWriterIndex()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void DiscardReadBytes()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void EnsureWritableBytes( int writableBytes )
		 {
			  bool availableBytes = @delegate.AvailableBytesToWrite() < writableBytes;
			  if ( availableBytes )
			  {
					throw new System.IndexOutOfRangeException( "Wanted " + writableBytes + " to be available for writing, " + "but there were only " + @delegate.AvailableBytesToWrite() + " available" );
			  }
		 }

		 public override sbyte GetByte( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadByte();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override short GetUnsignedByte( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadUnsignedByte();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override short GetShort( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadShort();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override int GetUnsignedShort( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadUnsignedShort();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override int GetMedium( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadMedium();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override int GetUnsignedMedium( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadUnsignedMedium();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override int GetInt( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadInt();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override long GetUnsignedInt( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadUnsignedInt();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override long GetLong( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadLong();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override char GetChar( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadChar();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override float GetFloat( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadFloat();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override double GetDouble( int index )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadDouble();
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override void GetBytes( int index, ChannelBuffer dst )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					ReadBytes( dst );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override void GetBytes( int index, ChannelBuffer dst, int length )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					ReadBytes( dst, length );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override void GetBytes( int index, ChannelBuffer dst, int dstIndex, int length )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					ReadBytes( dst, dstIndex, length );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override void GetBytes( int index, sbyte[] dst )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					readBytes( dst );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override void GetBytes( int index, sbyte[] dst, int dstIndex, int length )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					ReadBytes( dst, dstIndex, length );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override void GetBytes( int index, ByteBuffer dst )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					ReadBytes( dst );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void getBytes(int index, java.io.OutputStream out, int length) throws java.io.IOException
		 public override void GetBytes( int index, Stream @out, int length )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					ReadBytes( @out, length );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getBytes(int index, java.nio.channels.GatheringByteChannel out, int length) throws java.io.IOException
		 public override int GetBytes( int index, GatheringByteChannel @out, int length )
		 {
			  int pos = @delegate.PositionReader( index );
			  try
			  {
					return ReadBytes( @out, length );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override void SetByte( int index, int value )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteByte( value );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetShort( int index, int value )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteShort( value );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetMedium( int index, int value )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteMedium( value );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetInt( int index, int value )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteInt( value );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetLong( int index, long value )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteLong( value );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetChar( int index, int value )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteChar( value );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetFloat( int index, float value )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteFloat( value );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetDouble( int index, double value )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteDouble( value );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetBytes( int index, ChannelBuffer src )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteBytes( src );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetBytes( int index, ChannelBuffer src, int length )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteBytes( src, length );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetBytes( int index, ChannelBuffer src, int srcIndex, int length )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteBytes( src, srcIndex, length );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetBytes( int index, sbyte[] src )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteBytes( src );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetBytes( int index, sbyte[] src, int srcIndex, int length )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteBytes( src, srcIndex, length );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetBytes( int index, ByteBuffer src )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					WriteBytes( src );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int setBytes(int index, java.io.InputStream in, int length) throws java.io.IOException
		 public override int SetBytes( int index, Stream @in, int length )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					return WriteBytes( @in, length );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int setBytes(int index, java.nio.channels.ScatteringByteChannel in, int length) throws java.io.IOException
		 public override int SetBytes( int index, ScatteringByteChannel @in, int length )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					return WriteBytes( @in, length );
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override void SetZero( int index, int length )
		 {
			  int pos = @delegate.PositionWriter( index );
			  try
			  {
					for ( int i = 0; i < length; i++ )
					{
						 WriteByte( 0 );
					}
			  }
			  finally
			  {
					@delegate.PositionWriter( pos );
			  }
		 }

		 public override sbyte ReadByte()
		 {
			  try
			  {
					return @delegate.Get();
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 private System.IndexOutOfRangeException OutOfBounds( ReadPastEndException e )
		 {
			  return new System.IndexOutOfRangeException( "Tried to read past the end " + e );
		 }

		 public override short ReadUnsignedByte()
		 {
			  try
			  {
					return ( short )( @delegate.Get() & 0xFF );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override short ReadShort()
		 {
			  try
			  {
					return @delegate.Short;
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override int ReadUnsignedShort()
		 {
			  try
			  {
					return @delegate.Short & 0xFFFF;
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override int ReadMedium()
		 {
			  try
			  {
					int low = @delegate.Short;
					int high = @delegate.Get();
					return low | ( high << 16 );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override int ReadUnsignedMedium()
		 {
			  return ReadMedium();
		 }

		 public override int ReadInt()
		 {
			  try
			  {
					return @delegate.Int;
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override long ReadUnsignedInt()
		 {
			  try
			  {
					return @delegate.Int & 0xFFFFFFFFL;
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override long ReadLong()
		 {
			  try
			  {
					return @delegate.Long;
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override char ReadChar()
		 {
			  try
			  {
					short low = @delegate.Get();
					short high = @delegate.Get();
					return ( char )( low | ( high << 8 ) );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override float ReadFloat()
		 {
			  try
			  {
					return @delegate.Float;
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override double ReadDouble()
		 {
			  try
			  {
					return @delegate.Double;
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override ChannelBuffer ReadBytes( int length )
		 {
			  try
			  {
					sbyte[] array = new sbyte[length];
					@delegate.Get( array, length );
					return new ByteBufferBackedChannelBuffer( ByteBuffer.wrap( array ) );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override ChannelBuffer ReadBytes( ChannelBufferIndexFinder indexFinder )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelBuffer ReadSlice( int length )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelBuffer ReadSlice( ChannelBufferIndexFinder indexFinder )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void ReadBytes( ChannelBuffer dst )
		 {
			  ReadBytes( dst, dst.writableBytes() );
		 }

		 public override void ReadBytes( ChannelBuffer dst, int length )
		 {
			  try
			  {
					sbyte[] array = new sbyte[length];
					@delegate.Get( array, length );
					dst.writeBytes( array );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override void ReadBytes( ChannelBuffer dst, int dstIndex, int length )
		 {
			  dst.readerIndex( dstIndex );
			  ReadBytes( dst, length );
		 }

		 public override void ReadBytes( sbyte[] dst )
		 {
			  try
			  {
					@delegate.Get( dst, dst.Length );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override void ReadBytes( sbyte[] dst, int dstIndex, int length )
		 {
			  try
			  {
					sbyte[] array = new sbyte[length];
					@delegate.Get( array, length );
					Array.Copy( array, 0, dst, dstIndex, length );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override void ReadBytes( ByteBuffer dst )
		 {
			  sbyte[] array = new sbyte[dst.remaining()];
			  try
			  {
					@delegate.Get( array, array.Length );
					dst.put( array );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readBytes(java.io.OutputStream out, int length) throws java.io.IOException
		 public override void ReadBytes( Stream @out, int length )
		 {
			  sbyte[] array = new sbyte[length];
			  try
			  {
					@delegate.Get( array, length );
					@out.Write( array, 0, array.Length );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readBytes(java.nio.channels.GatheringByteChannel out, int length) throws java.io.IOException
		 public override int ReadBytes( GatheringByteChannel @out, int length )
		 {
			  sbyte[] array = new sbyte[length];
			  try
			  {
					@delegate.Get( array, length );
					return @out.write( ByteBuffer.wrap( array ) );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw OutOfBounds( e );
			  }
		 }

		 public override void SkipBytes( int length )
		 {
			  @delegate.PositionReader( @delegate.ReaderPosition() + length );
		 }

		 public override int SkipBytes( ChannelBufferIndexFinder indexFinder )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void WriteByte( int value )
		 {
			  @delegate.Put( ( sbyte )value );
		 }

		 public override void WriteShort( int value )
		 {
			  @delegate.PutShort( ( short )value );
		 }

		 public override void WriteMedium( int value )
		 {
			  @delegate.PutShort( ( short )value );
			  @delegate.Put( ( sbyte )( ( int )( ( uint )value >> 16 ) ) );
		 }

		 public override void WriteInt( int value )
		 {
			  @delegate.PutInt( value );
		 }

		 public override void WriteLong( long value )
		 {
			  @delegate.PutLong( value );
		 }

		 public override void WriteChar( int value )
		 {
			  @delegate.Put( ( sbyte )value );
			  @delegate.Put( ( sbyte )( ( int )( ( uint )value >> 8 ) ) );
		 }

		 public override void WriteFloat( float value )
		 {
			  @delegate.PutFloat( value );
		 }

		 public override void WriteDouble( double value )
		 {
			  @delegate.PutDouble( value );
		 }

		 public override void WriteBytes( ChannelBuffer src )
		 {
			  WriteBytes( src, src.readableBytes() );
		 }

		 public override void WriteBytes( ChannelBuffer src, int length )
		 {
			  sbyte[] array = new sbyte[length];
			  src.readBytes( array );
			  @delegate.Put( array, array.Length );
		 }

		 public override void WriteBytes( ChannelBuffer src, int srcIndex, int length )
		 {
			  src.readerIndex( srcIndex );
			  WriteBytes( src, length );
		 }

		 public override void WriteBytes( sbyte[] src )
		 {
			  @delegate.Put( src, src.Length );
		 }

		 public override void WriteBytes( sbyte[] src, int srcIndex, int length )
		 {
			  if ( srcIndex > 0 )
			  {
					sbyte[] array = new sbyte[length];
					Array.Copy( src, 0, array, srcIndex, length );
					src = array;
			  }
			  WriteBytes( src );
		 }

		 public override void WriteBytes( ByteBuffer src )
		 {
			  sbyte[] array = new sbyte[src.remaining()];
			  src.get( array );
			  WriteBytes( array );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int writeBytes(java.io.InputStream in, int length) throws java.io.IOException
		 public override int WriteBytes( Stream @in, int length )
		 {
			  sbyte[] array = new sbyte[length];
			  int read = @in.Read( array, 0, array.Length );
			  WriteBytes( array, 0, read );
			  return read;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int writeBytes(java.nio.channels.ScatteringByteChannel in, int length) throws java.io.IOException
		 public override int WriteBytes( ScatteringByteChannel @in, int length )
		 {
			  sbyte[] array = new sbyte[length];
			  int read = @in.read( ByteBuffer.wrap( array ) );
			  WriteBytes( array, 0, read );
			  return read;
		 }

		 public override void WriteZero( int length )
		 {
			  for ( int i = 0; i < length; i++ )
			  {
					WriteByte( 0 );
			  }
		 }

		 public override int IndexOf( int fromIndex, int toIndex, sbyte value )
		 {
			  int pos = @delegate.PositionReader( fromIndex );
			  try
			  {
					while ( @delegate.ReaderPosition() < toIndex )
					{
						 int thisPos = @delegate.ReaderPosition();
						 if ( @delegate.Get() == value )
						 {
							  return thisPos;
						 }
					}
					return -1;
			  }
			  catch ( ReadPastEndException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					@delegate.PositionReader( pos );
			  }
		 }

		 public override int IndexOf( int fromIndex, int toIndex, ChannelBufferIndexFinder indexFinder )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int BytesBefore( sbyte value )
		 {
			  int index = IndexOf( ReaderIndex(), WriterIndex(), value );
			  return index == -1 ? -1 : index - ReaderIndex();
		 }

		 public override int BytesBefore( ChannelBufferIndexFinder indexFinder )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int BytesBefore( int length, sbyte value )
		 {
			  return BytesBefore( ReaderIndex(), length, value );
		 }

		 public override int BytesBefore( int length, ChannelBufferIndexFinder indexFinder )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int BytesBefore( int index, int length, sbyte value )
		 {
			  int foundIndex = IndexOf( index, index + length, value );
			  return foundIndex == -1 ? -1 : foundIndex - index;
		 }

		 public override int BytesBefore( int index, int length, ChannelBufferIndexFinder indexFinder )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelBuffer Copy()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelBuffer Copy( int index, int length )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelBuffer Slice()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelBuffer Slice( int index, int length )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelBuffer Duplicate()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ByteBuffer ToByteBuffer()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ByteBuffer ToByteBuffer( int index, int length )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ByteBuffer[] ToByteBuffers()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ByteBuffer[] ToByteBuffers( int index, int length )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool HasArray()
		 {
			  return false;
		 }

		 public override sbyte[] Array()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int ArrayOffset()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override string ToString( Charset charset )
		 {
			  return ToString();
		 }

		 public override string ToString( int index, int length, Charset charset )
		 {
			  return ToString();
		 }

		 public override string ToString( string charsetName )
		 {
			  return ToString();
		 }

		 public override string ToString( string charsetName, ChannelBufferIndexFinder terminatorFinder )
		 {
			  return ToString();
		 }

		 public override string ToString( int index, int length, string charsetName )
		 {
			  return ToString();
		 }

		 public override string ToString( int index, int length, string charsetName, ChannelBufferIndexFinder terminatorFinder )
		 {
			  return ToString();
		 }

		 public override int CompareTo( ChannelBuffer buffer )
		 {
			  throw new System.NotSupportedException();
		 }
	}

}