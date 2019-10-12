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
namespace Org.Neo4j.causalclustering.core.consensus.schedule
{

	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using OnDemandJobScheduler = Org.Neo4j.Test.OnDemandJobScheduler;
	using FakeClock = Org.Neo4j.Time.FakeClock;

	public class OnDemandTimerService : TimerService
	{
		 private readonly FakeClock _clock;
		 private OnDemandJobScheduler _onDemandJobScheduler;

		 public OnDemandTimerService( FakeClock clock ) : base( new OnDemandJobScheduler(), NullLogProvider.Instance )
		 {
			  this._clock = clock;
			  _onDemandJobScheduler = ( OnDemandJobScheduler ) Scheduler;
		 }

		 public override void Invoke( TimerName name )
		 {
			  ICollection<Timer> timers = GetTimers( name );

			  foreach ( Timer timer in timers )
			  {
					Delay delay = timer.Delay();
					_clock.forward( delay.Amount(), delay.Unit() );
			  }

			  foreach ( Timer timer in timers )
			  {
					timer.Invoke();
			  }

			  _onDemandJobScheduler.runJob();
		 }
	}

}