using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.storemigration.participant
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using StoreVersion = Neo4Net.Kernel.impl.store.format.StoreVersion;
	using HighLimitV3_0_0 = Neo4Net.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using Result = Neo4Net.Kernel.impl.storemigration.StoreVersionCheck.Result;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.tracing.PageCacheTracer.NULL;

	public class StoreMigratorTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreMigratorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = org.junit.rules.RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _pageCacheRule ).around( _random );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory _directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDoActualStoreMigrationBetween3_0_5_and_next() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDoActualStoreMigrationBetween3_0_5AndNext()
		 {
			  // GIVEN a store in vE.H.0 format
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(databaseLayout.DatabaseDirectory()).setConfig(GraphDatabaseSettings.record_format, HighLimitV3_0_0.NAME).newGraphDatabase().shutdown();
			  Config config = Config.defaults( pagecache_memory, "8m" );

			  try (FileSystemAbstraction fs = new DefaultFileSystemAbstraction(); IJobScheduler _jobScheduler = new ThreadPoolJobScheduler(); PageCache _pageCache = new ConfiguringPageCacheFactory(fs, config, NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, NullLog.Instance, EmptyVersionContextSupplier.EMPTY, _jobScheduler)
								.OrCreatePageCache)
								{
					// For test code sanity
					string fromStoreVersion = StoreVersion.HIGH_LIMIT_V3_0_0.versionString();
					StoreVersionCheck.Result hasVersionResult = ( new StoreVersionCheck( _pageCache ) ).hasVersion( databaseLayout.MetadataStore(), fromStoreVersion );
					assertTrue( hasVersionResult.ActualVersion, hasVersionResult.Outcome.Successful );

					// WHEN
					StoreMigrator migrator = new StoreMigrator( fs, _pageCache, config, NullLogService.Instance, _jobScheduler );
					ProgressReporter monitor = mock( typeof( ProgressReporter ) );
					DatabaseLayout migrationLayout = _directory.databaseLayout( "migration" );
					migrator.Migrate( _directory.databaseLayout(), migrationLayout, monitor, fromStoreVersion, StoreVersion.HIGH_LIMIT_V3_0_6.versionString() );

					// THEN
					verifyNoMoreInteractions( monitor );
								}
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detectObsoleteCountStoresToRebuildDuringMigration()
		 public virtual void DetectObsoleteCountStoresToRebuildDuringMigration()
		 {
			  FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction();
			  PageCache pageCache = mock( typeof( PageCache ) );
			  Config config = Config.defaults();
			  CountsMigrator storeMigrator = new CountsMigrator( fileSystem, pageCache, config );
			  ISet<string> actualVersions = new HashSet<string>();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<string> expectedVersions = java.util.org.Neo4Net.kernel.impl.store.format.StoreVersion.values().Select(StoreVersion::versionString).collect(Collectors.toSet());

			  assertTrue( CountsMigrator.CountStoreRebuildRequired( StoreVersion.STANDARD_V2_3.versionString() ) );
			  actualVersions.Add( StoreVersion.STANDARD_V2_3.versionString() );
			  assertTrue( CountsMigrator.CountStoreRebuildRequired( StoreVersion.STANDARD_V3_0.versionString() ) );
			  actualVersions.Add( StoreVersion.STANDARD_V3_0.versionString() );
			  assertFalse( CountsMigrator.CountStoreRebuildRequired( StoreVersion.STANDARD_V3_2.versionString() ) );
			  actualVersions.Add( StoreVersion.STANDARD_V3_2.versionString() );
			  assertFalse( CountsMigrator.CountStoreRebuildRequired( StoreVersion.STANDARD_V3_4.versionString() ) );
			  actualVersions.Add( StoreVersion.STANDARD_V3_4.versionString() );

			  assertTrue( CountsMigrator.CountStoreRebuildRequired( StoreVersion.HIGH_LIMIT_V3_0_0.versionString() ) );
			  actualVersions.Add( StoreVersion.HIGH_LIMIT_V3_0_0.versionString() );
			  assertTrue( CountsMigrator.CountStoreRebuildRequired( StoreVersion.HIGH_LIMIT_V3_0_6.versionString() ) );
			  actualVersions.Add( StoreVersion.HIGH_LIMIT_V3_0_6.versionString() );
			  assertTrue( CountsMigrator.CountStoreRebuildRequired( StoreVersion.HIGH_LIMIT_V3_1_0.versionString() ) );
			  actualVersions.Add( StoreVersion.HIGH_LIMIT_V3_1_0.versionString() );
			  assertFalse( CountsMigrator.CountStoreRebuildRequired( StoreVersion.HIGH_LIMIT_V3_2_0.versionString() ) );
			  actualVersions.Add( StoreVersion.HIGH_LIMIT_V3_2_0.versionString() );
			  assertFalse( CountsMigrator.CountStoreRebuildRequired( StoreVersion.HIGH_LIMIT_V3_4_0.versionString() ) );
			  actualVersions.Add( StoreVersion.HIGH_LIMIT_V3_4_0.versionString() );

			  assertEquals( expectedVersions, actualVersions );
		 }

	}

}