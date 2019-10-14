using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.util
{

	using Neo4Net.Helpers.Collections;

	/// <summary>
	/// Comparator for strings that may, or may not, contain groups of digits representing numbers and where
	/// those numbers should be compared for what their numeric values are, not their string representations.
	/// This will solve a classic sorting issue that plain string sorting misses:
	/// <ol>
	/// <li>string-1</li>
	/// <li>string-2</li>
	/// <li>string-12</li>
	/// </ol>
	/// Where the above would be sorted as {@code string-1}, {@code string-12}, {@code string-2}, which may be
	/// undesirable in scenarios where the number matters. This comparator will sort the strings from the
	/// example above as {@code string-1}, {@code string-2}, {@code string-12}.
	/// </summary>
	public class NumberAwareStringComparator : IComparer<string>
	{
		 public static readonly IComparer<string> Instance = new NumberAwareStringComparator();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "rawtypes", "unchecked" }) @Override public int compare(String o1, String o2)
		 public override int Compare( string o1, string o2 )
		 {
			  IEnumerator<IComparable> c1 = Comparables( o1 );
			  IEnumerator<IComparable> c2 = Comparables( o2 );
			  // Single "|" to get both expressions always evaluated, you know, it's a good pattern to
			  // call hasNext before next on iterators.
			  bool c1Has;
			  bool c2Has;
			  while ( ( c1Has = c1.MoveNext() ) | (c2Has = c2.MoveNext()) )
			  {
					if ( !c1Has )
					{
						 return -1;
					}
					if ( !c2Has )
					{
						 return 1;
					}

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					int diff = c1.Current.compareTo( c2.next() );
					if ( diff != 0 )
					{
						 return diff;
					}
					// else continue
			  }
			  // All elements are comparable with each other
			  return 0;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private java.util.Iterator<Comparable> comparables(final String string)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private IEnumerator<IComparable> Comparables( string @string )
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this, @string );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<IComparable>
		 {
			 private readonly NumberAwareStringComparator _outerInstance;

			 private string @string;

			 public PrefetchingIteratorAnonymousInnerClass( NumberAwareStringComparator outerInstance, string @string )
			 {
				 this.outerInstance = outerInstance;
				 this.@string = @string;
			 }

			 private int index;

			 protected internal override IComparable fetchNextOrNull()
			 {
				  if ( index >= @string.Length )
				  { // End reached
						return null;
				  }

				  int startIndex = index;
				  char ch = @string[index];
				  bool isNumber = char.IsDigit( ch );
				  while ( char.IsDigit( ch ) == isNumber && ++index < @string.Length )
				  {
						ch = @string[index];
				  }
				  string substring = @string.Substring( startIndex, index - startIndex );
				  return isNumber ? Convert.ToInt64( substring ) : substring;
			 }
		 }
	}

}