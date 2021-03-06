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
namespace Org.Neo4j.cluster.protocol.snapshot
{
	using Org.Neo4j.cluster.com.message;
	using MessageHolder = Org.Neo4j.cluster.com.message.MessageHolder;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using Org.Neo4j.cluster.statemachine;

	/// <summary>
	/// State machine for the snapshot API
	/// </summary>
	public enum SnapshotState
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: start { @Override public SnapshotState handle(SnapshotContext context, org.neo4j.cluster.com.message.Message<SnapshotMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case setSnapshotProvider: { SnapshotProvider snapshotProvider = message.getPayload(); context.setSnapshotProvider(snapshotProvider); break; } case refreshSnapshot: { if(context.getClusterContext().getConfiguration().getMembers().size() <= 1 || context.getSnapshotProvider() == null) { return start; } else { org.neo4j.cluster.InstanceId coordinator = context.getClusterContext().getConfiguration().getElected(org.neo4j.cluster.protocol.cluster.ClusterConfiguration.COORDINATOR); if(coordinator != null) { outgoing.offer(org.neo4j.cluster.com.message.Message.to(SnapshotMessage.sendSnapshot, context.getClusterContext().getConfiguration().getUriForId(coordinator))); return refreshing; } else { return start; } } } case join: { return ready; } default: break; } return this; } },
		 start
		 {
			 public SnapshotState handle( SnapshotContext context, Message<SnapshotMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case setSnapshotProvider: { SnapshotProvider snapshotProvider = message.Payload; context.setSnapshotProvider( snapshotProvider ); break; } case refreshSnapshot:
					 {
						 if ( context.ClusterContext.Configuration.Members.size() <= 1 || context.SnapshotProvider == null ) { return start; } else
						 {
							 InstanceId coordinator = context.ClusterContext.Configuration.getElected( ClusterConfiguration.COORDINATOR ); if ( coordinator != null ) { outgoing.offer( Message.to( SnapshotMessage.SendSnapshot, context.ClusterContext.Configuration.getUriForId( coordinator ) ) ); return refreshing; } else { return start; }
						 }
					 }
					 case join: { return ready; } default: break;
				 }
				 return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: refreshing { @Override public SnapshotState handle(SnapshotContext context, org.neo4j.cluster.com.message.Message<SnapshotMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { if(message.getMessageType() == SnapshotMessage.snapshot) { SnapshotMessage.SnapshotState state = message.getPayload(); state.setState(context.getSnapshotProvider(), context.getClusterContext().getObjectInputStreamFactory()); return ready; } return this; } },
		 refreshing
		 {
			 public SnapshotState handle( SnapshotContext context, Message<SnapshotMessage> message, MessageHolder outgoing )
			 {
				 if ( message.MessageType == SnapshotMessage.Snapshot ) { SnapshotMessage.SnapshotState state = message.Payload; state.setState( context.SnapshotProvider, context.ClusterContext.ObjectInputStreamFactory ); return ready; } return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: ready { @Override public SnapshotState handle(SnapshotContext context, org.neo4j.cluster.com.message.Message<SnapshotMessage> message, org.neo4j.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case refreshSnapshot: { if(context.getClusterContext().getConfiguration().getMembers().size() <= 1 || context.getSnapshotProvider() == null) { return ready; } else { org.neo4j.cluster.InstanceId coordinator = context.getClusterContext().getConfiguration().getElected(org.neo4j.cluster.protocol.cluster.ClusterConfiguration.COORDINATOR); if(coordinator != null && !coordinator.equals(context.getClusterContext().getMyId())) { outgoing.offer(org.neo4j.cluster.com.message.Message.to(SnapshotMessage.sendSnapshot, context.getClusterContext().getConfiguration().getUriForId(coordinator))); return refreshing; } else { return ready; } } } case sendSnapshot: { outgoing.offer(org.neo4j.cluster.com.message.Message.respond(SnapshotMessage.snapshot, message, new SnapshotMessage.SnapshotState(context.getLearnerContext().getLastDeliveredInstanceId(), context.getSnapshotProvider(), context.getClusterContext().getObjectOutputStreamFactory()))); break; } case leave: { return start; } default: break; } return this; } }
		 ready
		 {
			 public SnapshotState handle( SnapshotContext context, Message<SnapshotMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case refreshSnapshot:
					 {
						 if ( context.ClusterContext.Configuration.Members.size() <= 1 || context.SnapshotProvider == null ) { return ready; } else
						 {
							 InstanceId coordinator = context.ClusterContext.Configuration.getElected( ClusterConfiguration.COORDINATOR ); if ( coordinator != null && !coordinator.Equals( context.ClusterContext.MyId ) ) { outgoing.offer( Message.to( SnapshotMessage.SendSnapshot, context.ClusterContext.Configuration.getUriForId( coordinator ) ) ); return refreshing; } else { return ready; }
						 }
					 }
					 case sendSnapshot: { outgoing.offer( Message.respond( SnapshotMessage.Snapshot, message, new SnapshotMessage.SnapshotState( context.LearnerContext.LastDeliveredInstanceId, context.SnapshotProvider, context.ClusterContext.ObjectOutputStreamFactory ) ) ); break; } case leave: { return start; } default: break;
				 }
				 return this;
			 }
		 }
	}

}