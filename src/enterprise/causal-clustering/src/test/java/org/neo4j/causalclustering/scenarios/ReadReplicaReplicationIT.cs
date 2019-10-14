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
namespace Neo4Net.causalclustering.scenarios
{
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CatchupPollingProcess = Neo4Net.causalclustering.catchup.tx.CatchupPollingProcess;
	using FileCopyMonitor = Neo4Net.causalclustering.catchup.tx.FileCopyMonitor;
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using FileNames = Neo4Net.causalclustering.core.consensus.log.segmented.FileNames;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using Neo4Net.Functions;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using WriteOperationsNotAllowedException = Neo4Net.Graphdb.security.WriteOperationsNotAllowedException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheCounters = Neo4Net.Io.pagecache.monitoring.PageCacheCounters;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using TransactionIdTracker = Neo4Net.Kernel.api.txtracking.TransactionIdTracker;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using UnsatisfiedDependencyException = Neo4Net.Kernel.impl.util.UnsatisfiedDependencyException;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.scenarios.SampleData.createData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.awaitEx;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class ReadReplicaReplicationIT
	{
		 private const int NR_CORE_MEMBERS = 3;
		 private const int NR_READ_REPLICAS = 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(NR_CORE_MEMBERS).withNumberOfReadReplicas(NR_READ_REPLICAS).withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.cluster_topology_refresh, "5s").withDiscoveryServiceType(EnterpriseDiscoveryServiceType.HAZELCAST);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(NR_CORE_MEMBERS).withNumberOfReadReplicas(NR_READ_REPLICAS).withSharedCoreParam(CausalClusteringSettings.cluster_topology_refresh, "5s").withDiscoveryServiceType(EnterpriseDiscoveryServiceType.Hazelcast);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToWriteToReadReplica() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToWriteToReadReplica()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  ReadReplicaGraphDatabase readReplica = cluster.FindAnyReadReplica().database();

