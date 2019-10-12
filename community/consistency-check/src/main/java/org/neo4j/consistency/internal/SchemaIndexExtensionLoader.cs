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
namespace Org.Neo4j.Consistency.@internal
{

	using Service = Org.Neo4j.Helpers.Service;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseKernelExtensions = Org.Neo4j.Kernel.extension.DatabaseKernelExtensions;
	using Org.Neo4j.Kernel.extension;
	using KernelExtensionFailureStrategies = Org.Neo4j.Kernel.extension.KernelExtensionFailureStrategies;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using SimpleKernelContext = Org.Neo4j.Kernel.impl.spi.SimpleKernelContext;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	/// <summary>
	/// Utility for loading <seealso cref="IndexProvider"/> instances from <seealso cref="DatabaseKernelExtensions"/>.
	/// </summary>
	public class SchemaIndexExtensionLoader
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static org.neo4j.kernel.extension.DatabaseKernelExtensions instantiateKernelExtensions(java.io.File databaseDirectory, org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.kernel.configuration.Config config, org.neo4j.logging.internal.LogService logService, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.scheduler.JobScheduler jobScheduler, org.neo4j.index.internal.gbptree.RecoveryCleanupWorkCollector recoveryCollector, org.neo4j.kernel.impl.factory.DatabaseInfo databaseInfo, org.neo4j.kernel.monitoring.Monitors monitors, org.neo4j.kernel.impl.core.TokenHolders tokenHolders)
		 public static DatabaseKernelExtensions InstantiateKernelExtensions( File databaseDirectory, FileSystemAbstraction fileSystem, Config config, LogService logService, PageCache pageCache, JobScheduler jobScheduler, RecoveryCleanupWorkCollector recoveryCollector, DatabaseInfo databaseInfo, Monitors monitors, TokenHolders tokenHolders )
		 {
			  Dependencies deps = new Dependencies();
			  deps.SatisfyDependencies( fileSystem, config, logService, pageCache, recoveryCollector, monitors, jobScheduler, tokenHolders );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") Iterable kernelExtensions = org.neo4j.helpers.Service.load(org.neo4j.kernel.extension.KernelExtensionFactory.class);
			  System.Collections.IEnumerable kernelExtensions = Service.load( typeof( KernelExtensionFactory ) );
			  KernelContext kernelContext = new SimpleKernelContext( databaseDirectory, databaseInfo, deps );
			  return new DatabaseKernelExtensions( kernelContext, kernelExtensions, deps, KernelExtensionFailureStrategies.ignore() );
		 }
	}

}