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


	using Path = Neo4Net.GraphDb.Path;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using Traverser = Neo4Net.GraphDb.Traversal.Traverser;
	using Uniqueness = Neo4Net.GraphDb.Traversal.Uniqueness;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluators.atDepth;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluators.toDepth;

	public class DepthPitfallGraphTest : TraversalTestBase
	{
		 /* Layout:
		  *    _(2)--__
		  *   /        \
		  * (1)-(3)-_   (6)
		  *  |\_     \  /
		  *  |  (4)__ \/
		  *  \_______(5)
		  */
		 private static readonly string[] _theWorldAsWeKnowIt = new string[] { "1 TO 2", "1 TO 3", "1 TO 4", "5 TO 3", "1 TO 5", "4 TO 5", "2 TO 6", "5 TO 6" };
		 private static readonly string[] _nodeUniquePaths = new string[] { "1", "1,2", "1,2,6", "1,2,6,5", "1,2,6,5,3", "1,2,6,5,4", "1,3", "1,3,5", "1,3,5,4", "1,3,5,6", "1,3,5,6,2", "1,4", "1,4,5", "1,4,5,3", "1,4,5,6", "1,4,5,6,2", "1,5", "1,5,3", "1,5,4", "1,5,6", "1,5,6,2" };
		 private static readonly string[] _relationshipUniqueExtraPaths = new string[] { "1,2,6,5,1", "1,2,6,5,1,3", "1,2,6,5,1,3,5", "1,2,6,5,1,3,5,4", "1,2,6,5,1,3,5,4,1", "1,2,6,5,1,4", "1,2,6,5,1,4,5", "1,2,6,5,1,4,5,3", "1,2,6,5,1,4,5,3,1", "1,2,6,5,3,1", "1,2,6,5,3,1,4", "1,2,6,5,3,1,4,5", "1,2,6,5,3,1,4,5,1", "1,2,6,5,3,1,5", "1,2,6,5,3,1,5,4", "1,2,6,5,3,1,5,4,1", "1,2,6,5,4,1", "1,2,6,5,4,1,3", "1,2,6,5,4,1,3,5", "1,2,6,5,4,1,3,5,1", "1,2,6,5,4,1,5", "1,2,6,5,4,1,5,3", "1,2,6,5,4,1,5,3,1", "1,3,5,1", "1,3,5,1,2", "1,3,5,1,2,6", "1,3,5,1,2,6,5", "1,3,5,1,2,6,5,4", "1,3,5,1,2,6,5,4,1", "1,3,5,1,4", "1,3,5,1,4,5", "1,3,5,1,4,5,6", "1,3,5,1,4,5,6,2", "1,3,5,1,4,5,6,2,1", "1,3,5,4,1", "1,3,5,4,1,2", "1,3,5,4,1,2,6", "1,3,5,4,1,2,6,5", "1,3,5,4,1,2,6,5,1", "1,3,5,4,1,5", "1,3,5,4,1,5,6", "1,3,5,4,1,5,6,2", "1,3,5,4,1,5,6,2,1", "1,3,5,6,2,1", "1,3,5,6,2,1,4", "1,3,5,6,2,1,4,5", "1,3,5,6,2,1,4,5,1", "1,3,5,6,2,1,5", "1,3,5,6,2,1,5,4", "1,3,5,6,2,1,5,4,1", "1,4,5,1", "1,4,5,1,2", "1,4,5,1,2,6", "1,4,5,1,2,6,5", "1,4,5,1,2,6,5,3", "1,4,5,1,2,6,5,3,1", "1,4,5,1,3", "1,4,5,1,3,5", "1,4,5,1,3,5,6", "1,4,5,1,3,5,6,2", "1,4,5,1,3,5,6,2,1", "1,4,5,3,1", "1,4,5,3,1,2", "1,4,5,3,1,2,6", "1,4,5,3,1,2,6,5", "1,4,5,3,1,2,6,5,1", "1,4,5,3,1,5", "1,4,5,3,1,5,6", "1,4,5,3,1,5,6,2", "1,4,5,3,1,5,6,2,1", "1,4,5,6,2,1", "1,4,5,6,2,1,3", "1,4,5,6,2,1,3,5", "1,4,5,6,2,1,3,5,1", "1,4,5,6,2,1,5", "1,4,5,6,2,1,5,3", "1,4,5,6,2,1,5,3,1", "1,5,3,1", "1,5,3,1,2", "1,5,3,1,2,6", "1,5,3,1,2,6,5", "1,5,3,1,2,6,5,4", "1,5,3,1,2,6,5,4,1", "1,5,3,1,4", "1,5,3,1,4,5", "1,5,3,1,4,5,6", "1,5,3,1,4,5,6,2", "1,5,3,1,4,5,6,2,1", "1,5,4,1", "1,5,4,1,2", "1,5,4,1,2,6", "1,5,4,1,2,6,5", "1,5,4,1,2,6,5,3", "1,5,4,1,2,6,5,3,1", "1,5,4,1,3", "1,5,4,1,3,5", "1,5,4,1,3,5,6", "1,5,4,1,3,5,6,2", "1,5,4,1,3,5,6,2,1", "1,5,6,2,1", "1,5,6,2,1,3", "1,5,6,2,1,3,5", "1,5,6,2,1,3,5,4", "1,5,6,2,1,3,5,4,1", "1,5,6,2,1,4", "1,5,6,2,1,4,5", "1,5,6,2,1,4,5,3", "1,5,6,2,1,4,5,3,1" };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  CreateGraph( _theWorldAsWeKnowIt );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSmallestPossibleInit()
		 public virtual void TestSmallestPossibleInit()
		 {
			  Traverser traversal = GraphDb.traversalDescription().traverse(Node("1"));
			  int count = 0;
			  using ( Transaction transaction = BeginTx() )
			  {
					foreach ( Path position in traversal )
					{
						 count++;
						 assertNotNull( position );
						 assertNotNull( position.EndNode() );
						 if ( position.Length() > 0 )
						 {
							  assertNotNull( position.LastRelationship() );
						 }
					}
					assertNotEquals( "empty traversal", 0, count );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllNodesAreReturnedOnceDepthFirst()
		 public virtual void TestAllNodesAreReturnedOnceDepthFirst()
		 {
			  TestAllNodesAreReturnedOnce( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllNodesAreReturnedOnceBreadthFirst()
		 public virtual void TestAllNodesAreReturnedOnceBreadthFirst()
		 {
			  TestAllNodesAreReturnedOnce( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void TestAllNodesAreReturnedOnce( TraversalDescription traversal )
		 {
			  Traverser traverser = traversal.Uniqueness( Uniqueness.NODE_GLOBAL ).traverse( Node( "1" ) );

			  ExpectNodes( traverser, "1", "2", "3", "4", "5", "6" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodesAreReturnedOnceWhenSufficientRecentlyUniqueDepthFirst()
		 public virtual void TestNodesAreReturnedOnceWhenSufficientRecentlyUniqueDepthFirst()
		 {
			  TestNodesAreReturnedOnceWhenSufficientRecentlyUnique( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodesAreReturnedOnceWhenSufficientRecentlyUniqueBreadthFirst()
		 public virtual void TestNodesAreReturnedOnceWhenSufficientRecentlyUniqueBreadthFirst()
		 {
			  TestNodesAreReturnedOnceWhenSufficientRecentlyUnique( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void TestNodesAreReturnedOnceWhenSufficientRecentlyUnique( TraversalDescription description )
		 {
			  Traverser traverser = description.Uniqueness( Uniqueness.NODE_RECENT, 6 ).traverse( Node( "1" ) );

			  ExpectNodes( traverser, "1", "2", "3", "4", "5", "6" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllRelationshipsAreReturnedOnceDepthFirst()
		 public virtual void TestAllRelationshipsAreReturnedOnceDepthFirst()
		 {
			  TestAllRelationshipsAreReturnedOnce( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllRelationshipsAreReturnedOnceBreadthFirst()
		 public virtual void TestAllRelationshipsAreReturnedOnceBreadthFirst()
		 {
			  TestAllRelationshipsAreReturnedOnce( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void TestAllRelationshipsAreReturnedOnce( TraversalDescription description )
		 {
			  Traverser traverser = GraphDb.traversalDescription().uniqueness(Uniqueness.RELATIONSHIP_GLOBAL).traverse(Node("1"));

			  ExpectRelationships( traverser, _theWorldAsWeKnowIt );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipsAreReturnedOnceWhenSufficientRecentlyUniqueDepthFirst()
		 public virtual void TestRelationshipsAreReturnedOnceWhenSufficientRecentlyUniqueDepthFirst()
		 {
			  TestRelationshipsAreReturnedOnceWhenSufficientRecentlyUnique( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipsAreReturnedOnceWhenSufficientRecentlyUniqueBreadthFirst()
		 public virtual void TestRelationshipsAreReturnedOnceWhenSufficientRecentlyUniqueBreadthFirst()
		 {
			  TestRelationshipsAreReturnedOnceWhenSufficientRecentlyUnique( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void TestRelationshipsAreReturnedOnceWhenSufficientRecentlyUnique( TraversalDescription description )
		 {
			  Traverser traverser = description.Uniqueness( Uniqueness.RELATIONSHIP_RECENT, _theWorldAsWeKnowIt.Length ).traverse( Node( "1" ) );

			  ExpectRelationships( traverser, _theWorldAsWeKnowIt );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllUniqueNodePathsAreReturnedDepthFirst()
		 public virtual void TestAllUniqueNodePathsAreReturnedDepthFirst()
		 {
			  TestAllUniqueNodePathsAreReturned( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllUniqueNodePathsAreReturnedBreadthFirst()
		 public virtual void TestAllUniqueNodePathsAreReturnedBreadthFirst()
		 {
			  TestAllUniqueNodePathsAreReturned( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void TestAllUniqueNodePathsAreReturned( TraversalDescription description )
		 {
			  Traverser traverser = description.Uniqueness( Uniqueness.NODE_PATH ).traverse( Node( "1" ) );

			  ExpectPaths( traverser, _nodeUniquePaths );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllUniqueRelationshipPathsAreReturnedDepthFirst()
		 public virtual void TestAllUniqueRelationshipPathsAreReturnedDepthFirst()
		 {
			  TestAllUniqueRelationshipPathsAreReturned( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAllUniqueRelationshipPathsAreReturnedBreadthFirst()
		 public virtual void TestAllUniqueRelationshipPathsAreReturnedBreadthFirst()
		 {
			  TestAllUniqueRelationshipPathsAreReturned( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void TestAllUniqueRelationshipPathsAreReturned( TraversalDescription description )
		 {
			  ISet<string> expected = new HashSet<string>( Arrays.asList( _nodeUniquePaths ) );
			  expected.addAll( Arrays.asList( _relationshipUniqueExtraPaths ) );

			  Traverser traverser = description.Uniqueness( Uniqueness.RELATIONSHIP_PATH ).traverse( Node( "1" ) );

			  ExpectPaths( traverser, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canPruneTraversalAtSpecificDepthDepthFirst()
		 public virtual void CanPruneTraversalAtSpecificDepthDepthFirst()
		 {
			  CanPruneTraversalAtSpecificDepth( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canPruneTraversalAtSpecificDepthBreadthFirst()
		 public virtual void CanPruneTraversalAtSpecificDepthBreadthFirst()
		 {
			  CanPruneTraversalAtSpecificDepth( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void CanPruneTraversalAtSpecificDepth( TraversalDescription description )
		 {
			  Traverser traverser = description.Uniqueness( Uniqueness.NONE ).evaluator( toDepth( 1 ) ).traverse( Node( "1" ) );

			  ExpectNodes( traverser, "1", "2", "3", "4", "5" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canPreFilterNodesDepthFirst()
		 public virtual void CanPreFilterNodesDepthFirst()
		 {
			  CanPreFilterNodes( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canPreFilterNodesBreadthFirst()
		 public virtual void CanPreFilterNodesBreadthFirst()
		 {
			  CanPreFilterNodes( GraphDb.traversalDescription().breadthFirst() );
		 }

		 private void CanPreFilterNodes( TraversalDescription description )
		 {
			  Traverser traverser = description.Uniqueness( Uniqueness.NONE ).evaluator( atDepth( 2 ) ).traverse( Node( "1" ) );

			  ExpectPaths( traverser, "1,2,6", "1,3,5", "1,4,5", "1,5,3", "1,5,4", "1,5,6" );
		 }
	}

}