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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using LoggingMonitor = Org.Neo4j.Kernel.Api.Index.LoggingMonitor;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ExtensionType = Org.Neo4j.Kernel.extension.ExtensionType;
	using Org.Neo4j.Kernel.extension;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	public abstract class AbstractIndexProviderFactory<DEPENDENCIES> : KernelExtensionFactory<DEPENDENCIES> where DEPENDENCIES : AbstractIndexProviderFactory.Dependencies
	{
		 protected internal AbstractIndexProviderFactory( string key ) : base( ExtensionType.DATABASE, key )
		 {
		 }

		 public override IndexProvider NewInstance( KernelContext context, DEPENDENCIES dependencies )
		 {
			  PageCache pageCache = dependencies.PageCache();
			  File databaseDir = context.Directory();
			  FileSystemAbstraction fs = dependencies.FileSystem();
			  Log log = dependencies.LogService.InternalLogProvider.getLog( LoggingClass() );
			  Monitors monitors = dependencies.Monitors();
			  monitors.AddMonitorListener( new LoggingMonitor( log ), DescriptorString() );
			  IndexProvider.Monitor monitor = monitors.NewMonitor( typeof( IndexProvider.Monitor ), DescriptorString() );
			  Config config = dependencies.Config;
			  OperationalMode operationalMode = context.DatabaseInfo().OperationalMode;
			  RecoveryCleanupWorkCollector recoveryCleanupWorkCollector = dependencies.RecoveryCleanupWorkCollector();
			  return InternalCreate( pageCache, databaseDir, fs, monitor, config, operationalMode, recoveryCleanupWorkCollector );
		 }

		 protected internal abstract System.Type LoggingClass();

		 protected internal abstract string DescriptorString();

		 protected internal abstract IndexProvider InternalCreate( PageCache pageCache, File storeDir, FileSystemAbstraction fs, IndexProvider.Monitor monitor, Config config, OperationalMode operationalMode, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector );

		 public interface Dependencies
		 {
			  PageCache PageCache();

			  FileSystemAbstraction FileSystem();

			  LogService LogService { get; }

			  Monitors Monitors();

			  Config Config { get; }

			  RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector();
		 }
	}

}