﻿/*
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

	using InProcessServerBuilder = Org.Neo4j.Harness.@internal.InProcessServerBuilder;

	/// <summary>
	/// Factories for creating <seealso cref="org.neo4j.harness.TestServerBuilder"/> instances.
	/// </summary>
	public sealed class TestServerBuilders
	{
		 /// <summary>
		 /// Create a builder capable of starting an in-process Neo4j instance. This builder will use the standard java temp
		 /// directory (configured via the 'java.io.tmpdir' system property) as the location for the temporary Neo4j directory.
		 /// </summary>
		 public static TestServerBuilder NewInProcessBuilder()
		 {
			  return new InProcessServerBuilder();
		 }

		 /// <summary>
		 /// Create a builder capable of starting an in-process Neo4j instance, running in a subdirectory of the specified directory.
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