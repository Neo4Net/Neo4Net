﻿using System;

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
namespace Neo4Net.Kernel
{
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using OnlineBackup = Neo4Net.backup.OnlineBackup;
	using AdminTool = Neo4Net.CommandLine.Admin.AdminTool;
	using BlockerLocator = Neo4Net.CommandLine.Admin.BlockerLocator;
	using CommandLocator = Neo4Net.CommandLine.Admin.CommandLocator;
	using RealOutsideWorld = Neo4Net.CommandLine.Admin.RealOutsideWorld;
	using UdcSettings = Neo4Net.Ext.Udc.UdcSettings;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using MetricsSettings = Neo4Net.metrics.MetricsSettings;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Neo4Net.Test.rule.EnterpriseDatabaseRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.metricsCsv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.MetricsTestHelper.readLongValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.metrics.source.db.PageCacheMetrics.PC_PAGE_FAULTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class PageCacheWarmupEnterpriseEditionIT : PageCacheWarmupTestSupport
	{
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.EnterpriseDatabaseRule db = new org.neo4j.test.rule.EnterpriseDatabaseRule(testDirectory)
		 public final EnterpriseDatabaseRule db = new EnterpriseDatabaseRuleAnonymousInnerClass( TestDirectory )
		 .startLazily();

		 private static void verifyEventuallyWarmsUp( long pagesInMemory, File metricsDirectory ) throws Exception
		 {
			  assertEventually( "Metrics report should include page cache page faults", () => readLongValue(metricsCsv(metricsDirectory, PC_PAGE_FAULTS)), greaterThanOrEqualTo(pagesInMemory), 20, SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void warmupMustReloadHotPagesAfterRestartAndFaultsMustBeVisibleViaMetrics() throws Exception
		 public void warmupMustReloadHotPagesAfterRestartAndFaultsMustBeVisibleViaMetrics() throws Exception
		 {
			  File metricsDirectory = TestDirectory.directory( "metrics" );
			  Db.withSetting( MetricsSettings.metricsEnabled, Settings.FALSE ).withSetting( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ).withSetting( GraphDatabaseSettings.pagecache_warmup_profiling_interval, "100ms" );
			  Db.ensureStarted();

			  CreateTestData( db );
			  long pagesInMemory = WaitForCacheProfile( Db.Monitors );

			  Db.restartDatabase( MetricsSettings.neoPageCacheEnabled.name(), Settings.TRUE, MetricsSettings.csvEnabled.name(), Settings.TRUE, MetricsSettings.csvInterval.name(), "100ms", MetricsSettings.csvPath.name(), metricsDirectory.AbsolutePath );

			  VerifyEventuallyWarmsUp( pagesInMemory, metricsDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cacheProfilesMustBeIncludedInOnlineBackups() throws Exception
		 public void cacheProfilesMustBeIncludedInOnlineBackups() throws Exception
		 {
			  int backupPort = PortAuthority.allocatePort();
			  Db.withSetting( MetricsSettings.metricsEnabled, Settings.FALSE ).withSetting( UdcSettings.udc_enabled, Settings.FALSE ).withSetting( OnlineBackupSettings.online_backup_enabled, Settings.TRUE ).withSetting( OnlineBackupSettings.online_backup_server, "localhost:" + backupPort ).withSetting( GraphDatabaseSettings.pagecache_warmup_profiling_interval, "100ms" );
			  Db.ensureStarted();

			  CreateTestData( db );
			  long pagesInMemory = WaitForCacheProfile( Db.Monitors );

			  BinaryLatch latch = PauseProfile( Db.Monitors ); // We don't want torn profile files in this test.

			  File metricsDirectory = TestDirectory.cleanDirectory( "metrics" );
			  File backupDir = TestDirectory.cleanDirectory( "backup" );
			  assertTrue( OnlineBackup.from( "localhost", backupPort ).backup( backupDir ).Consistent );
			  latch.Release();
			  DatabaseRule.RestartAction useBackupDir = ( fs, storeDir ) =>
			  {
				fs.deleteRecursively( storeDir.databaseDirectory() );
				fs.copyRecursively( backupDir, storeDir.databaseDirectory() );
			  };
			  Db.restartDatabase( useBackupDir, OnlineBackupSettings.online_backup_enabled.name(), Settings.FALSE, MetricsSettings.neoPageCacheEnabled.name(), Settings.TRUE, MetricsSettings.csvEnabled.name(), Settings.TRUE, MetricsSettings.csvInterval.name(), "100ms", MetricsSettings.csvPath.name(), metricsDirectory.AbsolutePath );

			  VerifyEventuallyWarmsUp( pagesInMemory, metricsDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cacheProfilesMustNotInterfereWithOnlineBackups() throws Exception
		 public void cacheProfilesMustNotInterfereWithOnlineBackups() throws Exception
		 {
			  // Here we are testing that the file modifications done by the page cache profiler,
			  // does not make online backup throw any exceptions.
			  int backupPort = PortAuthority.allocatePort();
			  Db.withSetting( MetricsSettings.metricsEnabled, Settings.FALSE ).withSetting( OnlineBackupSettings.online_backup_enabled, Settings.TRUE ).withSetting( OnlineBackupSettings.online_backup_server, "localhost:" + backupPort ).withSetting( GraphDatabaseSettings.pagecache_warmup_profiling_interval, "1ms" );
			  Db.ensureStarted();

			  CreateTestData( db );
			  WaitForCacheProfile( Db.Monitors );

			  for ( int i = 0; i < 20; i++ )
			  {
					string backupDir = TestDirectory.cleanDirectory( "backup" ).AbsolutePath;
					assertTrue( OnlineBackup.from( "localhost", backupPort ).full( backupDir ).Consistent );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cacheProfilesMustBeIncludedInOfflineBackups() throws Exception
		 public void cacheProfilesMustBeIncludedInOfflineBackups() throws Exception
		 {
			  Db.withSetting( MetricsSettings.metricsEnabled, Settings.FALSE ).withSetting( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ).withSetting( GraphDatabaseSettings.pagecache_warmup_profiling_interval, "100ms" );
			  Db.ensureStarted();
			  CreateTestData( db );
			  long pagesInMemory = WaitForCacheProfile( Db.Monitors );

			  Db.shutdownAndKeepStore();

			  AdminTool adminTool = new AdminTool(CommandLocator.fromServiceLocator(), BlockerLocator.fromServiceLocator(), new RealOutsideWorldAnonymousInnerClass(this)
						, true);
			  File databaseDir = Db.databaseLayout().databaseDirectory();
			  File data = TestDirectory.cleanDirectory( "data" );
			  File databases = new File( data, "databases" );
			  File graphdb = TestDirectory.databaseDir( databases );
			  FileUtils.copyRecursively( databaseDir, graphdb );
			  FileUtils.deleteRecursively( databaseDir );
			  Path homePath = data.toPath().Parent;
			  File dumpDir = TestDirectory.cleanDirectory( "dump-dir" );
			  adminTool.Execute( homePath, homePath, "dump", "--database=" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME, "--to=" + dumpDir );

			  FileUtils.deleteRecursively( graphdb );
			  File dumpFile = new File( dumpDir, "graph.db.dump" );
			  adminTool.Execute( homePath, homePath, "load", "--database=" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME, "--from=" + dumpFile );
			  FileUtils.copyRecursively( graphdb, databaseDir );
			  FileUtils.deleteRecursively( graphdb );

			  File metricsDirectory = TestDirectory.cleanDirectory( "metrics" );
			  Db.withSetting( MetricsSettings.neoPageCacheEnabled, Settings.TRUE ).withSetting( MetricsSettings.csvEnabled, Settings.TRUE ).withSetting( MetricsSettings.csvInterval, "100ms" ).withSetting( MetricsSettings.csvPath, metricsDirectory.AbsolutePath );
			  Db.ensureStarted();

			  VerifyEventuallyWarmsUp( pagesInMemory, metricsDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logPageCacheWarmupStartCompletionMessages() throws Exception
		 public void logPageCacheWarmupStartCompletionMessages() throws Exception
		 {
			  File metricsDirectory = TestDirectory.directory( "metrics" );
			  Db.withSetting( MetricsSettings.metricsEnabled, Settings.FALSE ).withSetting( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ).withSetting( GraphDatabaseSettings.pagecache_warmup_profiling_interval, "100ms" );
			  Db.ensureStarted();

			  CreateTestData( db );
			  long pagesInMemory = WaitForCacheProfile( Db.Monitors );

			  Db.restartDatabase( MetricsSettings.neoPageCacheEnabled.name(), Settings.TRUE, MetricsSettings.csvEnabled.name(), Settings.TRUE, MetricsSettings.csvInterval.name(), "100ms", MetricsSettings.csvPath.name(), metricsDirectory.AbsolutePath );

			  VerifyEventuallyWarmsUp( pagesInMemory, metricsDirectory );

			  _logProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Page cache warmup started.")));

			  _logProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Page cache warmup completed. %d pages loaded. Duration: %s.")));
		 }
	}

}