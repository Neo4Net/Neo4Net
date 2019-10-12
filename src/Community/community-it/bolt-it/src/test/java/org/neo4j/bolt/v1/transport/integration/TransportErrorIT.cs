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

	using RecordingByteChannel = Neo4Net.Bolt.v1.messaging.RecordingByteChannel;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using BufferedChannelOutput = Neo4Net.Bolt.v1.packstream.BufferedChannelOutput;
	using PackStream = Neo4Net.Bolt.v1.packstream.PackStream;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyDisconnects;

	public class TransportErrorIT : AbstractBoltTransportsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4jWithSocket server = new Neo4jWithSocket(getClass());
		 public Neo4jWithSocket Server = new Neo4jWithSocket( this.GetType() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Address = Server.lookupDefaultConnector();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleIncorrectFraming() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleIncorrectFraming()
		 {
			  // Given I have a message that gets truncated in the chunking, so part of it is missing
			  sbyte[] truncated = serialize( Util.Neo4jPack, new RunMessage( "UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared" ) );
			  truncated = Arrays.copyOf( truncated, truncated.Length - 12 );

			  // When
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(32, truncated));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMessagesWithIncorrectFields() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMessagesWithIncorrectFields()
		 {
			  // Given I send a message with the wrong types in its fields
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.messaging.RecordingByteChannel rawData = new org.neo4j.bolt.v1.messaging.RecordingByteChannel();
			  RecordingByteChannel rawData = new RecordingByteChannel();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.packstream.PackStream.Packer packer = new org.neo4j.bolt.v1.packstream.PackStream.Packer(new org.neo4j.bolt.v1.packstream.BufferedChannelOutput(rawData));
			  PackStream.Packer packer = new PackStream.Packer( new BufferedChannelOutput( rawData ) );

			  packer.PackStructHeader( 2, RunMessage.SIGNATURE );
			  packer.Pack( "RETURN 1" );
			  packer.pack( 1234 ); // Should've been a map
			  packer.Flush();

			  sbyte[] invalidMessage = rawData.Bytes;

			  // When
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(32, invalidMessage));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUnknownMessages() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUnknownMessages()
		 {
			  // Given I send a message with an invalid type
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.messaging.RecordingByteChannel rawData = new org.neo4j.bolt.v1.messaging.RecordingByteChannel();
			  RecordingByteChannel rawData = new RecordingByteChannel();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.packstream.PackStream.Packer packer = new org.neo4j.bolt.v1.packstream.PackStream.Packer(new org.neo4j.bolt.v1.packstream.BufferedChannelOutput(rawData));
			  PackStream.Packer packer = new PackStream.Packer( new BufferedChannelOutput( rawData ) );

			  packer.PackStructHeader( 1, ( sbyte )0x66 ); // Invalid message type
			  packer.pack( 1234 );
			  packer.Flush();

			  sbyte[] invalidMessage = rawData.Bytes;

			  // When
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(32, invalidMessage));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUnknownMarkerBytes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUnknownMarkerBytes()
		 {
			  // Given I send a message with an invalid type
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.messaging.RecordingByteChannel rawData = new org.neo4j.bolt.v1.messaging.RecordingByteChannel();
			  RecordingByteChannel rawData = new RecordingByteChannel();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.packstream.BufferedChannelOutput out = new org.neo4j.bolt.v1.packstream.BufferedChannelOutput(rawData);
			  BufferedChannelOutput @out = new BufferedChannelOutput( rawData );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.v1.packstream.PackStream.Packer packer = new org.neo4j.bolt.v1.packstream.PackStream.Packer(out);
			  PackStream.Packer packer = new PackStream.Packer( @out );

			  packer.PackStructHeader( 2, RunMessage.SIGNATURE );
			  @out.WriteByte( PackStream.RESERVED_C7 ); // Invalid marker byte
			  @out.Flush();

			  sbyte[] invalidMessage = rawData.Bytes;

			  // When
			  Connection.connect( Address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(32, invalidMessage));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, eventuallyDisconnects() );
		 }
	}

}