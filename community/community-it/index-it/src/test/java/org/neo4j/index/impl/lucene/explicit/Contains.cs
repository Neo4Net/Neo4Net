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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using Description = org.hamcrest.Description;
	using Factory = org.hamcrest.Factory;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;

	using Org.Neo4j.Graphdb.index;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;

	public class Contains<T> : TypeSafeMatcher<IndexHits<T>>
	{
		 private readonly T[] _expectedItems;
		 private string _message;

		 public Contains( params T[] expectedItems )
		 {
			  this._expectedItems = expectedItems;
		 }

		 public override bool MatchesSafely( IndexHits<T> indexHits )
		 {
			  ICollection<T> collection = Iterators.asCollection( indexHits.GetEnumerator() );

			  if ( _expectedItems.Length != collection.Count )
			  {
					_message = "IndexHits with a size of " + _expectedItems.Length + ", got one with " + collection.Count;
					_message += collection.ToString();
					return false;
			  }

			  foreach ( T item in _expectedItems )
			  {
					if ( !collection.Contains( item ) )
					{
						 _message = "Item (" + item + ") not found.";
						 return false;
					}

			  }
			  return true;
		 }

		 public override void DescribeTo( Description description )
		 {
			  if ( !string.ReferenceEquals( _message, null ) )
			  {
					description.appendText( _message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Factory public static <T> Contains<T> contains(T... expectedItems)
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Contains<T> ContainsConflict<T>( params T[] expectedItems )
		 {
			  return new Contains<T>( expectedItems );
		 }
	}

}