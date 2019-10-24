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
namespace Neo4Net.Server.rest.streaming
{
	using ClientHandlerException = com.sun.jersey.api.client.ClientHandlerException;
	using UniformInterfaceException = com.sun.jersey.api.client.UniformInterfaceException;
	using JSONException = org.json.JSONException;
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using StreamingFormat = Neo4Net.Server.rest.repr.StreamingFormat;
	using Graph = Neo4Net.Test.GraphDescription.Graph;
	using Neo4NetMatchers = Neo4Net.Test.mockito.matcher.Neo4NetMatchers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

	public class StreamingBatchOperationIT : AbstractRestFunctionalTestBase
	{

		 /// <summary>
		 /// By specifying an extended header attribute in the HTTP request,
		 /// the server will stream the results back as soon as they are processed on the server side
		 /// instead of constructing a full response when all entities are processed.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test @Graph("Joe knows John") public void execute_multiple_operations_in_batch_streaming() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph("Joe knows John")]
		 public virtual void ExecuteMultipleOperationsInBatchStreaming()
		 {
			  long idJoe = Data.get()["Joe"].Id;
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("PUT").key("to").value("/node/" + idJoe + "/properties").key("body").@object().key("age").value(1).endObject().key("id").value(0).endObject().@object().key("method").value("GET").key("to").value("/node/" + idJoe).key("id").value(1).endObject().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().key("id").value(2).endObject().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().key("id").value(3).endObject().endArray().ToString();

			  string IEntity = GenConflict.get().expectedType(APPLICATION_JSON_TYPE).withHeader(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER,"true").payload(jsonString).expectedStatus(200).post(BatchUri()).entity();
			  IList<IDictionary<string, object>> results = JsonHelper.jsonToList( IEntity );

			  assertEquals( 4, results.Count );

			  IDictionary<string, object> putResult = results[0];
			  IDictionary<string, object> getResult = results[1];
			  IDictionary<string, object> firstPostResult = results[2];
			  IDictionary<string, object> secondPostResult = results[3];

			  // Ids should be ok
			  assertEquals( 0, putResult["id"] );
			  assertEquals( 2, firstPostResult["id"] );
			  assertEquals( 3, secondPostResult["id"] );

			  // Should contain "from"
			  assertEquals( "/node/" + idJoe + "/properties", putResult["from"] );
			  assertEquals( "/node/" + idJoe, getResult["from"] );
			  assertEquals( "/node", firstPostResult["from"] );
			  assertEquals( "/node", secondPostResult["from"] );

			  // Post should contain location
			  assertTrue( ( ( string ) firstPostResult["location"] ).Length > 0 );
			  assertTrue( ( ( string ) secondPostResult["location"] ).Length > 0 );

			  // Should have created by the first PUT request
			  IDictionary<string, object> body = ( IDictionary<string, object> ) getResult["body"];
			  assertEquals( 1, ( ( IDictionary<string, object> ) body["data"] )["age"] );

		 }

