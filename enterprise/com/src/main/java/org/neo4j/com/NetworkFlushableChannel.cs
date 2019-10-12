/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.com
{

	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;

	using FlushableChannel = Org.Neo4j.Kernel.impl.transaction.log.FlushableChannel;

	public class NetworkFlushableChannel : Flushable, FlushableChannel
	{
		 private readonly ChannelBuffer @delegate;

		 public NetworkFlushableChannel( ChannelBuffer @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override void Flush()
		 {
		 }

		 public override FlushableChannel Put( sbyte value )
		 {
			  @delegate.writeByte( value );
			  return this;
		 }

		 public override FlushableChannel PutShort( short value )
		 {
			  @delegate.writeShort( value );
			  return this;
		 }

		 public override FlushableChannel PutInt( int value )
		 {
			  @delegate.writeInt( value );
			  return this;
		 }

		 public override FlushableChannel PutLong( long value )
		 {
			  @delegate.writeLong( value );
			  return this;
		 }

		 public override FlushableChannel PutFloat( float value )
		 {
			  @delegate.writeFloat( value );
			  return this;
		 }

		 public override FlushableChannel PutDouble( double value )
		 {
			  @delegate.writeDouble( value );
			  return this;
		 }

		 public override FlushableChannel Put( sbyte[] value, int length )
		 {
			  @delegate.writeBytes( value, 0, length );
			  return this;
		 }

		 public override void Close()
		 {
		 }

		 public override Flushable PrepareForFlush()
		 {
			  return this;
		 }
	}

}