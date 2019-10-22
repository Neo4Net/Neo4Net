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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;

	using BoundedNetworkWritableChannel = Neo4Net.causalclustering.messaging.BoundedNetworkWritableChannel;
	using StoreIdMarshal = Neo4Net.causalclustering.messaging.marshalling.storeid.StoreIdMarshal;

	public class PrepareStoreCopyRequestEncoder : MessageToByteEncoder<PrepareStoreCopyRequest>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void encode(io.netty.channel.ChannelHandlerContext channelHandlerContext, PrepareStoreCopyRequest prepareStoreCopyRequest, io.netty.buffer.ByteBuf byteBuf) throws Exception
		 protected internal override void Encode( ChannelHandlerContext channelHandlerContext, PrepareStoreCopyRequest prepareStoreCopyRequest, ByteBuf byteBuf )
		 {
			  StoreIdMarshal.INSTANCE.marshal( prepareStoreCopyRequest.StoreId, new BoundedNetworkWritableChannel( byteBuf ) );
		 }
	}

}