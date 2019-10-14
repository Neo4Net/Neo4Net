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
namespace Neo4Net.Bolt.runtime
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	internal class MutableConnectionStateTest
	{
		 private readonly MutableConnectionState _state = new MutableConnectionState();

		 private readonly BoltResult _result = mock( typeof( BoltResult ) );
		 private readonly BoltResponseHandler _responseHandler = mock( typeof( BoltResponseHandler ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleOnRecordsWithoutResponseHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleOnRecordsWithoutResponseHandler()
		 {
			  _state.ResponseHandler = null;

			  _state.onRecords( _result, true );

			  assertNull( _state.PendingError );
			  assertFalse( _state.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleOnRecordsWitResponseHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleOnRecordsWitResponseHandler()
		 {
			  _state.ResponseHandler = _responseHandler;

			  _state.onRecords( _result, true );

			  verify( _responseHandler ).onRecords( _result, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleOnMetadataWithoutResponseHandler()
		 internal virtual void ShouldHandleOnMetadataWithoutResponseHandler()
		 {
			  _state.ResponseHandler = null;

			  _state.onMetadata( "key", stringValue( "value" ) );

			  assertNull( _state.PendingError );
			  assertFalse( _state.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleOnMetadataWitResponseHandler()
		 internal virtual void ShouldHandleOnMetadataWitResponseHandler()
		 {
			  _state.ResponseHandler = _responseHandler;

			  _state.onMetadata( "key", stringValue( "value" ) );

			  verify( _responseHandler ).onMetadata( "key", stringValue( "value" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMarkIgnoredWithoutResponseHandler()
		 internal virtual void ShouldHandleMarkIgnoredWithoutResponseHandler()
		 {
			  _state.ResponseHandler = null;

			  _state.markIgnored();

			  assertNull( _state.PendingError );
			  assertTrue( _state.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMarkIgnoredWitResponseHandler()
		 internal virtual void ShouldHandleMarkIgnoredWitResponseHandler()
		 {
			  _state.ResponseHandler = _responseHandler;

			  _state.markIgnored();

			  verify( _responseHandler ).markIgnored();
			  assertNull( _state.PendingError );
			  assertFalse( _state.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMarkFailedWithoutResponseHandler()
		 internal virtual void ShouldHandleMarkFailedWithoutResponseHandler()
		 {
			  _state.ResponseHandler = null;

			  Neo4jError error = Neo4jError.From( new Exception() );
			  _state.markFailed( error );

			  assertEquals( error, _state.PendingError );
			  assertFalse( _state.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMarkFailedWitResponseHandler()
		 internal virtual void ShouldHandleMarkFailedWitResponseHandler()
		 {
			  _state.ResponseHandler = _responseHandler;

			  Neo4jError error = Neo4jError.From( new Exception() );
			  _state.markFailed( error );

			  verify( _responseHandler ).markFailed( error );
			  assertNull( _state.PendingError );
			  assertFalse( _state.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleOnFinishWithoutResponseHandler()
		 internal virtual void ShouldHandleOnFinishWithoutResponseHandler()
		 {
			  _state.ResponseHandler = null;

			  _state.onFinish();

			  assertNull( _state.PendingError );
			  assertFalse( _state.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleOnFinishWitResponseHandler()
		 internal virtual void ShouldHandleOnFinishWitResponseHandler()
		 {
			  _state.ResponseHandler = _responseHandler;

			  _state.onFinish();

			  verify( _responseHandler ).onFinish();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldResetPendingFailureAndIgnored()
		 internal virtual void ShouldResetPendingFailureAndIgnored()
		 {
			  _state.ResponseHandler = null;

			  Neo4jError error = Neo4jError.From( new Exception() );
			  _state.markIgnored();
			  _state.markFailed( error );

			  assertEquals( error, _state.PendingError );
			  assertTrue( _state.hasPendingIgnore() );

			  _state.resetPendingFailedAndIgnored();

			  assertNull( _state.PendingError );
			  assertFalse( _state.hasPendingIgnore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotProcessMessageWhenClosed()
		 internal virtual void ShouldNotProcessMessageWhenClosed()
		 {
			  _state.ResponseHandler = null;

			  _state.markClosed();

			  assertFalse( _state.canProcessMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotProcessMessageWithPendingError()
		 internal virtual void ShouldNotProcessMessageWithPendingError()
		 {
			  _state.ResponseHandler = null;

			  _state.markFailed( Neo4jError.From( new Exception() ) );

			  assertFalse( _state.canProcessMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotProcessMessageWithPendingIgnore()
		 internal virtual void ShouldNotProcessMessageWithPendingIgnore()
		 {
			  _state.ResponseHandler = null;

			  _state.markIgnored();

			  assertFalse( _state.canProcessMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInterrupt()
		 internal virtual void ShouldInterrupt()
		 {
			  assertFalse( _state.Interrupted );

			  assertEquals( 1, _state.incrementInterruptCounter() );
			  assertTrue( _state.Interrupted );

			  assertEquals( 2, _state.incrementInterruptCounter() );
			  assertTrue( _state.Interrupted );

			  assertEquals( 3, _state.incrementInterruptCounter() );
			  assertTrue( _state.Interrupted );

			  assertEquals( 2, _state.decrementInterruptCounter() );
			  assertTrue( _state.Interrupted );

			  assertEquals( 1, _state.decrementInterruptCounter() );
			  assertTrue( _state.Interrupted );

			  assertEquals( 0, _state.decrementInterruptCounter() );
			  assertFalse( _state.Interrupted );
		 }
	}

}