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
namespace Neo4Net.metrics
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using CoreMetrics = Neo4Net.metrics.source.causalclustering.CoreMetrics;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsSettings.csvPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsTestHelper.metricsCsv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsTestHelper.readLongValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsTestHelper.readTimerDoubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.metrics.MetricsTestHelper.readTimerLongValueAndAssert;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertEventually;

	public class RaftMessageProcessingMetricIT
	{
		 private const int TIMEOUT = 15;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.causalclustering.ClusterRule clusterRule = new Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(Neo4Net.causalclustering.core.CausalClusteringSettings.leader_election_timeout, "1s").withSharedCoreParam(MetricsSettings.metricsEnabled, Neo4Net.kernel.configuration.Settings.TRUE).withSharedCoreParam(MetricsSettings.csvEnabled, Neo4Net.kernel.configuration.Settings.TRUE).withSharedCoreParam(MetricsSettings.csvInterval, "100ms");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(CausalClusteringSettings.leader_election_timeout, "1s").withSharedCoreParam(MetricsSettings.MetricsEnabled, Settings.TRUE).withSharedCoreParam(MetricsSettings.CsvEnabled, Settings.TRUE).withSharedCoreParam(MetricsSettings.CsvInterval, "100ms");

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Neo4Net.causalclustering.discovery.Cluster<?> cluster;
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
//ORIGINAL LINE: @Test public void shouldMonitorMessageDelay() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMonitorMessageDelay()
		 {
			  // given
			  _cluster = ClusterRule.startCluster();

			  // then
			  CoreClusterMember leader = _cluster.awaitLeader();
			  File coreMetricsDir = new File( leader.HomeDir(), csvPath.DefaultValue );

			  assertEventually( "message delay eventually recorded", () => readLongValue(metricsCsv(coreMetricsDir, CoreMetrics.DELAY)), greaterThanOrEqualTo(0L), TIMEOUT, TimeUnit.SECONDS );

			  assertEventually( "message timer count eventually recorded", () => readTimerLongValueAndAssert(metricsCsv(coreMetricsDir, CoreMetrics.TIMER), (newValue, currentValue) => newValue >= currentValue, MetricsTestHelper.TimerField.Count), greaterThan(0L), TIMEOUT, TimeUnit.SECONDS );

			  assertEventually( "message timer max eventually recorded", () => readTimerDoubleValue(metricsCsv(coreMetricsDir, CoreMetrics.TIMER), MetricsTestHelper.TimerField.Max), greaterThanOrEqualTo(0d), TIMEOUT, TimeUnit.SECONDS );
		 }
	}

}