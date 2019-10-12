using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.backup.impl
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.@internal.matchers.ThrowableCauseMatcher.hasCause;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.SelectedBackupProtocol.ANY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logical_logs_location;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;

	public class OnlineBackupContextFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppress = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput Suppress = SuppressOutput.suppressAll();

		 private Path _homeDir;
		 private Path _configDir;
		 private Path _configFile;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _homeDir = TestDirectory.directory( "home" ).toPath();
			  _configDir = TestDirectory.directory( "config" ).toPath();
			  _configFile = _configDir.resolve( "neo4j.conf" );
			  string neo4jConfContents = "dbms.backup.address = localhost:1234";
			  Files.write( _configFile, singletonList( neo4jConfContents ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unspecifiedHostnameIsEmptyOptional() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnspecifiedHostnameIsEmptyOptional()
		 {
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( RequiredAnd( "--from=:1234" ) );
			  OnlineBackupRequiredArguments requiredArguments = context.RequiredArguments;

			  assertFalse( requiredArguments.Address.Hostname.Present );
			  assertEquals( 1234, requiredArguments.Address.Port.Value.intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unspecifiedPortIsEmptyOptional() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnspecifiedPortIsEmptyOptional()
		 {
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( RequiredAnd( "--from=abc" ) );
			  OnlineBackupRequiredArguments requiredArguments = context.RequiredArguments;

			  assertEquals( "abc", requiredArguments.Address.Hostname.get() );
			  assertFalse( requiredArguments.Address.Port.HasValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acceptHostWithTrailingPort() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcceptHostWithTrailingPort()
		 {
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( RequiredAnd( "--from=foo.bar.server:" ) );
			  OnlineBackupRequiredArguments requiredArguments = context.RequiredArguments;
			  assertEquals( "foo.bar.server", requiredArguments.Address.Hostname.get() );
			  assertFalse( requiredArguments.Address.Port.HasValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acceptPortWithPrecedingEmptyHost() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcceptPortWithPrecedingEmptyHost()
		 {
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( RequiredAnd( "--from=:1234" ) );
			  OnlineBackupRequiredArguments requiredArguments = context.RequiredArguments;
			  assertFalse( requiredArguments.Address.Hostname.Present );
			  assertEquals( 1234, requiredArguments.Address.Port.Value.intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acceptBothIfSpecified() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcceptBothIfSpecified()
		 {
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( RequiredAnd( "--from=foo.bar.server:1234" ) );
			  OnlineBackupRequiredArguments requiredArguments = context.RequiredArguments;
			  assertEquals( "foo.bar.server", requiredArguments.Address.Hostname.get() );
			  assertEquals( 1234, requiredArguments.Address.Port.Value.intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupDirectoryArgumentIsMandatory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupDirectoryArgumentIsMandatory()
		 {
			  Expected.expect( typeof( IncorrectUsage ) );
			  Expected.expectMessage( "Missing argument 'backup-dir'" );
			  ( new OnlineBackupContextFactory( _homeDir, _configDir ) ).createContext();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefaultTimeoutToTwentyMinutes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDefaultTimeoutToTwentyMinutes()
		 {
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( "--backup-dir=/", "--name=mybackup" );
			  OnlineBackupRequiredArguments requiredArguments = context.RequiredArguments;

			  assertEquals( MINUTES.toMillis( 20 ), requiredArguments.Timeout );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterpretAUnitlessTimeoutAsSeconds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInterpretAUnitlessTimeoutAsSeconds()
		 {
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( "--timeout=10", "--backup-dir=/", "--name=mybackup" );
			  OnlineBackupRequiredArguments requiredArguments = context.RequiredArguments;

			  assertEquals( SECONDS.toMillis( 10 ), requiredArguments.Timeout );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseATimeoutWithUnits() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseATimeoutWithUnits()
		 {
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( RequiredAnd( "--timeout=10h" ) );
			  OnlineBackupRequiredArguments requiredArguments = context.RequiredArguments;

			  assertEquals( HOURS.toMillis( 10 ), requiredArguments.Timeout );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTreatNameArgumentAsMandatory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTreatNameArgumentAsMandatory()
		 {
			  Expected.expect( typeof( IncorrectUsage ) );
			  Expected.expectMessage( "Missing argument 'name'" );

			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  handler.CreateContext( "--backup-dir=/" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportDirMustBeAPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportDirMustBeAPath()
		 {
			  Expected.expect( typeof( IncorrectUsage ) );
			  Expected.expectMessage( "cc-report-dir must be a path" );
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  handler.CreateContext( RequiredAnd( "--check-consistency", "--cc-report-dir" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void errorHandledForNonExistingAdditionalConfigFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ErrorHandledForNonExistingAdditionalConfigFile()
		 {
			  // given
			  Path additionalConf = _homeDir.resolve( "neo4j.conf" );

			  // and
			  Expected.expect( typeof( CommandFailed ) );
			  Expected.expectCause( hasCause( any( typeof( IOException ) ) ) );

			  // expect
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  handler.CreateContext( RequiredAnd( "--additional-config=" + additionalConf ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void prioritiseConfigDirOverHomeDir() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PrioritiseConfigDirOverHomeDir()
		 {
			  // given
			  Files.write( _configFile, singletonList( "causal_clustering.minimum_core_cluster_size_at_startup=4" ), WRITE );

			  // and
			  Path homeDirConfigFile = _homeDir.resolve( "neo4j.conf" );
			  Files.write( homeDirConfigFile, asList( "causal_clustering.minimum_core_cluster_size_at_startup=5", "causal_clustering.raft_in_queue_max_batch=21" ) );

			  // when
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  Config config = handler.CreateContext( RequiredAnd() ).Config;

			  // then
			  assertEquals( Convert.ToInt32( 3 ), config.Get( CausalClusteringSettings.minimum_core_cluster_size_at_formation ) );
			  assertEquals( Convert.ToInt32( 128 ), config.Get( CausalClusteringSettings.raft_in_queue_max_batch ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void prioritiseAdditionalOverConfigDir() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PrioritiseAdditionalOverConfigDir()
		 {
			  // given
			  Files.write( _configFile, asList( "causal_clustering.minimum_core_cluster_size_at_startup=4", "causal_clustering.raft_in_queue_max_batch=21" ) );

			  // and
			  Path additionalConf = _homeDir.resolve( "additional-neo4j.conf" );
			  Files.write( additionalConf, singletonList( "causal_clustering.minimum_core_cluster_size_at_startup=5" ) );

			  // when
			  OnlineBackupContextFactory handler = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = handler.CreateContext( RequiredAnd( "--additional-config=" + additionalConf ) );
			  Config config = context.Config;

			  // then
			  assertEquals( Convert.ToInt32( 3 ), config.Get( CausalClusteringSettings.minimum_core_cluster_size_at_formation ) );
			  assertEquals( Convert.ToInt32( 21 ), config.Get( CausalClusteringSettings.raft_in_queue_max_batch ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustIgnorePageCacheConfigInConfigFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustIgnorePageCacheConfigInConfigFile()
		 {
			  // given
			  Files.write( _configFile, singletonList( pagecache_memory.name() + "=42m" ) );

			  // when
			  OnlineBackupContextFactory contextBuilder = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = contextBuilder.CreateContext( RequiredAnd() );

			  // then
			  assertThat( context.Config.get( pagecache_memory ), @is( "8m" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustIgnorePageCacheConfigInAdditionalConfigFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustIgnorePageCacheConfigInAdditionalConfigFile()
		 {
			  // given
			  Path additionalConf = _homeDir.resolve( "additional-neo4j.conf" );
			  Files.write( additionalConf, singletonList( pagecache_memory.name() + "=42m" ) );

			  // when
			  OnlineBackupContextFactory builder = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = builder.CreateContext( RequiredAnd( "--additional-config=" + additionalConf ) );

			  // then
			  assertThat( context.Config.get( pagecache_memory ), @is( "8m" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRespectPageCacheConfigFromCommandLineArguments() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustRespectPageCacheConfigFromCommandLineArguments()
		 {
			  // when
			  OnlineBackupContextFactory builder = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = builder.CreateContext( RequiredAnd( "--pagecache=42m" ) );

			  // then
			  assertThat( context.Config.get( pagecache_memory ), @is( "42m" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logsMustBePlacedInTargetBackupDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogsMustBePlacedInTargetBackupDirectory()
		 {
			  // when
			  string name = "mybackup";
			  Path backupDir = _homeDir.resolve( "poke" );
			  Path backupPath = backupDir.resolve( name );
			  Files.createDirectories( backupDir );
			  OnlineBackupContextFactory builder = new OnlineBackupContextFactory( _homeDir, _configDir );
			  OnlineBackupContext context = builder.CreateContext( "--backup-dir=" + backupDir, "--name=" + name );
			  assertThat( context.Config.get( logical_logs_location ).AbsolutePath, @is( backupPath.ToString() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void defaultProtocolIsAny() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DefaultProtocolIsAny()
		 {
			  // given
			  OnlineBackupContextFactory builder = new OnlineBackupContextFactory( _homeDir, _configDir );

			  // when context resolved without proto override value
			  OnlineBackupContext context = builder.CreateContext( RequiredAnd() );

			  // then
			  assertEquals( ANY, context.RequiredArguments.SelectedBackupProtocol );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void overrideWithLegacy() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OverrideWithLegacy()
		 {
			  // with
			  IList<string> input = Arrays.asList( "common", "catchup" );
			  IList<SelectedBackupProtocol> expected = Arrays.asList( SelectedBackupProtocol.Common, SelectedBackupProtocol.Catchup );

			  for ( int useCase = 0; useCase < input.Count; useCase++ )
			  {
					// given
					OnlineBackupContextFactory builder = new OnlineBackupContextFactory( _homeDir, _configDir );

					// when
					OnlineBackupContext context = builder.CreateContext( RequiredAnd( "--protocol=" + input[useCase] ) );

					// then
					assertEquals( expected[useCase], context.RequiredArguments.SelectedBackupProtocol );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void prometheusShouldBeDisabledToAvoidPortConflicts() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PrometheusShouldBeDisabledToAvoidPortConflicts()
		 {
			  OnlineBackupContext context = ( new OnlineBackupContextFactory( _homeDir, _configDir ) ).createContext( RequiredAnd() );
			  assertEquals( Settings.FALSE, context.Config.Raw["metrics.prometheus.enabled"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ipv6CanBeProcessed() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Ipv6CanBeProcessed()
		 {
			  // given
			  OnlineBackupContextFactory builder = new OnlineBackupContextFactory( _homeDir, _configDir );

			  // when
			  OnlineBackupContext context = builder.CreateContext( RequiredAnd( "--from=[fd00:ce10::2]:6362" ) );

			  // then
			  assertEquals( "fd00:ce10::2", context.RequiredArguments.Address.Hostname.get() );
			  assertEquals( Convert.ToInt32( 6362 ), context.RequiredArguments.Address.Port.Value );
		 }

		 private string[] RequiredAnd( params string[] additionalArgs )
		 {
			  IList<string> args = new List<string>();
			  args.Add( "--backup-dir=/" );
			  args.Add( "--name=mybackup" );
			  Collections.addAll( args, additionalArgs );
			  return args.ToArray();
		 }
	}

}