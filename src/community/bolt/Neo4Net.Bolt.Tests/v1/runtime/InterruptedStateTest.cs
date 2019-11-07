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
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using MutableConnectionState = Neo4Net.Bolt.runtime.MutableConnectionState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

	internal class InterruptedStateTest
	{
		 private readonly InterruptedState _state = new InterruptedState();

		 private readonly BoltStateMachineState _readyState = mock( typeof( BoltStateMachineState ) );
		 private readonly BoltStateMachineState _failedState = mock( typeof( BoltStateMachineState ) );

		 private readonly StateMachineContext _context = mock( typeof( StateMachineContext ) );
		 private readonly MutableConnectionState _connectionState = new MutableConnectionState();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _state.ReadyState = _readyState;
			  _state.FailedState = _failedState;

			  when( _context.connectionState() ).thenReturn(_connectionState);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenNotInitialized() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowWhenNotInitialized()
		 {
			  InterruptedState state = new InterruptedState();

			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(ResetMessage.INSTANCE, _context) );

			  state.ReadyState = _readyState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(ResetMessage.INSTANCE, _context) );

			  state.ReadyState = null;
			  state.FailedState = _failedState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(ResetMessage.INSTANCE, _context) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessInterruptMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessInterruptMessage()
		 {
			  BoltStateMachineState newState = _state.process( InterruptSignal.INSTANCE, _context );

			  assertEquals( _state, newState ); // remains in interrupted state
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessResetMessageWhenInterrupted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessResetMessageWhenInterrupted()
		 {
			  _connectionState.incrementInterruptCounter();
			  _connectionState.incrementInterruptCounter();
			  assertTrue( _connectionState.Interrupted );
			  assertFalse( _connectionState.hasPendingIgnore() );

			  BoltStateMachineState newState = _state.process( ResetMessage.INSTANCE, _context );

			  assertEquals( _state, newState ); // remains in interrupted state
			  assertTrue( _connectionState.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessResetMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessResetMessage()
		 {
			  when( _context.resetMachine() ).thenReturn(true); // reset successful
			  BoltStateMachineState newState = _state.process( ResetMessage.INSTANCE, _context );

			  assertEquals( _readyState, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleFailureDuringResetMessageProcessing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleFailureDuringResetMessageProcessing()
		 {
			  when( _context.resetMachine() ).thenReturn(false); // reset failed
			  BoltStateMachineState newState = _state.process( ResetMessage.INSTANCE, _context );

			  assertEquals( _failedState, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIgnoreMessagesOtherThanInterruptAndReset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldIgnoreMessagesOtherThanInterruptAndReset()
		 {
			  IList<RequestMessage> messages = new IList<RequestMessage> { AckFailureMessage.INSTANCE, PullAllMessage.INSTANCE, DiscardAllMessage.INSTANCE, new RunMessage( "RETURN 1", EMPTY_MAP ), new InitMessage( "Driver", emptyMap() ) };

			  foreach ( RequestMessage message in messages )
			  {
					_connectionState.resetPendingFailedAndIgnored();

					BoltStateMachineState newState = _state.process( message, _context );

					assertEquals( _state, newState ); // remains in interrupted state
					assertTrue( _connectionState.hasPendingIgnore() );
			  }
		 }
	}

}