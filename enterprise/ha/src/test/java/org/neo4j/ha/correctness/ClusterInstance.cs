﻿using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.ha.correctness
{

	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using DelayedDirectExecutor = Org.Neo4j.cluster.DelayedDirectExecutor;
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using MultiPaxosServerFactory = Org.Neo4j.cluster.MultiPaxosServerFactory;
	using ProtocolServer = Org.Neo4j.cluster.ProtocolServer;
	using StateMachines = Org.Neo4j.cluster.StateMachines;
	using Org.Neo4j.cluster.com.message;
	using MessageProcessor = Org.Neo4j.cluster.com.message.MessageProcessor;
	using MessageSender = Org.Neo4j.cluster.com.message.MessageSender;
	using MessageSource = Org.Neo4j.cluster.com.message.MessageSource;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;
	using ObjectStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using AcceptorMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorMessage;
	using AtomicBroadcastMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using InMemoryAcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InMemoryAcceptorInstanceStore;
	using LearnerMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage;
	using ProposerMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;
	using MultiPaxosContext = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterMessage = Org.Neo4j.cluster.protocol.cluster.ClusterMessage;
	using ElectionMessage = Org.Neo4j.cluster.protocol.election.ElectionMessage;
	using ElectionRole = Org.Neo4j.cluster.protocol.election.ElectionRole;
	using HeartbeatMessage = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatMessage;
	using SnapshotContext = Org.Neo4j.cluster.protocol.snapshot.SnapshotContext;
	using SnapshotMessage = Org.Neo4j.cluster.protocol.snapshot.SnapshotMessage;
	using Org.Neo4j.cluster.statemachine;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using HighAvailabilityMemberInfoProvider = Org.Neo4j.Kernel.ha.HighAvailabilityMemberInfoProvider;
	using DefaultElectionCredentialsProvider = Org.Neo4j.Kernel.ha.cluster.DefaultElectionCredentialsProvider;
	using HighAvailabilityMemberState = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberState;
	using LastTxIdGetter = Org.Neo4j.Kernel.impl.core.LastTxIdGetter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

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
//ORIGINAL LINE: final org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext context = new org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext(id, org.neo4j.helpers.collection.Iterables.iterable(new org.neo4j.cluster.protocol.election.ElectionRole(org.neo4j.cluster.protocol.cluster.ClusterConfiguration.COORDINATOR)), new org.neo4j.cluster.protocol.cluster.ClusterConfiguration(configuration.getName(), logging, configuration.getMemberURIs()), executor, logging, objStreamFactory, objStreamFactory, acceptorInstances, timeouts, new org.neo4j.kernel.ha.cluster.DefaultElectionCredentialsProvider(id, new StateVerifierLastTxIdGetter(), new MemberInfoProvider()), config);
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
//ORIGINAL LINE: public Iterable<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>> process(org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> message)
		 public virtual IEnumerable<Message<MessageType>> Process<T1>( Message<T1> message ) where T1 : Org.Neo4j.cluster.com.message.MessageType
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

			  public override bool Process<T1>( Message<T1> message ) where T1 : Org.Neo4j.cluster.com.message.MessageType
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
//ORIGINAL LINE: private final java.util.List<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>> messages = new java.util.ArrayList<>();
			  internal readonly IList<Message<MessageType>> MessagesConflict = new List<Message<MessageType>>();
			  internal readonly URI Uri;

			  internal ClusterInstanceOutput( URI uri )
			  {
					this.Uri = uri;
			  }

			  public override bool Process<T1>( Message<T1> message ) where T1 : Org.Neo4j.cluster.com.message.MessageType
			  {
					MessagesConflict.Add( message.SetHeader( Message.HEADER_FROM, Uri.toASCIIString() ) );
					return true;
			  }

			  public override void Process<T1>( IList<T1> msgList ) where T1 : Org.Neo4j.cluster.com.message.MessageType
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType> msg : msgList)
					foreach ( Message<MessageType> msg in msgList )
					{
						 Process( msg );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<org.neo4j.cluster.com.message.Message<? extends org.neo4j.cluster.com.message.MessageType>> messages()
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