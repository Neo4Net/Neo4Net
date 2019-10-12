using System.IO;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.backup
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using BackupClient = Neo4Net.backup.impl.BackupClient;
	using BackupProtocolService = Neo4Net.backup.impl.BackupProtocolService;
	using BackupServer = Neo4Net.backup.impl.BackupServer;
	using ConsistencyCheck = Neo4Net.backup.impl.ConsistencyCheck;
	using ConsistencyCheckSettings = Neo4Net.Consistency.ConsistencyCheckSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfigLoadIOException = Neo4Net.Kernel.configuration.ConfigLoadIOException;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using SystemExitRule = Neo4Net.Test.rule.system.SystemExitRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

	public class BackupToolTest
	{
		private bool InstanceFieldsInitialized = false;

		public BackupToolTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Chain = RuleChain.outerRule( _suppressOutput ).around( _testDirectory ).around( _systemExitRule );
		}

		 private SystemExitRule _systemExitRule = SystemExitRule.none();
		 private TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private SuppressOutput _suppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(suppressOutput).around(testDirectory).around(systemExitRule);
		 public RuleChain Chain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldToolFailureExceptionCauseExitCode()
		 public virtual void ShouldToolFailureExceptionCauseExitCode()
		 {
			  _systemExitRule.expectExit( 1 );
			  BackupTool.ExitFailure( "tool failed" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBackupToolMainCauseExitCode()
		 public virtual void ShouldBackupToolMainCauseExitCode()
		 {
			  _systemExitRule.expectExit( 1 );
			  BackupTool.Main( new string[]{} );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseIncrementalOrFallbackToFull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseIncrementalOrFallbackToFull()
		 {
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // when
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // then
			  verify( service ).doIncrementalBackupOrFallbackToFull( eq( "localhost" ), eq( BackupServer.DEFAULT_PORT ), eq( DatabaseLayout.of( Paths.get( "my_backup" ).toFile() ) ), eq(ConsistencyCheck.FULL), any(typeof(Config)), eq(BackupClient.BIG_READ_TIMEOUT), eq(false) );
			  verify( systemOut ).println( "Performing backup from '" + new HostnamePort( "localhost", BackupServer.DEFAULT_PORT ) + "'" );
			  verify( systemOut ).println( "Done" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetTimeout()
		 {
			  string newTimeout = "3"; //seconds by default
			  long expectedTimeout = 3 * 1000;
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup", "-timeout", newTimeout };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // when
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // then
			  verify( service ).doIncrementalBackupOrFallbackToFull( eq( "localhost" ), eq( BackupServer.DEFAULT_PORT ), eq( DatabaseLayout.of( Paths.get( "my_backup" ).toFile() ) ), eq(ConsistencyCheck.FULL), any(typeof(Config)), eq(expectedTimeout), eq(false) );
			  verify( systemOut ).println( "Performing backup from '" + new HostnamePort( "localhost", BackupServer.DEFAULT_PORT ) + "'" );
			  verify( systemOut ).println( "Done" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreIncrementalFlag() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreIncrementalFlag()
		 {
			  string[] args = new string[]{ "-incremental", "-host", "localhost", "-to", "my_backup" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // when
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // then
			  verify( service ).doIncrementalBackupOrFallbackToFull( eq( "localhost" ), eq( BackupServer.DEFAULT_PORT ), eq( DatabaseLayout.of( Paths.get( "my_backup" ).toFile() ) ), eq(ConsistencyCheck.FULL), any(typeof(Config)), eq(BackupClient.BIG_READ_TIMEOUT), eq(false) );
			  verify( systemOut ).println( "Performing backup from '" + new HostnamePort( "localhost", BackupServer.DEFAULT_PORT ) + "'" );
			  verify( systemOut ).println( "Done" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreFullFlag() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreFullFlag()
		 {
			  string[] args = new string[]{ "-full", "-host", "localhost", "-to", "my_backup" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // when
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // then
			  verify( service ).doIncrementalBackupOrFallbackToFull( eq( "localhost" ), eq( BackupServer.DEFAULT_PORT ), eq( DatabaseLayout.of( Paths.get( "my_backup" ).toFile() ) ), eq(ConsistencyCheck.FULL), any(typeof(Config)), eq(BackupClient.BIG_READ_TIMEOUT), eq(false) );
			  verify( systemOut ).println( "Performing backup from '" + new HostnamePort( "localhost", BackupServer.DEFAULT_PORT ) + "'" );
			  verify( systemOut ).println( "Done" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void appliesDefaultTuningConfigurationForConsistencyChecker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AppliesDefaultTuningConfigurationForConsistencyChecker()
		 {
			  // given
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // when
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // then
			  ArgumentCaptor<Config> config = ArgumentCaptor.forClass( typeof( Config ) );
			  verify( service ).doIncrementalBackupOrFallbackToFull( anyString(), anyInt(), eq(DatabaseLayout.of(Paths.get("my_backup").toFile())), any(typeof(ConsistencyCheck)), config.capture(), eq(BackupClient.BIG_READ_TIMEOUT), eq(false) );
			  assertFalse( config.Value.get( ConsistencyCheckSettings.consistency_check_property_owners ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void passesOnConfigurationIfProvided() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PassesOnConfigurationIfProvided()
		 {
			  // given
			  File configFile = _testDirectory.file( Config.DEFAULT_CONFIG_FILE_NAME );
			  Properties properties = new Properties();
			  properties.setProperty( ConsistencyCheckSettings.consistency_check_property_owners.name(), "true" );
			  properties.store( new StreamWriter( configFile ), null );

			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup", "-config", configFile.Path };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // when
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // then
			  ArgumentCaptor<Config> config = ArgumentCaptor.forClass( typeof( Config ) );
			  verify( service ).doIncrementalBackupOrFallbackToFull( anyString(), anyInt(), eq(DatabaseLayout.of(Paths.get("my_backup").toFile())), any(typeof(ConsistencyCheck)), config.capture(), anyLong(), eq(false) );
			  assertTrue( config.Value.get( ConsistencyCheckSettings.consistency_check_property_owners ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exitWithFailureIfConfigSpecifiedButConfigFileDoesNotExist()
		 public virtual void ExitWithFailureIfConfigSpecifiedButConfigFileDoesNotExist()
		 {
			  // given
			  File configFile = _testDirectory.file( "nonexistent_file" );
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup", "-config", configFile.Path };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );
			  BackupTool backupTool = new BackupTool( service, systemOut );

			  try
			  {
					// when
					backupTool.Run( args );
					fail( "should exit abnormally" );
			  }
			  catch ( BackupTool.ToolFailureException e )
			  {
					// then
					assertThat( e.Message, containsString( "Could not read configuration file" ) );
					assertThat( e.InnerException, instanceOf( typeof( ConfigLoadIOException ) ) );
			  }

			  verifyZeroInteractions( service );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exitWithFailureIfNoSourceSpecified()
		 public virtual void ExitWithFailureIfNoSourceSpecified()
		 {
			  // given
			  string[] args = new string[]{ "-to", "my_backup" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );
			  BackupTool backupTool = new BackupTool( service, systemOut );

			  try
			  {
					// when
					backupTool.Run( args );
					fail( "should exit abnormally" );
			  }
			  catch ( BackupTool.ToolFailureException e )
			  {
					// then
					assertEquals( BackupTool.NoSourceSpecified, e.Message );
			  }

			  verifyZeroInteractions( service );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exitWithFailureIfInvalidSourceSpecified()
		 public virtual void ExitWithFailureIfInvalidSourceSpecified()
		 {
			  // given
			  string[] args = new string[]{ "-host", "foo:localhost", "-port", "123", "-to", "my_backup" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );
			  BackupTool backupTool = new BackupTool( service, systemOut );

			  try
			  {
					// when
					backupTool.Run( args );
					fail( "should exit abnormally" );
			  }
			  catch ( BackupTool.ToolFailureException e )
			  {
					// then
					assertEquals( BackupTool.WrongFromAddressSyntax, e.Message );
			  }

			  verifyZeroInteractions( service );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exitWithFailureIfNoDestinationSpecified()
		 public virtual void ExitWithFailureIfNoDestinationSpecified()
		 {
			  // given
			  string[] args = new string[]{ "-host", "localhost" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );
			  BackupTool backupTool = new BackupTool( service, systemOut );

			  try
			  {
					// when
					backupTool.Run( args );
					fail( "should exit abnormally" );
			  }
			  catch ( BackupTool.ToolFailureException e )
			  {
					// then
					assertEquals( "Specify target location with -to <target-directory>", e.Message );
			  }

			  verifyZeroInteractions( service );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void helpMessageForWrongUriShouldNotContainSchema()
		 public virtual void HelpMessageForWrongUriShouldNotContainSchema()
		 {
			  // given
			  string[] args = new string[]{ "-host", ":VeryWrongURI:", "-to", "/var/backup/graph" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  try
			  {
					// when
					( new BackupTool( service, systemOut ) ).Run( args );
					fail( "should exit abnormally" );
			  }
			  catch ( BackupTool.ToolFailureException e )
			  {
					// then
					assertThat( e.Message, equalTo( BackupTool.WrongFromAddressSyntax ) );
					assertThat( e.Message, not( containsString( "<schema>" ) ) );
			  }

			  verifyZeroInteractions( service, systemOut );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectVerifyFlagWithLegacyArguments() throws BackupTool.ToolFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectVerifyFlagWithLegacyArguments()
		 {
			  // Given
			  string host = "localhost";
			  Path targetDir = Paths.get( "/var/backup/neo4j/" );
			  string[] args = new string[] { "-from", host, "-to", targetDir.ToString(), "-verify", "false" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // When
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // Then
			  verify( service ).doIncrementalBackupOrFallbackToFull( eq( host ), eq( BackupServer.DEFAULT_PORT ), eq( DatabaseLayout.of( targetDir.toFile() ) ), eq(ConsistencyCheck.NONE), any(typeof(Config)), eq(BackupClient.BIG_READ_TIMEOUT), eq(false) );
			  verify( systemOut ).println( "Performing backup from '" + new HostnamePort( host, BackupServer.DEFAULT_PORT ) + "'" );
			  verify( systemOut ).println( "Done" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeUseOfDebugArgument() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeUseOfDebugArgument()
		 {
			  // given
			  string[] args = new string[]{ "-from", "localhost", "-to", "my_backup", "-gather-forensics" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // when
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // then
			  verify( service ).doIncrementalBackupOrFallbackToFull( anyString(), anyInt(), eq(DatabaseLayout.of(Paths.get("my_backup").toFile())), any(typeof(ConsistencyCheck)), any(typeof(Config)), anyLong(), eq(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveNoConsistencyCheckIfVerifyFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveNoConsistencyCheckIfVerifyFalse()
		 {
			  // Given
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup", "-verify", "false" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // When
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // Then
			  verify( service ).doIncrementalBackupOrFallbackToFull( anyString(), anyInt(), any(typeof(DatabaseLayout)), eq(ConsistencyCheck.NONE), any(typeof(Config)), anyLong(), anyBoolean() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreConsistencyCheckIfVerifyFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreConsistencyCheckIfVerifyFalse()
		 {
			  // Given
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup", "-verify", "false", "-consistency-checker", "legacy" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // When
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // Then
			  verify( service ).doIncrementalBackupOrFallbackToFull( anyString(), anyInt(), any(typeof(DatabaseLayout)), eq(ConsistencyCheck.NONE), any(typeof(Config)), anyLong(), anyBoolean() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveDefaultConsistencyCheckIfVerifyTrue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveDefaultConsistencyCheckIfVerifyTrue()
		 {
			  // Given
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup", "-verify", "true" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // When
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // Then
			  verify( service ).doIncrementalBackupOrFallbackToFull( anyString(), anyInt(), any(typeof(DatabaseLayout)), eq(ConsistencyCheck.FULL), any(typeof(Config)), anyLong(), anyBoolean() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectConsistencyCheckerWithDefaultVerify() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectConsistencyCheckerWithDefaultVerify()
		 {
			  // Given
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup", "-consistency-checker", "full" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  // When
			  ( new BackupTool( service, systemOut ) ).Run( args );

			  // Then
			  verify( service ).doIncrementalBackupOrFallbackToFull( anyString(), anyInt(), any(typeof(DatabaseLayout)), eq(ConsistencyCheck.FULL), any(typeof(Config)), anyLong(), anyBoolean() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCrashIfInvalidConsistencyCheckerSpecified() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCrashIfInvalidConsistencyCheckerSpecified()
		 {
			  // Given
			  string[] args = new string[]{ "-host", "localhost", "-to", "my_backup", "-verify", "true", "-consistency-checker", "notarealname" };
			  BackupProtocolService service = mock( typeof( BackupProtocolService ) );
			  PrintStream systemOut = mock( typeof( PrintStream ) );

			  try
			  {
					// When
					( new BackupTool( service, systemOut ) ).Run( args );
					fail( "Should throw exception if invalid consistency checker is specified." );
			  }
			  catch ( System.ArgumentException t )
			  {
					// Then
					assertThat( t.Message, containsString( "Unknown consistency check name" ) );
			  }
		 }
	}

}