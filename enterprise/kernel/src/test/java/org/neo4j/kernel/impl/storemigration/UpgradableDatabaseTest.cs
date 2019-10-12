using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.storemigration
{
	using Assume = org.junit.Assume;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Enclosed = org.junit.experimental.runners.Enclosed;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using StoreVersion = Org.Neo4j.Kernel.impl.store.format.StoreVersion;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using StandardFormatFamily = Org.Neo4j.Kernel.impl.store.format.standard.StandardFormatFamily;
	using StandardV2_3 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV2_3;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Version = Org.Neo4j.Kernel.@internal.Version;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogTailScanner = Org.Neo4j.Kernel.recovery.LogTailScanner;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.MigrationTestUtils.changeVersionNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.MigrationTestUtils.removeCheckPointFromTxLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.StoreUpgrader.UnexpectedUpgradingStoreVersionException.MESSAGE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Enclosed.class) public class UpgradableDatabaseTest
	public class UpgradableDatabaseTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class SupportedVersions
		 public class SupportedVersions
		 {
			 internal bool InstanceFieldsInitialized = false;

			 public SupportedVersions()
			 {
				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			 internal virtual void InitializeInstanceFields()
			 {
				 RuleChain = RuleChain.outerRule( TestDirectory ).around( FileSystemRule ).around( PageCacheRule );
			 }


			  internal readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
			  internal readonly PageCacheRule PageCacheRule = new PageCacheRule();
			  internal readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fileSystemRule).around(pageCacheRule);
			  public RuleChain RuleChain;

			  internal DatabaseLayout DatabaseLayout;
			  internal FileSystemAbstraction FileSystem;
			  internal LogTailScanner TailScanner;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String version;
			  public string Version;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<String> versions()
			  public static ICollection<string> Versions()
			  {
					return Collections.singletonList( StandardV2_3.STORE_VERSION );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void Setup()
			  {
					FileSystem = FileSystemRule.get();
					DatabaseLayout = TestDirectory.databaseLayout();
					MigrationTestUtils.FindFormatStoreDirectoryForVersion( Version, DatabaseLayout.databaseDirectory() );
					VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( DatabaseLayout.databaseDirectory(), FileSystem ).build();
					TailScanner = new LogTailScanner( logFiles, logEntryReader, new Monitors() );
			  }

			  internal virtual bool StoreFilesUpgradable( DatabaseLayout databaseLayout, UpgradableDatabase upgradableDatabase )
			  {
					try
					{
						 upgradableDatabase.CheckUpgradable( databaseLayout );
						 return true;
					}
					catch ( StoreUpgrader.UnableToUpgradeException )
					{
						 return false;
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptTheStoresInTheSampleDatabaseAsBeingEligibleForUpgrade()
			  public virtual void ShouldAcceptTheStoresInTheSampleDatabaseAsBeingEligibleForUpgrade()
			  {
					// given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UpgradableDatabase upgradableDatabase = getUpgradableDatabase();
					UpgradableDatabase upgradableDatabase = UpgradableDatabase;

					// when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = storeFilesUpgradable(databaseLayout, upgradableDatabase);
					bool result = StoreFilesUpgradable( DatabaseLayout, upgradableDatabase );

					// then
					assertTrue( result );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectOldVersionAsDifferentFromCurrent()
			  public virtual void ShouldDetectOldVersionAsDifferentFromCurrent()
			  {
					// given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UpgradableDatabase upgradableDatabase = getUpgradableDatabase();
					UpgradableDatabase upgradableDatabase = UpgradableDatabase;
					// when
					bool currentVersion = upgradableDatabase.HasCurrentVersion( DatabaseLayout );

					// then
					assertFalse( currentVersion );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectStoresIfDBIsNotShutdownCleanly() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldRejectStoresIfDBIsNotShutdownCleanly()
			  {
					// checkpoint has been introduced in 2.3
					Assume.assumeTrue( StandardV2_3.STORE_VERSION.Equals( Version ) );

					// given
					removeCheckPointFromTxLog( FileSystem, DatabaseLayout.databaseDirectory() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UpgradableDatabase upgradableDatabase = getUpgradableDatabase();
					UpgradableDatabase upgradableDatabase = UpgradableDatabase;

					// when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = storeFilesUpgradable(databaseLayout, upgradableDatabase);
					bool result = StoreFilesUpgradable( DatabaseLayout, upgradableDatabase );

					// then
					assertFalse( result );
			  }

			  internal virtual UpgradableDatabase UpgradableDatabase
			  {
				  get
				  {
						return new UpgradableDatabase( new StoreVersionCheck( PageCacheRule.getPageCache( FileSystem ) ), RecordFormat, TailScanner );
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class UnsupportedVersions
		 public class UnsupportedVersions
		 {
			 internal bool InstanceFieldsInitialized = false;

			 public UnsupportedVersions()
			 {
				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			 internal virtual void InitializeInstanceFields()
			 {
				 RuleChain = RuleChain.outerRule( TestDirectory ).around( FileSystemRule ).around( PageCacheRule );
			 }

			  internal const string NEOSTORE_FILENAME = "neostore";

			  internal readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
			  internal readonly PageCacheRule PageCacheRule = new PageCacheRule();
			  internal readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fileSystemRule).around(pageCacheRule);
			  public RuleChain RuleChain;

			  internal DatabaseLayout DatabaseLayout;
			  internal FileSystemAbstraction FileSystem;
			  internal LogTailScanner TailScanner;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String version;
			  public string Version;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<String> versions()
			  public static ICollection<string> Versions()
			  {
					return Arrays.asList( "v0.A.4", StoreVersion.HIGH_LIMIT_V3_0_0.versionString() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void Setup()
			  {
					FileSystem = FileSystemRule.get();
					DatabaseLayout = TestDirectory.databaseLayout();
					// doesn't matter which version we pick we are changing it to the wrong one...
					MigrationTestUtils.FindFormatStoreDirectoryForVersion( StandardV2_3.STORE_VERSION, DatabaseLayout.databaseDirectory() );
					changeVersionNumber( FileSystem, DatabaseLayout.file( NEOSTORE_FILENAME ), Version );
					File metadataStore = DatabaseLayout.metadataStore();
					PageCache pageCache = PageCacheRule.getPageCache( FileSystem );
					MetaDataStore.setRecord( pageCache, metadataStore, STORE_VERSION, MetaDataStore.versionStringToLong( Version ) );
					VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( DatabaseLayout.databaseDirectory(), FileSystem ).build();
					TailScanner = new LogTailScanner( logFiles, logEntryReader, new Monitors() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectOldVersionAsDifferentFromCurrent()
			  public virtual void ShouldDetectOldVersionAsDifferentFromCurrent()
			  {
					// given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UpgradableDatabase upgradableDatabase = getUpgradableDatabase();
					UpgradableDatabase upgradableDatabase = UpgradableDatabase;

					// when
					bool currentVersion = upgradableDatabase.HasCurrentVersion( DatabaseLayout );

					// then
					assertFalse( currentVersion );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommunicateWhatCausesInabilityToUpgrade()
			  public virtual void ShouldCommunicateWhatCausesInabilityToUpgrade()
			  {
					// given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UpgradableDatabase upgradableDatabase = getUpgradableDatabase();
					UpgradableDatabase upgradableDatabase = UpgradableDatabase;
					try
					{
						 // when
						 upgradableDatabase.CheckUpgradable( DatabaseLayout );
						 fail( "should not have been able to upgrade" );
					}
					catch ( StoreUpgrader.UnexpectedUpgradingStoreVersionException e )
					{
						 // then
						 assertEquals( string.format( MESSAGE, Version, upgradableDatabase.CurrentVersion(), Version.Neo4jVersion ), e.Message );
					}
					catch ( StoreUpgrader.UnexpectedUpgradingStoreFormatException e )
					{
						 // then
						 assertNotSame( StandardFormatFamily.INSTANCE, RecordFormatSelector.selectForVersion( Version ).FormatFamily );
						 assertEquals( string.format( StoreUpgrader.UnexpectedUpgradingStoreFormatException.MESSAGE, GraphDatabaseSettings.record_format.name() ), e.Message );
					}
			  }

			  internal virtual UpgradableDatabase UpgradableDatabase
			  {
				  get
				  {
						return new UpgradableDatabase( new StoreVersionCheck( PageCacheRule.getPageCache( FileSystem ) ), RecordFormat, TailScanner );
				  }
			  }
		 }

		 private static RecordFormats RecordFormat
		 {
			 get
			 {
				  return Standard.LATEST_RECORD_FORMATS;
			 }
		 }
	}

}