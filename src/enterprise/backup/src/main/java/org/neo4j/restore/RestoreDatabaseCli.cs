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
namespace Neo4Net.restore
{

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using MandatoryNamedArg = Neo4Net.CommandLine.Args.MandatoryNamedArg;
	using OptionalBooleanArg = Neo4Net.CommandLine.Args.OptionalBooleanArg;
	using Database = Neo4Net.CommandLine.Args.Common.Database;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.arguments.common.Database.ARG_DATABASE;

	public class RestoreDatabaseCli : AdminCommand
	{
		 private static readonly Arguments _arguments = new Arguments().withArgument(new MandatoryNamedArg("from", "backup-directory", "Path to backup to restore from.")).withDatabase().withArgument(new OptionalBooleanArg("force", false, "If an existing database should be replaced."));
		 private readonly Path _homeDir;
		 private readonly Path _configDir;

		 public RestoreDatabaseCli( Path homeDir, Path configDir )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
		 }

		 private static Config LoadNeo4NetConfig( Path homeDir, Path configDir, string databaseName )
		 {
			  return Config.fromFile( configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ) ).withHome( homeDir ).withSetting( GraphDatabaseSettings.active_database, databaseName ).withConnectorsDisabled().build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] incomingArguments) throws org.Neo4Net.commandline.admin.IncorrectUsage, org.Neo4Net.commandline.admin.CommandFailed
		 public override void Execute( string[] incomingArguments )
		 {
			  string databaseName;
			  string fromPath;
			  bool forceOverwrite;

			  try
			  {
					databaseName = _arguments.parse( incomingArguments ).get( ARG_DATABASE );
					fromPath = _arguments.get( "from" );
					forceOverwrite = _arguments.getBoolean( "force" );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }

			  Config config = LoadNeo4NetConfig( _homeDir, _configDir, databaseName );

			  try
			  {
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
					  {
						RestoreDatabaseCommand restoreDatabaseCommand = new RestoreDatabaseCommand( fileSystem, new File( fromPath ), config, databaseName, forceOverwrite );
						restoreDatabaseCommand.Execute();
					  }
			  }
			  catch ( IOException e )
			  {
					throw new CommandFailed( "Failed to restore database", e );
			  }
		 }

		 public static Arguments Arguments()
		 {
			  return _arguments;
		 }
	}

}