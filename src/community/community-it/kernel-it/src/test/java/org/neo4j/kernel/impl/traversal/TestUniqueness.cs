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

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Traverser = Neo4Net.GraphDb.traversal.Traverser;
	using Uniqueness = Neo4Net.GraphDb.traversal.Uniqueness;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluators.includeWhereEndNodeIs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Uniqueness.NODE_GLOBAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Uniqueness.NODE_LEVEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Uniqueness.RELATIONSHIP_GLOBAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Uniqueness.RELATIONSHIP_LEVEL;

	public class TestUniqueness : TraversalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeLevelUniqueness()
		 public virtual void NodeLevelUniqueness()
		 {
			  /*
			   *         (b)
			   *       /  |  \
			   *    (e)==(a)--(c)
			   *       \  |
			   *         (d)
			   */

			  CreateGraph( "a TO b", "a TO c", "a TO d", "a TO e", "a TO e", "b TO e", "d TO e", "c TO b" );
			  RelationshipType to = withName( "TO" );
			  using ( Transaction tx = BeginTx() )
			  {
					Node a = GetNodeWithName( "a" );
					Node e = GetNodeWithName( "e" );
					Path[] paths = SplitPathsOnePerLevel( GraphDb.traversalDescription().relationships(to, OUTGOING).uniqueness(NODE_LEVEL).evaluator(includeWhereEndNodeIs(e)).traverse(a) );
					NodePathRepresentation pathRepresentation = new NodePathRepresentation( NamePropertyRepresentation );

					assertEquals( "a,e", pathRepresentation.Represent( paths[1] ) );
					string levelTwoPathRepresentation = pathRepresentation.Represent( paths[2] );
					assertTrue( levelTwoPathRepresentation.Equals( "a,b,e" ) || levelTwoPathRepresentation.Equals( "a,d,e" ) );
					assertEquals( "a,c,b,e", pathRepresentation.Represent( paths[3] ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeGlobalUniqueness()
		 public virtual void NodeGlobalUniqueness()
		 {
			  /*
			   * (a)-TO->(b)-TO->(c)
			   *   \----TO---->/
			   */
			  CreateGraph( "a TO b", "a TO c", "b TO c" );
			  RelationshipType to = withName( "TO" );

			  using ( Transaction tx = BeginTx() )
			  {
					Node a = GetNodeWithName( "a" );
					Node c = GetNodeWithName( "c" );
					IEnumerator<Path> path = GraphDb.traversalDescription().relationships(to, OUTGOING).uniqueness(NODE_GLOBAL).evaluator(includeWhereEndNodeIs(c)).traverse(a).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Path thePath = path.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( path.hasNext() );
					NodePathRepresentation pathRepresentation = new NodePathRepresentation( NamePropertyRepresentation );

					assertEquals( "a,b,c", pathRepresentation.Represent( thePath ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipLevelAndGlobalUniqueness()
		 public virtual void RelationshipLevelAndGlobalUniqueness()
		 {
			  /*
			   *    (a)=TO=>(b)=TO=>(c)-TO->(d)
			   *       \====TO====>/
			   */

			  CreateGraph( "a TO b", "b TO c", "a TO b", "b TO c", "a TO c", "a TO c", "c TO d" );
			  RelationshipType to = withName( "TO" );

			  using ( Transaction tx = BeginTx() )
			  {
					Node a = GetNodeWithName( "a" );
					Node d = GetNodeWithName( "d" );

					IEnumerator<Path> paths = GraphDb.traversalDescription().relationships(to, OUTGOING).uniqueness(Uniqueness.NONE).evaluator(includeWhereEndNodeIs(d)).traverse(a).GetEnumerator();
					int count = 0;
					while ( paths.MoveNext() )
					{
						 count++;
						 paths.Current;
					}
					assertEquals( "wrong number of paths calculated, the test assumption is wrong", 6, count );

					// Now do the same traversal but with unique per level relationships
					paths = GraphDb.traversalDescription().relationships(to, OUTGOING).uniqueness(RELATIONSHIP_LEVEL).evaluator(includeWhereEndNodeIs(d)).traverse(a).GetEnumerator();
					count = 0;
					while ( paths.MoveNext() )
					{
						 count++;
						 paths.Current;
					}
					assertEquals( "wrong number of paths calculated with relationship level uniqueness", 2, count );
					/*
					*  And yet again, but this time with global uniqueness, it should present only one path, since
					*  c TO d is contained on all paths.
					*/
					paths = GraphDb.traversalDescription().relationships(to, OUTGOING).uniqueness(RELATIONSHIP_GLOBAL).evaluator(includeWhereEndNodeIs(d)).traverse(a).GetEnumerator();
					count = 0;
					while ( paths.MoveNext() )
					{
						 count++;
						 paths.Current;
					}
					assertEquals( "wrong number of paths calculated with relationship global uniqueness", 1, count );
			  }
		 }

		 private Path[] SplitPathsOnePerLevel( Traverser traverser )
		 {
			  Path[] paths = new Path[10];
			  foreach ( Path path in traverser )
			  {
					int depth = path.Length();
					if ( paths[depth] != null )
					{
						 fail( "More than one path one depth " + depth );
					}
					paths[depth] = path;
			  }
			  return paths;
		 }
	}

}