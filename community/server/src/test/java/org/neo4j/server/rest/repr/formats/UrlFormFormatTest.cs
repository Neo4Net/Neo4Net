﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.repr.formats
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;


	using Test = org.junit.Test;

	public class UrlFormFormatTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseEmptyMap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseEmptyMap()
		 {
			  UrlFormFormat format = new UrlFormFormat();
			  IDictionary<string, object> map = format.ReadMap( "" );

			  assertThat( map.Count, @is( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canParseSingleKeyMap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanParseSingleKeyMap()
		 {
			  UrlFormFormat format = new UrlFormFormat();
			  IDictionary<string, object> map = format.ReadMap( "var=A" );

			  assertThat( map.Count, @is( 1 ) );
			  assertThat( map["var"], @is( "A" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canParseListsInMaps() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanParseListsInMaps()
		 {
			  UrlFormFormat format = new UrlFormFormat();
			  IDictionary<string, object> map = format.ReadMap( "var=A&var=B" );

			  assertThat( map.Count, @is( 1 ) );
			  assertThat( ( ( IList<string> ) map["var"] )[0], @is( "A" ) );
			  assertThat( ( ( IList<string> ) map["var"] )[1], @is( "B" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportPhpStyleUrlEncodedPostBodiesForAListOnly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportPhpStyleUrlEncodedPostBodiesForAListOnly()
		 {
			  UrlFormFormat format = new UrlFormFormat();
			  IDictionary<string, object> map = format.ReadMap( "var[]=A&var[]=B" );

			  assertThat( map.Count, @is( 1 ) );
			  assertThat( ( ( IList<string> ) map["var"] )[0], @is( "A" ) );
			  assertThat( ( ( IList<string> ) map["var"] )[1], @is( "B" ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportPhpStyleUrlEncodedPostBodiesForAListAndNonListParams() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportPhpStyleUrlEncodedPostBodiesForAListAndNonListParams()
		 {
			  UrlFormFormat format = new UrlFormFormat();
			  IDictionary<string, object> map = format.ReadMap( "var[]=A&var[]=B&foo=bar" );

			  assertThat( map.Count, @is( 2 ) );
			  assertThat( ( ( IList<string> ) map["var"] )[0], @is( "A" ) );
			  assertThat( ( ( IList<string> ) map["var"] )[1], @is( "B" ) );
			  assertEquals( "bar", map["foo"] );
		 }

	}

}