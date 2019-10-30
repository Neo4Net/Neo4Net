using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using Request = org.eclipse.jetty.server.Request;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;
	using AbstractHandler = org.eclipse.jetty.server.handler.AbstractHandler;
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Test = org.junit.Test;


	using Neo4Net.GraphDb;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using QueryId = Neo4Net.Kernel.enterprise.builtinprocs.QueryId;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Operations = Neo4Net.Kernel.Impl.Newapi.Operations;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using Barrier = Neo4Net.Test.Barrier;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.enterprise.auth.plugin.api.PredefinedRoles.PUBLISHER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.assertion.Assert.assertEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.matchers.CommonMatchers.matchesOneToOneInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.concurrent.Runnables.EMPTY_RUNNABLE;

	public abstract class BuiltInProceduresInteractionTestBase<S> : ProcedureInteractionTestBase<S>
	{

		 //---------- list running transactions -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListSelfTransaction()
		 public virtual void ShouldListSelfTransaction()
		 {
			  AssertSuccess( AdminSubject, "CALL dbms.listTransactions()", r => assertKeyIs( r, "username", "adminSubject" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listBlockedTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListBlockedTransactions()
		 {
			  AssertEmpty( AdminSubject, "CREATE (:MyNode {prop: 2})" );
			  string firstModifier = "MATCH (n:MyNode) set n.prop=3";
			  string secondModifier = "MATCH (n:MyNode) set n.prop=4";
			  DoubleLatch latch = new DoubleLatch( 2 );
			  DoubleLatch blockedModifierLatch = new DoubleLatch( 2 );
			  OffsetDateTime startTime = StartTime;

			  ThreadedTransaction<S> tx = new ThreadedTransaction<S>( Neo, latch );
			  tx.Execute( ThreadingConflict, WriteSubject, firstModifier );
			  latch.Start();
			  latch.WaitForAllToStart();

			  ThreadedTransaction<S> tx2 = new ThreadedTransaction<S>( Neo, blockedModifierLatch );
			  tx2.ExecuteEarly( ThreadingConflict, WriteSubject, KernelTransaction.Type.@explicit, secondModifier );

			  WaitTransactionToStartWaitingForTheLock();

			  blockedModifierLatch.StartAndWaitForAllToStart();
			  string query = "CALL dbms.listTransactions()";
			  AssertSuccess(AdminSubject, query, r =>
			  {
				IList<IDictionary<string, object>> maps = CollectResults( r );

				Matcher<IDictionary<string, object>> listTransaction = ListedTransactionOfInteractionLevel( startTime, "adminSubject", query );
				Matcher<IDictionary<string, object>> blockedQueryMatcher = allOf( anyOf( HasCurrentQuery( secondModifier ), HasCurrentQuery( firstModifier ) ), HasStatus( "Blocked by:" ) );
				Matcher<IDictionary<string, object>> executedModifier = allOf( HasCurrentQuery( "" ), HasStatus( "Running" ) );

				assertThat( maps, matchesOneToOneInAnyOrder( listTransaction, blockedQueryMatcher, executedModifier ) );
			  });

			  latch.FinishAndWaitForAllToFinish();
			  tx.CloseAndAssertSuccess();
			  blockedModifierLatch.FinishAndWaitForAllToFinish();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitTransactionToStartWaitingForTheLock() throws InterruptedException
		 private void WaitTransactionToStartWaitingForTheLock()
		 {
			  while ( Thread.AllStackTraces.Keys.noneMatch( ThreadingRule.waitingWhileIn( typeof( Operations ), "acquireExclusiveNodeLock" ) ) )
			  {
					TimeUnit.MILLISECONDS.sleep( 10 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listTransactionWithMetadata() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListTransactionWithMetadata()
		 {
			  string setMetaDataQuery = "CALL dbms.setTXMetaData( { realUser: 'MyMan' } )";
			  string matchQuery = "MATCH (n) RETURN n";
			  string listTransactionsQuery = "CALL dbms.listTransactions()";

			  DoubleLatch latch = new DoubleLatch( 2 );
			  OffsetDateTime startTime = StartTime;

			  ThreadedTransaction<S> tx = new ThreadedTransaction<S>( Neo, latch );
			  tx.Execute( ThreadingConflict, WriteSubject, setMetaDataQuery, matchQuery );

			  latch.StartAndWaitForAllToStart();

			  AssertSuccess(AdminSubject, listTransactionsQuery, r =>
			  {
				IList<IDictionary<string, object>> maps = CollectResults( r );
				Matcher<IDictionary<string, object>> thisTransaction = ListedTransactionOfInteractionLevel( startTime, "adminSubject", listTransactionsQuery );
				Matcher<IDictionary<string, object>> matchQueryTransactionMatcher = ListedTransactionWithMetaData( startTime, "writeSubject", matchQuery, map( "realUser", "MyMan" ) );

				assertThat( maps, matchesOneToOneInAnyOrder( thisTransaction, matchQueryTransactionMatcher ) );
			  });

			  latch.FinishAndWaitForAllToFinish();
			  tx.CloseAndAssertSuccess();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listAllTransactionsWhenRunningAsAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListAllTransactionsWhenRunningAsAdmin()
		 {
			  DoubleLatch latch = new DoubleLatch( 3, true );
			  OffsetDateTime startTime = StartTime;

			  ThreadedTransaction<S> read1 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> read2 = new ThreadedTransaction<S>( Neo, latch );

			  string q1 = read1.Execute( ThreadingConflict, ReadSubject, "UNWIND [1,2,3] AS x RETURN x" );
			  string q2 = read2.Execute( ThreadingConflict, WriteSubject, "UNWIND [4,5,6] AS y RETURN y" );
			  latch.StartAndWaitForAllToStart();

			  string query = "CALL dbms.listTransactions()";
			  AssertSuccess(AdminSubject, query, r =>
			  {
				IList<IDictionary<string, object>> maps = CollectResults( r );

				Matcher<IDictionary<string, object>> thisTransaction = ListedTransactionOfInteractionLevel( startTime, "adminSubject", query );
				Matcher<IDictionary<string, object>> matcher1 = ListedTransaction( startTime, "readSubject", q1 );
				Matcher<IDictionary<string, object>> matcher2 = ListedTransaction( startTime, "writeSubject", q2 );

				assertThat( maps, matchesOneToOneInAnyOrder( matcher1, matcher2, thisTransaction ) );
			  });

			  latch.FinishAndWaitForAllToFinish();

			  read1.CloseAndAssertSuccess();
			  read2.CloseAndAssertSuccess();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyListOwnTransactionsWhenNotRunningAsAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyListOwnTransactionsWhenNotRunningAsAdmin()
		 {
			  DoubleLatch latch = new DoubleLatch( 3, true );
			  OffsetDateTime startTime = StartTime;
			  ThreadedTransaction<S> read1 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> read2 = new ThreadedTransaction<S>( Neo, latch );

			  string q1 = read1.Execute( ThreadingConflict, ReadSubject, "UNWIND [1,2,3] AS x RETURN x" );
			  string ignored = read2.Execute( ThreadingConflict, WriteSubject, "UNWIND [4,5,6] AS y RETURN y" );
			  latch.StartAndWaitForAllToStart();

			  string query = "CALL dbms.listTransactions()";
			  AssertSuccess(ReadSubject, query, r =>
			  {
				IList<IDictionary<string, object>> maps = CollectResults( r );

				Matcher<IDictionary<string, object>> thisTransaction = ListedTransaction( startTime, "readSubject", query );
				Matcher<IDictionary<string, object>> queryMatcher = ListedTransaction( startTime, "readSubject", q1 );

				assertThat( maps, matchesOneToOneInAnyOrder( queryMatcher, thisTransaction ) );
			  });

			  latch.FinishAndWaitForAllToFinish();

			  read1.CloseAndAssertSuccess();
			  read2.CloseAndAssertSuccess();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllTransactionsWithAuthDisabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllTransactionsWithAuthDisabled()
		 {
			  Neo.tearDown();
			  Neo = setUpNeoServer( stringMap( GraphDatabaseSettings.auth_enabled.name(), "false" ) );

			  DoubleLatch latch = new DoubleLatch( 2, true );
			  OffsetDateTime startTime = StartTime;

			  ThreadedTransaction<S> read = new ThreadedTransaction<S>( Neo, latch );

			  string q = read.Execute( ThreadingConflict, Neo.login( "user1", "" ), "UNWIND [1,2,3] AS x RETURN x" );
			  latch.StartAndWaitForAllToStart();

			  string query = "CALL dbms.listTransactions()";
			  try
			  {
					AssertSuccess(Neo.login("admin", ""), query, r =>
					{
					 IList<IDictionary<string, object>> maps = CollectResults( r );

					 Matcher<IDictionary<string, object>> thisQuery = ListedTransactionOfInteractionLevel( startTime, "", query ); // admin
					 Matcher<IDictionary<string, object>> matcher1 = ListedTransaction( startTime, "", q ); // user1
					 assertThat( maps, matchesOneToOneInAnyOrder( matcher1, thisQuery ) );
					});
			  }
			  finally
			  {
					latch.FinishAndWaitForAllToFinish();
			  }
			  read.CloseAndAssertSuccess();
		 }

		 //---------- list running queries -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllQueryIncludingMetaData() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllQueryIncludingMetaData()
		 {
			  string setMetaDataQuery = "CALL dbms.setTXMetaData( { realUser: 'MyMan' } )";
			  string matchQuery = "MATCH (n) RETURN n";
			  string listQueriesQuery = "CALL dbms.listQueries()";

			  DoubleLatch latch = new DoubleLatch( 2 );
			  OffsetDateTime startTime = now( ZoneOffset.UTC );

			  ThreadedTransaction<S> tx = new ThreadedTransaction<S>( Neo, latch );
			  tx.Execute( ThreadingConflict, WriteSubject, setMetaDataQuery, matchQuery );

			  latch.StartAndWaitForAllToStart();

			  AssertSuccess(AdminSubject, listQueriesQuery, r =>
			  {
				IList<IDictionary<string, object>> maps = CollectResults( r );
				Matcher<IDictionary<string, object>> thisQuery = ListedQueryOfInteractionLevel( startTime, "adminSubject", listQueriesQuery );
				Matcher<IDictionary<string, object>> matchQueryMatcher = ListedQueryWithMetaData( startTime, "writeSubject", matchQuery, map( "realUser", "MyMan" ) );

				assertThat( maps, matchesOneToOneInAnyOrder( thisQuery, matchQueryMatcher ) );
			  });

			  latch.FinishAndWaitForAllToFinish();
			  tx.CloseAndAssertSuccess();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldListAllQueriesWhenRunningAsAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllQueriesWhenRunningAsAdmin()
		 {
			  DoubleLatch latch = new DoubleLatch( 3, true );
			  OffsetDateTime startTime = StartTime;

			  ThreadedTransaction<S> read1 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> read2 = new ThreadedTransaction<S>( Neo, latch );

			  string q1 = read1.Execute( ThreadingConflict, ReadSubject, "UNWIND [1,2,3] AS x RETURN x" );
			  string q2 = read2.Execute( ThreadingConflict, WriteSubject, "UNWIND [4,5,6] AS y RETURN y" );
			  latch.StartAndWaitForAllToStart();

			  string query = "CALL dbms.listQueries()";
			  AssertSuccess(AdminSubject, query, r =>
			  {
				IList<IDictionary<string, object>> maps = CollectResults( r );

				Matcher<IDictionary<string, object>> thisQuery = ListedQueryOfInteractionLevel( startTime, "adminSubject", query );
				Matcher<IDictionary<string, object>> matcher1 = ListedQuery( startTime, "readSubject", q1 );
				Matcher<IDictionary<string, object>> matcher2 = ListedQuery( startTime, "writeSubject", q2 );

				assertThat( maps, matchesOneToOneInAnyOrder( matcher1, matcher2, thisQuery ) );
			  });

			  latch.FinishAndWaitForAllToFinish();

			  read1.CloseAndAssertSuccess();
			  read2.CloseAndAssertSuccess();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyListOwnQueriesWhenNotRunningAsAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyListOwnQueriesWhenNotRunningAsAdmin()
		 {
			  DoubleLatch latch = new DoubleLatch( 3, true );
			  OffsetDateTime startTime = StartTime;
			  ThreadedTransaction<S> read1 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> read2 = new ThreadedTransaction<S>( Neo, latch );

			  string q1 = read1.Execute( ThreadingConflict, ReadSubject, "UNWIND [1,2,3] AS x RETURN x" );
			  string ignored = read2.Execute( ThreadingConflict, WriteSubject, "UNWIND [4,5,6] AS y RETURN y" );
			  latch.StartAndWaitForAllToStart();

			  string query = "CALL dbms.listQueries()";
			  AssertSuccess(ReadSubject, query, r =>
			  {
				IList<IDictionary<string, object>> maps = CollectResults( r );

				Matcher<IDictionary<string, object>> thisQuery = ListedQuery( startTime, "readSubject", query );
				Matcher<IDictionary<string, object>> queryMatcher = ListedQuery( startTime, "readSubject", q1 );

				assertThat( maps, matchesOneToOneInAnyOrder( queryMatcher, thisQuery ) );
			  });

			  latch.FinishAndWaitForAllToFinish();

			  read1.CloseAndAssertSuccess();
			  read2.CloseAndAssertSuccess();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldListQueriesEvenIfUsingPeriodicCommit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListQueriesEvenIfUsingPeriodicCommit()
		 {
			  for ( int i = 8; i <= 11; i++ )
			  {
					// Spawns a throttled HTTP server, runs a PERIODIC COMMIT that fetches data from this server,
					// and checks that the query is visible when using listQueries()

					// Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.DoubleLatch latch = new org.Neo4Net.test.DoubleLatch(3, true);
					DoubleLatch latch = new DoubleLatch( 3, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.Barrier_Control barrier = new org.Neo4Net.test.Barrier_Control();
					Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();

					// Serve CSV via local web server, let Jetty find a random port for us
					Server server = CreateHttpServer( latch, barrier, i, 50 - i );
					server.start();
					int localPort = GetLocalPort( server );

					OffsetDateTime startTime = StartTime;

					// When
					ThreadedTransaction<S> write = new ThreadedTransaction<S>( Neo, latch );

					try
					{
						 string writeQuery = write.ExecuteEarly( ThreadingConflict, WriteSubject, KernelTransaction.Type.@implicit, format( "USING PERIODIC COMMIT 10 LOAD CSV FROM 'http://localhost:%d' AS line ", localPort ) + "CREATE (n:A {id: line[0], square: line[1]}) " + "RETURN count(*)" );
						 latch.StartAndWaitForAllToStart();

						 // Then
						 string query = "CALL dbms.listQueries()";
						 AssertSuccess(AdminSubject, query, r =>
						 {
						  IList<IDictionary<string, object>> maps = CollectResults( r );

						  Matcher<IDictionary<string, object>> thisMatcher = ListedQuery( startTime, "adminSubject", query );
						  Matcher<IDictionary<string, object>> writeMatcher = ListedQuery( startTime, "writeSubject", writeQuery );

						  assertThat( maps, hasItem( thisMatcher ) );
						  assertThat( maps, hasItem( writeMatcher ) );
						 });
					}
					finally
					{
						 // When
						 barrier.Release();
						 latch.FinishAndWaitForAllToFinish();
						 server.stop();

						 // Then
						 write.CloseAndAssertSuccess();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllQueriesWithAuthDisabled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllQueriesWithAuthDisabled()
		 {
			  Neo.tearDown();
			  Neo = setUpNeoServer( stringMap( GraphDatabaseSettings.auth_enabled.name(), "false" ) );

			  DoubleLatch latch = new DoubleLatch( 2, true );
			  OffsetDateTime startTime = StartTime;

			  ThreadedTransaction<S> read = new ThreadedTransaction<S>( Neo, latch );

			  string q = read.Execute( ThreadingConflict, Neo.login( "user1", "" ), "UNWIND [1,2,3] AS x RETURN x" );
			  latch.StartAndWaitForAllToStart();

			  string query = "CALL dbms.listQueries()";
			  try
			  {
					AssertSuccess(Neo.login("admin", ""), query, r =>
					{
					 IList<IDictionary<string, object>> maps = CollectResults( r );

					 Matcher<IDictionary<string, object>> thisQuery = ListedQueryOfInteractionLevel( startTime, "", query ); // admin
					 Matcher<IDictionary<string, object>> matcher1 = ListedQuery( startTime, "", q ); // user1
					 assertThat( maps, matchesOneToOneInAnyOrder( matcher1, thisQuery ) );
					});
			  }
			  finally
			  {
					latch.FinishAndWaitForAllToFinish();
			  }
			  read.CloseAndAssertSuccess();
		 }

		 //---------- Create Tokens query -------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateLabel()
		 public virtual void ShouldCreateLabel()
		 {
			  AssertFail( EditorSubject, "CREATE (:MySpecialLabel)", TokenCreateOpsNotAllowed );
			  AssertFail( EditorSubject, "CALL db.createLabel('MySpecialLabel')", TokenCreateOpsNotAllowed );
			  AssertEmpty( WriteSubject, "CALL db.createLabel('MySpecialLabel')" );
			  AssertSuccess( WriteSubject, "MATCH (n:MySpecialLabel) RETURN count(n) AS count", r => r.next().get("count").Equals(0) );
			  AssertEmpty( EditorSubject, "CREATE (:MySpecialLabel)" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateRelationshipType()
		 public virtual void ShouldCreateRelationshipType()
		 {
			  AssertEmpty( WriteSubject, "CREATE (a:Node {id:0}) CREATE ( b:Node {id:1} )" );
			  AssertFail( EditorSubject, "MATCH (a:Node), (b:Node) WHERE a.id = 0 AND b.id = 1 CREATE (a)-[:MySpecialRelationship]->(b)", TokenCreateOpsNotAllowed );
			  AssertFail( EditorSubject, "CALL db.createRelationshipType('MySpecialRelationship')", TokenCreateOpsNotAllowed );
			  AssertEmpty( WriteSubject, "CALL db.createRelationshipType('MySpecialRelationship')" );
			  AssertSuccess( EditorSubject, "MATCH (n)-[c:MySpecialRelationship]-(m) RETURN count(c) AS count", r => r.next().get("count").Equals(0) );
			  AssertEmpty( EditorSubject, "MATCH (a:Node), (b:Node) WHERE a.id = 0 AND b.id = 1 CREATE (a)-[:MySpecialRelationship]->(b)" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateProperty()
		 public virtual void ShouldCreateProperty()
		 {
			  AssertFail( EditorSubject, "CREATE (a) SET a.MySpecialProperty = 'a'", TokenCreateOpsNotAllowed );
			  AssertFail( EditorSubject, "CALL db.createProperty('MySpecialProperty')", TokenCreateOpsNotAllowed );
			  AssertEmpty( WriteSubject, "CALL db.createProperty('MySpecialProperty')" );
			  AssertSuccess( EditorSubject, "MATCH (n) WHERE n.MySpecialProperty IS NULL RETURN count(n) AS count", r => r.next().get("count").Equals(0) );
			  AssertEmpty( EditorSubject, "CREATE (a) SET a.MySpecialProperty = 'a'" );
		 }

		 //---------- terminate query -----------

		 /*
		  * User starts query1 that takes a lock and runs for a long time.
		  * User starts query2 that needs to wait for that lock.
		  * query2 is blocked waiting for lock to be released.
		  * Admin terminates query2.
		  * query2 is immediately terminated, even though locks have not been released.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryWaitingForLocksShouldBeKilledBeforeLocksAreReleased() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void QueryWaitingForLocksShouldBeKilledBeforeLocksAreReleased()
		 {
			  AssertEmpty( AdminSubject, "CREATE (:MyNode {prop: 2})" );

			  // create new latch
			  ClassWithProcedures.DoubleLatch = new DoubleLatch( 2 );

			  // start never-ending query
			  string query1 = "MATCH (n:MyNode) SET n.prop = 5 WITH * CALL test.neverEnding() RETURN 1";
			  ThreadedTransaction<S> tx1 = new ThreadedTransaction<S>( Neo, new DoubleLatch() );
			  tx1.ExecuteEarly( ThreadingConflict, WriteSubject, KernelTransaction.Type.@explicit, query1 );

			  // wait for query1 to be stuck in procedure with its write lock
			  ClassWithProcedures.DoubleLatch.startAndWaitForAllToStart();

			  // start query2
			  ThreadedTransaction<S> tx2 = new ThreadedTransaction<S>( Neo, new DoubleLatch() );
			  string query2 = "MATCH (n:MyNode) SET n.prop = 10 RETURN 1";
			  tx2.ExecuteEarly( ThreadingConflict, WriteSubject, KernelTransaction.Type.@explicit, query2 );

			  AssertQueryIsRunning( query2 );

			  // get the query id of query2 and kill it
			  AssertSuccess( AdminSubject, "CALL dbms.listQueries() YIELD query, queryId " + "WITH query, queryId WHERE query = '" + query2 + "'" + "CALL dbms.killQuery(queryId) YIELD queryId AS killedId " + "RETURN 1", itr => assertThat( Iterators.count( itr ), equalTo( 1L ) ) ); // consume iterator so resources are closed

			  tx2.CloseAndAssertSomeTermination();

			  // allow query1 to exit procedure and finish
			  ClassWithProcedures.DoubleLatch.finish();
			  tx1.CloseAndAssertSuccess();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKillQueryAsAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKillQueryAsAdmin()
		 {
			  ExecuteTwoQueriesAndKillTheFirst( ReadSubject, ReadSubject, AdminSubject );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKillQueryAsUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKillQueryAsUser()
		 {
			  ExecuteTwoQueriesAndKillTheFirst( ReadSubject, WriteSubject, ReadSubject );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void executeTwoQueriesAndKillTheFirst(S executor1, S executor2, S killer) throws Throwable
		 private void ExecuteTwoQueriesAndKillTheFirst( S executor1, S executor2, S killer )
		 {
			  DoubleLatch latch = new DoubleLatch( 3 );
			  ThreadedTransaction<S> tx1 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> tx2 = new ThreadedTransaction<S>( Neo, latch );
			  string q1 = tx1.Execute( ThreadingConflict, executor1, "UNWIND [1,2,3] AS x RETURN x" );
			  tx2.Execute( ThreadingConflict, executor2, "UNWIND [4,5,6] AS y RETURN y" );
			  latch.StartAndWaitForAllToStart();

			  string id1 = ExtractQueryId( q1 );

			  AssertSuccess(killer, "CALL dbms.killQuery('" + id1 + "') YIELD username " + "RETURN count(username) AS count, username", r =>
			  {
						  IList<IDictionary<string, object>> actual = CollectResults( r );
							Matcher<IDictionary<string, object>> mapMatcher = allOf( ( Matcher ) hasEntry( equalTo( "count" ), anyOf( equalTo( 1 ), equalTo( 1L ) ) ), ( Matcher ) hasEntry( equalTo( "username" ), equalTo( "readSubject" ) ) );
						  assertThat( actual, matchesOneToOneInAnyOrder( mapMatcher ) );
			  });

			  latch.FinishAndWaitForAllToFinish();
			  tx1.CloseAndAssertExplicitTermination();
			  tx2.CloseAndAssertSuccess();

			  AssertEmpty( AdminSubject, "CALL dbms.listQueries() YIELD query WITH * WHERE NOT query CONTAINS 'listQueries' RETURN *" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelfKillQuery()
		 public virtual void ShouldSelfKillQuery()
		 {
			  string result = Neo.executeQuery( ReadSubject, "WITH 'Hello' AS marker CALL dbms.listQueries() YIELD queryId AS id, query " + "WITH * WHERE query CONTAINS 'Hello' CALL dbms.killQuery(id) YIELD username " + "RETURN count(username) AS count, username", emptyMap(), Iterators.count );

			  assertThat( result, containsString( "Explicitly terminated by the user." ) );

			  AssertEmpty( AdminSubject, "CALL dbms.listQueries() YIELD query WITH * WHERE NOT query CONTAINS 'listQueries' RETURN *" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToTerminateOtherUsersQuery() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToTerminateOtherUsersQuery()
		 {
			  DoubleLatch latch = new DoubleLatch( 3, true );
			  ThreadedTransaction<S> read = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> write = new ThreadedTransaction<S>( Neo, latch );
			  string q1 = read.Execute( ThreadingConflict, ReadSubject, "UNWIND [1,2,3] AS x RETURN x" );
			  write.Execute( ThreadingConflict, WriteSubject, "UNWIND [4,5,6] AS y RETURN y" );
			  latch.StartAndWaitForAllToStart();

			  try
			  {
					string id1 = ExtractQueryId( q1 );
					AssertFail( WriteSubject, "CALL dbms.killQuery('" + id1 + "') YIELD username RETURN *", PERMISSION_DENIED );
					latch.FinishAndWaitForAllToFinish();
					read.CloseAndAssertSuccess();
					write.CloseAndAssertSuccess();
			  }
			  catch ( Exception t )
			  {
					latch.FinishAndWaitForAllToFinish();
					throw t;
			  }

			  AssertEmpty( AdminSubject, "CALL dbms.listQueries() YIELD query WITH * WHERE NOT query CONTAINS 'listQueries' RETURN *" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldTerminateQueriesEvenIfUsingPeriodicCommit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateQueriesEvenIfUsingPeriodicCommit()
		 {
			  for ( int batchSize = 8; batchSize <= 11; batchSize++ )
			  {
					// Spawns a throttled HTTP server, runs a PERIODIC COMMIT that fetches data from this server,
					// and checks that the query is visible when using listQueries()

					// Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.DoubleLatch latch = new org.Neo4Net.test.DoubleLatch(3, true);
					DoubleLatch latch = new DoubleLatch( 3, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.Barrier_Control barrier = new org.Neo4Net.test.Barrier_Control();
					Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();

					// Serve CSV via local web server, let Jetty find a random port for us
					Server server = CreateHttpServer( latch, barrier, batchSize, 50 - batchSize );
					server.start();
					int localPort = GetLocalPort( server );

					// When
					ThreadedTransaction<S> write = new ThreadedTransaction<S>( Neo, latch );

					try
					{
						 string writeQuery = write.ExecuteEarly( ThreadingConflict, WriteSubject, KernelTransaction.Type.@implicit, format( "USING PERIODIC COMMIT 10 LOAD CSV FROM 'http://localhost:%d' AS line ", localPort ) + "CREATE (n:A {id: line[0], square: line[1]}) RETURN count(*)" );
						 latch.StartAndWaitForAllToStart();

						 // Then
						 string writeQueryId = ExtractQueryId( writeQuery );

						 AssertSuccess(AdminSubject, "CALL dbms.killQuery('" + writeQueryId + "') YIELD username " + "RETURN count(username) AS count, username", r =>
						 {
									 IList<IDictionary<string, object>> actual = CollectResults( r );
									 Matcher<IDictionary<string, object>> mapMatcher = allOf( ( Matcher ) hasEntry( equalTo( "count" ), anyOf( equalTo( 1 ), equalTo( 1L ) ) ), ( Matcher ) hasEntry( equalTo( "username" ), equalTo( "writeSubject" ) ) );
									 assertThat( actual, matchesOneToOneInAnyOrder( mapMatcher ) );
						 });
					}
					finally
					{
						 // When
						 barrier.Release();
						 latch.FinishAndWaitForAllToFinish();

						 // Then
						 // We cannot assert on explicit termination here, because if the termination is detected when trying
						 // to lock we will only get the general TransactionTerminatedException
						 // (see {@link LockClientStateHolder}).
						 write.CloseAndAssertSomeTermination();

						 // stop server after assertion to avoid other kind of failures due to races (e.g., already closed
						 // lock clients )
						 server.stop();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKillMultipleUserQueries() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKillMultipleUserQueries()
		 {
			  DoubleLatch latch = new DoubleLatch( 5 );
			  ThreadedTransaction<S> read1 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> read2 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> read3 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> write = new ThreadedTransaction<S>( Neo, latch );
			  string q1 = read1.Execute( ThreadingConflict, ReadSubject, "UNWIND [1,2,3] AS x RETURN x" );
			  string q2 = read2.Execute( ThreadingConflict, ReadSubject, "UNWIND [4,5,6] AS y RETURN y" );
			  read3.Execute( ThreadingConflict, ReadSubject, "UNWIND [7,8,9] AS z RETURN z" );
			  write.Execute( ThreadingConflict, WriteSubject, "UNWIND [11,12,13] AS q RETURN q" );
			  latch.StartAndWaitForAllToStart();

			  string id1 = ExtractQueryId( q1 );
			  string id2 = ExtractQueryId( q2 );

			  string idParam = "['" + id1 + "', '" + id2 + "']";

			  AssertSuccess(AdminSubject, "CALL dbms.killQueries(" + idParam + ") YIELD username " + "RETURN count(username) AS count, username", r =>
			  {
						  IList<IDictionary<string, object>> actual = CollectResults( r );
						  Matcher<IDictionary<string, object>> mapMatcher = allOf( ( Matcher ) hasEntry( equalTo( "count" ), anyOf( equalTo( 2 ), equalTo( 2L ) ) ), ( Matcher ) hasEntry( equalTo( "username" ), equalTo( "readSubject" ) ) );
						  assertThat( actual, matchesOneToOneInAnyOrder( mapMatcher ) );
			  });

			  latch.FinishAndWaitForAllToFinish();
			  read1.CloseAndAssertExplicitTermination();
			  read2.CloseAndAssertExplicitTermination();
			  read3.CloseAndAssertSuccess();
			  write.CloseAndAssertSuccess();

			  AssertEmpty( AdminSubject, "CALL dbms.listQueries() YIELD query WITH * WHERE NOT query CONTAINS 'listQueries' RETURN *" );
		 }

		 internal virtual string ExtractQueryId( string writeQuery )
		 {
			  return ToRawValue( single( CollectSuccessResult( AdminSubject, "CALL dbms.listQueries()" ).Where( m => m.get( "query" ).Equals( ValueOf( writeQuery ) ) ).ToList() ).get("queryId") ).ToString();
		 }

		 //---------- set tx meta data -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveSetTXMetaDataProcedure()
		 public virtual void ShouldHaveSetTXMetaDataProcedure()
		 {
			  AssertEmpty( WriteSubject, "CALL dbms.setTXMetaData( { aKey: 'aValue' } )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readUpdatedMetadataValue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadUpdatedMetadataValue()
		 {
			  string testValue = "testValue";
			  string testKey = "test";
			  GraphDatabaseFacade graph = Neo.LocalGraph;
			  using ( InternalTransaction transaction = Neo.beginLocalTransactionAsUser( WriteSubject, KernelTransaction.Type.@explicit ) )
			  {
					graph.Execute( "CALL dbms.setTXMetaData({" + testKey + ":'" + testValue + "'})" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IDictionary<string, object> metadata = ( IDictionary<string, object> ) graph.Execute( "CALL dbms.getTXMetaData " ).next()["metadata"];
					assertEquals( testValue, metadata[testKey] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readEmptyMetadataInOtherTransaction()
		 public virtual void ReadEmptyMetadataInOtherTransaction()
		 {
			  string testValue = "testValue";
			  string testKey = "test";

			  AssertEmpty( WriteSubject, "CALL dbms.setTXMetaData({" + testKey + ":'" + testValue + "'})" );
			  AssertSuccess(WriteSubject, "CALL dbms.getTXMetaData", mapResourceIterator =>
			  {
				IDictionary<string, object> metadata = mapResourceIterator.next();
				assertNull( metadata.get( testKey ) );
				mapResourceIterator.close();
			  });
		 }

		 //---------- config manipulation -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setConfigValueShouldBeAccessibleOnlyToAdmins()
		 public virtual void SetConfigValueShouldBeAccessibleOnlyToAdmins()
		 {
			  string call = "CALL dbms.setConfigValue('dbms.logs.query.enabled', 'false')";
			  AssertFail( WriteSubject, call, PERMISSION_DENIED );
			  AssertFail( SchemaSubject, call, PERMISSION_DENIED );
			  AssertFail( ReadSubject, call, PERMISSION_DENIED );

			  AssertEmpty( AdminSubject, call );
		 }

		 //---------- procedure guard -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateLongRunningProcedureThatChecksTheGuardRegularlyIfKilled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateLongRunningProcedureThatChecksTheGuardRegularlyIfKilled()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.DoubleLatch latch = new org.Neo4Net.test.DoubleLatch(2, true);
			  DoubleLatch latch = new DoubleLatch( 2, true );
			  ClassWithProcedures.VolatileLatch = latch;

			  string loopQuery = "CALL test.loop";

			  Thread loopQueryThread = new Thread( () => assertFail(ReadSubject, loopQuery, "Explicitly terminated by the user.") );
			  loopQueryThread.Start();
			  latch.StartAndWaitForAllToStart();

			  try
			  {
					string loopId = ExtractQueryId( loopQuery );

					AssertSuccess(AdminSubject, "CALL dbms.killQuery('" + loopId + "') YIELD username " + "RETURN count(username) AS count, username", r =>
					{
								IList<IDictionary<string, object>> actual = CollectResults( r );
								Matcher<IDictionary<string, object>> mapMatcher = allOf( ( Matcher ) hasEntry( equalTo( "count" ), anyOf( equalTo( 1 ), equalTo( 1L ) ) ), ( Matcher ) hasEntry( equalTo( "username" ), equalTo( "readSubject" ) ) );
								assertThat( actual, matchesOneToOneInAnyOrder( mapMatcher ) );
					});
			  }
			  finally
			  {
					latch.FinishAndWaitForAllToFinish();
			  }

			  // there is a race with "test.loop" procedure - after decrementing latch it may take time to actually exit
			  loopQueryThread.Join( 10_000 );

			  AssertEmpty( AdminSubject, "CALL dbms.listQueries() YIELD query WITH * WHERE NOT query CONTAINS 'listQueries' RETURN *" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWriteAfterAllowedReadProcedureForWriteUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleWriteAfterAllowedReadProcedureForWriteUser()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  UserManager.addRoleToUser( PUBLISHER, "role1Subject" );
			  AssertEmpty( Neo.login( "role1Subject", "abc" ), "CALL test.allowedReadProcedure() YIELD value CREATE (:NEWNODE {name:value})" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonWriterToWriteAfterCallingAllowedWriteProc() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowNonWriterToWriteAfterCallingAllowedWriteProc()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "nopermission", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "nopermission" );
			  // should be able to invoke allowed procedure
			  AssertSuccess( Neo.login( "nopermission", "abc" ), "CALL test.allowedWriteProcedure()", itr => assertEquals( itr.ToList().size(), 2 ) );
			  // should not be able to do writes
			  AssertFail( Neo.login( "nopermission", "abc" ), "CALL test.allowedWriteProcedure() YIELD value CREATE (:NEWNODE {name:value})", WriteOpsNotAllowed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowUnauthorizedAccessToProcedure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowUnauthorizedAccessToProcedure()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "nopermission", password( "abc" ), false );
			  // should not be able to invoke any procedure
			  AssertFail( Neo.login( "nopermission", "abc" ), "CALL test.staticReadProcedure()", ReadOpsNotAllowed );
			  AssertFail( Neo.login( "nopermission", "abc" ), "CALL test.staticWriteProcedure()", WriteOpsNotAllowed );
			  AssertFail( Neo.login( "nopermission", "abc" ), "CALL test.staticSchemaProcedure()", SchemaOpsNotAllowed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonReaderToReadAfterCallingAllowedReadProc() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowNonReaderToReadAfterCallingAllowedReadProc()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "nopermission", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "nopermission" );
			  // should not be able to invoke any procedure
			  AssertSuccess( Neo.login( "nopermission", "abc" ), "CALL test.allowedReadProcedure()", itr => assertEquals( itr.ToList().size(), 1 ) );
			  AssertFail( Neo.login( "nopermission", "abc" ), "CALL test.allowedReadProcedure() YIELD value MATCH (n:Secret) RETURN n.pass", ReadOpsNotAllowed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNestedReadProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNestedReadProcedures()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  AssertSuccess( Neo.login( "role1Subject", "abc" ), "CALL test.nestedAllowedProcedure('test.allowedReadProcedure') YIELD value", r => assertKeyIs( r, "value", "foo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDoubleNestedReadProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDoubleNestedReadProcedures()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  AssertSuccess( Neo.login( "role1Subject", "abc" ), "CALL test.doubleNestedAllowedProcedure YIELD value", r => assertKeyIs( r, "value", "foo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNestedAllowedWriteProcedureFromAllowedReadProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNestedAllowedWriteProcedureFromAllowedReadProcedure()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  AssertFail( Neo.login( "role1Subject", "abc" ), "CALL test.nestedAllowedProcedure('test.allowedWriteProcedure') YIELD value", WriteOpsNotAllowed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNestedAllowedWriteProcedureFromAllowedReadProcedureEvenIfAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNestedAllowedWriteProcedureFromAllowedReadProcedureEvenIfAdmin()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  UserManager.addRoleToUser( PredefinedRoles.ADMIN, "role1Subject" );
			  AssertFail( Neo.login( "role1Subject", "abc" ), "CALL test.nestedAllowedProcedure('test.allowedWriteProcedure') YIELD value", WriteOpsNotAllowed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRestrictNestedReadProcedureFromAllowedWriteProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRestrictNestedReadProcedureFromAllowedWriteProcedures()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  AssertFail( Neo.login( "role1Subject", "abc" ), "CALL test.failingNestedAllowedWriteProcedure YIELD value", WriteOpsNotAllowed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNestedReadProcedureWithDifferentAllowedRole() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNestedReadProcedureWithDifferentAllowedRole()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  AssertSuccess( Neo.login( "role1Subject", "abc" ), "CALL test.nestedAllowedProcedure('test.otherAllowedReadProcedure') YIELD value", r => assertKeyIs( r, "value", "foo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNestedAllowedWriteProcedureFromNormalReadProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNestedAllowedWriteProcedureFromNormalReadProcedure()
		 {
			  UserManager = Neo.LocalUserManager;
			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  UserManager.addRoleToUser( PredefinedRoles.PUBLISHER, "role1Subject" ); // Even if subject has WRITE permission
			  // the procedure should restrict to READ
			  AssertFail( Neo.login( "role1Subject", "abc" ), "CALL test.nestedReadProcedure('test.allowedWriteProcedure') YIELD value", WriteOpsNotAllowed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFunctionWithAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleFunctionWithAllowed()
		 {
			  UserManager = Neo.LocalUserManager;

			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  AssertSuccess( Neo.login( "role1Subject", "abc" ), "RETURN test.allowedFunction1() AS value", r => assertKeyIs( r, "value", "foo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNestedFunctionsWithAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNestedFunctionsWithAllowed()
		 {
			  UserManager = Neo.LocalUserManager;

			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  AssertSuccess( Neo.login( "role1Subject", "abc" ), "RETURN test.nestedAllowedFunction('test.allowedFunction1()') AS value", r => assertKeyIs( r, "value", "foo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNestedFunctionWithDifferentAllowedRole() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNestedFunctionWithDifferentAllowedRole()
		 {
			  UserManager = Neo.LocalUserManager;

			  UserManager.newUser( "role1Subject", password( "abc" ), false );
			  UserManager.newRole( "role1" );
			  UserManager.addRoleToUser( "role1", "role1Subject" );
			  AssertSuccess( Neo.login( "role1Subject", "abc" ), "RETURN test.nestedAllowedFunction('test.allowedFunction2()') AS value", r => assertKeyIs( r, "value", "foo" ) );
		 }

		 //---------- clearing query cache -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotClearQueryCachesIfNotAdmin()
		 public virtual void ShouldNotClearQueryCachesIfNotAdmin()
		 {
			  AssertFail( NoneSubject, "CALL dbms.clearQueryCaches()", PERMISSION_DENIED );
			  AssertFail( ReadSubject, "CALL dbms.clearQueryCaches()", PERMISSION_DENIED );
			  AssertFail( WriteSubject, "CALL dbms.clearQueryCaches()", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.clearQueryCaches()", PERMISSION_DENIED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearQueryCachesIfAdmin()
		 public virtual void ShouldClearQueryCachesIfAdmin()
		 {
			  AssertSuccess( AdminSubject,"CALL dbms.clearQueryCaches()", ResourceIterator.close );
			  // any answer is okay, as long as it isn't denied. That is why we don't care about the actual result here
		 }

		 /*
		 This surface is hidden in 3.1, to possibly be completely removed or reworked later
		 ==================================================================================
		  */

		 //---------- terminate transactions for user -----------

		 //@Test
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shouldTerminateTransactionForUser() throws Throwable
		 public virtual void ShouldTerminateTransactionForUser()
		 {
			  DoubleLatch latch = new DoubleLatch( 2 );
			  ThreadedTransaction<S> write = new ThreadedTransaction<S>( Neo, latch );
			  write.ExecuteCreateNode( ThreadingConflict, WriteSubject );
			  latch.StartAndWaitForAllToStart();

			  AssertSuccess( AdminSubject, "CALL dbms.terminateTransactionsForUser( 'writeSubject' )", r => assertKeyIsMap( r, "username", "transactionsTerminated", map( "writeSubject", "1" ) ) );

			  AssertSuccess( AdminSubject, "CALL dbms.listTransactions()", r => assertKeyIsMap( r, "username", "activeTransactions", map( "adminSubject", "1" ) ) );

			  latch.FinishAndWaitForAllToFinish();

			  write.CloseAndAssertExplicitTermination();

			  AssertEmpty( AdminSubject, "MATCH (n:Test) RETURN n.name AS name" );
		 }

		 //@Test
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shouldTerminateOnlyGivenUsersTransaction() throws Throwable
		 public virtual void ShouldTerminateOnlyGivenUsersTransaction()
		 {
			  DoubleLatch latch = new DoubleLatch( 3 );
			  ThreadedTransaction<S> schema = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> write = new ThreadedTransaction<S>( Neo, latch );

			  Schema.executeCreateNode( ThreadingConflict, SchemaSubject );
			  write.ExecuteCreateNode( ThreadingConflict, WriteSubject );
			  latch.StartAndWaitForAllToStart();

			  AssertSuccess( AdminSubject, "CALL dbms.terminateTransactionsForUser( 'schemaSubject' )", r => assertKeyIsMap( r, "username", "transactionsTerminated", map( "schemaSubject", "1" ) ) );

			  AssertSuccess( AdminSubject, "CALL dbms.listTransactions()", r => assertKeyIsMap( r, "username", "activeTransactions", map( "adminSubject", "1", "writeSubject", "1" ) ) );

			  latch.FinishAndWaitForAllToFinish();

			  Schema.closeAndAssertExplicitTermination();
			  write.CloseAndAssertSuccess();

			  AssertSuccess( AdminSubject, "MATCH (n:Test) RETURN n.name AS name", r => assertKeyIs( r, "name", "writeSubject-node" ) );
		 }

		 //@Test
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shouldTerminateAllTransactionsForGivenUser() throws Throwable
		 public virtual void ShouldTerminateAllTransactionsForGivenUser()
		 {
			  DoubleLatch latch = new DoubleLatch( 3 );
			  ThreadedTransaction<S> schema1 = new ThreadedTransaction<S>( Neo, latch );
			  ThreadedTransaction<S> schema2 = new ThreadedTransaction<S>( Neo, latch );

			  schema1.ExecuteCreateNode( ThreadingConflict, SchemaSubject );
			  schema2.ExecuteCreateNode( ThreadingConflict, SchemaSubject );
			  latch.StartAndWaitForAllToStart();

			  AssertSuccess( AdminSubject, "CALL dbms.terminateTransactionsForUser( 'schemaSubject' )", r => assertKeyIsMap( r, "username", "transactionsTerminated", map( "schemaSubject", "2" ) ) );

			  AssertSuccess( AdminSubject, "CALL dbms.listTransactions()", r => assertKeyIsMap( r, "username", "activeTransactions", map( "adminSubject", "1" ) ) );

			  latch.FinishAndWaitForAllToFinish();

			  schema1.CloseAndAssertExplicitTermination();
			  schema2.CloseAndAssertExplicitTermination();

			  AssertEmpty( AdminSubject, "MATCH (n:Test) RETURN n.name AS name" );
		 }

		 //@Test
		 public virtual void ShouldNotTerminateTerminationTransaction()
		 {
			  AssertSuccess( AdminSubject, "CALL dbms.terminateTransactionsForUser( 'adminSubject' )", r => assertKeyIsMap( r, "username", "transactionsTerminated", map( "adminSubject", "0" ) ) );
			  AssertSuccess( ReadSubject, "CALL dbms.terminateTransactionsForUser( 'readSubject' )", r => assertKeyIsMap( r, "username", "transactionsTerminated", map( "readSubject", "0" ) ) );
		 }

		 //@Test
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shouldTerminateSelfTransactionsExceptTerminationTransactionIfAdmin() throws Throwable
		 public virtual void ShouldTerminateSelfTransactionsExceptTerminationTransactionIfAdmin()
		 {
			  ShouldTerminateSelfTransactionsExceptTerminationTransaction( AdminSubject );
		 }

		 //@Test
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shouldTerminateSelfTransactionsExceptTerminationTransactionIfNotAdmin() throws Throwable
		 public virtual void ShouldTerminateSelfTransactionsExceptTerminationTransactionIfNotAdmin()
		 {
			  ShouldTerminateSelfTransactionsExceptTerminationTransaction( WriteSubject );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldTerminateSelfTransactionsExceptTerminationTransaction(S subject) throws Throwable
		 private void ShouldTerminateSelfTransactionsExceptTerminationTransaction( S subject )
		 {
			  DoubleLatch latch = new DoubleLatch( 2 );
			  ThreadedTransaction<S> create = new ThreadedTransaction<S>( Neo, latch );
			  create.ExecuteCreateNode( ThreadingConflict, subject );

			  latch.StartAndWaitForAllToStart();

			  string subjectName = Neo.nameOf( subject );
			  AssertSuccess( subject, "CALL dbms.terminateTransactionsForUser( '" + subjectName + "' )", r => assertKeyIsMap( r, "username", "transactionsTerminated", map( subjectName, "1" ) ) );

			  latch.FinishAndWaitForAllToFinish();

			  create.CloseAndAssertExplicitTermination();

			  AssertEmpty( AdminSubject, "MATCH (n:Test) RETURN n.name AS name" );
		 }

		 //@Test
		 public virtual void ShouldNotTerminateTransactionsIfNonExistentUser()
		 {
			  AssertFail( AdminSubject, "CALL dbms.terminateTransactionsForUser( 'Petra' )", "User 'Petra' does not exist" );
			  AssertFail( AdminSubject, "CALL dbms.terminateTransactionsForUser( '' )", "User '' does not exist" );
		 }

		 //@Test
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shouldNotTerminateTransactionsIfNotAdmin() throws Throwable
		 public virtual void ShouldNotTerminateTransactionsIfNotAdmin()
		 {
			  DoubleLatch latch = new DoubleLatch( 2 );
			  ThreadedTransaction<S> write = new ThreadedTransaction<S>( Neo, latch );
			  write.ExecuteCreateNode( ThreadingConflict, WriteSubject );
			  latch.StartAndWaitForAllToStart();

			  AssertFail( NoneSubject, "CALL dbms.terminateTransactionsForUser( 'writeSubject' )", PERMISSION_DENIED );
			  AssertFail( PwdSubject, "CALL dbms.terminateTransactionsForUser( 'writeSubject' )", ChangePwdErrMsg );
			  AssertFail( ReadSubject, "CALL dbms.terminateTransactionsForUser( 'writeSubject' )", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.terminateTransactionsForUser( 'writeSubject' )", PERMISSION_DENIED );

			  AssertSuccess( AdminSubject, "CALL dbms.listTransactions()", r => assertKeyIs( r, "username", "adminSubject", "writeSubject" ) );

			  latch.FinishAndWaitForAllToFinish();

			  write.CloseAndAssertSuccess();

			  AssertSuccess( AdminSubject, "MATCH (n:Test) RETURN n.name AS name", r => assertKeyIs( r, "name", "writeSubject-node" ) );
		 }

		 //@Test
		 public virtual void ShouldTerminateRestrictedTransaction()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.DoubleLatch doubleLatch = new org.Neo4Net.test.DoubleLatch(2);
			  DoubleLatch doubleLatch = new DoubleLatch( 2 );

			  ClassWithProcedures.TestLatch = new ClassWithProcedures.LatchedRunnables( doubleLatch, EMPTY_RUNNABLE, EMPTY_RUNNABLE );

			  ( new Thread( () => assertFail(WriteSubject, "CALL test.waitForLatch()", "Explicitly terminated by the user.") ) ).Start();

			  doubleLatch.StartAndWaitForAllToStart();
			  try
			  {
					AssertSuccess( AdminSubject, "CALL dbms.terminateTransactionsForUser( 'writeSubject' )", r => assertKeyIsMap( r, "username", "transactionsTerminated", map( "writeSubject", "1" ) ) );
			  }
			  finally
			  {
					doubleLatch.FinishAndWaitForAllToFinish();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertQueryIsRunning(String query) throws InterruptedException
		 private void AssertQueryIsRunning( string query )
		 {
			  assertEventually( "Query did not appear in dbms.listQueries output", () => QueryIsRunning(query), equalTo(true), 1, TimeUnit.MINUTES );
		 }

		 private bool QueryIsRunning( string targetQuery )
		 {
			  string query = "CALL dbms.listQueries() YIELD query WITH query WHERE query = '" + targetQuery + "' RETURN 1";
			  MutableBoolean resultIsNotEmpty = new MutableBoolean();
			  Neo.executeQuery( AdminSubject, query, emptyMap(), itr => resultIsNotEmpty.setValue(itr.hasNext()) );
			  return resultIsNotEmpty.booleanValue();
		 }

		 /*
		 ==================================================================================
		  */

		 //---------- jetty helpers for serving CSV files -----------

		 private int GetLocalPort( Server server )
		 {
			  return ( ( ServerConnector )( server.Connectors[0] ) ).LocalPort;

		 }

		 //---------- matchers-----------

		 private Matcher<IDictionary<string, object>> ListedTransactionOfInteractionLevel( OffsetDateTime startTime, string username, string currentQuery )
		 {
			  return allOf( HasCurrentQuery( currentQuery ), HasUsername( username ), HasTransactionId(), HasStartTimeAfter(startTime), HasProtocol(Neo.ConnectionProtocol) );
		 }

		 private Matcher<IDictionary<string, object>> ListedQuery( OffsetDateTime startTime, string username, string query )
		 {
			  return allOf( HasQuery( query ), HasUsername( username ), HasQueryId(), HasStartTimeAfter(startTime), HasNoParameters() );
		 }

		 private Matcher<IDictionary<string, object>> ListedTransaction( OffsetDateTime startTime, string username, string currentQuery )
		 {
			  return allOf( HasCurrentQuery( currentQuery ), HasUsername( username ), HasTransactionId(), HasStartTimeAfter(startTime) );
		 }

		 /// <summary>
		 /// Executes a query through the NeoInteractionLevel required
		 /// </summary>
		 private Matcher<IDictionary<string, object>> ListedQueryOfInteractionLevel( OffsetDateTime startTime, string username, string query )
		 {
			  return allOf( HasQuery( query ), HasUsername( username ), HasQueryId(), HasStartTimeAfter(startTime), HasNoParameters(), HasProtocol(Neo.ConnectionProtocol) );
		 }

		 private Matcher<IDictionary<string, object>> ListedQueryWithMetaData( OffsetDateTime startTime, string username, string query, IDictionary<string, object> metaData )
		 {
			  return allOf( HasQuery( query ), HasUsername( username ), HasQueryId(), HasStartTimeAfter(startTime), HasNoParameters(), HasMetaData(metaData) );
		 }

		 private Matcher<IDictionary<string, object>> ListedTransactionWithMetaData( OffsetDateTime startTime, string username, string currentQuery, IDictionary<string, object> metaData )
		 {
			  return allOf( HasCurrentQuery( currentQuery ), HasUsername( username ), HasTransactionId(), HasStartTimeAfter(startTime), HasMetaData(metaData) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.hamcrest.Matcher<java.util.Map<String,Object>> hasQuery(String query)
		 private Matcher<IDictionary<string, object>> HasQuery( string query )
		 {
			  return ( Matcher ) hasEntry( equalTo( "query" ), equalTo( query ) );
		 }

		 private Matcher<IDictionary<string, object>> HasCurrentQuery( string currentQuery )
		 {
			  return ( Matcher ) hasEntry( equalTo( "currentQuery" ), equalTo( currentQuery ) );
		 }

		 private Matcher<IDictionary<string, object>> HasStatus( string statusPrefix )
		 {
			  return ( Matcher ) hasEntry( equalTo( "status" ), startsWith( statusPrefix ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.hamcrest.Matcher<java.util.Map<String,Object>> hasUsername(String username)
		 private Matcher<IDictionary<string, object>> HasUsername( string username )
		 {
			  return ( Matcher ) hasEntry( equalTo( "username" ), equalTo( username ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.hamcrest.Matcher<java.util.Map<String,Object>> hasQueryId()
		 private Matcher<IDictionary<string, object>> HasQueryId()
		 {
			  Matcher<string> queryId = equalTo( "queryId" );
			  Matcher valueMatcher = allOf( ( Matcher ) isA( typeof( string ) ), ( Matcher ) containsString( QueryId.QUERY_ID_PREFIX ) );
			  return hasEntry( queryId, valueMatcher );
		 }

		 private Matcher<IDictionary<string, object>> HasTransactionId()
		 {
			  Matcher<string> transactionId = equalTo( "transactionId" );
			  Matcher valueMatcher = allOf( ( Matcher ) isA( typeof( string ) ), ( Matcher ) containsString( "transaction-" ) );
			  return hasEntry( transactionId, valueMatcher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.hamcrest.Matcher<java.util.Map<String,Object>> hasStartTimeAfter(java.time.OffsetDateTime startTime)
		 private Matcher<IDictionary<string, object>> HasStartTimeAfter( OffsetDateTime startTime )
		 {
			  return ( Matcher ) hasEntry( equalTo( "startTime" ), new BaseMatcherAnonymousInnerClass( this, startTime ) );
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<string>
		 {
			 private readonly BuiltInProceduresInteractionTestBase<S> _outerInstance;

			 private OffsetDateTime _startTime;

			 public BaseMatcherAnonymousInnerClass( BuiltInProceduresInteractionTestBase<S> outerInstance, OffsetDateTime startTime )
			 {
				 this.outerInstance = outerInstance;
				 this._startTime = startTime;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "should be after " + _startTime.ToString() );
			 }

			 public override bool matches( object item )
			 {
				  OffsetDateTime otherTime = from( ISO_OFFSET_DATE_TIME.parse( item.ToString() ) );
				  return _startTime.compareTo( otherTime ) <= 0;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.hamcrest.Matcher<java.util.Map<String,Object>> hasNoParameters()
		 private Matcher<IDictionary<string, object>> HasNoParameters()
		 {
			  return ( Matcher ) hasEntry( equalTo( "parameters" ), equalTo( emptyMap() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.hamcrest.Matcher<java.util.Map<String,Object>> hasProtocol(String expected)
		 private Matcher<IDictionary<string, object>> HasProtocol( string expected )
		 {
			  return ( Matcher ) hasEntry( "protocol", expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.hamcrest.Matcher<java.util.Map<String,Object>> hasMetaData(java.util.Map<String,Object> expected)
		 private Matcher<IDictionary<string, object>> HasMetaData( IDictionary<string, object> expected )
		 {
			  return ( Matcher ) hasEntry( equalTo( "metaData" ), allOf( expected.SetOfKeyValuePairs().Select(EntryMapper()).ToList() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"rawtypes", "unchecked"}) private System.Func<java.util.Map.Entry<String,Object>,org.hamcrest.Matcher<java.util.Map.Entry<String,Object>>> entryMapper()
		 private System.Func<KeyValuePair<string, object>, Matcher<KeyValuePair<string, object>>> EntryMapper()
		 {
			  return entry =>
			  {
				Matcher keyMatcher = equalTo( entry.Key );
				Matcher valueMatcher = equalTo( entry.Value );
				return hasEntry( keyMatcher, valueMatcher );
			  };
		 }

		 private IList<IDictionary<string, object>> CollectResults( ResourceIterator<IDictionary<string, object>> results )
		 {
			  IList<IDictionary<string, object>> maps = results.ToList();
			  IList<IDictionary<string, object>> transformed = new List<IDictionary<string, object>>( maps.Count );
			  foreach ( IDictionary<string, object> map in maps )
			  {
					IDictionary<string, object> transformedMap = new Dictionary<string, object>( map.Count );
					foreach ( KeyValuePair<string, object> entry in map.SetOfKeyValuePairs() )
					{
						 transformedMap[entry.Key] = ToRawValue( entry.Value );
					}
					transformed.Add( transformedMap );
			  }
			  return transformed;
		 }

		 public static Server CreateHttpServer( DoubleLatch latch, Neo4Net.Test.Barrier_Control innerBarrier, int firstBatchSize, int otherBatchSize )
		 {
			  Server server = new Server( 0 );
			  server.Handler = new AbstractHandlerAnonymousInnerClass( latch, innerBarrier, firstBatchSize, otherBatchSize );
			  return server;
		 }

		 private class AbstractHandlerAnonymousInnerClass : AbstractHandler
		 {
			 private DoubleLatch _latch;
			 private Neo4Net.Test.Barrier_Control _innerBarrier;
			 private int _firstBatchSize;
			 private int _otherBatchSize;

			 public AbstractHandlerAnonymousInnerClass( DoubleLatch latch, Neo4Net.Test.Barrier_Control innerBarrier, int firstBatchSize, int otherBatchSize )
			 {
				 this._latch = latch;
				 this._innerBarrier = innerBarrier;
				 this._firstBatchSize = firstBatchSize;
				 this._otherBatchSize = otherBatchSize;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(String target, org.eclipse.jetty.server.Request baseRequest, javax.servlet.http.HttpServletRequest request, javax.servlet.http.HttpServletResponse response) throws java.io.IOException
			 public override void handle( string target, Request baseRequest, HttpServletRequest request, HttpServletResponse response )
			 {
				  response.ContentType = "text/plain; charset=utf-8";
				  response.Status = HttpServletResponse.SC_OK;
				  PrintWriter @out = response.Writer;

				  writeBatch( @out, _firstBatchSize );
				  @out.flush();
				  _latch.start();
				  _innerBarrier.reached();

				  _latch.finish();
				  writeBatch( @out, _otherBatchSize );
				  baseRequest.Handled = true;
			 }

			 private void writeBatch( PrintWriter @out, int batchSize )
			 {
				  for ( int i = 0; i < batchSize; i++ )
				  {
						@out.write( format( "%d %d\n", i, i * i ) );
						i++;
				  }
			 }
		 }

		 private static OffsetDateTime StartTime
		 {
			 get
			 {
				  return ofInstant( Instant.ofEpochMilli( now().toEpochSecond() ), ZoneOffset.UTC );
			 }
		 }
	}

}