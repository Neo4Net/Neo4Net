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
namespace Neo4Net.Server.rest
{
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Neo4Net.Functions;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using ResponseEntity = Neo4Net.Server.rest.RESTRequestGenerator.ResponseEntity;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using URIHelper = Neo4Net.Server.rest.domain.URIHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.FunctionalTestHelper.CLIENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class IndexNodeIT : AbstractRestFunctionalTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("List node indexes.") @Test public void shouldGetListOfNodeIndexesWhenOneExist() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("List node indexes.")]
		 public virtual void ShouldGetListOfNodeIndexesWhenOneExist()
		 {
			  string indexName = _indexes.newInstance();
			  _helper.createNodeIndex( indexName );
			  string entity = Gen().expectedStatus(200).get(_functionalTestHelper.nodeIndexUri()).entity();

			  IDictionary<string, object> map = JsonHelper.jsonToMap( entity );
			  assertNotNull( map[indexName] );

			  Dictionary<string, object> theIndex = new Dictionary<string, object>();
			  theIndex[indexName] = map[indexName];

			  assertEquals( "Was: " + theIndex + ", no-auto-index:" + _functionalTestHelper.removeAnyAutoIndex( theIndex ), 1, _functionalTestHelper.removeAnyAutoIndex( theIndex ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Create node index\n" + "\n" + "NOTE: Instead of creating the index this way, you can simply start to use\n" + "it, and it will be created automatically with default configuration.") @Test public void shouldCreateANamedNodeIndex()
		 [Documented("Create node index\n" + "\n" + "NOTE: Instead of creating the index this way, you can simply start to use\n" + "it, and it will be created automatically with default configuration.")]
		 public virtual void ShouldCreateANamedNodeIndex()
		 {
			  string indexName = _indexes.newInstance();
			  int expectedIndexes = _helper.NodeIndexes.Length + 1;
			  IDictionary<string, string> indexSpecification = new Dictionary<string, string>();
			  indexSpecification["name"] = indexName;

			  Gen().payload(JsonHelper.createJsonFrom(indexSpecification)).expectedStatus(201).expectedHeader("Location").post(_functionalTestHelper.nodeIndexUri());

			  assertEquals( expectedIndexes, _helper.NodeIndexes.Length );
			  assertThat( _helper.NodeIndexes, FunctionalTestHelper.arrayContains( indexName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateANamedNodeIndexWithSpaces()
		 public virtual void ShouldCreateANamedNodeIndexWithSpaces()
		 {
			  string indexName = _indexes.newInstance() + " with spaces";
			  int expectedIndexes = _helper.NodeIndexes.Length + 1;
			  IDictionary<string, string> indexSpecification = new Dictionary<string, string>();
			  indexSpecification["name"] = indexName;

			  Gen().payload(JsonHelper.createJsonFrom(indexSpecification)).expectedStatus(201).expectedHeader("Location").post(_functionalTestHelper.nodeIndexUri());

			  assertEquals( expectedIndexes, _helper.NodeIndexes.Length );
			  assertThat( _helper.NodeIndexes, FunctionalTestHelper.arrayContains( indexName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Create node index with configuration.\n\n" + "This request is only necessary if you want to customize the index settings. \n" + "If you are happy with the defaults, you can just start indexing nodes/relationships, as\n" + "non-existent indexes will automatically be created as you do. See\n" + "<<indexing-create-advanced>> for more information on index configuration.") @Test public void shouldCreateANamedNodeIndexWithConfiguration()
		 [Documented("Create node index with configuration.\n\n" + "This request is only necessary if you want to customize the index settings. \n" + "If you are happy with the defaults, you can just start indexing nodes/relationships, as\n" + "non-existent indexes will automatically be created as you do. See\n" + "<<indexing-create-advanced>> for more information on index configuration.")]
		 public virtual void ShouldCreateANamedNodeIndexWithConfiguration()
		 {
			  int expectedIndexes = _helper.NodeIndexes.Length + 1;

			  Gen().payload("{\"name\":\"fulltext\", \"config\":{\"type\":\"fulltext\",\"provider\":\"lucene\"}}").expectedStatus(201).expectedHeader("Location").post(_functionalTestHelper.nodeIndexUri());

			  assertEquals( expectedIndexes, _helper.NodeIndexes.Length );
			  assertThat( _helper.NodeIndexes, FunctionalTestHelper.arrayContains( "fulltext" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Add node to index.\n" + "\n" + "Associates a node with the given key/value pair in the given index.\n" + "\n" + "NOTE: Spaces in the URI have to be encoded as +%20+.\n" + "\n" + "CAUTION: This does *not* overwrite previous entries. If you index the\n" + "same key/value/item combination twice, two index entries are created. To\n" + "do update-type operations, you need to delete the old entry before adding\n" + "a new one.") @Test public void shouldAddToIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Add node to index.\n" + "\n" + "Associates a node with the given key/value pair in the given index.\n" + "\n" + "NOTE: Spaces in the URI have to be encoded as +%20+.\n" + "\n" + "CAUTION: This does *not* overwrite previous entries. If you index the\n" + "same key/value/item combination twice, two index entries are created. To\n" + "do update-type operations, you need to delete the old entry before adding\n" + "a new one.")]
		 public virtual void ShouldAddToIndex()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  const string key = "some-key";
			  const string value = "some value";
			  long nodeId = CreateNode();
			  // implicitly create the index
			  Gen().expectedStatus(201).payload(JsonHelper.createJsonFrom(GenerateNodeIndexCreationPayload(key, value, _functionalTestHelper.nodeUri(nodeId)))).post(_functionalTestHelper.indexNodeUri(indexName));
			  // look if we get one entry back
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexNodeUri(indexName, key, URIHelper.encode(value)));
			  string entity = response.Entity;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(entity);
			  ICollection<object> hits = ( ICollection<object> ) JsonHelper.readJson( entity );
			  assertEquals( 1, hits.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Find node by exact match.\n" + "\n" + "NOTE: Spaces in the URI have to be encoded as +%20+.") @Test public void shouldAddToIndexAndRetrieveItByExactMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Find node by exact match.\n" + "\n" + "NOTE: Spaces in the URI have to be encoded as +%20+.")]
		 public virtual void ShouldAddToIndexAndRetrieveItByExactMatch()
		 {
			  string indexName = _indexes.newInstance();
			  string key = "key";
			  string value = "the value";
			  long nodeId = CreateNode();
			  value = URIHelper.encode( value );
			  // implicitly create the index
			  JaxRsResponse response = RestRequest.Req().post(_functionalTestHelper.indexNodeUri(indexName), CreateJsonStringFor(nodeId, key, value));
			  assertEquals( 201, response.Status );

			  // search it exact
			  string entity = Gen().expectedStatus(200).get(_functionalTestHelper.indexNodeUri(indexName, key, URIHelper.encode(value))).entity();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(entity);
			  ICollection<object> hits = ( ICollection<object> ) JsonHelper.readJson( entity );
			  assertEquals( 1, hits.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Find node by query.\n" + "\n" + "The query language used here depends on what type of index you are\n" + "querying. The default index type is Lucene, in which case you should use\n" + "the Lucene query language here. Below an example of a fuzzy search over\n" + "multiple keys.\n" + "\n" + "See: {lucene-base-uri}/queryparser/org/apache/lucene/queryparser/classic/package-summary.html\n" + "\n" + "Getting the results with a predefined ordering requires adding the\n" + "parameter\n" + "\n" + "`order=ordering`\n" + "\n" + "where ordering is one of index, relevance or score. In this case an\n" + "additional field will be added to each result, named score, that holds\n" + "the float value that is the score reported by the query result.") @Test public void shouldAddToIndexAndRetrieveItByQuery() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Find node by query.\n" + "\n" + "The query language used here depends on what type of index you are\n" + "querying. The default index type is Lucene, in which case you should use\n" + "the Lucene query language here. Below an example of a fuzzy search over\n" + "multiple keys.\n" + "\n" + "See: {lucene-base-uri}/queryparser/org/apache/lucene/queryparser/classic/package-summary.html\n" + "\n" + "Getting the results with a predefined ordering requires adding the\n" + "parameter\n" + "\n" + "`order=ordering`\n" + "\n" + "where ordering is one of index, relevance or score. In this case an\n" + "additional field will be added to each result, named score, that holds\n" + "the float value that is the score reported by the query result.")]
		 public virtual void ShouldAddToIndexAndRetrieveItByQuery()
		 {
			  string indexName = _indexes.newInstance();
			  string key = "Name";
			  string value = "Builder";
			  long node = _helper.createNode( MapUtil.map( key, value ) );
			  _helper.addNodeToIndex( indexName, key, value, node );
			  _helper.addNodeToIndex( indexName, "Gender", "Male", node );

			  string entity = Gen().expectedStatus(200).get(_functionalTestHelper.indexNodeUri(indexName) + "?query=" + key + ":Build~0.1%20AND%20Gender:Male").entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(entity);
			  ICollection<object> hits = ( ICollection<object> ) JsonHelper.readJson( entity );
			  assertEquals( 1, hits.Count );
			  LinkedHashMap<string, string> nodeMap = ( LinkedHashMap ) hits.GetEnumerator().next();
			  assertNull( "score should not be present when not explicitly ordering", nodeMap.get( "score" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void orderedResultsAreSupersetOfUnordered() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OrderedResultsAreSupersetOfUnordered()
		 {
			  // Given
			  string indexName = _indexes.newInstance();
			  string key = "Name";
			  string value = "Builder";
			  long node = _helper.createNode( MapUtil.map( key, value ) );
			  _helper.addNodeToIndex( indexName, key, value, node );
			  _helper.addNodeToIndex( indexName, "Gender", "Male", node );

			  string entity = Gen().expectedStatus(200).get(_functionalTestHelper.indexNodeUri(indexName) + "?query=" + key + ":Build~0.1%20AND%20Gender:Male").entity();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Collection<java.util.LinkedHashMap<String, String>> hits = (java.util.Collection<java.util.LinkedHashMap<String, String>>) org.neo4j.server.rest.domain.JsonHelper.readJson(entity);
			  ICollection<LinkedHashMap<string, string>> hits = ( ICollection<LinkedHashMap<string, string>> ) JsonHelper.readJson( entity );
			  LinkedHashMap<string, string> nodeMapUnordered = hits.GetEnumerator().next();

			  // When
			  entity = Gen().expectedStatus(200).get(_functionalTestHelper.indexNodeUri(indexName) + "?query=" + key + ":Build~0.1%20AND%20Gender:Male&order=score").entity();

			  //noinspection unchecked
			  hits = ( ICollection<LinkedHashMap<string, string>> ) JsonHelper.readJson( entity );
			  LinkedHashMap<string, string> nodeMapOrdered = hits.GetEnumerator().next();

			  // Then
			  foreach ( KeyValuePair<string, string> unorderedEntry in nodeMapUnordered.entrySet() )
			  {
					assertEquals( "wrong entry for key: " + unorderedEntry.Key, unorderedEntry.Value, nodeMapOrdered.get( unorderedEntry.Key ) );
			  }
			  assertEquals( "There should be only one extra value for the ordered map", nodeMapOrdered.size(), nodeMapUnordered.size() + 1 );
		 }

		 //TODO:add compatibility tests for old syntax
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddToIndexAndRetrieveItByQuerySorted() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddToIndexAndRetrieveItByQuerySorted()
		 {
			  string indexName = _indexes.newInstance();
			  string key = "Name";
			  long node1 = _helper.createNode();
			  long node2 = _helper.createNode();

			  _helper.addNodeToIndex( indexName, key, "Builder2", node1 );
			  _helper.addNodeToIndex( indexName, "Gender", "Male", node1 );
			  _helper.addNodeToIndex( indexName, key, "Builder", node2 );
			  _helper.addNodeToIndex( indexName, "Gender", "Male", node2 );

			  string entity = Gen().expectedStatus(200).get(_functionalTestHelper.indexNodeUri(indexName) + "?query=" + key + ":Builder~%20AND%20Gender:Male&order=relevance").entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(entity);
			  ICollection<object> hits = ( ICollection<object> ) JsonHelper.readJson( entity );
			  assertEquals( 2, hits.Count );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Iterator<java.util.LinkedHashMap<String, Object>> it = (java.util.Iterator<java.util.LinkedHashMap<String, Object>>) hits.iterator();
			  IEnumerator<LinkedHashMap<string, object>> it = ( IEnumerator<LinkedHashMap<string, object>> ) hits.GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  LinkedHashMap<string, object> node2Map = it.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  LinkedHashMap<string, object> node1Map = it.next();
			  float score2 = ( ( double? ) node2Map.get( "score" ) ).Value;
			  float score1 = ( ( double? ) node1Map.get( "score" ) ).Value;
			  assertTrue( "results returned in wrong order for relevance ordering", ( ( string ) node2Map.get( "self" ) ).EndsWith( Convert.ToString( node2 ), StringComparison.Ordinal ) );
			  assertTrue( "results returned in wrong order for relevance ordering", ( ( string ) node1Map.get( "self" ) ).EndsWith( Convert.ToString( node1 ), StringComparison.Ordinal ) );
			  /*
			   * scores are always the same, just the ordering changes. So all subsequent tests will
			   * check the same condition.
			   */
			  assertTrue( "scores are reversed", score2 > score1 );

			  entity = Gen().expectedStatus(200).get(_functionalTestHelper.indexNodeUri(indexName) + "?query=" + key + ":Builder~%20AND%20Gender:Male&order=index").entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(entity);
			  hits = ( ICollection<object> ) JsonHelper.readJson( entity );
			  assertEquals( 2, hits.Count );
			  //noinspection unchecked
			  it = ( IEnumerator<LinkedHashMap<string, object>> ) hits.GetEnumerator();

			  /*
			   * index order, so as they were added
			   */
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  node1Map = it.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  node2Map = it.next();
			  score1 = ( ( double? ) node1Map.get( "score" ) ).Value;
			  score2 = ( ( double? ) node2Map.get( "score" ) ).Value;
			  assertTrue( "results returned in wrong order for index ordering", ( ( string ) node1Map.get( "self" ) ).EndsWith( Convert.ToString( node1 ), StringComparison.Ordinal ) );
			  assertTrue( "results returned in wrong order for index ordering", ( ( string ) node2Map.get( "self" ) ).EndsWith( Convert.ToString( node2 ), StringComparison.Ordinal ) );
			  assertTrue( "scores are reversed", score2 > score1 );

			  entity = Gen().expectedStatus(200).get(_functionalTestHelper.indexNodeUri(indexName) + "?query=" + key + ":Builder~%20AND%20Gender:Male&order=score").entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(entity);
			  hits = ( ICollection<object> ) JsonHelper.readJson( entity );
			  assertEquals( 2, hits.Count );
			  //noinspection unchecked
			  it = ( IEnumerator<LinkedHashMap<string, object>> ) hits.GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  node2Map = it.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  node1Map = it.next();
			  score2 = ( ( double? ) node2Map.get( "score" ) ).Value;
			  score1 = ( ( double? ) node1Map.get( "score" ) ).Value;
			  assertTrue( "results returned in wrong order for score ordering", ( ( string ) node2Map.get( "self" ) ).EndsWith( Convert.ToString( node2 ), StringComparison.Ordinal ) );
			  assertTrue( "results returned in wrong order for score ordering", ( ( string ) node1Map.get( "self" ) ).EndsWith( Convert.ToString( node1 ), StringComparison.Ordinal ) );
			  assertTrue( "scores are reversed", score2 > score1 );
		 }

		 /// <summary>
		 /// POST ${org.neo4j.server.rest.web}/index/node/{indexName}/{key}/{value}
		 /// "http://uri.for.node.to.index"
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith201CreatedWhenIndexingJsonNodeUri()
		 public virtual void ShouldRespondWith201CreatedWhenIndexingJsonNodeUri()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId = helper.createNode();
			  long nodeId = _helper.createNode();
			  const string key = "key";
			  const string value = "value";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  _helper.createNodeIndex( indexName );

			  JaxRsResponse response = RestRequest.Req().post(_functionalTestHelper.indexNodeUri(indexName), CreateJsonStringFor(nodeId, key, value));
			  assertEquals( 201, response.Status );
			  assertNotNull( response.Headers.getFirst( "Location" ) );
			  assertEquals( singletonList( nodeId ), _helper.getIndexedNodes( indexName, key, value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNodeRepresentationFromIndexUri() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetNodeRepresentationFromIndexUri()
		 {
			  long nodeId = _helper.createNode();
			  string key = "key2";
			  string value = "value";

			  string indexName = _indexes.newInstance();
			  _helper.createNodeIndex( indexName );
			  JaxRsResponse response = RestRequest.Req().post(_functionalTestHelper.indexNodeUri(indexName), CreateJsonStringFor(nodeId, key, value));

			  assertEquals( Status.CREATED.StatusCode, response.Status );
			  string indexUri = response.Headers.getFirst( "Location" );

			  response = RestRequest.Req().get(indexUri);
			  assertEquals( 200, response.Status );

			  string entity = response.Entity;

			  IDictionary<string, object> map = JsonHelper.jsonToMap( entity );
			  assertNotNull( map["self"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenRequestingIndexUriWhichDoesntExist()
		 public virtual void ShouldGet404WhenRequestingIndexUriWhichDoesntExist()
		 {
			  string key = "key3";
			  string value = "value";
			  string indexName = _indexes.newInstance();
			  string indexUri = _functionalTestHelper.nodeIndexUri() + indexName + "/" + key + "/" + value;
			  JaxRsResponse response = RestRequest.Req().get(indexUri);
			  assertEquals( Status.NOT_FOUND.StatusCode, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404WhenDeletingNonExistentIndex()
		 public virtual void ShouldGet404WhenDeletingNonExistentIndex()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  string indexUri = _functionalTestHelper.nodeIndexUri() + indexName;
			  JaxRsResponse response = RestRequest.Req().delete(indexUri);
			  assertEquals( Status.NOT_FOUND.StatusCode, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200AndArrayOfNodeRepsWhenGettingFromIndex() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGet200AndArrayOfNodeRepsWhenGettingFromIndex()
		 {
			  string key = "myKey";
			  string value = "myValue";

			  string name1 = "Thomas Anderson";
			  string name2 = "Agent Smith";

			  string indexName = _indexes.newInstance();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RestRequest request = RestRequest.req();
			  RestRequest request = RestRequest.Req();
			  JaxRsResponse responseToPost = request.Post( _functionalTestHelper.nodeUri(), "{\"name\":\"" + name1 + "\"}" );
			  assertEquals( 201, responseToPost.Status );
			  string location1 = responseToPost.Headers.getFirst( HttpHeaders.LOCATION );
			  responseToPost.Close();
			  responseToPost = request.Post( _functionalTestHelper.nodeUri(), "{\"name\":\"" + name2 + "\"}" );
			  assertEquals( 201, responseToPost.Status );
			  string location2 = responseToPost.Headers.getFirst( HttpHeaders.LOCATION );
			  responseToPost.Close();
			  responseToPost = request.Post( _functionalTestHelper.indexNodeUri( indexName ), CreateJsonStringFor( _functionalTestHelper.getNodeIdFromUri( location1 ), key, value ) );
			  assertEquals( 201, responseToPost.Status );
			  string indexLocation1 = responseToPost.Headers.getFirst( HttpHeaders.LOCATION );
			  responseToPost.Close();
			  responseToPost = request.Post( _functionalTestHelper.indexNodeUri( indexName ), CreateJsonStringFor( _functionalTestHelper.getNodeIdFromUri( location2 ), key, value ) );
			  assertEquals( 201, responseToPost.Status );
			  string indexLocation2 = responseToPost.Headers.getFirst( HttpHeaders.LOCATION );
			  IDictionary<string, string> uriToName = new Dictionary<string, string>();
			  uriToName[indexLocation1] = name1;
			  uriToName[indexLocation2] = name2;
			  responseToPost.Close();

			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexNodeUri(indexName, key, value));
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
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> properties = (java.util.Map<?, ?>) map.get("data");
					IDictionary<object, ?> properties = ( IDictionary<object, ?> ) map["data"];
					assertNotNull( map["self"] );
					string indexedUri = ( string ) map["indexed"];
					assertEquals( uriToName[indexedUri], properties["name"] );
					counter++;
			  }
			  assertEquals( 2, counter );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200WhenGettingNodesFromIndexWithNoHits()
		 public virtual void ShouldGet200WhenGettingNodesFromIndexWithNoHits()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  _helper.createNodeIndex( indexName );
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexNodeUri(indexName, "non-existent-key", "non-existent-value"));
			  assertEquals( 200, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Delete node index.") @Test public void shouldReturn204WhenRemovingNodeIndexes()
		 [Documented("Delete node index.")]
		 public virtual void ShouldReturn204WhenRemovingNodeIndexes()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  _helper.createNodeIndex( indexName );

			  Gen().expectedStatus(204).delete(_functionalTestHelper.indexNodeUri(indexName));
		 }

		 //
		 // REMOVING ENTRIES
		 //

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Remove all entries with a given node from an index.") @Test public void shouldBeAbleToRemoveIndexingById()
		 [Documented("Remove all entries with a given node from an index.")]
		 public virtual void ShouldBeAbleToRemoveIndexingById()
		 {
			  string key1 = "kvkey1";
			  string key2 = "kvkey2";
			  string value1 = "value1";
			  string value2 = "value2";
			  string indexName = _indexes.newInstance();
			  long node = _helper.createNode( MapUtil.map( key1, value1, key1, value2, key2, value1, key2, value2 ) );
			  _helper.addNodeToIndex( indexName, key1, value1, node );
			  _helper.addNodeToIndex( indexName, key1, value2, node );
			  _helper.addNodeToIndex( indexName, key2, value1, node );
			  _helper.addNodeToIndex( indexName, key2, value2, node );

			  Gen().expectedStatus(204).delete(_functionalTestHelper.indexNodeUri(indexName) + "/" + node);

			  assertEquals( 0, _helper.getIndexedNodes( indexName, key1, value1 ).Count );
			  assertEquals( 0, _helper.getIndexedNodes( indexName, key1, value2 ).Count );
			  assertEquals( 0, _helper.getIndexedNodes( indexName, key2, value1 ).Count );
			  assertEquals( 0, _helper.getIndexedNodes( indexName, key2, value2 ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Remove all entries with a given node and key from an index.") @Test public void shouldBeAbleToRemoveIndexingByIdAndKey()
		 [Documented("Remove all entries with a given node and key from an index.")]
		 public virtual void ShouldBeAbleToRemoveIndexingByIdAndKey()
		 {
			  string key1 = "kvkey1";
			  string key2 = "kvkey2";
			  string value1 = "value1";
			  string value2 = "value2";
			  string indexName = _indexes.newInstance();
			  long node = _helper.createNode( MapUtil.map( key1, value1, key1, value2, key2, value1, key2, value2 ) );
			  _helper.addNodeToIndex( indexName, key1, value1, node );
			  _helper.addNodeToIndex( indexName, key1, value2, node );
			  _helper.addNodeToIndex( indexName, key2, value1, node );
			  _helper.addNodeToIndex( indexName, key2, value2, node );

			  Gen().expectedStatus(204).delete(_functionalTestHelper.nodeIndexUri() + indexName + "/" + key2 + "/" + node);

			  assertEquals( 1, _helper.getIndexedNodes( indexName, key1, value1 ).Count );
			  assertEquals( 1, _helper.getIndexedNodes( indexName, key1, value2 ).Count );
			  assertEquals( 0, _helper.getIndexedNodes( indexName, key2, value1 ).Count );
			  assertEquals( 0, _helper.getIndexedNodes( indexName, key2, value2 ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Remove all entries with a given node, key and value from an index.") @Test public void shouldBeAbleToRemoveIndexingByIdAndKeyAndValue()
		 [Documented("Remove all entries with a given node, key and value from an index.")]
		 public virtual void ShouldBeAbleToRemoveIndexingByIdAndKeyAndValue()
		 {
			  string key1 = "kvkey1";
			  string key2 = "kvkey2";
			  string value1 = "value1";
			  string value2 = "value2";
			  string indexName = _indexes.newInstance();
			  long node = _helper.createNode( MapUtil.map( key1, value1, key1, value2, key2, value1, key2, value2 ) );
			  _helper.addNodeToIndex( indexName, key1, value1, node );
			  _helper.addNodeToIndex( indexName, key1, value2, node );
			  _helper.addNodeToIndex( indexName, key2, value1, node );
			  _helper.addNodeToIndex( indexName, key2, value2, node );

			  Gen().expectedStatus(204).delete(_functionalTestHelper.nodeIndexUri() + indexName + "/" + key1 + "/" + value1 + "/" + node);

			  assertEquals( 0, _helper.getIndexedNodes( indexName, key1, value1 ).Count );
			  assertEquals( 1, _helper.getIndexedNodes( indexName, key1, value2 ).Count );
			  assertEquals( 1, _helper.getIndexedNodes( indexName, key2, value1 ).Count );
			  assertEquals( 1, _helper.getIndexedNodes( indexName, key2, value2 ).Count );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToIndexValuesContainingSpaces() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToIndexValuesContainingSpaces()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId = helper.createNode();
			  long nodeId = _helper.createNode();
			  const string key = "key";
			  const string value = "value with   spaces  in it";

			  string indexName = _indexes.newInstance();
			  _helper.createNodeIndex( indexName );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RestRequest request = RestRequest.req();
			  RestRequest request = RestRequest.Req();
			  JaxRsResponse response = request.Post( _functionalTestHelper.indexNodeUri( indexName ), CreateJsonStringFor( nodeId, key, value ) );

			  assertEquals( Status.CREATED.StatusCode, response.Status );
			  URI location = response.Location;
			  response.Close();
			  response = request.Get( _functionalTestHelper.indexNodeUri( indexName, key, URIHelper.encode( value ) ) );
			  assertEquals( Status.OK.StatusCode, response.Status );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(response.getEntity());
			  ICollection<object> hits = ( ICollection<object> ) JsonHelper.readJson( response.Entity );
			  assertEquals( 1, hits.Count );
			  response.Close();

			  CLIENT.resource( location ).delete();
			  response = request.Get( _functionalTestHelper.indexNodeUri( indexName, key, URIHelper.encode( value ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(response.getEntity());
			  hits = ( ICollection<object> ) JsonHelper.readJson( response.Entity );
			  assertEquals( 0, hits.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith400WhenSendingCorruptJson()
		 public virtual void ShouldRespondWith400WhenSendingCorruptJson()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  _helper.createNodeIndex( indexName );
			  const string corruptJson = "{\"key\" \"myKey\"}";
			  JaxRsResponse response = RestRequest.Req().post(_functionalTestHelper.indexNodeUri(indexName), corruptJson);
			  assertEquals( 400, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get or create unique node (create).\n" + "\n" + "The node is created if it doesn't exist in the unique index already.") @Test public void get_or_create_a_node_in_an_unique_index() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get or create unique node (create).\n" + "\n" + "The node is created if it doesn't exist in the unique index already.")]
		 public virtual void GetOrCreateANodeInAnUniqueIndex()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Tobias";
			  _helper.createNodeIndex( index );
			  ResponseEntity response = Gen().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"properties\": {\"" + key + "\": \"" + value + "\", \"sequence\": 1}}").post(_functionalTestHelper.nodeIndexUri() + index + "?uniqueness=get_or_create");

			  MultivaluedMap<string, string> headers = response.Response().Headers;
			  IDictionary<string, object> result = JsonHelper.jsonToMap( response.Entity() );
			  assertEquals( result["indexed"], headers.getFirst( "Location" ) );
			  IDictionary<string, object> data = AssertCast( typeof( System.Collections.IDictionary ), result["data"] );
			  assertEquals( value, data[key] );
			  assertEquals( 1, data["sequence"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void get_or_create_node_with_array_properties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GetOrCreateNodeWithArrayProperties()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Tobias";
			  _helper.createNodeIndex( index );
			  ResponseEntity response = Gen().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"properties\": {\"" + key + "\": \"" + value + "\", \"array\": [1,2,3]}}").post(_functionalTestHelper.nodeIndexUri() + index + "?unique");

			  MultivaluedMap<string, string> headers = response.Response().Headers;
			  IDictionary<string, object> result = JsonHelper.jsonToMap( response.Entity() );
			  string location = headers.getFirst( "Location" );
			  assertEquals( result["indexed"], location );
			  IDictionary<string, object> data = AssertCast( typeof( System.Collections.IDictionary ), result["data"] );
			  assertEquals( value, data[key] );
			  assertEquals( Arrays.asList( 1, 2, 3 ), data["array"] );
			  Node node;
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					node = Graphdb().index().forNodes(index).get(key, value).Single;
			  }
			  assertThat( node, inTx( Graphdb(), hasProperty(key).withValue(value) ) );
			  assertThat( node, inTx( Graphdb(), hasProperty("array").withValue(new int[]{ 1, 2, 3 }) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get or create unique node (existing).\n" + "\n" + "Here,\n" + "a node is not created but the existing unique node returned, since another node\n" + "is indexed with the same data already. The node data returned is then that of the\n" + "already existing node.") @Test public void get_or_create_unique_node_if_already_existing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get or create unique node (existing).\n" + "\n" + "Here,\n" + "a node is not created but the existing unique node returned, since another node\n" + "is indexed with the same data already. The node data returned is then that of the\n" + "already existing node.")]
		 public virtual void GetOrCreateUniqueNodeIfAlreadyExisting()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";

			  GraphDatabaseService graphdb = graphdb();
			  using ( Transaction tx = graphdb().BeginTx() )
			  {
					Node peter = graphdb.CreateNode();
					peter.SetProperty( key, value );
					peter.SetProperty( "sequence", 1 );
					graphdb.Index().forNodes(index).add(peter, key, value);

					tx.Success();
			  }

			  _helper.createNodeIndex( index );
			  ResponseEntity response = Gen().expectedStatus(200).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"properties\": {\"" + key + "\": \"" + value + "\", \"sequence\": 2}}").post(_functionalTestHelper.nodeIndexUri() + index + "?uniqueness=get_or_create");

			  IDictionary<string, object> result = JsonHelper.jsonToMap( response.Entity() );
			  IDictionary<string, object> data = AssertCast( typeof( System.Collections.IDictionary ), result["data"] );
			  assertEquals( value, data[key] );
			  assertEquals( 1, data["sequence"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Create a unique node or return fail (create).\n" + "\n" + "Here, in case\n" + "of an already existing node, an error should be returned. In this\n" + "example, no existing indexed node is found and a new node is created.") @Test public void create_a_unique_node_or_fail_create() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Create a unique node or return fail (create).\n" + "\n" + "Here, in case\n" + "of an already existing node, an error should be returned. In this\n" + "example, no existing indexed node is found and a new node is created.")]
		 public virtual void CreateAUniqueNodeOrFailCreate()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Tobias";
			  _helper.createNodeIndex( index );
			  ResponseEntity response = GenConflict.get().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"properties\": {\"" + key + "\": \"" + value + "\", \"sequence\": 1}}").post(_functionalTestHelper.nodeIndexUri() + index + "?uniqueness=create_or_fail" + "");

			  MultivaluedMap<string, string> headers = response.Response().Headers;
			  IDictionary<string, object> result = JsonHelper.jsonToMap( response.Entity() );
			  assertEquals( result["indexed"], headers.getFirst( "Location" ) );
			  IDictionary<string, object> data = AssertCast( typeof( System.Collections.IDictionary ), result["data"] );
			  assertEquals( value, data[key] );
			  assertEquals( 1, data["sequence"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Create a unique node or return fail (fail).\n" + "\n" + "Here, in case\n" + "of an already existing node, an error should be returned. In this\n" + "example, an existing node indexed with the same data\n" + "is found and an error is returned.") @Test public void create_a_unique_node_or_return_fail___fail() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Create a unique node or return fail (fail).\n" + "\n" + "Here, in case\n" + "of an already existing node, an error should be returned. In this\n" + "example, an existing node indexed with the same data\n" + "is found and an error is returned.")]
		 public virtual void CreateAUniqueNodeOrReturnFail__Fail()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";

			  GraphDatabaseService graphdb = graphdb();
			  _helper.createNodeIndex( index );

			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node peter = graphdb.CreateNode();
					peter.SetProperty( key, value );
					peter.SetProperty( "sequence", 1 );
					graphdb.Index().forNodes(index).add(peter, key, value);

					tx.Success();
			  }

			  RestRequest.Req();

			  ResponseEntity response = GenConflict.get().expectedStatus(409).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"properties\": {\"" + key + "\": \"" + value + "\", \"sequence\": 2}}").post(_functionalTestHelper.nodeIndexUri() + index + "?uniqueness=create_or_fail");

			  IDictionary<string, object> result = JsonHelper.jsonToMap( response.Entity() );
			  IDictionary<string, object> data = AssertCast( typeof( System.Collections.IDictionary ), result["data"] );
			  assertEquals( value, data[key] );
			  assertEquals( 1, data["sequence"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Add an existing node to unique index (not indexed).\n" + "\n" + "Associates a node with the given key/value pair in the given unique\n" + "index.\n" + "\n" + "In this example, we are using `create_or_fail` uniqueness.") @Test public void addExistingNodeToUniqueIndexAdded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Add an existing node to unique index (not indexed).\n" + "\n" + "Associates a node with the given key/value pair in the given unique\n" + "index.\n" + "\n" + "In this example, we are using `create_or_fail` uniqueness.")]
		 public virtual void AddExistingNodeToUniqueIndexAdded()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  const string key = "some-key";
			  const string value = "some value";
			  long nodeId = CreateNode();
			  // implicitly create the index
			  Gen().expectedStatus(201).payload(JsonHelper.createJsonFrom(GenerateNodeIndexCreationPayload(key, value, _functionalTestHelper.nodeUri(nodeId)))).post(_functionalTestHelper.indexNodeUri(indexName) + "?uniqueness=create_or_fail");
			  // look if we get one entry back
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexNodeUri(indexName, key, URIHelper.encode(value)));
			  string entity = response.Entity;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> hits = (java.util.Collection<?>) org.neo4j.server.rest.domain.JsonHelper.readJson(entity);
			  ICollection<object> hits = ( ICollection<object> ) JsonHelper.readJson( entity );
			  assertEquals( 1, hits.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Add an existing node to unique index (already indexed).\n" + "\n" + "In this case, the node already exists in the index, and thus we get a `HTTP 409` status response,\n" + "as we have set the uniqueness to `create_or_fail`.") @Test public void addExistingNodeToUniqueIndexExisting()
		 [Documented("Add an existing node to unique index (already indexed).\n" + "\n" + "In this case, the node already exists in the index, and thus we get a `HTTP 409` status response,\n" + "as we have set the uniqueness to `create_or_fail`.")]
		 public virtual void AddExistingNodeToUniqueIndexExisting()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = indexes.newInstance();
			  string indexName = _indexes.newInstance();
			  const string key = "some-key";
			  const string value = "some value";

			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					Node peter = Graphdb().createNode();
					peter.SetProperty( key, value );
					Graphdb().index().forNodes(indexName).add(peter, key, value);

					tx.Success();
			  }

			  Gen().expectedStatus(409).payload(JsonHelper.createJsonFrom(GenerateNodeIndexCreationPayload(key, value, _functionalTestHelper.nodeUri(CreateNode())))).post(_functionalTestHelper.indexNodeUri(indexName) + "?uniqueness=create_or_fail");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Backward Compatibility Test (using old syntax ?unique)\n" + "Put node if absent - Create.\n" + "\n" + "Add a node to an index unless a node already exists for the given index data. In\n" + "this case, a new node is created since nothing existing is found in the index.") @Test public void put_node_if_absent___create()
		 [Documented("Backward Compatibility Test (using old syntax ?unique)\n" + "Put node if absent - Create.\n" + "\n" + "Add a node to an index unless a node already exists for the given index data. In\n" + "this case, a new node is created since nothing existing is found in the index.")]
		 public virtual void PutNodeIfAbsent__Create()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Mattias";
			  _helper.createNodeIndex( index );
			  string uri = _functionalTestHelper.nodeIndexUri() + index + "?unique";
			  Gen().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", " + "\"uri\":\"" + _functionalTestHelper.nodeUri(_helper.createNode()) + "\"}").post(uri);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void already_indexed_node_should_not_fail_on_create_or_fail()
		 public virtual void AlreadyIndexedNodeShouldNotFailOnCreateOrFail()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String index = indexes.newInstance();
			  string index = _indexes.newInstance();
			  string key = "name";
			  string value = "Peter";
			  GraphDatabaseService graphdb = graphdb();
			  _helper.createNodeIndex( index );
			  Node node;
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					node = graphdb.CreateNode();
					graphdb.Index().forNodes(index).add(node, key, value);
					tx.Success();
			  }

			  // When & Then
			  GenConflict.get().expectedStatus(201).payloadType(MediaType.APPLICATION_JSON_TYPE).payload("{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"uri\":\"" + _functionalTestHelper.nodeUri(node.Id) + "\"}").post(_functionalTestHelper.nodeIndexUri() + index + "?uniqueness=create_or_fail");
		 }

		 private static T AssertCast<T>( Type type, object @object )
		 {
				 type = typeof( T );
			  assertTrue( type.IsInstanceOfType( @object ) );
			  return type.cast( @object );
		 }

		 private long CreateNode()
		 {
			  GraphDatabaseService graphdb = Server().Database.Graph;
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node node = graphdb.CreateNode();
					tx.Success();
					return node.Id;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String createJsonStringFor(final long nodeId, final String key, final String value)
		 private string CreateJsonStringFor( long nodeId, string key, string value )
		 {
			  return "{\"key\": \"" + key + "\", \"value\": \"" + value + "\", \"uri\": \""
						+ _functionalTestHelper.nodeUri( nodeId ) + "\"}";
		 }

		 private object GenerateNodeIndexCreationPayload( string key, string value, string nodeUri )
		 {
			  IDictionary<string, string> results = new Dictionary<string, string>();
			  results["key"] = key;
			  results["value"] = value;
			  results["uri"] = nodeUri;
			  return results;
		 }

		 private readonly Factory<string> _indexes = UniqueStrings.WithPrefix( "index" );
	}

}