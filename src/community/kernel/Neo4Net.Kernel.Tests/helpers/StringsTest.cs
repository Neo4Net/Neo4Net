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
namespace Neo4Net.Helpers
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Strings.prettyPrint;

	internal class StringsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testPrettyPrint()
		 internal virtual void TestPrettyPrint()
		 {
			  assertEquals( "null", prettyPrint( null ) );
			  assertEquals( "42", prettyPrint( 42 ) );
			  assertEquals( "42", prettyPrint( "42" ) );
			  assertEquals( "[1, 2, 3, 4]", prettyPrint( new int[]{ 1, 2, 3, 4 } ) );
			  assertEquals( "[false, true, true, false]", prettyPrint( new bool[]{ false, true, true, false } ) );
			  assertEquals( "[a, b, z]", prettyPrint( new char[]{ 'a', 'b', 'z' } ) );
			  assertEquals( "[ab, cd, zx]", prettyPrint( new string[]{ "ab", "cd", "zx" } ) );
			  assertEquals("[Cat, [http://Neo4Net.com, http://Neo4Net.org], Dog, [1, 2, 3], [[[Wolf]]]]", prettyPrint(new object[]
			  {
				  "Cat", new URI[]{ URI.create( "http://Neo4Net.com" ), URI.create( "http://Neo4Net.org" ) },
				  "Dog", new int[]{ 1, 2, 3 },
				  new object[]
				  {
					  new object[]
					  {
						  new object[]{ "Wolf" }
					  }
				  }
			  }));

			  object[] recursiveArray = new object[] { 10.12345, null, "String" };
			  recursiveArray[1] = recursiveArray;
			  assertEquals( "[10.12345, [...], String]", prettyPrint( recursiveArray ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testEscape()
		 internal virtual void TestEscape()
		 {
			  assertEquals( "abc", Strings.Escape( "abc" ) );
			  assertEquals( "Abc", Strings.Escape( "Abc" ) );
			  assertEquals( "a\\\"bc", Strings.Escape( "a\"bc" ) );
			  assertEquals( "a\\\'bc", Strings.Escape( "a\'bc" ) );
			  assertEquals( "a\\\\bc", Strings.Escape( "a\\bc" ) );
			  assertEquals( "a\\nbc", Strings.Escape( "a\nbc" ) );
			  assertEquals( "a\\tbc", Strings.Escape( "a\tbc" ) );
			  assertEquals( "a\\rbc", Strings.Escape( "a\rbc" ) );
			  assertEquals( "a\\bbc", Strings.Escape( "a\bbc" ) );
			  assertEquals( "a\\fbc", Strings.Escape( "a\fbc" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testJoiningLines()
		 internal virtual void TestJoiningLines()
		 {
			  assertEquals( "a" + Environment.NewLine + "b" + Environment.NewLine + "c" + Environment.NewLine, Strings.JoinAsLines( "a", "b", "c" ) );
		 }
	}

}