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
namespace Org.Neo4j.Server.Security.Auth
{

	using DatabaseManagementSystemSettings = Org.Neo4j.Dbms.DatabaseManagementSystemSettings;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Service = Org.Neo4j.Helpers.Service;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using PasswordPolicy = Org.Neo4j.Kernel.api.security.PasswordPolicy;
	using SecurityModule = Org.Neo4j.Kernel.api.security.SecurityModule;
	using UserManager = Org.Neo4j.Kernel.api.security.UserManager;
	using UserManagerSupplier = Org.Neo4j.Kernel.api.security.UserManagerSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(SecurityModule.class) public class CommunitySecurityModule extends org.neo4j.kernel.api.security.SecurityModule
	public class CommunitySecurityModule : SecurityModule
	{
		 public const string COMMUNITY_SECURITY_MODULE_ID = "community-security-module";

		 private BasicAuthManager _authManager;

		 public CommunitySecurityModule() : base(COMMUNITY_SECURITY_MODULE_ID)
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setup(Dependencies dependencies) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void Setup( Dependencies dependencies )
		 {
			  Config config = dependencies.Config();
			  Procedures procedures = dependencies.Procedures();
			  LogProvider logProvider = dependencies.LogService().UserLogProvider;
			  FileSystemAbstraction fileSystem = dependencies.FileSystem();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UserRepository userRepository = getUserRepository(config, logProvider, fileSystem);
			  UserRepository userRepository = GetUserRepository( config, logProvider, fileSystem );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UserRepository initialUserRepository = getInitialUserRepository(config, logProvider, fileSystem);
			  UserRepository initialUserRepository = GetInitialUserRepository( config, logProvider, fileSystem );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.api.security.PasswordPolicy passwordPolicy = new BasicPasswordPolicy();
			  PasswordPolicy passwordPolicy = new BasicPasswordPolicy();

			  _authManager = new BasicAuthManager( userRepository, passwordPolicy, Clocks.systemClock(), initialUserRepository, config );

			  Life.add( dependencies.DependencySatisfier().satisfyDependency(_authManager) );

			  procedures.RegisterComponent( typeof( UserManager ), ctx => _authManager, false );
			  procedures.RegisterProcedure( typeof( AuthProcedures ) );
		 }

		 public override AuthManager AuthManager()
		 {
			  return _authManager;
		 }

		 public override UserManagerSupplier UserManagerSupplier()
		 {
			  return _authManager;
		 }

		 public const string USER_STORE_FILENAME = "auth";
		 public const string INITIAL_USER_STORE_FILENAME = "auth.ini";

		 public static FileUserRepository GetUserRepository( Config config, LogProvider logProvider, FileSystemAbstraction fileSystem )
		 {
			  return new FileUserRepository( fileSystem, GetUserRepositoryFile( config ), logProvider );
		 }

		 public static FileUserRepository GetInitialUserRepository( Config config, LogProvider logProvider, FileSystemAbstraction fileSystem )
		 {
			  return new FileUserRepository( fileSystem, GetInitialUserRepositoryFile( config ), logProvider );
		 }

		 public static File GetUserRepositoryFile( Config config )
		 {
			  return GetUserRepositoryFile( config, USER_STORE_FILENAME );
		 }

		 public static File GetInitialUserRepositoryFile( Config config )
		 {
			  return GetUserRepositoryFile( config, INITIAL_USER_STORE_FILENAME );
		 }

		 private static File GetUserRepositoryFile( Config config, string fileName )
		 {
			  // Resolve auth store file names
			  File authStoreDir = config.Get( DatabaseManagementSystemSettings.auth_store_directory );

			  // Because it contains sensitive information there is a legacy setting to configure
			  // the location of the user store file that we still respect
			  File userStoreFile = config.Get( GraphDatabaseSettings.auth_store );
			  if ( userStoreFile == null )
			  {
					userStoreFile = new File( authStoreDir, fileName );
			  }
			  return userStoreFile;
		 }
	}

}