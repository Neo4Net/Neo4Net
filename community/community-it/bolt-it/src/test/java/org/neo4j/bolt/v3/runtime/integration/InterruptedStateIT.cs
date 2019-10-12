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
	using BoltResponseRecorder = Org.Neo4j.Bolt.testing.BoltResponseRecorder;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InterruptSignal = Org.Neo4j.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using BeginMessage = Org.Neo4j.Bolt.v3.messaging.request.BeginMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.succeeded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.wasIgnored;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.CommitMessage.COMMIT_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.GoodbyeMessage.GOODBYE_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.RollbackMessage.ROLLBACK_MESSAGE;

	internal class InterruptedStateIT : BoltStateMachineStateTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveReadyOnReset_succ() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMoveReadyOnResetSucc()
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInInterruptedState;

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( ResetMessage.INSTANCE, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( machine.State(), instanceOf(typeof(ReadyState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStayInInterruptedOnMoreReset() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldStayInInterruptedOnMoreReset()
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInInterruptedState;
			  machine.Interrupt();
			  machine.Interrupt(); // need two reset to recover

			  // When & Then
			  machine.Process( ResetMessage.INSTANCE, nullResponseHandler() );
			  assertThat( machine.State(), instanceOf(typeof(InterruptedState)) );

			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( ResetMessage.INSTANCE, recorder );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( machine.State(), instanceOf(typeof(ReadyState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStayInInterruptedOnInterruptedSignal() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldStayInInterruptedOnInterruptedSignal()
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInInterruptedState;

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( InterruptSignal.INSTANCE, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( machine.State(), instanceOf(typeof(InterruptedState)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("illegalV3Messages") void shouldCloseConnectionOnIllegalV3Messages(org.neo4j.bolt.messaging.RequestMessage message) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseConnectionOnIllegalV3Messages( RequestMessage message )
		 {
			  ShouldCloseConnectionOnIllegalMessages( message );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldCloseConnectionOnIllegalMessages(org.neo4j.bolt.messaging.RequestMessage message) throws InterruptedException, org.neo4j.bolt.runtime.BoltConnectionFatality
		 private void ShouldCloseConnectionOnIllegalMessages( RequestMessage message )
		 {
			  // Given
			  BoltStateMachineV3 machine = BoltStateMachineInInterruptedState;

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( message, recorder );

			  // then
			  assertThat( recorder.NextResponse(), wasIgnored() );
			  assertThat( machine.State(), instanceOf(typeof(InterruptedState)) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.v3.BoltStateMachineV3 getBoltStateMachineInInterruptedState() throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 private BoltStateMachineV3 BoltStateMachineInInterruptedState
		 {
			 get
			 {
				  BoltStateMachineV3 machine = NewStateMachine();
				  machine.Process( NewHelloMessage(), nullResponseHandler() );
				  machine.Process( InterruptSignal.INSTANCE, nullResponseHandler() );
				  assertThat( machine.State(), instanceOf(typeof(InterruptedState)) );
				  return machine;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.stream.Stream<org.neo4j.bolt.messaging.RequestMessage> illegalV3Messages() throws org.neo4j.bolt.messaging.BoltIOException
		 private static Stream<RequestMessage> IllegalV3Messages()
		 {
			  return Stream.of( NewHelloMessage(), DiscardAllMessage.INSTANCE, PullAllMessage.INSTANCE, new BeginMessage(), COMMIT_MESSAGE, ROLLBACK_MESSAGE, new RunMessage("A cypher query"), GOODBYE_MESSAGE );
		 }
	}

}