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
namespace Neo4Net.Values.Storable
{
	/// <summary>
	/// Static methods for comparing and hashing chars, Strings and Text values.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public final class TextValues
	public sealed class TextValues
	{
		 private TextValues()
		 {
		 }

		 public static int CompareCharToString( char c, string s )
		 {
			  int length = s.Length;
			  int x = length == 0 ? 1 : 0;
			  if ( x == 0 )
			  {
					x = Character.compare( c, s[0] );
					if ( x == 0 && length > 1 )
					{
						 x = -1;
					}
			  }
			  return x;
		 }

		 public static int CompareTextArrays( TextArray a, TextArray b )
		 {
			  int i = 0;
			  int x = 0;
			  int length = Math.Min( a.Length(), b.Length() );

			  while ( x == 0 && i < length )
			  {
					x = a.StringValue( i ).CompareTo( b.StringValue( i ) );
					i++;
			  }
			  if ( x == 0 )
			  {
					x = a.Length() - b.Length();
			  }
			  return x;
		 }
	}

}