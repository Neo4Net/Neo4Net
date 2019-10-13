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
	using Neo4Net.cluster.statemachine;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.LearnerContext_Fields.LEARN_GAP_THRESHOLD;

	/// <summary>
	/// State machine for Paxos Learner
	/// </summary>
	public enum LearnerState
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: start { @Override public LearnerState handle(LearnerContext context, org.neo4j.cluster.com.message.Message<LearnerMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { if(message.getMessageType() == LearnerMessage.join) { return learner; } return this; } },
		 start
		 {
			 public LearnerState handle( LearnerContext context, Message<LearnerMessage> message, MessageHolder outgoing )
			 {
				 if ( message.MessageType == LearnerMessage.Join ) { return learner; } return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: learner { @Override public LearnerState handle(LearnerContext context, org.neo4j.cluster.com.message.Message<LearnerMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) throws java.io.IOException, ClassNotFoundException, java.net.URISyntaxException { switch(message.getMessageType()) { case learn: { LearnerMessage.LearnState learnState = message.getPayload(); final InstanceId instanceId = new InstanceId(message); PaxosInstance instance = context.getPaxosInstance(instanceId); org.neo4j.logging.Log log = context.getLog(getClass()); if(instanceId.getId() <= context.getLastDeliveredInstanceId()) { break; } context.learnedInstanceId(instanceId.getId()); instance.closed(learnState.getValue(), message.getHeader(org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID)); if(log.isDebugEnabled()) { String description; if(instance.value_2 instanceof org.neo4j.cluster.protocol.atomicbroadcast.Payload) { org.neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer atomicBroadcastSerializer = context.newSerializer(); description = atomicBroadcastSerializer.receive((org.neo4j.cluster.protocol.atomicbroadcast.Payload) instance.value_2).toString(); } else { description = instance.value_2.toString(); } log.debug("Learned and closed instance " + instance.id + " from conversation " + instance.conversationIdHeader + " and the content was " + description); } if(context.isMe(context.getCoordinator()) && instanceId.getId() != context.getLastDeliveredInstanceId() + 1) { context.getLog(LearnerState.class).debug("Gap developed in delivered instances," + "latest received was %s but last delivered was %d.", instanceId, context.getLastDeliveredInstanceId()); if(instanceId.getId() > context.getLastDeliveredInstanceId() + LEARN_GAP_THRESHOLD) { context.getLog(LearnerState.class).debug("Gap threshold reached (%d), proceeding to deliver everything pending " + "up until now", LEARN_GAP_THRESHOLD); boolean currentInstanceFound = false; long checkInstanceId = context.getLastDeliveredInstanceId() + 1; final long startingInstanceId = checkInstanceId; while ((instance = context.getPaxosInstance(new InstanceId(checkInstanceId))) != null) { if(checkInstanceId == instanceId.getId()) { currentInstanceFound = true; } instance.delivered(); context.setLastDeliveredInstanceId(checkInstanceId); org.neo4j.cluster.com.message.Message<AtomicBroadcastMessage> learnMessage = org.neo4j.cluster.com.message.Message.internal(AtomicBroadcastMessage.broadcastResponse, instance.value_2).setHeader(InstanceId.INSTANCE, instance.id.toString()).setHeader(org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID, instance.conversationIdHeader); outgoing.offer(learnMessage); checkInstanceId++; } context.getLog(LearnerMessage.LearnState.class).debug("Delivered everything from %d up until %d. Triggering message was %s, delivered: %b", startingInstanceId, checkInstanceId - 1, instanceId, currentInstanceFound); } } else { if(instanceId.getId() == context.getLastDeliveredInstanceId() + 1) { instance.delivered(); outgoing.offer(org.neo4j.cluster.com.message.Message.internal(AtomicBroadcastMessage.broadcastResponse, learnState.getValue()).setHeader(InstanceId.INSTANCE, instance.id.toString()).setHeader(org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID, instance.conversationIdHeader)); context.setLastDeliveredInstanceId(instanceId.getId()); long checkInstanceId = instanceId.getId() + 1; while ((instance = context.getPaxosInstance(new InstanceId(checkInstanceId))).isState(PaxosInstance.State.closed)) { instance.delivered(); context.setLastDeliveredInstanceId(checkInstanceId); org.neo4j.cluster.com.message.Message<AtomicBroadcastMessage> learnMessage = org.neo4j.cluster.com.message.Message.internal(AtomicBroadcastMessage.broadcastResponse, instance.value_2).setHeader(InstanceId.INSTANCE, instance.id.toString()).setHeader(org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID, instance.conversationIdHeader); outgoing.offer(learnMessage); checkInstanceId++; } if(checkInstanceId == context.getLastKnownLearnedInstanceInCluster() + 1) { context.cancelTimeout("learn"); } else { context.getLog(LearnerState.class).debug("*** HOLE! WAITING " + "FOR " + (context.getLastDeliveredInstanceId() + 1)); } } else { context.getLog(LearnerState.class).debug("*** GOT " + instanceId + ", WAITING FOR " + (context.getLastDeliveredInstanceId() + 1)); context.setTimeout("learn", org.neo4j.cluster.com.message.Message.timeout(LearnerMessage.learnTimedout, message)); } } break; } case learnTimedout: { if(!context.hasDeliveredAllKnownInstances()) { for(long instanceId = context.getLastDeliveredInstanceId() + 1; instanceId < context.getLastKnownLearnedInstanceInCluster(); instanceId++) { InstanceId id = new InstanceId(instanceId); PaxosInstance instance = context.getPaxosInstance(id); if(!instance.isState(PaxosInstance.State.closed) && !instance.isState(PaxosInstance.State.delivered)) { for(org.neo4j.cluster.InstanceId node : context.getAlive()) { java.net.URI nodeUri = context.getUriForId(node); if(!node.equals(context.getMyId())) { outgoing.offer(org.neo4j.cluster.com.message.Message.to(LearnerMessage.learnRequest, nodeUri, new LearnerMessage.LearnRequestState()).setHeader(InstanceId.INSTANCE, id.toString())); } } } } context.setTimeout("learn", org.neo4j.cluster.com.message.Message.timeout(LearnerMessage.learnTimedout, message)); } break; } case learnRequest: { InstanceId instanceId = new InstanceId(message); PaxosInstance instance = context.getPaxosInstance(instanceId); if(instance.isState(PaxosInstance.State.closed) || instance.isState(PaxosInstance.State.delivered)) { outgoing.offer(org.neo4j.cluster.com.message.Message.respond(LearnerMessage.learn, message, new LearnerMessage.LearnState(instance.value_2)).setHeader(InstanceId.INSTANCE, instanceId.toString()).setHeader(org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID, instance.conversationIdHeader)); } else { outgoing.offer(message.copyHeadersTo(org.neo4j.cluster.com.message.Message.respond(LearnerMessage.learnFailed, message, new LearnerMessage.LearnFailedState()), org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE)); } break; } case learnFailed: { InstanceId instanceId = new InstanceId(message); context.notifyLearnMiss(instanceId); break; } case catchUp: { long catchUpTo = message.getPayload<long>(); if(context.getLastKnownLearnedInstanceInCluster() < catchUpTo) { context.setNextInstanceId(catchUpTo + 1); for(long instanceId = context.getLastLearnedInstanceId() + 1; instanceId <= catchUpTo; instanceId++) { InstanceId id = new InstanceId(instanceId); PaxosInstance instance = context.getPaxosInstance(id); if(!instance.isState(PaxosInstance.State.closed) && !instance.isState(PaxosInstance.State.delivered)) { outgoing.offer(org.neo4j.cluster.com.message.Message.to(LearnerMessage.learnRequest, lastKnownAliveUriOrSenderUri(context, message), new LearnerMessage.LearnRequestState()).setHeader(InstanceId.INSTANCE, id.toString())); context.setTimeout("learn", org.neo4j.cluster.com.message.Message.timeout(LearnerMessage.learnTimedout, message)); break; } } org.neo4j.cluster.InstanceId instanceId = message.hasHeader(org.neo4j.cluster.com.message.Message.HEADER_INSTANCE_ID) ? new org.neo4j.cluster.InstanceId(int.Parse(message.getHeader(org.neo4j.cluster.com.message.Message.HEADER_INSTANCE_ID))) : context.getMyId(); context.setLastKnownLearnedInstanceInCluster(catchUpTo, instanceId); } break; } case leave: { context.leave(); return start; } default: break; } return this; } private java.net.URI lastKnownAliveUriOrSenderUri(LearnerContext context, org.neo4j.cluster.com.message.Message<LearnerMessage> message) throws java.net.URISyntaxException { org.neo4j.cluster.InstanceId lastKnownAliveInstance = context.getLastKnownAliveUpToDateInstance(); if(lastKnownAliveInstance != null) { return context.getUriForId(lastKnownAliveInstance); } else { return new java.net.URI(message.getHeader(org.neo4j.cluster.com.message.Message.HEADER_FROM)); } } }
		 learner
		 {
			 public LearnerState handle( LearnerContext context, Message<LearnerMessage> message, MessageHolder outgoing ) throws IOException, ClassNotFoundException, URISyntaxException
			 {
				 switch ( message.MessageType )
				 {
					 case learn:
					 {
						 LearnerMessage.LearnState learnState = message.Payload; final InstanceId instanceId = new InstanceId( message ); PaxosInstance instance = context.getPaxosInstance( instanceId ); Log log = context.getLog( this.GetType() ); if (instanceId.Id <= context.LastDeliveredInstanceId) { break; } context.learnedInstanceId(instanceId.Id); instance.closed(learnState.Value, message.getHeader(Message.HEADER_CONVERSATION_ID)); if (log.DebugEnabled)
						 {
							 string description; if ( instance.value_2 is Payload ) { AtomicBroadcastSerializer atomicBroadcastSerializer = context.newSerializer(); description = atomicBroadcastSerializer.receive((Payload) instance.value_2).ToString(); } else { description = instance.value_2.ToString(); } log.debug("Learned and closed instance " + instance.id + " from conversation " + instance.conversationIdHeader + " and the content was " + description);
						 }
						 if ( context.isMe( context.Coordinator ) && instanceId.Id != context.LastDeliveredInstanceId + 1 )
						 {
							 context.getLog( typeof( LearnerState ) ).debug( "Gap developed in delivered instances," + "latest received was %s but last delivered was %d.", instanceId, context.LastDeliveredInstanceId ); if ( instanceId.Id > context.LastDeliveredInstanceId + LEARN_GAP_THRESHOLD )
							 {
								 context.getLog( typeof( LearnerState ) ).debug( "Gap threshold reached (%d), proceeding to deliver everything pending " + "up until now", LEARN_GAP_THRESHOLD ); bool currentInstanceFound = false; long checkInstanceId = context.LastDeliveredInstanceId + 1; final long startingInstanceId = checkInstanceId; while ( ( instance = context.getPaxosInstance( new InstanceId( checkInstanceId ) ) ) != null )
								 {
									 if ( checkInstanceId == instanceId.Id ) { currentInstanceFound = true; } instance.delivered(); context.setLastDeliveredInstanceId(checkInstanceId); Message<AtomicBroadcastMessage> learnMessage = Message.@internal(AtomicBroadcastMessage.BroadcastResponse, instance.value_2).setHeader(InstanceId.INSTANCE, instance.id.ToString()).setHeader(Message.HEADER_CONVERSATION_ID, instance.conversationIdHeader); outgoing.offer(learnMessage); checkInstanceId++;
								 }
								 context.getLog( typeof( LearnerMessage.LearnState ) ).debug( "Delivered everything from %d up until %d. Triggering message was %s, delivered: %b", startingInstanceId, checkInstanceId - 1, instanceId, currentInstanceFound );
							 }
						 }
						 else
						 {
							 if ( instanceId.Id == context.LastDeliveredInstanceId + 1 )
							 {
								 instance.delivered(); outgoing.offer(Message.@internal(AtomicBroadcastMessage.BroadcastResponse, learnState.Value).setHeader(InstanceId.INSTANCE, instance.id.ToString()).setHeader(Message.HEADER_CONVERSATION_ID, instance.conversationIdHeader)); context.setLastDeliveredInstanceId(instanceId.Id); long checkInstanceId = instanceId.Id + 1; while ((instance = context.getPaxosInstance(new InstanceId(checkInstanceId))).isState(PaxosInstance.State.Closed)) { instance.delivered(); context.setLastDeliveredInstanceId(checkInstanceId); Message<AtomicBroadcastMessage> learnMessage = Message.@internal(AtomicBroadcastMessage.BroadcastResponse, instance.value_2).setHeader(InstanceId.INSTANCE, instance.id.ToString()).setHeader(Message.HEADER_CONVERSATION_ID, instance.conversationIdHeader); outgoing.offer(learnMessage); checkInstanceId++; } if (checkInstanceId == context.LastKnownLearnedInstanceInCluster + 1) { context.cancelTimeout("learn"); } else { context.getLog(typeof(LearnerState)).debug("*** HOLE! WAITING " + "FOR " + (context.LastDeliveredInstanceId + 1)); }
							 }
							 else { context.getLog( typeof( LearnerState ) ).debug( "*** GOT " + instanceId + ", WAITING FOR " + ( context.LastDeliveredInstanceId + 1 ) ); context.setTimeout( "learn", Message.timeout( LearnerMessage.LearnTimedout, message ) ); }
						 }
						 break;
					 }
					 case learnTimedout:
					 {
						 if ( !context.hasDeliveredAllKnownInstances() )
						 {
							 for ( long instanceId = context.LastDeliveredInstanceId + 1; instanceId < context.LastKnownLearnedInstanceInCluster; instanceId++ )
							 {
								 InstanceId id = new InstanceId( instanceId ); PaxosInstance instance = context.getPaxosInstance( id ); if ( !instance.isState( PaxosInstance.State.Closed ) && !instance.isState( PaxosInstance.State.Delivered ) )
								 {
									 for ( Neo4Net.cluster.InstanceId node : context.Alive )
									 {
										 URI nodeUri = context.getUriForId( node ); if ( !node.Equals( context.MyId ) ) { outgoing.offer( Message.to( LearnerMessage.LearnRequest, nodeUri, new LearnerMessage.LearnRequestState() ).setHeader(InstanceId.INSTANCE, id.ToString()) ); }
									 }
								 }
							 }
							 context.setTimeout( "learn", Message.timeout( LearnerMessage.LearnTimedout, message ) );
						 }
						 break;
					 }
					 case learnRequest:
					 {
						 InstanceId instanceId = new InstanceId( message ); PaxosInstance instance = context.getPaxosInstance( instanceId ); if ( instance.isState( PaxosInstance.State.Closed ) || instance.isState( PaxosInstance.State.Delivered ) ) { outgoing.offer( Message.respond( LearnerMessage.Learn, message, new LearnerMessage.LearnState( instance.value_2 ) ).setHeader( InstanceId.INSTANCE, instanceId.ToString() ).setHeader(Message.HEADER_CONVERSATION_ID, instance.conversationIdHeader) ); } else { outgoing.offer(message.copyHeadersTo(Message.respond(LearnerMessage.LearnFailed, message, new LearnerMessage.LearnFailedState()), Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE)); } break;
					 }
					 case learnFailed: { InstanceId instanceId = new InstanceId( message ); context.notifyLearnMiss( instanceId ); break; } case catchUp:
					 {
						 long catchUpTo = message.getPayload<long>(); if (context.LastKnownLearnedInstanceInCluster < catchUpTo)
						 {
							 context.setNextInstanceId( catchUpTo + 1 ); for ( long instanceId = context.LastLearnedInstanceId + 1; instanceId <= catchUpTo; instanceId++ )
							 {
								 InstanceId id = new InstanceId( instanceId ); PaxosInstance instance = context.getPaxosInstance( id ); if ( !instance.isState( PaxosInstance.State.Closed ) && !instance.isState( PaxosInstance.State.Delivered ) ) { outgoing.offer( Message.to( LearnerMessage.LearnRequest, lastKnownAliveUriOrSenderUri( context, message ), new LearnerMessage.LearnRequestState() ).setHeader(InstanceId.INSTANCE, id.ToString()) ); context.setTimeout("learn", Message.timeout(LearnerMessage.LearnTimedout, message)); break; }
							 }
							 Neo4Net.cluster.InstanceId instanceId = message.hasHeader( Message.HEADER_INSTANCE_ID ) ? new Neo4Net.cluster.InstanceId( int.Parse( message.getHeader( Message.HEADER_INSTANCE_ID ) ) ) : context.MyId; context.setLastKnownLearnedInstanceInCluster( catchUpTo, instanceId );
						 }
						 break;
					 }
					 case leave: { context.leave(); return start; } default: break;
				 }
				 return this;
			 }
			 private URI lastKnownAliveUriOrSenderUri( LearnerContext context, Message<LearnerMessage> message ) throws URISyntaxException
			 {
				 Neo4Net.cluster.InstanceId lastKnownAliveInstance = context.LastKnownAliveUpToDateInstance; if ( lastKnownAliveInstance != null ) { return context.getUriForId( lastKnownAliveInstance ); } else { return new URI( message.getHeader( Message.HEADER_FROM ) ); }
			 }
		 }
	}

}