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
namespace Org.Neo4j.Commandline.arguments
{
	using Test = org.junit.jupiter.api.Test;

	using Args = Org.Neo4j.Helpers.Args;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class OptionalBooleanArgTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void parsesValues1()
		 internal virtual void ParsesValues1()
		 {
			  OptionalBooleanArg arg = new OptionalBooleanArg( "foo", false, "" );

			  assertEquals( "false", arg.Parse( Args.parse() ) );
			  assertEquals( "false", arg.Parse( Args.parse( "--foo=false" ) ) );
			  assertEquals( "true", arg.Parse( Args.parse( "--foo=true" ) ) );
			  assertEquals( "true", arg.Parse( Args.parse( "--foo" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void parsesValues2()
		 internal virtual void ParsesValues2()
		 {
			  OptionalBooleanArg arg = new OptionalBooleanArg( "foo", true, "" );

			  assertEquals( "true", arg.Parse( Args.parse() ) );
			  assertEquals( "false", arg.Parse( Args.parse( "--foo=false" ) ) );
			  assertEquals( "true", arg.Parse( Args.parse( "--foo=true" ) ) );
			  assertEquals( "true", arg.Parse( Args.parse( "--foo" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void usageTest()
		 internal virtual void UsageTest()
		 {
			  OptionalBooleanArg arg = new OptionalBooleanArg( "foo", true, "" );

			  assertEquals( "[--foo[=<true|false>]]", arg.Usage() );
		 }
	}

}