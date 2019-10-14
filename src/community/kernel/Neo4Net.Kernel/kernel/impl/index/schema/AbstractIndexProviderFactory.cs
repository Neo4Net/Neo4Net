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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using LoggingMonitor = Neo4Net.Kernel.Api.Index.LoggingMonitor;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

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