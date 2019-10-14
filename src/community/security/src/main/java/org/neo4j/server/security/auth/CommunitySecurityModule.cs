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
namespace Neo4Net.Server.Security.Auth
{

	using DatabaseManagementSystemSettings = Neo4Net.Dbms.DatabaseManagementSystemSettings;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Service = Neo4Net.Helpers.Service;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using PasswordPolicy = Neo4Net.Kernel.api.security.PasswordPolicy;
	using SecurityModule = Neo4Net.Kernel.api.security.SecurityModule;
	using UserManager = Neo4Net.Kernel.api.security.UserManager;
	using UserManagerSupplier = Neo4Net.Kernel.api.security.UserManagerSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Clocks = Neo4Net.Time.Clocks;

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