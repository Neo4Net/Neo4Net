using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Bolt
{
	using SslContext = io.netty.handler.ssl.SslContext;
	using InternalLoggerFactory = io.netty.util.@internal.logging.InternalLoggerFactory;


	using BoltConnectionFactory = Org.Neo4j.Bolt.runtime.BoltConnectionFactory;
	using BoltSchedulerProvider = Org.Neo4j.Bolt.runtime.BoltSchedulerProvider;
	using BoltStateMachineFactory = Org.Neo4j.Bolt.runtime.BoltStateMachineFactory;
	using BoltStateMachineFactoryImpl = Org.Neo4j.Bolt.runtime.BoltStateMachineFactoryImpl;
	using CachedThreadPoolExecutorFactory = Org.Neo4j.Bolt.runtime.CachedThreadPoolExecutorFactory;
	using DefaultBoltConnectionFactory = Org.Neo4j.Bolt.runtime.DefaultBoltConnectionFactory;
	using ExecutorBoltSchedulerProvider = Org.Neo4j.Bolt.runtime.ExecutorBoltSchedulerProvider;
	using Authentication = Org.Neo4j.Bolt.security.auth.Authentication;
	using BasicAuthentication = Org.Neo4j.Bolt.security.auth.BasicAuthentication;
	using BoltProtocolFactory = Org.Neo4j.Bolt.transport.BoltProtocolFactory;
	using DefaultBoltProtocolFactory = Org.Neo4j.Bolt.transport.DefaultBoltProtocolFactory;
	using Netty4LoggerFactory = Org.Neo4j.Bolt.transport.Netty4LoggerFactory;
	using NettyServer = Org.Neo4j.Bolt.transport.NettyServer;
	using ProtocolInitializer = Org.Neo4j.Bolt.transport.NettyServer.ProtocolInitializer;
	using SocketTransport = Org.Neo4j.Bolt.transport.SocketTransport;
	using TransportThrottleGroup = Org.Neo4j.Bolt.transport.TransportThrottleGroup;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using UserManagerSupplier = Org.Neo4j.Kernel.api.security.UserManagerSupplier;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using UsageData = Org.Neo4j.Udc.UsageData;


	public class BoltServer : LifecycleAdapter
	{
		 // platform dependencies
		 private readonly DatabaseManager _databaseManager;
		 private readonly JobScheduler _jobScheduler;
		 private readonly ConnectorPortRegister _connectorPortRegister;
		 private readonly NetworkConnectionTracker _connectionTracker;
		 private readonly UsageData _usageData;
		 private readonly Config _config;
		 private readonly Clock _clock;
		 private readonly Monitors _monitors;
		 private readonly LogService _logService;

		 // edition specific dependencies are resolved dynamically
		 private readonly DependencyResolver _dependencyResolver;

		 private readonly LifeSupport _life = new LifeSupport();

		 public BoltServer( DatabaseManager databaseManager, JobScheduler jobScheduler, ConnectorPortRegister connectorPortRegister, NetworkConnectionTracker connectionTracker, UsageData usageData, Config config, Clock clock, Monitors monitors, LogService logService, DependencyResolver dependencyResolver )
		 {
			  this._databaseManager = databaseManager;
			  this._jobScheduler = jobScheduler;
			  this._connectorPortRegister = connectorPortRegister;
			  this._connectionTracker = connectionTracker;
			  this._usageData = usageData;
			  this._config = config;
			  this._clock = clock;
			  this._monitors = monitors;
			  this._logService = logService;
			  this._dependencyResolver = dependencyResolver;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  Log log = _logService.getInternalLog( typeof( BoltServer ) );
			  Log userLog = _logService.getUserLog( typeof( BoltServer ) );

			  InternalLoggerFactory.DefaultFactory = new Netty4LoggerFactory( _logService.InternalLogProvider );

			  Authentication authentication = CreateAuthentication();

			  TransportThrottleGroup throttleGroup = new TransportThrottleGroup( _config, _clock );

			  BoltSchedulerProvider boltSchedulerProvider = _life.add( new ExecutorBoltSchedulerProvider( _config, new CachedThreadPoolExecutorFactory( log ), _jobScheduler, _logService ) );
			  BoltConnectionFactory boltConnectionFactory = CreateConnectionFactory( _config, boltSchedulerProvider, throttleGroup, _logService, _clock );
			  BoltStateMachineFactory boltStateMachineFactory = CreateBoltFactory( authentication, _clock );

			  BoltProtocolFactory boltProtocolFactory = CreateBoltProtocolFactory( boltConnectionFactory, boltStateMachineFactory );

			  if ( _config.enabledBoltConnectors().Count > 0 && !_config.get(GraphDatabaseSettings.disconnected) )
			  {
					NettyServer server = new NettyServer( _jobScheduler.threadFactory( Group.BOLT_NETWORK_IO ), CreateConnectors( boltProtocolFactory, throttleGroup, log ), _connectorPortRegister, userLog );
					_life.add( server );
					log.Info( "Bolt server loaded" );
			  }

			  _life.start(); // init and start the nested lifecycle
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _life.shutdown(); // stop and shutdown the nested lifecycle
		 }

		 private BoltConnectionFactory CreateConnectionFactory( Config config, BoltSchedulerProvider schedulerProvider, TransportThrottleGroup throttleGroup, LogService logService, Clock clock )
		 {
			  return new DefaultBoltConnectionFactory( schedulerProvider, throttleGroup, config, logService, clock, _monitors );
		 }

		 private IDictionary<BoltConnector, NettyServer.ProtocolInitializer> CreateConnectors( BoltProtocolFactory boltProtocolFactory, TransportThrottleGroup throttleGroup, Log log )
		 {
			  return _config.enabledBoltConnectors().ToDictionary(identity(), connector => CreateProtocolInitializer(connector, boltProtocolFactory, throttleGroup, log));
		 }

		 private NettyServer.ProtocolInitializer CreateProtocolInitializer( BoltConnector connector, BoltProtocolFactory boltProtocolFactory, TransportThrottleGroup throttleGroup, Log log )
		 {
			  SslContext sslCtx;
			  bool requireEncryption;
			  BoltConnector.EncryptionLevel encryptionLevel = _config.get( connector.EncryptionLevel );
			  SslPolicyLoader sslPolicyLoader = _dependencyResolver.resolveDependency( typeof( SslPolicyLoader ) );

			  switch ( encryptionLevel )
			  {
			  case BoltConnector.EncryptionLevel.REQUIRED:
					// Encrypted connections are mandatory, a self-signed certificate may be generated.
					requireEncryption = true;
					sslCtx = CreateSslContext( sslPolicyLoader, _config );
					break;
			  case BoltConnector.EncryptionLevel.OPTIONAL:
					// Encrypted connections are optional, a self-signed certificate may be generated.
					requireEncryption = false;
					sslCtx = CreateSslContext( sslPolicyLoader, _config );
					break;
			  case BoltConnector.EncryptionLevel.DISABLED:
					// Encryption is turned off, no self-signed certificate will be generated.
					requireEncryption = false;
					sslCtx = null;
					break;
			  default:
					// In the unlikely event that we happen to fall through to the default option here,
					// there is a mismatch between the BoltConnector.EncryptionLevel enum and the options
					// handled in this switch statement. In this case, we'll log a warning and default to
					// disabling encryption, since this mirrors the functionality introduced in 3.0.
					log.Warn( "Unhandled encryption level %s - assuming DISABLED.", encryptionLevel.name() );
					requireEncryption = false;
					sslCtx = null;
					break;
			  }

			  ListenSocketAddress listenAddress = _config.get( connector.ListenAddress );
			  return new SocketTransport( connector.Key(), listenAddress, sslCtx, requireEncryption, _logService.InternalLogProvider, throttleGroup, boltProtocolFactory, _connectionTracker );
		 }

		 private static SslContext CreateSslContext( SslPolicyLoader sslPolicyFactory, Config config )
		 {
			  try
			  {
					string policyName = config.Get( GraphDatabaseSettings.bolt_ssl_policy );
					if ( string.ReferenceEquals( policyName, null ) )
					{
						 throw new System.ArgumentException( "No SSL policy has been configured for Bolt server" );
					}
					return sslPolicyFactory.GetPolicy( policyName ).nettyServerContext();
			  }
			  catch ( Exception e )
			  {
					throw new Exception( "Failed to initialize SSL encryption support, which is required to start this connector. " + "Error was: " + e.Message, e );
			  }
		 }

		 private Authentication CreateAuthentication()
		 {
			  return new BasicAuthentication( _dependencyResolver.resolveDependency( typeof( AuthManager ) ), _dependencyResolver.resolveDependency( typeof( UserManagerSupplier ) ) );
		 }

		 private BoltProtocolFactory CreateBoltProtocolFactory( BoltConnectionFactory connectionFactory, BoltStateMachineFactory stateMachineFactory )
		 {
			  return new DefaultBoltProtocolFactory( connectionFactory, stateMachineFactory, _logService );
		 }

		 private BoltStateMachineFactory CreateBoltFactory( Authentication authentication, Clock clock )
		 {
			  return new BoltStateMachineFactoryImpl( _databaseManager, _usageData, authentication, clock, _config, _logService );
		 }
	}

}