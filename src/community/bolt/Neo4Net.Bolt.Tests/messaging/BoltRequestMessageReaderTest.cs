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
namespace Neo4Net.Bolt.messaging
{
	using Test = org.junit.jupiter.api.Test;


	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using Neo4NetError = Neo4Net.Bolt.runtime.Neo4NetError;
	using SynchronousBoltConnection = Neo4Net.Bolt.runtime.SynchronousBoltConnection;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

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
			  Neo4NetPack_Unpacker unpacker = mock( typeof( Neo4NetPack_Unpacker ) );
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
			  Neo4NetPack_Unpacker unpacker = mock( typeof( Neo4NetPack_Unpacker ) );
			  BoltIOException error = new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, "Hello" );
			  when( unpacker.UnpackStructHeader() ).thenThrow(error);

			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  BoltConnection connection = new SynchronousBoltConnection( stateMachine );

			  BoltResponseHandler externalErrorResponseHandler = ResponseHandlerMock();
			  BoltRequestMessageReader reader = new TestBoltRequestMessageReader( connection, externalErrorResponseHandler, emptyList() );

			  reader.Read( unpacker );

			  verify( stateMachine ).handleExternalFailure( Neo4NetError.from( error ), externalErrorResponseHandler );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowForUnknownMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowForUnknownMessage()
		 {
			  Neo4NetPack_Unpacker unpacker = mock( typeof( Neo4NetPack_Unpacker ) );
			  when( unpacker.UnpackStructSignature() ).thenReturn('a');

			  RequestMessageDecoder decoder = new TestRequestMessageDecoder( 'b', ResponseHandlerMock(), mock(typeof(RequestMessage)) );
			  BoltRequestMessageReader reader = new TestBoltRequestMessageReader( ConnectionMock(), ResponseHandlerMock(), singletonList(decoder) );

			  BoltIOException e = assertThrows( typeof( BoltIOException ), () => reader.read(unpacker) );
			  assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, e.Status() );
			  assertFalse( e.CausesFailureMessage() );
			  assertEquals( "Message 0x61 is not a valid message signature.", e.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecodeKnownMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDecodeKnownMessage()
		 {
			  Neo4NetPack_Unpacker unpacker = mock( typeof( Neo4NetPack_Unpacker ) );
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
//ORIGINAL LINE: public RequestMessage decode(Neo4Net.bolt.messaging.Neo4NetPack_Unpacker unpacker) throws java.io.IOException
			  public override RequestMessage Decode( Neo4NetPack_Unpacker unpacker )
			  {
					return Message;
			  }
		 }
	}

}