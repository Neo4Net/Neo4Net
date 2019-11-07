﻿using System;

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
namespace Neo4Net.Bolt.v3.runtime.integration
{
	using Test = org.junit.jupiter.api.Test;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;

	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltResponseRecorder = Neo4Net.Bolt.testing.BoltResponseRecorder;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using BeginMessage = Neo4Net.Bolt.v3.messaging.request.BeginMessage;
	using RunMessage = Neo4Net.Bolt.v3.messaging.request.RunMessage;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.failedWithStatus;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.succeeded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.verifyKillsConnection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.wasIgnored;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v3.messaging.request.CommitMessage.COMMIT_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v3.messaging.request.GoodbyeMessage.GOODBYE_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v3.messaging.request.RollbackMessage.ROLLBACK_MESSAGE;

	internal class FailedStateIT : BoltStateMachineStateTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("ignoredMessages") void shouldIgnoreMessages(Neo4Net.bolt.messaging.RequestMessage message) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldIgnoreMessages( RequestMessage message )
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInFailedState;

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( message, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), wasIgnored() );
			  assertThat( machine.State(), instanceOf(typeof(FailedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveToInterruptedOnInterruptSignal() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveToInterruptedOnInterruptSignal()
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInFailedState;

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( InterruptSignal.INSTANCE, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( machine.State(), instanceOf(typeof(InterruptedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("illegalV3Messages") void shouldCloseConnectionOnIllegalV3Messages(Neo4Net.bolt.messaging.RequestMessage message) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseConnectionOnIllegalV3Messages( RequestMessage message )
		 {
			  ShouldCloseConnectionOnIllegalMessages( message );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldCloseConnectionOnIllegalMessages(Neo4Net.bolt.messaging.RequestMessage message) throws InterruptedException, Neo4Net.bolt.runtime.BoltConnectionFatality
		 private void ShouldCloseConnectionOnIllegalMessages( RequestMessage message )
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInFailedState;

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(message, recorder) );

			  // then
			  assertThat( recorder.NextResponse(), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid) );
			  assertNull( machine.State() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.bolt.v3.BoltStateMachineV3 getBoltStateMachineInFailedState() throws Neo4Net.bolt.runtime.BoltConnectionFatality, InterruptedException
		 private BoltStateMachineV3 BoltStateMachineInFailedState
		 {
			 get
			 {
				  BoltStateMachineV3 machine = NewStateMachine();
				  machine.Process( NewHelloMessage(), nullResponseHandler() );
   
				  RunMessage runMessage = mock( typeof( RunMessage ) );
				  when( runMessage.Statement() ).thenThrow(new Exception("error here"));
				  BoltResponseRecorder recorder = new BoltResponseRecorder();
				  machine.Process( runMessage, recorder );
   
				  assertThat( recorder.NextResponse(), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError) );
				  assertThat( machine.State(), instanceOf(typeof(FailedState)) );
				  return machine;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.stream.Stream<Neo4Net.bolt.messaging.RequestMessage> ignoredMessages() throws Neo4Net.bolt.messaging.BoltIOException
		 private static Stream<RequestMessage> IgnoredMessages()
		 {
			  return Stream.of( DiscardAllMessage.INSTANCE, PullAllMessage.INSTANCE, new RunMessage( "A cypher query" ), COMMIT_MESSAGE, ROLLBACK_MESSAGE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.stream.Stream<Neo4Net.bolt.messaging.RequestMessage> illegalV3Messages() throws Neo4Net.bolt.messaging.BoltIOException
		 private static Stream<RequestMessage> IllegalV3Messages()
		 {
			  return Stream.of( NewHelloMessage(), new BeginMessage(), GOODBYE_MESSAGE );
		 }
	}

}