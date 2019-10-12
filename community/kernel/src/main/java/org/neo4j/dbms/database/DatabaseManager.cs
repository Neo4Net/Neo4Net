using System.Collections.Generic;

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
namespace Org.Neo4j.Dbms.database
{

	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;

	public interface DatabaseManager : Lifecycle
	{

		 Optional<GraphDatabaseFacade> GetDatabaseFacade( string name );

		 GraphDatabaseFacade CreateDatabase( string name );

		 /// <summary>
		 /// Shutdown database with specified name. </summary>
		 /// <param name="name"> database name to shutdown </param>
		 void ShutdownDatabase( string name );

		 /// <summary>
		 /// Return sorted list of known database names </summary>
		 /// <returns> sorted list of known database names </returns>
		 IList<string> ListDatabases();
	}

}