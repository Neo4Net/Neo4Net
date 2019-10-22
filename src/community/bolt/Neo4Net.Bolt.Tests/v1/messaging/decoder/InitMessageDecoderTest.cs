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
namespace Neo4Net.Bolt.v1.messaging.decoder
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4NetPack_Unpacker = Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using RequestMessageDecoder = Neo4Net.Bolt.messaging.RequestMessageDecoder;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using AuthTokenDecoderTest = Neo4Net.Bolt.security.auth.AuthTokenDecoderTest;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PackedInputArray = Neo4Net.Bolt.v1.packstream.PackedInputArray;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.util.MessageMatchers.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.AuthTokenUtil.assertAuthTokenMatches;

	internal class InitMessageDecoderTest : AuthTokenDecoderTest
	{
		private bool InstanceFieldsInitialized = false;

		public InitMessageDecoderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_decoder = new InitMessageDecoder( _responseHandler );
		}

		 private readonly BoltResponseHandler _responseHandler = mock( typeof( BoltResponseHandler ) );
		 private RequestMessageDecoder _decoder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnCorrectSignature()
		 internal virtual void ShouldReturnCorrectSignature()
		 {
			  assertEquals( InitMessage.SIGNATURE, _decoder.signature() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnConnectResponseHandler()
		 internal virtual void ShouldReturnConnectResponseHandler()
		 {
			  assertEquals( _responseHandler, _decoder.responseHandler() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeAckFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeAckFailure()
		 {
			  Neo4NetPackV1 Neo4NetPack = new Neo4NetPackV1();
			  InitMessage originalMessage = new InitMessage( "My Driver", map( "user", "Neo4Net", "password", "secret" ) );

			  PackedInputArray innput = new PackedInputArray( serialize( Neo4NetPack, originalMessage ) );
			  Neo4NetPack_Unpacker unpacker = Neo4NetPack.NewUnpacker( innput );

			  // these two steps are executed before decoding in order to select a correct decoder
			  unpacker.UnpackStructHeader();
			  unpacker.UnpackStructSignature();

			  RequestMessage deserializedMessage = _decoder.decode( unpacker );
			  assertEquals( originalMessage, deserializedMessage );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void testShouldDecodeAuthToken(java.util.Map<String,Object> authToken, boolean checkDecodingResult) throws Exception
		 protected internal override void TestShouldDecodeAuthToken( IDictionary<string, object> authToken, bool checkDecodingResult )
		 {
			  Neo4NetPackV1 Neo4NetPack = new Neo4NetPackV1();
			  InitMessage originalMessage = new InitMessage( "My Driver", authToken );

			  PackedInputArray innput = new PackedInputArray( serialize( Neo4NetPack, originalMessage ) );
			  Neo4NetPack_Unpacker unpacker = Neo4NetPack.NewUnpacker( innput );

			  // these two steps are executed before decoding in order to select a correct decoder
			  unpacker.UnpackStructHeader();
			  unpacker.UnpackStructSignature();

			  RequestMessage deserializedMessage = _decoder.decode( unpacker );

			  if ( checkDecodingResult )
			  {
					AssertInitMessageMatches( originalMessage, deserializedMessage );
			  }
		 }

		 private static void AssertInitMessageMatches( InitMessage expected, RequestMessage actual )
		 {
			  assertEquals( expected.UserAgent(), ((InitMessage) actual).userAgent() );
			  assertAuthTokenMatches( expected.AuthToken(), ((InitMessage) actual).authToken() );
		 }
	}

}