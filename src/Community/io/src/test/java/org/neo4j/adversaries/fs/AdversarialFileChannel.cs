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
namespace Neo4Net.Adversaries.fs
{

	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using StoreFileChannel = Neo4Net.Io.fs.StoreFileChannel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AdversarialFileChannel extends org.neo4j.io.fs.StoreFileChannel
	public class AdversarialFileChannel : StoreFileChannel
	{
		 private readonly StoreChannel @delegate;
		 private readonly Adversary _adversary;

		 public static StoreFileChannel Wrap( StoreFileChannel channel, Adversary adversary )
		 {
			  return new AdversarialFileChannel( channel, adversary );
		 }

		 private AdversarialFileChannel( StoreFileChannel channel, Adversary adversary ) : base( channel )
		 {
			  this.@delegate = channel;
			  this._adversary = adversary;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs) throws java.io.IOException
		 public override long Write( ByteBuffer[] srcs )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					ByteBuffer mischievousBuffer = srcs[srcs.Length - 1];
					int oldLimit = MischiefLimit( mischievousBuffer );
					long written = @delegate.write( srcs );
					mischievousBuffer.limit( oldLimit );
					return written;
			  }
			  return @delegate.write( srcs );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src, long position )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  @delegate.WriteAll( src, position );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  @delegate.WriteAll( src );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs, int offset, int length) throws java.io.IOException
		 public override long Write( ByteBuffer[] srcs, int offset, int length )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					length = length == 1 ? 1 : length / 2;
					ByteBuffer mischievousBuffer = srcs[offset + length - 1];
					int oldLimit = MischiefLimit( mischievousBuffer );
					long written = @delegate.write( srcs, offset, length );
					mischievousBuffer.limit( oldLimit );
					return written;
			  }
			  return @delegate.write( srcs, offset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreFileChannel truncate(long size) throws java.io.IOException
		 public override StoreFileChannel Truncate( long size )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return ( StoreFileChannel ) @delegate.Truncate( size );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreFileChannel position(long newPosition) throws java.io.IOException
		 public override StoreFileChannel Position( long newPosition )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return ( StoreFileChannel ) @delegate.Position( newPosition );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst, long position) throws java.io.IOException
		 public override int Read( ByteBuffer dst, long position )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					int oldLimit = MischiefLimit( dst );
					int read = @delegate.Read( dst, position );
					dst.limit( oldLimit );
					return read;
			  }
			  return @delegate.Read( dst, position );
		 }

		 private int MischiefLimit( ByteBuffer buf )
		 {
			  int oldLimit = buf.limit();
			  int newLimit = oldLimit - buf.remaining() / 2;
			  buf.limit( newLimit );
			  return oldLimit;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(boolean metaData) throws java.io.IOException
		 public override void Force( bool metaData )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  @delegate.Force( metaData );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
		 public override int Read( ByteBuffer dst )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					int oldLimit = MischiefLimit( dst );
					int read = @delegate.read( dst );
					dst.limit( oldLimit );
					return read;
			  }
			  return @delegate.read( dst );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts, int offset, int length) throws java.io.IOException
		 public override long Read( ByteBuffer[] dsts, int offset, int length )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					ByteBuffer lastBuf = dsts[dsts.Length - 1];
					int oldLimit = MischiefLimit( lastBuf );
					long read = @delegate.read( dsts, offset, length );
					lastBuf.limit( oldLimit );
					return read;
			  }
			  return @delegate.read( dsts, offset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long position() throws java.io.IOException
		 public override long Position()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return @delegate.position();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.channels.FileLock tryLock() throws java.io.IOException
		 public override FileLock TryLock()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return @delegate.TryLock();
		 }

		 public override bool Open
		 {
			 get
			 {
				  _adversary.injectFailure();
				  return @delegate.Open;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts) throws java.io.IOException
		 public override long Read( ByteBuffer[] dsts )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					ByteBuffer lastBuf = dsts[dsts.Length - 1];
					int oldLimit = MischiefLimit( lastBuf );
					long read = @delegate.read( dsts );
					lastBuf.limit( oldLimit );
					return read;
			  }
			  return @delegate.read( dsts );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer src) throws java.io.IOException
		 public override int Write( ByteBuffer src )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					int oldLimit = MischiefLimit( src );
					int written = @delegate.write( src );
					src.limit( oldLimit );
					return written;
			  }
			  return @delegate.write( src );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  @delegate.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long size() throws java.io.IOException
		 public override long Size()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return @delegate.size();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  Force( false );
		 }
	}

}