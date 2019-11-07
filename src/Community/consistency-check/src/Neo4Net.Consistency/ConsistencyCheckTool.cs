﻿using System;
using System.Collections.Generic;

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
namespace Neo4Net.Consistency
{

	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;
	using Strings = Neo4Net.Helpers.Strings;
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
	using LogProvider = Neo4Net.Logging.LogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Args.jarUsage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Strings.joinAsLines;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.recovery.RecoveryRequiredChecker.assertRecoveryIsNotRequired;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;

	public class ConsistencyCheckTool
	{
		 private const string CONFIG = "config";
		 private const string VERBOSE = "v";

		 public static void Main( string[] args )
		 {
			  try
			  {
					Console.Error.WriteLine( "WARNING: ConsistencyCheckTool is deprecated and support for it will be" + "removed in a future version of Neo4Net. Please use Neo4Net-admin check-consistency." );
					RunConsistencyCheckTool( args, System.out, System.err );
			  }
			  catch ( ToolFailureException e )
			  {
					e.ExitTool();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static ConsistencyCheckService.Result runConsistencyCheckTool(String[] args, java.io.PrintStream outStream, java.io.PrintStream errStream) throws ToolFailureException
		 public static ConsistencyCheckService.Result RunConsistencyCheckTool( string[] args, PrintStream outStream, PrintStream errStream )
		 {
			  FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction();
			  try
			  {
					ConsistencyCheckTool tool = new ConsistencyCheckTool( new ConsistencyCheckService(), fileSystem, outStream, errStream );
					return tool.Run( args );
			  }
			  finally
			  {
					try
					{
						 fileSystem.Dispose();
					}
					catch ( IOException )
					{
						 Console.Error.Write( "Failure during file system shutdown." );
					}
			  }
		 }

		 private readonly ConsistencyCheckService _consistencyCheckService;
		 private readonly PrintStream _systemOut;
		 private readonly PrintStream _systemError;
		 private readonly FileSystemAbstraction _fs;

		 internal ConsistencyCheckTool( ConsistencyCheckService consistencyCheckService, FileSystemAbstraction fs, PrintStream systemOut, PrintStream systemError )
		 {
			  this._consistencyCheckService = consistencyCheckService;
			  this._fs = fs;
			  this._systemOut = systemOut;
			  this._systemError = systemError;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConsistencyCheckService.Result run(String... args) throws ToolFailureException
		 internal virtual ConsistencyCheckService.Result Run( params string[] args )
		 {
			  Args arguments = Args.withFlags( VERBOSE ).parse( args );

			  File storeDir = DetermineStoreDirectory( arguments );
			  Config tuningConfiguration = ReadConfiguration( arguments );
			  bool verbose = IsVerbose( arguments );

			  DatabaseLayout databaseLayout = DatabaseLayout.of( storeDir );
			  CheckDbState( databaseLayout, tuningConfiguration );

			  ZoneId logTimeZone = tuningConfiguration.Get( GraphDatabaseSettings.db_timezone ).ZoneId;
			  LogProvider logProvider = FormattedLogProvider.withZoneId( logTimeZone ).toOutputStream( _systemOut );
			  try
			  {
					return _consistencyCheckService.runFullConsistencyCheck( databaseLayout, tuningConfiguration, ProgressMonitorFactory.textual( _systemError ), logProvider, _fs, verbose, new ConsistencyFlags( tuningConfiguration ) );
			  }
			  catch ( ConsistencyCheckIncompleteException e )
			  {
					throw new ToolFailureException( "Check aborted due to exception", e );
			  }
		 }

		 private static bool IsVerbose( Args arguments )
		 {
			  return arguments.GetBoolean( VERBOSE, false, true ).Value;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkDbState(Neo4Net.io.layout.DatabaseLayout databaseLayout, Neo4Net.kernel.configuration.Config tuningConfiguration) throws ToolFailureException
		 private void CheckDbState( DatabaseLayout databaseLayout, Config tuningConfiguration )
		 {
			  try
			  {
					  using ( IJobScheduler jobScheduler = createInitializedScheduler(), PageCache pageCache = ConfigurableStandalonePageCacheFactory.createPageCache(_fs, tuningConfiguration, jobScheduler) )
					  {
						assertRecoveryIsNotRequired( _fs, pageCache, tuningConfiguration, databaseLayout, new Monitors() );
					  }
			  }
			  catch ( RecoveryRequiredException rre )
			  {
					throw new ToolFailureException( rre.Message );
			  }
			  catch ( Exception e )
			  {
					_systemError.printf( "Failure when checking for recovery state: '%s', continuing as normal.%n", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File determineStoreDirectory(Neo4Net.helpers.Args arguments) throws ToolFailureException
		 private File DetermineStoreDirectory( Args arguments )
		 {
			  IList<string> unprefixedArguments = arguments.Orphans();
			  if ( unprefixedArguments.Count != 1 )
			  {
					throw new ToolFailureException( Usage() );
			  }
			  File storeDir = new File( unprefixedArguments[0] );
			  if ( !storeDir.Directory )
			  {
					throw new ToolFailureException( Strings.joinAsLines( string.Format( "'{0}' is not a directory", storeDir ) ) + Usage() );
			  }
			  return storeDir;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Neo4Net.kernel.configuration.Config readConfiguration(Neo4Net.helpers.Args arguments) throws ToolFailureException
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

		 private string Usage()
		 {
			  return joinAsLines( jarUsage( this.GetType(), " [-config <Neo4Net.conf>] [-v] <storedir>" ), "WHERE:   -config <filename>  Is the location of an optional properties file", "                             containing tuning parameters for the consistency check.", "         -v                  Produce execution output.", "         <storedir>          Is the path to the store to check." );
		 }

		 public class ToolFailureException : Exception
		 {
			  internal ToolFailureException( string message ) : base( message )
			  {
			  }

			  internal ToolFailureException( string message, Exception cause ) : base( message, cause )
			  {
			  }

			  public virtual void ExitTool()
			  {
					PrintErrorMessage();
					Exit();
			  }

			  public virtual void PrintErrorMessage()
			  {
					Console.Error.WriteLine( Message );
					if ( Cause != null )
					{
						 Cause.printStackTrace( System.err );
					}
			  }
		 }

		 private static void Exit()
		 {
			  Environment.Exit( 1 );
		 }
	}

}