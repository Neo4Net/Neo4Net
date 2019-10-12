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
namespace Org.Neo4j.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OnDiskLastTxIdGetter = Org.Neo4j.Kernel.ha.transaction.OnDiskLastTxIdGetter;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class OnDiskLastTxIdGetterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetLastTxIdNoFilePresent()
		 public virtual void TestGetLastTxIdNoFilePresent()
		 {
			  // This is a sign that we have some bad coupling on our hands.
			  // We currently have to do this because of our lifecycle and construction ordering.
			  OnDiskLastTxIdGetter getter = new OnDiskLastTxIdGetter( () => 13L );
			  assertEquals( 13L, getter.LastTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lastTransactionIdIsBaseTxIdWhileNeoStoresAreStopped()
		 public virtual void LastTransactionIdIsBaseTxIdWhileNeoStoresAreStopped()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.StoreFactory storeFactory = new org.neo4j.kernel.impl.store.StoreFactory(org.neo4j.io.layout.DatabaseLayout.of(new java.io.File("store")), org.neo4j.kernel.configuration.Config.defaults(), new org.neo4j.kernel.impl.store.id.DefaultIdGeneratorFactory(fs.get()), pageCacheRule.getPageCache(fs.get()), fs.get(), org.neo4j.logging.NullLogProvider.getInstance(), org.neo4j.io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier.EMPTY);
			  StoreFactory storeFactory = new StoreFactory( DatabaseLayout.of( new File( "store" ) ), Config.defaults(), new DefaultIdGeneratorFactory(Fs.get()), PageCacheRule.getPageCache(Fs.get()), Fs.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.NeoStores neoStores = storeFactory.openAllNeoStores(true);
			  NeoStores neoStores = storeFactory.OpenAllNeoStores( true );
			  neoStores.Close();

			  System.Func<long> supplier = () => neoStores.MetaDataStore.LastCommittedTransactionId;
			  OnDiskLastTxIdGetter diskLastTxIdGetter = new OnDiskLastTxIdGetter( supplier );
			  assertEquals( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, diskLastTxIdGetter.LastTxId );
		 }
	}

}