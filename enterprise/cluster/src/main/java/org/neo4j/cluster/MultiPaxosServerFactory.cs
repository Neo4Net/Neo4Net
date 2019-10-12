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
namespace Org.Neo4j.cluster
{

	using MessageSender = Org.Neo4j.cluster.com.message.MessageSender;
	using MessageSource = Org.Neo4j.cluster.com.message.MessageSource;
	using ObjectInputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using AcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using AcceptorMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorMessage;
	using AcceptorState = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorState;
	using AtomicBroadcastMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using AtomicBroadcastState = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastState;
	using LearnerMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage;
	using LearnerState = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.LearnerState;
	using ProposerMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;
	using ProposerState = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.ProposerState;
	using MultiPaxosContext = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using Cluster = Org.Neo4j.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterMessage = Org.Neo4j.cluster.protocol.cluster.ClusterMessage;
	using ClusterState = Org.Neo4j.cluster.protocol.cluster.ClusterState;
	using ClusterLeaveReelectionListener = Org.Neo4j.cluster.protocol.election.ClusterLeaveReelectionListener;
	using Election = Org.Neo4j.cluster.protocol.election.Election;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionMessage = Org.Neo4j.cluster.protocol.election.ElectionMessage;
	using ElectionRole = Org.Neo4j.cluster.protocol.election.ElectionRole;
	using ElectionState = Org.Neo4j.cluster.protocol.election.ElectionState;
	using HeartbeatReelectionListener = Org.Neo4j.cluster.protocol.election.HeartbeatReelectionListener;
	using HeartbeatIAmAliveProcessor = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatIAmAliveProcessor;
	using HeartbeatJoinListener = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatJoinListener;
	using HeartbeatLeftListener = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatLeftListener;
	using HeartbeatMessage = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatMessage;
	using HeartbeatRefreshProcessor = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatRefreshProcessor;
	using HeartbeatState = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatState;
	using SnapshotContext = Org.Neo4j.cluster.protocol.snapshot.SnapshotContext;
	using SnapshotMessage = Org.Neo4j.cluster.protocol.snapshot.SnapshotMessage;
	using SnapshotState = Org.Neo4j.cluster.protocol.snapshot.SnapshotState;
	using Org.Neo4j.cluster.statemachine;
	using StateMachineRules = Org.Neo4j.cluster.statemachine.StateMachineRules;
	using TimeoutStrategy = Org.Neo4j.cluster.timeout.TimeoutStrategy;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.@internal;

	/// <summary>
	/// Factory for MultiPaxos <seealso cref="ProtocolServer"/>s.
	/// </summary>
	public class MultiPaxosServerFactory : ProtocolServerFactory
	{
		 private readonly ClusterConfiguration _initialConfig;
		 private readonly LogProvider _logging;
		 private StateMachines.Monitor _stateMachinesMonitor;

		 public MultiPaxosServerFactory( ClusterConfiguration initialConfig, LogProvider logging, StateMachines.Monitor stateMachinesMonitor )
		 {
			  this._initialConfig = initialConfig;
			  this._logging = logging;
			  this._stateMachinesMonitor = stateMachinesMonitor;
		 }

		 public override ProtocolServer NewProtocolServer( InstanceId me, TimeoutStrategy timeoutStrategy, MessageSource input, MessageSender output, AcceptorInstanceStore acceptorInstanceStore, ElectionCredentialsProvider electionCredentialsProvider, Executor stateMachineExecutor, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory, Config config )
		 {
			  DelayedDirectExecutor executor = new DelayedDirectExecutor( _logging );

			  // Create state machines
			  Timeouts timeouts = new Timeouts( timeoutStrategy );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext context = new org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext(me, org.neo4j.helpers.collection.Iterables.iterable(new org.neo4j.cluster.protocol.election.ElectionRole(org.neo4j.cluster.protocol.cluster.ClusterConfiguration.COORDINATOR)), new org.neo4j.cluster.protocol.cluster.ClusterConfiguration(initialConfig.getName(), logging, initialConfig.getMemberURIs()), executor, logging, objectInputStreamFactory, objectOutputStreamFactory, acceptorInstanceStore, timeouts, electionCredentialsProvider, config);
			  MultiPaxosContext context = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( ClusterConfiguration.COORDINATOR ) ), new ClusterConfiguration( _initialConfig.Name, _logging, _initialConfig.MemberURIs ), executor, _logging, objectInputStreamFactory, objectOutputStreamFactory, acceptorInstanceStore, timeouts, electionCredentialsProvider, config );

