/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.messaging
{
	using ByteBuf = io.netty.buffer.ByteBuf;

	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.mebiBytes;

	public class BoundedNetworkWritableChannel : WritableChannel, ByteBufBacked
	{
		 /// <summary>
		 /// This implementation puts an upper limit to the size of the state serialized in the buffer. The default
		 /// value for that should be sufficient for all replicated state except for transactions, the size of which
		 /// is unbounded.
		 /// </summary>
		 private static readonly long _defaultSizeLimit = mebiBytes( 2 );

		 private readonly ByteBuf @delegate;
		 private readonly int _initialWriterIndex;

		 private readonly long _sizeLimit;

		 public BoundedNetworkWritableChannel( ByteBuf @delegate ) : this( @delegate, _defaultSizeLimit )
		 {
		 }

		 public BoundedNetworkWritableChannel( ByteBuf @delegate, long sizeLimit )
		 {
			  this.@delegate = @delegate;
			  this._initialWriterIndex = @delegate.writerIndex();
			  this._sizeLimit = sizeLimit;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.Kernel.Api.StorageEngine.WritableChannel put(byte value) throws MessageTooBigException
		 public override WritableChannel Put( sbyte value )
		 {
			  CheckSize( Byte.BYTES );
			  @delegate.writeByte( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.Kernel.Api.StorageEngine.WritableChannel putShort(short value) throws MessageTooBigException
		 public override WritableChannel PutShort( short value )
		 {
			  CheckSize( Short.BYTES );
			  @delegate.writeShort( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.Kernel.Api.StorageEngine.WritableChannel putInt(int value) throws MessageTooBigException
		 public override WritableChannel PutInt( int value )
		 {
			  CheckSize( Integer.BYTES );
			  @delegate.writeInt( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.Kernel.Api.StorageEngine.WritableChannel putLong(long value) throws MessageTooBigException
		 public override WritableChannel PutLong( long value )
		 {
			  CheckSize( Long.BYTES );
			  @delegate.writeLong( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.Kernel.Api.StorageEngine.WritableChannel putFloat(float value) throws MessageTooBigException
		 public override WritableChannel PutFloat( float value )
		 {
			  CheckSize( Float.BYTES );
			  @delegate.writeFloat( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.Kernel.Api.StorageEngine.WritableChannel putDouble(double value) throws MessageTooBigException
		 public override WritableChannel PutDouble( double value )
		 {
			  CheckSize( Double.BYTES );
			  @delegate.writeDouble( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.Kernel.Api.StorageEngine.WritableChannel put(byte[] value, int length) throws MessageTooBigException
		 public override WritableChannel Put( sbyte[] value, int length )
		 {
			  CheckSize( length );
			  @delegate.writeBytes( value, 0, length );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkSize(int additional) throws MessageTooBigException
		 private void CheckSize( int additional )
		 {
			  int writtenSoFar = @delegate.writerIndex() - _initialWriterIndex;
			  int countToCheck = writtenSoFar + additional;
			  if ( countToCheck > _sizeLimit )
			  {
					throw new MessageTooBigException( format( "Size limit exceeded. Limit is %d, wanted to write %d with the writer index at %d (started at %d), written so far %d", _sizeLimit, additional, @delegate.writerIndex(), _initialWriterIndex, writtenSoFar ) );
			  }
		 }

		 public override ByteBuf ByteBuf()
		 {
			  return @delegate;
		 }
	}

}