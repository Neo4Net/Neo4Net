using System;
using System.Threading;

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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using FileCopyMonitor = Org.Neo4j.causalclustering.catchup.tx.FileCopyMonitor;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Org.Neo4j.causalclustering.discovery.ReadReplica;
	using ReadReplicaGraphDatabase = Org.Neo4j.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using TransactionFailureException = Org.Neo4j.Graphdb.TransactionFailureException;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogRotation = Org.Neo4j.Kernel.impl.transaction.log.rotation.LogRotation;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class ReadReplicaStoreCopyIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withSharedCoreParam(org.neo4j.graphdb.factory.GraphDatabaseSettings.keep_logical_logs, FALSE).withNumberOfCoreMembers(3).withNumberOfReadReplicas(1);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withSharedCoreParam(GraphDatabaseSettings.keep_logical_logs, FALSE).withNumberOfCoreMembers(3).withNumberOfReadReplicas(1);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 240_000) public void shouldNotBePossibleToStartTransactionsWhenReadReplicaCopiesStore() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBePossibleToStartTransactionsWhenReadReplicaCopiesStore()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  ReadReplica readReplica = cluster.FindAnyReadReplica();

			  readReplica.TxPollingClient().stop();

			  WriteSomeDataAndForceLogRotations( cluster );
			  Semaphore storeCopyBlockingSemaphore = AddStoreCopyBlockingMonitor( readReplica );
			  try
			  {
					readReplica.TxPollingClient().start();
					WaitForStoreCopyToStartAndBlock( storeCopyBlockingSemaphore );

					ReadReplicaGraphDatabase replicaGraphDatabase = readReplica.Database();
					try
					{
						 replicaGraphDatabase.BeginTx();
						 fail( "Exception expected" );
					}
					catch ( Exception e )
					{
						 assertThat( e, instanceOf( typeof( TransactionFailureException ) ) );
						 assertThat( e.Message, containsString( "Database is stopped to copy store" ) );
					}
			  }
			  finally
			  {
					// release all waiters of the semaphore
					storeCopyBlockingSemaphore.release( int.MaxValue );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeSomeDataAndForceLogRotations(org.neo4j.causalclustering.discovery.Cluster<?> cluster) throws Exception
		 private static void WriteSomeDataAndForceLogRotations<T1>( Cluster<T1> cluster )
		 {
			  for ( int i = 0; i < 20; i++ )
			  {
					cluster.CoreTx((db, tx) =>
					{
					 Db.execute( "CREATE ()" );
					 tx.success();
					});

					ForceLogRotationOnAllCores( cluster );
			  }
		 }

		 private static void ForceLogRotationOnAllCores<T1>( Cluster<T1> cluster )
		 {
			  foreach ( CoreClusterMember core in cluster.CoreMembers() )
			  {
					ForceLogRotationAndPruning( core );
			  }
		 }

		 private static void ForceLogRotationAndPruning( CoreClusterMember core )
		 {
			  try
			  {
					DependencyResolver dependencyResolver = core.Database().DependencyResolver;
					dependencyResolver.ResolveDependency( typeof( LogRotation ) ).rotateLogFile();
					SimpleTriggerInfo info = new SimpleTriggerInfo( "test" );
					dependencyResolver.ResolveDependency( typeof( CheckPointer ) ).forceCheckPoint( info );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private static Semaphore AddStoreCopyBlockingMonitor( ReadReplica readReplica )
		 {
			  Semaphore semaphore = new Semaphore( 0 );

			  readReplica.Monitors().addMonitorListener((FileCopyMonitor) file =>
			  {
				try
				{
					 semaphore.acquire();
				}
				catch ( InterruptedException e )
				{
					 Thread.CurrentThread.Interrupt();
					 throw new Exception( e );
				}
			  });

			  return semaphore;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void waitForStoreCopyToStartAndBlock(java.util.concurrent.Semaphore storeCopyBlockingSemaphore) throws Exception
		 private static void WaitForStoreCopyToStartAndBlock( Semaphore storeCopyBlockingSemaphore )
		 {
			  assertEventually( "Read replica did not copy files", storeCopyBlockingSemaphore.hasQueuedThreads, @is( true ), 60, TimeUnit.SECONDS );
		 }
	}

}