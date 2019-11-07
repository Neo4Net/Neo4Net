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
//	import static Neo4Net.helpers.TimeUtil.nanosToString;

	internal class TimeUtilTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void formatNanosToString()
		 internal virtual void FormatNanosToString()
		 {
			  assertEquals( "1ns", nanosToString( 1 ) );
			  assertEquals( "10ns", nanosToString( 10 ) );
			  assertEquals( "100ns", nanosToString( 100 ) );
			  assertEquals( "1μs", nanosToString( 1000 ) );
			  assertEquals( "10μs100ns", nanosToString( 10100 ) );
			  assertEquals( "101μs10ns", nanosToString( 101010 ) );
			  assertEquals( "10ms101μs10ns", nanosToString( 10101010 ) );
			  assertEquals( "1s20ms304μs50ns", nanosToString( 1020304050 ) );
			  assertEquals( "1m42s30ms405μs60ns", nanosToString( 102030405060L ) );
			  assertEquals( "2h50m3s40ms506μs70ns", nanosToString( 10203040506070L ) );
			  assertEquals( "11d19h25m4s50ms607μs80ns", nanosToString( 1020304050607080L ) );
		 }
	}

}