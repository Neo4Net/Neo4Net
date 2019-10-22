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
namespace Neo4Net.Bolt.v3.messaging
{
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;

	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using BoltRequestMessageReader = Neo4Net.Bolt.messaging.BoltRequestMessageReader;
	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using PackedInputArray = Neo4Net.Bolt.v1.packstream.PackedInputArray;
	using BeginMessage = Neo4Net.Bolt.v3.messaging.request.BeginMessage;
	using HelloMessage = Neo4Net.Bolt.v3.messaging.request.HelloMessage;
	using RunMessage = Neo4Net.Bolt.v3.messaging.request.RunMessage;

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
//	import static org.Neo4Net.bolt.v3.messaging.BoltProtocolV3ComponentFactory.encode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v3.messaging.BoltProtocolV3ComponentFactory.newNeo4NetPack;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v3.messaging.BoltProtocolV3ComponentFactory.requestMessageReader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v3.messaging.request.CommitMessage.COMMIT_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v3.messaging.request.RollbackMessage.ROLLBACK_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

	internal class BoltRequestMessageReaderV3Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("boltV3Messages") void shouldDecodeV3Messages(org.Neo4Net.bolt.messaging.RequestMessage message) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeV3Messages( RequestMessage message )
		 {
			  TestMessageDecoding( message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("boltV3UnsupportedMessages") void shouldNotDecodeUnsupportedMessages(org.Neo4Net.bolt.messaging.RequestMessage message) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotDecodeUnsupportedMessages( RequestMessage message )
		 {
			  assertThrows( typeof( Exception ), () => testMessageDecoding(message) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testMessageDecoding(org.Neo4Net.bolt.messaging.RequestMessage message) throws Exception
		 private static void TestMessageDecoding( RequestMessage message )
		 {
			  Neo4NetPack Neo4NetPack = newNeo4NetPack();

			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  BoltRequestMessageReader reader = requestMessageReader( stateMachine );

			  PackedInputArray input = new PackedInputArray( encode( Neo4NetPack, message ) );
			  Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker unpacker = Neo4NetPack.NewUnpacker( input );

			  reader.Read( unpacker );

			  verify( stateMachine ).process( eq( message ), any() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.stream.Stream<org.Neo4Net.bolt.messaging.RequestMessage> boltV3Messages() throws org.Neo4Net.bolt.messaging.BoltIOException
		 private static Stream<RequestMessage> BoltV3Messages()
		 {
			  return Stream.of( new HelloMessage( map( "user_agent", "My driver", "one", 1L, "two", 2L ) ), new RunMessage( "RETURN 1", EMPTY_MAP, EMPTY_MAP ), DiscardAllMessage.INSTANCE, PullAllMessage.INSTANCE, new BeginMessage(), COMMIT_MESSAGE, ROLLBACK_MESSAGE, ResetMessage.INSTANCE );
		 }

		 private static Stream<RequestMessage> BoltV3UnsupportedMessages()
		 {
			  return Stream.of(new InitMessage("My driver", map("one", 1L, "two", 2L)), AckFailureMessage.INSTANCE, new Neo4Net.Bolt.v1.messaging.request.RunMessage("RETURN 1")
			 );
		 }

	}

}