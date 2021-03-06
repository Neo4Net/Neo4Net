﻿using System;

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
namespace Org.Neo4j.Commandline.admin.security
{

	using Arguments = Org.Neo4j.Commandline.arguments.Arguments;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LegacyCredential = Org.Neo4j.Server.Security.Auth.LegacyCredential;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using CommunitySecurityModule = Org.Neo4j.Server.Security.Auth.CommunitySecurityModule;
	using FileUserRepository = Org.Neo4j.Server.Security.Auth.FileUserRepository;
	using Org.Neo4j.Server.Security.Auth;
	using UTF8 = Org.Neo4j.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.UserManager_Fields.INITIAL_PASSWORD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.UserManager_Fields.INITIAL_USER_NAME;

	public class SetInitialPasswordCommand : AdminCommand
	{

		 private static readonly Arguments _arguments = new Arguments().withMandatoryPositionalArgument(0, "password");

		 private readonly Path _homeDir;
		 private readonly Path _configDir;
		 private OutsideWorld _outsideWorld;

		 internal SetInitialPasswordCommand( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
			  this._outsideWorld = outsideWorld;
		 }

		 public static Arguments Arguments()
		 {
			  return _arguments;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
			  try
			  {
					Password = _arguments.parse( args ).get( 0 );
			  }
			  catch ( Exception e ) when ( e is IncorrectUsage || e is CommandFailed )
			  {
					throw e;
			  }
			  catch ( Exception throwable )
			  {
					throw new CommandFailed( throwable.Message, new Exception( throwable ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setPassword(String password) throws Throwable
		 private string Password
		 {
			 set
			 {
				  Config config = LoadNeo4jConfig();
				  FileSystemAbstraction fileSystem = _outsideWorld.fileSystem();
   
				  if ( RealUsersExist( config ) )
				  {
						File authFile = CommunitySecurityModule.getUserRepositoryFile( config );
						throw new CommandFailed( RealUsersExistErrorMsg( fileSystem, authFile ) );
				  }
				  else
				  {
						File file = CommunitySecurityModule.getInitialUserRepositoryFile( config );
						if ( fileSystem.FileExists( file ) )
						{
							 fileSystem.DeleteFile( file );
						}
   
						FileUserRepository userRepository = new FileUserRepository( fileSystem, file, NullLogProvider.Instance );
						userRepository.Start();
						userRepository.create(new User.Builder(INITIAL_USER_NAME, LegacyCredential.forPassword(UTF8.encode(value)))
											 .withRequiredPasswordChange( false ).build());
						userRepository.Shutdown();
						_outsideWorld.stdOutLine( "Changed password for user '" + INITIAL_USER_NAME + "'." );
				  }
			 }
		 }

		 private bool RealUsersExist( Config config )
		 {
			  bool result = false;
			  File authFile = CommunitySecurityModule.getUserRepositoryFile( config );

			  if ( _outsideWorld.fileSystem().fileExists(authFile) )
			  {
					result = true;

					// Check if it only contains the default neo4j user
					FileUserRepository userRepository = new FileUserRepository( _outsideWorld.fileSystem(), authFile, NullLogProvider.Instance );
					try
					{
							using ( Lifespan life = new Lifespan( userRepository ) )
							{
							 ListSnapshot<User> users = userRepository.PersistedSnapshot;
							 if ( users.Values().Count == 1 )
							 {
								  User user = users.Values()[0];
								  if ( INITIAL_USER_NAME.Equals( user.Name() ) && user.Credentials().matchesPassword(INITIAL_PASSWORD) )
								  {
										// We allow overwriting an unmodified default neo4j user
										result = false;
								  }
							 }
							}
					}
					catch ( IOException )
					{
						 // Do not allow overwriting if we had a problem reading the file
					}
			  }
			  return result;
		 }

		 private string RealUsersExistErrorMsg( FileSystemAbstraction fileSystem, File authFile )
		 {
			  string files;
			  File parentFile = authFile.ParentFile;
			  File roles = new File( parentFile, "roles" );

			  if ( fileSystem.FileExists( roles ) )
			  {
					files = "`auth` and `roles` files";
			  }
			  else
			  {
					files = "`auth` file";
			  }

			  return "the provided initial password was not set because existing Neo4j users were detected at `" + authFile.AbsolutePath + "`. Please remove the existing " + files + " if you want to reset your database " +
						 "to only have a default user with the provided password.";
		 }

		 internal virtual Config LoadNeo4jConfig()
		 {
			  return Config.fromFile( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ).toFile() ).withHome(_homeDir.toFile()).withNoThrowOnFileLoadFailure().withConnectorsDisabled().build();
		 }
	}

}