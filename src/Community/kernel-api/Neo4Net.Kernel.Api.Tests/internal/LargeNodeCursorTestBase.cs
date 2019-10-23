using System;
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
namespace Neo4Net.Kernel.Api.Internal
{
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public abstract class LargeNodeCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
		 private static IList<long> _nodeIds = new List<long>();
		 private static int _nNodes = 10000;

		 private static Random _random = new Random( 2 );

		 public override void CreateTestGraph( IGraphDatabaseService graphDb )
		 {
			  IList<Node> deleted = new List<Node>();
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					for ( int i = 0; i < _nNodes; i++ )
					{
						 Node node = graphDb.CreateNode();
						 if ( _random.nextBoolean() )
						 {
							  _nodeIds.Add( node.Id );
						 }
						 else
						 {
							  deleted.Add( node );
						 }
					}
					tx.Success();
			  }

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					foreach ( Node node in deleted )
					{
						 node.Delete();
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanNodes()
		 public virtual void ShouldScanNodes()
		 {
			  // given
			  IList<long> ids = new List<long>();
			  using ( NodeCursor nodes = cursors.allocateNodeCursor() )
			  {
					// when
					read.allNodesScan( nodes );
					while ( nodes.Next() )
					{
						 ids.Add( nodes.NodeReference() );
					}
			  }

			  // then
			  assertEquals( _nodeIds, ids );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessNodesByReference()
		 public virtual void ShouldAccessNodesByReference()
		 {
			  // given
			  using ( NodeCursor nodes = cursors.allocateNodeCursor() )
			  {
					foreach ( long id in _nodeIds )
					{
						 // when
						 read.singleNode( id, nodes );

						 // then
						 assertTrue( "should access defined node", nodes.Next() );
						 assertEquals( "should access the correct node", id, nodes.NodeReference() );
						 assertFalse( "should only access a single node", nodes.Next() );
					}
			  }
		 }
	}

}