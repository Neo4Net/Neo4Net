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
namespace Neo4Net.Kernel.impl.traversal
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using OrderedByTypeExpander = Neo4Net.Graphdb.impl.OrderedByTypeExpander;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public class TestOrderByTypeExpander : TraversalTestBase
	{
		 private readonly RelationshipType _next = withName( "NEXT" );
		 private readonly RelationshipType _firstComment = withName( "FIRST_COMMENT" );
		 private readonly RelationshipType _comment = withName( "COMMENT" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  /// <summary>
			  ///                          (A1)-NEXT->(A2)-NEXT->(A3)
			  ///                        /             |              \
			  ///            FIRST_COMMENT      FIRST_COMMENT        FIRST_COMMENT
			  ///             /                        |                    \
			  ///            v                         v                     v
			  ///          (C1)                       (C4)                  (C7)
			  ///           |                          |                     |
			  ///        COMMENT                    COMMENT               COMMENT
			  ///           |                          |                     |
			  ///           v                          v                     v
			  ///          (C2)                       (C5)                  (C8)
			  ///           |                          |                     |
			  ///        COMMENT                    COMMENT               COMMENT
			  ///           |                          |                     |
			  ///           v                          v                     v
			  ///          (C3)                       (C6)                  (C9)
			  /// </summary>
			  CreateGraph( "A1 NEXT A2", "A2 NEXT A3", "A1 FIRST_COMMENT C1", "C1 COMMENT C2", "C2 COMMENT C3", "A2 FIRST_COMMENT C4", "C4 COMMENT C5", "C5 COMMENT C6", "A3 FIRST_COMMENT C7", "C7 COMMENT C8", "C8 COMMENT C9" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureNodesAreTraversedInCorrectOrder()
		 public virtual void MakeSureNodesAreTraversedInCorrectOrder()
		 {
			  PathExpander expander = ( new OrderedByTypeExpander() ).add(_firstComment).add(_comment).add(_next);
			  IEnumerator<Node> itr = GraphDb.traversalDescription().depthFirst().expand(expander).traverse(Node("A1")).nodes().GetEnumerator();
			  AssertOrder( itr, "A1", "C1", "C2", "C3", "A2", "C4", "C5", "C6", "A3", "C7", "C8", "C9" );

			  expander = ( new OrderedByTypeExpander() ).add(_next).add(_firstComment).add(_comment);
			  itr = GraphDb.traversalDescription().depthFirst().expand(expander).traverse(Node("A1")).nodes().GetEnumerator();
			  AssertOrder( itr, "A1", "A2", "A3", "C7", "C8", "C9", "C4", "C5", "C6", "C1", "C2", "C3" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void evenDifferentDirectionsKeepsOrder()
		 public virtual void EvenDifferentDirectionsKeepsOrder()
		 {
			  PathExpander expander = ( new OrderedByTypeExpander() ).add(_next, INCOMING).add(_firstComment).add(_comment).add(_next, OUTGOING);
			  IEnumerator<Node> itr = GraphDb.traversalDescription().depthFirst().expand(expander).traverse(Node("A2")).nodes().GetEnumerator();
			  AssertOrder( itr, "A2", "A1", "C1", "C2", "C3", "C4", "C5", "C6", "A3", "C7", "C8", "C9" );
		 }

		 private void AssertOrder( IEnumerator<Node> itr, params string[] names )
		 {
			  using ( Transaction tx = BeginTx() )
			  {
					foreach ( string name in names )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Node node = itr.next();
						 assertEquals( "expected " + name + ", was " + node.GetProperty( "name" ), GetNodeWithName( name ), node );
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( itr.hasNext() );
					tx.Success();
			  }
		 }
	}

}