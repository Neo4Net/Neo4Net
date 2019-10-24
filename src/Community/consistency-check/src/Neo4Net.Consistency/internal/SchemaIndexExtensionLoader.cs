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
namespace Neo4Net.Consistency.Internal
{

	using Service = Neo4Net.Helpers.Service;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseKernelExtensions = Neo4Net.Kernel.extension.DatabaseKernelExtensions;
	using Neo4Net.Kernel.extension;
	using KernelExtensionFailureStrategies = Neo4Net.Kernel.extension.KernelExtensionFailureStrategies;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using SimpleKernelContext = Neo4Net.Kernel.impl.spi.SimpleKernelContext;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// Utility for loading <seealso cref="IndexProvider"/> instances from <seealso cref="DatabaseKernelExtensions"/>.
	/// </summary>
	public class SchemaIndexExtensionLoader
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static org.Neo4Net.kernel.extension.DatabaseKernelExtensions instantiateKernelExtensions(java.io.File databaseDirectory, org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, org.Neo4Net.kernel.configuration.Config config, org.Neo4Net.logging.internal.LogService logService, org.Neo4Net.io.pagecache.PageCache pageCache, org.Neo4Net.scheduler.JobScheduler jobScheduler, org.Neo4Net.index.internal.gbptree.RecoveryCleanupWorkCollector recoveryCollector, org.Neo4Net.kernel.impl.factory.DatabaseInfo databaseInfo, org.Neo4Net.kernel.monitoring.Monitors monitors, org.Neo4Net.kernel.impl.core.TokenHolders tokenHolders)
		 public static DatabaseKernelExtensions InstantiateKernelExtensions( File databaseDirectory, FileSystemAbstraction fileSystem, Config config, LogService logService, PageCache pageCache, IJobScheduler jobScheduler, RecoveryCleanupWorkCollector recoveryCollector, DatabaseInfo databaseInfo, Monitors monitors, TokenHolders tokenHolders )
		 {
			  Dependencies deps = new Dependencies();
			  deps.SatisfyDependencies( fileSystem, config, logService, pageCache, recoveryCollector, monitors, jobScheduler, tokenHolders );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") Iterable kernelExtensions = org.Neo4Net.helpers.Service.load(org.Neo4Net.kernel.extension.KernelExtensionFactory.class);
			  System.Collections.IEnumerable kernelExtensions = Service.load( typeof( KernelExtensionFactory ) );
			  KernelContext kernelContext = new SimpleKernelContext( databaseDirectory, databaseInfo, deps );
			  return new DatabaseKernelExtensions( kernelContext, kernelExtensions, deps, KernelExtensionFailureStrategies.ignore() );
		 }
	}

}