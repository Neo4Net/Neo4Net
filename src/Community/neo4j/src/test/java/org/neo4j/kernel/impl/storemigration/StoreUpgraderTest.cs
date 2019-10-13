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
namespace Neo4Net.Kernel.impl.storemigration
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Mockito = org.mockito.Mockito;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using UnableToUpgradeException = Neo4Net.Kernel.impl.storemigration.StoreUpgrader.UnableToUpgradeException;
	using MigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.MigrationProgressMonitor;
	using SilentMigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.SilentMigrationProgressMonitor;
	using VisibleMigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.VisibleMigrationProgressMonitor;
	using AbstractStoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.participant.AbstractStoreMigrationParticipant;
	using CountsMigrator = Neo4Net.Kernel.impl.storemigration.participant.CountsMigrator;
	using SchemaIndexMigrator = Neo4Net.Kernel.impl.storemigration.participant.SchemaIndexMigrator;
	using StoreMigrator = Neo4Net.Kernel.impl.storemigration.participant.StoreMigrator;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.emptyCollectionOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.MigrationTestUtils.removeCheckPointFromTxLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.MigrationTestUtils.verifyFilesHaveSameContent;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class StoreUpgraderTest
	public class StoreUpgraderTest
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _expectedException ).around( _fileSystemRule ).around( _pageCacheRule ).around( _directory );
			_fileSystem = _fileSystemRule.get();
		}

		 private const string INTERNAL_LOG_FILE = "debug.log";
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly ExpectedException _expectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(expectedException).around(fileSystemRule).around(pageCacheRule).around(directory);
		 public RuleChain RuleChain;

		 private DatabaseLayout _databaseLayout;
		 private FileSystemAbstraction _fileSystem;
		 private JobScheduler _jobScheduler;
		 private readonly RecordFormats _formats;

		 private readonly Config _allowMigrateConfig = Config.defaults( GraphDatabaseSettings.allow_upgrade, Settings.TRUE );

		 public StoreUpgraderTest( RecordFormats formats )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._formats = formats;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<org.neo4j.kernel.impl.store.format.RecordFormats> versions()
		 public static ICollection<RecordFormats> Versions()
		 {
			  return Collections.singletonList( StandardV2_3.RECORD_FORMATS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepareDb() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PrepareDb()
		 {
			  _jobScheduler = new ThreadPoolJobScheduler();
			  string version = _formats.storeVersion();
			  _databaseLayout = _directory.databaseLayout( "db_" + version );
			  File prepareDirectory = _directory.directory( "prepare_" + version );
			  PrepareSampleDatabase( version, _fileSystem, _databaseLayout, prepareDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _jobScheduler.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaltUpgradeIfUpgradeConfigurationVetoesTheProcess() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaltUpgradeIfUpgradeConfigurationVetoesTheProcess()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  Config deniedMigrationConfig = Config.defaults( GraphDatabaseSettings.allow_upgrade, "false" );
			  deniedMigrationConfig.Augment( GraphDatabaseSettings.record_format, Standard.LATEST_NAME );

			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );

			  try
			  {
					NewUpgrader( upgradableDatabase, deniedMigrationConfig, pageCache ).migrateIfNeeded( _databaseLayout );
					fail( "Should throw exception" );
			  }
			  catch ( UpgradeNotAllowedByConfigurationException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRefuseToUpgradeIfAnyOfTheStoresWereNotShutDownCleanly() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRefuseToUpgradeIfAnyOfTheStoresWereNotShutDownCleanly()
		 {
			  File comparisonDirectory = _directory.directory( "shouldRefuseToUpgradeIfAnyOfTheStoresWereNotShutDownCleanly-comparison" );
			  removeCheckPointFromTxLog( _fileSystem, _databaseLayout.databaseDirectory() );
			  _fileSystem.deleteRecursively( comparisonDirectory );
			  _fileSystem.copyRecursively( _databaseLayout.databaseDirectory(), comparisonDirectory );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );

			  try
			  {
					NewUpgrader( upgradableDatabase, pageCache ).migrateIfNeeded( _databaseLayout );
					fail( "Should throw exception" );
			  }
			  catch ( StoreUpgrader.UnableToUpgradeException )
			  {
					// expected
			  }

			  verifyFilesHaveSameContent( _fileSystem, comparisonDirectory, _databaseLayout.databaseDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRefuseToUpgradeIfAllOfTheStoresWereNotShutDownCleanly() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRefuseToUpgradeIfAllOfTheStoresWereNotShutDownCleanly()
		 {
			  File comparisonDirectory = _directory.directory( "shouldRefuseToUpgradeIfAllOfTheStoresWereNotShutDownCleanly-comparison" );
			  removeCheckPointFromTxLog( _fileSystem, _databaseLayout.databaseDirectory() );
			  _fileSystem.deleteRecursively( comparisonDirectory );
			  _fileSystem.copyRecursively( _databaseLayout.databaseDirectory(), comparisonDirectory );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );

			  try
			  {
					NewUpgrader( upgradableDatabase, pageCache ).migrateIfNeeded( _databaseLayout );
					fail( "Should throw exception" );
			  }
			  catch ( StoreUpgrader.UnableToUpgradeException )
			  {
					// expected
			  }

			  verifyFilesHaveSameContent( _fileSystem, comparisonDirectory, _databaseLayout.databaseDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContinueMovingFilesIfUpgradeCancelledWhileMoving() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContinueMovingFilesIfUpgradeCancelledWhileMoving()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );

			  string versionToMigrateTo = upgradableDatabase.CurrentVersion();
			  string versionToMigrateFrom = upgradableDatabase.CheckUpgradable( _databaseLayout ).storeVersion();

			  {
			  // GIVEN
					StoreUpgrader upgrader = NewUpgrader( upgradableDatabase, _allowMigrateConfig, pageCache );
					string failureMessage = "Just failing";
					upgrader.AddParticipant( ParticipantThatWillFailWhenMoving( failureMessage ) );

					// WHEN
					try
					{
						 upgrader.MigrateIfNeeded( _databaseLayout );
						 fail( "should have thrown" );
					}
					catch ( UnableToUpgradeException e )
					{ // THEN
						 assertTrue( e.InnerException is IOException );
						 assertEquals( failureMessage, e.InnerException.Message );
					}
			  }

			  {
			  // AND WHEN

					StoreUpgrader upgrader = NewUpgrader( upgradableDatabase, pageCache );
					StoreMigrationParticipant observingParticipant = Mockito.mock( typeof( StoreMigrationParticipant ) );
					upgrader.AddParticipant( observingParticipant );
					upgrader.MigrateIfNeeded( _databaseLayout );

					// THEN
					verify( observingParticipant, Mockito.never() ).migrate(any(typeof(DatabaseLayout)), any(typeof(DatabaseLayout)), any(typeof(ProgressReporter)), eq(versionToMigrateFrom), eq(versionToMigrateTo));
					verify( observingParticipant, Mockito.times( 1 ) ).moveMigratedFiles( any( typeof( DatabaseLayout ) ), any( typeof( DatabaseLayout ) ), eq( versionToMigrateFrom ), eq( versionToMigrateTo ) );

					verify( observingParticipant, Mockito.times( 1 ) ).cleanup( any( typeof( DatabaseLayout ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void upgradedNeoStoreShouldHaveNewUpgradeTimeAndUpgradeId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpgradedNeoStoreShouldHaveNewUpgradeTimeAndUpgradeId()
		 {
			  // Given
			  _fileSystem.deleteFile( _databaseLayout.file( INTERNAL_LOG_FILE ) );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );

			  // When
			  NewUpgrader( upgradableDatabase, _allowMigrateConfig, pageCache ).migrateIfNeeded( _databaseLayout );

			  // Then
			  StoreFactory factory = new StoreFactory( _databaseLayout, _allowMigrateConfig, new DefaultIdGeneratorFactory( _fileSystem ), pageCache, _fileSystem, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  using ( NeoStores neoStores = factory.OpenAllNeoStores() )
			  {
					assertThat( neoStores.MetaDataStore.UpgradeTransaction, equalTo( neoStores.MetaDataStore.LastCommittedTransaction ) );
					assertThat( neoStores.MetaDataStore.UpgradeTime, not( equalTo( MetaDataStore.FIELD_NOT_INITIALIZED ) ) );

					long minuteAgo = DateTimeHelper.CurrentUnixTimeMillis() - MINUTES.toMillis(1);
					assertThat( neoStores.MetaDataStore.UpgradeTime, greaterThan( minuteAgo ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void upgradeShouldNotLeaveLeftoverAndMigrationDirs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpgradeShouldNotLeaveLeftoverAndMigrationDirs()
		 {
			  // Given
			  _fileSystem.deleteFile( _databaseLayout.file( INTERNAL_LOG_FILE ) );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );

			  // When
			  NewUpgrader( upgradableDatabase, _allowMigrateConfig, pageCache ).migrateIfNeeded( _databaseLayout );

			  // Then
			  assertThat( MigrationHelperDirs(), @is(emptyCollectionOf(typeof(File))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void upgradeShouldGiveProgressMonitorProgressMessages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpgradeShouldGiveProgressMonitorProgressMessages()
		 {
			  // Given
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );

			  // When
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  NewUpgrader( upgradableDatabase, pageCache, _allowMigrateConfig, new VisibleMigrationProgressMonitor( logProvider.GetLog( "test" ) ) ).migrateIfNeeded( _databaseLayout );

			  // Then
			  AssertableLogProvider.MessageMatcher messageMatcher = logProvider.RawMessageMatcher();
			  messageMatcher.AssertContains( "Store files" );
			  messageMatcher.AssertContains( "Indexes" );
			  messageMatcher.AssertContains( "Counts store" );
			  messageMatcher.AssertContains( "Successfully finished" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void upgraderShouldCleanupLegacyLeftoverAndMigrationDirs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpgraderShouldCleanupLegacyLeftoverAndMigrationDirs()
		 {
			  // Given
			  _fileSystem.deleteFile( _databaseLayout.file( INTERNAL_LOG_FILE ) );
			  _fileSystem.mkdir( _databaseLayout.file( StoreUpgrader.MIGRATION_DIRECTORY ) );
			  _fileSystem.mkdir( _databaseLayout.file( StoreUpgrader.MIGRATION_LEFT_OVERS_DIRECTORY ) );
			  _fileSystem.mkdir( _databaseLayout.file( StoreUpgrader.MIGRATION_LEFT_OVERS_DIRECTORY + "_1" ) );
			  _fileSystem.mkdir( _databaseLayout.file( StoreUpgrader.MIGRATION_LEFT_OVERS_DIRECTORY + "_2" ) );
			  _fileSystem.mkdir( _databaseLayout.file( StoreUpgrader.MIGRATION_LEFT_OVERS_DIRECTORY + "_42" ) );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );

			  // When
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );
			  StoreUpgrader storeUpgrader = NewUpgrader( upgradableDatabase, pageCache );
			  storeUpgrader.MigrateIfNeeded( _databaseLayout );

			  // Then
			  assertThat( MigrationHelperDirs(), @is(emptyCollectionOf(typeof(File))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notParticipatingParticipantsAreNotPartOfMigration() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NotParticipatingParticipantsAreNotPartOfMigration()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  UpgradableDatabase upgradableDatabase = GetUpgradableDatabase( pageCache );
			  StoreUpgrader storeUpgrader = NewUpgrader( upgradableDatabase, pageCache );
			  assertThat( storeUpgrader.Participants, hasSize( 3 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void prepareSampleDatabase(String version, org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.layout.DatabaseLayout databaseLayout, java.io.File databaseDirectory) throws java.io.IOException
		 protected internal virtual void PrepareSampleDatabase( string version, FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout, File databaseDirectory )
		 {
			  MigrationTestUtils.PrepareSampleLegacyDatabase( version, fileSystem, databaseLayout.DatabaseDirectory(), databaseDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private UpgradableDatabase getUpgradableDatabase(org.neo4j.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private UpgradableDatabase GetUpgradableDatabase( PageCache pageCache )
		 {
			  VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> entryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _databaseLayout.databaseDirectory(), _fileSystem ).withLogEntryReader(entryReader).build();
			  LogTailScanner tailScanner = new LogTailScanner( logFiles, entryReader, new Monitors() );
			  return new UpgradableDatabase( new StoreVersionCheck( pageCache ), RecordFormats, tailScanner );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private StoreMigrationParticipant participantThatWillFailWhenMoving(final String failureMessage)
		 private StoreMigrationParticipant ParticipantThatWillFailWhenMoving( string failureMessage )
		 {
			  return new AbstractStoreMigrationParticipantAnonymousInnerClass( this, failureMessage );
		 }

		 private class AbstractStoreMigrationParticipantAnonymousInnerClass : AbstractStoreMigrationParticipant
		 {
			 private readonly StoreUpgraderTest _outerInstance;

			 private string _failureMessage;

			 public AbstractStoreMigrationParticipantAnonymousInnerClass( StoreUpgraderTest outerInstance, string failureMessage ) : base( "Failing" )
			 {
				 this.outerInstance = outerInstance;
				 this._failureMessage = failureMessage;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveMigratedFiles(org.neo4j.io.layout.DatabaseLayout migrationLayout, org.neo4j.io.layout.DatabaseLayout directoryLayout, String versionToUpgradeFrom, String versionToMigrateTo) throws java.io.IOException
			 public override void moveMigratedFiles( DatabaseLayout migrationLayout, DatabaseLayout directoryLayout, string versionToUpgradeFrom, string versionToMigrateTo )
			 {
				  throw new IOException( _failureMessage );
			 }
		 }

		 private StoreUpgrader NewUpgrader( UpgradableDatabase upgradableDatabase, Config config, PageCache pageCache )
		 {
			  return NewUpgrader( upgradableDatabase, pageCache, config );
		 }

		 private StoreUpgrader NewUpgrader( UpgradableDatabase upgradableDatabase, PageCache pageCache )
		 {
			  return NewUpgrader( upgradableDatabase, pageCache, _allowMigrateConfig );
		 }

		 private StoreUpgrader NewUpgrader( UpgradableDatabase upgradableDatabase, PageCache pageCache, Config config )
		 {
			  SilentMigrationProgressMonitor progressMonitor = new SilentMigrationProgressMonitor();

			  return NewUpgrader( upgradableDatabase, pageCache, config, progressMonitor );
		 }

		 private StoreUpgrader NewUpgrader( UpgradableDatabase upgradableDatabase, PageCache pageCache, Config config, MigrationProgressMonitor progressMonitor )
		 {
			  NullLogService instance = NullLogService.Instance;
			  StoreMigrator defaultMigrator = new StoreMigrator( _fileSystem, pageCache, TuningConfig, instance, _jobScheduler );
			  CountsMigrator countsMigrator = new CountsMigrator( _fileSystem, pageCache, TuningConfig );
			  SchemaIndexMigrator indexMigrator = new SchemaIndexMigrator( _fileSystem, IndexProvider.EMPTY );

			  StoreUpgrader upgrader = new StoreUpgrader( upgradableDatabase, progressMonitor, config, _fileSystem, pageCache, NullLogProvider.Instance );
			  upgrader.AddParticipant( indexMigrator );
			  upgrader.AddParticipant( AbstractStoreMigrationParticipant.NOT_PARTICIPATING );
			  upgrader.AddParticipant( AbstractStoreMigrationParticipant.NOT_PARTICIPATING );
			  upgrader.AddParticipant( AbstractStoreMigrationParticipant.NOT_PARTICIPATING );
			  upgrader.AddParticipant( AbstractStoreMigrationParticipant.NOT_PARTICIPATING );
			  upgrader.AddParticipant( defaultMigrator );
			  upgrader.AddParticipant( countsMigrator );
			  return upgrader;
		 }

		 private IList<File> MigrationHelperDirs()
		 {
			  File[] tmpDirs = _databaseLayout.listDatabaseFiles( ( file, name ) => file.Directory && ( name.Equals( StoreUpgrader.MIGRATION_DIRECTORY ) || name.StartsWith( StoreUpgrader.MIGRATION_LEFT_OVERS_DIRECTORY ) ) );
			  assertNotNull( "Some IO errors occurred", tmpDirs );
			  return Arrays.asList( tmpDirs );
		 }

		 private Config TuningConfig
		 {
			 get
			 {
				  return Config.defaults( GraphDatabaseSettings.record_format, RecordFormatsName );
			 }
		 }

		 protected internal virtual RecordFormats RecordFormats
		 {
			 get
			 {
				  return Standard.LATEST_RECORD_FORMATS;
			 }
		 }

		 protected internal virtual string RecordFormatsName
		 {
			 get
			 {
				  return Standard.LATEST_NAME;
			 }
		 }
	}

}