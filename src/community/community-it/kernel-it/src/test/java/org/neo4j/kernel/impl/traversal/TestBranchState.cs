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
	using Test = org.junit.Test;

	using Direction = Neo4Net.Graphdb.Direction;
	using Path = Neo4Net.Graphdb.Path;
	using Neo4Net.Graphdb;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.traversal;
	using Evaluation = Neo4Net.Graphdb.traversal.Evaluation;
	using Neo4Net.Graphdb.traversal;
	using Neo4Net.Graphdb.traversal;
	using Uniqueness = Neo4Net.Graphdb.traversal.Uniqueness;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluation.ofIncludes;

	public class TestBranchState : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void depthAsState()
		 public virtual void DepthAsState()
		 {
			  /*
			   * (a) -> (b) -> (c) -> (d)
			   *          \           ^
			   *           v         /
			   *           (e) -> (f) -> (g) -> (h)
			   */
			  CreateGraph( "a to b", "b to c", "c to d", "b to e", "e to f", "f to d", "f to g", "g to h" );

			  using ( Transaction tx = BeginTx() )
			  {
					DepthStateExpander expander = new DepthStateExpander();
					Iterables.count( GraphDb.traversalDescription().expand(expander, new Neo4Net.Graphdb.traversal.InitialBranchState_State<>(0, 0)).traverse(GetNodeWithName("a")) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void everyOtherDepthAsState()
		 public virtual void EveryOtherDepthAsState()
		 {
			  /*
			   * (a) -> (b) -> (c) -> (e)
			   */
			  CreateGraph( "a to b", "b to c", "c to d", "d to e" );
			  using ( Transaction tx = BeginTx() )
			  {

			  /*
			   * Asserts that state continues down branches even when expander doesn't
			   * set new state for every step.
			   */
					IncrementEveryOtherDepthCountingExpander expander = new IncrementEveryOtherDepthCountingExpander();
					Iterables.count( GraphDb.traversalDescription().expand(expander, new Neo4Net.Graphdb.traversal.InitialBranchState_State<>(0, 0)).traverse(GetNodeWithName("a")) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void evaluateState()
		 public virtual void EvaluateState()
		 {
			  /*
			   * (a)-1->(b)-2->(c)-3->(d)
			   *   \           ^
			   *    4         6
			   *    (e)-5->(f)
			   */
			  CreateGraph( "a TO b", "b TO c", "c TO d", "a TO e", "e TO f", "f TO c" );

			  using ( Transaction tx = BeginTx() )
			  {
					PathEvaluator<int> evaluator = new PathEvaluator_AdapterAnonymousInnerClass( this );

					ExpectPaths( GraphDb.traversalDescription().uniqueness(Uniqueness.NODE_PATH).expand(new RelationshipWeightExpander(), new Neo4Net.Graphdb.traversal.InitialBranchState_State<>(1, 1)).evaluator(evaluator).traverse(GetNodeWithName("a")), "a,b,c" );
					tx.Success();
			  }
		 }

		 private class PathEvaluator_AdapterAnonymousInnerClass : Neo4Net.Graphdb.traversal.PathEvaluator_Adapter<int>
		 {
			 private readonly TestBranchState _outerInstance;

			 public PathEvaluator_AdapterAnonymousInnerClass( TestBranchState outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override Evaluation evaluate( Path path, BranchState<int> state )
			 {
				  return ofIncludes( path.EndNode().getProperty("name").Equals("c") && state.State == 3 );
			 }
		 }

		 private class DepthStateExpander : PathExpander<int>
		 {
			  public override IEnumerable<Relationship> Expand( Path path, BranchState<int> state )
			  {
					assertEquals( path.Length(), state.State );
					state.State = state.State + 1;
					return path.EndNode().getRelationships(Direction.OUTGOING);
			  }

			  public override PathExpander<int> Reverse()
			  {
					return this;
			  }
		 }

		 private class IncrementEveryOtherDepthCountingExpander : PathExpander<int>
		 {
			  public override IEnumerable<Relationship> Expand( Path path, BranchState<int> state )
			  {
					assertEquals( path.Length() / 2, state.State );
					if ( path.Length() % 2 == 1 )
					{
						 state.State = state.State + 1;
					}
					return path.EndNode().getRelationships(Direction.OUTGOING);
			  }

			  public override PathExpander<int> Reverse()
			  {
					return this;
			  }
		 }

		 private class RelationshipWeightExpander : PathExpander<int>
		 {
			  public override IEnumerable<Relationship> Expand( Path path, BranchState<int> state )
			  {
					state.State = state.State + 1;
					return path.EndNode().getRelationships(OUTGOING);
			  }

			  public override PathExpander<int> Reverse()
			  {
					return this;
			  }
		 }
	}

}