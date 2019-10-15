using System.Threading;

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
namespace Neo4Net.Kernel.ha.cluster
{

	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// Default implementation of <seealso cref="ConversationSPI"/> used on master in HA setup.
	/// </summary>
	public class DefaultConversationSPI : ConversationSPI
	{
		 private readonly Locks _locks;
		 private readonly IJobScheduler _jobScheduler;

		 public DefaultConversationSPI( Locks locks, IJobScheduler jobScheduler )
		 {
			  this._locks = locks;
			  this._jobScheduler = jobScheduler;
		 }

		 public override Neo4Net.Kernel.impl.locking.Locks_Client AcquireClient()
		 {
			  return _locks.newClient();
		 }

		 public override JobHandle ScheduleRecurringJob( Group group, long interval, ThreadStart job )
		 {
			  return _jobScheduler.scheduleRecurring( group, job, interval, TimeUnit.MILLISECONDS );
		 }
	}

}