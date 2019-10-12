using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Ext.Udc.impl
{
	using FileUtils = org.apache.commons.io.FileUtils;
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using HttpHost = org.apache.http.HttpHost;
	using HttpGet = org.apache.http.client.methods.HttpGet;
	using DefaultHttpClient = org.apache.http.impl.client.DefaultHttpClient;
	using LocalServerTestBase = org.apache.http.localserver.LocalServerTestBase;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using OnlineBackupSettings = Neo4Net.backup.OnlineBackupSettings;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using RegexMatcher = Neo4Net.Test.mockito.matcher.RegexMatcher;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using UsageData = Neo4Net.Udc.UsageData;
	using UsageDataKeys = Neo4Net.Udc.UsageDataKeys;
	using Neo4Net.Util.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.CLUSTER_HASH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.DATABASE_MODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.EDITION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.MAC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.REGISTRATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.SOURCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.TAGS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.USER_AGENTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.VERSION;

	/// <summary>
	/// Unit testing for the UDC kernel extension.
	/// <para>
	/// The UdcExtensionImpl is loaded when a new
	/// GraphDatabase is instantiated, as part of
	/// <seealso cref="org.neo4j.helpers.Service.load"/>.
	/// </para>
	/// </summary>
	public class UdcExtensionImplIT : LocalServerTestBase
	{
		 private const string VERSION_PATTERN = "(\\d\\.\\d+(([.-]).*)?)|(dev)";
		 private static readonly Condition<int> _isZero = value => value == 0;
		 private static readonly Condition<int> _isGreaterThanZero = value => value > 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory path = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Path = TestDirectory.testDirectory();

		 private PingerHandler _handler;
		 private IDictionary<string, string> _config;
		 private GraphDatabaseService _graphdb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override void SetUp()
		 {
			  base.SetUp();
			  UdcTimerTask.SuccessCounts.Clear();
			  UdcTimerTask.FailureCounts.Clear();
			  _handler = new PingerHandler();
			  serverBootstrap.registerHandler( "/*", _handler );
			  HttpHost target = start();

			  int servicePort = target.Port;
			  string serviceHostName = target.HostName;
			  string serverAddress = serviceHostName + ":" + servicePort;

			  _config = new Dictionary<string, string>();
			  _config[UdcSettings.first_delay.name()] = "100";
			  _config[UdcSettings.udc_host.name()] = serverAddress;
			  _config[OnlineBackupSettings.online_backup_enabled.name()] = Settings.FALSE;

			  BlockUntilServerAvailable( new URL( "http", serviceHostName, servicePort, "/" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  Cleanup( _graphdb );
			  if ( httpclient != null )
			  {
					httpclient.close();
			  }
			  if ( server != null )
			  {
					server.shutdown( 0, TimeUnit.MILLISECONDS );
			  }
		 }

		 /// <summary>
		 /// Expect the counts to be initialized.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadWhenNormalGraphdbIsCreated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadWhenNormalGraphdbIsCreated()
		 {
			  // When
			  IDictionary<string, string> config = Collections.singletonMap( OnlineBackupSettings.online_backup_enabled.name(), Settings.FALSE );
			  _graphdb = CreateDatabase( config );

			  // Then, when the UDC extension successfully loads, it initializes the attempts count to 0
			  AssertGotSuccessWithRetry( _isZero );
		 }

		 /// <summary>
		 /// Expect separate counts for each graphdb.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadForEachCreatedGraphdb() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadForEachCreatedGraphdb()
		 {
			  IDictionary<string, string> config = Collections.singletonMap( OnlineBackupSettings.online_backup_enabled.name(), Settings.FALSE );
			  GraphDatabaseService graphdb1 = CreateDatabase( Path.directory( "first-db" ), config );
			  GraphDatabaseService graphdb2 = CreateDatabase( Path.directory( "second-db" ), config );
			  ISet<string> successCountValues = UdcTimerTask.SuccessCounts.Keys;
			  assertThat( successCountValues.Count, equalTo( 2 ) );
			  assertThat( "this", @is( not( "that" ) ) );
			  Cleanup( graphdb1 );
			  Cleanup( graphdb2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecordFailuresWhenThereIsNoServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecordFailuresWhenThereIsNoServer()
		 {
			  // When
			  _graphdb = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Path.directory("should-record-failures")).setConfig(UdcSettings.first_delay, "100").setConfig(UdcSettings.udc_host, "127.0.0.1:1").setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

			  // Then
			  AssertGotFailureWithRetry( _isGreaterThanZero );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecordSuccessesWhenThereIsAServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecordSuccessesWhenThereIsAServer()
		 {
			  // When
			  _graphdb = CreateDatabase( _config );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  AssertGotFailureWithRetry( _isZero );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSpecifySourceWithConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSpecifySourceWithConfig()
		 {
			  // When
			  _config[UdcSettings.udc_source.name()] = "unit-testing";
			  _graphdb = CreateDatabase( _config );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  assertEquals( "unit-testing", _handler.QueryMap[SOURCE] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecordDatabaseMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecordDatabaseMode()
		 {
			  // When
			  _graphdb = CreateDatabase( _config );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  assertEquals( "single", _handler.QueryMap[DATABASE_MODE] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecordClusterName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecordClusterName()
		 {
			  // When
			  _graphdb = CreateDatabase( _config );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );

			  string hashOfDefaultClusterName = "1108231321";
			  assertEquals( hashOfDefaultClusterName, _handler.QueryMap[CLUSTER_HASH] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void blockUntilServerAvailable(final java.net.URL url) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void BlockUntilServerAvailable( URL url )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PointerTo<bool> flag = new PointerTo<>(false);
			  PointerTo<bool> flag = new PointerTo<bool>( false );

			  Thread t = new Thread(() =>
			  {
				while ( !flag.Value )
				{
					 try
					 {
						  HttpGet httpget = new HttpGet( url.toURI() );
						  httpget.addHeader( "Accept", "application/json" );
						  DefaultHttpClient client = new DefaultHttpClient();
						  client.execute( httpget );

						  // If we get here, the server's ready
						  flag.Value = true;
						  latch.Signal();
					 }
					 catch ( Exception e )
					 {
						  throw new Exception( e );
					 }
				}
			  });

			  t.run();

			  assertTrue( latch.await( 1000, TimeUnit.MILLISECONDS ) );

			  t.Join();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReadDefaultRegistration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReadDefaultRegistration()
		 {
			  // When
			  _graphdb = CreateDatabase( _config );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  assertEquals( "unreg", _handler.QueryMap[REGISTRATION] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDetermineTestTagFromClasspath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDetermineTestTagFromClasspath()
		 {
			  // When
			  _graphdb = CreateDatabase( _config );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  assertEquals( "test,appserver,web", _handler.QueryMap[TAGS] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDetermineEditionFromClasspath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDetermineEditionFromClasspath()
		 {
			  // When
			  _graphdb = CreateDatabase( _config );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  assertEquals( Edition.enterprise.name(), _handler.QueryMap[EDITION] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDetermineUserAgent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDetermineUserAgent()
		 {
			  // Given
			  _graphdb = CreateDatabase( _config );

			  // When
			  MakeRequestWithAgent( "test/1.0" );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  assertEquals( "test/1.0", _handler.QueryMap[USER_AGENTS] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDetermineUserAgents() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDetermineUserAgents()
		 {
			  // Given
			  _graphdb = CreateDatabase( _config );

			  // When
			  MakeRequestWithAgent( "test/1.0" );
			  MakeRequestWithAgent( "foo/bar" );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  string userAgents = _handler.QueryMap[USER_AGENTS];
			  assertTrue( userAgents.Contains( "test/1.0" ) );
			  assertTrue( userAgents.Contains( "foo/bar" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeMacAddressInConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeMacAddressInConfig()
		 {
			  // When
			  _graphdb = CreateDatabase( _config );

			  // Then
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  assertNotNull( _handler.QueryMap[MAC] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludePrefixedSystemProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludePrefixedSystemProperties()
		 {
			  WithSystemProperty(UdcConstants.UDC_PROPERTY_PREFIX + ".test", "udc-property", () =>
			  {
				WithSystemProperty("os.test", "os-property", () =>
				{
					 _graphdb = CreateDatabase( _config );
					 AssertGotSuccessWithRetry( _isGreaterThanZero );
					 assertEquals( "udc-property", _handler.QueryMap["test"] );
					 assertEquals( "os-property", _handler.QueryMap["os.test"] );
					 return null;
				});
				return null;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeDistributionForWindows() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotIncludeDistributionForWindows()
		 {
			  WithSystemProperty("os.name", "Windows", () =>
			  {
				_graphdb = CreateDatabase( _config );
				AssertGotSuccessWithRetry( _isGreaterThanZero );
				assertEquals( UdcConstants.UNKNOWN_DIST, _handler.QueryMap["dist"] );
				return null;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeDistributionForLinux() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeDistributionForLinux()
		 {
			  if ( !SystemUtils.IS_OS_LINUX )
			  {
					return;
			  }
			  _graphdb = CreateDatabase( _config );
			  AssertGotSuccessWithRetry( _isGreaterThanZero );

			  assertEquals( DefaultUdcInformationCollector.SearchForPackageSystems(), _handler.QueryMap["dist"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeDistributionForMacOS() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotIncludeDistributionForMacOS()
		 {
			  WithSystemProperty("os.name", "Mac OS X", () =>
			  {
				_graphdb = CreateDatabase( _config );
				AssertGotSuccessWithRetry( _isGreaterThanZero );
				assertEquals( UdcConstants.UNKNOWN_DIST, _handler.QueryMap["dist"] );
				return null;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeVersionInConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeVersionInConfig()
		 {
			  _graphdb = CreateDatabase( _config );
			  AssertGotSuccessWithRetry( _isGreaterThanZero );
			  string version = _handler.QueryMap[VERSION];
			  assertThat( version, new RegexMatcher( Pattern.compile( VERSION_PATTERN ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverrideSourceWithSystemProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOverrideSourceWithSystemProperty()
		 {
			  WithSystemProperty(UdcSettings.udc_source.name(), "overridden", () =>
			  {
				_graphdb = CreateDatabase( Path.directory( "db-with-property" ), _config );
				AssertGotSuccessWithRetry( _isGreaterThanZero );
				string source = _handler.QueryMap[SOURCE];
				assertEquals( "overridden", source );
				return null;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchAllValidVersions()
		 public virtual void ShouldMatchAllValidVersions()
		 {
			  assertTrue( "1.8.M07".matches( VERSION_PATTERN ) );
			  assertTrue( "1.8.RC1".matches( VERSION_PATTERN ) );
			  assertTrue( "1.8.GA".matches( VERSION_PATTERN ) );
			  assertTrue( "1.8".matches( VERSION_PATTERN ) );
			  assertTrue( "1.9".matches( VERSION_PATTERN ) );
			  assertTrue( "1.9-SNAPSHOT".matches( VERSION_PATTERN ) );
			  assertTrue( "2.0-SNAPSHOT".matches( VERSION_PATTERN ) );
			  assertTrue( "1.9.M01".matches( VERSION_PATTERN ) );
			  assertTrue( "1.10".matches( VERSION_PATTERN ) );
			  assertTrue( "1.10-SNAPSHOT".matches( VERSION_PATTERN ) );
			  assertTrue( "1.10.M01".matches( VERSION_PATTERN ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFilterPlusBuildNumbers()
		 public virtual void ShouldFilterPlusBuildNumbers()
		 {
			  assertThat( DefaultUdcInformationCollector.FilterVersionForUDC( "1.9.0-M01+00001" ), @is( equalTo( "1.9.0-M01" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFilterSnapshotBuildNumbers()
		 public virtual void ShouldNotFilterSnapshotBuildNumbers()
		 {
			  assertThat( DefaultUdcInformationCollector.FilterVersionForUDC( "2.0-SNAPSHOT" ), @is( equalTo( "2.0-SNAPSHOT" ) ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFilterReleaseBuildNumbers()
		 public virtual void ShouldNotFilterReleaseBuildNumbers()
		 {
			  assertThat( DefaultUdcInformationCollector.FilterVersionForUDC( "1.9" ), @is( equalTo( "1.9" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseTheCustomConfiguration()
		 public virtual void ShouldUseTheCustomConfiguration()
		 {
			  // Given
			  _config[UdcSettings.udc_source.name()] = "my_source";
			  _config[UdcSettings.udc_registration_key.name()] = "my_key";

			  // When
			  _graphdb = CreateDatabase( _config );

			  // Then
			  Config config = ( ( GraphDatabaseAPI ) _graphdb ).DependencyResolver.resolveDependency( typeof( Config ) );

			  assertEquals( "my_source", config.Get( UdcSettings.udc_source ) );
			  assertEquals( "my_key", config.Get( UdcSettings.udc_registration_key ) );
		 }

		 private interface Condition<T>
		 {
			  bool IsTrue( T value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertGotSuccessWithRetry(Condition<int> condition) throws Exception
		 private static void AssertGotSuccessWithRetry( Condition<int> condition )
		 {
			  AssertGotPingWithRetry( UdcTimerTask.SuccessCounts, condition );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertGotFailureWithRetry(Condition<int> condition) throws Exception
		 private static void AssertGotFailureWithRetry( Condition<int> condition )
		 {
			  AssertGotPingWithRetry( UdcTimerTask.FailureCounts, condition );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertGotPingWithRetry(java.util.Map<String,int> counts, Condition<int> condition) throws Exception
		 private static void AssertGotPingWithRetry( IDictionary<string, int> counts, Condition<int> condition )
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					IDictionary<string, int>.ValueCollection countValues = Counts.Values;
					int? count = countValues.GetEnumerator().next();
					if ( condition.IsTrue( count.Value ) )
					{
						 return;
					}
					Thread.Sleep( 200 );
			  }
			  fail();
		 }

		 private GraphDatabaseService CreateDatabase( IDictionary<string, string> config )
		 {
			  return CreateDatabase( null, config );
		 }

		 private GraphDatabaseService CreateDatabase( File storeDir, IDictionary<string, string> config )
		 {
			  TestEnterpriseGraphDatabaseFactory factory = new TestEnterpriseGraphDatabaseFactory();
			  GraphDatabaseBuilder graphDatabaseBuilder = ( storeDir != null ) ? factory.NewImpermanentDatabaseBuilder( storeDir ) : factory.NewImpermanentDatabaseBuilder();
			  if ( config != null )
			  {
					graphDatabaseBuilder.Config = config;
			  }

			  return graphDatabaseBuilder.NewGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void cleanup(org.neo4j.graphdb.GraphDatabaseService gdb) throws java.io.IOException
		 private void Cleanup( GraphDatabaseService gdb )
		 {
			  if ( gdb != null )
			  {
					GraphDatabaseAPI db = ( GraphDatabaseAPI ) gdb;
					gdb.Shutdown();
					FileUtils.deleteDirectory( Db.databaseLayout().databaseDirectory() );
			  }
		 }

		 private class PointerTo<T>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal T ValueConflict;

			  internal PointerTo( T value )
			  {
					this.ValueConflict = value;
			  }

			  public virtual T Value
			  {
				  get
				  {
						return ValueConflict;
				  }
				  set
				  {
						this.ValueConflict = value;
				  }
			  }

		 }

		 private void MakeRequestWithAgent( string agent )
		 {
			  RecentK<string> clients = ( ( GraphDatabaseAPI ) _graphdb ).DependencyResolver.resolveDependency( typeof( UsageData ) ).get( UsageDataKeys.clientNames );
			  clients.Add( agent );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void withSystemProperty(String name, String value, java.util.concurrent.Callable<Void> block) throws Exception
		 private void WithSystemProperty( string name, string value, Callable<Void> block )
		 {
			  string original = System.getProperty( name );
			  System.setProperty( name, value );
			  try
			  {
					block.call();
			  }
			  finally
			  {
					if ( string.ReferenceEquals( original, null ) )
					{
						 System.clearProperty( name );
					}
					else
					{
						 System.setProperty( name, original );
					}
			  }
		 }
	}

}