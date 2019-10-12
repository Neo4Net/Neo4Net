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
namespace Org.Neo4j.Kernel.impl.query.clientconnection
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.SocketAddress.format;

	/// <seealso cref= ClientConnectionInfo Parent class for documentation and tests. </seealso>
	public class HttpConnectionInfo : ClientConnectionInfo
	{
		 private readonly string _connectionId;
		 private readonly string _protocol;
		 private readonly SocketAddress _clientAddress;
		 private readonly SocketAddress _serverAddress;
		 private readonly string _requestPath;

		 public HttpConnectionInfo( string connectionId, string protocol, SocketAddress clientAddress, SocketAddress serverAddress, string requestPath )
		 {
			  this._connectionId = connectionId;
			  this._protocol = protocol;
			  this._clientAddress = clientAddress;
			  this._serverAddress = serverAddress;
			  this._requestPath = requestPath;
		 }

		 public override string AsConnectionDetails()
		 {
			  return string.join( "\t", "server-session", _protocol, GetHostString( _clientAddress ), _requestPath );
		 }

		 public override string Protocol()
		 {
			  return _protocol;
		 }

		 public override string ConnectionId()
		 {
			  return _connectionId;
		 }

		 public override string ClientAddress()
		 {
			  return format( _clientAddress );
		 }

		 public override string RequestURI()
		 {
			  return _serverAddress == null ? _requestPath : _protocol + "://" + format( _serverAddress ) + _requestPath;
		 }

		 private static string GetHostString( SocketAddress address )
		 {
			  return address is InetSocketAddress ? ( ( InetSocketAddress ) address ).HostString : address.ToString();
		 }
	}

}