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
namespace Examples
{
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.index;
	using RelationshipIndex = Neo4Net.GraphDb.index.RelationshipIndex;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

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
			  IGraphDatabaseService service = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
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