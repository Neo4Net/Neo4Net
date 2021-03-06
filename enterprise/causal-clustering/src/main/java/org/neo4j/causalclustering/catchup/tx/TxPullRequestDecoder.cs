﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.catchup.tx
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;

	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using NetworkReadableClosableChannelNetty4 = Org.Neo4j.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using StoreIdMarshal = Org.Neo4j.causalclustering.messaging.marshalling.storeid.StoreIdMarshal;

	public class TxPullRequestDecoder : ByteToMessageDecoder
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void decode(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf msg, java.util.List<Object> out) throws Exception
		 protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf msg, IList<object> @out )
		 {
			  long txId = msg.readLong();
			  StoreId storeId = StoreIdMarshal.INSTANCE.unmarshal( new NetworkReadableClosableChannelNetty4( msg ) );
			  @out.Add( new TxPullRequest( txId, storeId ) );
		 }
	}

}