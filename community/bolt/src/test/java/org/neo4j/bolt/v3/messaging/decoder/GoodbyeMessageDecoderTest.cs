﻿/*
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
namespace Org.Neo4j.Bolt.v3.messaging.decoder
{
	using Test = org.junit.jupiter.api.Test;

	using RequestMessageDecoder = Org.Neo4j.Bolt.messaging.RequestMessageDecoder;
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using GoodbyeMessage = Org.Neo4j.Bolt.v3.messaging.request.GoodbyeMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.decoder.HelloMessageDecoderTest.assertOriginalMessageEqualsToDecoded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.GoodbyeMessage.GOODBYE_MESSAGE;

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