﻿/*
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

	using Neo4jPack_Unpacker = Neo4Net.Bolt.messaging.Neo4jPack_Unpacker;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using RequestMessageDecoder = Neo4Net.Bolt.messaging.RequestMessageDecoder;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using PackedInputArray = Neo4Net.Bolt.v1.packstream.PackedInputArray;
	using AnyValue = Neo4Net.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.map;

	internal class RunMessageDecoderTest
	{
		private bool InstanceFieldsInitialized = false;

		public RunMessageDecoderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_decoder = new RunMessageDecoder( _responseHandler );
		}

		 private readonly BoltResponseHandler _responseHandler = mock( typeof( BoltResponseHandler ) );
		 private RequestMessageDecoder _decoder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnCorrectSignature()
		 internal virtual void ShouldReturnCorrectSignature()
		 {
			  assertEquals( RunMessage.SIGNATURE, _decoder.signature() );
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
			  Neo4jPackV1 neo4jPack = new Neo4jPackV1();
			  RunMessage originalMessage = new RunMessage( "UNWIND range(1, 10) AS x RETURN x, $y", map( new string[]{ "y" }, new AnyValue[]{ longValue( 42 ) } ) );

			  PackedInputArray innput = new PackedInputArray( serialize( neo4jPack, originalMessage ) );
			  Neo4jPack_Unpacker unpacker = neo4jPack.NewUnpacker( innput );

			  // these two steps are executed before decoding in order to select a correct decoder
			  unpacker.UnpackStructHeader();
			  unpacker.UnpackStructSignature();

			  RequestMessage deserializedMessage = _decoder.decode( unpacker );
			  assertEquals( originalMessage, deserializedMessage );
		 }
	}

}