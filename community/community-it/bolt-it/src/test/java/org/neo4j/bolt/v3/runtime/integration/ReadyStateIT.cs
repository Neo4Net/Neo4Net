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

	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltResponseRecorder = Org.Neo4j.Bolt.testing.BoltResponseRecorder;
	using RecordedBoltResponse = Org.Neo4j.Bolt.testing.RecordedBoltResponse;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using InterruptSignal = Org.Neo4j.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using BeginMessage = Org.Neo4j.Bolt.v3.messaging.request.BeginMessage;
	using RunMessage = Org.Neo4j.Bolt.v3.messaging.request.RunMessage;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
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

	internal class ReadyStateIT : BoltStateMachineStateTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveToStreamingOnRun_succ() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveToStreamingOnRunSucc()
		 {
			  // Given
			  BoltStateMachineV3 machine = NewStateMachine();
			  machine.Process( NewHelloMessage(), nullResponseHandler() );

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "CREATE (n {k:'k'}) RETURN n.k", EmptyParams ), recorder );

			  // Then

			  RecordedBoltResponse response = recorder.NextResponse();
			  assertThat( response, succeeded() );
			  assertTrue( response.HasMetadata( "fields" ) );
			  assertTrue( response.HasMetadata( "t_first" ) );
			  assertThat( machine.State(), instanceOf(typeof(StreamingState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveToStreamingOnBegin_succ() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveToStreamingOnBeginSucc()
		 {
			  // Given
			  BoltStateMachineV3 machine = NewStateMachine();
			  machine.Process( NewHelloMessage(), nullResponseHandler() );

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new BeginMessage(), recorder );

			  // Then
			  RecordedBoltResponse response = recorder.NextResponse();
			  assertThat( response, succeeded() );
			  assertThat( machine.State(), instanceOf(typeof(TransactionReadyState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveToInterruptedOnInterrupt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveToInterruptedOnInterrupt()
		 {
			  // Given
			  BoltStateMachineV3 machine = NewStateMachine();
			  machine.Process( NewHelloMessage(), nullResponseHandler() );

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( InterruptSignal.INSTANCE, recorder );

			  // Then
			  assertThat( machine.State(), instanceOf(typeof(InterruptedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveToFailedStateOnRun_fail() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveToFailedStateOnRunFail()
		 {
			  // Given
			  BoltStateMachineV3 machine = NewStateMachine();
			  machine.Process( NewHelloMessage(), nullResponseHandler() );

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  RunMessage runMessage = mock( typeof( RunMessage ) );
			  when( runMessage.Statement() ).thenThrow(new Exception("Fail"));
			  machine.Process( runMessage, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError) );
			  assertThat( machine.State(), instanceOf(typeof(FailedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveToFailedStateOnBegin_fail() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveToFailedStateOnBeginFail()
		 {
			  // Given
			  BoltStateMachineV3 machine = NewStateMachine();
			  machine.Process( NewHelloMessage(), nullResponseHandler() );

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  BeginMessage beginMessage = mock( typeof( BeginMessage ) );
			  when( beginMessage.Bookmark() ).thenThrow(new Exception("Fail"));
			  machine.Process( beginMessage, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError) );
			  assertThat( machine.State(), instanceOf(typeof(FailedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("illegalV3Messages") void shouldCloseConnectionOnIllegalV3Messages(org.neo4j.bolt.messaging.RequestMessage message) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseConnectionOnIllegalV3Messages( RequestMessage message )
		 {
			  ShouldCloseConnectionOnIllegalMessages( message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("illegalV2Messages") void shouldCloseConnectionOnIllegalV2Messages(org.neo4j.bolt.messaging.RequestMessage message) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseConnectionOnIllegalV2Messages( RequestMessage message )
		 {
			  ShouldCloseConnectionOnIllegalMessages( message );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldCloseConnectionOnIllegalMessages(org.neo4j.bolt.messaging.RequestMessage message) throws InterruptedException, org.neo4j.bolt.runtime.BoltConnectionFatality
		 private void ShouldCloseConnectionOnIllegalMessages( RequestMessage message )
		 {
			  // Given
			  BoltStateMachineV3 machine = NewStateMachine();
			  machine.Process( NewHelloMessage(), nullResponseHandler() );

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(message, recorder) );

			  // then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
			  assertNull( machine.State() );
		 }

		 private static Stream<RequestMessage> IllegalV3Messages()
		 {
			  return Stream.of( NewHelloMessage(), DiscardAllMessage.INSTANCE, PullAllMessage.INSTANCE, COMMIT_MESSAGE, ROLLBACK_MESSAGE, GOODBYE_MESSAGE );
		 }

		 private static Stream<RequestMessage> IllegalV2Messages()
		 {
			  return Stream.of( new InitMessage( USER_AGENT, emptyMap() ), new Org.Neo4j.Bolt.v1.messaging.request.RunMessage("RETURN 1", EmptyParams) );
		 }
	}

}