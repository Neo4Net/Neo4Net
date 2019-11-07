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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
	using Evaluator = Neo4Net.GraphDb.Traversal.Evaluator;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.traversal.Evaluators.includeIfAcceptedByAny;

	public class TestMultipleFilters : TraversalTestBase
	{

		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupGraph()
		 public virtual void SetupGraph()
		 {
			  //
			  //                     (a)--------
			  //                     /          \
			  //                    v            v
			  //                  (b)-->(k)<----(c)-->(f)
			  //                  / \
			  //                 v   v
			  //                (d)  (e)
			  CreateGraph( "a TO b", "b TO d", "b TO e", "b TO k", "a TO c", "c TO f", "c TO k" );

			  _tx = BeginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
				_tx.close();
		 }

		 private class MustBeConnectedToNodeFilter : System.Predicate<Path>, Evaluator
		 {
			  internal readonly Node Node;

			  internal MustBeConnectedToNodeFilter( Node node )
			  {
					this.Node = node;
			  }

			  public override bool Test( Path item )
			  {
					ResourceIterable<Relationship> relationships = (IResourceIterable<Relationship> ) item.EndNode().getRelationships(Direction.OUTGOING);
					using ( IResourceIterator<Relationship> iterator = relationships.GetEnumerator() )
					{
						 while ( iterator.MoveNext() )
						 {
							  Relationship rel = iterator.Current;
							  if ( rel.EndNode.Equals( Node ) )
							  {
									return true;
							  }
						 }
						 return false;
					}
			  }

			  public override Evaluation Evaluate( Path path )
			  {
					return Test( path ) ? Evaluation.IncludeAndContinue : Evaluation.EXCLUDE_AND_CONTINUE;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNarrowingFilters()
		 public virtual void TestNarrowingFilters()
		 {
			  Evaluator mustBeConnectedToK = new MustBeConnectedToNodeFilter( GetNodeWithName( "k" ) );
			  Evaluator mustNotHaveMoreThanTwoOutRels = path => Evaluation.ofIncludes( Iterables.count( path.endNode().getRelationships(Direction.OUTGOING) ) <= 2 );

			  TraversalDescription description = GraphDb.traversalDescription().evaluator(mustBeConnectedToK);
			  ExpectNodes( description.Traverse( Node( "a" ) ), "b", "c" );
			  ExpectNodes( description.Evaluator( mustNotHaveMoreThanTwoOutRels ).traverse( Node( "a" ) ), "c" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBroadeningFilters()
		 public virtual void TestBroadeningFilters()
		 {
			  MustBeConnectedToNodeFilter mustBeConnectedToC = new MustBeConnectedToNodeFilter( GetNodeWithName( "c" ) );
			  MustBeConnectedToNodeFilter mustBeConnectedToE = new MustBeConnectedToNodeFilter( GetNodeWithName( "e" ) );

			  // Nodes connected (OUTGOING) to c (which "a" is)
			  ExpectNodes( GraphDb.traversalDescription().evaluator(mustBeConnectedToC).traverse(Node("a")), "a" );
			  // Nodes connected (OUTGOING) to c AND e (which none is)
			  ExpectNodes( GraphDb.traversalDescription().evaluator(mustBeConnectedToC).evaluator(mustBeConnectedToE).traverse(Node("a")) );
			  // Nodes connected (OUTGOING) to c OR e (which "a" and "b" is)
			  ExpectNodes( GraphDb.traversalDescription().evaluator(includeIfAcceptedByAny(mustBeConnectedToC, mustBeConnectedToE)).traverse(Node("a")), "a", "b" );
		 }
	}

}