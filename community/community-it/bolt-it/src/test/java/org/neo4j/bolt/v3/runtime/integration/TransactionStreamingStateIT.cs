﻿using System;

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
namespace Org.Neo4j.Bolt.v3.runtime.integration
{
	using Test = org.junit.jupiter.api.Test;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;

	using BoltIOException = Org.Neo4j.Bolt.messaging.BoltIOException;
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using BoltResponseRecorder = Org.Neo4j.Bolt.testing.BoltResponseRecorder;
	using RecordedBoltResponse = Org.Neo4j.Bolt.testing.RecordedBoltResponse;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InterruptSignal = Org.Neo4j.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using BeginMessage = Org.Neo4j.Bolt.v3.messaging.request.BeginMessage;
	using RunMessage = Org.Neo4j.Bolt.v3.messaging.request.RunMessage;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.failedWithStatus;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.succeeded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.verifyKillsConnection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.CommitMessage.COMMIT_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.GoodbyeMessage.GOODBYE_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.RollbackMessage.ROLLBACK_MESSAGE;

	internal class TransactionStreamingStateIT : BoltStateMachineStateTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveFromTxStreamingToTxReadyOnDiscardAll_succ() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveFromTxStreamingToTxReadyOnDiscardAllSucc()
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInTxStreamingState;

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( DiscardAllMessage.INSTANCE, recorder );

			  // Then
			  RecordedBoltResponse response = recorder.NextResponse();
			  assertThat( response, succeeded() );
			  assertFalse( response.HasMetadata( "bookmark" ) );
			  assertThat( machine.State(), instanceOf(typeof(TransactionReadyState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveFromTxStreamingToTxReadyOnPullAll_succ() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveFromTxStreamingToTxReadyOnPullAllSucc()
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInTxStreamingState;

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // Then
			  RecordedBoltResponse response = recorder.NextResponse();
			  assertThat( response, succeeded() );
			  assertTrue( response.HasMetadata( "type" ) );
			  assertTrue( response.HasMetadata( "t_last" ) );
			  assertFalse( response.HasMetadata( "bookmark" ) );
			  assertThat( machine.State(), instanceOf(typeof(TransactionReadyState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveFromTxStreamingToInterruptedOnInterrupt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveFromTxStreamingToInterruptedOnInterrupt()
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInTxStreamingState;

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( InterruptSignal.INSTANCE, recorder );

			  // Then
			  assertThat( machine.State(), instanceOf(typeof(InterruptedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("pullAllDiscardAllMessages") void shouldMoveFromTxStreamingStateToFailedStateOnPullAllOrDiscardAll_fail(org.neo4j.bolt.messaging.RequestMessage message) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveFromTxStreamingStateToFailedStateOnPullAllOrDiscardAllFail( RequestMessage message )
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInTxStreamingState;

			  // When

			  BoltResponseHandler handler = mock( typeof( BoltResponseHandler ) );
			  doThrow( new Exception( "Fail" ) ).when( handler ).onRecords( any(), anyBoolean() );
			  machine.Process( message, handler );

			  // Then
			  assertThat( machine.State(), instanceOf(typeof(FailedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("illegalV3Messages") void shouldCloseConnectionOnIllegalV3MessagesInTxStreamingState(org.neo4j.bolt.messaging.RequestMessage message) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseConnectionOnIllegalV3MessagesInTxStreamingState( RequestMessage message )
		 {
			  ShouldThrowExceptionOnIllegalMessagesInTxStreamingState( message );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldThrowExceptionOnIllegalMessagesInTxStreamingState(org.neo4j.bolt.messaging.RequestMessage message) throws Throwable
		 private void ShouldThrowExceptionOnIllegalMessagesInTxStreamingState( RequestMessage message )
		 {
			  // Given
			  BoltStateMachineV3 machine = NewStateMachine();
			  machine.Process( NewHelloMessage(), nullResponseHandler() );

			  machine.Process( new BeginMessage(), nullResponseHandler() );
			  machine.Process( new RunMessage( "CREATE (n {k:'k'}) RETURN n.k", EmptyParams ), nullResponseHandler() );
			  assertThat( machine.State(), instanceOf(typeof(TransactionStreamingState)) );

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(message, recorder) );

			  // then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
			  assertNull( machine.State() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.stream.Stream<org.neo4j.bolt.messaging.RequestMessage> illegalV3Messages() throws org.neo4j.bolt.messaging.BoltIOException
		 private static Stream<RequestMessage> IllegalV3Messages()
		 {
			  return Stream.of( NewHelloMessage(), new RunMessage("any string"), new BeginMessage(), ROLLBACK_MESSAGE, COMMIT_MESSAGE, ResetMessage.INSTANCE, GOODBYE_MESSAGE );
		 }

		 private static Stream<RequestMessage> PullAllDiscardAllMessages()
		 {
			  return Stream.of( PullAllMessage.INSTANCE, DiscardAllMessage.INSTANCE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.v3.BoltStateMachineV3 getBoltStateMachineInTxStreamingState() throws org.neo4j.bolt.runtime.BoltConnectionFatality, org.neo4j.bolt.messaging.BoltIOException
		 private BoltStateMachineV3 BoltStateMachineInTxStreamingState
		 {
			 get
			 {
				  BoltStateMachineV3 machine = NewStateMachine();
				  machine.Process( NewHelloMessage(), nullResponseHandler() );
   
				  machine.Process( new BeginMessage(), nullResponseHandler() );
				  assertThat( machine.State(), instanceOf(typeof(TransactionReadyState)) );
				  machine.Process( new RunMessage( "CREATE (n {k:'k'}) RETURN n.k", EmptyParams ), nullResponseHandler() );
				  assertThat( machine.State(), instanceOf(typeof(TransactionStreamingState)) ); // tx streaming state
				  return machine;
			 }
		 }
	}

}