using System;
using System.Collections.Concurrent;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Bolt.runtime
{

	using Config = Neo4Net.Kernel.configuration.Config;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	public class ExecutorBoltSchedulerProvider : LifecycleAdapter, BoltSchedulerProvider
	{
		 private readonly Config _config;
		 private readonly ExecutorFactory _executorFactory;
		 private readonly JobScheduler _scheduler;
		 private readonly LogService _logService;
		 private readonly Log _internalLog;
		 private readonly ConcurrentDictionary<string, BoltScheduler> _boltSchedulers;

		 private ExecutorService _forkJoinThreadPool;

		 public ExecutorBoltSchedulerProvider( Config config, ExecutorFactory executorFactory, JobScheduler scheduler, LogService logService )
		 {
			  this._config = config;
			  this._executorFactory = executorFactory;
			  this._scheduler = scheduler;
			  this._logService = logService;
			  this._internalLog = logService.GetInternalLog( this.GetType() );
			  this._boltSchedulers = new ConcurrentDictionary<string, BoltScheduler>();
		 }

		 public override void Start()
		 {
			  _forkJoinThreadPool = new ForkJoinPool();
			  _config.enabledBoltConnectors().ForEach(connector =>
			  {
				BoltScheduler boltScheduler = new ExecutorBoltScheduler( connector.key(), _executorFactory, _scheduler, _logService, _config.get(connector.thread_pool_min_size), _config.get(connector.thread_pool_max_size), _config.get(connector.thread_pool_keep_alive), _config.get(connector.unsupported_thread_pool_queue_size), _forkJoinThreadPool );
				boltScheduler.Start();
				_boltSchedulers[connector.key()] = boltScheduler;
			  });
		 }

		 public override void Stop()
		 {
			  _boltSchedulers.Values.forEach( this.stopScheduler );
			  _boltSchedulers.Clear();

			  _forkJoinThreadPool.shutdown();
			  _forkJoinThreadPool = null;
		 }

		 private void StopScheduler( BoltScheduler scheduler )
		 {
			  try
			  {
					scheduler.Stop();
			  }
			  catch ( Exception t )
			  {
					_internalLog.warn( string.Format( "An unexpected error occurred while stopping BoltScheduler [{0}]", scheduler.Connector() ), t );
			  }
		 }

		 public override BoltScheduler Get( BoltChannel channel )
		 {
			  BoltScheduler boltScheduler = _boltSchedulers[channel.Connector()];
			  if ( boltScheduler == null )
			  {
					throw new System.ArgumentException( string.Format( "Provided channel instance [local: {0}, remote: {1}] is not bound to any known bolt listen addresses.", channel.ServerAddress(), channel.ClientAddress() ) );
			  }

			  return boltScheduler;
		 }

	}

}