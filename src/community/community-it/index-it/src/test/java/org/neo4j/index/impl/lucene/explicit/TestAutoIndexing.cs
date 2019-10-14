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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using RelationshipIndex = Neo4Net.Graphdb.index.RelationshipIndex;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;

	public class TestAutoIndexing
	{
		 private GraphDatabaseAPI _graphDb;
		 private Transaction _tx;
		 private IDictionary<string, string> _config;

		 private void NewTransaction()
		 {
			  if ( _tx != null )
			  {
					_tx.success();
					_tx.close();
			  }
			  _tx = _graphDb.beginTx();
		 }

		 private IDictionary<string, string> Config
		 {
			 get
			 {
				  if ( _config == null )
				  {
						_config = new Dictionary<string, string>();
				  }
				  return _config;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startDb()
		 public virtual void StartDb()
		 {
			  _graphDb = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(Config).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopDb()
		 public virtual void StopDb()
		 {
			  if ( _tx != null )
			  {
					_tx.close();
			  }
			  if ( _graphDb != null )
			  {
					_graphDb.shutdown();
			  }
			  _tx = null;
			  _config = null;
			  _graphDb = null;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeAutoIndexFromAPISanity()
		 public virtual void TestNodeAutoIndexFromAPISanity()
		 {
			  NewTransaction();
			  AutoIndexer<Node> autoIndexer = _graphDb.index().NodeAutoIndexer;
			  autoIndexer.StartAutoIndexingProperty( "test_uuid" );
			  autoIndexer.Enabled = true;
			  assertEquals( 1, autoIndexer.AutoIndexedProperties.Count );
			  assertTrue( autoIndexer.AutoIndexedProperties.Contains( "test_uuid" ) );

			  Node node1 = _graphDb.createNode();
			  node1.SetProperty( "test_uuid", "node1" );
			  Node node2 = _graphDb.createNode();
			  node2.SetProperty( "test_uuid", "node2" );

			  NewTransaction();

			  assertEquals( node1, autoIndexer.AutoIndex.get( "test_uuid", "node1" ).Single );
			  assertEquals( node2, autoIndexer.AutoIndex.get( "test_uuid", "node2" ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAutoIndexesReportReadOnly()
		 public virtual void TestAutoIndexesReportReadOnly()
		 {
			  NewTransaction();
			  AutoIndexer<Node> autoIndexer = _graphDb.index().NodeAutoIndexer;
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					assertFalse( autoIndexer.AutoIndex.Writeable );
					tx.Success();
			  }
			  autoIndexer.StartAutoIndexingProperty( "test_uuid" );
			  autoIndexer.Enabled = true;
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					assertFalse( autoIndexer.AutoIndex.Writeable );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChangesAreVisibleInTransaction()
		 public virtual void TestChangesAreVisibleInTransaction()
		 {
			  NewTransaction();

			  AutoIndexer<Node> autoIndexer = _graphDb.index().NodeAutoIndexer;
			  autoIndexer.StartAutoIndexingProperty( "nodeProp" );
			  autoIndexer.Enabled = true;

			  Node node1 = _graphDb.createNode();
			  node1.SetProperty( "nodeProp", "nodePropValue" );
			  node1.SetProperty( "nodePropNonIndexable", "valueWhatever" );
			  ReadableIndex<Node> nodeIndex = autoIndexer.AutoIndex;
			  assertEquals( node1, nodeIndex.Get( "nodeProp", "nodePropValue" ).Single );

			  NewTransaction();

			  Node node2 = _graphDb.createNode();
			  node2.SetProperty( "nodeProp", "nodePropValue2" );
			  assertEquals( node2, nodeIndex.Get( "nodeProp", "nodePropValue2" ).Single );
			  node2.SetProperty( "nodeProp", "nodePropValue3" );
			  assertEquals( node2, nodeIndex.Get( "nodeProp", "nodePropValue3" ).Single );
			  node2.RemoveProperty( "nodeProp" );
			  assertFalse( nodeIndex.Get( "nodeProp", "nodePropValue2" ).hasNext() );
			  assertFalse( nodeIndex.Get( "nodeProp", "nodePropValue3" ).hasNext() );

			  NewTransaction();

			  assertEquals( node1, nodeIndex.Get( "nodeProp", "nodePropValue" ).Single );
			  assertFalse( nodeIndex.Get( "nodeProp", "nodePropValue2" ).hasNext() );
			  assertFalse( nodeIndex.Get( "nodeProp", "nodePropValue3" ).hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipAutoIndexFromAPISanity()
		 public virtual void TestRelationshipAutoIndexFromAPISanity()
		 {
			  NewTransaction();
			  const string propNameToIndex = "test";
			  AutoIndexer<Relationship> autoIndexer = _graphDb.index().RelationshipAutoIndexer;
			  autoIndexer.StartAutoIndexingProperty( propNameToIndex );
			  autoIndexer.Enabled = true;

			  Node node1 = _graphDb.createNode();
			  Node node2 = _graphDb.createNode();
			  Node node3 = _graphDb.createNode();

			  Relationship rel12 = node1.CreateRelationshipTo( node2, RelationshipType.withName( "DYNAMIC" ) );
			  Relationship rel23 = node2.CreateRelationshipTo( node3, RelationshipType.withName( "DYNAMIC" ) );

			  rel12.SetProperty( propNameToIndex, "rel12" );
			  rel23.SetProperty( propNameToIndex, "rel23" );

			  NewTransaction();

			  assertEquals( rel12, autoIndexer.AutoIndex.get( propNameToIndex, "rel12" ).Single );
			  assertEquals( rel23, autoIndexer.AutoIndex.get( propNameToIndex, "rel23" ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigAndAPICompatibility()
		 public virtual void TestConfigAndAPICompatibility()
		 {
			  StopDb();
			  _config = new Dictionary<string, string>();
			  _config[GraphDatabaseSettings.node_keys_indexable.name()] = "nodeProp1, nodeProp2";
			  _config[GraphDatabaseSettings.relationship_keys_indexable.name()] = "relProp1, relProp2";
			  _config[GraphDatabaseSettings.node_auto_indexing.name()] = "true";
			  _config[GraphDatabaseSettings.relationship_auto_indexing.name()] = "true";
			  StartDb();

			  NewTransaction();
			  assertTrue( _graphDb.index().NodeAutoIndexer.Enabled );
			  assertTrue( _graphDb.index().RelationshipAutoIndexer.Enabled );

			  AutoIndexer<Node> autoNodeIndexer = _graphDb.index().NodeAutoIndexer;
			  // Start auto indexing a new and an already auto indexed
			  autoNodeIndexer.StartAutoIndexingProperty( "nodeProp1" );
			  autoNodeIndexer.StartAutoIndexingProperty( "nodeProp3" );
			  assertEquals( 3, autoNodeIndexer.AutoIndexedProperties.Count );
			  assertTrue( autoNodeIndexer.AutoIndexedProperties.Contains( "nodeProp1" ) );
			  assertTrue( autoNodeIndexer.AutoIndexedProperties.Contains( "nodeProp2" ) );
			  assertTrue( autoNodeIndexer.AutoIndexedProperties.Contains( "nodeProp3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSmallGraphWithNonIndexableProps()
		 public virtual void TestSmallGraphWithNonIndexableProps()
		 {
			  StopDb();
			  _config = new Dictionary<string, string>();
			  _config[GraphDatabaseSettings.node_keys_indexable.name()] = "nodeProp1, nodeProp2";
			  _config[GraphDatabaseSettings.relationship_keys_indexable.name()] = "relProp1, relProp2";
			  _config[GraphDatabaseSettings.node_auto_indexing.name()] = "true";
			  _config[GraphDatabaseSettings.relationship_auto_indexing.name()] = "true";
			  StartDb();

			  NewTransaction();
			  assertTrue( _graphDb.index().NodeAutoIndexer.Enabled );
			  assertTrue( _graphDb.index().RelationshipAutoIndexer.Enabled );

			  // Build the graph, a 3-cycle
			  Node node1 = _graphDb.createNode();
			  Node node2 = _graphDb.createNode();
			  Node node3 = _graphDb.createNode();

			  Relationship rel12 = node1.CreateRelationshipTo( node2, RelationshipType.withName( "DYNAMIC" ) );
			  Relationship rel23 = node2.CreateRelationshipTo( node3, RelationshipType.withName( "DYNAMIC" ) );
			  Relationship rel31 = node3.CreateRelationshipTo( node1, RelationshipType.withName( "DYNAMIC" ) );

			  // Nodes
			  node1.SetProperty( "nodeProp1", "node1Value1" );
			  node1.SetProperty( "nodePropNonIndexable1", "node1ValueNonIndexable" );

			  node2.SetProperty( "nodeProp2", "node2Value1" );
			  node2.SetProperty( "nodePropNonIndexable2", "node2ValueNonIndexable" );

			  node3.SetProperty( "nodeProp1", "node3Value1" );
			  node3.SetProperty( "nodeProp2", "node3Value2" );
			  node3.SetProperty( "nodePropNonIndexable3", "node3ValueNonIndexable" );

			  // Relationships
			  rel12.SetProperty( "relProp1", "rel12Value1" );
			  rel12.SetProperty( "relPropNonIndexable1", "rel12ValueNonIndexable" );

			  rel23.SetProperty( "relProp2", "rel23Value1" );
			  rel23.SetProperty( "relPropNonIndexable2", "rel23ValueNonIndexable" );

			  rel31.SetProperty( "relProp1", "rel31Value1" );
			  rel31.SetProperty( "relProp2", "rel31Value2" );
			  rel31.SetProperty( "relPropNonIndexable3", "rel31ValueNonIndexable" );

			  NewTransaction();

			  // Committed, time to check
			  AutoIndexer<Node> autoNodeIndexer = _graphDb.index().NodeAutoIndexer;
			  assertEquals( node1, autoNodeIndexer.AutoIndex.get( "nodeProp1", "node1Value1" ).Single );
			  assertEquals( node2, autoNodeIndexer.AutoIndex.get( "nodeProp2", "node2Value1" ).Single );
			  assertEquals( node3, autoNodeIndexer.AutoIndex.get( "nodeProp1", "node3Value1" ).Single );
			  assertEquals( node3, autoNodeIndexer.AutoIndex.get( "nodeProp2", "node3Value2" ).Single );
			  assertFalse( autoNodeIndexer.AutoIndex.get( "nodePropNonIndexable1", "node1ValueNonIndexable" ).hasNext() );
			  assertFalse( autoNodeIndexer.AutoIndex.get( "nodePropNonIndexable2", "node2ValueNonIndexable" ).hasNext() );
			  assertFalse( autoNodeIndexer.AutoIndex.get( "nodePropNonIndexable3", "node3ValueNonIndexable" ).hasNext() );

			  AutoIndexer<Relationship> autoRelIndexer = _graphDb.index().RelationshipAutoIndexer;
			  assertEquals( rel12, autoRelIndexer.AutoIndex.get( "relProp1", "rel12Value1" ).Single );
			  assertEquals( rel23, autoRelIndexer.AutoIndex.get( "relProp2", "rel23Value1" ).Single );
			  assertEquals( rel31, autoRelIndexer.AutoIndex.get( "relProp1", "rel31Value1" ).Single );
			  assertEquals( rel31, autoRelIndexer.AutoIndex.get( "relProp2", "rel31Value2" ).Single );
			  assertFalse( autoRelIndexer.AutoIndex.get( "relPropNonIndexable1", "rel12ValueNonIndexable" ).hasNext() );
			  assertFalse( autoRelIndexer.AutoIndex.get( "relPropNonIndexable2", "rel23ValueNonIndexable" ).hasNext() );
			  assertFalse( autoRelIndexer.AutoIndex.get( "relPropNonIndexable3", "rel31ValueNonIndexable" ).hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultIsOff()
		 public virtual void TestDefaultIsOff()
		 {
			  NewTransaction();
			  Node node1 = _graphDb.createNode();
			  node1.SetProperty( "testProp", "node1" );

			  NewTransaction();
			  AutoIndexer<Node> autoIndexer = _graphDb.index().NodeAutoIndexer;
			  assertFalse( autoIndexer.AutoIndex.get( "testProp", "node1" ).hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultIfOffIsForEverything()
		 public virtual void TestDefaultIfOffIsForEverything()
		 {
			  NewTransaction();
			  _graphDb.index().NodeAutoIndexer.Enabled = true;
			  Node node1 = _graphDb.createNode();
			  node1.SetProperty( "testProp", "node1" );
			  node1.SetProperty( "testProp1", "node1" );
			  Node node2 = _graphDb.createNode();
			  node2.SetProperty( "testProp", "node2" );
			  node2.SetProperty( "testProp1", "node2" );

			  NewTransaction();
			  AutoIndexer<Node> autoIndexer = _graphDb.index().NodeAutoIndexer;
			  assertFalse( autoIndexer.AutoIndex.get( "testProp", "node1" ).hasNext() );
			  assertFalse( autoIndexer.AutoIndex.get( "testProp1", "node1" ).hasNext() );
			  assertFalse( autoIndexer.AutoIndex.get( "testProp", "node2" ).hasNext() );
			  assertFalse( autoIndexer.AutoIndex.get( "testProp1", "node2" ).hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultIsOffIfExplicit()
		 public virtual void TestDefaultIsOffIfExplicit()
		 {
			  StopDb();
			  _config = new Dictionary<string, string>();
			  _config[GraphDatabaseSettings.node_keys_indexable.name()] = "nodeProp1, nodeProp2";
			  _config[GraphDatabaseSettings.relationship_keys_indexable.name()] = "relProp1, relProp2";
			  _config[GraphDatabaseSettings.node_auto_indexing.name()] = "false";
			  _config[GraphDatabaseSettings.relationship_auto_indexing.name()] = "false";
			  StartDb();

			  NewTransaction();
			  AutoIndexer<Node> autoIndexer = _graphDb.index().NodeAutoIndexer;
			  autoIndexer.StartAutoIndexingProperty( "testProp" );

			  Node node1 = _graphDb.createNode();
			  node1.SetProperty( "nodeProp1", "node1" );
			  node1.SetProperty( "nodeProp2", "node1" );
			  node1.SetProperty( "testProp", "node1" );

			  NewTransaction();

			  assertFalse( autoIndexer.AutoIndex.get( "nodeProp1", "node1" ).hasNext() );
			  assertFalse( autoIndexer.AutoIndex.get( "nodeProp2", "node1" ).hasNext() );
			  assertFalse( autoIndexer.AutoIndex.get( "testProp", "node1" ).hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDefaultsAreSeparateForNodesAndRelationships()
		 public virtual void TestDefaultsAreSeparateForNodesAndRelationships()
		 {
			  StopDb();
			  _config = new Dictionary<string, string>();
			  _config[GraphDatabaseSettings.node_keys_indexable.name()] = "propName";
			  _config[GraphDatabaseSettings.node_auto_indexing.name()] = "true";
			  // Now only node properties named propName should be indexed.
			  StartDb();

			  NewTransaction();

			  Node node1 = _graphDb.createNode();
			  Node node2 = _graphDb.createNode();
			  node1.SetProperty( "propName", "node1" );
			  node2.SetProperty( "propName", "node2" );
			  node2.SetProperty( "propName_", "node2" );

			  Relationship rel = node1.CreateRelationshipTo( node2, RelationshipType.withName( "DYNAMIC" ) );
			  rel.SetProperty( "propName", "rel1" );

			  NewTransaction();

			  ReadableIndex<Node> autoIndex = _graphDb.index().NodeAutoIndexer.AutoIndex;
			  assertEquals( node1, autoIndex.Get( "propName", "node1" ).Single );
			  assertEquals( node2, autoIndex.Get( "propName", "node2" ).Single );
			  assertFalse( _graphDb.index().RelationshipAutoIndexer.AutoIndex.get("propName", "rel1").hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStartStopAutoIndexing()
		 public virtual void TestStartStopAutoIndexing()
		 {
			  StopDb();
			  _config = new Dictionary<string, string>();
			  _config[GraphDatabaseSettings.node_keys_indexable.name()] = "propName";
			  _config[GraphDatabaseSettings.node_auto_indexing.name()] = "true";
			  // Now only node properties named propName should be indexed.
			  StartDb();

			  NewTransaction();
			  AutoIndexer<Node> autoIndexer = _graphDb.index().NodeAutoIndexer;
			  assertTrue( autoIndexer.Enabled );

			  autoIndexer.Enabled = false;
			  assertFalse( autoIndexer.Enabled );

			  Node node1 = _graphDb.createNode();
			  Node node2 = _graphDb.createNode();
			  node1.SetProperty( "propName", "node" );
			  NewTransaction();

			  assertFalse( autoIndexer.AutoIndex.get( "nodeProp1", "node1" ).hasNext() );
			  autoIndexer.Enabled = true;
			  node2.SetProperty( "propName", "node" );

			  NewTransaction();

			  assertEquals( node2, autoIndexer.AutoIndex.get( "propName", "node" ).Single );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStopMonitoringProperty()
		 public virtual void TestStopMonitoringProperty()
		 {
			  NewTransaction();
			  AutoIndexer<Node> autoIndexer = _graphDb.index().NodeAutoIndexer;
			  autoIndexer.Enabled = true;
			  autoIndexer.StartAutoIndexingProperty( "propName" );
			  Node node1 = _graphDb.createNode();
			  Node node2 = _graphDb.createNode();
			  node1.SetProperty( "propName", "node" );
			  NewTransaction();
			  assertEquals( node1, autoIndexer.AutoIndex.get( "propName", "node" ).Single );
			  NewTransaction();
			  // Setting just another property to autoindex
			  autoIndexer.StartAutoIndexingProperty( "propName2" );
			  autoIndexer.StopAutoIndexingProperty( "propName" );
			  node2.SetProperty( "propName", "propValue" );
			  Node node3 = _graphDb.createNode();
			  node3.SetProperty( "propName2", "propValue" );
			  NewTransaction();
			  // Now node2 must be not there, node3 must be there and node1 should not have been touched
			  assertEquals( node1, autoIndexer.AutoIndex.get( "propName", "node" ).Single );
			  assertEquals( node3, autoIndexer.AutoIndex.get( "propName2", "propValue" ).Single );
			  // Now, since only propName2 is autoindexed, every other should be
			  // removed when touched, such as node1's propName
			  node1.SetProperty( "propName", "newValue" );
			  NewTransaction();
			  assertFalse( autoIndexer.AutoIndex.get( "propName", "newValue" ).hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGettingAutoIndexByNameReturnsSomethingReadOnly()
		 public virtual void TestGettingAutoIndexByNameReturnsSomethingReadOnly()
		 {
			  // Create the node and relationship auto-indexes
			  NewTransaction();
			  _graphDb.index().NodeAutoIndexer.Enabled = true;
			  _graphDb.index().NodeAutoIndexer.startAutoIndexingProperty("nodeProp");
			  _graphDb.index().RelationshipAutoIndexer.Enabled = true;
			  _graphDb.index().RelationshipAutoIndexer.startAutoIndexingProperty("relProp");

			  Node node1 = _graphDb.createNode();
			  Node node2 = _graphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, RelationshipType.withName( "FOO" ) );
			  node1.SetProperty( "nodeProp", "nodePropValue" );
			  rel.SetProperty( "relProp", "relPropValue" );

			  NewTransaction();

			  assertEquals( 1, _graphDb.index().nodeIndexNames().length );
			  assertEquals( 1, _graphDb.index().relationshipIndexNames().length );

			  assertEquals( "node_auto_index", _graphDb.index().nodeIndexNames()[0] );
			  assertEquals( "relationship_auto_index", _graphDb.index().relationshipIndexNames()[0] );

			  Index<Node> nodeIndex = _graphDb.index().forNodes("node_auto_index");
			  RelationshipIndex relIndex = _graphDb.index().forRelationships("relationship_auto_index");
			  assertEquals( node1, nodeIndex.get( "nodeProp", "nodePropValue" ).Single );
			  assertEquals( rel, relIndex.get( "relProp", "relPropValue" ).Single );
			  try
			  {
					nodeIndex.Add( null, null, null );
					fail( "Auto indexes should not allow external manipulation" );
			  }
			  catch ( System.NotSupportedException )
			  { // good
			  }

			  try
			  {
					relIndex.add( null, null, null );
					fail( "Auto indexes should not allow external manipulation" );
			  }
			  catch ( System.NotSupportedException )
			  { // good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveUnloadedHeavyProperty()
		 public virtual void TestRemoveUnloadedHeavyProperty()
		 {
			  /*
			   * Checks a bug where removing non-cached heavy properties
			   * would cause NPE in auto indexer.
			   */
			  NewTransaction();
			  _graphDb.index().NodeAutoIndexer.Enabled = true;
			  _graphDb.index().NodeAutoIndexer.startAutoIndexingProperty("nodeProp");

			  Node node1 = _graphDb.createNode();
			  // Large array, needed for making sure this is a heavy property
			  node1.SetProperty( "nodeProp", new int[] { -1, 2, 3, 4, 5, 6, 1, 1, 1, 1 } );

			  NewTransaction();

			  // clear the caches
			  NeoStoreDataSource dataSource = _graphDb.DependencyResolver.resolveDependency( typeof( NeoStoreDataSource ) );

			  node1.RemoveProperty( "nodeProp" );
			  NewTransaction();
			  assertFalse( node1.HasProperty( "nodeProp" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveRelationshipRemovesDocument()
		 public virtual void TestRemoveRelationshipRemovesDocument()
		 {
			  NewTransaction();
			  AutoIndexer<Relationship> autoIndexer = _graphDb.index().RelationshipAutoIndexer;
			  autoIndexer.StartAutoIndexingProperty( "foo" );
			  autoIndexer.Enabled = true;

			  Node node1 = _graphDb.createNode();
			  Node node2 = _graphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, RelationshipType.withName( "foo" ) );
			  rel.SetProperty( "foo", "bar" );

			  NewTransaction();

			  using ( IndexHits<Relationship> relationshipIndexHits = _graphDb.index().forRelationships("relationship_auto_index").query("_id_:*") )
			  {
					assertThat( relationshipIndexHits.Size(), equalTo(1) );
			  }

			  NewTransaction();

			  rel.Delete();

			  NewTransaction();

			  using ( IndexHits<Relationship> relationshipIndexHits = _graphDb.index().forRelationships("relationship_auto_index").query("_id_:*") )
			  {
					assertThat( relationshipIndexHits.Size(), equalTo(0) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeletingNodeRemovesItFromAutoIndex()
		 public virtual void TestDeletingNodeRemovesItFromAutoIndex()
		 {
			  NewTransaction();
			  AutoIndexer<Node> nodeAutoIndexer = _graphDb.index().NodeAutoIndexer;
			  nodeAutoIndexer.StartAutoIndexingProperty( "foo" );
			  nodeAutoIndexer.Enabled = true;

			  Node node1 = _graphDb.createNode();
			  node1.SetProperty( "foo", "bar" );

			  NewTransaction();

			  using ( IndexHits<Node> nodeIndexHits = _graphDb.index().forNodes("node_auto_index").query("_id_:*") )
			  {
					assertThat( nodeIndexHits.Size(), equalTo(1) );
			  }

			  node1.Delete();

			  NewTransaction();

			  using ( IndexHits<Node> nodeIndexHits = _graphDb.index().forNodes("node_auto_index").query("_id_:*") )
			  {
					assertThat( nodeIndexHits.Size(), equalTo(0) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyDeleteAffectedKeyWhenRemovingPropertyFromNode()
		 public virtual void ShouldOnlyDeleteAffectedKeyWhenRemovingPropertyFromNode()
		 {
			  // GIVEN a node with two auto-indexed properties
			  string key1 = "foo";
			  string key2 = "bar";
			  string value1 = "bip";
			  string value2 = "bop";
			  NewTransaction();
			  AutoIndexer<Node> nodeAutoIndexer = _graphDb.index().NodeAutoIndexer;
			  nodeAutoIndexer.StartAutoIndexingProperty( key1 );
			  nodeAutoIndexer.StartAutoIndexingProperty( key2 );
			  nodeAutoIndexer.Enabled = true;
			  Node node = _graphDb.createNode();
			  node.SetProperty( key1, value1 );
			  node.SetProperty( key2, value2 );
			  NewTransaction();

			  // WHEN removing one of them
			  node.RemoveProperty( key1 );
			  NewTransaction();

			  // THEN the other one should still be in the index
			  assertEquals( 0, count( nodeAutoIndexer.AutoIndex.get( key1, value1 ) ) );
			  assertEquals( 1, count( nodeAutoIndexer.AutoIndex.get( key2, value2 ) ) );
		 }
	}

}