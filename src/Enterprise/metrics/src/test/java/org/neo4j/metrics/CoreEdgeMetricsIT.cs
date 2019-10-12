using System;

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
namespace Neo4Net.metrics
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using Neo4Net.Function;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using CatchUpMetrics = Neo4Net.metrics.source.causalclustering.CatchUpMetrics;
	using CoreMetrics = Neo4Net.metrics.source.causalclustering.CoreMetrics;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_advertised_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsSettings.csvPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.metricsCsv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.readLongValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.causalclustering.ReadReplicaMetrics.PULL_UPDATES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.causalclustering.ReadReplicaMetrics.PULL_UPDATE_HIGHEST_TX_ID_RECEIVED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.causalclustering.ReadReplicaMetrics.PULL_UPDATE_HIGHEST_TX_ID_REQUESTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class CoreEdgeMetricsIT
	{
		 private const int TIMEOUT = 15;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(1).withSharedCoreParam(MetricsSettings.metricsEnabled, org.neo4j.kernel.configuration.Settings.TRUE).withSharedReadReplicaParam(MetricsSettings.metricsEnabled, org.neo4j.kernel.configuration.Settings.TRUE).withSharedCoreParam(MetricsSettings.csvEnabled, org.neo4j.kernel.configuration.Settings.TRUE).withSharedReadReplicaParam(MetricsSettings.csvEnabled, org.neo4j.kernel.configuration.Settings.TRUE).withSharedCoreParam(MetricsSettings.csvInterval, "100ms").withSharedReadReplicaParam(MetricsSettings.csvInterval, "100ms");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(1).withSharedCoreParam(MetricsSettings.MetricsEnabled, Settings.TRUE).withSharedReadReplicaParam(MetricsSettings.MetricsEnabled, Settings.TRUE).withSharedCoreParam(MetricsSettings.CsvEnabled, Settings.TRUE).withSharedReadReplicaParam(MetricsSettings.CsvEnabled, Settings.TRUE).withSharedCoreParam(MetricsSettings.CsvInterval, "100ms").withSharedReadReplicaParam(MetricsSettings.CsvInterval, "100ms");

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdown()
		 public virtual void Shutdown()
		 {
			  if ( _cluster != null )
			  {
					_cluster.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMonitorCoreEdge() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMonitorCoreEdge()
		 {
			  // given
			  _cluster = ClusterRule.startCluster();

			  // when
			  CoreClusterMember coreMember = _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // then
			  foreach ( CoreClusterMember db in _cluster.coreMembers() )
			  {
					AssertAllNodesVisible( Db.database() );
			  }

			  foreach ( ReadReplica db in _cluster.readReplicas() )
			  {
					AssertAllNodesVisible( Db.database() );
			  }

			  File coreMetricsDir = new File( coreMember.HomeDir(), csvPath.DefaultValue );

			  assertEventually( "append index eventually accurate", () => readLongValue(metricsCsv(coreMetricsDir, CoreMetrics.APPEND_INDEX)), greaterThan(0L), TIMEOUT, TimeUnit.SECONDS );

			  assertEventually( "commit index eventually accurate", () => readLongValue(metricsCsv(coreMetricsDir, CoreMetrics.COMMIT_INDEX)), greaterThan(0L), TIMEOUT, TimeUnit.SECONDS );

			  assertEventually( "term eventually accurate", () => readLongValue(metricsCsv(coreMetricsDir, CoreMetrics.TERM)), greaterThanOrEqualTo(0L), TIMEOUT, TimeUnit.SECONDS );

			  assertEventually("tx pull requests received eventually accurate", () =>
			  {
				long total = 0;
				foreach ( File homeDir in _cluster.coreMembers().Select(CoreClusterMember.homeDir).ToList() )
				{
					 File metricsDir = new File( homeDir, "metrics" );
					 total += readLongValue( metricsCsv( metricsDir, CatchUpMetrics.TX_PULL_REQUESTS_RECEIVED ) );
				}
				return total;
			  }, greaterThan( 0L ), TIMEOUT, TimeUnit.SECONDS);

			  assertEventually( "tx retries eventually accurate", () => readLongValue(metricsCsv(coreMetricsDir, CoreMetrics.TX_RETRIES)), equalTo(0L), TIMEOUT, TimeUnit.SECONDS );

			  assertEventually( "is leader eventually accurate", () => readLongValue(metricsCsv(coreMetricsDir, CoreMetrics.IS_LEADER)), greaterThanOrEqualTo(0L), TIMEOUT, TimeUnit.SECONDS );

			  File readReplicaMetricsDir = new File( _cluster.getReadReplicaById( 0 ).homeDir(), "metrics" );

			  assertEventually( "pull update request registered", () => readLongValue(metricsCsv(readReplicaMetricsDir, PULL_UPDATES)), greaterThan(0L), TIMEOUT, TimeUnit.SECONDS );

			  assertEventually( "pull update request registered", () => readLongValue(metricsCsv(readReplicaMetricsDir, PULL_UPDATE_HIGHEST_TX_ID_REQUESTED)), greaterThan(0L), TIMEOUT, TimeUnit.SECONDS );

			  assertEventually( "pull update response received", () => readLongValue(metricsCsv(readReplicaMetricsDir, PULL_UPDATE_HIGHEST_TX_ID_RECEIVED)), greaterThan(0L), TIMEOUT, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAllNodesVisible(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws Exception
		 private void AssertAllNodesVisible( GraphDatabaseAPI db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					ThrowingSupplier<long, Exception> nodeCount = () => count(Db.AllNodes);

					Config config = Db.DependencyResolver.resolveDependency( typeof( Config ) );

					assertEventually( "node to appear on core server " + config.Get( raft_advertised_address ), nodeCount, greaterThan( 0L ), TIMEOUT, SECONDS );

					foreach ( Node node in Db.AllNodes )
					{
						 assertEquals( "baz_bat", node.GetProperty( "foobar" ) );
					}

					tx.Success();
			  }
		 }
	}

}