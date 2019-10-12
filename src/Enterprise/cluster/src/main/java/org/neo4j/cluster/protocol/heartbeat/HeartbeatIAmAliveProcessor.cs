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
namespace Neo4Net.cluster.protocol.heartbeat
{

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageProcessor = Neo4Net.cluster.com.message.MessageProcessor;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;

	/// <summary>
	/// When a message is received, create an I Am Alive message as well, since we know that the sending instance is up.
	/// The exceptions to this rule are:
	/// - when the message is of type "I Am Alive", since this would lead to a feedback loop of more and more "I Am Alive"
	///   messages being sent.
	/// - when the message is of type "Suspicions", since these should be ignored for failed instances and generating an
	///   "I Am Alive" message for it would mark the instance as alive before ignoring its suspicions.
	/// </summary>
	public class HeartbeatIAmAliveProcessor : MessageProcessor
	{
		 private readonly MessageHolder _output;
		 private readonly ClusterContext _clusterContext;

		 public HeartbeatIAmAliveProcessor( MessageHolder output, ClusterContext clusterContext )
		 {
			  this._output = output;
			  this._clusterContext = clusterContext;
		 }

		 public override bool Process<T1>( Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
			  if ( !message.Internal && !message.MessageType.Equals( HeartbeatMessage.IAmAlive ) && !message.MessageType.Equals( HeartbeatMessage.Suspicions ) )
			  {
					// We assume the HEADER_FROM header always exists.
					string from = message.GetHeader( Message.HEADER_FROM );
					if ( !from.Equals( message.GetHeader( Message.HEADER_TO ) ) )
					{
						 InstanceId theId;
						 if ( message.HasHeader( Message.HEADER_INSTANCE_ID ) )
						 {
							  // HEADER_INSTANCE_ID is there since after 1.9.6
							  theId = new InstanceId( int.Parse( message.GetHeader( Message.HEADER_INSTANCE_ID ) ) );
						 }
						 else
						 {
							  theId = _clusterContext.Configuration.getIdForUri( URI.create( from ) );
						 }

						 if ( theId != null && _clusterContext.Configuration.Members.ContainsKey( theId ) && !_clusterContext.isMe( theId ) )
						 {
							  Message<HeartbeatMessage> heartbeatMessage = message.CopyHeadersTo( Message.@internal( HeartbeatMessage.IAmAlive, new HeartbeatMessage.IAmAliveState( theId ) ), Message.HEADER_FROM, Message.HEADER_INSTANCE_ID );
							  _output.offer( heartbeatMessage );
						 }
					}
			  }
			  return true;
		 }
	}

}