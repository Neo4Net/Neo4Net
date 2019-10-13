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
namespace Neo4Net.Bolt.v1.runtime
{
	using Test = org.junit.Test;


	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltConnectionAuthFatality = Neo4Net.Bolt.runtime.BoltConnectionAuthFatality;
	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using Neo4jError = Neo4Net.Bolt.runtime.Neo4jError;
	using TransactionStateMachineSPI = Neo4Net.Bolt.runtime.TransactionStateMachineSPI;
	using BoltResponseRecorder = Neo4Net.Bolt.testing.BoltResponseRecorder;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using Neo4Net.Functions;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using AuthorizationExpiredException = Neo4Net.Graphdb.security.AuthorizationExpiredException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.canReset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.failedWithStatus;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.hasNoTransaction;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.hasTransaction;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.inState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.isClosed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.succeeded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.verifyOneResponse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.wasIgnored;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.MachineRoom.EMPTY_PARAMS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.MachineRoom.USER_AGENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.MachineRoom.init;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.MachineRoom.newMachine;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.MachineRoom.newMachineWithTransaction;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.MachineRoom.newMachineWithTransactionSPI;

	public class BoltStateMachineV1Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allStateTransitionsShouldSendExactlyOneResponseToTheClient() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllStateTransitionsShouldSendExactlyOneResponseToTheClient()
		 {
			  IList<RequestMessage> messages = Arrays.asList( new InitMessage( USER_AGENT, emptyMap() ), AckFailureMessage.INSTANCE, ResetMessage.INSTANCE, new RunMessage("RETURN 1", EMPTY_PARAMS), DiscardAllMessage.INSTANCE, PullAllMessage.INSTANCE );

