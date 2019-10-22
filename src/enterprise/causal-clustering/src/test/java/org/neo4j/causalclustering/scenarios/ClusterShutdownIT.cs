using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.scenarios
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ClusterShutdownIT
	public class ClusterShutdownIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.causalclustering.ClusterRule clusterRule = new org.Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public java.util.Collection<int> shutdownOrder;
		 public ICollection<int> ShutdownOrder;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "shutdown order {0}") public static java.util.Collection<java.util.Collection<int>> shutdownOrders()
		 public static ICollection<ICollection<int>> ShutdownOrders()
		 {
			  return asList( asList( 0, 1, 2 ), asList( 1, 2, 0 ), asList( 2, 0, 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartCluster()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdownCluster()
		 public virtual void ShutdownCluster()
		 {
			  if ( _cluster != null )
			  {
					_cluster.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShutdownEvenThoughWaitingForLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShutdownEvenThoughWaitingForLock()
		 {
			  CoreClusterMember leader = _cluster.awaitLeader();
			  ShouldShutdownEvenThoughWaitingForLock0( _cluster, leader.ServerId(), ShutdownOrder );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createANode(java.util.concurrent.atomic.AtomicReference<org.Neo4Net.graphdb.Node> node) throws Exception
		 private void CreateANode( AtomicReference<Node> node )
		 {
			  _cluster.coreTx((coreGraphDatabase, transaction) =>
			  {
				node.set( coreGraphDatabase.createNode() );
				transaction.success();
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldShutdownEvenThoughWaitingForLock0(org.Neo4Net.causalclustering.discovery.Cluster<?> cluster, int victimId, java.util.Collection<int> shutdownOrder) throws Exception
		 private void ShouldShutdownEvenThoughWaitingForLock0<T1>( Cluster<T1> cluster, int victimId, ICollection<int> shutdownOrder )
		 {
			  const int longTime = 60_000;
			  const int numberOfLockAcquirers = 2;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.ExecutorService txExecutor = java.util.concurrent.Executors.newCachedThreadPool();
			  ExecutorService txExecutor = Executors.newCachedThreadPool(); // Blocking transactions are executed in
			  // parallel, not on the main thread.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.ExecutorService shutdownExecutor = java.util.concurrent.Executors.newFixedThreadPool(1);
			  ExecutorService shutdownExecutor = Executors.newFixedThreadPool( 1 ); // Shutdowns are executed
			  // serially, not on the main thread.

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch acquiredLocksCountdown = new java.util.concurrent.CountDownLatch(NUMBER_OF_LOCK_ACQUIRERS);
			  System.Threading.CountdownEvent acquiredLocksCountdown = new System.Threading.CountdownEvent( numberOfLockAcquirers );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch locksHolder = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent locksHolder = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.Neo4Net.graphdb.Node> node = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<Node> node = new AtomicReference<Node>();

			  CompletableFuture<Void> preShutdown = new CompletableFuture<Void>();

			  // set shutdown order
			  CompletableFuture<Void> afterShutdown = preShutdown;
			  foreach ( int? id in shutdownOrder )
			  {
					afterShutdown = afterShutdown.thenRunAsync( () => cluster.GetCoreMemberById(id.Value).shutdown(), shutdownExecutor );
			  }

			  CreateANode( node );

			  try
			  {
					// when - blocking on lock acquiring
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.GraphDatabaseService leader = cluster.getCoreMemberById(victimId).database();
					GraphDatabaseService leader = cluster.GetCoreMemberById( victimId ).database();

					for ( int i = 0; i < numberOfLockAcquirers; i++ )
					{
						 txExecutor.execute(() =>
						 {
						  try
						  {
							  using ( Transaction tx = leader.BeginTx() )
							  {
									acquiredLocksCountdown.Signal();
									tx.acquireWriteLock( node.get() );
									locksHolder.await();
									tx.success();
							  }
						  }
						  catch ( Exception )
						  {
								/* Since we are shutting down, a plethora of possible exceptions are expected. */
						  }
						 });
					}

					// await locks
					if ( !acquiredLocksCountdown.await( longTime, MILLISECONDS ) )
					{
						 throw new System.InvalidOperationException( "Failed to acquire locks" );
					}

					// then shutdown in given order works
					preShutdown.complete( null );
					afterShutdown.get( longTime, MILLISECONDS );
			  }
			  finally
			  {
					afterShutdown.cancel( true );
					locksHolder.Signal();
					txExecutor.shutdownNow();
					shutdownExecutor.shutdownNow();
			  }
		 }
	}

}