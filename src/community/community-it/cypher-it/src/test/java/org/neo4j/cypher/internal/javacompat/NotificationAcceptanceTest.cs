using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Cypher.Internal.javacompat
{
	using Test = org.junit.Test;


	using InputPosition = Neo4Net.GraphDb.InputPosition;
	using Notification = Neo4Net.GraphDb.Notification;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.impl.notification.NotificationCode.CREATE_UNIQUE_UNAVAILABLE_FALLBACK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.impl.notification.NotificationCode.EAGER_LOAD_CSV;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.impl.notification.NotificationCode.INDEX_HINT_UNFULFILLABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.impl.notification.NotificationCode.LENGTH_ON_NON_PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.impl.notification.NotificationCode.RUNTIME_UNSUPPORTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.impl.notification.NotificationCode.UNBOUNDED_SHORTEST_PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.impl.notification.NotificationDetail_Factory.index;

	public class NotificationAcceptanceTest : NotificationTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyWhenUsingCypher3_1ForTheRulePlannerWhenCypherVersionIsTheDefault()
		 public virtual void ShouldNotifyWhenUsingCypher3_1ForTheRulePlannerWhenCypherVersionIsTheDefault()
		 {
			  // when
			  Result result = Db().execute("CYPHER planner=rule RETURN 1");
			  InputPosition position = InputPosition.empty;

			  // then
			  assertThat( result.Notifications, ContainsItem( RulePlannerUnavailable ) );
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;
			  assertThat( arguments["version"], equalTo( "CYPHER 3.1" ) );
			  assertThat( arguments["planner"], equalTo( "RULE" ) );
			  result.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnWhenRequestingCompiledRuntimeOnUnsupportedQuery()
		 public virtual void ShouldWarnWhenRequestingCompiledRuntimeOnUnsupportedQuery()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotifyInStream( version, "EXPLAIN CYPHER runtime=compiled MATCH (a)-->(b), (c)-->(d) RETURN count(*)", InputPosition.empty, RUNTIME_UNSUPPORTED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnWhenRequestingSlottedRuntimeOnUnsupportedQuery()
		 public virtual void ShouldWarnWhenRequestingSlottedRuntimeOnUnsupportedQuery()
		 {
			  Stream.of( "CYPHER 3.5" ).forEach( version => shouldNotifyInStream( version, "explain cypher runtime=slotted merge (a)-[:X]->(b)", InputPosition.empty, RUNTIME_UNSUPPORTED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyWhenUsingCreateUniqueWhenCypherVersionIsDefault()
		 public virtual void ShouldNotifyWhenUsingCreateUniqueWhenCypherVersionIsDefault()
		 {
			  // when
			  Result result = Db().execute("EXPLAIN MATCH (b) WITH b LIMIT 1 CREATE UNIQUE (b)-[:REL]->()");
			  InputPosition position = new InputPosition( 33, 1, 34 );

			  // then
			  assertThat( result.Notifications, ContainsNotification( CREATE_UNIQUE_UNAVAILABLE_FALLBACK.notification( position ) ) );
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;
			  assertThat( arguments["version"], equalTo( "CYPHER 3.1" ) );
			  result.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyWhenUsingCreateUniqueWhenCypherVersionIs3_5()
		 public virtual void ShouldNotifyWhenUsingCreateUniqueWhenCypherVersionIs3_5()
		 {
			  // when
			  Result result = Db().execute("EXPLAIN CYPHER 3.5 MATCH (b) WITH b LIMIT 1 CREATE UNIQUE (b)-[:REL]->()");
			  InputPosition position = new InputPosition( 44, 1, 45 );

			  // then
			  assertThat( result.Notifications, ContainsNotification( CREATE_UNIQUE_UNAVAILABLE_FALLBACK.notification( position ) ) );
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;
			  assertThat( arguments["version"], equalTo( "CYPHER 3.1" ) );
			  result.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetErrorWhenUsingCreateUniqueWhenCypherVersionIs3_4()
		 public virtual void ShouldGetErrorWhenUsingCreateUniqueWhenCypherVersionIs3_4()
		 {
			  // expect exception
			  Thrown.expect( typeof( QueryExecutionException ) );
			  Thrown.expectMessage( "CREATE UNIQUE is no longer supported. You can achieve the same result using MERGE" );

			  // when
			  Db().execute("CYPHER 3.4 MATCH (b) WITH b LIMIT 1 CREATE UNIQUE (b)-[:REL]->()");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnWhenUsingLengthOnNonPath()
		 public virtual void ShouldWarnWhenUsingLengthOnNonPath()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				// pattern
				ShouldNotifyInStream( version, "explain match (a) where a.name='Alice' return length((a)-->()-->())", new InputPosition( 63, 1, 64 ), LENGTH_ON_NON_PATH );

				// collection
				ShouldNotifyInStream( version, " explain return length([1, 2, 3])", new InputPosition( 33, 1, 34 ), LENGTH_ON_NON_PATH );

				// string
				ShouldNotifyInStream( version, " explain return length('a string')", new InputPosition( 33, 1, 34 ), LENGTH_ON_NON_PATH );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyWhenUsingLengthOnPath()
		 public virtual void ShouldNotNotifyWhenUsingLengthOnPath()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, " explain match p=(a)-[*]->(b) return length(p)" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyWhenUsingSizeOnCollection()
		 public virtual void ShouldNotNotifyWhenUsingSizeOnCollection()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, "explain return size([1, 2, 3])" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyWhenUsingSizeOnString()
		 public virtual void ShouldNotNotifyWhenUsingSizeOnString()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, " explain return size('a string')" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyForCostUnsupportedUpdateQueryIfPlannerNotExplicitlyRequested()
		 public virtual void ShouldNotNotifyForCostUnsupportedUpdateQueryIfPlannerNotExplicitlyRequested()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, " EXPLAIN MATCH (n:Movie) SET n.title = 'The Movie'" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyForCostSupportedUpdateQuery()
		 public virtual void ShouldNotNotifyForCostSupportedUpdateQuery()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				ShouldNotNotifyInStream( version, "EXPLAIN CYPHER planner=cost MATCH (n:Movie) SET n:Seen" );
				ShouldNotNotifyInStream( version, "EXPLAIN CYPHER planner=idp MATCH (n:Movie) SET n:Seen" );
				ShouldNotNotifyInStream( version, "EXPLAIN CYPHER planner=dp MATCH (n:Movie) SET n:Seen" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyUsingJoinHintWithCost()
		 public virtual void ShouldNotNotifyUsingJoinHintWithCost()
		 {
			  IList<string> queries = Arrays.asList( "CYPHER planner=cost EXPLAIN MATCH (a)-->(b) USING JOIN ON b RETURN a, b", "CYPHER planner=cost EXPLAIN MATCH (a)-->(x)<--(b) USING JOIN ON x RETURN a, b" );

			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				foreach ( string query in queries )
				{
					 AssertNotifications( version + query, ContainsNoItem( JoinHintUnsupportedWarning ) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnPotentiallyCachedQueries()
		 public virtual void ShouldWarnOnPotentiallyCachedQueries()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				AssertNotifications( version + "explain match (a)-->(b), (c)-->(d) return *", ContainsItem( CartesianProductWarning ) );

				// no warning without explain
				ShouldNotNotifyInStream( version, "match (a)-->(b), (c)-->(d) return *" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnceWhenSingleIndexHintCannotBeFulfilled()
		 public virtual void ShouldWarnOnceWhenSingleIndexHintCannotBeFulfilled()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotifyInStreamWithDetail( version, " EXPLAIN MATCH (n:Person) USING INDEX n:Person(name) WHERE n.name = 'John' RETURN n", InputPosition.empty, INDEX_HINT_UNFULFILLABLE, index( "Person", "name" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnEachUnfulfillableIndexHint()
		 public virtual void ShouldWarnOnEachUnfulfillableIndexHint()
		 {
			  string query = " EXPLAIN MATCH (n:Person), (m:Party), (k:Animal) " + "USING INDEX n:Person(name) " + "USING INDEX m:Party(city) " +
						 "USING INDEX k:Animal(species) " + "WHERE n.name = 'John' AND m.city = 'Reykjavik' AND k.species = 'Sloth' " + "RETURN n";

			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				ShouldNotifyInStreamWithDetail( version, query, InputPosition.empty, INDEX_HINT_UNFULFILLABLE, index( "Person", "name" ) );
				ShouldNotifyInStreamWithDetail( version, query, InputPosition.empty, INDEX_HINT_UNFULFILLABLE, index( "Party", "city" ) );
				ShouldNotifyInStreamWithDetail( version, query, InputPosition.empty, INDEX_HINT_UNFULFILLABLE, index( "Animal", "species" ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnLiteralMaps()
		 public virtual void ShouldNotNotifyOnLiteralMaps()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, " explain return { id: 42 } " ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnNonExistingLabelUsingLoadCSV()
		 public virtual void ShouldNotNotifyOnNonExistingLabelUsingLoadCSV()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				// create node
				ShouldNotNotifyInStream( version, " EXPLAIN LOAD CSV WITH HEADERS FROM 'file:///fake.csv' AS row CREATE (n:Category)" );

				// merge node
				ShouldNotNotifyInStream( version, " EXPLAIN LOAD CSV WITH HEADERS FROM 'file:///fake.csv' AS row MERGE (n:Category)" );

				// set label to node
				ShouldNotNotifyInStream( version, " EXPLAIN LOAD CSV WITH HEADERS FROM 'file:///fake.csv' AS row CREATE (n) SET n:Category" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnNonExistingRelTypeUsingLoadCSV()
		 public virtual void ShouldNotNotifyOnNonExistingRelTypeUsingLoadCSV()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				// create rel
				ShouldNotNotifyInStream( version, " EXPLAIN LOAD CSV WITH HEADERS FROM 'file:///fake.csv' AS row CREATE ()-[:T]->()" );

				// merge rel
				ShouldNotNotifyInStream( version, " EXPLAIN LOAD CSV WITH HEADERS FROM 'file:///fake.csv' AS row MERGE ()-[:T]->()" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnNonExistingPropKeyIdUsingLoadCSV()
		 public virtual void ShouldNotNotifyOnNonExistingPropKeyIdUsingLoadCSV()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				// create node
				ShouldNotNotifyInStream( version, " EXPLAIN LOAD CSV WITH HEADERS FROM 'file:///fake.csv' AS row CREATE (n) SET n.p = 'a'" );

				// merge node
				ShouldNotNotifyInStream( version, " EXPLAIN LOAD CSV WITH HEADERS FROM 'file:///fake.csv' AS row MERGE (n) ON CREATE SET n.p = 'a'" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnEagerBeforeLoadCSVDelete()
		 public virtual void ShouldNotNotifyOnEagerBeforeLoadCSVDelete()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, "EXPLAIN MATCH (n) DELETE n WITH * LOAD CSV FROM 'file:///ignore/ignore.csv' AS line MERGE () RETURN line" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnEagerBeforeLoadCSVCreate()
		 public virtual void ShouldNotNotifyOnEagerBeforeLoadCSVCreate()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => assertNotifications( version + "EXPLAIN MATCH (a), (b) CREATE (c) WITH c LOAD CSV FROM 'file:///ignore/ignore.csv' AS line RETURN *", ContainsNoItem( EagerOperatorWarning ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnEagerAfterLoadCSV()
		 public virtual void ShouldWarnOnEagerAfterLoadCSV()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotifyInStream( version, "EXPLAIN MATCH (n) LOAD CSV FROM 'file:///ignore/ignore.csv' AS line WITH * DELETE n MERGE () RETURN line", InputPosition.empty, EAGER_LOAD_CSV ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnLoadCSVWithoutEager()
		 public virtual void ShouldNotNotifyOnLoadCSVWithoutEager()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, "EXPLAIN LOAD CSV FROM 'file:///ignore/ignore.csv' AS line MATCH (:A) CREATE (:B) RETURN line" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnEagerWithoutLoadCSV()
		 public virtual void ShouldNotNotifyOnEagerWithoutLoadCSV()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => assertNotifications( version + "EXPLAIN MATCH (a), (b) CREATE (c) RETURN *", ContainsNoItem( EagerOperatorWarning ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnLargeLabelScansWithLoadCVSMatch()
		 public virtual void ShouldWarnOnLargeLabelScansWithLoadCVSMatch()
		 {
			  for ( int i = 0; i < 11; i++ )
			  {
					using ( Transaction tx = Db().beginTx() )
					{
						 Db().createNode().addLabel(label("A"));
						 tx.Success();
					}
			  }
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => assertNotifications( version + "EXPLAIN LOAD CSV FROM 'file:///ignore/ignore.csv' AS line MATCH (a:A) RETURN *", ContainsNoItem( LargeLabelCSVWarning ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnLargeLabelScansWithLoadCVSMerge()
		 public virtual void ShouldWarnOnLargeLabelScansWithLoadCVSMerge()
		 {
			  for ( int i = 0; i < 11; i++ )
			  {
					using ( Transaction tx = Db().beginTx() )
					{
						 Db().createNode().addLabel(label("A"));
						 tx.Success();
					}
			  }
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => assertNotifications( version + "EXPLAIN LOAD CSV FROM 'file:///ignore/ignore.csv' AS line MERGE (a:A) RETURN *", ContainsNoItem( LargeLabelCSVWarning ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWarnOnSmallLabelScansWithLoadCVS()
		 public virtual void ShouldNotWarnOnSmallLabelScansWithLoadCVS()
		 {
			  using ( Transaction tx = Db().beginTx() )
			  {
					Db().createNode().addLabel(label("A"));
					tx.Success();
			  }
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				ShouldNotNotifyInStream( version, "EXPLAIN LOAD CSV FROM 'file:///ignore/ignore.csv' AS line MATCH (a:A) RETURN *" );
				ShouldNotNotifyInStream( version, "EXPLAIN LOAD CSV FROM 'file:///ignore/ignore.csv' AS line MERGE (a:A) RETURN *" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnUnboundedShortestPath()
		 public virtual void ShouldWarnOnUnboundedShortestPath()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotifyInStream( version, "EXPLAIN MATCH p = shortestPath((n)-[*]->(m)) RETURN m", new InputPosition( 44, 1, 45 ), UNBOUNDED_SHORTEST_PATH ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnDynamicPropertyLookupWithNoLabels()
		 public virtual void ShouldNotNotifyOnDynamicPropertyLookupWithNoLabels()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				Db().execute("CREATE INDEX ON :Person(name)");
				Db().execute("Call db.awaitIndexes()");
				ShouldNotNotifyInStream( version, "EXPLAIN MATCH (n) WHERE n['key-' + n.name] = 'value' RETURN n" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnDynamicPropertyLookupWithBothStaticAndDynamicProperties()
		 public virtual void ShouldWarnOnDynamicPropertyLookupWithBothStaticAndDynamicProperties()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				Db().execute("CREATE INDEX ON :Person(name)");
				Db().execute("Call db.awaitIndexes()");
				AssertNotifications( version + "EXPLAIN MATCH (n:Person) WHERE n.name = 'Tobias' AND n['key-' + n.name] = 'value' RETURN n", ContainsItem( DynamicPropertyWarning ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnDynamicPropertyLookupWithLabelHavingNoIndex()
		 public virtual void ShouldNotNotifyOnDynamicPropertyLookupWithLabelHavingNoIndex()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				Db().execute("CREATE INDEX ON :Person(name)");
				Db().execute("Call db.awaitIndexes()");
				using ( Transaction tx = Db().beginTx() )
				{
					 Db().createNode().addLabel(label("Foo"));
					 tx.success();
				}
				ShouldNotNotifyInStream( version, "EXPLAIN MATCH (n:Foo) WHERE n['key-' + n.name] = 'value' RETURN n" );

			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnUnfulfillableIndexSeekUsingDynamicProperty()
		 public virtual void ShouldWarnOnUnfulfillableIndexSeekUsingDynamicProperty()
		 {
			  IList<string> queries = new List<string>();

			  // dynamic property lookup with single label
			  queries.Add( "EXPLAIN MATCH (n:Person) WHERE n['key-' + n.name] = 'value' RETURN n" );

			  // dynamic property lookup with explicit label check
			  queries.Add( "EXPLAIN MATCH (n) WHERE n['key-' + n.name] = 'value' AND (n:Person) RETURN n" );

			  // dynamic property lookup with range seek
			  queries.Add( "EXPLAIN MATCH (n:Person) WHERE n['key-' + n.name] > 10 RETURN n" );

			  // dynamic property lookup with range seek (reverse)
			  queries.Add( "EXPLAIN MATCH (n:Person) WHERE 10 > n['key-' + n.name] RETURN n" );

			  // dynamic property lookup with a single label and property existence check with exists
			  queries.Add( "EXPLAIN MATCH (n:Person) WHERE exists(n['na' + 'me']) RETURN n" );

			  // dynamic property lookup with a single label and starts with
			  queries.Add( "EXPLAIN MATCH (n:Person) WHERE n['key-' + n.name] STARTS WITH 'Foo' RETURN n" );

			  // dynamic property lookup with a single label and regex
			  queries.Add( "EXPLAIN MATCH (n:Person) WHERE n['key-' + n.name] =~ 'Foo*' RETURN n" );

			  // dynamic property lookup with a single label and IN
			  queries.Add( "EXPLAIN MATCH (n:Person) WHERE n['key-' + n.name] IN ['Foo', 'Bar'] RETURN n" );

			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				foreach ( string query in queries )
				{
					 Db().execute("CREATE INDEX ON :Person(name)");
					 Db().execute("Call db.awaitIndexes()");
					 AssertNotifications( version + query, ContainsItem( DynamicPropertyWarning ) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnDynamicPropertyLookupWithSingleLabelAndNegativePredicate()
		 public virtual void ShouldNotNotifyOnDynamicPropertyLookupWithSingleLabelAndNegativePredicate()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				Db().execute("CREATE INDEX ON :Person(name)");
				Db().execute("Call db.awaitIndexes()");
				ShouldNotNotifyInStream( version, "EXPLAIN MATCH (n:Person) WHERE n['key-' + n.name] <> 'value' RETURN n" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnUnfulfillableIndexSeekUsingDynamicPropertyAndMultipleLabels()
		 public virtual void ShouldWarnOnUnfulfillableIndexSeekUsingDynamicPropertyAndMultipleLabels()
		 {
			  Stream.of( "CYPHER 3.5" ).forEach(version =>
			  {
				Db().execute("CREATE INDEX ON :Person(name)");
				Db().execute("Call db.awaitIndexes()");

				AssertNotifications( version + "EXPLAIN MATCH (n:Person:Foo) WHERE n['key-' + n.name] = 'value' RETURN n", ContainsItem( DynamicPropertyWarning ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnUnfulfillableIndexSeekUsingDynamicPropertyAndMultipleIndexedLabels()
		 public virtual void ShouldWarnOnUnfulfillableIndexSeekUsingDynamicPropertyAndMultipleIndexedLabels()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {

				Db().execute("CREATE INDEX ON :Person(name)");
				Db().execute("CREATE INDEX ON :Jedi(weapon)");
				Db().execute("Call db.awaitIndexes()");

				AssertNotifications( version + "EXPLAIN MATCH (n:Person:Jedi) WHERE n['key-' + n.name] = 'value' RETURN n", ContainsItem( DynamicPropertyWarning ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnCartesianProduct()
		 public virtual void ShouldWarnOnCartesianProduct()
		 {

			  AssertNotifications( "explain match (a)-->(b), (c)-->(d) return *", ContainsItem( CartesianProductWarning ) );

			  AssertNotifications( "explain cypher runtime=compiled match (a)-->(b), (c)-->(d) return *", ContainsItem( CartesianProductWarning ) );

			  AssertNotifications( "explain cypher runtime=interpreted match (a)-->(b), (c)-->(d) return *", ContainsItem( CartesianProductWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOnCartesianProductWithoutExplain()
		 public virtual void ShouldNotNotifyOnCartesianProductWithoutExplain()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, " match (a)-->(b), (c)-->(d) return *" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingLabel()
		 public virtual void ShouldWarnOnMissingLabel()
		 {
			  AssertNotifications( "EXPLAIN MATCH (a:NO_SUCH_THING) RETURN a", ContainsItem( UnknownLabelWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMisspelledLabel()
		 public virtual void ShouldWarnOnMisspelledLabel()
		 {
			  using ( Transaction tx = Db().beginTx() )
			  {
					Db().createNode().addLabel(label("Person"));
					tx.Success();
			  }

			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				AssertNotifications( version + "EXPLAIN MATCH (n:Preson) RETURN *", ContainsItem( UnknownLabelWarning ) );
				ShouldNotNotifyInStream( version, "EXPLAIN MATCH (n:Person) RETURN *" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingLabelWithCommentInBeginningWithOlderCypherVersions()
		 public virtual void ShouldWarnOnMissingLabelWithCommentInBeginningWithOlderCypherVersions()
		 {
			  AssertNotifications( "CYPHER 2.3 EXPLAIN//TESTING \nMATCH (n:X) return n Limit 1", ContainsItem( UnknownLabelWarning ) );

			  AssertNotifications( "CYPHER 3.1 EXPLAIN//TESTING \nMATCH (n:X) return n Limit 1", ContainsItem( UnknownLabelWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingLabelWithCommentInBeginning()
		 public virtual void ShouldWarnOnMissingLabelWithCommentInBeginning()
		 {
			  AssertNotifications( "EXPLAIN//TESTING \nMATCH (n:X) return n Limit 1", ContainsItem( UnknownLabelWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingLabelWithCommentInBeginningTwoLines()
		 public virtual void ShouldWarnOnMissingLabelWithCommentInBeginningTwoLines()
		 {
			  AssertNotifications( "//TESTING \n //TESTING \n EXPLAIN MATCH (n)\n MATCH (b:X) return n,b Limit 1", ContainsItem( UnknownLabelWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingLabelWithCommentInBeginningOnOneLine()
		 public virtual void ShouldWarnOnMissingLabelWithCommentInBeginningOnOneLine()
		 {
			  AssertNotifications( "explain /* Testing */ MATCH (n:X) RETURN n", ContainsItem( UnknownLabelWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingLabelWithCommentInMiddle()
		 public virtual void ShouldWarnOnMissingLabelWithCommentInMiddle()
		 {
			  AssertNotifications( "EXPLAIN\nMATCH (n)\n//TESTING \nMATCH (n:X)\nreturn n Limit 1", ContainsItem( UnknownLabelWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyForMissingLabelOnUpdate()
		 public virtual void ShouldNotNotifyForMissingLabelOnUpdate()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, " EXPLAIN CREATE (n:Person)" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingRelationshipType()
		 public virtual void ShouldWarnOnMissingRelationshipType()
		 {
			  AssertNotifications( "EXPLAIN MATCH ()-[a:NO_SUCH_THING]->() RETURN a", ContainsItem( UnknownRelationshipWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMisspelledRelationship()
		 public virtual void ShouldWarnOnMisspelledRelationship()
		 {
			  using ( Transaction tx = Db().beginTx() )
			  {
					Db().createNode().addLabel(label("Person"));
					tx.Success();
			  }

			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				Db().execute("CREATE (n)-[r:R]->(m)");
				AssertNotifications( version + "EXPLAIN MATCH ()-[r:r]->() RETURN *", ContainsItem( UnknownRelationshipWarning ) );
				ShouldNotNotifyInStream( version, "EXPLAIN MATCH ()-[r:R]->() RETURN *" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingRelationshipTypeWithComment()
		 public virtual void ShouldWarnOnMissingRelationshipTypeWithComment()
		 {
			  AssertNotifications( "EXPLAIN /*Comment*/ MATCH ()-[a:NO_SUCH_THING]->() RETURN a", ContainsItem( UnknownRelationshipWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingProperty()
		 public virtual void ShouldWarnOnMissingProperty()
		 {
			  AssertNotifications( "EXPLAIN MATCH (a {NO_SUCH_THING: 1337}) RETURN a", ContainsItem( UnknownPropertyKeyWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMisspelledProperty()
		 public virtual void ShouldWarnOnMisspelledProperty()
		 {
			  Db().execute("CREATE (n {prop : 42})");

			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
				Db().execute("CREATE (n)-[r:R]->(m)");
				AssertNotifications( version + "EXPLAIN MATCH (n) WHERE n.propp = 43 RETURN n", ContainsItem( UnknownPropertyKeyWarning ) );
				ShouldNotNotifyInStream( version, "EXPLAIN MATCH (n) WHERE n.prop = 43 RETURN n" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnOnMissingPropertyWithComment()
		 public virtual void ShouldWarnOnMissingPropertyWithComment()
		 {
			  AssertNotifications( "EXPLAIN /*Comment*/ MATCH (a {NO_SUCH_THING: 1337}) RETURN a", ContainsItem( UnknownPropertyKeyWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyForMissingPropertiesOnUpdate()
		 public virtual void ShouldNotNotifyForMissingPropertiesOnUpdate()
		 {
			  Stream.of( "CYPHER 2.3", "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => shouldNotNotifyInStream( version, " EXPLAIN CREATE (n {prop: 42})" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void version2_3ShouldWarnAboutBareNodes()
		 public virtual void Version2_3ShouldWarnAboutBareNodes()
		 {
			  Result res = Db().execute("EXPLAIN CYPHER 2.3 MATCH n RETURN n");
			  Debug.Assert( res.Notifications.GetEnumerator().hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveCorrectPositionWhetherFromCacheOrNot()
		 public virtual void ShouldGiveCorrectPositionWhetherFromCacheOrNot()
		 {
			  // Given
			  string cachedQuery = "MATCH (a:L1) RETURN a";
			  string nonCachedQuery = "MATCH (a:L2) RETURN a";
			  //make sure we cache the query
			  GraphDatabaseAPI db = db();
			  int limit = Db.DependencyResolver.resolveDependency( typeof( Config ) ).get( GraphDatabaseSettings.cypher_expression_recompilation_limit );
			  for ( int i = 0; i < limit + 1; i++ )
			  {
					Db.execute( cachedQuery ).resultAsString();
			  }

			  // When
			  Notification cachedNotification = Iterables.asList( Db.execute( "EXPLAIN " + cachedQuery ).Notifications )[0];
			  Notification nonCachedNotication = Iterables.asList( Db.execute( "EXPLAIN " + nonCachedQuery ).Notifications )[0];

			  // Then
			  assertThat( cachedNotification.Position, equalTo( new InputPosition( 17, 1, 18 ) ) );
			  assertThat( nonCachedNotication.Position, equalTo( new InputPosition( 17, 1, 18 ) ) );
		 }
	}

}