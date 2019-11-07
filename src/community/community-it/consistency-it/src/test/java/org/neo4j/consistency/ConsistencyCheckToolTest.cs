using System.IO;

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
namespace Neo4Net.Consistency
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;
	using Mockito = org.mockito.Mockito;


	using ToolFailureException = Neo4Net.Consistency.ConsistencyCheckTool.ToolFailureException;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogTimeZone = Neo4Net.Logging.LogTimeZone;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
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
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.logical_logs_location;

	public class ConsistencyCheckToolTest
	{
		private bool InstanceFieldsInitialized = false;

		public ConsistencyCheckToolTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _fs );
		}

		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fs);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void runsConsistencyCheck() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RunsConsistencyCheck()
		 {
			  // given
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  string[] args = new string[] { databaseLayout.DatabaseDirectory().AbsolutePath };
			  ConsistencyCheckService service = mock( typeof( ConsistencyCheckService ) );

			  // when
			  RunConsistencyCheckToolWith( service, args );

			  // then
			  verify( service ).runFullConsistencyCheck( eq( databaseLayout ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), anyBoolean(), any(typeof(ConsistencyFlags)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void consistencyCheckerLogUseSystemTimezoneIfConfigurable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConsistencyCheckerLogUseSystemTimezoneIfConfigurable()
		 {
			  TimeZone defaultTimeZone = TimeZone.Default;
			  try
			  {
					ConsistencyCheckService service = mock( typeof( ConsistencyCheckService ) );
					Mockito.when( service.runFullConsistencyCheck( any( typeof( DatabaseLayout ) ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), eq( false ), any( typeof( ConsistencyFlags ) ) ) ).then(invocationOnMock =>
					{
								LogProvider provider = invocationOnMock.getArgument( 3 );
								provider.getLog( "test" ).info( "testMessage" );
								return ConsistencyCheckService.Result.success( new File( StringUtils.EMPTY ) );
					});
					File storeDir = _testDirectory.directory();
					File configFile = _testDirectory.file( Config.DEFAULT_CONFIG_FILE_NAME );
					Properties properties = new Properties();
					properties.setProperty( GraphDatabaseSettings.db_timezone.name(), LogTimeZone.SYSTEM.name() );
					properties.store( new StreamWriter( configFile ), null );
					string[] args = new string[] { storeDir.Path, "-config", configFile.Path };

					CheckLogRecordTimeZone( service, args, 5, "+0500" );
					CheckLogRecordTimeZone( service, args, -5, "-0500" );
			  }
			  finally
			  {
					TimeZone.Default = defaultTimeZone;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void appliesDefaultTuningConfigurationForConsistencyChecker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AppliesDefaultTuningConfigurationForConsistencyChecker()
		 {
			  // given
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  string[] args = new string[] { databaseLayout.DatabaseDirectory().AbsolutePath };
			  ConsistencyCheckService service = mock( typeof( ConsistencyCheckService ) );

			  // when
			  RunConsistencyCheckToolWith( service, args );

			  // then
			  ArgumentCaptor<Config> config = ArgumentCaptor.forClass( typeof( Config ) );
			  verify( service ).runFullConsistencyCheck( eq( databaseLayout ), config.capture(), any(typeof(ProgressMonitorFactory)), any(typeof(LogProvider)), any(typeof(FileSystemAbstraction)), anyBoolean(), any(typeof(ConsistencyFlags)) );
			  assertFalse( config.Value.get( ConsistencyCheckSettings.ConsistencyCheckPropertyOwners ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void passesOnConfigurationIfProvided() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PassesOnConfigurationIfProvided()
		 {
			  // given
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  File configFile = _testDirectory.file( Config.DEFAULT_CONFIG_FILE_NAME );
			  Properties properties = new Properties();
			  properties.setProperty( ConsistencyCheckSettings.ConsistencyCheckPropertyOwners.name(), "true" );
			  properties.store( new StreamWriter( configFile ), null );

			  string[] args = new string[] { databaseLayout.DatabaseDirectory().AbsolutePath, "-config", configFile.Path };
			  ConsistencyCheckService service = mock( typeof( ConsistencyCheckService ) );

			  // when
			  RunConsistencyCheckToolWith( service, args );

			  // then
			  ArgumentCaptor<Config> config = ArgumentCaptor.forClass( typeof( Config ) );
			  verify( service ).runFullConsistencyCheck( eq( databaseLayout ), config.capture(), any(typeof(ProgressMonitorFactory)), any(typeof(LogProvider)), any(typeof(FileSystemAbstraction)), anyBoolean(), any(typeof(ConsistencyFlags)) );
			  assertTrue( config.Value.get( ConsistencyCheckSettings.ConsistencyCheckPropertyOwners ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exitWithFailureIndicatingCorrectUsageIfNoArgumentsSupplied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExitWithFailureIndicatingCorrectUsageIfNoArgumentsSupplied()
		 {
			  // given
			  ConsistencyCheckService service = mock( typeof( ConsistencyCheckService ) );
			  string[] args = new string[] {};

			  try
			  {
					// when
					RunConsistencyCheckToolWith( service, args );
					fail( "should have thrown exception" );
			  }
			  catch ( ConsistencyCheckTool.ToolFailureException e )
			  {
					// then
					assertThat( e.Message, containsString( "USAGE:" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exitWithFailureIfConfigSpecifiedButConfigFileDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExitWithFailureIfConfigSpecifiedButConfigFileDoesNotExist()
		 {
			  // given
			  File configFile = _testDirectory.file( "nonexistent_file" );
			  string[] args = new string[] { _testDirectory.directory().Path, "-config", configFile.Path };
			  ConsistencyCheckService service = mock( typeof( ConsistencyCheckService ) );

			  try
			  {
					// when
					RunConsistencyCheckToolWith( service, args );
					fail( "should have thrown exception" );
			  }
			  catch ( ConsistencyCheckTool.ToolFailureException e )
			  {
					// then
					assertThat( e.Message, containsString( "Could not read configuration file" ) );
					assertThat( e.InnerException.Message, containsString( "does not exist" ) );
			  }

			  verifyZeroInteractions( service );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.consistency.ConsistencyCheckTool.ToolFailureException.class) public void failWhenStoreWasNonCleanlyShutdown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailWhenStoreWasNonCleanlyShutdown()
		 {
			  CreateGraphDbAndKillIt( Config.defaults() );

			  RunConsistencyCheckToolWith( _fs.get(), _testDirectory.databaseDir().AbsolutePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.consistency.ConsistencyCheckTool.ToolFailureException.class) public void failOnNotCleanlyShutdownStoreWithLogsInCustomRelativeLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailOnNotCleanlyShutdownStoreWithLogsInCustomRelativeLocation()
		 {
			  File customConfigFile = _testDirectory.file( "customConfig" );
			  Config customConfig = Config.defaults( logical_logs_location, "otherLocation" );
			  CreateGraphDbAndKillIt( customConfig );
			  MapUtil.store( customConfig.Raw, _fs.openAsOutputStream( customConfigFile, false ) );
			  string[] args = new string[] { _testDirectory.databaseDir().Path, "-config", customConfigFile.Path };

			  RunConsistencyCheckToolWith( _fs.get(), args );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.consistency.ConsistencyCheckTool.ToolFailureException.class) public void failOnNotCleanlyShutdownStoreWithLogsInCustomAbsoluteLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailOnNotCleanlyShutdownStoreWithLogsInCustomAbsoluteLocation()
		 {
			  File customConfigFile = _testDirectory.file( "customConfig" );
			  File otherLocation = _testDirectory.directory( "otherLocation" );
			  Config customConfig = Config.defaults( logical_logs_location, otherLocation.AbsolutePath );
			  CreateGraphDbAndKillIt( customConfig );
			  MapUtil.store( customConfig.Raw, customConfigFile );
			  string[] args = new string[] { _testDirectory.databaseDir().Path, "-config", customConfigFile.Path };

			  RunConsistencyCheckToolWith( _fs.get(), args );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkLogRecordTimeZone(ConsistencyCheckService service, String[] args, int hoursShift, String timeZoneSuffix) throws Neo4Net.consistency.ConsistencyCheckTool.ToolFailureException, java.io.IOException
		 private static void CheckLogRecordTimeZone( ConsistencyCheckService service, string[] args, int hoursShift, string timeZoneSuffix )
		 {
			  TimeZone.Default = TimeZone.getTimeZone( ZoneOffset.ofHours( hoursShift ) );
			  MemoryStream outputStream = new MemoryStream();
			  PrintStream printStream = new PrintStream( outputStream );
			  RunConsistencyCheckToolWith( service, printStream, args );
			  string logLine = ReadLogLine( outputStream );
			  assertTrue( logLine, logLine.Contains( timeZoneSuffix ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String readLogLine(java.io.ByteArrayOutputStream outputStream) throws java.io.IOException
		 private static string ReadLogLine( MemoryStream outputStream )
		 {
			  MemoryStream byteArrayInputStream = new MemoryStream( outputStream.toByteArray() );
			  StreamReader bufferedReader = new StreamReader( byteArrayInputStream );
			  return bufferedReader.ReadLine();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createGraphDbAndKillIt(Neo4Net.kernel.configuration.Config config) throws Exception
		 private void CreateGraphDbAndKillIt( Config config )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.GraphDatabaseService db = new Neo4Net.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabaseBuilder(testDirectory.databaseDir()).setConfig(config.getRaw()).newGraphDatabase();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs.get()).newImpermanentDatabaseBuilder(_testDirectory.databaseDir()).setConfig(config.Raw).newGraphDatabase();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( label( "FOO" ) );
					Db.createNode( label( "BAR" ) );
					tx.Success();
			  }

			  _fs.snapshot( Db.shutdown );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void runConsistencyCheckToolWith(Neo4Net.io.fs.FileSystemAbstraction fileSystem, String... args) throws Neo4Net.consistency.ConsistencyCheckTool.ToolFailureException
		 private static void RunConsistencyCheckToolWith( FileSystemAbstraction fileSystem, params string[] args )
		 {
			  ( new ConsistencyCheckTool( mock( typeof( ConsistencyCheckService ) ), fileSystem, mock( typeof( PrintStream ) ), mock( typeof( PrintStream ) ) ) ).run( args );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void runConsistencyCheckToolWith(ConsistencyCheckService consistencyCheckService, String... args) throws Neo4Net.consistency.ConsistencyCheckTool.ToolFailureException, java.io.IOException
		 private static void RunConsistencyCheckToolWith( ConsistencyCheckService consistencyCheckService, params string[] args )
		 {
			  RunConsistencyCheckToolWith( consistencyCheckService, mock( typeof( PrintStream ) ), args );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void runConsistencyCheckToolWith(ConsistencyCheckService consistencyCheckService, java.io.PrintStream printStream, String... args) throws Neo4Net.consistency.ConsistencyCheckTool.ToolFailureException, java.io.IOException
		 private static void RunConsistencyCheckToolWith( ConsistencyCheckService consistencyCheckService, PrintStream printStream, params string[] args )
		 {
			  using ( FileSystemAbstraction fileSystemAbstraction = new DefaultFileSystemAbstraction() )
			  {
					( new ConsistencyCheckTool( consistencyCheckService, fileSystemAbstraction, printStream, printStream ) ).Run( args );
			  }
		 }
	}

}