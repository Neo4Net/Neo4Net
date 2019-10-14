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
namespace Neo4Net.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;

	public class FreeIdsAfterRecoveryTest
	{
		private bool InstanceFieldsInitialized = false;

		public FreeIdsAfterRecoveryTest()
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
		}

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompletelyRebuildIdGeneratorsAfterCrash()
		 public virtual void ShouldCompletelyRebuildIdGeneratorsAfterCrash()
		 {
			  // GIVEN
			  DatabaseLayout databaseLayout = _directory.databaseLayout();
			  StoreFactory storeFactory = new StoreFactory( databaseLayout, Config.defaults(), new DefaultIdGeneratorFactory(_fileSystemRule.get()), _pageCacheRule.getPageCache(_fileSystemRule.get()), _fileSystemRule.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  long highId;
			  using ( NeoStores stores = storeFactory.OpenAllNeoStores( true ) )
			  {
					// a node store with a "high" node
					NodeStore nodeStore = stores.NodeStore;
					nodeStore.HighId = 20;
					nodeStore.UpdateRecord( Node( nodeStore.NextId() ) );
					highId = nodeStore.HighId;
			  }

			  // populating its .id file with a bunch of ids
			  File nodeIdFile = databaseLayout.IdNodeStore();
			  using ( IdGeneratorImpl idGenerator = new IdGeneratorImpl( _fileSystemRule.get(), nodeIdFile, 10, 10_000, false, IdType.NODE, () => highId ) )
			  {
					for ( long id = 0; id < 15; id++ )
					{
						 idGenerator.FreeId( id );
					}

					// WHEN
					using ( NeoStores stores = storeFactory.OpenAllNeoStores( true ) )
					{
						 NodeStore nodeStore = stores.NodeStore;
						 assertFalse( nodeStore.StoreOk );

						 // simulating what recovery does
						 nodeStore.DeleteIdGenerator();
						 // recovery happens here...
						 nodeStore.MakeStoreOk();

						 // THEN
						 assertEquals( highId, nodeStore.NextId() );
					}
			  }
		 }

		 private static NodeRecord Node( long nextId )
		 {
			  NodeRecord node = new NodeRecord( nextId );
			  node.InUse = true;
			  return node;
		 }
	}

}