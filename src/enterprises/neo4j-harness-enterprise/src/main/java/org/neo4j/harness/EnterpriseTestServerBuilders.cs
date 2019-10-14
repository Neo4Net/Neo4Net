/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Harness
{

	using EnterpriseInProcessServerBuilder = Neo4Net.Harness.@internal.EnterpriseInProcessServerBuilder;

	/// <summary>
	/// Factories for creating <seealso cref="TestServerBuilder"/> instances.
	/// </summary>
	public sealed class EnterpriseTestServerBuilders
	{
		 /// <summary>
		 /// Create a builder capable of starting an in-process Neo4j instance. This builder will use the standard java temp
		 /// directory (configured via the 'java.io.tmpdir' system property) as the location for the temporary Neo4j directory.
		 /// </summary>
		 public static TestServerBuilder NewInProcessBuilder()
		 {
			  return new EnterpriseInProcessServerBuilder();
		 }

		 /// <summary>
		 /// Create a builder capable of starting an in-process Neo4j instance, running in a subdirectory of the specified directory.
		 /// </summary>
		 public static TestServerBuilder NewInProcessBuilder( File workingDirectory )
		 {
			  return new EnterpriseInProcessServerBuilder( workingDirectory );
		 }

		 private EnterpriseTestServerBuilders()
		 {
		 }
	}

}