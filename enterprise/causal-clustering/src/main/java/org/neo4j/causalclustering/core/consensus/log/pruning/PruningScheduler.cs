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
namespace Org.Neo4j.causalclustering.core.consensus.log.pruning
{


	using RaftLogPruner = Org.Neo4j.causalclustering.core.state.RaftLogPruner;
	using Predicates = Org.Neo4j.Function.Predicates;
	using UnderlyingStorageException = Org.Neo4j.Kernel.impl.store.UnderlyingStorageException;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class PruningScheduler : LifecycleAdapter
	{
		 private readonly RaftLogPruner _logPruner;
		 private readonly JobScheduler _scheduler;
		 private readonly long _recurringPeriodMillis;
		 private readonly ThreadStart job = () =>
		 {
			try
			{
				 _checkPointing = true;
				 if ( _stopped )
				 {
					  return;
				 }
				 _logPruner.prune();
			}
			catch ( IOException e )
			{
				 // no need to reschedule since the check pointer has raised a kernel panic and a shutdown is expected
				 throw new UnderlyingStorageException( e );
			}
			finally
			{
				 _checkPointing = false;
			}

			// reschedule only if it is not stopped
			if ( !_stopped )
			{
				 _handle = _scheduler.schedule( Group.RAFT_LOG_PRUNING, job, _recurringPeriodMillis, MILLISECONDS );
			}
		 };
		 private readonly Log _log;

		 private volatile JobHandle _handle;
		 private volatile bool _stopped;
		 private volatile bool _checkPointing;
		 private readonly System.Func<bool> checkPointingCondition = () =>
		 {
			return !_checkPointing;
		 };

		 public PruningScheduler( RaftLogPruner logPruner, JobScheduler scheduler, long recurringPeriodMillis, LogProvider logProvider )
		 {
			  this._logPruner = logPruner;
			  this._scheduler = scheduler;
			  this._recurringPeriodMillis = recurringPeriodMillis;
			  _log = logProvider.getLog( this.GetType() );
		 }

		 public override void Start()
		 {
			  _handle = _scheduler.schedule( Group.RAFT_LOG_PRUNING, job, _recurringPeriodMillis, MILLISECONDS );
		 }

		 public override void Stop()
		 {
			  _log.info( "PruningScheduler stopping" );
			  _stopped = true;
			  if ( _handle != null )
			  {
					_handle.cancel( false );
			  }
			  Predicates.awaitForever( checkPointingCondition, 100, MILLISECONDS );
		 }
	}

}