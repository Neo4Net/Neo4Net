using System;

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
namespace Neo4Net.causalclustering.core.consensus
{
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;

	using Neo4Net.causalclustering.messaging;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ChannelHandler.Sharable public class RaftMessageNettyHandler extends io.netty.channel.SimpleChannelInboundHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> implements org.neo4j.causalclustering.messaging.Inbound<RaftMessages_ReceivedInstantClusterIdAwareMessage<?>>
	public class RaftMessageNettyHandler : SimpleChannelInboundHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>, Inbound<RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.messaging.Inbound_MessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> actual;
		 private Neo4Net.causalclustering.messaging.Inbound_MessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> _actual;
		 private Log _log;

		 public RaftMessageNettyHandler( LogProvider logProvider )
		 {
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void RegisterHandler<T1>( Neo4Net.causalclustering.messaging.Inbound_MessageHandler<T1> actual )
		 {
			  this._actual = actual;
		 }

		 protected internal override void ChannelRead0<T1>( ChannelHandlerContext channelHandlerContext, RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> incomingMessage )
		 {
			  try
			  {
					_actual.handle( incomingMessage );
			  }
			  catch ( Exception e )
			  {
					_log.error( format( "Failed to process message %s", incomingMessage ), e );
			  }
		 }
	}

}