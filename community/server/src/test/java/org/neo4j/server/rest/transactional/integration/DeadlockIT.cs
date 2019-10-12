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
namespace Org.Neo4j.Server.rest.transactional.integration
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Org.Neo4j.Graphdb.Label;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Test;
	using Org.Neo4j.Test.rule.concurrent;
	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.DeadlockDetected;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class DeadlockIT : AbstractRestFunctionalTestBase
	{
		 private readonly HTTP.Builder _http = HTTP.withBaseUri( Server().baseUri() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.concurrent.OtherThreadRule<Object> otherThread = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public OtherThreadRule<object> OtherThread = new OtherThreadRule<object>();

		 private readonly System.Threading.CountdownEvent _secondNodeLocked = new System.Threading.CountdownEvent( 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCorrectStatusCodeOnDeadlock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnCorrectStatusCodeOnDeadlock()
		 {
			  // Given
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					Graphdb().createNode(Label.label("First"));
					Graphdb().createNode(Label.label("Second"));
					tx.Success();
			  }

			  // When I lock node:First
			  HTTP.Response begin = _http.POST( "db/data/transaction", quotedJson( "{ 'statements': [ { 'statement': 'MATCH (n:First) SET n.prop=1' } ] }" ) );

			  // and I lock node:Second, and wait for a lock on node:First in another transaction
			  OtherThread.execute( WriteToFirstAndSecond() );

			  // and I wait for those locks to be pending
			  assertTrue( _secondNodeLocked.await( 10, TimeUnit.SECONDS ) );
			  Thread.Sleep( 1000 );

			  // and I then try and lock node:Second in the first transaction
			  HTTP.Response deadlock = _http.POST( begin.Location(), quotedJson("{ 'statements': [ { 'statement': 'MATCH (n:Second) SET n.prop=1' } ] }") );

			  // Then
			  assertThat( deadlock.Get( "errors" ).get( 0 ).get( "code" ).TextValue, equalTo( DeadlockDetected.code().serialize() ) );
		 }

		 private OtherThreadExecutor.WorkerCommand<object, object> WriteToFirstAndSecond()
		 {
			  return state =>
			  {
				HTTP.Response post = _http.POST( "db/data/transaction", quotedJson( "{ 'statements': [ { 'statement': 'MATCH (n:Second) SET n.prop=1' } ] }" ) );
				_secondNodeLocked.Signal();
				_http.POST( post.location(), quotedJson("{ 'statements': [ { 'statement': 'MATCH (n:First) SET n.prop=1' } ] }") );
				return null;
			  };
		 }
	}

}