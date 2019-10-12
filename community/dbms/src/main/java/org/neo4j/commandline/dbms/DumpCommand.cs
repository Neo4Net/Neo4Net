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
namespace Org.Neo4j.Commandline.dbms
{

	using AdminCommand = Org.Neo4j.Commandline.admin.AdminCommand;
	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using Arguments = Org.Neo4j.Commandline.arguments.Arguments;
	using CompressionFormat = Org.Neo4j.Dbms.archive.CompressionFormat;
	using Dumper = Org.Neo4j.Dbms.archive.Dumper;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using StoreLockException = Org.Neo4j.Kernel.StoreLockException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfigurableStandalonePageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory;
	using RecoveryRequiredChecker = Org.Neo4j.Kernel.impl.recovery.RecoveryRequiredChecker;
	using RecoveryRequiredException = Org.Neo4j.Kernel.impl.recovery.RecoveryRequiredException;
	using Validators = Org.Neo4j.Kernel.impl.util.Validators;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.canonicalPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.arguments.common.Database.ARG_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.database_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logical_logs_location;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	public class DumpCommand : AdminCommand
	{
		 private static readonly Arguments _arguments = new Arguments().withDatabase().withTo("Destination (file or folder) of database dump.");

		 private readonly Path _homeDir;
		 private readonly Path _configDir;
		 private readonly Dumper _dumper;

		 public DumpCommand( Path homeDir, Path configDir, Dumper dumper )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
			  this._dumper = dumper;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
			  string database = _arguments.parse( args ).get( ARG_DATABASE );
			  Path archive = CalculateArchive( database, _arguments.getMandatoryPath( "to" ) );

			  Config config = BuildConfig( database );
			  Path databaseDirectory = canonicalPath( GetDatabaseDirectory( config ) );
			  DatabaseLayout databaseLayout = DatabaseLayout.of( databaseDirectory.toFile() );
			  Path transactionLogsDirectory = canonicalPath( GetTransactionalLogsDirectory( config ) );

			  try
			  {
					Validators.CONTAINS_EXISTING_DATABASE.validate( databaseLayout.DatabaseDirectory() );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new CommandFailed( "database does not exist: " + database, e );
			  }

			  try
			  {
					  using ( System.IDisposable ignored = StoreLockChecker.Check( databaseLayout.StoreLayout ) )
					  {
						CheckDbState( databaseLayout, config );
						Dump( database, databaseLayout, transactionLogsDirectory, archive );
					  }
			  }
			  catch ( StoreLockException e )
			  {
					throw new CommandFailed( "the database is in use -- stop Neo4j and try again", e );
			  }
			  catch ( IOException e )
			  {
					WrapIOException( e );
			  }
			  catch ( CannotWriteException e )
			  {
					throw new CommandFailed( "you do not have permission to dump the database -- is Neo4j running as a different user?", e );
			  }
		 }

		 private static Path GetDatabaseDirectory( Config config )
		 {
			  return config.Get( database_path ).toPath();
		 }

		 private static Path GetTransactionalLogsDirectory( Config config )
		 {
			  return config.Get( logical_logs_location ).toPath();
		 }

		 private Config BuildConfig( string databaseName )
		 {
			  return Config.fromFile( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ) ).withHome( _homeDir ).withConnectorsDisabled().withNoThrowOnFileLoadFailure().withSetting(GraphDatabaseSettings.active_database, databaseName).build();
		 }

		 private static Path CalculateArchive( string database, Path to )
		 {
			  return Files.isDirectory( to ) ? to.resolve( database + ".dump" ) : to;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dump(String database, org.neo4j.io.layout.DatabaseLayout databaseLayout, java.nio.file.Path transactionalLogsDirectory, java.nio.file.Path archive) throws org.neo4j.commandline.admin.CommandFailed
		 private void Dump( string database, DatabaseLayout databaseLayout, Path transactionalLogsDirectory, Path archive )
		 {
			  Path databasePath = databaseLayout.DatabaseDirectory().toPath();
			  try
			  {
					File storeLockFile = databaseLayout.StoreLayout.storeLockFile();
					System.Predicate<Path> pathPredicate = path => Objects.Equals( path.FileName.ToString(), storeLockFile.Name );
					_dumper.dump( databasePath, transactionalLogsDirectory, archive, CompressionFormat.ZSTD, pathPredicate );
			  }
			  catch ( FileAlreadyExistsException e )
			  {
					throw new CommandFailed( "archive already exists: " + e.Message, e );
			  }
			  catch ( NoSuchFileException e )
			  {
					if ( Paths.get( e.Message ).toAbsolutePath().Equals(databasePath) )
					{
						 throw new CommandFailed( "database does not exist: " + database, e );
					}
					WrapIOException( e );
			  }
			  catch ( IOException e )
			  {
					WrapIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkDbState(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config additionalConfiguration) throws org.neo4j.commandline.admin.CommandFailed
		 private static void CheckDbState( DatabaseLayout databaseLayout, Config additionalConfiguration )
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), JobScheduler jobScheduler = createInitialisedScheduler(), PageCache pageCache = ConfigurableStandalonePageCacheFactory.createPageCache(fileSystem, additionalConfiguration, jobScheduler) )
					  {
						RecoveryRequiredChecker.assertRecoveryIsNotRequired( fileSystem, pageCache, additionalConfiguration, databaseLayout, new Monitors() );
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void wrapIOException(java.io.IOException e) throws org.neo4j.commandline.admin.CommandFailed
		 private static void WrapIOException( IOException e )
		 {
			  throw new CommandFailed( format( "unable to dump database: %s: %s", e.GetType().Name, e.Message ), e );
		 }

		 public static Arguments Arguments()
		 {
			  return _arguments;
		 }
	}

}