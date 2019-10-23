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

	using Transaction = Neo4Net.GraphDb.Transaction;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using Traverser = Neo4Net.GraphDb.Traversal.Traverser;
	using Uniqueness = Neo4Net.GraphDb.Traversal.Uniqueness;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluators.excludeStartPosition;

	public class SmallestGraphEverTest : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  CreateGraph( "1 TO 2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUnrestrictedTraversalCanFinishDepthFirst()
		 public virtual void TestUnrestrictedTraversalCanFinishDepthFirst()
		 {
			  Execute( GraphDb.traversalDescription().depthFirst(), Uniqueness.NONE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUnrestrictedTraversalCanFinishBreadthFirst()
		 public virtual void TestUnrestrictedTraversalCanFinishBreadthFirst()
		 {
			  Execute( GraphDb.traversalDescription().breadthFirst(), Uniqueness.NONE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeGlobalTraversalCanFinishDepthFirst()
		 public virtual void TestNodeGlobalTraversalCanFinishDepthFirst()
		 {
			  Execute( GraphDb.traversalDescription().depthFirst(), Uniqueness.NODE_GLOBAL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeGlobalTraversalCanFinishBreadthFirst()
		 public virtual void TestNodeGlobalTraversalCanFinishBreadthFirst()
		 {
			  Execute( GraphDb.traversalDescription().breadthFirst(), Uniqueness.NODE_GLOBAL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipGlobalTraversalCanFinishDepthFirst()
		 public virtual void TestRelationshipGlobalTraversalCanFinishDepthFirst()
		 {
			  Execute( GraphDb.traversalDescription().depthFirst(), Uniqueness.RELATIONSHIP_GLOBAL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipGlobalTraversalCanFinishBreadthFirst()
		 public virtual void TestRelationshipGlobalTraversalCanFinishBreadthFirst()
		 {
			  Execute( GraphDb.traversalDescription().breadthFirst(), Uniqueness.RELATIONSHIP_GLOBAL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodePathTraversalCanFinishDepthFirst()
		 public virtual void TestNodePathTraversalCanFinishDepthFirst()
		 {
			  Execute( GraphDb.traversalDescription().depthFirst(), Uniqueness.NODE_PATH );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodePathTraversalCanFinishBreadthFirst()
		 public virtual void TestNodePathTraversalCanFinishBreadthFirst()
		 {
			  Execute( GraphDb.traversalDescription().breadthFirst(), Uniqueness.NODE_PATH );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipPathTraversalCanFinishDepthFirst()
		 public virtual void TestRelationshipPathTraversalCanFinishDepthFirst()
		 {
			  Execute( GraphDb.traversalDescription().depthFirst(), Uniqueness.RELATIONSHIP_PATH );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipPathTraversalCanFinishBreadthFirst()
		 public virtual void TestRelationshipPathTraversalCanFinishBreadthFirst()
		 {
			  Execute( GraphDb.traversalDescription().breadthFirst(), Uniqueness.RELATIONSHIP_PATH );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeRecentTraversalCanFinishDepthFirst()
		 public virtual void TestNodeRecentTraversalCanFinishDepthFirst()
		 {
			  Execute( GraphDb.traversalDescription().depthFirst(), Uniqueness.NODE_RECENT );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeRecentTraversalCanFinishBreadthFirst()
		 public virtual void TestNodeRecentTraversalCanFinishBreadthFirst()
		 {
			  Execute( GraphDb.traversalDescription().breadthFirst(), Uniqueness.NODE_RECENT );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipRecentTraversalCanFinishDepthFirst()
		 public virtual void TestRelationshipRecentTraversalCanFinishDepthFirst()
		 {
			  Execute( GraphDb.traversalDescription().depthFirst(), Uniqueness.RELATIONSHIP_RECENT );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipRecentTraversalCanFinishBreadthFirst()
		 public virtual void TestRelationshipRecentTraversalCanFinishBreadthFirst()
		 {
			  Execute( GraphDb.traversalDescription().breadthFirst(), Uniqueness.RELATIONSHIP_RECENT );
		 }

		 private void Execute( TraversalDescription traversal, Uniqueness uniqueness )
		 {
			  using ( Transaction transaction = BeginTx() )
			  {
					Traverser traverser = traversal.Uniqueness( uniqueness ).traverse( Node( "1" ) );
					assertNotEquals( "empty traversal", 0, Iterables.count( traverser ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTraverseRelationshipsWithStartNodeNotIncluded()
		 public virtual void TestTraverseRelationshipsWithStartNodeNotIncluded()
		 {
			  using ( Transaction transaction = BeginTx() )
			  {
					TraversalDescription traversal = GraphDb.traversalDescription().evaluator(excludeStartPosition());
					assertEquals( 1, Iterables.count( traversal.Traverse( Node( "1" ) ).relationships() ) );
			  }
		 }
	}

}