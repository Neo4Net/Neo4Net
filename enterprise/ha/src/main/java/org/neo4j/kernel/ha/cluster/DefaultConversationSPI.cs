using System.Threading;

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
namespace Org.Neo4j.Kernel.ha.cluster
{

	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	/// <summary>
	/// Default implementation of <seealso cref="ConversationSPI"/> used on master in HA setup.
	/// </summary>
	public class DefaultConversationSPI : ConversationSPI
	{
		 private readonly Locks _locks;
		 private readonly JobScheduler _jobScheduler;

		 public DefaultConversationSPI( Locks locks, JobScheduler jobScheduler )
		 {
			  this._locks = locks;
			  this._jobScheduler = jobScheduler;
		 }

		 public override Org.Neo4j.Kernel.impl.locking.Locks_Client AcquireClient()
		 {
			  return _locks.newClient();
		 }

		 public override JobHandle ScheduleRecurringJob( Group group, long interval, ThreadStart job )
		 {
			  return _jobScheduler.scheduleRecurring( group, job, interval, TimeUnit.MILLISECONDS );
		 }
	}

}