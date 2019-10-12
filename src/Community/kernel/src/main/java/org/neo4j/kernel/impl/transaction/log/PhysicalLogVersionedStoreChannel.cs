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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderReader.readLogHeader;

	public class PhysicalLogVersionedStoreChannel : LogVersionedStoreChannel
	{
		 private readonly StoreChannel _delegateChannel;
		 private readonly long _version;
		 private readonly sbyte _formatVersion;
		 private long _position;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PhysicalLogVersionedStoreChannel(org.neo4j.io.fs.StoreChannel delegateChannel, long version, byte formatVersion) throws java.io.IOException
		 public PhysicalLogVersionedStoreChannel( StoreChannel delegateChannel, long version, sbyte formatVersion )
		 {
			  this._delegateChannel = delegateChannel;
			  this._version = version;
			  this._formatVersion = formatVersion;
			  this._position = delegateChannel.position();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.channels.FileLock tryLock() throws java.io.IOException
		 public override FileLock TryLock()
		 {
			  return _delegateChannel.tryLock();
		 }

		 public override void WriteAll( ByteBuffer src, long position )
		 {
			  throw new System.NotSupportedException( "Not needed" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src) throws java.io.IOException
		 public override void WriteAll( ByteBuffer src )
		 {
			  Advance( src.remaining() );
			  _delegateChannel.writeAll( src );
		 }

		 public override int Read( ByteBuffer dst, long position )
		 {
			  throw new System.NotSupportedException( "Not needed" );
		 }

		 public override void ReadAll( ByteBuffer dst )
		 {
			  throw new System.NotSupportedException( "Not needed" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(boolean metaData) throws java.io.IOException
		 public override void Force( bool metaData )
		 {
			  _delegateChannel.force( metaData );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel position(long newPosition) throws java.io.IOException
		 public override StoreChannel Position( long newPosition )
		 {
			  this._position = newPosition;
			  return _delegateChannel.position( newPosition );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel truncate(long size) throws java.io.IOException
		 public override StoreChannel Truncate( long size )
		 {
			  return _delegateChannel.truncate( size );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
		 public override int Read( ByteBuffer dst )
		 {
			  return ( int ) Advance( _delegateChannel.read( dst ) );
		 }

		 private long Advance( long bytes )
		 {
			  if ( bytes != -1 )
			  {
					_position += bytes;
			  }
			  return bytes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer src) throws java.io.IOException
		 public override int Write( ByteBuffer src )
		 {
			  return ( int ) Advance( _delegateChannel.write( src ) );
		 }

		 public override long Position()
		 {
			  return _position;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long size() throws java.io.IOException
		 public override long Size()
		 {
			  return _delegateChannel.size();
		 }

		 public override bool Open
		 {
			 get
			 {
				  return _delegateChannel.Open;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _delegateChannel.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs, int offset, int length) throws java.io.IOException
		 public override long Write( ByteBuffer[] srcs, int offset, int length )
		 {
			  return Advance( _delegateChannel.write( srcs, offset, length ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs) throws java.io.IOException
		 public override long Write( ByteBuffer[] srcs )
		 {
			  return Advance( _delegateChannel.write( srcs ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts, int offset, int length) throws java.io.IOException
		 public override long Read( ByteBuffer[] dsts, int offset, int length )
		 {
			  return Advance( _delegateChannel.read( dsts, offset, length ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts) throws java.io.IOException
		 public override long Read( ByteBuffer[] dsts )
		 {
			  return Advance( _delegateChannel.read( dsts ) );
		 }

		 public virtual long Version
		 {
			 get
			 {
				  return _version;
			 }
		 }

		 public virtual sbyte LogFormatVersion
		 {
			 get
			 {
				  return _formatVersion;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  PhysicalLogVersionedStoreChannel that = ( PhysicalLogVersionedStoreChannel ) o;

			  return _version == that._version && _delegateChannel.Equals( that._delegateChannel );
		 }

		 public override int GetHashCode()
		 {
			  int result = _delegateChannel.GetHashCode();
			  result = 31 * result + ( int )( _version ^ ( ( long )( ( ulong )_version >> 32 ) ) );
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  Force( false );
		 }
	}

}