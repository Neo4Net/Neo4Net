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
namespace Org.Neo4j.Kernel
{

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using AutoIndexing = Org.Neo4j.Kernel.api.explicitindex.AutoIndexing;
	using DatabaseAvailability = Org.Neo4j.Kernel.availability.DatabaseAvailability;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.extension;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using ExplicitIndexProvider = Org.Neo4j.Kernel.Impl.Api.ExplicitIndexProvider;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using CoreAPIAvailabilityGuard = Org.Neo4j.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using AccessCapability = Org.Neo4j.Kernel.impl.factory.AccessCapability;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using QueryEngineProvider = Org.Neo4j.Kernel.impl.query.QueryEngineProvider;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Org.Neo4j.Kernel.impl.transaction.TransactionMonitor;
	using StoreCopyCheckPointMutex = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using LogFileCreationMonitor = Org.Neo4j.Kernel.impl.transaction.log.files.LogFileCreationMonitor;
	using CollectionsFactorySupplier = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using FileSystemWatcherService = Org.Neo4j.Kernel.impl.util.watcher.FileSystemWatcherService;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using TransactionEventHandlers = Org.Neo4j.Kernel.@internal.TransactionEventHandlers;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Tracers = Org.Neo4j.Kernel.monitoring.tracing.Tracers;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

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