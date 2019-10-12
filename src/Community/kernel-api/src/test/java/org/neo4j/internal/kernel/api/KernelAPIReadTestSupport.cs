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
namespace Neo4Net.@internal.Kernel.Api
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;

	/// <summary>
	/// This interface defines the functionality that's needed to run Kernel API Read tests (tests that extends
	/// KernelAPIReadTestBase) on a Kernel.
	/// </summary>
	public interface KernelAPIReadTestSupport
	{
		 /// <summary>
		 /// Setup the test. Called once. Starts a Kernel in the provided store directory, and populates the graph using
		 /// the provided create method.
		 /// </summary>
		 /// <param name="storeDir"> The directory in which to create the database. </param>
		 /// <param name="create"> Method which populates the database. </param>
		 /// <exception cref="IOException"> If database creation failed due to IO problems. </exception>
		 void Setup( File storeDir, System.Action<GraphDatabaseService> create );

		 /// <summary>
		 /// The Kernel to test. Called before every test. </summary>
		 /// <returns> The Kernel. </returns>
		 Kernel KernelToTest();

		 /// <summary>
		 /// Teardown the Kernel and any other resources once all tests have completed.
		 /// </summary>
		 void TearDown();
	}

}