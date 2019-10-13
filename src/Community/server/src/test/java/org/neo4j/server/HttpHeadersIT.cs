using System.Collections.Generic;

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
	using Client = com.sun.jersey.api.client.Client;
	using ClientRequest = com.sun.jersey.api.client.ClientRequest;
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;
	using ClientConfig = com.sun.jersey.api.client.config.ClientConfig;
	using DefaultClientConfig = com.sun.jersey.api.client.config.DefaultClientConfig;
	using HTTPSProperties = com.sun.jersey.client.urlconnection.HTTPSProperties;
	using After = org.junit.After;
	using Test = org.junit.Test;


	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;
	using InsecureTrustManager = Neo4Net.Test.server.InsecureTrustManager;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.sun.jersey.client.urlconnection.HTTPSProperties.PROPERTY_HTTPS_PROPERTIES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.jetty.http.HttpHeader.SERVER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.jetty.http.HttpHeader.STRICT_TRANSPORT_SECURITY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.CommunityServerBuilder.serverOnRandomPorts;

	public class HttpHeadersIT : ExclusiveServerTestBase
	{
		 private const string HSTS_HEADER_VALUE = "max-age=31536000; includeSubDomains; preload";

		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendJettyVersionWithHttpResponseHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSendJettyVersionWithHttpResponseHeaders()
		 {
			  StartServer();
			  TestNoJettyVersionInResponseHeaders( HttpUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendJettyVersionWithHttpsResponseHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSendJettyVersionWithHttpsResponseHeaders()
		 {
			  StartServer();
			  TestNoJettyVersionInResponseHeaders( HttpsUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendHstsHeaderWithHttpResponse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSendHstsHeaderWithHttpResponse()
		 {
			  StartServer( HSTS_HEADER_VALUE );
			  assertNull( RunRequestAndGetHstsHeaderValue( HttpUri() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendHstsHeaderWithHttpsResponse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendHstsHeaderWithHttpsResponse()
		 {
			  StartServer( HSTS_HEADER_VALUE );
			  assertEquals( HSTS_HEADER_VALUE, RunRequestAndGetHstsHeaderValue( HttpsUri() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendHstsHeaderWithHttpsResponseWhenNotConfigured() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSendHstsHeaderWithHttpsResponseWhenNotConfigured()
		 {
			  StartServer();
			  assertNull( RunRequestAndGetHstsHeaderValue( HttpsUri() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startServer() throws Exception
		 private void StartServer()
		 {
			  StartServer( null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startServer(String hstsValue) throws Exception
		 private void StartServer( string hstsValue )
		 {
			  _server = BuildServer( hstsValue );
			  _server.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private CommunityNeoServer buildServer(String hstsValue) throws Exception
		 private CommunityNeoServer BuildServer( string hstsValue )
		 {
			  CommunityServerBuilder builder = serverOnRandomPorts().withHttpsEnabled().usingDataDir(Folder.directory(Name.MethodName).AbsolutePath);

			  if ( !string.ReferenceEquals( hstsValue, null ) )
			  {
					builder.WithProperty( ServerSettings.http_strict_transport_security.name(), hstsValue );
			  }

			  return builder.Build();
		 }

		 private URI HttpUri()
		 {
			  return _server.baseUri();
		 }

		 private URI HttpsUri()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return _server.httpsUri().orElseThrow(System.InvalidOperationException::new);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testNoJettyVersionInResponseHeaders(java.net.URI baseUri) throws Exception
		 private static void TestNoJettyVersionInResponseHeaders( URI baseUri )
		 {
			  IDictionary<string, IList<string>> headers = RunRequestAndGetHeaders( baseUri );

			  assertNull( headers[SERVER.asString()] ); // no 'Server' header

			  foreach ( IList<string> values in headers.Values )
			  {
					assertFalse( values.Any( value => value.ToLower().Contains("jetty") ) ); // no 'jetty' in other header values
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String runRequestAndGetHstsHeaderValue(java.net.URI baseUri) throws Exception
		 private static string RunRequestAndGetHstsHeaderValue( URI baseUri )
		 {
			  return RunRequestAndGetHeaderValue( baseUri, STRICT_TRANSPORT_SECURITY.asString() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String runRequestAndGetHeaderValue(java.net.URI baseUri, String header) throws Exception
		 private static string RunRequestAndGetHeaderValue( URI baseUri, string header )
		 {
			  IList<string> values = RunRequestAndGetHeaderValues( baseUri, header );
			  if ( values.Count == 0 )
			  {
					return null;
			  }
			  else if ( values.Count == 1 )
			  {
					return values[0];
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Unexpected number of " + STRICT_TRANSPORT_SECURITY.asString() + " header values: " + values );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.List<String> runRequestAndGetHeaderValues(java.net.URI baseUri, String header) throws Exception
		 private static IList<string> RunRequestAndGetHeaderValues( URI baseUri, string header )
		 {
			  return RunRequestAndGetHeaders( baseUri ).getOrDefault( header, emptyList() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.Map<String,java.util.List<String>> runRequestAndGetHeaders(java.net.URI baseUri) throws Exception
		 private static IDictionary<string, IList<string>> RunRequestAndGetHeaders( URI baseUri )
		 {
			  URI uri = baseUri.resolve( "db/data/transaction/commit" );
			  ClientRequest request = CreateClientRequest( uri );

			  ClientResponse response = CreateClient().handle(request);
			  assertEquals( 200, response.Status );

			  return response.Headers;
		 }

		 private static ClientRequest CreateClientRequest( URI uri )
		 {
			  return ClientRequest.create().header("Accept", "application/json").build(uri, "POST");
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static com.sun.jersey.api.client.Client createClient() throws Exception
		 private static Client CreateClient()
		 {
			  HostnameVerifier hostnameVerifier = HttpsURLConnection.DefaultHostnameVerifier;
			  ClientConfig config = new DefaultClientConfig();
			  SSLContext ctx = SSLContext.getInstance( "TLS" );
			  ctx.init( null, new TrustManager[]{ new InsecureTrustManager() }, null );
			  config.Properties.put( PROPERTY_HTTPS_PROPERTIES, new HTTPSProperties( hostnameVerifier, ctx ) );
			  return Client.create( config );
		 }
	}

}