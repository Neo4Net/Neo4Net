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
namespace Org.Neo4j.Bolt.v1.runtime
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using ValueSource = org.junit.jupiter.@params.provider.ValueSource;


	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using BoltStateMachineState = Org.Neo4j.Bolt.runtime.BoltStateMachineState;
	using MutableConnectionState = Org.Neo4j.Bolt.runtime.MutableConnectionState;
	using StateMachineContext = Org.Neo4j.Bolt.runtime.StateMachineContext;
	using StatementMetadata = Org.Neo4j.Bolt.runtime.StatementMetadata;
	using StatementProcessor = Org.Neo4j.Bolt.runtime.StatementProcessor;
	using AckFailureMessage = Org.Neo4j.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InterruptSignal = Org.Neo4j.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using Bookmark = Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark;
	using AuthorizationExpiredException = Org.Neo4j.Graphdb.security.AuthorizationExpiredException;

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
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.ValueUtils.asMapValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	internal class ReadyStateTest
	{
		 private readonly ReadyState _state = new ReadyState();

		 private readonly BoltStateMachineState _streamingState = mock( typeof( BoltStateMachineState ) );
		 private readonly BoltStateMachineState _interruptedState = mock( typeof( BoltStateMachineState ) );
		 private readonly BoltStateMachineState _failedState = mock( typeof( BoltStateMachineState ) );
		 private readonly StatementProcessor _statementProcessor = mock( typeof( StatementProcessor ) );

		 private readonly StateMachineContext _context = mock( typeof( StateMachineContext ) );
		 private readonly MutableConnectionState _connectionState = new MutableConnectionState();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _state.StreamingState = _streamingState;
			  _state.InterruptedState = _interruptedState;
			  _state.FailedState = _failedState;

			  when( _context.connectionState() ).thenReturn(_connectionState);
			  when( _context.clock() ).thenReturn(Clock.systemUTC());
			  _connectionState.StatementProcessor = _statementProcessor;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenNotInitialized() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowWhenNotInitialized()
		 {
			  ReadyState state = new ReadyState();

			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(PullAllMessage.INSTANCE, _context) );

			  state.StreamingState = _streamingState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(PullAllMessage.INSTANCE, _context) );

			  state.StreamingState = null;
			  state.InterruptedState = _interruptedState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(PullAllMessage.INSTANCE, _context) );

			  state.InterruptedState = null;
			  state.FailedState = _failedState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(PullAllMessage.INSTANCE, _context) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessRunMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessRunMessage()
		 {
			  StatementMetadata statementMetadata = mock( typeof( StatementMetadata ) );
			  when( statementMetadata.FieldNames() ).thenReturn(new string[]{ "foo", "bar", "baz" });
			  when( _statementProcessor.run( any(), any() ) ).thenReturn(statementMetadata);

			  BoltResponseHandler responseHandler = mock( typeof( BoltResponseHandler ) );
			  _connectionState.ResponseHandler = responseHandler;

			  BoltStateMachineState nextState = _state.process( new RunMessage( "RETURN 1", EMPTY_MAP ), _context );

			  assertEquals( _streamingState, nextState );
			  verify( _statementProcessor ).run( "RETURN 1", EMPTY_MAP );
			  verify( responseHandler ).onMetadata( "fields", stringArray( "foo", "bar", "baz" ) );
			  verify( responseHandler ).onMetadata( eq( "result_available_after" ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleAuthFailureDuringRunMessageProcessing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleAuthFailureDuringRunMessageProcessing()
		 {
			  AuthorizationExpiredException error = new AuthorizationExpiredException( "Hello" );
			  when( _statementProcessor.run( any(), any() ) ).thenThrow(error);

			  BoltStateMachineState nextState = _state.process( new RunMessage( "RETURN 1", EMPTY_MAP ), _context );

			  assertEquals( _failedState, nextState );
			  verify( _context ).handleFailure( error, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleFailureDuringRunMessageProcessing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleFailureDuringRunMessageProcessing()
		 {
			  Exception error = new Exception( "Hello" );
			  when( _statementProcessor.run( any(), any() ) ).thenThrow(error);

			  BoltStateMachineState nextState = _state.process( new RunMessage( "RETURN 1", EMPTY_MAP ), _context );

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

			  assertEquals( _state, newState );
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
			  IList<RequestMessage> unsupportedMessages = new IList<RequestMessage> { PullAllMessage.INSTANCE, DiscardAllMessage.INSTANCE, AckFailureMessage.INSTANCE };

			  foreach ( RequestMessage message in unsupportedMessages )
			  {
					assertNull( _state.process( message, _context ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeginTransactionWithoutBookmark() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldBeginTransactionWithoutBookmark()
		 {
			  BoltStateMachineState newState = _state.process( new RunMessage( "BEGIN", EMPTY_MAP ), _context );
			  assertEquals( _streamingState, newState );
			  verify( _statementProcessor ).beginTransaction( null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeginTransactionWithSingleBookmark() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldBeginTransactionWithSingleBookmark()
		 {
			  IDictionary<string, object> @params = map( "bookmark", "neo4j:bookmark:v1:tx15" );

			  BoltStateMachineState newState = _state.process( new RunMessage( "BEGIN", asMapValue( @params ) ), _context );
			  assertEquals( _streamingState, newState );
			  verify( _statementProcessor ).beginTransaction( new Bookmark( 15 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeginTransactionWithMultipleBookmarks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldBeginTransactionWithMultipleBookmarks()
		 {
			  IDictionary<string, object> @params = map( "bookmarks", asList( "neo4j:bookmark:v1:tx7", "neo4j:bookmark:v1:tx1", "neo4j:bookmark:v1:tx92", "neo4j:bookmark:v1:tx39" ) );

			  BoltStateMachineState newState = _state.process( new RunMessage( "BEGIN", asMapValue( @params ) ), _context );
			  assertEquals( _streamingState, newState );
			  verify( _statementProcessor ).beginTransaction( new Bookmark( 92 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(strings = {"begin", "BEGIN", "   begin   ", "   BeGiN ;   "}) void shouldBeginTransaction(String statement) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldBeginTransaction( string statement )
		 {
			  BoltStateMachineState newState = _state.process( new RunMessage( statement ), _context );
			  assertEquals( _streamingState, newState );
			  verify( _statementProcessor ).beginTransaction( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(strings = {"commit", "COMMIT", "   commit   ", "   CoMmIt ;   "}) void shouldCommitTransaction(String statement) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCommitTransaction( string statement )
		 {
			  BoltStateMachineState newState = _state.process( new RunMessage( statement ), _context );
			  assertEquals( _streamingState, newState );
			  verify( _statementProcessor ).commitTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(strings = {"rollback", "ROLLBACK", "   rollback   ", "   RoLlBaCk ;   "}) void shouldRollbackTransaction(String statement) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRollbackTransaction( string statement )
		 {
			  BoltStateMachineState newState = _state.process( new RunMessage( statement ), _context );
			  assertEquals( _streamingState, newState );
			  verify( _statementProcessor ).rollbackTransaction();
		 }
	}

}