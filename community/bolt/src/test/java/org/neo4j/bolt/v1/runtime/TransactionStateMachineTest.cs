using System;

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
namespace Org.Neo4j.Bolt.v1.runtime
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using InOrder = org.mockito.InOrder;

	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using BoltResultHandle = Org.Neo4j.Bolt.runtime.BoltResultHandle;
	using Bookmark = Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using FakeClock = Org.Neo4j.Time.FakeClock;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.security.auth.AuthenticationResult.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	internal class TransactionStateMachineTest
	{
		 private static readonly string _periodicCommitQuery = "USING PERIODIC COMMIT 1 " +
					"LOAD CSV FROM ''https://neo4j.com/test.csv'' AS line " +
					"CREATE (:Node {id: line[0], name: line[1]})";

		 private TransactionStateMachineV1SPI _stateMachineSPI;
		 private TransactionStateMachine.MutableTransactionState _mutableState;
		 private TransactionStateMachine _stateMachine;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void createMocks()
		 internal virtual void CreateMocks()
		 {
			  _stateMachineSPI = mock( typeof( TransactionStateMachineV1SPI ) );
			  _mutableState = mock( typeof( TransactionStateMachine.MutableTransactionState ) );
			  _stateMachine = new TransactionStateMachine( _stateMachineSPI, AUTH_DISABLED, new FakeClock() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTransitionToExplicitTransactionOnBegin() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldTransitionToExplicitTransactionOnBegin()
		 {
			  assertEquals( TransactionStateMachine.State.ExplicitTransaction, TransactionStateMachine.State.AutoCommit.beginTransaction( _mutableState, _stateMachineSPI, null, null, null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTransitionToAutoCommitOnCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldTransitionToAutoCommitOnCommit()
		 {
			  assertEquals( TransactionStateMachine.State.AutoCommit, TransactionStateMachine.State.ExplicitTransaction.commitTransaction( _mutableState, _stateMachineSPI ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTransitionToAutoCommitOnRollback() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldTransitionToAutoCommitOnRollback()
		 {
			  assertEquals( TransactionStateMachine.State.AutoCommit, TransactionStateMachine.State.ExplicitTransaction.rollbackTransaction( _mutableState, _stateMachineSPI ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnBeginInExplicitTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowOnBeginInExplicitTransaction()
		 {
			  QueryExecutionKernelException e = assertThrows( typeof( QueryExecutionKernelException ), () => TransactionStateMachine.State.ExplicitTransaction.beginTransaction(_mutableState, _stateMachineSPI, null, null, null) );

			  assertEquals( "Nested transactions are not supported.", e.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAllowRollbackInAutoCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAllowRollbackInAutoCommit()
		 {
			  assertEquals( TransactionStateMachine.State.AutoCommit, TransactionStateMachine.State.AutoCommit.rollbackTransaction( _mutableState, _stateMachineSPI ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnCommitInAutoCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowOnCommitInAutoCommit()
		 {
			  QueryExecutionKernelException e = assertThrows( typeof( QueryExecutionKernelException ), () => TransactionStateMachine.State.AutoCommit.commitTransaction(_mutableState, _stateMachineSPI) );

			  assertEquals( "No current transaction to commit.", e.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotWaitWhenNoBookmarkSupplied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotWaitWhenNoBookmarkSupplied()
		 {
			  _stateMachine.beginTransaction( null );
			  verify( _stateMachineSPI, never() ).awaitUpToDate(anyLong());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAwaitSingleBookmark() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAwaitSingleBookmark()
		 {
			  MapValue @params = Map( "bookmark", "neo4j:bookmark:v1:tx15" );
			  _stateMachine.beginTransaction( Bookmark.fromParamsOrNull( @params ) );
			  verify( _stateMachineSPI ).awaitUpToDate( 15 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAwaitMultipleBookmarks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAwaitMultipleBookmarks()
		 {
			  MapValue @params = Map( "bookmarks", asList( "neo4j:bookmark:v1:tx15", "neo4j:bookmark:v1:tx5", "neo4j:bookmark:v1:tx92", "neo4j:bookmark:v1:tx9" ) );
			  _stateMachine.beginTransaction( Bookmark.fromParamsOrNull( @params ) );
			  verify( _stateMachineSPI ).awaitUpToDate( 92 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAwaitMultipleBookmarksWhenBothSingleAndMultipleSupplied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAwaitMultipleBookmarksWhenBothSingleAndMultipleSupplied()
		 {
			  MapValue @params = Map( "bookmark", "neo4j:bookmark:v1:tx42", "bookmarks", asList( "neo4j:bookmark:v1:tx47", "neo4j:bookmark:v1:tx67", "neo4j:bookmark:v1:tx45" ) );
			  _stateMachine.beginTransaction( Bookmark.fromParamsOrNull( @params ) );
			  verify( _stateMachineSPI ).awaitUpToDate( 67 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStartWithAutoCommitState()
		 internal virtual void ShouldStartWithAutoCommitState()
		 {
			  TransactionStateMachineV1SPI stateMachineSPI = mock( typeof( TransactionStateMachineV1SPI ) );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.AutoCommit ) );
			  assertNull( stateMachine.Ctx.currentTransaction );
			  assertNull( stateMachine.Ctx.currentResultHandle );
			  assertNull( stateMachine.Ctx.currentResult );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDoNothingInAutoCommitTransactionUponInitialisationWhenValidated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDoNothingInAutoCommitTransactionUponInitialisationWhenValidated()
		 {
			  KernelTransaction transaction = NewTimedOutTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  // We're in auto-commit state
			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.AutoCommit ) );
			  assertNull( stateMachine.Ctx.currentTransaction );

			  // call validate transaction
			  stateMachine.ValidateTransaction();

			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.AutoCommit ) );
			  assertNull( stateMachine.Ctx.currentTransaction );

			  verify( transaction, never() ).ReasonIfTerminated;
			  verify( transaction, never() ).failure();
			  verify( transaction, never() ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldResetInAutoCommitTransactionWhileStatementIsRunningWhenValidated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldResetInAutoCommitTransactionWhileStatementIsRunningWhenValidated()
		 {
			  KernelTransaction transaction = NewTimedOutTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  // We're in auto-commit state
			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.AutoCommit ) );
			  assertNull( stateMachine.Ctx.currentTransaction );

			  stateMachine.Run( "RETURN 1", null );

			  // We're in auto-commit state
			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.AutoCommit ) );
			  assertNotNull( stateMachine.Ctx.currentTransaction );

			  // call validate transaction
			  stateMachine.ValidateTransaction();

			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.AutoCommit ) );
			  assertNull( stateMachine.Ctx.currentTransaction );
			  assertNull( stateMachine.Ctx.currentResult );
			  assertNull( stateMachine.Ctx.currentResultHandle );

			  verify( transaction, times( 1 ) ).ReasonIfTerminated;
			  verify( transaction, times( 1 ) ).failure();
			  verify( transaction, times( 1 ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldResetInExplicitTransactionUponTxBeginWhenValidated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldResetInExplicitTransactionUponTxBeginWhenValidated()
		 {
			  KernelTransaction transaction = NewTimedOutTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  // start an explicit transaction
			  stateMachine.BeginTransaction( null );
			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.ExplicitTransaction ) );
			  assertNotNull( stateMachine.Ctx.currentTransaction );

			  // verify transaction, which is timed out
			  stateMachine.ValidateTransaction();

			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.AutoCommit ) );
			  assertNull( stateMachine.Ctx.currentTransaction );
			  assertNull( stateMachine.Ctx.currentResult );
			  assertNull( stateMachine.Ctx.currentResultHandle );

			  verify( transaction, times( 1 ) ).ReasonIfTerminated;
			  verify( transaction, times( 1 ) ).failure();
			  verify( transaction, times( 1 ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldResetInExplicitTransactionWhileStatementIsRunningWhenValidated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldResetInExplicitTransactionWhileStatementIsRunningWhenValidated()
		 {
			  KernelTransaction transaction = NewTimedOutTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  // start an explicit transaction
			  stateMachine.BeginTransaction( null );
			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.ExplicitTransaction ) );
			  assertNotNull( stateMachine.Ctx.currentTransaction );

			  stateMachine.Run( "RETURN 1", null );

			  // verify transaction, which is timed out
			  stateMachine.ValidateTransaction();

			  assertThat( stateMachine.StateConflict, @is( TransactionStateMachine.State.AutoCommit ) );
			  assertNull( stateMachine.Ctx.currentTransaction );
			  assertNull( stateMachine.Ctx.currentResult );
			  assertNull( stateMachine.Ctx.currentResultHandle );

			  verify( transaction, times( 1 ) ).ReasonIfTerminated;
			  verify( transaction, times( 1 ) ).failure();
			  verify( transaction, times( 1 ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUnbindTxAfterRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUnbindTxAfterRun()
		 {
			  KernelTransaction transaction = NewTimedOutTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  stateMachine.Run( "SOME STATEMENT", null );

			  verify( stateMachineSPI, times( 1 ) ).unbindTransactionFromCurrentThread();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUnbindTxAfterStreamResult() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUnbindTxAfterStreamResult()
		 {
			  KernelTransaction transaction = NewTimedOutTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  stateMachine.Run( "SOME STATEMENT", null );
			  stateMachine.StreamResult(boltResult =>
			  {

			  });

			  verify( stateMachineSPI, times( 2 ) ).unbindTransactionFromCurrentThread();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowDuringRunIfPendingTerminationNoticeExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowDuringRunIfPendingTerminationNoticeExists()
		 {
			  KernelTransaction transaction = NewTimedOutTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  stateMachine.Ctx.pendingTerminationNotice = Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut;

			  TransactionTerminatedException e = assertThrows( typeof( TransactionTerminatedException ), () => stateMachine.Run("SOME STATEMENT", null) );

			  assertEquals( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut, e.Status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowDuringStreamResultIfPendingTerminationNoticeExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowDuringStreamResultIfPendingTerminationNoticeExists()
		 {
			  KernelTransaction transaction = NewTimedOutTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  stateMachine.Run( "SOME STATEMENT", null );
			  stateMachine.Ctx.pendingTerminationNotice = Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut;

			  TransactionTerminatedException e = assertThrows(typeof(TransactionTerminatedException), () =>
			  {
				stateMachine.StreamResult(boltResult =>
				{
				});
			  });
			  assertEquals( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut, e.Status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseResultAndTransactionHandlesWhenExecutionFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseResultAndTransactionHandlesWhenExecutionFails()
		 {
			  KernelTransaction transaction = NewTransaction();
			  BoltResultHandle resultHandle = NewResultHandle( new Exception( "some error" ) );
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction, resultHandle );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  Exception e = assertThrows( typeof( Exception ), () => stateMachine.Run("SOME STATEMENT", null) );
			  assertEquals( "some error", e.Message );

			  assertNull( stateMachine.Ctx.currentResultHandle );
			  assertNull( stateMachine.Ctx.currentResult );
			  assertNull( stateMachine.Ctx.currentTransaction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseResultAndTransactionHandlesWhenConsumeFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseResultAndTransactionHandlesWhenConsumeFails()
		 {
			  KernelTransaction transaction = NewTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  stateMachine.Run( "SOME STATEMENT", null );

			  assertNotNull( stateMachine.Ctx.currentResultHandle );
			  assertNotNull( stateMachine.Ctx.currentResult );

			  Exception e = assertThrows(typeof(Exception), () =>
			  {
				stateMachine.StreamResult(boltResult =>
				{
					 throw new Exception( "some error" );
				});
			  });
			  assertEquals( "some error", e.Message );

			  assertNull( stateMachine.Ctx.currentResultHandle );
			  assertNull( stateMachine.Ctx.currentResult );
			  assertNull( stateMachine.Ctx.currentTransaction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseResultHandlesWhenExecutionFailsInExplicitTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseResultHandlesWhenExecutionFailsInExplicitTransaction()
		 {
			  KernelTransaction transaction = NewTransaction();
			  BoltResultHandle resultHandle = NewResultHandle( new Exception( "some error" ) );
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction, resultHandle );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  Exception e = assertThrows(typeof(Exception), () =>
			  {
				stateMachine.BeginTransaction( null );
				stateMachine.StreamResult(boltResult =>
				{

				});
				stateMachine.Run( "SOME STATEMENT", null );
			  });
			  assertEquals( "some error", e.Message );

			  assertNull( stateMachine.Ctx.currentResultHandle );
			  assertNull( stateMachine.Ctx.currentResult );
			  assertNotNull( stateMachine.Ctx.currentTransaction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseResultHandlesWhenConsumeFailsInExplicitTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseResultHandlesWhenConsumeFailsInExplicitTransaction()
		 {
			  KernelTransaction transaction = NewTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  stateMachine.BeginTransaction( null );
			  stateMachine.StreamResult(boltResult =>
			  {

			  });
			  stateMachine.Run( "SOME STATEMENT", null );

			  assertNotNull( stateMachine.Ctx.currentResultHandle );
			  assertNotNull( stateMachine.Ctx.currentResult );

			  Exception e = assertThrows(typeof(Exception), () =>
			  {
				stateMachine.StreamResult(boltResult =>
				{
					 throw new Exception( "some error" );
				});
			  });
			  assertEquals( "some error", e.Message );

			  assertNull( stateMachine.Ctx.currentResultHandle );
			  assertNull( stateMachine.Ctx.currentResult );
			  assertNotNull( stateMachine.Ctx.currentTransaction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOpenExplicitTransactionForPeriodicCommitQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotOpenExplicitTransactionForPeriodicCommitQuery()
		 {
			  KernelTransaction transaction = NewTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );
			  when( stateMachineSPI.IsPeriodicCommit( _periodicCommitQuery ) ).thenReturn( true );

			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  stateMachine.Run( _periodicCommitQuery, EMPTY_MAP );

			  // transaction was created only to stream back result of the periodic commit query
			  assertEquals( transaction, stateMachine.Ctx.currentTransaction );

			  InOrder inOrder = inOrder( stateMachineSPI );
			  inOrder.verify( stateMachineSPI ).isPeriodicCommit( _periodicCommitQuery );
			  // periodic commit query was executed without starting an explicit transaction
			  inOrder.verify( stateMachineSPI ).executeQuery( any( typeof( LoginContext ) ), eq( _periodicCommitQuery ), eq( EMPTY_MAP ), any(), any() );
			  // explicit transaction was started only after query execution to stream the result
			  inOrder.verify( stateMachineSPI ).beginTransaction( any( typeof( LoginContext ) ), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMarkForTerminationWhenNoTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotMarkForTerminationWhenNoTransaction()
		 {
			  KernelTransaction transaction = NewTransaction();
			  TransactionStateMachineV1SPI stateMachineSPI = NewTransactionStateMachineSPI( transaction );

			  TransactionStateMachine stateMachine = NewTransactionStateMachine( stateMachineSPI );

			  stateMachine.MarkCurrentTransactionForTermination();
			  verify( transaction, never() ).markForTermination(any());
		 }

		 private static KernelTransaction NewTransaction()
		 {
			  KernelTransaction transaction = mock( typeof( KernelTransaction ) );

			  when( transaction.Open ).thenReturn( true );

			  return transaction;
		 }

		 private static KernelTransaction NewTimedOutTransaction()
		 {
			  KernelTransaction transaction = NewTransaction();

			  when( transaction.ReasonIfTerminated ).thenReturn( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut );

			  return transaction;
		 }

		 private static TransactionStateMachine NewTransactionStateMachine( TransactionStateMachineV1SPI stateMachineSPI )
		 {
			  return new TransactionStateMachine( stateMachineSPI, AUTH_DISABLED, new FakeClock() );
		 }

		 private static MapValue Map( params object[] keyValues )
		 {
			  return ValueUtils.asMapValue( MapUtil.map( keyValues ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static TransactionStateMachineV1SPI newTransactionStateMachineSPI(org.neo4j.kernel.api.KernelTransaction transaction) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private static TransactionStateMachineV1SPI NewTransactionStateMachineSPI( KernelTransaction transaction )
		 {
			  BoltResultHandle resultHandle = NewResultHandle();
			  TransactionStateMachineV1SPI stateMachineSPI = mock( typeof( TransactionStateMachineV1SPI ) );

			  when( stateMachineSPI.BeginTransaction( any(), any(), any() ) ).thenReturn(transaction);
			  when( stateMachineSPI.ExecuteQuery( any(), anyString(), any(), any(), any() ) ).thenReturn(resultHandle);

			  return stateMachineSPI;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static TransactionStateMachineV1SPI newTransactionStateMachineSPI(org.neo4j.kernel.api.KernelTransaction transaction, org.neo4j.bolt.runtime.BoltResultHandle resultHandle) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private static TransactionStateMachineV1SPI NewTransactionStateMachineSPI( KernelTransaction transaction, BoltResultHandle resultHandle )
		 {
			  TransactionStateMachineV1SPI stateMachineSPI = mock( typeof( TransactionStateMachineV1SPI ) );

			  when( stateMachineSPI.BeginTransaction( any(), any(), any() ) ).thenReturn(transaction);
			  when( stateMachineSPI.ExecuteQuery( any(), anyString(), any(), any(), any() ) ).thenReturn(resultHandle);

			  return stateMachineSPI;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.bolt.runtime.BoltResultHandle newResultHandle() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private static BoltResultHandle NewResultHandle()
		 {
			  BoltResultHandle resultHandle = mock( typeof( BoltResultHandle ) );

			  when( resultHandle.Start() ).thenReturn(BoltResult.EMPTY);

			  return resultHandle;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.bolt.runtime.BoltResultHandle newResultHandle(Throwable t) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private static BoltResultHandle NewResultHandle( Exception t )
		 {
			  BoltResultHandle resultHandle = mock( typeof( BoltResultHandle ) );

			  when( resultHandle.Start() ).thenThrow(t);

			  return resultHandle;
		 }
	}

}