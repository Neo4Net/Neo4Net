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
namespace Neo4Net.Internal.Kernel.Api
{
	using Assume = org.junit.Assume;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.BOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTestSupport.assertCount;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTestSupport.assertCounts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTestSupport.count;

	public abstract class RelationshipTraversalCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
		 private static long _bare, _start, _end;
		 private static RelationshipTestSupport.StartNode _sparse, _dense;

		 protected internal virtual bool SupportsDirectTraversal()
		 {
			  return true;
		 }

		 protected internal virtual bool SupportsSparseNodes()
		 {
			  return true;
		 }

		 private static void BareStartAndEnd( GraphDatabaseService graphDb )
		 {
			  using ( Neo4Net.Graphdb.Transaction tx = graphDb.BeginTx() )
			  {
					_bare = graphDb.CreateNode().Id;

					Node x = graphDb.CreateNode(), y = graphDb.CreateNode();
					_start = x.Id;
					_end = y.Id;
					x.CreateRelationshipTo( y, withName( "GEN" ) );

					tx.Success();
			  }
		 }

		 public override void CreateTestGraph( GraphDatabaseService graphDb )
		 {
			  RelationshipTestSupport.SomeGraph( graphDb );
			  BareStartAndEnd( graphDb );

			  _sparse = RelationshipTestSupport.Sparse( graphDb );
			  _dense = RelationshipTestSupport.Dense( graphDb );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAccessGroupsOfBareNode()
		 public virtual void ShouldNotAccessGroupsOfBareNode()
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor(), RelationshipGroupCursor group = cursors.allocateRelationshipGroupCursor() )
			  {
					// when
					read.singleNode( _bare, node );
					assertTrue( "access node", node.Next() );
					node.Relationships( group );

					// then
					assertFalse( "access group", group.next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseRelationshipsOfGivenType()
		 public virtual void ShouldTraverseRelationshipsOfGivenType()
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor(), RelationshipGroupCursor group = cursors.allocateRelationshipGroupCursor(), RelationshipTraversalCursor relationship = cursors.allocateRelationshipTraversalCursor() )
			  {
					int empty = 0;
					// when
					read.allNodesScan( node );
					while ( node.Next() )
					{
						 node.Relationships( group );
						 bool none = true;
						 while ( group.next() )
						 {
							  none = false;
							  Sizes degree = new Sizes();
							  group.Outgoing( relationship );
							  while ( relationship.next() )
							  {
									assertEquals( "node #" + node.NodeReference() + " relationship should have same label as group", group.Type(), relationship.Type() );
									degree.Outgoing++;
							  }
							  group.Incoming( relationship );
							  while ( relationship.next() )
							  {
									assertEquals( "node #" + node.NodeReference() + "relationship should have same label as group", group.Type(), relationship.Type() );
									degree.Incoming++;
							  }
							  group.Loops( relationship );
							  while ( relationship.next() )
							  {
									assertEquals( "node #" + node.NodeReference() + "relationship should have same label as group", group.Type(), relationship.Type() );
									degree.Loop++;
							  }

							  // then
							  assertNotEquals( "all", 0, degree.Incoming + degree.Outgoing + degree.Loop );
							  assertEquals( "node #" + node.NodeReference() + " outgoing", group.OutgoingCount(), degree.Outgoing );
							  assertEquals( "node #" + node.NodeReference() + " incoming", group.IncomingCount(), degree.Incoming );
							  assertEquals( "node #" + node.NodeReference() + " loop", group.LoopCount(), degree.Loop );
							  assertEquals( "node #" + node.NodeReference() + " all = incoming + outgoing - loop", group.TotalCount(), degree.Incoming + degree.Outgoing + degree.Loop );
						 }
						 if ( none )
						 {
							  empty++;
						 }
					}

					// then
					assertEquals( "number of empty nodes", 1, empty );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFollowSpecificRelationship()
		 public virtual void ShouldFollowSpecificRelationship()
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor(), RelationshipGroupCursor group = cursors.allocateRelationshipGroupCursor(), RelationshipTraversalCursor relationship = cursors.allocateRelationshipTraversalCursor() )
			  {
					// when - traversing from start to end
					read.singleNode( _start, node );
					assertTrue( "access start node", node.Next() );
					node.Relationships( group );
					assertTrue( "access relationship group", group.next() );
					group.Outgoing( relationship );
					assertTrue( "access outgoing relationships", relationship.next() );

					// then
					assertEquals( "source node", _start, relationship.SourceNodeReference() );
					assertEquals( "target node", _end, relationship.TargetNodeReference() );

					assertEquals( "node of origin", _start, relationship.OriginNodeReference() );
					assertEquals( "neighbouring node", _end, relationship.NeighbourNodeReference() );

					assertEquals( "relationship should have same label as group", group.Type(), relationship.Type() );

					assertFalse( "only a single relationship", relationship.next() );

					group.Incoming( relationship );
					assertFalse( "no incoming relationships", relationship.next() );
					group.Loops( relationship );
					assertFalse( "no loop relationships", relationship.next() );

					assertFalse( "only a single group", group.next() );

					// when - traversing from end to start
					read.singleNode( _end, node );
					assertTrue( "access start node", node.Next() );
					node.Relationships( group );
					assertTrue( "access relationship group", group.next() );
					group.Incoming( relationship );
					assertTrue( "access incoming relationships", relationship.next() );

					// then
					assertEquals( "source node", _start, relationship.SourceNodeReference() );
					assertEquals( "target node", _end, relationship.TargetNodeReference() );

					assertEquals( "node of origin", _end, relationship.OriginNodeReference() );
					assertEquals( "neighbouring node", _start, relationship.NeighbourNodeReference() );

					assertEquals( "relationship should have same label as group", group.Type(), relationship.Type() );

					assertFalse( "only a single relationship", relationship.next() );

					group.Outgoing( relationship );
					assertFalse( "no outgoing relationships", relationship.next() );
					group.Loops( relationship );
					assertFalse( "no loop relationships", relationship.next() );

					assertFalse( "only a single group", group.next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveBeenAbleToCreateDenseAndSparseNodes()
		 public virtual void ShouldHaveBeenAbleToCreateDenseAndSparseNodes()
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor() )
			  {
					read.singleNode( _dense.id, node );
					assertTrue( "access dense node", node.Next() );
					assertTrue( "dense node", node.Dense );

					read.singleNode( _sparse.id, node );
					assertTrue( "access sparse node", node.Next() );
					assertFalse( "sparse node", node.Dense && SupportsSparseNodes() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseSparseNodeViaGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseSparseNodeViaGroups()
		 {
			  TraverseViaGroups( _sparse, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseDenseNodeViaGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseDenseNodeViaGroups()
		 {
			  TraverseViaGroups( _dense, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseSparseNodeViaGroupsWithDetachedReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseSparseNodeViaGroupsWithDetachedReferences()
		 {
			  TraverseViaGroups( _sparse, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseDenseNodeViaGroupsWithDetachedReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseDenseNodeViaGroupsWithDetachedReferences()
		 {
			  TraverseViaGroups( _dense, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseSparseNodeWithoutGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseSparseNodeWithoutGroups()
		 {
			  Assume.assumeTrue( SupportsSparseNodes() && SupportsDirectTraversal() );
			  TraverseWithoutGroups( _sparse, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseDenseNodeWithoutGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseDenseNodeWithoutGroups()
		 {
			  Assume.assumeTrue( SupportsDirectTraversal() );
			  TraverseWithoutGroups( _dense, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseSparseNodeWithoutGroupsWithDetachedReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseSparseNodeWithoutGroupsWithDetachedReferences()
		 {
			  Assume.assumeTrue( SupportsSparseNodes() );
			  TraverseWithoutGroups( _sparse, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseDenseNodeWithoutGroupsWithDetachedReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseDenseNodeWithoutGroupsWithDetachedReferences()
		 {
			  Assume.assumeTrue( SupportsDirectTraversal() );
			  TraverseWithoutGroups( _dense, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void traverseViaGroups(RelationshipTestSupport.StartNode start, boolean detached) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void TraverseViaGroups( RelationshipTestSupport.StartNode start, bool detached )
		 {
			  // given
			  IDictionary<string, int> expectedCounts = start.ExpectedCounts();

			  using ( NodeCursor node = cursors.allocateNodeCursor(), RelationshipGroupCursor group = cursors.allocateRelationshipGroupCursor(), RelationshipTraversalCursor relationship = cursors.allocateRelationshipTraversalCursor() )
			  {
					// when
					read.singleNode( start.Id, node );
					assertTrue( "access node", node.Next() );
					if ( detached )
					{
						 read.relationshipGroups( start.Id, node.RelationshipGroupReference(), group );
					}
					else
					{
						 node.Relationships( group );
					}

					while ( group.next() )
					{
						 // outgoing
						 if ( detached )
						 {
							  read.relationships( start.Id, group.OutgoingReference(), relationship );
						 }
						 else
						 {
							  group.Outgoing( relationship );
						 }
						 // then
						 assertCount( tx, relationship, expectedCounts, group.Type(), OUTGOING );

						 // incoming
						 if ( detached )
						 {
							  read.relationships( start.Id, group.IncomingReference(), relationship );
						 }
						 else
						 {
							  group.Incoming( relationship );
						 }
						 // then
						 assertCount( tx, relationship, expectedCounts, group.Type(), INCOMING );

						 // loops
						 if ( detached )
						 {
							  read.relationships( start.Id, group.LoopsReference(), relationship );
						 }
						 else
						 {
							  group.Loops( relationship );
						 }
						 // then
						 assertCount( tx, relationship, expectedCounts, group.Type(), BOTH );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void traverseWithoutGroups(RelationshipTestSupport.StartNode start, boolean detached) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void TraverseWithoutGroups( RelationshipTestSupport.StartNode start, bool detached )
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor(), RelationshipTraversalCursor relationship = cursors.allocateRelationshipTraversalCursor() )
			  {
					// when
					read.singleNode( start.Id, node );
					assertTrue( "access node", node.Next() );

					if ( detached )
					{
						 read.relationships( start.Id, node.AllRelationshipsReference(), relationship );
					}
					else
					{
						 node.AllRelationships( relationship );
					}

					IDictionary<string, int> counts = count( tx, relationship );

					// then
					assertCounts( start.ExpectedCounts(), counts );
			  }
		 }

		 private class Sizes
		 {
			  internal int Incoming, Outgoing, Loop;
		 }
	}

}