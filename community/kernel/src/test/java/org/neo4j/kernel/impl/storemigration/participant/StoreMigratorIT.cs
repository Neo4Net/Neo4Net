﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.storemigration.participant
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using TransactionId = Org.Neo4j.Kernel.impl.store.TransactionId;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using StandardV2_3 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV2_3;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using SilentMigrationProgressMonitor = Org.Neo4j.Kernel.impl.storemigration.monitoring.SilentMigrationProgressMonitor;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogTailScanner = Org.Neo4j.Kernel.recovery.LogTailScanner;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class StoreMigratorIT
	public class StoreMigratorIT
	{
		private bool InstanceFieldsInitialized = false;

		public StoreMigratorIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _pageCacheRule );
			_fs = _fileSystemRule.get();
		}

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private static readonly Config _config = Config.defaults( GraphDatabaseSettings.pagecache_memory, "8m" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

		 private readonly Monitors _monitors = new Monitors();
		 private FileSystemAbstraction _fs;
		 private JobScheduler _jobScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String version;
		 public string Version;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public org.neo4j.kernel.impl.transaction.log.LogPosition expectedLogPosition;
		 public LogPosition ExpectedLogPosition;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public System.Func<org.neo4j.kernel.impl.store.TransactionId, bool> txIdComparator;
		 public System.Func<TransactionId, bool> TxIdComparator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> versions()
		 public static ICollection<object[]> Versions()
		 {
			  return Arrays.asList<object[]>( new object[]{ StandardV2_3.STORE_VERSION, new LogPosition( 3, 169 ), TxInfoAcceptanceOnIdAndTimestamp( 39, UNKNOWN_TX_COMMIT_TIMESTAMP ) } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _jobScheduler = new ThreadPoolJobScheduler();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _jobScheduler.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToResumeMigrationOnMoving() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToResumeMigrationOnMoving()
		 {
			  // GIVEN a legacy database
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File prepare = _directory.directory( "prepare" );
			  MigrationTestUtils.prepareSampleLegacyDatabase( Version, _fs, databaseLayout.DatabaseDirectory(), prepare );
			  // and a state of the migration saying that it has done the actual migration
			  LogService logService = NullLogService.Instance;
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  LogTailScanner tailScanner = GetTailScanner( databaseLayout.DatabaseDirectory() );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache, tailScanner );

			  string versionToMigrateFrom = upgradableDatabase.CheckUpgradable( databaseLayout ).storeVersion();
			  SilentMigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();
			  StoreMigrator migrator = new StoreMigrator( _fs, pageCache, _config, logService, _jobScheduler );
			  CountsMigrator countsMigrator = new CountsMigrator( _fs, pageCache, _config );
			  DatabaseLayout migrationLayout = _directory.databaseLayout( StoreUpgrader.MIGRATION_DIRECTORY );
			  migrator.Migrate( databaseLayout, migrationLayout, progressMonitor.StartSection( "section" ), versionToMigrateFrom, upgradableDatabase.CurrentVersion() );
			  countsMigrator.Migrate( databaseLayout, migrationLayout, progressMonitor.StartSection( "section" ), versionToMigrateFrom, upgradableDatabase.CurrentVersion() );

			  // WHEN simulating resuming the migration
			  migrator = new StoreMigrator( _fs, pageCache, _config, logService, _jobScheduler );
			  countsMigrator = new CountsMigrator( _fs, pageCache, _config );
			  migrator.MoveMigratedFiles( migrationLayout, databaseLayout, versionToMigrateFrom, upgradableDatabase.CurrentVersion() );
			  countsMigrator.MoveMigratedFiles( migrationLayout, databaseLayout, versionToMigrateFrom, upgradableDatabase.CurrentVersion() );

			  // THEN starting the new store should be successful
			  StoreFactory storeFactory = new StoreFactory( databaseLayout, _config, new DefaultIdGeneratorFactory( _fs ), pageCache, _fs, logService.InternalLogProvider, EmptyVersionContextSupplier.EMPTY );
			  storeFactory.OpenAllNeoStores().close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToMigrateWithoutErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToMigrateWithoutErrors()
		 {
			  // GIVEN a legacy database
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File prepare = _directory.directory( "prepare" );
			  MigrationTestUtils.prepareSampleLegacyDatabase( Version, _fs, databaseLayout.DatabaseDirectory(), prepare );

			  AssertableLogProvider logProvider = new AssertableLogProvider( true );
			  LogService logService = new SimpleLogService( logProvider, logProvider );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );

			  LogTailScanner tailScanner = GetTailScanner( databaseLayout.DatabaseDirectory() );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache, tailScanner );

			  string versionToMigrateFrom = upgradableDatabase.CheckUpgradable( databaseLayout ).storeVersion();
			  SilentMigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();
			  StoreMigrator migrator = new StoreMigrator( _fs, pageCache, _config, logService, _jobScheduler );
			  CountsMigrator countsMigrator = new CountsMigrator( _fs, pageCache, _config );
			  DatabaseLayout migrationLayout = _directory.databaseLayout( StoreUpgrader.MIGRATION_DIRECTORY );

			  // WHEN migrating
			  migrator.Migrate( databaseLayout, migrationLayout, progressMonitor.StartSection( "section" ), versionToMigrateFrom, upgradableDatabase.CurrentVersion() );
			  countsMigrator.Migrate( databaseLayout, migrationLayout, progressMonitor.StartSection( "section" ), versionToMigrateFrom, upgradableDatabase.CurrentVersion() );
			  migrator.MoveMigratedFiles( migrationLayout, databaseLayout, versionToMigrateFrom, upgradableDatabase.CurrentVersion() );
			  countsMigrator.MoveMigratedFiles( migrationLayout, databaseLayout, versionToMigrateFrom, upgradableDatabase.CurrentVersion() );

			  // THEN starting the new store should be successful
			  StoreFactory storeFactory = new StoreFactory( databaseLayout, _config, new DefaultIdGeneratorFactory( _fs ), pageCache, _fs, logService.InternalLogProvider, EmptyVersionContextSupplier.EMPTY );
			  storeFactory.OpenAllNeoStores().close();
			  logProvider.RawMessageMatcher().assertNotContains("ERROR");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToResumeMigrationOnRebuildingCounts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToResumeMigrationOnRebuildingCounts()
		 {
			  // GIVEN a legacy database
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File prepare = _directory.directory( "prepare" );
			  MigrationTestUtils.prepareSampleLegacyDatabase( Version, _fs, databaseLayout.DatabaseDirectory(), prepare );
			  // and a state of the migration saying that it has done the actual migration
			  LogService logService = NullLogService.Instance;
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  LogTailScanner tailScanner = GetTailScanner( databaseLayout.DatabaseDirectory() );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache, tailScanner );

			  string versionToMigrateFrom = upgradableDatabase.CheckUpgradable( databaseLayout ).storeVersion();
			  SilentMigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();
			  StoreMigrator migrator = new StoreMigrator( _fs, pageCache, _config, logService, _jobScheduler );
			  DatabaseLayout migrationLayout = _directory.databaseLayout( StoreUpgrader.MIGRATION_DIRECTORY );
			  migrator.Migrate( databaseLayout, migrationLayout, progressMonitor.StartSection( "section" ), versionToMigrateFrom, upgradableDatabase.CurrentVersion() );

			  // WHEN simulating resuming the migration
			  progressMonitor = new SilentMigrationProgressMonitor();
			  CountsMigrator countsMigrator = new CountsMigrator( _fs, pageCache, _config );
			  countsMigrator.Migrate( databaseLayout, migrationLayout, progressMonitor.StartSection( "section" ), versionToMigrateFrom, upgradableDatabase.CurrentVersion() );
			  migrator.MoveMigratedFiles( migrationLayout, databaseLayout, versionToMigrateFrom, upgradableDatabase.CurrentVersion() );
			  countsMigrator.MoveMigratedFiles( migrationLayout, databaseLayout, versionToMigrateFrom, upgradableDatabase.CurrentVersion() );

			  // THEN starting the new store should be successful
			  StoreFactory storeFactory = new StoreFactory( databaseLayout, _config, new DefaultIdGeneratorFactory( _fs ), pageCache, _fs, logService.InternalLogProvider, EmptyVersionContextSupplier.EMPTY );
			  storeFactory.OpenAllNeoStores().close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeTheLastTxLogPositionCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComputeTheLastTxLogPositionCorrectly()
		 {
			  // GIVEN a legacy database
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File prepare = _directory.directory( "prepare" );
			  MigrationTestUtils.prepareSampleLegacyDatabase( Version, _fs, databaseLayout.DatabaseDirectory(), prepare );
			  // and a state of the migration saying that it has done the actual migration
			  LogService logService = NullLogService.Instance;
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  LogTailScanner tailScanner = GetTailScanner( databaseLayout.DatabaseDirectory() );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache, tailScanner );

			  string versionToMigrateFrom = upgradableDatabase.CheckUpgradable( databaseLayout ).storeVersion();
			  SilentMigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();
			  StoreMigrator migrator = new StoreMigrator( _fs, pageCache, _config, logService, _jobScheduler );
			  DatabaseLayout migrationLayout = _directory.databaseLayout( StoreUpgrader.MIGRATION_DIRECTORY );

			  // WHEN migrating
			  migrator.Migrate( databaseLayout, migrationLayout, progressMonitor.StartSection( "section" ), versionToMigrateFrom, upgradableDatabase.CurrentVersion() );

			  // THEN it should compute the correct last tx log position
			  assertEquals( ExpectedLogPosition, migrator.ReadLastTxLogPosition( migrationLayout ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeTheLastTxInfoCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComputeTheLastTxInfoCorrectly()
		 {
			  // given
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  File prepare = _directory.directory( "prepare" );
			  MigrationTestUtils.prepareSampleLegacyDatabase( Version, _fs, databaseLayout.DatabaseDirectory(), prepare );
			  // and a state of the migration saying that it has done the actual migration
			  LogService logService = NullLogService.Instance;
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  LogTailScanner tailScanner = GetTailScanner( databaseLayout.DatabaseDirectory() );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache, tailScanner );

			  string versionToMigrateFrom = upgradableDatabase.CheckUpgradable( databaseLayout ).storeVersion();
			  SilentMigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();
			  StoreMigrator migrator = new StoreMigrator( _fs, pageCache, _config, logService, _jobScheduler );
			  DatabaseLayout migrationLayout = _directory.databaseLayout( StoreUpgrader.MIGRATION_DIRECTORY );

			  // when
			  migrator.Migrate( databaseLayout, migrationLayout, progressMonitor.StartSection( "section" ), versionToMigrateFrom, upgradableDatabase.CurrentVersion() );

			  // then
			  assertTrue( TxIdComparator.apply( migrator.ReadLastTxInformation( migrationLayout ) ) );
		 }

		 private static UpgradableDatabase GetUpgradableDatabase( PageCache pageCache, LogTailScanner tailScanner )
		 {
			  return new UpgradableDatabase( new StoreVersionCheck( pageCache ), SelectFormat(), tailScanner );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.recovery.LogTailScanner getTailScanner(java.io.File databaseDirectory) throws java.io.IOException
		 private LogTailScanner GetTailScanner( File databaseDirectory )
		 {
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( databaseDirectory, _fs ).build();
			  return new LogTailScanner( logFiles, new VersionAwareLogEntryReader<Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>(), _monitors );
		 }

		 private static RecordFormats SelectFormat()
		 {
			  return Standard.LATEST_RECORD_FORMATS;
		 }

		 private static System.Func<TransactionId, bool> TxInfoAcceptanceOnIdAndTimestamp( long id, long timestamp )
		 {
			  return txInfo => txInfo.transactionId() == id && txInfo.commitTimestamp() == timestamp;
		 }
	}

}