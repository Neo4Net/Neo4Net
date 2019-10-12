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
	using MessageToByteEncoder = io.netty.handler.codec.MessageToByteEncoder;

	using BoundedNetworkWritableChannel = Org.Neo4j.causalclustering.messaging.BoundedNetworkWritableChannel;
	using StoreIdMarshal = Org.Neo4j.causalclustering.messaging.marshalling.storeid.StoreIdMarshal;

	public class TxPullRequestEncoder : MessageToByteEncoder<TxPullRequest>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void encode(io.netty.channel.ChannelHandlerContext ctx, TxPullRequest request, io.netty.buffer.ByteBuf out) throws Exception
		 protected internal override void Encode( ChannelHandlerContext ctx, TxPullRequest request, ByteBuf @out )
		 {
			  @out.writeLong( request.PreviousTxId() );
			  StoreIdMarshal.INSTANCE.marshal( request.ExpectedStoreId(), new BoundedNetworkWritableChannel(@out) );
		 }
	}

}