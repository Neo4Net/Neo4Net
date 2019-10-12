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
namespace Org.Neo4j.Test.limited
{

	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;

	public class LimitedFileChannel : StoreChannel
	{
		 private readonly StoreChannel _inner;
		 private readonly LimitedFilesystemAbstraction _fs;

		 public LimitedFileChannel( StoreChannel inner, LimitedFilesystemAbstraction limitedFilesystemAbstraction )
		 {
			  this._inner = inner;
			  _fs = limitedFilesystemAbstraction;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer byteBuffer) throws java.io.IOException
		 public override int Read( ByteBuffer byteBuffer )
		 {
			  return _inner.read( byteBuffer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] byteBuffers, int i, int i1) throws java.io.IOException
		 public override long Read( ByteBuffer[] byteBuffers, int i, int i1 )
		 {
			  return _inner.read( byteBuffers, i, i1 );
		 }

		 public override long Read( ByteBuffer[] dsts )
		 {
			  return 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer byteBuffer) throws java.io.IOException
		 public override int Write( ByteBuffer byteBuffer )
		 {
			  _fs.ensureHasSpace();
			  return _inner.write( byteBuffer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] byteBuffers, int i, int i1) throws java.io.IOException
		 public override long Write( ByteBuffer[] byteBuffers, int i, int i1 )
		 {
			  _fs.ensureHasSpace();
			  return _inner.write( byteBuffers, i, i1 );
		 }

		 public override long Write( ByteBuffer[] srcs )
		 {
			  return 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long position() throws java.io.IOException
		 public override long Position()
		 {
			  return _inner.position();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LimitedFileChannel position(long l) throws java.io.IOException
		 public override LimitedFileChannel Position( long l )
		 {
			  return new LimitedFileChannel( _inner.position( l ), _fs );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long size() throws java.io.IOException
		 public override long Size()
		 {
			  return _inner.size();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LimitedFileChannel truncate(long l) throws java.io.IOException
		 public override LimitedFileChannel Truncate( long l )
		 {
			  return new LimitedFileChannel( _inner.truncate( l ), _fs );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(boolean b) throws java.io.IOException
		 public override void Force( bool b )
		 {
			  _fs.ensureHasSpace();
			  _inner.force( b );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer byteBuffer, long l) throws java.io.IOException
		 public override int Read( ByteBuffer byteBuffer, long l )
		 {
			  return _inner.read( byteBuffer, l );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readAll(ByteBuffer dst) throws java.io.IOException
		 public override void ReadAll( ByteBuffer dst )
		 {
			  _inner.readAll( dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.channels.FileLock tryLock() throws java.io.IOException
		 public override FileLock TryLock()
		 {
			  return _inner.tryLock();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src, long position )
		 {
			  _fs.ensureHasSpace();
			  _inner.writeAll( src, position );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src )
		 {
			  _fs.ensureHasSpace();
			  _inner.writeAll( src );
		 }

		 public override bool Open
		 {
			 get
			 {
				  return _inner.Open;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _inner.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  _inner.flush();
		 }
	}

}