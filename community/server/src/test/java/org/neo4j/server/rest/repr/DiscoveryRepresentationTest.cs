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
namespace Org.Neo4j.Server.rest.repr
{
	using Test = org.junit.Test;


	using DiscoverableURIs = Org.Neo4j.Server.rest.discovery.DiscoverableURIs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.DiscoverableURIs.Precedence.NORMAL;

	public class DiscoveryRepresentationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAMapContainingDataAndManagementURIs() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateAMapContainingDataAndManagementURIs()
		 {
			  string managementUri = "/management";
			  string dataUri = "/data";
			  DiscoveryRepresentation dr = new DiscoveryRepresentation(new DiscoverableURIs.Builder()
									.add( "management", managementUri, NORMAL ).add( "data", dataUri, NORMAL ).add( "bolt", new URI( "bolt://localhost:7687" ), NORMAL ).build());

			  IDictionary<string, object> mapOfUris = RepresentationTestAccess.Serialize( dr );

			  object mappedManagementUri = mapOfUris["management"];
			  object mappedDataUri = mapOfUris["data"];
			  object mappedBoltUri = mapOfUris["bolt"];

			  assertNotNull( mappedManagementUri );
			  assertNotNull( mappedDataUri );
			  assertNotNull( mappedBoltUri );

			  URI baseUri = RepresentationTestBase.BaseUri;

			  assertEquals( mappedManagementUri.ToString(), Serializer.JoinBaseWithRelativePath(baseUri, managementUri) );
			  assertEquals( mappedDataUri.ToString(), Serializer.JoinBaseWithRelativePath(baseUri, dataUri) );
			  assertEquals( mappedBoltUri.ToString(), "bolt://localhost:7687" );
		 }
	}

}