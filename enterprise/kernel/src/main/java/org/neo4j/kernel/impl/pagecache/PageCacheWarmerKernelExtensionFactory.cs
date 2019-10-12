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
namespace Org.Neo4j.Kernel.impl.pagecache
{
	using Service = Org.Neo4j.Helpers.Service;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ExtensionType = Org.Neo4j.Kernel.extension.ExtensionType;
	using Org.Neo4j.Kernel.extension;
	using PageCacheWarmerLoggingMonitor = Org.Neo4j.Kernel.impl.pagecache.monitor.PageCacheWarmerLoggingMonitor;
	using PageCacheWarmerMonitor = Org.Neo4j.Kernel.impl.pagecache.monitor.PageCacheWarmerMonitor;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) public class PageCacheWarmerKernelExtensionFactory extends org.neo4j.kernel.extension.KernelExtensionFactory<PageCacheWarmerKernelExtensionFactory.Dependencies>
	public class PageCacheWarmerKernelExtensionFactory : KernelExtensionFactory<PageCacheWarmerKernelExtensionFactory.Dependencies>
	{
		 public interface Dependencies
		 {
			  JobScheduler JobScheduler();

			  DatabaseAvailabilityGuard AvailabilityGuard();

			  PageCache PageCache();

			  FileSystemAbstraction FileSystemAbstraction();

			  NeoStoreDataSource DataSource { get; }

			  LogService LogService();

			  Monitors Monitors();

			  Config Config();
		 }

		 public PageCacheWarmerKernelExtensionFactory() : base(ExtensionType.DATABASE, "pagecachewarmer")
		 {
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies deps )
		 {
			  JobScheduler scheduler = deps.JobScheduler();
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = deps.AvailabilityGuard();
			  PageCache pageCache = deps.PageCache();
			  FileSystemAbstraction fs = deps.FileSystemAbstraction();
			  LogService logService = deps.LogService();
			  NeoStoreDataSource dataSourceManager = deps.DataSource;
			  Log log = logService.GetInternalLog( typeof( PageCacheWarmer ) );
			  Monitors monitors = deps.Monitors();
			  PageCacheWarmerMonitor monitor = monitors.NewMonitor( typeof( PageCacheWarmerMonitor ) );
			  monitors.AddMonitorListener( new PageCacheWarmerLoggingMonitor( log ) );
			  Config config = deps.Config();
			  return new PageCacheWarmerKernelExtension( scheduler, databaseAvailabilityGuard, pageCache, fs, dataSourceManager, log, monitor, config );
		 }
	}

}