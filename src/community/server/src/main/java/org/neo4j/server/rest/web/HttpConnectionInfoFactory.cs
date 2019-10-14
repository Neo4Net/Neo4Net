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
namespace Neo4Net.Server.rest.web
{

	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using HttpConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.HttpConnectionInfo;
	using JettyHttpConnection = Neo4Net.Server.web.JettyHttpConnection;

	public class HttpConnectionInfoFactory
	{
		 private HttpConnectionInfoFactory()
		 {
		 }

		 public static ClientConnectionInfo Create( HttpServletRequest request )
		 {
			  string connectionId;
			  string protocol = request.Scheme;
			  SocketAddress clientAddress;
			  SocketAddress serverAddress;
			  string requestURI = request.RequestURI;

			  JettyHttpConnection connection = JettyHttpConnection.CurrentJettyHttpConnection;
			  if ( connection != null )
			  {
					connectionId = connection.Id();
					clientAddress = connection.ClientAddress();
					serverAddress = connection.ServerAddress();
			  }
			  else
			  {
					// connection is unknown, connection object can't be extracted or is missing from the Jetty thread-local
					// get all the available information directly from the request
					connectionId = null;
					clientAddress = new InetSocketAddress( request.RemoteAddr, request.RemotePort );
					serverAddress = new InetSocketAddress( request.ServerName, request.ServerPort );
			  }

			  return new HttpConnectionInfo( connectionId, protocol, clientAddress, serverAddress, requestURI );
		 }
	}

}