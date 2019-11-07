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

	using LogPositionMarker = Neo4Net.Kernel.impl.transaction.log.LogPositionMarker;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using ReadPastEndException = Neo4Net.Kernel.Api.StorageEngine.ReadPastEndException;

	public class NetworkReadableClosableChannelNetty4 : ReadableClosablePositionAwareChannel
	{
		 private readonly ByteBuf @delegate;

		 public NetworkReadableClosableChannelNetty4( ByteBuf input )
		 {
			  this.@delegate = input;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte get() throws java.io.IOException
		 public override sbyte Get()
		 {
			  EnsureBytes( Byte.BYTES );
			  return @delegate.readByte();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public short getShort() throws java.io.IOException
		 public virtual short Short
		 {
			 get
			 {
				  EnsureBytes( Short.BYTES );
				  return @delegate.readShort();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getInt() throws java.io.IOException
		 public virtual int Int
		 {
			 get
			 {
				  EnsureBytes( Integer.BYTES );
				  return @delegate.readInt();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLong() throws java.io.IOException
		 public virtual long Long
		 {
			 get
			 {
				  EnsureBytes( Long.BYTES );
				  return @delegate.readLong();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public float getFloat() throws java.io.IOException
		 public virtual float Float
		 {
			 get
			 {
				  EnsureBytes( Float.BYTES );
				  return @delegate.readFloat();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double getDouble() throws java.io.IOException
		 public virtual double Double
		 {
			 get
			 {
				  EnsureBytes( Double.BYTES );
				  return @delegate.readDouble();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void get(byte[] bytes, int length) throws java.io.IOException
		 public override void Get( sbyte[] bytes, int length )
		 {
			  EnsureBytes( length );
			  @delegate.readBytes( bytes, 0, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureBytes(int byteCount) throws Neo4Net.Kernel.Api.StorageEngine.ReadPastEndException
		 private void EnsureBytes( int byteCount )
		 {
			  if ( @delegate.readableBytes() < byteCount )
			  {
					throw ReadPastEndException.INSTANCE;
			  }
		 }

		 public override LogPositionMarker GetCurrentPosition( LogPositionMarker positionMarker )
		 {
			  positionMarker.Unspecified();
			  return positionMarker;
		 }

		 public override void Close()
		 {
			  // no op
		 }
	}

}