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
namespace Neo4Net.Bolt.v1.transport.socket.client
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;


	public class SocketConnectionTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyReadOnceIfAllBytesAreRead() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyReadOnceIfAllBytesAreRead()
		 {
			  // GIVEN
			  Socket socket = mock( typeof( Socket ) );
			  Stream stream = mock( typeof( Stream ) );
			  when( socket.InputStream ).thenReturn( stream );
			  when( stream.Read( any( typeof( sbyte[] ) ), anyInt(), anyInt() ) ).thenReturn(4);
			  SocketConnection connection = new SocketConnection( socket );
			  connection.Connect( new HostnamePort( "my.domain", 1234 ) );

			  // WHEN
			  connection.Recv( 4 );

			  // THEN
			  verify( stream, times( 1 ) ).read( any( typeof( sbyte[] ) ), anyInt(), anyInt() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyReadUntilAllBytesAreRead() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyReadUntilAllBytesAreRead()
		 {
			  // GIVEN
			  Socket socket = mock( typeof( Socket ) );
			  Stream stream = mock( typeof( Stream ) );
			  when( socket.InputStream ).thenReturn( stream );
			  when( stream.Read( any( typeof( sbyte[] ) ), anyInt(), anyInt() ) ).thenReturn(4).thenReturn(4).thenReturn(2).thenReturn(-1);
			  SocketConnection connection = new SocketConnection( socket );
			  connection.Connect( new HostnamePort( "my.domain", 1234 ) );

			  // WHEN
			  connection.Recv( 10 );

			  // THEN
			  verify( stream, times( 3 ) ).read( any( typeof( sbyte[] ) ), anyInt(), anyInt() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfNotEnoughBytesAreRead() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfNotEnoughBytesAreRead()
		 {
			  // GIVEN
			  Socket socket = mock( typeof( Socket ) );
			  Stream stream = mock( typeof( Stream ) );
			  when( socket.InputStream ).thenReturn( stream );
			  when( stream.Read( any( typeof( sbyte[] ) ), anyInt(), anyInt() ) ).thenReturn(4).thenReturn(-1);
			  SocketConnection connection = new SocketConnection( socket );
			  connection.Connect( new HostnamePort( "my.domain", 1234 ) );

			  // EXPECT
			  ExpectedException.expect( typeof( IOException ) );

			  // WHEN
			  connection.Recv( 10 );
		 }
	}

}