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
namespace Neo4Net.Test.mockito.matcher
{
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;

	using Iterables = Neo4Net.Helpers.Collection.Iterables;

	/// <summary>
	/// An org.hamcrest Matcher that matches Iterables. </summary>
	/// @param <T> The parameter of the Iterable to match </param>
	public class IterableMatcher<T> : TypeSafeMatcher<IEnumerable<T>>
	{
		 private readonly IEnumerable<T> _toMatch;

		 private IterableMatcher( IEnumerable<T> toMatch )
		 {
			  this._toMatch = toMatch;
		 }

		 protected internal override bool MatchesSafely( IEnumerable<T> objects )
		 {
			  return ItemsMatches( _toMatch, objects );
		 }

		 internal static bool ItemsMatches<T>( IEnumerable<T> expected, IEnumerable<T> actual )
		 {
			  if ( Iterables.count( expected ) != Iterables.count( actual ) )
			  {
					return false;
			  }
			  IEnumerator<T> original = expected.GetEnumerator();
			  IEnumerator<T> matched = actual.GetEnumerator();
			  T fromOriginal;
			  T fromToMatch;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  for ( ; original.hasNext() && matched.hasNext(); )
			  {
					fromOriginal = original.Current;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					fromToMatch = matched.next();
					if ( !fromOriginal.Equals( fromToMatch ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public override void DescribeTo( Description description )
		 {
			  description.appendValueList( "Iterable [", ",", "]", _toMatch );
		 }

		 public static IterableMatcher<T> MatchesIterable<T>( IEnumerable<T> toMatch )
		 {
			  return new IterableMatcher<T>( toMatch );
		 }
	}

}