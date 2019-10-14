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
namespace Neo4Net.Kernel
{

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using TokenNameLookup = Neo4Net.Internal.Kernel.Api.TokenNameLookup;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using DatabaseAvailability = Neo4Net.Kernel.availability.DatabaseAvailability;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.extension;
	using CommitProcessFactory = Neo4Net.Kernel.Impl.Api.CommitProcessFactory;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using SchemaWriteGuard = Neo4Net.Kernel.Impl.Api.SchemaWriteGuard;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using CoreAPIAvailabilityGuard = Neo4Net.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using QueryEngineProvider = Neo4Net.Kernel.impl.query.QueryEngineProvider;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using StoreCopyCheckPointMutex = Neo4Net.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using LogFileCreationMonitor = Neo4Net.Kernel.impl.transaction.log.files.LogFileCreationMonitor;
	using CollectionsFactorySupplier = Neo4Net.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using FileSystemWatcherService = Neo4Net.Kernel.impl.util.watcher.FileSystemWatcherService;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using TransactionEventHandlers = Neo4Net.Kernel.Internal.TransactionEventHandlers;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Tracers = Neo4Net.Kernel.monitoring.tracing.Tracers;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	public interface DatabaseCreationContext
	{
		 string DatabaseName { get; }

		 DatabaseLayout DatabaseLayout { get; }

		 Config Config { get; }

		 IdGeneratorFactory IdGeneratorFactory { get; }

		 LogService LogService { get; }

		 JobScheduler Scheduler { get; }

		 TokenNameLookup TokenNameLookup { get; }

		 DependencyResolver GlobalDependencies { get; }

		 TokenHolders TokenHolders { get; }

		 Locks Locks { get; }

		 StatementLocksFactory StatementLocksFactory { get; }

		 SchemaWriteGuard SchemaWriteGuard { get; }

		 TransactionEventHandlers TransactionEventHandlers { get; }

		 IndexingService.Monitor IndexingServiceMonitor { get; }

		 FileSystemAbstraction Fs { get; }

		 TransactionMonitor TransactionMonitor { get; }

		 DatabaseHealth DatabaseHealth { get; }

		 LogFileCreationMonitor PhysicalLogMonitor { get; }

		 TransactionHeaderInformationFactory TransactionHeaderInformationFactory { get; }

		 CommitProcessFactory CommitProcessFactory { get; }

		 AutoIndexing AutoIndexing { get; }

		 IndexConfigStore IndexConfigStore { get; }

		 ExplicitIndexProvider ExplicitIndexProvider { get; }

		 PageCache PageCache { get; }

		 ConstraintSemantics ConstraintSemantics { get; }

		 Monitors Monitors { get; }

		 Tracers Tracers { get; }

		 Procedures Procedures { get; }

		 IOLimiter IoLimiter { get; }

		 DatabaseAvailabilityGuard DatabaseAvailabilityGuard { get; }

		 CoreAPIAvailabilityGuard CoreAPIAvailabilityGuard { get; }

		 SystemNanoClock Clock { get; }

		 AccessCapability AccessCapability { get; }

		 StoreCopyCheckPointMutex StoreCopyCheckPointMutex { get; }

		 RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector { get; }

		 IdController IdController { get; }

		 DatabaseInfo DatabaseInfo { get; }

		 VersionContextSupplier VersionContextSupplier { get; }

		 CollectionsFactorySupplier CollectionsFactorySupplier { get; }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> getKernelExtensionFactories();
		 IEnumerable<KernelExtensionFactory<object>> KernelExtensionFactories { get; }

		 System.Func<File, FileSystemWatcherService> WatcherServiceFactory { get; }

		 GraphDatabaseFacade Facade { get; }

		 IEnumerable<QueryEngineProvider> EngineProviders { get; }

		 DatabaseAvailability DatabaseAvailability { get; }
	}

}