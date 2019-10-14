using System;

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
namespace Neo4Net.cluster.protocol.heartbeat
{

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageProcessor = Neo4Net.cluster.com.message.MessageProcessor;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;

	/// <summary>
	/// When a message is sent out, reset the timeout for sending heartbeat to the HEADER_TO host, since we only have to send i_am_alive if
	/// nothing else is going on.
	/// </summary>
	public class HeartbeatRefreshProcessor : MessageProcessor
	{
		 private readonly MessageHolder _outgoing;
		 private readonly ClusterContext _clusterContext;

		 public HeartbeatRefreshProcessor( MessageHolder outgoing, ClusterContext clusterContext )
		 {
			  this._outgoing = outgoing;
			  this._clusterContext = clusterContext;
		 }

		 public override bool Process<T1>( Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
			  if ( !message.Internal && !message.MessageType.Equals( HeartbeatMessage.IAmAlive ) )
			  {
					try
					{
						 string to = message.GetHeader( Message.HEADER_TO );

						 InstanceId serverId = _clusterContext.Configuration.getIdForUri( new URI( to ) );

						 if ( !_clusterContext.isMe( serverId ) )
						 {
							  _outgoing.offer( Message.Internal( HeartbeatMessage.ResetSendHeartbeat, serverId ) );
						 }
					}
					catch ( URISyntaxException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
			  }
			  return true;
		 }
	}

}