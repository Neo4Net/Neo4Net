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
namespace Neo4Net.Kernel.impl.pagecache
{
	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using PageCacheWarmerLoggingMonitor = Neo4Net.Kernel.impl.pagecache.monitor.PageCacheWarmerLoggingMonitor;
	using PageCacheWarmerMonitor = Neo4Net.Kernel.impl.pagecache.monitor.PageCacheWarmerMonitor;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) public class PageCacheWarmerKernelExtensionFactory extends org.neo4j.kernel.extension.KernelExtensionFactory<PageCacheWarmerKernelExtensionFactory.Dependencies>
	public class PageCacheWarmerKernelExtensionFactory : KernelExtensionFactory<PageCacheWarmerKernelExtensionFactory.Dependencies>
	{
		 public interface Dependencies
		 {
			  IJobScheduler IJobScheduler();

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
			  IJobScheduler scheduler = deps.JobScheduler();
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