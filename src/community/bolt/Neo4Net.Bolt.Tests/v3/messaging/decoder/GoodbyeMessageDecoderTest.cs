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

	using RequestMessageDecoder = Neo4Net.Bolt.messaging.RequestMessageDecoder;
	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using GoodbyeMessage = Neo4Net.Bolt.v3.messaging.request.GoodbyeMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v3.messaging.decoder.HelloMessageDecoderTest.assertOriginalMessageEqualsToDecoded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v3.messaging.request.GoodbyeMessage.GOODBYE_MESSAGE;

	internal class GoodbyeMessageDecoderTest
	{
		private bool InstanceFieldsInitialized = false;

		public GoodbyeMessageDecoderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_decoder = new GoodbyeMessageDecoder( _connection, _responseHandler );
		}

		 private readonly BoltResponseHandler _responseHandler = mock( typeof( BoltResponseHandler ) );
		 private readonly BoltConnection _connection = mock( typeof( BoltConnection ) );
		 private RequestMessageDecoder _decoder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnCorrectSignature()
		 internal virtual void ShouldReturnCorrectSignature()
		 {
			  assertEquals( GoodbyeMessage.SIGNATURE, _decoder.signature() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnConnectResponseHandler()
		 internal virtual void ShouldReturnConnectResponseHandler()
		 {
			  assertEquals( _responseHandler, _decoder.responseHandler() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeGoodbyeMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDecodeGoodbyeMessage()
		 {
			  assertOriginalMessageEqualsToDecoded( GOODBYE_MESSAGE, _decoder );
			  verify( _connection ).stop();
		 }
	}

}