using System;
using System.Collections.Generic;
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
namespace Neo4Net.Cypher
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CypherCacheHitMonitor = Neo4Net.Cypher.Internal.compatibility.CypherCacheHitMonitor;
	using Label = Neo4Net.GraphDb.Label;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4Net.Helpers.Collections;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class QueryInvalidationIT
	{
		 private const int USERS = 10;
		 private const int CONNECTIONS = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.ImpermanentDatabaseRule().withSetting(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.query_statistics_divergence_threshold, "0.5").withSetting(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.cypher_min_replan_interval, "1s");
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule().withSetting(GraphDatabaseSettings.query_statistics_divergence_threshold, "0.5").withSetting(GraphDatabaseSettings.cypher_min_replan_interval, "1s");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRePlanAfterDataChangesFromAnEmptyDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRePlanAfterDataChangesFromAnEmptyDatabase()
		 {
			  // GIVEN
			  TestMonitor monitor = new TestMonitor();
			  Db.resolveDependency( typeof( Monitors ) ).addMonitorListener( monitor );
			  // - setup schema -
			  CreateIndex();
			  // - execute the query without the existence data -
			  ExecuteDistantFriendsCountQuery( USERS );

			  long replanTime = DateTimeHelper.CurrentUnixTimeMillis() + 1_800;

			  // - create data -
			  CreateData( 0, USERS, CONNECTIONS );

			  // - after the query TTL has expired -
			  while ( DateTimeHelper.CurrentUnixTimeMillis() < replanTime )
			  {
					Thread.Sleep( 100 );
			  }

			  // WHEN
			  monitor.Reset();
			  // - execute the query again -
			  ExecuteDistantFriendsCountQuery( USERS );

			  // THEN
			  assertEquals( "Query should have been replanned.", 1, monitor.Discards.get() );
			  assertThat( "Replan should have occurred after TTL", monitor.WaitTime.get(), greaterThanOrEqualTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRePlanAfterDataChangesFromAPopulatedDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRePlanAfterDataChangesFromAPopulatedDatabase()
		 {
			  // GIVEN
			  Config config = Db.DependencyResolver.resolveDependency( typeof( Config ) );
			  double divergenceThreshold = config.Get( GraphDatabaseSettings.query_statistics_divergence_threshold );
			  long replanInterval = config.Get( GraphDatabaseSettings.cypher_min_replan_interval ).toMillis();

			  TestMonitor monitor = new TestMonitor();
			  Db.resolveDependency( typeof( Monitors ) ).addMonitorListener( monitor );
			  // - setup schema -
			  CreateIndex();
			  //create some data
			  CreateData( 0, USERS, CONNECTIONS );
			  ExecuteDistantFriendsCountQuery( USERS );

			  long replanTime = DateTimeHelper.CurrentUnixTimeMillis() + replanInterval;

			  assertTrue( "Test does not work with edge setting for query_statistics_divergence_threshold: " + divergenceThreshold, divergenceThreshold > 0.0 && divergenceThreshold < 1.0 );

			  int usersToCreate = ( ( int )( Math.Ceiling( ( ( double ) USERS ) / ( 1.0 - divergenceThreshold ) ) ) ) - USERS + 1;

			  //create more data
			  CreateData( USERS, usersToCreate, CONNECTIONS );

			  // - after the query TTL has expired -
			  while ( DateTimeHelper.CurrentUnixTimeMillis() <= replanTime )
			  {
					Thread.Sleep( 100 );
			  }

			  // WHEN
			  monitor.Reset();
			  // - execute the query again -
			  ExecuteDistantFriendsCountQuery( USERS );

			  // THEN
			  assertEquals( "Query should have been replanned.", 1, monitor.Discards.get() );
			  assertThat( "Replan should have occurred after TTL", monitor.WaitTime.get(), greaterThanOrEqualTo(replanInterval / 1000) );
		 }

		 private void CreateIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(Label.label("User")).on("userId").create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, SECONDS);
					tx.Success();
			  }
		 }

		 private void CreateData( long startingUserId, int numUsers, int numConnections )
		 {
			  for ( long userId = startingUserId; userId < numUsers + startingUserId; userId++ )
			  {
					Db.execute( "CREATE (newUser:User {userId: {userId}})", singletonMap( "userId", userId ) );
			  }
			  IDictionary<string, object> @params = new Dictionary<string, object>();
			  for ( int i = 0; i < numConnections; i++ )
			  {
					long user1 = startingUserId + RandomInt( numUsers );
					long user2;
					do
					{
						 user2 = startingUserId + RandomInt( numUsers );
					} while ( user1 == user2 );
					@params["user1"] = user1;
					@params["user2"] = user2;
					Db.execute( "MATCH (user1:User { userId: {user1} }), (user2:User { userId: {user2} }) " + "MERGE (user1) -[:FRIEND]- (user2)", @params );
			  }
		 }

		 private void ExecuteDistantFriendsCountQuery( int userId )
		 {
			  IDictionary<string, object> @params = singletonMap( "userId", ( long ) RandomInt( userId ) );

			  using ( Result result = Db.execute( "MATCH (user:User { userId: {userId} } ) -[:FRIEND]- () -[:FRIEND]- (distantFriend) " + "RETURN COUNT(distinct distantFriend)", @params ) )
			  {
					while ( result.MoveNext() )
					{
						 result.Current;
					}
			  }
		 }

		 private static int RandomInt( int max )
		 {
			  return ThreadLocalRandom.current().Next(max);
		 }

		 private class TestMonitor : CypherCacheHitMonitor<Pair<string, scala.collection.immutable.Map<string, Type>>>
		 {
			  internal readonly AtomicInteger Hits = new AtomicInteger();
			  internal readonly AtomicInteger Misses = new AtomicInteger();
			  internal readonly AtomicInteger Discards = new AtomicInteger();
			  internal readonly AtomicInteger Recompilations = new AtomicInteger();
			  internal readonly AtomicLong WaitTime = new AtomicLong();

			  public override void CacheHit( Pair<string, scala.collection.immutable.Map<string, Type>> key )
			  {
					Hits.incrementAndGet();
			  }

			  public override void CacheMiss( Pair<string, scala.collection.immutable.Map<string, Type>> key )
			  {
					Misses.incrementAndGet();
			  }

			  public override void CacheDiscard( Pair<string, scala.collection.immutable.Map<string, Type>> key, string ignored, int secondsSinceReplan )
			  {
					Discards.incrementAndGet();
					WaitTime.addAndGet( secondsSinceReplan );
			  }

			  public override void CacheRecompile( Pair<string, scala.collection.immutable.Map<string, Type>> key )
			  {
					Recompilations.incrementAndGet();
			  }

			  public override string ToString()
			  {
					return "TestMonitor{hits=" + Hits + ", misses=" + Misses + ", discards=" + Discards + ", waitTime=" + WaitTime + ", recompilations=" + Recompilations + "}";
			  }

			  public virtual void Reset()
			  {
					Hits.set( 0 );
					Recompilations.set( 0 );
					Misses.set( 0 );
					Discards.set( 0 );
					WaitTime.set( 0 );
			  }
		 }
	}

}