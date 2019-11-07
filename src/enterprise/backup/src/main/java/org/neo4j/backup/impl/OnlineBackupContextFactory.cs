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

namespace Neo4Net.backup.impl
{

	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using MandatoryNamedArg = Neo4Net.CommandLine.Args.MandatoryNamedArg;
	using OptionalBooleanArg = Neo4Net.CommandLine.Args.OptionalBooleanArg;
	using OptionalNamedArg = Neo4Net.CommandLine.Args.OptionalNamedArg;
	using MandatoryCanonicalPath = Neo4Net.CommandLine.Args.Common.MandatoryCanonicalPath;
	using OptionalCanonicalPath = Neo4Net.CommandLine.Args.Common.OptionalCanonicalPath;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using TimeUtil = Neo4Net.Helpers.TimeUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using Converters = Neo4Net.Kernel.impl.util.Converters;
	using OptionalHostnamePort = Neo4Net.Kernel.impl.util.OptionalHostnamePort;

	internal class OnlineBackupContextFactory
	{
		 internal const string ARG_NAME_BACKUP_DIRECTORY = "backup-dir";
		 internal const string ARG_DESC_BACKUP_DIRECTORY = "Directory to place backup in.";

		 internal const string ARG_NAME_BACKUP_NAME = "name";
		 internal const string ARG_DESC_BACKUP_NAME = "Name of backup. If a backup with this name already exists an incremental backup will be attempted.";

		 internal const string ARG_NAME_BACKUP_SOURCE = "from";
		 internal const string ARG_DESC_BACKUP_SOURCE = "Host and port of Neo4Net.";
		 internal const string ARG_DFLT_BACKUP_SOURCE = "localhost:6362";

		 internal const string ARG_NAME_PROTO_OVERRIDE = "protocol";
		 internal const string ARG_DESC_PROTO_OVERRIDE = "Preferred backup protocol";
		 internal const string ARG_DFLT_PROTO_OVERRIDE = "any";

		 internal const string ARG_NAME_TIMEOUT = "timeout";
		 internal const string ARG_DESC_TIMEOUT = "Timeout in the form <time>[ms|s|m|h], where the default unit is seconds.";
		 internal const string ARG_DFLT_TIMEOUT = "20m";

		 internal const string ARG_NAME_PAGECACHE = "pagecache";
		 internal const string ARG_DESC_PAGECACHE = "The size of the page cache to use for the backup process.";
		 internal const string ARG_DFLT_PAGECACHE = "8m";

		 internal const string ARG_NAME_REPORT_DIRECTORY = "cc-report-dir";
		 internal const string ARG_DESC_REPORT_DIRECTORY = "Directory where consistency report will be written.";

		 internal const string ARG_NAME_ADDITIONAL_CONFIG_DIR = "additional-config";
		 internal const string ARG_DESC_ADDITIONAL_CONFIG_DIR = "Configuration file to supply additional configuration in. This argument is DEPRECATED.";

		 internal const string ARG_NAME_FALLBACK_FULL = "fallback-to-full";
		 internal const string ARG_DESC_FALLBACK_FULL = "If an incremental backup fails backup will move the old backup to <name>.err.<N> and fallback to a full " + "backup instead.";

		 internal const string ARG_NAME_CHECK_CONSISTENCY = "check-consistency";
		 internal const string ARG_DESC_CHECK_CONSISTENCY = "If a consistency check should be made.";

		 internal const string ARG_NAME_CHECK_GRAPH = "cc-graph";
		 internal const string ARG_DESC_CHECK_GRAPH = "Perform consistency checks between nodes, relationships, properties, types and tokens.";

		 internal const string ARG_NAME_CHECK_INDEXES = "cc-indexes";
		 internal const string ARG_DESC_CHECK_INDEXES = "Perform consistency checks on indexes.";

		 internal const string ARG_NAME_CHECK_LABELS = "cc-label-scan-store";
		 internal const string ARG_DESC_CHECK_LABELS = "Perform consistency checks on the label scan store.";

		 internal const string ARG_NAME_CHECK_OWNERS = "cc-property-owners";
		 internal const string ARG_DESC_CHECK_OWNERS = "Perform additional consistency checks on property ownership. This check is *very* expensive in time and " + "memory.";

		 private readonly Path _homeDir;
		 private readonly Path _configDir;

		 internal OnlineBackupContextFactory( Path homeDir, Path configDir )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
		 }

		 public static Arguments Arguments()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  string argExampleProtoOverride = ( string ) Stream.of( SelectedBackupProtocol.values() ).map(SelectedBackupProtocol::getName).sorted().collect(Collectors.joining("|"));

