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

	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class NetworkWritableChannel : WritableChannel, ByteBufBacked
	{
		 private readonly ByteBuf @delegate;

		 public NetworkWritableChannel( ByteBuf byteBuf )
		 {
			  this.@delegate = byteBuf;
		 }

		 public override WritableChannel Put( sbyte value )
		 {
			  @delegate.writeByte( value );
			  return this;
		 }

		 public override WritableChannel PutShort( short value )
		 {
			  @delegate.writeShort( value );
			  return this;
		 }

		 public override WritableChannel PutInt( int value )
		 {
			  @delegate.writeInt( value );
			  return this;
		 }

		 public override WritableChannel PutLong( long value )
		 {
			  @delegate.writeLong( value );
			  return this;
		 }

		 public override WritableChannel PutFloat( float value )
		 {
			  @delegate.writeFloat( value );
			  return this;
		 }

		 public override WritableChannel PutDouble( double value )
		 {
			  @delegate.writeDouble( value );
			  return this;
		 }

		 public override WritableChannel Put( sbyte[] value, int length )
		 {
			  @delegate.writeBytes( value, 0, length );
			  return this;
		 }

		 public override ByteBuf ByteBuf()
		 {
			  return @delegate;
		 }
	}

}