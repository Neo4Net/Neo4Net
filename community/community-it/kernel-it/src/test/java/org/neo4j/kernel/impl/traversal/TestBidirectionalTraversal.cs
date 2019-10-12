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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Org.Neo4j.Graphdb;
	using PathExpanderBuilder = Org.Neo4j.Graphdb.PathExpanderBuilder;
	using PathExpanders = Org.Neo4j.Graphdb.PathExpanders;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using StandardBranchCollisionDetector = Org.Neo4j.Graphdb.impl.traversal.StandardBranchCollisionDetector;
	using BidirectionalTraversalDescription = Org.Neo4j.Graphdb.traversal.BidirectionalTraversalDescription;
	using BranchCollisionPolicy = Org.Neo4j.Graphdb.traversal.BranchCollisionPolicy;
	using Evaluators = Org.Neo4j.Graphdb.traversal.Evaluators;
	using Org.Neo4j.Graphdb.traversal;
	using SideSelectorPolicies = Org.Neo4j.Graphdb.traversal.SideSelectorPolicies;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;
	using TraversalDescription = Org.Neo4j.Graphdb.traversal.TraversalDescription;
	using Traverser = Org.Neo4j.Graphdb.traversal.Traverser;
	using Uniqueness = Org.Neo4j.Graphdb.traversal.Uniqueness;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluators.includeIfContainsAll;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Uniqueness.NODE_PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Uniqueness.RELATIONSHIP_PATH;

	public class TestBidirectionalTraversal : TraversalTestBase
	{
		 internal RelationshipType To = withName( "TO" );
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void init()
		 public virtual void Init()
		 {
			  _tx = BeginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _tx.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void bothSidesMustHaveSameUniqueness()
		 public virtual void BothSidesMustHaveSameUniqueness()
		 {
			  CreateGraph( "A TO B" );

			  Traverser traverse = GraphDb.bidirectionalTraversalDescription().startSide(GraphDb.traversalDescription().uniqueness(Uniqueness.NODE_GLOBAL)).endSide(GraphDb.traversalDescription().uniqueness(Uniqueness.RELATIONSHIP_GLOBAL)).traverse(GetNodeWithName("A"), GetNodeWithName("B"));
			  using ( ResourceIterator<Path> iterator = traverse.GetEnumerator() )
			  {
					Iterators.count( iterator );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathsForOneDirection()
		 public virtual void PathsForOneDirection()
		 {
			  /*
			   * (a)-->(b)==>(c)-->(d)
			   *   ^               /
			   *    \--(f)<--(e)<-/
			   */
			  CreateGraph( "a TO b", "b TO c", "c TO d", "d TO e", "e TO f", "f TO a" );

			  PathExpander<Void> expander = PathExpanders.forTypeAndDirection( To, OUTGOING );
			  ExpectPaths( GraphDb.bidirectionalTraversalDescription().mirroredSides(GraphDb.traversalDescription().uniqueness(NODE_PATH).expand(expander)).traverse(GetNodeWithName("a"), GetNodeWithName("f")), "a,b,c,d,e,f" );

			  ExpectPaths( GraphDb.bidirectionalTraversalDescription().mirroredSides(GraphDb.traversalDescription().uniqueness(RELATIONSHIP_PATH).expand(expander)).traverse(GetNodeWithName("a"), GetNodeWithName("f")), "a,b,c,d,e,f", "a,b,c,d,e,f" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void collisionEvaluator()
		 public virtual void CollisionEvaluator()
		 {
			  /*
			   *           (d)-->(e)--
			   *            ^     |   \
			   *            |     v    v
			   *           (a)-->(b)<--(f)
			   *            |    ^
			   *            v   /
			   *           (c)-/
			   */
			  CreateGraph( "a TO b", "a TO c", "c TO b", "a TO d", "d TO e", "e TO b", "e TO f", "f TO b" );

			  PathExpander<Void> expander = PathExpanders.forTypeAndDirection( To, OUTGOING );
			  BidirectionalTraversalDescription traversal = GraphDb.bidirectionalTraversalDescription().mirroredSides(GraphDb.traversalDescription().uniqueness(NODE_PATH).expand(expander));
			  ExpectPaths( traversal.CollisionEvaluator( includeIfContainsAll( GetNodeWithName( "e" ) ) ).traverse( GetNodeWithName( "a" ), GetNodeWithName( "b" ) ), "a,d,e,b", "a,d,e,f,b" );
			  ExpectPaths( traversal.CollisionEvaluator( includeIfContainsAll( GetNodeWithName( "e" ), GetNodeWithName( "f" ) ) ).traverse( GetNodeWithName( "a" ), GetNodeWithName( "b" ) ), "a,d,e,f,b" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleCollisionEvaluators()
		 public virtual void MultipleCollisionEvaluators()
		 {
			  /*
			   *           (g)
			   *           ^ \
			   *          /   v
			   *  (a)-->(b)   (c)
			   *   |        --^ ^
			   *   v       /    |
			   *  (d)-->(e)----(f)
			   */
			  CreateGraph( "a TO b", "b TO g", "g TO c", "a TO d", "d TO e", "e TO c", "e TO f", "f TO c" );

			  ExpectPaths( GraphDb.bidirectionalTraversalDescription().mirroredSides(GraphDb.traversalDescription().uniqueness(NODE_PATH)).collisionEvaluator(Evaluators.atDepth(3)).collisionEvaluator(includeIfContainsAll(GetNodeWithName("e"))).traverse(GetNodeWithName("a"), GetNodeWithName("c")), "a,d,e,c" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleStartAndEndNodes()
		 public virtual void MultipleStartAndEndNodes()
		 {
			  /*
			   * (a)--\         -->(f)
			   *       v       /
			   * (b)-->(d)<--(e)-->(g)
			   *       ^
			   * (c)--/
			   */
			  CreateGraph( "a TO d", "b TO d", "c TO d", "e TO d", "e TO f", "e TO g" );

			  PathExpander<Void> expander = PathExpanderBuilder.empty().add(To).build();
			  TraversalDescription side = GraphDb.traversalDescription().uniqueness(NODE_PATH).expand(expander);
			  ExpectPaths( GraphDb.bidirectionalTraversalDescription().mirroredSides(side).traverse(asList(GetNodeWithName("a"), GetNodeWithName("b"), GetNodeWithName("c")), asList(GetNodeWithName("f"), GetNodeWithName("g"))), "a,d,e,f", "a,d,e,g", "b,d,e,f", "b,d,e,g", "c,d,e,f", "c,d,e,g" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ensureCorrectPathEntitiesInShortPath()
		 public virtual void EnsureCorrectPathEntitiesInShortPath()
		 {
			  /*
			   * (a)-->(b)
			   */
			  CreateGraph( "a TO b" );

			  Node a = GetNodeWithName( "a" );
			  Node b = GetNodeWithName( "b" );
			  Relationship r = a.GetSingleRelationship( To, OUTGOING );
			  Path path = Iterables.single( GraphDb.bidirectionalTraversalDescription().mirroredSides(GraphDb.traversalDescription().relationships(To, OUTGOING).uniqueness(NODE_PATH)).collisionEvaluator(Evaluators.atDepth(1)).sideSelector(SideSelectorPolicies.LEVEL, 1).traverse(a, b) );
			  AssertContainsInOrder( path.Nodes(), a, b );
			  AssertContainsInOrder( path.ReverseNodes(), b, a );
			  AssertContainsInOrder( path.Relationships(), r );
			  AssertContainsInOrder( path.ReverseRelationships(), r );
			  AssertContainsInOrder( path, a, r, b );
			  assertEquals( a, path.StartNode() );
			  assertEquals( b, path.EndNode() );
			  assertEquals( r, path.LastRelationship() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mirroredTraversalReversesInitialState()
		 public virtual void MirroredTraversalReversesInitialState()
		 {
			  /*
			   * (a)-->(b)-->(c)-->(d)
			   */
			  CreateGraph( "a TO b", "b TO c", "c TO d" );

			  BranchCollisionPolicy collisionPolicy = ( evaluator, pathPredicate ) => new StandardBranchCollisionDetectorAnonymousInnerClass( this );

			  Iterables.count( GraphDb.bidirectionalTraversalDescription().mirroredSides(GraphDb.traversalDescription().uniqueness(NODE_PATH).expand(PathExpanders.forType(To), new Org.Neo4j.Graphdb.traversal.InitialBranchState_State<>(0, 10))).collisionPolicy(collisionPolicy).traverse(GetNodeWithName("a"), GetNodeWithName("d")) );
		 }

		 private class StandardBranchCollisionDetectorAnonymousInnerClass : StandardBranchCollisionDetector
		 {
			 private readonly TestBidirectionalTraversal _outerInstance;

			 public StandardBranchCollisionDetectorAnonymousInnerClass( TestBidirectionalTraversal outerInstance ) : base( null, null )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override bool includePath( Path path, TraversalBranch startPath, TraversalBranch endPath )
			 {
				  assertEquals( 0, startPath.State() );
				  assertEquals( 10, endPath.State() );
				  return true;
			 }
		 }
	}

}