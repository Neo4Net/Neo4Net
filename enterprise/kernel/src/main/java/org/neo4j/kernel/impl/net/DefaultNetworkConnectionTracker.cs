using System.Collections.Concurrent;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.impl.net
{

	using NetworkConnectionIdGenerator = Org.Neo4j.Kernel.api.net.NetworkConnectionIdGenerator;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using TrackedNetworkConnection = Org.Neo4j.Kernel.api.net.TrackedNetworkConnection;

	/// <summary>
	/// A <seealso cref="NetworkConnectionTracker"/> that keeps all given connections in a <seealso cref="System.Collections.Concurrent.ConcurrentDictionary"/>.
	/// </summary>
	public class DefaultNetworkConnectionTracker : NetworkConnectionTracker
	{
		 private readonly NetworkConnectionIdGenerator _idGenerator = new NetworkConnectionIdGenerator();
		 private readonly IDictionary<string, TrackedNetworkConnection> _connectionsById = new ConcurrentDictionary<string, TrackedNetworkConnection>();

		 public override string NewConnectionId( string connector )
		 {
			  return _idGenerator.newConnectionId( connector );
		 }

		 public override void Add( TrackedNetworkConnection connection )
		 {
			  TrackedNetworkConnection previousConnection = _connectionsById[connection.Id()] = connection;
			  if ( previousConnection != null )
			  {
					throw new System.ArgumentException( "Attempt to register a connection with an existing id " + connection.Id() + ". " + "Existing connection: " + previousConnection + ", new connection: " + connection );
			  }
		 }

		 public override void Remove( TrackedNetworkConnection connection )
		 {
			  _connectionsById.Remove( connection.Id() );
		 }

		 public override TrackedNetworkConnection Get( string id )
		 {
			  return _connectionsById[id];
		 }

		 public override IList<TrackedNetworkConnection> ActiveConnections()
		 {
			  return new List<TrackedNetworkConnection>( _connectionsById.Values );
		 }
	}

}