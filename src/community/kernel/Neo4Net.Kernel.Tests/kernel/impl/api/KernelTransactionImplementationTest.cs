using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;
	using DefaultPageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracer;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using ExplicitIndexTransactionState = Neo4Net.Kernel.api.txstate.ExplicitIndexTransactionState;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using AuxiliaryTransactionState = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using NoOpClient = Neo4Net.Kernel.impl.locking.NoOpClient;
	using SimpleStatementLocks = Neo4Net.Kernel.impl.locking.SimpleStatementLocks;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using ResourceLocker = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceLocker;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class KernelTransactionImplementationTest extends KernelTransactionTestBase
	public class KernelTransactionImplementationTest : KernelTransactionTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public System.Action<org.Neo4Net.kernel.api.KernelTransaction> transactionInitializer;
		 public System.Action<KernelTransaction> TransactionInitializer;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public boolean isWriteTx;
		 public bool IsWriteTx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public String ignored;
		 public string Ignored; // to make JUnit happy...

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{2}") public static java.util.Collection<Object[]> parameters()
		 public static ICollection<object[]> Parameters()
		 {
			  System.Action<KernelTransaction> readTxInitializer = tx =>
			  {
			  };
			  System.Action<KernelTransaction> writeTxInitializer = tx =>
			  {
				using ( KernelStatement statement = ( KernelStatement ) tx.acquireStatement() )
				{
					 statement.TxState().nodeDoCreate(42);
				}
			  };
			  return Arrays.asList( new object[]{ readTxInitializer, false, "readOperationsInNewTransaction" }, new object[]{ writeTxInitializer, true, "write" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyMetadataReturnedWhenMetadataIsNotSet() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EmptyMetadataReturnedWhenMetadataIsNotSet()
		 {
			  using ( KernelTransaction transaction = NewTransaction( LoginContext() ) )
			  {
					IDictionary<string, object> metaData = transaction.MetaData;
					assertTrue( metaData.Count == 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void accessSpecifiedTransactionMetadata() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AccessSpecifiedTransactionMetadata()
		 {
			  using ( KernelTransaction transaction = NewTransaction( LoginContext() ) )
			  {
					IDictionary<string, object> externalMetadata = map( "Robot", "Bender", "Human", "Fry" );
					transaction.MetaData = externalMetadata;
					IDictionary<string, object> transactionMetadata = transaction.MetaData;
					assertFalse( transactionMetadata.Count == 0 );
					assertEquals( "Bender", transactionMetadata["Robot"] );
					assertEquals( "Fry", transactionMetadata["Human"] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitSuccessfulTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitSuccessfulTransaction()
		 {
			  // GIVEN
			  using ( KernelTransaction transaction = NewTransaction( LoginContext() ) )
			  {
					// WHEN
					TransactionInitializer.accept( transaction );
					transaction.Success();
			  }

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( true, IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackUnsuccessfulTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackUnsuccessfulTransaction()
		 {
			  // GIVEN
			  using ( KernelTransaction transaction = NewTransaction( LoginContext() ) )
			  {
					// WHEN
					TransactionInitializer.accept( transaction );
			  }

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackFailedTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackFailedTransaction()
		 {
			  // GIVEN
			  using ( KernelTransaction transaction = NewTransaction( LoginContext() ) )
			  {
					// WHEN
					TransactionInitializer.accept( transaction );
					transaction.Failure();
			  }

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackAndThrowOnFailedAndSuccess()
		 public virtual void ShouldRollbackAndThrowOnFailedAndSuccess()
		 {
			  // GIVEN
			  bool exceptionReceived = false;
			  try
			  {
					  using ( KernelTransaction transaction = NewTransaction( LoginContext() ) )
					  {
						// WHEN
						TransactionInitializer.accept( transaction );
						transaction.Failure();
						transaction.Success();
					  }
			  }
			  catch ( TransactionFailureException )
			  {
					// Expected.
					exceptionReceived = true;
			  }

			  // THEN
			  assertTrue( exceptionReceived );
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackOnClosingTerminatedTransaction()
		 public virtual void ShouldRollbackOnClosingTerminatedTransaction()
		 {
			  // GIVEN
			  KernelTransaction transaction = NewTransaction( LoginContext() );

			  TransactionInitializer.accept( transaction );
			  transaction.Success();
			  transaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  try
			  {
					// WHEN
					transaction.Close();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransactionTerminatedException ) ) );
			  }

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  verify( TransactionMonitor, times( 1 ) ).transactionTerminated( IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackOnClosingSuccessfulButTerminatedTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackOnClosingSuccessfulButTerminatedTransaction()
		 {
			  using ( KernelTransaction transaction = NewTransaction( LoginContext() ) )
			  {
					// WHEN
					TransactionInitializer.accept( transaction );
					transaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, transaction.ReasonIfTerminated.get() );
			  }

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  verify( TransactionMonitor, times( 1 ) ).transactionTerminated( IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackOnClosingTerminatedButSuccessfulTransaction()
		 public virtual void ShouldRollbackOnClosingTerminatedButSuccessfulTransaction()
		 {
			  // GIVEN
			  KernelTransaction transaction = NewTransaction( LoginContext() );

			  TransactionInitializer.accept( transaction );
			  transaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );
			  transaction.Success();
			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, transaction.ReasonIfTerminated.get() );

			  try
			  {
					// WHEN
					transaction.Close();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransactionTerminatedException ) ) );
			  }

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  verify( TransactionMonitor, times( 1 ) ).transactionTerminated( IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDowngradeFailureState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDowngradeFailureState()
		 {
			  using ( KernelTransaction transaction = NewTransaction( LoginContext() ) )
			  {
					// WHEN
					TransactionInitializer.accept( transaction );
					transaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );
					transaction.Failure();
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, transaction.ReasonIfTerminated.get() );
			  }

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  verify( TransactionMonitor, times( 1 ) ).transactionTerminated( IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreTerminateAfterCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreTerminateAfterCommit()
		 {
			  KernelTransaction transaction = NewTransaction( LoginContext() );
			  TransactionInitializer.accept( transaction );
			  transaction.Success();
			  transaction.Close();
			  transaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( true, IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreTerminateAfterRollback() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreTerminateAfterRollback()
		 {
			  KernelTransaction transaction = NewTransaction( LoginContext() );
			  TransactionInitializer.accept( transaction );
			  transaction.Close();
			  transaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.TransactionTerminatedException.class) public void shouldThrowOnTerminationInCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowOnTerminationInCommit()
		 {
			  KernelTransaction transaction = NewTransaction( LoginContext() );
			  TransactionInitializer.accept( transaction );
			  transaction.Success();
			  transaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  transaction.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreTerminationDuringRollback() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreTerminationDuringRollback()
		 {
			  KernelTransaction transaction = NewTransaction( LoginContext() );
			  TransactionInitializer.accept( transaction );
			  transaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );
			  transaction.Close();

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  verify( TransactionMonitor, times( 1 ) ).transactionTerminated( IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowTerminatingFromADifferentThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowTerminatingFromADifferentThread()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.DoubleLatch latch = new org.Neo4Net.test.DoubleLatch(1);
			  DoubleLatch latch = new DoubleLatch( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.api.KernelTransaction transaction = newTransaction(loginContext());
			  KernelTransaction transaction = NewTransaction( LoginContext() );
			  TransactionInitializer.accept( transaction );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> terminationFuture = java.util.concurrent.Executors.newSingleThreadExecutor().submit(() ->
			  Future<object> terminationFuture = Executors.newSingleThreadExecutor().submit(() =>
			  {
				latch.WaitForAllToStart();
				transaction.MarkForTermination( Status.General.UnknownError );
				latch.Finish();
			  });

			  // WHEN
			  transaction.Success();
			  latch.StartAndWaitForAllToStartAndFinish();

			  assertNull( terminationFuture.get( 1, TimeUnit.MINUTES ) );

			  try
			  {
					transaction.Close();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransactionTerminatedException ) ) );
			  }

			  // THEN
			  verify( TransactionMonitor, times( 1 ) ).transactionFinished( false, IsWriteTx );
			  verify( TransactionMonitor, times( 1 ) ).transactionTerminated( IsWriteTx );
			  VerifyExtraInteractionWithTheMonitor( TransactionMonitor, IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldUseStartTimeAndTxIdFromWhenStartingTxAsHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseStartTimeAndTxIdFromWhenStartingTxAsHeader()
		 {
			  // GIVEN a transaction starting at one point in time
			  long startingTime = Clock.millis();
			  ExplicitIndexTransactionState explicitIndexState = mock( typeof( ExplicitIndexTransactionState ) );
			  AuxTxStateManager.registerProvider( new ExplicitIndexTransactionStateProviderAnonymousInnerClass( this, explicitIndexState ) );
			  when( explicitIndexState.hasChanges() ).thenReturn(true);
			  doAnswer(invocation =>
			  {
				 ICollection<StorageCommand> commands = invocation.getArgument( 0 );
				commands.add( mock( typeof( Command ) ) );
				return null;
			  }).when( StorageEngine ).createCommands( any( typeof( System.Collections.ICollection ) ), any( typeof( TransactionState ) ), any( typeof( StorageReader ) ), any( typeof( ResourceLocker ) ), anyLong(), any(typeof(TxStateVisitor.Decorator)) );

			  using ( KernelTransactionImplementation transaction = NewTransaction( LoginContext() ) )
			  {
					SimpleStatementLocks statementLocks = new SimpleStatementLocks( mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) ) );
					transaction.Initialize( 5L, BASE_TX_COMMIT_TIMESTAMP, statementLocks, KernelTransaction.Type.@implicit, SecurityContext.AUTH_DISABLED, 0L, 1L );
					transaction.TxState();
					using ( KernelStatement statement = transaction.AcquireStatement() )
					{
						 statement.ExplicitIndexTxState(); // which will pull it from the supplier and the mocking above
						 // will have it say that it has changes.
					}
					// WHEN committing it at a later point
					Clock.forward( 5, MILLISECONDS );
					// ...and simulating some other transaction being committed
					when( MetaDataStore.LastCommittedTransactionId ).thenReturn( 7L );
					transaction.Success();
			  }

			  // THEN start time and last tx when started should have been taken from when the transaction started
			  assertEquals( 5L, CommitProcess.transaction.LatestCommittedTxWhenStarted );
			  assertEquals( startingTime, CommitProcess.transaction.TimeStarted );
			  assertEquals( startingTime + 5, CommitProcess.transaction.TimeCommitted );
		 }

		 private class ExplicitIndexTransactionStateProviderAnonymousInnerClass : ExplicitIndexTransactionStateProvider
		 {
			 private readonly KernelTransactionImplementationTest _outerInstance;

			 private ExplicitIndexTransactionState _explicitIndexState;

			 public ExplicitIndexTransactionStateProviderAnonymousInnerClass( KernelTransactionImplementationTest outerInstance, ExplicitIndexTransactionState explicitIndexState ) : base( null, null )
			 {
				 this.outerInstance = outerInstance;
				 this._explicitIndexState = explicitIndexState;
			 }

			 public override AuxiliaryTransactionState createNewAuxiliaryTransactionState()
			 {
				  return _explicitIndexState;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulTxShouldNotifyKernelTransactionsThatItIsClosed() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SuccessfulTxShouldNotifyKernelTransactionsThatItIsClosed()
		 {
			  KernelTransactionImplementation tx = NewTransaction( LoginContext() );

			  tx.Success();
			  tx.Close();

			  verify( TxPool ).release( tx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedTxShouldNotifyKernelTransactionsThatItIsClosed() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailedTxShouldNotifyKernelTransactionsThatItIsClosed()
		 {
			  KernelTransactionImplementation tx = NewTransaction( LoginContext() );

			  tx.Failure();
			  tx.Close();

			  verify( TxPool ).release( tx );
		 }

		 private void VerifyExtraInteractionWithTheMonitor( TransactionMonitor transactionMonitor, bool isWriteTx )
		 {
			  if ( isWriteTx )
			  {
					verify( this.TransactionMonitor, times( 1 ) ).upgradeToWriteTransaction();
			  }
			  verifyNoMoreInteractions( transactionMonitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncrementReuseCounterOnReuse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncrementReuseCounterOnReuse()
		 {
			  // GIVEN
			  KernelTransactionImplementation transaction = NewTransaction( LoginContext() );
			  int reuseCount = transaction.ReuseCount;

			  // WHEN
			  transaction.Close();
			  SimpleStatementLocks statementLocks = new SimpleStatementLocks( new NoOpClient() );
			  transaction.Initialize( 1, BASE_TX_COMMIT_TIMESTAMP, statementLocks, KernelTransaction.Type.@implicit, LoginContext().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME), 0L, 1L );

			  // THEN
			  assertEquals( reuseCount + 1, transaction.ReuseCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markForTerminationNotInitializedTransaction()
		 public virtual void MarkForTerminationNotInitializedTransaction()
		 {
			  KernelTransactionImplementation tx = NewNotInitializedTransaction();
			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, tx.ReasonIfTerminated.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markForTerminationInitializedTransaction()
		 public virtual void MarkForTerminationInitializedTransaction()
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext(), locksClient );

			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, tx.ReasonIfTerminated.get() );
			  verify( locksClient ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markForTerminationTerminatedTransaction()
		 public virtual void MarkForTerminationTerminatedTransaction()
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext(), locksClient );
			  TransactionInitializer.accept( tx );

			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Outdated );
			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.LockClientStopped );

			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated, tx.ReasonIfTerminated.get() );
			  verify( locksClient ).stop();
			  verify( TransactionMonitor ).transactionTerminated( IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminatedTxMarkedNeitherSuccessNorFailureClosesWithoutThrowing() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminatedTxMarkedNeitherSuccessNorFailureClosesWithoutThrowing()
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext(), locksClient );
			  TransactionInitializer.accept( tx );
			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  tx.Close();

			  verify( locksClient ).stop();
			  verify( TransactionMonitor ).transactionTerminated( IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminatedTxMarkedForSuccessThrowsOnClose()
		 public virtual void TerminatedTxMarkedForSuccessThrowsOnClose()
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext(), locksClient );
			  TransactionInitializer.accept( tx );
			  tx.Success();
			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  try
			  {
					tx.Close();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminatedTxMarkedForFailureClosesWithoutThrowing() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminatedTxMarkedForFailureClosesWithoutThrowing()
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext(), locksClient );
			  TransactionInitializer.accept( tx );
			  tx.Failure();
			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  tx.Close();

			  verify( locksClient ).stop();
			  verify( TransactionMonitor ).transactionTerminated( IsWriteTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminatedTxMarkedForBothSuccessAndFailureThrowsOnClose()
		 public virtual void TerminatedTxMarkedForBothSuccessAndFailureThrowsOnClose()
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext(), locksClient );
			  TransactionInitializer.accept( tx );
			  tx.Success();
			  tx.Failure();
			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError );

			  try
			  {
					tx.Close();
					fail();
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void txMarkedForBothSuccessAndFailureThrowsOnClose()
		 public virtual void TxMarkedForBothSuccessAndFailureThrowsOnClose()
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext(), locksClient );
			  tx.Success();
			  tx.Failure();

			  try
			  {
					tx.Close();
					fail();
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransactionFailureException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void initializedTransactionShouldHaveNoTerminationReason()
		 public virtual void InitializedTransactionShouldHaveNoTerminationReason()
		 {
			  KernelTransactionImplementation tx = NewTransaction( LoginContext() );
			  assertFalse( tx.ReasonIfTerminated.Present );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCorrectTerminationReason()
		 public virtual void ShouldReportCorrectTerminationReason()
		 {
			  Status status = Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated;
			  KernelTransactionImplementation tx = NewTransaction( LoginContext() );
			  tx.MarkForTermination( status );
			  assertSame( status, tx.ReasonIfTerminated.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closedTransactionShouldHaveNoTerminationReason() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClosedTransactionShouldHaveNoTerminationReason()
		 {
			  KernelTransactionImplementation tx = NewTransaction( LoginContext() );
			  tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
			  tx.Close();
			  assertFalse( tx.ReasonIfTerminated.Present );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallCloseListenerOnCloseWhenCommitting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallCloseListenerOnCloseWhenCommitting()
		 {
			  // given
			  AtomicLong closeTxId = new AtomicLong( long.MinValue );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext() );
			  tx.RegisterCloseListener( closeTxId.set );

			  // when
			  if ( IsWriteTx )
			  {
					tx.UpgradeToDataWrites();
					tx.TxState().nodeDoCreate(42L);
			  }
			  tx.Success();
			  tx.Close();

			  // then
			  assertThat( closeTxId.get(), IsWriteTx ? greaterThan(BASE_TX_ID) : equalTo(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallCloseListenerOnCloseWhenRollingBack() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallCloseListenerOnCloseWhenRollingBack()
		 {
			  // given
			  AtomicLong closeTxId = new AtomicLong( long.MinValue );
			  KernelTransactionImplementation tx = NewTransaction( LoginContext() );
			  tx.RegisterCloseListener( closeTxId.set );

			  // when
			  tx.Failure();
			  tx.Close();

			  // then
			  assertEquals( -1L, closeTxId.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionWithCustomTimeout()
		 public virtual void TransactionWithCustomTimeout()
		 {
			  long transactionTimeout = 5L;
			  KernelTransactionImplementation transaction = NewTransaction( transactionTimeout );
			  assertEquals( "Transaction should have custom configured timeout.", transactionTimeout, transaction.Timeout() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionStartTime()
		 public virtual void TransactionStartTime()
		 {
			  long startTime = Clock.forward( 5, TimeUnit.MINUTES ).millis();
			  KernelTransactionImplementation transaction = newTransaction( AUTH_DISABLED );
			  assertEquals( "Transaction start time should be the same as clock time.", startTime, transaction.StartTime() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markForTerminationWithCorrectReuseCount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MarkForTerminationWithCorrectReuseCount()
		 {
			  int reuseCount = 10;
			  Neo4Net.Kernel.Api.Exceptions.Status_Transaction terminationReason = Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated;

			  KernelTransactionImplementation tx = NewNotInitializedTransaction();
			  InitializeAndClose( tx, reuseCount );

			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  SimpleStatementLocks statementLocks = new SimpleStatementLocks( locksClient );
			  tx.Initialize( 42, 42, statementLocks, KernelTransaction.Type.@implicit, LoginContext().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME), 0L, 0L );

			  assertTrue( tx.MarkForTermination( reuseCount, terminationReason ) );

			  assertEquals( terminationReason, tx.ReasonIfTerminated.get() );
			  verify( locksClient ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markForTerminationWithIncorrectReuseCount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MarkForTerminationWithIncorrectReuseCount()
		 {
			  int reuseCount = 13;
			  int nextReuseCount = reuseCount + 2;
			  Neo4Net.Kernel.Api.Exceptions.Status_Transaction terminationReason = Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated;

			  KernelTransactionImplementation tx = NewNotInitializedTransaction();
			  InitializeAndClose( tx, reuseCount );

			  Neo4Net.Kernel.impl.locking.Locks_Client locksClient = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  SimpleStatementLocks statementLocks = new SimpleStatementLocks( locksClient );
			  tx.Initialize( 42, 42, statementLocks, KernelTransaction.Type.@implicit, LoginContext().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME), 0L, 0L );

			  assertFalse( tx.MarkForTermination( nextReuseCount, terminationReason ) );

			  assertFalse( tx.ReasonIfTerminated.Present );
			  verify( locksClient, never() ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeClosedTransactionIsNotAllowed() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseClosedTransactionIsNotAllowed()
		 {
			  KernelTransactionImplementation transaction = newTransaction( 1000 );
			  transaction.Close();

			  ExpectedException.expect( typeof( System.InvalidOperationException ) );
			  ExpectedException.expectMessage( "This transaction has already been completed." );
			  transaction.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void resetTransactionStatisticsOnRelease() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ResetTransactionStatisticsOnRelease()
		 {
			  KernelTransactionImplementation transaction = newTransaction( 1000 );
			  transaction.GetStatistics().addWaitingTime(1);
			  transaction.GetStatistics().addWaitingTime(1);
			  assertEquals( 2, transaction.GetStatistics().getWaitingTimeNanos(0) );
			  transaction.Close();
			  assertEquals( 0, transaction.GetStatistics().getWaitingTimeNanos(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportTransactionStatistics()
		 public virtual void ReportTransactionStatistics()
		 {
			  KernelTransactionImplementation transaction = newTransaction( 100 );
			  KernelTransactionImplementation.Statistics statistics = new KernelTransactionImplementation.Statistics( transaction, new AtomicReference<CpuClock>( new ThreadBasedCpuClock() ), new AtomicReference<HeapAllocation>(new ThreadBasedAllocation()) );
			  PredictablePageCursorTracer tracer = new PredictablePageCursorTracer();
			  statistics.Init( 2, tracer );

			  assertEquals( 2, statistics.CpuTimeMillis() );
			  assertEquals( 2, statistics.HeapAllocatedBytes() );
			  assertEquals( 1, statistics.TotalTransactionPageCacheFaults() );
			  assertEquals( 4, statistics.TotalTransactionPageCacheHits() );
			  statistics.AddWaitingTime( 1 );
			  assertEquals( 1, statistics.GetWaitingTimeNanos( 0 ) );

			  statistics.Reset();

			  statistics.Init( 4, tracer );
			  assertEquals( 4, statistics.CpuTimeMillis() );
			  assertEquals( 4, statistics.HeapAllocatedBytes() );
			  assertEquals( 2, statistics.TotalTransactionPageCacheFaults() );
			  assertEquals( 6, statistics.TotalTransactionPageCacheHits() );
			  assertEquals( 0, statistics.GetWaitingTimeNanos( 0 ) );
		 }

		 private LoginContext LoginContext()
		 {
			  return IsWriteTx ? AnonymousContext.write() : AnonymousContext.read();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void initializeAndClose(KernelTransactionImplementation tx, int times) throws Exception
		 private void InitializeAndClose( KernelTransactionImplementation tx, int times )
		 {
			  for ( int i = 0; i < times; i++ )
			  {
					SimpleStatementLocks statementLocks = new SimpleStatementLocks( new NoOpClient() );
					tx.Initialize( i + 10, i + 10, statementLocks, KernelTransaction.Type.@implicit, LoginContext().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME), 0L, 0L );
					tx.Close();
			  }
		 }

		 private class ThreadBasedCpuClock : CpuClock
		 {
			  internal long Iteration;
			  public override long CpuTimeNanos( long threadId )
			  {
					Iteration++;
					return MILLISECONDS.toNanos( Iteration * threadId );
			  }
		 }

		 private class ThreadBasedAllocation : HeapAllocation
		 {
			  internal long Iteration;
			  public override long AllocatedBytes( long threadId )
			  {
					Iteration++;
					return Iteration * threadId;
			  }
		 }

		 private class PredictablePageCursorTracer : DefaultPageCursorTracer
		 {
			  internal long Iteration = 1;

			  public override long AccumulatedHits()
			  {
					Iteration++;
					return Iteration * 2;
			  }

			  public override long AccumulatedFaults()
			  {
					return Iteration;
			  }
		 }
	}

}