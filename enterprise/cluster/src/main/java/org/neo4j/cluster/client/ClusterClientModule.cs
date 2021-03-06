﻿using System.Collections.Generic;

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
namespace Org.Neo4j.cluster.client
{
	using InternalLoggerFactory = org.jboss.netty.logging.InternalLoggerFactory;


	using NetworkReceiver = Org.Neo4j.cluster.com.NetworkReceiver;
	using NetworkSender = Org.Neo4j.cluster.com.NetworkSender;
	using AsyncLogging = Org.Neo4j.cluster.logging.AsyncLogging;
	using NettyLoggerFactory = Org.Neo4j.cluster.logging.NettyLoggerFactory;
	using AtomicBroadcastSerializer = Org.Neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectInputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using ObjectStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using AcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using AtomicBroadcastMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using InMemoryAcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.InMemoryAcceptorInstanceStore;
	using LearnerMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage;
	using ProposerMessage = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterMessage = Org.Neo4j.cluster.protocol.cluster.ClusterMessage;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionMessage = Org.Neo4j.cluster.protocol.election.ElectionMessage;
	using HeartbeatMessage = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatMessage;
	using StateTransitionLogger = Org.Neo4j.cluster.statemachine.StateTransitionLogger;
	using FixedTimeoutStrategy = Org.Neo4j.cluster.timeout.FixedTimeoutStrategy;
	using MessageTimeoutStrategy = Org.Neo4j.cluster.timeout.MessageTimeoutStrategy;
	using TimeoutStrategy = Org.Neo4j.cluster.timeout.TimeoutStrategy;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using NamedThreadFactory = Org.Neo4j.Helpers.NamedThreadFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.NamedThreadFactory.daemon;

	/// <summary>
	/// This is a builder for <seealso cref="ClusterClient"/> instances.
	/// <p/>
	/// While a <seealso cref="Dependencies"/> instance is passed into the constructor there are no services other than those
	/// explicitly passed in that are required, and instead it is only used to register any created services for others to
	/// use.
	/// </summary>
	public class ClusterClientModule
	{
		 public readonly ClusterClient ClusterClient;
		 private readonly ProtocolServer _server;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ClusterClientModule(org.neo4j.kernel.lifecycle.LifeSupport life, org.neo4j.kernel.impl.util.Dependencies dependencies, final org.neo4j.kernel.monitoring.Monitors monitors, final org.neo4j.kernel.configuration.Config config, org.neo4j.logging.internal.LogService logService, org.neo4j.cluster.protocol.election.ElectionCredentialsProvider electionCredentialsProvider)
		 public ClusterClientModule( LifeSupport life, Dependencies dependencies, Monitors monitors, Config config, LogService logService, ElectionCredentialsProvider electionCredentialsProvider )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.LogProvider logging = org.neo4j.cluster.logging.AsyncLogging.provider(life, logService.getInternalLogProvider());
			  LogProvider logging = AsyncLogging.provider( life, logService.InternalLogProvider );
			  InternalLoggerFactory.DefaultFactory = new NettyLoggerFactory( logging );

			  TimeoutStrategy timeoutStrategy = ( new MessageTimeoutStrategy( new FixedTimeoutStrategy( config.Get( ClusterSettings.default_timeout ).toMillis() ) ) ).timeout(HeartbeatMessage.sendHeartbeat, config.Get(ClusterSettings.heartbeat_interval).toMillis()).timeout(HeartbeatMessage.timed_out, config.Get(ClusterSettings.heartbeat_timeout).toMillis()).timeout(AtomicBroadcastMessage.broadcastTimeout, config.Get(ClusterSettings.broadcast_timeout).toMillis()).timeout(LearnerMessage.learnTimedout, config.Get(ClusterSettings.learn_timeout).toMillis()).timeout(ProposerMessage.phase1Timeout, config.Get(ClusterSettings.phase1_timeout).toMillis()).timeout(ProposerMessage.phase2Timeout, config.Get(ClusterSettings.phase2_timeout).toMillis()).timeout(ClusterMessage.joiningTimeout, config.Get(ClusterSettings.join_timeout).toMillis()).timeout(ClusterMessage.configurationTimeout, config.Get(ClusterSettings.configuration_timeout).toMillis()).timeout(ClusterMessage.leaveTimedout, config.Get(ClusterSettings.leave_timeout).toMillis()).timeout(ElectionMessage.electionTimeout, config.Get(ClusterSettings.election_timeout).toMillis());

