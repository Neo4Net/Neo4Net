using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.cluster.protocol.election
{

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using AtomicBroadcastMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using ProposerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using Neo4Net.cluster.statemachine;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.firstOrNull;

	/// <summary>
	/// State machine that implements the <seealso cref="Election"/> API.
	/// </summary>
	public enum ElectionState
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: start { @Override public ElectionState handle(ElectionContext context, Neo4Net.cluster.com.message.Message<ElectionMessage> message, Neo4Net.cluster.com.message.MessageHolder outgoing) { if(message.getMessageType() == ElectionMessage.created) { context.created(); return election; } else if(message.getMessageType() == ElectionMessage.join) { return election; } return this; } },
		 start
		 {
			 public ElectionState handle( ElectionContext context, Message<ElectionMessage> message, MessageHolder outgoing )
			 {
				 if ( message.MessageType == ElectionMessage.Created ) { context.created(); return election; } else if (message.MessageType == ElectionMessage.Join) { return election; } return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: election { @Override public ElectionState handle(ElectionContext context, Neo4Net.cluster.com.message.Message<ElectionMessage> message, Neo4Net.cluster.com.message.MessageHolder outgoing) { Neo4Net.logging.Log log = context.getLog(ElectionState.class); switch(message.getMessageType()) { case demote: { if(!context.electionOk()) { log.warn("Context says election is not OK to proceed. " + "Failed instances are: " + context.getFailed() + ", cluster members are: " + context.getMembers()); break; } Neo4Net.cluster.InstanceId demoteNode = message.getPayload(); context.nodeFailed(demoteNode); if(context.isInCluster()) { java.util.List<Neo4Net.cluster.InstanceId> aliveInstances = Neo4Net.helpers.collection.Iterables.asList(context.getAlive()); java.util.Collections.sort(aliveInstances); boolean isElector = aliveInstances.indexOf(context.getMyId()) == 0; if(isElector) { log.debug("I (" + context.getMyId() + ") am the elector, executing the election"); Iterable<String> rolesRequiringElection = context.getRolesRequiringElection(); for(String role : rolesRequiringElection) { if(!context.isElectionProcessInProgress(role)) { log.debug("Starting election process for role " + role); context.startElectionProcess(role); for(java.util.Map.Entry<Neo4Net.cluster.InstanceId, java.net.URI> server : context.getMembers().entrySet()) { if(!context.getFailed().contains(server.getKey())) { outgoing.offer(Neo4Net.cluster.com.message.Message.to(ElectionMessage.vote, server.getValue(), context.voteRequestForRole(new ElectionRole(role)))); } } context.setTimeout("election-" + role, Neo4Net.cluster.com.message.Message.timeout(ElectionMessage.electionTimeout, message, new ElectionTimeoutData(role, message))); } else { log.debug("Election already in progress for role " + role); } } } } break; } case performRoleElections: { if(!context.electionOk()) { log.warn("Context says election is not OK to proceed. " + "Failed instances are: " + context.getFailed() + ", cluster members are: " + context.getMembers()); break; } if(context.isInCluster()) { boolean isElector = context.isElector(); if(isElector) { context.getLog(ElectionState.class).info("I am the elector, doing election..."); Iterable<ElectionRole> rolesRequiringElection = context.getPossibleRoles(); for(ElectionRole role : rolesRequiringElection) { String roleName = role.getName(); if(!context.isElectionProcessInProgress(roleName)) { context.getLog(ElectionState.class).debug("Starting election process for role " + roleName); context.startElectionProcess(roleName); boolean sentSome = false; for(java.util.Map.Entry<Neo4Net.cluster.InstanceId, java.net.URI> server : context.getMembers().entrySet()) { if(!context.isFailed(server.getKey()) && !server.getKey().equals(context.getElected(roleName))) { outgoing.offer(Neo4Net.cluster.com.message.Message.to(ElectionMessage.vote, server.getValue(), context.voteRequestForRole(role))); sentSome = true; } } if(!sentSome) { outgoing.offer(Neo4Net.cluster.com.message.Message.internal(ElectionMessage.vote, context.voteRequestForRole(new ElectionRole(roleName)))); } else { context.setTimeout("election-" + roleName, Neo4Net.cluster.com.message.Message.timeout(ElectionMessage.electionTimeout, message, new ElectionTimeoutData(roleName, message))); } } else { log.debug("Election already in progress for role " + roleName); } } } else { java.util.Set<Neo4Net.cluster.InstanceId> aliveInstances = Neo4Net.helpers.collection.Iterables.asSet(context.getAlive()); aliveInstances.removeAll(context.getFailed()); java.util.List<Neo4Net.cluster.InstanceId> adjustedAlive = Neo4Net.helpers.collection.Iterables.asList(aliveInstances); java.util.Collections.sort(adjustedAlive); context.getLog(ElectionState.class).info("I am NOT the elector, sending to " + adjustedAlive); outgoing.offer(message.setHeader(Neo4Net.cluster.com.message.Message.HEADER_TO, context.getUriForId(firstOrNull(adjustedAlive)).toString())); } } break; } case vote: { Object request = message.getPayload(); ElectionContext_VoteRequest voteRequest = (ElectionContext_VoteRequest) request; outgoing.offer(Neo4Net.cluster.com.message.Message.respond(ElectionMessage.voted, message, new ElectionMessage.VersionedVotedData(voteRequest.getRole(), context.getMyId(), context.getCredentialsForRole(voteRequest.getRole()), voteRequest.getVersion()))); break; } case voted: { ElectionMessage.VotedData data = message.getPayload(); long version = -1; if(data instanceof ElectionMessage.VersionedVotedData) { version = ((ElectionMessage.VersionedVotedData) data).getVersion(); } boolean accepted = context.voted(data.getRole(), data.getInstanceId(), data.getElectionCredentials(), version); String voter = message.hasHeader(Neo4Net.cluster.com.message.Message.HEADER_FROM) ? message.getHeader(Neo4Net.cluster.com.message.Message.HEADER_FROM) : "I"; log.debug(voter + " voted " + data + " which i " + (accepted ? "accepted" : "did not accept")); if(!accepted) { break; } Neo4Net.cluster.InstanceId currentElected = context.getElected(data.getRole()); if(context.getVoteCount(data.getRole()) == context.getNeededVoteCount()) { Neo4Net.cluster.InstanceId winner = context.getElectionWinner(data.getRole()); context.cancelTimeout("election-" + data.getRole()); context.forgetElection(data.getRole()); if(winner != null) { log.debug("Elected " + winner + " as " + data.getRole()); Neo4Net.cluster.protocol.cluster.ClusterMessage.VersionedConfigurationStateChange configurationChangeState = context.newConfigurationStateChange(); configurationChangeState.elected(data.getRole(), winner); outgoing.offer(Neo4Net.cluster.com.message.Message.internal(Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage.broadcast, configurationChangeState)); } else { log.warn("Election could not pick a winner"); if(currentElected != null) { Neo4Net.cluster.protocol.cluster.ClusterMessage.ConfigurationChangeState configurationChangeState = new Neo4Net.cluster.protocol.cluster.ClusterMessage.ConfigurationChangeState(); configurationChangeState.unelected(data.getRole(), currentElected); outgoing.offer(Neo4Net.cluster.com.message.Message.internal(Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage.propose, configurationChangeState)); } } } else if(context.getVoteCount(data.getRole()) == context.getNeededVoteCount() - 1 && currentElected != null && !context.hasCurrentlyElectedVoted(data.getRole(), currentElected)) { outgoing.offer(Neo4Net.cluster.com.message.Message.to(ElectionMessage.vote, context.getUriForId(currentElected), context.voteRequestForRole(new ElectionRole(data.getRole())))); } break; } case electionTimeout: { ElectionTimeoutData electionTimeoutData = message.getPayload(); log.warn(String.format("Election timed out for '%s'- trying again", electionTimeoutData.getRole())); context.forgetElection(electionTimeoutData.getRole()); outgoing.offer(electionTimeoutData.getMessage()); break; } case leave: { return start; } default: break; } return this; } };
		 election
		 {
			 public ElectionState handle( ElectionContext context, Message<ElectionMessage> message, MessageHolder outgoing )
			 {
				 Log log = context.getLog( typeof( ElectionState ) ); switch ( message.MessageType )
				 {
					 case demote:
					 {
						 if ( !context.electionOk() ) { log.warn("Context says election is not OK to proceed. " + "Failed instances are: " + context.Failed + ", cluster members are: " + context.Members); break; } InstanceId demoteNode = message.Payload; context.nodeFailed(demoteNode); if (context.InCluster)
						 {
							 IList<InstanceId> aliveInstances = Iterables.asList( context.Alive ); Collections.sort( aliveInstances ); bool isElector = aliveInstances.IndexOf( context.MyId ) == 0; if ( isElector )
							 {
								 log.debug( "I (" + context.MyId + ") am the elector, executing the election" ); IEnumerable<string> rolesRequiringElection = context.RolesRequiringElection; for ( string role : rolesRequiringElection )
								 {
									 if ( !context.isElectionProcessInProgress( role ) )
									 {
										 log.debug( "Starting election process for role " + role ); context.startElectionProcess( role ); for ( KeyValuePair<InstanceId, URI> server : context.Members.entrySet() )
										 {
											 if ( !context.Failed.contains( server.Key ) ) { outgoing.offer( Message.to( ElectionMessage.Vote, server.Value, context.voteRequestForRole( new ElectionRole( role ) ) ) ); }
										 }
										 context.setTimeout( "election-" + role, Message.timeout( ElectionMessage.ElectionTimeout, message, new ElectionTimeoutData( role, message ) ) );
									 }
									 else { log.debug( "Election already in progress for role " + role ); }
								 }
							 }
						 }
						 break;
					 }
					 case performRoleElections:
					 {
						 if ( !context.electionOk() ) { log.warn("Context says election is not OK to proceed. " + "Failed instances are: " + context.Failed + ", cluster members are: " + context.Members); break; } if (context.InCluster)
						 {
							 bool isElector = context.Elector; if ( isElector )
							 {
								 context.getLog( typeof( ElectionState ) ).info( "I am the elector, doing election..." ); IEnumerable<ElectionRole> rolesRequiringElection = context.PossibleRoles; for ( ElectionRole role : rolesRequiringElection )
								 {
									 string roleName = role.Name; if ( !context.isElectionProcessInProgress( roleName ) )
									 {
										 context.getLog( typeof( ElectionState ) ).debug( "Starting election process for role " + roleName ); context.startElectionProcess( roleName ); bool sentSome = false; for ( KeyValuePair<InstanceId, URI> server : context.Members.entrySet() )
										 {
											 if ( !context.isFailed( server.Key ) && !server.Key.Equals( context.getElected( roleName ) ) ) { outgoing.offer( Message.to( ElectionMessage.Vote, server.Value, context.voteRequestForRole( role ) ) ); sentSome = true; }
										 }
										 if ( !sentSome ) { outgoing.offer( Message.Internal( ElectionMessage.Vote, context.voteRequestForRole( new ElectionRole( roleName ) ) ) ); } else { context.setTimeout( "election-" + roleName, Message.timeout( ElectionMessage.ElectionTimeout, message, new ElectionTimeoutData( roleName, message ) ) ); }
									 }
									 else { log.debug( "Election already in progress for role " + roleName ); }
								 }
							 }
							 else { ISet<InstanceId> aliveInstances = Iterables.asSet( context.Alive ); aliveInstances.removeAll( context.Failed ); IList<InstanceId> adjustedAlive = Iterables.asList( aliveInstances ); Collections.sort( adjustedAlive ); context.getLog( typeof( ElectionState ) ).info( "I am NOT the elector, sending to " + adjustedAlive ); outgoing.offer( message.setHeader( Message.HEADER_TO, context.getUriForId( firstOrNull( adjustedAlive ) ).ToString() ) ); }
						 }
						 break;
					 }
					 case vote: { object request = message.Payload; ElectionContext_VoteRequest voteRequest = ( ElectionContext_VoteRequest ) request; outgoing.offer( Message.respond( ElectionMessage.Voted, message, new ElectionMessage.VersionedVotedData( voteRequest.Role, context.MyId, context.getCredentialsForRole( voteRequest.Role ), voteRequest.Version ) ) ); break; } case voted:
					 {
						 ElectionMessage.VotedData data = message.Payload; long version = -1; if ( data is ElectionMessage.VersionedVotedData ) { version = ( ( ElectionMessage.VersionedVotedData ) data ).Version; } bool accepted = context.voted( data.Role, data.InstanceId, data.ElectionCredentials, version ); string voter = message.hasHeader( Message.HEADER_FROM ) ? message.getHeader( Message.HEADER_FROM ) : "I"; log.debug( voter + " voted " + data + " which i " + ( accepted ? "accepted" : "did not accept" ) ); if ( !accepted ) { break; } InstanceId currentElected = context.getElected( data.Role ); if ( context.getVoteCount( data.Role ) == context.NeededVoteCount )
						 {
							 InstanceId winner = context.getElectionWinner( data.Role ); context.cancelTimeout( "election-" + data.Role ); context.forgetElection( data.Role ); if ( winner != null ) { log.debug( "Elected " + winner + " as " + data.Role ); ClusterMessage.VersionedConfigurationStateChange configurationChangeState = context.newConfigurationStateChange(); configurationChangeState.elected(data.Role, winner); outgoing.offer(Message.Internal(AtomicBroadcastMessage.broadcast, configurationChangeState)); } else
							 {
								 log.warn( "Election could not pick a winner" ); if ( currentElected != null ) { ClusterMessage.ConfigurationChangeState configurationChangeState = new ClusterMessage.ConfigurationChangeState(); configurationChangeState.unelected(data.Role, currentElected); outgoing.offer(Message.Internal(ProposerMessage.propose, configurationChangeState)); }
							 }
						 }
						 else if ( context.getVoteCount( data.Role ) == context.NeededVoteCount - 1 && currentElected != null && !context.hasCurrentlyElectedVoted( data.Role, currentElected ) ) { outgoing.offer( Message.to( ElectionMessage.Vote, context.getUriForId( currentElected ), context.voteRequestForRole( new ElectionRole( data.Role ) ) ) ); } break;
					 }
					 case electionTimeout: { ElectionTimeoutData electionTimeoutData = message.Payload; log.warn( string.Format( "Election timed out for '{0}'- trying again", electionTimeoutData.Role ) ); context.forgetElection( electionTimeoutData.Role ); outgoing.offer( electionTimeoutData.Message ); break; } case leave: { return start; } default: break;
				 }
				 return this;
			 }
		 };

		 public static class ElectionTimeoutData
		 {
			 private final string role; private final Message message; public ElectionTimeoutData( string role, Message message ) { this.role = role; this.message = message; } public string Role { return role; } public Message Message { return message; }
		 }
	}

}