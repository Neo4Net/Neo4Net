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
namespace Neo4Net.Server.web
{
	using HttpCompliance = org.eclipse.jetty.http.HttpCompliance;
	using EndPoint = org.eclipse.jetty.io.EndPoint;
	using Connector = org.eclipse.jetty.server.Connector;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using HttpConnection = org.eclipse.jetty.server.HttpConnection;

	using TrackedNetworkConnection = Neo4Net.Kernel.api.net.TrackedNetworkConnection;

	/// <summary>
	/// Extension of the default Jetty <seealso cref="HttpConnection"/> which contains additional properties like id, connect time, user, etc.
	/// It is bound of the Jetty worker thread when active.
	/// </summary>
	/// <seealso cref= HttpConnection#getCurrentConnection() </seealso>
	public class JettyHttpConnection : HttpConnection, TrackedNetworkConnection
	{
		 private readonly string _id;
		 private readonly long _connectTime;

		 private volatile string _username;
		 private volatile string _userAgent;

		 public JettyHttpConnection( string id, HttpConfiguration config, Connector connector, EndPoint endPoint, HttpCompliance compliance, bool recordComplianceViolations ) : base( config, connector, endPoint, compliance, recordComplianceViolations )
		 {
			  this._id = id;
			  this._connectTime = DateTimeHelper.CurrentUnixTimeMillis();
		 }

		 public override string Id()
		 {
			  return _id;
		 }

		 public override long ConnectTime()
		 {
			  return _connectTime;
		 }

		 public override string Connector()
		 {
			  return Connector.Name;
		 }

		 public override SocketAddress ServerAddress()
		 {
			  return EndPoint.LocalAddress;
		 }

		 public override SocketAddress ClientAddress()
		 {
			  return EndPoint.RemoteAddress;
		 }

		 public override string Username()
		 {
			  return _username;
		 }

		 public override string UserAgent()
		 {
			  return _userAgent;
		 }

		 public override void UpdateUser( string username, string userAgent )
		 {
			  this._username = username;
			  this._userAgent = userAgent;
		 }

		 public static void UpdateUserForCurrentConnection( string username, string userAgent )
		 {
			  JettyHttpConnection connection = CurrentJettyHttpConnection;
			  if ( connection != null )
			  {
					connection.UpdateUser( username, userAgent );
			  }
		 }

		 public static JettyHttpConnection CurrentJettyHttpConnection
		 {
			 get
			 {
				  HttpConnection connection = HttpConnection.CurrentConnection;
				  return connection is JettyHttpConnection ? ( JettyHttpConnection ) connection : null;
			 }
		 }
	}

}