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
namespace Neo4Net.Kernel.impl.query.clientconnection
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.SocketAddress.format;

	/// <seealso cref= ClientConnectionInfo Parent class for documentation and tests. </seealso>
	public class BoltConnectionInfo : ClientConnectionInfo
	{
		 private readonly string _connectionId;
		 private readonly string _principalName;
		 private readonly string _clientName;
		 private readonly SocketAddress _clientAddress;
		 private readonly SocketAddress _serverAddress;

		 public BoltConnectionInfo( string connectionId, string principalName, string clientName, SocketAddress clientAddress, SocketAddress serverAddress )
		 {
			  this._connectionId = connectionId;
			  this._principalName = principalName;
			  this._clientName = clientName;
			  this._clientAddress = clientAddress;
			  this._serverAddress = serverAddress;
		 }

		 public override string AsConnectionDetails()
		 {
			  return string.Format( "bolt-session\tbolt\t{0}\t{1}\t\tclient{2}\tserver{3}>", _principalName, _clientName, _clientAddress, _serverAddress );
		 }

		 public override string Protocol()
		 {
			  return "bolt";
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
			  return format( _serverAddress );
		 }
	}

}