using System;

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
namespace Neo4Net.Dbms.CommandLine
{

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using CompressionFormat = Neo4Net.Dbms.archive.CompressionFormat;
	using Dumper = Neo4Net.Dbms.archive.Dumper;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StoreLockException = Neo4Net.Kernel.StoreLockException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfigurableStandalonePageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory;
	using RecoveryRequiredChecker = Neo4Net.Kernel.impl.recovery.RecoveryRequiredChecker;
	using RecoveryRequiredException = Neo4Net.Kernel.impl.recovery.RecoveryRequiredException;
	using Validators = Neo4Net.Kernel.impl.util.Validators;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.commandline.Util.canonicalPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.commandline.arguments.common.Database.ARG_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.database_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.logical_logs_location;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;

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
//ORIGINAL LINE: public void execute(String[] args) throws Neo4Net.commandline.admin.IncorrectUsage, Neo4Net.commandline.admin.CommandFailed
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
					throw new CommandFailed( "the database is in use -- stop Neo4Net and try again", e );
			  }
			  catch ( IOException e )
			  {
					WrapIOException( e );
			  }
			  catch ( CannotWriteException e )
			  {
					throw new CommandFailed( "you do not have permission to dump the database -- is Neo4Net running as a different user?", e );
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
//ORIGINAL LINE: private void dump(String database, Neo4Net.io.layout.DatabaseLayout databaseLayout, java.nio.file.Path transactionalLogsDirectory, java.nio.file.Path archive) throws Neo4Net.commandline.admin.CommandFailed
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
//ORIGINAL LINE: private static void checkDbState(Neo4Net.io.layout.DatabaseLayout databaseLayout, Neo4Net.kernel.configuration.Config additionalConfiguration) throws Neo4Net.commandline.admin.CommandFailed
		 private static void CheckDbState( DatabaseLayout databaseLayout, Config additionalConfiguration )
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), IJobScheduler jobScheduler = createInitializedScheduler(), PageCache pageCache = ConfigurableStandalonePageCacheFactory.createPageCache(fileSystem, additionalConfiguration, jobScheduler) )
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
//ORIGINAL LINE: private static void wrapIOException(java.io.IOException e) throws Neo4Net.commandline.admin.CommandFailed
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