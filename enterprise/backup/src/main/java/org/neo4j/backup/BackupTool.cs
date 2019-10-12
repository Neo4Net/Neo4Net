using System;

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
namespace Org.Neo4j.backup
{

	using BackupClient = Org.Neo4j.backup.impl.BackupClient;
	using BackupOutcome = Org.Neo4j.backup.impl.BackupOutcome;
	using BackupProtocolService = Org.Neo4j.backup.impl.BackupProtocolService;
	using BackupServer = Org.Neo4j.backup.impl.BackupServer;
	using ConsistencyCheck = Org.Neo4j.backup.impl.ConsistencyCheck;
	using ComException = Org.Neo4j.com.ComException;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Args = Org.Neo4j.Helpers.Args;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using Service = Org.Neo4j.Helpers.Service;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using MismatchingStoreIdException = Org.Neo4j.Kernel.impl.store.MismatchingStoreIdException;
	using UnexpectedStoreVersionException = Org.Neo4j.Kernel.impl.store.UnexpectedStoreVersionException;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupProtocolServiceFactory.backupProtocolService;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	/// @deprecated Use the {@code neo4j-admin backup} instead. 
	[Obsolete("Use the {@code neo4j-admin backup} instead.")]
	public class BackupTool
	{
		 private const string TO = "to";
		 private const string HOST = "host";
		 private const string PORT = "port";

		 private const string FROM = "from";

		 private const string VERIFY = "verify";
		 private const string CONFIG = "config";

		 private const string CONSISTENCY_CHECKER = "consistency-checker";

		 private const string TIMEOUT = "timeout";
		 private const string FORENSICS = "gather-forensics";
		 public const string DEFAULT_SCHEME = "single";
		 internal const string MISMATCHED_STORE_ID = "You tried to perform a backup from database %s, " + "but the target directory contained a backup from database %s. ";

		 internal static readonly string WrongFromAddressSyntax = "Please properly specify a location to backup in the" + " form " + Dash( HOST ) + " <host> " + Dash( PORT ) + " <port>";

		 internal const string UNKNOWN_SCHEMA_MESSAGE_PATTERN = "%s was specified as a backup module but it was not found. " + "Please make sure that the implementing service is on the classpath.";

		 internal static readonly string NoSourceSpecified = "Please specify " + Dash( HOST ) + " and optionally " + Dash( PORT ) + ", examples:\n" + "  " + Dash( HOST ) + " 192.168.1.34\n" + "  " + Dash( HOST ) + " 192.168.1.34 " + Dash( PORT ) + " 1234";

		 public static void Main( string[] args )
		 {
			  Console.Error.WriteLine( "WARNING: neo4j-backup is deprecated and support for it will be removed in a future\n" + "version of Neo4j; please use neo4j-admin backup instead.\n" );
			  try
			  {
					  using ( BackupProtocolService backupProtocolService = backupProtocolService() )
					  {
						BackupTool tool = new BackupTool( backupProtocolService, System.out );
						BackupOutcome backupOutcome = tool.Run( args );
      
						if ( !backupOutcome.Consistent )
						{
							 ExitFailure( "WARNING: The database is inconsistent." );
						}
					  }
			  }
			  catch ( Exception e )
			  {
					Console.WriteLine( "Backup failed." );
					ExitFailure( e.Message );
			  }
		 }

		 private readonly BackupProtocolService _backupProtocolService;
		 private readonly PrintStream _systemOut;

