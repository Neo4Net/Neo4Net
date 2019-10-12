using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Bolt
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Config = Org.Neo4j.driver.v1.Config;
	using Driver = Org.Neo4j.driver.v1.Driver;
	using GraphDatabase = Org.Neo4j.driver.v1.GraphDatabase;
	using Session = Org.Neo4j.driver.v1.Session;
	using Transaction = Org.Neo4j.driver.v1.Transaction;
	using ClientException = Org.Neo4j.driver.v1.exceptions.ClientException;
	using ServiceUnavailableException = Org.Neo4j.driver.v1.exceptions.ServiceUnavailableException;
	using TransientException = Org.Neo4j.driver.v1.exceptions.TransientException;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Result = Org.Neo4j.Graphdb.Result;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using EnterpriseNeo4jRule = Org.Neo4j.Harness.junit.EnterpriseNeo4jRule;
	using Neo4jRule = Org.Neo4j.Harness.junit.Neo4jRule;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using KernelTransactions = Org.Neo4j.Kernel.Impl.Api.KernelTransactions;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using VerboseTimeout = Org.Neo4j.Test.rule.VerboseTimeout;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.driver.@internal.logging.DevNullLogging.DEV_NULL_LOGGING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.await;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.rootCause;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.NamedThreadFactory.daemon;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class SessionResetIT
	{
		private bool InstanceFieldsInitialized = false;

		public SessionResetIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _timeout ).around( _db );
		}

		 private const string SHORT_QUERY_1 = "CREATE (n:Node {name: 'foo', occupation: 'bar'})";
		 private const string SHORT_QUERY_2 = "MATCH (n:Node {name: 'foo'}) RETURN count(n)";
		 private const string LONG_QUERY = "UNWIND range(0, 10000000) AS i CREATE (n:Node {idx: i}) DELETE n";

		 private static readonly int _stressItThreadCount = Runtime.Runtime.availableProcessors() * 2;
		 private static readonly long _stressItDurationMs = SECONDS.toMillis( 5 );
		 private static readonly string[] _stressItQueries = new string[] { SHORT_QUERY_1, SHORT_QUERY_2, LONG_QUERY };

		 private readonly VerboseTimeout _timeout = VerboseTimeout.builder().withTimeout(6, MINUTES).build();
		 private readonly Neo4jRule _db = new EnterpriseNeo4jRule().withConfig(GraphDatabaseSettings.load_csv_file_url_root, "import").withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).dumpLogsOnFailure(System.out);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(timeout).around(db);
		 public RuleChain RuleChain;

		 private Driver _driver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _driver = GraphDatabase.driver( _db.boltURI(), Config.build().withLogging(DEV_NULL_LOGGING).toConfig() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  IOUtils.closeAllSilently( _driver );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateAutoCommitQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateAutoCommitQuery()
		 {
			  TestQueryTermination( LONG_QUERY, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateQueryInExplicitTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateQueryInExplicitTransaction()
		 {
			  TestQueryTermination( LONG_QUERY, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateAutoCommitQueriesRandomly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateAutoCommitQueriesRandomly()
		 {
			  TestRandomQueryTermination( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateQueriesInExplicitTransactionsRandomly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateQueriesInExplicitTransactionsRandomly()
		 {
			  TestRandomQueryTermination( false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testRandomQueryTermination(boolean autoCommit) throws Exception
		 private void TestRandomQueryTermination( bool autoCommit )
		 {
			  ExecutorService executor = Executors.newFixedThreadPool( _stressItThreadCount, daemon( "test-worker" ) );
			  ISet<Session> runningSessions = newSetFromMap( new ConcurrentDictionary<Session>() );
			  AtomicBoolean stop = new AtomicBoolean();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> futures = new java.util.ArrayList<>();
			  IList<Future<object>> futures = new List<Future<object>>();

			  for ( int i = 0; i < _stressItThreadCount; i++ )
			  {
					futures.Add(executor.submit(() =>
					{
					 ThreadLocalRandom random = ThreadLocalRandom.current();
					 while ( !stop.get() )
					 {
						  RunRandomQuery( autoCommit, random, runningSessions, stop );
					 }
					}));
			  }

			  long deadline = DateTimeHelper.CurrentUnixTimeMillis() + _stressItDurationMs;
			  while ( !stop.get() )
			  {
					if ( DateTimeHelper.CurrentUnixTimeMillis() > deadline )
					{
						 stop.set( true );
					}

					ResetAny( runningSessions );

					MILLISECONDS.sleep( 30 );
			  }

			  _driver.close();
			  AwaitAll( futures );
			  AssertDatabaseIsIdle();
		 }

		 private void RunRandomQuery( bool autoCommit, Random random, ISet<Session> runningSessions, AtomicBoolean stop )
		 {
			  try
			  {
					Session session = _driver.session();
					runningSessions.Add( session );
					try
					{
						 string query = _stressItQueries[random.Next( _stressItQueries.Length - 1 )];
						 RunQuery( session, query, autoCommit );
					}
					finally
					{
						 runningSessions.remove( session );
						 session.close();
					}
			  }
			  catch ( Exception error )
			  {
					if ( !stop.get() && !IsAcceptable(error) )
					{
						 stop.set( true );
						 throw error;
					}
					// else it is fine to receive some errors from the driver because
					// sessions are being reset concurrently by the main thread, driver can also be closed concurrently
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testQueryTermination(String query, boolean autoCommit) throws Exception
		 private void TestQueryTermination( string query, bool autoCommit )
		 {
			  Future<Void> queryResult = RunQueryInDifferentThreadAndResetSession( query, autoCommit );

			  try
			  {
					queryResult.get( 10, SECONDS );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( ExecutionException ) ) );
					assertTrue( IsTransactionTerminatedException( e.InnerException ) );
			  }

			  AssertDatabaseIsIdle();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.concurrent.Future<Void> runQueryInDifferentThreadAndResetSession(String query, boolean autoCommit) throws Exception
		 private Future<Void> RunQueryInDifferentThreadAndResetSession( string query, bool autoCommit )
		 {
			  AtomicReference<Session> sessionRef = new AtomicReference<Session>();

			  Future<Void> queryResult = runAsync(() =>
			  {
				using ( Session session = _driver.session() )
				{
					 sessionRef.set( session );
					 RunQuery( session, query, autoCommit );
				}
			  });

			  await( () => ActiveQueriesCount() == 1, 10, SECONDS );
			  SECONDS.sleep( 1 ); // additionally wait a bit before resetting the session

			  Session session = sessionRef.get();
			  assertNotNull( session );
			  session.reset();

			  return queryResult;
		 }

		 private static void RunQuery( Session session, string query, bool autoCommit )
		 {
			  if ( autoCommit )
			  {
					session.run( query ).consume();
			  }
			  else
			  {
					using ( Transaction tx = session.beginTransaction() )
					{
						 tx.run( query );
						 tx.success();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertDatabaseIsIdle() throws InterruptedException
		 private void AssertDatabaseIsIdle()
		 {
			  assertEventually( "Wrong number of active queries", this.activeQueriesCount, @is( 0L ), 10, SECONDS );
			  assertEventually( "Wrong number of active transactions", this.activeTransactionsCount, @is( 0L ), 10, SECONDS );
		 }

		 private long ActiveQueriesCount()
		 {
			  using ( Result result = Db().execute("CALL dbms.listQueries() YIELD queryId RETURN count(queryId) AS result") )
			  {
					return ( long ) single( result ).get( "result" ) - 1; // do not count listQueries procedure invocation
			  }
		 }

		 private long ActiveTransactionsCount()
		 {
			  DependencyResolver resolver = Db().DependencyResolver;
			  KernelTransactions kernelTransactions = resolver.ResolveDependency( typeof( KernelTransactions ) );
			  return kernelTransactions.ActiveTransactions().Count;
		 }

		 private GraphDatabaseAPI Db()
		 {
			  return ( GraphDatabaseAPI ) _db.GraphDatabaseService;
		 }

		 private static void ResetAny( ISet<Session> sessions )
		 {
			  sessions.First().ifPresent(session =>
			  {
				if ( sessions.remove( session ) )
				{
					 ResetSafely( session );
				}
			  });
		 }

		 private static void ResetSafely( Session session )
		 {
			  try
			  {
					if ( session.Open )
					{
						 session.reset();
					}
			  }
			  catch ( ClientException e )
			  {
					if ( session.Open )
					{
						 throw e;
					}
					// else this thread lost race with close and it's fine
			  }
		 }

		 private static bool IsAcceptable( Exception error )
		 {
			  Exception cause = rootCause( error );

			  return IsTransactionTerminatedException( cause ) || cause is ServiceUnavailableException || cause is ClientException || cause is ClosedChannelException;
		 }

		 private static bool IsTransactionTerminatedException( Exception error )
		 {
			  return error is TransientException && error.Message.StartsWith( "The transaction has been terminated" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void awaitAll(java.util.List<java.util.concurrent.Future<?>> futures) throws Exception
		 private static void AwaitAll<T1>( IList<T1> futures )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
			  foreach ( Future<object> future in futures )
			  {
					assertNull( future.get( 1, MINUTES ) );
			  }
		 }
	}

}