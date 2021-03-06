﻿using System;

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
namespace Org.Neo4j.Bolt.v1.messaging
{
	using Test = org.junit.Test;

	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using BoltResponseMessageWriter = Org.Neo4j.Bolt.messaging.BoltResponseMessageWriter;
	using FailureMessage = Org.Neo4j.Bolt.v1.messaging.response.FailureMessage;
	using SuccessMessage = Org.Neo4j.Bolt.v1.messaging.response.SuccessMessage;
	using PackOutputClosedException = Org.Neo4j.Bolt.v1.packstream.PackOutputClosedException;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.both;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.matchers.CommonMatchers.hasSuppressed;

	public class MessageProcessingHandlerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallHaltOnUnexpectedFailures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallHaltOnUnexpectedFailures()
		 {
			  // Given
			  BoltResponseMessageWriter msgWriter = NewResponseHandlerMock();
			  doThrow( new Exception( "Something went horribly wrong" ) ).when( msgWriter ).write( any( typeof( SuccessMessage ) ) );

			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  MessageProcessingHandler handler = new MessageProcessingHandler( msgWriter, connection, mock( typeof( Log ) ) );

			  // When
			  handler.OnFinish();

			  // Then
			  verify( connection ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogOriginalErrorWhenOutputIsClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogOriginalErrorWhenOutputIsClosed()
		 {
			  TestLoggingOfOriginalErrorWhenOutputIsClosed( Neo4jError.from( new Exception( "Non-fatal error" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogOriginalFatalErrorWhenOutputIsClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogOriginalFatalErrorWhenOutputIsClosed()
		 {
			  TestLoggingOfOriginalErrorWhenOutputIsClosed( Neo4jError.fatalFrom( new Exception( "Fatal error" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogWriteErrorAndOriginalErrorWhenUnknownFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogWriteErrorAndOriginalErrorWhenUnknownFailure()
		 {
			  TestLoggingOfWriteErrorAndOriginalErrorWhenUnknownFailure( Neo4jError.from( new Exception( "Non-fatal error" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogWriteErrorAndOriginalFatalErrorWhenUnknownFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogWriteErrorAndOriginalFatalErrorWhenUnknownFailure()
		 {
			  TestLoggingOfWriteErrorAndOriginalErrorWhenUnknownFailure( Neo4jError.fatalFrom( new Exception( "Fatal error" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogShortWarningOnClientDisconnectMidwayThroughQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogShortWarningOnClientDisconnectMidwayThroughQuery()
		 {
			  // Connections dying is not exceptional per-se, so we don't need to fill the log with
			  // eye-catching stack traces; but it could be indicative of some issue, so log a brief
			  // warning in the debug log at least.

			  // Given
			  PackOutputClosedException outputClosed = new PackOutputClosedException( "Output closed", "<client>" );
			  Neo4jError txTerminated = Neo4jError.from( new TransactionTerminatedException( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated ) );

			  // When
			  AssertableLogProvider logProvider = EmulateFailureWritingError( txTerminated, outputClosed );

			  // Then
			  logProvider.AssertExactly( inLog( "Test" ).warn( equalTo( "Client %s disconnected while query was running. Session has been cleaned up. " + "This can be caused by temporary network problems, but if you see this often, ensure your " + "applications are properly waiting for operations to complete before exiting." ), equalTo( "<client>" ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testLoggingOfOriginalErrorWhenOutputIsClosed(org.neo4j.bolt.runtime.Neo4jError original) throws Exception
		 private static void TestLoggingOfOriginalErrorWhenOutputIsClosed( Neo4jError original )
		 {
			  PackOutputClosedException outputClosed = new PackOutputClosedException( "Output closed", "<client>" );
			  AssertableLogProvider logProvider = EmulateFailureWritingError( original, outputClosed );
			  logProvider.AssertExactly( inLog( "Test" ).warn( startsWith( "Unable to send error back to the client" ), equalTo( original.Cause() ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testLoggingOfWriteErrorAndOriginalErrorWhenUnknownFailure(org.neo4j.bolt.runtime.Neo4jError original) throws Exception
		 private static void TestLoggingOfWriteErrorAndOriginalErrorWhenUnknownFailure( Neo4jError original )
		 {
			  Exception outputError = new Exception( "Output failed" );
			  AssertableLogProvider logProvider = EmulateFailureWritingError( original, outputError );
			  logProvider.AssertExactly( inLog( "Test" ).error( startsWith( "Unable to send error back to the client" ), both( equalTo( outputError ) ).and( hasSuppressed( original.Cause() ) ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.logging.AssertableLogProvider emulateFailureWritingError(org.neo4j.bolt.runtime.Neo4jError error, Throwable errorDuringWrite) throws Exception
		 private static AssertableLogProvider EmulateFailureWritingError( Neo4jError error, Exception errorDuringWrite )
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  BoltResponseMessageWriter responseHandler = NewResponseHandlerMock( error.Fatal, errorDuringWrite );

			  MessageProcessingHandler handler = new MessageProcessingHandler( responseHandler, mock( typeof( BoltConnection ) ), logProvider.GetLog( "Test" ) );

			  handler.MarkFailed( error );
			  handler.OnFinish();

			  return logProvider;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.bolt.messaging.BoltResponseMessageWriter newResponseHandlerMock(boolean fatalError, Throwable error) throws Exception
		 private static BoltResponseMessageWriter NewResponseHandlerMock( bool fatalError, Exception error )
		 {
			  BoltResponseMessageWriter handler = NewResponseHandlerMock();

			  doThrow( error ).when( handler ).write( any( typeof( FailureMessage ) ) );
			  return handler;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static org.neo4j.bolt.messaging.BoltResponseMessageWriter newResponseHandlerMock()
		 private static BoltResponseMessageWriter NewResponseHandlerMock()
		 {
			  return mock( typeof( BoltResponseMessageWriter ) );
		 }
	}

}