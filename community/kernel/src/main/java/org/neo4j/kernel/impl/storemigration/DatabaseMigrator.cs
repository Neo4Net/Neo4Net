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
namespace Org.Neo4j.Kernel.impl.storemigration
{
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ExplicitIndexProvider = Org.Neo4j.Kernel.Impl.Api.ExplicitIndexProvider;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using MigrationProgressMonitor = Org.Neo4j.Kernel.impl.storemigration.monitoring.MigrationProgressMonitor;
	using CountsMigrator = Org.Neo4j.Kernel.impl.storemigration.participant.CountsMigrator;
	using ExplicitIndexMigrator = Org.Neo4j.Kernel.impl.storemigration.participant.ExplicitIndexMigrator;
	using NativeLabelScanStoreMigrator = Org.Neo4j.Kernel.impl.storemigration.participant.NativeLabelScanStoreMigrator;
	using StoreMigrator = Org.Neo4j.Kernel.impl.storemigration.participant.StoreMigrator;
	using LogTailScanner = Org.Neo4j.Kernel.recovery.LogTailScanner;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	/// <summary>
	/// DatabaseMigrator collects all dependencies required for store migration,
	/// prepare and construct all store upgrade participants in correct order and allow clients just migrate store
	/// specified by provided location.
	/// </summary>
	/// <seealso cref= StoreUpgrader </seealso>
	public class DatabaseMigrator
	{
		 private readonly MigrationProgressMonitor _progressMonitor;
		 private readonly FileSystemAbstraction _fs;
		 private readonly Config _config;
		 private readonly LogService _logService;
		 private readonly IndexProviderMap _indexProviderMap;
		 private readonly ExplicitIndexProvider _explicitIndexProvider;
		 private readonly PageCache _pageCache;
		 private readonly RecordFormats _format;
		 private readonly LogTailScanner _tailScanner;
		 private readonly JobScheduler _jobScheduler;

		 public DatabaseMigrator( MigrationProgressMonitor progressMonitor, FileSystemAbstraction fs, Config config, LogService logService, IndexProviderMap indexProviderMap, ExplicitIndexProvider indexProvider, PageCache pageCache, RecordFormats format, LogTailScanner tailScanner, JobScheduler jobScheduler )
		 {
			  this._progressMonitor = progressMonitor;
			  this._fs = fs;
			  this._config = config;
			  this._logService = logService;
			  this._indexProviderMap = indexProviderMap;
			  this._explicitIndexProvider = indexProvider;
			  this._pageCache = pageCache;
			  this._format = format;
			  this._tailScanner = tailScanner;
			  this._jobScheduler = jobScheduler;
		 }

		 /// <summary>
		 /// Performs construction of <seealso cref="StoreUpgrader"/> and all of the necessary participants and performs store
		 /// migration if that is required. </summary>
		 /// <param name="directoryStructure"> database to migrate </param>
		 public virtual void Migrate( DatabaseLayout directoryStructure )
		 {
			  LogProvider logProvider = _logService.InternalLogProvider;
			  UpgradableDatabase upgradableDatabase = new UpgradableDatabase( new StoreVersionCheck( _pageCache ), _format, _tailScanner );
			  StoreUpgrader storeUpgrader = new StoreUpgrader( upgradableDatabase, _progressMonitor, _config, _fs, _pageCache, logProvider );

			  ExplicitIndexMigrator explicitIndexMigrator = new ExplicitIndexMigrator( _fs, _explicitIndexProvider, logProvider );
			  StoreMigrator storeMigrator = new StoreMigrator( _fs, _pageCache, _config, _logService, _jobScheduler );
			  NativeLabelScanStoreMigrator nativeLabelScanStoreMigrator = new NativeLabelScanStoreMigrator( _fs, _pageCache, _config );
			  CountsMigrator countsMigrator = new CountsMigrator( _fs, _pageCache, _config );

			  _indexProviderMap.accept( provider => storeUpgrader.addParticipant( provider.storeMigrationParticipant( _fs, _pageCache ) ) );
			  storeUpgrader.AddParticipant( explicitIndexMigrator );
			  storeUpgrader.AddParticipant( storeMigrator );
			  storeUpgrader.AddParticipant( nativeLabelScanStoreMigrator );
			  storeUpgrader.AddParticipant( countsMigrator );
			  storeUpgrader.MigrateIfNeeded( directoryStructure );
		 }
	}

}