			  MultiPaxosServerFactory protocolServerFactory = new MultiPaxosServerFactory( new ClusterConfiguration( config.Get( ClusterSettings.cluster_name ), logging ), logging, monitors.NewMonitor( typeof( StateMachines.Monitor ) ) );

			  NetworkReceiver receiver = dependencies.satisfyDependency(new NetworkReceiver(monitors.NewMonitor(typeof(NetworkReceiver.Monitor)), new ConfigurationAnonymousInnerClass(this, config)
			 , logging));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory objectInputStreamFactory = new org.neo4j.cluster.protocol.atomicbroadcast.ObjectStreamFactory();
			  ObjectInputStreamFactory objectInputStreamFactory = new ObjectStreamFactory();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory objectOutputStreamFactory = new org.neo4j.cluster.protocol.atomicbroadcast.ObjectStreamFactory();
			  ObjectOutputStreamFactory objectOutputStreamFactory = new ObjectStreamFactory();

			  receiver.AddNetworkChannelsListener( new NetworkChannelsListenerAnonymousInnerClass( this, logging, objectInputStreamFactory, objectOutputStreamFactory ) );

			  NetworkSender sender = dependencies.satisfyDependency(new NetworkSender(monitors.NewMonitor(typeof(NetworkSender.Monitor)), new ConfigurationAnonymousInnerClass(this, config)
			 , receiver, logging));

			  ExecutorLifecycleAdapter stateMachineExecutor = new ExecutorLifecycleAdapter( () => Executors.newSingleThreadExecutor(new NamedThreadFactory("State machine", monitors.NewMonitor(typeof(NamedThreadFactory.Monitor)))) );

			  AcceptorInstanceStore acceptorInstanceStore = new InMemoryAcceptorInstanceStore();

			  _server = protocolServerFactory.NewProtocolServer( config.Get( ClusterSettings.server_id ),timeoutStrategy, receiver, sender, acceptorInstanceStore, electionCredentialsProvider, stateMachineExecutor, objectInputStreamFactory, objectOutputStreamFactory, config );

			  life.Add( sender );
			  life.Add( stateMachineExecutor );
			  life.Add( receiver );

			  // Timeout timer - triggers every 10 ms
			  life.Add( new TimeoutTrigger( _server, monitors ) );

			  life.add(new ClusterJoin(new ConfigurationAnonymousInnerClass(this, config)
			 , _server, logService));

			  ClusterClient = dependencies.SatisfyDependency( new ClusterClient( life, _server ) );
		 }

		 private class ConfigurationAnonymousInnerClass : NetworkReceiver.Configuration
		 {
			 private readonly ClusterClientModule _outerInstance;

			 private Config _config;

			 public ConfigurationAnonymousInnerClass( ClusterClientModule outerInstance, Config config )
			 {
				 this.outerInstance = outerInstance;
				 this._config = config;
			 }

			 public HostnamePort clusterServer()
			 {
				  return _config.get( ClusterSettings.cluster_server );
			 }

			 public int defaultPort()
			 {
				  return 5001;
			 }

			 public string name()
			 {
				  return _config.get( ClusterSettings.instance_name );
			 }
		 }

		 private class NetworkChannelsListenerAnonymousInnerClass : NetworkReceiver.NetworkChannelsListener
		 {
			 private readonly ClusterClientModule _outerInstance;

			 private LogProvider _logging;
			 private ObjectInputStreamFactory _objectInputStreamFactory;
			 private ObjectOutputStreamFactory _objectOutputStreamFactory;

