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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using UserManager = Neo4Net.Kernel.Api.security.UserManager;
	using User = Neo4Net.Kernel.impl.security.User;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using CommunitySecurityModule = Neo4Net.Server.Security.Auth.CommunitySecurityModule;
	using FileUserRepository = Neo4Net.Server.Security.Auth.FileUserRepository;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertException;

	public class SetInitialPasswordCommandTest
	{
		private bool InstanceFieldsInitialized = false;

		public SetInitialPasswordCommandTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDir = TestDirectory.testDirectory( _fileSystemRule.get() );
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _testDir );
		}

		 private SetInitialPasswordCommand _setPasswordCommand;
		 private File _authInitFile;
		 private File _authFile;
		 private FileSystemAbstraction _fileSystem;

		 private readonly EphemeralFileSystemRule _fileSystemRule = new EphemeralFileSystemRule();
		 private TestDirectory _testDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDir);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fileSystem = _fileSystemRule.get();
			  OutsideWorld mock = mock( typeof( OutsideWorld ) );
			  when( mock.FileSystem() ).thenReturn(_fileSystem);
			  _setPasswordCommand = new SetInitialPasswordCommand( _testDir.directory( "home" ).toPath(), _testDir.directory("conf").toPath(), mock );
			  _authInitFile = CommunitySecurityModule.getInitialUserRepositoryFile( _setPasswordCommand.loadNeo4NetConfig() );
			  CommunitySecurityModule.getUserRepositoryFile( _setPasswordCommand.loadNeo4NetConfig() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailSetPasswordWithNoArguments()
		 public virtual void ShouldFailSetPasswordWithNoArguments()
		 {
			  assertException( () => _setPasswordCommand.execute(new string[0]), typeof(IncorrectUsage), "not enough arguments" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailSetPasswordWithTooManyArguments()
		 public virtual void ShouldFailSetPasswordWithTooManyArguments()
		 {
			  string[] arguments = new string[] { "", "123", "321" };
			  assertException( () => _setPasswordCommand.execute(arguments), typeof(IncorrectUsage), "unrecognized arguments: '123 321'" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetInitialPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetInitialPassword()
		 {
			  // Given
			  assertFalse( _fileSystem.fileExists( _authInitFile ) );

			  // When
			  string[] arguments = new string[] { "123" };
			  _setPasswordCommand.execute( arguments );

			  // Then
			  AssertAuthIniFile( "123" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverwriteInitialPasswordFileIfExists() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOverwriteInitialPasswordFileIfExists()
		 {
			  // Given
			  _fileSystem.mkdirs( _authInitFile.ParentFile );
			  _fileSystem.create( _authInitFile );

			  // When
			  string[] arguments = new string[] { "123" };
			  _setPasswordCommand.execute( arguments );

			  // Then
			  AssertAuthIniFile( "123" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkAlsoWithSamePassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkAlsoWithSamePassword()
		 {
			  string[] arguments = new string[] { "Neo4Net" };
			  _setPasswordCommand.execute( arguments );

			  // Then
			  AssertAuthIniFile( "Neo4Net" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintNiceHelp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintNiceHelp()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "Neo4Net-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new SetInitialPasswordCommandProvider(), ps.println );

					assertEquals( string.Format( "usage: Neo4Net-admin set-initial-password <password>%n" + "%n" + "environment variables:%n" + "    Neo4Net_CONF    Path to directory which contains Neo4Net.conf.%n" + "    Neo4Net_DEBUG   Set to anything to enable debug output.%n" + "    Neo4Net_HOME    Neo4Net home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "Sets the initial password of the initial admin user ('Neo4Net').%n" ), baos.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAuthIniFile(String password) throws Throwable
		 private void AssertAuthIniFile( string password )
		 {
			  assertTrue( _fileSystem.fileExists( _authInitFile ) );
			  FileUserRepository userRepository = new FileUserRepository( _fileSystem, _authInitFile, NullLogProvider.Instance );
			  userRepository.Start();
			  User Neo4Net = userRepository.GetUserByName( Neo4Net.Kernel.Api.security.UserManager_Fields.INITIAL_USER_NAME );
			  assertNotNull( Neo4Net );
			  assertTrue( Neo4Net.Credentials().matchesPassword(password) );
			  assertFalse( Neo4Net.HasFlag( User.PASSWORD_CHANGE_REQUIRED ) );
		 }
	}

}