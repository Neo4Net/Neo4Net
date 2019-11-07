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
namespace Neo4Net.Server.web
{
	using ConnectionFactory = org.eclipse.jetty.server.ConnectionFactory;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;

	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using NetworkConnectionTracker = Neo4Net.Kernel.Api.net.NetworkConnectionTracker;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;

	public class HttpConnectorFactory
	{
		 private const string NAME = "http";

		 private readonly string _name;
		 private readonly NetworkConnectionTracker _connectionTracker;
		 private readonly Config _configuration;

		 public HttpConnectorFactory( NetworkConnectionTracker connectionTracker, Config config ) : this( NAME, connectionTracker, config )
		 {
		 }

		 protected internal HttpConnectorFactory( string name, NetworkConnectionTracker connectionTracker, Config configuration )
		 {
			  this._name = name;
			  this._connectionTracker = connectionTracker;
			  this._configuration = configuration;
		 }

		 public virtual ConnectionFactory CreateHttpConnectionFactory()
		 {
			  return new JettyHttpConnectionFactory( _connectionTracker, CreateHttpConfig() );
		 }

		 protected internal virtual HttpConfiguration CreateHttpConfig()
		 {
			  HttpConfiguration httpConfig = new HttpConfiguration();
			  httpConfig.RequestHeaderSize = _configuration.get( ServerSettings.maximum_request_header_size );
			  httpConfig.ResponseHeaderSize = _configuration.get( ServerSettings.maximum_response_header_size );
			  httpConfig.SendServerVersion = false;
			  return httpConfig;
		 }

		 public virtual ServerConnector CreateConnector( Server server, ListenSocketAddress address, JettyThreadCalculator jettyThreadCalculator )
		 {
			  ConnectionFactory httpFactory = CreateHttpConnectionFactory();
			  return CreateConnector( server, address, jettyThreadCalculator, httpFactory );
		 }

		 public virtual ServerConnector CreateConnector( Server server, ListenSocketAddress address, JettyThreadCalculator jettyThreadCalculator, params ConnectionFactory[] httpFactories )
		 {
			  int acceptors = jettyThreadCalculator.Acceptors;
			  int selectors = jettyThreadCalculator.Selectors;

			  ServerConnector connector = new ServerConnector( server, null, null, null, acceptors, selectors, httpFactories );

			  connector.Name = _name;

			  connector.ConnectionFactories = Arrays.asList( httpFactories );

			  // TCP backlog, per socket, 50 is the default, consider adapting if needed
			  connector.AcceptQueueSize = 50;

			  connector.Host = address.Hostname;
			  connector.Port = address.Port;

			  return connector;
		 }
	}

}