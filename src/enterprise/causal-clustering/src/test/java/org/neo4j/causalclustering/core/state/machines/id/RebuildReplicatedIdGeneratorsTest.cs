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
namespace Neo4Net.causalclustering.core.state.machines.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseIdTypeConfigurationProvider = Neo4Net.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RebuildReplicatedIdGeneratorsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.PageCacheRule pageCacheRule = new org.Neo4Net.test.rule.PageCacheRule();
		 public PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
		 private ReplicatedIdRangeAcquirer _idRangeAcquirer = mock( typeof( ReplicatedIdRangeAcquirer ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rebuildReplicatedIdGeneratorsOnRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RebuildReplicatedIdGeneratorsOnRecovery()
		 {
			  DefaultFileSystemAbstraction fileSystem = FileSystemRule.get();
			  File stickyGenerator = new File( TestDirectory.databaseDir(), "stickyGenerator" );
			  File nodeStoreIdGenerator = TestDirectory.databaseLayout().idNodeStore();

			  StoreFactory storeFactory = new StoreFactory( TestDirectory.databaseLayout(), Config.defaults(), GetIdGenerationFactory(fileSystem), PageCacheRule.getPageCache(fileSystem), fileSystem, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  using ( NeoStores neoStores = storeFactory.OpenAllNeoStores( true ) )
			  {
					NodeStore nodeStore = neoStores.NodeStore;
					for ( int i = 0; i < 50; i++ )
					{
						 NodeRecord nodeRecord = nodeStore.NewRecord();
						 nodeRecord.InUse = true;
						 nodeRecord.Id = nodeStore.NextId();
						 if ( i == 47 )
						 {
							  FileUtils.copyFile( nodeStoreIdGenerator, stickyGenerator );
						 }
						 nodeStore.UpdateRecord( nodeRecord );
					}
			  }

			  FileUtils.copyFile( stickyGenerator, nodeStoreIdGenerator );
			  using ( NeoStores reopenedStores = storeFactory.OpenAllNeoStores() )
			  {
					reopenedStores.MakeStoreOk();
					assertEquals( 51L, reopenedStores.NodeStore.nextId() );
			  }
		 }

		 private ReplicatedIdGeneratorFactory GetIdGenerationFactory( FileSystemAbstraction fileSystemAbstraction )
		 {
			  when( _idRangeAcquirer.acquireIds( IdType.NODE ) ).thenReturn( new IdAllocation( new IdRange( PrimitiveLongCollections.EMPTY_LONG_ARRAY, 0, 10000 ), 0, 0 ) );
			  return new ReplicatedIdGeneratorFactory( fileSystemAbstraction, _idRangeAcquirer, NullLogProvider.Instance, new EnterpriseIdTypeConfigurationProvider( Config.defaults() ) );
		 }
	}

}