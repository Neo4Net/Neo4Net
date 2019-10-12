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
namespace Org.Neo4j.causalclustering.core.state.machines.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using EnterpriseIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RebuildReplicatedIdGeneratorsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
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