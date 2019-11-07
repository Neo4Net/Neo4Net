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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using UserManager = Neo4Net.Kernel.Api.security.UserManager;
	using User = Neo4Net.Kernel.impl.security.User;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using FileUserRepository = Neo4Net.Server.Security.Auth.FileUserRepository;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
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

	public class SetInitialPasswordCommandIT
	{
		 private FileSystemAbstraction _fileSystem = new EphemeralFileSystemAbstraction();
		 private File _confDir;
		 private File _homeDir;
		 private OutsideWorld @out;
		 private AdminTool _tool;

		 private const string SET_PASSWORD = "set-initial-password";

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
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _fileSystem.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetPassword()
		 {
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "abc" );
			  AssertAuthIniFile( "abc" );

			  verify( @out ).stdOutLine( "Changed password for user 'Neo4Net'." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverwriteIfSetPasswordAgain() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOverwriteIfSetPasswordAgain()
		 {
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "abc" );
			  AssertAuthIniFile( "abc" );
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "muchBetter" );
			  AssertAuthIniFile( "muchBetter" );

			  verify( @out, times( 2 ) ).stdOutLine( "Changed password for user 'Neo4Net'." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithSamePassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithSamePassword()
		 {
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "Neo4Net" );
			  AssertAuthIniFile( "Neo4Net" );
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "Neo4Net" );
			  AssertAuthIniFile( "Neo4Net" );

			  verify( @out, times( 2 ) ).stdOutLine( "Changed password for user 'Neo4Net'." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetUsageOnWrongArguments1()
		 public virtual void ShouldGetUsageOnWrongArguments1()
		 {
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD );
			  AssertNoAuthIniFile();

			  verify( @out ).stdErrLine( "not enough arguments" );
			  verify( @out, times( 3 ) ).stdErrLine( "" );
			  verify( @out ).stdErrLine( "usage: Neo4Net-admin set-initial-password <password>" );
			  verify( @out ).stdErrLine( string.Format( "environment variables:" ) );
			  verify( @out ).stdErrLine( string.Format( "    Neo4Net_CONF    Path to directory which contains Neo4Net.conf." ) );
			  verify( @out ).stdErrLine( string.Format( "    Neo4Net_DEBUG   Set to anything to enable debug output." ) );
			  verify( @out ).stdErrLine( string.Format( "    Neo4Net_HOME    Neo4Net home directory." ) );
			  verify( @out ).stdErrLine( string.Format( "    HEAP_SIZE     Set JVM maximum heap size during command execution." ) );
			  verify( @out ).stdErrLine( string.Format( "                  Takes a number and a unit, for example 512m." ) );
			  verify( @out ).stdErrLine( "Sets the initial password of the initial admin user ('Neo4Net')." );
			  verify( @out ).exit( 1 );
			  verifyNoMoreInteractions( @out );
			  verify( @out, never() ).stdOutLine(anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetUsageOnWrongArguments2()
		 public virtual void ShouldGetUsageOnWrongArguments2()
		 {
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "foo", "bar" );
			  AssertNoAuthIniFile();

			  verify( @out ).stdErrLine( "unrecognized arguments: 'bar'" );
			  verify( @out, times( 3 ) ).stdErrLine( "" );
			  verify( @out ).stdErrLine( "usage: Neo4Net-admin set-initial-password <password>" );
			  verify( @out ).stdErrLine( string.Format( "environment variables:" ) );
			  verify( @out ).stdErrLine( string.Format( "    Neo4Net_CONF    Path to directory which contains Neo4Net.conf." ) );
			  verify( @out ).stdErrLine( string.Format( "    Neo4Net_DEBUG   Set to anything to enable debug output." ) );
			  verify( @out ).stdErrLine( string.Format( "    Neo4Net_HOME    Neo4Net home directory." ) );
			  verify( @out ).stdErrLine( string.Format( "    HEAP_SIZE     Set JVM maximum heap size during command execution." ) );
			  verify( @out ).stdErrLine( string.Format( "                  Takes a number and a unit, for example 512m." ) );

			  verify( @out ).stdErrLine( "Sets the initial password of the initial admin user ('Neo4Net')." );
			  verify( @out ).exit( 1 );
			  verifyNoMoreInteractions( @out );
			  verify( @out, never() ).stdOutLine(anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldErrorIfRealUsersAlreadyExistCommunity() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldErrorIfRealUsersAlreadyExistCommunity()
		 {
			  // Given
			  File authFile = GetAuthFile( "auth" );
			  _fileSystem.mkdirs( authFile.ParentFile );
			  _fileSystem.create( authFile );

			  // When
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "will-be-ignored" );

			  // Then
			  AssertNoAuthIniFile();
			  verify( @out, times( 1 ) ).stdErrLine( "command failed: the provided initial password was not set because existing Neo4Net users were " + "detected at `" + authFile.AbsolutePath + "`. Please remove the existing `auth` file if you " + "want to reset your database to only have a default user with the provided password." );
			  verify( @out ).exit( 1 );
			  verify( @out, times( 0 ) ).stdOutLine( anyString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldErrorIfRealUsersAlreadyExistEnterprise() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldErrorIfRealUsersAlreadyExistEnterprise()
		 {
			  // Given
			  File authFile = GetAuthFile( "auth" );
			  File rolesFile = GetAuthFile( "roles" );

			  _fileSystem.mkdirs( authFile.ParentFile );
			  _fileSystem.create( authFile );
			  _fileSystem.create( rolesFile );

			  // When
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "will-be-ignored" );

			  // Then
			  AssertNoAuthIniFile();
			  verify( @out, times( 1 ) ).stdErrLine( "command failed: the provided initial password was not set because existing Neo4Net users were " + "detected at `" + authFile.AbsolutePath + "`. Please remove the existing `auth` and `roles` files if you " + "want to reset your database to only have a default user with the provided password." );
			  verify( @out ).exit( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldErrorIfRealUsersAlreadyExistV2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldErrorIfRealUsersAlreadyExistV2()
		 {
			  // Given
			  // Create an `auth` file with the default Neo4Net user, but not the default password
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "not-the-default-password" );
			  File authFile = GetAuthFile( "auth" );
			  _fileSystem.mkdirs( authFile.ParentFile );
			  _fileSystem.renameFile( GetAuthFile( "auth.ini" ), authFile );

			  // When
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "will-be-ignored" );

			  // Then
			  AssertNoAuthIniFile();
			  verify( @out, times( 1 ) ).stdErrLine( "command failed: the provided initial password was not set because existing Neo4Net users were " + "detected at `" + authFile.AbsolutePath + "`. Please remove the existing `auth` file if you " + "want to reset your database to only have a default user with the provided password." );
			  verify( @out ).exit( 1 );

			  verify( @out, times( 1 ) ).stdOutLine( "Changed password for user 'Neo4Net'." ); // This is from the initial setup
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotErrorIfOnlyTheUnmodifiedDefaultNeo4NetUserAlreadyExists() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotErrorIfOnlyTheUnmodifiedDefaultNeo4NetUserAlreadyExists()
		 {
			  // Given
			  // Create an `auth` file with the default Neo4Net user
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, Neo4Net.Kernel.Api.security.UserManager_Fields.INITIAL_PASSWORD );
			  File authFile = GetAuthFile( "auth" );
			  _fileSystem.mkdirs( authFile.ParentFile );
			  _fileSystem.renameFile( GetAuthFile( "auth.ini" ), authFile );

			  // When
			  _tool.execute( _homeDir.toPath(), _confDir.toPath(), SET_PASSWORD, "should-not-be-ignored" );

			  // Then
			  AssertAuthIniFile( "should-not-be-ignored" );
			  verify( @out, times( 2 ) ).stdOutLine( "Changed password for user 'Neo4Net'." );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAuthIniFile(String password) throws Throwable
		 private void AssertAuthIniFile( string password )
		 {
			  File authIniFile = GetAuthFile( "auth.ini" );
			  assertTrue( _fileSystem.fileExists( authIniFile ) );
			  FileUserRepository userRepository = new FileUserRepository( _fileSystem, authIniFile, NullLogProvider.Instance );
			  userRepository.Start();
			  User Neo4Net = userRepository.GetUserByName( Neo4Net.Kernel.Api.security.UserManager_Fields.INITIAL_USER_NAME );
			  assertNotNull( Neo4Net );
			  assertTrue( Neo4Net.Credentials().matchesPassword(password) );
			  assertFalse( Neo4Net.HasFlag( User.PASSWORD_CHANGE_REQUIRED ) );
		 }

		 private void AssertNoAuthIniFile()
		 {
			  assertFalse( _fileSystem.fileExists( GetAuthFile( "auth.ini" ) ) );
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