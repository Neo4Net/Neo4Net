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
namespace Org.Neo4j.Kernel.impl.store.format.highlimit
{
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using HighLimitV3_0_0 = Org.Neo4j.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using StoreMigrator = Org.Neo4j.Kernel.impl.storemigration.participant.StoreMigrator;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;

	public class HighLimitStoreMigrationTest
	{
		private bool InstanceFieldsInitialized = false;

		public HighLimitStoreMigrationTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Chain = RuleChain.outerRule( _pageCacheRule ).around( _fileSystemRule ).around( _testDirectory );
		}

		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(pageCacheRule).around(fileSystemRule).around(testDirectory);
		 public RuleChain Chain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void haveDifferentFormatCapabilitiesAsHighLimit3_0()
		 public virtual void HaveDifferentFormatCapabilitiesAsHighLimit3_0()
		 {
			  assertFalse( HighLimit.RecordFormats.hasCompatibleCapabilities( HighLimitV3_0_0.RECORD_FORMATS, CapabilityType.FORMAT ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateHighLimit3_0StoreFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MigrateHighLimit3_0StoreFiles()
		 {
			  FileSystemAbstraction fileSystem = _fileSystemRule.get();
			  PageCache pageCache = _pageCacheRule.getPageCache( fileSystem );
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					StoreMigrator migrator = new StoreMigrator( fileSystem, pageCache, Config.defaults(), NullLogService.Instance, jobScheduler );

					DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
					DatabaseLayout migrationLayout = _testDirectory.databaseLayout( "migration" );

					PrepareNeoStoreFile( fileSystem, databaseLayout, HighLimitV3_0_0.STORE_VERSION, pageCache );

					ProgressReporter progressMonitor = mock( typeof( ProgressReporter ) );

					migrator.Migrate( databaseLayout, migrationLayout, progressMonitor, HighLimitV3_0_0.STORE_VERSION, HighLimit.StoreVersion );

					int newStoreFilesCount = fileSystem.ListFiles( migrationLayout.DatabaseDirectory() ).Length;
					assertThat( "Store should be migrated and new store files should be created.", newStoreFilesCount, Matchers.greaterThanOrEqualTo( StoreType.values().length ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void prepareNeoStoreFile(org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.layout.DatabaseLayout databaseLayout, String storeVersion, org.neo4j.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private static void PrepareNeoStoreFile( FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout, string storeVersion, PageCache pageCache )
		 {
			  File neoStoreFile = CreateNeoStoreFile( fileSystem, databaseLayout );
			  long value = MetaDataStore.versionStringToLong( storeVersion );
			  MetaDataStore.setRecord( pageCache, neoStoreFile, STORE_VERSION, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File createNeoStoreFile(org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private static File CreateNeoStoreFile( FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout )
		 {
			  File neoStoreFile = databaseLayout.MetadataStore();
			  fileSystem.Create( neoStoreFile ).close();
			  return neoStoreFile;
		 }
	}

}