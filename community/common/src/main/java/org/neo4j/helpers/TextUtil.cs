using System;
using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.Helpers
{

	/// @deprecated This class will be removed from public API in 4.0. 
	[Obsolete("This class will be removed from public API in 4.0.")]
	public class TextUtil
	{
		 private TextUtil()
		 {
		 }

		 public static string TemplateString<T1>( string templateString, IDictionary<T1> data )
		 {
			  return templateString( templateString, "\\$", data );
		 }

		 public static string TemplateString<T1>( string templateString, string variablePrefix, IDictionary<T1> data )
		 {
			  // Sort data strings on length.
			  IDictionary<int, IList<string>> lengthMap = new Dictionary<int, IList<string>>();
			  int longest = 0;
			  foreach ( string key in data.Keys )
			  {
					int length = key.Length;
					if ( length > longest )
					{
						 longest = length;
					}

					IList<string> innerList;
					int? innerKey = length;
					if ( lengthMap.ContainsKey( innerKey ) )
					{
						 innerList = lengthMap[innerKey];
					}
					else
					{
						 innerList = new List<string>();
						 lengthMap[innerKey] = innerList;
					}
					innerList.Add( key );
			  }

			  // Replace it.
			  string result = templateString;
			  for ( int i = longest; i >= 0; i-- )
			  {
					int? lengthKey = i;
					if ( !lengthMap.ContainsKey( lengthKey ) )
					{
						 continue;
					}

					IList<string> list = lengthMap[lengthKey];
					foreach ( string key in list )
					{
						 object value = data[key];
						 if ( value != null )
						 {
							  string replacement = data[key].ToString();
							  string regExpMatchString = variablePrefix + key;
							  result = result.replaceAll( regExpMatchString, replacement );
						 }
					}
			  }

			  return result;
		 }

		 public static string LastWordOrQuoteOf( string text, bool preserveQuotation )
		 {
			  string[] quoteParts = text.Split( "\"", true );
			  string lastPart = quoteParts[quoteParts.Length - 1];
			  bool isWithinQuotes = quoteParts.Length % 2 == 0;
			  string lastWord;
			  if ( isWithinQuotes )
			  {
					lastWord = lastPart;
					if ( preserveQuotation )
					{
						 lastWord = "\"" + lastWord + ( text.EndsWith( "\"", StringComparison.Ordinal ) ? "\"" : "" );
					}
			  }
			  else
			  {
					string[] lastPartParts = SplitAndKeepEscapedSpaces( lastPart, preserveQuotation );
					lastWord = lastPartParts[lastPartParts.Length - 1];
			  }
			  return lastWord;
		 }

		 private static string[] SplitAndKeepEscapedSpaces( string @string, bool preserveEscapes )
		 {
			  return SplitAndKeepEscapedSpaces( @string, preserveEscapes, preserveEscapes );
		 }

		 private static string[] SplitAndKeepEscapedSpaces( string @string, bool preserveEscapes, bool preserveSpaceEscapes )
		 {
			  ICollection<string> result = new List<string>();
			  StringBuilder current = new StringBuilder();
			  for ( int i = 0; i < @string.Length; i++ )
			  {
					char ch = @string[i];
					if ( ch == ' ' )
					{
						 bool isEscapedSpace = i > 0 && @string[i - 1] == '\\';
						 if ( !isEscapedSpace )
						 {
							  result.Add( current.ToString() );
							  current = new StringBuilder();
							  continue;
						 }
						 if ( preserveEscapes && !preserveSpaceEscapes )
						 {
							  current.Length = current.Length - 1;
						 }
					}

					if ( preserveEscapes || ch != '\\' )
					{
						 current.Append( ch );
					}
			  }
			  if ( current.Length > 0 )
			  {
					result.Add( current.ToString() );
			  }
			  return result.toArray( new string[result.Count] );
		 }

		 /// <summary>
		 /// Tokenizes a string, regarding quotes.
		 /// </summary>
		 /// <param name="string"> the string to tokenize. </param>
		 /// <returns> the tokens from the line. </returns>
		 public static string[] TokenizeStringWithQuotes( string @string )
		 {
			  return TokenizeStringWithQuotes( @string, true, false );
		 }

		 /// <summary>
		 /// Tokenizes a string, regarding quotes. Examples:
		 /// 
		 /// o '"One two"'              ==&gt; [ "One two" ]
		 /// o 'One two'                ==&gt; [ "One", "two" ]
		 /// o 'One "two three" four'   ==&gt; [ "One", "two three", "four" ]
		 /// </summary>
		 /// <param name="string"> the string to tokenize. </param>
		 /// <param name="trim">  whether or not to trim each token. </param>
		 /// <param name="preserveEscapeCharacters"> whether or not to preserve escape characters '\', otherwise skip them. </param>
		 /// <returns> the tokens from the line. </returns>
		 public static string[] TokenizeStringWithQuotes( string @string, bool trim, bool preserveEscapeCharacters )
		 {
			  return TokenizeStringWithQuotes( @string, trim, preserveEscapeCharacters, preserveEscapeCharacters );
		 }

		 /// <summary>
		 /// Tokenizes a string, regarding quotes with a possibility to keep escaping characters but removing them when they used for space escaping.
		 /// </summary>
		 public static string[] TokenizeStringWithQuotes( string @string, bool trim, bool preserveEscapeCharacters, bool preserveSpaceEscapeCharacters )
		 {
			  if ( trim )
			  {
					@string = @string.Trim();
			  }
			  List<string> result = new List<string>();
			  @string = @string.Trim();
			  bool inside = @string.StartsWith( "\"", StringComparison.Ordinal );
			  StringTokenizer quoteTokenizer = new StringTokenizer( @string, "\"" );
			  while ( quoteTokenizer.hasMoreTokens() )
			  {
					string token = quoteTokenizer.nextToken();
					if ( trim )
					{
						 token = token.Trim();
					}
					if ( token.Length != 0 )
					{
						 if ( inside )
						 {
							  // Don't split
							  result.Add( token );
						 }
						 else
						 {
							  Collections.addAll( result, TextUtil.SplitAndKeepEscapedSpaces( token, preserveEscapeCharacters, preserveSpaceEscapeCharacters ) );
						 }
					}
					inside = !inside;
			  }
			  return result.ToArray();
		 }
	}

}