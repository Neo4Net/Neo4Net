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
namespace Neo4Net.Kernel.ha
{

	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// This scheduler is part of slave lifecycle that will schedule periodic pulling on slave switch
	/// and turn them off during slave shutdown.
	/// </summary>
	/// <seealso cref= UpdatePuller </seealso>
	public class UpdatePullerScheduler : LifecycleAdapter
	{
		 private readonly JobScheduler _scheduler;
		 private readonly Log _log;
		 private readonly UpdatePuller _updatePuller;
		 private readonly long _pullIntervalMillis;
		 private JobHandle _intervalJobHandle;

		 public UpdatePullerScheduler( JobScheduler scheduler, LogProvider logProvider, UpdatePuller updatePullingThread, long pullIntervalMillis )
		 {
			  this._scheduler = scheduler;
			  this._log = logProvider.getLog( this.GetType() );
			  this._updatePuller = updatePullingThread;
			  this._pullIntervalMillis = pullIntervalMillis;
		 }

		 public override void Init()
		 {
			  if ( _pullIntervalMillis > 0 )
			  {
					_intervalJobHandle = _scheduler.scheduleRecurring(Group.PULL_UPDATES, () =>
					{
					 try
					 {
						  _updatePuller.pullUpdates();
					 }
					 catch ( InterruptedException e )
					 {
						  _log.error( "Pull updates failed", e );
					 }
					}, _pullIntervalMillis, _pullIntervalMillis, TimeUnit.MILLISECONDS);
			  }
		 }

		 public override void Shutdown()
		 {
			  if ( _intervalJobHandle != null )
			  {
					_intervalJobHandle.cancel( false );
			  }
		 }
	}

}