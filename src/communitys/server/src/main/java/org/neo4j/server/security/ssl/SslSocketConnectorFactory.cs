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
namespace Neo4Net.Server.security.ssl
{
	using HttpVersion = org.eclipse.jetty.http.HttpVersion;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using Customizer = org.eclipse.jetty.server.HttpConfiguration.Customizer;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;
	using SslConnectionFactory = org.eclipse.jetty.server.SslConnectionFactory;
	using SslContextFactory = org.eclipse.jetty.util.ssl.SslContextFactory;


	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnectorFactory = Neo4Net.Server.web.HttpConnectorFactory;
	using JettyThreadCalculator = Neo4Net.Server.web.JettyThreadCalculator;
	using SslPolicy = Neo4Net.Ssl.SslPolicy;

	public class SslSocketConnectorFactory : HttpConnectorFactory
	{
		 private const string NAME = "https";

		 private readonly HttpConfiguration.Customizer _requestCustomizer;

		 public SslSocketConnectorFactory( NetworkConnectionTracker connectionTracker, Config config ) : base( NAME, connectionTracker, config )
		 {
			  _requestCustomizer = new HttpsRequestCustomizer( config );
		 }

		 protected internal override HttpConfiguration CreateHttpConfig()
		 {
			  HttpConfiguration httpConfig = base.CreateHttpConfig();
			  httpConfig.addCustomizer( _requestCustomizer );
			  return httpConfig;
		 }

		 public virtual ServerConnector CreateConnector( Server server, SslPolicy sslPolicy, ListenSocketAddress address, JettyThreadCalculator jettyThreadCalculator )
		 {
			  SslConnectionFactory sslConnectionFactory = CreateSslConnectionFactory( sslPolicy );
			  return createConnector( server, address, jettyThreadCalculator, sslConnectionFactory, CreateHttpConnectionFactory() );
		 }

		 private SslConnectionFactory CreateSslConnectionFactory( SslPolicy sslPolicy )
		 {
			  SslContextFactory sslContextFactory = new SslContextFactory();

			  string password = System.Guid.randomUUID().ToString();
			  sslContextFactory.KeyStore = sslPolicy.GetKeyStore( password.ToCharArray(), password.ToCharArray() );
			  sslContextFactory.KeyStorePassword = password;
			  sslContextFactory.KeyManagerPassword = password;

			  IList<string> ciphers = sslPolicy.CipherSuites;
			  if ( ciphers != null )
			  {
					sslContextFactory.IncludeCipherSuites = ciphers.ToArray();
					sslContextFactory.setExcludeCipherSuites();
			  }

			  string[] protocols = sslPolicy.TlsVersions;
			  if ( protocols != null )
			  {
					sslContextFactory.IncludeProtocols = protocols;
					sslContextFactory.setExcludeProtocols();
			  }

			  switch ( sslPolicy.ClientAuth )
			  {
			  case REQUIRE:
					sslContextFactory.NeedClientAuth = true;
					break;
			  case OPTIONAL:
					sslContextFactory.WantClientAuth = true;
					break;
			  case NONE:
					sslContextFactory.WantClientAuth = false;
					sslContextFactory.NeedClientAuth = false;
					break;
			  default:
					throw new System.ArgumentException( "Not supported: " + sslPolicy.ClientAuth );
			  }

			  return new SslConnectionFactory( sslContextFactory, HttpVersion.HTTP_1_1.asString() );
		 }
	}

}