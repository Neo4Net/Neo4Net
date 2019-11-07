using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.discovery
{
	using HttpClient = org.eclipse.jetty.client.HttpClient;
	using HttpHeader = org.eclipse.jetty.http.HttpHeader;
	using MimeTypes = org.eclipse.jetty.http.MimeTypes;
	using Connector = org.eclipse.jetty.server.Connector;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using HttpConnectionFactory = org.eclipse.jetty.server.HttpConnectionFactory;
	using Request = org.eclipse.jetty.server.Request;
	using SecureRequestCustomizer = org.eclipse.jetty.server.SecureRequestCustomizer;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;
	using SslConnectionFactory = org.eclipse.jetty.server.SslConnectionFactory;
	using AbstractHandler = org.eclipse.jetty.server.handler.AbstractHandler;
	using SslContextFactory = org.eclipse.jetty.util.ssl.SslContextFactory;
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using SslPolicy = Neo4Net.Ssl.SslPolicy;
	using SslResource = Neo4Net.Ssl.SslResource;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.discovery.MultiRetryStrategyTest.testRetryStrategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.ssl.SslResourceBuilder.selfSignedKeyId;

	public class KubernetesResolverIT
	{
		private bool InstanceFieldsInitialized = false;

		public KubernetesResolverIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_config = Config.builder().withSetting(CausalClusteringSettings.kubernetes_address, "localhost:" + _port).withSetting(CausalClusteringSettings.kubernetes_label_selector, _testLabelSelector).withSetting(CausalClusteringSettings.kubernetes_service_port_name, _testPortName).build();
			_expectedAddress = new AdvertisedSocketAddress( string.Format( "{0}.{1}.svc.cluster.local", _testServiceName, _testNamespace ), _testPortNumber );
			_resolver = new KubernetesResolver.KubernetesClient( new SimpleLogService( _userLogProvider, _logProvider ), _httpClient, _testAuthToken, _testNamespace, _config, testRetryStrategy( 1 ) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDir = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

		 private readonly int _port = PortAuthority.allocatePort();
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private readonly AssertableLogProvider _userLogProvider = new AssertableLogProvider();
		 private readonly string _testPortName = "test-port-name";
		 private readonly string _testServiceName = "test-service-name";
		 private readonly int _testPortNumber = 4313;
		 private readonly string _testNamespace = "test-namespace";
		 private readonly string _testLabelSelector = "test-label-selector";
		 private readonly string _testAuthToken = "Oh go on then";
		 private Config _config;

		 private AdvertisedSocketAddress _expectedAddress;

		 private readonly HttpClient _httpClient = new HttpClient( new SslContextFactory( true ) );

		 private HostnameResolver _resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResolveAddressesFromApiReturningShortJson() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResolveAddressesFromApiReturningShortJson()
		 {
			  WithServer(ShortJson(), () =>
			  {
				ICollection<AdvertisedSocketAddress> addresses = _resolver.resolve( null );

				assertThat( addresses, contains( _expectedAddress ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResolveAddressesFromApiReturningLongJson() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResolveAddressesFromApiReturningLongJson()
		 {
			  WithServer(LongJson(), () =>
			  {
				ICollection<AdvertisedSocketAddress> addresses = _resolver.resolve( null );

				assertThat( addresses, contains( _expectedAddress ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogResolvedAddressesToUserLog() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogResolvedAddressesToUserLog()
		 {
			  WithServer(LongJson(), () =>
			  {
				_resolver.resolve( null );

				_userLogProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Resolved %s from Kubernetes API at %s namespace %s labelSelector %s")));
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogEmptyAddressesToDebugLog() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogEmptyAddressesToDebugLog()
		 {
			  string response = "{ \"kind\":\"ServiceList\", \"items\":[] }";
			  WithServer(response, () =>
			  {
				_resolver.resolve( null );

				_logProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Resolved empty hosts from Kubernetes API at %s namespace %s labelSelector %s")));
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogParseErrorToDebugLog() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogParseErrorToDebugLog()
		 {
			  string response = "{}";
			  WithServer(response, () =>
			  {
				_resolver.resolve( null );
				_logProvider.formattedMessageMatcher().assertContains("Failed to parse result from Kubernetes API");
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportFailureDueToAuth() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportFailureDueToAuth()
		 {
			  Expected.expect( typeof( System.InvalidOperationException ) );
			  Expected.expectMessage( "Forbidden" );

			  WithServer(FailJson(), () =>
			  {
				_resolver.resolve( null );
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void withServer(String json, Runnable test) throws Exception
		 public virtual void WithServer( string json, ThreadStart test )
		 {
			  Server server = setUp( json );

			  try
			  {
					test.run();
			  }
			  finally
			  {
					TearDown( server );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String failJson() throws java.io.IOException, java.net.URISyntaxException
		 private string FailJson()
		 {
			  return ReadJsonFile( "authFail.json" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String shortJson() throws java.io.IOException, java.net.URISyntaxException
		 private string ShortJson()
		 {
			  return ReadJsonFile( "short.json" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String longJson() throws java.io.IOException, java.net.URISyntaxException
		 private string LongJson()
		 {
			  return ReadJsonFile( "long.json" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String readJsonFile(final String fileName) throws java.io.IOException, java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private string ReadJsonFile( string fileName )
		 {
			  Path path = Paths.get( this.GetType().getResource("/Neo4Net.causalclustering.discovery/" + fileName).toURI() );
			  string fullFile = Files.lines( path ).collect( Collectors.joining( "\n" ) );
			  return string.format( fullFile, _testServiceName, _testPortName, _testPortNumber );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.eclipse.jetty.server.Server setUp(String response) throws Exception
		 private Server setUp( string response )
		 {
			  Server server = new Server();
			  server.Handler = new FakeKubernetesHandler( _testNamespace, _testLabelSelector, _testAuthToken, response );

			  HttpConfiguration https = new HttpConfiguration();
			  https.addCustomizer( new SecureRequestCustomizer() );

			  string keyStorePass = "key store pass";
			  string privateKeyPass = "private key pass";
			  SslResource server1 = selfSignedKeyId( 0 ).trustKeyId( 1 ).install( TestDir.directory( "k8s" ) );
			  SslPolicy sslPolicy = Neo4Net.Ssl.SslContextFactory.MakeSslPolicy( server1 );
			  KeyStore keyStore = sslPolicy.GetKeyStore( keyStorePass.ToCharArray(), privateKeyPass.ToCharArray() );

			  SslContextFactory sslContextFactory = new SslContextFactory();
			  sslContextFactory.KeyStore = keyStore;
			  sslContextFactory.KeyStorePassword = keyStorePass;
			  sslContextFactory.KeyManagerPassword = privateKeyPass;

			  ServerConnector sslConnector = new ServerConnector(server, new SslConnectionFactory(sslContextFactory, "http/1.1"), new HttpConnectionFactory(https)
			 );

			  sslConnector.Port = _port;

			  server.Connectors = new Connector[]{ sslConnector };

			  server.start();

			  _httpClient.start();

			  return server;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void tearDown(org.eclipse.jetty.server.Server server) throws Exception
		 private void TearDown( Server server )
		 {
			  _httpClient.stop();
			  server.stop();
		 }

		 private class FakeKubernetesHandler : AbstractHandler
		 {
			  internal readonly string ExpectedNamespace;
			  internal readonly string ExpectedLabelSelector;
			  internal readonly string ExpectedAuthToken;
			  internal readonly string Body;

			  internal FakeKubernetesHandler( string expectedNamespace, string labelSelector, string authToken, string body )
			  {
					this.ExpectedNamespace = expectedNamespace;
					this.ExpectedLabelSelector = labelSelector;
					this.ExpectedAuthToken = authToken;
					this.Body = body;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(String target, org.eclipse.jetty.server.Request baseRequest, javax.servlet.http.HttpServletRequest request, javax.servlet.http.HttpServletResponse response) throws java.io.IOException
			  public override void Handle( string target, Request baseRequest, HttpServletRequest request, HttpServletResponse response )
			  {
					PrintWriter @out = response.Writer;
					response.ContentType = MimeTypes.Type.APPLICATION_JSON.asString();

					string path = request.PathInfo;
					string expectedPath = string.format( KubernetesResolver.KubernetesClient.PATH, ExpectedNamespace );

					string labelSelector = request.getParameter( "labelSelector" );
					string auth = request.getHeader( HttpHeader.AUTHORIZATION.name() );
					string expectedAuth = "Bearer " + ExpectedAuthToken;

					if ( !expectedPath.Equals( path ) )
					{
						 response.Status = HttpServletResponse.SC_BAD_REQUEST;
						 @out.println( Fail( "Unexpected path: " + path ) );
					}
					else if ( !ExpectedLabelSelector.Equals( labelSelector ) )
					{
						 response.Status = HttpServletResponse.SC_BAD_REQUEST;
						 @out.println( Fail( "Unexpected labelSelector: " + labelSelector ) );
					}
					else if ( !expectedAuth.Equals( auth ) )
					{
						 response.Status = HttpServletResponse.SC_BAD_REQUEST;
						 @out.println( Fail( "Unexpected auth header value: " + auth ) );
					}
					else if ( !"GET".Equals( request.Method ) )
					{
						 response.Status = HttpServletResponse.SC_BAD_REQUEST;
						 @out.println( Fail( "Unexpected method: " + request.Method ) );
					}
					else
					{
						 response.Status = HttpServletResponse.SC_OK;
						 if ( !string.ReferenceEquals( Body, null ) )
						 {
							  @out.println( Body );
						 }
					}

					baseRequest.Handled = true;
			  }

			  internal virtual string Fail( string message )
			  {
					return string.Format( "{{ \"kind\": \"Status\", \"message\": \"{0}\"}}", message );
			  }
		 }
	}

}