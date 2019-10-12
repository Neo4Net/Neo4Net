using System.Collections.Generic;

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
namespace Org.Neo4j.Graphdb
{
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;


	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;

	public class NativeLabelScanStoreUpdateIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.DatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static readonly DatabaseRule DbRule = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public readonly TestName TestName = new TestName();

		 private Label _first;
		 private Label _second;
		 private Label _third;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupLabels()
		 public virtual void SetupLabels()
		 {
			  _first = Label.label( "First-" + TestName.MethodName );
			  _second = Label.label( "Second-" + TestName.MethodName );
			  _third = Label.label( "Third-" + TestName.MethodName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNodesWithCreatedLabel()
		 public virtual void ShouldGetNodesWithCreatedLabel()
		 {
			  // GIVEN
			  Node node1 = CreateLabeledNode( _first );
			  Node node2 = CreateLabeledNode( _second );
			  Node node3 = CreateLabeledNode( _third );
			  Node node4 = CreateLabeledNode( _first, _second, _third );
			  Node node5 = CreateLabeledNode( _first, _third );

			  // THEN
			  assertEquals( asSet( node1, node4, node5 ), Iterables.asSet( GetAllNodesWithLabel( _first ) ) );
			  assertEquals( asSet( node2, node4 ), Iterables.asSet( GetAllNodesWithLabel( _second ) ) );
			  assertEquals( asSet( node3, node4, node5 ), Iterables.asSet( GetAllNodesWithLabel( _third ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNodesWithAddedLabel()
		 public virtual void ShouldGetNodesWithAddedLabel()
		 {
			  // GIVEN
			  Node node1 = CreateLabeledNode( _first );
			  Node node2 = CreateLabeledNode( _second );
			  Node node3 = CreateLabeledNode( _third );
			  Node node4 = CreateLabeledNode( _first );
			  Node node5 = CreateLabeledNode( _first );

			  // WHEN
			  AddLabels( node4, _second, _third );
			  AddLabels( node5, _third );

			  // THEN
			  assertEquals( asSet( node1, node4, node5 ), Iterables.asSet( GetAllNodesWithLabel( _first ) ) );
			  assertEquals( asSet( node2, node4 ), Iterables.asSet( GetAllNodesWithLabel( _second ) ) );
			  assertEquals( asSet( node3, node4, node5 ), Iterables.asSet( GetAllNodesWithLabel( _third ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNodesAfterDeletedNodes()
		 public virtual void ShouldGetNodesAfterDeletedNodes()
		 {
			  // GIVEN
			  Node node1 = CreateLabeledNode( _first, _second );
			  Node node2 = CreateLabeledNode( _first, _third );

			  // WHEN
			  DeleteNode( node1 );

			  // THEN
			  assertEquals( asSet( node2 ), GetAllNodesWithLabel( _first ) );
			  assertEquals( emptySet(), GetAllNodesWithLabel(_second) );
			  assertEquals( asSet( node2 ), GetAllNodesWithLabel( _third ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNodesAfterRemovedLabels()
		 public virtual void ShouldGetNodesAfterRemovedLabels()
		 {
			  // GIVEN
			  Node node1 = CreateLabeledNode( _first, _second );
			  Node node2 = CreateLabeledNode( _first, _third );

			  // WHEN
			  RemoveLabels( node1, _first );
			  RemoveLabels( node2, _third );

			  // THEN
			  assertEquals( asSet( node2 ), GetAllNodesWithLabel( _first ) );
			  assertEquals( asSet( node1 ), GetAllNodesWithLabel( _second ) );
			  assertEquals( emptySet(), GetAllNodesWithLabel(_third) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void retrieveNodeIdsInAscendingOrder()
		 public virtual void RetrieveNodeIdsInAscendingOrder()
		 {
			  for ( int i = 0; i < 50; i++ )
			  {
					CreateLabeledNode( Labels.First, Labels.Second );
					CreateLabeledNode( Labels.Second );
					CreateLabeledNode( Labels.First );
			  }
			  long nodeWithThirdLabel = CreateLabeledNode( Labels.Third ).Id;

			  VerifyFoundNodes( Labels.Third, "Expect to see 1 matched nodeId: " + nodeWithThirdLabel, nodeWithThirdLabel );

			  Node nodeById = GetNodeById( 1 );
			  AddLabels( nodeById, Labels.Third );

			  VerifyFoundNodes( Labels.Third, "Expect to see 2 matched nodeIds: 1, " + nodeWithThirdLabel, 1, nodeWithThirdLabel );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLargeAmountsOfNodesAddedAndRemovedInSameTx()
		 public virtual void ShouldHandleLargeAmountsOfNodesAddedAndRemovedInSameTx()
		 {
			  // Given
			  GraphDatabaseService db = DbRule;
			  int labelsToAdd = 80;
			  int labelsToRemove = 40;

			  // When
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();

					// I create a lot of labels, enough to push the store to use two dynamic records
					for ( int l = 0; l < labelsToAdd; l++ )
					{
						 node.AddLabel( label( "Label-" + l ) );
					}

					// and I delete some of them, enough to bring the number of dynamic records needed down to 1
					for ( int l = 0; l < labelsToRemove; l++ )
					{
						 node.RemoveLabel( label( "Label-" + l ) );
					}

					tx.Success();
			  }

			  // Then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					// All the labels remaining should be in the label scan store
					for ( int l = labelsToAdd - 1; l >= labelsToRemove; l-- )
					{
						 Label label = label( "Label-" + l );
						 assertThat( "Should have found node when looking for label " + label, single( Db.findNodes( label ) ), equalTo( node ) );
					}
			  }
		 }

		 private void VerifyFoundNodes( Label label, string sizeMismatchMessage, params long[] expectedNodeIds )
		 {
			  using ( Transaction ignored = DbRule.beginTx() )
			  {
					ResourceIterator<Node> nodes = DbRule.findNodes( label );
					IList<Node> nodeList = Iterators.asList( nodes );
					assertThat( sizeMismatchMessage, nodeList, Matchers.hasSize( expectedNodeIds.Length ) );
					int index = 0;
					foreach ( Node node in nodeList )
					{
						 assertEquals( expectedNodeIds[index++], node.Id );
					}
			  }
		 }

		 private void RemoveLabels( Node node, params Label[] labels )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					foreach ( Label label in labels )
					{
						 node.RemoveLabel( label );
					}
					tx.Success();
			  }
		 }

		 private void DeleteNode( Node node )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }
		 }

		 private ISet<Node> GetAllNodesWithLabel( Label label )
		 {
			  using ( Transaction ignored = DbRule.beginTx() )
			  {
					return asSet( DbRule.findNodes( label ) );
			  }
		 }

		 private Node CreateLabeledNode( params Label[] labels )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					Node node = DbRule.createNode( labels );
					tx.Success();
					return node;
			  }
		 }

		 private void AddLabels( Node node, params Label[] labels )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					foreach ( Label label in labels )
					{
						 node.AddLabel( label );
					}
					tx.Success();
			  }
		 }

		 private Node GetNodeById( long id )
		 {
			  using ( Transaction ignored = DbRule.beginTx() )
			  {
					return DbRule.getNodeById( id );
			  }
		 }

		 private enum Labels
		 {
			  First,
			  Second,
			  Third
		 }
	}

}