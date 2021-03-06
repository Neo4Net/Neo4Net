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
namespace Org.Neo4j.Bolt.v1.runtime
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltStateMachineState = Org.Neo4j.Bolt.runtime.BoltStateMachineState;
	using MutableConnectionState = Org.Neo4j.Bolt.runtime.MutableConnectionState;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using StateMachineContext = Org.Neo4j.Bolt.runtime.StateMachineContext;
	using AckFailureMessage = Org.Neo4j.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InterruptSignal = Org.Neo4j.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	internal class FailedStateTest
	{
		 private readonly FailedState _state = new FailedState();

		 private readonly BoltStateMachineState _readyState = mock( typeof( BoltStateMachineState ) );
		 private readonly BoltStateMachineState _interruptedState = mock( typeof( BoltStateMachineState ) );

		 private readonly StateMachineContext _context = mock( typeof( StateMachineContext ) );
		 private readonly MutableConnectionState _connectionState = new MutableConnectionState();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _state.ReadyState = _readyState;
			  _state.InterruptedState = _interruptedState;

			  when( _context.connectionState() ).thenReturn(_connectionState);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenNotInitialized() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowWhenNotInitialized()
		 {
			  FailedState state = new FailedState();

			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(AckFailureMessage.INSTANCE, _context) );

			  state.ReadyState = _readyState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(AckFailureMessage.INSTANCE, _context) );

			  state.ReadyState = null;
			  state.InterruptedState = _interruptedState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(AckFailureMessage.INSTANCE, _context) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessRunMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessRunMessage()
		 {
			  BoltStateMachineState newState = _state.process( new RunMessage( "RETURN 1", EMPTY_MAP ), _context );

			  assertEquals( _state, newState ); // remains in failed state
			  assertTrue( _connectionState.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessPullAllMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessPullAllMessage()
		 {
			  BoltStateMachineState newState = _state.process( PullAllMessage.INSTANCE, _context );

			  assertEquals( _state, newState ); // remains in failed state
			  assertTrue( _connectionState.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessDiscardAllMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessDiscardAllMessage()
		 {
			  BoltStateMachineState newState = _state.process( DiscardAllMessage.INSTANCE, _context );

			  assertEquals( _state, newState ); // remains in failed state
			  assertTrue( _connectionState.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessAckFailureMessageWithPendingIgnore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessAckFailureMessageWithPendingIgnore()
		 {
			  _connectionState.markIgnored();
			  assertTrue( _connectionState.hasPendingIgnore() );

			  BoltStateMachineState newState = _state.process( AckFailureMessage.INSTANCE, _context );

			  assertEquals( _readyState, newState );
			  assertFalse( _connectionState.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessAckFailureMessageWithPendingError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessAckFailureMessageWithPendingError()
		 {
			  Neo4jError error = Neo4jError.from( new Exception() );
			  _connectionState.markFailed( error );
			  assertEquals( error, _connectionState.PendingError );

			  BoltStateMachineState newState = _state.process( AckFailureMessage.INSTANCE, _context );

			  assertEquals( _readyState, newState );
			  assertNull( _connectionState.PendingError );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessResetMessageWithPerndingIgnore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessResetMessageWithPerndingIgnore()
		 {
			  when( _context.resetMachine() ).thenReturn(true); // reset successful
			  _connectionState.markIgnored();
			  assertTrue( _connectionState.hasPendingIgnore() );

			  BoltStateMachineState newState = _state.process( ResetMessage.INSTANCE, _context );

			  assertEquals( _readyState, newState );
			  assertFalse( _connectionState.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessResetMessageWithPerndingError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessResetMessageWithPerndingError()
		 {
			  when( _context.resetMachine() ).thenReturn(true); // reset successful
			  Neo4jError error = Neo4jError.from( new Exception() );
			  _connectionState.markFailed( error );
			  assertEquals( error, _connectionState.PendingError );

			  BoltStateMachineState newState = _state.process( ResetMessage.INSTANCE, _context );

			  assertEquals( _readyState, newState );
			  assertNull( _connectionState.PendingError );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleResetMessageFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleResetMessageFailure()
		 {
			  when( _context.resetMachine() ).thenReturn(false); // reset failed

			  BoltStateMachineState newState = _state.process( ResetMessage.INSTANCE, _context );

			  assertEquals( _state, newState ); // remains in failed state
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessInterruptMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessInterruptMessage()
		 {
			  BoltStateMachineState newState = _state.process( InterruptSignal.INSTANCE, _context );

			  assertEquals( _interruptedState, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotProcessUnsupportedMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotProcessUnsupportedMessage()
		 {
			  RequestMessage unsupportedMessage = mock( typeof( RequestMessage ) );

			  BoltStateMachineState newState = _state.process( unsupportedMessage, _context );

			  assertNull( newState );
		 }
	}

}