			  SnapshotContext snapshotContext = new SnapshotContext( context.ClusterContext, context.LearnerContext );

			  return NewProtocolServer( me, input, output, stateMachineExecutor, executor, timeouts, context, snapshotContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public ProtocolServer newProtocolServer(InstanceId me, org.neo4j.cluster.com.message.MessageSource input, org.neo4j.cluster.com.message.MessageSender output, java.util.concurrent.Executor stateMachineExecutor, DelayedDirectExecutor executor, org.neo4j.cluster.timeout.Timeouts timeouts, org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext context, org.neo4j.cluster.protocol.snapshot.SnapshotContext snapshotContext)
		 public virtual ProtocolServer NewProtocolServer( InstanceId me, MessageSource input, MessageSender output, Executor stateMachineExecutor, DelayedDirectExecutor executor, Timeouts timeouts, MultiPaxosContext context, SnapshotContext snapshotContext )
		 {
			  return ConstructSupportingInfrastructureFor(me, input, output, executor, timeouts, stateMachineExecutor, context, new StateMachine[]
			  {
				  new StateMachine( context.AtomicBroadcastContext, typeof( AtomicBroadcastMessage ), AtomicBroadcastState.start, _logging ),
				  new StateMachine( context.AcceptorContext, typeof( AcceptorMessage ), AcceptorState.start, _logging ),
				  new StateMachine( context.ProposerContext, typeof( ProposerMessage ), ProposerState.start, _logging ),
				  new StateMachine( context.LearnerContext, typeof( LearnerMessage ), LearnerState.start, _logging ),
				  new StateMachine( context.HeartbeatContext, typeof( HeartbeatMessage ), HeartbeatState.start, _logging ),
				  new StateMachine( context.ElectionContext, typeof( ElectionMessage ), ElectionState.start, _logging ),
				  new StateMachine( snapshotContext, typeof( SnapshotMessage ), SnapshotState.start, _logging ),
				  new StateMachine( context.ClusterContext, typeof( ClusterMessage ), ClusterState.start, _logging )
			  });
		 }

		 /// <summary>
		 /// Sets up the supporting infrastructure and communication hooks for our state machines. This is here to support
		 /// an external requirement for assembling protocol servers given an existing set of state machines (used to prove
		 /// correctness).
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") public ProtocolServer constructSupportingInfrastructureFor(InstanceId me, org.neo4j.cluster.com.message.MessageSource input, org.neo4j.cluster.com.message.MessageSender output, DelayedDirectExecutor executor, org.neo4j.cluster.timeout.Timeouts timeouts, java.util.concurrent.Executor stateMachineExecutor, final org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext context, org.neo4j.cluster.statemachine.StateMachine[] machines)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual ProtocolServer ConstructSupportingInfrastructureFor( InstanceId me, MessageSource input, MessageSender output, DelayedDirectExecutor executor, Timeouts timeouts, Executor stateMachineExecutor, MultiPaxosContext context, StateMachine[] machines )
		 {
			  StateMachines stateMachines = new StateMachines( _logging, _stateMachinesMonitor, input, output, timeouts, executor, stateMachineExecutor, me );

			  foreach ( StateMachine machine in machines )
			  {
					stateMachines.AddStateMachine( machine );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ProtocolServer server = new ProtocolServer(me, stateMachines, logging);
			  ProtocolServer server = new ProtocolServer( me, stateMachines, _logging );

			  server.AddBindingListener( me1 => context.ClusterContext.setBoundAt( me1 ) );

			  stateMachines.AddMessageProcessor( new HeartbeatRefreshProcessor( stateMachines.Outgoing, context.ClusterContext ) );
			  input.AddMessageProcessor( new HeartbeatIAmAliveProcessor( stateMachines.Outgoing, context.ClusterContext ) );

			  Cluster cluster = server.NewClient( typeof( Cluster ) );
			  cluster.AddClusterListener( new HeartbeatJoinListener( stateMachines.Outgoing ) );
			  cluster.AddClusterListener( new HeartbeatLeftListener( context.HeartbeatContext, _logging ) );

			  context.HeartbeatContext.addHeartbeatListener( new HeartbeatReelectionListener( server.NewClient( typeof( Election ) ), _logging ) );
			  context.ClusterContext.addClusterListener( new ClusterLeaveReelectionListener( server.NewClient( typeof( Election ) ), _logging ) );

			  StateMachineRules rules = ( new StateMachineRules( stateMachines.Outgoing ) ).rule( ClusterState.start, ClusterMessage.create, ClusterState.entered, @internal( AtomicBroadcastMessage.entered ), @internal( ProposerMessage.join ), @internal( AcceptorMessage.join ), @internal( LearnerMessage.join ), @internal( HeartbeatMessage.join ), @internal( ElectionMessage.created ), @internal( SnapshotMessage.join ) ).rule( ClusterState.discovery, ClusterMessage.configurationResponse, ClusterState.joining, @internal( AcceptorMessage.join ), @internal( LearnerMessage.join ), @internal( AtomicBroadcastMessage.join ) ).rule( ClusterState.discovery, ClusterMessage.configurationResponse, ClusterState.entered, @internal( AtomicBroadcastMessage.entered ), @internal( ProposerMessage.join ), @internal( AcceptorMessage.join ), @internal( LearnerMessage.join ), @internal( HeartbeatMessage.join ), @internal( ElectionMessage.join ), @internal( SnapshotMessage.join ) ).rule( ClusterState.joining, ClusterMessage.configurationChanged, ClusterState.entered, @internal( AtomicBroadcastMessage.entered ), @internal( ProposerMessage.join ), @internal( AcceptorMessage.join ), @internal( LearnerMessage.join ), @internal( HeartbeatMessage.join ), @internal( ElectionMessage.join ), @internal( SnapshotMessage.join ) ).rule( ClusterState.joining, ClusterMessage.joinFailure, ClusterState.start, @internal( AtomicBroadcastMessage.leave ), @internal( AcceptorMessage.leave ), @internal( LearnerMessage.leave ), @internal( ProposerMessage.leave ) ).rule( ClusterState.entered, ClusterMessage.leave, ClusterState.start, @internal( AtomicBroadcastMessage.leave ), @internal( AcceptorMessage.leave ), @internal( LearnerMessage.leave ), @internal( HeartbeatMessage.leave ), @internal( SnapshotMessage.leave ), @internal( ElectionMessage.leave ), @internal( ProposerMessage.leave ) ).rule( ClusterState.entered, ClusterMessage.leave, ClusterState.start, @internal( AtomicBroadcastMessage.leave ), @internal( AcceptorMessage.leave ), @internal( LearnerMessage.leave ), @internal( HeartbeatMessage.leave ), @internal( ElectionMessage.leave ), @internal( SnapshotMessage.leave ), @internal( ProposerMessage.leave ) ).rule( ClusterState.leaving, ClusterMessage.configurationChanged, ClusterState.start, @internal( AtomicBroadcastMessage.leave ), @internal( AcceptorMessage.leave ), @internal( LearnerMessage.leave ), @internal( HeartbeatMessage.leave ), @internal( ElectionMessage.leave ), @internal( SnapshotMessage.leave ), @internal( ProposerMessage.leave ) ).rule( ClusterState.leaving, ClusterMessage.leaveTimedout, ClusterState.start, @internal( AtomicBroadcastMessage.leave ), @internal( AcceptorMessage.leave ), @internal( LearnerMessage.leave ), @internal( HeartbeatMessage.leave ), @internal( ElectionMessage.leave ), @internal( SnapshotMessage.leave ), @internal( ProposerMessage.leave ) );

			  stateMachines.AddStateTransitionListener( rules );

			  return server;
		 }
	}

}