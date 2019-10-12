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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using PackedOutputArray = Neo4Net.Bolt.v1.packstream.PackedOutputArray;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AnyValue = Neo4Net.Values.AnyValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Edges.ALICE_KNOWS_BOB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.ALICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.ALL_PATHS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyDisconnects;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.auth_enabled;

	public class UnsupportedStructTypesV1V2IT : AbstractBoltTransportsTest
	{
		 private const string USER_AGENT = "TestClient/1.0";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4jWithSocket server = new Neo4jWithSocket(getClass(), settings -> settings.put(auth_enabled.name(), "false"));
		 public Neo4jWithSocket Server = new Neo4jWithSocket( this.GetType(), settings => settings.put(auth_enabled.name(), "false") );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  Address = Server.lookupDefaultConnector();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenNullKeyIsSentWithInit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenNullKeyIsSentWithInit()
		 {
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() );
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );

			  Connection.send( Util.chunk( 64, CreateMsgWithNullKey( InitMessage.SIGNATURE ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Value `null` is not supported as key in maps, must be a non-nullable string." ) ) );
			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenDuplicateKeyIsSentWithInit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenDuplicateKeyIsSentWithInit()
		 {
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() );
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );

			  Connection.send( Util.chunk( 64, CreateMsgWithDuplicateKey( InitMessage.SIGNATURE ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Duplicate map key `key1`." ) ) );
			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenNullKeyIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenNullKeyIsSentWithRun()
		 {
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() );
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  Connection.send( Util.chunk( new InitMessage( USER_AGENT, Collections.emptyMap() ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  Connection.send( Util.chunk( 64, CreateMsgWithNullKey( RunMessage.SIGNATURE ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Value `null` is not supported as key in maps, must be a non-nullable string." ) ) );
			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenDuplicateKeyIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenDuplicateKeyIsSentWithRun()
		 {
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() );
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  Connection.send( Util.chunk( new InitMessage( USER_AGENT, Collections.emptyMap() ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  Connection.send( Util.chunk( 64, CreateMsgWithDuplicateKey( RunMessage.SIGNATURE ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Duplicate map key `key1`." ) ) );
			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenNodeIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenNodeIsSentWithRun()
		 {
			  TestFailureWithV1Value( ALICE, "Node" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenRelationshipIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenRelationshipIsSentWithRun()
		 {
			  TestFailureWithV1Value( ALICE_KNOWS_BOB, "Relationship" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenPathIsSentWithRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenPathIsSentWithRun()
		 {
			  foreach ( PathValue path in ALL_PATHS )
			  {
					try
					{
						 TestFailureWithV1Value( path, "Path" );
					}
					finally
					{
						 Reconnect();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateConnectionWhenUnknownMessageIsSent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateConnectionWhenUnknownMessageIsSent()
		 {
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() );
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  Connection.send( Util.chunk( new InitMessage( USER_AGENT, Collections.emptyMap() ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  Connection.send( Util.chunk( 64, CreateMsg( ( sbyte )'A' ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateConnectionWhenUnknownTypeIsSent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateConnectionWhenUnknownTypeIsSent()
		 {
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() );
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  Connection.send( Util.chunk( new InitMessage( USER_AGENT, Collections.emptyMap() ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  Connection.send( Util.chunk( 64, CreateMsgWithUnknownValue( RunMessage.SIGNATURE ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testFailureWithV1Value(org.neo4j.values.AnyValue value, String description) throws Exception
		 private void TestFailureWithV1Value( AnyValue value, string description )
		 {
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() );
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  Connection.send( Util.chunk( new InitMessage( USER_AGENT, Collections.emptyMap() ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  Connection.send( Util.chunk( 64, CreateRunWithV1Value( value ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Statement.TypeError, description + " values cannot be unpacked with this version of bolt." ) ) );
			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] createRunWithV1Value(org.neo4j.values.AnyValue value) throws java.io.IOException
		 private sbyte[] CreateRunWithV1Value( AnyValue value )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = Neo4jPack.newPacker( @out );

			  packer.PackStructHeader( 2, RunMessage.SIGNATURE );
			  packer.Pack( "RETURN $x" );
			  packer.PackMapHeader( 1 );
			  packer.Pack( "x" );
			  packer.Pack( value );

			  return @out.Bytes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] createMsg(byte signature) throws java.io.IOException
		 private sbyte[] CreateMsg( sbyte signature )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = Neo4jPack.newPacker( @out );

			  packer.PackStructHeader( 0, signature );

			  return @out.Bytes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] createMsgWithNullKey(byte signature) throws java.io.IOException
		 private sbyte[] CreateMsgWithNullKey( sbyte signature )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = Neo4jPack.newPacker( @out );

			  packer.PackStructHeader( 2, signature );
			  packer.Pack( "Text" );
			  PackMapWithNullKey( packer );

			  return @out.Bytes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] createMsgWithDuplicateKey(byte signature) throws java.io.IOException
		 private sbyte[] CreateMsgWithDuplicateKey( sbyte signature )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = Neo4jPack.newPacker( @out );

			  packer.PackStructHeader( 2, signature );
			  packer.Pack( "Text" );
			  PackMapWithDuplicateKey( packer );

			  return @out.Bytes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] createMsgWithUnknownValue(byte signature) throws java.io.IOException
		 private sbyte[] CreateMsgWithUnknownValue( sbyte signature )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = Neo4jPack.newPacker( @out );

			  packer.PackStructHeader( 2, signature );
			  packer.Pack( "Text" );
			  packer.PackMapHeader( 1 );
			  packer.Pack( "key1" );
			  packer.PackStructHeader( 0, ( sbyte )'A' );

			  return @out.Bytes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void packMapWithNullKey(org.neo4j.bolt.messaging.Neo4jPack_Packer packer) throws java.io.IOException
		 private static void PackMapWithNullKey( Neo4Net.Bolt.messaging.Neo4jPack_Packer packer )
		 {
			  packer.PackMapHeader( 2 );
			  packer.Pack( "key1" );
			  packer.Pack( ValueUtils.of( null ) );
			  packer.Pack( ValueUtils.of( null ) );
			  packer.Pack( "value1" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void packMapWithDuplicateKey(org.neo4j.bolt.messaging.Neo4jPack_Packer packer) throws java.io.IOException
		 private static void PackMapWithDuplicateKey( Neo4Net.Bolt.messaging.Neo4jPack_Packer packer )
		 {
			  packer.PackMapHeader( 2 );
			  packer.Pack( "key1" );
			  packer.Pack( "value1" );
			  packer.Pack( "key1" );
			  packer.Pack( "value2" );
		 }
	}

}