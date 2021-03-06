﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Api.integrationtest
{
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Org.Neo4j.@internal.Kernel.Api.TokenWrite;
	using InvalidTransactionTypeKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using SchemaKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class KernelIT : KernelIntegrationTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingBeansApiWithKernelAPI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MixingBeansApiWithKernelAPI()
		 {
			  // 1: Start your transactions through the Beans API
			  Transaction transaction = Db.beginTx();

			  // 2: Get a hold of a KernelAPI transaction this way:
			  KernelTransaction ktx = StatementContextSupplier.getKernelTransactionBoundToThisThread( true );

			  // 3: Now you can interact through both the statement context and the kernel API to manipulate the
			  //    same transaction.
			  Node node = Db.createNode();

			  int labelId = ktx.TokenWrite().labelGetOrCreateForName("labello");
			  ktx.DataWrite().nodeAddLabel(node.Id, labelId);

			  // 4: Commit through the beans API
			  transaction.Success();
			  transaction.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void schemaStateShouldBeEvictedOnIndexComingOnline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SchemaStateShouldBeEvictedOnIndexComingOnline()
		 {
			  // GIVEN
			  SchemaWriteInNewTransaction();
			  GetOrCreateSchemaState( "my key", "my state" );
			  Commit();

			  // WHEN
			  CreateIndex( NewTransaction( AUTH_DISABLED ) );
			  Commit();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(20, SECONDS);
					tx.Success();
			  }
			  // THEN schema state is eventually updated (clearing the schema cache is not atomic with respect to flipping
			  // the new index to the ONLINE state, but happens as soon as possible *after* the index becomes ONLINE).
			  assertEventually( "Schema state should have been updated", () => SchemaStateContains("my key"), @is(false), 1, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void schemaStateShouldBeEvictedOnIndexDropped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SchemaStateShouldBeEvictedOnIndexDropped()
		 {
			  // GIVEN
			  IndexReference idx = CreateIndex( NewTransaction( AUTH_DISABLED ) );
			  Commit();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(20, SECONDS);
					GetOrCreateSchemaState( "my key", "some state" );
					tx.Success();
			  }
			  // WHEN
			  SchemaWriteInNewTransaction().indexDrop(idx);
			  Commit();

			  // THEN schema state should be immediately updated (this works because the schema cache is updated during
			  // transaction apply, while the schema lock is held).
			  assertFalse( SchemaStateContains( "my key" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void txReturnsCorrectIdWhenCommitted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TxReturnsCorrectIdWhenCommitted()
		 {
			  ExecuteDummyTxs( Db, 42 );

			  Org.Neo4j.@internal.Kernel.Api.Transaction tx = NewTransaction( AUTH_DISABLED );
			  tx.DataWrite().nodeCreate();
			  tx.Success();

			  long previousCommittedTxId = LastCommittedTxId( Db );

			  assertEquals( previousCommittedTxId + 1, tx.CloseTransaction() );
			  assertFalse( tx.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void txReturnsCorrectIdWhenRolledBack() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TxReturnsCorrectIdWhenRolledBack()
		 {
			  ExecuteDummyTxs( Db, 42 );

			  Org.Neo4j.@internal.Kernel.Api.Transaction tx = NewTransaction( AUTH_DISABLED );
			  tx.DataWrite().nodeCreate();
			  tx.Failure();

			  assertEquals( KernelTransaction.ROLLBACK, tx.CloseTransaction() );
			  assertFalse( tx.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void txReturnsCorrectIdWhenMarkedForTermination() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TxReturnsCorrectIdWhenMarkedForTermination()
		 {
			  ExecuteDummyTxs( Db, 42 );

			  Org.Neo4j.@internal.Kernel.Api.Transaction tx = NewTransaction( AUTH_DISABLED );
			  tx.DataWrite().nodeCreate();
			  tx.MarkForTermination( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated );

			  assertEquals( KernelTransaction.ROLLBACK, tx.CloseTransaction() );
			  assertFalse( tx.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void txReturnsCorrectIdWhenFailedAndMarkedForTermination() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TxReturnsCorrectIdWhenFailedAndMarkedForTermination()
		 {
			  ExecuteDummyTxs( Db, 42 );

			  Org.Neo4j.@internal.Kernel.Api.Transaction tx = NewTransaction( AUTH_DISABLED );
			  tx.DataWrite().nodeCreate();
			  tx.Failure();
			  tx.MarkForTermination( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated );

			  assertEquals( KernelTransaction.ROLLBACK, tx.CloseTransaction() );
			  assertFalse( tx.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void txReturnsCorrectIdWhenReadOnly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TxReturnsCorrectIdWhenReadOnly()
		 {
			  ExecuteDummyTxs( Db, 42 );

			  Org.Neo4j.@internal.Kernel.Api.Transaction tx = NewTransaction();
			  using ( NodeCursor node = tx.Cursors().allocateNodeCursor() )
			  {
					tx.DataRead().singleNode(1, node);
					node.Next();
			  }
			  tx.Success();

			  assertEquals( KernelTransaction.READ_ONLY, tx.CloseTransaction() );
			  assertFalse( tx.Open );
		 }

		 private static void ExecuteDummyTxs( GraphDatabaseService db, int count )
		 {
			  for ( int i = 0; i < count; i++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode();
						 tx.Success();
					}
			  }
		 }

		 private static long LastCommittedTxId( GraphDatabaseAPI db )
		 {
			  TransactionIdStore txIdStore = Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
			  return txIdStore.LastCommittedTransactionId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference createIndex(org.neo4j.internal.kernel.api.Transaction transaction) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException, org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
		 private IndexReference CreateIndex( Org.Neo4j.@internal.Kernel.Api.Transaction transaction )
		 {
			  TokenWrite tokenWrite = transaction.TokenWrite();
			  SchemaWrite schemaWrite = transaction.SchemaWrite();
			  LabelSchemaDescriptor schemaDescriptor = forLabel( tokenWrite.LabelGetOrCreateForName( "hello" ), tokenWrite.PropertyKeyGetOrCreateForName( "hepp" ) );
			  return schemaWrite.IndexCreate( schemaDescriptor );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getOrCreateSchemaState(String key, final String maybeSetThisState)
		 private string GetOrCreateSchemaState( string key, string maybeSetThisState )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = StatementContextSupplier.getKernelTransactionBoundToThisThread( true );
					string state = ktx.SchemaRead().schemaStateGetOrCreate(key, s => maybeSetThisState);
					tx.Success();
					return state;
			  }
		 }

		 private bool SchemaStateContains( string key )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = StatementContextSupplier.getKernelTransactionBoundToThisThread( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean result = new java.util.concurrent.atomic.AtomicBoolean(true);
					AtomicBoolean result = new AtomicBoolean( true );
					ktx.SchemaRead().schemaStateGetOrCreate(key, s =>
					{
					 result.set( false );
					 return null;
					});
					tx.Success();
					return result.get();
			  }
		 }
	}

}