using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
namespace Neo4Net.Server.rest.transactional.integration
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using DatabaseTransactionStats = Neo4Net.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using XForwardUtil = Neo4Net.Server.web.XForwardUtil;
	using HTTP = Neo4Net.Test.server.HTTP;
	using Response = Neo4Net.Test.server.HTTP.Response;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.jsonNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.containsNoErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.hasErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.isValidRFCTimestamp;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.matches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.rawPayload;

	public class TransactionIT : AbstractRestFunctionalTestBase
	{
		 private readonly HTTP.Builder _http = HTTP.withBaseUri( Server().baseUri() );
		 private ExecutorService _executors;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _executors = Executors.newFixedThreadPool( max( 3, Runtime.Runtime.availableProcessors() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _executors.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__execute__commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_Execute_Commit()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  assertThat( begin.Status(), equalTo(201) );
			  AssertHasTxLocation( begin );

			  string commitResource = begin.StringFromContent( "commit" );
			  assertThat( commitResource, matches( "http://localhost:\\d+/db/data/transaction/\\d+/commit" ) );
			  assertThat( begin.Get( "transaction" ).get( "expires" ).asText(), ValidRFCTimestamp );

			  // execute
			  HTTP.Response execute = _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ] }") );
			  assertThat( execute.Status(), equalTo(200) );
			  assertThat( execute.Get( "transaction" ).get( "expires" ).asText(), ValidRFCTimestamp );

			  // commit
			  HTTP.Response commit = _http.POST( commitResource );

			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__execute__rollback()
		 public virtual void Begin_Execute_Rollback()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  assertThat( begin.Status(), equalTo(201) );
			  AssertHasTxLocation( begin );

			  // execute
			  _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ] }") );

			  // rollback
			  HTTP.Response commit = _http.DELETE( begin.Location() );

			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__execute_and_commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_ExecuteAndCommit()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  assertThat( begin.Status(), equalTo(201) );
			  AssertHasTxLocation( begin );

			  string commitResource = begin.StringFromContent( "commit" );
			  assertThat( commitResource, equalTo( begin.Location() + "/commit" ) );

			  // execute and commit
			  HTTP.Response commit = _http.POST( commitResource, quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n)' } ] }" ) );

			  assertThat( commit, containsNoErrors() );
			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute__commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecute_Commit()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin and execute
			  HTTP.Response begin = _http.POST( "db/data/transaction", quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n)' } ] }" ) );

			  string commitResource = begin.StringFromContent( "commit" );

			  // commit
			  HTTP.Response commit = _http.POST( commitResource );

			  assertThat( commit.Status(), equalTo(200) );
			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute__commit_with_badly_escaped_statement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecute_CommitWithBadlyEscapedStatement()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();
			  string json = "{ \"statements\": [ { \"statement\": \"LOAD CSV WITH HEADERS FROM " +
								 "\\\"xx file://C:/countries.csvxxx\\\\\" as csvLine MERGE (c:Country { Code: csvLine.Code })\" " +
								 "} ] }";

			  // begin and execute
			  // given statement is badly escaped and it is a client error, thus tx is rolled back at once
			  HTTP.Response begin = _http.POST( "db/data/transaction", quotedJson( json ) );

			  string commitResource = begin.StringFromContent( "commit" );

			  // commit fails because tx was rolled back on the previous step
			  HTTP.Response commit = _http.POST( commitResource );

			  assertThat( begin.Status(), equalTo(201) );
			  assertThat( begin, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat ) );

			  assertThat( commit.Status(), equalTo(404) );
			  assertThat( commit, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionNotFound ) );

			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__execute__commit__execute() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_Execute_Commit_Execute()
		 {
			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );
			  string commitResource = begin.StringFromContent( "commit" );

			  // execute
			  _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ] }") );

			  // commit
			  _http.POST( commitResource );

			  // execute
			  HTTP.Response execute2 = _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ]" + " }") );

			  assertThat( execute2.Status(), equalTo(404) );
			  assertThat( execute2, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionNotFound ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_and_commit()
		 public virtual void BeginAndExecuteAndCommit()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin and execute and commit
			  HTTP.Response begin = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n)' } ] }" ) );

			  assertThat( begin.Status(), equalTo(200) );
			  assertThat( begin, containsNoErrors() );
			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returned_rest_urls_must_be_useable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReturnedRestUrlsMustBeUseable()
		 {
			  // begin and execute and commit "resultDataContents":["REST"]
			  HTTP.RawPayload payload = quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n {a: 1}) return n', " + "'resultDataContents' : ['REST'] } ] }" );
			  HTTP.Response begin = _http.POST( "db/data/transaction/commit", payload );

			  assertThat( begin.Status(), equalTo(200) );
			  JsonNode results = begin.Get( "results" );
			  JsonNode result = results.get( 0 );
			  JsonNode data = result.get( "data" );
			  JsonNode firstDataSegment = data.get( 0 );
			  JsonNode restData = firstDataSegment.get( "rest" );
			  JsonNode firstRestSegment = restData.get( 0 );
			  string propertiesUri = firstRestSegment.get( "properties" ).asText();

			  HTTP.Response propertiesResponse = _http.GET( propertiesUri );
			  assertThat( propertiesResponse.Status(), @is(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_and_commit_with_badly_escaped_statement()
		 public virtual void BeginAndExecuteAndCommitWithBadlyEscapedStatement()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();
			  string json = "{ \"statements\": [ { \"statement\": \"LOAD CSV WITH HEADERS FROM " +
								 "\\\"xx file://C:/countries.csvxxx\\\\\" as csvLine MERGE (c:Country { Code: csvLine.Code })\" " +
								 "} ] }";
			  // begin and execute and commit
			  HTTP.Response begin = _http.POST( "db/data/transaction/commit", quotedJson( json ) );

			  assertThat( begin.Status(), equalTo(200) );
			  assertThat( begin, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat ) );
			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_periodic_commit_and_commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecutePeriodicCommitAndCommit()
		 {
			  int nodes = 11;
			  int batch = 2;
			  ServerTestUtils.withCSVFile(nodes, url =>
			  {
				Response response;
				long nodesInDatabaseBeforeTransaction;
				long txIdBefore;
				int times = 0;
				do
				{
					 nodesInDatabaseBeforeTransaction = CountNodes();
					 txIdBefore = ResolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;

					 // begin and execute and commit

					 response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'USING PERIODIC COMMIT " + batch + " LOAD CSV FROM " + "\\\"" + url + "\\\" AS line CREATE ()' } ] }" ) );
					 times++;
				} while ( response.get( "errors" ).GetEnumerator().hasNext() && (times < 5) );

				long txIdAfter = ResolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;

				assertThat( "Last response is: " + response, response, containsNoErrors() );
				assertThat( response.status(), equalTo(200) );
				assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + nodes) );
				assertThat( txIdAfter, equalTo( txIdBefore + ( ( nodes / batch ) + 1 ) ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_periodic_commit_that_returns_data_and_commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecutePeriodicCommitThatReturnsDataAndCommit()
		 {
			  int nodes = 11;
			  int batchSize = 2;
			  ServerTestUtils.withCSVFile(nodes, url =>
			  {
				long nodesInDatabaseBeforeTransaction = CountNodes();
				long txIdBefore = ResolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;

				// begin and execute and commit
				Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'USING PERIODIC COMMIT " + batchSize + " LOAD CSV FROM " + "\\\"" + url + "\\\" AS line CREATE (n {id1: 23}) RETURN n' } ] }" ) );
				long txIdAfter = ResolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;

				assertThat( response.status(), equalTo(200) );

				assertThat( response, containsNoErrors() );

				JsonNode columns = response.get( "results" ).get( 0 ).get( "columns" );
				assertThat( columns.ToString(), equalTo("[\"n\"]") );
				assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + nodes) );
				long nBatches = ( nodes / batchSize ) + 1;
				long expectedTxCount = nBatches + 1; // tx which create the property key token `id`
				assertThat( txIdAfter - txIdBefore, equalTo( expectedTxCount ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_cypher_23_periodic_commit_that_returns_data_and_commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecuteCypher_23PeriodicCommitThatReturnsDataAndCommit()
		 {
			  // to get rid off the property key id creation in the actual test
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					Node node = Graphdb().createNode();
					node.SetProperty( "id", 42 );
			  }

			  int nodes = 11;
			  int batch = 2;
			  ServerTestUtils.withCSVFile(nodes, url =>
			  {
				long nodesInDatabaseBeforeTransaction = CountNodes();
				long txIdBefore = ResolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;

				// begin and execute and commit
				Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'CYPHER 2.3 USING PERIODIC COMMIT " + batch + " LOAD CSV FROM" + " \\\"" + url + "\\\" AS line CREATE (n {id: 23}) RETURN n' } ] }" ) );
				long txIdAfter = ResolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;

				assertThat( response.status(), equalTo(200) );

				assertThat( response, containsNoErrors() );

				JsonNode columns = response.get( "results" ).get( 0 ).get( "columns" );
				assertThat( columns.ToString(), equalTo("[\"n\"]") );
				assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + nodes) );
				assertThat( txIdAfter, equalTo( txIdBefore + ( ( nodes / batch ) + 1 ) ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_periodic_commit_followed_by_another_statement_and_commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecutePeriodicCommitFollowedByAnotherStatementAndCommit()
		 {
			  ServerTestUtils.withCSVFile(1, url =>
			  {
				// begin and execute and commit
				Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'USING PERIODIC COMMIT LOAD CSV FROM \\\"" + url + "\\\" AS line CREATE (n {id: 23}) RETURN n' }, { 'statement': 'RETURN 1' } ] }" ) );

				assertThat( response.status(), equalTo(200) );
				assertThat( response, hasErrors( Status.Statement.SemanticError ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_invalid_query_and_commit()
		 public virtual void BeginAndExecuteInvalidQueryAndCommit()
		 {
			  // begin and execute and commit
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'MATCH n RETURN m' } ] }" ) );

			  assertThat( response.Status(), equalTo(200) );
			  assertThat( response, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_multiple_periodic_commit_last_and_commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecuteMultiplePeriodicCommitLastAndCommit()
		 {
			  ServerTestUtils.withCSVFile(1, url =>
			  {
				// begin and execute and commit
				Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'CREATE ()' }, " + "{ 'statement': 'USING PERIODIC COMMIT LOAD CSV FROM \\\"" + url + "\\\" AS line " + "CREATE ()' } ] }" ) );

				assertThat( response, hasErrors( Status.Statement.SemanticError ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__execute__execute_and_periodic_commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_Execute_ExecuteAndPeriodicCommit()
		 {
			  ServerTestUtils.withCSVFile(1, url =>
			  {
				// begin
				Response begin = _http.POST( "db/data/transaction" );

				// execute
				_http.POST( begin.location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE ()' } ] }") );

				// execute
				Response response = _http.POST( begin.location(), quotedJson("{ 'statements': [ { 'statement': 'USING" + " PERIODIC COMMIT LOAD CSV FROM \\\"" + url + "\\\" AS line CREATE ()' } ] }") );

				assertThat( response, hasErrors( Status.Statement.SemanticError ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_and_execute_periodic_commit__commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecutePeriodicCommit_Commit()
		 {
			  ServerTestUtils.withCSVFile(1, url =>
			  {
				// begin and execute
				Response begin = _http.POST( "db/data/transaction", quotedJson( "{ 'statements': [ { 'statement': 'USING PERIODIC COMMIT LOAD CSV FROM \\\"" + url + "\\\" AS line CREATE ()' } ] }" ) );

				assertThat( begin, hasErrors( Status.Statement.SemanticError ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__execute_multiple__commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_ExecuteMultiple_Commit()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  string commitResource = begin.StringFromContent( "commit" );

			  // execute
			  _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' }, " + "{ 'statement': 'CREATE (n)' } ] }") );

			  // commit
			  HTTP.Response commit = _http.POST( commitResource );
			  assertThat( commit, containsNoErrors() );
			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__execute__execute__commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_Execute_Execute_Commit()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  string commitResource = begin.StringFromContent( "commit" );

			  // execute
			  _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ]" + " }") );

			  // execute
			  _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ]" + " }") );

			  // commit
			  _http.POST( commitResource );

			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin_create_two_nodes_delete_one() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginCreateTwoNodesDeleteOne()
		 {
			  /*
			   * This issue was reported from the community. It resulted in a refactoring of the interaction
			   * between TxManager and TransactionContexts.
			   */

			  // GIVEN
			  long nodesInDatabaseBeforeTransaction = CountNodes();
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", rawPayload( "{ \"statements\" : [{\"statement\" : \"CREATE (n0:DecibelEntity :AlbumGroup{DecibelID : " + "'34a2201b-f4a9-420f-87ae-00a9c691cc5c', Title : 'Dance With Me', " + "ArtistString : 'Ra Ra Riot', MainArtistAlias : 'Ra Ra Riot', " + "OriginalReleaseDate : '2013-01-08', IsCanon : 'False'}) return id(n0)\"}, " + "{\"statement\" : \"CREATE (n1:DecibelEntity :AlbumRelease{DecibelID : " + "'9ed529fa-7c19-11e2-be78-bcaec5bea3c3', Title : 'Dance With Me', " + "ArtistString : 'Ra Ra Riot', MainArtistAlias : 'Ra Ra Riot', LabelName : 'Barsuk " + "Records', " + "FormatNames : 'File', TrackCount : '3', MediaCount : '1', Duration : '460.000000', " + "ReleaseDate : '2013-01-08', ReleaseYear : '2013', ReleaseRegion : 'USA', " + "Cline : 'Barsuk Records', Pline : 'Barsuk Records', CYear : '2013', PYear : '2013', " + "ParentalAdvisory : 'False', IsLimitedEdition : 'False'}) return id(n1)\"}]}" ) );
			  assertEquals( 200, response.Status() );
			  JsonNode everything = jsonNode( response.RawContent() );
			  JsonNode result = everything.get( "results" ).get( 0 );
			  long id = result.get( "data" ).get( 0 ).get( "row" ).get( 0 ).LongValue;

			  // WHEN
			  _http.POST( "db/data/cypher", rawPayload( "{\"query\":\"match (n) where id(n) = " + id + " delete n\"}" ) );

			  // THEN
			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction + 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__rollback__commit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_Rollback_Commit()
		 {
			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  assertThat( begin.Status(), equalTo(201) );
			  AssertHasTxLocation( begin );
			  string commitResource = begin.StringFromContent( "commit" );

			  // terminate
			  HTTP.Response interrupt = _http.DELETE( begin.Location() );
			  assertThat( interrupt.Status(), equalTo(200) );

			  // commit
			  HTTP.Response commit = _http.POST( commitResource );

			  assertThat( commit.Status(), equalTo(404) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__rollback__execute()
		 public virtual void Begin_Rollback_Execute()
		 {
			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  assertThat( begin.Status(), equalTo(201) );
			  AssertHasTxLocation( begin );

			  // terminate
			  HTTP.Response interrupt = _http.DELETE( begin.Location() );
			  assertThat( interrupt.Status(), equalTo(200) );

			  // execute
			  HTTP.Response execute = _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ] }") );

			  assertThat( execute.Status(), equalTo(404) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 30_000) public void begin__execute__rollback_concurrently() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_Execute_RollbackConcurrently()
		 {
			  // begin
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.server.HTTP.Response begin = http.POST("db/data/transaction");
			  HTTP.Response begin = _http.POST( "db/data/transaction" );
			  assertThat( begin.Status(), equalTo(201) );
			  AssertHasTxLocation( begin );

			  Label sharedLockLabel = Label.label( "sharedLock" );
			  _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n:" + sharedLockLabel + ")' } ] }" ) );

			  System.Threading.CountdownEvent nodeLockLatch = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent nodeReleaseLatch = new System.Threading.CountdownEvent( 1 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> lockerFuture = executors.submit(() -> lockNodeWithLabel(sharedLockLabel, nodeLockLatch, nodeReleaseLatch));
			  Future<object> lockerFuture = _executors.submit( () => lockNodeWithLabel(sharedLockLabel, nodeLockLatch, nodeReleaseLatch) );
			  nodeLockLatch.await();

			  // execute
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String executeResource = begin.location();
			  string executeResource = begin.Location();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String statement = "MATCH (n:" + sharedLockLabel + ") DELETE n RETURN count(n)";
			  string statement = "MATCH (n:" + sharedLockLabel + ") DELETE n RETURN count(n)";

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Future<org.neo4j.test.server.HTTP.Response> executeFuture = executors.submit(() ->
			  Future<HTTP.Response> executeFuture = _executors.submit(() =>
			  {
				HTTP.Builder requestBuilder = HTTP.withBaseUri( Server().baseUri() );
				Response response = requestBuilder.POST( executeResource, quotedJson( "{ 'statements': [ { 'statement': '" + statement + "' } ] }" ) );
				assertThat( response.status(), equalTo(200) );
				return response;
			  });

			  // terminate
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Future<org.neo4j.test.server.HTTP.Response> interruptFuture = executors.submit(() ->
			  Future<HTTP.Response> interruptFuture = _executors.submit(() =>
			  {
				WaitForStatementExecution( statement );

				Response response = _http.DELETE( executeResource );
				assertThat( response.ToString(), response.status(), equalTo(200) );
				nodeReleaseLatch.Signal();
				return response;
			  });

			  interruptFuture.get();
			  lockerFuture.get();
			  HTTP.Response execute = executeFuture.get();
			  assertThat( execute, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed ) );

			  HTTP.Response execute2 = _http.POST( executeResource, quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n)' } ] }" ) );
			  assertThat( execute2.Status(), equalTo(404) );
			  assertThat( execute2, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionNotFound ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void status_codes_should_appear_in_response()
		 public virtual void StatusCodesShouldAppearInResponse()
		 {
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'RETURN {n}' } ] }" ) );

			  assertThat( response.Status(), equalTo(200) );
			  assertThat( response, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ParameterMissing ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executing_single_statement_in_new_transaction_and_failing_to_read_the_output_should_interrupt() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExecutingSingleStatementInNewTransactionAndFailingToReadTheOutputShouldInterrupt()
		 {
			  // given
			  long initialNodes = CountNodes();
			  DatabaseTransactionStats txMonitor = ( ( GraphDatabaseAPI ) Graphdb() ).DependencyResolver.resolveDependency(typeof(DatabaseTransactionStats));
			  long initialTerminations = txMonitor.NumberOfTerminatedTransactions;

			  // when sending a request and aborting in the middle of receiving the result
			  Socket socket = new Socket( "localhost", LocalHttpPort );
			  PrintStream @out = new PrintStream( socket.OutputStream );

			  string output = quotedJson( "{ 'statements': [ { 'statement': 'WITH * UNWIND range(0, 9999) AS i CREATE (n {i: i}) RETURN n' } ] " + "}" ).get();
			  @out.print( "POST /db/data/transaction/commit HTTP/1.1\r\n" );
			  @out.print( "Host: localhost:7474\r\n" );
			  @out.print( "Content-type: application/json; charset=utf-8\r\n" );
			  @out.print( "Content-length: " + output.GetBytes().length + "\r\n" );
			  @out.print( "\r\n" );
			  @out.print( output );
			  @out.print( "\r\n" );

			  Stream inputStream = socket.InputStream;
			  Reader reader = new StreamReader( inputStream );

			  int numRead = 0;
			  while ( numRead < 300 )
			  {
					numRead += reader.read( new char[300] );
			  }
			  socket.close();

			  using ( Transaction ignored = Graphdb().beginTx() )
			  {
					assertEquals( initialNodes, CountNodes() );
			  }

			  // then soon the transaction should have been terminated
			  long endTime = DateTimeHelper.CurrentUnixTimeMillis() + 5000;
			  long additionalTerminations = 0;

			  while ( true )
			  {
					additionalTerminations = txMonitor.NumberOfTerminatedTransactions - initialTerminations;

					if ( additionalTerminations > 0 || DateTimeHelper.CurrentUnixTimeMillis() > endTime )
					{
						 break;
					}

					Thread.Sleep( 100 );
			  }

			  assertEquals( 1, additionalTerminations );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_include_graph_format_when_requested() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeGraphFormatWhenRequested()
		 {
			  long initialData = CountNodes( "Foo" );

			  // given
			  _http.POST( "db/data/transaction/commit", SingleStatement( "CREATE (n:Foo:Bar)" ) );

			  // when
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'MATCH (n:Foo) RETURN n', 'resultDataContents':['row'," + "'graph'] } ] }" ) );

			  // then
			  assertThat( response.Status(), equalTo(200) );
			  JsonNode data = response.Get( "results" ).get( 0 ).get( "data" );
			  assertTrue( "data is a list", data.Array );
			  assertEquals( "one entry", initialData + 1, data.size() );
			  JsonNode entry = data.get( 0 );
			  assertTrue( "entry has row", entry.has( "row" ) );
			  assertTrue( "entry has graph", entry.has( "graph" ) );
			  JsonNode nodes = entry.get( "graph" ).get( "nodes" );
			  JsonNode rels = entry.get( "graph" ).get( "relationships" );
			  assertTrue( "nodes is a list", nodes.Array );
			  assertTrue( "relationships is a list", rels.Array );
			  assertEquals( "one node", 1, nodes.size() );
			  assertEquals( "no relationships", 0, rels.size() );
			  ISet<string> labels = new HashSet<string>();
			  foreach ( JsonNode node in nodes.get( 0 ).get( "labels" ) )
			  {
					labels.Add( node.TextValue );
			  }
			  assertEquals( "labels", asSet( "Foo", "Bar" ), labels );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_serialize_collect_correctly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeCollectCorrectly()
		 {
			  // given
			  _http.POST( "db/data/transaction/commit", SingleStatement( "CREATE (n:Foo)" ) );

			  // when
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'MATCH (n:Foo) RETURN COLLECT(n)' } ] }" ) );

			  // then
			  assertThat( response.Status(), equalTo(200) );

			  JsonNode data = response.Get( "results" ).get( 0 );
			  assertThat( data.get( "columns" ).get( 0 ).asText(), equalTo("COLLECT(n)") );
			  assertThat( data.get( "data" ).get( 0 ).get( "row" ).size(), equalTo(1) );
			  assertThat( data.get( "data" ).get( 0 ).get( "row" ).get( 0 ).get( 0 ).size(), equalTo(0) );

			  assertThat( response.Get( "errors" ).size(), equalTo(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeMapsCorrectlyInRowsFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeMapsCorrectlyInRowsFormat()
		 {
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'RETURN {one:{two:[true, {three: 42}]}}' } ] }" ) );

			  // then
			  assertThat( response.Status(), equalTo(200) );

			  JsonNode data = response.Get( "results" ).get( 0 );
			  JsonNode row = data.get( "data" ).get( 0 ).get( "row" );
			  assertThat( row.size(), equalTo(1) );
			  JsonNode firstCell = row.get( 0 );
			  assertThat( firstCell.get( "one" ).get( "two" ).size(), @is(2) );
			  assertThat( firstCell.get( "one" ).get( "two" ).get( 0 ).asBoolean(), @is(true) );
			  assertThat( firstCell.get( "one" ).get( "two" ).get( 1 ).get( "three" ).asInt(), @is(42) );

			  assertThat( response.Get( "errors" ).size(), equalTo(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeMapsCorrectlyInRestFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeMapsCorrectlyInRestFormat()
		 {
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': " + "'RETURN {one:{two:[true, {three: " + "42}]}}', " + "'resultDataContents':['rest'] } ] " + "}" ) );

			  // then
			  assertThat( response.Status(), equalTo(200) );

			  JsonNode data = response.Get( "results" ).get( 0 );
			  JsonNode rest = data.get( "data" ).get( 0 ).get( "rest" );
			  assertThat( rest.size(), equalTo(1) );
			  JsonNode firstCell = rest.get( 0 );
			  assertThat( firstCell.get( "one" ).get( "two" ).size(), @is(2) );
			  assertThat( firstCell.get( "one" ).get( "two" ).get( 0 ).asBoolean(), @is(true) );
			  assertThat( firstCell.get( "one" ).get( "two" ).get( 1 ).get( "three" ).asInt(), @is(42) );

			  assertThat( response.Get( "errors" ).size(), equalTo(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMapParametersCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMapParametersCorrectly()
		 {
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': " + "'WITH {map} AS map RETURN map[0]', 'parameters':{'map':[{'index':0,'name':'a'},{'index':1,'name':'b'}]} } ] }" ) );

			  // then
			  assertThat( response.Status(), equalTo(200) );

			  JsonNode data = response.Get( "results" ).get( 0 );
			  JsonNode row = data.get( "data" ).get( 0 ).get( "row" );
			  assertThat( row.size(), equalTo(1) );

			  assertThat( row.get( 0 ).get( "index" ).asInt(), equalTo(0) );
			  assertThat( row.get( 0 ).get( "name" ).asText(), equalTo("a") );

			  assertThat( response.Get( "errors" ).size(), equalTo(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void restFormatNodesShouldHaveSensibleUris() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RestFormatNodesShouldHaveSensibleUris()
		 {
			  // given
			  const string hostname = "localhost";
			  const string scheme = "http";

			  // when
			  HTTP.Response rs = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n:Foo:Bar) RETURN n', 'resultDataContents':['rest'] } ] }" ) );

			  // then
			  JsonNode restNode = rs.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "rest" ).get( 0 );

			  AssertPath( restNode.get( "labels" ), "/node/\\d+/labels", hostname, scheme );
			  AssertPath( restNode.get( "outgoing_relationships" ), "/node/\\d+/relationships/out", hostname, scheme );
			  AssertPath( restNode.get( "traverse" ), "/node/\\d+/traverse/\\{returnType\\}", hostname, scheme );
			  AssertPath( restNode.get( "all_typed_relationships" ), "/node/\\d+/relationships/all/\\{-list\\|&\\|types\\}", hostname, scheme );
			  AssertPath( restNode.get( "self" ), "/node/\\d+", hostname, scheme );
			  AssertPath( restNode.get( "property" ), "/node/\\d+/properties/\\{key\\}", hostname, scheme );
			  AssertPath( restNode.get( "properties" ), "/node/\\d+/properties", hostname, scheme );
			  AssertPath( restNode.get( "outgoing_typed_relationships" ), "/node/\\d+/relationships/out/\\{-list\\|&\\|types\\}", hostname, scheme );
			  AssertPath( restNode.get( "incoming_relationships" ), "/node/\\d+/relationships/in", hostname, scheme );
			  AssertPath( restNode.get( "create_relationship" ), "/node/\\d+/relationships", hostname, scheme );
			  AssertPath( restNode.get( "paged_traverse" ), "/node/\\d+/paged/traverse/\\{returnType\\}\\{\\?pageSize," + "leaseTime\\}", "localhost", scheme );
			  AssertPath( restNode.get( "all_relationships" ), "/node/\\d+/relationships/all", hostname, scheme );
			  AssertPath( restNode.get( "incoming_typed_relationships" ), "/node/\\d+/relationships/in/\\{-list\\|&\\|types\\}", hostname, scheme );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void restFormattedNodesShouldHaveSensibleUrisWhenUsingXForwardHeader() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RestFormattedNodesShouldHaveSensibleUrisWhenUsingXForwardHeader()
		 {
			  // given
			  const string hostname = "dummy.example.org";
			  const string scheme = "http";

			  // when
			  HTTP.Response rs = _http.withHeaders( XForwardUtil.X_FORWARD_HOST_HEADER_KEY, hostname ).POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n:Foo:Bar) RETURN n', " + "'resultDataContents':['rest'] } ] }" ) );

			  // then
			  JsonNode restNode = rs.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "rest" ).get( 0 );

			  AssertPath( restNode.get( "labels" ), "/node/\\d+/labels", hostname, scheme );
			  AssertPath( restNode.get( "outgoing_relationships" ), "/node/\\d+/relationships/out", hostname, scheme );
			  AssertPath( restNode.get( "traverse" ), "/node/\\d+/traverse/\\{returnType\\}", hostname, scheme );
			  AssertPath( restNode.get( "all_typed_relationships" ), "/node/\\d+/relationships/all/\\{-list\\|&\\|types\\}", hostname, scheme );
			  AssertPath( restNode.get( "self" ), "/node/\\d+", hostname, scheme );
			  AssertPath( restNode.get( "property" ), "/node/\\d+/properties/\\{key\\}", hostname, scheme );
			  AssertPath( restNode.get( "properties" ), "/node/\\d+/properties", hostname, scheme );
			  AssertPath( restNode.get( "outgoing_typed_relationships" ), "/node/\\d+/relationships/out/\\{-list\\|&\\|types\\}", hostname, scheme );
			  AssertPath( restNode.get( "incoming_relationships" ), "/node/\\d+/relationships/in", hostname, scheme );
			  AssertPath( restNode.get( "create_relationship" ), "/node/\\d+/relationships", hostname, scheme );
			  AssertPath( restNode.get( "paged_traverse" ), "/node/\\d+/paged/traverse/\\{returnType\\}\\{\\?pageSize," + "leaseTime\\}", hostname, scheme );
			  AssertPath( restNode.get( "all_relationships" ), "/node/\\d+/relationships/all", hostname, scheme );
			  AssertPath( restNode.get( "incoming_typed_relationships" ), "/node/\\d+/relationships/in/\\{-list\\|&\\|types\\}", hostname, scheme );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctStatusCodeWhenUsingHintWithoutAnyIndex()
		 public virtual void CorrectStatusCodeWhenUsingHintWithoutAnyIndex()
		 {
			  // begin and execute and commit
			  HTTP.Response begin = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': " + "'MATCH (n:Test) USING INDEX n:Test(foo) WHERE n.foo = 42 RETURN n.foo' } ] }" ) );
			  assertThat( begin, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Request.Schema.IndexNotFound ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transaction_not_in_response_on_failure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionNotInResponseOnFailure()
		 {
			  // begin
			  HTTP.Response begin = _http.POST( "db/data/transaction" );

			  string commitResource = begin.StringFromContent( "commit" );

			  // execute valid statement
			  HTTP.Response valid = _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'RETURN 42' } ] }") );
			  assertThat( valid.Status(), equalTo(200) );
			  assertThat( valid.Get( "transaction" ), notNullValue() );

			  // execute invalid statement
			  HTTP.Response invalid = _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'RETRUN 42' } ] }") );
			  assertThat( invalid.Status(), equalTo(200) );
			  //transaction has been closed and rolled back
			  assertThat( invalid.Get( "transaction" ), nullValue() );

			  // commit
			  HTTP.Response commit = _http.POST( commitResource );

			  //no transaction open anymore, we have failed
			  assertThat( commit.Status(), equalTo(404) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWhenHittingTheASTCacheInCypher() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWhenHittingTheASTCacheInCypher()
		 {
			  // give a cached plan
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", SingleStatement( "MATCH (group:Group {name: \\\"AAA\\\"}) RETURN *" ) );

			  assertThat( response.Status(), equalTo(200) );
			  assertThat( response.Get( "errors" ).size(), equalTo(0) );

			  // when we hit the ast cache
			  response = _http.POST( "db/data/transaction/commit", SingleStatement( "MATCH (group:Group {name: \\\"BBB\\\"}) RETURN *" ) );

			  // then no errors (in particular no NPE)
			  assertThat( response.Status(), equalTo(200) );
			  assertThat( response.Get( "errors" ).size(), equalTo(0) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void assertPath(org.codehaus.jackson.JsonNode jsonURIString, String path, String hostname, final String scheme)
		 private void AssertPath( JsonNode jsonURIString, string path, string hostname, string scheme )
		 {
			  assertTrue( "Expected a uri matching '" + scheme + "://" + hostname + ":\\d+/db/data" + path + "', " + "but got '" + jsonURIString.asText() + "'.", jsonURIString.asText().matches(scheme + "://" + hostname + ":\\d+/db/data" + path) );
		 }

		 private HTTP.RawPayload SingleStatement( string statement )
		 {
			  return rawPayload( "{\"statements\":[{\"statement\":\"" + statement + "\"}]}" );
		 }

		 private long CountNodes( params string[] labels )
		 {
			  ISet<Label> givenLabels = new HashSet<Label>( labels.Length );
			  foreach ( string label in labels )
			  {
					givenLabels.Add( Label.label( label ) );
			  }

			  using ( Transaction transaction = Graphdb().beginTx() )
			  {
					long count = 0;
					foreach ( Node node in Graphdb().AllNodes )
					{
						 ISet<Label> nodeLabels = Iterables.asSet( node.Labels );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
						 if ( nodeLabels.containsAll( givenLabels ) )
						 {
							  count++;
						 }
					}
					transaction.Failure();
					return count;
			  }
		 }

		 private void AssertHasTxLocation( HTTP.Response begin )
		 {
			  assertThat( begin.Location(), matches("http://localhost:\\d+/db/data/transaction/\\d+") );
		 }

		 private void LockNodeWithLabel( Label sharedLockLabel, System.Threading.CountdownEvent nodeLockLatch, System.Threading.CountdownEvent nodeReleaseLatch )
		 {
			  GraphDatabaseService db = Graphdb();
			  try
			  {
					  using ( Transaction ignored = Db.beginTx() )
					  {
						Node node = Db.findNodes( sharedLockLabel ).next();
						node.SetProperty( "a", "b" );
						nodeLockLatch.Signal();
						nodeReleaseLatch.await();
					  }
			  }
			  catch ( InterruptedException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private static void WaitForStatementExecution( string statement )
		 {
			  KernelTransactions kernelTransactions = Server().Database.Graph.DependencyResolver.resolveDependency(typeof(KernelTransactions));
			  while ( !IsStatementExecuting( kernelTransactions, statement ) )
			  {
					Thread.yield();
			  }
		 }

		 private static bool IsStatementExecuting( KernelTransactions kernelTransactions, string statement )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return kernelTransactions.ActiveTransactions().stream().flatMap(KernelTransactionHandle::executingQueries).anyMatch(executingQuery => statement.Equals(executingQuery.queryText()));
		 }
	}

}