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
namespace Neo4Net.Server.rest.web
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ConstraintDefinition = Neo4Net.Graphdb.schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.Graphdb.schema.ConstraintType;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using Neo4Net.Helpers.Collections;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Database = Neo4Net.Server.database.Database;
	using WrappedDatabase = Neo4Net.Server.database.WrappedDatabase;
	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using EndNodeNotFoundException = Neo4Net.Server.rest.domain.EndNodeNotFoundException;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using StartNodeNotFoundException = Neo4Net.Server.rest.domain.StartNodeNotFoundException;
	using IndexedEntityRepresentation = Neo4Net.Server.rest.repr.IndexedEntityRepresentation;
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using NodeRepresentation = Neo4Net.Server.rest.repr.NodeRepresentation;
	using NodeRepresentationTest = Neo4Net.Server.rest.repr.NodeRepresentationTest;
	using RelationshipRepresentation = Neo4Net.Server.rest.repr.RelationshipRepresentation;
	using RelationshipRepresentationTest = Neo4Net.Server.rest.repr.RelationshipRepresentationTest;
	using RelationshipDirection = Neo4Net.Server.rest.web.DatabaseActions.RelationshipDirection;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.firstOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestAccess.nodeUriToId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestAccess.serialize;

	public class DatabaseActionsTest
	{
		 private static readonly Label _label = label( "Label" );
		 private static GraphDbHelper _graphdbHelper;
		 private static Database _database;
		 private static GraphDatabaseFacade _graph;
		 private static DatabaseActions _actions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void createDb()
		 public static void CreateDb()
		 {
			  _graph = ( GraphDatabaseFacade ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.record_id_batch_size, "1").newGraphDatabase();
			  _database = new WrappedDatabase( _graph );
			  _graphdbHelper = new GraphDbHelper( _database );
			  _actions = new TransactionWrappedDatabaseActions( _database.Graph );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void shutdownDatabase()
		 public static void ShutdownDatabase()
		 {
			  _graph.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void clearDb()
		 public virtual void ClearDb()
		 {
			  ServerHelper.cleanTheDatabase( _graph );
		 }

		 private long CreateNode( IDictionary<string, object> properties )
		 {

			  long nodeId;
			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Node node = _database.Graph.createNode( _label );
					foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( entry.Key, entry.Value );
					}
					nodeId = node.Id;
					tx.Success();
			  }
			  return nodeId;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdNodeShouldBeInDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatedNodeShouldBeInDatabase()
		 {
			  NodeRepresentation noderep = _actions.createNode( Collections.emptyMap() );

			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					assertNotNull( _database.Graph.getNodeById( noderep.Id ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeInDatabaseShouldBeRetrievable() throws NodeNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NodeInDatabaseShouldBeRetrievable()
		 {
			  long nodeId = ( new GraphDbHelper( _database ) ).createNode();
			  assertNotNull( _actions.getNode( nodeId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToStorePropertiesInAnExistingNode() throws PropertyValueException, NodeNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToStorePropertiesInAnExistingNode()
		 {
			  long nodeId = _graphdbHelper.createNode();
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["baz"] = 17;
			  _actions.setAllNodeProperties( nodeId, properties );

			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Node node = _database.Graph.getNodeById( nodeId );
					AssertHasProperties( node, properties );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = PropertyValueException.class) public void shouldFailOnTryingToStoreMixedArraysAsAProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnTryingToStoreMixedArraysAsAProperty()
		 {
			  long nodeId = _graphdbHelper.createNode();
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  object[] dodgyArray = new object[3];
			  dodgyArray[0] = 0;
			  dodgyArray[1] = 1;
			  dodgyArray[2] = "two";
			  properties["foo"] = dodgyArray;

			  _actions.setAllNodeProperties( nodeId, properties );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverwriteExistingProperties() throws PropertyValueException, NodeNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOverwriteExistingProperties()
		 {

			  long nodeId;
			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Node node = _database.Graph.createNode();
					node.SetProperty( "remove me", "trash" );
					nodeId = node.Id;
					tx.Success();
			  }

			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["baz"] = 17;
			  _actions.setAllNodeProperties( nodeId, properties );
			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Node node = _database.Graph.getNodeById( nodeId );
					AssertHasProperties( node, properties );
					assertNull( node.GetProperty( "remove me", null ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetPropertiesOnNode() throws NodeNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetPropertiesOnNode()
		 {

			  long nodeId;
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["neo"] = "Thomas A. Anderson";
			  properties["number"] = 15L;
			  Node node;
			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					node = _database.Graph.createNode();
					foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( entry.Key, entry.Value );
					}
					nodeId = node.Id;
					tx.Success();
			  }

			  using ( Transaction transaction = _graph.beginTx() )
			  {
					assertEquals( properties, serialize( _actions.getAllNodeProperties( nodeId ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveNodeWithNoRelationsFromDBOnDelete() throws NodeNotFoundException, org.neo4j.graphdb.ConstraintViolationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveNodeWithNoRelationsFromDBOnDelete()
		 {
			  long nodeId;
			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Node node = _database.Graph.createNode();
					nodeId = node.Id;
					tx.Success();
			  }

			  int nodeCount = _graphdbHelper.NumberOfNodes;
			  _actions.deleteNode( nodeId );
			  assertEquals( nodeCount - 1, _graphdbHelper.NumberOfNodes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSetPropertyOnNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSetPropertyOnNode()
		 {
			  long nodeId = CreateNode( Collections.emptyMap() );
			  string key = "foo";
			  object value = "bar";
			  _actions.setNodeProperty( nodeId, key, value );
			  assertEquals( Collections.singletonMap( key, value ), _graphdbHelper.getNodeProperties( nodeId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void settingAnEmptyArrayShouldWorkIfOriginalEntityHasAnEmptyArrayAsWell() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SettingAnEmptyArrayShouldWorkIfOriginalEntityHasAnEmptyArrayAsWell()
		 {
			  // Given
			  long nodeId = CreateNode( map( "emptyArray", new int[]{} ) );

			  // When
			  _actions.setNodeProperty( nodeId, "emptyArray", new List<>() );

			  // Then
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					assertThat( ( ( IList<object> ) serialize( _actions.getNodeProperty( nodeId, "emptyArray" ) ) ).Count, @is( 0 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetPropertyOnNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetPropertyOnNode()
		 {
			  string key = "foo";
			  object value = "bar";
			  long nodeId = CreateNode( Collections.singletonMap( key, value ) );
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					assertEquals( value, serialize( _actions.getNodeProperty( nodeId, key ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveNodeProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRemoveNodeProperties()
		 {
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  long nodeId = CreateNode( properties );
			  _actions.removeAllNodeProperties( nodeId );

			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Node node = _database.Graph.getNodeById( nodeId );
					assertFalse( node.PropertyKeys.GetEnumerator().hasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreRelationshipsBetweenTwoExistingNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreRelationshipsBetweenTwoExistingNodes()
		 {
			  int relationshipCount = _graphdbHelper.NumberOfRelationships;
			  _actions.createRelationship( _graphdbHelper.createNode(), _graphdbHelper.createNode(), "LOVES", Collections.emptyMap() );
			  assertEquals( relationshipCount + 1, _graphdbHelper.NumberOfRelationships );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreSuppliedPropertiesWhenCreatingRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreSuppliedPropertiesWhenCreatingRelationship()
		 {
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["string"] = "value";
			  properties["integer"] = 17;
			  long relId = _actions.createRelationship( _graphdbHelper.createNode(), _graphdbHelper.createNode(), "LOVES", properties ).Id;

			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Relationship rel = _database.Graph.getRelationshipById( relId );
					foreach ( string key in rel.PropertyKeys )
					{
						 assertTrue( "extra property stored", properties.ContainsKey( key ) );
					}
					foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
					{
						 assertEquals( entry.Value, rel.GetProperty( entry.Key ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateRelationshipBetweenNonExistentNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCreateRelationshipBetweenNonExistentNodes()
		 {
			  long nodeId = _graphdbHelper.createNode();
			  IDictionary<string, object> properties = Collections.emptyMap();
			  try
			  {
					_actions.createRelationship( nodeId, nodeId * 1000, "Loves", properties );
					fail();
			  }
			  catch ( EndNodeNotFoundException )
			  {
					// ok
			  }
			  try
			  {
					_actions.createRelationship( nodeId * 1000, nodeId, "Loves", properties );
					fail();
			  }
			  catch ( StartNodeNotFoundException )
			  {
					// ok
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowCreateRelationshipWithSameStartAsEndNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowCreateRelationshipWithSameStartAsEndNode()
		 {
			  long nodeId = _graphdbHelper.createNode();
			  IDictionary<string, object> properties = Collections.emptyMap();
			  RelationshipRepresentation rel = _actions.createRelationship( nodeId, nodeId, "Loves", properties );
			  assertNotNull( rel );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveNodeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRemoveNodeProperty()
		 {
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  long nodeId = CreateNode( properties );
			  _actions.removeNodeProperty( nodeId, "foo" );

			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Node node = _database.Graph.getNodeById( nodeId );
					assertEquals( 15, node.GetProperty( "number" ) );
					assertFalse( node.HasProperty( "foo" ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTrueIfNodePropertyRemoved() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTrueIfNodePropertyRemoved()
		 {
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  long nodeId = CreateNode( properties );
			  _actions.removeNodeProperty( nodeId, "foo" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = NoSuchPropertyException.class) public void shouldReturnFalseIfNodePropertyNotRemoved() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseIfNodePropertyNotRemoved()
		 {
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  long nodeId = CreateNode( properties );
			  _actions.removeNodeProperty( nodeId, "baz" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRetrieveARelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRetrieveARelationship()
		 {
			  long relationship = _graphdbHelper.createRelationship( "ENJOYED" );
			  assertNotNull( _actions.getRelationship( relationship ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetPropertiesOnRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetPropertiesOnRelationship()
		 {

			  long relationshipId;
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["neo"] = "Thomas A. Anderson";
			  properties["number"] = 15L;
			  using ( Transaction tx = _database.Graph.beginTx() )
			  {
					Node startNode = _database.Graph.createNode();
					Node endNode = _database.Graph.createNode();
					Relationship relationship = startNode.CreateRelationshipTo( endNode, RelationshipType.withName( "knows" ) );
					foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
					{
						 relationship.SetProperty( entry.Key, entry.Value );
					}
					relationshipId = relationship.Id;
					tx.Success();
			  }

			  using ( Transaction transaction = _graph.beginTx() )
			  {
					assertEquals( properties, serialize( _actions.getAllRelationshipProperties( relationshipId ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRetrieveASinglePropertyFromARelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRetrieveASinglePropertyFromARelationship()
		 {
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["neo"] = "Thomas A. Anderson";
			  properties["number"] = 15L;

			  long relationshipId = _graphdbHelper.createRelationship( "LOVES" );
			  _graphdbHelper.setRelationshipProperties( relationshipId, properties );

			  object relationshipProperty;
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					relationshipProperty = serialize( _actions.getRelationshipProperty( relationshipId, "foo" ) );
			  }
			  assertEquals( "bar", relationshipProperty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDeleteARelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDeleteARelationship()
		 {
			  long relationshipId = _graphdbHelper.createRelationship( "LOVES" );

			  _actions.deleteRelationship( relationshipId );
			  try
			  {
					_graphdbHelper.getRelationship( relationshipId );
					fail();
			  }
			  catch ( NotFoundException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRetrieveRelationshipsFromNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRetrieveRelationshipsFromNode()
		 {
			  long nodeId = _graphdbHelper.createNode();
			  _graphdbHelper.createRelationship( "LIKES", nodeId, _graphdbHelper.createNode() );
			  _graphdbHelper.createRelationship( "LIKES", _graphdbHelper.createNode(), nodeId );
			  _graphdbHelper.createRelationship( "HATES", nodeId, _graphdbHelper.createNode() );

			  using ( Transaction transaction = _graph.beginTx() )
			  {
					VerifyRelReps( 3, _actions.getNodeRelationships( nodeId, RelationshipDirection.all, Collections.emptyList() ) );
					VerifyRelReps( 1, _actions.getNodeRelationships( nodeId, RelationshipDirection.@in, Collections.emptyList() ) );
					VerifyRelReps( 2, _actions.getNodeRelationships( nodeId, RelationshipDirection.@out, Collections.emptyList() ) );

					VerifyRelReps( 3, _actions.getNodeRelationships( nodeId, RelationshipDirection.all, Arrays.asList( "LIKES", "HATES" ) ) );
					VerifyRelReps( 1, _actions.getNodeRelationships( nodeId, RelationshipDirection.@in, Arrays.asList( "LIKES", "HATES" ) ) );
					VerifyRelReps( 2, _actions.getNodeRelationships( nodeId, RelationshipDirection.@out, Arrays.asList( "LIKES", "HATES" ) ) );

					VerifyRelReps( 2, _actions.getNodeRelationships( nodeId, RelationshipDirection.all, Arrays.asList( "LIKES" ) ) );
					VerifyRelReps( 1, _actions.getNodeRelationships( nodeId, RelationshipDirection.@in, Arrays.asList( "LIKES" ) ) );
					VerifyRelReps( 1, _actions.getNodeRelationships( nodeId, RelationshipDirection.@out, Arrays.asList( "LIKES" ) ) );

					VerifyRelReps( 1, _actions.getNodeRelationships( nodeId, RelationshipDirection.all, Arrays.asList( "HATES" ) ) );
					VerifyRelReps( 0, _actions.getNodeRelationships( nodeId, RelationshipDirection.@in, Arrays.asList( "HATES" ) ) );
					VerifyRelReps( 1, _actions.getNodeRelationships( nodeId, RelationshipDirection.@out, Arrays.asList( "HATES" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetAnyRelationshipsWhenRetrievingFromNodeWithoutRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotGetAnyRelationshipsWhenRetrievingFromNodeWithoutRelationships()
		 {
			  long nodeId = _graphdbHelper.createNode();

			  using ( Transaction transaction = _graph.beginTx() )
			  {
					VerifyRelReps( 0, _actions.getNodeRelationships( nodeId, RelationshipDirection.all, Collections.emptyList() ) );
					VerifyRelReps( 0, _actions.getNodeRelationships( nodeId, RelationshipDirection.@in, Collections.emptyList() ) );
					VerifyRelReps( 0, _actions.getNodeRelationships( nodeId, RelationshipDirection.@out, Collections.emptyList() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSetRelationshipProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSetRelationshipProperties()
		 {
			  long relationshipId = _graphdbHelper.createRelationship( "KNOWS" );
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 10;
			  _actions.setAllRelationshipProperties( relationshipId, properties );
			  assertEquals( properties, _graphdbHelper.getRelationshipProperties( relationshipId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSetRelationshipProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSetRelationshipProperty()
		 {
			  long relationshipId = _graphdbHelper.createRelationship( "KNOWS" );
			  string key = "foo";
			  object value = "bar";
			  _actions.setRelationshipProperty( relationshipId, key, value );
			  assertEquals( Collections.singletonMap( key, value ), _graphdbHelper.getRelationshipProperties( relationshipId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveRelationProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveRelationProperties()
		 {
			  long relId = _graphdbHelper.createRelationship( "PAIR-PROGRAMS_WITH" );
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["foo"] = "bar";
			  map["baz"] = 22;
			  _graphdbHelper.setRelationshipProperties( relId, map );

			  _actions.removeAllRelationshipProperties( relId );

			  assertTrue( _graphdbHelper.getRelationshipProperties( relId ).Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveRelationshipProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveRelationshipProperty()
		 {
			  long relId = _graphdbHelper.createRelationship( "PAIR-PROGRAMS_WITH" );
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["foo"] = "bar";
			  map["baz"] = 22;
			  _graphdbHelper.setRelationshipProperties( relId, map );

			  _actions.removeRelationshipProperty( relId, "foo" );
			  assertEquals( 1, _graphdbHelper.getRelationshipProperties( relId ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private void verifyRelReps(int expectedSize, org.neo4j.server.rest.repr.ListRepresentation repr)
		 private void VerifyRelReps( int expectedSize, ListRepresentation repr )
		 {
			  IList<object> relreps = serialize( repr );
			  assertEquals( expectedSize, relreps.Count );
			  foreach ( object relrep in relreps )
			  {
					RelationshipRepresentationTest.verifySerialisation( ( IDictionary<string, object> ) relrep );
			  }
		 }

		 private void AssertHasProperties( PropertyContainer container, IDictionary<string, object> properties )
		 {
			  foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
			  {
					assertEquals( entry.Value, container.GetProperty( entry.Key ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToIndexNode()
		 public virtual void ShouldBeAbleToIndexNode()
		 {
			  string key = "mykey";
			  string value = "myvalue";
			  long nodeId = _graphdbHelper.createNode();
			  string indexName = "node";

			  _actions.createNodeIndex( MapUtil.map( "name", indexName ) );

			  IList<object> listOfIndexedNodes;
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					listOfIndexedNodes = serialize( _actions.getIndexedNodes( indexName, key, value ) );
			  }
			  assertFalse( listOfIndexedNodes.GetEnumerator().hasNext() );
			  _actions.addToNodeIndex( indexName, key, value, nodeId );
			  assertEquals( Arrays.asList( nodeId ), _graphdbHelper.getIndexedNodes( indexName, key, value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToFulltextIndex()
		 public virtual void ShouldBeAbleToFulltextIndex()
		 {
			  string key = "key";
			  string value = "the value with spaces";
			  long nodeId = _graphdbHelper.createNode();
			  string indexName = "fulltext-node";
			  _graphdbHelper.createNodeFullTextIndex( indexName );
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					assertFalse( serialize( _actions.getIndexedNodes( indexName, key, value ) ).GetEnumerator().hasNext() );
			  }
			  _actions.addToNodeIndex( indexName, key, value, nodeId );
			  assertEquals( Arrays.asList( nodeId ), _graphdbHelper.getIndexedNodes( indexName, key, value ) );
			  assertEquals( Arrays.asList( nodeId ), _graphdbHelper.getIndexedNodes( indexName, key, "the value with spaces" ) );
			  assertEquals( Arrays.asList( nodeId ), _graphdbHelper.queryIndexedNodes( indexName, key, "the" ) );
			  assertEquals( Arrays.asList( nodeId ), _graphdbHelper.queryIndexedNodes( indexName, key, "value" ) );
			  assertEquals( Arrays.asList( nodeId ), _graphdbHelper.queryIndexedNodes( indexName, key, "with" ) );
			  assertEquals( Arrays.asList( nodeId ), _graphdbHelper.queryIndexedNodes( indexName, key, "spaces" ) );
			  assertEquals( Arrays.asList( nodeId ), _graphdbHelper.queryIndexedNodes( indexName, key, "*spaces*" ) );
			  assertTrue( _graphdbHelper.getIndexedNodes( indexName, key, "nohit" ).Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetExtendedNodeRepresentationsWhenGettingFromIndex()
		 public virtual void ShouldGetExtendedNodeRepresentationsWhenGettingFromIndex()
		 {
			  string key = "mykey3";
			  string value = "value";

			  long nodeId = _graphdbHelper.createNode( _label );
			  string indexName = "node";
			  _graphdbHelper.addNodeToIndex( indexName, key, value, nodeId );
			  int counter = 0;

			  IList<object> indexedNodes;
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					indexedNodes = serialize( _actions.getIndexedNodes( indexName, key, value ) );
			  }

			  foreach ( object indexedNode in indexedNodes )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String, Object> serialized = (java.util.Map<String, Object>) indexedNode;
					IDictionary<string, object> serialized = ( IDictionary<string, object> ) indexedNode;
					NodeRepresentationTest.verifySerialisation( serialized );
					assertNotNull( serialized["indexed"] );
					counter++;
			  }
			  assertEquals( 1, counter );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveNodeFromIndex()
		 public virtual void ShouldBeAbleToRemoveNodeFromIndex()
		 {
			  string key = "mykey2";
			  string value = "myvalue";
			  string value2 = "myvalue2";
			  string indexName = "node";
			  long nodeId = _graphdbHelper.createNode();
			  _actions.addToNodeIndex( indexName, key, value, nodeId );
			  _actions.addToNodeIndex( indexName, key, value2, nodeId );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key, value ).Count );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key, value2 ).Count );
			  _actions.removeFromNodeIndex( indexName, key, value, nodeId );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key, value ).Count );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key, value2 ).Count );
			  _actions.removeFromNodeIndex( indexName, key, value2, nodeId );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key, value ).Count );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key, value2 ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveNodeFromIndexWithoutKeyValue()
		 public virtual void ShouldBeAbleToRemoveNodeFromIndexWithoutKeyValue()
		 {
			  string key1 = "kvkey1";
			  string key2 = "kvkey2";
			  string value = "myvalue";
			  string value2 = "myvalue2";
			  string indexName = "node";
			  long nodeId = _graphdbHelper.createNode();
			  _actions.addToNodeIndex( indexName, key1, value, nodeId );
			  _actions.addToNodeIndex( indexName, key1, value2, nodeId );
			  _actions.addToNodeIndex( indexName, key2, value, nodeId );
			  _actions.addToNodeIndex( indexName, key2, value2, nodeId );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key1, value ).Count );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key1, value2 ).Count );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key2, value ).Count );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key2, value2 ).Count );
			  _actions.removeFromNodeIndexNoValue( indexName, key1, nodeId );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key1, value ).Count );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key1, value2 ).Count );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key2, value ).Count );
			  assertEquals( 1, _graphdbHelper.getIndexedNodes( indexName, key2, value2 ).Count );
			  _actions.removeFromNodeIndexNoKeyValue( indexName, nodeId );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key1, value ).Count );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key1, value2 ).Count );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key2, value ).Count );
			  assertEquals( 0, _graphdbHelper.getIndexedNodes( indexName, key2, value2 ).Count );
		 }

		 private long[] CreateMoreComplexGraph()
		 {
			  // (a)
			  // / \
			  // v v
			  // (b)<---(c) (d)-->(e)
			  // \ / \ / /
			  // v v v v /
			  // (f)--->(g)<----

			  long a = _graphdbHelper.createNode();
			  long b = _graphdbHelper.createNode();
			  long c = _graphdbHelper.createNode();
			  long d = _graphdbHelper.createNode();
			  long e = _graphdbHelper.createNode();
			  long f = _graphdbHelper.createNode();
			  long g = _graphdbHelper.createNode();
			  _graphdbHelper.createRelationship( "to", a, c );
			  _graphdbHelper.createRelationship( "to", a, d );
			  _graphdbHelper.createRelationship( "to", c, b );
			  _graphdbHelper.createRelationship( "to", d, e );
			  _graphdbHelper.createRelationship( "to", b, f );
			  _graphdbHelper.createRelationship( "to", c, f );
			  _graphdbHelper.createRelationship( "to", f, g );
			  _graphdbHelper.createRelationship( "to", d, g );
			  _graphdbHelper.createRelationship( "to", e, g );
			  _graphdbHelper.createRelationship( "to", c, g );
			  return new long[]{ a, g };
		 }

		 private void CreateRelationshipWithProperties( long start, long end, IDictionary<string, object> properties )
		 {
			  long rel = _graphdbHelper.createRelationship( "to", start, end );
			  _graphdbHelper.setRelationshipProperties( rel, properties );
		 }

		 private long[] CreateDijkstraGraph( bool includeOnes )
		 {
			  /* Layout:
			   *                       (y)
			   *                        ^
			   *                        [2]  _____[1]___
			   *                          \ v           |
			   * (start)--[1]->(a)--[9]-->(x)<-        (e)--[2]->(f)
			   *                |         ^ ^^  \       ^
			   *               [1]  ---[7][5][4] -[3]  [1]
			   *                v  /       | /      \  /
			   *               (b)--[1]-->(c)--[1]->(d)
			   */

			  IDictionary<string, object> costOneProperties = includeOnes ? map( "cost", ( double ) 1 ) : map();
			  long start = _graphdbHelper.createNode();
			  long a = _graphdbHelper.createNode();
			  long b = _graphdbHelper.createNode();
			  long c = _graphdbHelper.createNode();
			  long d = _graphdbHelper.createNode();
			  long e = _graphdbHelper.createNode();
			  long f = _graphdbHelper.createNode();
			  long x = _graphdbHelper.createNode();
			  long y = _graphdbHelper.createNode();

			  CreateRelationshipWithProperties( start, a, costOneProperties );
			  CreateRelationshipWithProperties( a, x, map( "cost", ( double ) 9 ) );
			  CreateRelationshipWithProperties( a, b, costOneProperties );
			  CreateRelationshipWithProperties( b, x, map( "cost", ( double ) 7 ) );
			  CreateRelationshipWithProperties( b, c, costOneProperties );
			  CreateRelationshipWithProperties( c, x, map( "cost", ( double ) 5 ) );
			  CreateRelationshipWithProperties( c, x, map( "cost", ( double ) 4 ) );
			  CreateRelationshipWithProperties( c, d, costOneProperties );
			  CreateRelationshipWithProperties( d, x, map( "cost", ( double ) 3 ) );
			  CreateRelationshipWithProperties( d, e, costOneProperties );
			  CreateRelationshipWithProperties( e, x, costOneProperties );
			  CreateRelationshipWithProperties( e, f, map( "cost", ( double ) 2 ) );
			  CreateRelationshipWithProperties( x, y, map( "cost", ( double ) 2 ) );
			  return new long[]{ start, x };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetShortestPaths()
		 public virtual void ShouldBeAbleToGetShortestPaths()
		 {
			  long[] nodes = CreateMoreComplexGraph();

			  // /paths
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					IList<object> result = serialize( _actions.findPaths( nodes[0], nodes[1], MapUtil.map( "max_depth", 2, "algorithm", "shortestPath", "relationships", MapUtil.map( "type", "to", "direction", "out" ) ) ) );
					AssertPaths( 2, nodes, 2, result );
					// /path
					IDictionary<string, object> path = serialize( _actions.findSinglePath( nodes[0], nodes[1], MapUtil.map( "max_depth", 2, "algorithm", "shortestPath", "relationships", MapUtil.map( "type", "to", "direction", "out" ) ) ) );
					AssertPaths( 1, nodes, 2, Arrays.asList( path ) );

					// /path {single: false} (has no effect)
					path = serialize( _actions.findSinglePath( nodes[0], nodes[1], MapUtil.map( "max_depth", 2, "algorithm", "shortestPath", "relationships", MapUtil.map( "type", "to", "direction", "out" ), "single", false ) ) );
					AssertPaths( 1, nodes, 2, Arrays.asList( path ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetPathsUsingDijkstra()
		 public virtual void ShouldBeAbleToGetPathsUsingDijkstra()
		 {
			  long[] nodes = CreateDijkstraGraph( true );

			  using ( Transaction transaction = _graph.beginTx() )
			  {
					// /paths
					AssertPaths( 1, nodes, 6, serialize( _actions.findPaths( nodes[0], nodes[1], map( "algorithm", "dijkstra", "cost_property", "cost", "relationships", map( "type", "to", "direction", "out" ) ) ) ) );

					// /path
					IDictionary<string, object> path = serialize( _actions.findSinglePath( nodes[0], nodes[1], map( "algorithm", "dijkstra", "cost_property", "cost", "relationships", map( "type", "to", "direction", "out" ) ) ) );
					AssertPaths( 1, nodes, 6, Arrays.asList( path ) );
					assertEquals( 6.0d, path["weight"] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetPathsUsingDijkstraWithDefaults()
		 public virtual void ShouldBeAbleToGetPathsUsingDijkstraWithDefaults()
		 {
			  long[] nodes = CreateDijkstraGraph( false );

			  // /paths
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					IList<object> result = serialize( _actions.findPaths( nodes[0], nodes[1], map( "algorithm", "dijkstra", "cost_property", "cost", "default_cost", 1, "relationships", map( "type", "to", "direction", "out" ) ) ) );
					AssertPaths( 1, nodes, 6, result );

					// /path
					IDictionary<string, object> path = serialize( _actions.findSinglePath( nodes[0], nodes[1], map( "algorithm", "dijkstra", "cost_property", "cost", "default_cost", 1, "relationships", map( "type", "to", "direction", "out" ) ) ) );
					AssertPaths( 1, nodes, 6, Arrays.asList( path ) );
					assertEquals( 6.0d, path["weight"] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.NotFoundException.class) public void shouldHandleNoFoundPathsCorrectly()
		 public virtual void ShouldHandleNoFoundPathsCorrectly()
		 {
			  long[] nodes = CreateMoreComplexGraph();
			  _actions.findSinglePath( nodes[0], nodes[1], map( "max_depth", 2, "algorithm", "shortestPath", "relationships", map( "type", "to", "direction", "in" ), "single", false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddLabelToNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddLabelToNode()
		 {
			  // GIVEN
			  long node = _actions.createNode( null ).Id;
			  ICollection<string> labels = new List<string>();
			  string labelName = "Wonk";
			  labels.Add( labelName );

			  // WHEN
			  _actions.addLabelToNode( node, labels );

			  // THEN
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					IEnumerable<string> result = _graphdbHelper.getNodeLabels( node );
					assertEquals( labelName, single( result ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveLabelFromNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveLabelFromNode()
		 {
			  // GIVEN
			  string labelName = "mylabel";
			  long node = _actions.createNode( null, label( labelName ) ).Id;

			  // WHEN
			  _actions.removeLabelFromNode( node, labelName );

			  // THEN
			  assertEquals( 0, _graphdbHelper.getLabelCount( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListExistingLabelsOnNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListExistingLabelsOnNode()
		 {
			  // GIVEN
			  long node = _graphdbHelper.createNode();
			  string labelName1 = "LabelOne";
			  string labelName2 = "labelTwo";
			  _graphdbHelper.addLabelToNode( node, labelName1 );
			  _graphdbHelper.addLabelToNode( node, labelName2 );

			  // WHEN

			  IList<string> labels;
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					labels = ( System.Collections.IList ) serialize( _actions.getNodeLabels( node ) );
			  }

			  // THEN
			  assertEquals( asSet( labelName1, labelName2 ), Iterables.asSet( labels ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getNodesWithLabel()
		 public virtual void getNodesWithLabel()
		 {
			  // GIVEN
			  string label1 = "first";
			  string label2 = "second";
			  long node1 = _graphdbHelper.createNode( label( label1 ) );
			  long node2 = _graphdbHelper.createNode( label( label1 ), label( label2 ) );
			  _graphdbHelper.createNode( label( label2 ) );

			  // WHEN
			  IList<object> representation;
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					representation = serialize( _actions.getNodesWithLabel( label1, map() ) );
			  }

			  // THEN
			  assertEquals(asSet(node1, node2), Iterables.asSet(Iterables.map(from =>
			  {
				IDictionary<object, ?> nodeMap = ( IDictionary<object, ?> ) from;
				return nodeUriToId( ( string ) nodeMap.get( "self" ) );
			  }, representation)));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void getNodesWithLabelAndSeveralPropertiesShouldFail()
		 public virtual void getNodesWithLabelAndSeveralPropertiesShouldFail()
		 {
			  // WHEN
			  _actions.getNodesWithLabel( "Person", map( "name", "bob", "age", 12 ) );
		 }

		 private void AssertPaths( int numPaths, long[] nodes, int length, IList<object> result )
		 {
			  assertEquals( numPaths, result.Count );
			  foreach ( object path in result )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String, Object> serialized = (java.util.Map<String, Object>) path;
					IDictionary<string, object> serialized = ( IDictionary<string, object> ) path;
					assertTrue( serialized["start"].ToString().EndsWith("/" + nodes[0], StringComparison.Ordinal) );
					assertTrue( serialized["end"].ToString().EndsWith("/" + nodes[1], StringComparison.Ordinal) );
					assertEquals( length, serialized["length"] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateSchemaIndex()
		 public virtual void ShouldCreateSchemaIndex()
		 {
			  // GIVEN
			  string labelName = "person";
			  string propertyKey = "name";

			  // WHEN
			  _actions.createSchemaIndex( labelName, Arrays.asList( propertyKey ) );

			  // THEN
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					IEnumerable<IndexDefinition> defs = _graphdbHelper.getSchemaIndexes( labelName );
					assertEquals( 1, Iterables.count( defs ) );
					assertEquals( propertyKey, firstOrNull( firstOrNull( defs ).PropertyKeys ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropSchemaIndex()
		 public virtual void ShouldDropSchemaIndex()
		 {
			  // GIVEN
			  string labelName = "user";
			  string propertyKey = "login";
			  IndexDefinition index = _graphdbHelper.createSchemaIndex( labelName, propertyKey );

			  // WHEN
			  _actions.dropSchemaIndex( labelName, propertyKey );

			  // THEN
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					assertFalse( "Index should have been dropped", Iterables.asSet( _graphdbHelper.getSchemaIndexes( labelName ) ).Contains( index ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetSchemaIndexes()
		 public virtual void ShouldGetSchemaIndexes()
		 {
			  // GIVEN
			  string labelName = "mylabel";
			  string propertyKey = "name";
			  _graphdbHelper.createSchemaIndex( labelName, propertyKey );

			  // WHEN
			  IList<object> serialized;
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					serialized = serialize( _actions.getSchemaIndexes( labelName ) );
			  }

			  // THEN
			  assertEquals( 1, serialized.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> definition = (java.util.Map<?, ?>) serialized.get(0);
			  IDictionary<object, ?> definition = ( IDictionary<object, ?> ) serialized[0];
			  assertEquals( labelName, definition["label"] );
			  assertEquals( asList( propertyKey ), definition["property_keys"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreatePropertyUniquenessConstraint()
		 public virtual void ShouldCreatePropertyUniquenessConstraint()
		 {
			  // GIVEN
			  string labelName = "person";
			  string propertyKey = "name";

			  // WHEN
			  _actions.createPropertyUniquenessConstraint( labelName, asList( propertyKey ) );

			  // THEN
			  using ( Transaction tx = _graph.beginTx() )
			  {
					IEnumerable<ConstraintDefinition> defs = _graphdbHelper.getPropertyUniquenessConstraints( labelName, propertyKey );
					assertEquals( asSet( propertyKey ), Iterables.asSet( single( defs ).PropertyKeys ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropPropertyUniquenessConstraint()
		 public virtual void ShouldDropPropertyUniquenessConstraint()
		 {
			  // GIVEN
			  string labelName = "user";
			  string propertyKey = "login";
			  ConstraintDefinition index = _graphdbHelper.createPropertyUniquenessConstraint( labelName, new IList<string> { propertyKey } );

			  // WHEN
			  _actions.dropPropertyUniquenessConstraint( labelName, asList( propertyKey ) );

			  // THEN
			  assertFalse( "Constraint should have been dropped", Iterables.asSet( _graphdbHelper.getPropertyUniquenessConstraints( labelName, propertyKey ) ).Contains( index ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropNonExistentConstraint()
		 public virtual void DropNonExistentConstraint()
		 {
			  // GIVEN
			  string labelName = "user";
			  string propertyKey = "login";
			  ConstraintDefinition constraint = _graphdbHelper.createPropertyUniquenessConstraint( labelName, new IList<string> { propertyKey } );

			  // EXPECT
			  ExpectedException.expect( typeof( ConstraintViolationException ) );

			  // WHEN
			  using ( Transaction tx = _graph.beginTx() )
			  {
					constraint.Drop();
					constraint.Drop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetPropertyUniquenessConstraint()
		 public virtual void ShouldGetPropertyUniquenessConstraint()
		 {
			  // GIVEN
			  string labelName = "mylabel";
			  string propertyKey = "name";
			  _graphdbHelper.createPropertyUniquenessConstraint( labelName, new IList<string> { propertyKey } );

			  // WHEN
			  IList<object> serialized;
			  using ( Transaction transaction = _graph.beginTx() )
			  {
					serialized = serialize( _actions.getPropertyUniquenessConstraint( labelName, asList( propertyKey ) ) );
			  }

			  // THEN
			  assertEquals( 1, serialized.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> definition = (java.util.Map<?, ?>) serialized.get(0);
			  IDictionary<object, ?> definition = ( IDictionary<object, ?> ) serialized[0];
			  assertEquals( labelName, definition["label"] );
			  assertEquals( asList( propertyKey ), definition["property_keys"] );
			  assertEquals( ConstraintType.UNIQUENESS.name(), definition["type"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexNodeOnlyOnce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIndexNodeOnlyOnce()
		 {
			  long nodeId = _graphdbHelper.createNode();
			  _graphdbHelper.createRelationshipIndex( "myIndex" );

			  using ( Transaction tx = _graph.beginTx() )
			  {
					Pair<IndexedEntityRepresentation, bool> result = _actions.getOrCreateIndexedNode( "myIndex", "foo", "bar", nodeId, null );

					assertThat( result.Other(), @is(true) );
					assertThat( serialize( _actions.getIndexedNodes( "myIndex", "foo", "bar" ) ).size(), @is(1) );
					assertThat( _actions.nodeIsIndexed( "myIndex", "foo", "bar", nodeId ), @is( true ) );

					tx.Success();
			  }

			  using ( Transaction tx = _graph.beginTx() )
			  {
					Pair<IndexedEntityRepresentation, bool> result = _actions.getOrCreateIndexedNode( "myIndex", "foo", "bar", nodeId, null );

					assertThat( result.Other(), @is(false) );
					assertThat( serialize( _actions.getIndexedNodes( "myIndex", "foo", "bar" ) ).size(), @is(1) );
					assertThat( _actions.nodeIsIndexed( "myIndex", "foo", "bar", nodeId ), @is( true ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndexRelationshipOnlyOnce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIndexRelationshipOnlyOnce()
		 {
			  long relationshipId = _graphdbHelper.createRelationship( "FOO" );
			  _graphdbHelper.createRelationshipIndex( "myIndex" );

			  using ( Transaction tx = _graph.beginTx() )
			  {
					Pair<IndexedEntityRepresentation, bool> result = _actions.getOrCreateIndexedRelationship( "myIndex", "foo", "bar", relationshipId, null, null, null, null );

					assertThat( result.Other(), @is(true) );
					assertThat( serialize( _actions.getIndexedRelationships( "myIndex", "foo", "bar" ) ).size(), @is(1) );
					assertThat( _actions.relationshipIsIndexed( "myIndex", "foo", "bar", relationshipId ), @is( true ) );

					tx.Success();
			  }

			  using ( Transaction tx = _graph.beginTx() )
			  {
					Pair<IndexedEntityRepresentation, bool> result = _actions.getOrCreateIndexedRelationship( "myIndex", "foo", "bar", relationshipId, null, null, null, null );

					assertThat( result.Other(), @is(false) );
					assertThat( serialize( _actions.getIndexedRelationships( "myIndex", "foo", "bar" ) ).size(), @is(1) );
					assertThat( _actions.relationshipIsIndexed( "myIndex", "foo", "bar", relationshipId ), @is( true ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIndexNodeWhenAnotherNodeAlreadyIndexed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotIndexNodeWhenAnotherNodeAlreadyIndexed()
		 {
			  _graphdbHelper.createRelationshipIndex( "myIndex" );

			  using ( Transaction tx = _graph.beginTx() )
			  {
					long nodeId = _graphdbHelper.createNode();
					Pair<IndexedEntityRepresentation, bool> result = _actions.getOrCreateIndexedNode( "myIndex", "foo", "bar", nodeId, null );

					assertThat( result.Other(), @is(true) );
					assertThat( serialize( _actions.getIndexedNodes( "myIndex", "foo", "bar" ) ).size(), @is(1) );
					assertThat( _actions.nodeIsIndexed( "myIndex", "foo", "bar", nodeId ), @is( true ) );

					tx.Success();
			  }

			  using ( Transaction tx = _graph.beginTx() )
			  {
					long nodeId = _graphdbHelper.createNode();
					Pair<IndexedEntityRepresentation, bool> result = _actions.getOrCreateIndexedNode( "myIndex", "foo", "bar", nodeId, null );

					assertThat( result.Other(), @is(false) );
					assertThat( serialize( _actions.getIndexedNodes( "myIndex", "foo", "bar" ) ).size(), @is(1) );
					assertThat( _actions.nodeIsIndexed( "myIndex", "foo", "bar", nodeId ), @is( false ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIndexRelationshipWhenAnotherRelationshipAlreadyIndexed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotIndexRelationshipWhenAnotherRelationshipAlreadyIndexed()
		 {

			  _graphdbHelper.createRelationshipIndex( "myIndex" );

			  using ( Transaction tx = _graph.beginTx() )
			  {
					long relationshipId = _graphdbHelper.createRelationship( "FOO" );
					Pair<IndexedEntityRepresentation, bool> result = _actions.getOrCreateIndexedRelationship( "myIndex", "foo", "bar", relationshipId, null, null, null, null );

					assertThat( result.Other(), @is(true) );
					assertThat( serialize( _actions.getIndexedRelationships( "myIndex", "foo", "bar" ) ).size(), @is(1) );
					assertThat( _actions.relationshipIsIndexed( "myIndex", "foo", "bar", relationshipId ), @is( true ) );

					tx.Success();
			  }

			  using ( Transaction tx = _graph.beginTx() )
			  {
					long relationshipId = _graphdbHelper.createRelationship( "FOO" );
					Pair<IndexedEntityRepresentation, bool> result = _actions.getOrCreateIndexedRelationship( "myIndex", "foo", "bar", relationshipId, null, null, null, null );

					assertThat( result.Other(), @is(false) );
					assertThat( serialize( _actions.getIndexedRelationships( "myIndex", "foo", "bar" ) ).size(), @is(1) );
					assertThat( _actions.relationshipIsIndexed( "myIndex", "foo", "bar", relationshipId ), @is( false ) );

					tx.Success();
			  }
		 }
	}

}