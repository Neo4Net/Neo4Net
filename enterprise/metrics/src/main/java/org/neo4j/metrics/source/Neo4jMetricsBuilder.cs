using System;

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
namespace Org.Neo4j.metrics.source
{
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using CoreMetaData = Org.Neo4j.causalclustering.core.consensus.CoreMetaData;
	using PageCacheCounters = Org.Neo4j.Io.pagecache.monitoring.PageCacheCounters;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ClusterMembers;
	using LogRotationMonitor = Org.Neo4j.Kernel.Impl.Api.LogRotationMonitor;
	using Edition = Org.Neo4j.Kernel.impl.factory.Edition;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using StoreEntityCounters = Org.Neo4j.Kernel.impl.store.stats.StoreEntityCounters;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointerMonitor = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointerMonitor;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using TransactionCounters = Org.Neo4j.Kernel.impl.transaction.stats.TransactionCounters;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using EventReporter = Org.Neo4j.metrics.output.EventReporter;
	using CatchUpMetrics = Org.Neo4j.metrics.source.causalclustering.CatchUpMetrics;
	using CoreMetrics = Org.Neo4j.metrics.source.causalclustering.CoreMetrics;
	using ReadReplicaMetrics = Org.Neo4j.metrics.source.causalclustering.ReadReplicaMetrics;
	using ClusterMetrics = Org.Neo4j.metrics.source.cluster.ClusterMetrics;
	using NetworkMetrics = Org.Neo4j.metrics.source.cluster.NetworkMetrics;
	using BoltMetrics = Org.Neo4j.metrics.source.db.BoltMetrics;
	using CheckPointingMetrics = Org.Neo4j.metrics.source.db.CheckPointingMetrics;
	using CypherMetrics = Org.Neo4j.metrics.source.db.CypherMetrics;
	using EntityCountMetrics = Org.Neo4j.metrics.source.db.EntityCountMetrics;
	using LogRotationMetrics = Org.Neo4j.metrics.source.db.LogRotationMetrics;
	using PageCacheMetrics = Org.Neo4j.metrics.source.db.PageCacheMetrics;
	using TransactionMetrics = Org.Neo4j.metrics.source.db.TransactionMetrics;
	using GCMetrics = Org.Neo4j.metrics.source.jvm.GCMetrics;
	using MemoryBuffersMetrics = Org.Neo4j.metrics.source.jvm.MemoryBuffersMetrics;
	using MemoryPoolMetrics = Org.Neo4j.metrics.source.jvm.MemoryPoolMetrics;
	using ThreadMetrics = Org.Neo4j.metrics.source.jvm.ThreadMetrics;
	using ServerMetrics = Org.Neo4j.metrics.source.server.ServerMetrics;

	public class Neo4jMetricsBuilder
	{
		 private readonly MetricRegistry _registry;
		 private readonly LifeSupport _life;
		 private readonly EventReporter _reporter;
		 private readonly Config _config;
		 private readonly LogService _logService;
		 private readonly KernelContext _kernelContext;
		 private readonly Dependencies _dependencies;

		 public interface Dependencies
		 {
			  Monitors Monitors();

			  TransactionCounters TransactionCounters();

			  PageCacheCounters PageCacheCounters();

			  System.Func<ClusterMembers> ClusterMembers();

			  System.Func<CoreMetaData> Raft();

			  DataSourceManager DataSourceManager();
		 }

		 public Neo4jMetricsBuilder( MetricRegistry registry, EventReporter reporter, Config config, LogService logService, KernelContext kernelContext, Dependencies dependencies, LifeSupport life )
		 {
			  this._registry = registry;
			  this._reporter = reporter;
			  this._config = config;
			  this._logService = logService;
			  this._kernelContext = kernelContext;
			  this._dependencies = dependencies;
			  this._life = life;
		 }

		 public virtual bool Build()
		 {
			  bool result = false;
			  if ( _config.get( MetricsSettings.neoTxEnabled ) )
			  {
					_life.add( new TransactionMetrics( _registry, DatabaseDependencySupplier( typeof( TransactionIdStore ) ), _dependencies.transactionCounters() ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.neoPageCacheEnabled ) )
			  {
					_life.add( new PageCacheMetrics( _registry, _dependencies.pageCacheCounters() ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.neoCheckPointingEnabled ) )
			  {
					_life.add( new CheckPointingMetrics( _reporter, _registry, _dependencies.monitors(), DatabaseDependencySupplier(typeof(CheckPointerMonitor)) ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.neoLogRotationEnabled ) )
			  {
					_life.add( new LogRotationMetrics( _reporter, _registry, _dependencies.monitors(), DatabaseDependencySupplier(typeof(LogRotationMonitor)) ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.neoCountsEnabled ) )
			  {
					if ( _kernelContext.databaseInfo().edition != Edition.community && _kernelContext.databaseInfo().edition != Edition.unknown )
					{
						 _life.add( new EntityCountMetrics( _registry, DatabaseDependencySupplier( typeof( StoreEntityCounters ) ) ) );
						 result = true;
					}
			  }

			  if ( _config.get( MetricsSettings.neoNetworkEnabled ) )
			  {
					_life.add( new NetworkMetrics( _registry, _dependencies.monitors() ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.neoClusterEnabled ) )
			  {
					if ( _kernelContext.databaseInfo().operationalMode == OperationalMode.ha )
					{
						 _life.add( new ClusterMetrics( _dependencies.monitors(), _registry, _dependencies.clusterMembers() ) );
						 result = true;
					}
			  }

			  if ( _config.get( MetricsSettings.cypherPlanningEnabled ) )
			  {
					_life.add( new CypherMetrics( _registry, _dependencies.monitors() ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.jvmGcEnabled ) )
			  {
					_life.add( new GCMetrics( _registry ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.jvmThreadsEnabled ) )
			  {
					_life.add( new ThreadMetrics( _registry ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.boltMessagesEnabled ) )
			  {
					_life.add( new BoltMetrics( _registry, _dependencies.monitors() ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.jvmMemoryEnabled ) )
			  {
					_life.add( new MemoryPoolMetrics( _registry ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.jvmBuffersEnabled ) )
			  {
					_life.add( new MemoryBuffersMetrics( _registry ) );
					result = true;
			  }

			  if ( _config.get( MetricsSettings.causalClusteringEnabled ) )
			  {
					OperationalMode mode = _kernelContext.databaseInfo().operationalMode;
					if ( mode == OperationalMode.core )
					{
						 _life.add( new CoreMetrics( _dependencies.monitors(), _registry, _dependencies.raft() ) );
						 _life.add( new CatchUpMetrics( _dependencies.monitors(), _registry ) );
						 result = true;
					}
					else if ( mode == OperationalMode.read_replica )
					{
						 _life.add( new ReadReplicaMetrics( _dependencies.monitors(), _registry ) );
						 _life.add( new CatchUpMetrics( _dependencies.monitors(), _registry ) );
						 result = true;
					}
			  }

			  bool httpOrHttpsEnabled = _config.enabledHttpConnectors().Count > 0;
			  if ( httpOrHttpsEnabled && _config.get( MetricsSettings.neoServerEnabled ) )
			  {
					_life.add( new ServerMetrics( _registry, _logService, _kernelContext.dependencySatisfier() ) );
					result = true;
			  }

			  return result;
		 }

		 private System.Func<T> DatabaseDependencySupplier<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return () => _dependencies.dataSourceManager().DataSource.DependencyResolver.resolveDependency(clazz);
		 }
	}

}