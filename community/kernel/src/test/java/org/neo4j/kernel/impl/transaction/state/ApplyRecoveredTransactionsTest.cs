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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using CacheAccessBackDoor = Org.Neo4j.Kernel.impl.core.CacheAccessBackDoor;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using NodeCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.NodeCommand;
	using RelationshipCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using CommandHandlerContract = Org.Neo4j.Kernel.impl.transaction.command.CommandHandlerContract;
	using NeoStoreBatchTransactionApplier = Org.Neo4j.Kernel.impl.transaction.command.NeoStoreBatchTransactionApplier;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ApplyRecoveredTransactionsTest
	{
		private bool InstanceFieldsInitialized = false;

		public ApplyRecoveredTransactionsTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fs );
			RuleChain = RuleChain.outerRule( _fs ).around( _testDirectory ).around( _pageCacheRule );
		}

		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory).around(pageCacheRule);
		 public RuleChain RuleChain;

		 private NeoStores _neoStores;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  FileSystemAbstraction fs = this._fs.get();
			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(fs), _pageCacheRule.getPageCache(fs), fs, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  _neoStores = storeFactory.OpenAllNeoStores( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectHighIdWhenApplyingExternalTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetCorrectHighIdWhenApplyingExternalTransactions()
		 {
			  // WHEN recovering a transaction that creates some data
			  long nodeId = _neoStores.NodeStore.nextId();
			  long relationshipId = _neoStores.RelationshipStore.nextId();
			  int type = 1;
			  ApplyExternalTransaction( 1, new Command.NodeCommand( new NodeRecord( nodeId ), InUse( Created( new NodeRecord( nodeId ) ) ) ), new Command.RelationshipCommand( null, InUse( Created( With( new RelationshipRecord( relationshipId ), nodeId, nodeId, type ) ) ) ) );

			  // and when, later on, recovering a transaction deleting some of those
			  ApplyExternalTransaction( 2, new Command.NodeCommand( InUse( Created( new NodeRecord( nodeId ) ) ), new NodeRecord( nodeId ) ), new Command.RelationshipCommand( null, new RelationshipRecord( relationshipId ) ) );

			  // THEN that should be possible and the high ids should be correct, i.e. highest applied + 1
			  assertEquals( nodeId + 1, _neoStores.NodeStore.HighId );
			  assertEquals( relationshipId + 1, _neoStores.RelationshipStore.HighId );
		 }

		 private RelationshipRecord With( RelationshipRecord relationship, long startNode, long endNode, int type )
		 {
			  relationship.FirstNode = startNode;
			  relationship.SecondNode = endNode;
			  relationship.Type = type;
			  return relationship;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void applyExternalTransaction(long transactionId, org.neo4j.kernel.impl.transaction.command.Command...commands) throws Exception
		 private void ApplyExternalTransaction( long transactionId, params Command[] commands )
		 {
			  LockService lockService = mock( typeof( LockService ) );
			  when( lockService.AcquireNodeLock( anyLong(), any(typeof(Org.Neo4j.Kernel.impl.locking.LockService_LockType)) ) ).thenReturn(LockService.NO_LOCK);
			  when( lockService.AcquireRelationshipLock( anyLong(), any(typeof(Org.Neo4j.Kernel.impl.locking.LockService_LockType)) ) ).thenReturn(LockService.NO_LOCK);
			  NeoStoreBatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( _neoStores, mock( typeof( CacheAccessBackDoor ) ), lockService );
			  TransactionRepresentation tx = new PhysicalTransactionRepresentation( Arrays.asList( commands ) );
			  CommandHandlerContract.apply(applier, txApplier =>
			  {
				tx.Accept( txApplier );
				return false;
			  }, new TransactionToApply( tx, transactionId ));
		 }

		 private static RECORD InUse<RECORD>( RECORD record ) where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  record.InUse = true;
			  return record;
		 }

		 private static RECORD Created<RECORD>( RECORD record ) where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  record.setCreated();
			  return record;
		 }
	}

}