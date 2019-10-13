using System;

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
namespace Neo4Net.Bolt.v1.messaging
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Neo4Net.Bolt.v1.messaging.response.SuccessMessage;
	using BufferedChannelInput = Neo4Net.Bolt.v1.packstream.BufferedChannelInput;
	using BufferedChannelOutput = Neo4Net.Bolt.v1.packstream.BufferedChannelOutput;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using AnyValue = Neo4Net.Values.AnyValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.lineSeparator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.PATH_WITH_LENGTH_ONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.PATH_WITH_LENGTH_TWO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.PATH_WITH_LENGTH_ZERO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.PATH_WITH_LOOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.PATH_WITH_NODES_VISITED_MULTIPLE_TIMES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.PATH_WITH_RELATIONSHIP_TRAVERSED_AGAINST_ITS_DIRECTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.PATH_WITH_RELATIONSHIP_TRAVERSED_MULTIPLE_TIMES_IN_SAME_DIRECTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.response.IgnoredMessage.IGNORED_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.spi.Records.record;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.nodeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.relationshipValue;

	public class BoltResponseMessageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private readonly Neo4jPack _neo4jPack = new Neo4jPackV1();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleCommonMessages() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleCommonMessages()
		 {
			  AssertSerializes( new RecordMessage( record( longValue( 1L ), stringValue( "b" ), longValue( 2L ) ) ) );
			  AssertSerializes( new SuccessMessage( VirtualValues.EMPTY_MAP ) );
			  AssertSerializes( new FailureMessage( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, "Err" ) );
			  AssertSerializes( IGNORED_MESSAGE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeBasicTypes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeBasicTypes()
		 {
			  AssertSerializesNeoValue( null );
			  AssertSerializesNeoValue( true );
			  AssertSerializesNeoValue( false );

			  AssertSerializesNeoValue( long.MaxValue );
			  AssertSerializesNeoValue( 1337L );
			  AssertSerializesNeoValue( long.MinValue );

			  AssertSerializesNeoValue( double.Epsilon );
			  AssertSerializesNeoValue( 13.37d );
			  AssertSerializesNeoValue( double.MaxValue );

			  AssertSerializesNeoValue( "" );
			  AssertSerializesNeoValue( "A basic piece of text" );
			  AssertSerializesNeoValue( StringHelper.NewString( new sbyte[16000], StandardCharsets.UTF_8 ) );

			  AssertSerializesNeoValue( emptyList() );
			  AssertSerializesNeoValue( asList( null, null ) );
			  AssertSerializesNeoValue( asList( true, false ) );
			  AssertSerializesNeoValue( asList( "one", "", "three" ) );
			  AssertSerializesNeoValue( asList( 12.4d, 0.0d ) );

			  AssertSerializesNeoValue( map( "k", null ) );
			  AssertSerializesNeoValue( map( "k", true ) );
			  AssertSerializesNeoValue( map( "k", false ) );
			  AssertSerializesNeoValue( map( "k", 1337L ) );
			  AssertSerializesNeoValue( map( "k", 133.7d ) );
			  AssertSerializesNeoValue( map( "k", "Hello" ) );
			  AssertSerializesNeoValue( map( "k", asList( "one", "", "three" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeNode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeNode()
		 {
			  NodeValue nodeValue = nodeValue( 12L, stringArray( "User", "Banana" ), VirtualValues.map( new string[]{ "name", "age" }, new AnyValue[]{ stringValue( "Bob" ), intValue( 14 ) } ) );
			  assertThat( Serialized( nodeValue ), equalTo( "B1 71 91 B3 4E 0C 92 84 55 73 65 72 86 42 61 6E" + lineSeparator() + "61 6E 61 A2 84 6E 61 6D 65 83 42 6F 62 83 61 67" + lineSeparator() + "65 0E" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeRelationship() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeRelationship()
		 {
			  RelationshipValue rel = relationshipValue( 12L, nodeValue( 1L, stringArray(), VirtualValues.EMPTY_MAP ), nodeValue(2L, stringArray(), VirtualValues.EMPTY_MAP), stringValue("KNOWS"), VirtualValues.map(new string[]{ "name", "age" }, new AnyValue[]{ stringValue("Bob"), intValue(14) }) );
			  assertThat( Serialized( rel ), equalTo( "B1 71 91 B5 52 0C 01 02 85 4B 4E 4F 57 53 A2 84" + lineSeparator() + "6E 61 6D 65 83 42 6F 62 83 61 67 65 0E" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializePaths() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializePaths()
		 {
			  assertThat( Serialized( PATH_WITH_LENGTH_ZERO ), equalTo( "B1 71 91 B3 50 91 B3 4E C9 03 E9 92 86 50 65 72" + lineSeparator() + "73 6F 6E 88 45 6D 70 6C 6F 79 65 65 A2 84 6E 61" + lineSeparator() + "6D 65 85 41 6C 69 63 65 83 61 67 65 21 90 90" ) );
			  assertThat( Serialized( PATH_WITH_LENGTH_ONE ), equalTo( "B1 71 91 B3 50 92 B3 4E C9 03 E9 92 86 50 65 72" + lineSeparator() + "73 6F 6E 88 45 6D 70 6C 6F 79 65 65 A2 84 6E 61" + lineSeparator() + "6D 65 85 41 6C 69 63 65 83 61 67 65 21 B3 4E C9" + lineSeparator() + "03 EA 92 86 50 65 72 73 6F 6E 88 45 6D 70 6C 6F" + lineSeparator() + "79 65 65 A2 84 6E 61 6D 65 83 42 6F 62 83 61 67" + lineSeparator() + "65 2C 91 B3 72 0C 85 4B 4E 4F 57 53 A1 85 73 69" + lineSeparator() + "6E 63 65 C9 07 CF 92 01 01" ) );
			  assertThat( Serialized( PATH_WITH_LENGTH_TWO ), equalTo( "B1 71 91 B3 50 93 B3 4E C9 03 E9 92 86 50 65 72" + lineSeparator() + "73 6F 6E 88 45 6D 70 6C 6F 79 65 65 A2 84 6E 61" + lineSeparator() + "6D 65 85 41 6C 69 63 65 83 61 67 65 21 B3 4E C9" + lineSeparator() + "03 EB 91 86 50 65 72 73 6F 6E A1 84 6E 61 6D 65" + lineSeparator() + "85 43 61 72 6F 6C B3 4E C9 03 EC 90 A1 84 6E 61" + lineSeparator() + "6D 65 84 44 61 76 65 92 B3 72 0D 85 4C 49 4B 45" + lineSeparator() + "53 A0 B3 72 22 8A 4D 41 52 52 49 45 44 5F 54 4F" + lineSeparator() + "A0 94 01 01 02 02" ) );
			  assertThat( Serialized( PATH_WITH_RELATIONSHIP_TRAVERSED_AGAINST_ITS_DIRECTION ), equalTo( "B1 71 91 B3 50 94 B3 4E C9 03 E9 92 86 50 65 72" + lineSeparator() + "73 6F 6E 88 45 6D 70 6C 6F 79 65 65 A2 84 6E 61" + lineSeparator() + "6D 65 85 41 6C 69 63 65 83 61 67 65 21 B3 4E C9" + lineSeparator() + "03 EA 92 86 50 65 72 73 6F 6E 88 45 6D 70 6C 6F" + lineSeparator() + "79 65 65 A2 84 6E 61 6D 65 83 42 6F 62 83 61 67" + lineSeparator() + "65 2C B3 4E C9 03 EB 91 86 50 65 72 73 6F 6E A1" + lineSeparator() + "84 6E 61 6D 65 85 43 61 72 6F 6C B3 4E C9 03 EC" + lineSeparator() + "90 A1 84 6E 61 6D 65 84 44 61 76 65 93 B3 72 0C" + lineSeparator() + "85 4B 4E 4F 57 53 A1 85 73 69 6E 63 65 C9 07 CF" + lineSeparator() + "B3 72 20 88 44 49 53 4C 49 4B 45 53 A0 B3 72 22" + lineSeparator() + "8A 4D 41 52 52 49 45 44 5F 54 4F A0 96 01 01 FE" + lineSeparator() + "02 03 03" ) );
			  assertThat( Serialized( PATH_WITH_NODES_VISITED_MULTIPLE_TIMES ), equalTo( "B1 71 91 B3 50 93 B3 4E C9 03 E9 92 86 50 65 72" + lineSeparator() + "73 6F 6E 88 45 6D 70 6C 6F 79 65 65 A2 84 6E 61" + lineSeparator() + "6D 65 85 41 6C 69 63 65 83 61 67 65 21 B3 4E C9" + lineSeparator() + "03 EA 92 86 50 65 72 73 6F 6E 88 45 6D 70 6C 6F" + lineSeparator() + "79 65 65 A2 84 6E 61 6D 65 83 42 6F 62 83 61 67" + lineSeparator() + "65 2C B3 4E C9 03 EB 91 86 50 65 72 73 6F 6E A1" + lineSeparator() + "84 6E 61 6D 65 85 43 61 72 6F 6C 93 B3 72 0C 85" + lineSeparator() + "4B 4E 4F 57 53 A1 85 73 69 6E 63 65 C9 07 CF B3" + lineSeparator() + "72 0D 85 4C 49 4B 45 53 A0 B3 72 20 88 44 49 53" + lineSeparator() + "4C 49 4B 45 53 A0 9A 01 01 FF 00 02 02 03 01 FD" + lineSeparator() + "02" ) );
			  assertThat( Serialized( PATH_WITH_RELATIONSHIP_TRAVERSED_MULTIPLE_TIMES_IN_SAME_DIRECTION ), equalTo( "B1 71 91 B3 50 94 B3 4E C9 03 E9 92 86 50 65 72" + lineSeparator() + "73 6F 6E 88 45 6D 70 6C 6F 79 65 65 A2 84 6E 61" + lineSeparator() + "6D 65 85 41 6C 69 63 65 83 61 67 65 21 B3 4E C9" + lineSeparator() + "03 EB 91 86 50 65 72 73 6F 6E A1 84 6E 61 6D 65" + lineSeparator() + "85 43 61 72 6F 6C B3 4E C9 03 EA 92 86 50 65 72" + lineSeparator() + "73 6F 6E 88 45 6D 70 6C 6F 79 65 65 A2 84 6E 61" + lineSeparator() + "6D 65 83 42 6F 62 83 61 67 65 2C B3 4E C9 03 EC" + lineSeparator() + "90 A1 84 6E 61 6D 65 84 44 61 76 65 94 B3 72 0D" + lineSeparator() + "85 4C 49 4B 45 53 A0 B3 72 20 88 44 49 53 4C 49" + lineSeparator() + "4B 45 53 A0 B3 72 0C 85 4B 4E 4F 57 53 A1 85 73" + lineSeparator() + "69 6E 63 65 C9 07 CF B3 72 22 8A 4D 41 52 52 49" + lineSeparator() + "45 44 5F 54 4F A0 9A 01 01 02 02 FD 00 01 01 04" + lineSeparator() + "03" ) );
			  assertThat( Serialized( PATH_WITH_LOOP ), equalTo( "B1 71 91 B3 50 92 B3 4E C9 03 EB 91 86 50 65 72" + lineSeparator() + "73 6F 6E A1 84 6E 61 6D 65 85 43 61 72 6F 6C B3" + lineSeparator() + "4E C9 03 EC 90 A1 84 6E 61 6D 65 84 44 61 76 65" + lineSeparator() + "92 B3 72 22 8A 4D 41 52 52 49 45 44 5F 54 4F A0" + lineSeparator() + "B3 72 2C 89 57 4F 52 4B 53 5F 46 4F 52 A0 94 01" + lineSeparator() + "01 02 01" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String serialized(org.neo4j.values.AnyValue object) throws java.io.IOException
		 private string Serialized( AnyValue @object )
		 {
			  RecordMessage message = new RecordMessage( record( @object ) );
			  return HexPrinter.hex( serialize( _neo4jPack, message ), 4, " " );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSerializes(org.neo4j.bolt.messaging.ResponseMessage msg) throws java.io.IOException
		 private void AssertSerializes( ResponseMessage msg )
		 {
			  assertThat( SerializeAndDeserialize( msg ), equalTo( msg ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T extends org.neo4j.bolt.messaging.ResponseMessage> T serializeAndDeserialize(T msg) throws java.io.IOException
		 private T SerializeAndDeserialize<T>( T msg ) where T : Neo4Net.Bolt.messaging.ResponseMessage
		 {
			  RecordingByteChannel channel = new RecordingByteChannel();
			  BoltResponseMessageReader reader = new BoltResponseMessageReader( _neo4jPack.newUnpacker( ( new BufferedChannelInput( 16 ) ).reset( channel ) ) );
			  BufferedChannelOutput output = new BufferedChannelOutput( channel );
			  BoltResponseMessageWriterV1 writer = new BoltResponseMessageWriterV1( _neo4jPack.newPacker, output, NullLogService.Instance );

			  writer.Write( msg );
			  writer.Flush();

			  channel.Eof();
			  return Unpack( reader, channel );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T extends org.neo4j.bolt.messaging.ResponseMessage> T unpack(BoltResponseMessageReader reader, RecordingByteChannel channel)
		 private T Unpack<T>( BoltResponseMessageReader reader, RecordingByteChannel channel ) where T : Neo4Net.Bolt.messaging.ResponseMessage
		 {
			  // Unpack
			  string serialized = HexPrinter.hex( channel.Bytes );
			  BoltResponseMessageRecorder messages = new BoltResponseMessageRecorder();
			  try
			  {
					reader.Read( messages );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( "Failed to unpack message, wire data was:\n" + serialized + "[" + channel.Bytes.Length + "b]", e );
			  }

			  return ( T ) messages.AsList()[0];
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSerializesNeoValue(Object val) throws java.io.IOException
		 private void AssertSerializesNeoValue( object val )
		 {
			  AssertSerializes( new RecordMessage( record( ValueUtils.of( val ) ) ) );
		 }

	}

}