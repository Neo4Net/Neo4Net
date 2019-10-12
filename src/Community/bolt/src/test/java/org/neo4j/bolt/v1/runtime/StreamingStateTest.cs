using System;
using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.runtime
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using MutableConnectionState = Neo4Net.Bolt.runtime.MutableConnectionState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using StatementProcessor = Neo4Net.Bolt.runtime.StatementProcessor;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using AuthorizationExpiredException = Neo4Net.Graphdb.security.AuthorizationExpiredException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	internal class StreamingStateTest
	{
		 private readonly StreamingState _state = new StreamingState();

		 private readonly BoltStateMachineState _readyState = mock( typeof( BoltStateMachineState ) );
		 private readonly BoltStateMachineState _interruptedState = mock( typeof( BoltStateMachineState ) );
		 private readonly BoltStateMachineState _failedState = mock( typeof( BoltStateMachineState ) );

		 private readonly StateMachineContext _context = mock( typeof( StateMachineContext ) );
		 private readonly MutableConnectionState _connectionState = new MutableConnectionState();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _state.ReadyState = _readyState;
			  _state.InterruptedState = _interruptedState;
			  _state.FailedState = _failedState;

			  when( _context.connectionState() ).thenReturn(_connectionState);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenNotInitialized() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowWhenNotInitialized()
		 {
			  StreamingState state = new StreamingState();

			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(PullAllMessage.INSTANCE, _context) );

			  state.ReadyState = _readyState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(PullAllMessage.INSTANCE, _context) );

			  state.ReadyState = null;
			  state.InterruptedState = _interruptedState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(PullAllMessage.INSTANCE, _context) );

			  state.InterruptedState = null;
			  state.FailedState = _failedState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(PullAllMessage.INSTANCE, _context) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessPullAllMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessPullAllMessage()
		 {
			  StatementProcessor statementProcessor = mock( typeof( StatementProcessor ) );
			  _connectionState.StatementProcessor = statementProcessor;

			  BoltStateMachineState nextState = _state.process( PullAllMessage.INSTANCE, _context );

			  assertEquals( _readyState, nextState );
			  verify( statementProcessor ).streamResult( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleAuthErrorWhenProcessingPullAllMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleAuthErrorWhenProcessingPullAllMessage()
		 {
			  AuthorizationExpiredException error = new AuthorizationExpiredException( "Hello" );

			  StatementProcessor statementProcessor = mock( typeof( StatementProcessor ) );
			  doThrow( error ).when( statementProcessor ).streamResult( any() );
			  _connectionState.StatementProcessor = statementProcessor;

			  BoltStateMachineState nextState = _state.process( PullAllMessage.INSTANCE, _context );

			  assertEquals( _failedState, nextState );
			  verify( _context ).handleFailure( error, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleErrorWhenProcessingPullAllMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleErrorWhenProcessingPullAllMessage()
		 {
			  Exception error = new Exception( "Hello" );

			  StatementProcessor statementProcessor = mock( typeof( StatementProcessor ) );
			  doThrow( error ).when( statementProcessor ).streamResult( any() );
			  _connectionState.StatementProcessor = statementProcessor;

			  BoltStateMachineState nextState = _state.process( PullAllMessage.INSTANCE, _context );

			  assertEquals( _failedState, nextState );
			  verify( _context ).handleFailure( error, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessDiscardAllMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessDiscardAllMessage()
		 {
			  StatementProcessor statementProcessor = mock( typeof( StatementProcessor ) );
			  _connectionState.StatementProcessor = statementProcessor;

			  BoltStateMachineState nextState = _state.process( DiscardAllMessage.INSTANCE, _context );

			  assertEquals( _readyState, nextState );
			  verify( statementProcessor ).streamResult( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleAuthErrorWhenProcessingDiscardAllMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleAuthErrorWhenProcessingDiscardAllMessage()
		 {
			  AuthorizationExpiredException error = new AuthorizationExpiredException( "Hello" );

			  StatementProcessor statementProcessor = mock( typeof( StatementProcessor ) );
			  doThrow( error ).when( statementProcessor ).streamResult( any() );
			  _connectionState.StatementProcessor = statementProcessor;

			  BoltStateMachineState nextState = _state.process( DiscardAllMessage.INSTANCE, _context );

			  assertEquals( _failedState, nextState );
			  verify( _context ).handleFailure( error, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleErrorWhenProcessingDiscardAllMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleErrorWhenProcessingDiscardAllMessage()
		 {
			  Exception error = new Exception( "Hello" );

			  StatementProcessor statementProcessor = mock( typeof( StatementProcessor ) );
			  doThrow( error ).when( statementProcessor ).streamResult( any() );
			  _connectionState.StatementProcessor = statementProcessor;

			  BoltStateMachineState nextState = _state.process( DiscardAllMessage.INSTANCE, _context );

			  assertEquals( _failedState, nextState );
			  verify( _context ).handleFailure( error, false );
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
//ORIGINAL LINE: @Test void shouldHandleResetMessageFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleResetMessageFailure()
		 {
			  when( _context.resetMachine() ).thenReturn(false); // reset failed

			  BoltStateMachineState newState = _state.process( ResetMessage.INSTANCE, _context );

			  assertEquals( _failedState, newState );
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
//ORIGINAL LINE: @Test void shouldNotProcessUnsupportedMessages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotProcessUnsupportedMessages()
		 {
			  IList<RequestMessage> unsupportedMessages = new IList<RequestMessage> { AckFailureMessage.INSTANCE, new RunMessage( "RETURN 1", EMPTY_MAP ), new InitMessage( "Driver 2.5", emptyMap() ) };

			  foreach ( RequestMessage message in unsupportedMessages )
			  {
					assertNull( _state.process( message, _context ) );
			  }
		 }
	}

}