			 public NetworkChannelsListenerAnonymousInnerClass( ClusterClientModule outerInstance, LogProvider logging, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory )
			 {
				 this.outerInstance = outerInstance;
				 this._logging = logging;
				 this._objectInputStreamFactory = objectInputStreamFactory;
				 this._objectOutputStreamFactory = objectOutputStreamFactory;
			 }

			 private volatile StateTransitionLogger logger;

			 public void listeningAt( URI me )
			 {
				  _outerInstance.server.listeningAt( me );
				  if ( logger == null )
				  {
						logger = new StateTransitionLogger( _logging, new AtomicBroadcastSerializer( _objectInputStreamFactory, _objectOutputStreamFactory ) );
						_outerInstance.server.addStateTransitionListener( logger );
				  }
			 }

			 public void channelOpened( URI to )
			 {
				  _logging.getLog( typeof( NetworkReceiver ) ).info( to + " connected to me at " + _outerInstance.server.boundAt() );
			 }

			 public void channelClosed( URI to )
			 {
				  _logging.getLog( typeof( NetworkReceiver ) ).info( to + " disconnected from me at " + _outerInstance.server.boundAt() );
			 }
		 }

		 private class ConfigurationAnonymousInnerClass : NetworkSender.Configuration
		 {
			 private readonly ClusterClientModule _outerInstance;

			 private Config _config;

			 public ConfigurationAnonymousInnerClass( ClusterClientModule outerInstance, Config config )
			 {
				 this.outerInstance = outerInstance;
				 this._config = config;
			 }

			 public int defaultPort()
			 {
				  return 5001;
			 }

			 public int port()
			 {
				  return _config.get( ClusterSettings.cluster_server ).Port;
			 }
		 }

		 private class ConfigurationAnonymousInnerClass : ClusterJoin.Configuration
		 {
			 private readonly ClusterClientModule _outerInstance;

			 private Config _config;

			 public ConfigurationAnonymousInnerClass( ClusterClientModule outerInstance, Config config )
			 {
				 this.outerInstance = outerInstance;
				 this._config = config;
			 }

			 public IList<HostnamePort> InitialHosts
			 {
				 get
				 {
					  return _config.get( ClusterSettings.initial_hosts );
				 }
			 }

			 public string ClusterName
			 {
				 get
				 {
					  return _config.get( ClusterSettings.cluster_name );
				 }
			 }

			 public bool AllowedToCreateCluster
			 {
				 get
				 {
					  return _config.get( ClusterSettings.allow_init_cluster );
				 }
			 }

			 public long ClusterJoinTimeout
			 {
				 get
				 {
					  return _config.get( ClusterSettings.join_timeout ).toMillis();
				 }
			 }
		 }

		 private class TimeoutTrigger : Lifecycle
		 {
			  internal readonly ProtocolServer Server;
			  internal readonly Monitors Monitors;

			  internal ScheduledExecutorService Scheduler;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.concurrent.ScheduledFuture<?> tickFuture;
			  internal ScheduledFuture<object> TickFuture;

			  internal TimeoutTrigger( ProtocolServer server, Monitors monitors )
			  {
					this.Server = server;
					this.Monitors = monitors;
			  }

			  public override void Init()
			  {
					Server.Timeouts.tick( DateTimeHelper.CurrentUnixTimeMillis() );
			  }

			  public override void Start()
			  {
					Scheduler = Executors.newSingleThreadScheduledExecutor( daemon( "timeout-clusterClient", Monitors.newMonitor( typeof( NamedThreadFactory.Monitor ) ) ) );

					TickFuture = Scheduler.scheduleWithFixedDelay(() =>
					{
					 long now = DateTimeHelper.CurrentUnixTimeMillis();

					 Server.Timeouts.tick( now );
					}, 0, 10, TimeUnit.MILLISECONDS);
			  }

			  public override void Stop()
			  {
					TickFuture.cancel( true );
					Scheduler.shutdownNow();
			  }

			  public override void Shutdown()
			  {
			  }
		 }
	}

}