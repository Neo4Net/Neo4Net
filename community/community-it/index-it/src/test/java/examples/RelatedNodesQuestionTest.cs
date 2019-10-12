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
namespace Examples
{
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Graphdb.index;
	using RelationshipIndex = Org.Neo4j.Graphdb.index.RelationshipIndex;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	/// <summary>
	/// Trying out code from: http://stackoverflow.com/questions/5346011
	/// 
	/// @author Anders Nawroth
	/// </summary>
	public class RelatedNodesQuestionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void question5346011()
		 public virtual void Question5346011()
		 {
			  GraphDatabaseService service = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  using ( Transaction tx = service.BeginTx() )
			  {
					RelationshipIndex index = service.Index().forRelationships("exact");
					// ...creation of the nodes and relationship
					Node node1 = service.CreateNode();
					Node node2 = service.CreateNode();
					string uuid = "xyz";
					Relationship relationship = node1.CreateRelationshipTo( node2, RelationshipType.withName( "related" ) );
					index.add( relationship, "uuid", uuid );
					// query
					using ( IndexHits<Relationship> hits = index.Get( "uuid", uuid, node1, node2 ) )
					{
						 assertEquals( 1, hits.Size() );
					}
					tx.Success();
			  }
			  service.Shutdown();
		 }
	}

}