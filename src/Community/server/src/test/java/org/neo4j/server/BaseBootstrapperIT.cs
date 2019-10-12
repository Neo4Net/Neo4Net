using System.Collections.Generic;

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
namespace Neo4Net.Server
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.config;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.data_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.forced_kernel_id;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.store;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.ServerTestUtils.verifyConnector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public abstract class BaseBootstrapperIT : ExclusiveServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder tempDir = new org.junit.rules.TemporaryFolder();
		 public TemporaryFolder TempDir = new TemporaryFolder();

		 protected internal ServerBootstrapper Bootstrapper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  Bootstrapper = NewBootstrapper();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  if ( Bootstrapper != null )
			  {
					Bootstrapper.stop();
			  }
		 }

		 protected internal abstract ServerBootstrapper NewBootstrapper();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartStopNeoServerWithoutAnyConfigFiles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartStopNeoServerWithoutAnyConfigFiles()
		 {
			  // When
			  int resultCode = ServerBootstrapper.Start( Bootstrapper, WithConnectorsOnRandomPortsConfig( "--home-dir", TempDir.newFolder( "home-dir" ).AbsolutePath, "-c", ConfigOption( data_directory, TempDir.Root.AbsolutePath ), "-c", ConfigOption( logs_directory, TempDir.Root.AbsolutePath ), "-c", "dbms.backup.enabled=false" ) );

			  // Then
			  assertEquals( ServerBootstrapper.OK, resultCode );
			  assertEventually( "Server was not started", Bootstrapper.isRunning, @is( true ), 1, TimeUnit.MINUTES );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canSpecifyConfigFile() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanSpecifyConfigFile()
		 {
			  // Given
			  File configFile = TempDir.newFile( Config.DEFAULT_CONFIG_FILE_NAME );

			  IDictionary<string, string> properties = stringMap( forced_kernel_id.name(), "ourcustomvalue" );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  properties.putAll( ServerTestUtils.DefaultRelativeProperties );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  properties.putAll( ConnectorsOnRandomPortsConfig() );

			  store( properties, configFile );

			  // When
			  ServerBootstrapper.Start( Bootstrapper, "--home-dir", TempDir.newFolder( "home-dir" ).AbsolutePath, "--config-dir", configFile.ParentFile.AbsolutePath );

			  // Then
			  assertThat( Bootstrapper.Server.Config.get( forced_kernel_id ), equalTo( "ourcustomvalue" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canOverrideConfigValues() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanOverrideConfigValues()
		 {
			  // Given
			  File configFile = TempDir.newFile( Config.DEFAULT_CONFIG_FILE_NAME );

			  IDictionary<string, string> properties = stringMap( forced_kernel_id.name(), "thisshouldnotshowup" );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  properties.putAll( ServerTestUtils.DefaultRelativeProperties );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  properties.putAll( ConnectorsOnRandomPortsConfig() );

			  store( properties, configFile );

			  // When
			  ServerBootstrapper.Start( Bootstrapper, "--home-dir", TempDir.newFolder( "home-dir" ).AbsolutePath, "--config-dir", configFile.ParentFile.AbsolutePath, "-c", ConfigOption( forced_kernel_id, "mycustomvalue" ) );

			  // Then
			  assertThat( Bootstrapper.Server.Config.get( forced_kernel_id ), equalTo( "mycustomvalue" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithHttpHttpsAndBoltDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartWithHttpHttpsAndBoltDisabled()
		 {
			  TestStartupWithConnectors( false, false, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithHttpEnabledAndHttpsBoltDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartWithHttpEnabledAndHttpsBoltDisabled()
		 {
			  TestStartupWithConnectors( true, false, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithHttpsEnabledAndHttpBoltDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartWithHttpsEnabledAndHttpBoltDisabled()
		 {
			  TestStartupWithConnectors( false, true, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithBoltEnabledAndHttpHttpsDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartWithBoltEnabledAndHttpHttpsDisabled()
		 {
			  TestStartupWithConnectors( false, false, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithHttpHttpsEnabledAndBoltDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartWithHttpHttpsEnabledAndBoltDisabled()
		 {
			  TestStartupWithConnectors( true, true, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithHttpBoltEnabledAndHttpsDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartWithHttpBoltEnabledAndHttpsDisabled()
		 {
			  TestStartupWithConnectors( true, false, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithHttpsBoltEnabledAndHttpDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartWithHttpsBoltEnabledAndHttpDisabled()
		 {
			  TestStartupWithConnectors( false, true, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testStartupWithConnectors(boolean httpEnabled, boolean httpsEnabled, boolean boltEnabled) throws Exception
		 private void TestStartupWithConnectors( bool httpEnabled, bool httpsEnabled, bool boltEnabled )
		 {
			  int resultCode = ServerBootstrapper.Start( Bootstrapper, "--home-dir", TempDir.newFolder( "home-dir" ).AbsolutePath, "-c", ConfigOption( data_directory, TempDir.Root.AbsolutePath ), "-c", ConfigOption( logs_directory, TempDir.Root.AbsolutePath ), "-c", "dbms.connector.http.enabled=" + httpEnabled, "-c", "dbms.connector.http.listen_address=:0", "-c", "dbms.connector.https.enabled=" + httpsEnabled, "-c", "dbms.connector.https.listen_address=:0", "-c", "dbms.connector.bolt.enabled=" + boltEnabled, "-c", "dbms.connector.bolt.listen_address=:0" );

			  assertEquals( ServerBootstrapper.OK, resultCode );
			  assertEventually( "Server was not started", Bootstrapper.isRunning, @is( true ), 1, TimeUnit.MINUTES );
			  AssertDbAccessibleAsEmbedded();

			  verifyConnector( Db(), "http", httpEnabled );
			  verifyConnector( Db(), "https", httpsEnabled );
			  verifyConnector( Db(), "bolt", boltEnabled );
		 }

		 protected internal virtual string ConfigOption<T1>( Setting<T1> setting, string value )
		 {
			  return setting.Name() + "=" + value;
		 }

		 protected internal static string[] WithConnectorsOnRandomPortsConfig( params string[] otherConfigs )
		 {
			  Stream<string> configs = Stream.of( otherConfigs );

			  Stream<string> connectorsConfig = ConnectorsOnRandomPortsConfig().SetOfKeyValuePairs().Select(entry => entry.Key + "=" + entry.Value).flatMap(config => Stream.of("-c", config));

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.concat( configs, connectorsConfig ).toArray( string[]::new );
		 }

		 protected internal static IDictionary<string, string> ConnectorsOnRandomPortsConfig()
		 {
			  return stringMap( "dbms.connector.http.type", "HTTP", "dbms.connector.http.listen_address", "localhost:0", "dbms.connector.http.encryption", "NONE", "dbms.connector.http.enabled", "true", "dbms.connector.https.type", "HTTP", "dbms.connector.https.listen_address", "localhost:0", "dbms.connector.https.encryption", "TLS", "dbms.connector.https.enabled", "true", "dbms.connector.bolt.type", "BOLT", "dbms.connector.bolt.listen_address", "localhost:0", "dbms.connector.bolt.tls_level", "OPTIONAL", "dbms.connector.bolt.enabled", "true" );
		 }

		 private void AssertDbAccessibleAsEmbedded()
		 {
			  GraphDatabaseAPI db = db();

			  Label label = () => "Node";
			  string propertyKey = "key";
			  string propertyValue = "value";

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( label ).setProperty( propertyKey, propertyValue );
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = single( Db.findNodes( label ) );
					assertEquals( propertyValue, node.GetProperty( propertyKey ) );
					tx.Success();
			  }
		 }

		 private GraphDatabaseAPI Db()
		 {
			  return Bootstrapper.Server.Database.Graph;
		 }
	}

}