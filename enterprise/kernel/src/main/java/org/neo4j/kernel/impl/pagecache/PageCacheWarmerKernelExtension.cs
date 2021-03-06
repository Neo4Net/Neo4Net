﻿/*
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
namespace Org.Neo4j.Kernel.impl.pagecache
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using PageCacheWarmerMonitor = Org.Neo4j.Kernel.impl.pagecache.monitor.PageCacheWarmerMonitor;
	using NeoStoreFileListing = Org.Neo4j.Kernel.impl.transaction.state.NeoStoreFileListing;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	internal class PageCacheWarmerKernelExtension : LifecycleAdapter
	{
		 private readonly DatabaseAvailabilityGuard _databaseAvailabilityGuard;
		 private readonly NeoStoreDataSource _dataSource;
		 private readonly Config _config;
		 private readonly PageCacheWarmer _pageCacheWarmer;
		 private readonly WarmupAvailabilityListener _availabilityListener;
		 private volatile bool _started;

		 internal PageCacheWarmerKernelExtension( JobScheduler scheduler, DatabaseAvailabilityGuard databaseAvailabilityGuard, PageCache pageCache, FileSystemAbstraction fs, NeoStoreDataSource dataSource, Log log, PageCacheWarmerMonitor monitor, Config config )
		 {
			  this._databaseAvailabilityGuard = databaseAvailabilityGuard;
			  this._dataSource = dataSource;
			  this._config = config;
			  _pageCacheWarmer = new PageCacheWarmer( fs, pageCache, scheduler, dataSource.DatabaseLayout.databaseDirectory() );
			  _availabilityListener = new WarmupAvailabilityListener( scheduler, _pageCacheWarmer, config, log, monitor );
		 }

		 public override void Start()
		 {
			  if ( _config.get( GraphDatabaseSettings.pagecache_warmup_enabled ) )
			  {
					_pageCacheWarmer.start();
					_databaseAvailabilityGuard.addListener( _availabilityListener );
					NeoStoreFileListing.registerStoreFileProvider( _pageCacheWarmer );
					_started = true;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  if ( _started )
			  {
					_databaseAvailabilityGuard.removeListener( _availabilityListener );
					_availabilityListener.unavailable(); // Make sure scheduled jobs get cancelled.
					_pageCacheWarmer.stop();
					_started = false;
			  }
		 }

		 private NeoStoreFileListing NeoStoreFileListing
		 {
			 get
			 {
				  return _dataSource.DependencyResolver.resolveDependency( typeof( NeoStoreFileListing ) );
			 }
		 }
	}

}