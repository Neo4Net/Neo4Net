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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Test;
	using Neo4Net.Test.rule.concurrent;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.exceptions.Status_Transaction.DeadlockDetected;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.server.HTTP.RawPayload.quotedJson;

	public class DeadlockIT : AbstractRestFunctionalTestBase
	{
		 private readonly HTTP.Builder _http = HTTP.withBaseUri( Server().baseUri() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.concurrent.OtherThreadRule<Object> otherThread = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>();
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