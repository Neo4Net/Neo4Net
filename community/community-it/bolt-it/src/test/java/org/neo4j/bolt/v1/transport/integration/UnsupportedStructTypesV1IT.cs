﻿using System;
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
namespace Org.Neo4j.Bolt.v1.transport.integration
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using Neo4jPackV1 = Org.Neo4j.Bolt.v1.messaging.Neo4jPackV1;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using PackedOutputArray = Org.Neo4j.Bolt.v1.packstream.PackedOutputArray;
	using SecureSocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using SecureWebSocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.SecureWebSocketConnection;
	using SocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Org.Neo4j.Bolt.v1.transport.socket.client.TransportConnection;
	using WebSocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.WebSocketConnection;
	using Neo4jPackV2 = Org.Neo4j.Bolt.v2.messaging.Neo4jPackV2;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyDisconnects;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.auth_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class UnsupportedStructTypesV1IT
	public class UnsupportedStructTypesV1IT
	{
		 private const string USER_AGENT = "TestClient/1.0";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4jWithSocket server = new Neo4jWithSocket(getClass(), settings -> settings.put(auth_enabled.name(), "false"));
		 public Neo4jWithSocket Server = new Neo4jWithSocket( this.GetType(), settings => settings.put(auth_enabled.name(), "false") );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Class connectionClass;
		 public Type ConnectionClass;

		 private HostnamePort _address;
		 private TransportConnection _connection;
		 private TransportTestUtil _util;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<Class> transports()
		 public static IList<Type> Transports()
		 {
			  return new IList<Type> { typeof( SocketConnection ), typeof( WebSocketConnection ), typeof( SecureSocketConnection ), typeof( SecureWebSocketConnection ) };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _address = Server.lookupDefaultConnector();
			  _connection = System.Activator.CreateInstance( ConnectionClass );
			  _util = new TransportTestUtil( new Neo4jPackV1() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  if ( _connection != null )
			  {
					_connection.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenPoint2DIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenPoint2DIsSentWithRun()
		 {
			  TestFailureWithV2Value( pointValue( CoordinateReferenceSystem.WGS84, 1.2, 3.4 ), "Point" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenPoint3DIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenPoint3DIsSentWithRun()
		 {
			  TestFailureWithV2Value( pointValue( CoordinateReferenceSystem.WGS84_3D, 1.2, 3.4, 4.5 ), "Point" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenDurationIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenDurationIsSentWithRun()
		 {
			  TestFailureWithV2Value( ValueUtils.of( Duration.ofDays( 10 ) ), "Duration" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenDateIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenDateIsSentWithRun()
		 {
			  TestFailureWithV2Value( ValueUtils.of( LocalDate.now() ), "Date" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenLocalTimeIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenLocalTimeIsSentWithRun()
		 {
			  TestFailureWithV2Value( ValueUtils.of( LocalTime.now() ), "LocalTime" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenLocalDateTimeIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenLocalDateTimeIsSentWithRun()
		 {
			  TestFailureWithV2Value( ValueUtils.of( DateTime.Now ), "LocalDateTime" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenOffsetTimeIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenOffsetTimeIsSentWithRun()
		 {
			  TestFailureWithV2Value( ValueUtils.of( OffsetTime.now() ), "OffsetTime" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenOffsetDateTimeIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenOffsetDateTimeIsSentWithRun()
		 {
			  TestFailureWithV2Value( ValueUtils.of( OffsetDateTime.now() ), "OffsetDateTime" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenZonedDateTimeIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenZonedDateTimeIsSentWithRun()
		 {
			  TestFailureWithV2Value( ValueUtils.of( ZonedDateTime.now() ), "ZonedDateTime" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testFailureWithV2Value(org.neo4j.values.AnyValue value, String description) throws Exception
		 private void TestFailureWithV2Value( AnyValue value, string description )
		 {
			  _connection.connect( _address ).send( _util.defaultAcceptedVersions() );
			  assertThat( _connection, _util.eventuallyReceivesSelectedProtocolVersion() );
			  _connection.send( _util.chunk( new InitMessage( USER_AGENT, Collections.emptyMap() ) ) );
			  assertThat( _connection, _util.eventuallyReceives( msgSuccess() ) );

			  _connection.send( _util.chunk( 64, CreateRunWithV2Value( value ) ) );

			  assertThat( _connection, _util.eventuallyReceives( msgFailure( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.TypeError, description + " values cannot be unpacked with this version of bolt." ) ) );
			  assertThat( _connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] createRunWithV2Value(org.neo4j.values.AnyValue value) throws java.io.IOException
		 private sbyte[] CreateRunWithV2Value( AnyValue value )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = ( new Neo4jPackV2() ).newPacker(@out);

			  packer.PackStructHeader( 2, RunMessage.SIGNATURE );
			  packer.Pack( "RETURN $x" );
			  packer.PackMapHeader( 1 );
			  packer.Pack( "x" );
			  packer.Pack( value );

			  return @out.Bytes();
		 }
	}

}