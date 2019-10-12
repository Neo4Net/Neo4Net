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
namespace Org.Neo4j.Kernel.impl.store
{
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NodeRecordFormat = Org.Neo4j.Kernel.impl.store.format.standard.NodeRecordFormat;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using ConfigurablePageCacheRule = Org.Neo4j.Test.rule.ConfigurablePageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class TestGrowingFileMemoryMapping
	{
		private bool InstanceFieldsInitialized = false;

		public TestGrowingFileMemoryMapping()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _fileSystemRule ).around( _pageCacheRule );
		}


		 private readonly ConfigurablePageCacheRule _pageCacheRule = new ConfigurablePageCacheRule();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGrowAFileWhileContinuingToMemoryMapNewRegions()
		 public virtual void ShouldGrowAFileWhileContinuingToMemoryMapNewRegions()
		 {
			  // don't run on windows because memory mapping doesn't work properly there
			  assumeTrue( !SystemUtils.IS_OS_WINDOWS );

			  // given
			  const int numberOfRecords = 1000000;

			  Config config = Config.defaults( pagecache_memory, MmapSize( numberOfRecords, NodeRecordFormat.RECORD_SIZE ) );
			  FileSystemAbstraction fileSystemAbstraction = _fileSystemRule.get();
			  DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( fileSystemAbstraction );
			  PageCache pageCache = _pageCacheRule.getPageCache( fileSystemAbstraction, config );
			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), config, idGeneratorFactory, pageCache, fileSystemAbstraction, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );

			  NeoStores neoStores = storeFactory.OpenAllNeoStores( true );
			  NodeStore nodeStore = neoStores.NodeStore;

			  // when
			  int iterations = 2 * numberOfRecords;
			  long startingId = nodeStore.NextId();
			  long nodeId = startingId;
			  for ( int i = 0; i < iterations; i++ )
			  {
					NodeRecord record = new NodeRecord( nodeId, false, i, 0 );
					record.InUse = true;
					nodeStore.UpdateRecord( record );
					nodeId = nodeStore.NextId();
			  }

			  // then
			  NodeRecord record = new NodeRecord( 0, false, 0, 0 );
			  for ( int i = 0; i < iterations; i++ )
			  {
					record.Id = startingId + i;
					nodeStore.GetRecord( i, record, NORMAL );
					assertTrue( "record[" + i + "] should be in use", record.InUse() );
					assertThat( "record[" + i + "] should have nextRelId of " + i, record.NextRel, @is( ( long ) i ) );
			  }

			  neoStores.Close();
		 }

		 private static string MmapSize( int numberOfRecords, int recordSize )
		 {
			  int bytes = numberOfRecords * recordSize;
			  long mebiByte = ByteUnit.mebiBytes( 1 );
			  if ( bytes < mebiByte )
			  {
					throw new System.ArgumentException( "too few records: " + numberOfRecords );
			  }
			  return bytes / mebiByte + "M";
		 }
	}

}