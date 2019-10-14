using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.traversal
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Direction = Neo4Net.Graphdb.Direction;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Evaluation = Neo4Net.Graphdb.traversal.Evaluation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;

	public class CircularGraphTest : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createTheGraph()
		 public virtual void CreateTheGraph()
		 {
			  CreateGraph( "1 TO 2", "2 TO 3", "3 TO 1" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCircularBug()
		 public virtual void TestCircularBug()
		 {
			  const long timestamp = 3;
			  using ( Transaction tx = BeginTx() )
			  {
					GetNodeWithName( "2" ).setProperty( "timestamp", 1L );
					GetNodeWithName( "3" ).setProperty( "timestamp", 2L );
					tx.Success();
			  }

			  using ( Transaction tx2 = BeginTx() )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.RelationshipType type = org.neo4j.graphdb.RelationshipType.withName("TO");
					RelationshipType type = RelationshipType.withName( "TO" );
					IEnumerator<Node> nodes = GraphDb.traversalDescription().depthFirst().relationships(type, Direction.OUTGOING).evaluator(path =>
					{
								Relationship rel = path.lastRelationship();
								bool relIsOfType = rel != null && rel.isType( type );
								bool prune = relIsOfType && ( long? ) path.endNode().getProperty("timestamp").Value >= timestamp;
								return Evaluation.of( relIsOfType, !prune );
					}).traverse( Node( "1" ) ).nodes().GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( "2", nodes.next().getProperty("name") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( "3", nodes.next().getProperty("name") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( nodes.hasNext() );
			  }
		 }
	}

}