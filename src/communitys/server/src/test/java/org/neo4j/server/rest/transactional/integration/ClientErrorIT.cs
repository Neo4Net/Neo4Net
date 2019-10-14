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
namespace Neo4Net.Server.rest.transactional.integration
{
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.containsNoErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.hasErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.POST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ClientErrorIT extends org.neo4j.server.rest.AbstractRestFunctionalTestBase
	public class ClientErrorIT : AbstractRestFunctionalTestBase
	{
		 private const int UNIQUE_ISBN = 12345;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String query;
		 public string Query;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public org.neo4j.kernel.api.exceptions.Status errorStatus;
		 public Status ErrorStatus;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0} should cause {1}") public static java.util.List<Object[]> queriesWithStatuses()
		 public static IList<object[]> QueriesWithStatuses()
		 {
			  return Arrays.asList( new object[]{ "Not a valid query", Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError }, new object[]{ "RETURN {foo}", Neo4Net.Kernel.Api.Exceptions.Status_Statement.ParameterMissing }, new object[]{ "MATCH (n) WITH n.prop AS n2 RETURN n2.prop", Neo4Net.Kernel.Api.Exceptions.Status_Statement.TypeError }, new object[]{ "CYPHER 1.9 EXPLAIN MATCH n RETURN n", Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError }, new object[]{ "RETURN 10 / 0", Neo4Net.Kernel.Api.Exceptions.Status_Statement.ArithmeticError }, new object[]{ "CREATE INDEX ON :Person(name)", Neo4Net.Kernel.Api.Exceptions.Status_Transaction.ForbiddenDueToTransactionType }, new object[]{ "CREATE (n:``)", Neo4Net.Kernel.Api.Exceptions.Status_Schema.TokenNameError }, new object[]{ "CREATE (b:Book {isbn: " + UNIQUE_ISBN + "})", Neo4Net.Kernel.Api.Exceptions.Status_Schema.ConstraintValidationFailed }, new object[]{ "LOAD CSV FROM 'http://127.0.0.1/null/' AS line CREATE (a {name:line[0]})", Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void prepareDatabase()
		 public static void PrepareDatabase()
		 {
			  POST( TxCommitUri(), quotedJson("{'statements': [{'statement': 'CREATE INDEX ON :Book(name)'}]}") );

			  POST( TxCommitUri(), quotedJson("{'statements': [{'statement': 'CREATE CONSTRAINT ON (b:Book) ASSERT b.isbn IS UNIQUE'}]}") );

			  POST( TxCommitUri(), quotedJson("{'statements': [{'statement': 'CREATE (b:Book {isbn: " + UNIQUE_ISBN + "})'}]}") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientErrorShouldRollbackTheTransaction() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClientErrorShouldRollbackTheTransaction()
		 {
			  // Given
			  HTTP.Response first = POST( TxUri(), quotedJson("{'statements': [{'statement': 'CREATE (n {prop : 1})'}]}") );
			  assertThat( first.Status(), @is(201) );
			  assertThat( first, containsNoErrors() );
			  long txId = ExtractTxId( first );

			  // When
			  HTTP.Response malformed = POST( TxUri( txId ), quotedJson( "{'statements': [{'statement': '" + Query + "'}]}" ) );

			  // Then

			  // malformed POST contains expected error
			  assertThat( malformed.Status(), @is(200) );
			  assertThat( malformed, hasErrors( ErrorStatus ) );

			  // transaction was rolled back on the previous step and we can't commit it
			  HTTP.Response commit = POST( first.StringFromContent( "commit" ) );
			  assertThat( commit.Status(), @is(404) );
		 }
	}

}