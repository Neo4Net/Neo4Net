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

	using ReadPastEndException = Neo4Net.Storageengine.Api.ReadPastEndException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// Implementation of <seealso cref="ReadableClosablePositionAwareChannel"/> operating over a {@code byte[]} in memory.
	/// </summary>
	public class InMemoryClosableChannel : ReadableClosablePositionAwareChannel, FlushablePositionAwareChannel
	{
		 private readonly sbyte[] _bytes;
		 private readonly Reader _reader;
		 private readonly Writer _writer;

		 public InMemoryClosableChannel() : this(1000)
		 {
		 }

		 public InMemoryClosableChannel( sbyte[] bytes, bool append )
		 {
			  this._bytes = bytes;
			  ByteBuffer writeBuffer = ByteBuffer.wrap( this._bytes );
			  ByteBuffer readBuffer = ByteBuffer.wrap( this._bytes );
			  if ( append )
			  {
					writeBuffer.position( bytes.Length );
			  }
			  this._writer = new Writer( this, writeBuffer );
			  this._reader = new Reader( this, readBuffer );
		 }

		 public InMemoryClosableChannel( int bufferSize ) : this( new sbyte[bufferSize], false )
		 {
		 }

		 public virtual void Reset()
		 {
			  _writer.clear();
			  _reader.clear();
			  Arrays.fill( _bytes, ( sbyte ) 0 );
		 }

		 public virtual Reader Reader()
		 {
			  return _reader;
		 }

		 public virtual Writer Writer()
		 {
			  return _writer;
		 }

		 public override InMemoryClosableChannel Put( sbyte b )
		 {
			  _writer.put( b );
			  return this;
		 }

		 public override InMemoryClosableChannel PutShort( short s )
		 {
			  _writer.putShort( s );
			  return this;
		 }

		 public override InMemoryClosableChannel PutInt( int i )
		 {
			  _writer.putInt( i );
			  return this;
		 }

		 public override InMemoryClosableChannel PutLong( long l )
		 {
			  _writer.putLong( l );
			  return this;
		 }

		 public override InMemoryClosableChannel PutFloat( float f )
		 {
			  _writer.putFloat( f );
			  return this;
		 }

		 public override InMemoryClosableChannel PutDouble( double d )
		 {
			  _writer.putDouble( d );
			  return this;
		 }

		 public override InMemoryClosableChannel Put( sbyte[] bytes, int length )
		 {
			  _writer.put( bytes, length );
			  return this;
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override void Close()
		 {
			  _reader.Dispose();
			  _writer.Dispose();
		 }

		 public override Flushable PrepareForFlush()
		 {
			  return _noOpFlushable;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte get() throws org.neo4j.storageengine.api.ReadPastEndException
		 public override sbyte Get()
		 {
			  return _reader.get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public short getShort() throws org.neo4j.storageengine.api.ReadPastEndException
		 public virtual short Short
		 {
			 get
			 {
				  return _reader.Short;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getInt() throws org.neo4j.storageengine.api.ReadPastEndException
		 public virtual int Int
		 {
			 get
			 {
				  return _reader.Int;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLong() throws org.neo4j.storageengine.api.ReadPastEndException
		 public virtual long Long
		 {
			 get
			 {
				  return _reader.Long;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public float getFloat() throws org.neo4j.storageengine.api.ReadPastEndException
		 public virtual float Float
		 {
			 get
			 {
				  return _reader.Float;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double getDouble() throws org.neo4j.storageengine.api.ReadPastEndException
		 public virtual double Double
		 {
			 get
			 {
				  return _reader.Double;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void get(byte[] bytes, int length) throws org.neo4j.storageengine.api.ReadPastEndException
		 public override void Get( sbyte[] bytes, int length )
		 {
			  _reader.get( bytes, length );
		 }

		 public override LogPositionMarker GetCurrentPosition( LogPositionMarker positionMarker )
		 {
			  // Hmm, this would be for the writer.
			  return _writer.getCurrentPosition( positionMarker );
		 }

		 public virtual int PositionWriter( int position )
		 {
			  int previous = _writer.position();
			  _writer.position( position );
			  return previous;
		 }

		 public virtual int PositionReader( int position )
		 {
			  int previous = _reader.position();
			  _reader.position( position );
			  return previous;
		 }

		 public virtual int ReaderPosition()
		 {
			  return _reader.position();
		 }

		 public virtual int WriterPosition()
		 {
			  return _writer.position();
		 }

		 public virtual void TruncateTo( int offset )
		 {
			  _reader.limit( offset );
		 }

		 public virtual int Capacity()
		 {
			  return _bytes.Length;
		 }

		 public virtual int AvailableBytesToRead()
		 {
			  return _reader.remaining();
		 }

		 public virtual int AvailableBytesToWrite()
		 {
			  return _writer.remaining();
		 }

		 private static readonly Flushable _noOpFlushable = () =>
		 {
		 };

		 internal class ByteBufferBase : PositionAwareChannel, System.IDisposable
		 {
			 private readonly InMemoryClosableChannel _outerInstance;

			  protected internal readonly ByteBuffer Buffer;

			  internal ByteBufferBase( InMemoryClosableChannel outerInstance, ByteBuffer buffer )
			  {
				  this._outerInstance = outerInstance;
					this.Buffer = buffer;
			  }

			  internal virtual void Clear()
			  {
					Buffer.clear();
			  }

			  internal virtual int Position()
			  {
					return Buffer.position();
			  }

			  internal virtual void Position( int position )
			  {
					Buffer.position( position );
			  }

			  internal virtual int Remaining()
			  {
					return Buffer.remaining();
			  }

			  internal virtual void Limit( int offset )
			  {
					Buffer.limit( offset );
			  }

			  public override void Close()
			  {
			  }

			  public override LogPositionMarker GetCurrentPosition( LogPositionMarker positionMarker )
			  {
					positionMarker.Mark( 0, Buffer.position() );
					return positionMarker;
			  }
		 }

		 public class Reader : ByteBufferBase, ReadableClosablePositionAwareChannel, PositionableChannel
		 {
			 private readonly InMemoryClosableChannel _outerInstance;

			  internal Reader( InMemoryClosableChannel outerInstance, ByteBuffer buffer ) : base( outerInstance, buffer )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte get() throws org.neo4j.storageengine.api.ReadPastEndException
			  public override sbyte Get()
			  {
					EnsureAvailableToRead( 1 );
					return Buffer.get();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public short getShort() throws org.neo4j.storageengine.api.ReadPastEndException
			  public virtual short Short
			  {
				  get
				  {
						EnsureAvailableToRead( 2 );
						return Buffer.Short;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getInt() throws org.neo4j.storageengine.api.ReadPastEndException
			  public virtual int Int
			  {
				  get
				  {
						EnsureAvailableToRead( 4 );
						return Buffer.Int;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLong() throws org.neo4j.storageengine.api.ReadPastEndException
			  public virtual long Long
			  {
				  get
				  {
						EnsureAvailableToRead( 8 );
						return Buffer.Long;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public float getFloat() throws org.neo4j.storageengine.api.ReadPastEndException
			  public virtual float Float
			  {
				  get
				  {
						EnsureAvailableToRead( 4 );
						return Buffer.Float;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double getDouble() throws org.neo4j.storageengine.api.ReadPastEndException
			  public virtual double Double
			  {
				  get
				  {
						EnsureAvailableToRead( 8 );
						return Buffer.Double;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void get(byte[] bytes, int length) throws org.neo4j.storageengine.api.ReadPastEndException
			  public override void Get( sbyte[] bytes, int length )
			  {
					EnsureAvailableToRead( length );
					Buffer.get( bytes, 0, length );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureAvailableToRead(int i) throws org.neo4j.storageengine.api.ReadPastEndException
			  internal virtual void EnsureAvailableToRead( int i )
			  {
					if ( Remaining() < i || Position() + i > outerInstance.writer.Position() )
					{
						 throw ReadPastEndException.INSTANCE;
					}
			  }

			  public virtual long CurrentPosition
			  {
				  set
				  {
						Buffer.position( toIntExact( value ) );
				  }
			  }
		 }

		 public class Writer : ByteBufferBase, FlushablePositionAwareChannel
		 {
			 private readonly InMemoryClosableChannel _outerInstance;

			  internal Writer( InMemoryClosableChannel outerInstance, ByteBuffer buffer ) : base( outerInstance, buffer )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override Writer Put( sbyte b )
			  {
					Buffer.put( b );
					return this;
			  }

			  public override Writer PutShort( short s )
			  {
					Buffer.putShort( s );
					return this;
			  }

			  public override Writer PutInt( int i )
			  {
					Buffer.putInt( i );
					return this;
			  }

			  public override Writer PutLong( long l )
			  {
					Buffer.putLong( l );
					return this;
			  }

			  public override Writer PutFloat( float f )
			  {
					Buffer.putFloat( f );
					return this;
			  }

			  public override Writer PutDouble( double d )
			  {
					Buffer.putDouble( d );
					return this;
			  }

			  public override Writer Put( sbyte[] bytes, int length )
			  {
					Buffer.put( bytes, 0, length );
					return this;
			  }

			  public override Flushable PrepareForFlush()
			  {
					return _noOpFlushable;
			  }
		 }
	}

}