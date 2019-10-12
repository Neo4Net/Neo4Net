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
	using TraversalDescription = Neo4Net.Graphdb.traversal.TraversalDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluators.atDepth;

	public class DepthOneTraversalTest : TraversalTestBase
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

		 private void ShouldGetBothNodesOnDepthOne( TraversalDescription description )
		 {
			  description = description.Evaluator( atDepth( 1 ) );
			  ExpectNodes( description.Traverse( GetNodeWithName( "3" ) ), "1", "2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetBothNodesOnDepthOneForDepthFirst()
		 public virtual void ShouldGetBothNodesOnDepthOneForDepthFirst()
		 {
			  ShouldGetBothNodesOnDepthOne( GraphDb.traversalDescription().depthFirst() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetBothNodesOnDepthOneForBreadthFirst()
		 public virtual void ShouldGetBothNodesOnDepthOneForBreadthFirst()
		 {
			  ShouldGetBothNodesOnDepthOne( GraphDb.traversalDescription().breadthFirst() );
		 }
	}

}