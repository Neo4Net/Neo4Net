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
namespace Neo4Net.Bolt
{
	using SslContext = io.netty.handler.ssl.SslContext;
	using InternalLoggerFactory = io.netty.util.@internal.logging.InternalLoggerFactory;


	using BoltConnectionFactory = Neo4Net.Bolt.runtime.BoltConnectionFactory;
	using BoltSchedulerProvider = Neo4Net.Bolt.runtime.BoltSchedulerProvider;
	using BoltStateMachineFactory = Neo4Net.Bolt.runtime.BoltStateMachineFactory;
	using BoltStateMachineFactoryImpl = Neo4Net.Bolt.runtime.BoltStateMachineFactoryImpl;
	using CachedThreadPoolExecutorFactory = Neo4Net.Bolt.runtime.CachedThreadPoolExecutorFactory;
	using DefaultBoltConnectionFactory = Neo4Net.Bolt.runtime.DefaultBoltConnectionFactory;
	using ExecutorBoltSchedulerProvider = Neo4Net.Bolt.runtime.ExecutorBoltSchedulerProvider;
	using Authentication = Neo4Net.Bolt.security.auth.Authentication;
	using BasicAuthentication = Neo4Net.Bolt.security.auth.BasicAuthentication;
	using BoltProtocolFactory = Neo4Net.Bolt.transport.BoltProtocolFactory;
	using DefaultBoltProtocolFactory = Neo4Net.Bolt.transport.DefaultBoltProtocolFactory;
	using Netty4LoggerFactory = Neo4Net.Bolt.transport.Netty4LoggerFactory;
	using NettyServer = Neo4Net.Bolt.transport.NettyServer;
	using ProtocolInitializer = Neo4Net.Bolt.transport.NettyServer.ProtocolInitializer;
	using SocketTransport = Neo4Net.Bolt.transport.SocketTransport;
	using TransportThrottleGroup = Neo4Net.Bolt.transport.TransportThrottleGroup;
	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using UserManagerSupplier = Neo4Net.Kernel.api.security.UserManagerSupplier;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using SslPolicyLoader = Neo4Net.Kernel.configuration.ssl.SslPolicyLoader;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using UsageData = Neo4Net.Udc.UsageData;


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