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
namespace Neo4Net.@internal.Kernel.Api
{
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public abstract class NodeCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
		 private static IList<long> _nodeIds;
		 private static long _foo, _bar, _baz, _barbaz, _bare, _gone;

		 public override void CreateTestGraph( GraphDatabaseService graphDb )
		 {
			  Node deleted;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					_foo = graphDb.CreateNode( label( "Foo" ) ).Id;
					_bar = graphDb.CreateNode( label( "Bar" ) ).Id;
					_baz = graphDb.CreateNode( label( "Baz" ) ).Id;
					_barbaz = graphDb.CreateNode( label( "Bar" ), label( "Baz" ) ).Id;
					_gone = ( deleted = graphDb.CreateNode() ).Id;
					_bare = graphDb.CreateNode().Id;

					tx.Success();
			  }

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					deleted.Delete();

					tx.Success();
			  }

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					_nodeIds = new List<long>();
					foreach ( Node node in graphDb.AllNodes )
					{
						 _nodeIds.Add( node.Id );
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

		 // This is functionality which is only required for the hacky db.schema not to leak real data
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAccessNegativeReferences()
		 public virtual void ShouldNotAccessNegativeReferences()
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor() )
			  {
					// when
					read.singleNode( -2L, node );

					// then
					assertFalse( "should not access negative reference node", node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindDeletedNode()
		 public virtual void ShouldNotFindDeletedNode()
		 {
			  // given
			  using ( NodeCursor nodes = cursors.allocateNodeCursor() )
			  {
					// when
					read.singleNode( _gone, nodes );

					// then
					assertFalse( "should not access deleted node", nodes.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadLabels()
		 public virtual void ShouldReadLabels()
		 {
			  // given
			  using ( NodeCursor nodes = cursors.allocateNodeCursor() )
			  {
					LabelSet labels;

					// when
					read.singleNode( _foo, nodes );

					// then
					assertTrue( "should access defined node", nodes.Next() );
					labels = nodes.Labels();
					assertEquals( "number of labels", 1, labels.NumberOfLabels() );
					int fooLabel = labels.Label( 0 );
					assertTrue( nodes.HasLabel( fooLabel ) );
					assertFalse( "should only access a single node", nodes.Next() );

					// when
					read.singleNode( _bar, nodes );

					// then
					assertTrue( "should access defined node", nodes.Next() );
					labels = nodes.Labels();
					assertEquals( "number of labels", 1, labels.NumberOfLabels() );
					int barLabel = labels.Label( 0 );
					assertFalse( nodes.HasLabel( fooLabel ) );
					assertTrue( nodes.HasLabel( barLabel ) );
					assertFalse( "should only access a single node", nodes.Next() );

					// when
					read.singleNode( _baz, nodes );

					// then
					assertTrue( "should access defined node", nodes.Next() );
					labels = nodes.Labels();
					assertEquals( "number of labels", 1, labels.NumberOfLabels() );
					int bazLabel = labels.Label( 0 );
					assertFalse( nodes.HasLabel( fooLabel ) );
					assertFalse( nodes.HasLabel( barLabel ) );
					assertTrue( nodes.HasLabel( bazLabel ) );
					assertFalse( "should only access a single node", nodes.Next() );

					assertNotEquals( "distinct labels", fooLabel, barLabel );
					assertNotEquals( "distinct labels", fooLabel, bazLabel );
					assertNotEquals( "distinct labels", barLabel, bazLabel );

					// when
					read.singleNode( _barbaz, nodes );

					// then
					assertTrue( "should access defined node", nodes.Next() );
					labels = nodes.Labels();
					assertEquals( "number of labels", 2, labels.NumberOfLabels() );
					if ( labels.Label( 0 ) == barLabel )
					{
						 assertEquals( bazLabel, labels.Label( 1 ) );
					}
					else
					{
						 assertEquals( bazLabel, labels.Label( 0 ) );
						 assertEquals( barLabel, labels.Label( 1 ) );
					}
					assertFalse( nodes.HasLabel( fooLabel ) );
					assertTrue( nodes.HasLabel( barLabel ) );
					assertTrue( nodes.HasLabel( bazLabel ) );

					assertFalse( "should only access a single node", nodes.Next() );

					// when
					read.singleNode( _bare, nodes );

					// then
					assertTrue( "should access defined node", nodes.Next() );
					labels = nodes.Labels();
					assertEquals( "number of labels", 0, labels.NumberOfLabels() );
					assertFalse( nodes.HasLabel( fooLabel ) );
					assertFalse( nodes.HasLabel( barLabel ) );
					assertFalse( nodes.HasLabel( bazLabel ) );
					assertFalse( "should only access a single node", nodes.Next() );
			  }
		 }
	}

}