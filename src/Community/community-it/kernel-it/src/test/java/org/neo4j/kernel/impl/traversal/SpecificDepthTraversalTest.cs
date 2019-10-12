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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Transaction = Neo4Net.Graphdb.Transaction;
	using Evaluators = Neo4Net.Graphdb.traversal.Evaluators;
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;

	public class SpecificDepthTraversalTest : TraversalTestBase
	{
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createTheGraph()
		 public virtual void CreateTheGraph()
		 {
			  CreateGraph( "0 ROOT 1", "1 KNOWS 2", "2 KNOWS 3", "2 KNOWS 4", "4 KNOWS 5", "5 KNOWS 6", "3 KNOWS 1" );
			  _tx = BeginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _tx.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetStartNodeOnDepthZero()
		 public virtual void ShouldGetStartNodeOnDepthZero()
		 {
			  TraversalDescription description = GraphDb.traversalDescription().evaluator(Evaluators.atDepth(0));
			  ExpectNodes( description.Traverse( GetNodeWithName( "6" ) ), "6" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectNodesFromToDepthOne()
		 public virtual void ShouldGetCorrectNodesFromToDepthOne()
		 {
			  TraversalDescription description = GraphDb.traversalDescription().evaluator(Evaluators.fromDepth(1)).evaluator(Evaluators.toDepth(1));
			  ExpectNodes( description.Traverse( GetNodeWithName( "6" ) ), "5" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectNodeAtDepthOne()
		 public virtual void ShouldGetCorrectNodeAtDepthOne()
		 {
			  TraversalDescription description = GraphDb.traversalDescription().evaluator(Evaluators.atDepth(1));
			  ExpectNodes( description.Traverse( GetNodeWithName( "6" ) ), "5" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectNodesAtDepthZero()
		 public virtual void ShouldGetCorrectNodesAtDepthZero()
		 {
			  TraversalDescription description = GraphDb.traversalDescription().evaluator(Evaluators.fromDepth(0)).evaluator(Evaluators.toDepth(0));
			  ExpectNodes( description.Traverse( GetNodeWithName( "6" ) ), "6" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetStartNodeWhenFromToIsZeroBreadthFirst()
		 public virtual void ShouldGetStartNodeWhenFromToIsZeroBreadthFirst()
		 {
			  TraversalDescription description = GraphDb.traversalDescription().breadthFirst().evaluator(Evaluators.fromDepth(0)).evaluator(Evaluators.toDepth(0));

			  ExpectNodes( description.Traverse( GetNodeWithName( "0" ) ), "0" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetStartNodeWhenAtIsZeroBreadthFirst()
		 public virtual void ShouldGetStartNodeWhenAtIsZeroBreadthFirst()
		 {
			  TraversalDescription description = GraphDb.traversalDescription().breadthFirst().evaluator(Evaluators.atDepth(0));

			  ExpectNodes( description.Traverse( GetNodeWithName( "2" ) ), "2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetSecondNodeWhenFromToIsTwoBreadthFirst()
		 public virtual void ShouldGetSecondNodeWhenFromToIsTwoBreadthFirst()
		 {
			  TraversalDescription description = GraphDb.traversalDescription().breadthFirst().evaluator(Evaluators.fromDepth(2)).evaluator(Evaluators.toDepth(2));

			  ExpectNodes( description.Traverse( GetNodeWithName( "5" ) ), "2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetSecondNodeWhenAtIsTwoBreadthFirst()
		 public virtual void ShouldGetSecondNodeWhenAtIsTwoBreadthFirst()
		 {
			  TraversalDescription description = GraphDb.traversalDescription().breadthFirst().evaluator(Evaluators.atDepth(2));

			  ExpectNodes( description.Traverse( GetNodeWithName( "6" ) ), "4" );
		 }
	}

}