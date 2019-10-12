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
	using SuccessMessage = Org.Neo4j.Bolt.v1.messaging.response.SuccessMessage;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	internal class SuccessMessageEncoderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEncodeSuccessMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldEncodeSuccessMessage()
		 {
			  // Given
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = mock( typeof( Org.Neo4j.Bolt.messaging.Neo4jPack_Packer ) );
			  SuccessMessageEncoder encoder = new SuccessMessageEncoder();

			  // When
			  MapValue meta = mock( typeof( MapValue ) );
			  encoder.Encode( packer, new SuccessMessage( meta ) );

			  // Then
			  verify( packer ).packStructHeader( anyInt(), eq(SuccessMessage.SIGNATURE) );
			  verify( packer ).pack( meta );
		 }
	}

}