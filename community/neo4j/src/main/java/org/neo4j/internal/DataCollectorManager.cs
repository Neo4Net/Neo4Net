using System.Collections.Generic;

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
namespace Org.Neo4j.@internal
{

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using DataCollectorModule = Org.Neo4j.@internal.Collector.DataCollectorModule;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using EmbeddedProxySPI = Org.Neo4j.Kernel.impl.core.EmbeddedProxySPI;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using DefaultValueMapper = Org.Neo4j.Kernel.impl.util.DefaultValueMapper;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public class DataCollectorManager : LifecycleAdapter
	{
		 private readonly DataSourceManager _dataSourceManager;
		 private readonly JobScheduler _jobScheduler;
		 private readonly Procedures _procedures;
		 private readonly Monitors _monitors;
		 private readonly Config _config;
		 private readonly IList<AutoCloseable> _dataCollectors;

		 public DataCollectorManager( DataSourceManager dataSourceManager, JobScheduler jobScheduler, Procedures procedures, Monitors monitors, Config config )
		 {
			  this._dataSourceManager = dataSourceManager;
			  this._jobScheduler = jobScheduler;
			  this._procedures = procedures;
			  this._monitors = monitors;
			  this._config = config;
			  this._dataCollectors = new List<AutoCloseable>();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  // When we have multiple dbs, this has to be suitably modified to get the right kernel and procedures
			  NeoStoreDataSource dataSource = _dataSourceManager.DataSource;
			  EmbeddedProxySPI embeddedProxySPI = dataSource.DependencyResolver.resolveDependency( typeof( EmbeddedProxySPI ), Org.Neo4j.Graphdb.DependencyResolver_SelectionStrategy.ONLY );
			  _dataCollectors.Add( DataCollectorModule.setupDataCollector( _procedures, _jobScheduler, dataSource.Kernel, _monitors, new DefaultValueMapper( embeddedProxySPI ), _config ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  try
			  {
					IOUtils.closeAll( _dataCollectors );
			  }
			  finally
			  {
					_dataCollectors.Clear();
			  }
		 }
	}

}