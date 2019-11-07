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
namespace Neo4Net.Server
{
	using URIBuilder = org.apache.http.client.utils.URIBuilder;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;
	using HTTP = Neo4Net.Test.server.HTTP;
	using InsecureTrustManager = Neo4Net.Test.server.InsecureTrustManager;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.helpers.CommunityServerBuilder.serverOnRandomPorts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.GET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.POST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.RawPayload.quotedJson;

	public class HttpsAccessIT : ExclusiveServerTestBase
	{
		 private SSLSocketFactory _originalSslSocketFactory;
		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _originalSslSocketFactory = HttpsURLConnection.DefaultSSLSocketFactory;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  HttpsURLConnection.DefaultSSLSocketFactory = _originalSslSocketFactory;
			  _server.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverShouldSupportSsl() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ServerShouldSupportSsl()
		 {
			  StartServer();

			  assertThat( GET( HttpsUri() ).status(), @is(200) );
			  assertThat( GET( _server.baseUri().ToString() ).status(), @is(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void txEndpointShouldReplyWithHttpsWhenItReturnsURLs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TxEndpointShouldReplyWithHttpsWhenItReturnsURLs()
		 {
			  StartServer();

			  string baseUri = _server.baseUri().ToString();
			  HTTP.Response response = POST( baseUri + "db/data/transaction", quotedJson( "{'statements':[]}" ) );

			  assertThat( response.Location(), startsWith(baseUri) );
			  assertThat( response.Get( "commit" ).asText(), startsWith(baseUri) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExposeBaseUriWhenHttpEnabledAndHttpsDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExposeBaseUriWhenHttpEnabledAndHttpsDisabled()
		 {
			  StartServer( true, false );

			  URI uri = _server.baseUri();

			  assertEquals( "http", uri.Scheme );
			  HostnamePort expectedHostPort = AddressForConnector( "http" );
			  assertEquals( expectedHostPort.Host, uri.Host );
			  assertEquals( expectedHostPort.Port, uri.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExposeBaseUriWhenHttpDisabledAndHttpsEnabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExposeBaseUriWhenHttpDisabledAndHttpsEnabled()
		 {
			  StartServer( false, true );

			  URI uri = _server.baseUri();

			  assertEquals( "https", uri.Scheme );
			  HostnamePort expectedHostPort = AddressForConnector( "https" );
			  assertEquals( expectedHostPort.Host, uri.Host );
			  assertEquals( expectedHostPort.Port, uri.Port );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startServer() throws Exception
		 private void StartServer()
		 {
			  StartServer( true, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startServer(boolean httpEnabled, boolean httpsEnabled) throws Exception
		 private void StartServer( bool httpEnabled, bool httpsEnabled )
		 {
			  CommunityServerBuilder serverBuilder = serverOnRandomPorts().usingDataDir(Folder.directory(Name.MethodName).AbsolutePath);
			  if ( !httpEnabled )
			  {
					serverBuilder.WithHttpDisabled();
			  }
			  if ( httpsEnabled )
			  {
					serverBuilder.WithHttpsEnabled();
			  }

			  _server = serverBuilder.Build();
			  _server.start();

			  // Because we are generating a non-CA-signed certificate, we need to turn off verification in the client.
			  // This is ironic, since there is no proper verification on the CA side in the first place, but I digress.
			  TrustManager[] trustAllCerts = new TrustManager[] { new InsecureTrustManager() };

			  // Install the all-trusting trust manager
			  SSLContext sc = SSLContext.getInstance( "TLS" );
			  sc.init( null, trustAllCerts, new SecureRandom() );
			  HttpsURLConnection.DefaultSSLSocketFactory = sc.SocketFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String httpsUri() throws Exception
		 private string HttpsUri()
		 {
			  HostnamePort hostPort = AddressForConnector( "https" );
			  assertNotNull( hostPort );

			  return ( new URIBuilder() ).setScheme("https").setHost(hostPort.Host).setPort(hostPort.Port).build().ToString();
		 }

		 private HostnamePort AddressForConnector( string name )
		 {
			  DependencyResolver resolver = _server.database.Graph.DependencyResolver;
			  ConnectorPortRegister portRegister = resolver.ResolveDependency( typeof( ConnectorPortRegister ) );
			  return portRegister.GetLocalAddress( name );
		 }
	}

}