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
namespace Neo4Net.Kernel.impl.query
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Neo4Net.Graphdb;
	using Result = Neo4Net.Graphdb.Result;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using EnterpriseAuthManager = Neo4Net.Kernel.enterprise.api.security.EnterpriseAuthManager;
	using EnterpriseLoginContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogTimeZone = Neo4Net.Logging.LogTimeZone;
	using EmbeddedInteraction = Neo4Net.Server.security.enterprise.auth.EmbeddedInteraction;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.endsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.log_queries;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.log_queries_max_archives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.log_queries_rotation_threshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthSubject.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;

	public class QueryLoggerIT
	{

		 // It is imperative that this test executes using a real filesystem; otherwise rotation failures will not be
		 // detected on Windows.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystem = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystem = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

		 private GraphDatabaseBuilder _databaseBuilder;
		 private const string QUERY = "CREATE (n:Foo {bar: 'baz'})";

		 private File _logsDirectory;
		 private File _logFilename;
		 private EmbeddedInteraction _db;
		 private GraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _logsDirectory = new File( TestDirectory.storeDir(), "logs" );
			  _logFilename = new File( _logsDirectory, "query.log" );
			  AssertableLogProvider inMemoryLog = new AssertableLogProvider();
			  _databaseBuilder = ( new TestEnterpriseGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(FileSystem.get())).setInternalLogProvider(inMemoryLog).newImpermanentDatabaseBuilder(TestDirectory.databaseDir());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _db != null )
			  {
					_db.tearDown();
			  }
			  if ( _database != null )
			  {
					_database.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogCustomUserName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogCustomUserName()
		 {
			  // turn on query logging
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, String> config = stringMap(logs_directory.name(), logsDirectory.getPath(), log_queries.name(), org.neo4j.kernel.configuration.Settings.TRUE);
			  IDictionary<string, string> config = stringMap( logs_directory.name(), _logsDirectory.Path, log_queries.name(), Settings.TRUE );
			  _db = new EmbeddedInteraction( _databaseBuilder, config );

			  // create users
			  _db.LocalUserManager.newUser( "mats", password( "neo4j" ), false );
			  _db.LocalUserManager.newUser( "andres", password( "neo4j" ), false );
			  _db.LocalUserManager.addRoleToUser( "architect", "mats" );
			  _db.LocalUserManager.addRoleToUser( "reader", "andres" );

			  EnterpriseLoginContext mats = _db.login( "mats", "neo4j" );

			  // run query
			  _db.executeQuery( mats, "UNWIND range(0, 10) AS i CREATE (:Foo {p: i})", Collections.emptyMap(), ResourceIterator.close );
			  _db.executeQuery( mats, "CREATE (:Label)", Collections.emptyMap(), ResourceIterator.close );

			  // switch user, run query
			  EnterpriseLoginContext andres = _db.login( "andres", "neo4j" );
			  _db.executeQuery( andres, "MATCH (n:Label) RETURN n", Collections.emptyMap(), ResourceIterator.close );

			  _db.tearDown();

			  // THEN
			  IList<string> logLines = ReadAllLines( _logFilename );

			  assertThat( logLines, hasSize( 3 ) );
			  assertThat( logLines[0], containsString( "mats" ) );
			  assertThat( logLines[1], containsString( "mats" ) );
			  assertThat( logLines[2], containsString( "andres" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogTXMetaDataInQueryLog() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogTXMetaDataInQueryLog()
		 {
			  // turn on query logging
			  _databaseBuilder.setConfig( logs_directory, _logsDirectory.Path );
			  _databaseBuilder.setConfig( log_queries, Settings.TRUE );
			  _db = new EmbeddedInteraction( _databaseBuilder, Collections.emptyMap() );
			  GraphDatabaseFacade graph = _db.LocalGraph;

			  _db.LocalUserManager.setUserPassword( "neo4j", password( "123" ), false );

			  EnterpriseLoginContext subject = _db.login( "neo4j", "123" );
			  _db.executeQuery( subject, "UNWIND range(0, 10) AS i CREATE (:Foo {p: i})", Collections.emptyMap(), ResourceIterator.close );

			  // Set meta data and execute query in transaction
			  using ( InternalTransaction tx = _db.beginLocalTransactionAsUser( subject, KernelTransaction.Type.@explicit ) )
			  {
					graph.Execute( "CALL dbms.setTXMetaData( { User: 'Johan' } )", Collections.emptyMap() );
					graph.Execute( "CALL dbms.procedures() YIELD name RETURN name", Collections.emptyMap() ).close();
					graph.Execute( "MATCH (n) RETURN n", Collections.emptyMap() ).close();
					graph.Execute( QUERY, Collections.emptyMap() );
					tx.Success();
			  }

			  // Ensure that old meta data is not retained
			  using ( InternalTransaction tx = _db.beginLocalTransactionAsUser( subject, KernelTransaction.Type.@explicit ) )
			  {
					graph.Execute( "CALL dbms.setTXMetaData( { Location: 'Sweden' } )", Collections.emptyMap() );
					graph.Execute( "MATCH ()-[r]-() RETURN count(r)", Collections.emptyMap() ).close();
					tx.Success();
			  }

			  _db.tearDown();

			  // THEN
			  IList<string> logLines = ReadAllLines( _logFilename );

			  assertThat( logLines, hasSize( 7 ) );
			  assertThat( logLines[0], not( containsString( "User: 'Johan'" ) ) );
			  // we don't care if setTXMetaData contains the meta data
			  //assertThat( logLines.get( 1 ), containsString( "User: Johan" ) );
			  assertThat( logLines[2], containsString( "User: 'Johan'" ) );
			  assertThat( logLines[3], containsString( "User: 'Johan'" ) );
			  assertThat( logLines[4], containsString( "User: 'Johan'" ) );

			  // we want to make sure that the new transaction does not carry old meta data
			  assertThat( logLines[5], not( containsString( "User: 'Johan'" ) ) );
			  assertThat( logLines[6], containsString( "Location: 'Sweden'" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogQuerySlowerThanThreshold() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogQuerySlowerThanThreshold()
		 {
			  _database = _databaseBuilder.setConfig( log_queries, Settings.TRUE ).setConfig( logs_directory, _logsDirectory.Path ).setConfig( GraphDatabaseSettings.log_queries_parameter_logging_enabled, Settings.FALSE ).newGraphDatabase();

			  ExecuteQueryAndShutdown( _database );

			  IList<string> logLines = ReadAllLines( _logFilename );
			  assertEquals( 1, logLines.Count );
			  assertThat( logLines[0], endsWith( string.Format( " ms: {0} - {1} - {{}}", ClientConnectionInfo(), QUERY ) ) );
			  assertThat( logLines[0], containsString( AUTH_DISABLED.username() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogParametersWhenNestedMap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogParametersWhenNestedMap()
		 {
			  _database = _databaseBuilder.setConfig( log_queries, Settings.TRUE ).setConfig( logs_directory, _logsDirectory.Path ).setConfig( GraphDatabaseSettings.log_queries_parameter_logging_enabled, Settings.TRUE ).newGraphDatabase();

			  IDictionary<string, object> props = new LinkedHashMap<string, object>(); // to be sure about ordering in the last assertion
			  props["name"] = "Roland";
			  props["position"] = "Gunslinger";
			  props["followers"] = Arrays.asList( "Jake", "Eddie", "Susannah" );

			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params["props"] = props;

			  string query = "CREATE ({props})";
			  ExecuteQueryAndShutdown( _database, query, @params );

			  IList<string> logLines = ReadAllLines( _logFilename );
			  assertEquals( 1, logLines.Count );
			  assertThat( logLines[0], endsWith( string.Format( " ms: {0} - {1} - {{props: {{name: 'Roland', position: 'Gunslinger', followers: ['Jake', 'Eddie', 'Susannah']}}}}" + " - {{}}", ClientConnectionInfo(), query ) ) );
			  assertThat( logLines[0], containsString( AUTH_DISABLED.username() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogRuntime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogRuntime()
		 {
			  _database = _databaseBuilder.setConfig( GraphDatabaseSettings.log_queries, Settings.TRUE ).setConfig( GraphDatabaseSettings.logs_directory, _logsDirectory.Path ).setConfig( GraphDatabaseSettings.log_queries_runtime_logging_enabled, Settings.TRUE ).newGraphDatabase();

			  string query = "RETURN 42";
			  ExecuteQueryAndShutdown( _database, query, Collections.emptyMap() );

			  IList<string> logLines = ReadAllLines( _logFilename );
			  assertEquals( 1, logLines.Count );
			  assertThat( logLines[0], endsWith( string.Format( " ms: {0} - {1} - {{}} - runtime=interpreted - {{}}", ClientConnectionInfo(), query ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogParametersWhenList() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogParametersWhenList()
		 {
			  _database = _databaseBuilder.setConfig( log_queries, Settings.TRUE ).setConfig( logs_directory, _logsDirectory.Path ).newGraphDatabase();

			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  @params["ids"] = Arrays.asList( 0, 1, 2 );
			  string query = "MATCH (n) WHERE id(n) in {ids} RETURN n.name";
			  ExecuteQueryAndShutdown( _database, query, @params );

			  IList<string> logLines = ReadAllLines( _logFilename );
			  assertEquals( 1, logLines.Count );
			  assertThat( logLines[0], endsWith( string.Format( " ms: {0} - {1} - {{ids: [0, 1, 2]}} - {{}}", ClientConnectionInfo(), query ) ) );
			  assertThat( logLines[0], containsString( AUTH_DISABLED.username() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disabledQueryLogging()
		 public virtual void DisabledQueryLogging()
		 {
			  _database = _databaseBuilder.setConfig( log_queries, Settings.FALSE ).setConfig( GraphDatabaseSettings.log_queries_filename, _logFilename.Path ).newGraphDatabase();

			  ExecuteQueryAndShutdown( _database );

			  assertFalse( FileSystem.fileExists( _logFilename ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disabledQueryLogRotation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DisabledQueryLogRotation()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File logsDirectory = new java.io.File(testDirectory.storeDir(), "logs");
			  File logsDirectory = new File( TestDirectory.storeDir(), "logs" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File logFilename = new java.io.File(logsDirectory, "query.log");
			  File logFilename = new File( logsDirectory, "query.log" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File shiftedLogFilename1 = new java.io.File(logsDirectory, "query.log.1");
			  File shiftedLogFilename1 = new File( logsDirectory, "query.log.1" );
			  _database = _databaseBuilder.setConfig( log_queries, Settings.TRUE ).setConfig( logs_directory, logsDirectory.Path ).setConfig( log_queries_rotation_threshold, "0" ).newGraphDatabase();

			  // Logging is done asynchronously, so write many times to make sure we would have rotated something
			  for ( int i = 0; i < 100; i++ )
			  {
					_database.execute( QUERY );
			  }

			  _database.shutdown();

			  assertFalse( "There should not exist a shifted log file because rotation is disabled", shiftedLogFilename1.exists() );

			  IList<string> lines = ReadAllLines( logFilename );
			  assertEquals( 100, lines.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryLogRotation()
		 public virtual void QueryLogRotation()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File logsDirectory = new java.io.File(testDirectory.storeDir(), "logs");
			  File logsDirectory = new File( TestDirectory.storeDir(), "logs" );
			  _databaseBuilder.setConfig( log_queries, Settings.TRUE ).setConfig( logs_directory, logsDirectory.Path ).setConfig( log_queries_max_archives, "100" ).setConfig( log_queries_rotation_threshold, "1" );
			  _database = _databaseBuilder.newGraphDatabase();

			  // Logging is done asynchronously, and it turns out it's really hard to make it all work the same on Linux
			  // and on Windows, so just write many times to make sure we rotate several times.

			  for ( int i = 0; i < 100; i++ )
			  {
					_database.execute( QUERY );
			  }

			  _database.shutdown();

			  File[] queryLogs = FileSystem.get().listFiles(logsDirectory, (dir, name) => name.StartsWith("query.log"));
			  assertThat( "Expect to have more then one query log file.", queryLogs.Length, greaterThanOrEqualTo( 2 ) );

			  IList<string> loggedQueries = java.util.queryLogs.Select( this.readAllLinesSilent ).flatMap( System.Collections.ICollection.stream ).ToList();
			  assertThat( "Expected log file to have at least one log entry", loggedQueries, hasSize( 100 ) );

			  _database = _databaseBuilder.newGraphDatabase();
			  // Now modify max_archives and rotation_threshold at runtime, and observe that we end up with fewer larger files
			  _database.execute( "CALL dbms.setConfigValue('" + log_queries_max_archives.name() + "','1')" );
			  _database.execute( "CALL dbms.setConfigValue('" + log_queries_rotation_threshold.name() + "','20m')" );
			  for ( int i = 0; i < 100; i++ )
			  {
					_database.execute( QUERY );
			  }

			  _database.shutdown();

			  queryLogs = FileSystem.get().listFiles(logsDirectory, (dir, name) => name.StartsWith("query.log"));
			  assertThat( "Expect to have more then one query log file.", queryLogs.Length, lessThan( 100 ) );

			  loggedQueries = java.util.queryLogs.Select( this.readAllLinesSilent ).flatMap( System.Collections.ICollection.stream ).ToList();
			  assertThat( "Expected log file to have at least one log entry", loggedQueries.Count, lessThanOrEqualTo( 202 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogPassword() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLogPassword()
		 {
			  _database = _databaseBuilder.setConfig( log_queries, Settings.TRUE ).setConfig( logs_directory, _logsDirectory.Path ).setConfig( GraphDatabaseSettings.auth_enabled, Settings.TRUE ).newGraphDatabase();
			  GraphDatabaseFacade facade = ( GraphDatabaseFacade ) this._database;

			  EnterpriseAuthManager authManager = facade.DependencyResolver.resolveDependency( typeof( EnterpriseAuthManager ) );
			  EnterpriseLoginContext neo = authManager.Login( AuthToken.newBasicAuthToken( "neo4j", "neo4j" ) );

			  string query = "CALL dbms.security.changePassword('abc123')";
			  try
			  {
					  using ( InternalTransaction tx = facade.BeginTransaction( KernelTransaction.Type.@explicit, neo ) )
					  {
						Result res = facade.Execute( tx, query, VirtualValues.EMPTY_MAP );
						res.Close();
						tx.Success();
					  }
			  }
			  finally
			  {
					facade.Shutdown();
			  }

			  IList<string> logLines = ReadAllLines( _logFilename );
			  assertEquals( 1, logLines.Count );
			  assertThat( logLines[0], containsString( "CALL dbms.security.changePassword(******)" ) );
			  assertThat( logLines[0],not( containsString( "abc123" ) ) );
			  assertThat( logLines[0], containsString( neo.Subject().username() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canBeEnabledAndDisabledAtRuntime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanBeEnabledAndDisabledAtRuntime()
		 {
			  _database = _databaseBuilder.setConfig( log_queries, Settings.FALSE ).setConfig( GraphDatabaseSettings.log_queries_filename, _logFilename.Path ).newGraphDatabase();
			  IList<string> strings;

			  try
			  {
					_database.execute( QUERY ).close();

					// File will not be created until query logging is enabled.
					assertFalse( FileSystem.fileExists( _logFilename ) );

					_database.execute( "CALL dbms.setConfigValue('" + log_queries.name() + "', 'true')" ).close();
					_database.execute( QUERY ).close();

					// Both config change and query should exist
					strings = ReadAllLines( _logFilename );
					assertEquals( 2, strings.Count );

					_database.execute( "CALL dbms.setConfigValue('" + log_queries.name() + "', 'false')" ).close();
					_database.execute( QUERY ).close();
			  }
			  finally
			  {
					_database.shutdown();
			  }

			  // Value should not change when disabled
			  strings = ReadAllLines( _logFilename );
			  assertEquals( 2, strings.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logQueriesWithSystemTimeZoneIsConfigured()
		 public virtual void LogQueriesWithSystemTimeZoneIsConfigured()
		 {
			  TimeZone defaultTimeZone = TimeZone.Default;
			  try
			  {
					TimeZone.Default = TimeZone.getTimeZone( ZoneOffset.ofHours( 5 ) );
					ExecuteSingleQueryWithTimeZoneLog();
					TimeZone.Default = TimeZone.getTimeZone( ZoneOffset.ofHours( -5 ) );
					ExecuteSingleQueryWithTimeZoneLog();
					IList<string> allQueries = ReadAllLinesSilent( _logFilename );
					assertTrue( allQueries[0].Contains( "+0500" ) );
					assertTrue( allQueries[1].Contains( "-0500" ) );
			  }
			  finally
			  {
					TimeZone.Default = defaultTimeZone;
			  }
		 }

		 private void ExecuteSingleQueryWithTimeZoneLog()
		 {
			  _database = _databaseBuilder.setConfig( log_queries, Settings.TRUE ).setConfig( GraphDatabaseSettings.db_timezone, LogTimeZone.SYSTEM.name() ).setConfig(logs_directory, _logsDirectory.Path).newGraphDatabase();
			  _database.execute( QUERY ).close();
			  _database.shutdown();
		 }

		 private static void ExecuteQueryAndShutdown( GraphDatabaseService database )
		 {
			  ExecuteQueryAndShutdown( database, QUERY, Collections.emptyMap() );
		 }

		 private static void ExecuteQueryAndShutdown( GraphDatabaseService database, string query, IDictionary<string, object> @params )
		 {
			  Result execute = database.Execute( query, @params );
			  execute.Close();
			  database.Shutdown();
		 }

		 private static string ClientConnectionInfo()
		 {
			  return ClientConnectionInfo.EMBEDDED_CONNECTION.withUsername( AUTH_DISABLED.username() ).asConnectionDetails();
		 }

		 private IList<string> ReadAllLinesSilent( File logFilename )
		 {
			  try
			  {
					return ReadAllLines( FileSystem.get(), logFilename );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<String> readAllLines(java.io.File logFilename) throws java.io.IOException
		 private IList<string> ReadAllLines( File logFilename )
		 {
			  return ReadAllLines( FileSystem.get(), logFilename );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.util.List<String> readAllLines(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File logFilename) throws java.io.IOException
		 internal static IList<string> ReadAllLines( FileSystemAbstraction fs, File logFilename )
		 {
			  IList<string> logLines = new List<string>();
			  // this is needed as the EphemeralFSA is broken, and creates a new file when reading a non-existent file from
			  // a valid directory
			  if ( !fs.FileExists( logFilename ) )
			  {
					throw new FileNotFoundException( "File does not exist." );
			  }

			  using ( StreamReader reader = new StreamReader( fs.OpenAsReader( logFilename, StandardCharsets.UTF_8 ) ) )
			  {
					for ( string line; ( line = reader.ReadLine() ) != null; )
					{
						 logLines.Add( line );
					}
			  }
			  return logLines;
		 }
	}

}