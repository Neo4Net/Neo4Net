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
namespace Neo4Net.Test.mockito.matcher
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;

	public class RegexMatcher : TypeSafeMatcher<string>
	{
		 private readonly Pattern _pattern;

		 public RegexMatcher( Pattern pattern )
		 {
			  this._pattern = pattern;
		 }

		 public static Matcher<string> Pattern( string regex )
		 {
			  return new RegexMatcher( Pattern.compile( regex ) );
		 }

		 protected internal override bool MatchesSafely( string item )
		 {
			  return _pattern.matcher( item ).matches();
		 }

		 public override void DescribeTo( Description description )
		 {
			  description.appendText( "a string matching /" );
			  description.appendText( _pattern.ToString() );
			  description.appendText( "/" );
		 }
	}

}