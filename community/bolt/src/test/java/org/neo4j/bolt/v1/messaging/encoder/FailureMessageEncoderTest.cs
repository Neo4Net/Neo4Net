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
namespace Org.Neo4j.Bolt.v1.messaging.encoder
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using FailureMessage = Org.Neo4j.Bolt.v1.messaging.response.FailureMessage;
	using FatalFailureMessage = Org.Neo4j.Bolt.v1.messaging.response.FatalFailureMessage;
	using QueryResult = Org.Neo4j.Cypher.result.QueryResult;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using Log = Org.Neo4j.Logging.Log;
	using AnyValue = Org.Neo4j.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class FailureMessageEncoderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEncodeFailureMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldEncodeFailureMessage()
		 {
			  // Given
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = mock( typeof( Org.Neo4j.Bolt.messaging.Neo4jPack_Packer ) );
			  Log log = mock( typeof( Log ) );
			  FailureMessageEncoder encoder = new FailureMessageEncoder( log );

			  // When
			  Org.Neo4j.Cypher.result.QueryResult_Record value = mock( typeof( Org.Neo4j.Cypher.result.QueryResult_Record ) );
			  when( value.Fields() ).thenReturn(new AnyValue[0]);
			  encoder.Encode( packer, new FailureMessage( Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError, "I am an error message" ) );

			  // Then
			  verify( log, never() ).debug(anyString(), any(typeof(FailureMessage)));

			  verify( packer ).packStructHeader( anyInt(), eq(FailureMessage.SIGNATURE) );
			  verify( packer ).packMapHeader( 2 );
			  verify( packer ).pack( "code" );
			  verify( packer ).pack( "message" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLogErrorIfIsFatalError() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldLogErrorIfIsFatalError()
		 {
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = mock( typeof( Org.Neo4j.Bolt.messaging.Neo4jPack_Packer ) );
			  Log log = mock( typeof( Log ) );
			  FailureMessageEncoder encoder = new FailureMessageEncoder( log );

			  // When
			  Org.Neo4j.Cypher.result.QueryResult_Record value = mock( typeof( Org.Neo4j.Cypher.result.QueryResult_Record ) );
			  when( value.Fields() ).thenReturn(new AnyValue[0]);
			  FatalFailureMessage message = new FatalFailureMessage( Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError, "I am an error message" );
			  encoder.Encode( packer, message );

			  // Then
			  verify( log ).debug( "Encoding a fatal failure message to send. Message: %s", message );
		 }
	}

}