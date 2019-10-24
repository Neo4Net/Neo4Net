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
namespace Neo4Net.cluster
{

	using MessageSender = Neo4Net.cluster.com.message.MessageSender;
	using MessageSource = Neo4Net.cluster.com.message.MessageSource;
	using ObjectInputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using AcceptorInstanceStore = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using AcceptorMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AcceptorMessage;
	using AcceptorState = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AcceptorState;
	using AtomicBroadcastMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using AtomicBroadcastState = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastState;
	using LearnerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage;
	using LearnerState = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.LearnerState;
	using ProposerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;
	using ProposerState = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerState;
	using MultiPaxosContext = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using Cluster = Neo4Net.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using ClusterState = Neo4Net.cluster.protocol.cluster.ClusterState;
	using ClusterLeaveReelectionListener = Neo4Net.cluster.protocol.election.ClusterLeaveReelectionListener;
	using Election = Neo4Net.cluster.protocol.election.Election;
	using ElectionCredentialsProvider = Neo4Net.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionMessage = Neo4Net.cluster.protocol.election.ElectionMessage;
	using ElectionRole = Neo4Net.cluster.protocol.election.ElectionRole;
	using ElectionState = Neo4Net.cluster.protocol.election.ElectionState;
	using HeartbeatReelectionListener = Neo4Net.cluster.protocol.election.HeartbeatReelectionListener;
	using HeartbeatIAmAliveProcessor = Neo4Net.cluster.protocol.heartbeat.HeartbeatIAmAliveProcessor;
	using HeartbeatJoinListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatJoinListener;
	using HeartbeatLeftListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatLeftListener;
	using HeartbeatMessage = Neo4Net.cluster.protocol.heartbeat.HeartbeatMessage;
	using HeartbeatRefreshProcessor = Neo4Net.cluster.protocol.heartbeat.HeartbeatRefreshProcessor;
	using HeartbeatState = Neo4Net.cluster.protocol.heartbeat.HeartbeatState;
	using SnapshotContext = Neo4Net.cluster.protocol.snapshot.SnapshotContext;
	using SnapshotMessage = Neo4Net.cluster.protocol.snapshot.SnapshotMessage;
	using SnapshotState = Neo4Net.cluster.protocol.snapshot.SnapshotState;
	using Neo4Net.cluster.statemachine;
	using StateMachineRules = Neo4Net.cluster.statemachine.StateMachineRules;
	using TimeoutStrategy = Neo4Net.cluster.timeout.TimeoutStrategy;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.com.message.Message.Internal;

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
//ORIGINAL LINE: final org.Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext context = new org.Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext(me, org.Neo4Net.helpers.collection.Iterables.iterable(new org.Neo4Net.cluster.protocol.election.ElectionRole(org.Neo4Net.cluster.protocol.cluster.ClusterConfiguration.COORDINATOR)), new org.Neo4Net.cluster.protocol.cluster.ClusterConfiguration(initialConfig.getName(), logging, initialConfig.getMemberURIs()), executor, logging, objectInputStreamFactory, objectOutputStreamFactory, acceptorInstanceStore, timeouts, electionCredentialsProvider, config);
			  MultiPaxosContext context = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( ClusterConfiguration.COORDINATOR ) ), new ClusterConfiguration( _initialConfig.Name, _logging, _initialConfig.MemberURIs ), executor, _logging, objectInputStreamFactory, objectOutputStreamFactory, acceptorInstanceStore, timeouts, electionCredentialsProvider, config );

			  SnapshotContext snapshotContext = new SnapshotContext( context.ClusterContext, context.LearnerContext );

			  return NewProtocolServer( me, input, output, stateMachineExecutor, executor, timeouts, context, snapshotContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public ProtocolServer newProtocolServer(InstanceId me, org.Neo4Net.cluster.com.message.MessageSource input, org.Neo4Net.cluster.com.message.MessageSender output, java.util.concurrent.Executor stateMachineExecutor, DelayedDirectExecutor executor, org.Neo4Net.cluster.timeout.Timeouts timeouts, org.Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext context, org.Neo4Net.cluster.protocol.snapshot.SnapshotContext snapshotContext)
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
//ORIGINAL LINE: @SuppressWarnings("rawtypes") public ProtocolServer constructSupportingInfrastructureFor(InstanceId me, org.Neo4Net.cluster.com.message.MessageSource input, org.Neo4Net.cluster.com.message.MessageSender output, DelayedDirectExecutor executor, org.Neo4Net.cluster.timeout.Timeouts timeouts, java.util.concurrent.Executor stateMachineExecutor, final org.Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext context, org.Neo4Net.cluster.statemachine.StateMachine[] machines)
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