/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;

	using StoreId = Neo4Net.causalclustering.identity.StoreId;

	using static Neo4Net.causalclustering.catchup.CatchupServerProtocol.State;

	public class GetStoreIdRequestHandler : SimpleChannelInboundHandler<GetStoreIdRequest>
	{
		 private readonly CatchupServerProtocol _protocol;
		 private readonly System.Func<StoreId> _storeIdSupplier;

		 public GetStoreIdRequestHandler( CatchupServerProtocol protocol, System.Func<StoreId> storeIdSupplier )
		 {
			  this._protocol = protocol;
			  this._storeIdSupplier = storeIdSupplier;
		 }

		 protected internal override void ChannelRead0( ChannelHandlerContext ctx, GetStoreIdRequest msg )
		 {
			  ctx.writeAndFlush( ResponseMessageType.STORE_ID );
			  ctx.writeAndFlush( _storeIdSupplier.get() );
			  _protocol.expect( State.MESSAGE_TYPE );
		 }
	}

}