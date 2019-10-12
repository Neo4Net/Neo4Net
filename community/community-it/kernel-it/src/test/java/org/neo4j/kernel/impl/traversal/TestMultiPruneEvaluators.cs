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
namespace Org.Neo4j.Kernel.impl.traversal
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Direction = Org.Neo4j.Graphdb.Direction;
	using Path = Org.Neo4j.Graphdb.Path;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;
	using Evaluator = Org.Neo4j.Graphdb.traversal.Evaluator;
	using Evaluators = Org.Neo4j.Graphdb.traversal.Evaluators;
	using TraversalDescription = Org.Neo4j.Graphdb.traversal.TraversalDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluators.toDepth;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;

	public class TestMultiPruneEvaluators : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupGraph()
		 public virtual void SetupGraph()
		 {
			  CreateGraph( "a to b", "a to c", "a to d", "a to e", "b to f", "b to g", "b to h", "c to i", "d to j", "d to k", "d to l", "e to m", "e to n", "k to o", "k to p", "k to q", "k to r" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMaxDepthAndCustomPruneEvaluatorCombined()
		 public virtual void TestMaxDepthAndCustomPruneEvaluatorCombined()
		 {
			  Evaluator lessThanThreeRels = path => count( path.endNode().getRelationships(Direction.OUTGOING).GetEnumerator() ) < 3 ? Evaluation.INCLUDE_AND_PRUNE : Evaluation.INCLUDE_AND_CONTINUE;

			  TraversalDescription description = GraphDb.traversalDescription().evaluator(Evaluators.all()).evaluator(toDepth(1)).evaluator(lessThanThreeRels);
			  ISet<string> expectedNodes = new HashSet<string>( asList( "a", "b", "c", "d", "e" ) );
			  using ( Transaction tx = BeginTx() )
			  {
					foreach ( Path position in description.Traverse( Node( "a" ) ) )
					{
						 string name = ( string ) position.EndNode().getProperty("name");
						 assertTrue( name + " shouldn't have been returned", expectedNodes.remove( name ) );
					}
					tx.Success();
			  }
			  assertTrue( expectedNodes.Count == 0 );
		 }
	}

}