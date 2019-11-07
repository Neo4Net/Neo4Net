using System;
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
namespace Neo4Net.CommandLine.Admin.security
{

	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LegacyCredential = Neo4Net.Server.Security.Auth.LegacyCredential;
	using User = Neo4Net.Kernel.impl.security.User;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using CommunitySecurityModule = Neo4Net.Server.Security.Auth.CommunitySecurityModule;
	using FileUserRepository = Neo4Net.Server.Security.Auth.FileUserRepository;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;

	public class SetDefaultAdminCommand : AdminCommand
	{
		 public const string ADMIN_INI = "admin.ini";
		 public const string COMMAND_NAME = "set-default-admin";
		 private static readonly Arguments _arguments = new Arguments().withMandatoryPositionalArgument(0, "username");

		 private readonly Path _homeDir;
		 private readonly Path _configDir;
		 private OutsideWorld _outsideWorld;

		 internal SetDefaultAdminCommand( Path homeDir, Path configDir, OutsideWorld outsideWorld )
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
//ORIGINAL LINE: public void execute(String[] args) throws Neo4Net.commandline.admin.IncorrectUsage, Neo4Net.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
			  try
			  {
					DefaultAdmin = _arguments.parse( args ).get( 0 );
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
//ORIGINAL LINE: private void setDefaultAdmin(String username) throws Throwable
		 private string DefaultAdmin
		 {
			 set
			 {
				  FileSystemAbstraction fileSystem = _outsideWorld.fileSystem();
				  Config config = LoadNeo4NetConfig();
   
				  FileUserRepository users = CommunitySecurityModule.getUserRepository( config, NullLogProvider.Instance, fileSystem );
   
				  users.Init();
				  users.Start();
				  ISet<string> userNames = users.AllUsernames;
				  users.Stop();
				  users.Shutdown();
   
				  if ( userNames.Count == 0 )
				  {
						FileUserRepository initialUsers = CommunitySecurityModule.getInitialUserRepository( config, NullLogProvider.Instance, fileSystem );
						initialUsers.Init();
						initialUsers.Start();
						userNames = initialUsers.AllUsernames;
						initialUsers.Stop();
						initialUsers.Shutdown();
				  }
   
				  if ( !userNames.Contains( value ) )
				  {
						throw new CommandFailed( string.Format( "no such user: '{0}'", value ) );
				  }
   
				  File adminIniFile = new File( CommunitySecurityModule.getUserRepositoryFile( config ).ParentFile, ADMIN_INI );
				  if ( fileSystem.FileExists( adminIniFile ) )
				  {
						fileSystem.DeleteFile( adminIniFile );
				  }
				  UserRepository admins = new FileUserRepository( fileSystem, adminIniFile, NullLogProvider.Instance );
				  admins.Init();
				  admins.Start();
				  admins.Create( ( new User.Builder( value, LegacyCredential.INACCESSIBLE ) ).build() );
				  admins.Stop();
				  admins.Shutdown();
   
				  _outsideWorld.stdOutLine( "default admin user set to '" + value + "'" );
			 }
		 }

		 internal virtual Config LoadNeo4NetConfig()
		 {
			  return Config.fromFile( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ) ).withHome( _homeDir ).withConnectorsDisabled().withNoThrowOnFileLoadFailure().build();
		 }
	}

}