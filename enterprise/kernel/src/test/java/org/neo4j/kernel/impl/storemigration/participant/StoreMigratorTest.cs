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
namespace Org.Neo4j.Kernel.impl.storemigration.participant
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfiguringPageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using StoreVersion = Org.Neo4j.Kernel.impl.store.format.StoreVersion;
	using HighLimitV3_0_0 = Org.Neo4j.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using Result = Org.Neo4j.Kernel.impl.storemigration.StoreVersionCheck.Result;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using NullLog = Org.Neo4j.Logging.NullLog;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

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
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.tracing.PageCacheTracer.NULL;

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
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
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

			  try (FileSystemAbstraction fs = new DefaultFileSystemAbstraction(); JobScheduler _jobScheduler = new ThreadPoolJobScheduler(); PageCache _pageCache = new ConfiguringPageCacheFactory(fs, config, NULL, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, NullLog.Instance, EmptyVersionContextSupplier.EMPTY, _jobScheduler)
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
			  ISet<string> expectedVersions = java.util.org.neo4j.kernel.impl.store.format.StoreVersion.values().Select(StoreVersion::versionString).collect(Collectors.toSet());

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