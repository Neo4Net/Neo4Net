using System;
using System.Collections.Generic;
using System.Text;

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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using DatabaseShutdownException = Neo4Net.GraphDb.DatabaseShutdownException;
	using AuthorizationExpiredException = Neo4Net.GraphDb.security.AuthorizationExpiredException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using AuxiliaryTransactionStateManager = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using StandardConstraintSemantics = Neo4Net.Kernel.impl.constraints.StandardConstraintSemantics;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using CanWrite = Neo4Net.Kernel.impl.factory.CanWrite;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using SimpleStatementLocksFactory = Neo4Net.Kernel.impl.locking.SimpleStatementLocksFactory;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Tracers = Neo4Net.Kernel.monitoring.tracing.Tracers;
	using NullLog = Neo4Net.Logging.NullLog;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;
	using ResourceLocker = Neo4Net.Storageengine.Api.@lock.ResourceLocker;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Neo4Net.Storageengine.Api.txstate.TxStateVisitor;
	using Neo4Net.Test;
	using Race = Neo4Net.Test.Race;
	using Neo4Net.Test.rule.concurrent;
	using Clocks = Neo4Net.Time.Clocks;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
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
//	import static org.mockito.ArgumentMatchers.anyCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.TransactionHeaderInformationFactory.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.collection.CollectionsFactorySupplier_Fields.ON_HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.MockedNeoStores.mockedTokenHolders;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.assertion.Assert.assertException;

	public class KernelTransactionsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>("T2-" + getClass().getName());
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>( "T2-" + this.GetType().FullName );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

		 private const long TEST_TIMEOUT = 10_000;
		 private static readonly SystemNanoClock _clock = Clocks.nanoClock();
		 private static DatabaseAvailabilityGuard _databaseAvailabilityGuard;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _databaseAvailabilityGuard = new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, _clock, NullLog.Instance );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListActiveTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListActiveTransactions()
		 {
			  // Given
			  KernelTransactions transactions = NewTestKernelTransactions();

			  // When
			  KernelTransaction first = GetKernelTransaction( transactions );
			  KernelTransaction second = GetKernelTransaction( transactions );
			  KernelTransaction third = GetKernelTransaction( transactions );

			  first.Close();

			  // Then
			  assertThat( transactions.ActiveTransactions(), equalTo(asSet(NewHandle(second), NewHandle(third))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisposeTransactionsWhenAsked() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisposeTransactionsWhenAsked()
		 {
			  // Given
			  KernelTransactions transactions = NewKernelTransactions();

			  transactions.DisposeAll();

			  KernelTransaction first = GetKernelTransaction( transactions );
			  KernelTransaction second = GetKernelTransaction( transactions );
			  KernelTransaction leftOpen = GetKernelTransaction( transactions );
			  first.Close();
			  second.Close();

			  // When
			  transactions.DisposeAll();

			  // Then
			  KernelTransaction postDispose = GetKernelTransaction( transactions );
			  assertThat( postDispose, not( equalTo( first ) ) );
			  assertThat( postDispose, not( equalTo( second ) ) );

			  assertNotNull( leftOpen.ReasonIfTerminated );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeRandomBytesInAdditionalHeader() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeRandomBytesInAdditionalHeader()
		 {
			  // Given
			  TransactionRepresentation[] transactionRepresentation = new TransactionRepresentation[1];

			  KernelTransactions registry = NewKernelTransactions( NewRememberingCommitProcess( transactionRepresentation ) );

			  // When
			  using ( KernelTransaction transaction = GetKernelTransaction( registry ) )
			  {
					// Just pick anything that can flag that changes have been made to this transaction
					( ( KernelTransactionImplementation ) transaction ).TxState().nodeDoCreate(0);
					transaction.Success();
			  }

			  // Then
			  sbyte[] additionalHeader = transactionRepresentation[0].AdditionalHeader();
			  assertNotNull( additionalHeader );
			  assertTrue( additionalHeader.Length > 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseClosedTransactionObjects() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReuseClosedTransactionObjects()
		 {
			  // GIVEN
			  KernelTransactions transactions = NewKernelTransactions();
			  KernelTransaction a = GetKernelTransaction( transactions );

			  // WHEN
			  a.Close();
			  KernelTransaction b = GetKernelTransaction( transactions );

			  // THEN
			  assertSame( a, b );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTellWhenTransactionsFromSnapshotHaveBeenClosed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTellWhenTransactionsFromSnapshotHaveBeenClosed()
		 {
			  // GIVEN
			  KernelTransactions transactions = NewKernelTransactions();
			  KernelTransaction a = GetKernelTransaction( transactions );
			  KernelTransaction b = GetKernelTransaction( transactions );
			  KernelTransaction c = GetKernelTransaction( transactions );
			  KernelTransactionsSnapshot snapshot = transactions.Get();
			  assertFalse( snapshot.AllClosed() );

			  // WHEN a gets closed
			  a.Close();
			  assertFalse( snapshot.AllClosed() );

			  // WHEN c gets closed and (test knowing too much) that instance getting reused in another transaction "d".
			  c.Close();
			  KernelTransaction d = GetKernelTransaction( transactions );
			  assertFalse( snapshot.AllClosed() );

			  // WHEN b finally gets closed
			  b.Close();
			  assertTrue( snapshot.AllClosed() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSnapshotDuringHeavyLoad() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSnapshotDuringHeavyLoad()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final KernelTransactions transactions = newKernelTransactions();
			  KernelTransactions transactions = NewKernelTransactions();
			  Race race = new Race();
			  const int threads = 50;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean end = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean end = new AtomicBoolean();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReferenceArray<KernelTransactionsSnapshot> snapshots = new java.util.concurrent.atomic.AtomicReferenceArray<>(threads);
			  AtomicReferenceArray<KernelTransactionsSnapshot> snapshots = new AtomicReferenceArray<KernelTransactionsSnapshot>( threads );

			  // Representing "transaction" threads
			  for ( int i = 0; i < threads; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int threadIndex = i;
					int threadIndex = i;
					race.AddContestant(() =>
					{
					 ThreadLocalRandom random = ThreadLocalRandom.current();
					 while ( !end.get() )
					 {
						  try
						  {
							  using ( KernelTransaction transaction = GetKernelTransaction( transactions ) )
							  {
									KernelTransactionsSnapshot snapshot = null;
									try
									{
										 parkNanos( MILLISECONDS.toNanos( random.Next( 3 ) ) );
										 if ( snapshots.get( threadIndex ) == null )
										 {
											  requireNonNull( transactions, "transactions is null" );
											  snapshot = requireNonNull( transactions.Get(), "transactions.get() returned null" );
											  snapshots.set( threadIndex, snapshot );
											  parkNanos( MILLISECONDS.toNanos( random.Next( 3 ) ) );
										 }
									}
									catch ( Exception e )
									{
										 StringBuilder sb = ( new StringBuilder( "Gotcha!\n" ) ).Append( "threadIndex=" ).Append( threadIndex ).Append( '\n' ).Append( "transaction=" ).Append( transaction ).Append( '\n' ).Append( "snapshots=" ).Append( snapshots ).Append( '\n' ).Append( "snapshot=" ).Append( snapshot ).Append( '\n' ).Append( "end=" ).Append( end );
										 throw new Exception( sb.ToString(), e );
									}
							  }
						  }
						  catch ( TransactionFailureException e )
						  {
								throw new Exception( e );
						  }
					 }
					});
			  }

			  // Just checks snapshots
			  race.AddContestant(() =>
			  {
				ThreadLocalRandom random = ThreadLocalRandom.current();
				int snapshotsLeft = 1_000;
				while ( snapshotsLeft > 0 )
				{
					 int threadIndex = random.Next( threads );
					 KernelTransactionsSnapshot snapshot = snapshots.get( threadIndex );
					 if ( snapshot != null && snapshot.AllClosed() )
					 {
						  snapshotsLeft--;
						  snapshots.set( threadIndex, null );
					 }
				}

				// End condition of this test can be described as:
				//   when 1000 snapshots have been seen as closed.
				// setting this boolean to true will have all other threads end as well so that race.go() will end
				end.set( true );
			  });

			  // WHEN
			  race.Go();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionCloseRemovesTxFromActiveTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionCloseRemovesTxFromActiveTransactions()
		 {
			  KernelTransactions kernelTransactions = NewTestKernelTransactions();

			  KernelTransaction tx1 = GetKernelTransaction( kernelTransactions );
			  KernelTransaction tx2 = GetKernelTransaction( kernelTransactions );
			  KernelTransaction tx3 = GetKernelTransaction( kernelTransactions );

			  tx1.Close();
			  tx3.Close();

			  assertEquals( asSet( NewHandle( tx2 ) ), kernelTransactions.ActiveTransactions() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void disposeAllMarksAllTransactionsForTermination() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DisposeAllMarksAllTransactionsForTermination()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();

			  KernelTransaction tx1 = GetKernelTransaction( kernelTransactions );
			  KernelTransaction tx2 = GetKernelTransaction( kernelTransactions );
			  KernelTransaction tx3 = GetKernelTransaction( kernelTransactions );

			  kernelTransactions.DisposeAll();

			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, tx1.ReasonIfTerminated.get() );
			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, tx2.ReasonIfTerminated.get() );
			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, tx3.ReasonIfTerminated.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionClosesUnderlyingStoreReaderWhenDisposed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionClosesUnderlyingStoreReaderWhenDisposed()
		 {
			  StorageReader storeStatement1 = mock( typeof( StorageReader ) );
			  StorageReader storeStatement2 = mock( typeof( StorageReader ) );
			  StorageReader storeStatement3 = mock( typeof( StorageReader ) );

			  KernelTransactions kernelTransactions = newKernelTransactions( mock( typeof( TransactionCommitProcess ) ), storeStatement1, storeStatement2, storeStatement3 );

			  // start and close 3 transactions from different threads
			  StartAndCloseTransaction( kernelTransactions );
			  Executors.newSingleThreadExecutor().submit(() => startAndCloseTransaction(kernelTransactions)).get();
			  Executors.newSingleThreadExecutor().submit(() => startAndCloseTransaction(kernelTransactions)).get();

			  kernelTransactions.DisposeAll();

			  verify( storeStatement1 ).close();
			  verify( storeStatement2 ).close();
			  verify( storeStatement3 ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threadThatBlocksNewTxsCantStartNewTxs() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreadThatBlocksNewTxsCantStartNewTxs()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();
			  kernelTransactions.BlockNewTransactions();
			  try
			  {
					kernelTransactions.NewInstance( KernelTransaction.Type.@implicit, AnonymousContext.write(), 0L );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void blockNewTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BlockNewTransactions()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();
			  kernelTransactions.BlockNewTransactions();

			  Future<KernelTransaction> txOpener = T2.execute( state => kernelTransactions.NewInstance( @explicit, AnonymousContext.write(), 0L ) );
			  T2.get().waitUntilWaiting(location => location.isAt(typeof(KernelTransactions), "newInstance"));

			  AssertNotDone( txOpener );

			  kernelTransactions.UnblockNewTransactions();
			  assertNotNull( txOpener.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void unblockNewTransactionsFromWrongThreadThrows() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnblockNewTransactionsFromWrongThreadThrows()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();
			  kernelTransactions.BlockNewTransactions();

			  Future<KernelTransaction> txOpener = T2.execute( state => kernelTransactions.NewInstance( @explicit, AnonymousContext.write(), 0L ) );
			  T2.get().waitUntilWaiting(location => location.isAt(typeof(KernelTransactions), "newInstance"));

			  AssertNotDone( txOpener );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> wrongUnblocker = unblockTxsInSeparateThread(kernelTransactions);
			  Future<object> wrongUnblocker = UnblockTxsInSeparateThread( kernelTransactions );

			  try
			  {
					wrongUnblocker.get();
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( ExecutionException ) ) );
					assertThat( e.InnerException, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
			  AssertNotDone( txOpener );

			  kernelTransactions.UnblockNewTransactions();
			  assertNotNull( txOpener.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLeakTransactionOnSecurityContextFreezeFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLeakTransactionOnSecurityContextFreezeFailure()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();
			  LoginContext loginContext = mock( typeof( LoginContext ) );
			  when( loginContext.Authorize( any(), any() ) ).thenThrow(new AuthorizationExpiredException("Freeze failed."));

			  assertException( () => kernelTransactions.NewInstance(KernelTransaction.Type.@explicit, loginContext, 0L), typeof(AuthorizationExpiredException), "Freeze failed." );

			  assertThat( "We should not have any transaction", kernelTransactions.ActiveTransactions(), @is(empty()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exceptionWhenStartingNewTransactionOnShutdownInstance() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExceptionWhenStartingNewTransactionOnShutdownInstance()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();

			  _databaseAvailabilityGuard.shutdown();

			  ExpectedException.expect( typeof( DatabaseShutdownException ) );
			  kernelTransactions.NewInstance( KernelTransaction.Type.@explicit, AUTH_DISABLED, 0L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exceptionWhenStartingNewTransactionOnStoppedKernelTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExceptionWhenStartingNewTransactionOnStoppedKernelTransactions()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();

			  T2.execute((OtherThreadExecutor.WorkerCommand<Void, Void>) state =>
			  {
				StopKernelTransactions( kernelTransactions );
				return null;
			  }).get();

			  ExpectedException.expect( typeof( System.InvalidOperationException ) );
			  kernelTransactions.NewInstance( KernelTransaction.Type.@explicit, AUTH_DISABLED, 0L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startNewTransactionOnRestartedKErnelTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartNewTransactionOnRestartedKErnelTransactions()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();

			  kernelTransactions.Stop();
			  kernelTransactions.Start();
			  assertNotNull( "New transaction created by restarted kernel transactions component.", kernelTransactions.NewInstance( KernelTransaction.Type.@explicit, AUTH_DISABLED, 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementalUserTransactionId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncrementalUserTransactionId()
		 {
			  KernelTransactions kernelTransactions = NewKernelTransactions();
			  using ( KernelTransaction kernelTransaction = kernelTransactions.NewInstance( KernelTransaction.Type.@explicit, AnonymousContext.none(), 0L ) )
			  {
					assertEquals( 1, kernelTransactions.ActiveTransactions().GetEnumerator().next().UserTransactionId );
			  }

			  using ( KernelTransaction kernelTransaction = kernelTransactions.NewInstance( KernelTransaction.Type.@explicit, AnonymousContext.none(), 0L ) )
			  {
					assertEquals( 2, kernelTransactions.ActiveTransactions().GetEnumerator().next().UserTransactionId );
			  }

			  using ( KernelTransaction kernelTransaction = kernelTransactions.NewInstance( KernelTransaction.Type.@explicit, AnonymousContext.none(), 0L ) )
			  {
					assertEquals( 3, kernelTransactions.ActiveTransactions().GetEnumerator().next().UserTransactionId );
			  }
		 }

		 private static void StopKernelTransactions( KernelTransactions kernelTransactions )
		 {
			  try
			  {
					kernelTransactions.Stop();
			  }
			  catch ( Exception t )
			  {
					throw new Exception( t );
			  }
		 }

		 private static void StartAndCloseTransaction( KernelTransactions kernelTransactions )
		 {
			  try
			  {
					kernelTransactions.NewInstance( KernelTransaction.Type.@explicit, AUTH_DISABLED, 0L ).close();
			  }
			  catch ( TransactionFailureException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static KernelTransactions newKernelTransactions() throws Throwable
		 private static KernelTransactions NewKernelTransactions()
		 {
			  return NewKernelTransactions( mock( typeof( TransactionCommitProcess ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static KernelTransactions newTestKernelTransactions() throws Throwable
		 private static KernelTransactions NewTestKernelTransactions()
		 {
			  return newKernelTransactions( true, mock( typeof( TransactionCommitProcess ) ), mock( typeof( StorageReader ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static KernelTransactions newKernelTransactions(TransactionCommitProcess commitProcess) throws Throwable
		 private static KernelTransactions NewKernelTransactions( TransactionCommitProcess commitProcess )
		 {
			  return newKernelTransactions( false, commitProcess, mock( typeof( StorageReader ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static KernelTransactions newKernelTransactions(TransactionCommitProcess commitProcess, org.Neo4Net.storageengine.api.StorageReader firstReader, org.Neo4Net.storageengine.api.StorageReader... otherReaders) throws Throwable
		 private static KernelTransactions NewKernelTransactions( TransactionCommitProcess commitProcess, StorageReader firstReader, params StorageReader[] otherReaders )
		 {
			  return NewKernelTransactions( false, commitProcess, firstReader, otherReaders );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static KernelTransactions newKernelTransactions(boolean testKernelTransactions, TransactionCommitProcess commitProcess, org.Neo4Net.storageengine.api.StorageReader firstReader, org.Neo4Net.storageengine.api.StorageReader... otherReaders) throws Throwable
		 private static KernelTransactions NewKernelTransactions( bool testKernelTransactions, TransactionCommitProcess commitProcess, StorageReader firstReader, params StorageReader[] otherReaders )
		 {
			  Locks locks = mock( typeof( Locks ) );
			  Neo4Net.Kernel.impl.locking.Locks_Client client = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
			  when( locks.NewClient() ).thenReturn(client);

			  StorageEngine storageEngine = mock( typeof( StorageEngine ) );
			  when( storageEngine.NewReader() ).thenReturn(firstReader, otherReaders);
			  doAnswer(invocation =>
			  {
				ICollection<StorageCommand> argument = invocation.getArgument( 0 );
				argument.add( mock( typeof( StorageCommand ) ) );
				return null;
			  }).when( storageEngine ).createCommands( anyCollection(), any(typeof(ReadableTransactionState)), any(typeof(StorageReader)), any(typeof(ResourceLocker)), anyLong(), any(typeof(TxStateVisitor.Decorator)) );

			  return NewKernelTransactions( locks, storageEngine, commitProcess, testKernelTransactions );
		 }

		 private static KernelTransactions NewKernelTransactions( Locks locks, StorageEngine storageEngine, TransactionCommitProcess commitProcess, bool testKernelTransactions )
		 {
			  LifeSupport life = new LifeSupport();
			  life.Start();

			  TransactionIdStore transactionIdStore = mock( typeof( TransactionIdStore ) );
			  when( transactionIdStore.LastCommittedTransaction ).thenReturn( new TransactionId( 0, 0, 0 ) );

			  Tracers tracers = new Tracers( "null", NullLog.Instance, new Monitors(), mock(typeof(JobScheduler)), _clock );
			  StatementLocksFactory statementLocksFactory = new SimpleStatementLocksFactory( locks );

			  StatementOperationParts statementOperations = mock( typeof( StatementOperationParts ) );
			  KernelTransactions transactions;
			  if ( testKernelTransactions )
			  {
					transactions = CreateTestTransactions( storageEngine, commitProcess, transactionIdStore, tracers, statementLocksFactory, statementOperations, _clock, _databaseAvailabilityGuard );
			  }
			  else
			  {
					transactions = CreateTransactions( storageEngine, commitProcess, transactionIdStore, tracers, statementLocksFactory, statementOperations, _clock, _databaseAvailabilityGuard );
			  }
			  transactions.Start();
			  return transactions;
		 }

		 private static KernelTransactions CreateTransactions( StorageEngine storageEngine, TransactionCommitProcess commitProcess, TransactionIdStore transactionIdStore, Tracers tracers, StatementLocksFactory statementLocksFactory, StatementOperationParts statementOperations, SystemNanoClock clock, AvailabilityGuard databaseAvailabilityGuard )
		 {
			  return new KernelTransactions( Config.defaults(), statementLocksFactory, null, statementOperations, null, DEFAULT, commitProcess, mock(typeof(AuxiliaryTransactionStateManager)), new TransactionHooks(), mock(typeof(TransactionMonitor)), databaseAvailabilityGuard, tracers, storageEngine, new Procedures(), transactionIdStore, clock, new AtomicReference<CpuClock>(CpuClock.NOT_AVAILABLE), new AtomicReference<HeapAllocation>(HeapAllocation.NOT_AVAILABLE), new CanWrite(), AutoIndexing.UNSUPPORTED, mock(typeof(ExplicitIndexStore)), EmptyVersionContextSupplier.EMPTY, ON_HEAP, mock(typeof(ConstraintSemantics)), mock(typeof(SchemaState)), mock(typeof(IndexingService)), mockedTokenHolders(), DEFAULT_DATABASE_NAME, new Dependencies() );
		 }

		 private static TestKernelTransactions CreateTestTransactions( StorageEngine storageEngine, TransactionCommitProcess commitProcess, TransactionIdStore transactionIdStore, Tracers tracers, StatementLocksFactory statementLocksFactory, StatementOperationParts statementOperations, SystemNanoClock clock, AvailabilityGuard databaseAvailabilityGuard )
		 {
			  return new TestKernelTransactions( statementLocksFactory, null, statementOperations, null, DEFAULT, commitProcess, mock( typeof( AuxiliaryTransactionStateManager ) ), new TransactionHooks(), mock(typeof(TransactionMonitor)), databaseAvailabilityGuard, tracers, storageEngine, new Procedures(), transactionIdStore, clock, new CanWrite(), AutoIndexing.UNSUPPORTED, EmptyVersionContextSupplier.EMPTY, mockedTokenHolders(), new Dependencies() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static TransactionCommitProcess newRememberingCommitProcess(final org.Neo4Net.kernel.impl.transaction.TransactionRepresentation[] slot) throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private static TransactionCommitProcess NewRememberingCommitProcess( TransactionRepresentation[] slot )
		 {
			  TransactionCommitProcess commitProcess = mock( typeof( TransactionCommitProcess ) );

			  when( commitProcess.Commit( any( typeof( TransactionToApply ) ), any( typeof( CommitEvent ) ), any( typeof( TransactionApplicationMode ) ) ) ).then(invocation =>
			  {
						  slot[0] = ( ( TransactionToApply ) invocation.getArgument( 0 ) ).TransactionRepresentation();
						  return 1L;
			  });

			  return commitProcess;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static java.util.concurrent.Future<?> unblockTxsInSeparateThread(final KernelTransactions kernelTransactions)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private static Future<object> UnblockTxsInSeparateThread( KernelTransactions kernelTransactions )
		 {
			  return Executors.newSingleThreadExecutor().submit(kernelTransactions.unblockNewTransactions);
		 }

		 private static void AssertNotDone<T1>( Future<T1> future )
		 {
			  assertFalse( future.Done );
		 }

		 private static KernelTransactionHandle NewHandle( KernelTransaction tx )
		 {
			  return new TestKernelTransactionHandle( tx );
		 }

		 private static KernelTransaction GetKernelTransaction( KernelTransactions transactions )
		 {
			  return transactions.NewInstance( KernelTransaction.Type.@implicit, AnonymousContext.none(), 0L );
		 }

		 private class TestKernelTransactions : KernelTransactions
		 {
			  internal TestKernelTransactions( StatementLocksFactory statementLocksFactory, ConstraintIndexCreator constraintIndexCreator, StatementOperationParts statementOperations, SchemaWriteGuard schemaWriteGuard, TransactionHeaderInformationFactory txHeaderFactory, TransactionCommitProcess transactionCommitProcess, AuxiliaryTransactionStateManager auxTxStateManager, TransactionHooks hooks, TransactionMonitor transactionMonitor, AvailabilityGuard databaseAvailabilityGuard, Tracers tracers, StorageEngine storageEngine, Procedures procedures, TransactionIdStore transactionIdStore, SystemNanoClock clock, AccessCapability accessCapability, AutoIndexing autoIndexing, VersionContextSupplier versionContextSupplier, TokenHolders tokenHolders, Dependencies dataSourceDependencies ) : base( Config.defaults(), statementLocksFactory, constraintIndexCreator, statementOperations, schemaWriteGuard, txHeaderFactory, transactionCommitProcess, auxTxStateManager, hooks, transactionMonitor, databaseAvailabilityGuard, tracers, storageEngine, procedures, transactionIdStore, clock, new AtomicReference<CpuClock>(CpuClock.NOT_AVAILABLE), new AtomicReference<HeapAllocation>(HeapAllocation.NOT_AVAILABLE), accessCapability, autoIndexing, mock(typeof(ExplicitIndexStore)), versionContextSupplier, ON_HEAP, new StandardConstraintSemantics(), mock(typeof(SchemaState)), mock(typeof(IndexingService)), tokenHolders, DEFAULT_DATABASE_NAME, dataSourceDependencies )
			  {
			  }

			  internal override KernelTransactionHandle CreateHandle( KernelTransactionImplementation tx )
			  {
					return new TestKernelTransactionHandle( tx );
			  }
		 }
	}

}