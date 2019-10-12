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
namespace Org.Neo4j.Bolt.v1.messaging.encoder
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using IgnoredMessage = Org.Neo4j.Bolt.v1.messaging.response.IgnoredMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.response.IgnoredMessage.IGNORED_MESSAGE;

	internal class IgnoredMessageEncoderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEncodeIgnoredMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldEncodeIgnoredMessage()
		 {
			  // Given
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = mock( typeof( Org.Neo4j.Bolt.messaging.Neo4jPack_Packer ) );
			  IgnoredMessageEncoder encoder = new IgnoredMessageEncoder();

			  // When
			  encoder.Encode( packer, IGNORED_MESSAGE );

			  // Then
			  verify( packer ).packStructHeader( anyInt(), eq(IgnoredMessage.SIGNATURE) );
		 }
	}

}