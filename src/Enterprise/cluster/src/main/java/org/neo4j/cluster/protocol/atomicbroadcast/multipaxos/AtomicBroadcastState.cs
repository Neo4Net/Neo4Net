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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using HeartbeatMessage = Neo4Net.cluster.protocol.heartbeat.HeartbeatMessage;
	using Neo4Net.cluster.statemachine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.@internal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.to;

	/// <summary>
	/// State Machine for implementation of Atomic Broadcast client interface
	/// </summary>
	public enum AtomicBroadcastState
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: start { @Override public AtomicBroadcastState handle(AtomicBroadcastContext context, org.neo4j.cluster.com.message.Message<AtomicBroadcastMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case entered: { return broadcasting; } case join: { return joining; } default: { defaultHandling(context, message); } } return this; } },
		 start
		 {
			 public AtomicBroadcastState handle( AtomicBroadcastContext context, Message<AtomicBroadcastMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case entered: { return broadcasting; } case join: { return joining; } default: { defaultHandling( context, message ); }
				 }
				 return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: joining { @Override public AtomicBroadcastState handle(AtomicBroadcastContext context, org.neo4j.cluster.com.message.Message<AtomicBroadcastMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case failed: { outgoing.offer(internal(org.neo4j.cluster.protocol.cluster.ClusterMessage.joinFailure, new java.util.concurrent.TimeoutException("Could not join cluster"))); return start; } case broadcastResponse: { if(message.getPayload() instanceof org.neo4j.cluster.protocol.cluster.ClusterMessage.ConfigurationChangeState) { outgoing.offer(message.copyHeadersTo(internal(org.neo4j.cluster.protocol.cluster.ClusterMessage.configurationChanged, message.getPayload()))); } break; } case entered: { return broadcasting; } default: { defaultHandling(context, message); } } return this; } },
		 joining
		 {
			 public AtomicBroadcastState handle( AtomicBroadcastContext context, Message<AtomicBroadcastMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case failed: { outgoing.offer( @internal( ClusterMessage.joinFailure, new TimeoutException( "Could not join cluster" ) ) ); return start; } case broadcastResponse:
					 {
						 if ( message.Payload is ClusterMessage.ConfigurationChangeState ) { outgoing.offer( message.copyHeadersTo( @internal( ClusterMessage.configurationChanged, message.Payload ) ) ); } break;
					 }
					 case entered: { return broadcasting; } default: { defaultHandling( context, message ); }
				 }
				 return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: broadcasting { @Override public AtomicBroadcastState handle(AtomicBroadcastContext context, org.neo4j.cluster.com.message.Message<AtomicBroadcastMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case broadcast: case failed: { if(context.hasQuorum()) { org.neo4j.cluster.InstanceId coordinator = context.getCoordinator(); if(coordinator != null) { java.net.URI coordinatorUri = context.getUriForId(coordinator); outgoing.offer(message.copyHeadersTo(to(ProposerMessage.propose, coordinatorUri, message.getPayload()))); context.setTimeout("broadcast-" + message.getHeader(org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID), timeout(AtomicBroadcastMessage.broadcastTimeout, message, message.getPayload())); } else { outgoing.offer(message.copyHeadersTo(internal(ProposerMessage.propose, message.getPayload()), org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID, org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE)); } } else { context.getLog(AtomicBroadcastState.class).warn("No quorum and therefor dropping broadcast msg: " + message.getPayload()); } break; } case broadcastResponse: { context.cancelTimeout("broadcast-" + message.getHeader(org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID)); if(message.getPayload() instanceof org.neo4j.cluster.protocol.cluster.ClusterMessage.ConfigurationChangeState) { outgoing.offer(message.copyHeadersTo(internal(org.neo4j.cluster.protocol.cluster.ClusterMessage.configurationChanged, message.getPayload()))); org.neo4j.cluster.protocol.cluster.ClusterMessage.ConfigurationChangeState change = message.getPayload(); if(change.getJoinUri() != null) { outgoing.offer(message.copyHeadersTo(org.neo4j.cluster.com.message.Message.internal(org.neo4j.cluster.protocol.heartbeat.HeartbeatMessage.i_am_alive, new org.neo4j.cluster.protocol.heartbeat.HeartbeatMessage.IAmAliveState(change.getJoin())), org.neo4j.cluster.com.message.Message.HEADER_FROM)); } } else { context.receive(message.getPayload()); } break; } case broadcastTimeout: { break; } case leave: { return start; } default: { defaultHandling(context, message); } } return this; } };
		 broadcasting
		 {
			 public AtomicBroadcastState handle( AtomicBroadcastContext context, Message<AtomicBroadcastMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case broadcast: case failed:
					 {
						 if ( context.hasQuorum() )
						 {
							 Neo4Net.cluster.InstanceId coordinator = context.Coordinator; if ( coordinator != null ) { URI coordinatorUri = context.getUriForId( coordinator ); outgoing.offer( message.copyHeadersTo( to( ProposerMessage.Propose, coordinatorUri, message.Payload ) ) ); context.setTimeout( "broadcast-" + message.getHeader( Message.HEADER_CONVERSATION_ID ), timeout( AtomicBroadcastMessage.BroadcastTimeout, message, message.Payload ) ); } else { outgoing.offer( message.copyHeadersTo( @internal( ProposerMessage.Propose, message.Payload ), Message.HEADER_CONVERSATION_ID, Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE ) ); }
						 }
						 else { context.getLog( typeof( AtomicBroadcastState ) ).warn( "No quorum and therefor dropping broadcast msg: " + message.Payload ); } break;
					 }
					 case broadcastResponse:
					 {
						 context.cancelTimeout( "broadcast-" + message.getHeader( Message.HEADER_CONVERSATION_ID ) ); if ( message.Payload is ClusterMessage.ConfigurationChangeState )
						 {
							 outgoing.offer( message.copyHeadersTo( @internal( ClusterMessage.configurationChanged, message.Payload ) ) ); ClusterMessage.ConfigurationChangeState change = message.Payload; if ( change.JoinUri != null ) { outgoing.offer( message.copyHeadersTo( Message.@internal( HeartbeatMessage.i_am_alive, new HeartbeatMessage.IAmAliveState( change.Join ) ), Message.HEADER_FROM ) ); }
						 }
						 else { context.receive( message.Payload ); } break;
					 }
					 case broadcastTimeout: { break; } case leave: { return start; } default: { defaultHandling( context, message ); }
				 }
				 return this;
			 }
		 };

		 private static void defaultHandling( AtomicBroadcastContext context, Message<AtomicBroadcastMessage> message )
		 {
			 switch ( message.MessageType )
			 {
				 case addAtomicBroadcastListener: { context.addAtomicBroadcastListener( message.Payload ); break; } case removeAtomicBroadcastListener: { context.removeAtomicBroadcastListener( message.Payload ); break; } default: break;
			 }
		 }
	}

}