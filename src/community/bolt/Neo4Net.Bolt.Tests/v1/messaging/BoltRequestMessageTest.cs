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
namespace Neo4Net.Bolt.v1.messaging
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using BoltRequestMessageReader = Neo4Net.Bolt.messaging.BoltRequestMessageReader;
	using BoltResponseMessageWriter = Neo4Net.Bolt.messaging.BoltResponseMessageWriter;
	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using SynchronousBoltConnection = Neo4Net.Bolt.runtime.SynchronousBoltConnection;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using BufferedChannelOutput = Neo4Net.Bolt.v1.packstream.BufferedChannelOutput;
	using PackedInputArray = Neo4Net.Bolt.v1.packstream.PackedInputArray;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using AnyValue = Neo4Net.Values.AnyValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.lineSeparator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.spi.Records.record;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.nodeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.relationshipValue;

	public class BoltRequestMessageTest
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
			  AssertSerializes( new InitMessage( "MyClient/1.0", map( "scheme", "basic" ) ) );
			  AssertSerializes( AckFailureMessage.INSTANCE );
			  AssertSerializes( ResetMessage.INSTANCE );
			  AssertSerializes( new RunMessage( "CREATE (n) RETURN åäö" ) );
			  AssertSerializes( DiscardAllMessage.INSTANCE );
			  AssertSerializes( PullAllMessage.INSTANCE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleParameterizedStatements() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleParameterizedStatements()
		 {
			  // Given
			  MapValue parameters = ValueUtils.asMapValue( map( "n", 12L ) );

			  // When
			  RunMessage msg = SerializeAndDeserialize( new RunMessage( "asd", parameters ) );

			  // Then
			  MapValue @params = msg.Params();
			  assertThat( @params, equalTo( parameters ) );
		 }

		 //"B1 71 91 B3 4E 0C 92 |84 55 73 65 72 | 86 42 61 6E\n61 6E 61 A284 6E 61 6D 65 83 42 6F 62 83 61 67\n65 0E"
		 //"B1 71 91 B3 4E 0C 92 |86 42 61 6E 61 6E 61| 84 55\n73 65 72 A2 84 6E 61 6D 65 83 42 6F 62 83 61 67\n65 0E
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeNode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeNode()
		 {
			  NodeValue nodeValue = nodeValue( 12L, stringArray( "User", "Banana" ), map( new string[]{ "name", "age" }, new AnyValue[]{ stringValue( "Bob" ), intValue( 14 ) } ) );
			  assertThat( Serialized( nodeValue ), equalTo( "B1 71 91 B3 4E 0C 92 84 55 73 65 72 86 42 61 6E" + lineSeparator() + "61 6E 61 A2 84 6E 61 6D 65 83 42 6F 62 83 61 67" + lineSeparator() + "65 0E" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeRelationship() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeRelationship()
		 {
			  RelationshipValue rel = relationshipValue( 12L, nodeValue( 1L, stringArray(), VirtualValues.EMPTY_MAP ), nodeValue(2L, stringArray(), VirtualValues.EMPTY_MAP), stringValue("KNOWS"), map(new string[]{ "name", "age" }, new AnyValue[]{ stringValue("Bob"), intValue(14) }) );
			  assertThat( Serialized( rel ), equalTo( "B1 71 91 B5 52 0C 01 02 85 4B 4E 4F 57 53 A2 84" + lineSeparator() + "6E 61 6D 65 83 42 6F 62 83 61 67 65 0E" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String serialized(org.neo4j.values.AnyValue object) throws java.io.IOException
		 private string Serialized( AnyValue @object )
		 {
			  RecordMessage message = new RecordMessage( record( @object ) );
			  return HexPrinter.hex( serialize( _neo4jPack, message ), 4, " " );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSerializes(org.neo4j.bolt.messaging.RequestMessage msg) throws Exception
		 private void AssertSerializes( RequestMessage msg )
		 {
			  assertThat( SerializeAndDeserialize( msg ), equalTo( msg ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T extends org.neo4j.bolt.messaging.RequestMessage> T serializeAndDeserialize(T msg) throws Exception
		 private T SerializeAndDeserialize<T>( T msg ) where T : Neo4Net.Bolt.messaging.RequestMessage
		 {
			  RecordingByteChannel channel = new RecordingByteChannel();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = _neo4jPack.newPacker( new BufferedChannelOutput( channel ) );
			  BoltRequestMessageWriter writer = new BoltRequestMessageWriter( packer );

			  writer.Write( msg ).flush();
			  channel.Eof();

			  return Unpack( channel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T extends org.neo4j.bolt.messaging.RequestMessage> T unpack(RecordingByteChannel channel) throws Exception
		 private T Unpack<T>( RecordingByteChannel channel ) where T : Neo4Net.Bolt.messaging.RequestMessage
		 {
			  IList<RequestMessage> messages = new List<RequestMessage>();
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  doAnswer( new AnswerAnonymousInnerClass( this, messages ) )BoltRequestMessageReader reader = new BoltRequestMessageReaderV1( new SynchronousBoltConnection( stateMachine ), mock( typeof( BoltResponseMessageWriter ) ), NullLogService.Instance );
			 .when( stateMachine ).process( any(), any() );

			  sbyte[] bytes = channel.Bytes;
			  string serialized = HexPrinter.hex( bytes );
			  Neo4Net.Bolt.messaging.Neo4jPack_Unpacker unpacker = _neo4jPack.newUnpacker( new PackedInputArray( bytes ) );
			  try
			  {
					reader.read( unpacker );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( "Failed to unpack message, wire data was:\n" + serialized + "[" + bytes.Length + "b]", e );
			  }

			  return ( T ) messages[0];
		 }

		 private class AnswerAnonymousInnerClass : Answer<Void>
		 {
			 private readonly BoltRequestMessageTest _outerInstance;

			 private IList<RequestMessage> _messages;

			 public AnswerAnonymousInnerClass( BoltRequestMessageTest outerInstance, IList<RequestMessage> messages )
			 {
				 this.outerInstance = outerInstance;
				 this._messages = messages;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void answer(org.mockito.invocation.InvocationOnMock invocationOnMock) throws Throwable
			 public override Void answer( InvocationOnMock invocationOnMock )
			 {
				  RequestMessage msg = invocationOnMock.getArgument( 0 );
				  _messages.Add( msg );
				  return null;
			 }
		 }
	}

}