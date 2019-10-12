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


	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestAccess.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestBase.NODE_URI_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestBase.RELATIONSHIP_URI_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestBase.assertUriMatches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.node;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.relationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.Link.link;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.Properties.properties;

	public class PathRepresentationTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveLength()
		 public virtual void ShouldHaveLength()
		 {
			  assertNotNull( Pathrep().length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveStartNodeLink()
		 public virtual void ShouldHaveStartNodeLink()
		 {
			  assertUriMatches( NODE_URI_PATTERN, Pathrep().startNode() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveEndNodeLink()
		 public virtual void ShouldHaveEndNodeLink()
		 {
			  assertUriMatches( NODE_URI_PATTERN, Pathrep().endNode() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveNodeList()
		 public virtual void ShouldHaveNodeList()
		 {
			  assertNotNull( Pathrep().nodes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveRelationshipList()
		 public virtual void ShouldHaveRelationshipList()
		 {
			  assertNotNull( Pathrep().relationships() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveDirectionList()
		 public virtual void ShouldHaveDirectionList()
		 {
			  assertNotNull( Pathrep().directions() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerialiseToMap()
		 public virtual void ShouldSerialiseToMap()
		 {
			  IDictionary<string, object> repr = serialize( Pathrep() );
			  assertNotNull( repr );
			  VerifySerialisation( repr );
		 }

		 /*
		  * Construct a sample path representation of the form:
		  *
		  *     (A)-[:LOVES]->(B)<-[:HATES]-(C)-[:KNOWS]->(D)
		  *
		  * This contains two forward relationships and one backward relationship
		  * which is represented in the "directions" value of the output. We should
		  * therefore see something like the following:
		  *
		  * {
		  *     "length" : 3,
		  *     "start" : "http://neo4j.org/node/0",
		  *     "end" : "http://neo4j.org/node/3",
		  *     "nodes" : [
		  *         "http://neo4j.org/node/0", "http://neo4j.org/node/1",
		  *         "http://neo4j.org/node/2", "http://neo4j.org/node/3"
		  *     ],
		  *     "relationships" : [
		  *         "http://neo4j.org/relationship/17",
		  *         "http://neo4j.org/relationship/18",
		  *         "http://neo4j.org/relationship/19"
		  *     ],
		  *     "directions" : [ "->", "<-", "->" ]
		  * }
		  *
		  */
		 private PathRepresentation<Path> Pathrep()
		 {
			  Node a = node( 0, properties() );
			  Node b = node( 1, properties() );
			  Node c = node( 2, properties() );
			  Node d = node( 3, properties() );

			  Relationship ab = relationship( 17, a, "LOVES", b );
			  Relationship cb = relationship( 18, c, "HATES", b );
			  Relationship cd = relationship( 19, c, "KNOWS", d );

			  return new PathRepresentation<Path>( path( a, link( ab, b ), link( cb, c ), link( cd, d ) ) );
		 }

		 public static void VerifySerialisation( IDictionary<string, object> pathrep )
		 {
			  assertNotNull( pathrep["length"] );
			  int length = int.Parse( pathrep["length"].ToString() );

			  assertUriMatches( NODE_URI_PATTERN, pathrep["start"].ToString() );
			  assertUriMatches( NODE_URI_PATTERN, pathrep["end"].ToString() );

			  object nodes = pathrep["nodes"];
			  assertTrue( nodes is System.Collections.IList );
			  System.Collections.IList nodeList = ( System.Collections.IList ) nodes;
			  assertEquals( length + 1, nodeList.Count );
			  foreach ( object node in nodeList )
			  {
					assertUriMatches( NODE_URI_PATTERN, node.ToString() );
			  }

			  object rels = pathrep["relationships"];
			  assertTrue( rels is System.Collections.IList );
			  System.Collections.IList relList = ( System.Collections.IList ) rels;
			  assertEquals( length, relList.Count );
			  foreach ( object rel in relList )
			  {
					assertUriMatches( RELATIONSHIP_URI_PATTERN, rel.ToString() );
			  }

			  object directions = pathrep["directions"];
			  assertTrue( directions is System.Collections.IList );
			  System.Collections.IList directionList = ( System.Collections.IList ) directions;
			  assertEquals( length, directionList.Count );
			  assertEquals( "->", directionList[0].ToString() );
			  assertEquals( "<-", directionList[1].ToString() );
			  assertEquals( "->", directionList[2].ToString() );
		 }

	}

}