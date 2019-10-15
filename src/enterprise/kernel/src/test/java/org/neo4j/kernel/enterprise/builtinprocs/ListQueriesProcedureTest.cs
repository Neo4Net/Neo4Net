using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.enterprise.builtinprocs
{
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentEnterpriseDatabaseRule = Neo4Net.Test.rule.ImpermanentEnterpriseDatabaseRule;
	using VerboseTimeout = Neo4Net.Test.rule.VerboseTimeout;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.cypher_hints_error;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.track_query_allocation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.track_query_cpu_time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.concurrent.ThreadingRule.waitingWhileIn;

	public class ListQueriesProcedureTest
	{
		private bool InstanceFieldsInitialized = false;

		public ListQueriesProcedureTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Chain = RuleChain.outerRule( _db ).around( _threads );
		}

		 private readonly DatabaseRule _db = new ImpermanentEnterpriseDatabaseRule().withSetting(cypher_hints_error, Settings.TRUE).withSetting(GraphDatabaseSettings.track_query_allocation, Settings.TRUE).withSetting(track_query_cpu_time, Settings.TRUE).startLazily();

		 private readonly ThreadingRule _threads = new ThreadingRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(db).around(threads);
		 public RuleChain Chain;

		 private const int SECONDS_TIMEOUT = 240;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.VerboseTimeout timeout = org.neo4j.test.rule.VerboseTimeout.builder().withTimeout(SECONDS_TIMEOUT - 2, java.util.concurrent.TimeUnit.SECONDS).build();
		 public VerboseTimeout Timeout = VerboseTimeout.builder().withTimeout(SECONDS_TIMEOUT - 2, TimeUnit.SECONDS).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainTheQueryItself()
		 public virtual void ShouldContainTheQueryItself()
		 {
			  // given
			  string query = "CALL dbms.listQueries";

			  // when
			  Result result = _db.execute( query );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  IDictionary<string, object> row = result.Next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
			  assertEquals( query, row["query"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeDeprecatedFields()
		 public virtual void ShouldNotIncludeDeprecatedFields()
		 {
			  // when
			  Result result = _db.execute( "CALL dbms.listQueries" );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  IDictionary<string, object> row = result.Next();
			  assertThat( row, not( hasKey( "elapsedTime" ) ) );
			  assertThat( row, not( hasKey( "connectionDetails" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideElapsedCpuTimePlannerConnectionDetailsPageHitsAndFaults() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideElapsedCpuTimePlannerConnectionDetailsPageHitsAndFaults()
		 {
			  // given
			  string query = "MATCH (n) SET n.v = n.v + 1";
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					// when
					IDictionary<string, object> data = GetQueryListing( query );

					// then
					assertThat( data, hasKey( "elapsedTimeMillis" ) );
					object elapsedTime = data["elapsedTimeMillis"];
					assertThat( elapsedTime, instanceOf( typeof( Long ) ) );
					assertThat( data, hasKey( "cpuTimeMillis" ) );
					object cpuTime1 = data["cpuTimeMillis"];
					assertThat( cpuTime1, instanceOf( typeof( Long ) ) );
					assertThat( data, hasKey( "resourceInformation" ) );
					object ri = data["resourceInformation"];
					assertThat( ri, instanceOf( typeof( System.Collections.IDictionary ) ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String,Object> resourceInformation = (java.util.Map<String,Object>) ri;
					IDictionary<string, object> resourceInformation = ( IDictionary<string, object> ) ri;
					assertEquals( "waiting", data["status"] );
					assertEquals( "EXCLUSIVE", resourceInformation["lockMode"] );
					assertEquals( "NODE", resourceInformation["resourceType"] );
					assertArrayEquals( new long[] { test.ResourceConflict().Id }, (long[]) resourceInformation["resourceIds"] );
					assertThat( data, hasKey( "waitTimeMillis" ) );
					object waitTime1 = data["waitTimeMillis"];
					assertThat( waitTime1, instanceOf( typeof( Long ) ) );

					// when
					data = GetQueryListing( query );

					// then
					long? cpuTime2 = ( long? ) data["cpuTimeMillis"];
					assertThat( cpuTime2, greaterThanOrEqualTo( ( long? ) cpuTime1 ) );
					long? waitTime2 = ( long? ) data["waitTimeMillis"];
					assertThat( waitTime2, greaterThanOrEqualTo( ( long? ) waitTime1 ) );

					// ListPlannerAndRuntimeUsed
					// then
					assertThat( data, hasKey( "planner" ) );
					assertThat( data, hasKey( "runtime" ) );
					assertThat( data["planner"], instanceOf( typeof( string ) ) );
					assertThat( data["runtime"], instanceOf( typeof( string ) ) );

					// SpecificConnectionDetails

					// then
					assertThat( data, hasKey( "protocol" ) );
					assertThat( data, hasKey( "connectionId" ) );
					assertThat( data, hasKey( "clientAddress" ) );
					assertThat( data, hasKey( "requestUri" ) );

					//ContainPageHitsAndPageFaults
					// then
					assertThat( data, hasEntry( equalTo( "pageHits" ), instanceOf( typeof( Long ) ) ) );
					assertThat( data, hasEntry( equalTo( "pageFaults" ), instanceOf( typeof( Long ) ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideAllocatedBytes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideAllocatedBytes()
		 {
			  // given
			  string query = "MATCH (n) SET n.v = n.v + 1";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node node;
			  Node node;
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					node = test.ResourceConflict();
					// when
					IDictionary<string, object> data = GetQueryListing( query );

					// then
					assertThat( data, hasKey( "allocatedBytes" ) );
					object allocatedBytes = data["allocatedBytes"];
					assertThat( allocatedBytes, anyOf( nullValue(), (Matcher) allOf(instanceOf(typeof(Long)), greaterThan(0L)) ) );
			  }

			  using ( Resource<Node> test = test( () => node, query ) )
			  {
					// when
					IDictionary<string, object> data = GetQueryListing( query );

					assertThat( data, hasKey( "allocatedBytes" ) );
					object allocatedBytes = data["allocatedBytes"];
					assertThat( allocatedBytes, anyOf( nullValue(), (Matcher) allOf(instanceOf(typeof(Long)), greaterThan(0L)) ) );
					assertSame( node, test.ResourceConflict() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListActiveLocks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListActiveLocks()
		 {
			  // given
			  string query = "MATCH (x:X) SET x.v = 5 WITH count(x) AS num MATCH (y:Y) SET y.c = num";
			  ISet<long> locked = new HashSet<long>();
			  try (Resource<Node> test = test(() =>
			  {
				for ( int i = 0; i < 5; i++ )
				{
					 locked.Add( _db.createNode( label( "X" ) ).Id );
				}
				return _db.createNode( label( "Y" ) );
			  }
			 , query))
			 {
					// when
					using ( Result rows = _db.execute( "CALL dbms.listQueries() " + "YIELD query AS queryText, queryId, activeLockCount " + "WHERE queryText = $queryText " + "CALL dbms.listActiveLocks(queryId) YIELD mode, resourceType, resourceId " + "RETURN *", singletonMap( "queryText", query ) ) )
					{
						 // then
						 ISet<long> ids = new HashSet<long>();
						 long? lockCount = null;
						 long rowCount = 0;
						 while ( rows.MoveNext() )
						 {
							  IDictionary<string, object> row = rows.Current;
							  object resourceType = row["resourceType"];
							  object activeLockCount = row["activeLockCount"];
							  if ( lockCount == null )
							  {
									assertThat( "activeLockCount", activeLockCount, instanceOf( typeof( Long ) ) );
									lockCount = ( long? ) activeLockCount;
							  }
							  else
							  {
									assertEquals( "activeLockCount", lockCount, activeLockCount );
							  }
							  if ( ResourceTypes.LABEL.name().Equals(resourceType) )
							  {
									assertEquals( "SHARED", row["mode"] );
									assertEquals( 0L, row["resourceId"] );
							  }
							  else
							  {
									assertEquals( "NODE", resourceType );
									assertEquals( "EXCLUSIVE", row["mode"] );
									ids.Add( ( long? ) row["resourceId"] );
							  }
							  rowCount++;
						 }
						 assertEquals( locked, ids );
						 assertNotNull( "activeLockCount", lockCount );
						 assertEquals( lockCount.Value, rowCount ); // note: only true because query is blocked
					}
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyGetActiveLockCountFromCurrentQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyGetActiveLockCountFromCurrentQuery()
		 {
			  // given
			  string query1 = "MATCH (x:X) SET x.v = 1";
			  string query2 = "MATCH (y:Y) SET y.v = 2 WITH count(y) AS y MATCH (z:Z) SET z.v = y";
			  try (Resource<Node> test = test(() =>
			  {
				for ( int i = 0; i < 5; i++ )
				{
					 _db.createNode( label( "X" ) );
				}
				_db.createNode( label( "Y" ) );
				return _db.createNode( label( "Z" ) );
			  }
			 , query1, query2))
			 {
					// when
					using ( Result rows = _db.execute( "CALL dbms.listQueries() " + "YIELD query AS queryText, queryId, activeLockCount " + "WHERE queryText = $queryText " + "CALL dbms.listActiveLocks(queryId) YIELD resourceId " + "WITH queryText, queryId, activeLockCount, count(resourceId) AS allLocks " + "RETURN *", singletonMap( "queryText", query2 ) ) )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( "should have at least one row", rows.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 IDictionary<string, object> row = rows.Next();
						 object activeLockCount = row["activeLockCount"];
						 object allLocks = row["allLocks"];
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertFalse( "should have at most one row", rows.HasNext() );
						 assertThat( "activeLockCount", activeLockCount, instanceOf( typeof( Long ) ) );
						 assertThat( "allLocks", allLocks, instanceOf( typeof( Long ) ) );
						 assertThat( ( long? ) activeLockCount, lessThan( ( long? ) allLocks ) );
					}
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainSpecificConnectionDetails()
		 public virtual void ShouldContainSpecificConnectionDetails()
		 {
			  // when
			  IDictionary<string, object> data = GetQueryListing( "CALL dbms.listQueries" );

			  // then
			  assertThat( data, hasKey( "protocol" ) );
			  assertThat( data, hasKey( "connectionId" ) );
			  assertThat( data, hasKey( "clientAddress" ) );
			  assertThat( data, hasKey( "requestUri" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainPageHitsAndPageFaults() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContainPageHitsAndPageFaults()
		 {
			  // given
			  string query = "MATCH (n) SET n.v = n.v + 1";
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					// when
					IDictionary<string, object> data = GetQueryListing( query );

					// then
					assertThat( data, hasEntry( equalTo( "pageHits" ), instanceOf( typeof( Long ) ) ) );
					assertThat( data, hasEntry( equalTo( "pageFaults" ), instanceOf( typeof( Long ) ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUsedIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListUsedIndexes()
		 {
			  // given
			  string label = "IndexedLabel";
			  string property = "indexedProperty";
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().indexFor(label(label)).on(property).create();
					tx.Success();
			  }
			  EnsureIndexesAreOnline();
			  ShouldListUsedIndexes( label, property );
		 }

		 private void EnsureIndexesAreOnline()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(SECONDS_TIMEOUT, SECONDS);
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUsedUniqueIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListUsedUniqueIndexes()
		 {
			  // given
			  string label = "UniqueLabel";
			  string property = "uniqueProperty";
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().constraintFor(label(label)).assertPropertyIsUnique(property).create();
					tx.Success();
			  }
			  EnsureIndexesAreOnline();
			  ShouldListUsedIndexes( label, property );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListIndexesUsedForScans() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListIndexesUsedForScans()
		 {
			  // given
			  const string query = "MATCH (n:Node) USING INDEX n:Node(value) WHERE 1 < n.value < 10 SET n.value = 2";
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().indexFor(label("Node")).on("value").create();
					tx.Success();
			  }
			  EnsureIndexesAreOnline();
			  try (Resource<Node> test = test(() =>
			  {
				Node node = _db.createNode( label( "Node" ) );
				node.setProperty( "value", 5L );
				return node;
			  }
			 , query))
			 {
					// when
					IDictionary<string, object> data = GetQueryListing( query );

					// then
					assertThat( data, hasEntry( equalTo( "indexes" ), instanceOf( typeof( System.Collections.IList ) ) ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<java.util.Map<String,Object>> indexes = (java.util.List<java.util.Map<String,Object>>) data.get("indexes");
					IList<IDictionary<string, object>> indexes = ( IList<IDictionary<string, object>> ) data["indexes"];
					assertEquals( "number of indexes used", 1, indexes.Count );
					IDictionary<string, object> index = indexes[0];
					assertThat( index, hasEntry( "identifier", "n" ) );
					assertThat( index, hasEntry( "label", "Node" ) );
					assertThat( index, hasEntry( "propertyKey", "value" ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisableCpuTimeTracking() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisableCpuTimeTracking()
		 {
			  // given
			  string query = "MATCH (n) SET n.v = n.v + 1";
			  _db.withSetting( track_query_cpu_time, FALSE );
			  IDictionary<string, object> data;

			  // when
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					data = GetQueryListing( query );
			  }

			  // then
			  assertThat( data, hasEntry( equalTo( "cpuTimeMillis" ), nullValue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cpuTimeTrackingShouldBeADynamicSetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CpuTimeTrackingShouldBeADynamicSetting()
		 {
			  // given
			  string query = "MATCH (n) SET n.v = n.v + 1";
			  IDictionary<string, object> data;

			  // when
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					data = GetQueryListing( query );
			  }
			  // then
			  assertThat( data, hasEntry( equalTo( "cpuTimeMillis" ), notNullValue() ) );

			  // when
			  _db.execute( "call dbms.setConfigValue('" + track_query_cpu_time.name() + "', 'false')" );
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					data = GetQueryListing( query );
			  }
			  // then
			  assertThat( data, hasEntry( equalTo( "cpuTimeMillis" ), nullValue() ) );

			  // when
			  _db.execute( "call dbms.setConfigValue('" + track_query_cpu_time.name() + "', 'true')" );
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					data = GetQueryListing( query );
			  }
			  // then
			  assertThat( data, hasEntry( equalTo( "cpuTimeMillis" ), notNullValue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisableHeapAllocationTracking() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisableHeapAllocationTracking()
		 {
			  // given
			  string query = "MATCH (n) SET n.v = n.v + 1";
			  _db.withSetting( track_query_allocation, FALSE );
			  IDictionary<string, object> data;

			  // when
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					data = GetQueryListing( query );
			  }

			  // then
			  assertThat( data, hasEntry( equalTo( "allocatedBytes" ), nullValue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void heapAllocationTrackingShouldBeADynamicSetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HeapAllocationTrackingShouldBeADynamicSetting()
		 {
			  // given
			  string query = "MATCH (n) SET n.v = n.v + 1";
			  IDictionary<string, object> data;

			  // when
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					data = GetQueryListing( query );
			  }
			  // then
			  assertThat( data, hasEntry( equalTo( "allocatedBytes" ), anyOf( nullValue(), (Matcher) allOf(instanceOf(typeof(Long)), greaterThan(0L)) ) ) );

			  // when
			  _db.execute( "call dbms.setConfigValue('" + track_query_allocation.name() + "', 'false')" );
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					data = GetQueryListing( query );
			  }
			  // then
			  assertThat( data, hasEntry( equalTo( "allocatedBytes" ), nullValue() ) );

			  // when
			  _db.execute( "call dbms.setConfigValue('" + track_query_allocation.name() + "', 'true')" );
			  using ( Resource<Node> test = test( _db.createNode, query ) )
			  {
					data = GetQueryListing( query );
			  }
			  // then
			  assertThat( data, hasEntry( equalTo( "allocatedBytes" ), anyOf( nullValue(), (Matcher) allOf(instanceOf(typeof(Long)), greaterThan(0L)) ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldListUsedIndexes(String label, String property) throws Exception
		 private void ShouldListUsedIndexes( string label, string property )
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String QUERY1 = "MATCH (n:" + label + "{" + property + ":5}) USING INDEX n:" + label + "(" + property +
			  string query1 = "MATCH (n:" + label + "{" + property + ":5}) USING INDEX n:" + label + "(" + property +
						 ") SET n." + property + " = 3";
			  try (Resource<Node> test = test(() =>
			  {
				Node node = _db.createNode( label( label ) );
				node.setProperty( property, 5L );
				return node;
			  }
			 , query1))
			 {
					// when
					IDictionary<string, object> data = GetQueryListing( query1 );

					// then
					assertThat( data, hasEntry( equalTo( "indexes" ), instanceOf( typeof( System.Collections.IList ) ) ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<java.util.Map<String,Object>> indexes = (java.util.List<java.util.Map<String,Object>>) data.get("indexes");
					IList<IDictionary<string, object>> indexes = ( IList<IDictionary<string, object>> ) data["indexes"];
					assertEquals( "number of indexes used", 1, indexes.Count );
					IDictionary<string, object> index = indexes[0];
					assertThat( index, hasEntry( "identifier", "n" ) );
					assertThat( index, hasEntry( "label", label ) );
					assertThat( index, hasEntry( "propertyKey", property ) );
			 }

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String QUERY2 = "MATCH (n:" + label + "{" + property + ":3}) USING INDEX n:" + label + "(" + property +
			  string query2 = "MATCH (n:" + label + "{" + property + ":3}) USING INDEX n:" + label + "(" + property +
						 ") MATCH (u:" + label + "{" + property + ":4}) USING INDEX u:" + label + "(" + property +
						 ") CREATE (n)-[:KNOWS]->(u)";
			  try (Resource<Node> test = test(() =>
			  {
				Node node = _db.createNode( label( label ) );
				node.setProperty( property, 4L );
				return node;
			  }
			 , query2))
			 {
					// when
					IDictionary<string, object> data = GetQueryListing( query2 );

					// then
					assertThat( data, hasEntry( equalTo( "indexes" ), instanceOf( typeof( System.Collections.IList ) ) ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<java.util.Map<String,Object>> indexes = (java.util.List<java.util.Map<String,Object>>) data.get("indexes");
					IList<IDictionary<string, object>> indexes = ( IList<IDictionary<string, object>> ) data["indexes"];
					assertEquals( "number of indexes used", 2, indexes.Count );

					IDictionary<string, object> index1 = indexes[0];
					assertThat( index1, hasEntry( "identifier", "n" ) );
					assertThat( index1, hasEntry( "label", label ) );
					assertThat( index1, hasEntry( "propertyKey", property ) );

					IDictionary<string, object> index2 = indexes[1];
					assertThat( index2, hasEntry( "identifier", "u" ) );
					assertThat( index2, hasEntry( "label", label ) );
					assertThat( index2, hasEntry( "propertyKey", property ) );
			 }
		 }

		 private IDictionary<string, object> GetQueryListing( string query )
		 {
			  using ( Result rows = _db.execute( "CALL dbms.listQueries" ) )
			  {
					while ( rows.MoveNext() )
					{
						 IDictionary<string, object> row = rows.Current;
						 if ( query.Equals( row["query"] ) )
						 {
							  return row;
						 }
					}
			  }
			  throw new AssertionError( "query not active: " + query );
		 }

		 private class Resource<T> : IDisposable
		 {
			  internal readonly System.Threading.CountdownEvent Latch;
			  internal readonly System.Threading.CountdownEvent FinishLatch;
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly T ResourceConflict;

			  internal Resource( System.Threading.CountdownEvent latch, System.Threading.CountdownEvent finishLatch, T resource )
			  {
					this.Latch = latch;
					this.FinishLatch = finishLatch;
					this.ResourceConflict = resource;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws InterruptedException
			  public override void Close()
			  {
					Latch.Signal();
					FinishLatch.await();
			  }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public virtual T ResourceConflict()
			  {
					return ResourceConflict;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T extends org.neo4j.graphdb.PropertyContainer> Resource<T> test(System.Func<T> setup, String... queries) throws InterruptedException, java.util.concurrent.ExecutionException
		 private Resource<T> Test<T>( System.Func<T> setup, params string[] queries ) where T : Neo4Net.Graphdb.PropertyContainer
		 {
			  System.Threading.CountdownEvent resourceLocked = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent listQueriesLatch = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent finishQueriesLatch = new System.Threading.CountdownEvent( 1 );
			  T resource;
			  using ( Transaction tx = _db.beginTx() )
			  {
					resource = setup();
					tx.Success();
			  }
			  _threads.execute(parameter =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 tx.AcquireWriteLock( resource );
					 resourceLocked.Signal();
					 listQueriesLatch.await();
				}
				return null;
			  }, null);
			  resourceLocked.await();

			  _threads.executeAndAwait(parameter =>
			  {
				try
				{
					using ( Transaction tx = _db.beginTx() )
					{
						 foreach ( string query in queries )
						 {
							  _db.execute( query ).close();
						 }
						 tx.Success();
					}
				}
				catch ( Exception t )
				{
					 Console.WriteLine( t.ToString() );
					 Console.Write( t.StackTrace );
					 throw new Exception( t );
				}
				finally
				{
					 finishQueriesLatch.Signal();
				}
				return null;
			  }, null, waitingWhileIn( typeof( GraphDatabaseFacade ), "execute" ), SECONDS_TIMEOUT, SECONDS);

			  return new Resource<T>( listQueriesLatch, finishQueriesLatch, resource );
		 }
	}

}