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
namespace Neo4Net.Bolt.v3.messaging.request
{
	using Test = org.junit.jupiter.api.Test;


	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;

	internal class BeginMessageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseEmptyTransactionMetadataCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldParseEmptyTransactionMetadataCorrectly()
		 {
			  // When
			  BeginMessage message = new BeginMessage();

			  // Then
			  assertNull( message.TransactionMetadata() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowExceptionIfFailedToParseTransactionMetadataCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowExceptionIfFailedToParseTransactionMetadataCorrectly()
		 {
			  // Given
			  IDictionary<string, object> msgMetadata = map( "tx_metadata", "invalid value type" );
			  MapValue meta = ValueUtils.asMapValue( msgMetadata );
			  // When & Then
			  BoltIOException exception = assertThrows( typeof( BoltIOException ), () => new BeginMessage(meta) );
			  assertThat( exception.Message, startsWith( "Expecting transaction metadata value to be a Map value" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTransactionMetadataCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldParseTransactionMetadataCorrectly()
		 {
			  // Given
			  IDictionary<string, object> txMetadata = map( "creation-time", Duration.ofMillis( 4321L ) );
			  IDictionary<string, object> msgMetadata = map( "tx_metadata", txMetadata );
			  MapValue meta = ValueUtils.asMapValue( msgMetadata );

			  // When
			  BeginMessage beginMessage = new BeginMessage( meta );

			  // Then
			  assertThat( beginMessage.TransactionMetadata().ToString(), equalTo(txMetadata.ToString()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowExceptionIfFailedToParseTransactionTimeoutCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowExceptionIfFailedToParseTransactionTimeoutCorrectly()
		 {
			  // Given
			  IDictionary<string, object> msgMetadata = map( "tx_timeout", "invalid value type" );
			  MapValue meta = ValueUtils.asMapValue( msgMetadata );
			  // When & Then
			  BoltIOException exception = assertThrows( typeof( BoltIOException ), () => new BeginMessage(meta) );
			  assertThat( exception.Message, startsWith( "Expecting transaction timeout value to be a Long value" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTransactionTimeoutCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldParseTransactionTimeoutCorrectly()
		 {
			  // Given
			  IDictionary<string, object> msgMetadata = map( "tx_timeout", 123456L );
			  MapValue meta = ValueUtils.asMapValue( msgMetadata );

			  // When
			  BeginMessage beginMessage = new BeginMessage( meta );

			  // Then
			  assertThat( beginMessage.TransactionTimeout().toMillis(), equalTo(123456L) );
		 }
	}

}