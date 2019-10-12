using System;

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
namespace Org.Neo4j.Server.rest.repr.util
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;

	public class RFC1123Test
	{
		 private readonly DateTime _calendar = DateTime.getInstance( RFC1123.Gmt, Locale.US );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseRFC1123() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseRFC1123()
		 {
			  // given
			  string input = "Mon, 15 Aug 2005 15:52:01 +0000";

			  // when
			  DateTime result = RFC1123.ParseTimestamp( input );

			  // then
			  _calendar = new DateTime( result );
			  assertEquals( DayOfWeek.Monday, _calendar.DayOfWeek );
			  assertEquals( 15, _calendar.Day );
			  assertEquals( 8, _calendar.Month );
			  assertEquals( 2005, _calendar.Year );
			  assertEquals( 15, _calendar.Hour );
			  assertEquals( 52, _calendar.Minute );
			  assertEquals( 1, _calendar.Second );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatRFC1123() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFormatRFC1123()
		 {
			  // given
			  string input = "Mon, 15 Aug 2005 15:52:01 +0000";

			  // when
			  string output = RFC1123.FormatDate( RFC1123.ParseTimestamp( input ) );

			  // then
			  assertEquals( input, output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSameInstanceInSameThread()
		 public virtual void ShouldReturnSameInstanceInSameThread()
		 {
			  // given
			  RFC1123 instance = RFC1123.Instance();

			  // when
			  RFC1123 instance2 = RFC1123.Instance();

			  // then
			  assertSame( "Expected to get same instance from second call to RFC1123.instance() in same thread", instance, instance2 );
		 }
	}

}