using System;

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
namespace Org.Neo4j.com.storecopy
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using Suppliers = Org.Neo4j.Function.Suppliers;
	using Org.Neo4j.Helpers.Collection;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.StoreIdTestFactory.newStoreIdForCurrentVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;

	public class ResponsePackerIT
	{
		private bool InstanceFieldsInitialized = false;

		public ResponsePackerIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fsRule ).around( _pageCacheRule ).around( _testDirectory );
		}

		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(pageCacheRule).around(testDirectory);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackTheHighestTxCommittedAsObligation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPackTheHighestTxCommittedAsObligation()
		 {
			  // GIVEN
			  LogicalTransactionStore transactionStore = mock( typeof( LogicalTransactionStore ) );
			  FileSystemAbstraction fs = _fsRule.get();
			  PageCache pageCache = _pageCacheRule.getPageCache( fs );

			  using ( NeoStores neoStore = CreateNeoStore( fs, pageCache ) )
			  {
					MetaDataStore store = neoStore.MetaDataStore;
					store.TransactionCommitted( 2, 111, BASE_TX_COMMIT_TIMESTAMP );
					store.TransactionCommitted( 3, 222, BASE_TX_COMMIT_TIMESTAMP );
					store.TransactionCommitted( 4, 333, BASE_TX_COMMIT_TIMESTAMP );
					store.TransactionCommitted( 5, 444, BASE_TX_COMMIT_TIMESTAMP );
					store.TransactionCommitted( 6, 555, BASE_TX_COMMIT_TIMESTAMP );

					// skip 7 to emulate the fact we have an hole in the committed tx ids list

					const long expectedTxId = 8L;
					store.TransactionCommitted( expectedTxId, 777, BASE_TX_COMMIT_TIMESTAMP );

					ResponsePacker packer = new ResponsePacker( transactionStore, store, Suppliers.singleton( newStoreIdForCurrentVersion() ) );

					// WHEN
					Response<object> response = packer.PackTransactionObligationResponse( new RequestContext( 0, 0, 0, 0, 0 ), new object() );

					// THEN
					assertTrue( response is TransactionObligationResponse );
					( ( TransactionObligationResponse ) response ).accept( new HandlerAnonymousInnerClass( this, expectedTxId ) );
			  }
		 }

		 private class HandlerAnonymousInnerClass : Response.Handler
		 {
			 private readonly ResponsePackerIT _outerInstance;

			 private long _expectedTxId;

			 public HandlerAnonymousInnerClass( ResponsePackerIT outerInstance, long expectedTxId )
			 {
				 this.outerInstance = outerInstance;
				 this._expectedTxId = expectedTxId;
			 }

			 public void obligation( long txId )
			 {
				  assertEquals( _expectedTxId, txId );
			 }

			 public Visitor<CommittedTransactionRepresentation, Exception> transactions()
			 {
				  throw new System.NotSupportedException( "not expected" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.store.NeoStores createNeoStore(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private NeoStores CreateNeoStore( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  Config config = Config.defaults();
			  DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( fs );
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), config, idGeneratorFactory, pageCache, fs, logProvider, EmptyVersionContextSupplier.EMPTY );
			  return storeFactory.OpenAllNeoStores( true );
		 }
	}

}