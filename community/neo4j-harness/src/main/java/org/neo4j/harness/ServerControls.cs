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
namespace Org.Neo4j.Harness
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Configuration = Org.Neo4j.Graphdb.config.Configuration;

	/// <summary>
	/// Control panel for a Neo4j test instance.
	/// </summary>
	public interface ServerControls : AutoCloseable
	{
		 /// <summary>
		 /// Returns the URI to the Bolt Protocol connector of the instance. </summary>
		 URI BoltURI();

		 /// <summary>
		 /// Returns the URI to the root resource of the instance. For example, http://localhost:7474/ </summary>
		 URI HttpURI();

		 /// <summary>
		 /// Returns ths URI to the root resource of the instance using the https protocol.
		 /// For example, https://localhost:7475/.
		 /// </summary>
		 Optional<URI> HttpsURI();

		 /// <summary>
		 /// Stop the test instance and delete all files related to it on disk. </summary>
		 void Close();

		 /// <summary>
		 /// Access the <seealso cref="org.neo4j.graphdb.GraphDatabaseService"/> used by the server </summary>
		 GraphDatabaseService Graph();

		 /// <summary>
		 /// Returns the server's configuration </summary>
		 Configuration Config();

		 /// <summary>
		 /// Prints logs to the specified print stream if log is available </summary>
		 void PrintLogs( PrintStream @out );
	}

}