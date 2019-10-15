﻿/*
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
	using Connection = org.eclipse.jetty.io.Connection;
	using EndPoint = org.eclipse.jetty.io.EndPoint;
	using Connector = org.eclipse.jetty.server.Connector;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using HttpConnectionFactory = org.eclipse.jetty.server.HttpConnectionFactory;

	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;

	/// <summary>
	/// Extension of the default Jetty <seealso cref="HttpConnectionFactory"/> which creates connections with additional properties.
	/// Created connections also notify <seealso cref="NetworkConnectionTracker"/> when open or closed.
	/// </summary>
	public class JettyHttpConnectionFactory : HttpConnectionFactory
	{
		 private readonly NetworkConnectionTracker _connectionTracker;
		 private readonly JettyHttpConnectionListener _connectionListener;

		 public JettyHttpConnectionFactory( NetworkConnectionTracker connectionTracker, HttpConfiguration configuration ) : base( configuration )
		 {
			  this._connectionTracker = connectionTracker;
			  this._connectionListener = new JettyHttpConnectionListener( connectionTracker );
		 }

		 public override Connection NewConnection( Connector connector, EndPoint endPoint )
		 {
			  JettyHttpConnection connection = CreateConnection( connector, endPoint );
			  connection.addListener( _connectionListener );
			  return configure( connection, connector, endPoint );
		 }

		 private JettyHttpConnection CreateConnection( Connector connector, EndPoint endPoint )
		 {
			  string connectionId = _connectionTracker.newConnectionId( connector.Name );
			  return new JettyHttpConnection( connectionId, HttpConfiguration, connector, endPoint, HttpCompliance, RecordHttpComplianceViolations );
		 }
	}

}