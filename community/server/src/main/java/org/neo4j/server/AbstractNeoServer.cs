﻿using System;
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
namespace Org.Neo4j.Server
{
	using Configuration = org.apache.commons.configuration.Configuration;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Dependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using RunCarefully = Org.Neo4j.Helpers.RunCarefully;
	using DiagnosticsManager = Org.Neo4j.@internal.Diagnostics.DiagnosticsManager;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using UserManagerSupplier = Org.Neo4j.Kernel.api.security.UserManagerSupplier;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using HttpConnector = Org.Neo4j.Kernel.configuration.HttpConnector;
	using Encryption = Org.Neo4j.Kernel.configuration.HttpConnector.Encryption;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using Version = Org.Neo4j.Kernel.@internal.Version;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using CypherExecutor = Org.Neo4j.Server.database.CypherExecutor;
	using CypherExecutorProvider = Org.Neo4j.Server.database.CypherExecutorProvider;
	using Database = Org.Neo4j.Server.database.Database;
	using DatabaseProvider = Org.Neo4j.Server.database.DatabaseProvider;
	using GraphDatabaseServiceProvider = Org.Neo4j.Server.database.GraphDatabaseServiceProvider;
	using GraphFactory = Org.Neo4j.Server.database.GraphFactory;
	using Org.Neo4j.Server.database;
	using LifecycleManagingDatabase = Org.Neo4j.Server.database.LifecycleManagingDatabase;
	using RESTApiModule = Org.Neo4j.Server.modules.RESTApiModule;
	using ServerModule = Org.Neo4j.Server.modules.ServerModule;
	using ConfigAdapter = Org.Neo4j.Server.plugins.ConfigAdapter;
	using PluginInvocatorProvider = Org.Neo4j.Server.plugins.PluginInvocatorProvider;
	using PluginManager = Org.Neo4j.Server.plugins.PluginManager;
	using InputFormatProvider = Org.Neo4j.Server.rest.repr.InputFormatProvider;
	using OutputFormatProvider = Org.Neo4j.Server.rest.repr.OutputFormatProvider;
	using RepresentationFormatRepository = Org.Neo4j.Server.rest.repr.RepresentationFormatRepository;
	using TransactionFacade = Org.Neo4j.Server.rest.transactional.TransactionFacade;
	using TransactionFilter = Org.Neo4j.Server.rest.transactional.TransactionFilter;
	using TransactionHandleRegistry = Org.Neo4j.Server.rest.transactional.TransactionHandleRegistry;
	using TransactionRegistry = Org.Neo4j.Server.rest.transactional.TransactionRegistry;
	using TransitionalPeriodTransactionMessContainer = Org.Neo4j.Server.rest.transactional.TransitionalPeriodTransactionMessContainer;
	using DatabaseActions = Org.Neo4j.Server.rest.web.DatabaseActions;
	using AsyncRequestLog = Org.Neo4j.Server.web.AsyncRequestLog;
	using SimpleUriBuilder = Org.Neo4j.Server.web.SimpleUriBuilder;
	using WebServer = Org.Neo4j.Server.web.WebServer;
	using WebServerProvider = Org.Neo4j.Server.web.WebServerProvider;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;
	using Clocks = Org.Neo4j.Time.Clocks;
	using UsageData = Org.Neo4j.Udc.UsageData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.round;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.db_timezone;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.configuration.ServerSettings.http_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.configuration.ServerSettings.http_logging_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.configuration.ServerSettings.http_logging_rotation_keep_number;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.configuration.ServerSettings.http_logging_rotation_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.database.InjectableProvider.providerForSingleton;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.database.InjectableProvider.providerFromSupplier;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.exception.ServerStartupErrors.translateToServerStartupError;

	public abstract class AbstractNeoServer : NeoServer
	{
		public abstract IEnumerable<Org.Neo4j.Server.rest.management.AdvertisableService> Services { get; }
		 private const long MINIMUM_TIMEOUT = 1000L;
		 /// <summary>
		 /// We add a second to the timeout if the user configures a 1-second timeout.
		 /// <para>
		 /// This ensures the expiry time displayed to the user is always at least 1 second, even after it is rounded down.
		 /// </para>
		 /// </summary>
		 private const long ROUNDING_SECOND = 1000L;

