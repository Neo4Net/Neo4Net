﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt.messaging
{
	using Test = org.junit.jupiter.api.Test;


	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using SynchronousBoltConnection = Org.Neo4j.Bolt.runtime.SynchronousBoltConnection;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class BoltRequestMessageReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateFatalError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateFatalError()
		 {
			  Neo4jPack_Unpacker unpacker = mock( typeof( Neo4jPack_Unpacker ) );
			  Exception error = new Exception();
			  when( unpacker.UnpackStructHeader() ).thenThrow(error);

			  BoltRequestMessageReader reader = new TestBoltRequestMessageReader( ConnectionMock(), ResponseHandlerMock(), emptyList() );

			  Exception e = assertThrows( typeof( Exception ), () => reader.read(unpacker) );
			  assertEquals( error, e );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleErrorThatCausesFailureMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleErrorThatCausesFailureMessage()
		 {
			  Neo4jPack_Unpacker unpacker = mock( typeof( Neo4jPack_Unpacker ) );
			  BoltIOException error = new BoltIOException( Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError, "Hello" );
			  when( unpacker.UnpackStructHeader() ).thenThrow(error);

			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  BoltConnection connection = new SynchronousBoltConnection( stateMachine );

			  BoltResponseHandler externalErrorResponseHandler = ResponseHandlerMock();
			  BoltRequestMessageReader reader = new TestBoltRequestMessageReader( connection, externalErrorResponseHandler, emptyList() );

			  reader.Read( unpacker );

			  verify( stateMachine ).handleExternalFailure( Neo4jError.from( error ), externalErrorResponseHandler );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowForUnknownMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowForUnknownMessage()
		 {
			  Neo4jPack_Unpacker unpacker = mock( typeof( Neo4jPack_Unpacker ) );
			  when( unpacker.UnpackStructSignature() ).thenReturn('a');

			  RequestMessageDecoder decoder = new TestRequestMessageDecoder( 'b', ResponseHandlerMock(), mock(typeof(RequestMessage)) );
			  BoltRequestMessageReader reader = new TestBoltRequestMessageReader( ConnectionMock(), ResponseHandlerMock(), singletonList(decoder) );

			  BoltIOException e = assertThrows( typeof( BoltIOException ), () => reader.read(unpacker) );
			  assertEquals( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, e.Status() );
			  assertFalse( e.CausesFailureMessage() );
			  assertEquals( "Message 0x61 is not a valid message signature.", e.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeKnownMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDecodeKnownMessage()
		 {
			  Neo4jPack_Unpacker unpacker = mock( typeof( Neo4jPack_Unpacker ) );
			  when( unpacker.UnpackStructSignature() ).thenReturn('a');

			  RequestMessage message = mock( typeof( RequestMessage ) );
			  BoltResponseHandler responseHandler = ResponseHandlerMock();
			  RequestMessageDecoder decoder = new TestRequestMessageDecoder( 'a', responseHandler, message );

			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  BoltConnection connection = new SynchronousBoltConnection( stateMachine );

			  BoltRequestMessageReader reader = new TestBoltRequestMessageReader( connection, ResponseHandlerMock(), singletonList(decoder) );

			  reader.Read( unpacker );

			  verify( stateMachine ).process( message, responseHandler );
		 }

		 private static BoltConnection ConnectionMock()
		 {
			  return mock( typeof( BoltConnection ) );
		 }

		 private static BoltResponseHandler ResponseHandlerMock()
		 {
			  return mock( typeof( BoltResponseHandler ) );
		 }

		 private class TestBoltRequestMessageReader : BoltRequestMessageReader
		 {
			  internal TestBoltRequestMessageReader( BoltConnection connection, BoltResponseHandler externalErrorResponseHandler, IList<RequestMessageDecoder> decoders ) : base( connection, externalErrorResponseHandler, decoders )
			  {
			  }
		 }

		 private class TestRequestMessageDecoder : RequestMessageDecoder
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int SignatureConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly BoltResponseHandler ResponseHandlerConflict;
			  internal readonly RequestMessage Message;

			  internal TestRequestMessageDecoder( int signature, BoltResponseHandler responseHandler, RequestMessage message )
			  {
					this.SignatureConflict = signature;
					this.ResponseHandlerConflict = responseHandler;
					this.Message = message;
			  }

			  public override int Signature()
			  {
					return SignatureConflict;
			  }

			  public override BoltResponseHandler ResponseHandler()
			  {
					return ResponseHandlerConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RequestMessage decode(org.neo4j.bolt.messaging.Neo4jPack_Unpacker unpacker) throws java.io.IOException
			  public override RequestMessage Decode( Neo4jPack_Unpacker unpacker )
			  {
					return Message;
			  }
		 }
	}

}