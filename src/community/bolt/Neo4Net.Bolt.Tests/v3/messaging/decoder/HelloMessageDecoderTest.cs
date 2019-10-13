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
namespace Neo4Net.Bolt.v3.messaging.decoder
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using RequestMessageDecoder = Neo4Net.Bolt.messaging.RequestMessageDecoder;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using AuthTokenDecoderTest = Neo4Net.Bolt.security.auth.AuthTokenDecoderTest;
	using PackedInputArray = Neo4Net.Bolt.v1.packstream.PackedInputArray;
	using HelloMessage = Neo4Net.Bolt.v3.messaging.request.HelloMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.BoltProtocolV3ComponentFactory.encode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.BoltProtocolV3ComponentFactory.newNeo4jPack;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.AuthTokenUtil.assertAuthTokenMatches;

	internal class HelloMessageDecoderTest : AuthTokenDecoderTest
	{
		private bool InstanceFieldsInitialized = false;

		public HelloMessageDecoderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_decoder = new HelloMessageDecoder( _responseHandler );
		}

		 private readonly BoltResponseHandler _responseHandler = mock( typeof( BoltResponseHandler ) );
		 private RequestMessageDecoder _decoder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnCorrectSignature()
		 internal virtual void ShouldReturnCorrectSignature()
		 {
			  assertEquals( HelloMessage.SIGNATURE, _decoder.signature() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnConnectResponseHandler()
		 internal virtual void ShouldReturnConnectResponseHandler()
		 {
			  assertEquals( _responseHandler, _decoder.responseHandler() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeHelloMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeHelloMessage()
		 {
			  HelloMessage originalMessage = new HelloMessage( map( "user_agent", "My Driver", "user", "neo4j", "password", "secret" ) );
			  AssertOriginalMessageEqualsToDecoded( originalMessage, _decoder );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void assertOriginalMessageEqualsToDecoded(org.neo4j.bolt.messaging.RequestMessage originalMessage, org.neo4j.bolt.messaging.RequestMessageDecoder decoder) throws Exception
		 internal static void AssertOriginalMessageEqualsToDecoded( RequestMessage originalMessage, RequestMessageDecoder decoder )
		 {
			  Neo4jPack neo4jPack = newNeo4jPack();

			  PackedInputArray input = new PackedInputArray( encode( neo4jPack, originalMessage ) );
			  Neo4Net.Bolt.messaging.Neo4jPack_Unpacker unpacker = neo4jPack.NewUnpacker( input );

			  // these two steps are executed before decoding in order to select a correct decoder
			  unpacker.UnpackStructHeader();
			  unpacker.UnpackStructSignature();

			  RequestMessage deserializedMessage = decoder.Decode( unpacker );
			  assertEquals( originalMessage, deserializedMessage );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void testShouldDecodeAuthToken(java.util.Map<String,Object> authToken, boolean checkDecodingResult) throws Exception
		 protected internal override void TestShouldDecodeAuthToken( IDictionary<string, object> authToken, bool checkDecodingResult )
		 {
			  Neo4jPack neo4jPack = newNeo4jPack();
			  authToken["user_agent"] = "My Driver";
			  HelloMessage originalMessage = new HelloMessage( authToken );

			  PackedInputArray input = new PackedInputArray( encode( neo4jPack, originalMessage ) );
			  Neo4Net.Bolt.messaging.Neo4jPack_Unpacker unpacker = neo4jPack.NewUnpacker( input );

			  // these two steps are executed before decoding in order to select a correct decoder
			  unpacker.UnpackStructHeader();
			  unpacker.UnpackStructSignature();

			  RequestMessage deserializedMessage = _decoder.decode( unpacker );

			  if ( checkDecodingResult )
			  {
					AssertHelloMessageMatches( originalMessage, deserializedMessage );
			  }
		 }

		 private static void AssertHelloMessageMatches( HelloMessage expected, RequestMessage actual )
		 {
			  assertAuthTokenMatches( expected.Meta(), ((HelloMessage) actual).meta() );
		 }
	}

}