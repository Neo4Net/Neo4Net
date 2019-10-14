using System;
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
namespace Upgrade
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using MigrationTestUtils = Neo4Net.Kernel.impl.storemigration.MigrationTestUtils;
	using StoreUpgrader = Neo4Net.Kernel.impl.storemigration.StoreUpgrader;
	using StoreVersionCheck = Neo4Net.Kernel.impl.storemigration.StoreVersionCheck;
	using UpgradableDatabase = Neo4Net.Kernel.impl.storemigration.UpgradableDatabase;
	using MigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.MigrationProgressMonitor;
	using SilentMigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.SilentMigrationProgressMonitor;
	using SchemaIndexMigrator = Neo4Net.Kernel.impl.storemigration.participant.SchemaIndexMigrator;
	using StoreMigrator = Neo4Net.Kernel.impl.storemigration.participant.StoreMigrator;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.store.StoreAssertions.assertConsistentStore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.MigrationTestUtils.checkNeoStoreHasDefaultFormatVersion;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class StoreUpgraderInterruptionTestIT
	public class StoreUpgraderInterruptionTestIT
	{
		private bool InstanceFieldsInitialized = false;

		public StoreUpgraderInterruptionTestIT()
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
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public String version;
		 public string Version;
		 private static readonly Config _config = Config.defaults( GraphDatabaseSettings.pagecache_memory, "8m" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.Collection<String> versions()
		 public static ICollection<string> Versions()
		 {
			  return Collections.singletonList( StandardV2_3.STORE_VERSION );
		 }

		 private FileSystemAbstraction _fs;
		 private JobScheduler _jobScheduler;
		 private DatabaseLayout _workingDatabaseLayout;
		 private File _prepareDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpLabelScanStore()
		 public virtual void SetUpLabelScanStore()
		 {
			  _jobScheduler = new ThreadPoolJobScheduler();
			  _workingDatabaseLayout = _directory.databaseLayout();
			  _prepareDirectory = _directory.directory( "prepare" );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _jobScheduler.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedWithUpgradeAfterPreviousAttemptDiedDuringMigration() throws java.io.IOException, org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedWithUpgradeAfterPreviousAttemptDiedDuringMigration()
		 {
			  MigrationTestUtils.prepareSampleLegacyDatabase( Version, _fs, _workingDatabaseLayout.databaseDirectory(), _prepareDirectory );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  StoreVersionCheck check = new StoreVersionCheck( pageCache );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( check );
			  SilentMigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();
			  LogService logService = NullLogService.Instance;
			  StoreMigrator failingStoreMigrator = new StoreMigratorAnonymousInnerClass( this, _fs, pageCache, _config, logService, _jobScheduler );

			  try
			  {
					NewUpgrader( upgradableDatabase, pageCache, progressMonitor, CreateIndexMigrator(), failingStoreMigrator ).migrateIfNeeded(_workingDatabaseLayout);
					fail( "Should throw exception" );
			  }
			  catch ( Exception e )
			  {
					assertEquals( "This upgrade is failing", e.Message );
			  }

			  progressMonitor = new SilentMigrationProgressMonitor();
			  StoreMigrator migrator = new StoreMigrator( _fs, pageCache, _config, logService, _jobScheduler );
			  SchemaIndexMigrator indexMigrator = CreateIndexMigrator();
			  NewUpgrader( upgradableDatabase, pageCache, progressMonitor, indexMigrator, migrator ).migrateIfNeeded( _workingDatabaseLayout );

			  assertTrue( checkNeoStoreHasDefaultFormatVersion( check, _workingDatabaseLayout ) );

			  // Since consistency checker is in read only mode we need to start/stop db to generate label scan store.
			  StartStopDatabase( _workingDatabaseLayout.databaseDirectory() );
			  assertConsistentStore( _workingDatabaseLayout );
		 }

		 private class StoreMigratorAnonymousInnerClass : StoreMigrator
		 {
			 private readonly StoreUpgraderInterruptionTestIT _outerInstance;

			 public StoreMigratorAnonymousInnerClass( StoreUpgraderInterruptionTestIT outerInstance, FileSystemAbstraction fs, PageCache pageCache, Config config, LogService logService, JobScheduler jobScheduler ) : base( fs, pageCache, config, logService, jobScheduler )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void migrate(org.neo4j.io.layout.DatabaseLayout directoryLayout, org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.kernel.impl.util.monitoring.ProgressReporter progressReporter, String versionToMigrateFrom, String versionToMigrateTo) throws java.io.IOException
			 public override void migrate( DatabaseLayout directoryLayout, DatabaseLayout migrationLayout, ProgressReporter progressReporter, string versionToMigrateFrom, string versionToMigrateTo )
			 {
				  base.migrate( directoryLayout, migrationLayout, progressReporter, versionToMigrateFrom, versionToMigrateTo );
				  throw new Exception( "This upgrade is failing" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.storemigration.UpgradableDatabase getUpgradableDatabase(org.neo4j.kernel.impl.storemigration.StoreVersionCheck check) throws java.io.IOException
		 private UpgradableDatabase GetUpgradableDatabase( StoreVersionCheck check )
		 {
			  VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _workingDatabaseLayout.databaseDirectory(), _fs ).build();
			  LogTailScanner tailScanner = new LogTailScanner( logFiles, logEntryReader, new Monitors() );
			  return new UpgradableDatabase( check, Standard.LATEST_RECORD_FORMATS, tailScanner );
		 }

		 private SchemaIndexMigrator CreateIndexMigrator()
		 {
			  return new SchemaIndexMigrator( _fs, IndexProvider.EMPTY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedWithUpgradeAfterPreviousAttemptDiedDuringMovingFiles() throws java.io.IOException, org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedWithUpgradeAfterPreviousAttemptDiedDuringMovingFiles()
		 {
			  MigrationTestUtils.prepareSampleLegacyDatabase( Version, _fs, _workingDatabaseLayout.databaseDirectory(), _prepareDirectory );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  StoreVersionCheck check = new StoreVersionCheck( pageCache );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( check );
			  SilentMigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();
			  LogService logService = NullLogService.Instance;
			  StoreMigrator failingStoreMigrator = new StoreMigratorAnonymousInnerClass2( this, _fs, pageCache, _config, logService, _jobScheduler );

			  try
			  {
					NewUpgrader( upgradableDatabase, pageCache, progressMonitor, CreateIndexMigrator(), failingStoreMigrator ).migrateIfNeeded(_workingDatabaseLayout);
					fail( "Should throw exception" );
			  }
			  catch ( Exception e )
			  {
					assertEquals( "This upgrade is failing", e.Message );
			  }

			  assertTrue( checkNeoStoreHasDefaultFormatVersion( check, _workingDatabaseLayout ) );

			  progressMonitor = new SilentMigrationProgressMonitor();
			  StoreMigrator migrator = new StoreMigrator( _fs, pageCache, _config, logService, _jobScheduler );
			  NewUpgrader( upgradableDatabase, pageCache, progressMonitor, CreateIndexMigrator(), migrator ).migrateIfNeeded(_workingDatabaseLayout);

			  assertTrue( checkNeoStoreHasDefaultFormatVersion( check, _workingDatabaseLayout ) );

			  pageCache.Close();

			  // Since consistency checker is in read only mode we need to start/stop db to generate label scan store.
			  StartStopDatabase( _workingDatabaseLayout.databaseDirectory() );
			  assertConsistentStore( _workingDatabaseLayout );
		 }

		 private class StoreMigratorAnonymousInnerClass2 : StoreMigrator
		 {
			 private readonly StoreUpgraderInterruptionTestIT _outerInstance;

			 public StoreMigratorAnonymousInnerClass2( StoreUpgraderInterruptionTestIT outerInstance, FileSystemAbstraction fs, PageCache pageCache, Config config, LogService logService, JobScheduler jobScheduler ) : base( fs, pageCache, config, logService, jobScheduler )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveMigratedFiles(org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.io.layout.DatabaseLayout directoryLayout, String versionToUpgradeFrom, String versionToMigrateTo) throws java.io.IOException
			 public override void moveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToUpgradeFrom, string versionToMigrateTo )
			 {
				  base.moveMigratedFiles( migrationLayout, directoryLayout, versionToUpgradeFrom, versionToMigrateTo );
				  throw new Exception( "This upgrade is failing" );
			 }
		 }

		 private StoreUpgrader NewUpgrader( UpgradableDatabase upgradableDatabase, PageCache pageCache, MigrationProgressMonitor progressMonitor, SchemaIndexMigrator indexMigrator, StoreMigrator migrator )
		 {
			  Config allowUpgrade = Config.defaults( GraphDatabaseSettings.allow_upgrade, "true" );

			  StoreUpgrader upgrader = new StoreUpgrader( upgradableDatabase, progressMonitor, allowUpgrade, _fs, pageCache, NullLogProvider.Instance );
			  upgrader.AddParticipant( indexMigrator );
			  upgrader.AddParticipant( migrator );
			  return upgrader;
		 }

		 private static void StartStopDatabase( File storeDir )
		 {
			  GraphDatabaseService databaseService = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.allow_upgrade, "true").newGraphDatabase();
			  databaseService.Shutdown();
		 }
	}

}