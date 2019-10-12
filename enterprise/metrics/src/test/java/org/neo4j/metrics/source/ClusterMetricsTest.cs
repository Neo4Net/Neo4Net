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
namespace Org.Neo4j.metrics.source
{
	using Counter = com.codahale.metrics.Counter;
	using Gauge = com.codahale.metrics.Gauge;
	using Histogram = com.codahale.metrics.Histogram;
	using Meter = com.codahale.metrics.Meter;
	using MetricFilter = com.codahale.metrics.MetricFilter;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using ScheduledReporter = com.codahale.metrics.ScheduledReporter;
	using Timer = com.codahale.metrics.Timer;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using HighAvailabilityMemberState = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberState;
	using HighAvailabilityMemberStateMachine = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using ClusterMember = Org.Neo4j.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ClusterMembers;
	using ObservedClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ObservedClusterMembers;
	using HighAvailabilityModeSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using ClusterMetrics = Org.Neo4j.metrics.source.cluster.ClusterMetrics;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ClusterMetricsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public ExpectedException Thrown = ExpectedException.none();

		 private readonly MetricRegistry _metricRegistry = new MetricRegistry();
		 private readonly Monitors _monitors = new Monitors();
		 private readonly LifeSupport _life = new LifeSupport();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clusterMetricsReportMasterAvailable()
		 public virtual void ClusterMetricsReportMasterAvailable()
		 {
			  // given
			  System.Func<ClusterMembers> clusterMembers = GetClusterMembers( HighAvailabilityModeSwitcher.MASTER, HighAvailabilityMemberState.MASTER );

			  _life.add( new ClusterMetrics( _monitors, _metricRegistry, clusterMembers ) );
			  _life.start();

			  // when
			  TestReporter reporter = new TestReporter( this, _metricRegistry );
			  reporter.start( 10, TimeUnit.MILLISECONDS );

			  // then wait for the reporter to get a report
			  reporter.report();
			  assertEquals( 1, reporter.IsMasterValue );
			  assertEquals( 1, reporter.IsAvailableValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clusterMetricsReportSlaveAvailable()
		 public virtual void ClusterMetricsReportSlaveAvailable()
		 {
			  // given
			  System.Func<ClusterMembers> clusterMembers = GetClusterMembers( HighAvailabilityModeSwitcher.SLAVE, HighAvailabilityMemberState.SLAVE );

			  _life.add( new ClusterMetrics( _monitors, _metricRegistry, clusterMembers ) );
			  _life.start();

			  // when
			  TestReporter reporter = new TestReporter( this, _metricRegistry );
			  reporter.start( 10, TimeUnit.MILLISECONDS );

			  //then wait for the reporter to get a report
			  reporter.report();
			  assertEquals( 0, reporter.IsMasterValue );
			  assertEquals( 1, reporter.IsAvailableValue );
		 }

		 private static System.Func<ClusterMembers> GetClusterMembers( string memberRole, HighAvailabilityMemberState memberState )
		 {
			  HighAvailabilityMemberStateMachine stateMachine = mock( typeof( HighAvailabilityMemberStateMachine ) );
			  when( stateMachine.CurrentState ).thenReturn( memberState );
			  ClusterMember clusterMember = spy( new ClusterMember( new InstanceId( 1 ) ) );
			  when( clusterMember.HARole ).thenReturn( memberRole );
			  ObservedClusterMembers observedClusterMembers = mock( typeof( ObservedClusterMembers ) );
			  when( observedClusterMembers.CurrentMember ).thenReturn( clusterMember );
			  return () => new ClusterMembers(observedClusterMembers, stateMachine);
		 }

		 private class TestReporter : ScheduledReporter
		 {
			 private readonly ClusterMetricsTest _outerInstance;

			  internal int IsMasterValue;
			  internal int IsAvailableValue;

			  protected internal TestReporter( ClusterMetricsTest outerInstance, MetricRegistry registry ) : base( registry, "TestReporter", MetricFilter.ALL, TimeUnit.MILLISECONDS, TimeUnit.MILLISECONDS )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override void Report( SortedDictionary<string, Gauge> gauges, SortedDictionary<string, Counter> counters, SortedDictionary<string, Histogram> histograms, SortedDictionary<string, Meter> meters, SortedDictionary<string, Timer> timers )
			  {
					IsMasterValue = ( int? ) gauges[ClusterMetrics.IS_MASTER].Value.Value;
					IsAvailableValue = ( int? ) gauges[ClusterMetrics.IS_AVAILABLE].Value.Value;
			  }
		 }

	}

}