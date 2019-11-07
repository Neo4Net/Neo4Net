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
namespace Neo4Net.cluster.protocol.cluster
{

	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using AtomicBroadcastMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using ProposerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;
	using Neo4Net.cluster.statemachine;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.com.message.Message.DISCOVERED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.com.message.Message.Internal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.com.message.Message.respond;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.com.message.Message.timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.com.message.Message.to;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.count;

	/// <summary>
	/// State machine for the Cluster API
	/// </summary>
	/// <seealso cref= Cluster </seealso>
	/// <seealso cref= ClusterMessage </seealso>
	public enum ClusterState
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: start { @Override public ClusterState handle(ClusterContext context, Neo4Net.cluster.com.message.Message<ClusterMessage> message, Neo4Net.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case addClusterListener: { context.addClusterListener(message.getPayload()); break; } case removeClusterListener: { context.removeClusterListener(message.getPayload()); break; } case create: { String name = message.getPayload(); context.getLog(ClusterState.class).info("Creating cluster: " + name); context.created(name); return entered; } case join: { Object[] args = message.getPayload(); String name = (String) args[0]; URI[] clusterInstanceUris = (java.net.URI[]) args[1]; context.joining(name, Neo4Net.helpers.collection.Iterables.iterable(clusterInstanceUris)); context.getLog(getClass()).info("Trying to join with DISCOVERY header " + context.generateDiscoveryHeader()); for(java.net.URI potentialClusterInstanceUri : clusterInstanceUris) { outgoing.offer(to(ClusterMessage.configurationRequest, potentialClusterInstanceUri, new ClusterMessage.ConfigurationRequestState(context.getMyId(), context.boundAt())).setHeader(DISCOVERED, context.generateDiscoveryHeader())); } context.setTimeout("discovery", timeout(ClusterMessage.configurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(1))); return discovery; } default: break; } return this; } },
		 start
		 {
			 public ClusterState handle( ClusterContext context, Message<ClusterMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case addClusterListener: { context.addClusterListener( message.Payload ); break; } case removeClusterListener: { context.removeClusterListener( message.Payload ); break; } case create: { string name = message.Payload; context.getLog( typeof( ClusterState ) ).info( "Creating cluster: " + name ); context.created( name ); return entered; } case join:
					 {
						 Object[] args = message.Payload; string name = ( string ) args[0]; URI[] clusterInstanceUris = ( URI[] ) args[1]; context.joining( name, Iterables.iterable( clusterInstanceUris ) ); context.getLog( this.GetType() ).info("Trying to join with DISCOVERY header " + context.generateDiscoveryHeader()); for (URI potentialClusterInstanceUri : clusterInstanceUris) { outgoing.offer(to(ClusterMessage.ConfigurationRequest, potentialClusterInstanceUri, new ClusterMessage.ConfigurationRequestState(context.MyId, context.boundAt())).setHeader(DISCOVERED, context.generateDiscoveryHeader())); } context.setTimeout("discovery", timeout(ClusterMessage.ConfigurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(1))); return discovery;
					 }
					 default: break;
				 }
				 return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: discovery { @Override public ClusterState handle(ClusterContext context, Neo4Net.cluster.com.message.Message<ClusterMessage> message, Neo4Net.cluster.com.message.MessageHolder outgoing) throws java.net.URISyntaxException { java.util.List<ClusterMessage.ConfigurationRequestState> discoveredInstances = context.getDiscoveredInstances(); context.getLog(getClass()).info(format("Discovered instances are %s", discoveredInstances)); switch(message.getMessageType()) { case configurationResponse: { context.cancelTimeout("discovery"); ClusterMessage.ConfigurationResponseState state = message.getPayload(); context.getLog(ClusterState.class).info("Joining cluster " + state.getClusterName()); if(!context.getConfiguration().getName().equals(state.getClusterName())) { context.getLog(ClusterState.class).warn("Joined cluster name is different than " + "the one configured. Expected " + context.getConfiguration().getName() + ", got " + state.getClusterName() + "."); } java.util.HashMap<Neo4Net.cluster.InstanceId, java.net.URI> memberList = new java.util.HashMap<>(state.getMembers()); context.discoveredLastReceivedInstanceId(state.getLatestReceivedInstanceId().getId()); context.acquiredConfiguration(memberList, state.getRoles(), state.getFailedMembers()); if(!memberList.containsKey(context.getMyId()) || !memberList.get(context.getMyId()).equals(context.boundAt())) { context.getLog(ClusterState.class).info(format("%s joining:%s, last delivered:%d", context.getMyId().toString(), context.getConfiguration().toString(), state.getLatestReceivedInstanceId().getId())); ClusterMessage.ConfigurationChangeState newState = new ClusterMessage.ConfigurationChangeState(); newState.join(context.getMyId(), context.boundAt()); Neo4Net.cluster.InstanceId coordinator = state.getRoles().get(ClusterConfiguration.COORDINATOR); if(coordinator != null) { java.net.URI coordinatorUri = context.getConfiguration().getUriForId(coordinator); outgoing.offer(to(Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage.propose, coordinatorUri, newState)); } else { outgoing.offer(to(Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage.propose, new java.net.URI(message.getHeader(Neo4Net.cluster.com.message.Message.HEADER_FROM)), newState)); } context.getLog(ClusterState.class).debug("Setup join timeout for " + message.getHeader(Neo4Net.cluster.com.message.Message.HEADER_CONVERSATION_ID)); context.setTimeout("join", timeout(ClusterMessage.joiningTimeout, message, new java.net.URI(message.getHeader(Neo4Net.cluster.com.message.Message.HEADER_FROM)))); return joining; } else { context.joined(); outgoing.offer(internal(ClusterMessage.joinResponse, context.getConfiguration())); return entered; } } case configurationTimeout: { if(context.hasJoinBeenDenied()) { outgoing.offer(internal(ClusterMessage.joinFailure, new ClusterEntryDeniedException(context.getMyId(), context.getJoinDeniedConfigurationResponseState()))); return start; } ClusterMessage.ConfigurationTimeoutState state = message.getPayload(); if(state.getRemainingPings() > 0) { context.getLog(getClass()).info(format("Trying to join with DISCOVERY header %s", context.generateDiscoveryHeader())); for(java.net.URI potentialClusterInstanceUri : context.getJoiningInstances()) { outgoing.offer(to(ClusterMessage.configurationRequest, potentialClusterInstanceUri, new ClusterMessage.ConfigurationRequestState(context.getMyId(), context.boundAt())).setHeader(DISCOVERED, context.generateDiscoveryHeader())); } context.setTimeout("join", timeout(ClusterMessage.configurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(state.getRemainingPings() - 1))); } else { if(!discoveredInstances.isEmpty() || count(context.getJoiningInstances()) == 1) { java.util.Collections.sort(discoveredInstances); ClusterMessage.ConfigurationRequestState ourRequestState = new ClusterMessage.ConfigurationRequestState(context.getMyId(), context.boundAt()); boolean imAlone = count(context.getJoiningInstances()) == 1 && discoveredInstances.contains(ourRequestState) && discoveredInstances.size() == 1; boolean haveDiscoveredMajority = discoveredInstances.size() >= Neo4Net.helpers.collection.Iterables.count(context.getJoiningInstances()); boolean wantToStartCluster = !discoveredInstances.isEmpty() && discoveredInstances.get(0).getJoiningId().compareTo(context.getMyId()) >= 0; if(imAlone || haveDiscoveredMajority && wantToStartCluster) { discoveredInstances.clear(); outgoing.offer(internal(ClusterMessage.joinFailure, new java.util.concurrent.TimeoutException("Join failed, timeout waiting for configuration"))); return start; } else { discoveredInstances.clear(); context.getLog(getClass()).info(format("Trying to join with DISCOVERY header %s", context.generateDiscoveryHeader())); for(java.net.URI potentialClusterInstanceUri : context.getJoiningInstances()) { outgoing.offer(to(ClusterMessage.configurationRequest, potentialClusterInstanceUri, new ClusterMessage.ConfigurationRequestState(context.getMyId(), context.boundAt())).setHeader(DISCOVERED, context.generateDiscoveryHeader())); } context.setTimeout("discovery", timeout(ClusterMessage.configurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(4))); } } else { context.setTimeout("join", timeout(ClusterMessage.configurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(4))); } } return this; } case configurationRequest: { ClusterMessage.ConfigurationRequestState configurationRequested = message.getPayload(); configurationRequested = new ClusterMessage.ConfigurationRequestState(configurationRequested.getJoiningId(), java.net.URI.create(message.getHeader(Neo4Net.cluster.com.message.Message.HEADER_FROM))); context.addContactingInstance(configurationRequested, message.getHeader(DISCOVERED, "")); context.getLog(getClass()).info(format("Received configuration request %s and " + "the header was %s", configurationRequested, message.getHeader(DISCOVERED, ""))); if(!discoveredInstances.contains(configurationRequested)) { for(ClusterMessage.ConfigurationRequestState discoveredInstance : discoveredInstances) { if(discoveredInstance.getJoiningId().equals(configurationRequested.getJoiningId())) { outgoing.offer(internal(ClusterMessage.joinFailure, new IllegalStateException(format("Failed to join cluster because I saw two instances with the " + "same ServerId. One is %s. The other is %s", discoveredInstance, configurationRequested)))); return start; } } if(context.shouldFilterContactingInstances()) { if(context.haveWeContactedInstance(configurationRequested)) { context.getLog(getClass()).info(format("%s had header %s which " + "contains us. This means we've contacted them and they are in our " + "initial hosts.", configurationRequested, message.getHeader(DISCOVERED, ""))); discoveredInstances.add(configurationRequested); } else { context.getLog(getClass()).warn(format("joining instance %s was not in %s, i will not consider it " + "for " + "purposes of cluster creation", configurationRequested.getJoiningUri(), context.getJoiningInstances())); } } else { discoveredInstances.add(configurationRequested); } } break; } case joinDenied: { context.joinDenied(message.getPayload()); return this; } default: break; } return this; } },
		 discovery
		 {
			 public ClusterState handle( ClusterContext context, Message<ClusterMessage> message, MessageHolder outgoing ) throws URISyntaxException
			 {
				 IList<ClusterMessage.ConfigurationRequestState> discoveredInstances = context.DiscoveredInstances; context.getLog( this.GetType() ).info(format("Discovered instances are %s", discoveredInstances)); switch (message.MessageType)
				 {
					 case configurationResponse:
					 {
						 context.cancelTimeout( "discovery" ); ClusterMessage.ConfigurationResponseState state = message.Payload; context.getLog( typeof( ClusterState ) ).info( "Joining cluster " + state.ClusterName ); if ( !context.Configuration.Name.Equals( state.ClusterName ) ) { context.getLog( typeof( ClusterState ) ).warn( "Joined cluster name is different than " + "the one configured. Expected " + context.Configuration.Name + ", got " + state.ClusterName + "." ); } Dictionary<InstanceId, URI> memberList = new Dictionary<>( state.Members ); context.discoveredLastReceivedInstanceId( state.LatestReceivedInstanceId.Id ); context.acquiredConfiguration( memberList, state.Roles, state.FailedMembers ); if ( !memberList.containsKey( context.MyId ) || !memberList.get( context.MyId ).Equals( context.boundAt() ) )
						 {
							 context.getLog( typeof( ClusterState ) ).info( format( "%s joining:%s, last delivered:%d", context.MyId.ToString(), context.Configuration.ToString(), state.LatestReceivedInstanceId.Id ) ); ClusterMessage.ConfigurationChangeState newState = new ClusterMessage.ConfigurationChangeState(); newState.join(context.MyId, context.boundAt()); InstanceId coordinator = state.Roles.get(ClusterConfiguration.COORDINATOR); if (coordinator != null) { URI coordinatorUri = context.Configuration.getUriForId(coordinator); outgoing.offer(to(ProposerMessage.propose, coordinatorUri, newState)); } else { outgoing.offer(to(ProposerMessage.propose, new URI(message.getHeader(Message.HEADER_FROM)), newState)); } context.getLog(typeof(ClusterState)).debug("Setup join timeout for " + message.getHeader(Message.HEADER_CONVERSATION_ID)); context.setTimeout("join", timeout(ClusterMessage.JoiningTimeout, message, new URI(message.getHeader(Message.HEADER_FROM)))); return joining;
						 }
						 else { context.joined(); outgoing.offer(@internal(ClusterMessage.JoinResponse, context.Configuration)); return entered; }
					 }
					 case configurationTimeout:
					 {
						 if ( context.hasJoinBeenDenied() ) { outgoing.offer(@internal(ClusterMessage.JoinFailure, new ClusterEntryDeniedException(context.MyId, context.JoinDeniedConfigurationResponseState))); return start; } ClusterMessage.ConfigurationTimeoutState state = message.Payload; if (state.RemainingPings > 0)
						 {
							 context.getLog( this.GetType() ).info(format("Trying to join with DISCOVERY header %s", context.generateDiscoveryHeader())); for (URI potentialClusterInstanceUri : context.JoiningInstances) { outgoing.offer(to(ClusterMessage.ConfigurationRequest, potentialClusterInstanceUri, new ClusterMessage.ConfigurationRequestState(context.MyId, context.boundAt())).setHeader(DISCOVERED, context.generateDiscoveryHeader())); } context.setTimeout("join", timeout(ClusterMessage.ConfigurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(state.RemainingPings - 1)));
						 }
						 else
						 {
							 if ( !discoveredInstances.Empty || count( context.JoiningInstances ) == 1 )
							 {
								 Collections.sort( discoveredInstances ); ClusterMessage.ConfigurationRequestState ourRequestState = new ClusterMessage.ConfigurationRequestState( context.MyId, context.boundAt() ); bool imAlone = count(context.JoiningInstances) == 1 && discoveredInstances.contains(ourRequestState) && discoveredInstances.size() == 1; bool haveDiscoveredMajority = discoveredInstances.size() >= Iterables.count(context.JoiningInstances); bool wantToStartCluster = !discoveredInstances.Empty && discoveredInstances.get(0).JoiningId.compareTo(context.MyId) >= 0; if (imAlone || haveDiscoveredMajority && wantToStartCluster) { discoveredInstances.clear(); outgoing.offer(@internal(ClusterMessage.JoinFailure, new TimeoutException("Join failed, timeout waiting for configuration"))); return start; } else
								 {
									 discoveredInstances.clear(); context.getLog(this.GetType()).info(format("Trying to join with DISCOVERY header %s", context.generateDiscoveryHeader())); for (URI potentialClusterInstanceUri : context.JoiningInstances) { outgoing.offer(to(ClusterMessage.ConfigurationRequest, potentialClusterInstanceUri, new ClusterMessage.ConfigurationRequestState(context.MyId, context.boundAt())).setHeader(DISCOVERED, context.generateDiscoveryHeader())); } context.setTimeout("discovery", timeout(ClusterMessage.ConfigurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(4)));
								 }
							 }
							 else { context.setTimeout( "join", timeout( ClusterMessage.ConfigurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState( 4 ) ) ); }
						 }
						 return this;
					 }
					 case configurationRequest:
					 {
						 ClusterMessage.ConfigurationRequestState configurationRequested = message.Payload; configurationRequested = new ClusterMessage.ConfigurationRequestState( configurationRequested.JoiningId, URI.create( message.getHeader( Message.HEADER_FROM ) ) ); context.addContactingInstance( configurationRequested, message.getHeader( DISCOVERED, "" ) ); context.getLog( this.GetType() ).info(format("Received configuration request %s and " + "the header was %s", configurationRequested, message.getHeader(DISCOVERED, ""))); if (!discoveredInstances.contains(configurationRequested))
						 {
							 for ( ClusterMessage.ConfigurationRequestState discoveredInstance : discoveredInstances )
							 {
								 if ( discoveredInstance.JoiningId.Equals( configurationRequested.JoiningId ) ) { outgoing.offer( @internal( ClusterMessage.JoinFailure, new System.InvalidOperationException( format( "Failed to join cluster because I saw two instances with the " + "same ServerId. One is %s. The other is %s", discoveredInstance, configurationRequested ) ) ) ); return start; }
							 }
							 if ( context.shouldFilterContactingInstances() )
							 {
								 if ( context.haveWeContactedInstance( configurationRequested ) ) { context.getLog( this.GetType() ).info(format("%s had header %s which " + "contains us. This means we've contacted them and they are in our " + "initial hosts.", configurationRequested, message.getHeader(DISCOVERED, ""))); discoveredInstances.add(configurationRequested); } else { context.getLog(this.GetType()).warn(format("joining instance %s was not in %s, i will not consider it " + "for " + "purposes of cluster creation", configurationRequested.JoiningUri, context.JoiningInstances)); }
							 }
							 else { discoveredInstances.add( configurationRequested ); }
						 }
						 break;
					 }
					 case joinDenied: { context.joinDenied( message.Payload ); return this; } default: break;
				 }
				 return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: joining { @Override public ClusterState handle(ClusterContext context, Neo4Net.cluster.com.message.Message<ClusterMessage> message, Neo4Net.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case configurationChanged: { ClusterMessage.ConfigurationChangeState state = message.getPayload(); if(context.getMyId().equals(state.getJoin())) { context.cancelTimeout("join"); context.joined(); outgoing.offer(message.copyHeadersTo(internal(ClusterMessage.joinResponse, context.getConfiguration()))); return entered; } else { state.apply(context); return this; } } case joiningTimeout: { context.getLog(ClusterState.class).info("Join timeout for " + message.getHeader(Neo4Net.cluster.com.message.Message.HEADER_CONVERSATION_ID)); if(context.hasJoinBeenDenied()) { outgoing.offer(internal(ClusterMessage.joinFailure, new ClusterEntryDeniedException(context.getMyId(), context.getJoinDeniedConfigurationResponseState()))); return start; } for(java.net.URI potentialClusterInstanceUri : context.getJoiningInstances()) { outgoing.offer(to(ClusterMessage.configurationRequest, potentialClusterInstanceUri, new ClusterMessage.ConfigurationRequestState(context.getMyId(), context.boundAt())).setHeader(DISCOVERED, context.generateDiscoveryHeader())); } context.setTimeout("discovery", timeout(ClusterMessage.configurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(4))); return discovery; } case joinFailure: { return start; } default: break; } return this; } },
		 joining
		 {
			 public ClusterState handle( ClusterContext context, Message<ClusterMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case configurationChanged:
					 {
						 ClusterMessage.ConfigurationChangeState state = message.Payload; if ( context.MyId.Equals( state.Join ) ) { context.cancelTimeout( "join" ); context.joined(); outgoing.offer(message.copyHeadersTo(@internal(ClusterMessage.JoinResponse, context.Configuration))); return entered; } else { state.apply(context); return this; }
					 }
					 case joiningTimeout:
					 {
						 context.getLog( typeof( ClusterState ) ).info( "Join timeout for " + message.getHeader( Message.HEADER_CONVERSATION_ID ) ); if ( context.hasJoinBeenDenied() ) { outgoing.offer(@internal(ClusterMessage.JoinFailure, new ClusterEntryDeniedException(context.MyId, context.JoinDeniedConfigurationResponseState))); return start; } for (URI potentialClusterInstanceUri : context.JoiningInstances) { outgoing.offer(to(ClusterMessage.ConfigurationRequest, potentialClusterInstanceUri, new ClusterMessage.ConfigurationRequestState(context.MyId, context.boundAt())).setHeader(DISCOVERED, context.generateDiscoveryHeader())); } context.setTimeout("discovery", timeout(ClusterMessage.ConfigurationTimeout, message, new ClusterMessage.ConfigurationTimeoutState(4))); return discovery;
					 }
					 case joinFailure: { return start; } default: break;
				 }
				 return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: entered { @Override public ClusterState handle(ClusterContext context, Neo4Net.cluster.com.message.Message<ClusterMessage> message, Neo4Net.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case addClusterListener: { context.addClusterListener(message.getPayload()); break; } case removeClusterListener: { context.removeClusterListener(message.getPayload()); break; } case configurationRequest: { ClusterMessage.ConfigurationRequestState request = message.getPayload(); request = new ClusterMessage.ConfigurationRequestState(request.getJoiningId(), java.net.URI.create(message.getHeader(Neo4Net.cluster.com.message.Message.HEADER_FROM))); Neo4Net.cluster.InstanceId joiningId = request.getJoiningId(); java.net.URI joiningUri = request.getJoiningUri(); boolean isInCluster = context.getMembers().containsKey(joiningId); boolean isCurrentlyAlive = context.isCurrentlyAlive(joiningId); boolean messageComesFromSameHost = request.getJoiningId().equals(context.getMyId()); boolean otherInstanceJoiningWithSameId = context.isInstanceJoiningFromDifferentUri(joiningId, joiningUri); boolean isFromSameURIAsTheOneWeAlreadyKnow = context.getUriForId(joiningId) != null && context.getUriForId(joiningId).equals(joiningUri); boolean somethingIsWrong = (isInCluster && !messageComesFromSameHost && isCurrentlyAlive && !isFromSameURIAsTheOneWeAlreadyKnow) || otherInstanceJoiningWithSameId ; if(somethingIsWrong) { if(otherInstanceJoiningWithSameId) { context.getLog(ClusterState.class).info(format("Denying entry to instance %s" + " because another instance is currently joining with the same id.", joiningId)); } else { context.getLog(ClusterState.class).info(format("Denying entry to " + "instance %s because that instance is already in the cluster.", joiningId)); } outgoing.offer(message.copyHeadersTo(respond(ClusterMessage.joinDenied, message, new ClusterMessage.ConfigurationResponseState(context.getConfiguration().getRoles(), context.getConfiguration().getMembers(), new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId(context.getLastDeliveredInstanceId()), context.getFailedInstances(), context.getConfiguration().getName())))); } else { context.instanceIsJoining(joiningId, joiningUri); outgoing.offer(message.copyHeadersTo(respond(ClusterMessage.configurationResponse, message, new ClusterMessage.ConfigurationResponseState(context.getConfiguration().getRoles(), context.getConfiguration().getMembers(), new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId(context.getLastDeliveredInstanceId()), context.getFailedInstances(), context.getConfiguration().getName())))); } break; } case configurationChanged: { ClusterMessage.ConfigurationChangeState state = message.getPayload(); state.apply(context); break; } case leave: { java.util.List<java.net.URI> nodeList = new java.util.ArrayList<>(context.getConfiguration().getMemberURIs()); if(nodeList.size() == 1) { context.getLog(ClusterState.class).info(format("Shutting down cluster: %s", context.getConfiguration().getName())); context.left(); return start; } else { context.getLog(ClusterState.class).info(format("Leaving:%s", nodeList)); ClusterMessage.ConfigurationChangeState newState = new ClusterMessage.ConfigurationChangeState(); newState.leave(context.getMyId()); outgoing.offer(internal(Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage.broadcast, newState)); context.setTimeout("leave", timeout(ClusterMessage.leaveTimedout, message)); return leaving; } } default: break; } return this; } },
		 entered
		 {
			 public ClusterState handle( ClusterContext context, Message<ClusterMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case addClusterListener: { context.addClusterListener( message.Payload ); break; } case removeClusterListener: { context.removeClusterListener( message.Payload ); break; } case configurationRequest:
					 {
						 ClusterMessage.ConfigurationRequestState request = message.Payload; request = new ClusterMessage.ConfigurationRequestState( request.JoiningId, URI.create( message.getHeader( Message.HEADER_FROM ) ) ); InstanceId joiningId = request.JoiningId; URI joiningUri = request.JoiningUri; bool isInCluster = context.Members.containsKey( joiningId ); bool isCurrentlyAlive = context.isCurrentlyAlive( joiningId ); bool messageComesFromSameHost = request.JoiningId.Equals( context.MyId ); bool otherInstanceJoiningWithSameId = context.isInstanceJoiningFromDifferentUri( joiningId, joiningUri ); bool isFromSameURIAsTheOneWeAlreadyKnow = context.getUriForId( joiningId ) != null && context.getUriForId( joiningId ).Equals( joiningUri ); bool somethingIsWrong = ( isInCluster && !messageComesFromSameHost && isCurrentlyAlive && !isFromSameURIAsTheOneWeAlreadyKnow ) || otherInstanceJoiningWithSameId ; if ( somethingIsWrong )
						 {
							 if ( otherInstanceJoiningWithSameId ) { context.getLog( typeof( ClusterState ) ).info( format( "Denying entry to instance %s" + " because another instance is currently joining with the same id.", joiningId ) ); } else { context.getLog( typeof( ClusterState ) ).info( format( "Denying entry to " + "instance %s because that instance is already in the cluster.", joiningId ) ); } outgoing.offer( message.copyHeadersTo( respond( ClusterMessage.JoinDenied, message, new ClusterMessage.ConfigurationResponseState( context.Configuration.Roles, context.Configuration.Members, new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( context.LastDeliveredInstanceId ), context.FailedInstances, context.Configuration.Name ) ) ) );
						 }
						 else { context.instanceIsJoining( joiningId, joiningUri ); outgoing.offer( message.copyHeadersTo( respond( ClusterMessage.ConfigurationResponse, message, new ClusterMessage.ConfigurationResponseState( context.Configuration.Roles, context.Configuration.Members, new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( context.LastDeliveredInstanceId ), context.FailedInstances, context.Configuration.Name ) ) ) ); } break;
					 }
					 case configurationChanged: { ClusterMessage.ConfigurationChangeState state = message.Payload; state.apply( context ); break; } case leave:
					 {
						 IList<URI> nodeList = new List<>( context.Configuration.MemberURIs ); if ( nodeList.size() == 1 ) { context.getLog(typeof(ClusterState)).info(format("Shutting down cluster: %s", context.Configuration.Name)); context.left(); return start; } else { context.getLog(typeof(ClusterState)).info(format("Leaving:%s", nodeList)); ClusterMessage.ConfigurationChangeState newState = new ClusterMessage.ConfigurationChangeState(); newState.leave(context.MyId); outgoing.offer(@internal(AtomicBroadcastMessage.broadcast, newState)); context.setTimeout("leave", timeout(ClusterMessage.LeaveTimedout, message)); return leaving; }
					 }
					 default: break;
				 }
				 return this;
			 }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: leaving { @Override public ClusterState handle(ClusterContext context, Neo4Net.cluster.com.message.Message<ClusterMessage> message, Neo4Net.cluster.com.message.MessageHolder outgoing) { switch(message.getMessageType()) { case configurationChanged: { ClusterMessage.ConfigurationChangeState state = message.getPayload(); if(state.isLeaving(context.getMyId())) { context.cancelTimeout("leave"); context.left(); return start; } else { state.apply(context); return leaving; } } case leaveTimedout: { context.getLog(ClusterState.class).warn("Failed to leave. Cluster may consider this" + " instance still a member"); context.left(); return start; } default: break; } return this; } }
		 leaving
		 {
			 public ClusterState handle( ClusterContext context, Message<ClusterMessage> message, MessageHolder outgoing )
			 {
				 switch ( message.MessageType )
				 {
					 case configurationChanged:
					 {
						 ClusterMessage.ConfigurationChangeState state = message.Payload; if ( state.isLeaving( context.MyId ) ) { context.cancelTimeout( "leave" ); context.left(); return start; } else { state.apply(context); return leaving; }
					 }
					 case leaveTimedout: { context.getLog( typeof( ClusterState ) ).warn( "Failed to leave. Cluster may consider this" + " instance still a member" ); context.left(); return start; } default: break;
				 }
				 return this;
			 }
		 }
	}

}