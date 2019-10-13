using System;

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
namespace Neo4Net.Consistency
{

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using OptionalBooleanArg = Neo4Net.CommandLine.Args.OptionalBooleanArg;
	using OptionalCanonicalPath = Neo4Net.CommandLine.Args.Common.OptionalCanonicalPath;
	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfigurableStandalonePageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory;
	using RecoveryRequiredException = Neo4Net.Kernel.impl.recovery.RecoveryRequiredException;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.arguments.common.Database.ARG_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.database_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.recovery.RecoveryRequiredChecker.assertRecoveryIsNotRequired;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	public class CheckConsistencyCommand : AdminCommand
	{
		 public const string CHECK_GRAPH = "check-graph";
		 public const string CHECK_INDEXES = "check-indexes";
		 public const string CHECK_INDEX_STRUCTURE = "check-index-structure";
		 public const string CHECK_LABEL_SCAN_STORE = "check-label-scan-store";
		 public const string CHECK_PROPERTY_OWNERS = "check-property-owners";
		 private static readonly Arguments _arguments = new Arguments().withDatabase().withArgument(new OptionalCanonicalPath("backup", "/path/to/backup", "", "Path to backup to check consistency of. Cannot be used together with --database.")).withArgument(new OptionalBooleanArg("verbose", false, "Enable verbose output.")).withArgument(new OptionalCanonicalPath("report-dir", "directory", ".", "Directory to write report file in.")).withArgument(new OptionalCanonicalPath("additional-config", "config-file-path", "", "Configuration file to supply additional configuration in. This argument is DEPRECATED.")).withArgument(new OptionalBooleanArg(CHECK_GRAPH, true, "Perform checks between nodes, relationships, properties, types and tokens.")).withArgument(new OptionalBooleanArg(CHECK_INDEXES, true, "Perform checks on indexes.")).withArgument(new OptionalBooleanArg(CHECK_INDEX_STRUCTURE, false, "Perform structure checks on indexes.")).withArgument(new OptionalBooleanArg(CHECK_LABEL_SCAN_STORE, true, "Perform checks on the label scan store.")).withArgument(new OptionalBooleanArg(CHECK_PROPERTY_OWNERS, false, "Perform additional checks on property ownership. This check is *very* expensive in time and " + "memory."));

		 private readonly Path _homeDir;
		 private readonly Path _configDir;
		 private readonly ConsistencyCheckService _consistencyCheckService;

		 public CheckConsistencyCommand( Path homeDir, Path configDir ) : this( homeDir, configDir, new ConsistencyCheckService() )
		 {
		 }

		 public CheckConsistencyCommand( Path homeDir, Path configDir, ConsistencyCheckService consistencyCheckService )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
			  this._consistencyCheckService = consistencyCheckService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String database;
			  string database;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean verbose;
			  bool verbose;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Optional<java.nio.file.Path> additionalConfigFile;
			  Optional<Path> additionalConfigFile;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.file.Path reportDir;
			  Path reportDir;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Optional<java.nio.file.Path> backupPath;
			  Optional<Path> backupPath;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean checkGraph;
			  bool checkGraph;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean checkIndexes;
			  bool checkIndexes;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean checkIndexStructure;
			  bool checkIndexStructure;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean checkLabelScanStore;
			  bool checkLabelScanStore;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean checkPropertyOwners;
			  bool checkPropertyOwners;

			  try
			  {
					database = _arguments.parse( args ).get( ARG_DATABASE );
					backupPath = _arguments.getOptionalPath( "backup" );
					verbose = _arguments.getBoolean( "verbose" );
					additionalConfigFile = _arguments.getOptionalPath( "additional-config" );
					reportDir = _arguments.getOptionalPath( "report-dir" ).orElseThrow( () => new System.ArgumentException("report-dir must be a valid path") );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }

			  if ( backupPath.Present )
			  {
					if ( _arguments.has( ARG_DATABASE ) )
					{
						 throw new IncorrectUsage( "Only one of '--" + ARG_DATABASE + "' and '--backup' can be specified." );
					}
					if ( !backupPath.get().toFile().Directory )
					{
						 throw new CommandFailed( format( "Specified backup should be a directory: %s", backupPath.get() ) );
					}
			  }

			  Config config = LoadNeo4jConfig( _homeDir, _configDir, database, LoadAdditionalConfig( additionalConfigFile ) );

			  try
			  {
					// We can remove the loading from config file in 4.0
					if ( _arguments.has( CHECK_GRAPH ) )
					{
						 checkGraph = _arguments.getBoolean( CHECK_GRAPH );
					}
					else
					{
						 checkGraph = config.Get( ConsistencyCheckSettings.ConsistencyCheckGraph );
					}
					if ( _arguments.has( CHECK_INDEXES ) )
					{
						 checkIndexes = _arguments.getBoolean( CHECK_INDEXES );
					}
					else
					{
						 checkIndexes = config.Get( ConsistencyCheckSettings.ConsistencyCheckIndexes );
					}
					if ( _arguments.has( CHECK_INDEX_STRUCTURE ) )
					{
						 checkIndexStructure = _arguments.getBoolean( CHECK_INDEX_STRUCTURE );
					}
					else
					{
						 checkIndexStructure = config.Get( ConsistencyCheckSettings.ConsistencyCheckIndexStructure );
					}
					if ( _arguments.has( CHECK_LABEL_SCAN_STORE ) )
					{
						 checkLabelScanStore = _arguments.getBoolean( CHECK_LABEL_SCAN_STORE );
					}
					else
					{
						 checkLabelScanStore = config.Get( ConsistencyCheckSettings.ConsistencyCheckLabelScanStore );
					}
					if ( _arguments.has( CHECK_PROPERTY_OWNERS ) )
					{
						 checkPropertyOwners = _arguments.getBoolean( CHECK_PROPERTY_OWNERS );
					}
					else
					{
						 checkPropertyOwners = config.Get( ConsistencyCheckSettings.ConsistencyCheckPropertyOwners );
					}
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }

			  try
			  {
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
					  {
						File databaseDirectory = backupPath.map( Path.toFile ).orElse( config.Get( database_path ) );
						DatabaseLayout databaseLayout = DatabaseLayout.of( databaseDirectory );
						CheckDbState( databaseLayout, config );
						ZoneId logTimeZone = config.Get( GraphDatabaseSettings.db_timezone ).ZoneId;
						// Only output progress indicator if a console receives the output
						ProgressMonitorFactory progressMonitorFactory = ProgressMonitorFactory.NONE;
						if ( System.console() != null )
						{
							 progressMonitorFactory = ProgressMonitorFactory.textual( System.out );
						}
      
						ConsistencyCheckService.Result consistencyCheckResult = _consistencyCheckService.runFullConsistencyCheck( databaseLayout, config, progressMonitorFactory, FormattedLogProvider.withZoneId( logTimeZone ).toOutputStream( System.out ), fileSystem, verbose, reportDir.toFile(), new ConsistencyFlags(checkGraph, checkIndexes, checkIndexStructure, checkLabelScanStore, checkPropertyOwners) );
      
						if ( !consistencyCheckResult.Successful )
						{
							 throw new CommandFailed( format( "Inconsistencies found. See '%s' for details.", consistencyCheckResult.ReportFile() ) );
						}
					  }
			  }
			  catch ( Exception e ) when ( e is ConsistencyCheckIncompleteException || e is IOException )
			  {
					throw new CommandFailed( "Consistency checking failed." + e.Message, e );
			  }
		 }

