﻿/*
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
namespace Org.Neo4j.Bolt.v1.transport.socket.client
{
	using WebSocketClient = org.eclipse.jetty.websocket.client.WebSocketClient;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class WebSocketConnectionTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowAnyExceptionWhenDataReceivedBeforeClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotThrowAnyExceptionWhenDataReceivedBeforeClose()
		 {
			  // Given
			  WebSocketClient client = mock( typeof( WebSocketClient ) );
			  WebSocketConnection conn = new WebSocketConnection( client );
			  when( client.Stopped ).thenReturn( true );

			  sbyte[] data = new sbyte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			  // When
			  conn.OnWebSocketBinary( data, 0, 10 );
			  conn.Recv( 10 );

			  // Then
			  // no exception
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIOExceptionWhenNotEnoughDataReceivedBeforeClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIOExceptionWhenNotEnoughDataReceivedBeforeClose()
		 {
			  // Given
			  WebSocketClient client = mock( typeof( WebSocketClient ) );
			  WebSocketConnection conn = new WebSocketConnection( client );
			  when( client.Stopped ).thenReturn( true, true );

			  sbyte[] data = new sbyte[] { 0, 1, 2, 3 };

			  // When && Then
			  conn.OnWebSocketBinary( data, 0, 4 );

			  ExpectedException.expect( typeof( IOException ) );
			  ExpectedException.expectMessage( "Connection closed while waiting for data from the server." );
			  conn.Recv( 10 );
		 }
	}

}