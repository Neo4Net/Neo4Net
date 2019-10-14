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
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LegacyCredential = Neo4Net.Server.Security.Auth.LegacyCredential;
	using User = Neo4Net.Kernel.impl.security.User;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using CommunitySecurityModule = Neo4Net.Server.Security.Auth.CommunitySecurityModule;
	using FileUserRepository = Neo4Net.Server.Security.Auth.FileUserRepository;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SetDefaultAdminCommandIT
	{
		 private FileSystemAbstraction _fileSystem = new EphemeralFileSystemAbstraction();
		 private File _confDir;
		 private File _homeDir;
		 private OutsideWorld @out;
		 private AdminTool _tool;

		 private const string SET_ADMIN = "set-default-admin";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  File graphDir = new File( GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  _confDir = new File( graphDir, "conf" );
			  _homeDir = new File( graphDir, "home" );
			  @out = mock( typeof( OutsideWorld ) );
			  ResetOutsideWorldMock();
			  _tool = new AdminTool( CommandLocator.fromServiceLocator(), BlockerLocator.fromServiceLocator(), @out, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetDefaultAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetDefaultAdmin()
		 {
			  InsertUser( "jane", false );
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN, "jane" );
			  AssertAdminIniFile( "jane" );

			  verify( @out ).stdOutLine( "default admin user set to 'jane'" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetDefaultAdminForInitialUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetDefaultAdminForInitialUser()
		 {
			  InsertUser( "jane", true );
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN, "jane" );
			  AssertAdminIniFile( "jane" );

			  verify( @out ).stdOutLine( "default admin user set to 'jane'" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverwrite() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOverwrite()
		 {
			  InsertUser( "jane", false );
			  InsertUser( "janette", false );
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN, "jane" );
			  AssertAdminIniFile( "jane" );
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN, "janette" );
			  AssertAdminIniFile( "janette" );

			  verify( @out ).stdOutLine( "default admin user set to 'jane'" );
			  verify( @out ).stdOutLine( "default admin user set to 'janette'" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldErrorWithNoSuchUser()
		 public virtual void ShouldErrorWithNoSuchUser()
		 {
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN, "bob" );
			  verify( @out ).stdErrLine( "command failed: no such user: 'bob'" );
			  verify( @out ).exit( 1 );
			  verify( @out, never() ).stdOutLine(anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreInitialUserIfUsersExist() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreInitialUserIfUsersExist()
		 {
			  InsertUser( "jane", false );
			  InsertUser( "janette", true );
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN, "jane" );
			  AssertAdminIniFile( "jane" );
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN, "janette" );

			  verify( @out ).stdOutLine( "default admin user set to 'jane'" );
			  verify( @out ).stdErrLine( "command failed: no such user: 'janette'" );
			  verify( @out ).exit( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetUsageOnWrongArguments1()
		 public virtual void ShouldGetUsageOnWrongArguments1()
		 {
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN );
			  AssertNoAuthIniFile();

			  verify( @out ).stdErrLine( "not enough arguments" );
			  verify( @out, times( 3 ) ).stdErrLine( "" );
			  verify( @out ).stdErrLine( "usage: neo4j-admin set-default-admin <username>" );
			  verify( @out, times( 3 ) ).stdErrLine( "" );
			  verify( @out ).stdErrLine( string.Format( "environment variables:" ) );
			  verify( @out ).stdErrLine( string.Format( "    NEO4J_CONF    Path to directory which contains neo4j.conf." ) );
			  verify( @out ).stdErrLine( string.Format( "    NEO4J_DEBUG   Set to anything to enable debug output." ) );
			  verify( @out ).stdErrLine( string.Format( "    NEO4J_HOME    Neo4j home directory." ) );
			  verify( @out ).stdErrLine( string.Format( "    HEAP_SIZE     Set JVM maximum heap size during command execution." ) );
			  verify( @out ).stdErrLine( string.Format( "                  Takes a number and a unit, for example 512m." ) );
			  verify( @out ).stdErrLine( string.Format( "Sets the user to become admin if users but no roles are present, for example%n" + "when upgrading to neo4j 3.1 enterprise." ) );
			  verify( @out ).exit( 1 );
			  verifyNoMoreInteractions( @out );
			  verify( @out, never() ).stdOutLine(anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetUsageOnWrongArguments2()
		 public virtual void ShouldGetUsageOnWrongArguments2()
		 {
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_ADMIN, "foo", "bar" );
			  AssertNoAuthIniFile();

			  verify( @out ).stdErrLine( "unrecognized arguments: 'bar'" );
			  verify( @out, times( 3 ) ).stdErrLine( "" );
			  verify( @out ).stdErrLine( "usage: neo4j-admin set-default-admin <username>" );
			  verify( @out, times( 3 ) ).stdErrLine( "" );
			  verify( @out ).stdErrLine( string.Format( "environment variables:" ) );
			  verify( @out ).stdErrLine( string.Format( "    NEO4J_CONF    Path to directory which contains neo4j.conf." ) );
			  verify( @out ).stdErrLine( string.Format( "    NEO4J_DEBUG   Set to anything to enable debug output." ) );
			  verify( @out ).stdErrLine( string.Format( "    NEO4J_HOME    Neo4j home directory." ) );
			  verify( @out ).stdErrLine( string.Format( "    HEAP_SIZE     Set JVM maximum heap size during command execution." ) );
			  verify( @out ).stdErrLine( string.Format( "                  Takes a number and a unit, for example 512m." ) );
			  verify( @out ).stdErrLine( string.Format( "Sets the user to become admin if users but no roles are present, for example%n" + "when upgrading to neo4j 3.1 enterprise." ) );
			  verify( @out ).exit( 1 );
			  verifyNoMoreInteractions( @out );
			  verify( @out, never() ).stdOutLine(anyString());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insertUser(String username, boolean initial) throws Throwable
		 private void InsertUser( string username, bool initial )
		 {
			  File userFile = GetAuthFile( initial ? CommunitySecurityModule.INITIAL_USER_STORE_FILENAME : CommunitySecurityModule.USER_STORE_FILENAME );
			  FileUserRepository userRepository = new FileUserRepository( _fileSystem, userFile, NullLogProvider.Instance );
			  userRepository.Start();
			  userRepository.Create( ( new User.Builder( username, LegacyCredential.INACCESSIBLE ) ).build() );
			  assertTrue( userRepository.AllUsernames.Contains( username ) );
			  userRepository.Stop();
			  userRepository.Shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAdminIniFile(String username) throws Throwable
		 private void AssertAdminIniFile( string username )
		 {
			  File adminIniFile = GetAuthFile( SetDefaultAdminCommand.ADMIN_INI );
			  assertTrue( _fileSystem.fileExists( adminIniFile ) );
			  FileUserRepository userRepository = new FileUserRepository( _fileSystem, adminIniFile, NullLogProvider.Instance );
			  userRepository.Start();
			  assertThat( userRepository.AllUsernames, containsInAnyOrder( username ) );
			  userRepository.Stop();
			  userRepository.Shutdown();
		 }

		 private void AssertNoAuthIniFile()
		 {
			  assertFalse( _fileSystem.fileExists( GetAuthFile( SetDefaultAdminCommand.ADMIN_INI ) ) );
		 }

		 private File GetAuthFile( string name )
		 {
			  return new File( new File( new File( _homeDir, "data" ), "dbms" ), name );
		 }

		 private void ResetOutsideWorldMock()
		 {
			  reset( @out );
			  when( @out.FileSystem() ).thenReturn(_fileSystem);
		 }
	}

}