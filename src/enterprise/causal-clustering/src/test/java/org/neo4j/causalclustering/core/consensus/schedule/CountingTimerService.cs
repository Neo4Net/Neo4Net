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
namespace Neo4Net.causalclustering.core.consensus.schedule
{

	using LogProvider = Neo4Net.Logging.LogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	public class CountingTimerService : TimerService
	{
		 private readonly IDictionary<string, long> _counts = new Dictionary<string, long>();

		 public CountingTimerService( IJobScheduler scheduler, LogProvider logProvider ) : base( scheduler, logProvider )
		 {
		 }

		 public override Timer Create( TimerName name, Group group, TimeoutHandler handler )
		 {
			  TimeoutHandler countingHandler = timer =>
			  {
				long count = _counts.getOrDefault( name.Name(), 0L );
				_counts[name.Name()] = count + 1;
				handler( timer );
			  };
			  return base.Create( name, group, countingHandler );
		 }

		 public virtual long InvocationCount( TimerName name )
		 {
			  return _counts.getOrDefault( name.Name(), 0L );
		 }
	}

}