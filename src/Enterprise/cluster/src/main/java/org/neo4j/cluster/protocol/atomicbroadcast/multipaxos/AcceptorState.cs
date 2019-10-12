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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{
	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using Neo4Net.cluster.statemachine;

	/// <summary>
	/// State machine for Paxos Acceptor
	/// </summary>
	public enum AcceptorState
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: start { @Override public AcceptorState handle(AcceptorContext context, org.neo4j.cluster.com.message.Message<AcceptorMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { if(message.getMessageType() == AcceptorMessage.join) { return acceptor; } return this; } },
		 start
		 {
			 public AcceptorState handle( AcceptorContext context, Message<AcceptorMessage> message, MessageHolder outgoing )
			 {
				 if ( message.MessageType == AcceptorMessage.Join ) { return acceptor; } return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: acceptor { @Override public AcceptorState handle(AcceptorContext context, org.neo4j.cluster.com.message.Message<AcceptorMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case prepare: { AcceptorMessage.PrepareState incomingState = message.getPayload(); InstanceId instanceId = new InstanceId(message); AcceptorInstance localState = context.getAcceptorInstance(instanceId); if(incomingState.getBallot() >= localState.getBallot()) { context.promise(localState, incomingState.getBallot()); outgoing.offer(message.copyHeadersTo(org.neo4j.cluster.com.message.Message.respond(ProposerMessage.promise, message, new ProposerMessage.PromiseState(incomingState.getBallot(), localState.getValue())), InstanceId.INSTANCE)); } else { context.getLog(AcceptorState.class).debug("Rejecting prepare from " + message.getHeader(org.neo4j.cluster.com.message.Message.HEADER_FROM) + " for instance " + message.getHeader(InstanceId.INSTANCE) + " and ballot " + incomingState.getBallot() + " (i had a prepare state ballot = " + localState.getBallot() + ")"); outgoing.offer(message.copyHeadersTo(org.neo4j.cluster.com.message.Message.respond(ProposerMessage.rejectPrepare, message, new ProposerMessage.RejectPrepare(localState.getBallot())), InstanceId.INSTANCE)); } break; } case accept: { AcceptorMessage.AcceptState acceptState = message.getPayload(); InstanceId instanceId = new InstanceId(message); AcceptorInstance instance = context.getAcceptorInstance(instanceId); if(acceptState.getBallot() == instance.getBallot()) { context.accept(instance, acceptState.getValue()); instance.accept(acceptState.getValue()); outgoing.offer(message.copyHeadersTo(org.neo4j.cluster.com.message.Message.respond(ProposerMessage.accepted, message, new ProposerMessage.AcceptedState()), InstanceId.INSTANCE)); } else { context.getLog(AcceptorState.class).debug("Reject " + instanceId + " accept ballot:" + acceptState.getBallot() + " actual ballot:" + instance.getBallot()); outgoing.offer(message.copyHeadersTo(org.neo4j.cluster.com.message.Message.respond(ProposerMessage.rejectAccept, message, new ProposerMessage.RejectAcceptState()), InstanceId.INSTANCE)); } break; } case leave: { context.leave(); return start; } default: break; } return this; } },
		 acceptor
		 {
			 public AcceptorState handle( AcceptorContext context, Message<AcceptorMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case prepare:
					 {
						 AcceptorMessage.PrepareState incomingState = message.Payload; InstanceId instanceId = new InstanceId( message ); AcceptorInstance localState = context.getAcceptorInstance( instanceId ); if ( incomingState.Ballot >= localState.Ballot ) { context.promise( localState, incomingState.Ballot ); outgoing.offer( message.copyHeadersTo( Message.respond( ProposerMessage.Promise, message, new ProposerMessage.PromiseState( incomingState.Ballot, localState.Value ) ), InstanceId.INSTANCE ) ); } else { context.getLog( typeof( AcceptorState ) ).debug( "Rejecting prepare from " + message.getHeader( Message.HEADER_FROM ) + " for instance " + message.getHeader( InstanceId.INSTANCE ) + " and ballot " + incomingState.Ballot + " (i had a prepare state ballot = " + localState.Ballot + ")" ); outgoing.offer( message.copyHeadersTo( Message.respond( ProposerMessage.RejectPrepare, message, new ProposerMessage.RejectPrepare( localState.Ballot ) ), InstanceId.INSTANCE ) ); } break;
					 }
					 case accept:
					 {
						 AcceptorMessage.AcceptState acceptState = message.Payload; InstanceId instanceId = new InstanceId( message ); AcceptorInstance instance = context.getAcceptorInstance( instanceId ); if ( acceptState.Ballot == instance.Ballot ) { context.accept( instance, acceptState.Value ); instance.accept( acceptState.Value ); outgoing.offer( message.copyHeadersTo( Message.respond( ProposerMessage.Accepted, message, new ProposerMessage.AcceptedState() ), InstanceId.INSTANCE ) ); } else { context.getLog(typeof(AcceptorState)).debug("Reject " + instanceId + " accept ballot:" + acceptState.Ballot + " actual ballot:" + instance.Ballot); outgoing.offer(message.copyHeadersTo(Message.respond(ProposerMessage.RejectAccept, message, new ProposerMessage.RejectAcceptState()), InstanceId.INSTANCE)); } break;
					 }
					 case leave: { context.leave(); return start; } default: break;
				 }
				 return this;
			 }
		 },
	}

}