		 private static Config LoadAdditionalConfig( Optional<Path> additionalConfigFile )
		 {
			  if ( additionalConfigFile.Present )
			  {
					try
					{
						 return Config.fromFile( additionalConfigFile.get() ).build();
					}
					catch ( Exception e )
					{
						 throw new System.ArgumentException( string.Format( "Could not read configuration file [{0}]", additionalConfigFile ), e );
					}
			  }

			  return Config.defaults();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkDbState(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config additionalConfiguration) throws org.neo4j.commandline.admin.CommandFailed
		 private static void CheckDbState( DatabaseLayout databaseLayout, Config additionalConfiguration )
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), JobScheduler jobScheduler = createInitialisedScheduler(), PageCache pageCache = ConfigurableStandalonePageCacheFactory.createPageCache(fileSystem, additionalConfiguration, jobScheduler) )
					  {
						assertRecoveryIsNotRequired( fileSystem, pageCache, additionalConfiguration, databaseLayout, new Monitors() );
					  }
			  }
			  catch ( RecoveryRequiredException rre )
			  {
					throw new CommandFailed( rre.Message );
			  }
			  catch ( Exception e )
			  {
					throw new CommandFailed( "Failure when checking for recovery state: '%s'." + e.Message, e );
			  }
		 }

		 private static Config LoadNeo4jConfig( Path homeDir, Path configDir, string databaseName, Config additionalConfig )
		 {
			  Config config = Config.fromFile( configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ) ).withHome( homeDir ).withConnectorsDisabled().withNoThrowOnFileLoadFailure().build();
			  config.Augment( additionalConfig );
			  config.Augment( GraphDatabaseSettings.active_database, databaseName );
			  return config;
		 }

		 public static Arguments Arguments()
		 {
			  return _arguments;
		 }
	}

}