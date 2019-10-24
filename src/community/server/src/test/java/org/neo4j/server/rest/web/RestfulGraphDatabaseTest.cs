using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.Index;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using Status_Request = Neo4Net.Kernel.Api.Exceptions.Status_Request;
	using Status_Schema = Neo4Net.Kernel.Api.Exceptions.Status_Schema;
	using Status_Statement = Neo4Net.Kernel.Api.Exceptions.Status_Statement;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Database = Neo4Net.Server.database.Database;
	using WrappedDatabase = Neo4Net.Server.database.WrappedDatabase;
	using ConfigAdapter = Neo4Net.Server.plugins.ConfigAdapter;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using RelationshipRepresentationTest = Neo4Net.Server.rest.repr.RelationshipRepresentationTest;
	using JsonFormat = Neo4Net.Server.rest.repr.formats.JsonFormat;
	using RelationshipDirection = Neo4Net.Server.rest.web.DatabaseActions.RelationshipDirection;
	using AmpersandSeparatedCollection = Neo4Net.Server.rest.web.RestfulGraphDatabase.AmpersandSeparatedCollection;
	using UTF8 = Neo4Net.Strings.UTF8;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using IEntityOutputFormat = Neo4Net.Test.server.EntityOutputFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.exceptions.Status_Request.InvalidFormat;

	public class RestfulGraphDatabaseTest
	{
		 private const string NODE_AUTO_INDEX = "node_auto_index";
		 private const string RELATIONSHIP_AUTO_INDEX = "relationship_auto_index";
		 private const string BASE_URI = "http://Neo4Net.org/";
		 private const string NODE_SUBPATH = "node/";
		 private static RestfulGraphDatabase _service;
		 private static Database _database;
		 private static GraphDbHelper _helper;
		 private static IEntityOutputFormat _output;
		 private static GraphDatabaseFacade _graph;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void doBefore()
		 public static void DoBefore()
		 {
			  _graph = ( GraphDatabaseFacade ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  _database = new WrappedDatabase( _graph );
			  _helper = new GraphDbHelper( _database );
			  _output = new IEntityOutputFormat( new JsonFormat(), URI.create(BASE_URI), null );
			  DatabaseActions databaseActions = new DatabaseActions( _database.Graph );
			  _service = new TransactionWrappingRestfulGraphDatabase(_graph, new RestfulGraphDatabase(new JsonFormat(), _output, databaseActions, new ConfigAdapter(Config.defaults())
						));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void deleteAllIndexes() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeleteAllIndexes()
		 {
			  foreach ( string name in _helper.NodeIndexes )
			  {
					if ( NODE_AUTO_INDEX.Equals( name ) )
					{
						 StopAutoIndexAllPropertiesAndDisableAutoIndex( "node" );
					}
					else
					{
						 _service.deleteNodeIndex( name );
					}
			  }
			  foreach ( string name in _helper.RelationshipIndexes )
			  {
					if ( RELATIONSHIP_AUTO_INDEX.Equals( name ) )
					{
						 StopAutoIndexAllPropertiesAndDisableAutoIndex( "relationship" );
					}
					else
					{
						 _service.deleteRelationshipIndex( name );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void stopAutoIndexAllPropertiesAndDisableAutoIndex(String type) throws org.Neo4Net.server.rest.domain.JsonParseException
		 protected internal virtual void StopAutoIndexAllPropertiesAndDisableAutoIndex( string type )
		 {
			  Response response = _service.getAutoIndexedProperties( type );
			  IList<string> properties = IEntityAsList( response );
			  foreach ( string property in properties )
			  {
					_service.stopAutoIndexingProperty( type, property );
			  }
			  _service.setAutoIndexerEnabled( type, "false" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void shutdownDatabase()
		 public static void ShutdownDatabase()
		 {
			  _graph.shutdown();
		 }

		 private static string IEntityAsString( Response response )
		 {
			  sbyte[] bytes = ( sbyte[] ) response.Entity;
			  return UTF8.decode( bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static java.util.List<String> IEntityAsList(javax.ws.rs.core.Response response) throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static IList<string> IEntityAsList( Response response )
		 {
			  string IEntity = IEntityAsString( response );
			  return ( IList<string> ) JsonHelper.readJson( IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailGracefullyWhenViolatingConstraintOnPropertyUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailGracefullyWhenViolatingConstraintOnPropertyUpdate()
		 {
			  Response response = _service.createPropertyUniquenessConstraint( "Person", "{\"property_keys\":[\"name\"]}" );
			  assertEquals( 200, response.Status );

			  CreatePerson( "Fred" );
			  string wilma = CreatePerson( "Wilma" );

			  Response setAllNodePropertiesResponse = _service.setAllNodeProperties( parseLong( wilma ), "{\"name\":\"Fred\"}" );
			  assertEquals( 409, setAllNodePropertiesResponse.Status );
			  assertEquals( Status_Schema.ConstraintValidationFailed.code().serialize(), SingleErrorCode(setAllNodePropertiesResponse) );

			  Response singleNodePropertyResponse = _service.setNodeProperty( parseLong( wilma ), "name", "\"Fred\"" );
			  assertEquals( 409, singleNodePropertyResponse.Status );
			  assertEquals( Status_Schema.ConstraintValidationFailed.code().serialize(), SingleErrorCode(singleNodePropertyResponse) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String createPerson(final String name) throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private string CreatePerson( string name )
		 {
			  Response response = _service.createNode( "{\"name\" : \"" + name + "\"}" );
			  assertEquals( 201, response.Status );
			  string self = ( string ) JsonHelper.jsonToMap( IEntityAsString( response ) )["self"];
			  string nodeId = self.Substring( self.IndexOf( NODE_SUBPATH, StringComparison.Ordinal ) + NODE_SUBPATH.Length );
			  response = _service.addNodeLabel( parseLong( nodeId ), "\"Person\"" );
			  assertEquals( 204, response.Status );
			  return nodeId;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith201LocationHeaderAndNodeRepresentationInJSONWhenEmptyNodeCreated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith201LocationHeaderAndNodeRepresentationInJSONWhenEmptyNodeCreated()
		 {
			  Response response = _service.createNode( null );

			  assertEquals( 201, response.Status );
			  assertNotNull( response.Metadata.get( "Location" ).get( 0 ) );

			  CheckContentTypeCharsetUtf8( response );
			  string json = IEntityAsString( response );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( json );

			  assertNotNull( map );

			  assertTrue( map.ContainsKey( "self" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith201LocationHeaderAndNodeRepresentationInJSONWhenPopulatedNodeCreated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith201LocationHeaderAndNodeRepresentationInJSONWhenPopulatedNodeCreated()
		 {
			  Response response = _service.createNode( "{\"foo\" : \"bar\"}" );

			  assertEquals( 201, response.Status );
			  assertNotNull( response.Metadata.get( "Location" ).get( 0 ) );

			  CheckContentTypeCharsetUtf8( response );
			  string json = IEntityAsString( response );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( json );

			  assertNotNull( map );

			  assertTrue( map.ContainsKey( "self" ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String, Object> data = (java.util.Map<String, Object>) map.get("data");
			  IDictionary<string, object> data = ( IDictionary<string, object> ) map["data"];

			  assertEquals( "bar", data["foo"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldRespondWith201LocationHeaderAndNodeRepresentationInJSONWhenPopulatedNodeCreatedWithArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith201LocationHeaderAndNodeRepresentationInJSONWhenPopulatedNodeCreatedWithArrays()
		 {
			  Response response = _service.createNode( "{\"foo\" : [\"bar\", \"baz\"] }" );

			  assertEquals( 201, response.Status );
			  assertNotNull( response.Metadata.get( "Location" ).get( 0 ) );
			  string json = IEntityAsString( response );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( json );

			  assertNotNull( map );

			  IDictionary<string, object> data = ( IDictionary<string, object> ) map["data"];

			  IList<string> foo = ( IList<string> ) data["foo"];
			  assertNotNull( foo );
			  assertEquals( 2, foo.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenNodeCreatedWithUnsupportedPropertyData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenNodeCreatedWithUnsupportedPropertyData()
		 {
			  Response response = _service.createNode( "{\"foo\" : {\"bar\" : \"baz\"}}" );

			  assertEquals( 400, response.Status );
			  assertEquals( Status_Statement.ArgumentError.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenNodeCreatedWithInvalidJSON() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenNodeCreatedWithInvalidJSON()
		 {
			  Response response = _service.createNode( "this:::isNot::JSON}" );

			  assertEquals( 400, response.Status );
			  assertEquals( InvalidFormat.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndNodeRepresentationInJSONWhenNodeRequested() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndNodeRepresentationInJSONWhenNodeRequested()
		 {
			  Response response = _service.getNode( _helper.createNode() );
			  assertEquals( 200, response.Status );
			  string json = IEntityAsString( response );
			  IDictionary<string, object> map = JsonHelper.jsonToMap( json );
			  assertNotNull( map );
			  assertTrue( map.ContainsKey( "self" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenRequestedNodeDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenRequestedNodeDoesNotExist()
		 {
			  Response response = _service.getNode( 9000000000000L );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204AfterSettingPropertiesOnExistingNode()
		 public virtual void ShouldRespondWith204AfterSettingPropertiesOnExistingNode()
		 {
			  Response response = _service.setAllNodeProperties( _helper.createNode(), "{\"foo\" : \"bar\", \"a-boolean\": true, \"boolean-array\": [true, false, false]}" );
			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenSettingPropertiesOnNodeThatDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenSettingPropertiesOnNodeThatDoesNotExist()
		 {
			  Response response = _service.setAllNodeProperties( 9000000000000L, "{\"foo\" : \"bar\"}" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenTransferringCorruptJsonPayload() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenTransferringCorruptJsonPayload()
		 {
			  Response response = _service.setAllNodeProperties( _helper.createNode(), "{\"foo\" : bad-json-here \"bar\"}" );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Request.InvalidFormat.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenTransferringIncompatibleJsonPayload() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenTransferringIncompatibleJsonPayload()
		 {
			  Response response = _service.setAllNodeProperties( _helper.createNode(), "{\"foo\" : {\"bar\" : \"baz\"}}" );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Statement.ArgumentError.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200ForGetNodeProperties()
		 public virtual void ShouldRespondWith200ForGetNodeProperties()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  _helper.setNodeProperties( nodeId, properties );
			  Response response = _service.getAllNodeProperties( nodeId );
			  assertEquals( 200, response.Status );

			  CheckContentTypeCharsetUtf8( response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetPropertiesForGetNodeProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetPropertiesForGetNodeProperties()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  properties["double"] = 15.7;
			  _helper.setNodeProperties( nodeId, properties );
			  Response response = _service.getAllNodeProperties( nodeId );
			  string jsonBody = IEntityAsString( response );
			  IDictionary<string, object> readProperties = JsonHelper.jsonToMap( jsonBody );
			  assertEquals( properties, readProperties );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204OnSuccessfulDelete()
		 public virtual void ShouldRespondWith204OnSuccessfulDelete()
		 {
			  long id = _helper.createNode();

			  Response response = _service.deleteNode( id );

			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith409IfNodeCannotBeDeleted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith409IfNodeCannotBeDeleted()
		 {
			  long id = _helper.createNode();
			  _helper.createRelationship( "LOVES", id, _helper.createNode() );

			  Response response = _service.deleteNode( id );

			  assertEquals( 409, response.Status );
			  assertEquals( Status_Schema.ConstraintValidationFailed.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404IfNodeToBeDeletedDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404IfNodeToBeDeletedDoesNotExist()
		 {
			  long nonExistentId = 999999;
			  Response response = _service.deleteNode( nonExistentId );

			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204ForSetNodeProperty()
		 public virtual void ShouldRespondWith204ForSetNodeProperty()
		 {
			  long nodeId = _helper.createNode();
			  string key = "foo";
			  string json = "\"bar\"";
			  Response response = _service.setNodeProperty( nodeId, key, json );
			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetRightValueForSetNodeProperty()
		 public virtual void ShouldSetRightValueForSetNodeProperty()
		 {
			  long nodeId = _helper.createNode();
			  string key = "foo";
			  object value = "bar";
			  string json = "\"" + value + "\"";
			  _service.setNodeProperty( nodeId, key, json );
			  IDictionary<string, object> readProperties = _helper.getNodeProperties( nodeId );
			  assertEquals( Collections.singletonMap( key, value ), readProperties );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404ForSetNodePropertyOnNonExistingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404ForSetNodePropertyOnNonExistingNode()
		 {
			  string key = "foo";
			  string json = "\"bar\"";
			  Response response = _service.setNodeProperty( 999999, key, json );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400ForSetNodePropertyWithInvalidJson() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400ForSetNodePropertyWithInvalidJson()
		 {
			  string key = "foo";
			  string json = "{invalid json";
			  Response response = _service.setNodeProperty( 999999, key, json );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Request.InvalidFormat.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404ForGetNonExistentNodeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404ForGetNonExistentNodeProperty()
		 {
			  long nodeId = _helper.createNode();
			  Response response = _service.getNodeProperty( nodeId, "foo" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.PropertyNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404ForGetNodePropertyOnNonExistentNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404ForGetNodePropertyOnNonExistentNode()
		 {
			  long nodeId = 999999;
			  Response response = _service.getNodeProperty( nodeId, "foo" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200ForGetNodeProperty()
		 public virtual void ShouldRespondWith200ForGetNodeProperty()
		 {
			  long nodeId = _helper.createNode();
			  string key = "foo";
			  object value = "bar";
			  _helper.setNodeProperties( nodeId, Collections.singletonMap( key, value ) );
			  Response response = _service.getNodeProperty( nodeId, "foo" );
			  assertEquals( 200, response.Status );

			  CheckContentTypeCharsetUtf8( response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCorrectValueForGetNodeProperty()
		 public virtual void ShouldReturnCorrectValueForGetNodeProperty()
		 {
			  long nodeId = _helper.createNode();
			  string key = "foo";
			  object value = "bar";
			  _helper.setNodeProperties( nodeId, Collections.singletonMap( key, value ) );
			  Response response = _service.getNodeProperty( nodeId, "foo" );
			  assertEquals( JsonHelper.createJsonFrom( value ), IEntityAsString( response ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith201AndLocationWhenRelationshipIsCreatedWithoutProperties()
		 public virtual void ShouldRespondWith201AndLocationWhenRelationshipIsCreatedWithoutProperties()

		 {
			  long startNode = _helper.createNode();
			  long endNode = _helper.createNode();
			  Response response = _service.createRelationship( startNode, "{\"to\" : \"" + BASE_URI + endNode + "\", \"type\" : \"LOVES\"}" );
			  assertEquals( 201, response.Status );
			  assertNotNull( response.Metadata.get( "Location" ).get( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith201AndLocationWhenRelationshipIsCreatedWithProperties()
		 public virtual void ShouldRespondWith201AndLocationWhenRelationshipIsCreatedWithProperties()

		 {
			  long startNode = _helper.createNode();
			  long endNode = _helper.createNode();
			  Response response = _service.createRelationship( startNode, "{\"to\" : \"" + BASE_URI + endNode + "\", \"type\" : \"LOVES\", " + "\"properties\" : {\"foo\" : \"bar\"}}" );
			  assertEquals( 201, response.Status );
			  assertNotNull( response.Metadata.get( "Location" ).get( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnRelationshipRepresentationWhenCreatingRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnRelationshipRepresentationWhenCreatingRelationship()
		 {
			  long startNode = _helper.createNode();
			  long endNode = _helper.createNode();
			  Response response = _service.createRelationship( startNode, "{\"to\" : \"" + BASE_URI + endNode + "\", \"type\" : \"LOVES\", \"data\" : {\"foo\" : \"bar\"}}" );
			  IDictionary<string, object> map = JsonHelper.jsonToMap( IEntityAsString( response ) );
			  assertNotNull( map );
			  assertTrue( map.ContainsKey( "self" ) );

			  CheckContentTypeCharsetUtf8( response );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String, Object> data = (java.util.Map<String, Object>) map.get("data");
			  IDictionary<string, object> data = ( IDictionary<string, object> ) map["data"];

			  assertEquals( "bar", data["foo"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenTryingToCreateRelationshipFromNonExistentNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenTryingToCreateRelationshipFromNonExistentNode()
		 {
			  long nodeId = _helper.createNode();
			  Response response = _service.createRelationship( nodeId + 1000, "{\"to\" : \"" + BASE_URI + nodeId + "\", \"type\" : \"LOVES\"}" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenTryingToCreateRelationshipToNonExistentNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenTryingToCreateRelationshipToNonExistentNode()
		 {
			  long nodeId = _helper.createNode();
			  Response response = _service.createRelationship( nodeId, "{\"to\" : \"" + BASE_URI + ( nodeId + 1000 ) + "\", \"type\" : \"LOVES\"}" );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith201WhenTryingToCreateRelationshipBackToSelf()
		 public virtual void ShouldRespondWith201WhenTryingToCreateRelationshipBackToSelf()
		 {
			  long nodeId = _helper.createNode();
			  Response response = _service.createRelationship( nodeId, "{\"to\" : \"" + BASE_URI + nodeId + "\", \"type\" : \"LOVES\"}" );
			  assertEquals( 201, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenTryingToCreateRelationshipWithBadJson() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenTryingToCreateRelationshipWithBadJson()
		 {
			  long startNode = _helper.createNode();
			  long endNode = _helper.createNode();
			  Response response = _service.createRelationship( startNode, "{\"to\" : \"" + BASE_URI + endNode + "\", \"type\" ***and junk*** : \"LOVES\"}" );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Request.InvalidFormat.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenTryingToCreateRelationshipWithUnsupportedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenTryingToCreateRelationshipWithUnsupportedProperties()

		 {
			  long startNode = _helper.createNode();
			  long endNode = _helper.createNode();
			  Response response = _service.createRelationship( startNode, "{\"to\" : \"" + BASE_URI + endNode + "\", \"type\" : \"LOVES\", \"data\" : {\"foo\" : {\"bar\" : \"baz\"}}}" );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Statement.ArgumentError.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204ForRemoveNodeProperties()
		 public virtual void ShouldRespondWith204ForRemoveNodeProperties()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  _helper.setNodeProperties( nodeId, properties );
			  Response response = _service.deleteAllNodeProperties( nodeId );
			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveNodeProperties()
		 public virtual void ShouldBeAbleToRemoveNodeProperties()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  _helper.setNodeProperties( nodeId, properties );
			  _service.deleteAllNodeProperties( nodeId );
			  assertTrue( _helper.getNodeProperties( nodeId ).Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404ForRemoveNodePropertiesForNonExistingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404ForRemoveNodePropertiesForNonExistingNode()
		 {
			  long nodeId = 999999;
			  Response response = _service.deleteAllNodeProperties( nodeId );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveNodeProperty()
		 public virtual void ShouldBeAbleToRemoveNodeProperty()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  _helper.setNodeProperties( nodeId, properties );
			  _service.deleteNodeProperty( nodeId, "foo" );
			  assertEquals( Collections.singletonMap( "number", ( object ) 15 ), _helper.getNodeProperties( nodeId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenRemovingNonExistingProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet404WhenRemovingNonExistingProperty()
		 {
			  long nodeId = _helper.createNode();
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  properties["number"] = 15;
			  _helper.setNodeProperties( nodeId, properties );
			  Response response = _service.deleteNodeProperty( nodeId, "baz" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.PropertyNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenRemovingPropertyFromNonExistingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet404WhenRemovingPropertyFromNonExistingNode()
		 {
			  long nodeId = 999999;
			  Response response = _service.deleteNodeProperty( nodeId, "foo" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200WhenRetrievingARelationshipFromANode()
		 public virtual void ShouldGet200WhenRetrievingARelationshipFromANode()
		 {
			  long relationshipId = _helper.createRelationship( "BEATS" );
			  Response response = _service.getRelationship( relationshipId );
			  assertEquals( 200, response.Status );

			  CheckContentTypeCharsetUtf8( response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenRetrievingRelationshipThatDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet404WhenRetrievingRelationshipThatDoesNotExist()
		 {
			  Response response = _service.getRelationship( 999999 );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndDataForGetRelationshipProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndDataForGetRelationshipProperties()
		 {
			  long relationshipId = _helper.createRelationship( "knows" );
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["foo"] = "bar";
			  _helper.setRelationshipProperties( relationshipId, properties );
			  Response response = _service.getAllRelationshipProperties( relationshipId );
			  assertEquals( 200, response.Status );

			  CheckContentTypeCharsetUtf8( response );

			  IDictionary<string, object> readProperties = JsonHelper.jsonToMap( IEntityAsString( response ) );
			  assertEquals( properties, readProperties );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200WhenSuccessfullyRetrievedPropertyOnRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet200WhenSuccessfullyRetrievedPropertyOnRelationship()
		 {

			  long relationshipId = _helper.createRelationship( "knows" );
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["some-key"] = "some-value";
			  _helper.setRelationshipProperties( relationshipId, properties );

			  Response response = _service.getRelationshipProperty( relationshipId, "some-key" );

			  assertEquals( 200, response.Status );
			  assertEquals( "some-value", JsonHelper.readJson( IEntityAsString( response ) ) );

			  CheckContentTypeCharsetUtf8( response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenCannotResolveAPropertyOnRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet404WhenCannotResolveAPropertyOnRelationship()
		 {
			  long relationshipId = _helper.createRelationship( "knows" );
			  Response response = _service.getRelationshipProperty( relationshipId, "some-key" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.PropertyNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet204WhenRemovingARelationship()
		 public virtual void ShouldGet204WhenRemovingARelationship()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );

			  Response response = _service.deleteRelationship( relationshipId );
			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenRemovingNonExistentRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet404WhenRemovingNonExistentRelationship()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );

			  Response response = _service.deleteRelationship( relationshipId + 1000 );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingRelationshipsForANode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingRelationshipsForANode()
		 {
			  long nodeId = _helper.createNode();
			  _helper.createRelationship( "LIKES", nodeId, _helper.createNode() );
			  _helper.createRelationship( "LIKES", _helper.createNode(), nodeId );
			  _helper.createRelationship( "HATES", nodeId, _helper.createNode() );

			  Response response = _service.getNodeRelationships( nodeId, RelationshipDirection.all, new AmpersandSeparatedCollection( "" ) );
			  assertEquals( 200, response.Status );

			  CheckContentTypeCharsetUtf8( response );

			  VerifyRelReps( 3, IEntityAsString( response ) );

			  response = _service.getNodeRelationships( nodeId, RelationshipDirection.@in, new AmpersandSeparatedCollection( "" ) );
			  assertEquals( 200, response.Status );
			  VerifyRelReps( 1, IEntityAsString( response ) );

			  response = _service.getNodeRelationships( nodeId, RelationshipDirection.@out, new AmpersandSeparatedCollection( "" ) );
			  assertEquals( 200, response.Status );
			  VerifyRelReps( 2, IEntityAsString( response ) );

			  response = _service.getNodeRelationships( nodeId, RelationshipDirection.@out, new AmpersandSeparatedCollection( "LIKES&HATES" ) );
			  assertEquals( 200, response.Status );
			  VerifyRelReps( 2, IEntityAsString( response ) );

			  response = _service.getNodeRelationships( nodeId, RelationshipDirection.all, new AmpersandSeparatedCollection( "LIKES" ) );
			  assertEquals( 200, response.Status );
			  VerifyRelReps( 2, IEntityAsString( response ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnDuplicatesIfSameTypeSpecifiedMoreThanOnce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnDuplicatesIfSameTypeSpecifiedMoreThanOnce()
		 {
			  long nodeId = _helper.createNode();
			  _helper.createRelationship( "LIKES", nodeId, _helper.createNode() );
			  Response response = _service.getNodeRelationships( nodeId, RelationshipDirection.all, new AmpersandSeparatedCollection( "LIKES&LIKES" ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> array = (java.util.Collection<?>) org.Neo4Net.server.rest.domain.JsonHelper.readJson(entityAsString(response));
			  ICollection<object> array = ( ICollection<object> ) JsonHelper.readJson( IEntityAsString( response ) );
			  assertEquals( 1, array.Count );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRelReps(int expectedSize, String IEntity) throws org.Neo4Net.server.rest.domain.JsonParseException
		 private void VerifyRelReps( int expectedSize, string IEntity )
		 {
			  IList<IDictionary<string, object>> relreps = JsonHelper.jsonToList( IEntity );
			  assertEquals( expectedSize, relreps.Count );
			  foreach ( IDictionary<string, object> relrep in relreps )
			  {
					RelationshipRepresentationTest.verifySerialisation( relrep );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndEmptyListOfRelationshipRepresentationsWhenGettingRelationshipsForANodeWithoutRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndEmptyListOfRelationshipRepresentationsWhenGettingRelationshipsForANodeWithoutRelationships()
		 {
			  long nodeId = _helper.createNode();

			  Response response = _service.getNodeRelationships( nodeId, RelationshipDirection.all, new AmpersandSeparatedCollection( "" ) );
			  assertEquals( 200, response.Status );
			  VerifyRelReps( 0, IEntityAsString( response ) );

			  CheckContentTypeCharsetUtf8( response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenGettingIncomingRelationshipsForNonExistingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenGettingIncomingRelationshipsForNonExistingNode()

		 {
			  Response response = _service.getNodeRelationships( 999999, RelationshipDirection.all, new AmpersandSeparatedCollection( "" ) );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204AndSetCorrectDataWhenSettingRelationshipProperties()
		 public virtual void ShouldRespondWith204AndSetCorrectDataWhenSettingRelationshipProperties()

		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );
			  string json = "{\"name\": \"Mattias\", \"age\": 30}";
			  Response response = _service.setAllRelationshipProperties( relationshipId, json );
			  assertEquals( 204, response.Status );
			  IDictionary<string, object> setProperties = new Dictionary<string, object>();
			  setProperties["name"] = "Mattias";
			  setProperties["age"] = 30;
			  assertEquals( setProperties, _helper.getRelationshipProperties( relationshipId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenSettingRelationshipPropertiesWithBadJson() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenSettingRelationshipPropertiesWithBadJson()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );
			  string json = "{\"name: \"Mattias\", \"age\": 30}";
			  Response response = _service.setAllRelationshipProperties( relationshipId, json );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Request.InvalidFormat.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenSettingRelationshipPropertiesOnNonExistingRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenSettingRelationshipPropertiesOnNonExistingRelationship()

		 {
			  long relationshipId = 99999999;
			  string json = "{\"name\": \"Mattias\", \"age\": 30}";
			  Response response = _service.setAllRelationshipProperties( relationshipId, json );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204AndSetCorrectDataWhenSettingRelationshipProperty()
		 public virtual void ShouldRespondWith204AndSetCorrectDataWhenSettingRelationshipProperty()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );
			  string key = "name";
			  object value = "Mattias";
			  string json = "\"" + value + "\"";
			  Response response = _service.setRelationshipProperty( relationshipId, key, json );
			  assertEquals( 204, response.Status );
			  assertEquals( value, _helper.getRelationshipProperties( relationshipId )["name"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenSettingRelationshipPropertyWithBadJson() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith400WhenSettingRelationshipPropertyWithBadJson()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );
			  string json = "}Mattias";
			  Response response = _service.setRelationshipProperty( relationshipId, "name", json );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Request.InvalidFormat.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenSettingRelationshipPropertyOnNonExistingRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenSettingRelationshipPropertyOnNonExistingRelationship()

		 {
			  long relationshipId = 99999999;
			  string json = "\"Mattias\"";
			  Response response = _service.setRelationshipProperty( relationshipId, "name", json );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204WhenSuccessfullyRemovedRelationshipProperties()
		 public virtual void ShouldRespondWith204WhenSuccessfullyRemovedRelationshipProperties()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );
			  _helper.setRelationshipProperties( relationshipId, Collections.singletonMap( "foo", "bar" ) );

			  Response response = _service.deleteAllRelationshipProperties( relationshipId );
			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204WhenSuccessfullyRemovedRelationshipPropertiesWhichAreEmpty()
		 public virtual void ShouldRespondWith204WhenSuccessfullyRemovedRelationshipPropertiesWhichAreEmpty()

		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );

			  Response response = _service.deleteAllRelationshipProperties( relationshipId );
			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenNoRelationshipFromWhichToRemoveProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenNoRelationshipFromWhichToRemoveProperties()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );

			  Response response = _service.deleteAllRelationshipProperties( relationshipId + 1000 );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith204WhenRemovingRelationshipProperty()
		 public virtual void ShouldRespondWith204WhenRemovingRelationshipProperty()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );
			  _helper.setRelationshipProperties( relationshipId, Collections.singletonMap( "foo", "bar" ) );

			  Response response = _service.deleteRelationshipProperty( relationshipId, "foo" );

			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenRemovingRelationshipPropertyWhichDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenRemovingRelationshipPropertyWhichDoesNotExist()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );
			  Response response = _service.deleteRelationshipProperty( relationshipId, "foo" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.PropertyNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenNoRelationshipFromWhichToRemoveProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith404WhenNoRelationshipFromWhichToRemoveProperty()
		 {
			  long relationshipId = _helper.createRelationship( "KNOWS" );

			  Response response = _service.deleteRelationshipProperty( relationshipId * 1000, "some-key" );
			  assertEquals( 404, response.Status );
			  assertEquals( Status_Statement.EntityNotFound.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithNoIndexOrOnlyNodeAutoIndex()
		 public virtual void ShouldRespondWithNoIndexOrOnlyNodeAutoIndex()
		 {
			  Response isEnabled = _service.isAutoIndexerEnabled( "node" );
			  assertEquals( "false", IEntityAsString( isEnabled ) );
			  Response response = _service.NodeIndexRoot;
			  if ( response.Status == 200 )
			  {
					ISet<string> indexes = _output.ResultAsMap.Keys;
					assertEquals( 1, indexes.Count );
					assertEquals( indexes.GetEnumerator().next(), NODE_AUTO_INDEX );
			  }
			  else
			  {
					assertEquals( 204, response.Status );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithAvailableIndexNodeRoots()
		 public virtual void ShouldRespondWithAvailableIndexNodeRoots()
		 {
			  int numberOfAutoIndexesWhichCouldNotBeDeletedAtTestSetup = _helper.NodeIndexes.Length;
			  string indexName = "someNodes";
			  _helper.createNodeIndex( indexName );
			  Response response = _service.NodeIndexRoot;
			  assertEquals( 200, response.Status );

			  using ( Transaction ignored = _graph.beginTx() )
			  {
					IDictionary<string, object> resultAsMap = _output.ResultAsMap;
					assertThat( resultAsMap.Count, @is( numberOfAutoIndexesWhichCouldNotBeDeletedAtTestSetup + 1 ) );
					assertThat( resultAsMap, hasKey( indexName ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithNoContentWhenNoRelationshipIndexesExist()
		 public virtual void ShouldRespondWithNoContentWhenNoRelationshipIndexesExist()
		 {
			  Response response = _service.RelationshipIndexRoot;
			  assertEquals( 204, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithAvailableIndexRelationshipRoots()
		 public virtual void ShouldRespondWithAvailableIndexRelationshipRoots()
		 {
			  string indexName = "someRelationships";
			  _helper.createRelationshipIndex( indexName );
			  Response response = _service.RelationshipIndexRoot;
			  assertEquals( 200, response.Status );

			  using ( Transaction ignored = _graph.beginTx() )
			  {
					IDictionary<string, object> resultAsMap = _output.ResultAsMap;
					assertThat( resultAsMap.Count, @is( 1 ) );
					assertThat( resultAsMap, hasKey( indexName ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetRoot() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetRoot()
		 {
			  Response response = _service.Root;
			  assertEquals( 200, response.Status );
			  string IEntity = IEntityAsString( response );
			  IDictionary<string, object> map = JsonHelper.jsonToMap( IEntity );
			  assertNotNull( map["node"] );
			  //this can be null
	//        assertNotNull( map.get( "reference_node" ) );
			  assertNotNull( map["Neo4Net_version"] );
			  assertNotNull( map["node_index"] );
			  assertNotNull( map["extensions_info"] );
			  assertNotNull( map["relationship_index"] );
			  assertNotNull( map["batch"] );

			  CheckContentTypeCharsetUtf8( response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetRootWhenNoReferenceNodePresent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetRootWhenNoReferenceNodePresent()
		 {
			  _helper.deleteNode( 0L );

			  Response response = _service.Root;
			  assertEquals( 200, response.Status );
			  string IEntity = IEntityAsString( response );
			  IDictionary<string, object> map = JsonHelper.jsonToMap( IEntity );
			  assertNotNull( map["node"] );

			  assertNotNull( map["node_index"] );
			  assertNotNull( map["extensions_info"] );
			  assertNotNull( map["relationship_index"] );

			  assertNull( map["reference_node"] );

			  CheckContentTypeCharsetUtf8( response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToIndexNode()
		 public virtual void ShouldBeAbleToIndexNode()
		 {
			  Response response = _service.createNode( null );
			  URI nodeUri = ( URI ) response.Metadata.getFirst( "Location" );

			  IDictionary<string, string> postBody = new Dictionary<string, string>();
			  postBody["key"] = "mykey";
			  postBody["value"] = "my/key";
			  postBody["uri"] = nodeUri.ToString();

			  response = _service.addToNodeIndex( "node", null, null, JsonHelper.createJsonFrom( postBody ) );

			  assertEquals( 201, response.Status );
			  assertNotNull( response.Metadata.getFirst( "Location" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToIndexANodePropertyThatsTooLarge()
		 public virtual void ShouldNotBeAbleToIndexANodePropertyThatsTooLarge()
		 {
			  Response response = _service.createNode( null );
			  URI nodeUri = ( URI ) response.Metadata.getFirst( "Location" );

			  IDictionary<string, string> postBody = new Dictionary<string, string>();
			  postBody["key"] = "mykey";

			  char[] alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

			  StringBuilder largePropertyValue = new StringBuilder();
			  Random random = new Random();
			  for ( int i = 0; i < 30_000; i++ )
			  {
					largePropertyValue.Append( alphabet[random.Next( alphabet.Length )] );
			  }

			  postBody["value"] = largePropertyValue.ToString();
			  postBody["uri"] = nodeUri.ToString();

			  response = _service.addToNodeIndex( "node", null, null, JsonHelper.createJsonFrom( postBody ) );

			  assertEquals( 413, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToIndexNodeUniquely()
		 public virtual void ShouldBeAbleToIndexNodeUniquely()
		 {
			  IDictionary<string, string> postBody = new Dictionary<string, string>();
			  postBody["key"] = "mykey";
			  postBody["value"] = "my/key";

			  Response response = _service.addToNodeIndex( "unique-nodes", "", "", JsonHelper.createJsonFrom( postBody ) );

			  assertEquals( 201, response.Status );
			  assertNotNull( response.Metadata.getFirst( "Location" ) );

			  response = _service.addToNodeIndex( "unique-nodes", "", "", JsonHelper.createJsonFrom( postBody ) );

			  assertEquals( 200, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToIndexNodeUniquelyWithBothUriAndPropertiesInPayload() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToIndexNodeUniquelyWithBothUriAndPropertiesInPayload()
		 {
			  URI node = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  IDictionary<string, object> postBody = new Dictionary<string, object>();
			  postBody["key"] = "mykey";
			  postBody["value"] = "my/key";
			  postBody["uri"] = node.ToString();
			  postBody["properties"] = new Dictionary<string, object>();

			  Response response = _service.addToNodeIndex( "unique-nodes", "", "", JsonHelper.createJsonFrom( postBody ) );
			  assertEquals( 400, response.Status );
			  assertEquals( Status_Statement.ArgumentError.code().serialize(), SingleErrorCode(response) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uniquelyIndexedNodeGetsTheSpecifiedKeyAndValueAsPropertiesIfNoPropertiesAreSpecified()
		 public virtual void UniquelyIndexedNodeGetsTheSpecifiedKeyAndValueAsPropertiesIfNoPropertiesAreSpecified()
		 {
			  const string key = "somekey";
			  string value = "somevalue";

			  IDictionary<string, object> postBody = new Dictionary<string, object>();
			  postBody["key"] = key;
			  postBody["value"] = value;

			  Response response = _service.addToNodeIndex( "unique-nodes", "", "", JsonHelper.createJsonFrom( postBody ) );
			  assertEquals( 201, response.Status );
			  object node = response.Metadata.getFirst( "Location" );
			  assertNotNull( node );
			  string uri = node.ToString();
			  IDictionary<string, object> properties = _helper.getNodeProperties( parseLong( uri.Substring( uri.LastIndexOf( '/' ) + 1 ) ) );
			  assertEquals( 1, properties.Count );
			  assertEquals( value, properties[key] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void specifiedPropertiesOverrideKeyAndValueForUniquelyIndexedNodes()
		 public virtual void SpecifiedPropertiesOverrideKeyAndValueForUniquelyIndexedNodes()
		 {
			  const string key = "a_key";
			  string value = "a value";

			  IDictionary<string, object> postBody = new Dictionary<string, object>();
			  postBody["key"] = key;
			  postBody["value"] = value;
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["name"] = "Jürgen";
			  properties["age"] = "42";
			  properties["occupation"] = "crazy";
			  postBody["properties"] = properties;

			  Response response = _service.addToNodeIndex( "unique-nodes", "", "", JsonHelper.createJsonFrom( postBody ) );
			  assertEquals( 201, response.Status );
			  object node = response.Metadata.getFirst( "Location" );
			  assertNotNull( node );
			  string uri = node.ToString();
			  assertEquals( properties, _helper.getNodeProperties( parseLong( uri.Substring( uri.LastIndexOf( '/' ) + 1 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToCreateAnIndexWithEmptyName()
		 public virtual void ShouldNotBeAbleToCreateAnIndexWithEmptyName()
		 {
			  URI node = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );

			  IDictionary<string, string> createRel = new Dictionary<string, string>();
			  createRel["to"] = node.ToString();
			  createRel["type"] = "knows";
			  URI rel = ( URI ) _service.createRelationship( _helper.createNode(), JsonHelper.createJsonFrom(createRel) ).Metadata.getFirst("Location");

			  IDictionary<string, string> indexPostBody = new Dictionary<string, string>();
			  indexPostBody["key"] = "mykey";
			  indexPostBody["value"] = "myvalue";

			  indexPostBody["uri"] = node.ToString();
			  Response response = _service.addToNodeIndex( "", "", "", JsonHelper.createJsonFrom( indexPostBody ) );
			  assertEquals( "http bad request when trying to create an index with empty name", 400, response.Status );

			  indexPostBody["uri"] = rel.ToString();
			  response = _service.addToRelationshipIndex( "", "", "", JsonHelper.createJsonFrom( indexPostBody ) );
			  assertEquals( "http bad request when trying to create an index with empty name", 400, response.Status );

			  IDictionary<string, string> basicIndexCreation = new Dictionary<string, string>();
			  basicIndexCreation["name"] = "";

			  response = _service.jsonCreateNodeIndex( JsonHelper.createJsonFrom( basicIndexCreation ) );
			  assertEquals( "http bad request when trying to create an index with empty name", 400, response.Status );

			  response = _service.jsonCreateRelationshipIndex( JsonHelper.createJsonFrom( basicIndexCreation ) );
			  assertEquals( "http bad request when trying to create an index with empty name", 400, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToIndexNodeUniquelyWithRequiredParameterMissing()
		 public virtual void ShouldNotBeAbleToIndexNodeUniquelyWithRequiredParameterMissing()
		 {
			  _service.createNode( null ).Metadata.getFirst( "Location" );
			  IDictionary<string, object> body = new Dictionary<string, object>();
			  body["key"] = "mykey";
			  body["value"] = "my/key";
			  foreach ( string key in body.Keys )
			  {
					IDictionary<string, object> postBody = new Dictionary<string, object>( body );
					postBody.Remove( key );
					Response response = _service.addToNodeIndex( "unique-nodes", "", "", JsonHelper.createJsonFrom( postBody ) );

					assertEquals( "unexpected response code with \"" + key + "\" missing.", 400, response.Status );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToIndexRelationshipUniquely()
		 public virtual void ShouldBeAbleToIndexRelationshipUniquely()
		 {
			  URI start = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  URI end = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  IDictionary<string, string> postBody = new Dictionary<string, string>();
			  postBody["key"] = "mykey";
			  postBody["value"] = "my/key";
			  postBody["start"] = start.ToString();
			  postBody["end"] = end.ToString();
			  postBody["type"] = "knows";
			  for ( int i = 0; i < 2; i++ )
			  {
					Response response = _service.addToNodeIndex( "unique-relationships", "", "", JsonHelper.createJsonFrom( postBody ) );

					assertEquals( 201 - i, response.Status );
					if ( i == 0 )
					{
						 assertNotNull( response.Metadata.getFirst( "Location" ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uniquelyIndexedRelationshipGetsTheSpecifiedKeyAndValueAsPropertiesIfNoPropertiesAreSpecified()
		 public virtual void UniquelyIndexedRelationshipGetsTheSpecifiedKeyAndValueAsPropertiesIfNoPropertiesAreSpecified()
		 {
			  const string key = "somekey";
			  string value = "somevalue";
			  URI start = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  URI end = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );

			  IDictionary<string, object> postBody = new Dictionary<string, object>();
			  postBody["key"] = key;
			  postBody["value"] = value;
			  postBody["start"] = start.ToString();
			  postBody["end"] = end.ToString();
			  postBody["type"] = "knows";

			  Response response = _service.addToRelationshipIndex( "unique-relationships", "", "", JsonHelper.createJsonFrom( postBody ) );
			  assertEquals( 201, response.Status );
			  object rel = response.Metadata.getFirst( "Location" );
			  assertNotNull( rel );
			  string uri = rel.ToString();
			  IDictionary<string, object> properties = _helper.getRelationshipProperties( parseLong( uri.Substring( uri.LastIndexOf( '/' ) + 1 ) ) );
			  assertEquals( 1, properties.Count );
			  assertEquals( value, properties[key] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void specifiedPropertiesOverrideKeyAndValueForUniquelyIndexedRelationships()
		 public virtual void SpecifiedPropertiesOverrideKeyAndValueForUniquelyIndexedRelationships()
		 {
			  const string key = "a_key";
			  string value = "a value";
			  URI start = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  URI end = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );

			  IDictionary<string, object> postBody = new Dictionary<string, object>();
			  postBody["key"] = key;
			  postBody["value"] = value;
			  postBody["start"] = start.ToString();
			  postBody["end"] = end.ToString();
			  postBody["type"] = "knows";
			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["name"] = "Jürgen";
			  properties["age"] = "42";
			  properties["occupation"] = "crazy";
			  postBody["properties"] = properties;

			  Response response = _service.addToRelationshipIndex( "unique-relationships", "", "", JsonHelper.createJsonFrom( postBody ) );
			  assertEquals( 201, response.Status );
			  object rel = response.Metadata.getFirst( "Location" );
			  assertNotNull( rel );
			  string uri = rel.ToString();
			  assertEquals( properties, _helper.getRelationshipProperties( parseLong( uri.Substring( uri.LastIndexOf( '/' ) + 1 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToIndexRelationshipUniquelyWithBothUriAndCreationalDataInPayload()
		 public virtual void ShouldNotBeAbleToIndexRelationshipUniquelyWithBothUriAndCreationalDataInPayload()
		 {
			  URI start = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  URI end = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  string path = start.Path;
			  URI rel = ( URI ) _service.createRelationship( parseLong( path.Substring( path.LastIndexOf( '/' ) + 1 ) ), "{\"to\":\"" + end + "\",\"type\":\"knows\"}" ).Metadata.getFirst( "Location" );
			  IDictionary<string, object> unwanted = new Dictionary<string, object>();
			  unwanted["properties"] = new Hashtable();
			  unwanted["start"] = start.ToString();
			  unwanted["end"] = end.ToString();
			  unwanted["type"] = "friend";
			  foreach ( KeyValuePair<string, object> bad in unwanted.SetOfKeyValuePairs() )
			  {
					IDictionary<string, object> postBody = new Dictionary<string, object>();
					postBody["key"] = "mykey";
					postBody["value"] = "my/key";
					postBody["uri"] = rel.ToString();
					postBody[bad.Key] = bad.Value;

					Response response = _service.addToRelationshipIndex( "unique-relationships", "", "", JsonHelper.createJsonFrom( postBody ) );
					assertEquals( "unexpected response code with \"" + bad.Key + "\".", 400, response.Status );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToIndexRelationshipUniquelyWithRequiredParameterMissing()
		 public virtual void ShouldNotBeAbleToIndexRelationshipUniquelyWithRequiredParameterMissing()
		 {
			  URI start = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  URI end = ( URI ) _service.createNode( null ).Metadata.getFirst( "Location" );
			  IDictionary<string, object> body = new Dictionary<string, object>();
			  body["key"] = "mykey";
			  body["value"] = "my/key";
			  body["start"] = start.ToString();
			  body["end"] = end.ToString();
			  body["type"] = "knows";
			  foreach ( string key in body.Keys )
			  {
					IDictionary<string, object> postBody = new Dictionary<string, object>( body );
					postBody.Remove( key );
					Response response = _service.addToRelationshipIndex( "unique-relationships", "", "", JsonHelper.createJsonFrom( postBody ) );

					assertEquals( "unexpected response code with \"" + key + "\" missing.", 400, response.Status );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveNodeIndex()
		 public virtual void ShouldBeAbleToRemoveNodeIndex()
		 {
			  string indexName = "myFancyIndex";

			  int numberOfAutoIndexesWhichCouldNotBeDeletedAtTestSetup = _helper.NodeIndexes.Length;

			  _helper.createNodeIndex( indexName );
			  _helper.createNodeIndex( "another one" );

			  assertEquals( numberOfAutoIndexesWhichCouldNotBeDeletedAtTestSetup + 2, _helper.NodeIndexes.Length );

			  Response response = _service.deleteNodeIndex( indexName );

			  assertEquals( 204, response.Status );
			  assertEquals( numberOfAutoIndexesWhichCouldNotBeDeletedAtTestSetup + 1, _helper.NodeIndexes.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveRelationshipIndex()
		 public virtual void ShouldBeAbleToRemoveRelationshipIndex()
		 {
			  string indexName = "myFancyIndex";

			  assertEquals( 0, _helper.RelationshipIndexes.Length );

			  _helper.createRelationshipIndex( indexName );

			  assertEquals( 1, _helper.RelationshipIndexes.Length );

			  Response response = _service.deleteRelationshipIndex( indexName );

			  assertEquals( 204, response.Status );
			  assertEquals( 0, _helper.RelationshipIndexes.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetNodeRepresentationFromIndexUri() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetNodeRepresentationFromIndexUri()
		 {
			  string key = "key_get_noderep";
			  string value = "value";
			  long nodeId = _helper.createNode();
			  string indexName = "all-the-best-nodes";
			  _helper.addNodeToIndex( indexName, key, value, nodeId );
			  Response response = _service.getNodeFromIndexUri( indexName, key, value, nodeId );
			  assertEquals( 200, response.Status );

			  CheckContentTypeCharsetUtf8( response );
			  assertNull( response.Metadata.get( "Location" ) );
			  IDictionary<string, object> map = JsonHelper.jsonToMap( IEntityAsString( response ) );
			  assertNotNull( map );
			  assertTrue( map.ContainsKey( "self" ) );
		 }

		 private void CheckContentTypeCharsetUtf8( Response response )
		 {
			  assertTrue( response.Metadata.getFirst( HttpHeaders.CONTENT_TYPE ).ToString().Contains("UTF-8") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetRelationshipRepresentationFromIndexUri() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetRelationshipRepresentationFromIndexUri()
		 {
			  string key = "key_get_noderep";
			  string value = "value";
			  long startNodeId = _helper.createNode();
			  long endNodeId = _helper.createNode();
			  string relationshipType = "knows";
			  long relationshipId = _helper.createRelationship( relationshipType, startNodeId, endNodeId );

			  string indexName = "all-the-best-relationships";
			  _helper.addRelationshipToIndex( indexName, key, value, relationshipId );
			  Response response = _service.getRelationshipFromIndexUri( indexName, key, value, relationshipId );
			  assertEquals( 200, response.Status );
			  CheckContentTypeCharsetUtf8( response );

			  assertNull( response.Metadata.get( "Location" ) );
			  IDictionary<string, object> map = JsonHelper.jsonToMap( IEntityAsString( response ) );
			  assertNotNull( map );
			  assertTrue( map.ContainsKey( "self" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetListOfNodeRepresentationsFromIndexLookup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetListOfNodeRepresentationsFromIndexLookup()
		 {
			  ModelBuilder.DomainModel matrixers = ModelBuilder.GenerateMatrix( _service );

			  KeyValuePair<string, string> indexedKeyValue = matrixers.IndexedNodeKeyValues.SetOfKeyValuePairs().GetEnumerator().next();
			  Response response = _service.getIndexedNodes( matrixers.NodeIndexName, indexedKeyValue.Key, indexedKeyValue.Value );
			  assertEquals( Response.Status.OK.StatusCode, response.Status );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> items = (java.util.Collection<?>) org.Neo4Net.server.rest.domain.JsonHelper.readJson(entityAsString(response));
			  ICollection<object> items = ( ICollection<object> ) JsonHelper.readJson( IEntityAsString( response ) );
			  int counter = 0;
			  foreach ( object item in items )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> map = (java.util.Map<?, ?>) item;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) item;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> properties = (java.util.Map<?, ?>) map.get("data");
					IDictionary<object, ?> properties = ( IDictionary<object, ?> ) map["data"];
					assertNotNull( map["self"] );
					string indexedUri = ( string ) map["indexed"];
					assertEquals( matrixers.IndexedNodeUriToEntityMap[new URI( indexedUri )].properties.get( "name" ), properties["name"] );
					counter++;
			  }
			  assertEquals( 2, counter );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetListOfNodeRepresentationsFromIndexQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetListOfNodeRepresentationsFromIndexQuery()
		 {
			  ModelBuilder.DomainModel matrixers = ModelBuilder.GenerateMatrix( _service );

			  KeyValuePair<string, string> indexedKeyValue = matrixers.IndexedNodeKeyValues.SetOfKeyValuePairs().GetEnumerator().next();
			  // query for the first letter with which the nodes were indexed.
			  Response response = _service.getIndexedNodesByQuery( matrixers.NodeIndexName, indexedKeyValue.Key + ":" + indexedKeyValue.Value.substring( 0, 1 ) + "*", "" );
			  assertEquals( Response.Status.OK.StatusCode, response.Status );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> items = (java.util.Collection<?>) org.Neo4Net.server.rest.domain.JsonHelper.readJson(entityAsString(response));
			  ICollection<object> items = ( ICollection<object> ) JsonHelper.readJson( IEntityAsString( response ) );
			  int counter = 0;
			  foreach ( object item in items )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> map = (java.util.Map<?, ?>) item;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) item;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> properties = (java.util.Map<?, ?>) map.get("data");
					IDictionary<object, ?> properties = ( IDictionary<object, ?> ) map["data"];
					string indexedUri = ( string ) map["indexed"]; // unlike exact
					// match, a query
					// can not return
					// a sensible
					// index uri for
					// the result
					assertNull( indexedUri );
					string selfUri = ( string ) map["self"];
					assertNotNull( selfUri );
					assertEquals( matrixers.NodeUriToEntityMap[new URI( selfUri )].properties.get( "name" ), properties["name"] );
					counter++;
			  }
			  assertThat( counter, @is( greaterThanOrEqualTo( 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetListOfNodeRepresentationsFromIndexQueryWithDefaultKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetListOfNodeRepresentationsFromIndexQueryWithDefaultKey()
		 {
			  ModelBuilder.DomainModel matrixers = ModelBuilder.GenerateMatrix( _service );

			  KeyValuePair<string, string> indexedKeyValue = matrixers.IndexedNodeKeyValues.SetOfKeyValuePairs().GetEnumerator().next();
			  // query for the first letter with which the nodes were indexed.
			  Response response = _service.getIndexedNodesByQuery( matrixers.NodeIndexName, indexedKeyValue.Key, indexedKeyValue.Value.substring( 0, 1 ) + "*", "" );
			  assertEquals( Response.Status.OK.StatusCode, response.Status );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> items = (java.util.Collection<?>) org.Neo4Net.server.rest.domain.JsonHelper.readJson(entityAsString(response));
			  ICollection<object> items = ( ICollection<object> ) JsonHelper.readJson( IEntityAsString( response ) );
			  int counter = 0;
			  foreach ( object item in items )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> map = (java.util.Map<?, ?>) item;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) item;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> properties = (java.util.Map<?, ?>) map.get("data");
					IDictionary<object, ?> properties = ( IDictionary<object, ?> ) map["data"];
					string indexedUri = ( string ) map["indexed"]; // unlike exact
					// match, a query
					// can not return
					// a sensible
					// index uri for
					// the result
					assertNull( indexedUri );
					string selfUri = ( string ) map["self"];
					assertNotNull( selfUri );
					assertEquals( matrixers.NodeUriToEntityMap[new URI( selfUri )].properties.get( "name" ), properties["name"] );
					counter++;
			  }
			  assertThat( counter, @is( greaterThanOrEqualTo( 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetListOfRelationshipRepresentationsFromIndexLookup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetListOfRelationshipRepresentationsFromIndexLookup()
		 {
			  string key = "key_get";
			  string value = "value";

			  long startNodeId = _helper.createNode();
			  long endNodeId = _helper.createNode();

			  string relationshipType1 = "KNOWS";
			  long relationshipId1 = _helper.createRelationship( relationshipType1, startNodeId, endNodeId );
			  string relationshipType2 = "PLAYS-NICE-WITH";
			  long relationshipId2 = _helper.createRelationship( relationshipType2, startNodeId, endNodeId );

			  string indexName = "matrixal-relationships";
			  _helper.createRelationshipIndex( indexName );
			  _helper.addRelationshipToIndex( indexName, key, value, relationshipId1 );
			  _helper.addRelationshipToIndex( indexName, key, value, relationshipId2 );

			  Response response = _service.getIndexedRelationships( indexName, key, value );
			  assertEquals( Response.Status.OK.StatusCode, response.Status );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> items = (java.util.Collection<?>) org.Neo4Net.server.rest.domain.JsonHelper.readJson(entityAsString(response));
			  ICollection<object> items = ( ICollection<object> ) JsonHelper.readJson( IEntityAsString( response ) );
			  int counter = 0;
			  foreach ( object item in items )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> map = (java.util.Map<?, ?>) item;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) item;
					assertNotNull( map["self"] );
					string indexedUri = ( string ) map["indexed"];
					assertThat( indexedUri, containsString( key ) );
					assertThat( indexedUri, containsString( value ) );
					assertTrue( indexedUri.EndsWith( Convert.ToString( relationshipId1 ), StringComparison.Ordinal ) || indexedUri.EndsWith( Convert.ToString( relationshipId2 ), StringComparison.Ordinal ) );
					counter++;
			  }
			  assertEquals( 2, counter );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetListOfRelationshipRepresentationsFromIndexQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetListOfRelationshipRepresentationsFromIndexQuery()
		 {
			  string key = "key_get";
			  string value = "value";

			  long startNodeId = _helper.createNode();
			  long endNodeId = _helper.createNode();

			  string relationshipType1 = "KNOWS";
			  long relationshipId1 = _helper.createRelationship( relationshipType1, startNodeId, endNodeId );
			  string relationshipType2 = "PLAYS-NICE-WITH";
			  long relationshipId2 = _helper.createRelationship( relationshipType2, startNodeId, endNodeId );

			  string indexName = "matrixal-relationships";
			  _helper.createRelationshipIndex( indexName );
			  _helper.addRelationshipToIndex( indexName, key, value, relationshipId1 );
			  _helper.addRelationshipToIndex( indexName, key, value, relationshipId2 );

			  Response response = _service.getIndexedRelationshipsByQuery( indexName, key + ":" + value.Substring( 0, 1 ) + "*", "" );
			  assertEquals( Response.Status.OK.StatusCode, response.Status );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> items = (java.util.Collection<?>) org.Neo4Net.server.rest.domain.JsonHelper.readJson(entityAsString(response));
			  ICollection<object> items = ( ICollection<object> ) JsonHelper.readJson( IEntityAsString( response ) );
			  int counter = 0;
			  foreach ( object item in items )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> map = (java.util.Map<?, ?>) item;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) item;
					string indexedUri = ( string ) map["indexed"];
					assertNull( indexedUri ); // queries can not return a sensible index
					// uri
					string selfUri = ( string ) map["self"];
					assertNotNull( selfUri );
					assertTrue( selfUri.EndsWith( Convert.ToString( relationshipId1 ), StringComparison.Ordinal ) || selfUri.EndsWith( Convert.ToString( relationshipId2 ), StringComparison.Ordinal ) );
					counter++;
			  }
			  assertThat( counter, @is( greaterThanOrEqualTo( 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetListOfRelationshipRepresentationsFromIndexQueryWithDefaultKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetListOfRelationshipRepresentationsFromIndexQueryWithDefaultKey()
		 {
			  string key = "key_get";
			  string value = "value";

			  long startNodeId = _helper.createNode();
			  long endNodeId = _helper.createNode();

			  string relationshipType1 = "KNOWS";
			  long relationshipId1 = _helper.createRelationship( relationshipType1, startNodeId, endNodeId );
			  string relationshipType2 = "PLAYS-NICE-WITH";
			  long relationshipId2 = _helper.createRelationship( relationshipType2, startNodeId, endNodeId );

			  string indexName = "matrixal-relationships";
			  _helper.createRelationshipIndex( indexName );
			  _helper.addRelationshipToIndex( indexName, key, value, relationshipId1 );
			  _helper.addRelationshipToIndex( indexName, key, value, relationshipId2 );

			  Response response = _service.getIndexedRelationshipsByQuery( indexName, key, value.Substring( 0, 1 ) + "*", "" );
			  assertEquals( Response.Status.OK.StatusCode, response.Status );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> items = (java.util.Collection<?>) org.Neo4Net.server.rest.domain.JsonHelper.readJson(entityAsString(response));
			  ICollection<object> items = ( ICollection<object> ) JsonHelper.readJson( IEntityAsString( response ) );
			  int counter = 0;
			  foreach ( object item in items )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> map = (java.util.Map<?, ?>) item;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) item;
					string indexedUri = ( string ) map["indexed"];
					assertNull( indexedUri ); // queries can not return a sensible index
					// uri
					string selfUri = ( string ) map["self"];
					assertNotNull( selfUri );
					assertTrue( selfUri.EndsWith( Convert.ToString( relationshipId1 ), StringComparison.Ordinal ) || selfUri.EndsWith( Convert.ToString( relationshipId2 ), StringComparison.Ordinal ) );
					counter++;
			  }
			  assertThat( counter, @is( greaterThanOrEqualTo( 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200AndEmptyListWhenNothingFoundInIndexLookup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet200AndEmptyListWhenNothingFoundInIndexLookup()
		 {
			  string indexName = "nothing-in-this-index";
			  _helper.createNodeIndex( indexName );
			  Response response = _service.getIndexedNodes( indexName, "fooo", "baaar" );
			  assertEquals( Response.Status.OK.StatusCode, response.Status );

			  CheckContentTypeCharsetUtf8( response );

			  string IEntity = IEntityAsString( response );
			  object parsedJson = JsonHelper.readJson( IEntity );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: assertTrue(parsedJson instanceof java.util.Collection<?>);
			  assertTrue( parsedJson is ICollection<object> );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: assertTrue(((java.util.Collection<?>) parsedJson).isEmpty());
			  assertTrue( ( ( ICollection<object> ) parsedJson ).Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveNodeFromIndex()
		 public virtual void ShouldBeAbleToRemoveNodeFromIndex()
		 {
			  long nodeId = _helper.createNode();
			  string key = "key_remove";
			  string value = "value";
			  _helper.addNodeToIndex( "node", key, value, nodeId );
			  assertEquals( 1, _helper.getIndexedNodes( "node", key, value ).Count );
			  Response response = _service.deleteFromNodeIndex( "node", key, value, nodeId );
			  assertEquals( Response.Status.NO_CONTENT.StatusCode, response.Status );
			  assertEquals( 0, _helper.getIndexedNodes( "node", key, value ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveRelationshipFromIndex()
		 public virtual void ShouldBeAbleToRemoveRelationshipFromIndex()
		 {
			  long startNodeId = _helper.createNode();
			  long endNodeId = _helper.createNode();
			  string relationshipType = "related-to";
			  long relationshipId = _helper.createRelationship( relationshipType, startNodeId, endNodeId );
			  string key = "key_remove";
			  string value = "value";
			  string indexName = "relationships";
			  _helper.addRelationshipToIndex( indexName, key, value, relationshipId );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key, value ).Count );
			  Response response = _service.deleteFromRelationshipIndex( indexName, key, value, relationshipId );
			  assertEquals( Response.Status.NO_CONTENT.StatusCode, response.Status );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key, value ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404IfRemovingNonExistentNodeIndexing()
		 public virtual void ShouldGet404IfRemovingNonExistentNodeIndexing()
		 {
			  Response response = _service.deleteFromNodeIndex( "nodes", "bogus", "bogus", 999999 );
			  assertEquals( Response.Status.NOT_FOUND.StatusCode, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404IfRemovingNonExistentRelationshipIndexing()
		 public virtual void ShouldGet404IfRemovingNonExistentRelationshipIndexing()
		 {
			  Response response = _service.deleteFromRelationshipIndex( "relationships", "bogus", "bogus", 999999 );
			  assertEquals( Response.Status.NOT_FOUND.StatusCode, response.Status );
		 }

		 private static string MarkWithUnicodeMarker( string @string )
		 {
			  return ( ( char ) 0xfeff ).ToString() + @string;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToFindSinglePathBetweenTwoNodes()
		 public virtual void ShouldBeAbleToFindSinglePathBetweenTwoNodes()
		 {
			  long n1 = _helper.createNode();
			  long n2 = _helper.createNode();
			  _helper.createRelationship( "knows", n1, n2 );
			  IDictionary<string, object> config = MapUtil.map( "max depth", 3, "algorithm", "shortestPath", "to", Convert.ToString( n2 ), "relationships", MapUtil.map( "type", "knows", "direction", "out" ) );
			  string payload = JsonHelper.createJsonFrom( config );

			  Response response = _service.singlePath( n1, payload );

			  assertThat( response.Status, @is( 200 ) );
			  using ( Transaction ignored = _graph.beginTx() )
			  {
					IDictionary<string, object> resultAsMap = _output.ResultAsMap;
					assertThat( resultAsMap["length"], @is( 1 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToFindSinglePathBetweenTwoNodesEvenWhenAskingForAllPaths()
		 public virtual void ShouldBeAbleToFindSinglePathBetweenTwoNodesEvenWhenAskingForAllPaths()
		 {
			  long n1 = _helper.createNode();
			  long n2 = _helper.createNode();
			  _helper.createRelationship( "knows", n1, n2 );
			  IDictionary<string, object> config = MapUtil.map( "max depth", 3, "algorithm", "shortestPath", "to", Convert.ToString( n2 ), "relationships", MapUtil.map( "type", "knows", "direction", "out" ) );
			  string payload = JsonHelper.createJsonFrom( config );

			  Response response = _service.allPaths( n1, payload );

			  assertThat( response.Status, @is( 200 ) );
			  using ( Transaction ignored = _graph.beginTx() )
			  {
					IList<object> resultAsList = _output.ResultAsList;
					assertThat( resultAsList.Count, @is( 1 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToParseJsonEvenWithUnicodeMarkerAtTheStart()
		 public virtual void ShouldBeAbleToParseJsonEvenWithUnicodeMarkerAtTheStart()
		 {
			  Response response = _service.createNode( MarkWithUnicodeMarker( "{\"name\":\"Mattias\"}" ) );
			  assertEquals( Response.Status.CREATED.StatusCode, response.Status );
			  string nodeLocation = response.Metadata.getFirst( HttpHeaders.LOCATION ).ToString();

			  long node = _helper.createNode();
			  assertEquals( Response.Status.NO_CONTENT.StatusCode, _service.setNodeProperty( node, "foo", MarkWithUnicodeMarker( "\"bar\"" ) ).Status );
			  assertEquals( Response.Status.NO_CONTENT.StatusCode, _service.setNodeProperty( node, "foo", MarkWithUnicodeMarker( "" + 10 ) ).Status );
			  assertEquals( Response.Status.NO_CONTENT.StatusCode, _service.setAllNodeProperties( node, MarkWithUnicodeMarker( "{\"name\":\"Something\"," + "\"number\":10}" ) ).Status );

			  assertEquals( Response.Status.CREATED.StatusCode, _service.createRelationship( node, MarkWithUnicodeMarker( "{\"to\":\"" + nodeLocation + "\",\"type\":\"knows\"}" ) ).Status );

			  long relationship = _helper.createRelationship( "knows" );
			  assertEquals( Response.Status.NO_CONTENT.StatusCode, _service.setRelationshipProperty( relationship, "foo", MarkWithUnicodeMarker( "\"bar\"" ) ).Status );
			  assertEquals( Response.Status.NO_CONTENT.StatusCode, _service.setRelationshipProperty( relationship, "foo", MarkWithUnicodeMarker( "" + 10 ) ).Status );
			  assertEquals( Response.Status.NO_CONTENT.StatusCode, _service.setAllRelationshipProperties( relationship, MarkWithUnicodeMarker( "{\"name\":\"Something\",\"number\":10}" ) ).Status );

			  assertEquals( Response.Status.CREATED.StatusCode, _service.addToNodeIndex( "node", null, null, MarkWithUnicodeMarker( "{\"key\":\"foo\", \"value\":\"bar\", \"uri\": \"" + nodeLocation + "\"}" ) ).Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdvertiseUriForQueringAllRelationsInTheDatabase()
		 public virtual void ShouldAdvertiseUriForQueringAllRelationsInTheDatabase()
		 {
			  Response response = _service.Root;
			  assertThat( StringHelper.NewString( ( sbyte[] ) response.Entity ), containsString( "\"relationship_types\" : \"http://Neo4Net.org/relationship/types\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeAutoIndexerEnabling()
		 public virtual void NodeAutoIndexerEnabling()
		 {
			  TestAutoIndexEnableForType( "node" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipAutoIndexerEnabling()
		 public virtual void RelationshipAutoIndexerEnabling()
		 {
			  TestAutoIndexEnableForType( "relationship" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addRemoveAutoindexPropertiesOnNodes() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddRemoveAutoindexPropertiesOnNodes()
		 {
			  AddRemoveAutoindexProperties( "node" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addRemoveAutoindexPropertiesOnRelationships() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddRemoveAutoindexPropertiesOnRelationships()
		 {
			  AddRemoveAutoindexProperties( "relationship" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeAutoindexingSupposedToWork()
		 public virtual void NodeAutoindexingSupposedToWork()
		 {
			  string type = "node";
			  Response response = _service.startAutoIndexingProperty( type, "myAutoIndexedProperty" );
			  assertEquals( 204, response.Status );

			  response = _service.setAutoIndexerEnabled( type, "true" );
			  assertEquals( 204, response.Status );

			  _service.createNode( "{\"myAutoIndexedProperty\" : \"value\"}" );

			  using ( Transaction ignored = _graph.beginTx() )
			  {
					IndexHits<Node> indexResult = _database.Graph.index().NodeAutoIndexer.AutoIndex.get("myAutoIndexedProperty", "value");
					assertEquals( 1, indexResult.Size() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnAllLabelsPresentInTheDatabase() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnAllLabelsPresentInTheDatabase()
		 {
			  // given
			  _helper.createNode( Label.label( "ALIVE" ) );
			  long nodeId = _helper.createNode( Label.label( "DEAD" ) );
			  _helper.deleteNode( nodeId );

			  // when
			  Response response = _service.getAllLabels( false );

			  // then
			  assertEquals( 200, response.Status );

			  IList<string> labels = IEntityAsList( response );
			  assertThat( labels, hasItem( "DEAD" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnAllLabelsInUseInTheDatabase() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnAllLabelsInUseInTheDatabase()
		 {
			  // given
			  _helper.createNode( Label.label( "ALIVE" ) );
			  long nodeId = _helper.createNode( Label.label( "DEAD" ) );
			  _helper.deleteNode( nodeId );

			  // when
			  Response response = _service.getAllLabels( true );

			  // then
			  assertEquals( 200, response.Status );

			  IList<string> labels = IEntityAsList( response );
			  assertThat( labels, not( hasItem( "DEAD" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private void addRemoveAutoindexProperties(String type) throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void AddRemoveAutoindexProperties( string type )
		 {
			  Response response = _service.getAutoIndexedProperties( type );
			  assertEquals( 200, response.Status );
			  string IEntity = IEntityAsString( response );
			  IList<string> properties = ( IList<string> ) JsonHelper.readJson( IEntity );
			  assertEquals( 0, properties.Count );

			  response = _service.startAutoIndexingProperty( type, "myAutoIndexedProperty1" );
			  assertEquals( 204, response.Status );

			  response = _service.startAutoIndexingProperty( type, "myAutoIndexedProperty2" );
			  assertEquals( 204, response.Status );

			  response = _service.getAutoIndexedProperties( type );
			  assertEquals( 200, response.Status );
			  IEntity = IEntityAsString( response );
			  properties = ( IList<string> ) JsonHelper.readJson( IEntity );
			  assertEquals( 2, properties.Count );
			  assertTrue( properties.Contains( "myAutoIndexedProperty1" ) );
			  assertTrue( properties.Contains( "myAutoIndexedProperty2" ) );

			  response = _service.stopAutoIndexingProperty( type, "myAutoIndexedProperty2" );
			  assertEquals( 204, response.Status );

			  response = _service.getAutoIndexedProperties( type );
			  assertEquals( 200, response.Status );
			  IEntity = IEntityAsString( response );
			  properties = ( IList<string> ) JsonHelper.readJson( IEntity );
			  assertEquals( 1, properties.Count );
			  assertTrue( properties.Contains( "myAutoIndexedProperty1" ) );
		 }
		 private void TestAutoIndexEnableForType( string type )
		 {
			  Response response = _service.isAutoIndexerEnabled( type );
			  assertEquals( 200, response.Status );
			  assertFalse( bool.Parse( IEntityAsString( response ) ) );

			  response = _service.setAutoIndexerEnabled( type, "true" );
			  assertEquals( 204, response.Status );

			  response = _service.isAutoIndexerEnabled( type );
			  assertEquals( 200, response.Status );
			  assertTrue( bool.Parse( IEntityAsString( response ) ) );

			  response = _service.setAutoIndexerEnabled( type, "false" );
			  assertEquals( 204, response.Status );

			  response = _service.isAutoIndexerEnabled( type );
			  assertEquals( 200, response.Status );
			  assertFalse( bool.Parse( IEntityAsString( response ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private String singleErrorCode(javax.ws.rs.core.Response response) throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private string SingleErrorCode( Response response )
		 {
			  string json = IEntityAsString( response );
			  IDictionary<string, object> map = JsonHelper.jsonToMap( json );
			  IList<object> errors = ( IList<object> ) map["errors"];
			  assertEquals( 1, errors.Count );
			  IDictionary<string, string> error = ( IDictionary<string, string> ) errors[0];
			  return error["code"];
		 }
	}

}