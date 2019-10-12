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
namespace Org.Neo4j.Helpers
{
	public class ListenSocketAddress : SocketAddress
	{
		 public ListenSocketAddress( string hostname, int port ) : base( hostname, port )
		 {
		 }

		 /// <summary>
		 /// Textual representation format for a listen socket address. </summary>
		 /// <param name="hostname"> of the address. </param>
		 /// <param name="port"> of the address. </param>
		 /// <returns> a string representing the address. </returns>
		 public static string ListenAddress( string hostname, int port )
		 {
			  return ( new ListenSocketAddress( hostname, port ) ).ToString();
		 }
	}

}