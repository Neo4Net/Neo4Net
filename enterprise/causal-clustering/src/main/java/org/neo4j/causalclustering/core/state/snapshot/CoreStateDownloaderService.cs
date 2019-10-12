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
namespace Org.Neo4j.causalclustering.core.state.snapshot
{

	using CatchupAddressProvider = Org.Neo4j.causalclustering.catchup.CatchupAddressProvider;
	using TimeoutStrategy = Org.Neo4j.causalclustering.helper.TimeoutStrategy;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;

	public class CoreStateDownloaderService : LifecycleAdapter
	{
		 private readonly JobScheduler _jobScheduler;
		 private readonly CoreStateDownloader _downloader;
		 private readonly CommandApplicationProcess _applicationProcess;
		 private readonly Log _log;
		 private readonly Org.Neo4j.causalclustering.helper.TimeoutStrategy_Timeout _downloaderPauseStrategy;
		 private PersistentSnapshotDownloader _currentJob;
		 private JobHandle _jobHandle;
		 private bool _stopped;
		 private System.Func<DatabaseHealth> _dbHealth;
		 private readonly Monitors _monitors;

		 public CoreStateDownloaderService( JobScheduler jobScheduler, CoreStateDownloader downloader, CommandApplicationProcess applicationProcess, LogProvider logProvider, Org.Neo4j.causalclustering.helper.TimeoutStrategy_Timeout downloaderPauseStrategy, System.Func<DatabaseHealth> dbHealth, Monitors monitors )
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