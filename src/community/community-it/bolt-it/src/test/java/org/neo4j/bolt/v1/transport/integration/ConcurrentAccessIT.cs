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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgSuccess;

	/// <summary>
	/// Multiple concurrent users should be able to connect simultaneously. We test this with multiple users running
	/// load that they roll back, asserting they don't see each others changes.
	/// </summary>
	public class ConcurrentAccessIT : AbstractBoltTransportsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4NetWithSocket server = new Neo4NetWithSocket(getClass(), settings -> settings.put(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.auth_enabled.name(), "false"));
		 public Neo4NetWithSocket Server = new Neo4NetWithSocket( this.GetType(), settings => settings.put(GraphDatabaseSettings.auth_enabled.name(), "false") );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunSimpleStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunSimpleStatement()
		 {
			  // Given
			  int numWorkers = 5;
			  int numRequests = 1_000;

			  IList<Callable<Void>> workers = CreateWorkers( numWorkers, numRequests );
			  ExecutorService exec = Executors.newFixedThreadPool( numWorkers );

			  try
			  {
					// When & then
					foreach ( Future<Void> f in exec.invokeAll( workers ) )
					{
						 f.get( 60, TimeUnit.SECONDS );
					}
			  }
			  finally
			  {
					exec.shutdownNow();
					exec.awaitTermination( 30, TimeUnit.SECONDS );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<java.util.concurrent.Callable<Void>> createWorkers(int numWorkers, int numRequests) throws Exception
		 private IList<Callable<Void>> CreateWorkers( int numWorkers, int numRequests )
		 {
			  IList<Callable<Void>> workers = new LinkedList<Callable<Void>>();
			  for ( int i = 0; i < numWorkers; i++ )
			  {
					workers.Add( NewWorker( numRequests ) );
			  }
			  return workers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.concurrent.Callable<Void> newWorker(final int iterationsToRun) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private Callable<Void> NewWorker( int iterationsToRun )
		 {
			  return new CallableAnonymousInnerClass( this, iterationsToRun );

		 }

		 private class CallableAnonymousInnerClass : Callable<Void>
		 {
			 private readonly ConcurrentAccessIT _outerInstance;

			 private int _iterationsToRun;

			 public CallableAnonymousInnerClass( ConcurrentAccessIT outerInstance, int iterationsToRun )
			 {
				 this.outerInstance = outerInstance;
				 this._iterationsToRun = iterationsToRun;
				 init = outerInstance.Util.chunk( new InitMessage( "TestClient", emptyMap() ) );
				 createAndRollback = outerInstance.Util.chunk( new RunMessage( "BEGIN" ), PullAllMessage.INSTANCE, new RunMessage( "CREATE (n)" ), PullAllMessage.INSTANCE, new RunMessage( "ROLLBACK" ), PullAllMessage.INSTANCE );
				 matchAll = outerInstance.Util.chunk( new RunMessage( "MATCH (n) RETURN n" ), PullAllMessage.INSTANCE );
			 }

			 private readonly sbyte[] init;
			 private readonly sbyte[] createAndRollback;

			 private readonly sbyte[] matchAll;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void call() throws Exception
			 public override Void call()
			 {
				  // Connect
				  TransportConnection client = outerInstance.NewConnection();
				  client.Connect( _outerInstance.server.lookupDefaultConnector() ).send(_outerInstance.util.defaultAcceptedVersions());
				  assertThat( client, _outerInstance.util.eventuallyReceivesSelectedProtocolVersion() );

				  init( client );

				  for ( int i = 0; i < _iterationsToRun; i++ )
				  {
						createAndRollback( client );
				  }

				  return null;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void init(org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection client) throws Exception
			 private void init( TransportConnection client )
			 {
				  client.Send( init );
				  assertThat( client, _outerInstance.util.eventuallyReceives( msgSuccess() ) );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createAndRollback(org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection client) throws Exception
			 private void createAndRollback( TransportConnection client )
			 {
				  client.Send( createAndRollback );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryMatcher = hasEntry(is("fields"), equalTo(emptyList()));
				  Matcher<IDictionary<string, ?>> entryMatcher = hasEntry( @is( "fields" ), equalTo( emptyList() ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<String,?>> messageMatcher = org.hamcrest.CoreMatchers.allOf(entryMatcher, hasKey("result_available_after"));
				  Matcher<IDictionary<string, ?>> messageMatcher = CoreMatchers.allOf( entryMatcher, hasKey( "result_available_after" ) );
				  assertThat( client, _outerInstance.util.eventuallyReceives( msgSuccess( messageMatcher ), msgSuccess(), msgSuccess(messageMatcher), msgSuccess(), msgSuccess(messageMatcher), msgSuccess() ) );

				  // Verify no visible data
				  client.Send( matchAll );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> fieldsMatcher = hasEntry(is("fields"), equalTo(singletonList("n")));
				  Matcher<IDictionary<string, ?>> fieldsMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "n" ) ) );
				  assertThat( client, _outerInstance.util.eventuallyReceives( msgSuccess( CoreMatchers.allOf( fieldsMatcher, hasKey( "result_available_after" ) ) ), msgSuccess() ) );

			 }
		 }
	}

}