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
namespace Neo4Net.metrics.source.causalclustering
{
	using SlidingWindowReservoir = com.codahale.metrics.SlidingWindowReservoir;
	using Test = org.junit.Test;

	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class RaftMessageProcessingMetricTest
	{
		 private RaftMessageProcessingMetric _metric = RaftMessageProcessingMetric.CreateUsing( () => new SlidingWindowReservoir(1000) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefaultAllMessageTypesToEmptyTimer()
		 public virtual void ShouldDefaultAllMessageTypesToEmptyTimer()
		 {
			  foreach ( Neo4Net.causalclustering.core.consensus.RaftMessages_Type type in Enum.GetValues( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_Type ) ) )
			  {
					assertEquals( 0, _metric.timer( type ).Count );
			  }
			  assertEquals( 0, _metric.timer().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUpdateAllMessageTypes()
		 public virtual void ShouldBeAbleToUpdateAllMessageTypes()
		 {
			  // given
			  int durationNanos = 5;
			  double delta = 0.002;

			  // when
			  foreach ( Neo4Net.causalclustering.core.consensus.RaftMessages_Type type in Enum.GetValues( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_Type ) ) )
			  {
					_metric.updateTimer( type, Duration.ofNanos( durationNanos ) );
					assertEquals( 1, _metric.timer( type ).Count );
					assertEquals( durationNanos, _metric.timer( type ).Snapshot.Mean, delta );
			  }

			  // then
			  assertEquals( Enum.GetValues( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_Type ) ).length, _metric.timer().Count );
			  assertEquals( durationNanos, _metric.timer().Snapshot.Mean, delta );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefaultDelayToZero()
		 public virtual void ShouldDefaultDelayToZero()
		 {
			  assertEquals( 0, _metric.delay() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateDelay()
		 public virtual void ShouldUpdateDelay()
		 {
			  _metric.Delay = Duration.ofMillis( 5 );
			  assertEquals( 5, _metric.delay() );
		 }
	}

}