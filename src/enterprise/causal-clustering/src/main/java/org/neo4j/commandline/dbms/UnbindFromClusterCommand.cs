using System;

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
namespace Neo4Net.Dbms.CommandLine
{

	using ClusterStateDirectory = Neo4Net.causalclustering.core.state.ClusterStateDirectory;
	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using StoreLockException = Neo4Net.Kernel.StoreLockException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Validators = Neo4Net.Kernel.impl.util.Validators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.arguments.common.Database.ARG_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Config.fromFile;

	public class UnbindFromClusterCommand : AdminCommand
	{
		 private static readonly Arguments _arguments = new Arguments().withDatabase();
		 private Path _homeDir;
		 private Path _configDir;
		 private OutsideWorld _outsideWorld;

		 internal UnbindFromClusterCommand( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
			  this._outsideWorld = outsideWorld;
		 }

		 internal static Arguments Arguments()
		 {
			  return _arguments;
		 }

		 private static Config LoadNeo4NetConfig( Path homeDir, Path configDir, string databaseName )
		 {
			  return fromFile( configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ) ).withSetting( GraphDatabaseSettings.active_database, databaseName ).withHome( homeDir ).withConnectorsDisabled().withNoThrowOnFileLoadFailure().build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws org.Neo4Net.commandline.admin.IncorrectUsage, org.Neo4Net.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
			  try
			  {
					Config config = LoadNeo4NetConfig( _homeDir, _configDir, _arguments.parse( args ).get( ARG_DATABASE ) );
					File dataDirectory = config.Get( GraphDatabaseSettings.data_directory );
					Path pathToSpecificDatabase = config.Get( GraphDatabaseSettings.database_path ).toPath();

					bool hasDatabase = true;
					try
					{
						 Validators.CONTAINS_EXISTING_DATABASE.validate( pathToSpecificDatabase.toFile() );
					}
					catch ( System.ArgumentException )
					{
						 // No such database, it must have been deleted. Must be OK to delete cluster state
						 hasDatabase = false;
					}

					if ( hasDatabase )
					{
						 ConfirmTargetDirectoryIsWritable( DatabaseLayout.of( pathToSpecificDatabase.toFile() ).StoreLayout );
					}

					ClusterStateDirectory clusterStateDirectory = ClusterStateDirectory.withoutInitializing( dataDirectory );

					if ( _outsideWorld.fileSystem().fileExists(clusterStateDirectory.Get()) )
					{
						 DeleteClusterStateIn( clusterStateDirectory.Get() );
					}
					else
					{
						 _outsideWorld.stdErrLine( "This instance was not bound. No work performed." );
					}
			  }
			  catch ( StoreLockException e )
			  {
					throw new CommandFailed( "Database is currently locked. Please shutdown Neo4Net.", e );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }
			  catch ( Exception e ) when ( e is UnbindFailureException || e is CannotWriteException || e is IOException )
			  {
					throw new CommandFailed( e.Message, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void confirmTargetDirectoryIsWritable(org.Neo4Net.io.layout.StoreLayout storeLayout) throws CannotWriteException, java.io.IOException
		 private static void ConfirmTargetDirectoryIsWritable( StoreLayout storeLayout )
		 {
			  using ( System.IDisposable ignored = StoreLockChecker.Check( storeLayout ) )
			  {
					// empty
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteClusterStateIn(java.io.File target) throws UnbindFailureException
		 private void DeleteClusterStateIn( File target )
		 {
			  try
			  {
					_outsideWorld.fileSystem().deleteRecursively(target);
			  }
			  catch ( IOException e )
			  {
					throw new UnbindFailureException( this, e );
			  }
		 }

		 private class UnbindFailureException : Exception
		 {
			 private readonly UnbindFromClusterCommand _outerInstance;

			  internal UnbindFailureException( UnbindFromClusterCommand outerInstance, Exception e ) : base( e )
			  {
				  this._outerInstance = outerInstance;
			  }
		 }
	}

}