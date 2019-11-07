using System;

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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Test = org.junit.Test;

	using Write = Neo4Net.Kernel.Api.Internal.Write;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using Neo4Net.Kernel.Api;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using ReadableTransactionState = Neo4Net.Kernel.Api.StorageEngine.TxState.ReadableTransactionState;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TransactionHookIT : KernelIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveTxStateOnCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveTxStateOnCommit()
		 {
			  // Given
			  TransactionHook hook = mock( typeof( TransactionHook ) );
			  InternalKernel().registerTransactionHook(hook);

			  // When
			  Write ops = DataWriteInNewTransaction();
			  ops.NodeCreate();
			  Commit();

			  // Then
			  verify( hook ).beforeCommit( any( typeof( ReadableTransactionState ) ), any( typeof( KernelTransaction ) ), any( typeof( StorageReader ) ) );
			  verify( hook ).afterCommit( any( typeof( ReadableTransactionState ) ), any( typeof( KernelTransaction ) ), any() );
			  verifyNoMoreInteractions( hook );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackOnFailureInBeforeCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackOnFailureInBeforeCommit()
		 {
			  // Given
			  TransactionHook hook = mock( typeof( TransactionHook ) );
			  const string message = "Original";
			  when( hook.beforeCommit( any( typeof( ReadableTransactionState ) ), any( typeof( KernelTransaction ) ), any( typeof( StorageReader ) ) ) ).thenReturn( new TransactionHook_OutcomeAnonymousInnerClass( this, message ) );
			  InternalKernel().registerTransactionHook(hook);

			  // When
			  Write ops = DataWriteInNewTransaction();
			  ops.NodeCreate();

			  try
			  {
					Commit();
					fail( "Expected this to fail." );
			  }
			  catch ( TransactionFailureException e )
			  {
					assertThat( e.Status(), equalTo(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionHookFailed) );
					assertThat( e.InnerException.Message, equalTo( message ) );
			  }
			  // Then
			  verify( hook ).beforeCommit( any( typeof( ReadableTransactionState ) ), any( typeof( KernelTransaction ) ), any( typeof( StorageReader ) ) );
			  verify( hook ).afterRollback( any( typeof( ReadableTransactionState ) ), any( typeof( KernelTransaction ) ), any( typeof( Neo4Net.Kernel.Api.TransactionHook_Outcome ) ) );
			  verifyNoMoreInteractions( hook );
		 }

		 private class TransactionHook_OutcomeAnonymousInnerClass : Neo4Net.Kernel.Api.TransactionHook_Outcome
		 {
			 private readonly TransactionHookIT _outerInstance;

			 private string _message;

			 public TransactionHook_OutcomeAnonymousInnerClass( TransactionHookIT outerInstance, string message )
			 {
				 this.outerInstance = outerInstance;
				 this._message = message;
			 }

			 public bool Successful
			 {
				 get
				 {
					  return false;
				 }
			 }

			 public Exception failure()
			 {
				  return new Exception( _message );
			 }
		 }
	}

}