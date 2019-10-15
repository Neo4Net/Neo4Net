using System;

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
namespace Neo4Net.Kernel.impl.pagecache
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AvailabilityListener = Neo4Net.Kernel.availability.AvailabilityListener;
	using Config = Neo4Net.Kernel.configuration.Config;
	using PageCacheWarmerMonitor = Neo4Net.Kernel.impl.pagecache.monitor.PageCacheWarmerMonitor;
	using Log = Neo4Net.Logging.Log;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	internal class WarmupAvailabilityListener : AvailabilityListener
	{
		 private readonly IJobScheduler _scheduler;
		 private readonly PageCacheWarmer _pageCacheWarmer;
		 private readonly Config _config;
		 private readonly Log _log;
		 private readonly PageCacheWarmerMonitor _monitor;

		 // We use the monitor lock to guard the job handle. However, it could happen that a job has already started, ends
		 // up waiting for the lock while it's being held by another thread calling `unavailable()`. In that case, we need
		 // to make sure that the signal to stop is not lost. Cancelling a job handle only works on jobs that haven't
		 // started yet, since we don't propagate an interrupt. This is why we check the `available` field in the
		 // `scheduleProfile` method.
		 private volatile bool _available;
		 private JobHandle _jobHandle; // Guarded by `this`.

		 internal WarmupAvailabilityListener( IJobScheduler scheduler, PageCacheWarmer pageCacheWarmer, Config config, Log log, PageCacheWarmerMonitor monitor )
		 {
			  this._scheduler = scheduler;
			  this._pageCacheWarmer = pageCacheWarmer;
			  this._config = config;
			  this._log = log;
			  this._monitor = monitor;
		 }

		 public override void Available()
		 {
			 lock ( this )
			 {
				  _available = true;
				  _jobHandle = _scheduler.schedule( Group.FILE_IO_HELPER, this.startWarmup );
			 }
		 }

		 private void StartWarmup()
		 {
			  if ( !_available )
			  {
					return;
			  }
			  try
			  {
					_monitor.warmupStarted();
					_pageCacheWarmer.reheat().ifPresent(_monitor.warmupCompleted);
			  }
			  catch ( Exception e )
			  {
					_log.debug( "Active page cache warmup failed, " + "so it may take longer for the cache to be populated with hot data.", e );
			  }

			  ScheduleProfile();
		 }

		 private void ScheduleProfile()
		 {
			 lock ( this )
			 {
				  if ( !_available )
				  {
						return;
				  }
				  long frequencyMillis = _config.get( GraphDatabaseSettings.pagecache_warmup_profiling_interval ).toMillis();
				  _jobHandle = _scheduler.scheduleRecurring( Group.FILE_IO_HELPER, this.doProfile, frequencyMillis, TimeUnit.MILLISECONDS );
			 }
		 }

		 private void DoProfile()
		 {
			  try
			  {
					_pageCacheWarmer.profile().ifPresent(_monitor.profileCompleted);
			  }
			  catch ( Exception e )
			  {
					_log.debug( "Page cache profiling failed, so no new profile of what data is hot or not was produced. " + "This may reduce the effectiveness of a future page cache warmup process.", e );
			  }
		 }

		 public override void Unavailable()
		 {
			 lock ( this )
			 {
				  _available = false;
				  if ( _jobHandle != null )
				  {
						_jobHandle.cancel( false );
						_jobHandle = null;
				  }
			 }
		 }
	}

}