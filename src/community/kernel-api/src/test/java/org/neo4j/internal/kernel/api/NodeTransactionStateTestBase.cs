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
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;

	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") public abstract class NodeTransactionStateTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class NodeTransactionStateTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNodeInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNodeInTransaction()
		 {
			  long nodeId;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );
						 assertEquals( nodeId, node.NodeReference() );
						 assertFalse( "should only find one node", node.Next() );
					}
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertEquals( nodeId, graphDb.getNodeById( nodeId ).Id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNewLabeledNodeInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNewLabeledNodeInTransaction()
		 {
			  long nodeId;
			  int labelId;
			  const string labelName = "Town";

			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					labelId = tx.Token().labelGetOrCreateForName(labelName);
					tx.DataWrite().nodeAddLabel(nodeId, labelId);

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );

						 LabelSet labels = node.Labels();
						 assertEquals( 1, labels.NumberOfLabels() );
						 assertEquals( labelId, labels.Label( 0 ) );
						 assertTrue( node.HasLabel( labelId ) );
						 assertFalse( node.HasLabel( labelId + 1 ) );
						 assertFalse( "should only find one node", node.Next() );
					}
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).Labels, equalTo( Iterables.iterable( label( labelName ) ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeLabelChangesInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeLabelChangesInTransaction()
		 {
			  long nodeId;
			  int toRetain, toDelete, toAdd, toRegret;
			  const string toRetainName = "ToRetain";
			  const string toDeleteName = "ToDelete";
			  const string toAddName = "ToAdd";
			  const string toRegretName = "ToRegret";

			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					toRetain = tx.Token().labelGetOrCreateForName(toRetainName);
					toDelete = tx.Token().labelGetOrCreateForName(toDeleteName);
					tx.DataWrite().nodeAddLabel(nodeId, toRetain);
					tx.DataWrite().nodeAddLabel(nodeId, toDelete);
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).Labels, containsInAnyOrder( label( toRetainName ), label( toDeleteName ) ) );
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					toAdd = tx.Token().labelGetOrCreateForName(toAddName);
					tx.DataWrite().nodeAddLabel(nodeId, toAdd);
					tx.DataWrite().nodeRemoveLabel(nodeId, toDelete);

					toRegret = tx.Token().labelGetOrCreateForName(toRegretName);
					tx.DataWrite().nodeAddLabel(nodeId, toRegret);
					tx.DataWrite().nodeRemoveLabel(nodeId, toRegret);

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );

						 AssertLabels( node.Labels(), toRetain, toAdd );
						 assertTrue( node.HasLabel( toAdd ) );
						 assertTrue( node.HasLabel( toRetain ) );
						 assertFalse( node.HasLabel( toDelete ) );
						 assertFalse( node.HasLabel( toRegret ) );
						 assertFalse( "should only find one node", node.Next() );
					}
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).Labels, containsInAnyOrder( label( toRetainName ), label( toAddName ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDiscoverDeletedNodeInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverDeletedNodeInTransaction()
		 {
			  long nodeId;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					assertTrue( tx.DataWrite().nodeDelete(nodeId) );
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertFalse( node.Next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleNodeDeletions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMultipleNodeDeletions()
		 {
			  long nodeId;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					assertTrue( tx.DataWrite().nodeDelete(nodeId) );
					assertFalse( tx.DataWrite().nodeDelete(nodeId) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNewNodePropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNewNodePropertyInTransaction()
		 {
			  long nodeId;
			  string propKey1 = "prop1";
			  string propKey2 = "prop2";

			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					int prop1 = tx.Token().propertyKeyGetOrCreateForName(propKey1);
					int prop2 = tx.Token().propertyKeyGetOrCreateForName(propKey2);
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, prop1, stringValue("hello")), NO_VALUE );
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, prop2, stringValue("world")), NO_VALUE );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );

						 node.Properties( property );
						 assertTrue( property.Next() );
						 //First property
						 assertEquals( prop1, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("hello") );
						 //second property
						 assertTrue( property.Next() );
						 assertEquals( prop2, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("world") );

						 assertFalse( "should only find two properties", property.Next() );
						 assertFalse( "should only find one node", node.Next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeAddedPropertyFromExistingNodeWithoutPropertiesInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeAddedPropertyFromExistingNodeWithoutPropertiesInTransaction()
		 {
			  // Given
			  long nodeId;
			  string propKey = "prop1";
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					int propToken = tx.Token().propertyKeyGetOrCreateForName(propKey);
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, propToken, stringValue("hello")), NO_VALUE );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );

						 node.Properties( property );
						 assertTrue( property.Next() );
						 assertEquals( propToken, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("hello") );

						 assertFalse( "should only find one properties", property.Next() );
						 assertFalse( "should only find one node", node.Next() );
					}

					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).getProperty( propKey ), equalTo( "hello" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeAddedPropertyFromExistingNodeWithPropertiesInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeAddedPropertyFromExistingNodeWithPropertiesInTransaction()
		 {
			  // Given
			  long nodeId;
			  string propKey1 = "prop1";
			  string propKey2 = "prop2";
			  int propToken1;
			  int propToken2;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					propToken1 = tx.Token().propertyKeyGetOrCreateForName(propKey1);
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, propToken1, stringValue("hello")), NO_VALUE );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					propToken2 = tx.Token().propertyKeyGetOrCreateForName(propKey2);
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, propToken2, stringValue("world")), NO_VALUE );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );

						 node.Properties( property );

						 //property 2, start with tx state
						 assertTrue( property.Next() );
						 assertEquals( propToken2, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("world") );

						 //property 1, from disk
						 assertTrue( property.Next() );
						 assertEquals( propToken1, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("hello") );

						 assertFalse( "should only find two properties", property.Next() );
						 assertFalse( "should only find one node", node.Next() );
					}
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).getProperty( propKey1 ), equalTo( "hello" ) );
					assertThat( graphDb.getNodeById( nodeId ).getProperty( propKey2 ), equalTo( "world" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeUpdatedPropertyFromExistingNodeWithPropertiesInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeUpdatedPropertyFromExistingNodeWithPropertiesInTransaction()
		 {
			  // Given
			  long nodeId;
			  string propKey = "prop1";
			  int propToken;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					propToken = tx.Token().propertyKeyGetOrCreateForName(propKey);
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, propToken, stringValue("hello")), NO_VALUE );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, propToken, stringValue("world")), stringValue("hello") );
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );

						 node.Properties( property );

						 assertTrue( property.Next() );
						 assertEquals( propToken, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("world") );

						 assertFalse( "should only find one property", property.Next() );
						 assertFalse( "should only find one node", node.Next() );
					}

					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).getProperty( propKey ), equalTo( "world" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeRemovedPropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeRemovedPropertyInTransaction()
		 {
			  // Given
			  long nodeId;
			  string propKey = "prop1";
			  int propToken;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					propToken = tx.Token().propertyKeyGetOrCreateForName(propKey);
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, propToken, stringValue("hello")), NO_VALUE );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					assertEquals( tx.DataWrite().nodeRemoveProperty(nodeId, propToken), stringValue("hello") );
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );

						 node.Properties( property );
						 assertFalse( "should not find any properties", property.Next() );
						 assertFalse( "should only find one node", node.Next() );
					}

					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertFalse( graphDb.getNodeById( nodeId ).hasProperty( propKey ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeRemovedThenAddedPropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeRemovedThenAddedPropertyInTransaction()
		 {
			  // Given
			  long nodeId;
			  string propKey = "prop1";
			  int propToken;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					propToken = tx.Token().propertyKeyGetOrCreateForName(propKey);
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, propToken, stringValue("hello")), NO_VALUE );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					assertEquals( tx.DataWrite().nodeRemoveProperty(nodeId, propToken), stringValue("hello") );
					assertEquals( tx.DataWrite().nodeSetProperty(nodeId, propToken, stringValue("world")), NO_VALUE );
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(nodeId, node);
						 assertTrue( "should access node", node.Next() );

						 node.Properties( property );
						 assertTrue( property.Next() );
						 assertEquals( propToken, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("world") );

						 assertFalse( "should not find any properties", property.Next() );
						 assertFalse( "should only find one node", node.Next() );
					}

					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).getProperty( propKey ), equalTo( "world" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeExistingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeExistingNode()
		 {
			  // Given
			  long node;
			  using ( Transaction tx = beginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					assertTrue( tx.DataRead().nodeExists(node) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeNonExistingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeNonExistingNode()
		 {
			  // Given, empty db

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					assertFalse( tx.DataRead().nodeExists(1337L) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNodeExistingInTxOnly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNodeExistingInTxOnly()
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					long node = tx.DataWrite().nodeCreate();
					assertTrue( tx.DataRead().nodeExists(node) );

			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDeletedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeDeletedNode()
		 {
			  // Given
			  long node;
			  using ( Transaction tx = beginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					tx.DataWrite().nodeDelete(node);
					assertFalse( tx.DataRead().nodeExists(node) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindDeletedNodeInLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDeletedNodeInLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeDelete(node.NodeConflict);
					tx.DataRead().nodeLabelScan(node.LabelsConflict[0], cursor);

					// then
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindNodeWithRemovedLabelInLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNodeWithRemovedLabelInLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(node.NodeConflict, node.LabelsConflict[0]);
					tx.DataRead().nodeLabelScan(node.LabelsConflict[0], cursor);

					// then
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindUpdatedNodeInInLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindUpdatedNodeInInLabelScan()
		 {
			  // Given
			  Node node = CreateNode();

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					int label = tx.TokenWrite().labelGetOrCreateForName("label");
					tx.DataWrite().nodeAddLabel(node.NodeConflict, label);
					tx.DataRead().nodeLabelScan(label, cursor);

					// then
					assertTrue( cursor.Next() );
					assertEquals( node.NodeConflict, cursor.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindSwappedNodeInLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindSwappedNodeInLabelScan()
		 {
			  // Given
			  Node node1 = CreateNode( "label" );
			  Node node2 = CreateNode();

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(node1.NodeConflict, node1.LabelsConflict[0]);
					tx.DataWrite().nodeAddLabel(node2.NodeConflict, node1.LabelsConflict[0]);
					tx.DataRead().nodeLabelScan(node1.LabelsConflict[0], cursor);

					// then
					assertTrue( cursor.Next() );
					assertEquals( node2.NodeConflict, cursor.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldNotFindDeletedNodeInDisjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDeletedNodeInDisjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label1", "label2" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeDelete(node.NodeConflict);
					tx.DataRead().nodeLabelUnionScan(cursor, node.LabelsConflict);

					// then
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldFindNodeWithOneRemovedLabelInDisjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindNodeWithOneRemovedLabelInDisjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label1", "label2" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(node.NodeConflict, node.LabelsConflict[1]);
					tx.DataRead().nodeLabelUnionScan(cursor, node.LabelsConflict);

					// then
					assertTrue( cursor.Next() );
					assertEquals( node.NodeConflict, cursor.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldNotFindNodeWithAllRemovedLabelsInDisjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNodeWithAllRemovedLabelsInDisjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label1", "label2" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(node.NodeConflict, node.LabelsConflict[0]);
					tx.DataWrite().nodeRemoveLabel(node.NodeConflict, node.LabelsConflict[1]);
					tx.DataRead().nodeLabelUnionScan(cursor, node.LabelsConflict);

					// then
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldNotFindNodeWithOneRemovedLabelsInDisjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNodeWithOneRemovedLabelsInDisjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label1" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					int label1 = tx.TokenWrite().labelGetOrCreateForName("label1");
					int label2 = tx.TokenWrite().labelGetOrCreateForName("label2");

					tx.DataWrite().nodeRemoveLabel(node.NodeConflict, label1);
					tx.DataRead().nodeLabelUnionScan(cursor, label1, label2);

					// then
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldFindUpdatedNodeInInDisjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindUpdatedNodeInInDisjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label1" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					int label2 = tx.TokenWrite().labelGetOrCreateForName("label2");
					tx.DataWrite().nodeAddLabel(node.NodeConflict, label2);
					tx.DataRead().nodeLabelUnionScan(cursor, node.LabelsConflict[0], label2);

					// then
					assertTrue( cursor.Next() );
					assertEquals( node.NodeConflict, cursor.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldNotFindDeletedNodeInConjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDeletedNodeInConjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label1", "label2" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeDelete(node.NodeConflict);
					tx.DataRead().nodeLabelIntersectionScan(cursor, node.LabelsConflict);

					// then
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldNotFindNodeWithRemovedLabelInConjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNodeWithRemovedLabelInConjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label1", "label2" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(node.NodeConflict, node.LabelsConflict[1]);
					tx.DataRead().nodeLabelIntersectionScan(cursor, node.LabelsConflict);

					// then
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldFindUpdatedNodeInInConjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindUpdatedNodeInInConjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode( "label1" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					int label2 = tx.TokenWrite().labelGetOrCreateForName("label2");
					tx.DataWrite().nodeAddLabel(node.NodeConflict, label2);
					tx.DataRead().nodeLabelIntersectionScan(cursor, node.LabelsConflict[0], label2);

					// then
					assertTrue( cursor.Next() );
					assertEquals( node.NodeConflict, cursor.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldNotFindNodeWithJustOneUpdatedLabelInInConjunctionLabelScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNodeWithJustOneUpdatedLabelInInConjunctionLabelScan()
		 {
			  // Given
			  Node node = CreateNode();

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
			  {
					// when
					int label1 = tx.TokenWrite().labelGetOrCreateForName("labe1");
					int label2 = tx.TokenWrite().labelGetOrCreateForName("label2");
					tx.DataWrite().nodeAddLabel(node.NodeConflict, label2);
					tx.DataRead().nodeLabelIntersectionScan(cursor, label1, label2);

					// then
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountNewLabelsFromTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCountNewLabelsFromTxState()
		 {
			  // Given
			  Node node1 = CreateNode( "label" );
			  Node node2 = CreateNode();

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction() )
			  {
					// when
					tx.DataWrite().nodeAddLabel(node2.NodeConflict, node1.LabelsConflict[0]);
					long countTxState = tx.DataRead().countsForNode(node1.LabelsConflict[0]);
					long countNoTxState = tx.DataRead().countsForNodeWithoutTxState(node1.LabelsConflict[0]);

					// then
					assertEquals( 2, countTxState );
					assertEquals( 1, countNoTxState );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountNewNodesFromTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCountNewNodesFromTxState()
		 {
			  // Given
			  CreateNode();
			  CreateNode();

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction() )
			  {
					// when
					tx.DataWrite().nodeCreate();
					long countTxState = tx.DataRead().countsForNode(-1);
					long countNoTxState = tx.DataRead().countsForNodeWithoutTxState(-1);

					// then
					assertEquals( 3, countTxState );
					assertEquals( 2, countNoTxState );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCountRemovedLabelsFromTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCountRemovedLabelsFromTxState()
		 {
			  // Given
			  Node node1 = CreateNode( "label" );
			  Node node2 = CreateNode( "label" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(node2.NodeConflict, node2.LabelsConflict[0]);
					long countTxState = tx.DataRead().countsForNode(node1.LabelsConflict[0]);
					long countNoTxState = tx.DataRead().countsForNodeWithoutTxState(node1.LabelsConflict[0]);

					// then
					assertEquals( 1, countTxState );
					assertEquals( 2, countNoTxState );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCountRemovedNodesFromTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCountRemovedNodesFromTxState()
		 {
			  // Given
			  Node node1 = CreateNode( "label" );
			  Node node2 = CreateNode( "label" );

			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction() )
			  {
					// when
					tx.DataWrite().nodeDelete(node2.NodeConflict);
					long countTxState = tx.DataRead().countsForNode(node1.LabelsConflict[0]);
					long countNoTxState = tx.DataRead().countsForNodeWithoutTxState(node1.LabelsConflict[0]);

					// then
					assertEquals( 1, countTxState );
					assertEquals( 2, countNoTxState );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hasPropertiesShouldSeeNewlyCreatedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HasPropertiesShouldSeeNewlyCreatedProperties()
		 {
			  // Given
			  long node;
			  using ( Transaction tx = beginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					using ( NodeCursor cursor = tx.Cursors().allocateNodeCursor(), PropertyCursor props = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(node, cursor);
						 assertTrue( cursor.Next() );
						 assertFalse( HasProperties( cursor, props ) );
						 tx.DataWrite().nodeSetProperty(node, tx.TokenWrite().propertyKeyGetOrCreateForName("prop"), stringValue("foo"));
						 assertTrue( HasProperties( cursor, props ) );
					}
			  }
		 }

		 private bool HasProperties( NodeCursor cursor, PropertyCursor props )
		 {
			  cursor.Properties( props );
			  return props.Next();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hasPropertiesShouldSeeNewlyCreatedPropertiesOnNewlyCreatedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HasPropertiesShouldSeeNewlyCreatedPropertiesOnNewlyCreatedNode()
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					long node = tx.DataWrite().nodeCreate();
					using ( NodeCursor cursor = tx.Cursors().allocateNodeCursor(), PropertyCursor props = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(node, cursor);
						 assertTrue( cursor.Next() );
						 assertFalse( HasProperties( cursor, props ) );
						 tx.DataWrite().nodeSetProperty(node, tx.TokenWrite().propertyKeyGetOrCreateForName("prop"), stringValue("foo"));
						 assertTrue( HasProperties( cursor, props ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hasPropertiesShouldSeeNewlyRemovedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HasPropertiesShouldSeeNewlyRemovedProperties()
		 {
			  // Given
			  long node;
			  int prop1, prop2, prop3;
			  using ( Transaction tx = beginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					prop1 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop1");
					prop2 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop2");
					prop3 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop3");
					tx.DataWrite().nodeSetProperty(node, prop1, longValue(1));
					tx.DataWrite().nodeSetProperty(node, prop2, longValue(2));
					tx.DataWrite().nodeSetProperty(node, prop3, longValue(3));
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					using ( NodeCursor cursor = tx.Cursors().allocateNodeCursor(), PropertyCursor props = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(node, cursor);
						 assertTrue( cursor.Next() );

						 assertTrue( HasProperties( cursor, props ) );
						 tx.DataWrite().nodeRemoveProperty(node, prop1);
						 assertTrue( HasProperties( cursor, props ) );
						 tx.DataWrite().nodeRemoveProperty(node, prop2);
						 assertTrue( HasProperties( cursor, props ) );
						 tx.DataWrite().nodeRemoveProperty(node, prop3);
						 assertFalse( HasProperties( cursor, props ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertyTypeShouldBeTxStateAware() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PropertyTypeShouldBeTxStateAware()
		 {
			  // Given
			  long node;
			  using ( Transaction tx = beginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					using ( NodeCursor nodes = tx.Cursors().allocateNodeCursor(), PropertyCursor properties = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(node, nodes);
						 assertTrue( nodes.Next() );
						 assertFalse( HasProperties( nodes, properties ) );
						 int prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
						 tx.DataWrite().nodeSetProperty(node, prop, stringValue("foo"));
						 nodes.Properties( properties );

						 assertTrue( properties.Next() );
						 assertThat( properties.PropertyType(), equalTo(ValueGroup.TEXT) );
					}
			  }
		 }

		 private void AssertLabels( LabelSet labels, params int[] expected )
		 {
			  assertEquals( expected.Length, labels.NumberOfLabels() );
			  Arrays.sort( expected );
			  int[] labelArray = new int[labels.NumberOfLabels()];
			  for ( int i = 0; i < labels.NumberOfLabels(); i++ )
			  {
					labelArray[i] = labels.Label( i );
			  }
			  Arrays.sort( labelArray );
			  assertTrue( "labels match expected", Arrays.Equals( expected, labelArray ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Node createNode(String... labels) throws Exception
		 public virtual Node CreateNode( params string[] labels )
		 {
			  long node;
			  int[] labelIds = new int[labels.Length];
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					node = write.NodeCreate();

					for ( int i = 0; i < labels.Length; i++ )
					{
						 labelIds[i] = tx.TokenWrite().labelGetOrCreateForName(labels[i]);
						 write.NodeAddLabel( node, labelIds[i] );
					}
					tx.Success();
			  }
			  return new Node( node, labelIds );
		 }

		 private class Node
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly long NodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int[] LabelsConflict;

			  internal Node( long node, int[] labels )
			  {
					this.NodeConflict = node;
					this.LabelsConflict = labels;
			  }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public virtual long NodeConflict()
			  {
					return NodeConflict;
			  }

			  public virtual int[] Labels()
			  {
					return LabelsConflict;
			  }
		 }
	}

}