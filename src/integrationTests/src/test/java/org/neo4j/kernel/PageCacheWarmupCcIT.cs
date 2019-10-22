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
namespace Neo4Net.Kernel
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using LeaderOnlyStrategy = Neo4Net.causalclustering.upstream.strategies.LeaderOnlyStrategy;
	using UdcSettings = Neo4Net.Ext.Udc.UdcSettings;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using PageCacheWarmerMonitorAdapter = Neo4Net.Kernel.impl.pagecache.monitor.PageCacheWarmerMonitorAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class PageCacheWarmupCcIT : PageCacheWarmupTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.causalclustering.ClusterRule clusterRule = new org.Neo4Net.test.causalclustering.ClusterRule().withNumberOfReadReplicas(0).withSharedCoreParam(org.Neo4Net.ext.udc.UdcSettings.udc_enabled, org.Neo4Net.kernel.configuration.Settings.FALSE).withSharedCoreParam(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.pagecache_warmup_profiling_interval, "100ms").withSharedCoreParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.multi_dc_license, org.Neo4Net.kernel.configuration.Settings.TRUE).withSharedCoreParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.upstream_selection_strategy, org.Neo4Net.causalclustering.upstream.strategies.LeaderOnlyStrategy.IDENTITY).withInstanceCoreParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.refuse_to_be_leader, id -> id == 0 ? "false" : "true").withSharedReadReplicaParam(org.Neo4Net.ext.udc.UdcSettings.udc_enabled, org.Neo4Net.kernel.configuration.Settings.FALSE).withSharedReadReplicaParam(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.pagecache_warmup_profiling_interval, "100ms").withSharedReadReplicaParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.multi_dc_license, org.Neo4Net.kernel.configuration.Settings.TRUE).withSharedReadReplicaParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.pull_interval, "100ms").withSharedReadReplicaParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.upstream_selection_strategy, org.Neo4Net.causalclustering.upstream.strategies.LeaderOnlyStrategy.IDENTITY);
		 public ClusterRule ClusterRule = new ClusterRule().withNumberOfReadReplicas(0).withSharedCoreParam(UdcSettings.udc_enabled, Settings.FALSE).withSharedCoreParam(GraphDatabaseSettings.pagecache_warmup_profiling_interval, "100ms").withSharedCoreParam(CausalClusteringSettings.multi_dc_license, Settings.TRUE).withSharedCoreParam(CausalClusteringSettings.upstream_selection_strategy, LeaderOnlyStrategy.IDENTITY).withInstanceCoreParam(CausalClusteringSettings.refuse_to_be_leader, id => id == 0 ? "false" : "true").withSharedReadReplicaParam(UdcSettings.udc_enabled, Settings.FALSE).withSharedReadReplicaParam(GraphDatabaseSettings.pagecache_warmup_profiling_interval, "100ms").withSharedReadReplicaParam(CausalClusteringSettings.multi_dc_license, Settings.TRUE).withSharedReadReplicaParam(CausalClusteringSettings.pull_interval, "100ms").withSharedReadReplicaParam(CausalClusteringSettings.upstream_selection_strategy, LeaderOnlyStrategy.IDENTITY);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private CoreClusterMember _leader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long warmUpCluster() throws Exception
		 private long WarmUpCluster()
		 {
			  _leader = _cluster.awaitLeader(); // Make sure we have a cluster leader.
			  _cluster.coreTx((db, tx) =>
			  {
				// Create some test data to touch a bunch of pages.
				CreateTestData( db );
				tx.success();
			  });
			  AtomicLong pagesInMemory = new AtomicLong();
			  _cluster.coreTx((db, tx) =>
			  {
				// Wait for an initial profile on the leader. This profile might have raced with the 'createTestData'
				// transaction above, so it might be incomplete.
				WaitForCacheProfile( _leader.monitors() );
				// Now we can wait for a clean profile on the leader, and note the count for verifying later.
				pagesInMemory.set( WaitForCacheProfile( _leader.monitors() ) );
			  });
			  foreach ( CoreClusterMember member in _cluster.coreMembers() )
			  {
					WaitForCacheProfile( member.Monitors() );
			  }
			  return pagesInMemory.get();
		 }

		 private static void VerifyWarmupHappensAfterStoreCopy( ClusterMember member, long pagesInMemory )
		 {
			  AtomicLong pagesLoadedInWarmup = new AtomicLong();
			  BinaryLatch warmupLatch = InjectWarmupLatch( member, pagesLoadedInWarmup );
			  member.start();
			  warmupLatch.Await();
			  // Check that we warmup up all right:
			  assertThat( pagesLoadedInWarmup.get(), greaterThanOrEqualTo(pagesInMemory) );
		 }

		 private static BinaryLatch InjectWarmupLatch( ClusterMember member, AtomicLong pagesLoadedInWarmup )
		 {
			  BinaryLatch warmupLatch = new BinaryLatch();
			  Monitors monitors = member.monitors();
			  monitors.AddMonitorListener( new PageCacheWarmerMonitorAdapterAnonymousInnerClass( pagesLoadedInWarmup, warmupLatch ) );
			  return warmupLatch;
		 }

		 private class PageCacheWarmerMonitorAdapterAnonymousInnerClass : PageCacheWarmerMonitorAdapter
		 {
			 private AtomicLong _pagesLoadedInWarmup;
			 private BinaryLatch _warmupLatch;

			 public PageCacheWarmerMonitorAdapterAnonymousInnerClass( AtomicLong pagesLoadedInWarmup, BinaryLatch warmupLatch )
			 {
				 this._pagesLoadedInWarmup = pagesLoadedInWarmup;
				 this._warmupLatch = warmupLatch;
			 }

			 public override void warmupCompleted( long pagesLoaded )
			 {
				  _pagesLoadedInWarmup.set( pagesLoaded );
				  _warmupLatch.release();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cacheProfilesMustBeIncludedInStoreCopyToCore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CacheProfilesMustBeIncludedInStoreCopyToCore()
		 {
			  long pagesInMemory = WarmUpCluster();
			  CoreClusterMember member = _cluster.newCoreMember();
			  VerifyWarmupHappensAfterStoreCopy( member, pagesInMemory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cacheProfilesMustBeIncludedInStoreCopyToReadReplica() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CacheProfilesMustBeIncludedInStoreCopyToReadReplica()
		 {
			  long pagesInMemory = WarmUpCluster();
			  ReadReplica member = _cluster.newReadReplica();
			  VerifyWarmupHappensAfterStoreCopy( member, pagesInMemory );
		 }
	}

}