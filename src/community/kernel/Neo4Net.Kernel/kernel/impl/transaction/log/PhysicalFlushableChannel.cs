using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using ByteUnit = Neo4Net.Io.ByteUnit;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;

	/// <summary>
	/// The main implementation of <seealso cref="FlushableChannel"/>. This class provides buffering over a simple <seealso cref="StoreChannel"/>
	/// and, as a side effect, allows control of the flushing of that buffer to disk.
	/// </summary>
	public class PhysicalFlushableChannel : FlushableChannel
	{
		 public static readonly int DefaultBufferSize = ( int ) ByteUnit.kibiBytes( 512 );

		 private volatile bool _closed;

		 protected internal readonly ByteBuffer Buffer;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal StoreChannel ChannelConflict;

		 public PhysicalFlushableChannel( StoreChannel channel ) : this( channel, DefaultBufferSize )
		 {
		 }

		 public PhysicalFlushableChannel( StoreChannel channel, int bufferSize )
		 {
			  this.ChannelConflict = channel;
			  this.Buffer = ByteBuffer.allocate( bufferSize );
		 }

		 internal virtual LogVersionedStoreChannel Channel
		 {
			 set
			 {
				  this.ChannelConflict = value;
			 }
		 }

		 /// <summary>
		 /// External synchronization between this method and close is required so that they aren't called concurrently.
		 /// Currently that's done by acquiring the PhysicalLogFile monitor.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Flushable prepareForFlush() throws java.io.IOException
		 public override Flushable PrepareForFlush()
		 {
			  Buffer.flip();
			  StoreChannel channel = this.ChannelConflict;
			  try
			  {
					channel.WriteAll( Buffer );
			  }
			  catch ( ClosedChannelException e )
			  {
					HandleClosedChannelException( e );
			  }
			  Buffer.clear();
			  return channel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void handleClosedChannelException(java.nio.channels.ClosedChannelException e) throws java.nio.channels.ClosedChannelException
		 private void HandleClosedChannelException( ClosedChannelException e )
		 {
			  // We don't want to check the closed flag every time we empty, instead we can avoid unnecessary the
			  // volatile read and catch ClosedChannelException where we see if the channel being closed was
			  // deliberate or not. If it was deliberately closed then throw IllegalStateException instead so
			  // that callers won't treat this as a kernel panic.
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "This log channel has been closed", e );
			  }

			  // OK, this channel was closed without us really knowing about it, throw exception as is.
			  throw e;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel put(byte value) throws java.io.IOException
		 public override FlushableChannel Put( sbyte value )
		 {
			  BufferWithGuaranteedSpace( 1 ).put( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putShort(short value) throws java.io.IOException
		 public override FlushableChannel PutShort( short value )
		 {
			  BufferWithGuaranteedSpace( 2 ).putShort( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putInt(int value) throws java.io.IOException
		 public override FlushableChannel PutInt( int value )
		 {
			  BufferWithGuaranteedSpace( 4 ).putInt( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putLong(long value) throws java.io.IOException
		 public override FlushableChannel PutLong( long value )
		 {
			  BufferWithGuaranteedSpace( 8 ).putLong( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putFloat(float value) throws java.io.IOException
		 public override FlushableChannel PutFloat( float value )
		 {
			  BufferWithGuaranteedSpace( 4 ).putFloat( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putDouble(double value) throws java.io.IOException
		 public override FlushableChannel PutDouble( double value )
		 {
			  BufferWithGuaranteedSpace( 8 ).putDouble( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel put(byte[] value, int length) throws java.io.IOException
		 public override FlushableChannel Put( sbyte[] value, int length )
		 {
			  int offset = 0;
			  while ( offset < length )
			  {
					int chunkSize = min( length - offset, Buffer.capacity() >> 1 );
					BufferWithGuaranteedSpace( chunkSize ).put( value, offset, chunkSize );

					offset += chunkSize;
			  }
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ByteBuffer bufferWithGuaranteedSpace(int spaceInBytes) throws java.io.IOException
		 private ByteBuffer BufferWithGuaranteedSpace( int spaceInBytes )
		 {
			  Debug.Assert( spaceInBytes < Buffer.capacity() );
			  if ( Buffer.remaining() < spaceInBytes )
			  {
					PrepareForFlush();
			  }
			  return Buffer;
		 }

		 /// <summary>
		 /// External synchronization between this method and emptyBufferIntoChannelAndClearIt is required so that they
		 /// aren't called concurrently. Currently that's done by acquiring the PhysicalLogFile monitor.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  PrepareForFlush().flush();
			  _closed = true;
			  ChannelConflict.close();
		 }

		 /// <returns> the position of the channel, also taking into account buffer position. </returns>
		 /// <exception cref="IOException"> if underlying channel throws <seealso cref="IOException"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long position() throws java.io.IOException
		 public virtual long Position()
		 {
			  return ChannelConflict.position() + Buffer.position();
		 }

		 /// <summary>
		 /// Sets position of this channel to the new {@code position}. This works only if the underlying channel
		 /// supports positioning.
		 /// </summary>
		 /// <param name="position"> new position (byte offset) to set as new current position. </param>
		 /// <exception cref="IOException"> if underlying channel throws <seealso cref="IOException"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void position(long position) throws java.io.IOException
		 public virtual void Position( long position )
		 {
			  // Currently we take the pessimistic approach of flushing (doesn't imply forcing) buffer to
			  // channel before moving to a new position. This works in all cases, but there could be
			  // made an optimization where we could see that we're moving within the current buffer range
			  // and if so skip flushing and simply move the cursor in the buffer.
			  PrepareForFlush().flush();
			  ChannelConflict.position( position );
		 }
	}

}