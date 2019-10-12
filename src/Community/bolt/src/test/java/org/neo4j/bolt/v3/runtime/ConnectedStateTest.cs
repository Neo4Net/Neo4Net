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
namespace Neo4Net.Bolt.v3.runtime
{
	using Test = org.junit.jupiter.api.Test;

	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using MutableConnectionState = Neo4Net.Bolt.runtime.MutableConnectionState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using AuthenticationResult = Neo4Net.Bolt.security.auth.AuthenticationResult;
	using HelloMessage = Neo4Net.Bolt.v3.messaging.request.HelloMessage;
	using StringValue = Neo4Net.Values.Storable.StringValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
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
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken_Fields.CREDENTIALS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken_Fields.PRINCIPAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	internal class ConnectedStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddServerVersionMetadataOnHelloMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddServerVersionMetadataOnHelloMessage()
		 {
			  // Given
			  // hello message
			  IDictionary<string, object> meta = map( "user_agent", "3.0", PRINCIPAL, "neo4j", CREDENTIALS, "password" );
			  HelloMessage helloMessage = new HelloMessage( meta );

			  // setup state machine
			  ConnectedState state = new ConnectedState();
			  BoltStateMachineState readyState = mock( typeof( BoltStateMachineState ) );

			  StateMachineContext context = mock( typeof( StateMachineContext ) );
			  BoltStateMachineSPI boltSpi = mock( typeof( BoltStateMachineSPI ), RETURNS_MOCKS );
			  MutableConnectionState connectionState = new MutableConnectionState();

			  state.ReadyState = readyState;

			  when( context.BoltSpi() ).thenReturn(boltSpi);
			  when( context.ConnectionState() ).thenReturn(connectionState);

			  when( boltSpi.Version() ).thenReturn("42.42.42");
			  MutableConnectionState connectionStateMock = mock( typeof( MutableConnectionState ) );
			  when( context.ConnectionState() ).thenReturn(connectionStateMock);
			  when( context.ConnectionId() ).thenReturn("connection-uuid");

			  when( boltSpi.Authenticate( meta ) ).thenReturn( AuthenticationResult.AUTH_DISABLED );

			  // When
			  BoltStateMachineState newState = state.Process( helloMessage, context );

			  // Then
			  assertEquals( readyState, newState );
			  verify( connectionStateMock ).onMetadata( "server", stringValue( "42.42.42" ) );
			  verify( connectionStateMock ).onMetadata( eq( "connection_id" ), any( typeof( StringValue ) ) );
		 }
	}

}