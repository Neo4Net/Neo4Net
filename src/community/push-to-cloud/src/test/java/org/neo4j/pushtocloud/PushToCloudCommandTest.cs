using System.Collections.Generic;
using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Neo4Net.Pushtocloud
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using EnvironmentVariables = org.junit.contrib.java.lang.system.EnvironmentVariables;
	using InOrder = org.mockito.InOrder;


	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Database = Neo4Net.CommandLine.Args.Common.Database;
	using Neo4Net.GraphDb.config;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using Copier = Neo4Net.Pushtocloud.PushToCloudCommand.Copier;
	using DumpCreator = Neo4Net.Pushtocloud.PushToCloudCommand.DumpCreator;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.pushtocloud.PushToCloudCommand.ARG_BOLT_URI;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.pushtocloud.PushToCloudCommand.ARG_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.pushtocloud.PushToCloudCommand.ARG_CONFIRMED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.pushtocloud.PushToCloudCommand.ARG_DUMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.pushtocloud.PushToCloudCommand.ARG_DUMP_TO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.pushtocloud.PushToCloudCommand.ARG_PASSWORD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.pushtocloud.PushToCloudCommand.ARG_USERNAME;

	public class PushToCloudCommandTest
	{
		 private const string SOME_EXAMPLE_BOLT_URI = "bolt+routing://database_id.databases.Neo4Net.io";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.contrib.java.lang.system.EnvironmentVariables environmentVariables = new org.junit.contrib.java.lang.system.EnvironmentVariables();
		 public readonly EnvironmentVariables EnvironmentVariables = new EnvironmentVariables();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadUsernameAndPasswordFromUserInput() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadUsernameAndPasswordFromUserInput()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  string username = "Neo4Net";
			  char[] password = new char[] { 'a', 'b', 'c' };
			  OutsideWorld outsideWorld = ( new ControlledOutsideWorld( new DefaultFileSystemAbstraction() ) ).withPromptResponse(username).withPasswordResponse(password);
			  PushToCloudCommand command = command().copier(targetCommunicator).OutsideWorld(outsideWorld).build();

			  // when
			  command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );

			  // then
			  verify( targetCommunicator ).authenticate( anyBoolean(), any(), eq(username), eq(password), anyBoolean() );
			  verify( targetCommunicator ).copy( anyBoolean(), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptConfirmationViaCommandLine() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptConfirmationViaCommandLine()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  string username = "Neo4Net";
			  char[] password = new char[] { 'a', 'b', 'c' };
			  OutsideWorld outsideWorld = ( new ControlledOutsideWorld( new DefaultFileSystemAbstraction() ) ).withPromptResponse(username).withPasswordResponse(password);
			  PushToCloudCommand command = command().copier(targetCommunicator).OutsideWorld(outsideWorld).build();

			  // when
			  command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI), Arg(ARG_CONFIRMED, "true") ) );

			  // then
			  verify( targetCommunicator ).authenticate( anyBoolean(), any(), eq(username), eq(password), anyBoolean() );
			  verify( targetCommunicator ).copy( anyBoolean(), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptDumpAsSource() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptDumpAsSource()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  PushToCloudCommand command = command().copier(targetCommunicator).Build();

			  // when
			  Path dump = CreateSimpleDatabaseDump();
			  command.Execute( array( Arg( ARG_DUMP, dump.ToString() ), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );

			  // then
			  verify( targetCommunicator ).copy( anyBoolean(), any(), eq(dump), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptDatabaseNameAsSource() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptDatabaseNameAsSource()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  DumpCreator dumpCreator = mock( typeof( DumpCreator ) );
			  PushToCloudCommand command = command().copier(targetCommunicator).DumpCreator(dumpCreator).build();

			  // when
			  string databaseName = "Neo4Net";
			  command.Execute( array( Arg( ARG_DATABASE, databaseName ), Arg( ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI ) ) );

			  // then
			  verify( dumpCreator ).dumpDatabase( eq( databaseName ), any() );
			  verify( targetCommunicator ).copy( anyBoolean(), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptDatabaseNameAsSourceUsingGivenDumpTarget() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptDatabaseNameAsSourceUsingGivenDumpTarget()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  DumpCreator dumpCreator = mock( typeof( DumpCreator ) );
			  PushToCloudCommand command = command().copier(targetCommunicator).DumpCreator(dumpCreator).build();

			  // when
			  string databaseName = "Neo4Net";
			  Path dumpFile = Directory.file( "some-dump-file" ).toPath();
			  command.Execute( array( Arg( ARG_DATABASE, databaseName ), Arg( ARG_DUMP_TO, dumpFile.ToString() ), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );

			  // then
			  verify( dumpCreator ).dumpDatabase( databaseName, dumpFile );
			  verify( targetCommunicator ).copy( anyBoolean(), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnDatabaseNameAsSourceUsingExistingDumpTarget() throws java.io.IOException, org.Neo4Net.commandline.admin.IncorrectUsage, org.Neo4Net.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnDatabaseNameAsSourceUsingExistingDumpTarget()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  DumpCreator dumpCreator = mock( typeof( DumpCreator ) );
			  PushToCloudCommand command = command().copier(targetCommunicator).DumpCreator(dumpCreator).build();

			  // when
			  string databaseName = "Neo4Net";
			  Path dumpFile = Directory.file( "some-dump-file" ).toPath();
			  Files.write( dumpFile, "some data".GetBytes() );
			  try
			  {
					command.Execute( array( Arg( ARG_DATABASE, databaseName ), Arg( ARG_DUMP_TO, dumpFile.ToString() ), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );
					fail( "Should have failed" );
			  }
			  catch ( CommandFailed commandFailed )
			  {
					// then
					assertThat( commandFailed.Message, containsString( "already exists" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptBothDumpAndDatabaseNameAsSource() throws java.io.IOException, org.Neo4Net.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptBothDumpAndDatabaseNameAsSource()
		 {
			  // given
			  PushToCloudCommand command = command().copier(MockedTargetCommunicator()).Build();

			  // when
			  try
			  {
					command.Execute( array( Arg( ARG_DUMP, Directory.file( "some-dump-file" ).toPath().ToString() ), Arg(ARG_DATABASE, "Neo4Net"), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );
					fail( "Should have failed" );
			  }
			  catch ( IncorrectUsage )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptOnlyUsernameOrPasswordFromArgument() throws java.io.IOException, org.Neo4Net.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptOnlyUsernameOrPasswordFromArgument()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  string username = "Neo4Net";
			  char[] password = new char[] { 'a', 'b', 'c' };
			  OutsideWorld outsideWorld = ( new ControlledOutsideWorld( new DefaultFileSystemAbstraction() ) ).withPromptResponse(username).withPasswordResponse(password);
			  PushToCloudCommand command = command().copier(targetCommunicator).OutsideWorld(outsideWorld).build();

			  // when
			  try
			  {
					command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_USERNAME, "user"), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );
					fail( "Should have failed" );
			  }
			  catch ( IncorrectUsage )
			  {
					// then good
			  }

			  try
			  {
					command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_PASSWORD, "pass"), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );
					fail( "Should have failed" );
			  }
			  catch ( IncorrectUsage )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptOnlyUsernameOrPasswordFromEnvVar() throws java.io.IOException, org.Neo4Net.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptOnlyUsernameOrPasswordFromEnvVar()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  string username = "Neo4Net";
			  char[] password = new char[] { 'a', 'b', 'c' };
			  OutsideWorld outsideWorld = ( new ControlledOutsideWorld( new DefaultFileSystemAbstraction() ) ).withPromptResponse(username).withPasswordResponse(password);
			  PushToCloudCommand command = command().copier(targetCommunicator).OutsideWorld(outsideWorld).build();

			  // when
			  try
			  {
					EnvironmentVariables.set( "Neo4Net_USERNAME", "Neo4Net" );
					EnvironmentVariables.set( "Neo4Net_PASSWORD", null );
					command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );
					fail( "Should have failed" );
			  }
			  catch ( IncorrectUsage )
			  {
					// then good
			  }

			  try
			  {
					EnvironmentVariables.set( "Neo4Net_USERNAME", null );
					EnvironmentVariables.set( "Neo4Net_PASSWORD", "pass" );
					command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );
					fail( "Should have failed" );
			  }
			  catch ( IncorrectUsage )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptOnlyUsernameAndPasswordFromEnvAndCli() throws java.io.IOException, org.Neo4Net.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptOnlyUsernameAndPasswordFromEnvAndCli()
		 {
			  // given
			  Copier targetCommunicator = MockedTargetCommunicator();
			  string username = "Neo4Net";
			  char[] password = new char[] { 'a', 'b', 'c' };
			  OutsideWorld outsideWorld = ( new ControlledOutsideWorld( new DefaultFileSystemAbstraction() ) ).withPromptResponse(username).withPasswordResponse(password);
			  PushToCloudCommand command = command().copier(targetCommunicator).OutsideWorld(outsideWorld).build();

			  // when
			  try
			  {
					EnvironmentVariables.set( "Neo4Net_USERNAME", "Neo4Net" );
					EnvironmentVariables.set( "Neo4Net_PASSWORD", "pass" );
					command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_USERNAME, "Neo4Net"), Arg(ARG_PASSWORD, "pass"), Arg(ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI) ) );
					fail( "Should have failed" );
			  }
			  catch ( IncorrectUsage )
			  {
					// then good
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChooseToDumpDefaultDatabaseIfNeitherDumpNorDatabaseIsGiven() throws java.io.IOException, org.Neo4Net.commandline.admin.CommandFailed, org.Neo4Net.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChooseToDumpDefaultDatabaseIfNeitherDumpNorDatabaseIsGiven()
		 {
			  // given
			  DumpCreator dumpCreator = mock( typeof( DumpCreator ) );
			  Copier copier = mock( typeof( Copier ) );
			  PushToCloudCommand command = command().dumpCreator(dumpCreator).Copier(copier).build();

			  // when
			  command.Execute( array( Arg( ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI ) ) );

			  // then
			  string defaultDatabase = ( new Database() ).defaultValue();
			  verify( dumpCreator ).dumpDatabase( eq( defaultDatabase ), any() );
			  verify( copier ).copy( anyBoolean(), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnDumpPointingToMissingFile() throws java.io.IOException, org.Neo4Net.commandline.admin.IncorrectUsage, org.Neo4Net.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnDumpPointingToMissingFile()
		 {
			  // given
			  PushToCloudCommand command = command().copier(MockedTargetCommunicator()).Build();

			  // when
			  try
			  {
					File dumpFile = Directory.file( "some-dump-file" );
					command.Execute( array( Arg( ARG_DUMP, dumpFile.AbsolutePath ), Arg( ARG_BOLT_URI, SOME_EXAMPLE_BOLT_URI ) ) );
					fail( "Should have failed" );
			  }
			  catch ( CommandFailed )
			  {
					// then good
			  }
		 }

		 // TODO: 2019-08-07 shouldFailOnDumpPointingToInvalidDumpFile

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecognizeBothEnvironmentAndDatabaseIdFromBoltURI() throws java.io.IOException, org.Neo4Net.commandline.admin.CommandFailed, org.Neo4Net.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecognizeBothEnvironmentAndDatabaseIdFromBoltURI()
		 {
			  // given
			  Copier copier = mock( typeof( Copier ) );
			  PushToCloudCommand command = command().copier(copier).Build();

			  // when
			  command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_BOLT_URI, "bolt+routing://mydbid-testenvironment.databases.Neo4Net.io") ) );

			  // then
			  verify( copier ).copy( anyBoolean(), eq("https://console-testenvironment.Neo4Net.io/v1/databases/mydbid"), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecognizeDatabaseIdFromBoltURI() throws java.io.IOException, org.Neo4Net.commandline.admin.CommandFailed, org.Neo4Net.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecognizeDatabaseIdFromBoltURI()
		 {
			  // given
			  Copier copier = mock( typeof( Copier ) );
			  PushToCloudCommand command = command().copier(copier).Build();

			  // when
			  command.Execute( array( Arg( ARG_DUMP, CreateSimpleDatabaseDump().ToString() ), Arg(ARG_BOLT_URI, "bolt+routing://mydbid.databases.Neo4Net.io") ) );

			  // then
			  verify( copier ).copy( anyBoolean(), eq("https://console.Neo4Net.io/v1/databases/mydbid"), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateBeforeDumping() throws org.Neo4Net.commandline.admin.CommandFailed, java.io.IOException, org.Neo4Net.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthenticateBeforeDumping()
		 {
			  // given
			  Copier copier = MockedTargetCommunicator();
			  DumpCreator dumper = mock( typeof( DumpCreator ) );
			  PushToCloudCommand command = command().copier(copier).DumpCreator(dumper).build();

			  // when
			  command.Execute( array( Arg( ARG_BOLT_URI, "bolt+routing://mydbid.databases.Neo4Net.io" ) ) );

			  // then
			  InOrder inOrder = inOrder( copier, dumper );
			  inOrder.verify( copier ).authenticate( anyBoolean(), anyString(), anyString(), any(), anyBoolean() );
			  inOrder.verify( dumper ).dumpDatabase( anyString(), any() );
			  inOrder.verify( copier ).copy( anyBoolean(), anyString(), any(), anyString() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.pushtocloud.PushToCloudCommand.Copier mockedTargetCommunicator() throws org.Neo4Net.commandline.admin.CommandFailed
		 private Copier MockedTargetCommunicator()
		 {
			  Copier copier = mock( typeof( Copier ) );
			  when( copier.Authenticate( anyBoolean(), any(), any(), any(), anyBoolean() ) ).thenReturn("abc");
			  return copier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.file.Path createSimpleDatabaseDump() throws java.io.IOException
		 private Path CreateSimpleDatabaseDump()
		 {
			  Path dump = Directory.file( "dump" ).toPath();
			  Files.write( dump, "some data".GetBytes() );
			  return dump;
		 }

		 private string Arg( string key, string value )
		 {
			  return format( "--%s=%s", key, value );
		 }

		 private Builder Command()
		 {
			  return new Builder( this );
		 }

		 private class Builder
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 HomeDir = outerInstance.Directory.directory().toPath();
				 ConfigDir = outerInstance.Directory.directory( "conf" ).toPath();
			 }

			 private readonly PushToCloudCommandTest _outerInstance;

			 public Builder( PushToCloudCommandTest outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			  internal Path HomeDir;
			  internal Path ConfigDir;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal OutsideWorld OutsideWorldConflict = new ControlledOutsideWorld( new DefaultFileSystemAbstraction() );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal DumpCreator DumpCreatorConflict = mock( typeof( DumpCreator ) );
			  internal Copier TargetCommunicator;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<org.Neo4Net.graphdb.config.Setting<?>,String> settings = new java.util.HashMap<>();
			  internal readonly IDictionary<Setting<object>, string> Settings = new Dictionary<Setting<object>, string>();

			  internal virtual Builder Config<T1>( Setting<T1> setting, string value )
			  {
					Settings[setting] = value;
					return this;
			  }

			  internal virtual Builder Copier( Copier targetCommunicator )
			  {
					this.TargetCommunicator = targetCommunicator;
					return this;
			  }

			  internal virtual Builder OutsideWorld( OutsideWorld outsideWorld )
			  {
					this.OutsideWorldConflict = outsideWorld;
					return this;
			  }

			  internal virtual Builder DumpCreator( DumpCreator dumpCreator )
			  {
					this.DumpCreatorConflict = dumpCreator;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PushToCloudCommand build() throws java.io.IOException
			  internal virtual PushToCloudCommand Build()
			  {
					return new PushToCloudCommand( HomeDir, BuildConfig(), OutsideWorldConflict, TargetCommunicator, DumpCreatorConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.file.Path buildConfig() throws java.io.IOException
			  internal virtual Path BuildConfig()
			  {
					StringBuilder configFileContents = new StringBuilder();
					Settings.forEach( ( key, value ) => configFileContents.Append( format( "%s=%s%n", key.name(), value ) ) );
					Path configFile = ConfigDir.resolve( "Neo4Net.conf" );
					Files.write( configFile, configFileContents.ToString().GetBytes() );
					return configFile;
			  }
		 }
	}

}