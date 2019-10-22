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
	using ClientHandlerException = com.sun.jersey.api.client.ClientHandlerException;
	using UniformInterfaceException = com.sun.jersey.api.client.UniformInterfaceException;
	using JsonNode = org.codehaus.jackson.JsonNode;
	using JSONException = org.json.JSONException;
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using Graph = Neo4Net.Test.GraphDescription.Graph;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

	public class BatchOperationIT : AbstractRestFunctionalDocTestBase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Execute multiple operations in batch.\n" + "\n" + "The batch service expects an array of job descriptions as input, each job\n" + "description describing an action to be performed via the normal server\n" + "API.\n" + "\n" + "Each job description should contain a +to+ attribute, with a value\n" + "relative to the data API root (so http://localhost:7474/db/data/node becomes\n" + "just /node), and a +method+ attribute containing HTTP verb to use.\n" + "\n" + "Optionally you may provide a +body+ attribute, and an +id+ attribute to\n" + "help you keep track of responses, although responses are guaranteed to be\n" + "returned in the same order the job descriptions are received.\n" + "\n" + "The following figure outlines the different parts of the job\n" + "descriptions:\n" + "\n" + "image::batch-request-api.png[]") @SuppressWarnings("unchecked") @Test @Graph("Joe knows John") public void shouldPerformMultipleOperations() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Execute multiple operations in batch.\n" + "\n" + "The batch service expects an array of job descriptions as input, each job\n" + "description describing an action to be performed via the normal server\n" + "API.\n" + "\n" + "Each job description should contain a +to+ attribute, with a value\n" + "relative to the data API root (so http://localhost:7474/db/data/node becomes\n" + "just /node), and a +method+ attribute containing HTTP verb to use.\n" + "\n" + "Optionally you may provide a +body+ attribute, and an +id+ attribute to\n" + "help you keep track of responses, although responses are guaranteed to be\n" + "returned in the same order the job descriptions are received.\n" + "\n" + "The following figure outlines the different parts of the job\n" + "descriptions:\n" + "\n" + "image::batch-request-api.png[]"), Graph("Joe knows John")]
		 public virtual void ShouldPerformMultipleOperations()
		 {
			  long idJoe = Data.get()["Joe"].Id;
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("PUT").key("to").value("/node/" + idJoe + "/properties").key("body").@object().key("age").value(1).endObject().key("id").value(0).endObject().@object().key("method").value("GET").key("to").value("/node/" + idJoe).key("id").value(1).endObject().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().key("id").value(2).endObject().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().key("id").value(3).endObject().endArray().ToString();

			  string IEntity = GenConflict.get().payload(jsonString).expectedStatus(200).post(BatchUri()).entity();

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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Refer to items created earlier in the same batch job.\n" + "\n" + "The batch operation API allows you to refer to the URI returned from a\n" + "created resource in subsequent job descriptions, within the same batch\n" + "call.\n" + "\n" + "Use the +{[JOB ID]}+ special syntax to inject URIs from created resources\n" + "into JSON strings in subsequent job descriptions.") @Test public void shouldBeAbleToReferToCreatedResource() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Refer to items created earlier in the same batch job.\n" + "\n" + "The batch operation API allows you to refer to the URI returned from a\n" + "created resource in subsequent job descriptions, within the same batch\n" + "call.\n" + "\n" + "Use the +{[JOB ID]}+ special syntax to inject URIs from created resources\n" + "into JSON strings in subsequent job descriptions.")]
		 public virtual void ShouldBeAbleToReferToCreatedResource()
		 {
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("id").value(0).key("body").@object().key("name").value("bob").endObject().endObject().@object().key("method").value("POST").key("to").value("/node").key("id").value(1).key("body").@object().key("age").value(12).endObject().endObject().@object().key("method").value("POST").key("to").value("{0}/relationships").key("id").value(3).key("body").@object().key("to").value("{1}").key("data").@object().key("since").value("2010").endObject().key("type").value("KNOWS").endObject().endObject().@object().key("method").value("POST").key("to").value("/index/relationship/my_rels").key("id").value(4).key("body").@object().key("key").value("since").key("value").value("2010").key("uri").value("{3}").endObject().endObject().endArray().ToString();

			  string IEntity = GenConflict.get().expectedStatus(200).payload(jsonString).post(BatchUri()).entity();

			  IList<IDictionary<string, object>> results = JsonHelper.jsonToList( IEntity );

			  assertEquals( 4, results.Count );

	//        String rels = gen.get()
	//                .expectedStatus( 200 ).get( getRelationshipIndexUri( "my_rels", "since", "2010")).entity();
	//        assertEquals(1, JsonHelper.jsonToList(  rels ).size());
		 }

		 private string BatchUri()
		 {
			  return DataUri + "batch";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetLocationHeadersWhenCreatingThings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetLocationHeadersWhenCreatingThings()
		 {
			  int originalNodeCount = CountNodes();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String jsonString = new PrettyJSON().array().object().key("method").value("POST").key("to").value("/node").key("body").object().key("age").value(1).endObject().endObject().endArray().toString();
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().endObject().endArray().ToString();

			  JaxRsResponse response = RestRequest.Req().post(BatchUri(), jsonString);

			  assertEquals( 200, response.Status );
			  assertEquals( originalNodeCount + 1, CountNodes() );

			  IList<IDictionary<string, object>> results = JsonHelper.jsonToList( response.Entity );

			  assertEquals( 1, results.Count );

			  IDictionary<string, object> result = results[0];
			  assertTrue( ( ( string ) result["location"] ).Length > 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardUnderlyingErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardUnderlyingErrors()
		 {
			  JaxRsResponse response = RestRequest.Req().post(BatchUri(), new PrettyJSON()
					.array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").array().value(true).value("hello").endArray().endObject().endObject().endArray().ToString());
			  assertEquals( 500, response.Status );
			  IDictionary<string, object> res = JsonHelper.jsonToMap( response.Entity );

			  assertTrue( ( ( string )res["message"] ).StartsWith( "Invalid JSON array in POST body", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackAllWhenGivenIncorrectRequest() throws com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException, org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackAllWhenGivenIncorrectRequest()
		 {

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value("1").endObject().endObject().@object().key("method").value("POST").key("to").value("/node").key("body").array().value("a_list").value("this_makes_no_sense").endArray().endObject().endArray().ToString();

			  int originalNodeCount = CountNodes();

			  JaxRsResponse response = RestRequest.Req().post(BatchUri(), jsonString);

			  assertEquals( 500, response.Status );
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

			  string IEntity = GenConflict.get().expectedStatus(200).payload(jsonString).post(BatchUri()).entity();

			  // Pull out the property value from the depths of the response
			  IDictionary<string, object> response = ( IDictionary<string, object> ) JsonHelper.jsonToList( IEntity )[0]["body"];
			  string returnedValue = ( string )( ( IDictionary<string, object> )response["data"] )[complicatedString];

			  // Ensure nothing was borked.
			  assertThat( "Expected twisted unicode case to work, but response was: " + IEntity, returnedValue, @is( complicatedString ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFailingCypherStatementCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleFailingCypherStatementCorrectly()
		 {
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/cypher").key("body").@object().key("query").value("create (n) set n.foo = {maps:'not welcome'} return n").key("params").@object().key("id").value("0").endObject().endObject().endObject().@object().key("method").value("POST").key("to").value("/node").endObject().endArray().ToString();

			  string IEntity = GenConflict.get().expectedStatus(500).payload(jsonString).post(BatchUri()).entity();

			  // Pull out the property value from the depths of the response
			  IDictionary<string, object> result = JsonHelper.jsonToMap( IEntity );
			  string exception = ( string ) result["exception"];
			  assertThat( exception, @is( "BatchOperationFailedException" ) );
			  string innerException = ( string ) JsonHelper.jsonToMap( ( string ) result["message"] )["exception"];
			  assertThat( innerException, @is( "CypherTypeException" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("Peter likes Jazz") public void shouldHandleEscapedStrings() throws com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException, org.json.JSONException, org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph("Peter likes Jazz")]
		 public virtual void ShouldHandleEscapedStrings()
		 {
			  string @string = "Jazz";
			  Node gnode = GetNode( @string );
			  assertThat( gnode, inTx( Graphdb(), hasProperty("name").withValue(@string) ) );

			  string name = "string\\ and \"test\"";

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("PUT").key("to").value("/node/" + gnode.Id + "/properties").key("body").@object().key("name").value(name).endObject().endObject().endArray().ToString();
			  GenConflict.get().expectedStatus(200).payload(jsonString).post(BatchUri()).entity();

			  jsonString = ( new PrettyJSON() ).array().@object().key("method").value("GET").key("to").value("/node/" + gnode.Id + "/properties/name").endObject().endArray().ToString();
			  string IEntity = GenConflict.get().expectedStatus(200).payload(jsonString).post(BatchUri()).entity();

			  IList<IDictionary<string, object>> results = JsonHelper.jsonToList( IEntity );
			  assertEquals( results[0]["body"], name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackAllWhenInsertingIllegalData() throws com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException, org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackAllWhenInsertingIllegalData()
		 {

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().endObject().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").@object().key("age").value(1).endObject().endObject().endObject().endArray().ToString();

			  int originalNodeCount = CountNodes();

			  JaxRsResponse response = RestRequest.Req().post(BatchUri(), jsonString);

			  assertEquals( 500, response.Status );
			  assertEquals( originalNodeCount, CountNodes() );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackAllOnSingle404() throws com.sun.jersey.api.client.ClientHandlerException, com.sun.jersey.api.client.UniformInterfaceException, org.json.JSONException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackAllOnSingle404()
		 {

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/node").key("body").@object().key("age").value(1).endObject().endObject().@object().key("method").value("POST").key("to").value("www.google.com").endObject().endArray().ToString();

			  int originalNodeCount = CountNodes();

			  JaxRsResponse response = RestRequest.Req().post(BatchUri(), jsonString);

			  assertEquals( 500, response.Status );
			  assertEquals( originalNodeCount, CountNodes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReferToUniquelyCreatedEntities() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReferToUniquelyCreatedEntities()
		 {
			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/index/node/Cultures?unique").key("body").@object().key("key").value("ID").key("value").value("fra").key("properties").@object().key("ID").value("fra").endObject().endObject().key("id").value(0).endObject().@object().key("method").value("POST").key("to").value("/node").key("id").value(1).endObject().@object().key("method").value("POST").key("to").value("{1}/relationships").key("body").@object().key("to").value("{0}").key("type").value("has").endObject().key("id").value(2).endObject().endArray().ToString();

			  JaxRsResponse response = RestRequest.Req().post(BatchUri(), jsonString);

			  assertEquals( 200, response.Status );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailWhenRemovingAndAddingLabelsInOneBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFailWhenRemovingAndAddingLabelsInOneBatch()
		 {
			  // given

			  /*
			  curl -X POST http://localhost:7474/db/data/batch -H 'Content-Type: application/json'
			  -d '[
			     {"body":{"name":"Alice"},"to":"node","id":0,"method":"POST"},
			     {"body":["expert","coder"],"to":"{0}/labels","id":1,"method":"POST"},
			     {"body":["novice","chef"],"to":"{0}/labels","id":2,"method":"PUT"}
			  ]'
			  */

			  string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("node").key("id").value(0).key("body").@object().key("key").value("name").key("value").value("Alice").endObject().endObject().@object().key("method").value("POST").key("to").value("{0}/labels").key("id").value(1).key("body").array().value("expert").value("coder").endArray().endObject().@object().key("method").value("PUT").key("to").value("{0}/labels").key("id").value(2).key("body").array().value("novice").value("chef").endArray().endObject().endArray().ToString();

			  // when
			  JaxRsResponse response = RestRequest.Req().post(BatchUri(), jsonString);

			  // then
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

			  JaxRsResponse response = RestRequest.Req().post(BatchUri(), jsonString);

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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenUsingPeriodicCommitViaNewTxEndpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenUsingPeriodicCommitViaNewTxEndpoint()
		 {
			  ServerTestUtils.withCSVFile(1, url =>
			  {
				// Given
				string jsonString = ( new PrettyJSON() ).array().@object().key("method").value("POST").key("to").value("/transaction/commit").key("body").@object().key("statements").array().@object().key("statement").value("USING PERIODIC COMMIT LOAD CSV FROM '" + url + "' AS line CREATE ()").endObject().endArray().endObject().endObject().endArray().ToString();

				// When
				JsonNode result = JsonHelper.jsonNode( GenConflict.get().expectedStatus(200).payload(jsonString).post(BatchUri()).entity() );

				// Then
				JsonNode results = result.get( 0 ).get( "body" ).get( "results" );
				JsonNode errors = result.get( 0 ).get( "body" ).get( "errors" );

				assertTrue( "Results not an array", results.Array );
				assertEquals( 0, results.size() );
				assertTrue( "Errors not an array", errors.Array );
				assertEquals( 1, errors.size() );

				string errorCode = errors.get( 0 ).get( "code" ).TextValue;
				assertEquals( "Neo.ClientError.Statement.SemanticError", errorCode );
			  });
		 }

		 private int CountNodes()
		 {
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					int count = 0;
					foreach ( Node node in Graphdb().AllNodes )
					{
						 count++;
					}
					return count;
			  }
		 }
	}

}