		 private static readonly Pattern[] _defaultUriWhitelist = new Pattern[]{ Pattern.compile( "/browser.*" ), Pattern.compile( "/" ) };
		 public static readonly string Neo4jIsStartingMessage = "======== Neo4j " + Version.Neo4jVersion + " ========";

		 protected internal readonly LogProvider UserLogProvider;
		 private readonly Log _log;

		 private readonly IList<ServerModule> _serverModules = new List<ServerModule>();
		 private readonly SimpleUriBuilder _uriBuilder = new SimpleUriBuilder();
		 private readonly Config _config;
		 private readonly LifeSupport _life = new LifeSupport();
		 private readonly ListenSocketAddress _httpListenAddress;
		 private readonly ListenSocketAddress _httpsListenAddress;
		 private AdvertisedSocketAddress _httpAdvertisedAddress;
		 private AdvertisedSocketAddress _httpsAdvertisedAddress;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly Database DatabaseConflict;
		 private DependencyResolver _dependencyResolver;
		 protected internal CypherExecutor CypherExecutor;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal WebServer WebServerConflict;
		 protected internal System.Func<AuthManager> AuthManagerSupplier;
		 protected internal System.Func<UserManagerSupplier> UserManagerSupplier;
		 protected internal System.Func<SslPolicyLoader> SslPolicyFactorySupplier;
		 private DatabaseActions _databaseActions;
		 private TransactionFacade _transactionFacade;

		 private TransactionHandleRegistry _transactionRegistry;
		 private ConnectorPortRegister _connectorPortRegister;
		 private HttpConnector _httpConnector;
		 private HttpConnector _httpsConnector;
		 private AsyncRequestLog _requestLog;
		 private readonly System.Func<AvailabilityGuard> _availabilityGuardSupplier;

		 protected internal abstract IEnumerable<ServerModule> CreateServerModules();

		 protected internal abstract WebServer CreateWebServer();

		 public AbstractNeoServer( Config config, GraphFactory graphFactory, Dependencies dependencies )
		 {
			  this._config = config;
			  this.UserLogProvider = dependencies.UserLogProvider();
			  this._log = UserLogProvider.getLog( this.GetType() );
			  _log.info( Neo4jIsStartingMessage );

			  VerifyConnectorsConfiguration( config );

			  _httpConnector = FindConnector( config, HttpConnector.Encryption.NONE );
			  _httpListenAddress = ListenAddressFor( config, _httpConnector );
			  _httpAdvertisedAddress = AdvertisedAddressFor( config, _httpConnector );

			  _httpsConnector = FindConnector( config, HttpConnector.Encryption.TLS );
			  _httpsListenAddress = ListenAddressFor( config, _httpsConnector );
			  _httpsAdvertisedAddress = AdvertisedAddressFor( config, _httpsConnector );

			  DatabaseConflict = new LifecycleManagingDatabase( config, graphFactory, dependencies );
			  this._availabilityGuardSupplier = ( ( LifecycleManagingDatabase ) DatabaseConflict ).getAvailabilityGuard;
			  _life.add( DatabaseConflict );
			  _life.add( new ServerDependenciesLifeCycleAdapter( this ) );
			  _life.add( new ServerComponentsLifecycleAdapter( this ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws ServerStartupException
		 public override void Start()
		 {
			  try
			  {
					_life.start();
			  }
			  catch ( Exception t )
			  {
					// If the database has been started, attempt to cleanly shut it down to avoid unclean shutdowns.
					_life.shutdown();

					throw translateToServerStartupError( t );
			  }
		 }

		 public virtual DependencyResolver DependencyResolver
		 {
			 get
			 {
				  return _dependencyResolver;
			 }
		 }

		 protected internal virtual DatabaseActions CreateDatabaseActions()
		 {
			  return new DatabaseActions( DatabaseConflict.Graph );
		 }

		 private TransactionFacade CreateTransactionalActions()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long timeoutMillis = getTransactionTimeoutMillis();
			  long timeoutMillis = TransactionTimeoutMillis;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.Clock clock = org.neo4j.time.Clocks.systemClock();
			  Clock clock = Clocks.systemClock();

			  _transactionRegistry = new TransactionHandleRegistry( clock, timeoutMillis, UserLogProvider );

			  // ensure that this is > 0
			  long runEvery = round( timeoutMillis / 2.0 );

			  ResolveDependency( typeof( JobScheduler ) ).scheduleRecurring(Group.SERVER_TRANSACTION_TIMEOUT, () =>
			  {
				long maxAge = clock.millis() - timeoutMillis;
				_transactionRegistry.rollbackSuspendedTransactionsIdleSince( maxAge );
			  }, runEvery, MILLISECONDS);

			  return new TransactionFacade( new TransitionalPeriodTransactionMessContainer( DatabaseConflict.Graph ), ResolveDependency( typeof( QueryExecutionEngine ) ), ResolveDependency( typeof( GraphDatabaseQueryService ) ), _transactionRegistry, UserLogProvider );
		 }

		 /// <summary>
		 /// We are going to ensure the minimum timeout is 2 seconds. The timeout value is communicated to the user in
		 /// seconds rounded down, meaning if a user set a 1 second timeout, he would be told there was less than 1 second
		 /// remaining before he would need to renew the timeout.
		 /// </summary>
		 private long TransactionTimeoutMillis
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final long timeout = config.get(org.neo4j.server.configuration.ServerSettings.transaction_idle_timeout).toMillis();
				  long timeout = _config.get( ServerSettings.transaction_idle_timeout ).toMillis();
				  return Math.Max( timeout, MINIMUM_TIMEOUT + ROUNDING_SECOND );
			 }
		 }

