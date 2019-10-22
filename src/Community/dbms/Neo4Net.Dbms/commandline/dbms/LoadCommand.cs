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
namespace Neo4Net.CommandLine.dbms
{

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using OptionalBooleanArg = Neo4Net.CommandLine.Args.OptionalBooleanArg;
	using MandatoryCanonicalPath = Neo4Net.CommandLine.Args.Common.MandatoryCanonicalPath;
	using IncorrectFormat = Neo4Net.Dbms.archive.IncorrectFormat;
	using Loader = Neo4Net.Dbms.archive.Loader;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.Util.canonicalPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.Util.checkLock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.Util.isSameOrChildPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.Util.wrapIOException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.arguments.common.Database.ARG_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.database_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.logical_logs_location;

	public class LoadCommand : AdminCommand
	{

		 private static readonly Arguments _arguments = new Arguments().withArgument(new MandatoryCanonicalPath("from", "archive-path", "Path to archive created with the " + "dump command.")).withDatabase().withArgument(new OptionalBooleanArg("force", false, "If an existing database should be replaced."));

		 private readonly Path _homeDir;
		 private readonly Path _configDir;
		 private readonly Loader _loader;
		 public LoadCommand( Path homeDir, Path configDir, Loader loader )
		 {
			  requireNonNull( homeDir );
			  requireNonNull( configDir );
			  requireNonNull( loader );
			  this._homeDir = homeDir;
			  this._configDir = configDir;
			  this._loader = loader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws org.Neo4Net.commandline.admin.IncorrectUsage, org.Neo4Net.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
			  _arguments.parse( args );
			  Path archive = _arguments.getMandatoryPath( "from" );
			  string database = _arguments.get( ARG_DATABASE );
			  bool force = _arguments.getBoolean( "force" );

			  Config config = BuildConfig( database );

			  Path databaseDirectory = canonicalPath( GetDatabaseDirectory( config ) );
			  Path transactionLogsDirectory = canonicalPath( GetTransactionalLogsDirectory( config ) );

			  DeleteIfNecessary( databaseDirectory, transactionLogsDirectory, force );
			  Load( archive, database, databaseDirectory, transactionLogsDirectory );
		 }

		 private Path GetDatabaseDirectory( Config config )
		 {
			  return config.Get( database_path ).toPath();
		 }

		 private Path GetTransactionalLogsDirectory( Config config )
		 {
			  return config.Get( logical_logs_location ).toPath();
		 }

		 private Config BuildConfig( string databaseName )
		 {
			  return Config.fromFile( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ) ).withHome( _homeDir ).withConnectorsDisabled().withNoThrowOnFileLoadFailure().withSetting(GraphDatabaseSettings.active_database, databaseName).build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteIfNecessary(java.nio.file.Path databaseDirectory, java.nio.file.Path transactionLogsDirectory, boolean force) throws org.Neo4Net.commandline.admin.CommandFailed
		 private void DeleteIfNecessary( Path databaseDirectory, Path transactionLogsDirectory, bool force )
		 {
			  try
			  {
					if ( force )
					{
						 checkLock( DatabaseLayout.of( databaseDirectory.toFile() ).StoreLayout );
						 FileUtils.deletePathRecursively( databaseDirectory );
						 if ( !isSameOrChildPath( databaseDirectory, transactionLogsDirectory ) )
						 {
							  FileUtils.deletePathRecursively( transactionLogsDirectory );
						 }
					}
			  }
			  catch ( IOException e )
			  {
					wrapIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void load(java.nio.file.Path archive, String database, java.nio.file.Path databaseDirectory, java.nio.file.Path transactionLogsDirectory) throws org.Neo4Net.commandline.admin.CommandFailed
		 private void Load( Path archive, string database, Path databaseDirectory, Path transactionLogsDirectory )
		 {
			  try
			  {
					_loader.load( archive, databaseDirectory, transactionLogsDirectory );
			  }
			  catch ( NoSuchFileException e )
			  {
					if ( Paths.get( e.Message ).toAbsolutePath().Equals(archive.toAbsolutePath()) )
					{
						 throw new CommandFailed( "archive does not exist: " + archive, e );
					}
					wrapIOException( e );
			  }
			  catch ( FileAlreadyExistsException e )
			  {
					throw new CommandFailed( "database already exists: " + database, e );
			  }
			  catch ( AccessDeniedException e )
			  {
					throw new CommandFailed( "you do not have permission to load a database -- is Neo4Net running as a " + "different user?", e );
			  }
			  catch ( IOException e )
			  {
					wrapIOException( e );
			  }
			  catch ( IncorrectFormat incorrectFormat )
			  {
					throw new CommandFailed( "Not a valid Neo4Net archive: " + archive, incorrectFormat );
			  }
		 }

		 public static Arguments Arguments()
		 {
			  return _arguments;
		 }
	}

}