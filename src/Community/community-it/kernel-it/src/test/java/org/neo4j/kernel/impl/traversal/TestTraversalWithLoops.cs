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
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Evaluation = Neo4Net.Graphdb.traversal.Evaluation;
	using Evaluator = Neo4Net.Graphdb.traversal.Evaluator;
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;
	using Uniqueness = Neo4Net.Graphdb.traversal.Uniqueness;

	public class TestTraversalWithLoops : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void traverseThroughNodeWithLoop()
		 public virtual void TraverseThroughNodeWithLoop()
		 {
			  /*
			   * (a)-->(b)-->(c)-->(d)-->(e)
			   *             /  \ /  \
			   *             \__/ \__/
			   */

			  CreateGraph( "a TO b", "b TO c", "c TO c", "c TO d", "d TO d", "d TO e" );

			  using ( Transaction tx = BeginTx() )
			  {
					Node a = GetNodeWithName( "a" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node e = getNodeWithName("e");
					Node e = GetNodeWithName( "e" );
					Evaluator onlyEndNode = path => Evaluation.ofIncludes( path.endNode().Equals(e) );
					TraversalDescription basicTraverser = GraphDb.traversalDescription().evaluator(onlyEndNode);
					ExpectPaths( basicTraverser.Traverse( a ), "a,b,c,d,e" );
					ExpectPaths( basicTraverser.Uniqueness( Uniqueness.RELATIONSHIP_PATH ).traverse( a ), "a,b,c,d,e", "a,b,c,c,d,e", "a,b,c,d,d,e", "a,b,c,c,d,d,e" );
					tx.Success();
			  }
		 }
	}

}