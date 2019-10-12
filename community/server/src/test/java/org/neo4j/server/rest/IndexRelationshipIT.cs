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
namespace Org.Neo4j.Server.rest
{
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Org.Neo4j.Function;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using ResponseEntity = Org.Neo4j.Server.rest.RESTRequestGenerator.ResponseEntity;
	using GraphDbHelper = Org.Neo4j.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using JsonParseException = Org.Neo4j.Server.rest.domain.JsonParseException;
	using URIHelper = Org.Neo4j.Server.rest.domain.URIHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.FunctionalTestHelper.CLIENT;

	public class IndexRelationshipIT : AbstractRestFunctionalTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;
		 private static RestRequest _request;

		 private enum MyRelationshipTypes
		 {
			  Knows
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
			  _request = RestRequest.Req();
		 }

		 /// <summary>
		 /// POST ${org.neo4j.server.rest.web}/index/relationship {
		 /// "name":"index-name" "config":{ // optional map of index configuration
		 /// params "key1":"value1", "key2":"value2" } }
		 /// 
		 /// POST ${org.neo4j.server.rest.web}/index/relationship/{indexName}/{key}/{
		 /// value} "http://uri.for.node.to.index"
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateANamedRelationshipIndexAndAddToIt() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateANamedRelationshipIndexAndAddToIt()
		 {
			  string indexName = _indexes.newInstance();
			  int expectedIndexes = _helper.RelationshipIndexes.Length + 1;
			  IDictionary<string, string> indexSpecification = new Dictionary<string, string>();
			  indexSpecification["name"] = indexName;
			  JaxRsResponse response = HttpPostIndexRelationshipRoot( JsonHelper.createJsonFrom( indexSpecification ) );
			  assertEquals( 201, response.Status );
			  assertNotNull( response.Headers.get( "Location" ).get( 0 ) );
			  assertEquals( expectedIndexes, _helper.RelationshipIndexes.Length );
			  assertNotNull( _helper.createRelationshipIndex( indexName ) );
			  // Add a relationship to the index
			  string key = "key";
			  string value = "value";
			  string relationshipType = "related-to";
			  long relationshipId = _helper.createRelationship( relationshipType );
			  response = HttpPostIndexRelationshipNameKeyValue( indexName, relationshipId, key, value );
			  assertEquals( Status.CREATED.StatusCode, response.Status );
			  string indexUri = response.Headers.get( "Location" ).get( 0 );
			  assertNotNull( indexUri );
			  assertEquals( Arrays.asList( ( long? ) relationshipId ), _helper.getIndexedRelationships( indexName, key, value ) );
			  // Get the relationship from the indexed URI (Location in header)
			  response = HttpGet( indexUri );
			  assertEquals( 200, response.Status );
			  string discoveredEntity = response.Entity;
			  IDictionary<string, object> map = JsonHelper.jsonToMap( discoveredEntity );
			  assertNotNull( map["self"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenRequestingIndexUriWhichDoesntExist()
		 public virtual void ShouldGet404WhenRequestingIndexUriWhichDoesntExist()
		 {
			  string key = "key3";
			  string value = "value";
			  string indexName = _indexes.newInstance();
			  string indexUri = _functionalTestHelper.relationshipIndexUri() + indexName + "/" + key + "/" + value;
			  JaxRsResponse response = HttpGet( indexUri );
			  assertEquals( Status.NOT_FOUND.StatusCode, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenDeletingNonExistentIndex()
		 public virtual void ShouldGet404WhenDeletingNonExistentIndex()
		 {
			  string indexName = _indexes.newInstance();
			  string indexUri = _functionalTestHelper.relationshipIndexUri() + indexName;
			  JaxRsResponse response = _request.delete( indexUri );
			  assertEquals( Status.NOT_FOUND.StatusCode, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200AndArrayOfRelationshipRepsWhenGettingFromIndex() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet200AndArrayOfRelationshipRepsWhenGettingFromIndex()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long startNode = helper.createNode();
			  long startNode = _helper.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long endNode = helper.createNode();
			  long endNode = _helper.createNode();
			  const string key = "key_get";
			  const string value = "value";
			  const string relationshipName1 = "related-to";
			  const string relationshipName2 = "dislikes";
			  string jsonString = JsonRelationshipCreationSpecification( relationshipName1, endNode, key, value );
			  JaxRsResponse createRelationshipResponse = HttpPostCreateRelationship( startNode, jsonString );
			  assertEquals( 201, createRelationshipResponse.Status );
			  string relationshipLocation1 = createRelationshipResponse.Location.ToString();
			  jsonString = JsonRelationshipCreationSpecification( relationshipName2, endNode, key, value );
			  createRelationshipResponse = HttpPostCreateRelationship( startNode, jsonString );
			  assertEquals( 201, createRelationshipResponse.Status );
			  string relationshipLocation2 = createRelationshipResponse.Headers.get( HttpHeaders.LOCATION ).get( 0 );
			  string indexName = _indexes.newInstance();
			  JaxRsResponse indexCreationResponse = HttpPostIndexRelationshipRoot( "{\"name\":\"" + indexName + "\"}" );
			  assertEquals( 201, indexCreationResponse.Status );
			  JaxRsResponse indexedRelationshipResponse = HttpPostIndexRelationshipNameKeyValue( indexName, _functionalTestHelper.getRelationshipIdFromUri( relationshipLocation1 ), key, value );
			  string indexLocation1 = indexedRelationshipResponse.Headers.get( HttpHeaders.LOCATION ).get( 0 );
			  indexedRelationshipResponse = HttpPostIndexRelationshipNameKeyValue( indexName, _functionalTestHelper.getRelationshipIdFromUri( relationshipLocation2 ), key, value );
			  string indexLocation2 = indexedRelationshipResponse.Headers.get( HttpHeaders.LOCATION ).get( 0 );
			  IDictionary<string, string> uriToName = new Dictionary<string, string>();
			  uriToName[indexLocation1] = relationshipName1;
			  uriToName[indexLocation2] = relationshipName2;
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexRelationshipUri(indexName, key, value));
			  assertEquals( 200, response.Status );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> items = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(response.getEntity());
			  ICollection<object> items = ( ICollection<object> ) JsonHelper.readJson( response.Entity );
			  int counter = 0;
			  foreach ( object item in items )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> map = (java.util.Map<?, ?>) item;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) item;
					assertNotNull( map["self"] );
					string indexedUri = ( string ) map["indexed"];
					assertEquals( uriToName[indexedUri], map["type"] );
					counter++;
			  }
			  assertEquals( 2, counter );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200WhenGettingRelationshipFromIndexWithNoHits()
		 public virtual void ShouldGet200WhenGettingRelationshipFromIndexWithNoHits()
		 {
			  string indexName = _indexes.newInstance();
			  _helper.createRelationshipIndex( indexName );
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexRelationshipUri(indexName, "non-existent-key", "non-existent-value"));
			  assertEquals( 200, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200WhenQueryingIndex()
		 public virtual void ShouldGet200WhenQueryingIndex()
		 {
			  string indexName = _indexes.newInstance();
			  string key = "bobsKey";
			  string value = "bobsValue";
			  long relationship = _helper.createRelationship( "TYPE" );
			  _helper.addRelationshipToIndex( indexName, key, value, relationship );
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexRelationshipUri(indexName) + "?query=" + key + ":" + value);
			  assertEquals( 200, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveIndexing()
		 public virtual void ShouldBeAbleToRemoveIndexing()
		 {
			  string key1 = "kvkey1";
			  string key2 = "kvkey2";
			  string value1 = "value1";
			  string value2 = "value2";
			  string indexName = _indexes.newInstance();
			  long relationship = _helper.createRelationship( "some type" );
			  _helper.setRelationshipProperties( relationship, MapUtil.map( key1, value1, key1, value2, key2, value1, key2, value2 ) );
			  _helper.addRelationshipToIndex( indexName, key1, value1, relationship );
			  _helper.addRelationshipToIndex( indexName, key1, value2, relationship );
			  _helper.addRelationshipToIndex( indexName, key2, value1, relationship );
			  _helper.addRelationshipToIndex( indexName, key2, value2, relationship );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key1, value1 ).Count );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key1, value2 ).Count );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key2, value1 ).Count );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key2, value2 ).Count );
			  JaxRsResponse response = RestRequest.Req().delete(_functionalTestHelper.relationshipIndexUri() + indexName + "/" + key1 + "/" + value1 + "/" + relationship);
			  assertEquals( 204, response.Status );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key1, value1 ).Count );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key1, value2 ).Count );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key2, value1 ).Count );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key2, value2 ).Count );
			  response = RestRequest.Req().delete(_functionalTestHelper.relationshipIndexUri() + indexName + "/" + key2 + "/" + relationship);
			  assertEquals( 204, response.Status );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key1, value1 ).Count );
			  assertEquals( 1, _helper.getIndexedRelationships( indexName, key1, value2 ).Count );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key2, value1 ).Count );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key2, value2 ).Count );
			  response = RestRequest.Req().delete(_functionalTestHelper.relationshipIndexUri() + indexName + "/" + relationship);
			  assertEquals( 204, response.Status );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key1, value1 ).Count );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key1, value2 ).Count );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key2, value1 ).Count );
			  assertEquals( 0, _helper.getIndexedRelationships( indexName, key2, value2 ).Count );
			  // Delete the index
			  response = RestRequest.Req().delete(_functionalTestHelper.indexRelationshipUri(indexName));
			  assertEquals( 204, response.Status );
			  assertFalse( asList( _helper.RelationshipIndexes ).contains( indexName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToIndexValuesContainingSpaces() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToIndexValuesContainingSpaces()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long startNodeId = helper.createNode();
			  long startNodeId = _helper.createNode();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long endNodeId = helper.createNode();
			  long endNodeId = _helper.createNode();
			  const string relationshiptype = "tested-together";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long relationshipId = helper.createRelationship(relationshiptype, startNodeId, endNodeId);
			  long relationshipId = _helper.createRelationship( relationshiptype, startNodeId, endNodeId );
			  const string key = "key";
			  const string value = "value with   spaces  in it";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  _helper.createRelationshipIndex( indexName );
			  JaxRsResponse response = HttpPostIndexRelationshipNameKeyValue( indexName, relationshipId, key, value );
			  assertEquals( Status.CREATED.StatusCode, response.Status );
			  URI location = response.Location;
			  response.Close();
			  response = HttpGetIndexRelationshipNameKeyValue( indexName, key, URIHelper.encode( value ) );
			  assertEquals( Status.OK.StatusCode, response.Status );
			  string responseEntity = response.Entity;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(responseEntity);
			  ICollection<object> hits = ( ICollection<object> ) JsonHelper.readJson( responseEntity );
			  assertEquals( 1, hits.Count );
			  response.Close();
			  CLIENT.resource( location ).delete();
			  response = HttpGetIndexRelationshipNameKeyValue( indexName, key, URIHelper.encode( value ) );
			  assertEquals( 200, response.Status );
			  responseEntity = response.Entity;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(responseEntity);
			  hits = ( ICollection<object> ) JsonHelper.readJson( responseEntity );
			  assertEquals( 0, hits.Count );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenSendingCorruptJson()
		 public virtual void ShouldRespondWith400WhenSendingCorruptJson()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  _helper.createRelationshipIndex( indexName );
			  const string corruptJson = "{[}";
			  JaxRsResponse response = RestRequest.Req().post(_functionalTestHelper.indexRelationshipUri(indexName), corruptJson);
			  assertEquals( 400, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get or create unique relationship (create).\n" + "\n" + "Create a unique relationship in an index.\n" + "If a relationship matching the given key and value already exists in the index, it will be returned.\n" + "If not, a new relationship will be created.\n" + "\n" + "NOTE: The type and direction of the relationship is not regarded when determining uniqueness.") @Test public void get_or_create_relationship()
		 [Documented("Get or create unique relationship (create).\n" + "\n" + "Create a unique relationship in an index.\n" + "If a relationship matching the given key and value already exists in the index, it will be returned.\n" + "If not, a new relationship will be created.\n" + "\n" + "NOTE: The type and direction of the relationship is not regarded when determining uniqueness.")]
		 public virtual void GetOrCreateRelationship()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string type = "knowledge";
			  string key = "name";
			  string value = "Tobias";
			  _helper.createRelationshipIndex( index );
			  long start = _helper.createNode();
			  long end = _helper.createNode();
			  GenConflict.get().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\":\"" + value + "\", \"start\": \"" + _functionalTestHelper.nodeUri(start) + "\", \"end\": \"" + _functionalTestHelper.nodeUri(end) + "\", \"type\": \"" + type + "\"}").post(_functionalTestHelper.relationshipIndexUri() + index + "/?uniqueness=get_or_create");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get or create unique relationship (existing).\n" + "\n" + "Here, in case\n" + "of an already existing relationship, the sent data is ignored and the\n" + "existing relationship returned.") @Test public void get_or_create_unique_relationship_existing()
		 [Documented("Get or create unique relationship (existing).\n" + "\n" + "Here, in case\n" + "of an already existing relationship, the sent data is ignored and the\n" + "existing relationship returned.")]
		 public virtual void GetOrCreateUniqueRelationshipExisting()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";
			  GraphDatabaseService graphdb = graphdb();
			  _helper.createRelationshipIndex( index );
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node node1 = graphdb.CreateNode();
					Node node2 = graphdb.CreateNode();
					Relationship rel = node1.CreateRelationshipTo( node2, MyRelationshipTypes.Knows );
					graphdb.Index().forRelationships(index).add(rel, key, value);
					tx.Success();
			  }
			  GenConflict.get().expectedStatus(200).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"start\": \"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\", \"end\": \"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\", \"type\":\"" + MyRelationshipTypes.Knows + "\"}").post(_functionalTestHelper.relationshipIndexUri() + index + "?uniqueness=get_or_create");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Create a unique relationship or return fail (create).\n" + "\n" + "Here, in case\n" + "of an already existing relationship, an error should be returned. In this\n" + "example, no existing relationship is found and a new relationship is created.") @Test public void create_a_unique_relationship_or_return_fail___create() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Create a unique relationship or return fail (create).\n" + "\n" + "Here, in case\n" + "of an already existing relationship, an error should be returned. In this\n" + "example, no existing relationship is found and a new relationship is created.")]
		 public virtual void CreateAUniqueRelationshipOrReturnFail__Create()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Tobias";
			  _helper.createRelationshipIndex( index );
			  ResponseEntity response = GenConflict.get().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"start\": \"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\", \"end\": \"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\", \"type\":\"" + MyRelationshipTypes.Knows + "\"}").post(_functionalTestHelper.relationshipIndexUri() + index + "?uniqueness=create_or_fail");
			  MultivaluedMap<string, string> headers = response.Response().Headers;
			  IDictionary<string, object> result = JsonHelper.jsonToMap( response.Entity() );
			  assertEquals( result["indexed"], headers.getFirst( "Location" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Create a unique relationship or return fail (fail).\n" + "\n" + "Here, in case\n" + "of an already existing relationship, an error should be returned. In this\n" + "example, an existing relationship is found and an error is returned.") @Test public void create_a_unique_relationship_or_return_fail___fail()
		 [Documented("Create a unique relationship or return fail (fail).\n" + "\n" + "Here, in case\n" + "of an already existing relationship, an error should be returned. In this\n" + "example, an existing relationship is found and an error is returned.")]
		 public virtual void CreateAUniqueRelationshipOrReturnFail__Fail()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";
			  GraphDatabaseService graphdb = graphdb();
			  _helper.createRelationshipIndex( index );
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node node1 = graphdb.CreateNode();
					Node node2 = graphdb.CreateNode();
					Relationship rel = node1.CreateRelationshipTo( node2, MyRelationshipTypes.Knows );
					graphdb.Index().forRelationships(index).add(rel, key, value);
					tx.Success();
			  }
			  GenConflict.get().expectedStatus(409).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"start\": \"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\", \"end\": \"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\", \"type\":\"" + MyRelationshipTypes.Knows + "\"}").post(_functionalTestHelper.relationshipIndexUri() + index + "?uniqueness=create_or_fail");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Add an existing relationship to a unique index (not indexed).\n" + "\n" + "If a relationship matching the given key and value already exists in the index, it will be returned.\n" + "If not, an `HTTP 409` (conflict) status will be returned in this case, as we are using `create_or_fail`.\n" + "\n" + "It's possible to use `get_or_create` uniqueness as well.\n" + "\n" + "NOTE: The type and direction of the relationship is not regarded when determining uniqueness.") @Test public void put_relationship_or_fail_if_absent()
		 [Documented("Add an existing relationship to a unique index (not indexed).\n" + "\n" + "If a relationship matching the given key and value already exists in the index, it will be returned.\n" + "If not, an `HTTP 409` (conflict) status will be returned in this case, as we are using `create_or_fail`.\n" + "\n" + "It's possible to use `get_or_create` uniqueness as well.\n" + "\n" + "NOTE: The type and direction of the relationship is not regarded when determining uniqueness.")]
		 public virtual void PutRelationshipOrFailIfAbsent()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";
			  _helper.createRelationshipIndex( index );
			  GenConflict.get().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"uri\":\"" + _functionalTestHelper.relationshipUri(_helper.createRelationship("KNOWS", _helper.createNode(), _helper.createNode())) + "\"}").post(_functionalTestHelper.relationshipIndexUri() + index + "?uniqueness=create_or_fail");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Add an existing relationship to a unique index (already indexed).") @Test public void put_relationship_if_absent_only_fail()
		 [Documented("Add an existing relationship to a unique index (already indexed).")]
		 public virtual void PutRelationshipIfAbsentOnlyFail()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";
			  GraphDatabaseService graphdb = graphdb();
			  _helper.createRelationshipIndex( index );
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node node1 = graphdb.CreateNode();
					Node node2 = graphdb.CreateNode();
					Relationship rel = node1.CreateRelationshipTo( node2, MyRelationshipTypes.Knows );
					graphdb.Index().forRelationships(index).add(rel, key, value);
					tx.Success();
			  }

			  Relationship rel;
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node node1 = graphdb.CreateNode();
					Node node2 = graphdb.CreateNode();
					rel = node1.CreateRelationshipTo( node2, MyRelationshipTypes.Knows );
					tx.Success();
			  }

			  // When & Then
			  GenConflict.get().expectedStatus(409).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"uri\":\"" + _functionalTestHelper.relationshipUri(rel.Id) + "\"}").post(_functionalTestHelper.relationshipIndexUri() + index + "?uniqueness=create_or_fail");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void already_indexed_relationship_should_not_fail_on_create_or_fail()
		 public virtual void AlreadyIndexedRelationshipShouldNotFailOnCreateOrFail()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";
			  GraphDatabaseService graphdb = graphdb();
			  _helper.createRelationshipIndex( index );
			  Relationship rel;
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node node1 = graphdb.CreateNode();
					Node node2 = graphdb.CreateNode();
					rel = node1.CreateRelationshipTo( node2, MyRelationshipTypes.Knows );
					graphdb.Index().forRelationships(index).add(rel, key, value);
					tx.Success();
			  }

			  // When & Then
			  GenConflict.get().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"uri\":\"" + _functionalTestHelper.relationshipUri(rel.Id) + "\"}").post(_functionalTestHelper.relationshipIndexUri() + index + "?uniqueness=create_or_fail");
		 }

		 /// <summary>
		 /// This can be safely removed in version 1.11 an onwards.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createUniqueShouldBeBackwardsCompatibleWith1_8()
		 public virtual void CreateUniqueShouldBeBackwardsCompatibleWith1_8()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";
			  GraphDatabaseService graphdb = graphdb();
			  _helper.createRelationshipIndex( index );
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node node1 = graphdb.CreateNode();
					Node node2 = graphdb.CreateNode();
					Relationship rel = node1.CreateRelationshipTo( node2, MyRelationshipTypes.Knows );
					graphdb.Index().forRelationships(index).add(rel, key, value);
					tx.Success();
			  }
			  GenConflict.get().expectedStatus(200).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"start\": \"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\", \"end\": \"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\", \"type\":\"" + MyRelationshipTypes.Knows + "\"}").post(_functionalTestHelper.relationshipIndexUri() + index + "?unique");
		 }

		 private JaxRsResponse HttpPostIndexRelationshipRoot( string jsonIndexSpecification )
		 {
			  return RestRequest.Req().post(_functionalTestHelper.relationshipIndexUri(), jsonIndexSpecification);
		 }

		 private JaxRsResponse HttpGetIndexRelationshipNameKeyValue( string indexName, string key, string value )
		 {
			  return RestRequest.Req().get(_functionalTestHelper.indexRelationshipUri(indexName, key, value));
		 }

		 private JaxRsResponse HttpPostIndexRelationshipNameKeyValue( string indexName, long relationshipId, string key, string value )
		 {
			  return RestRequest.Req().post(_functionalTestHelper.indexRelationshipUri(indexName), CreateJsonStringFor(relationshipId, key, value));
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String createJsonStringFor(final long relationshipId, final String key, final String value)
		 private string CreateJsonStringFor( long relationshipId, string key, string value )
		 {
			  return "{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"uri\": \""
						+ _functionalTestHelper.relationshipUri( relationshipId ) + "\"}";
		 }

		 private JaxRsResponse HttpGet( string indexUri )
		 {
			  return _request.get( indexUri );
		 }

		 private JaxRsResponse HttpPostCreateRelationship( long startNode, string jsonString )
		 {
			  return RestRequest.Req().post(_functionalTestHelper.dataUri() + "node/" + startNode + "/relationships", jsonString);
		 }

		 private string JsonRelationshipCreationSpecification( string relationshipName, long endNode, string key, string value )
		 {
			  return "{\"to\" : \"" + _functionalTestHelper.dataUri() + "node/" + endNode + "\"," + "\"type\" : \""
						+ relationshipName + "\", " + "\"data\" : {\"" + key + "\" : \"" + value + "\"}}";
		 }

		 private readonly Factory<string> _indexes = UniqueStrings.WithPrefix( "index" );
	}

}