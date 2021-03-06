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
namespace Org.Neo4j.cluster.protocol.heartbeat
{

	using Org.Neo4j.cluster.com.message;
	using MessageHolder = Org.Neo4j.cluster.com.message.MessageHolder;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;
	using LearnerMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage;
	using Org.Neo4j.cluster.statemachine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.@internal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.to;

	/// <summary>
	/// State machine that implements the <seealso cref="Heartbeat"/> API
	/// </summary>
	public enum HeartbeatState
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: start { @Override public HeartbeatState handle(HeartbeatContext context, org.neo4j.cluster.com.message.Message<HeartbeatMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case addHeartbeatListener: { context.addHeartbeatListener(message.getPayload()); break; } case removeHeartbeatListener: { context.removeHeartbeatListener(message.getPayload()); break; } case join: { for(org.neo4j.cluster.InstanceId instanceId : context.getOtherInstances()) { context.setTimeout(HeartbeatMessage.i_am_alive + "-" + instanceId, timeout(HeartbeatMessage.timed_out, message, instanceId)); outgoing.offer(timeout(HeartbeatMessage.sendHeartbeat, message, instanceId)); } return heartbeat; } default: break; } return this; } },
		 start
		 {
			 public HeartbeatState handle( HeartbeatContext context, Message<HeartbeatMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case addHeartbeatListener: { context.addHeartbeatListener( message.Payload ); break; } case removeHeartbeatListener: { context.removeHeartbeatListener( message.Payload ); break; } case join:
					 {
						 for ( InstanceId instanceId : context.OtherInstances ) { context.setTimeout( HeartbeatMessage.IAmAlive + "-" + instanceId, timeout( HeartbeatMessage.TimedOut, message, instanceId ) ); outgoing.offer( timeout( HeartbeatMessage.SendHeartbeat, message, instanceId ) ); } return heartbeat;
					 }
					 default: break;
				 }
				 return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: heartbeat { @Override public HeartbeatState handle(HeartbeatContext context, org.neo4j.cluster.com.message.Message<HeartbeatMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case i_am_alive: { HeartbeatMessage.IAmAliveState state = message.getPayload(); if(context.isMe(state.getServer())) { break; } if(state.getServer() == null) { break; } if(context.alive(state.getServer())) { for(org.neo4j.cluster.InstanceId aliveServer : context.getAlive()) { if(!aliveServer.equals(context.getMyId())) { java.net.URI aliveServerUri = context.getUriForId(aliveServer); outgoing.offer(org.neo4j.cluster.com.message.Message.to(HeartbeatMessage.suspicions, aliveServerUri, new HeartbeatMessage.SuspicionsState(context.getSuspicionsFor(context.getMyId())))); } } } resetTimeout(context, message, state); if(message.hasHeader("last-learned")) { long lastLearned = long.Parse(message.getHeader("last-learned")); if(lastLearned > context.getLastKnownLearnedInstanceInCluster()) { org.neo4j.cluster.com.message.Message<org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage> catchUpMessage = message.copyHeadersTo(internal(org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage.catchUp, lastLearned), org.neo4j.cluster.com.message.Message.HEADER_FROM, org.neo4j.cluster.com.message.Message.HEADER_INSTANCE_ID); outgoing.offer(catchUpMessage); } } break; } case timed_out: { org.neo4j.cluster.InstanceId server = message.getPayload(); context.getLog(HeartbeatState.class).debug("Received timed out for server " + server); if(context.getMembers().containsKey(server)) { context.suspect(server); context.setTimeout(HeartbeatMessage.i_am_alive + "-" + server, timeout(HeartbeatMessage.timed_out, message, server)); for(org.neo4j.cluster.InstanceId aliveServer : context.getAlive()) { if(!aliveServer.equals(context.getMyId())) { java.net.URI sendTo = context.getUriForId(aliveServer); outgoing.offer(org.neo4j.cluster.com.message.Message.to(HeartbeatMessage.suspicions, sendTo, new HeartbeatMessage.SuspicionsState(context.getSuspicionsFor(context.getMyId())))); } } } else { context.serverLeftCluster(server); } break; } case sendHeartbeat: { org.neo4j.cluster.InstanceId to = message.getPayload(); if(!context.isMe(to)) { if(context.getMembers().containsKey(to)) { java.net.URI toSendTo = context.getUriForId(to); outgoing.offer(to(HeartbeatMessage.i_am_alive, toSendTo, new HeartbeatMessage.IAmAliveState(context.getMyId())).setHeader("last-learned", context.getLastLearnedInstanceId() + "")); context.setTimeout(HeartbeatMessage.sendHeartbeat + "-" + to, timeout(HeartbeatMessage.sendHeartbeat, message, to)); } } break; } case reset_send_heartbeat: { org.neo4j.cluster.InstanceId to = message.getPayload(); if(!context.isMe(to)) { String timeoutName = HeartbeatMessage.sendHeartbeat + "-" + to; context.cancelTimeout(timeoutName); context.setTimeout(timeoutName, org.neo4j.cluster.com.message.Message.timeout(HeartbeatMessage.sendHeartbeat, message, to)); } break; } case suspicions: { HeartbeatMessage.SuspicionsState suspicions = message.getPayload(); org.neo4j.cluster.InstanceId fromId = new org.neo4j.cluster.InstanceId(int.Parse(message.getHeader(org.neo4j.cluster.com.message.Message.HEADER_INSTANCE_ID))); context.getLog(HeartbeatState.class).debug(format("Received suspicions as %s from %s", suspicions, fromId)); suspicions.getSuspicions().remove(context.getMyId()); context.suspicions(fromId, suspicions.getSuspicions()); break; } case leave: { context.getLog(HeartbeatState.class).debug("Received leave"); return start; } case addHeartbeatListener: { context.addHeartbeatListener(message.getPayload()); break; } case removeHeartbeatListener: { context.removeHeartbeatListener(message.getPayload()); break; } default: break; } return this; } private void resetTimeout(HeartbeatContext context, org.neo4j.cluster.com.message.Message<HeartbeatMessage> message, HeartbeatMessage.IAmAliveState state) { String key = HeartbeatMessage.i_am_alive + "-" + state.getServer(); org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> oldTimeout = context.cancelTimeout(key); if(oldTimeout != null && oldTimeout.hasHeader(org.neo4j.cluster.com.message.Message.HEADER_TIMEOUT_COUNT)) { int timeoutCount = int.Parse(oldTimeout.getHeader(org.neo4j.cluster.com.message.Message.HEADER_TIMEOUT_COUNT)); if(timeoutCount > 0) { long timeout = context.getTimeoutFor(oldTimeout); context.getLog(HeartbeatState.class).debug("Received " + state + " after missing " + timeoutCount + " (" + timeout * timeoutCount + "ms)"); } } context.setTimeout(key, timeout(HeartbeatMessage.timed_out, message, state.getServer())); } }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 heartbeat
		 {
			 public HeartbeatState handle( HeartbeatContext context, Message<HeartbeatMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case i_am_alive:
					 {
						 HeartbeatMessage.IAmAliveState state = message.Payload; if ( context.isMe( state.Server ) ) { break; } if ( state.Server == null ) { break; } if ( context.alive( state.Server ) )
						 {
							 for ( InstanceId aliveServer : context.Alive )
							 {
								 if ( !aliveServer.Equals( context.MyId ) ) { URI aliveServerUri = context.getUriForId( aliveServer ); outgoing.offer( Message.to( HeartbeatMessage.Suspicions, aliveServerUri, new HeartbeatMessage.SuspicionsState( context.getSuspicionsFor( context.MyId ) ) ) ); }
							 }
						 }
						 resetTimeout( context, message, state ); if ( message.hasHeader( "last-learned" ) )
						 {
							 long lastLearned = long.Parse( message.getHeader( "last-learned" ) ); if ( lastLearned > context.LastKnownLearnedInstanceInCluster ) { Message<LearnerMessage> catchUpMessage = message.copyHeadersTo( @internal( LearnerMessage.catchUp, lastLearned ), Message.HEADER_FROM, Message.HEADER_INSTANCE_ID ); outgoing.offer( catchUpMessage ); }
						 }
						 break;
					 }
					 case timed_out:
					 {
						 InstanceId server = message.Payload; context.getLog( typeof( HeartbeatState ) ).debug( "Received timed out for server " + server ); if ( context.Members.containsKey( server ) )
						 {
							 context.suspect( server ); context.setTimeout( HeartbeatMessage.IAmAlive + "-" + server, timeout( HeartbeatMessage.TimedOut, message, server ) ); for ( InstanceId aliveServer : context.Alive )
							 {
								 if ( !aliveServer.Equals( context.MyId ) ) { URI sendTo = context.getUriForId( aliveServer ); outgoing.offer( Message.to( HeartbeatMessage.Suspicions, sendTo, new HeartbeatMessage.SuspicionsState( context.getSuspicionsFor( context.MyId ) ) ) ); }
							 }
						 }
						 else { context.serverLeftCluster( server ); } break;
					 }
					 case sendHeartbeat:
					 {
						 InstanceId to = message.Payload; if ( !context.isMe( to ) )
						 {
							 if ( context.Members.containsKey( to ) ) { URI toSendTo = context.getUriForId( to ); outgoing.offer( to( HeartbeatMessage.IAmAlive, toSendTo, new HeartbeatMessage.IAmAliveState( context.MyId ) ).setHeader( "last-learned", context.LastLearnedInstanceId + "" ) ); context.setTimeout( HeartbeatMessage.SendHeartbeat + "-" + to, timeout( HeartbeatMessage.SendHeartbeat, message, to ) ); }
						 }
						 break;
					 }
					 case reset_send_heartbeat:
					 {
						 InstanceId to = message.Payload; if ( !context.isMe( to ) ) { string timeoutName = HeartbeatMessage.SendHeartbeat + "-" + to; context.cancelTimeout( timeoutName ); context.setTimeout( timeoutName, Message.timeout( HeartbeatMessage.SendHeartbeat, message, to ) ); } break;
					 }
					 case suspicions: { HeartbeatMessage.SuspicionsState suspicions = message.Payload; InstanceId fromId = new InstanceId( int.Parse( message.getHeader( Message.HEADER_INSTANCE_ID ) ) ); context.getLog( typeof( HeartbeatState ) ).debug( format( "Received suspicions as %s from %s", suspicions, fromId ) ); suspicions.Suspicions.remove( context.MyId ); context.suspicions( fromId, suspicions.Suspicions ); break; } case leave: { context.getLog( typeof( HeartbeatState ) ).debug( "Received leave" ); return start; } case addHeartbeatListener: { context.addHeartbeatListener( message.Payload ); break; } case removeHeartbeatListener: { context.removeHeartbeatListener( message.Payload ); break; } default: break;
				 }
				 return this;
			 }
			 private void resetTimeout( HeartbeatContext context, Message<HeartbeatMessage> message, HeartbeatMessage.IAmAliveState state )
			 {
				 string key = HeartbeatMessage.IAmAlive + "-" + state.Server; Message<MessageType> oldTimeout = context.cancelTimeout( key ); if ( oldTimeout != null && oldTimeout.hasHeader( Message.HEADER_TIMEOUT_COUNT ) )
				 {
					 int timeoutCount = int.Parse( oldTimeout.getHeader( Message.HEADER_TIMEOUT_COUNT ) ); if ( timeoutCount > 0 ) { long timeout = context.getTimeoutFor( oldTimeout ); context.getLog( typeof( HeartbeatState ) ).debug( "Received " + state + " after missing " + timeoutCount + " (" + timeout * timeoutCount + "ms)" ); }
				 }
				 context.setTimeout( key, timeout( HeartbeatMessage.TimedOut, message, state.Server ) );
			 }
		 }
	}

}