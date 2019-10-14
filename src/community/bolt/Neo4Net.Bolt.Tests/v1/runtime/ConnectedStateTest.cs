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
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using MutableConnectionState = Neo4Net.Bolt.runtime.MutableConnectionState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using AuthenticationResult = Neo4Net.Bolt.security.auth.AuthenticationResult;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken.newBasicAuthToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	internal class ConnectedStateTest
	{
		 private const string USER_AGENT = "Driver 2.0";
		 private static readonly IDictionary<string, object> _authToken = newBasicAuthToken( "neo4j", "password" );
		 private static readonly InitMessage _initMessage = new InitMessage( USER_AGENT, _authToken );

		 private readonly ConnectedState _state = new ConnectedState();

		 private readonly BoltStateMachineState _readyState = mock( typeof( BoltStateMachineState ) );
		 private readonly BoltStateMachineState _failedState = mock( typeof( BoltStateMachineState ) );

		 private readonly StateMachineContext _context = mock( typeof( StateMachineContext ) );
		 private readonly BoltStateMachineSPI _boltSpi = mock( typeof( BoltStateMachineSPI ), RETURNS_MOCKS );
		 private readonly MutableConnectionState _connectionState = new MutableConnectionState();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _state.ReadyState = _readyState;
			  _state.FailedState = _failedState;

			  when( _context.boltSpi() ).thenReturn(_boltSpi);
			  when( _context.connectionState() ).thenReturn(_connectionState);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenNotInitialized() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowWhenNotInitialized()
		 {
			  ConnectedState state = new ConnectedState();

			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(_initMessage, _context) );

			  state.ReadyState = _readyState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(_initMessage, _context) );

			  state.ReadyState = null;
			  state.FailedState = _failedState;
			  assertThrows( typeof( System.InvalidOperationException ), () => state.Process(_initMessage, _context) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAuthenticateOnInitMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAuthenticateOnInitMessage()
		 {
			  BoltStateMachineState newState = _state.process( _initMessage, _context );

			  assertEquals( _readyState, newState );
			  verify( _boltSpi ).authenticate( _authToken );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInitializeStatementProcessorOnInitMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldInitializeStatementProcessorOnInitMessage()
		 {
			  BoltStateMachineState newState = _state.process( _initMessage, _context );

			  assertEquals( _readyState, newState );
			  assertNotNull( _connectionState.StatementProcessor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddMetadataOnExpiredCredentialsOnInitMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddMetadataOnExpiredCredentialsOnInitMessage()
		 {
			  MutableConnectionState connectionStateMock = mock( typeof( MutableConnectionState ) );
			  when( _context.connectionState() ).thenReturn(connectionStateMock);

			  AuthenticationResult authResult = mock( typeof( AuthenticationResult ) );
			  when( authResult.CredentialsExpired() ).thenReturn(true);
			  when( authResult.LoginContext ).thenReturn( LoginContext.AUTH_DISABLED );
			  when( _boltSpi.authenticate( _authToken ) ).thenReturn( authResult );

			  BoltStateMachineState newState = _state.process( _initMessage, _context );

			  assertEquals( _readyState, newState );
			  verify( connectionStateMock ).onMetadata( "credentials_expired", TRUE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddServerVersionMetadataOnInitMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddServerVersionMetadataOnInitMessage()
		 {
			  when( _boltSpi.version() ).thenReturn("42.42.42");
			  MutableConnectionState connectionStateMock = mock( typeof( MutableConnectionState ) );
			  when( _context.connectionState() ).thenReturn(connectionStateMock);

			  AuthenticationResult authResult = mock( typeof( AuthenticationResult ) );
			  when( authResult.CredentialsExpired() ).thenReturn(true);
			  when( authResult.LoginContext ).thenReturn( LoginContext.AUTH_DISABLED );
			  when( _boltSpi.authenticate( _authToken ) ).thenReturn( authResult );

			  BoltStateMachineState newState = _state.process( _initMessage, _context );

			  assertEquals( _readyState, newState );
			  verify( connectionStateMock ).onMetadata( "server", stringValue( "42.42.42" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRegisterClientInUDCOnInitMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRegisterClientInUDCOnInitMessage()
		 {
			  BoltStateMachineState newState = _state.process( _initMessage, _context );

			  assertEquals( _readyState, newState );
			  verify( _boltSpi ).udcRegisterClient( eq( USER_AGENT ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleFailuresOnInitMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleFailuresOnInitMessage()
		 {
			  Exception error = new Exception( "Hello" );
			  when( _boltSpi.authenticate( _authToken ) ).thenThrow( error );

			  BoltStateMachineState newState = _state.process( _initMessage, _context );

			  assertEquals( _failedState, newState );
			  verify( _context ).handleFailure( error, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotProcessUnsupportedMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotProcessUnsupportedMessage()
		 {
			  IList<RequestMessage> unsupportedMessages = new IList<RequestMessage> { AckFailureMessage.INSTANCE, DiscardAllMessage.INSTANCE, InterruptSignal.INSTANCE, PullAllMessage.INSTANCE, ResetMessage.INSTANCE, new RunMessage( "RETURN 1", EMPTY_MAP ) };

			  foreach ( RequestMessage message in unsupportedMessages )
			  {
					assertNull( _state.process( message, _context ) );
			  }
		 }
	}

}