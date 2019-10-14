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
namespace Neo4Net.Csv.Reader
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class SectionedCharBufferTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompactIntoItself() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCompactIntoItself()
		 {
			  // GIVEN
			  Reader data = new StringReader( "01234567" );
			  SectionedCharBuffer buffer = new SectionedCharBuffer( 4 ); // will yield an 8-char array in total
			  buffer.ReadFrom( data );

			  // WHEN
			  buffer.Compact( buffer, buffer.Front() - 2 );

			  // THEN
			  assertEquals( '2', buffer.Array()[2] );
			  assertEquals( '3', buffer.Array()[3] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompactIntoAnotherBuffer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCompactIntoAnotherBuffer()
		 {
			  // GIVEN
			  Reader data = new StringReader( "01234567" );
			  SectionedCharBuffer buffer1 = new SectionedCharBuffer( 8 );
			  SectionedCharBuffer buffer2 = new SectionedCharBuffer( 8 );
			  buffer1.ReadFrom( data );

			  // WHEN
			  buffer2.ReadFrom( data );
			  // simulate reading 2 chars as one value, then reading 2 bytes and requesting more
			  buffer1.Compact( buffer2, buffer1.Pivot() + 2 );

			  // THEN
			  assertEquals( '2', buffer2.Array()[2] );
			  assertEquals( '3', buffer2.Array()[3] );
			  assertEquals( '4', buffer2.Array()[4] );
			  assertEquals( '5', buffer2.Array()[5] );
			  assertEquals( '6', buffer2.Array()[6] );
			  assertEquals( '7', buffer2.Array()[7] );
		 }
	}

}