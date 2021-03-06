﻿using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
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
namespace Org.Neo4j
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using Org.Neo4j.Function;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ServerControls = Org.Neo4j.Harness.ServerControls;
	using TestServerBuilders = Org.Neo4j.Harness.TestServerBuilders;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using HaSettings = Org.Neo4j.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using KernelTransactions = Org.Neo4j.Kernel.Impl.Api.KernelTransactions;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using LockClientStoppedException = Org.Neo4j.Kernel.impl.locking.LockClientStoppedException;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using JsonParseException = Org.Neo4j.Server.rest.domain.JsonParseException;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using HTTP = Org.Neo4j.Test.server.HTTP;
	using RawPayload = Org.Neo4j.Test.server.HTTP.RawPayload;
	using Response = Org.Neo4j.Test.server.HTTP.Response;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
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
//	import static org.neo4j.helpers.NamedThreadFactory.named;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.containsNoErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.transactional.integration.TransactionMatchers.hasErrors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.withBaseUri;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TransactionTerminationIT
	public class TransactionTerminationIT
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionTerminationIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( SuppressOutput.suppressAll() ).around(_cleanupRule).around(_clusterRule);
		}

		 private static readonly Label _label = Label.label( "Foo" );
		 private const string PROPERTY = "bar";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public String lockManagerName;
		 public string LockManagerName;

		 private readonly CleanupRule _cleanupRule = new CleanupRule();
		 private readonly ClusterRule _clusterRule = new ClusterRule().withCluster(clusterOfSize(3));

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(org.neo4j.test.rule.SuppressOutput.suppressAll()).around(cleanupRule).around(clusterRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "lockManager = {0}") public static Iterable<Object[]> lockManagerNames()
		 public static IEnumerable<object[]> LockManagerNames()
		 {
			  return Arrays.asList( new object[]{ "forseti" }, new object[]{ "community" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateSingleInstanceRestTransactionThatWaitsForLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateSingleInstanceRestTransactionThatWaitsForLock()
		 {
			  ServerControls server = _cleanupRule.add( TestServerBuilders.newInProcessBuilder().withConfig(GraphDatabaseSettings.auth_enabled, Settings.FALSE).withConfig(GraphDatabaseSettings.lock_manager, LockManagerName).withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newServer() );

			  GraphDatabaseService db = server.Graph();
			  HTTP.Builder http = withBaseUri( server.HttpURI() );

			  long value1 = 1L;
			  long value2 = 2L;

			  CreateNode( db );

			  HTTP.Response tx1 = StartTx( http );
			  HTTP.Response tx2 = StartTx( http );

			  AssertNumberOfActiveTransactions( 2, db );

			  HTTP.Response update1 = ExecuteUpdateStatement( tx1, value1, http );
			  assertThat( update1.Status(), equalTo(200) );
			  assertThat( update1, containsNoErrors() );

			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> tx2Result = executeInSeparateThread("tx2", () ->
			  Future<object> tx2Result = ExecuteInSeparateThread("tx2", () =>
			  {
				latch.Signal();
				Response update2 = ExecuteUpdateStatement( tx2, value2, http );
				AssertTxWasTerminated( update2 );
			  });

			  Await( latch );
			  SleepForAWhile();

			  Terminate( tx2, http );
			  Commit( tx1, http );

			  HTTP.Response update3 = ExecuteUpdateStatement( tx2, value2, http );
			  assertThat( update3.Status(), equalTo(404) );

			  tx2Result.get( 1, TimeUnit.MINUTES );

			  AssertNodeExists( db, value1 );
		 }

		 /// <summary>
		 /// HA has been deprecated.
		 /// @Test
		 /// public void terminateSlaveTransactionThatWaitsForLockOnMaster() throws Exception
		 /// {
		 ///    ClusterManager.ManagedCluster cluster = startCluster();
		 /// 
		 ///    String masterValue = "master";
		 ///    String slaveValue = "slave";
		 /// 
		 ///    HighlyAvailableGraphDatabase master = cluster.getMaster();
		 ///    HighlyAvailableGraphDatabase slave = cluster.getAnySlave();
		 /// 
		 ///    createNode( cluster );
		 /// 
		 ///    CountDownLatch masterTxStarted = new CountDownLatch( 1 );
		 ///    CountDownLatch masterTxCommit = new CountDownLatch( 1 );
		 ///    Future<?> masterTx = setPropertyInSeparateThreadAndWaitBeforeCommit( "masterTx", master, masterValue,
		 ///            masterTxStarted, masterTxCommit );
		 /// 
		 ///    await( masterTxStarted );
		 /// 
		 ///    AtomicReference<Transaction> slaveTxReference = new AtomicReference<>();
		 ///    CountDownLatch slaveTxStarted = new CountDownLatch( 1 );
		 ///    Future<?> slaveTx = setPropertyInSeparateThreadAndAttemptToCommit( "slaveTx", slave, slaveValue, slaveTxStarted,
		 ///            slaveTxReference );
		 /// 
		 ///    slaveTxStarted.await();
		 ///    sleepForAWhile();
		 /// 
		 ///    terminate( slaveTxReference );
		 ///    assertTxWasTerminated( slaveTx );
		 /// 
		 ///    masterTxCommit.countDown();
		 ///    assertNull( masterTx.get() );
		 ///    assertNodeExists( cluster, masterValue );
		 /// }
		 /// 
		 /// 
		 /// @Test
		 /// public void terminateMasterTransactionThatWaitsForLockAcquiredBySlave() throws Exception
		 /// {
		 ///    ClusterManager.ManagedCluster cluster = startCluster();
		 /// 
		 ///    String masterValue = "master";
		 ///    String slaveValue = "slave";
		 /// 
		 ///    HighlyAvailableGraphDatabase master = cluster.getMaster();
		 ///    HighlyAvailableGraphDatabase slave = cluster.getAnySlave();
		 /// 
		 ///    createNode( cluster );
		 /// 
		 ///    CountDownLatch slaveTxStarted = new CountDownLatch( 1 );
		 ///    CountDownLatch slaveTxCommit = new CountDownLatch( 1 );
		 ///    Future<?> slaveTx = setPropertyInSeparateThreadAndWaitBeforeCommit( "slaveTx", slave, slaveValue,
		 ///            slaveTxStarted, slaveTxCommit );
		 /// 
		 ///    await( slaveTxStarted );
		 /// 
		 ///    AtomicReference<Transaction> masterTxReference = new AtomicReference<>();
		 ///    CountDownLatch masterTxStarted = new CountDownLatch( 1 );
		 ///    Future<?> masterTx = setPropertyInSeparateThreadAndAttemptToCommit( "masterTx", master, masterValue,
		 ///            masterTxStarted, masterTxReference );
		 /// 
		 ///    masterTxStarted.await();
		 ///    sleepForAWhile();
		 /// 
		 ///    terminate( masterTxReference );
		 ///    assertTxWasTerminated( masterTx );
		 /// 
		 ///    slaveTxCommit.countDown();
		 ///    assertNull( slaveTx.get() );
		 ///    assertNodeExists( cluster, slaveValue );
		 /// }
		 /// </summary>

		 private static void CreateNode( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( _label );
					tx.Success();
			  }
		 }

		 private void CreateNode( ClusterManager.ManagedCluster cluster )
		 {
			  CreateNode( cluster.Master );
			  cluster.Sync();
		 }

		 private static void AssertNodeExists( GraphDatabaseService db, object value )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = FindNode( db );
					assertTrue( node.HasProperty( PROPERTY ) );
					assertEquals( value, node.GetProperty( PROPERTY ) );
					tx.Success();
			  }
		 }

		 private static void AssertNodeExists( ClusterManager.ManagedCluster cluster, object value )
		 {
			  cluster.Sync();
			  AssertNodeExists( cluster.Master, value );
		 }

		 private static Node FindNode( GraphDatabaseService db )
		 {
			  return single( Db.findNodes( _label ) );
		 }

		 private static HTTP.Response StartTx( HTTP.Builder http )
		 {
			  HTTP.Response tx = http.Post( "db/data/transaction" );
			  assertThat( tx.Status(), equalTo(201) );
			  assertThat( tx, containsNoErrors() );
			  return tx;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void commit(org.neo4j.test.server.HTTP.Response tx, org.neo4j.test.server.HTTP.Builder http) throws org.neo4j.server.rest.domain.JsonParseException
		 private static void Commit( HTTP.Response tx, HTTP.Builder http )
		 {
			  http.Post( tx.StringFromContent( "commit" ) );
		 }

		 private static void Terminate( HTTP.Response tx, HTTP.Builder http )
		 {
			  http.Delete( tx.Location() );
		 }

		 private void Terminate( AtomicReference<Transaction> txReference )
		 {
			  Transaction tx = txReference.get();
			  assertNotNull( tx );
			  tx.Terminate();
		 }

		 private static HTTP.Response ExecuteUpdateStatement( HTTP.Response tx, long value, HTTP.Builder http )
		 {
			  string updateQuery = "MATCH (n:" + _label + ") SET n." + PROPERTY + "=" + value;
			  HTTP.RawPayload json = quotedJson( "{'statements': [{'statement':'" + updateQuery + "'}]}" );
			  return http.Post( tx.Location(), json );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertNumberOfActiveTransactions(int expectedCount, org.neo4j.graphdb.GraphDatabaseService db) throws InterruptedException
		 private static void AssertNumberOfActiveTransactions( int expectedCount, GraphDatabaseService db )
		 {
			  ThrowingSupplier<int, Exception> txCount = () => ActiveTxCount(db);
			  assertEventually( "Wrong active tx count", txCount, equalTo( expectedCount ), 1, TimeUnit.MINUTES );
		 }

		 private static int ActiveTxCount( GraphDatabaseService db )
		 {
			  DependencyResolver resolver = ( ( GraphDatabaseAPI ) db ).DependencyResolver;
			  KernelTransactions kernelTransactions = resolver.ResolveDependency( typeof( KernelTransactions ) );
			  return kernelTransactions.ActiveTransactions().Count;
		 }

		 private static void AssertTxWasTerminated( HTTP.Response txResponse )
		 {
			  assertEquals( 200, txResponse.Status() );
			  assertThat( txResponse, hasErrors( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed ) );
			  assertThat( txResponse.RawContent(), containsString(typeof(LockClientStoppedException).Name) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertTxWasTerminated(java.util.concurrent.Future<?> txFuture) throws InterruptedException
		 private void AssertTxWasTerminated<T1>( Future<T1> txFuture )
		 {
			  try
			  {
					txFuture.get();
					fail( "Exception expected" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void sleepForAWhile() throws InterruptedException
		 private static void SleepForAWhile()
		 {
			  Thread.Sleep( 2_000 );
		 }

		 private static void Await( System.Threading.CountdownEvent latch )
		 {
			  try
			  {
					assertTrue( latch.await( 2, TimeUnit.MINUTES ) );
			  }
			  catch ( InterruptedException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.concurrent.Future<?> setPropertyInSeparateThreadAndWaitBeforeCommit(String threadName, org.neo4j.graphdb.GraphDatabaseService db, Object value, java.util.concurrent.CountDownLatch txStarted, java.util.concurrent.CountDownLatch txCommit)
		 private static Future<object> SetPropertyInSeparateThreadAndWaitBeforeCommit( string threadName, GraphDatabaseService db, object value, System.Threading.CountdownEvent txStarted, System.Threading.CountdownEvent txCommit )
		 {
			  return ExecuteInSeparateThread(threadName, () =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Node node = FindNode( db );
					 node.setProperty( PROPERTY, value );
					 txStarted.Signal();
					 Await( txCommit );
					 tx.success();
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.concurrent.Future<?> setPropertyInSeparateThreadAndAttemptToCommit(String threadName, org.neo4j.graphdb.GraphDatabaseService db, Object value, java.util.concurrent.CountDownLatch txStarted, java.util.concurrent.atomic.AtomicReference<org.neo4j.graphdb.Transaction> txReference)
		 private static Future<object> SetPropertyInSeparateThreadAndAttemptToCommit( string threadName, GraphDatabaseService db, object value, System.Threading.CountdownEvent txStarted, AtomicReference<Transaction> txReference )
		 {
			  return ExecuteInSeparateThread(threadName, () =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 txReference.set( tx );
					 Node node = FindNode( db );
					 txStarted.Signal();
					 node.setProperty( PROPERTY, value );
					 tx.success();
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.concurrent.Future<?> executeInSeparateThread(String threadName, Runnable runnable)
		 private static Future<object> ExecuteInSeparateThread( string threadName, ThreadStart runnable )
		 {
			  return Executors.newSingleThreadExecutor( named( threadName ) ).submit( runnable );
		 }

		 private ClusterManager.ManagedCluster StartCluster()
		 {
			  _clusterRule.withSharedSetting( GraphDatabaseSettings.lock_manager, LockManagerName );

			  ClusterManager.ManagedCluster cluster = _clusterRule.startCluster();
			  cluster.Await( ClusterManager.allSeesAllAsAvailable() );
			  return cluster;
		 }
	}

}