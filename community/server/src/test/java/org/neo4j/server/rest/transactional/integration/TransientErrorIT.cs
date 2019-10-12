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
namespace Org.Neo4j.Server.rest.transactional.integration
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using JsonParseException = Org.Neo4j.Server.rest.domain.JsonParseException;
	using Org.Neo4j.Test.rule.concurrent;
	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.containsNoErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.hasErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.POST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class TransientErrorIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> otherThread = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> OtherThread = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60000) public void deadlockShouldRollbackTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeadlockShouldRollbackTransaction()
		 {
			  // Given
			  HTTP.Response initial = POST( TxCommitUri(), quotedJson("{'statements': [{'statement': 'CREATE (n1 {prop : 1}), (n2 {prop : 2})'}]}") );
			  assertThat( initial.Status(), @is(200) );
			  assertThat( initial, containsNoErrors() );

			  // When

			  // tx1 takes a write lock on node1
			  HTTP.Response firstInTx1 = POST( TxUri(), quotedJson("{'statements': [{'statement': 'MATCH (n {prop : 1}) SET n.prop = 3'}]}") );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long tx1 = extractTxId(firstInTx1);
			  long tx1 = ExtractTxId( firstInTx1 );

			  // tx2 takes a write lock on node2
			  HTTP.Response firstInTx2 = POST( TxUri(), quotedJson("{'statements': [{'statement': 'MATCH (n {prop : 2}) SET n.prop = 4'}]}") );
			  long tx2 = ExtractTxId( firstInTx2 );

			  // tx1 attempts to take a write lock on node2
			  Future<HTTP.Response> future = OtherThread.execute( state => POST( TxUri( tx1 ), quotedJson( "{'statements': [{'statement': 'MATCH (n {prop : 2}) SET n.prop = 5'}]}" ) ) );

			  // tx2 attempts to take a write lock on node1
			  HTTP.Response secondInTx2 = POST( TxUri( tx2 ), quotedJson( "{'statements': [{'statement': 'MATCH (n {prop : 1}) SET n.prop = 6'}]}" ) );

			  HTTP.Response secondInTx1 = future.get();

			  // Then
			  assertThat( secondInTx1.Status(), @is(200) );
			  assertThat( secondInTx2.Status(), @is(200) );

			  // either tx1 or tx2 should fail because of the deadlock
			  HTTP.Response failed;
			  if ( ContainsError( secondInTx1 ) )
			  {
					failed = secondInTx1;
			  }
			  else if ( ContainsError( secondInTx2 ) )
			  {
					failed = secondInTx2;
			  }
			  else
			  {
					failed = null;
					fail( "Either tx1 or tx2 is expected to fail" );
			  }

			  assertThat( failed, hasErrors( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.DeadlockDetected ) );

			  // transaction was rolled back on the previous step and we can't commit it
			  HTTP.Response commit = POST( failed.StringFromContent( "commit" ) );
			  assertThat( commit.Status(), @is(404) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unavailableCsvResourceShouldRollbackTransaction() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnavailableCsvResourceShouldRollbackTransaction()
		 {
			  // Given
			  HTTP.Response first = POST( TxUri(), quotedJson("{'statements': [{'statement': 'CREATE ()'}]}") );
			  assertThat( first.Status(), @is(201) );
			  assertThat( first, containsNoErrors() );

			  long txId = ExtractTxId( first );

			  // When
			  HTTP.Response second = POST( TxUri( txId ), quotedJson( "{'statements': [{'statement': 'LOAD CSV FROM \\\"http://127.0.0.1/null/\\\" AS line " + "CREATE (a {name:line[0]})'}]}" ) );

			  // Then

			  // request fails because specified CSV resource is invalid
			  assertThat( second.Status(), @is(200) );
			  assertThat( second, hasErrors( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.ExternalResourceFailed ) );

			  // transaction was rolled back on the previous step and we can't commit it
			  HTTP.Response commit = POST( second.StringFromContent( "commit" ) );
			  assertThat( commit.Status(), @is(404) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static boolean containsError(org.neo4j.test.server.HTTP.Response response) throws org.neo4j.server.rest.domain.JsonParseException
		 private static bool ContainsError( HTTP.Response response )
		 {
			  return response.Get( "errors" ).GetEnumerator().hasNext();
		 }
	}

}