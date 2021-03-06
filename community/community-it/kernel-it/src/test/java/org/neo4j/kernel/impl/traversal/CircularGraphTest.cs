﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.traversal
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;

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