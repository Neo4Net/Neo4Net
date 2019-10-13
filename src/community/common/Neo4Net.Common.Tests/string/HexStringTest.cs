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
namespace Neo4Net.@string
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class HexStringTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEncodeBytesToString()
		 internal virtual void ShouldEncodeBytesToString()
		 {
			  string result = HexString.EncodeHexString( new sbyte[]{ unchecked( ( sbyte ) 0xFF ), unchecked( ( sbyte ) 0x94 ), ( sbyte ) 0x5C, ( sbyte ) 0x00, ( sbyte ) 0x3D } );
			  assertEquals( "FF945C003D", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEncodeEmptyBytesToEmptyString()
		 internal virtual void ShouldEncodeEmptyBytesToEmptyString()
		 {
			  string result = HexString.EncodeHexString( new sbyte[]{} );
			  assertEquals( "", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeStringToBytes()
		 internal virtual void ShouldDecodeStringToBytes()
		 {
			  sbyte[] result = HexString.DecodeHexString( "00f34CEFFF3e02" );
			  sbyte[] expected = new sbyte[] { ( sbyte ) 0x00, unchecked( ( sbyte ) 0xF3 ), ( sbyte ) 0x4C, unchecked( ( sbyte ) 0xEF ), unchecked( ( sbyte ) 0xFF ), ( sbyte ) 0x3E, ( sbyte ) 0x02 };
			  assertArrayEquals( expected, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDecodeEmptyStringToEmptyBytes()
		 internal virtual void ShouldDecodeEmptyStringToEmptyBytes()
		 {
			  sbyte[] result = HexString.DecodeHexString( "" );
			  assertArrayEquals( new sbyte[]{}, result );
		 }
	}

}