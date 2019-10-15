using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Internal
{

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using DataCollectorModule = Neo4Net.Internal.Collector.DataCollectorModule;
	using IOUtils = Neo4Net.Io.IOUtils;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DefaultValueMapper = Neo4Net.Kernel.impl.util.DefaultValueMapper;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	public class DataCollectorManager : LifecycleAdapter
	{
		 private readonly DataSourceManager _dataSourceManager;
		 private readonly IJobScheduler _jobScheduler;
		 private readonly Procedures _procedures;
		 private readonly Monitors _monitors;
		 private readonly Config _config;
		 private readonly IList<IDisposable> _dataCollectors;

		 public DataCollectorManager( DataSourceManager dataSourceManager, IJobScheduler jobScheduler, Procedures procedures, Monitors monitors, Config config )
		 {
			  this._dataSourceManager = dataSourceManager;
			  this._jobScheduler = jobScheduler;
			  this._procedures = procedures;
			  this._monitors = monitors;
			  this._config = config;
			  this._dataCollectors = new List<IDisposable>();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  // When we have multiple dbs, this has to be suitably modified to get the right kernel and procedures
			  NeoStoreDataSource dataSource = _dataSourceManager.DataSource;
			  EmbeddedProxySPI embeddedProxySPI = dataSource.DependencyResolver.resolveDependency( typeof( EmbeddedProxySPI ), Neo4Net.Graphdb.DependencyResolver_SelectionStrategy.ONLY );
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