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
namespace Neo4Net.Commandline.Admin.security
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LegacyCredential = Neo4Net.Server.Security.Auth.LegacyCredential;
	using User = Neo4Net.Kernel.impl.security.User;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using CommunitySecurityModule = Neo4Net.Server.Security.Auth.CommunitySecurityModule;
	using FileUserRepository = Neo4Net.Server.Security.Auth.FileUserRepository;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

	public class SetDefaultAdminCommandTest
	{
		private bool InstanceFieldsInitialized = false;

		public SetDefaultAdminCommandTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			TestDir = TestDirectory.testDirectory( _fileSystem );
		}

		 private SetDefaultAdminCommand _setDefaultAdmin;
		 private File _adminIniFile;
		 private FileSystemAbstraction _fileSystem = new EphemeralFileSystemAbstraction();
		 private Config _config;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expect = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expect = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory(fileSystem);
		 public TestDirectory TestDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  OutsideWorld mock = mock( typeof( OutsideWorld ) );
			  when( mock.FileSystem() ).thenReturn(_fileSystem);
			  _setDefaultAdmin = new SetDefaultAdminCommand( TestDir.directory( "home" ).toPath(), TestDir.directory("conf").toPath(), mock );
			  _config = _setDefaultAdmin.loadNeo4jConfig();
			  UserRepository users = CommunitySecurityModule.getUserRepository( _config, NullLogProvider.Instance, _fileSystem );
			  users.create(new User.Builder("jake", LegacyCredential.forPassword("123"))
									.withRequiredPasswordChange( false ).build());
			  _adminIniFile = new File( CommunitySecurityModule.getUserRepositoryFile( _config ).ParentFile, "admin.ini" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailForNoArguments()
		 public virtual void ShouldFailForNoArguments()
		 {
			  assertException( () => _setDefaultAdmin.execute(new string[0]), typeof(IncorrectUsage), "not enough arguments" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailForTooManyArguments()
		 public virtual void ShouldFailForTooManyArguments()
		 {
			  string[] arguments = new string[] { "", "123", "321" };
			  assertException( () => _setDefaultAdmin.execute(arguments), typeof(IncorrectUsage), "unrecognized arguments: '123 321'" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetDefaultAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetDefaultAdmin()
		 {
			  // Given
			  assertFalse( _fileSystem.fileExists( _adminIniFile ) );

			  // When
			  string[] arguments = new string[] { "jake" };
			  _setDefaultAdmin.execute( arguments );

			  // Then
			  AssertAdminIniFile( "jake" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSetDefaultAdminForNonExistentUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSetDefaultAdminForNonExistentUser()
		 {
			  // Then
			  Expect.expect( typeof( CommandFailed ) );
			  Expect.expectMessage( "no such user: 'noName'" );

			  // When
			  string[] arguments = new string[] { "noName" };
			  _setDefaultAdmin.execute( arguments );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintNiceHelp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintNiceHelp()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "neo4j-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new SetDefaultAdminCommandProvider(), ps.println );

					assertEquals( string.Format( "usage: neo4j-admin set-default-admin <username>%n" + "%n" + "environment variables:%n" + "    NEO4J_CONF    Path to directory which contains neo4j.conf.%n" + "    NEO4J_DEBUG   Set to anything to enable debug output.%n" + "    NEO4J_HOME    Neo4j home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "Sets the user to become admin if users but no roles are present, for example%n" + "when upgrading to neo4j 3.1 enterprise.%n" ), baos.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAdminIniFile(String username) throws Throwable
		 private void AssertAdminIniFile( string username )
		 {
			  assertTrue( _fileSystem.fileExists( _adminIniFile ) );
			  FileUserRepository userRepository = new FileUserRepository( _fileSystem, _adminIniFile, NullLogProvider.Instance );
			  userRepository.Start();
			  assertThat( userRepository.AllUsernames, containsInAnyOrder( username ) );
		 }
	}

}