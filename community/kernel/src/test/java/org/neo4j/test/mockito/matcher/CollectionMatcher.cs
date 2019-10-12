using System.Collections.Generic;

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
namespace Org.Neo4j.Test.mockito.matcher
{
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	/// <summary>
	/// An org.hamcrest Matcher that matches <seealso cref="System.Collections.ICollection collections"/>. </summary>
	/// @param <T> The parameter of the Collection to match </param>
	public class CollectionMatcher<T> : TypeSafeMatcher<ICollection<T>>
	{
		 private readonly ICollection<T> _toMatch;

		 private CollectionMatcher( ICollection<T> toMatch )
		 {
			  this._toMatch = toMatch;
		 }

		 protected internal override bool MatchesSafely( ICollection<T> objects )
		 {
			  return IterableMatcher.ItemsMatches( _toMatch, objects );
		 }

		 public override void DescribeTo( Description description )
		 {
			  description.appendValueList( "Collection [", ",", "]", _toMatch );
		 }

		 public static CollectionMatcher<T> MatchesCollection<T>( ICollection<T> toMatch )
		 {
			  return new CollectionMatcher<T>( toMatch );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> CollectionMatcher<T> matchesCollection(T... toMatch)
		 public static CollectionMatcher<T> MatchesCollection<T>( params T[] toMatch )
		 {
			  return new CollectionMatcher<T>( asList( toMatch ) );
		 }
	}

}