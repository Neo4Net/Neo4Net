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
namespace Neo4Net.metrics.source
{
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using CoreMetaData = Neo4Net.causalclustering.core.consensus.CoreMetaData;
	using PageCacheCounters = Neo4Net.Io.pagecache.monitoring.PageCacheCounters;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using LogRotationMonitor = Neo4Net.Kernel.Impl.Api.LogRotationMonitor;
	using Edition = Neo4Net.Kernel.impl.factory.Edition;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using StoreEntityCounters = Neo4Net.Kernel.impl.store.stats.StoreEntityCounters;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointerMonitor = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointerMonitor;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using TransactionCounters = Neo4Net.Kernel.impl.transaction.stats.TransactionCounters;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using EventReporter = Neo4Net.metrics.output.EventReporter;
	using CatchUpMetrics = Neo4Net.metrics.source.causalclustering.CatchUpMetrics;
	using CoreMetrics = Neo4Net.metrics.source.causalclustering.CoreMetrics;
	using ReadReplicaMetrics = Neo4Net.metrics.source.causalclustering.ReadReplicaMetrics;
	using ClusterMetrics = Neo4Net.metrics.source.cluster.ClusterMetrics;
	using NetworkMetrics = Neo4Net.metrics.source.cluster.NetworkMetrics;
	using BoltMetrics = Neo4Net.metrics.source.db.BoltMetrics;
	using CheckPointingMetrics = Neo4Net.metrics.source.db.CheckPointingMetrics;
	using CypherMetrics = Neo4Net.metrics.source.db.CypherMetrics;
	using EntityCountMetrics = Neo4Net.metrics.source.db.EntityCountMetrics;
	using LogRotationMetrics = Neo4Net.metrics.source.db.LogRotationMetrics;
	using PageCacheMetrics = Neo4Net.metrics.source.db.PageCacheMetrics;
	using TransactionMetrics = Neo4Net.metrics.source.db.TransactionMetrics;
	using GCMetrics = Neo4Net.metrics.source.jvm.GCMetrics;
	using MemoryBuffersMetrics = Neo4Net.metrics.source.jvm.MemoryBuffersMetrics;
	using MemoryPoolMetrics = Neo4Net.metrics.source.jvm.MemoryPoolMetrics;
	using ThreadMetrics = Neo4Net.metrics.source.jvm.ThreadMetrics;
	using ServerMetrics = Neo4Net.metrics.source.server.ServerMetrics;

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