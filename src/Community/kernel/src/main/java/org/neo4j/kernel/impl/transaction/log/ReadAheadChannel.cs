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

	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using ReadPastEndException = Neo4Net.Storageengine.Api.ReadPastEndException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.kibiBytes;

	/// <summary>
	/// A buffering implementation of <seealso cref="ReadableClosableChannel"/>. This class also allows subclasses to read content
	/// spanning more than one file, by properly implementing <seealso cref="next(StoreChannel)"/>. </summary>
	/// @param <T> The type of StoreChannel wrapped </param>
	public class ReadAheadChannel<T> : ReadableClosableChannel, PositionableChannel where T : Neo4Net.Io.fs.StoreChannel
	{
		 public static readonly int DefaultReadAheadSize = toIntExact( kibiBytes( 4 ) );

		 protected internal T Channel;
		 private readonly ByteBuffer _aheadBuffer;
		 private readonly int _readAheadSize;

		 public ReadAheadChannel( T channel ) : this( channel, DefaultReadAheadSize )
		 {
		 }

		 public ReadAheadChannel( T channel, int readAheadSize ) : this( channel, ByteBuffer.allocate( readAheadSize ) )
		 {
		 }

		 public ReadAheadChannel( T channel, ByteBuffer byteBuffer )
		 {
			  this._aheadBuffer = byteBuffer;
			  this._aheadBuffer.position( _aheadBuffer.capacity() );
			  this.Channel = channel;
			  this._readAheadSize = byteBuffer.capacity();
		 }

		 /// <summary>
		 /// This is the position within the buffered stream (and not the
		 /// underlying channel, which will generally be further ahead).
		 /// </summary>
		 /// <returns> The position within the buffered stream. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long position() throws java.io.IOException
		 public virtual long Position()
		 {
			  return Channel.position() - _aheadBuffer.remaining();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte get() throws java.io.IOException
		 public override sbyte Get()
		 {
			  EnsureDataExists( 1 );
			  return _aheadBuffer.get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public short getShort() throws java.io.IOException
		 public virtual short Short
		 {
			 get
			 {
				  EnsureDataExists( 2 );
				  return _aheadBuffer.Short;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getInt() throws java.io.IOException
		 public virtual int Int
		 {
			 get
			 {
				  EnsureDataExists( 4 );
				  return _aheadBuffer.Int;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLong() throws java.io.IOException
		 public virtual long Long
		 {
			 get
			 {
				  EnsureDataExists( 8 );
				  return _aheadBuffer.Long;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public float getFloat() throws java.io.IOException
		 public virtual float Float
		 {
			 get
			 {
				  EnsureDataExists( 4 );
				  return _aheadBuffer.Float;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double getDouble() throws java.io.IOException
		 public virtual double Double
		 {
			 get
			 {
				  EnsureDataExists( 8 );
				  return _aheadBuffer.Double;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void get(byte[] bytes, int length) throws java.io.IOException
		 public override void Get( sbyte[] bytes, int length )
		 {
			  Debug.Assert( length <= bytes.Length );

			  int bytesGotten = 0;
			  while ( bytesGotten < length )
			  { // get max 1024 bytes at the time, so that ensureDataExists functions as it should
					int chunkSize = min( _readAheadSize >> 2, length - bytesGotten );
					EnsureDataExists( chunkSize );
					_aheadBuffer.get( bytes, bytesGotten, chunkSize );
					bytesGotten += chunkSize;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  Channel.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureDataExists(int requestedNumberOfBytes) throws java.io.IOException
		 private void EnsureDataExists( int requestedNumberOfBytes )
		 {
			  int remaining = _aheadBuffer.remaining();
			  if ( remaining >= requestedNumberOfBytes )
			  {
					return;
			  }

			  // We ran out, try to read some more
			  // start by copying the remaining bytes to the beginning
			  _aheadBuffer.compact();

			  while ( _aheadBuffer.position() < _aheadBuffer.capacity() )
			  { // read from the current channel to try and fill the buffer
					int read = Channel.read( _aheadBuffer );
					if ( read == -1 )
					{
						 // current channel ran out...
						 if ( _aheadBuffer.position() >= requestedNumberOfBytes )
						 { // ...although we have satisfied the request
							  break;
						 }

						 // ... we need to read even further, into the next version
						 T nextChannel = Next( Channel );
						 Debug.Assert( nextChannel != null );
						 if ( nextChannel == Channel )
						 {
							  // no more channels so we cannot satisfy the requested number of bytes
							  _aheadBuffer.flip();
							  throw ReadPastEndException.INSTANCE;
						 }
						 Channel = nextChannel;
					}
			  }
			  // prepare for reading
			  _aheadBuffer.flip();
		 }

		 /// <summary>
		 /// Hook for allowing subclasses to read content spanning a sequence of files. This method is called when the current
		 /// file channel is exhausted and a new channel is required for reading. The default implementation returns the
		 /// argument, which is the condition for indicating no more content, resulting in a <seealso cref="ReadPastEndException"/> being
		 /// thrown. </summary>
		 /// <param name="channel"> The channel that has just been exhausted. </param>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected T next(T channel) throws java.io.IOException
		 protected internal virtual T Next( T channel )
		 {
			  return channel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setCurrentPosition(long byteOffset) throws java.io.IOException
		 public virtual long CurrentPosition
		 {
			 set
			 {
				  long positionRelativeToAheadBuffer = value - ( Channel.position() - _aheadBuffer.limit() );
				  if ( positionRelativeToAheadBuffer >= _aheadBuffer.limit() || positionRelativeToAheadBuffer < 0 )
				  {
						// Beyond what we currently have buffered
						_aheadBuffer.position( _aheadBuffer.limit() );
						Channel.position( value );
				  }
				  else
				  {
						_aheadBuffer.position( toIntExact( positionRelativeToAheadBuffer ) );
				  }
			 }
		 }
	}

}