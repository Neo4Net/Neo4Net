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
namespace Neo4Net.cluster.client
{
	using InternalLoggerFactory = org.jboss.netty.logging.InternalLoggerFactory;


	using NetworkReceiver = Neo4Net.cluster.com.NetworkReceiver;
	using NetworkSender = Neo4Net.cluster.com.NetworkSender;
	using AsyncLogging = Neo4Net.cluster.logging.AsyncLogging;
	using NettyLoggerFactory = Neo4Net.cluster.logging.NettyLoggerFactory;
	using AtomicBroadcastSerializer = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectInputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using AcceptorInstanceStore = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using AtomicBroadcastMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.AtomicBroadcastMessage;
	using InMemoryAcceptorInstanceStore = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InMemoryAcceptorInstanceStore;
	using LearnerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.LearnerMessage;
	using ProposerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using ElectionCredentialsProvider = Neo4Net.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionMessage = Neo4Net.cluster.protocol.election.ElectionMessage;
	using HeartbeatMessage = Neo4Net.cluster.protocol.heartbeat.HeartbeatMessage;
	using StateTransitionLogger = Neo4Net.cluster.statemachine.StateTransitionLogger;
	using FixedTimeoutStrategy = Neo4Net.cluster.timeout.FixedTimeoutStrategy;
	using MessageTimeoutStrategy = Neo4Net.cluster.timeout.MessageTimeoutStrategy;
	using TimeoutStrategy = Neo4Net.cluster.timeout.TimeoutStrategy;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.NamedThreadFactory.daemon;

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
//ORIGINAL LINE: public ClusterClientModule(Neo4Net.kernel.lifecycle.LifeSupport life, Neo4Net.kernel.impl.util.Dependencies dependencies, final Neo4Net.kernel.monitoring.Monitors monitors, final Neo4Net.kernel.configuration.Config config, Neo4Net.logging.internal.LogService logService, Neo4Net.cluster.protocol.election.ElectionCredentialsProvider electionCredentialsProvider)
		 public ClusterClientModule( LifeSupport life, Dependencies dependencies, Monitors monitors, Config config, LogService logService, ElectionCredentialsProvider electionCredentialsProvider )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.LogProvider logging = Neo4Net.cluster.logging.AsyncLogging.provider(life, logService.getInternalLogProvider());
			  LogProvider logging = AsyncLogging.provider( life, logService.InternalLogProvider );
			  InternalLoggerFactory.DefaultFactory = new NettyLoggerFactory( logging );

			  TimeoutStrategy timeoutStrategy = ( new MessageTimeoutStrategy( new FixedTimeoutStrategy( config.Get( ClusterSettings.default_timeout ).toMillis() ) ) ).timeout(HeartbeatMessage.sendHeartbeat, config.Get(ClusterSettings.heartbeat_interval).toMillis()).timeout(HeartbeatMessage.timed_out, config.Get(ClusterSettings.heartbeat_timeout).toMillis()).timeout(AtomicBroadcastMessage.broadcastTimeout, config.Get(ClusterSettings.broadcast_timeout).toMillis()).timeout(LearnerMessage.learnTimedout, config.Get(ClusterSettings.learn_timeout).toMillis()).timeout(ProposerMessage.phase1Timeout, config.Get(ClusterSettings.phase1_timeout).toMillis()).timeout(ProposerMessage.phase2Timeout, config.Get(ClusterSettings.phase2_timeout).toMillis()).timeout(ClusterMessage.joiningTimeout, config.Get(ClusterSettings.join_timeout).toMillis()).timeout(ClusterMessage.configurationTimeout, config.Get(ClusterSettings.configuration_timeout).toMillis()).timeout(ClusterMessage.leaveTimedout, config.Get(ClusterSettings.leave_timeout).toMillis()).timeout(ElectionMessage.electionTimeout, config.Get(ClusterSettings.election_timeout).toMillis());

			  MultiPaxosServerFactory protocolServerFactory = new MultiPaxosServerFactory( new ClusterConfiguration( config.Get( ClusterSettings.cluster_name ), logging ), logging, monitors.NewMonitor( typeof( StateMachines.Monitor ) ) );

			  NetworkReceiver receiver = dependencies.satisfyDependency(new NetworkReceiver(monitors.NewMonitor(typeof(NetworkReceiver.Monitor)), new ConfigurationAnonymousInnerClass(this, config)
			 , logging));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory objectInputStreamFactory = new Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory();
			  ObjectInputStreamFactory objectInputStreamFactory = new ObjectStreamFactory();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory objectOutputStreamFactory = new Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory();
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