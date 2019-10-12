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
namespace Neo4Net.metrics.source.causalclustering
{
	using Reservoir = com.codahale.metrics.Reservoir;
	using Timer = com.codahale.metrics.Timer;


	using RaftMessageProcessingMonitor = Neo4Net.causalclustering.core.consensus.RaftMessageProcessingMonitor;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;

	public class RaftMessageProcessingMetric : RaftMessageProcessingMonitor
	{
		 private readonly AtomicLong _delay = new AtomicLong( 0 );
		 private readonly Timer _timer;
		 private readonly IDictionary<Neo4Net.causalclustering.core.consensus.RaftMessages_Type, Timer> _typeTimers = new Dictionary<Neo4Net.causalclustering.core.consensus.RaftMessages_Type, Timer>( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_Type ) );

		 public static RaftMessageProcessingMetric Create()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return new RaftMessageProcessingMetric( Timer::new );
		 }

		 public static RaftMessageProcessingMetric CreateUsing( System.Func<Reservoir> reservoir )
		 {
			  return new RaftMessageProcessingMetric( () => new Timer(reservoir()) );
		 }

		 private RaftMessageProcessingMetric( System.Func<Timer> timerFactory )
		 {
			  foreach ( Neo4Net.causalclustering.core.consensus.RaftMessages_Type type in Enum.GetValues( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_Type ) ) )
			  {
					_typeTimers[type] = timerFactory();
			  }
			  this._timer = timerFactory();
		 }

		 public virtual long Delay()
		 {
			  return _delay.get();
		 }

		 public virtual Duration Delay
		 {
			 set
			 {
				  this._delay.set( value.toMillis() );
			 }
		 }

		 public virtual Timer Timer()
		 {
			  return _timer;
		 }

		 public virtual Timer Timer( Neo4Net.causalclustering.core.consensus.RaftMessages_Type type )
		 {
			  return _typeTimers[type];
		 }

		 public override void UpdateTimer( Neo4Net.causalclustering.core.consensus.RaftMessages_Type type, Duration duration )
		 {
			  long nanos = duration.toNanos();
			  _timer.update( nanos, TimeUnit.NANOSECONDS );
			  _typeTimers[type].update( nanos, TimeUnit.NANOSECONDS );
		 }
	}

}