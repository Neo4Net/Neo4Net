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
namespace Neo4Net.Bolt.v1.messaging.encoder
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using AnyValue = Neo4Net.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class RecordMessageEncoderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEncodeRecordMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldEncodeRecordMessage()
		 {
			  // Given
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = mock( typeof( Neo4Net.Bolt.messaging.Neo4jPack_Packer ) );
			  RecordMessageEncoder encoder = new RecordMessageEncoder();

			  // When
			  Neo4Net.Cypher.result.QueryResult_Record value = mock( typeof( Neo4Net.Cypher.result.QueryResult_Record ) );
			  when( value.Fields() ).thenReturn(new AnyValue[0]);
			  encoder.Encode( packer, new RecordMessage( value ) );

			  // Then
			  verify( packer ).packStructHeader( anyInt(), eq(RecordMessage.SIGNATURE) );
			  verify( packer ).packListHeader( 0 );
		 }
	}

}