		 internal BackupTool( BackupProtocolService backupProtocolService, PrintStream systemOut )
		 {
			  this._backupProtocolService = backupProtocolService;
			  this._systemOut = systemOut;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.backup.impl.BackupOutcome run(String[] args) throws ToolFailureException
		 internal virtual BackupOutcome Run( string[] args )
		 {
			  Args arguments = Args.withFlags( VERIFY ).parse( args );

			  if ( !arguments.HasNonNull( TO ) )
			  {
					throw new ToolFailureException( "Specify target location with " + Dash( TO ) + " <target-directory>" );
			  }

			  if ( arguments.HasNonNull( FROM ) && !arguments.Has( HOST ) && !arguments.Has( PORT ) )
			  {
					return RunBackupWithLegacyArgs( arguments );
			  }
			  else if ( arguments.HasNonNull( HOST ) )
			  {
					return RunBackup( arguments );
			  }
			  else
			  {
					throw new ToolFailureException( NoSourceSpecified );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.backup.impl.BackupOutcome runBackupWithLegacyArgs(org.neo4j.helpers.Args args) throws ToolFailureException
		 private BackupOutcome RunBackupWithLegacyArgs( Args args )
		 {
			  string from = args.Get( FROM ).Trim();
			  Path to = Paths.get( args.Get( TO ).Trim() );
			  Config tuningConfiguration = ReadConfiguration( args );
			  bool forensics = args.GetBoolean( FORENSICS, false, true ).Value;
			  ConsistencyCheck consistencyCheck = ParseConsistencyChecker( args );

			  long timeout = args.GetDuration( TIMEOUT, BackupClient.BIG_READ_TIMEOUT );

			  URI backupURI = ResolveBackupUri( from, args, tuningConfiguration );

			  HostnamePort hostnamePort = NewHostnamePort( backupURI );

			  return ExecuteBackup( hostnamePort, to, consistencyCheck, tuningConfiguration, timeout, forensics );
		 }

		 private static ConsistencyCheck ParseConsistencyChecker( Args args )
		 {
			  bool verify = args.GetBoolean( VERIFY, true, true ).Value;
			  if ( verify )
			  {
					string consistencyCheckerName = args.Get( CONSISTENCY_CHECKER, ConsistencyCheck.FULL.name(), ConsistencyCheck.FULL.name() );
					return ConsistencyCheck.fromString( consistencyCheckerName );
			  }
			  return ConsistencyCheck.NONE;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.backup.impl.BackupOutcome runBackup(org.neo4j.helpers.Args args) throws ToolFailureException
		 private BackupOutcome RunBackup( Args args )
		 {
			  string host = args.Get( HOST ).Trim();
			  int port = args.GetNumber( PORT, BackupServer.DEFAULT_PORT ).intValue();
			  Path to = Paths.get( args.Get( TO ).Trim() );
			  Config tuningConfiguration = ReadConfiguration( args );
			  bool forensics = args.GetBoolean( FORENSICS, false, true ).Value;
			  ConsistencyCheck consistencyCheck = ParseConsistencyChecker( args );

			  if ( host.Contains( ":" ) )
			  {
					if ( !host.StartsWith( "[", StringComparison.Ordinal ) )
					{
						 host = "[" + host;
					}
					if ( !host.EndsWith( "]", StringComparison.Ordinal ) )
					{
						 host += "]";
					}
			  }

			  long timeout = args.GetDuration( TIMEOUT, BackupClient.BIG_READ_TIMEOUT );

			  URI backupURI = NewURI( DEFAULT_SCHEME + "://" + host + ":" + port ); // a bit of validation

			  HostnamePort hostnamePort = NewHostnamePort( backupURI );

			  return ExecuteBackup( hostnamePort, to, consistencyCheck, tuningConfiguration, timeout, forensics );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.backup.impl.BackupOutcome executeBackup(org.neo4j.helpers.HostnamePort hostnamePort, java.nio.file.Path to, org.neo4j.backup.impl.ConsistencyCheck consistencyCheck, org.neo4j.kernel.configuration.Config config, long timeout, boolean forensics) throws ToolFailureException
		 internal virtual BackupOutcome ExecuteBackup( HostnamePort hostnamePort, Path to, ConsistencyCheck consistencyCheck, Config config, long timeout, bool forensics )
		 {
			  try
			  {
					_systemOut.println( "Performing backup from '" + hostnamePort + "'" );
					string host = hostnamePort.Host;
					int port = hostnamePort.Port;

					BackupOutcome outcome = _backupProtocolService.doIncrementalBackupOrFallbackToFull( host, port, DatabaseLayout.of( to.toFile() ), consistencyCheck, config, timeout, forensics );
					_systemOut.println( "Done" );
					return outcome;
			  }
			  catch ( Exception e ) when ( e is UnexpectedStoreVersionException || e is IncrementalBackupNotPossibleException )
			  {
					throw new ToolFailureException( e.Message, e );
			  }
			  catch ( MismatchingStoreIdException e )
			  {
					throw new ToolFailureException( string.format( MISMATCHED_STORE_ID, e.Expected, e.Encountered ) );
			  }
			  catch ( ComException e )
			  {
					throw new ToolFailureException( "Couldn't connect to '" + hostnamePort + "'", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.kernel.configuration.Config readConfiguration(org.neo4j.helpers.Args arguments) throws ToolFailureException
		 private static Config ReadConfiguration( Args arguments )
		 {

			  string configFilePath = arguments.Get( CONFIG, null );
			  if ( !string.ReferenceEquals( configFilePath, null ) )
			  {
					File configFile = new File( configFilePath );
					try
					{
						 return Config.fromFile( configFile ).build();
					}
					catch ( Exception e )
					{
						 throw new ToolFailureException( string.Format( "Could not read configuration file [{0}]", configFilePath ), e );
					}
			  }
			  return Config.defaults();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.net.URI resolveBackupUri(String from, org.neo4j.helpers.Args arguments, org.neo4j.kernel.configuration.Config config) throws ToolFailureException
		 private static URI ResolveBackupUri( string from, Args arguments, Config config )
		 {
			  if ( from.Contains( "," ) )
			  {
					if ( !from.StartsWith( "ha://", StringComparison.Ordinal ) )
					{
						 CheckNoSchemaIsPresent( from );
						 from = "ha://" + from;
					}
					return ResolveUriWithProvider( "ha", config, from, arguments );
			  }
			  if ( !from.StartsWith( "single://", StringComparison.Ordinal ) )
			  {
					from = from.Replace( "ha://", "" );
					CheckNoSchemaIsPresent( from );
					from = "single://" + from;
			  }
			  return NewURI( from );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkNoSchemaIsPresent(String address) throws ToolFailureException
		 private static void CheckNoSchemaIsPresent( string address )
		 {
			  if ( address.Contains( "://" ) )
			  {
					throw new ToolFailureException( WrongFromAddressSyntax );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.net.URI newURI(String uriString) throws ToolFailureException
		 private static URI NewURI( string uriString )
		 {
			  try
			  {
					return new URI( uriString );
			  }
			  catch ( URISyntaxException )
			  {
					throw new ToolFailureException( WrongFromAddressSyntax );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.net.URI resolveUriWithProvider(String providerName, org.neo4j.kernel.configuration.Config config, String from, org.neo4j.helpers.Args args) throws ToolFailureException
		 private static URI ResolveUriWithProvider( string providerName, Config config, string from, Args args )
		 {
			  BackupExtensionService service;
			  try
			  {
					service = Service.load( typeof( BackupExtensionService ), providerName );
			  }
			  catch ( NoSuchElementException )
			  {
					throw new ToolFailureException( string.format( UNKNOWN_SCHEMA_MESSAGE_PATTERN, providerName ) );
			  }

			  try
			  {
					ZoneId logTimeZone = config.Get( GraphDatabaseSettings.db_timezone ).ZoneId;
					FormattedLogProvider userLogProvider = FormattedLogProvider.withZoneId( logTimeZone ).toOutputStream( System.out );
					return service.Resolve( from, args, new SimpleLogService( userLogProvider, NullLogProvider.Instance ) );
			  }
			  catch ( Exception t )
			  {
					throw new ToolFailureException( t.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.helpers.HostnamePort newHostnamePort(java.net.URI backupURI) throws ToolFailureException
		 private static HostnamePort NewHostnamePort( URI backupURI )
		 {
			  if ( backupURI == null || backupURI.Host == null )
			  {
					throw new ToolFailureException( WrongFromAddressSyntax );
			  }
			  string host = backupURI.Host;
			  int port = backupURI.Port;
			  if ( port == -1 )
			  {
					port = BackupServer.DEFAULT_PORT;
			  }
			  return new HostnamePort( host, port );
		 }

		 private static string Dash( string name )
		 {
			  return "-" + name;
		 }

		 internal static void ExitFailure( string msg )
		 {
			  Console.WriteLine( msg );
			  Environment.Exit( 1 );
		 }

		 internal class ToolFailureException : Exception
		 {
			  internal ToolFailureException( string message ) : base( message )
			  {
			  }

			  internal ToolFailureException( string message, Exception cause ) : base( message, cause )
			  {
			  }
		 }
	}

}