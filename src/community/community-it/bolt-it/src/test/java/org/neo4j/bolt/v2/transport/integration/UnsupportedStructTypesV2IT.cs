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


	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using StructType = Neo4Net.Bolt.messaging.StructType;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using PackedOutputArray = Neo4Net.Bolt.v1.packstream.PackedOutputArray;
	using Neo4NetWithSocket = Neo4Net.Bolt.v1.transport.integration.Neo4NetWithSocket;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using SecureSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using SecureWebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureWebSocketConnection;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using WebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.WebSocketConnection;
	using Neo4NetPackV2 = Neo4Net.Bolt.v2.messaging.Neo4NetPackV2;
	using Neo4Net.Functions;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.transport.integration.TransportTestUtil.eventuallyDisconnects;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.auth_enabled;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class UnsupportedStructTypesV2IT
	public class UnsupportedStructTypesV2IT
	{
		 private const string USER_AGENT = "TestClient/2.0";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.bolt.v1.transport.integration.Neo4NetWithSocket server = new org.Neo4Net.bolt.v1.transport.integration.Neo4NetWithSocket(getClass(), settings -> settings.put(auth_enabled.name(), "false"));
		 public Neo4NetWithSocket Server = new Neo4NetWithSocket( this.GetType(), settings => settings.put(auth_enabled.name(), "false") );

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
			  _util = new TransportTestUtil( new Neo4NetPackV2() );
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
//ORIGINAL LINE: @Test public void shouldFailWhenPoint2DIsSentWithInvalidCrsId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenPoint2DIsSentWithInvalidCrsId()
		 {
			  TestFailureWithUnpackableValue(packer =>
			  {
				packer.packStructHeader( 3, StructType.POINT_2D.signature() );
				packer.pack( Values.of( 5 ) );
				packer.pack( Values.of( 3.15 ) );
				packer.pack( Values.of( 4.012 ) );
			  }, "Unable to construct Point value: `Unknown coordinate reference system code: 5`");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenPoint3DIsSentWithInvalidCrsId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenPoint3DIsSentWithInvalidCrsId()
		 {
			  TestFailureWithUnpackableValue(packer =>
			  {
				packer.packStructHeader( 4, StructType.POINT_3D.signature() );
				packer.pack( Values.of( 1200 ) );
				packer.pack( Values.of( 3.15 ) );
				packer.pack( Values.of( 4.012 ) );
				packer.pack( Values.of( 5.905 ) );
			  }, "Unable to construct Point value: `Unknown coordinate reference system code: 1200`");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenPoint2DDimensionsDoNotMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenPoint2DDimensionsDoNotMatch()
		 {
			  TestDisconnectWithUnpackableValue(packer =>
			  {
				packer.packStructHeader( 3, StructType.POINT_3D.signature() );
				packer.pack( Values.of( CoordinateReferenceSystem.Cartesian_3D.Code ) );
				packer.pack( Values.of( 3.15 ) );
				packer.pack( Values.of( 4.012 ) );
			  }, "Unable to construct Point value: `Cannot create point, CRS cartesian-3d expects 3 dimensions, but got coordinates [3.15, 4.012]`");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenPoint3DDimensionsDoNotMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenPoint3DDimensionsDoNotMatch()
		 {
			  TestFailureWithUnpackableValue(packer =>
			  {
				packer.packStructHeader( 4, StructType.POINT_3D.signature() );
				packer.pack( Values.of( CoordinateReferenceSystem.Cartesian.Code ) );
				packer.pack( Values.of( 3.15 ) );
				packer.pack( Values.of( 4.012 ) );
				packer.pack( Values.of( 5.905 ) );
			  }, "Unable to construct Point value: `Cannot create point, CRS cartesian expects 2 dimensions, but got coordinates [3.15, 4.012, 5.905]`");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenZonedDateTimeZoneIdIsNotKnown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenZonedDateTimeZoneIdIsNotKnown()
		 {
			  TestFailureWithUnpackableValue(packer =>
			  {
				packer.packStructHeader( 3, StructType.DATE_TIME_WITH_ZONE_NAME.signature() );
				packer.pack( Values.of( 0 ) );
				packer.pack( Values.of( 0 ) );
				packer.pack( Values.of( "Europe/Marmaris" ) );
			  }, "Unable to construct ZonedDateTime value: `Unknown time-zone ID: Europe/Marmaris`");
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testFailureWithUnpackableValue(org.Neo4Net.function.ThrowingConsumer<org.Neo4Net.bolt.messaging.Neo4NetPack_Packer, java.io.IOException> valuePacker, String expectedMessage) throws Exception
		 private void TestFailureWithUnpackableValue( ThrowingConsumer<Neo4Net.Bolt.messaging.Neo4NetPack_Packer, IOException> valuePacker, string expectedMessage )
		 {
			  _connection.connect( _address ).send( _util.defaultAcceptedVersions() );
			  assertThat( _connection, _util.eventuallyReceivesSelectedProtocolVersion() );
			  _connection.send( _util.chunk( new InitMessage( USER_AGENT, Collections.emptyMap() ) ) );
			  assertThat( _connection, _util.eventuallyReceives( msgSuccess() ) );

			  _connection.send( _util.chunk( 64, CreateRunWith( valuePacker ) ) );

			  assertThat( _connection, _util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Statement.TypeError, expectedMessage ) ) );
			  assertThat( _connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testDisconnectWithUnpackableValue(org.Neo4Net.function.ThrowingConsumer<org.Neo4Net.bolt.messaging.Neo4NetPack_Packer, java.io.IOException> valuePacker, String expectedMessage) throws Exception
		 private void TestDisconnectWithUnpackableValue( ThrowingConsumer<Neo4Net.Bolt.messaging.Neo4NetPack_Packer, IOException> valuePacker, string expectedMessage )
		 {
			  _connection.connect( _address ).send( _util.defaultAcceptedVersions() );
			  assertThat( _connection, _util.eventuallyReceivesSelectedProtocolVersion() );
			  _connection.send( _util.chunk( new InitMessage( USER_AGENT, Collections.emptyMap() ) ) );
			  assertThat( _connection, _util.eventuallyReceives( msgSuccess() ) );

			  _connection.send( _util.chunk( 64, CreateRunWith( valuePacker ) ) );

			  assertThat( _connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] createRunWith(org.Neo4Net.function.ThrowingConsumer<org.Neo4Net.bolt.messaging.Neo4NetPack_Packer, java.io.IOException> valuePacker) throws java.io.IOException
		 private sbyte[] CreateRunWith( ThrowingConsumer<Neo4Net.Bolt.messaging.Neo4NetPack_Packer, IOException> valuePacker )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = ( new Neo4NetPackV2() ).newPacker(@out);

			  packer.PackStructHeader( 2, RunMessage.SIGNATURE );
			  packer.Pack( "RETURN $x" );
			  packer.PackMapHeader( 1 );
			  packer.Pack( "x" );
			  valuePacker.Accept( packer );

			  return @out.Bytes();
		 }
	}

}