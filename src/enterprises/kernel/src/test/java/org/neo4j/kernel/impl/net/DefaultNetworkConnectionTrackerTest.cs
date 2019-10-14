/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.net
{
	using Test = org.junit.jupiter.api.Test;

	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using TrackedNetworkConnection = Neo4Net.Kernel.api.net.TrackedNetworkConnection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class DefaultNetworkConnectionTrackerTest
	{
		 private readonly TrackedNetworkConnection _connection1 = ConnectionMock( "1" );
		 private readonly TrackedNetworkConnection _connection2 = ConnectionMock( "2" );
		 private readonly TrackedNetworkConnection _connection3 = ConnectionMock( "3" );

		 private readonly NetworkConnectionTracker _tracker = new DefaultNetworkConnectionTracker();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateIds()
		 internal virtual void ShouldCreateIds()
		 {
			  assertEquals( "bolt-0", _tracker.newConnectionId( "bolt" ) );
			  assertEquals( "bolt-1", _tracker.newConnectionId( "bolt" ) );
			  assertEquals( "bolt-2", _tracker.newConnectionId( "bolt" ) );

			  assertEquals( "http-3", _tracker.newConnectionId( "http" ) );
			  assertEquals( "http-4", _tracker.newConnectionId( "http" ) );

			  assertEquals( "https-5", _tracker.newConnectionId( "https" ) );
			  assertEquals( "https-6", _tracker.newConnectionId( "https" ) );
			  assertEquals( "https-7", _tracker.newConnectionId( "https" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAllowAddingOfConnections()
		 internal virtual void ShouldAllowAddingOfConnections()
		 {
			  _tracker.add( _connection1 );
			  _tracker.add( _connection2 );
			  _tracker.add( _connection3 );

			  assertThat( _tracker.activeConnections(), containsInAnyOrder(_connection1, _connection2, _connection3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailWhenAddingConnectionWithSameId()
		 internal virtual void ShouldFailWhenAddingConnectionWithSameId()
		 {
			  _tracker.add( _connection1 );

			  assertThrows( typeof( System.ArgumentException ), () => _tracker.add(_connection1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRemoveConnections()
		 internal virtual void ShouldRemoveConnections()
		 {
			  _tracker.add( _connection1 );
			  _tracker.add( _connection2 );
			  _tracker.add( _connection3 );

			  _tracker.remove( _connection2 );
			  assertThat( _tracker.activeConnections(), containsInAnyOrder(_connection1, _connection3) );

			  _tracker.remove( _connection1 );
			  assertThat( _tracker.activeConnections(), containsInAnyOrder(_connection3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDoNothingWhenRemovingUnknownConnection()
		 internal virtual void ShouldDoNothingWhenRemovingUnknownConnection()
		 {
			  _tracker.add( _connection1 );
			  _tracker.add( _connection3 );
			  assertThat( _tracker.activeConnections(), containsInAnyOrder(_connection1, _connection3) );

			  _tracker.remove( _connection2 );

			  assertThat( _tracker.activeConnections(), containsInAnyOrder(_connection1, _connection3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGetKnownConnectionById()
		 internal virtual void ShouldGetKnownConnectionById()
		 {
			  _tracker.add( _connection1 );
			  _tracker.add( _connection2 );

			  assertEquals( _connection1, _tracker.get( _connection1.id() ) );
			  assertEquals( _connection2, _tracker.get( _connection2.id() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnNullForUnknownId()
		 internal virtual void ShouldReturnNullForUnknownId()
		 {
			  _tracker.add( _connection1 );

			  assertNotNull( _tracker.get( _connection1.id() ) );
			  assertNull( _tracker.get( _connection2.id() ) );
			  assertNull( _tracker.get( _connection3.id() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldListActiveConnectionsWhenEmpty()
		 internal virtual void ShouldListActiveConnectionsWhenEmpty()
		 {
			  assertThat( _tracker.activeConnections(), empty() );
		 }

		 private static TrackedNetworkConnection ConnectionMock( string id )
		 {
			  TrackedNetworkConnection connection = mock( typeof( TrackedNetworkConnection ) );
			  when( connection.Id() ).thenReturn(id);
			  return connection;
		 }
	}

}