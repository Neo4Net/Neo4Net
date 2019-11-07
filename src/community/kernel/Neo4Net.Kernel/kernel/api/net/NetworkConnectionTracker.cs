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
namespace Neo4Net.Kernel.Api.net
{

	/// <summary>
	/// Container for all established and active network connections to the database.
	/// </summary>
	public interface NetworkConnectionTracker
	{
		 string NewConnectionId( string connector );

		 void Add( TrackedNetworkConnection connection );

		 void Remove( TrackedNetworkConnection connection );

		 TrackedNetworkConnection Get( string id );

		 IList<TrackedNetworkConnection> ActiveConnections();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 NetworkConnectionTracker NO_OP = new NetworkConnectionTracker()
	//	 {
	//		  private final NetworkConnectionIdGenerator idGenerator = new NetworkConnectionIdGenerator();
	//
	//		  @@Override public String newConnectionId(String connector)
	//		  {
	//				// need to generate a valid ID because it appears in logs, bolt messages, etc.
	//				return idGenerator.newConnectionId(connector);
	//		  }
	//
	//		  @@Override public void add(TrackedNetworkConnection connection)
	//		  {
	//		  }
	//
	//		  @@Override public void remove(TrackedNetworkConnection connection)
	//		  {
	//		  }
	//
	//		  @@Override public TrackedNetworkConnection get(String id)
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public List<TrackedNetworkConnection> activeConnections()
	//		  {
	//				return emptyList();
	//		  }
	//	 };
	}

}