using System.IO;

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
namespace Neo4Net.Server.rest.transactional.integration
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Test = org.junit.Test;


	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Request.InvalidFormat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Statement.SyntaxError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.containsNoStackTraces;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.hasErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.POST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.rawPayload;

	/// <summary>
	/// Tests for error messages and graceful handling of problems with the transactional endpoint.
	/// </summary>
	public class TransactionErrorIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__commit_with_invalid_cypher() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_CommitWithInvalidCypher()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin
			  HTTP.Response response = POST( TxUri(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ] }") );
			  string commitResource = response.StringFromContent( "commit" );

			  // commit with invalid cypher
			  response = POST( commitResource, quotedJson( "{ 'statements': [ { 'statement': 'CREATE ;;' } ] }" ) );

			  assertThat( response.Status(), @is(200) );
			  assertThat( response, hasErrors( SyntaxError ) );
			  assertThat( response, containsNoStackTraces() );

			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void begin__commit_with_malformed_json() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Begin_CommitWithMalformedJson()
		 {
			  long nodesInDatabaseBeforeTransaction = CountNodes();

			  // begin
			  HTTP.Response begin = POST( TxUri(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ] }") );
			  string commitResource = begin.StringFromContent( "commit" );

			  // commit with malformed json
			  HTTP.Response response = POST( commitResource, rawPayload( "[{asd,::}]" ) );

			  assertThat( response.Status(), @is(200) );
			  assertThat( response, hasErrors( InvalidFormat ) );

			  assertThat( CountNodes(), equalTo(nodesInDatabaseBeforeTransaction) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ResultOfMethodCallIgnored") @Test public void begin_and_execute_periodic_commit_that_fails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginAndExecutePeriodicCommitThatFails()
		 {
			  File file = File.createTempFile( "begin_and_execute_periodic_commit_that_fails", ".csv" ).AbsoluteFile;
			  try
			  {
					PrintStream @out = new PrintStream( new FileStream( file, FileMode.Create, FileAccess.Write ) );
					@out.println( "1" );
					@out.println( "2" );
					@out.println( "0" );
					@out.println( "3" );
					@out.close();

					string url = file.toURI().toURL().ToString().Replace("\\", "\\\\");
					string query = "USING PERIODIC COMMIT 1 LOAD CSV FROM \\\"" + url + "\\\" AS line CREATE ({name: 1/toInt(line[0])});";

					// begin and execute and commit
					HTTP.RawPayload payload = quotedJson( "{ 'statements': [ { 'statement': '" + query + "' } ] }" );
					HTTP.Response response = POST( TxCommitUri(), payload );

					assertThat( response.Status(), equalTo(200) );
					assertThat( response, hasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ArithmeticError ) );

					JsonNode message = response.Get( "errors" ).get( 0 ).get( "message" );
					assertTrue( "Expected LOAD CSV line number information", message.ToString().Contains("on line 3. Possibly the last row committed during import is line 2. " + "Note that this information might not be accurate.") );
			  }
			  finally
			  {
					file.delete();
			  }
		 }

		 private long CountNodes()
		 {
			  return TransactionMatchers.CountNodes( Graphdb() );
		 }
	}

}