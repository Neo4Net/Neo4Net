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

	/// <summary>
	/// A wrapper for a channel with an offset. Users must make
	/// sure that the channel is within bounds or exceptions
	/// will occur.
	/// </summary>
	public class OffsetChannel : StoreChannel
	{
		 private readonly StoreChannel @delegate;
		 private readonly long _offset;

		 public OffsetChannel( StoreChannel @delegate, long offset )
		 {
			  this.@delegate = @delegate;
			  this._offset = offset;
		 }

		 private long Offset( long position )
		 {
			  if ( position < 0 )
			  {
					throw new System.ArgumentException( "Position must be >= 0." );
			  }
			  return Math.addExact( position, _offset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.channels.FileLock tryLock() throws java.io.IOException
		 public override FileLock TryLock()
		 {
			  return @delegate.TryLock();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src, long position )
		 {
			  @delegate.WriteAll( src, Offset( position ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src )
		 {
			  @delegate.WriteAll( src );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst, long position) throws java.io.IOException
		 public override int Read( ByteBuffer dst, long position )
		 {
			  return @delegate.Read( dst, Offset( position ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readAll(ByteBuffer dst) throws java.io.IOException
		 public override void ReadAll( ByteBuffer dst )
		 {
			  @delegate.ReadAll( dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(boolean metaData) throws java.io.IOException
		 public override void Force( bool metaData )
		 {
			  @delegate.Force( metaData );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
		 public override int Read( ByteBuffer dst )
		 {
			  return @delegate.read( dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer src) throws java.io.IOException
		 public override int Write( ByteBuffer src )
		 {
			  return @delegate.write( src );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long position() throws java.io.IOException
		 public override long Position()
		 {
			  return @delegate.position() - _offset;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreChannel position(long newPosition) throws java.io.IOException
		 public override StoreChannel Position( long newPosition )
		 {
			  return @delegate.Position( Offset( newPosition ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long size() throws java.io.IOException
		 public override long Size()
		 {
			  return @delegate.size() - _offset;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreChannel truncate(long size) throws java.io.IOException
		 public override StoreChannel Truncate( long size )
		 {
			  return @delegate.Truncate( Offset( size ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  @delegate.flush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs, int offset, int length) throws java.io.IOException
		 public override long Write( ByteBuffer[] srcs, int offset, int length )
		 {
			  return @delegate.write( srcs, offset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs) throws java.io.IOException
		 public override long Write( ByteBuffer[] srcs )
		 {
			  return @delegate.write( srcs );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts, int offset, int length) throws java.io.IOException
		 public override long Read( ByteBuffer[] dsts, int offset, int length )
		 {
			  return @delegate.read( dsts, offset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts) throws java.io.IOException
		 public override long Read( ByteBuffer[] dsts )
		 {
			  return @delegate.read( dsts );
		 }

		 public override bool Open
		 {
			 get
			 {
				  return @delegate.Open;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  @delegate.close();
		 }
	}

}