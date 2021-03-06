﻿using System.Collections.Generic;

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

	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	/// <summary>
	/// A timer service allowing the creation of timers which can be set to expire
	/// at a future point in time.
	/// </summary>
	public class TimerService
	{
		 protected internal readonly JobScheduler Scheduler;
		 private readonly ICollection<Timer> _timers = new List<Timer>();
		 private readonly Log _log;

		 public TimerService( JobScheduler scheduler, LogProvider logProvider )
		 {
			  this.Scheduler = scheduler;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 /// <summary>
		 /// Creates a timer in the deactivated state.
		 /// </summary>
		 /// <param name="name"> The name of the timer. </param>
		 /// <param name="group"> The scheduler group from which timeouts fire. </param>
		 /// <param name="handler"> The handler invoked on a timeout. </param>
		 /// <returns> The deactivated timer. </returns>
		 public virtual Timer Create( TimerName name, Group group, TimeoutHandler handler )
		 {
			 lock ( this )
			 {
				  Timer timer = new Timer( name, Scheduler, _log, group, handler );
				  _timers.Add( timer );
				  return timer;
			 }
		 }

		 /// <summary>
		 /// Gets all timers registered under the specified name.
		 /// </summary>
		 /// <param name="name"> The name of the timer(s). </param>
		 /// <returns> The timers matching the name. </returns>
		 public virtual ICollection<Timer> GetTimers( TimerName name )
		 {
			 lock ( this )
			 {
				  return _timers.Where( timer => timer.name().Equals(name) ).ToList();
			 }
		 }

		 /// <summary>
		 /// Invokes all timers matching the name.
		 /// </summary>
		 /// <param name="name"> The name of the timer(s). </param>
		 public virtual void Invoke( TimerName name )
		 {
			 lock ( this )
			 {
				  GetTimers( name ).forEach( Timer.invoke );
			 }
		 }

		 /// <summary>
		 /// Convenience interface for timer enums.
		 /// </summary>
		 public interface TimerName
		 {
			  string Name();
		 }
	}

}