		 /// <summary>
		 /// Use this method to register server modules from subclasses
		 /// </summary>
		 protected internal void RegisterModule( ServerModule module )
		 {
			  _serverModules.Add( module );
		 }

		 private void StartModules()
		 {
			  foreach ( ServerModule module in _serverModules )
			  {
					module.Start();
			  }
		 }

		 private void StopModules()
		 {
			  ( new RunCarefully( map( module => module.stop, _serverModules ) ) ).run();
		 }

		 private void ClearModules()
		 {
			  _serverModules.Clear();
		 }

		 public virtual Config Config
		 {
			 get
			 {
				  return _config;
			 }
		 }

		 protected internal virtual void ConfigureWebServer()
		 {
			  WebServerConflict.HttpAddress = _httpListenAddress;
			  WebServerConflict.HttpsAddress = _httpsListenAddress;
			  WebServerConflict.MaxThreads = _config.get( ServerSettings.webserver_max_threads );
			  WebServerConflict.WadlEnabled = _config.get( ServerSettings.wadl_enabled );
			  WebServerConflict.DefaultInjectables = CreateDefaultInjectables();

			  string sslPolicyName = _config.get( ServerSettings.ssl_policy );
			  if ( !string.ReferenceEquals( sslPolicyName, null ) )
			  {
					SslPolicy sslPolicy = SslPolicyFactorySupplier.get().getPolicy(sslPolicyName);
					WebServerConflict.SslPolicy = sslPolicy;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void startWebServer() throws Exception
		 protected internal virtual void StartWebServer()
		 {
			  try
			  {
					SetUpHttpLogging();
					WebServerConflict.start();
					RegisterHttpAddressAfterStartup();
					RegisterHttpsAddressAfterStartup();
					_log.info( "Remote interface available at %s", BaseUri() );
			  }
			  catch ( Exception e )
			  {
					ListenSocketAddress address = _httpListenAddress != null ? _httpListenAddress : _httpsListenAddress;
					_log.error( "Failed to start Neo4j on %s: %s", address, e.Message );
					throw e;
			  }
		 }

		 private void RegisterHttpAddressAfterStartup()
		 {
			  if ( _httpConnector != null )
			  {
					InetSocketAddress localHttpAddress = WebServerConflict.LocalHttpAddress;
					_connectorPortRegister.register( _httpConnector.key(), localHttpAddress );
					if ( _httpAdvertisedAddress.Port == 0 )
					{
						 _httpAdvertisedAddress = new AdvertisedSocketAddress( localHttpAddress.HostString, localHttpAddress.Port );
					}
			  }
		 }

		 private void RegisterHttpsAddressAfterStartup()
		 {
			  if ( _httpsConnector != null )
			  {
					InetSocketAddress localHttpsAddress = WebServerConflict.LocalHttpsAddress;
					_connectorPortRegister.register( _httpsConnector.key(), localHttpsAddress );
					if ( _httpsAdvertisedAddress.Port == 0 )
					{
						 _httpsAdvertisedAddress = new AdvertisedSocketAddress( localHttpsAddress.HostString, localHttpsAddress.Port );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setUpHttpLogging() throws java.io.IOException
		 private void SetUpHttpLogging()
		 {
			  if ( !Config.get( http_logging_enabled ) )
			  {
					return;
			  }

			  _requestLog = new AsyncRequestLog( _dependencyResolver.resolveDependency( typeof( FileSystemAbstraction ) ), _config.get( db_timezone ).ZoneId, _config.get( http_log_path ).ToString(), _config.get(http_logging_rotation_size), _config.get(http_logging_rotation_keep_number) );
			  WebServerConflict.RequestLog = _requestLog;
		 }

		 protected internal virtual Pattern[] UriWhitelist
		 {
			 get
			 {
				  return _defaultUriWhitelist;
			 }
		 }

		 public override void Stop()
		 {
			  TryShutdownAvailabiltyGuard();
			  _life.stop();
		 }

		 private void TryShutdownAvailabiltyGuard()
		 {
			  AvailabilityGuard guard = _availabilityGuardSupplier.get();
			  if ( guard != null )
			  {
					guard.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void stopWebServer() throws Exception
		 private void StopWebServer()
		 {
			  if ( WebServerConflict != null )
			  {
					WebServerConflict.stop();
			  }
			  if ( _requestLog != null )
			  {
					_requestLog.stop();
			  }
		 }

		 public virtual Database Database
		 {
			 get
			 {
				  return DatabaseConflict;
			 }
		 }

		 public virtual TransactionRegistry TransactionRegistry
		 {
			 get
			 {
				  return _transactionRegistry;
			 }
		 }

		 public override URI BaseUri()
		 {
			  return _httpAdvertisedAddress != null ? _uriBuilder.buildURI( _httpAdvertisedAddress, false ) : _uriBuilder.buildURI( _httpsAdvertisedAddress, true );
		 }

		 public virtual Optional<URI> HttpsUri()
		 {
			  return Optional.ofNullable( _httpsAdvertisedAddress ).map( address => _uriBuilder.buildURI( address, true ) );
		 }

		 public virtual WebServer WebServer
		 {
			 get
			 {
				  return WebServerConflict;
			 }
		 }

		 public virtual PluginManager ExtensionManager
		 {
			 get
			 {
				  RESTApiModule module = GetModule( typeof( RESTApiModule ) );
				  if ( module != null )
				  {
						return module.Plugins;
				  }
				  return null;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected java.util.Collection<org.neo4j.server.database.InjectableProvider<?>> createDefaultInjectables()
		 protected internal virtual ICollection<InjectableProvider<object>> CreateDefaultInjectables()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.neo4j.server.database.InjectableProvider<?>> singletons = new java.util.ArrayList<>();
			  ICollection<InjectableProvider<object>> singletons = new List<InjectableProvider<object>>();

			  Database database = Database;

			  singletons.Add( new DatabaseProvider( database ) );
			  singletons.Add( new DatabaseActions.Provider( _databaseActions ) );
			  singletons.Add( new GraphDatabaseServiceProvider( database ) );
			  singletons.Add( new NeoServerProvider( this ) );
			  singletons.Add( providerForSingleton( new ConfigAdapter( Config ), typeof( Configuration ) ) );
			  singletons.Add( providerForSingleton( Config, typeof( Config ) ) );

			  singletons.Add( new WebServerProvider( WebServer ) );

			  PluginInvocatorProvider pluginInvocatorProvider = new PluginInvocatorProvider( this );
			  singletons.Add( pluginInvocatorProvider );
			  RepresentationFormatRepository repository = new RepresentationFormatRepository( this );

			  singletons.Add( new InputFormatProvider( repository ) );
			  singletons.Add( new OutputFormatProvider( repository ) );
			  singletons.Add( new CypherExecutorProvider( CypherExecutor ) );

			  singletons.Add( providerForSingleton( _transactionFacade, typeof( TransactionFacade ) ) );
			  singletons.Add( providerFromSupplier( AuthManagerSupplier, typeof( AuthManager ) ) );
			  singletons.Add( providerFromSupplier( UserManagerSupplier, typeof( UserManagerSupplier ) ) );
			  singletons.Add( new TransactionFilter( database ) );
			  singletons.Add( new LoggingProvider( UserLogProvider ) );
			  singletons.Add( providerForSingleton( UserLogProvider.getLog( typeof( NeoServer ) ), typeof( Log ) ) );
			  singletons.Add( providerForSingleton( ResolveDependency( typeof( UsageData ) ), typeof( UsageData ) ) );

			  return singletons;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.neo4j.server.modules.ServerModule> T getModule(Class<T> clazz)
		 private T GetModule<T>( Type clazz ) where T : Org.Neo4j.Server.modules.ServerModule
		 {
				 clazz = typeof( T );
			  foreach ( ServerModule sm in _serverModules )
			  {
					if ( sm.GetType() == clazz )
					{
						 return ( T ) sm;
					}
			  }

			  return null;
		 }

		 protected internal virtual T ResolveDependency<T>( Type type )
		 {
				 type = typeof( T );
			  return _dependencyResolver.resolveDependency( type );
		 }

		 private static void VerifyConnectorsConfiguration( Config config )
		 {
			  HttpConnector httpConnector = FindConnector( config, HttpConnector.Encryption.NONE );
			  HttpConnector httpsConnector = FindConnector( config, HttpConnector.Encryption.TLS );

			  if ( httpConnector == null && httpsConnector == null )
			  {
					throw new System.ArgumentException( "Either HTTP or HTTPS connector must be configured to run the server" );
			  }
		 }

		 private static HttpConnector FindConnector( Config config, HttpConnector.Encryption encryption )
		 {
			  return config.EnabledHttpConnectors().Where(connector => connector.encryptionLevel() == encryption).First().orElse(null);
		 }

		 private static ListenSocketAddress ListenAddressFor( Config config, HttpConnector connector )
		 {
			  return connector == null ? null : config.Get( connector.ListenAddress );
		 }

		 private static AdvertisedSocketAddress AdvertisedAddressFor( Config config, HttpConnector connector )
		 {
			  return connector == null ? null : config.Get( connector.AdvertisedAddress );
		 }

		 private class ServerDependenciesLifeCycleAdapter : LifecycleAdapter
		 {
			 private readonly AbstractNeoServer _outerInstance;

			 public ServerDependenciesLifeCycleAdapter( AbstractNeoServer outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Start()
			  {
					outerInstance.dependencyResolver = outerInstance.DatabaseConflict.Graph.DependencyResolver;

					outerInstance.AuthManagerSupplier = outerInstance.dependencyResolver.ProvideDependency( typeof( AuthManager ) );
					outerInstance.UserManagerSupplier = outerInstance.dependencyResolver.ProvideDependency( typeof( UserManagerSupplier ) );
					outerInstance.SslPolicyFactorySupplier = outerInstance.dependencyResolver.ProvideDependency( typeof( SslPolicyLoader ) );
					outerInstance.WebServerConflict = outerInstance.CreateWebServer();

					foreach ( ServerModule moduleClass in outerInstance.CreateServerModules() )
					{
						 outerInstance.RegisterModule( moduleClass );
					}
			  }
		 }

		 private class ServerComponentsLifecycleAdapter : LifecycleAdapter
		 {
			 private readonly AbstractNeoServer _outerInstance;

			 public ServerComponentsLifecycleAdapter( AbstractNeoServer outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
			  public override void Start()
			  {
					DiagnosticsManager diagnosticsManager = outerInstance.ResolveDependency( typeof( DiagnosticsManager ) );
					Log diagnosticsLog = diagnosticsManager.TargetLog;
					diagnosticsLog.Info( "--- SERVER STARTED START ---" );
					outerInstance.connectorPortRegister = outerInstance.dependencyResolver.ResolveDependency( typeof( ConnectorPortRegister ) );
					outerInstance.databaseActions = outerInstance.CreateDatabaseActions();

					outerInstance.transactionFacade = outerInstance.createTransactionalActions();

					outerInstance.CypherExecutor = new CypherExecutor( outerInstance.DatabaseConflict, outerInstance.UserLogProvider );

					outerInstance.ConfigureWebServer();

					outerInstance.CypherExecutor.start();

					outerInstance.startModules();

					outerInstance.StartWebServer();

					diagnosticsLog.Info( "--- SERVER STARTED END ---" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Exception
			  public override void Stop()
			  {
					outerInstance.stopWebServer();
					outerInstance.stopModules();
					outerInstance.clearModules();
			  }
		 }
	}

}