			  // when
			  try
			  {
					  using ( Transaction tx = readReplica.BeginTx() )
					  {
						Node node = readReplica.CreateNode();
						node.SetProperty( "foobar", "baz_bat" );
						node.AddLabel( Label.label( "Foo" ) );
						tx.Success();
						fail( "should have thrown" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException )
			  {
					// then all good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allServersBecomeAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllServersBecomeAvailable()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  // then
			  foreach ( ReadReplica readReplica in cluster.ReadReplicas() )
			  {
					ThrowingSupplier<bool, Exception> availability = () => readReplica.database().isAvailable(0);
					assertEventually( "read replica becomes available", availability, @is( true ), 10, SECONDS );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEventuallyPullTransactionDownToAllReadReplicas() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEventuallyPullTransactionDownToAllReadReplicas()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withNumberOfReadReplicas(0).startCluster();
			  Cluster<object> cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();
			  int nodesBeforeReadReplicaStarts = 1;

			  cluster.CoreTx((db, tx) =>
			  {
				Db.schema().constraintFor(Label.label("Foo")).assertPropertyIsUnique("foobar").create();
				tx.success();
			  });

			  // when
			  for ( int i = 0; i < 100; i++ )
			  {
					cluster.CoreTx((db, tx) =>
					{
					 createData( db, nodesBeforeReadReplicaStarts );
					 tx.success();
					});
			  }

			  ISet<Path> labelScanStoreFiles = new HashSet<Path>();
			  cluster.CoreTx( ( db, tx ) => gatherLabelScanStoreFiles( db, labelScanStoreFiles ) );

			  AtomicBoolean labelScanStoreCorrectlyPlaced = new AtomicBoolean( false );
			  Monitors monitors = new Monitors();
			  ReadReplica rr = cluster.AddReadReplicaWithIdAndMonitors( 0, monitors );
			  monitors.AddMonitorListener((FileCopyMonitor) file =>
			  {
				if ( labelScanStoreFiles.Contains( file.toPath().FileName ) )
				{
					 labelScanStoreCorrectlyPlaced.set( true );
				}
			  });

			  rr.Start();

			  for ( int i = 0; i < 100; i++ )
			  {
					cluster.CoreTx((db, tx) =>
					{
					 createData( db, nodesBeforeReadReplicaStarts );
					 tx.success();
					});
			  }

			  // then
			  foreach ( ReadReplica server in cluster.ReadReplicas() )
			  {
					GraphDatabaseService readReplica = server.database();
					using ( Transaction tx = readReplica.BeginTx() )
					{
						 ThrowingSupplier<long, Exception> nodeCount = () => count(readReplica.AllNodes);
						 assertEventually( "node to appear on read replica", nodeCount, @is( 400L ), 1, MINUTES );

						 foreach ( Node node in readReplica.AllNodes )
						 {
							  assertThat( node.GetProperty( "foobar" ).ToString(), startsWith("baz_bat") );
						 }

						 tx.Success();
					}
			  }

			  assertTrue( labelScanStoreCorrectlyPlaced.get() );
		 }

		 private static void GatherLabelScanStoreFiles( GraphDatabaseAPI db, ISet<Path> labelScanStoreFiles )
		 {
			  Path databaseDirectory = Db.databaseLayout().databaseDirectory().toPath();
			  LabelScanStore labelScanStore = Db.DependencyResolver.resolveDependency( typeof( LabelScanStore ) );
			  using ( ResourceIterator<File> files = labelScanStore.SnapshotStoreFiles() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Path relativePath = databaseDirectory.relativize( Files.next().toPath().toAbsolutePath() );
					labelScanStoreFiles.Add( relativePath );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShutdownRatherThanPullUpdatesFromCoreMemberWithDifferentStoreIdIfLocalStoreIsNonEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShutdownRatherThanPullUpdatesFromCoreMemberWithDifferentStoreIdIfLocalStoreIsNonEmpty()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withNumberOfReadReplicas(0).startCluster();
			  Cluster<object> cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();

			  cluster.CoreTx( _createSomeData );

			  cluster.AwaitCoreMemberWithRole( Role.FOLLOWER, 2, TimeUnit.SECONDS );

			  // Get a read replica and make sure that it is operational
			  ReadReplica readReplica = cluster.AddReadReplicaWithId( 4 );
			  readReplica.Start();
			  readReplica.Database().beginTx().close();

			  // Change the store id, so it should fail to join the cluster again
			  ChangeStoreId( readReplica );
			  readReplica.Shutdown();

			  try
			  {
					readReplica.Start();
					fail( "Should have failed to start" );
			  }
			  catch ( Exception required )
			  {
					// Lifecycle should throw exception, server should not start.
					assertThat( required.InnerException, instanceOf( typeof( LifecycleException ) ) );
					assertThat( required.InnerException.InnerException, instanceOf( typeof( Exception ) ) );
					assertThat( required.InnerException.InnerException.Message, containsString( "This read replica cannot join the cluster. " + "The local database is not empty and has a mismatching storeId:" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aReadReplicShouldBeAbleToRejoinTheCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AReadReplicShouldBeAbleToRejoinTheCluster()
		 {
			  int readReplicaId = 4;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withNumberOfReadReplicas(0).startCluster();
			  Cluster<object> cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();

			  cluster.CoreTx( _createSomeData );

			  cluster.AddReadReplicaWithId( readReplicaId ).start();

			  // let's spend some time by adding more data
			  cluster.CoreTx( _createSomeData );

			  awaitEx( () => ReadReplicasUpToDateAsTheLeader(cluster.AwaitLeader(), cluster.ReadReplicas()), 1, TimeUnit.MINUTES );
			  cluster.RemoveReadReplicaWithMemberId( readReplicaId );

			  // let's spend some time by adding more data
			  cluster.CoreTx( _createSomeData );

			  cluster.AddReadReplicaWithId( readReplicaId ).start();

			  awaitEx( () => ReadReplicasUpToDateAsTheLeader(cluster.AwaitLeader(), cluster.ReadReplicas()), 1, TimeUnit.MINUTES );

			  System.Func<ClusterMember, DbRepresentation> toRep = db => DbRepresentation.of( Db.database() );
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<DbRepresentation> dbs = cluster.CoreMembers().Select(toRep).collect(toSet());
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  dbs.addAll( cluster.ReadReplicas().Select(toRep).collect(toSet()) );

			  cluster.Shutdown();

			  assertEquals( 1, dbs.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readReplicasShouldRestartIfTheWholeClusterIsRestarted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadReplicasShouldRestartIfTheWholeClusterIsRestarted()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  // when
			  cluster.Shutdown();
			  cluster.Start();

			  // then
			  foreach ( ReadReplica readReplica in cluster.ReadReplicas() )
			  {
					ThrowingSupplier<bool, Exception> availability = () => readReplica.database().isAvailable(0);
					assertEventually( "read replica becomes available", availability, @is( true ), 10, SECONDS );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDownloadANewStoreAfterPruning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDownloadANewStoreAfterPruning()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap( GraphDatabaseSettings.keep_logical_logs.name(), "keep_none", GraphDatabaseSettings.logical_log_rotation_threshold.name(), "1M", GraphDatabaseSettings.check_point_interval_time.name(), "100ms" );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withSharedCoreParams(params).startCluster();
			  Cluster<object> cluster = ClusterRule.withSharedCoreParams( @params ).startCluster();

			  cluster.CoreTx((db, tx) =>
			  {
				createData( db, 10 );
				tx.success();
			  });

			  awaitEx( () => ReadReplicasUpToDateAsTheLeader(cluster.AwaitLeader(), cluster.ReadReplicas()), 1, TimeUnit.MINUTES );

			  ReadReplica readReplica = cluster.GetReadReplicaById( 0 );
			  long highestReadReplicaLogVersion = PhysicalLogFiles( readReplica ).HighestLogVersion;

			  // when
			  readReplica.Shutdown();

			  CoreClusterMember core;
			  do
			  {
					core = cluster.CoreTx((db, tx) =>
					{
					 createData( db, 1_000 );
					 tx.success();
					});

			  } while ( PhysicalLogFiles( core ).LowestLogVersion <= highestReadReplicaLogVersion );

			  readReplica.Start();

			  // then
			  awaitEx( () => ReadReplicasUpToDateAsTheLeader(cluster.AwaitLeader(), cluster.ReadReplicas()), 1, TimeUnit.MINUTES );

			  assertEventually( "The read replica has the same data as the core members", () => DbRepresentation.of(readReplica.Database()), equalTo(DbRepresentation.of(cluster.AwaitLeader().database())), 10, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToPullTxAfterHavingDownloadedANewStoreAfterPruning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToPullTxAfterHavingDownloadedANewStoreAfterPruning()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap( GraphDatabaseSettings.keep_logical_logs.name(), "keep_none", GraphDatabaseSettings.logical_log_rotation_threshold.name(), "1M", GraphDatabaseSettings.check_point_interval_time.name(), "100ms" );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withSharedCoreParams(params).startCluster();
			  Cluster<object> cluster = ClusterRule.withSharedCoreParams( @params ).startCluster();

			  cluster.CoreTx((db, tx) =>
			  {
				createData( db, 10 );
				tx.success();
			  });

			  awaitEx( () => ReadReplicasUpToDateAsTheLeader(cluster.AwaitLeader(), cluster.ReadReplicas()), 1, TimeUnit.MINUTES );

			  ReadReplica readReplica = cluster.GetReadReplicaById( 0 );
			  long highestReadReplicaLogVersion = PhysicalLogFiles( readReplica ).HighestLogVersion;

			  readReplica.Shutdown();

			  CoreClusterMember core;
			  do
			  {
					core = cluster.CoreTx((db, tx) =>
					{
					 createData( db, 1_000 );
					 tx.success();
					});

			  } while ( PhysicalLogFiles( core ).LowestLogVersion <= highestReadReplicaLogVersion );

			  readReplica.Start();

			  awaitEx( () => ReadReplicasUpToDateAsTheLeader(cluster.AwaitLeader(), cluster.ReadReplicas()), 1, TimeUnit.MINUTES );

			  // when
			  cluster.CoreTx((db, tx) =>
			  {
				createData( db, 10 );
				tx.success();
			  });

			  // then
			  assertEventually( "The read replica has the same data as the core members", () => DbRepresentation.of(readReplica.Database()), equalTo(DbRepresentation.of(cluster.AwaitLeader().database())), 10, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionsShouldNotAppearOnTheReadReplicaWhilePollingIsPaused() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionsShouldNotAppearOnTheReadReplicaWhilePollingIsPaused()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

			  ReadReplicaGraphDatabase readReplicaGraphDatabase = cluster.FindAnyReadReplica().database();
			  CatchupPollingProcess pollingClient = readReplicaGraphDatabase.DependencyResolver.resolveDependency( typeof( CatchupPollingProcess ) );
			  pollingClient.Stop();

			  cluster.CoreTx((coreGraphDatabase, transaction) =>
			  {
				coreGraphDatabase.createNode();
				transaction.success();
			  });

			  CoreGraphDatabase leaderDatabase = cluster.AwaitLeader().database();
			  long transactionVisibleOnLeader = TransactionIdTracker( leaderDatabase ).newestEncounteredTxId();

			  // when the poller is paused, transaction doesn't make it to the read replica
			  try
			  {
					TransactionIdTracker( readReplicaGraphDatabase ).awaitUpToDate( transactionVisibleOnLeader, ofSeconds( 15 ) );
					fail( "should have thrown exception" );
			  }
			  catch ( TransactionFailureException )
			  {
					// expected timeout
			  }

			  // when the poller is resumed, it does make it to the read replica
			  pollingClient.Start();
			  TransactionIdTracker( readReplicaGraphDatabase ).awaitUpToDate( transactionVisibleOnLeader, ofSeconds( 15 ) );
		 }

		 private static TransactionIdTracker TransactionIdTracker( GraphDatabaseAPI database )
		 {
			  System.Func<TransactionIdStore> transactionIdStore = database.DependencyResolver.provideDependency( typeof( TransactionIdStore ) );
			  AvailabilityGuard databaseAvailabilityGuard = database.DependencyResolver.resolveDependency( typeof( DatabaseAvailabilityGuard ) );
			  return new TransactionIdTracker( transactionIdStore, databaseAvailabilityGuard );
		 }

		 private static LogFiles PhysicalLogFiles( ClusterMember clusterMember )
		 {
			  return clusterMember.database().DependencyResolver.resolveDependency(typeof(LogFiles));
		 }

		 private static bool ReadReplicasUpToDateAsTheLeader( CoreClusterMember leader, ICollection<ReadReplica> readReplicas )
		 {
			  long leaderTxId = LastClosedTransactionId( true, leader.Database() );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return readReplicas.Select( ReadReplica::database ).Select( db => LastClosedTransactionId( false, db ) ).Aggregate( true, ( acc, txId ) => acc && txId == leaderTxId, bool?.logicalAnd );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void changeStoreId(org.neo4j.causalclustering.discovery.ReadReplica replica) throws java.io.IOException
		 private static void ChangeStoreId( ReadReplica replica )
		 {
			  File neoStoreFile = DatabaseLayout.of( replica.DatabaseDirectory() ).metadataStore();
			  PageCache pageCache = replica.Database().DependencyResolver.resolveDependency(typeof(PageCache));
			  MetaDataStore.setRecord( pageCache, neoStoreFile, TIME, DateTimeHelper.CurrentUnixTimeMillis() );
		 }

		 private static long LastClosedTransactionId( bool fail, GraphDatabaseFacade db )
		 {
			  try
			  {
					return Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;
			  }
			  catch ( Exception ex ) when ( ex is System.InvalidOperationException || ex is UnsatisfiedDependencyException )
			  {
					if ( !fail )
					{
						 // the db is down we'll try again...
						 return -1;
					}
					else
					{
						 throw ex;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionIfReadReplicaRecordFormatDiffersToCoreRecordFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionIfReadReplicaRecordFormatDiffersToCoreRecordFormat()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withNumberOfReadReplicas(0).withRecordFormat(org.neo4j.kernel.impl.store.format.highlimit.HighLimit.NAME).startCluster();
			  Cluster<object> cluster = ClusterRule.withNumberOfReadReplicas( 0 ).withRecordFormat( HighLimit.NAME ).startCluster();

			  // when
			  cluster.CoreTx( _createSomeData );

			  try
			  {
					string format = Standard.LATEST_NAME;
					cluster.AddReadReplicaWithIdAndRecordFormat( 0, format ).start();
					fail( "starting read replica with '" + format + "' format should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.InnerException.InnerException.Message, containsString( "Failed to start database with copied store" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCopyStoresFromCoreToReadReplica() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCopyStoresFromCoreToReadReplica()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap( CausalClusteringSettings.raft_log_rotation_size.name(), "1k", CausalClusteringSettings.raft_log_pruning_frequency.name(), "500ms", CausalClusteringSettings.state_machine_flush_window_size.name(), "1", CausalClusteringSettings.raft_log_pruning_strategy.name(), "1 entries" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.withNumberOfReadReplicas(0).withSharedCoreParams(params).withRecordFormat(org.neo4j.kernel.impl.store.format.highlimit.HighLimit.NAME).startCluster();
			  Cluster<object> cluster = ClusterRule.withNumberOfReadReplicas( 0 ).withSharedCoreParams( @params ).withRecordFormat( HighLimit.NAME ).startCluster();

			  cluster.CoreTx((db, tx) =>
			  {
				Node node = Db.createNode( Label.label( "L" ) );
				for ( int i = 0; i < 10; i++ )
				{
					 node.setProperty( "prop-" + i, "this is a quite long string to get to the log limit soonish" );
				}
				tx.success();
			  });

			  long baseVersion = VersionBy( cluster.AwaitLeader().raftLogDirectory(), Math.max );

			  CoreClusterMember coreGraphDatabase = null;
			  for ( int j = 0; j < 2; j++ )
			  {
					coreGraphDatabase = cluster.CoreTx((db, tx) =>
					{
					 Node node = Db.createNode( Label.label( "L" ) );
					 for ( int i = 0; i < 10; i++ )
					 {
						  node.setProperty( "prop-" + i, "this is a quite long string to get to the log limit soonish" );
					 }
					 tx.success();
					});
			  }

			  File raftLogDir = coreGraphDatabase.RaftLogDirectory();
			  assertEventually( "pruning happened", () => VersionBy(raftLogDir, Math.min), greaterThan(baseVersion), 5, SECONDS );

			  // when
			  cluster.AddReadReplicaWithIdAndRecordFormat( 4, HighLimit.NAME ).start();

			  // then
			  foreach ( ReadReplica readReplica in cluster.ReadReplicas() )
			  {
					assertEventually( "read replica available", () => readReplica.database().isAvailable(0), @is(true), 10, SECONDS );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long versionBy(java.io.File raftLogDir, System.Func<long, long, long> operator) throws java.io.IOException
		 private static long VersionBy( File raftLogDir, System.Func<long, long, long> @operator )
		 {
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					SortedDictionary<long, File> logs = ( new FileNames( raftLogDir ) ).getAllFiles( fileSystem, mock( typeof( Log ) ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return logs.Keys.Aggregate( @operator ).orElseThrow( System.InvalidOperationException::new );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageFaultsFromReplicationMustCountInMetrics() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageFaultsFromReplicationMustCountInMetrics()
		 {
			  // Given initial pin counts on all members
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  System.Func<ReadReplica, PageCacheCounters> getPageCacheCounters = ccm => ccm.database().DependencyResolver.resolveDependency(typeof(PageCacheCounters));
			  IList<PageCacheCounters> countersList = cluster.ReadReplicas().Select(getPageCacheCounters).ToList();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  long[] initialPins = countersList.Select( PageCacheCounters::pins ).ToArray();

			  // when the leader commits a write transaction,
			  cluster.CoreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // then the replication should cause pins on a majority of core members to increase.
			  // However, the commit returns as soon as the transaction has been replicated through the Raft log, which
			  // happens before the transaction is applied on the members, and then replicated to read-replicas.
			  // Therefor we are racing with the transaction application on the read-replicas, so we have to spin.
			  int minimumUpdatedMembersCount = countersList.Count / 2 + 1;
			  assertEventually("Expected followers to eventually increase pin counts", () =>
			  {
				long[] pinsAfterCommit = countersList.Select( PageCacheCounters.pins ).ToArray();
				int membersWithIncreasedPinCount = 0;
				for ( int i = 0; i < initialPins.Length; i++ )
				{
					 long before = initialPins[i];
					 long after = pinsAfterCommit[i];
					 if ( before < after )
					 {
						  membersWithIncreasedPinCount++;
					 }
				}
				return membersWithIncreasedPinCount;
			  }, Matchers.@is( greaterThanOrEqualTo( minimumUpdatedMembersCount ) ), 10, SECONDS);
		 }

		 private readonly System.Action<CoreGraphDatabase, Transaction> _createSomeData = ( db, tx ) =>
		 {
		  createData( db, 10 );
		  tx.success();
		 };
	}

}