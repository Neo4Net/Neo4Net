using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Server.rest.transactional
{
	using Test = org.junit.Test;


	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using ResponseEntity = Neo4Net.Server.rest.RESTRequestGenerator.ResponseEntity;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using RFC1123 = Neo4Net.Server.rest.repr.util.RFC1123;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.collection.IsMapContaining.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.jsonToMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.GET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.POST;

	public class TransactionTestIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Begin a transaction\n" + "\n" + "You begin a new transaction by posting zero or more Cypher statements\n" + "to the transaction endpoint. The server will respond with the result of\n" + "your statements, as well as the location of your open transaction.") public void begin_a_transaction() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Begin a transaction\n" + "\n" + "You begin a new transaction by posting zero or more Cypher statements\n" + "to the transaction endpoint. The server will respond with the result of\n" + "your statements, as well as the location of your open transaction.")]
		 public virtual void BeginATransaction()
		 {
			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(201).payload(QuotedJson("{ 'statements': [ { 'statement': 'CREATE (n {props}) RETURN n', " + "'parameters': { 'props': { 'name': 'My Node' } } } ] }")).post(DataUri + "transaction");

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  AssertNoErrors( result );
			  IDictionary<string, object> node = ResultCell( result, 0, 0 );
			  assertThat( node["name"], equalTo( "My Node" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Execute statements in an open transaction\n" + "\n" + "Given that you have an open transaction, you can make a number of requests, each of which executes additional\n" + "statements, and keeps the transaction open by resetting the transaction timeout.") public void execute_statements_in_an_open_transaction() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Execute statements in an open transaction\n" + "\n" + "Given that you have an open transaction, you can make a number of requests, each of which executes additional\n" + "statements, and keeps the transaction open by resetting the transaction timeout.")]
		 public virtual void ExecuteStatementsInAnOpenTransaction()
		 {
			  // Given
			  string location = POST( DataUri + "transaction" ).location();

			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ { 'statement': 'CREATE (n) RETURN n' } ] }")).post(location);

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  assertThat( result, hasKey( "transaction" ) );
			  AssertNoErrors( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Execute statements in an open transaction in REST format for the return.\n" + "\n" + "Given that you have an open transaction, you can make a number of requests, each of which executes additional\n" + "statements, and keeps the transaction open by resetting the transaction timeout. Specifying the `REST` format will\n" + "give back full Neo4j Rest API representations of the Neo4j Nodes, Relationships and Paths, if returned.") public void execute_statements_in_an_open_transaction_using_REST() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Execute statements in an open transaction in REST format for the return.\n" + "\n" + "Given that you have an open transaction, you can make a number of requests, each of which executes additional\n" + "statements, and keeps the transaction open by resetting the transaction timeout. Specifying the `REST` format will\n" + "give back full Neo4j Rest API representations of the Neo4j Nodes, Relationships and Paths, if returned.")]
		 public virtual void ExecuteStatementsInAnOpenTransactionUsingREST()
		 {
			  // Given
			  string location = POST( DataUri + "transaction" ).location();

			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ { 'statement': 'CREATE (n) RETURN n','resultDataContents':['REST'] } ] }")).post(location);

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  List<object> rest = ( List<object> )( ( System.Collections.IDictionary )( ( List<object> )( ( System.Collections.IDictionary )( ( List<object> )result["results"] )[0] )["data"] )[0] )["rest"];
			  string selfUri = ( string )( ( System.Collections.IDictionary )rest[0] )["self"];
			  assertTrue( selfUri.StartsWith( DatabaseUri, StringComparison.Ordinal ) );
			  AssertNoErrors( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Reset transaction timeout of an open transaction\n" + "\n" + "Every orphaned transaction is automatically expired after a period of inactivity.  This may be prevented\n" + "by resetting the transaction timeout.\n" + "\n" + "The timeout may be reset by sending a keep-alive request to the server that executes an empty list of statements.\n" + "This request will reset the transaction timeout and return the new time at which the transaction will\n" + "expire as an RFC1123 formatted timestamp value in the ``transaction'' section of the response.") public void reset_transaction_timeout_of_an_open_transaction() throws org.neo4j.server.rest.domain.JsonParseException, java.text.ParseException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Reset transaction timeout of an open transaction\n" + "\n" + "Every orphaned transaction is automatically expired after a period of inactivity.  This may be prevented\n" + "by resetting the transaction timeout.\n" + "\n" + "The timeout may be reset by sending a keep-alive request to the server that executes an empty list of statements.\n" + "This request will reset the transaction timeout and return the new time at which the transaction will\n" + "expire as an RFC1123 formatted timestamp value in the ``transaction'' section of the response.")]
		 public virtual void ResetTransactionTimeoutOfAnOpenTransaction()
		 {
			  // Given
			  HTTP.Response initialResponse = POST( DataUri + "transaction" );
			  string location = initialResponse.Location();
			  long initialExpirationTime = ExpirationTime( jsonToMap( initialResponse.RawContent() ) );

			  // This generous wait time is necessary to compensate for limited resolution of RFC 1123 timestamps
			  // and the fact that the system clock is allowed to run "backwards" between threads
			  // (cf. http://stackoverflow.com/questions/2978598)
			  //
			  Thread.Sleep( 3000 );

			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ ] }")).post(location);

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  AssertNoErrors( result );
			  long newExpirationTime = ExpirationTime( result );

			  assertTrue( "Expiration time was not increased", newExpirationTime > initialExpirationTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Commit an open transaction\n" + "\n" + "Given you have an open transaction, you can send a commit request. Optionally, you submit additional statements\n" + "along with the request that will be executed before committing the transaction.") public void commit_an_open_transaction() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Commit an open transaction\n" + "\n" + "Given you have an open transaction, you can send a commit request. Optionally, you submit additional statements\n" + "along with the request that will be executed before committing the transaction.")]
		 public virtual void CommitAnOpenTransaction()
		 {
			  // Given
			  string location = POST( DataUri + "transaction" ).location();

			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ { 'statement': 'CREATE (n) RETURN id(n)' } ] }")).post(location + "/commit");

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  AssertNoErrors( result );

			  int? id = ResultCell( result, 0, 0 );
			  assertThat( GET( getNodeUri( id ) ).status(), @is(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Begin and commit a transaction in one request\n" + "\n" + "If there is no need to keep a transaction open across multiple HTTP requests, you can begin a transaction,\n" + "execute statements, and commit with just a single HTTP request.") public void begin_and_commit_a_transaction_in_one_request() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Begin and commit a transaction in one request\n" + "\n" + "If there is no need to keep a transaction open across multiple HTTP requests, you can begin a transaction,\n" + "execute statements, and commit with just a single HTTP request.")]
		 public virtual void BeginAndCommitATransactionInOneRequest()
		 {
			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ { 'statement': 'CREATE (n) RETURN id(n)' } ] }")).post(DataUri + "transaction/commit");

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  AssertNoErrors( result );

			  int? id = ResultCell( result, 0, 0 );
			  assertThat( GET( getNodeUri( id ) ).status(), @is(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Execute multiple statements\n" + "\n" + "You can send multiple Cypher statements in the same request.\n" + "The response will contain the result of each statement.") public void execute_multiple_statements() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Execute multiple statements\n" + "\n" + "You can send multiple Cypher statements in the same request.\n" + "The response will contain the result of each statement.")]
		 public virtual void ExecuteMultipleStatements()
		 {
			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ { 'statement': 'CREATE (n) RETURN id(n)' }, " + "{ 'statement': 'CREATE (n {props}) RETURN n', " + "'parameters': { 'props': { 'name': 'My Node' } } } ] }")).post(DataUri + "transaction/commit");

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  AssertNoErrors( result );
			  int? id = ResultCell( result, 0, 0 );
			  assertThat( GET( getNodeUri( id ) ).status(), @is(200) );
			  assertThat( response.Entity(), containsString("My Node") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Return results in graph format\n" + "\n" + "If you want to understand the graph structure of nodes and relationships returned by your query,\n" + "you can specify the \"graph\" results data format. For example, this is useful when you want to visualise the\n" + "graph structure. The format collates all the nodes and relationships from all columns of the result,\n" + "and also flattens collections of nodes and relationships, including paths.") public void return_results_in_graph_format() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Return results in graph format\n" + "\n" + "If you want to understand the graph structure of nodes and relationships returned by your query,\n" + "you can specify the \"graph\" results data format. For example, this is useful when you want to visualise the\n" + "graph structure. The format collates all the nodes and relationships from all columns of the result,\n" + "and also flattens collections of nodes and relationships, including paths.")]
		 public virtual void ReturnResultsInGraphFormat()
		 {
			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{'statements':[{'statement':" + "'CREATE ( bike:Bike { weight: 10 } ) " + "CREATE ( frontWheel:Wheel { spokes: 3 } ) " + "CREATE ( backWheel:Wheel { spokes: 32 } ) " + "CREATE p1 = (bike)-[:HAS { position: 1 } ]->(frontWheel) " + "CREATE p2 = (bike)-[:HAS { position: 2 } ]->(backWheel) " + "RETURN bike, p1, p2', " + "'resultDataContents': ['row','graph']}] }")).post(DataUri + "transaction/commit");

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  AssertNoErrors( result );

			  IDictionary<string, IList<object>> row = GraphRow( result, 0 );
			  assertEquals( 3, row["nodes"].Count );
			  assertEquals( 2, row["relationships"].Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Rollback an open transaction\n" + "\n" + "Given that you have an open transaction, you can send a rollback request. The server will rollback the\n" + "transaction. Any further statements trying to run in this transaction will fail immediately.") public void rollback_an_open_transaction() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Rollback an open transaction\n" + "\n" + "Given that you have an open transaction, you can send a rollback request. The server will rollback the\n" + "transaction. Any further statements trying to run in this transaction will fail immediately.")]
		 public virtual void RollbackAnOpenTransaction()
		 {
			  // Given
			  HTTP.Response firstReq = POST( DataUri + "transaction", HTTP.RawPayload.quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n) RETURN id(n)' } ] }" ) );
			  string location = firstReq.Location();

			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).delete(location);

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  AssertNoErrors( result );

			  int? id = ResultCell( firstReq, 0, 0 );
			  assertThat( GET( getNodeUri( id ) ).status(), @is(404) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Handling errors\n" + "\n" + "The result of any request against the transaction endpoint is streamed back to the client.\n" + "Therefore the server does not know whether the request will be successful or not when it sends the HTTP status\n" + "code.\n" + "\n" + "Because of this, all requests against the transactional endpoint will return 200 or 201 status code, regardless\n" + "of whether statements were successfully executed. At the end of the response payload, the server includes a list\n" + "of errors that occurred while executing statements. If this list is empty, the request completed successfully.\n" + "\n" + "If any errors occur while executing statements, the server will roll back the transaction.\n" + "\n" + "In this example, we send the server an invalid statement to demonstrate error handling.\n" + " \n" + "For more information on the status codes, see <<status-codes>>.") public void handling_errors() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Handling errors\n" + "\n" + "The result of any request against the transaction endpoint is streamed back to the client.\n" + "Therefore the server does not know whether the request will be successful or not when it sends the HTTP status\n" + "code.\n" + "\n" + "Because of this, all requests against the transactional endpoint will return 200 or 201 status code, regardless\n" + "of whether statements were successfully executed. At the end of the response payload, the server includes a list\n" + "of errors that occurred while executing statements. If this list is empty, the request completed successfully.\n" + "\n" + "If any errors occur while executing statements, the server will roll back the transaction.\n" + "\n" + "In this example, we send the server an invalid statement to demonstrate error handling.\n" + " \n" + "For more information on the status codes, see <<status-codes>>.")]
		 public virtual void HandlingErrors()
		 {
			  // Given
			  string location = POST( DataUri + "transaction" ).location();

			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ { 'statement': 'This is not a valid Cypher Statement.' } ] }")).post(location + "/commit");

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  AssertErrors( result, Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Handling errors in an open transaction\n" + "\n" + "Whenever there is an error in a request the server will rollback the transaction.\n" + "By inspecting the response for the presence/absence of the `transaction` key you can tell if the " + "transaction is still open") public void errors_in_open_transaction() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Handling errors in an open transaction\n" + "\n" + "Whenever there is an error in a request the server will rollback the transaction.\n" + "By inspecting the response for the presence/absence of the `transaction` key you can tell if the " + "transaction is still open")]
		 public virtual void ErrorsInOpenTransaction()
		 {
			  // Given
			  string location = POST( DataUri + "transaction" ).location();

			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ { 'statement': 'This is not a valid Cypher Statement.' } ] }")).post(location);

			  // Then
			  IDictionary<string, object> result = jsonToMap( response.Entity() );
			  assertThat( result, not( hasKey( "transaction" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Include query statistics\n" + "\n" + "By setting `includeStats` to `true` for a statement, query statistics will be returned for it.") public void include_query_statistics() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Include query statistics\n" + "\n" + "By setting `includeStats` to `true` for a statement, query statistics will be returned for it.")]
		 public virtual void IncludeQueryStatistics()
		 {
			  // Document
			  ResponseEntity response = GenConflict.get().expectedStatus(200).payload(QuotedJson("{ 'statements': [ { 'statement': 'CREATE (n) RETURN id(n)', 'includeStats': true } ] }")).post(DataUri + "transaction/commit");

			  // Then
			  IDictionary<string, object> entity = jsonToMap( response.Entity() );
			  AssertNoErrors( entity );
			  IDictionary<string, object> firstResult = ( ( IList<IDictionary<string, object>> ) entity["results"] )[0];

			  assertThat( firstResult, hasKey( "stats" ) );
			  IDictionary<string, object> stats = ( IDictionary<string, object> ) firstResult["stats"];
			  assertThat( stats["nodes_created"], equalTo( 1 ) );
		 }

		 private void AssertNoErrors( IDictionary<string, object> response )
		 {
			  AssertErrors( response );
		 }

		 private void AssertErrors( IDictionary<string, object> response, params Status[] expectedErrors )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Iterator<java.util.Map<String, Object>> errors = ((java.util.List<java.util.Map<String, Object>>) response.get("errors")).iterator();
			  IEnumerator<IDictionary<string, object>> errors = ( ( IList<IDictionary<string, object>> ) response["errors"] ).GetEnumerator();
			  IEnumerator<Status> expected = iterator( expectedErrors );

			  while ( expected.MoveNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( errors.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( errors.next().get("code"), equalTo(expected.Current.code().serialize()) );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( errors.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IDictionary<string, object> error = errors.next();
					fail( "Expected no more errors, but got " + error["code"] + " - '" + error["message"] + "'." );
			  }
		 }

		 private T ResultCell<T>( HTTP.Response response, int row, int column )
		 {
			  return ResultCell( response.Content<IDictionary<string, object>>(), row, column );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T> T resultCell(java.util.Map<String, Object> response, int row, int column)
		 private T ResultCell<T>( IDictionary<string, object> response, int row, int column )
		 {
			  IDictionary<string, object> result = ( ( IList<IDictionary<string, object>> ) response["results"] )[0];
			  IList<IDictionary<string, System.Collections.IList>> data = ( IList<IDictionary<string, System.Collections.IList>> ) result["data"];
			  return ( T ) data[row]["row"][column];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private java.util.Map<String, java.util.List<Object>> graphRow(java.util.Map<String, Object> response, int row)
		 private IDictionary<string, IList<object>> GraphRow( IDictionary<string, object> response, int row )
		 {
			  IDictionary<string, object> result = ( ( IList<IDictionary<string, object>> ) response["results"] )[0];
			  IList<IDictionary<string, System.Collections.IList>> data = ( IList<IDictionary<string, System.Collections.IList>> ) result["data"];
			  return ( IDictionary<string, IList<object>> ) data[row]["graph"];
		 }

		 private string QuotedJson( string singleQuoted )
		 {
			  return singleQuoted.replaceAll( "'", "\"" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long expirationTime(java.util.Map<String, Object> entity) throws java.text.ParseException
		 private long ExpirationTime( IDictionary<string, object> entity )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: String timestampString = (String)((java.util.Map<?, ?>) entity.get("transaction")).get("expires");
			  string timestampString = ( string )( ( IDictionary<object, ?> ) entity["transaction"] )["expires"];
			  return RFC1123.parseTimestamp( timestampString ).Ticks;
		 }
	}

}