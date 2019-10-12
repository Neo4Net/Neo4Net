﻿/*
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
namespace Org.Neo4j.causalclustering.core.state.snapshot
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;

	using CatchupServerProtocol = Org.Neo4j.causalclustering.catchup.CatchupServerProtocol;
	using ResponseMessageType = Org.Neo4j.causalclustering.catchup.ResponseMessageType;

	using static Org.Neo4j.causalclustering.catchup.CatchupServerProtocol.State;

	public class CoreSnapshotRequestHandler : SimpleChannelInboundHandler<CoreSnapshotRequest>
	{
		 private readonly CatchupServerProtocol _protocol;
		 private readonly CoreSnapshotService _snapshotService;

		 public CoreSnapshotRequestHandler( CatchupServerProtocol protocol, CoreSnapshotService snapshotService )
		 {
			  this._protocol = protocol;
			  this._snapshotService = snapshotService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void channelRead0(io.netty.channel.ChannelHandlerContext ctx, CoreSnapshotRequest msg) throws Exception
		 protected internal override void ChannelRead0( ChannelHandlerContext ctx, CoreSnapshotRequest msg )
		 {
			  SendStates( ctx, _snapshotService.snapshot() );
			  _protocol.expect( State.MESSAGE_TYPE );
		 }

		 private void SendStates( ChannelHandlerContext ctx, CoreSnapshot coreSnapshot )
		 {
			  ctx.writeAndFlush( ResponseMessageType.CORE_SNAPSHOT );
			  ctx.writeAndFlush( coreSnapshot );
		 }
	}

}