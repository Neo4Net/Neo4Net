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
namespace Neo4Net.Harness
{

	using InProcessServerBuilder = Neo4Net.Harness.Internal.InProcessServerBuilder;

	/// <summary>
	/// Factories for creating <seealso cref="Neo4Net.harness.TestServerBuilder"/> instances.
	/// </summary>
	public sealed class TestServerBuilders
	{
		 /// <summary>
		 /// Create a builder capable of starting an in-process Neo4Net instance. This builder will use the standard java temp
		 /// directory (configured via the 'java.io.tmpdir' system property) as the location for the temporary Neo4Net directory.
		 /// </summary>
		 public static TestServerBuilder NewInProcessBuilder()
		 {
			  return new InProcessServerBuilder();
		 }

		 /// <summary>
		 /// Create a builder capable of starting an in-process Neo4Net instance, running in a subdirectory of the specified directory.
		 /// </summary>
		 public static TestServerBuilder NewInProcessBuilder( File workingDirectory )
		 {
			  return new InProcessServerBuilder( workingDirectory );
		 }

		 private TestServerBuilders()
		 {
		 }
	}

}