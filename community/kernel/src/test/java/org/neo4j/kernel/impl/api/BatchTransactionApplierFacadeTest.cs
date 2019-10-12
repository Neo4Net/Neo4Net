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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using LockGroup = Org.Neo4j.Kernel.impl.locking.LockGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BatchTransactionApplierFacadeTest
	{

		 private BatchTransactionApplierFacade _facade;
		 private BatchTransactionApplier _applier1;
		 private BatchTransactionApplier _applier2;
		 private BatchTransactionApplier _applier3;
		 private TransactionApplier _txApplier1;
		 private TransactionApplier _txApplier2;
		 private TransactionApplier _txApplier3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _txApplier1 = mock( typeof( TransactionApplier ) );
			  _applier1 = mock( typeof( BatchTransactionApplier ) );
			  when( _applier1.startTx( any( typeof( TransactionToApply ) ) ) ).thenReturn( _txApplier1 );
			  when( _applier1.startTx( any( typeof( TransactionToApply ) ), any( typeof( LockGroup ) ) ) ).thenReturn( _txApplier1 );

			  _txApplier2 = mock( typeof( TransactionApplier ) );
			  _applier2 = mock( typeof( BatchTransactionApplier ) );
			  when( _applier2.startTx( any( typeof( TransactionToApply ) ) ) ).thenReturn( _txApplier2 );
			  when( _applier2.startTx( any( typeof( TransactionToApply ) ), any( typeof( LockGroup ) ) ) ).thenReturn( _txApplier2 );

			  _txApplier3 = mock( typeof( TransactionApplier ) );
			  _applier3 = mock( typeof( BatchTransactionApplier ) );
			  when( _applier3.startTx( any( typeof( TransactionToApply ) ) ) ).thenReturn( _txApplier3 );
			  when( _applier3.startTx( any( typeof( TransactionToApply ) ), any( typeof( LockGroup ) ) ) ).thenReturn( _txApplier3 );

			  _facade = new BatchTransactionApplierFacade( _applier1, _applier2, _applier3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStartTxCorrectOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestStartTxCorrectOrder()
		 {
			  // GIVEN
			  TransactionToApply tx = mock( typeof( TransactionToApply ) );

			  // WHEN
			  TransactionApplierFacade result = ( TransactionApplierFacade ) _facade.startTx( tx );

			  // THEN
			  InOrder inOrder = inOrder( _applier1, _applier2, _applier3 );

			  inOrder.verify( _applier1 ).startTx( tx );
			  inOrder.verify( _applier2 ).startTx( tx );
			  inOrder.verify( _applier3 ).startTx( tx );

			  assertEquals( _txApplier1, result.Appliers[0] );
			  assertEquals( _txApplier2, result.Appliers[1] );
			  assertEquals( _txApplier3, result.Appliers[2] );
			  assertEquals( 3, result.Appliers.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStartTxCorrectOrderWithLockGroup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestStartTxCorrectOrderWithLockGroup()
		 {
			  // GIVEN
			  TransactionToApply tx = mock( typeof( TransactionToApply ) );
			  LockGroup lockGroup = mock( typeof( LockGroup ) );

			  // WHEN
			  TransactionApplierFacade result = ( TransactionApplierFacade ) _facade.startTx( tx, lockGroup );

			  // THEN
			  InOrder inOrder = inOrder( _applier1, _applier2, _applier3 );

			  inOrder.verify( _applier1 ).startTx( tx, lockGroup );
			  inOrder.verify( _applier2 ).startTx( tx, lockGroup );
			  inOrder.verify( _applier3 ).startTx( tx, lockGroup );

			  assertEquals( _txApplier1, result.Appliers[0] );
			  assertEquals( _txApplier2, result.Appliers[1] );
			  assertEquals( _txApplier3, result.Appliers[2] );
			  assertEquals( 3, result.Appliers.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeShouldBeDoneInReverseOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseShouldBeDoneInReverseOrder()
		 {
			  // No idea why it was done like this before refactoring

			  // WHEN
			  _facade.close();

			  // THEN
			  InOrder inOrder = inOrder( _applier1, _applier2, _applier3 );

			  inOrder.verify( _applier3 ).close();
			  inOrder.verify( _applier2 ).close();
			  inOrder.verify( _applier1 ).close();
		 }
	}

}