			  return ( new Arguments() ).withArgument(new MandatoryCanonicalPath(ARG_NAME_BACKUP_DIRECTORY, "backup-path", ARG_DESC_BACKUP_DIRECTORY)).withArgument(new MandatoryNamedArg(ARG_NAME_BACKUP_NAME, "graph.db-backup", ARG_DESC_BACKUP_NAME)).withArgument(new OptionalNamedArg(ARG_NAME_BACKUP_SOURCE, "address", ARG_DFLT_BACKUP_SOURCE, ARG_DESC_BACKUP_SOURCE)).withArgument(new OptionalNamedArg(ARG_NAME_PROTO_OVERRIDE, argExampleProtoOverride, ARG_DFLT_PROTO_OVERRIDE, ARG_DESC_PROTO_OVERRIDE)).withArgument(new OptionalBooleanArg(ARG_NAME_FALLBACK_FULL, true, ARG_DESC_FALLBACK_FULL)).withArgument(new OptionalNamedArg(ARG_NAME_TIMEOUT, "timeout", ARG_DFLT_TIMEOUT, ARG_DESC_TIMEOUT)).withArgument(new OptionalNamedArg(ARG_NAME_PAGECACHE, "8m", ARG_DFLT_PAGECACHE, ARG_DESC_PAGECACHE)).withArgument(new OptionalBooleanArg(ARG_NAME_CHECK_CONSISTENCY, true, ARG_DESC_CHECK_CONSISTENCY)).withArgument(new OptionalCanonicalPath(ARG_NAME_REPORT_DIRECTORY, "directory", ".", ARG_DESC_REPORT_DIRECTORY)).withArgument(new OptionalCanonicalPath(ARG_NAME_ADDITIONAL_CONFIG_DIR, "config-file-path", "", ARG_DESC_ADDITIONAL_CONFIG_DIR)).withArgument(new OptionalBooleanArg(ARG_NAME_CHECK_GRAPH, true, ARG_DESC_CHECK_GRAPH)).withArgument(new OptionalBooleanArg(ARG_NAME_CHECK_INDEXES, true, ARG_DESC_CHECK_INDEXES)).withArgument(new OptionalBooleanArg(ARG_NAME_CHECK_LABELS, true, ARG_DESC_CHECK_LABELS)).withArgument(new OptionalBooleanArg(ARG_NAME_CHECK_OWNERS, false, ARG_DESC_CHECK_OWNERS));
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public OnlineBackupContext createContext(String... args) throws Neo4Net.commandline.admin.IncorrectUsage, Neo4Net.commandline.admin.CommandFailed
		 public virtual OnlineBackupContext CreateContext( params string[] args )
		 {
			  try
			  {
					Arguments arguments = arguments();
					arguments.Parse( args );

					OptionalHostnamePort address = Converters.toOptionalHostnamePortFromRawAddress( arguments.Get( ARG_NAME_BACKUP_SOURCE ) );
					Path folder = this.GetBackupDirectory( arguments );
					string name = arguments.Get( ARG_NAME_BACKUP_NAME );
					bool fallbackToFull = arguments.GetBoolean( ARG_NAME_FALLBACK_FULL );
					bool doConsistencyCheck = arguments.GetBoolean( ARG_NAME_CHECK_CONSISTENCY );
					long timeout = ( long? ) arguments.Get( ARG_NAME_TIMEOUT, TimeUtil.parseTimeMillis ).Value;
					SelectedBackupProtocol selectedBackupProtocol = SelectedBackupProtocol.fromUserInput( arguments.Get( ARG_NAME_PROTO_OVERRIDE ) );
					string pagecacheMemory = arguments.Get( ARG_NAME_PAGECACHE );
					Optional<Path> additionalConfig = arguments.GetOptionalPath( ARG_NAME_ADDITIONAL_CONFIG_DIR );
					Path reportDir = ( Path ) arguments.GetOptionalPath( ARG_NAME_REPORT_DIRECTORY ).orElseThrow(() =>
					{
																																				return new System.ArgumentException( ARG_NAME_REPORT_DIRECTORY + " must be a path" );
					});
					OnlineBackupRequiredArguments requiredArguments = new OnlineBackupRequiredArguments( address, folder, name, selectedBackupProtocol, fallbackToFull, doConsistencyCheck, timeout, reportDir );

					Path configFile = _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME );
					Config.Builder builder = Config.fromFile( configFile );
					Path logPath = requiredArguments.ResolvedLocationFromName;

					Config config = builder.WithHome( this._homeDir ).withSetting( GraphDatabaseSettings.logical_logs_location, logPath.ToString() ).withConnectorsDisabled().withNoThrowOnFileLoadFailure().build();

					additionalConfig.map( this.loadAdditionalConfigFile ).ifPresent( config.augment );

					// We only replace the page cache memory setting.
					// Any other custom page swapper, etc. settings are preserved and used.
					config.Augment( GraphDatabaseSettings.pagecache_memory, pagecacheMemory );

					// Disable prometheus to avoid binding exceptions
					config.Augment( "metrics.prometheus.enabled", Settings.FALSE );

					// Build consistency-checker configuration.
					// Note: We can remove the loading from config file in 4.0.
					System.Func<string, Setting<bool>, bool> oneOf = ( a, s ) => arguments.Has( a ) ? arguments.GetBoolean( a ) : config.Get( s );

					ConsistencyFlags consistencyFlags = new ConsistencyFlags( config );

					return new OnlineBackupContext( requiredArguments, config, consistencyFlags );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }
			  catch ( UncheckedIOException e )
			  {
					throw new CommandFailed( e.Message, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.file.Path getBackupDirectory(Neo4Net.commandline.arguments.Arguments arguments) throws Neo4Net.commandline.admin.CommandFailed
		 private Path GetBackupDirectory( Arguments arguments )
		 {
			  Path path = arguments.GetMandatoryPath( ARG_NAME_BACKUP_DIRECTORY );
			  try
			  {
					return path.toRealPath();
			  }
			  catch ( IOException )
			  {
					throw new CommandFailed( string.Format( "Directory '{0}' does not exist.", path ) );
			  }
		 }

		 private Config LoadAdditionalConfigFile( Path path )
		 {
			  return Config.fromFile( path ).build();
		 }
	}

}