			  foreach ( RequestMessage message in messages )
			  {
					verifyOneResponse( ( machine, recorder ) => machine.process( message, recorder ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void initialStateShouldBeConnected()
		 public virtual void InitialStateShouldBeConnected()
		 {
			  assertThat( newMachine(), inState(typeof(ConnectedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackOpenTransactionOnReset() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackOpenTransactionOnReset()
		 {
			  // Given a FAILED machine with an open transaction
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = newMachineWithTransaction();
			  BoltStateMachine machine = newMachineWithTransaction();
			  machine.MarkFailed( Neo4jError.from( new Exception() ) );

			  // When RESET occurs
			  machine.Process( ResetMessage.INSTANCE, nullResponseHandler() );

			  // Then the transaction should have been rolled back...
			  assertThat( machine, hasNoTransaction() );

			  // ...and the machine should go back to READY
			  assertThat( machine, inState( typeof( ReadyState ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackOpenTransactionOnClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackOpenTransactionOnClose()
		 {
			  // Given a ready machine with an open transaction
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = newMachineWithTransaction();
			  BoltStateMachine machine = newMachineWithTransaction();

			  // When the machine is shut down
			  machine.Close();

			  // Then the transaction should have been rolled back
			  assertThat( machine, hasNoTransaction() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToResetWhenInReadyState() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToResetWhenInReadyState()
		 {
			  BoltStateMachine machine = init( newMachine() );
			  assertThat( machine, canReset() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetWithOpenTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetWithOpenTransaction()
		 {
			  BoltStateMachine machine = newMachineWithTransaction();
			  assertThat( machine, canReset() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetWithOpenTransactionAndOpenResult() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetWithOpenTransactionAndOpenResult()
		 {
			  // Given a ready machine with an open transaction...
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = newMachineWithTransaction();
			  BoltStateMachine machine = newMachineWithTransaction();

			  // ...and an open result
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), nullResponseHandler() );

			  // Then
			  assertThat( machine, canReset() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetWithOpenResult() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetWithOpenResult()
		 {
			  // Given a ready machine...
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = init(newMachine());
			  BoltStateMachine machine = init( newMachine() );

			  // ...and an open result
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), nullResponseHandler() );

			  // Then
			  assertThat( machine, canReset() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenOutOfOrderRollback() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenOutOfOrderRollback()
		 {
			  // Given a failed machine
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = newMachine();
			  BoltStateMachine machine = newMachine();
			  machine.MarkFailed( Neo4jError.from( new Exception() ) );

			  // When
			  machine.Process( new RunMessage( "ROLLBACK", EMPTY_PARAMS ), nullResponseHandler() );

			  // Then
			  assertThat( machine, inState( typeof( FailedState ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGoBackToReadyAfterAckFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGoBackToReadyAfterAckFailure()
		 {
			  // Given a failed machine
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = newMachine();
			  BoltStateMachine machine = newMachine();
			  machine.MarkFailed( Neo4jError.from( new Exception() ) );

			  // When
			  machine.Process( AckFailureMessage.INSTANCE, nullResponseHandler() );

			  // Then
			  assertThat( machine, inState( typeof( ReadyState ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRollbackOpenTransactionOnAckFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRollbackOpenTransactionOnAckFailure()
		 {
			  // Given a ready machine with an open transaction
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = newMachineWithTransaction();
			  BoltStateMachine machine = newMachineWithTransaction();

			  // ...and (for some reason) a FAILED state
			  machine.MarkFailed( Neo4jError.from( new Exception() ) );

			  // When the failure is acknowledged
			  machine.Process( AckFailureMessage.INSTANCE, nullResponseHandler() );

			  // Then the transaction should still be open
			  assertThat( machine, hasTransaction() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemainStoppedAfterInterrupted() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemainStoppedAfterInterrupted()
		 {
			  // Given a ready machine
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = init(newMachine());
			  BoltStateMachine machine = init( newMachine() );

			  // ...which is subsequently closed
			  machine.Close();
			  assertThat( machine, Closed );

			  // When and interrupt and reset occurs
			  machine.Interrupt();
			  machine.Process( ResetMessage.INSTANCE, nullResponseHandler() );

			  // Then the machine should remain closed
			  assertThat( machine, Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToKillMessagesAheadInLineWithAnInterrupt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToKillMessagesAheadInLineWithAnInterrupt()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = init(newMachine());
			  BoltStateMachine machine = init( newMachine() );

			  // When
			  machine.Interrupt();

			  // ...and
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), recorder );
			  machine.Process( ResetMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), wasIgnored() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleInterruptsShouldBeMatchedWithMultipleResets() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleInterruptsShouldBeMatchedWithMultipleResets()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = init(newMachine());
			  BoltStateMachine machine = init( newMachine() );

			  // When
			  machine.Interrupt();
			  machine.Interrupt();

			  // ...and
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), recorder );
			  machine.Process( ResetMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), wasIgnored() );
			  assertThat( recorder.NextResponse(), wasIgnored() );
			  assertThat( recorder.NextResponse(), wasIgnored() );

			  // But when
			  recorder.Reset();
			  machine.Process( ResetMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPublishingError() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestPublishingError()
		 {
			  // Given a new ready machine...
			  BoltStateMachine machine = init( newMachine() );

			  // ...and a result ready to be retrieved...
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), nullResponseHandler() );

			  // ...and a handler guaranteed to break
			  BoltResponseRecorder recorder = new BoltResponseRecorderAnonymousInnerClass( this );

			  // When we pull using that handler
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // Then the breakage should surface as a FAILURE
			  assertThat( recorder.NextResponse(), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError) );

			  // ...and the machine should have entered a FAILED state
			  assertThat( machine, inState( typeof( FailedState ) ) );
		 }

		 private class BoltResponseRecorderAnonymousInnerClass : BoltResponseRecorder
		 {
			 private readonly BoltStateMachineV1Test _outerInstance;

			 public BoltResponseRecorderAnonymousInnerClass( BoltStateMachineV1Test outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void onRecords( BoltResult result, bool pull )
			 {
				  throw new Exception( "I've been expecting you, Mr Bond." );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRollbackError() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRollbackError()
		 {
			  // Given
			  BoltStateMachine machine = init( newMachine() );

			  // Given there is a running transaction
			  machine.Process( new RunMessage( "BEGIN", EMPTY_PARAMS ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // And given that transaction will fail to roll back
			  TransactionStateMachine txMachine = TxStateMachine( machine );
			  when( txMachine.Ctx.currentTransaction.Open ).thenReturn( true );
			  doThrow( new TransactionFailureException( "No Mr. Bond, I expect you to die." ) ).when( txMachine.Ctx.currentTransaction ).close();

			  // When
			  machine.Process( new RunMessage( "ROLLBACK", EMPTY_PARAMS ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // Then
			  assertThat( machine, inState( typeof( FailedState ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailOnNestedTransactions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailOnNestedTransactions()
		 {
			  // Given
			  BoltStateMachine machine = init( newMachine() );

			  // Given there is a running transaction
			  machine.Process( new RunMessage( "BEGIN", EMPTY_PARAMS ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // When
			  machine.Process( new RunMessage( "BEGIN", EMPTY_PARAMS ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // Then
			  assertThat( machine, inState( typeof( FailedState ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCantDoAnythingIfInFailedState() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCantDoAnythingIfInFailedState()
		 {
			  // Given a FAILED machine
			  BoltStateMachine machine = init( newMachine() );
			  machine.MarkFailed( Neo4jError.from( new Exception() ) );

			  // Then no RUN...
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), nullResponseHandler() );
			  assertThat( machine, inState( typeof( FailedState ) ) );
			  // ...DISCARD_ALL...
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );
			  assertThat( machine, inState( typeof( FailedState ) ) );
			  // ...or PULL_ALL should be possible
			  machine.Process( PullAllMessage.INSTANCE, nullResponseHandler() );
			  assertThat( machine, inState( typeof( FailedState ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUsingResetToAcknowledgeError() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestUsingResetToAcknowledgeError()
		 {
			  // Given
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // Given a FAILED machine
			  BoltStateMachine machine = init( newMachine() );
			  machine.MarkFailed( Neo4jError.from( new Exception() ) );

			  // When I RESET...
			  machine.Process( ResetMessage.INSTANCE, recorder );

			  // ...successfully
			  assertThat( recorder.NextResponse(), succeeded() );

			  // Then if I RUN a statement...
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), recorder );

			  // ...everything should be fine again
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void actionsDisallowedBeforeInitialized()
		 public virtual void ActionsDisallowedBeforeInitialized()
		 {
			  // Given
			  BoltStateMachine machine = newMachine();

			  // When
			  try
			  {
					machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), nullResponseHandler() );
					fail( "Failed to fail fatally" );
			  }

			  // Then
			  catch ( BoltConnectionFatality )
			  {
					// fatality correctly generated
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldTerminateOnAuthExpiryDuringREADYRun() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateOnAuthExpiryDuringREADYRun()
		 {
			  // Given
			  TransactionStateMachineSPI transactionSPI = mock( typeof( TransactionStateMachineSPI ) );
			  doThrow( new AuthorizationExpiredException( "Auth expired!" ) ).when( transactionSPI ).beginTransaction( any(), any(), any() );

			  BoltStateMachine machine = newMachineWithTransactionSPI( transactionSPI );

			  // When & Then
			  try
			  {
					machine.Process( new RunMessage( "THIS WILL BE IGNORED", EMPTY_PARAMS ), nullResponseHandler() );
					fail( "Exception expected" );
			  }
			  catch ( BoltConnectionAuthFatality e )
			  {
					assertEquals( "Auth expired!", e.InnerException.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldTerminateOnAuthExpiryDuringSTREAMINGPullAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateOnAuthExpiryDuringSTREAMINGPullAll()
		 {
			  // Given
			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );
			  doThrow( new AuthorizationExpiredException( "Auth expired!" ) ).when( responseHandler ).onRecords( any(), anyBoolean() );
			  BoltStateMachine machine = init( newMachine() );
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), nullResponseHandler() ); // move to streaming state
			  // We assume the only implementation of statement processor is TransactionStateMachine
			  TxStateMachine( machine ).Ctx.currentResult = BoltResult.EMPTY;

			  // When & Then
			  try
			  {
					machine.Process( PullAllMessage.INSTANCE, responseHandler );
					fail( "Exception expected" );
			  }
			  catch ( BoltConnectionAuthFatality e )
			  {
					assertEquals( "Auth expired!", e.InnerException.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldTerminateOnAuthExpiryDuringSTREAMINGDiscardAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateOnAuthExpiryDuringSTREAMINGDiscardAll()
		 {
			  // Given
			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );
			  doThrow( new AuthorizationExpiredException( "Auth expired!" ) ).when( responseHandler ).onRecords( any(), anyBoolean() );
			  BoltStateMachine machine = init( newMachine() );
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_PARAMS ), nullResponseHandler() ); // move to streaming state
			  // We assume the only implementation of statement processor is TransactionStateMachine
			  TxStateMachine( machine ).Ctx.currentResult = BoltResult.EMPTY;

			  // When & Then
			  try
			  {
					machine.Process( DiscardAllMessage.INSTANCE, responseHandler );
					fail( "Exception expected" );
			  }
			  catch ( BoltConnectionAuthFatality e )
			  {
					assertEquals( "Auth expired!", e.InnerException.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void callResetEvenThoughAlreadyClosed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CallResetEvenThoughAlreadyClosed()
		 {
			  // Given
			  BoltStateMachine machine = init( newMachine() );

			  // When we close
			  TransactionStateMachine statementProcessor = TxStateMachine( machine );
			  machine.Close();
			  assertThat( statementProcessor.Ctx.currentTransaction, nullValue() );
			  assertThat( machine, Closed );

			  //But someone runs a query and thus opens a new transaction
			  statementProcessor.Run( "RETURN 1", EMPTY_PARAMS );
			  assertThat( statementProcessor.Ctx.currentTransaction, notNullValue() );

			  // Then, when we close again we should make sure the transaction is closed again
			  machine.Close();
			  assertThat( statementProcessor.Ctx.currentTransaction, nullValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseBoltChannelWhenClosed()
		 public virtual void ShouldCloseBoltChannelWhenClosed()
		 {
			  BoltStateMachineV1SPI spi = mock( typeof( BoltStateMachineV1SPI ) );
			  BoltChannel boltChannel = mock( typeof( BoltChannel ) );
			  BoltStateMachine machine = new BoltStateMachineV1( spi, boltChannel, Clock.systemUTC() );

			  machine.Close();

			  verify( boltChannel ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetPendingErrorOnMarkFailedIfNoHandler()
		 public virtual void ShouldSetPendingErrorOnMarkFailedIfNoHandler()
		 {
			  BoltStateMachineV1SPI spi = mock( typeof( BoltStateMachineV1SPI ) );
			  BoltChannel boltChannel = mock( typeof( BoltChannel ) );
			  BoltStateMachine machine = new BoltStateMachineV1( spi, boltChannel, Clock.systemUTC() );
			  Neo4jError error = Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "no threads" );

			  machine.MarkFailed( error );

			  assertEquals( error, PendingError( machine ) );
			  assertThat( machine, inState( typeof( FailedState ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextInitMessageOnMarkFailedIfNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextInitMessageOnMarkFailedIfNoHandler()
		 {
			  TestMarkFailedOnNextMessage( ( machine, handler ) => machine.process( new InitMessage( "Test/1.0", emptyMap() ), handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextRunMessageOnMarkFailedIfNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextRunMessageOnMarkFailedIfNoHandler()
		 {
			  TestMarkFailedOnNextMessage( ( machine, handler ) => machine.process( new RunMessage( "RETURN 1", VirtualValues.EMPTY_MAP ), handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextPullAllMessageOnMarkFailedIfNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextPullAllMessageOnMarkFailedIfNoHandler()
		 {
			  TestMarkFailedOnNextMessage( ( machine, handler ) => machine.process( PullAllMessage.INSTANCE, handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextDiscardAllMessageOnMarkFailedIfNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextDiscardAllMessageOnMarkFailedIfNoHandler()
		 {
			  TestMarkFailedOnNextMessage( ( machine, handler ) => machine.process( DiscardAllMessage.INSTANCE, handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextResetMessageOnMarkFailedIfNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextResetMessageOnMarkFailedIfNoHandler()
		 {
			  TestReadyStateAfterMarkFailedOnNextMessage( ( machine, handler ) => machine.process( ResetMessage.INSTANCE, handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGotoReadyStateOnNextAckFailureMessageOnMarkFailedIfNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGotoReadyStateOnNextAckFailureMessageOnMarkFailedIfNoHandler()
		 {
			  TestReadyStateAfterMarkFailedOnNextMessage( ( machine, handler ) => machine.process( AckFailureMessage.INSTANCE, handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextExternalErrorMessageOnMarkFailedIfNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextExternalErrorMessageOnMarkFailedIfNoHandler()
		 {
			  TestMarkFailedOnNextMessage( ( machine, handler ) => machine.handleExternalFailure( Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "invalid" ), handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetPendingIgnoreOnMarkFailedIfAlreadyFailedAndNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetPendingIgnoreOnMarkFailedIfAlreadyFailedAndNoHandler()
		 {
			  BoltStateMachine machine = newMachine();
			  Neo4jError error1 = Neo4jError.from( new Exception() );
			  machine.MarkFailed( error1 );

			  Neo4jError error2 = Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "no threads" );
			  machine.MarkFailed( error2 );

			  assertTrue( PendingIgnore( machine ) );
			  assertEquals( error1, PendingError( machine ) ); // error remained the same and was ignored
			  assertThat( machine, inState( typeof( FailedState ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextInitMessageOnMarkFailedIfAlreadyFailedAndNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextInitMessageOnMarkFailedIfAlreadyFailedAndNoHandler()
		 {
			  TestMarkFailedShouldYieldIgnoredIfAlreadyFailed( ( machine, handler ) => machine.process( new InitMessage( "Test/1.0", emptyMap() ), handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextRunMessageOnMarkFailedIfAlreadyFailedAndNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextRunMessageOnMarkFailedIfAlreadyFailedAndNoHandler()
		 {
			  TestMarkFailedShouldYieldIgnoredIfAlreadyFailed( ( machine, handler ) => machine.process( new RunMessage( "RETURN 1", VirtualValues.EMPTY_MAP ), handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextPullAllMessageOnMarkFailedIfAlreadyFailedAndNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextPullAllMessageOnMarkFailedIfAlreadyFailedAndNoHandler()
		 {
			  TestMarkFailedShouldYieldIgnoredIfAlreadyFailed( ( machine, handler ) => machine.process( PullAllMessage.INSTANCE, handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextDiscardAllMessageOnMarkFailedIfAlreadyFailedAndNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextDiscardAllMessageOnMarkFailedIfAlreadyFailedAndNoHandler()
		 {
			  TestMarkFailedShouldYieldIgnoredIfAlreadyFailed( ( machine, handler ) => machine.process( DiscardAllMessage.INSTANCE, handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextResetMessageOnMarkFailedIfAlreadyFailedAndNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextResetMessageOnMarkFailedIfAlreadyFailedAndNoHandler()
		 {
			  TestMarkFailedShouldYieldSuccessIfAlreadyFailed( ( machine, handler ) => machine.process( ResetMessage.INSTANCE, handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextAckFailureMessageOnMarkFailedIfAlreadyFailedAndNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextAckFailureMessageOnMarkFailedIfAlreadyFailedAndNoHandler()
		 {
			  TestMarkFailedShouldYieldSuccessIfAlreadyFailed( ( machine, handler ) => machine.process( AckFailureMessage.INSTANCE, handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnNextExternalErrorMessageOnMarkFailedIfAlreadyFailedAndNoHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnNextExternalErrorMessageOnMarkFailedIfAlreadyFailedAndNoHandler()
		 {
			  TestMarkFailedShouldYieldIgnoredIfAlreadyFailed( ( machine, handler ) => machine.handleExternalFailure( Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "invalid" ), handler ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeResponseHandlerOnMarkFailedIfThereIsHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeResponseHandlerOnMarkFailedIfThereIsHandler()
		 {
			  BoltStateMachine machine = init( newMachine() );
			  Neo4jError error = Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "no threads" );

			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );
			  ( ( BoltStateMachineV1 ) machine ).ConnectionState().ResponseHandler = responseHandler;
			  machine.MarkFailed( error );

			  assertNull( PendingError( machine ) );
			  assertFalse( PendingIgnore( machine ) );
			  assertThat( machine, inState( typeof( FailedState ) ) );
			  verify( responseHandler ).markFailed( error );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailWhenMarkedForTerminationAndPullAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFailWhenMarkedForTerminationAndPullAll()
		 {
			  BoltStateMachineV1SPI spi = mock( typeof( BoltStateMachineV1SPI ), RETURNS_MOCKS );
			  BoltStateMachine machine = init( newMachine( spi ) );
			  machine.Process( new RunMessage( "RETURN 42", EMPTY_PARAMS ), nullResponseHandler() ); // move to streaming state
			  TxStateMachine( machine ).Ctx.currentResult = BoltResult.EMPTY;

			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );

			  machine.MarkForTermination();
			  machine.Process( PullAllMessage.INSTANCE, responseHandler );

			  verify( spi, never() ).reportError(any());
			  assertThat( machine, not( inState( typeof( FailedState ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedOnResetOnFailedState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedOnResetOnFailedState()
		 {
			  // Given
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // Given a FAILED machine
			  BoltStateMachine machine = init( newMachine() );

			  machine.MarkFailed( Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "No Threads Available" ) );
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // When I RESET...
			  machine.Interrupt();
			  machine.MarkFailed( Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "No Threads Available" ) );
			  machine.Process( ResetMessage.INSTANCE, recorder );

			  assertThat( recorder.NextResponse(), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable) );
			  // ...successfully
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedOnConsecutiveResetsOnFailedState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedOnConsecutiveResetsOnFailedState()
		 {
			  // Given
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // Given a FAILED machine
			  BoltStateMachine machine = init( newMachine() );

			  machine.MarkFailed( Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "No Threads Available" ) );
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // When I RESET...
			  machine.Interrupt();
			  machine.Interrupt();
			  machine.MarkFailed( Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "No Threads Available" ) );
			  machine.Process( ResetMessage.INSTANCE, recorder );
			  machine.MarkFailed( Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "No Threads Available" ) );
			  machine.Process( ResetMessage.INSTANCE, recorder );

			  assertThat( recorder.NextResponse(), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable) );
			  // ...successfully
			  assertThat( recorder.NextResponse(), wasIgnored() );
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testMarkFailedOnNextMessage(org.neo4j.function.ThrowingBiConsumer<org.neo4j.bolt.runtime.BoltStateMachine,org.neo4j.bolt.runtime.BoltResponseHandler,org.neo4j.bolt.runtime.BoltConnectionFatality> action) throws Exception
		 private static void TestMarkFailedOnNextMessage( ThrowingBiConsumer<BoltStateMachine, BoltResponseHandler, BoltConnectionFatality> action )
		 {
			  // Given
			  BoltStateMachine machine = init( newMachine() );
			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );

			  Neo4jError error = Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "no threads" );
			  machine.MarkFailed( error );

			  // When
			  action.Accept( machine, responseHandler );

			  // Expect
			  assertNull( PendingError( machine ) );
			  assertFalse( PendingIgnore( machine ) );
			  assertThat( machine, inState( typeof( FailedState ) ) );
			  verify( responseHandler ).markFailed( error );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testReadyStateAfterMarkFailedOnNextMessage(org.neo4j.function.ThrowingBiConsumer<org.neo4j.bolt.runtime.BoltStateMachine,org.neo4j.bolt.runtime.BoltResponseHandler,org.neo4j.bolt.runtime.BoltConnectionFatality> action) throws Exception
		 private static void TestReadyStateAfterMarkFailedOnNextMessage( ThrowingBiConsumer<BoltStateMachine, BoltResponseHandler, BoltConnectionFatality> action )
		 {
			  // Given
			  BoltStateMachine machine = init( newMachine() );
			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );

			  Neo4jError error = Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "no threads" );
			  machine.MarkFailed( error );

			  // When
			  action.Accept( machine, responseHandler );

			  // Expect
			  assertNull( PendingError( machine ) );
			  assertFalse( PendingIgnore( machine ) );
			  assertThat( machine, inState( typeof( ReadyState ) ) );
			  verify( responseHandler, never() ).markFailed(any());
			  verify( responseHandler, never() ).markIgnored();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testMarkFailedShouldYieldIgnoredIfAlreadyFailed(org.neo4j.function.ThrowingBiConsumer<org.neo4j.bolt.runtime.BoltStateMachine,org.neo4j.bolt.runtime.BoltResponseHandler,org.neo4j.bolt.runtime.BoltConnectionFatality> action) throws Exception
		 private static void TestMarkFailedShouldYieldIgnoredIfAlreadyFailed( ThrowingBiConsumer<BoltStateMachine, BoltResponseHandler, BoltConnectionFatality> action )
		 {
			  // Given
			  BoltStateMachine machine = init( newMachine() );
			  machine.MarkFailed( Neo4jError.from( new Exception() ) );
			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );

			  Neo4jError error = Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "no threads" );
			  machine.MarkFailed( error );

			  // When
			  action.Accept( machine, responseHandler );

			  // Expect
			  assertNull( PendingError( machine ) );
			  assertFalse( PendingIgnore( machine ) );
			  assertThat( machine, inState( typeof( FailedState ) ) );
			  verify( responseHandler ).markIgnored();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testMarkFailedShouldYieldSuccessIfAlreadyFailed(org.neo4j.function.ThrowingBiConsumer<org.neo4j.bolt.runtime.BoltStateMachine,org.neo4j.bolt.runtime.BoltResponseHandler,org.neo4j.bolt.runtime.BoltConnectionFatality> action) throws Exception
		 private static void TestMarkFailedShouldYieldSuccessIfAlreadyFailed( ThrowingBiConsumer<BoltStateMachine, BoltResponseHandler, BoltConnectionFatality> action )
		 {
			  // Given
			  BoltStateMachine machine = init( newMachine() );
			  machine.MarkFailed( Neo4jError.from( new Exception() ) );
			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );

			  Neo4jError error = Neo4jError.from( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "no threads" );
			  machine.MarkFailed( error );

			  // When
			  action.Accept( machine, responseHandler );

			  // Expect
			  assertNull( PendingError( machine ) );
			  assertFalse( PendingIgnore( machine ) );
			  assertThat( machine, inState( typeof( ReadyState ) ) );
			  verify( responseHandler, never() ).markIgnored();
			  verify( responseHandler, never() ).markFailed(any());
		 }

		 private static TransactionStateMachine TxStateMachine( BoltStateMachine machine )
		 {
			  return ( TransactionStateMachine )( ( BoltStateMachineV1 ) machine ).StatementProcessor();
		 }

		 private static Neo4jError PendingError( BoltStateMachine machine )
		 {
			  return ( ( BoltStateMachineV1 ) machine ).ConnectionState().PendingError;
		 }

		 private static bool PendingIgnore( BoltStateMachine machine )
		 {
			  return ( ( BoltStateMachineV1 ) machine ).ConnectionState().hasPendingIgnore();
		 }
	}

}