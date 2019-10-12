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
namespace Org.Neo4j.Kernel.impl.proc
{

	/// <summary>
	/// Contains simple parsing utils for parsing Cypher lists, maps and values.
	/// 
	/// The methods here are not very optimized and are deliberately simple. If you find yourself using these method
	/// for parsing big json documents you should probably rethink your choice.
	/// </summary>
	public sealed class ParseUtil
	{
		 private ParseUtil()
		 {
			  throw new System.NotSupportedException( "Do not instantiate" );
		 }

		 internal static IDictionary<string, object> ParseMap( string s )
		 {
			  int pos = 0;
			  int braceCounter = 0;
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  StringBuilder builder = new StringBuilder();
			  bool inList = false;
			  while ( pos < s.Length )
			  {

					char character = s[pos];
					switch ( character )
					{
					case ' ':
						 ++pos;
						 break;
					case '{':
						 if ( braceCounter++ > 0 )
						 {
							  builder.Append( s[pos] );
						 }
						 ++pos;
						 break;
					case ',':
						 if ( !inList && braceCounter == 1 )
						 {
							  AddKeyValue( map, builder.ToString().Trim() );
							  builder = new StringBuilder();
						 }
						 else
						 {
							  builder.Append( s[pos] );
						 }
						 ++pos;
						 break;
					case '}':
						 if ( --braceCounter == 0 )
						 {
							  AddKeyValue( map, builder.ToString().Trim() );
						 }
						 else
						 {
							  builder.Append( s[pos] );
						 }
						 ++pos;
						 break;
					case '[':
						 inList = true;
						 builder.Append( s[pos++] );
						 break;
					case ']':
						 inList = false;
						 builder.Append( s[pos++] );
						 break;
					default:
						 builder.Append( s[pos++] );
						 break;
					}
			  }
			  if ( braceCounter != 0 )
			  {
					throw new System.ArgumentException( string.Format( "{0} contains unbalanced '{{', '}}'.", s ) );
			  }

			  return map;
		 }

		 private static void AddKeyValue( IDictionary<string, object> map, string keyValue )
		 {
			  if ( keyValue.Length == 0 )
			  {
					return;
			  }
			  int split = keyValue.IndexOf( ':' );
			  if ( split < 0 )
			  {
					throw new System.ArgumentException( "Keys and values must be separated with ':'" );
			  }
			  string key = ParseKey( keyValue.Substring( 0, split ).Trim() );
			  object value = ParseValue( keyValue.Substring( split + 1 ).Trim(), typeof(object) );

			  if ( map.ContainsKey( key ) )
			  {
					throw new System.ArgumentException( string.Format( "Multiple occurrences of key '{0}'", key ) );
			  }
			  map[key] = value;
		 }

		 private static string ParseKey( string s )
		 {
			  int pos = 0;
			  while ( pos < s.Length )
			  {
					char c = s[pos];
					switch ( c )
					{
					case '\'':
					case '\"':
						 ++pos;
						 break;
					default:
						 return s.Substring( pos, ( s.Length - pos ) - pos );
					}
			  }

			  throw new System.ArgumentException( "" );
		 }

		 /// <summary>
		 /// Parses value into object. Make sure you call trim on the string
		 /// before calling this method. The type is used for type checking lists.
		 /// </summary>
		 private static object ParseValue( string s, Type type )
		 {
			  int pos = 0;
			  while ( pos < s.Length )
			  {
					char c = s[pos];
					int closing;
					switch ( c )
					{
					case ' ':
						 ++pos;
						 break;
					case '\'':
						 closing = s.LastIndexOf( '\'' );
						 if ( closing < 0 )
						 {
							  throw new System.ArgumentException( "Did not find a matching end quote, '" );
						 }
						 return s.Substring( pos + 1, closing - ( pos + 1 ) );
					case '\"':
						 closing = s.LastIndexOf( '\"' );
						 if ( closing < 0 )
						 {
							  throw new System.ArgumentException( "Did not find a matching end quote, \"" );
						 }
						 return s.Substring( pos + 1, closing - ( pos + 1 ) );
					case '{':
						 return ParseMap( s.Substring( pos ) );
					case '[':
						 if ( type is ParameterizedType )
						 {
							  return ParseList( s.Substring( pos ), ( ( ParameterizedType ) type ).ActualTypeArguments[0] );
						 }
						 else
						 {
							  return ParseList( s.Substring( pos ), typeof( object ) );
						 }

					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						 string number = s.Substring( pos );
						 try
						 {
							  return Convert.ToInt64( number );
						 }
						 catch ( System.FormatException )
						 {
							  return Convert.ToDouble( number );
						 }

						 //deliberate fallthrough
					case 'n':
						 if ( s[pos + 1] == 'u' && s[pos + 2] == 'l' && s[pos + 3] == 'l' )
						 {
							  return null;
						 }

					case 't':
						 if ( s[pos + 1] == 'r' && s[pos + 2] == 'u' && s[pos + 3] == 'e' )
						 {
							  return true;
						 }
					case 'f':
						 if ( s[pos + 1] == 'a' && s[pos + 2] == 'l' && s[pos + 3] == 's' && s[pos + 4] == 'e' )
						 {
							  return false;
						 }

					default:
						 throw new System.ArgumentException( string.Format( "{0} is not a valid value", s ) );
					}
			  }

			  throw new System.ArgumentException( string.Format( "{0} is not a valid value", s ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static <T> java.util.List<T> parseList(String s, Type type)
		 internal static IList<T> ParseList<T>( string s, Type type )
		 {
			  int pos = 0;
			  int braceCounter = 0;
			  IList<object> list = new List<object>();
			  StringBuilder builder = new StringBuilder();
			  while ( pos < s.Length )
			  {

					char character = s[pos];
					switch ( character )
					{
					case ' ':
						 ++pos;
						 break;
					case '[':
						 if ( braceCounter++ > 0 )
						 {
							  builder.Append( s[pos] );
						 }
						 ++pos;
						 break;
					case ',':
						 if ( braceCounter == 1 )
						 {
							  object o = ParseValue( builder.ToString().Trim(), type );
							  AssertType( o, type );
							  list.Add( o );
							  builder = new StringBuilder();
						 }
						 else
						 {
							  builder.Append( s[pos] );
						 }
						 ++pos;
						 break;
					case ']':
						 if ( --braceCounter == 0 )
						 {
							  string value = builder.ToString().Trim();
							  if ( value.Length > 0 )
							  {

									object o = ParseValue( value, type );
									AssertType( o, type );

									list.Add( o );
							  }
						 }
						 else
						 {
							  builder.Append( s[pos] );
						 }
						 ++pos;
						 break;
					default:
						 builder.Append( s[pos++] );
						 break;
					}
			  }
			  if ( braceCounter != 0 )
			  {
					throw new System.ArgumentException( string.Format( "{0} contains unbalanced '[', ']'.", s ) );
			  }

			  return ( IList<T> ) list;
		 }

		 private static void AssertType( object obj, Type type )
		 {
			  if ( obj == null )
			  {
					return;
			  }
			  //Since type erasure has already happened here we cannot verify ParameterizedType
			  if ( type is Type )
			  {
					Type clazz = ( Type ) type;
					if ( !clazz.IsAssignableFrom( obj.GetType() ) )
					{
						 throw new System.ArgumentException( string.Format( "Expects a list of {0} but got a list of {1}", clazz.Name, obj.GetType().Name ) );
					}

			  }
		 }
	}

}