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
namespace Neo4Net.Bolt.v2.transport.integration
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;


	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using Neo4jWithSocket = Neo4Net.Bolt.v1.transport.integration.Neo4jWithSocket;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using SecureSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using SecureWebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureWebSocketConnection;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using WebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.WebSocketConnection;
	using Neo4jPackV2 = Neo4Net.Bolt.v2.messaging.Neo4jPackV2;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using AnyValue = Neo4Net.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.spi.StreamMatchers.eqRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.auth_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.map;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BoltV2TransportIT
	public class BoltV2TransportIT
	{
		 private const string USER_AGENT = "TestClient/2.0";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket server = new org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket(getClass(), settings -> settings.put(auth_enabled.name(), "false"));
		 public Neo4jWithSocket Server = new Neo4jWithSocket( this.GetType(), settings => settings.put(auth_enabled.name(), "false") );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public Class connectionClass;
		 public Type ConnectionClass;

		 private HostnamePort _address;
		 private TransportConnection _connection;
		 private TransportTestUtil _util;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<Class> transports()
		 public static IList<Type> Transports()
		 {
			  return new IList<Type> { typeof( SocketConnection ), typeof( WebSocketConnection ), typeof( SecureSocketConnection ), typeof( SecureWebSocketConnection ) };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _address = Server.lookupDefaultConnector();
			  _connection = System.Activator.CreateInstance( ConnectionClass );
			  _util = new TransportTestUtil( new Neo4jPackV2() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  if ( _connection != null )
			  {
					_connection.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNegotiateProtocolV2() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNegotiateProtocolV2()
		 {
			  _connection.connect( _address ).send( _util.acceptedVersions( 2, 0, 0, 0 ) ).send( _util.chunk( new InitMessage( USER_AGENT, emptyMap() ) ) );

			  assertThat( _connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 2 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNegotiateProtocolV2WhenClientSupportsBothV1AndV2() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNegotiateProtocolV2WhenClientSupportsBothV1AndV2()
		 {
			  _connection.connect( _address ).send( _util.acceptedVersions( 2, 1, 0, 0 ) ).send( _util.chunk( new InitMessage( USER_AGENT, emptyMap() ) ) );

			  assertThat( _connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 2 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendPoint2D() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendPoint2D()
		 {
			  TestSendingOfBoltV2Value( pointValue( WGS84, 39.111748, -76.775635 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceivePoint2D() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceivePoint2D()
		 {
			  TestReceivingOfBoltV2Value( "RETURN point({x: 40.7624, y: 73.9738})", pointValue( Cartesian, 40.7624, 73.9738 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceivePoint2D() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceivePoint2D()
		 {
			  TestSendingAndReceivingOfBoltV2Value( pointValue( WGS84, 38.8719, 77.0563 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendDuration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendDuration()
		 {
			  TestSendingOfBoltV2Value( duration( 5, 3, 34, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveDuration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveDuration()
		 {
			  TestReceivingOfBoltV2Value( "RETURN duration({months: 3, days: 100, seconds: 999, nanoseconds: 42})", duration( 3, 100, 999, 42 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceiveDuration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceiveDuration()
		 {
			  TestSendingAndReceivingOfBoltV2Value( duration( 17, 9, 2, 1_000_000 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendDate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendDate()
		 {
			  TestSendingOfBoltV2Value( date( 1991, 8, 24 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveDate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveDate()
		 {
			  TestReceivingOfBoltV2Value( "RETURN date('2015-02-18')", date( 2015, 2, 18 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceiveDate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceiveDate()
		 {
			  TestSendingAndReceivingOfBoltV2Value( date( 2005, 5, 22 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendLocalTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendLocalTime()
		 {
			  TestSendingOfBoltV2Value( localTime( 2, 35, 10, 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveLocalTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveLocalTime()
		 {
			  TestReceivingOfBoltV2Value( "RETURN localtime('11:04:35')", localTime( 11, 0x4, 35, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceiveLocalTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceiveLocalTime()
		 {
			  TestSendingAndReceivingOfBoltV2Value( localTime( 22, 10, 10, 99 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendTime()
		 {
			  TestSendingOfBoltV2Value( time( 424242, ZoneOffset.of( "+08:30" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveTime()
		 {
			  TestReceivingOfBoltV2Value( "RETURN time('14:30+0100')", time( 14, 30, 0, 0, ZoneOffset.ofHours( 1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceiveTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceiveTime()
		 {
			  TestSendingAndReceivingOfBoltV2Value( time( 19, 22, 44, 100, ZoneOffset.ofHours( -5 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendLocalDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendLocalDateTime()
		 {
			  TestSendingOfBoltV2Value( localDateTime( 2002, 5, 22, 15, 15, 25, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveLocalDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveLocalDateTime()
		 {
			  TestReceivingOfBoltV2Value( "RETURN localdatetime('20150202T19:32:24')", localDateTime( 2015, 2, 2, 19, 32, 24, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceiveLocalDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceiveLocalDateTime()
		 {
			  TestSendingAndReceivingOfBoltV2Value( localDateTime( 1995, 12, 12, 10, 30, 0, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendDateTimeWithTimeZoneName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendDateTimeWithTimeZoneName()
		 {
			  TestSendingOfBoltV2Value( datetime( 1956, 9, 14, 11, 20, 25, 0, "Europe/Stockholm" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveDateTimeWithTimeZoneName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveDateTimeWithTimeZoneName()
		 {
			  TestReceivingOfBoltV2Value( "RETURN datetime({year:1984, month:10, day:11, hour:21, minute:30, timezone:'Europe/London'})", datetime( 1984, 10, 11, 21, 30, 0, 0, "Europe/London" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceiveDateTimeWithTimeZoneName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceiveDateTimeWithTimeZoneName()
		 {
			  TestSendingAndReceivingOfBoltV2Value( datetime( 1984, 10, 11, 21, 30, 0, 0, "Europe/London" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendDateTimeWithTimeZoneOffset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendDateTimeWithTimeZoneOffset()
		 {
			  TestSendingOfBoltV2Value( datetime( 424242, 0, ZoneOffset.ofHoursMinutes( -7, -15 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveDateTimeWithTimeZoneOffset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveDateTimeWithTimeZoneOffset()
		 {
			  TestReceivingOfBoltV2Value( "RETURN datetime({year:2022, month:3, day:2, hour:19, minute:10, timezone:'+02:30'})", datetime( 2022, 3, 2, 19, 10, 0, 0, ZoneOffset.ofHoursMinutes( 2, 30 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceiveDateTimeWithTimeZoneOffset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceiveDateTimeWithTimeZoneOffset()
		 {
			  TestSendingAndReceivingOfBoltV2Value( datetime( 1899, 1, 1, 12, 12, 32, 0, ZoneOffset.ofHoursMinutes( -4, -15 ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T extends org.neo4j.values.AnyValue> void testSendingOfBoltV2Value(T value) throws Exception
		 private void TestSendingOfBoltV2Value<T>( T value ) where T : Neo4Net.Values.AnyValue
		 {
			  NegotiateBoltV2();

			  _connection.send( _util.chunk( new RunMessage( "CREATE (n:Node {value: $value}) RETURN 42", map( new string[]{ "value" }, new AnyValue[]{ value } ) ), PullAllMessage.INSTANCE ) );

			  assertThat( _connection, _util.eventuallyReceives( msgSuccess(), msgRecord(eqRecord(equalTo(longValue(42)))), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T extends org.neo4j.values.AnyValue> void testReceivingOfBoltV2Value(String query, T expectedValue) throws Exception
		 private void TestReceivingOfBoltV2Value<T>( string query, T expectedValue ) where T : Neo4Net.Values.AnyValue
		 {
			  NegotiateBoltV2();

			  _connection.send( _util.chunk( new RunMessage( query ), PullAllMessage.INSTANCE ) );

			  assertThat( _connection, _util.eventuallyReceives( msgSuccess(), msgRecord(eqRecord(equalTo(expectedValue))), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T extends org.neo4j.values.AnyValue> void testSendingAndReceivingOfBoltV2Value(T value) throws Exception
		 private void TestSendingAndReceivingOfBoltV2Value<T>( T value ) where T : Neo4Net.Values.AnyValue
		 {
			  NegotiateBoltV2();

			  _connection.send( _util.chunk( new RunMessage( "RETURN $value", map( new string[]{ "value" }, new AnyValue[]{ value } ) ), PullAllMessage.INSTANCE ) );

			  assertThat( _connection, _util.eventuallyReceives( msgSuccess(), msgRecord(eqRecord(equalTo(value))), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void negotiateBoltV2() throws Exception
		 private void NegotiateBoltV2()
		 {
			  _connection.connect( _address ).send( _util.acceptedVersions( 2, 0, 0, 0 ) ).send( _util.chunk( new InitMessage( USER_AGENT, emptyMap() ) ) );

			  assertThat( _connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 2 } ) );
			  assertThat( _connection, _util.eventuallyReceives( msgSuccess() ) );
		 }
	}

}