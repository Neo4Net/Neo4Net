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

	using NetworkReceiver = Org.Neo4j.cluster.com.NetworkReceiver;
	using NetworkSender = Org.Neo4j.cluster.com.NetworkSender;
	using AtomicBroadcastSerializer = Org.Neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectInputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using AcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using StateTransitionLogger = Org.Neo4j.cluster.statemachine.StateTransitionLogger;
	using TimeoutStrategy = Org.Neo4j.cluster.timeout.TimeoutStrategy;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using NamedThreadFactory = Org.Neo4j.Helpers.NamedThreadFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// TODO
	/// </summary>
	public class NetworkedServerFactory
	{
		 private LifeSupport _life;
		 private ProtocolServerFactory _protocolServerFactory;
		 private TimeoutStrategy _timeoutStrategy;
		 private readonly NetworkReceiver.Monitor _networkReceiverMonitor;
		 private readonly NetworkSender.Monitor _networkSenderMonitor;
		 private LogProvider _logProvider;
		 private ObjectInputStreamFactory _objectInputStreamFactory;
		 private ObjectOutputStreamFactory _objectOutputStreamFactory;
		 private readonly NamedThreadFactory.Monitor _namedThreadFactoryMonitor;

		 public NetworkedServerFactory( LifeSupport life, ProtocolServerFactory protocolServerFactory, TimeoutStrategy timeoutStrategy, LogProvider logProvider, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory, NetworkReceiver.Monitor networkReceiverMonitor, NetworkSender.Monitor networkSenderMonitor, NamedThreadFactory.Monitor namedThreadFactoryMonitor )
		 {
			  this._life = life;
			  this._protocolServerFactory = protocolServerFactory;
			  this._timeoutStrategy = timeoutStrategy;
			  this._networkReceiverMonitor = networkReceiverMonitor;
			  this._networkSenderMonitor = networkSenderMonitor;
			  this._logProvider = logProvider;
			  this._objectInputStreamFactory = objectInputStreamFactory;
			  this._objectOutputStreamFactory = objectOutputStreamFactory;
			  this._namedThreadFactoryMonitor = namedThreadFactoryMonitor;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ProtocolServer newNetworkedServer(final org.neo4j.kernel.configuration.Config config, org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore acceptorInstanceStore, org.neo4j.cluster.protocol.election.ElectionCredentialsProvider electionCredentialsProvider)
		 public virtual ProtocolServer NewNetworkedServer( Config config, AcceptorInstanceStore acceptorInstanceStore, ElectionCredentialsProvider electionCredentialsProvider )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.com.NetworkReceiver receiver = new org.neo4j.cluster.com.NetworkReceiver(networkReceiverMonitor, new org.neo4j.cluster.com.NetworkReceiver.Configuration()
			  NetworkReceiver receiver = new NetworkReceiver(_networkReceiverMonitor, new ConfigurationAnonymousInnerClass(this, config)
			 , _logProvider);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.com.NetworkSender sender = new org.neo4j.cluster.com.NetworkSender(networkSenderMonitor, new org.neo4j.cluster.com.NetworkSender.Configuration()
			  NetworkSender sender = new NetworkSender(_networkSenderMonitor, new ConfigurationAnonymousInnerClass(this, config)
			 , receiver, _logProvider);

			  ExecutorLifecycleAdapter stateMachineExecutor = new ExecutorLifecycleAdapter( () => Executors.newSingleThreadExecutor(new NamedThreadFactory("State machine", _namedThreadFactoryMonitor)) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ProtocolServer protocolServer = protocolServerFactory.newProtocolServer(config.get(ClusterSettings.server_id), timeoutStrategy, receiver, sender, acceptorInstanceStore, electionCredentialsProvider, stateMachineExecutor, objectInputStreamFactory, objectOutputStreamFactory, config);
			  ProtocolServer protocolServer = _protocolServerFactory.newProtocolServer( config.Get( ClusterSettings.ServerId ), _timeoutStrategy, receiver, sender, acceptorInstanceStore, electionCredentialsProvider, stateMachineExecutor, _objectInputStreamFactory, _objectOutputStreamFactory, config );
			  receiver.AddNetworkChannelsListener( new NetworkChannelsListenerAnonymousInnerClass( this, protocolServer ) );

			  _life.add( stateMachineExecutor );

			  // Timeout timer - triggers every 10 ms
			  _life.add( new LifecycleAnonymousInnerClass( this, protocolServer ) );

			  // Add this last to ensure that timeout service is setup first
			  _life.add( sender );
			  _life.add( receiver );

			  return protocolServer;
		 }

		 private class ConfigurationAnonymousInnerClass : NetworkReceiver.Configuration
		 {
			 private readonly NetworkedServerFactory _outerInstance;

			 private Config _config;

			 public ConfigurationAnonymousInnerClass( NetworkedServerFactory outerInstance, Config config )
			 {
				 this.outerInstance = outerInstance;
				 this._config = config;
			 }

			 public HostnamePort clusterServer()
			 {
				  return _config.get( ClusterSettings.ClusterServer );
			 }

			 public int defaultPort()
			 {
				  return 5001;
			 }

			 public string name()
			 {
				  return null;
			 }
		 }

		 private class ConfigurationAnonymousInnerClass : NetworkSender.Configuration
		 {
			 private readonly NetworkedServerFactory _outerInstance;

			 private Config _config;

			 public ConfigurationAnonymousInnerClass( NetworkedServerFactory outerInstance, Config config )
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
				  return _config.get( ClusterSettings.ClusterServer ).Port;
			 }
		 }

		 private class NetworkChannelsListenerAnonymousInnerClass : NetworkReceiver.NetworkChannelsListener
		 {
			 private readonly NetworkedServerFactory _outerInstance;

			 private Org.Neo4j.cluster.ProtocolServer _protocolServer;

			 public NetworkChannelsListenerAnonymousInnerClass( NetworkedServerFactory outerInstance, Org.Neo4j.cluster.ProtocolServer protocolServer )
			 {
				 this.outerInstance = outerInstance;
				 this._protocolServer = protocolServer;
			 }

			 private StateTransitionLogger logger;

			 public void listeningAt( URI me )
			 {
				  _protocolServer.listeningAt( me );
				  if ( logger == null )
				  {
						logger = new StateTransitionLogger( _outerInstance.logProvider, new AtomicBroadcastSerializer( _outerInstance.objectInputStreamFactory, _outerInstance.objectOutputStreamFactory ) );
						_protocolServer.addStateTransitionListener( logger );
				  }
			 }

			 public void channelOpened( URI to )
			 {
			 }

			 public void channelClosed( URI to )
			 {
			 }
		 }

		 private class LifecycleAnonymousInnerClass : Lifecycle
		 {
			 private readonly NetworkedServerFactory _outerInstance;

			 private Org.Neo4j.cluster.ProtocolServer _protocolServer;

			 public LifecycleAnonymousInnerClass( NetworkedServerFactory outerInstance, Org.Neo4j.cluster.ProtocolServer protocolServer )
			 {
				 this.outerInstance = outerInstance;
				 this._protocolServer = protocolServer;
			 }

			 private ScheduledExecutorService scheduler;

			 public void init()
			 {
				  _protocolServer.Timeouts.tick( DateTimeHelper.CurrentUnixTimeMillis() );
			 }

			 public void start()
			 {
				  scheduler = Executors.newSingleThreadScheduledExecutor( new NamedThreadFactory( "timeout" ) );

				  scheduler.scheduleWithFixedDelay(() =>
				  {
					long now = DateTimeHelper.CurrentUnixTimeMillis();

					_protocolServer.Timeouts.tick( now );
				  }, 0, 10, TimeUnit.MILLISECONDS);
			 }

			 public void stop()
			 {
				  scheduler.shutdownNow();
			 }

			 public void shutdown()
			 {
			 }
		 }
	}

}