		 /// <summary>
		 /// The batch operation API allows you to refer to the URI returned from a
		 /// created resource in subsequent job descriptions, within the same batch
		 /// call.
		 /// 
		 /// Use the +{[JOB ID]}+ special syntax to inject URIs from created resources
		 /// into JSON strings in subsequent job descriptions.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void refer_to_items_created_earlier_in_the_same_batch_job_streaming() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReferToItemsCreatedEarlierInTheSameBatchJobStreaming()
		 {
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("id").value(0).key("body").@object().key("name").value("bob").endObject().endObject().@object().key("method").value("POST").key("to").value("/node").key("id").value(1).key("body").@object().key("age").value(12).endObject().endObject().@object().key("method").value("POST").key("to").value("{0}/relationships").key("id").value(3).key("body").@object().key("to").value("{1}").key("data").@object().key("since").value("2010").endObject().key("type").value("KNOWS").endObject().endObject().@object().key("method").value("POST").key("to").value("/index/relationship/my_rels").key("id").value(4).key("body").@object().key("key").value("since").key("value").value("2010").key("uri").value("{3}").endObject().endObject().endArray().ToString();

			  string IEntity = GenConflict.get().expectedType(APPLICATION_JSON_TYPE).withHeader(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").expectedStatus(200).payload(jsonString).post(BatchUri()).entity();

			  IList<IDictionary<string, object>> results = JsonHelper.jsonToList( IEntity );

			  assertEquals( 4, results.Count );

	//        String rels = gen.get()
	//                .expectedStatus( 200 ).get( getRelationshipIndexUri( "my_rels", "since", "2010")).entity();
	//        assertEquals(1, JsonHelper.jsonToList(  rels ).size());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetLocationHeadersWhenCreatingThings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetLocationHeadersWhenCreatingThings()
		 {

			  long originalNodeCount = CountNodes();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String jsonString = new org.Neo4Net.server.rest.PrettyJSON().array().object().key("method").value("POST").key("to").value("/node").key("body").object().key("age").value(1).endObject().endObject().endArray().toString();
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().endObject().endArray().ToString();

			  JaxRsResponse response = RestRequest.req().accept(APPLICATION_JSON_TYPE).header(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").post(BatchUri(), jsonString);

			  assertEquals( 200, response.Status );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String IEntity = response.getEntity();
			  string IEntity = response.Entity;
			  IList<IDictionary<string, object>> results = JsonHelper.jsonToList( IEntity );

			  assertEquals( originalNodeCount + 1, CountNodes() );
			  assertEquals( 1, results.Count );

			  IDictionary<string, object> result = results[0];
			  assertTrue( ( ( string ) result["location"] ).Length > 0 );
		 }

		 private string BatchUri()
		 {
			  return DataUri + "batch";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardUnderlyingErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardUnderlyingErrors()
		 {

			  JaxRsResponse response = RestRequest.req().accept(APPLICATION_JSON_TYPE).header(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER,"true").post(BatchUri(), new PrettyJSON()
							  .array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").array().value(true).value("hello").endArray().endObject().endObject().endArray().ToString());
					IDictionary<string, object> res = SingleResult( response, 0 );

			  assertTrue( ( ( string )res["message"] ).StartsWith( "Invalid JSON array in POST body", StringComparison.Ordinal ) );
			  assertEquals( 400, res["status"] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Map<String, Object> singleResult(org.Neo4Net.server.rest.JaxRsResponse response, int i) throws org.Neo4Net.server.rest.domain.JsonParseException
		 private IDictionary<string, object> SingleResult( JaxRsResponse response, int i )
		 {
			  return JsonHelper.jsonToList( response.Entity )[i];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackAllWhenGivenIncorrectRequest() throws org.Neo4Net.server.rest.domain.JsonParseException, com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException, org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackAllWhenGivenIncorrectRequest()
		 {

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value("1").endObject().endObject().@object().key("method").value("POST").key("to").value("/node").key("body").array().value("a_list").value("this_makes_no_sense").endArray().endObject().endArray().ToString();

			  long originalNodeCount = CountNodes();

			  JaxRsResponse response = RestRequest.req().accept(APPLICATION_JSON_TYPE).header(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").post(BatchUri(), jsonString);
			  assertEquals( 200, response.Status );

			  // Message of the ClassCastException differs in Oracle JDK [typeX cannot be cast to typeY]
			  // and IBM JDK [typeX incompatible with typeY]. That is why we check parts of the message and exception class.
			  IDictionary<string, string> body = ( System.Collections.IDictionary ) SingleResult( response, 1 )["body"];
			  assertEquals( typeof( BadInputException ).Name, body["exception"] );
			  assertThat( body["message"], containsString( "java.util.ArrayList" ) );
			  assertThat( body["message"], containsString( "java.util.Map" ) );
			  assertEquals( 400, SingleResult( response, 1 )["status"] );

			  assertEquals( originalNodeCount, CountNodes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldHandleUnicodeGetCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUnicodeGetCorrectly()
		 {
			  string asianText = "\u4f8b\u5b50";
			  string germanText = "öäüÖÄÜß";

			  string complicatedString = asianText + germanText;

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key(complicatedString).value(complicatedString).endObject().endObject().endArray().ToString();

			  string IEntity = GenConflict.get().expectedType(APPLICATION_JSON_TYPE).withHeader(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER,"true").expectedStatus(200).payload(jsonString).post(BatchUri()).entity();

			  // Pull out the property value from the depths of the response
			  IDictionary<string, object> response = ( IDictionary<string, object> ) JsonHelper.jsonToList( IEntity )[0]["body"];
			  string returnedValue = ( string )( ( IDictionary<string, object> )response["data"] )[complicatedString];

			  // Ensure nothing was borked.
			  assertThat( returnedValue, @is( complicatedString ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("Peter likes Jazz") public void shouldHandleEscapedStrings() throws com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException, org.json.JSONException, org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph("Peter likes Jazz")]
		 public virtual void ShouldHandleEscapedStrings()
		 {
			  string @string = "Jazz";
			  Node gnode = GetNode( @string );
			  assertThat( gnode, inTx( Graphdb(), Neo4NetMatchers.hasProperty("name").withValue(@string) ) );

			  string name = "string\\ and \"test\"";

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("PUT").key("to").value("/node/" + gnode.Id + "/properties").key("body").@object().key("name").value(name).endObject().endObject().endArray().ToString();
			  GenConflict.get().expectedType(APPLICATION_JSON_TYPE).withHeader(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").expectedStatus(200).payload(jsonString).post(BatchUri()).entity();

			  jsonString = ( new PrettyJSON() ).array().@object().key("method").value("GET").key("to").value("/node/" + gnode.Id + "/properties/name").endObject().endArray().ToString();
			  string IEntity = GenConflict.get().expectedStatus(200).payload(jsonString).post(BatchUri()).entity();

			  IList<IDictionary<string, object>> results = JsonHelper.jsonToList( IEntity );
			  assertEquals( results[0]["body"], name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackAllWhenInsertingIllegalData() throws org.Neo4Net.server.rest.domain.JsonParseException, com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException, org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackAllWhenInsertingIllegalData()
		 {

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().endObject().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").@object().key("age").value(1).endObject().endObject().endObject().endArray().ToString();

			  long originalNodeCount = CountNodes();

			  JaxRsResponse response = RestRequest.req().accept(APPLICATION_JSON_TYPE).header(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").post(BatchUri(), jsonString);
			  assertEquals( 200, response.Status );
			  assertEquals( 400, SingleResult( response,1 )["status"] );
			  assertEquals( originalNodeCount, CountNodes() );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackAllOnSingle404() throws org.Neo4Net.server.rest.domain.JsonParseException, com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException, org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackAllOnSingle404()
		 {

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().endObject().@object().key("method").value("POST").key("to").value("www.google.com").endObject().endArray().ToString();

			  long originalNodeCount = CountNodes();

			  JaxRsResponse response = RestRequest.req().accept(APPLICATION_JSON_TYPE).header(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").post(BatchUri(), jsonString);
			  assertEquals( 200, response.Status );
			  assertEquals( 404, SingleResult( response,1 )["status"] );
			  assertEquals( originalNodeCount, CountNodes() );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReferToUniquelyCreatedEntities() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReferToUniquelyCreatedEntities()
		 {
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/index/node/Cultures?unique").key("body").@object().key("key").value("ID").key("value").value("fra").key("properties").@object().key("ID").value("fra").endObject().endObject().key("id").value(0).endObject().@object().key("method").value("POST").key("to").value("/node").key("id").value(1).endObject().@object().key("method").value("POST").key("to").value("{1}/relationships").key("body").@object().key("to").value("{0}").key("type").value("has").endObject().key("id").value(2).endObject().endArray().ToString();

			  JaxRsResponse response = RestRequest.req().accept(APPLICATION_JSON_TYPE).header(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").post(BatchUri(), jsonString);

			  assertEquals( 200, response.Status );

		 }

		 // It has to be possible to create relationships among created and not-created nodes
		 // in batch operation.  Tests the fix for issue #690.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReferToNotCreatedUniqueEntities() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReferToNotCreatedUniqueEntities()
		 {
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/index/node/Cultures?unique").key("body").@object().key("key").value("name").key("value").value("tobias").key("properties").@object().key("name").value("Tobias Tester").endObject().endObject().key("id").value(0).endObject().@object().key("method").value("POST").key("to").value("/index/node/Cultures?unique").key("body").@object().key("key").value("name").key("value").value("andres").key("properties").@object().key("name").value("Andres Tester").endObject().endObject().key("id").value(1).endObject().@object().key("method").value("POST").key("to").value("/index/node/Cultures?unique").key("body").@object().key("key").value("name").key("value").value("andres").key("properties").@object().key("name").value("Andres Tester").endObject().endObject().key("id").value(2).endObject().@object().key("method").value("POST").key("to").value("/index/relationship/my_rels/?unique").key("body").@object().key("key").value("name").key("value").value("tobias-andres").key("start").value("{0}").key("end").value("{1}").key("type").value("FRIENDS").endObject().key("id").value(3).endObject().@object().key("method").value("POST").key("to").value("/index/relationship/my_rels/?unique").key("body").@object().key("key").value("name").key("value").value("andres-tobias").key("start").value("{2}").key("end").value("{0}").key("type").value("FRIENDS").endObject().key("id").value(4).endObject().@object().key("method").value("POST").key("to").value("/index/relationship/my_rels/?unique").key("body").@object().key("key").value("name").key("value").value("andres-tobias").key("start").value("{1}").key("end").value("{0}").key("type").value("FRIENDS").endObject().key("id").value(5).endObject().endArray().ToString();

			  JaxRsResponse response = RestRequest.req().accept(APPLICATION_JSON_TYPE).header(Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").post(BatchUri(), jsonString);

			  assertEquals( 200, response.Status );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String IEntity = response.getEntity();
			  string IEntity = response.Entity;
			  IList<IDictionary<string, object>> results = JsonHelper.jsonToList( IEntity );
			  assertEquals( 6, results.Count );
			  IDictionary<string, object> andresResult1 = results[1];
			  IDictionary<string, object> andresResult2 = results[2];
			  IDictionary<string, object> secondRelationship = results[4];
			  IDictionary<string, object> thirdRelationship = results[5];

			  // Same people
			  IDictionary<string, object> body1 = ( IDictionary<string, object> ) andresResult1["body"];
			  IDictionary<string, object> body2 = ( IDictionary<string, object> ) andresResult2["body"];
			  assertEquals( body1["id"], body2["id"] );
			  // Same relationship
			  body1 = ( IDictionary<string, object> ) secondRelationship["body"];
			  body2 = ( IDictionary<string, object> ) thirdRelationship["body"];
			  assertEquals( body1["self"], body2["self"] );
			  // Created for {2} {0}
			  assertTrue( ( ( string ) secondRelationship["location"] ).Length > 0 );
			  // {2} = {1} = Andres
			  body1 = ( IDictionary<string, object> ) secondRelationship["body"];
			  body2 = ( IDictionary<string, object> ) andresResult1["body"];
			  assertEquals( body1["start"], body2["self"] );

		 }
		 private long CountNodes()
		 {
			  using ( Transaction transaction = Graphdb().beginTx() )
			  {
					return Iterables.count( Graphdb().AllNodes );
			  }
		 }
	}

}