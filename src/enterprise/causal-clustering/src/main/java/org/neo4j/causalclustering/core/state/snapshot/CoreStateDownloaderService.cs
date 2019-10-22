/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state.snapshot
{

	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using JobHandle = Neo4Net.Scheduler.JobHandle;

	public class CoreStateDownloaderService : LifecycleAdapter
	{
		 private readonly IJobScheduler _jobScheduler;
		 private readonly CoreStateDownloader _downloader;
		 private readonly CommandApplicationProcess _applicationProcess;
		 private readonly Log _log;
		 private readonly Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout _downloaderPauseStrategy;
		 private PersistentSnapshotDownloader _currentJob;
		 private JobHandle _jobHandle;
		 private bool _stopped;
		 private System.Func<DatabaseHealth> _dbHealth;
		 private readonly Monitors _monitors;

		 public CoreStateDownloaderService( IJobScheduler jobScheduler, CoreStateDownloader downloader, CommandApplicationProcess applicationProcess, LogProvider logProvider, Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout downloaderPauseStrategy, System.Func<DatabaseHealth> dbHealth, Monitors monitors )
		 {
			  this._jobScheduler = jobScheduler;
			  this._downloader = downloader;
			  this._applicationProcess = applicationProcess;
			  this._log = logProvider.getLog( this.GetType() );
			  this._downloaderPauseStrategy = downloaderPauseStrategy;
			  this._dbHealth = dbHealth;
			  this._monitors = monitors;
		 }

		 public virtual Optional<JobHandle> ScheduleDownload( CatchupAddressProvider addressProvider )
		 {
			 lock ( this )
			 {
				  if ( _stopped )
				  {
						return null;
				  }
      
				  if ( _currentJob == null || _currentJob.hasCompleted() )
				  {
						_currentJob = new PersistentSnapshotDownloader( addressProvider, _applicationProcess, _downloader, _log, _downloaderPauseStrategy, _dbHealth, _monitors );
						_jobHandle = _jobScheduler.schedule( Group.DOWNLOAD_SNAPSHOT, _currentJob );
						return _jobHandle;
				  }
				  return _jobHandle;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void stop() throws Throwable
		 public override void Stop()
		 {
			 lock ( this )
			 {
				  _stopped = true;
      
				  if ( _currentJob != null )
				  {
						_currentJob.stop();
				  }
			 }
		 }

		 public virtual Optional<JobHandle> DownloadJob()
		 {
			 lock ( this )
			 {
				  return Optional.ofNullable( _jobHandle );
			 }
		 }
	}

}