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
namespace Neo4Net.@internal.Kernel.Api
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public abstract class DeepRelationshipTraversalCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
		 private static long _threeRoot;
		 private static int _expectedTotal, _expectedUnique;

		 private RelationshipType _parent = withName( "PARENT" );

		 public override void CreateTestGraph( GraphDatabaseService graphDb )
		 {
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Node root = graphDb.CreateNode();
					_threeRoot = root.Id;

					Node[] leafs = new Node[32];
					for ( int i = 0; i < leafs.Length; i++ )
					{
						 leafs[i] = graphDb.CreateNode();
					}
					int offset = 0, duplicate = 12;

					Node interdup = graphDb.CreateNode();
					interdup.CreateRelationshipTo( root, _parent );
					offset = Relate( duplicate, leafs, offset, interdup );
					for ( int i = 0; i < 5; i++ )
					{
						 Node inter = graphDb.CreateNode();
						 inter.CreateRelationshipTo( root, _parent );
						 offset = Relate( 3 + i, leafs, offset, inter );
					}
					interdup.CreateRelationshipTo( root, _parent );
					for ( int i = 0; i < 4; i++ )
					{
						 Node inter = graphDb.CreateNode();
						 inter.CreateRelationshipTo( root, _parent );
						 offset = Relate( 2 + i, leafs, offset, inter );
					}

					Node inter = graphDb.CreateNode();
					inter.CreateRelationshipTo( root, _parent );
					offset = Relate( 1, leafs, offset, inter );

					_expectedTotal = offset + duplicate;
					_expectedUnique = leafs.Length;

					tx.Success();
			  }
		 }

		 private int Relate( int count, Node[] selection, int offset, Node parent )
		 {
			  for ( int i = 0; i < count; i++ )
			  {
					selection[offset++ % selection.Length].CreateRelationshipTo( parent, _parent );
			  }
			  return offset;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseTreeOfDepthThree()
		 public virtual void ShouldTraverseTreeOfDepthThree()
		 {
			  using ( NodeCursor node = cursors.allocateNodeCursor(), RelationshipGroupCursor group = cursors.allocateRelationshipGroupCursor(), RelationshipTraversalCursor relationship1 = cursors.allocateRelationshipTraversalCursor(), RelationshipTraversalCursor relationship2 = cursors.allocateRelationshipTraversalCursor() )
			  {
					MutableLongSet leafs = new LongHashSet();
					long total = 0;

					// when
					read.singleNode( _threeRoot, node );
					assertTrue( "access root node", node.Next() );
					node.Relationships( group );
					assertFalse( "single root", node.Next() );

					assertTrue( "access group of root", group.next() );
					group.Incoming( relationship1 );
					assertFalse( "single group of root", group.next() );

					while ( relationship1.next() )
					{
						 relationship1.Neighbour( node );

						 assertTrue( "child level 1", node.Next() );
						 node.Relationships( group );
						 assertFalse( "single node", node.Next() );

						 assertTrue( "group of level 1 child", group.next() );
						 group.Incoming( relationship2 );
						 assertFalse( "single group of level 1 child", group.next() );

						 while ( relationship2.next() )
						 {
							  leafs.add( relationship2.NeighbourNodeReference() );
							  total++;
						 }
					}

					// then
					assertEquals( "total number of leaf nodes", _expectedTotal, total );
					assertEquals( "number of distinct leaf nodes", _expectedUnique, leafs.size() );
			  }
		 }
	}

}