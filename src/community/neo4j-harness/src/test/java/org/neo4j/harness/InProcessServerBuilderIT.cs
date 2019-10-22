using System;

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
namespace Neo4Net.Harness
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using MyUnmanagedExtension = Neo4Net.Harness.extensionpackage.MyUnmanagedExtension;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using ClientAuth = Neo4Net.Ssl.ClientAuth;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.harness.TestServerBuilders.newInProcessBuilder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.ServerTestUtils.connectorAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.ServerTestUtils.verifyConnector;

	public class InProcessServerBuilderIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLaunchAServerInSpecifiedDirectory()
		 public virtual void ShouldLaunchAServerInSpecifiedDirectory()
		 {
			  // Given
			  File workDir = TestDir.directory( "specific" );

			  // When
			  using ( ServerControls server = GetTestServerBuilder( workDir ).newServer() )
			  {
					// Then
					assertThat( HTTP.GET( server.HttpURI().ToString() ).status(), equalTo(200) );
					assertThat( workDir.list().length, equalTo(1) );
			  }

			  // And after it's been closed, it should've cleaned up after itself.
			  assertThat( Arrays.ToString( workDir.list() ), workDir.list().length, equalTo(0) );
		 }

		 private TestServerBuilder GetTestServerBuilder( File workDir )
		 {
			  return newInProcessBuilder( workDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowCustomServerAndDbConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowCustomServerAndDbConfig()
		 {
			  // Given
			  TrustAllSSLCerts();

			  // Get default trusted cypher suites
			  SSLServerSocketFactory ssf = ( SSLServerSocketFactory ) SSLServerSocketFactory.Default;
			  string[] defaultCiphers = ssf.DefaultCipherSuites;

			  // When
			  HttpConnector httpConnector = new HttpConnector( "0", HttpConnector.Encryption.NONE );
			  HttpConnector httpsConnector = new HttpConnector( "1", HttpConnector.Encryption.TLS );
			  using ( ServerControls server = GetTestServerBuilder( TestDir.directory() ).withConfig(httpConnector.Type, "HTTP").withConfig(httpConnector.Enabled, "true").withConfig(httpConnector.Encryption, "NONE").withConfig(httpConnector.ListenAddress, "localhost:0").withConfig(httpsConnector.Type, "HTTP").withConfig(httpsConnector.Enabled, "true").withConfig(httpsConnector.Encryption, "TLS").withConfig(httpsConnector.ListenAddress, "localhost:0").withConfig(GraphDatabaseSettings.dense_node_threshold, "20").withConfig("https.ssl_policy", "test").withConfig("dbms.ssl.policy.test.base_directory", TestDir.directory("certificates").AbsolutePath).withConfig("dbms.ssl.policy.test.allow_key_generation", "true").withConfig("dbms.ssl.policy.test.ciphers", string.join(",", defaultCiphers)).withConfig("dbms.ssl.policy.test.tls_versions", "TLSv1.2, TLSv1.1, TLSv1").withConfig("dbms.ssl.policy.test.client_auth", ClientAuth.NONE.name()).withConfig("dbms.ssl.policy.test.trust_all", "true").newServer() )
			  {
					// Then
					assertThat( HTTP.GET( server.HttpURI().ToString() ).status(), equalTo(200) );
					assertThat( HTTP.GET( server.HttpsURI().get().ToString() ).status(), equalTo(200) );
					AssertDBConfig( server, "20", GraphDatabaseSettings.dense_node_threshold.name() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMountUnmanagedExtensionsByClass()
		 public virtual void ShouldMountUnmanagedExtensionsByClass()
		 {
			  // When
			  using ( ServerControls server = GetTestServerBuilder( TestDir.directory() ).withExtension("/path/to/my/extension", typeof(MyUnmanagedExtension)).newServer() )
			  {
					// Then
					assertThat( HTTP.GET( server.HttpURI().ToString() + "path/to/my/extension/myExtension" ).status(), equalTo(234) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMountUnmanagedExtensionsByPackage()
		 public virtual void ShouldMountUnmanagedExtensionsByPackage()
		 {
			  // When
			  using ( ServerControls server = GetTestServerBuilder( TestDir.directory() ).withExtension("/path/to/my/extension", "org.Neo4Net.harness.extensionpackage").newServer() )
			  {
					// Then
					assertThat( HTTP.GET( server.HttpURI().ToString() + "path/to/my/extension/myExtension" ).status(), equalTo(234) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindFreePort()
		 public virtual void ShouldFindFreePort()
		 {
			  // Given one server is running
			  using ( ServerControls firstServer = GetTestServerBuilder( TestDir.directory() ).newServer() )
			  {
					// When I start a second server
					using ( ServerControls secondServer = GetTestServerBuilder( TestDir.directory() ).newServer() )
					{
						 // Then
						 assertThat( secondServer.HttpURI().Port, not(firstServer.HttpURI().Port) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunBuilderOnExistingStoreDir() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunBuilderOnExistingStoreDir()
		 {
			  // When
			  // create graph db with one node upfront
			  File existingStoreDir = TestDir.directory( "existingStore" );
			  File storeDir = Config.defaults( GraphDatabaseSettings.data_directory, existingStoreDir.toPath().ToString() ).get(GraphDatabaseSettings.database_path);
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
			  try
			  {
					Db.execute( "create ()" );
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  using ( ServerControls server = GetTestServerBuilder( TestDir.databaseDir() ).copyFrom(existingStoreDir).newServer() )
			  {
					// Then
					using ( Transaction tx = server.Graph().beginTx() )
					{
						 ResourceIterable<Node> allNodes = Iterables.asResourceIterable( server.Graph().AllNodes );

						 assertTrue( Iterables.count( allNodes ) > 0 );

						 // When: create another node
						 server.Graph().createNode();
						 tx.Success();
					}
			  }

			  // Then: we still only have one node since the server is supposed to work on a copy
			  db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 assertEquals( 1, Iterables.count( Db.AllNodes ) );
						 tx.Success();
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOpenBoltPort() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOpenBoltPort()
		 {
			  // given
			  using ( ServerControls controls = GetTestServerBuilder( TestDir.directory() ).newServer() )
			  {
					URI uri = controls.BoltURI();

					// when
					( new SocketConnection() ).connect(new HostnamePort(uri.Host, uri.Port));

					// then no exception
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenProvidingANonDirectoryAsSource() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenProvidingANonDirectoryAsSource()
		 {

			  File notADirectory = File.createTempFile( "prefix", "suffix" );
			  assertFalse( notADirectory.Directory );

			  try
			  {
					  using ( ServerControls ignored = GetTestServerBuilder( TestDir.directory() ).copyFrom(notADirectory).newServer() )
					  {
						fail( "server should not start" );
					  }
			  }
			  catch ( Exception rte )
			  {
					Exception cause = rte.InnerException;
					assertTrue( cause is IOException );
					assertTrue( cause.Message.contains( "exists but is not a directory" ) );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnBoltUriWhenMultipleBoltConnectorsConfigured()
		 public virtual void ShouldReturnBoltUriWhenMultipleBoltConnectorsConfigured()
		 {
			  TestServerBuilder serverBuilder = newInProcessBuilder( TestDir.directory() ).withConfig("dbms.connector.another_bolt.type", "BOLT").withConfig("dbms.connector.another_bolt.enabled", "true").withConfig("dbms.connector.another_bolt.listen_address", ":0").withConfig("dbms.connector.bolt.enabled", "true").withConfig("dbms.connector.bolt.listen_address", ":0");

			  using ( ServerControls server = serverBuilder.NewServer() )
			  {
					HostnamePort boltHostPort = connectorAddress( server.Graph(), "bolt" );
					HostnamePort anotherBoltHostPort = connectorAddress( server.Graph(), "another_bolt" );

					assertNotNull( boltHostPort );
					assertNotNull( anotherBoltHostPort );
					assertNotEquals( boltHostPort, anotherBoltHostPort );

					URI boltUri = server.BoltURI();
					assertEquals( "bolt", boltUri.Scheme );
					assertEquals( boltHostPort.Host, boltUri.Host );
					assertEquals( boltHostPort.Port, boltUri.Port );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnBoltUriWhenDefaultBoltConnectorOffAndOtherConnectorConfigured()
		 public virtual void ShouldReturnBoltUriWhenDefaultBoltConnectorOffAndOtherConnectorConfigured()
		 {
			  TestServerBuilder serverBuilder = newInProcessBuilder( TestDir.directory() ).withConfig("dbms.connector.bolt.enabled", "false").withConfig("dbms.connector.another_bolt.type", "BOLT").withConfig("dbms.connector.another_bolt.enabled", "true").withConfig("dbms.connector.another_bolt.listen_address", ":0");

			  using ( ServerControls server = serverBuilder.NewServer() )
			  {
					HostnamePort boltHostPort = connectorAddress( server.Graph(), "bolt" );
					HostnamePort anotherBoltHostPort = connectorAddress( server.Graph(), "another_bolt" );

					assertNull( boltHostPort );
					assertNotNull( anotherBoltHostPort );

					URI boltUri = server.BoltURI();
					assertEquals( "bolt", boltUri.Scheme );
					assertEquals( anotherBoltHostPort.Host, boltUri.Host );
					assertEquals( anotherBoltHostPort.Port, boltUri.Port );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartServerWithHttpHttpsAndBoltDisabled()
		 public virtual void ShouldStartServerWithHttpHttpsAndBoltDisabled()
		 {
			  TestStartupWithConnectors( false, false, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartServerWithHttpEnabledAndHttpsBoltDisabled()
		 public virtual void ShouldStartServerWithHttpEnabledAndHttpsBoltDisabled()
		 {
			  TestStartupWithConnectors( true, false, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartServerWithHttpsEnabledAndHttpBoltDisabled()
		 public virtual void ShouldStartServerWithHttpsEnabledAndHttpBoltDisabled()
		 {
			  TestStartupWithConnectors( false, true, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartServerWithBoltEnabledAndHttpHttpsDisabled()
		 public virtual void ShouldStartServerWithBoltEnabledAndHttpHttpsDisabled()
		 {
			  TestStartupWithConnectors( false, false, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartServerWithHttpHttpsEnabledAndBoltDisabled()
		 public virtual void ShouldStartServerWithHttpHttpsEnabledAndBoltDisabled()
		 {
			  TestStartupWithConnectors( true, true, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartServerWithHttpBoltEnabledAndHttpsDisabled()
		 public virtual void ShouldStartServerWithHttpBoltEnabledAndHttpsDisabled()
		 {
			  TestStartupWithConnectors( true, false, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartServerWithHttpsBoltEnabledAndHttpDisabled()
		 public virtual void ShouldStartServerWithHttpsBoltEnabledAndHttpDisabled()
		 {
			  TestStartupWithConnectors( false, true, true );
		 }

		 private void TestStartupWithConnectors( bool httpEnabled, bool httpsEnabled, bool boltEnabled )
		 {
			  TestServerBuilder serverBuilder = newInProcessBuilder( TestDir.directory() ).withConfig("dbms.connector.http.enabled", Convert.ToString(httpEnabled)).withConfig("dbms.connector.http.listen_address", ":0").withConfig("dbms.connector.https.enabled", Convert.ToString(httpsEnabled)).withConfig("dbms.connector.https.listen_address", ":0").withConfig("dbms.connector.bolt.enabled", Convert.ToString(boltEnabled)).withConfig("dbms.connector.bolt.listen_address", ":0");

			  using ( ServerControls server = serverBuilder.NewServer() )
			  {
					GraphDatabaseService db = server.Graph();

					AssertDbAccessible( db );
					verifyConnector( db, "http", httpEnabled );
					verifyConnector( db, "https", httpsEnabled );
					verifyConnector( db, "bolt", boltEnabled );
			  }
		 }

		 private static void AssertDbAccessible( IGraphDatabaseService db )
		 {
			  Label label = () => "Person";
			  string propertyKey = "name";
			  string propertyValue = "Thor Odinson";

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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertDBConfig(ServerControls server, String expected, String key) throws org.Neo4Net.server.rest.domain.JsonParseException
		 private void AssertDBConfig( ServerControls server, string expected, string key )
		 {
			  JsonNode beans = HTTP.GET( server.HttpURI().ToString() + "db/manage/server/jmx/domain/org.Neo4Net/" ).get("beans");
			  JsonNode configurationBean = FindNamedBean( beans, "Configuration" ).get( "attributes" );
			  bool foundKey = false;
			  foreach ( JsonNode attribute in configurationBean )
			  {
					if ( attribute.get( "name" ).asText().Equals(key) )
					{
						 assertThat( attribute.get( "value" ).asText(), equalTo(expected) );
						 foundKey = true;
						 break;
					}
			  }
			  if ( !foundKey )
			  {
					fail( "No config key '" + key + "'." );
			  }
		 }

		 private JsonNode FindNamedBean( JsonNode beans, string beanName )
		 {
			  foreach ( JsonNode bean in beans )
			  {
					JsonNode name = bean.get( "name" );
					if ( name != null && name.asText().EndsWith(",name=" + beanName) )
					{
						 return bean;
					}
			  }
			  throw new NoSuchElementException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void trustAllSSLCerts() throws java.security.NoSuchAlgorithmException, java.security.KeyManagementException
		 private void TrustAllSSLCerts()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: javax.net.ssl.TrustManager[] trustAllCerts = new javax.net.ssl.TrustManager[]{ new javax.net.ssl.X509TrustManager() { @Override public void checkClientTrusted(java.security.cert.X509Certificate[] arg0, String arg1) { } @Override public void checkServerTrusted(java.security.cert.X509Certificate[] arg0, String arg1) { } @Override public java.security.cert.X509Certificate[] getAcceptedIssuers() { return null; } } };
			  TrustManager[] trustAllCerts = new TrustManager[]{ new X509TrustManager() { public void checkClientTrusted(X509Certificate[] arg0, string arg1) { } public void checkServerTrusted(X509Certificate[] arg0, string arg1) { } public X509Certificate[] AcceptedIssuers { return null; } } };

			  // Install the all-trusting trust manager
			  SSLContext sc = SSLContext.getInstance( "TLS" );
			  sc.init( null, trustAllCerts, new SecureRandom() );
			  HttpsURLConnection.DefaultSSLSocketFactory = sc.SocketFactory;
		 }
	}

}