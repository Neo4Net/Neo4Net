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
namespace Neo4Net.Kernel
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Locks = Neo4Net.@internal.Kernel.Api.Locks;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using PlaceboTransaction = Neo4Net.Kernel.impl.coreapi.PlaceboTransaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TestPlaceboTransaction
	{
		 private Transaction _placeboTx;
		 private Node _resource;
		 private KernelTransaction _kernelTransaction;
		 private Locks _locks;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  ThreadToStatementContextBridge bridge = mock( typeof( ThreadToStatementContextBridge ) );
			  Statement statement = mock( typeof( Statement ) );
			  when( bridge.Get() ).thenReturn(statement);
			  _kernelTransaction = spy( typeof( KernelTransaction ) );
			  _locks = mock( typeof( Locks ) );
			  when( _kernelTransaction.locks() ).thenReturn(_locks);
			  _placeboTx = new PlaceboTransaction( _kernelTransaction );
			  _resource = mock( typeof( Node ) );
			  when( _resource.Id ).thenReturn( 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackParentByDefault()
		 public virtual void ShouldRollbackParentByDefault()
		 {
			  // When
			  _placeboTx.close();

			  // Then
			  verify( _kernelTransaction ).failure();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackParentIfFailureCalled()
		 public virtual void ShouldRollbackParentIfFailureCalled()
		 {
			  // When
			  _placeboTx.failure();
			  _placeboTx.close();

			  // Then
			  verify( _kernelTransaction, times( 2 ) ).failure(); // We accept two calls to failure, since KernelTX#failure is idempotent
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRollbackParentIfSuccessCalled()
		 public virtual void ShouldNotRollbackParentIfSuccessCalled()
		 {
			  // When
			  _placeboTx.success();
			  _placeboTx.close();

			  // Then
			  verify( _kernelTransaction, never() ).failure();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successCannotOverrideFailure()
		 public virtual void SuccessCannotOverrideFailure()
		 {
			  // When
			  _placeboTx.failure();
			  _placeboTx.success();
			  _placeboTx.close();

			  // Then
			  verify( _kernelTransaction ).failure();
			  verify( _kernelTransaction, never() ).success();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAcquireReadLock()
		 public virtual void CanAcquireReadLock()
		 {
			  // when
			  _placeboTx.acquireReadLock( _resource );

			  // then
			  verify( _locks ).acquireSharedNodeLock( _resource.Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAcquireWriteLock()
		 public virtual void CanAcquireWriteLock()
		 {
			  // when
			  _placeboTx.acquireWriteLock( _resource );

			  // then
			  verify( _locks ).acquireExclusiveNodeLock( _resource.Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTerminationReason()
		 public virtual void ShouldReturnTerminationReason()
		 {
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ) );
			  when( kernelTransaction.ReasonIfTerminated ).thenReturn( null ).thenReturn( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Interrupted );

			  PlaceboTransaction tx = new PlaceboTransaction( kernelTransaction );

			  Optional<Status> terminationReason1 = tx.TerminationReason();
			  Optional<Status> terminationReason2 = tx.TerminationReason();

			  assertFalse( terminationReason1.Present );
			  assertTrue( terminationReason2.Present );
			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Interrupted, terminationReason2.get() );
		 }
	}

}