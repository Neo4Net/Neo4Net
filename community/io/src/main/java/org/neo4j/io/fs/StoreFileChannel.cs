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
namespace Org.Neo4j.Io.fs
{

	public class StoreFileChannel : StoreChannel
	{
		 private readonly FileChannel _channel;

		 public StoreFileChannel( FileChannel channel )
		 {
			  this._channel = channel;
		 }

		 public StoreFileChannel( StoreFileChannel channel )
		 {
			  this._channel = channel._channel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs) throws java.io.IOException
		 public override long Write( ByteBuffer[] srcs )
		 {
			  return _channel.write( srcs );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs, int offset, int length) throws java.io.IOException
		 public override long Write( ByteBuffer[] srcs, int offset, int length )
		 {
			  return _channel.write( srcs, offset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src, long position )
		 {
			  long filePosition = position;
			  long expectedEndPosition = filePosition + src.limit() - src.position();
			  int bytesWritten;
			  while ( ( filePosition += bytesWritten = _channel.write( src, filePosition ) ) < expectedEndPosition )
			  {
					if ( bytesWritten < 0 )
					{
						 throw new IOException( "Unable to write to disk, reported bytes written was " + bytesWritten );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src )
		 {
			  long bytesToWrite = src.limit() - src.position();
			  int bytesWritten;
			  while ( ( bytesToWrite -= bytesWritten = Write( src ) ) > 0 )
			  {
					if ( bytesWritten < 0 )
					{
						 throw new IOException( "Unable to write to disk, reported bytes written was " + bytesWritten );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreFileChannel truncate(long size) throws java.io.IOException
		 public override StoreFileChannel Truncate( long size )
		 {
			  _channel.truncate( size );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreFileChannel position(long newPosition) throws java.io.IOException
		 public override StoreFileChannel Position( long newPosition )
		 {
			  _channel.position( newPosition );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst, long position) throws java.io.IOException
		 public override int Read( ByteBuffer dst, long position )
		 {
			  return _channel.read( dst, position );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readAll(ByteBuffer dst) throws java.io.IOException
		 public override void ReadAll( ByteBuffer dst )
		 {
			  while ( dst.hasRemaining() )
			  {
					int bytesRead = _channel.read( dst );
					if ( bytesRead < 0 )
					{
						 throw new System.InvalidOperationException( "Channel has reached end-of-stream." );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(boolean metaData) throws java.io.IOException
		 public override void Force( bool metaData )
		 {
			  _channel.force( metaData );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
		 public override int Read( ByteBuffer dst )
		 {
			  return _channel.read( dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts, int offset, int length) throws java.io.IOException
		 public override long Read( ByteBuffer[] dsts, int offset, int length )
		 {
			  return _channel.read( dsts, offset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long position() throws java.io.IOException
		 public override long Position()
		 {
			  return _channel.position();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.channels.FileLock tryLock() throws java.io.IOException
		 public override FileLock TryLock()
		 {
			  return _channel.tryLock();
		 }

		 public override bool Open
		 {
			 get
			 {
				  return _channel.Open;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts) throws java.io.IOException
		 public override long Read( ByteBuffer[] dsts )
		 {
			  return _channel.read( dsts );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer src) throws java.io.IOException
		 public override int Write( ByteBuffer src )
		 {
			  return _channel.write( src );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _channel.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long size() throws java.io.IOException
		 public override long Size()
		 {
			  return _channel.size();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  Force( false );
		 }

		 internal static FileChannel Unwrap( StoreChannel channel )
		 {
			  StoreFileChannel sfc = ( StoreFileChannel ) channel;
			  return sfc._channel;
		 }
	}

}