using System;

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
namespace Org.Neo4j.Bolt.v3.messaging
{
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;

	using BoltIOException = Org.Neo4j.Bolt.messaging.BoltIOException;
	using BoltRequestMessageReader = Org.Neo4j.Bolt.messaging.BoltRequestMessageReader;
	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using AckFailureMessage = Org.Neo4j.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using PackedInputArray = Org.Neo4j.Bolt.v1.packstream.PackedInputArray;
	using BeginMessage = Org.Neo4j.Bolt.v3.messaging.request.BeginMessage;
	using HelloMessage = Org.Neo4j.Bolt.v3.messaging.request.HelloMessage;
	using RunMessage = Org.Neo4j.Bolt.v3.messaging.request.RunMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.BoltProtocolV3ComponentFactory.encode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.BoltProtocolV3ComponentFactory.newNeo4jPack;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.BoltProtocolV3ComponentFactory.requestMessageReader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.CommitMessage.COMMIT_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.RollbackMessage.ROLLBACK_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	internal class BoltRequestMessageReaderV3Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("boltV3Messages") void shouldDecodeV3Messages(org.neo4j.bolt.messaging.RequestMessage message) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeV3Messages( RequestMessage message )
		 {
			  TestMessageDecoding( message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("boltV3UnsupportedMessages") void shouldNotDecodeUnsupportedMessages(org.neo4j.bolt.messaging.RequestMessage message) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotDecodeUnsupportedMessages( RequestMessage message )
		 {
			  assertThrows( typeof( Exception ), () => testMessageDecoding(message) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testMessageDecoding(org.neo4j.bolt.messaging.RequestMessage message) throws Exception
		 private static void TestMessageDecoding( RequestMessage message )
		 {
			  Neo4jPack neo4jPack = newNeo4jPack();

			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  BoltRequestMessageReader reader = requestMessageReader( stateMachine );

			  PackedInputArray input = new PackedInputArray( encode( neo4jPack, message ) );
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker = neo4jPack.NewUnpacker( input );

			  reader.Read( unpacker );

			  verify( stateMachine ).process( eq( message ), any() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.stream.Stream<org.neo4j.bolt.messaging.RequestMessage> boltV3Messages() throws org.neo4j.bolt.messaging.BoltIOException
		 private static Stream<RequestMessage> BoltV3Messages()
		 {
			  return Stream.of( new HelloMessage( map( "user_agent", "My driver", "one", 1L, "two", 2L ) ), new RunMessage( "RETURN 1", EMPTY_MAP, EMPTY_MAP ), DiscardAllMessage.INSTANCE, PullAllMessage.INSTANCE, new BeginMessage(), COMMIT_MESSAGE, ROLLBACK_MESSAGE, ResetMessage.INSTANCE );
		 }

		 private static Stream<RequestMessage> BoltV3UnsupportedMessages()
		 {
			  return Stream.of(new InitMessage("My driver", map("one", 1L, "two", 2L)), AckFailureMessage.INSTANCE, new Org.Neo4j.Bolt.v1.messaging.request.RunMessage("RETURN 1")
			 );
		 }

	}

}