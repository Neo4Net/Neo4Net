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
namespace Org.Neo4j.Server.rest
{
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using JsonParseException = Org.Neo4j.Server.rest.domain.JsonParseException;
	using GraphDescription = Org.Neo4j.Test.GraphDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DegreeIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get the degree of a node\n" + "\n" + "Return the total number of relationships associated with a node.") @Test @GraphDescription.Graph({"Root knows Mattias", "Root knows Johan"}) public void get_degree() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get the degree of a node\n" + "\n" + "Return the total number of relationships associated with a node.")]
		 public virtual void GetDegree()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  string nodeUri = GetNodeUri( nodes["Root"] );

			  // Document
			  RESTRequestGenerator.ResponseEntity response = GenConflict.get().expectedStatus(200).get(nodeUri + "/degree/all");

			  // Then
			  assertEquals( 2, JsonHelper.jsonNode( response.Response().Entity ).asInt() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get the degree of a node by direction\n" + "\n" + "Return the number of relationships of a particular direction for a node.\n" + "Specify `all`, `in` or `out`.") @Test @GraphDescription.Graph({"Root knows Mattias", "Root knows Johan"}) public void get_degree_by_direction() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get the degree of a node by direction\n" + "\n" + "Return the number of relationships of a particular direction for a node.\n" + "Specify `all`, `in` or `out`.")]
		 public virtual void GetDegreeByDirection()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  string nodeUri = GetNodeUri( nodes["Root"] );

			  // Document
			  RESTRequestGenerator.ResponseEntity response = GenConflict.get().expectedStatus(200).get(nodeUri + "/degree/out");

			  // Then
			  assertEquals( 2, JsonHelper.jsonNode( response.Response().Entity ).asInt() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get the degree of a node by direction and types\n" + "\n" + "If you are only interested in the degree of a particular relationship type, or a set of " + "relationship types, you specify relationship types after the direction.\n" + "You can combine multiple relationship types by using the `&` character.") @Test @GraphDescription.Graph({"Root KNOWS Mattias", "Root KNOWS Johan", "Root LIKES Cookie"}) public void get_degree_by_direction_and_type() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get the degree of a node by direction and types\n" + "\n" + "If you are only interested in the degree of a particular relationship type, or a set of " + "relationship types, you specify relationship types after the direction.\n" + "You can combine multiple relationship types by using the `&` character.")]
		 public virtual void GetDegreeByDirectionAndType()
		 {
			  IDictionary<string, Node> nodes = Data.get();
			  string nodeUri = GetNodeUri( nodes["Root"] );

			  // Document
			  RESTRequestGenerator.ResponseEntity response = GenConflict.get().expectedStatus(200).get(nodeUri + "/degree/out/KNOWS&LIKES");

			  // Then
			  assertEquals( 3, JsonHelper.jsonNode( response.Response().Entity ).asInt() );
		 }
	}

}