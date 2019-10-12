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
namespace Neo4Net.causalclustering
{

	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Neo4Net.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.RANDOM_NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.UPGRADE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.UPGRADE_TRANSACTION_ID;

	public class TestStoreId
	{
		 private TestStoreId()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void assertAllStoresHaveTheSameStoreId(java.util.List<java.io.File> coreStoreDirs, org.neo4j.io.fs.FileSystemAbstraction fs) throws Exception
		 public static void AssertAllStoresHaveTheSameStoreId( IList<File> coreStoreDirs, FileSystemAbstraction fs )
		 {
			  ISet<StoreId> storeIds = GetStoreIds( coreStoreDirs, fs );
			  assertEquals( "Store Ids " + storeIds, 1, storeIds.Count );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Set<org.neo4j.causalclustering.identity.StoreId> getStoreIds(java.util.List<java.io.File> coreStoreDirs, org.neo4j.io.fs.FileSystemAbstraction fs) throws Exception
		 public static ISet<StoreId> GetStoreIds( IList<File> coreStoreDirs, FileSystemAbstraction fs )
		 {
			  ISet<StoreId> storeIds = new HashSet<StoreId>();
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler(), PageCache pageCache = StandalonePageCacheFactory.createPageCache(fs, jobScheduler) )
			  {
					foreach ( File coreStoreDir in coreStoreDirs )
					{
						 storeIds.Add( DoReadStoreId( coreStoreDir, pageCache ) );
					}
			  }

			  return storeIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.causalclustering.identity.StoreId doReadStoreId(java.io.File databaseDirectory, org.neo4j.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private static StoreId DoReadStoreId( File databaseDirectory, PageCache pageCache )
		 {
			  File metadataStore = DatabaseLayout.of( databaseDirectory ).metadataStore();

			  long creationTime = MetaDataStore.getRecord( pageCache, metadataStore, TIME );
			  long randomNumber = MetaDataStore.getRecord( pageCache, metadataStore, RANDOM_NUMBER );
			  long upgradeTime = MetaDataStore.getRecord( pageCache, metadataStore, UPGRADE_TIME );
			  long upgradeId = MetaDataStore.getRecord( pageCache, metadataStore, UPGRADE_TRANSACTION_ID );

			  return new StoreId( creationTime, randomNumber, upgradeTime, upgradeId );
		 }
	}

}