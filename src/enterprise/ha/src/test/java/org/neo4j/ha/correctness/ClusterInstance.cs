using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.ha.correctness
{

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using DelayedDirectExecutor = Neo4Net.cluster.DelayedDirectExecutor;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using MultiPaxosServerFactory = Neo4Net.cluster.MultiPaxosServerFactory;
	using ProtocolServer = Neo4Net.cluster.ProtocolServer;
	using StateMachines = Neo4Net.cluster.StateMachines;
	using Neo4Net.cluster.com.message;
	using MessageProcessor = Neo4Net.cluster.com.message.MessageProcessor;
	using MessageSender = Neo4Net.cluster.com.message.MessageSender;
	using MessageSource = Neo4Net.cluster.com.message.MessageSource;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using AcceptorMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AcceptorMessage;
	using AtomicBroadcastMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using InMemoryAcceptorInstanceStore = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InMemoryAcceptorInstanceStore;
	using LearnerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage;
	using ProposerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;
	using MultiPaxosContext = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using ElectionMessage = Neo4Net.cluster.protocol.election.ElectionMessage;
	using ElectionRole = Neo4Net.cluster.protocol.election.ElectionRole;
	using HeartbeatMessage = Neo4Net.cluster.protocol.heartbeat.HeartbeatMessage;
	using SnapshotContext = Neo4Net.cluster.protocol.snapshot.SnapshotContext;
	using SnapshotMessage = Neo4Net.cluster.protocol.snapshot.SnapshotMessage;
	using Neo4Net.cluster.statemachine;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HighAvailabilityMemberInfoProvider = Neo4Net.Kernel.ha.HighAvailabilityMemberInfoProvider;
	using DefaultElectionCredentialsProvider = Neo4Net.Kernel.ha.cluster.DefaultElectionCredentialsProvider;
	using HighAvailabilityMemberState = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberState;
	using LastTxIdGetter = Neo4Net.Kernel.impl.core.LastTxIdGetter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class ClusterInstance
	{
		 private readonly Executor _stateMachineExecutor;
		 private readonly LogProvider _logging;
		 private readonly MultiPaxosServerFactory _factory;
		 private readonly ProtocolServer _server;
		 private readonly MultiPaxosContext _ctx;
		 private readonly InMemoryAcceptorInstanceStore _acceptorInstanceStore;
		 private readonly ProverTimeouts _timeouts;
		 private readonly ClusterInstanceInput _input;
		 private readonly ClusterInstanceOutput _output;
		 private readonly URI _uri;

		 public static readonly Executor DirectExecutor = ThreadStart.run;

		 private bool _online = true;

		 public static ClusterInstance NewClusterInstance( InstanceId id, URI uri, Monitors monitors, ClusterConfiguration configuration, int maxSurvivableFailedMembers, LogProvider logging )
		 {
			  MultiPaxosServerFactory factory = new MultiPaxosServerFactory( configuration, logging, monitors.NewMonitor( typeof( StateMachines.Monitor ) ) );

			  ClusterInstanceInput input = new ClusterInstanceInput();
			  ClusterInstanceOutput output = new ClusterInstanceOutput( uri );

			  ObjectStreamFactory objStreamFactory = new ObjectStreamFactory();

			  ProverTimeouts timeouts = new ProverTimeouts( uri );

			  InMemoryAcceptorInstanceStore acceptorInstances = new InMemoryAcceptorInstanceStore();

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( maxSurvivableFailedMembers );

			  DelayedDirectExecutor executor = new DelayedDirectExecutor( logging );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext context = new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext(id, Neo4Net.helpers.collection.Iterables.iterable(new Neo4Net.cluster.protocol.election.ElectionRole(Neo4Net.cluster.protocol.cluster.ClusterConfiguration.COORDINATOR)), new Neo4Net.cluster.protocol.cluster.ClusterConfiguration(configuration.getName(), logging, configuration.getMemberURIs()), executor, logging, objStreamFactory, objStreamFactory, acceptorInstances, timeouts, new Neo4Net.kernel.ha.cluster.DefaultElectionCredentialsProvider(id, new StateVerifierLastTxIdGetter(), new MemberInfoProvider()), config);
			  MultiPaxosContext context = new MultiPaxosContext( id, Iterables.iterable( new ElectionRole( ClusterConfiguration.COORDINATOR ) ), new ClusterConfiguration( configuration.Name, logging, configuration.MemberURIs ), executor, logging, objStreamFactory, objStreamFactory, acceptorInstances, timeouts, new DefaultElectionCredentialsProvider( id, new StateVerifierLastTxIdGetter(), new MemberInfoProvider() ), config );
			  context.ClusterContext.BoundAt = uri;

			  SnapshotContext snapshotContext = new SnapshotContext( context.ClusterContext, context.LearnerContext );

			  DelayedDirectExecutor taskExecutor = new DelayedDirectExecutor( logging );
			  ProtocolServer ps = factory.NewProtocolServer( id, input, output, DirectExecutor, taskExecutor, timeouts, context, snapshotContext );

			  return new ClusterInstance( DirectExecutor, logging, factory, ps, context, acceptorInstances, timeouts, input, output, uri );
		 }

		 internal ClusterInstance( Executor stateMachineExecutor, LogProvider logging, MultiPaxosServerFactory factory, ProtocolServer server, MultiPaxosContext ctx, InMemoryAcceptorInstanceStore acceptorInstanceStore, ProverTimeouts timeouts, ClusterInstanceInput input, ClusterInstanceOutput output, URI uri )
		 {
			  this._stateMachineExecutor = stateMachineExecutor;
			  this._logging = logging;
			  this._factory = factory;
			  this._server = server;
			  this._ctx = ctx;
			  this._acceptorInstanceStore = acceptorInstanceStore;
			  this._timeouts = timeouts;
			  this._input = input;
			  this._output = output;
			  this._uri = uri;
		 }

		 public virtual InstanceId Id()
		 {
			  return _server.ServerId;
		 }

		 /// <summary>
		 /// Process a message, returns all messages generated as output.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<Neo4Net.cluster.com.message.Message<? extends Neo4Net.cluster.com.message.MessageType>> process(Neo4Net.cluster.com.message.Message<? extends Neo4Net.cluster.com.message.MessageType> message)
		 public virtual IEnumerable<Message<MessageType>> Process<T1>( Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
		 {
			  if ( _online )
			  {
					_input.process( message );
					return _output.messages();
			  }
			  else
			  {
					return Iterables.empty();
			  }
		 }

		 public override string ToString()
		 {
			  return "[" + Id() + ":" + Iterables.ToString(StateMachineStates(), ",") + "]";
		 }

		 private IEnumerable<string> StateMachineStates()
		 {
			  return Iterables.map( stateMachine => stateMachine.State.ToString(), _server.StateMachines.StateMachines );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ClusterInstance that = ( ClusterInstance ) o;

			  if ( !ToString().Equals(that.ToString()) )
			  {
					return false;
			  }

			  if ( !_uri.Equals( that._uri ) )
			  {
					return false;
			  }

			  // TODO: For now, we only look at the states of the underlying state machines,
			  // and ignore, at our peril, the MultiPaxosContext as part of this equality checks.
			  // This means the prover ignores lots of possible paths it could generate, as it considers two
			  // machines with different multi paxos state potentially equal and will ignore exploring both.
			  // This should be undone as soon as possible. It's here because we need a better mechanism than
			  // .equals() to compare that two contexts are the same, which is not yet implemented.

			  return true;
		 }

		 public override int GetHashCode()
		 {
			  return ToString().GetHashCode();
		 }

		 private StateMachine SnapshotStateMachine( LogProvider logProvider, MultiPaxosContext snapshotCtx, StateMachine stateMachine )
		 {
			  // This is done this way because all the state machines are sharing one piece of global state
			  // (MultiPaxosContext), which is snapshotted as one coherent component. This means the state machines
			  // cannot snapshot themselves, an external service needs to snapshot the full shared state and then create
			  // new state machines sharing that state.

			  object ctx;
			  Type msgType = stateMachine.MessageType;
			  if ( msgType == typeof( AtomicBroadcastMessage ) )
			  {
					ctx = snapshotCtx.AtomicBroadcastContext;
			  }
			  else if ( msgType == typeof( AcceptorMessage ) )
			  {
					ctx = snapshotCtx.AcceptorContext;
			  }
			  else if ( msgType == typeof( ProposerMessage ) )
			  {
					ctx = snapshotCtx.ProposerContext;
			  }
			  else if ( msgType == typeof( LearnerMessage ) )
			  {
					ctx = snapshotCtx.LearnerContext;
			  }
			  else if ( msgType == typeof( HeartbeatMessage ) )
			  {
					ctx = snapshotCtx.HeartbeatContext;
			  }
			  else if ( msgType == typeof( ElectionMessage ) )
			  {
					ctx = snapshotCtx.ElectionContext;
			  }
			  else if ( msgType == typeof( SnapshotMessage ) )
			  {
					ctx = new SnapshotContext( snapshotCtx.ClusterContext, snapshotCtx.LearnerContext );
			  }
			  else if ( msgType == typeof( ClusterMessage ) )
			  {
					ctx = snapshotCtx.ClusterContext;
			  }
			  else
			  {
					throw new System.ArgumentException( "I don't know how to snapshot this state machine: " + stateMachine );
			  }
			  return new StateMachine( ctx, stateMachine.MessageType, stateMachine.State, logProvider );
		 }

		 public virtual ClusterInstance NewCopy()
		 {
			  // A very invasive method of cloning a protocol server. Nonetheless, since this is mostly an experiment at this
			  // point, it seems we can refactor later on to have a cleaner clone mechanism.
			  // Because state machines share state, and are simultaneously conceptually unaware of each other, implementing
			  // a clean snapshot mechanism is very hard. I've opted for having a dirty one here in the test code rather
			  // than introducing a hack into the runtime code.

			  ProverTimeouts timeoutsSnapshot = _timeouts.snapshot();
			  InMemoryAcceptorInstanceStore snapshotAcceptorInstances = _acceptorInstanceStore.snapshot();

			  ClusterInstanceOutput output = new ClusterInstanceOutput( _uri );
			  ClusterInstanceInput input = new ClusterInstanceInput();

			  DelayedDirectExecutor executor = new DelayedDirectExecutor( _logging );

			  ObjectStreamFactory objectStreamFactory = new ObjectStreamFactory();
			  MultiPaxosContext snapshotCtx = _ctx.snapshot(_logging, timeoutsSnapshot, executor, snapshotAcceptorInstances, objectStreamFactory, objectStreamFactory, new DefaultElectionCredentialsProvider(_server.ServerId, new StateVerifierLastTxIdGetter(), new MemberInfoProvider())
			 );

			  IList<StateMachine> snapshotMachines = new List<StateMachine>();
			  foreach ( StateMachine stateMachine in _server.StateMachines.StateMachines )
			  {
					snapshotMachines.Add( SnapshotStateMachine( _logging, snapshotCtx, stateMachine ) );
			  }

			  ProtocolServer snapshotProtocolServer = _factory.constructSupportingInfrastructureFor( _server.ServerId, input, output, executor, timeoutsSnapshot, _stateMachineExecutor, snapshotCtx, snapshotMachines.ToArray() );

			  return new ClusterInstance( _stateMachineExecutor, _logging, _factory, snapshotProtocolServer, snapshotCtx, snapshotAcceptorInstances, timeoutsSnapshot, input, output, _uri );
		 }

		 public virtual URI Uri()
		 {
			  return _uri;
		 }

		 public virtual bool HasPendingTimeouts()
		 {
			  return _timeouts.hasTimeouts();
		 }

		 public virtual ClusterAction PopTimeout()
		 {
			  return _timeouts.pop();
		 }

		 /// <summary>
		 /// Make this instance stop responding to calls, and cancel all pending timeouts.
		 /// </summary>
		 public virtual void Crash()
		 {
			  _timeouts.cancelAllTimeouts();
			  this._online = false;
		 }

		 private class ClusterInstanceInput : MessageSource, MessageProcessor
		 {
			  internal readonly IList<MessageProcessor> Processors = new List<MessageProcessor>();

			  public override bool Process<T1>( Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
			  {
					foreach ( MessageProcessor processor in Processors )
					{
						 if ( !processor.Process( message ) )
						 {
							  return false;
						 }
					}

					return true;
			  }

			  public override void AddMessageProcessor( MessageProcessor messageProcessor )
			  {
					Processors.Add( messageProcessor );
			  }
		 }

		 private class ClusterInstanceOutput : MessageSender
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<Neo4Net.cluster.com.message.Message<? extends Neo4Net.cluster.com.message.MessageType>> messages = new java.util.ArrayList<>();
			  internal readonly IList<Message<MessageType>> MessagesConflict = new List<Message<MessageType>>();
			  internal readonly URI Uri;

			  internal ClusterInstanceOutput( URI uri )
			  {
					this.Uri = uri;
			  }

			  public override bool Process<T1>( Message<T1> message ) where T1 : Neo4Net.cluster.com.message.MessageType
			  {
					MessagesConflict.Add( message.SetHeader( Message.HEADER_FROM, Uri.toASCIIString() ) );
					return true;
			  }

			  public override void Process<T1>( IList<T1> msgList ) where T1 : Neo4Net.cluster.com.message.MessageType
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Neo4Net.cluster.com.message.Message<? extends Neo4Net.cluster.com.message.MessageType> msg : msgList)
					foreach ( Message<MessageType> msg in msgList )
					{
						 Process( msg );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<Neo4Net.cluster.com.message.Message<? extends Neo4Net.cluster.com.message.MessageType>> messages()
			  public virtual IEnumerable<Message<MessageType>> Messages()
			  {
					return MessagesConflict;
			  }
		 }

		 internal class MemberInfoProvider : HighAvailabilityMemberInfoProvider
		 {
			  public virtual HighAvailabilityMemberState HighAvailabilityMemberState
			  {
				  get
				  {
						throw new System.NotSupportedException( "TODO" );
				  }
			  }
		 }

		 // TODO: Make this emulate commits happening
		 internal class StateVerifierLastTxIdGetter : LastTxIdGetter
		 {
			  public virtual long LastTxId
			  {
				  get
				  {
						return 0;
				  }
			  }
		 }
	}

}