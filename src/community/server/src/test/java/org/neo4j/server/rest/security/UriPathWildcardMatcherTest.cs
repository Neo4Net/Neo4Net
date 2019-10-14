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
namespace Neo4Net.Server.rest.security
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class UriPathWildcardMatcherTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWithoutAsteriskAtStart()
		 public virtual void ShouldFailWithoutAsteriskAtStart()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "/some/uri/path" );

			  assertFalse( matcher.Matches( "preamble/some/uri/path" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWithoutAsteriskAtEnd()
		 public virtual void ShouldFailWithoutAsteriskAtEnd()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "/some/uri/path/and/some/more" );

			  assertFalse( matcher.Matches( "/some/uri/path/with/middle/bit/and/some/more" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchAsteriskAtStart()
		 public virtual void ShouldMatchAsteriskAtStart()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "*/some/uri/path" );

			  assertTrue( matcher.Matches( "anything/i/like/followed/by/some/uri/path" ) );
			  assertFalse( matcher.Matches( "anything/i/like/followed/by/some/deliberately/changed/to/fail/uri/path" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchAsteriskAtEnd()
		 public virtual void ShouldMatchAsteriskAtEnd()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "/some/uri/path/*" );

			  assertTrue( matcher.Matches( "/some/uri/path/followed/by/anything/i/like" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchAsteriskInMiddle()
		 public virtual void ShouldMatchAsteriskInMiddle()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "/some/uri/path/*/and/some/more" );

			  assertTrue( matcher.Matches( "/some/uri/path/with/middle/bit/and/some/more" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchMultipleAsterisksInMiddle()
		 public virtual void ShouldMatchMultipleAsterisksInMiddle()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "/some/uri/path/*/and/some/more/*/and/a/final/bit" );

			  assertTrue( matcher.Matches( "/some/uri/path/with/middle/bit/and/some/more/with/additional/asterisk/part/and/a/final/bit" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchMultipleAsterisksAtStartAndInMiddle()
		 public virtual void ShouldMatchMultipleAsterisksAtStartAndInMiddle()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "*/some/uri/path/*/and/some/more/*/and/a/final/bit" );

			  assertTrue( matcher.Matches( "a/bit/of/preamble/and/then/some/uri/path/with/middle/bit/and/some/more/with/additional/asterisk" + "/part/and/a/final/bit" ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchMultipleAsterisksAtEndAndInMiddle()
		 public virtual void ShouldMatchMultipleAsterisksAtEndAndInMiddle()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "/some/uri/path/*/and/some/more/*/and/a/final/bit/*" );

			  assertTrue( matcher.Matches( "/some/uri/path/with/middle/bit/and/some/more/with/additional/asterisk/part/and/a/final/bit/and/now" + "/some/post/amble" ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchMultipleAsterisksAtStartAndEndAndInMiddle()
		 public virtual void ShouldMatchMultipleAsterisksAtStartAndEndAndInMiddle()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "*/some/uri/path/*/and/some/more/*/and/a/final/bit/*" );

			  assertTrue( matcher.Matches( "a/bit/of/preamble/and/then//some/uri/path/with/middle/bit/and/some/more/with/additional/asterisk" + "/part/and/a/final/bit/and/now/some/post/amble" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchMultipleSimpleString()
		 public virtual void ShouldMatchMultipleSimpleString()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "str" );

			  assertTrue( matcher.Matches( "str" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchMultipleSimpleStringWithALeadingWildcard()
		 public virtual void ShouldMatchMultipleSimpleStringWithALeadingWildcard()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "*str" );

			  assertTrue( matcher.Matches( "my_str" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToMatchMultipleSimpleStringWithATrailingWildcard()
		 public virtual void ShouldFailToMatchMultipleSimpleStringWithATrailingWildcard()
		 {
			  UriPathWildcardMatcher matcher = new UriPathWildcardMatcher( "str*" );

			  assertFalse( matcher.Matches( "my_str" ) );
		 }
	}

}