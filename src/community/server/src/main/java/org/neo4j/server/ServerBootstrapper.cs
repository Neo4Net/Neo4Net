using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server
{
	using Signal = sun.misc.Signal;


	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IOUtils = Neo4Net.Io.IOUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfigurationValidator = Neo4Net.Kernel.configuration.ConfigurationValidator;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;
	using BufferingExecutor = Neo4Net.Kernel.impl.scheduler.BufferingExecutor;
	using JvmChecker = Neo4Net.Kernel.info.JvmChecker;
	using JvmMetadataRepository = Neo4Net.Kernel.info.JvmMetadataRepository;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using RotatingFileOutputStreamSupplier = Neo4Net.Logging.RotatingFileOutputStreamSupplier;
	using Group = Neo4Net.Scheduler.Group;
	using GraphFactory = Neo4Net.Server.database.GraphFactory;
	using JULBridge = Neo4Net.Server.logging.JULBridge;
	using JettyLogBridge = Neo4Net.Server.logging.JettyLogBridge;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.neo4jVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.file.Files.createOrOpenAsOutputStream;

	public abstract class ServerBootstrapper : Bootstrapper
	{
		 public const int OK = 0;
		 private const int WEB_SERVER_STARTUP_ERROR_CODE = 1;
		 private const int GRAPH_DATABASE_STARTUP_ERROR_CODE = 2;
		 private const string SIGTERM = "TERM";
		 private const string SIGINT = "INT";

		 private volatile NeoServer _server;
		 private volatile System.IDisposable _userLogFileStream;
		 private Thread _shutdownHook;
		 private GraphDatabaseDependencies _dependencies = GraphDatabaseDependencies.newDependencies();
		 // in case we have errors loading/validating the configuration log to stdout
		 private Log _log = FormattedLogProvider.toOutputStream( System.out ).getLog( this.GetType() );
		 private string _serverAddress = "unknown address";

		 public static int Start( Bootstrapper boot, params string[] argv )
		 {
			  ServerCommandLineArgs args = ServerCommandLineArgs.Parse( argv );

			  if ( args.Version() )
			  {
					Console.WriteLine( "neo4j " + neo4jVersion() );
					return 0;
			  }

			  if ( args.HomeDir() == null )
			  {
					throw new ServerStartupException( "Argument --home-dir is required and was not provided." );
			  }

			  return boot.Start( args.HomeDir(), args.ConfigFile(), args.ConfigOverrides() );
		 }

		 public override int Start( File homeDir, Optional<File> configFile, IDictionary<string, string> configOverrides )
		 {
			  AddShutdownHook();
			  InstallSignalHandlers();
			  try
			  {
					// Create config file from arguments
					Config config = Config.builder().withFile(configFile).withSettings(configOverrides).withHome(homeDir).withValidators(ConfigurationValidators()).withNoThrowOnFileLoadFailure().withServerDefaults().build();

					LogProvider userLogProvider = SetupLogging( config );
					_dependencies = _dependencies.userLogProvider( userLogProvider );
					_log = userLogProvider.getLog( this.GetType() );
					config.Logger = _log;

					_serverAddress = config.HttpConnectors().Where(c => Encryption.NONE.Equals(c.encryptionLevel())).First().Select(connector => config.Get(connector.listen_address).ToString()).orElse(_serverAddress);

					CheckCompatibility();

					_server = CreateNeoServer( config, _dependencies );
					_server.start();

					return OK;
			  }
			  catch ( ServerStartupException e )
			  {
					e.DescribeTo( _log );
					return WEB_SERVER_STARTUP_ERROR_CODE;
			  }
			  catch ( TransactionFailureException tfe )
			  {
					string locationMsg = ( _server == null ) ? "" : " Another process may be using database location " + _server.Database.Location;
					_log.error( format( "Failed to start Neo4j on %s.", _serverAddress ) + locationMsg, tfe );
					return GRAPH_DATABASE_STARTUP_ERROR_CODE;
			  }
			  catch ( Exception e )
			  {
					_log.error( format( "Failed to start Neo4j on %s.", _serverAddress ), e );
					return WEB_SERVER_STARTUP_ERROR_CODE;
			  }
		 }

		 public override int Stop()
		 {
			  string location = "unknown location";
			  try
			  {
					DoShutdown();

					RemoveShutdownHook();

					return 0;
			  }
			  catch ( Exception e )
			  {
					_log.error( "Failed to cleanly shutdown Neo Server on port [%s], database [%s]. Reason [%s] ", _serverAddress, location, e.Message, e );
					return 1;
			  }
		 }

		 public virtual bool Running
		 {
			 get
			 {
				  return _server != null && _server.Database != null && _server.Database.Running;
			 }
		 }

		 public virtual NeoServer Server
		 {
			 get
			 {
				  return _server;
			 }
		 }

		 public virtual Log Log
		 {
			 get
			 {
				  return _log;
			 }
		 }

		 private NeoServer CreateNeoServer( Config config, GraphDatabaseDependencies dependencies )
		 {
			  GraphFactory graphFactory = CreateGraphFactory( config );

			  bool httpAndHttpsDisabled = config.EnabledHttpConnectors().Count == 0;
			  if ( httpAndHttpsDisabled )
			  {
					return new DisabledNeoServer( graphFactory, dependencies, config );
			  }
			  return CreateNeoServer( graphFactory, config, dependencies );
		 }

		 protected internal abstract GraphFactory CreateGraphFactory( Config config );

		 /// <summary>
		 /// Create a new server component. This method is invoked only when at least one HTTP connector is enabled.
		 /// </summary>
		 protected internal abstract NeoServer CreateNeoServer( GraphFactory graphFactory, Config config, GraphDatabaseDependencies dependencies );

		 protected internal virtual ICollection<ConfigurationValidator> ConfigurationValidators()
		 {
			  return Collections.emptyList();
		 }

		 private LogProvider SetupLogging( Config config )
		 {
			  FormattedLogProvider.Builder builder = FormattedLogProvider.withoutRenderingContext().withZoneId(config.Get(GraphDatabaseSettings.db_timezone).ZoneId).withDefaultLogLevel(config.Get(GraphDatabaseSettings.store_internal_log_level));

			  LogProvider userLogProvider = config.Get( GraphDatabaseSettings.store_user_log_to_stdout ) ? builder.ToOutputStream( System.out ) : CreateFileSystemUserLogProvider( config, builder );

			  JULBridge.resetJUL();
			  Logger.getLogger( "" ).Level = Level.WARNING;
			  JULBridge.forwardTo( userLogProvider );
			  JettyLogBridge.LogProvider = userLogProvider;
			  return userLogProvider;
		 }

		 // Exit gracefully if possible
		 private void InstallSignalHandlers()
		 {
			  InstallSignalHandler( SIGTERM, false ); // SIGTERM is invoked when system service is stopped
			  InstallSignalHandler( SIGINT, true ); // SIGINT is invoked when user hits ctrl-c  when running `neo4j console`
		 }

		 private void InstallSignalHandler( string sig, bool tolerateErrors )
		 {
			  try
			  {
					// System.exit() will trigger the shutdown hook
					Signal.handle( new Signal( sig ), signal => Environment.Exit( 0 ) );
			  }
			  catch ( Exception e )
			  {
					if ( !tolerateErrors )
					{
						 throw e;
					}
					// Errors occur on IBM JDK with IllegalArgumentException: Signal already used by VM: INT
					// I can't find anywhere where we send a SIGINT to neo4j process so I don't think this is that important
			  }
		 }

		 private void DoShutdown()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
			  if ( _userLogFileStream != null )
			  {
					CloseUserLogFileStream();
			  }
		 }

		 private void CloseUserLogFileStream()
		 {
			  IOUtils.closeAllUnchecked( _userLogFileStream );
		 }

		 private void AddShutdownHook()
		 {
			  _shutdownHook = new Thread(() =>
			  {
			  _log.info( "Neo4j Server shutdown initiated by request" );
			  DoShutdown();
			  });
			  Runtime.Runtime.addShutdownHook( _shutdownHook );
		 }

		 private void RemoveShutdownHook()
		 {
			  if ( _shutdownHook != null )
			  {
					if ( !Runtime.Runtime.removeShutdownHook( _shutdownHook ) )
					{
						 _log.warn( "Unable to remove shutdown hook" );
					}
			  }
		 }

		 private LogProvider CreateFileSystemUserLogProvider( Config config, FormattedLogProvider.Builder builder )
		 {
			  BufferingExecutor deferredExecutor = new BufferingExecutor();
			  _dependencies = _dependencies.withDeferredExecutor( deferredExecutor, Group.LOG_ROTATION );

			  FileSystemAbstraction fs = new DefaultFileSystemAbstraction();
			  File destination = config.Get( GraphDatabaseSettings.store_user_log_path );
			  long? rotationThreshold = config.Get( GraphDatabaseSettings.store_user_log_rotation_threshold );
			  try
			  {
					if ( rotationThreshold == 0L )
					{
						 Stream userLog = createOrOpenAsOutputStream( fs, destination, true );
						 // Assign it to the server instance so that it gets closed when the server closes
						 this._userLogFileStream = userLog;
						 return builder.ToOutputStream( userLog );
					}
					RotatingFileOutputStreamSupplier rotatingUserLogSupplier = new RotatingFileOutputStreamSupplier( fs, destination, rotationThreshold.Value, config.Get( GraphDatabaseSettings.store_user_log_rotation_delay ).toMillis(), config.Get(GraphDatabaseSettings.store_user_log_max_archives), deferredExecutor );
					// Assign it to the server instance so that it gets closed when the server closes
					this._userLogFileStream = rotatingUserLogSupplier;
					return builder.ToOutputStream( rotatingUserLogSupplier );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private void CheckCompatibility()
		 {
			  ( new JvmChecker( _log, new JvmMetadataRepository() ) ).checkJvmCompatibilityAndIssueWarning();
		 }
	}

}