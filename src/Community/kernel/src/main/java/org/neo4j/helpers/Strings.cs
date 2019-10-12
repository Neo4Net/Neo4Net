using System;
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
namespace Neo4Net.Helpers
{

	/// <summary>
	/// Helper functions for working with strings.
	/// </summary>
	[Obsolete]
	public sealed class Strings
	{
		 [Obsolete]
		 public const string TAB = "\t";

		 private Strings()
		 {
		 }

		 [Obsolete]
		 public static string PrettyPrint( object o )
		 {
			  if ( o == null )
			  {
					return "null";
			  }

			  Type clazz = o.GetType();
			  if ( clazz.IsArray )
			  {
					if ( clazz == typeof( sbyte[] ) )
					{
						 return Arrays.ToString( ( sbyte[] ) o );
					}
					else if ( clazz == typeof( short[] ) )
					{
						 return Arrays.ToString( ( short[] ) o );
					}
					else if ( clazz == typeof( int[] ) )
					{
						 return Arrays.ToString( ( int[] ) o );
					}
					else if ( clazz == typeof( long[] ) )
					{
						 return Arrays.ToString( ( long[] ) o );
					}
					else if ( clazz == typeof( float[] ) )
					{
						 return Arrays.ToString( ( float[] ) o );
					}
					else if ( clazz == typeof( double[] ) )
					{
						 return Arrays.ToString( ( double[] ) o );
					}
					else if ( clazz == typeof( char[] ) )
					{
						 return Arrays.ToString( ( char[] ) o );
					}
					else if ( clazz == typeof( bool[] ) )
					{
						 return Arrays.ToString( ( bool[] ) o );
					}
					else
					{
						 return Arrays.deepToString( ( object[] ) o );
					}
			  }
			  else
			  {
					return o.ToString();
			  }
		 }

		 [Obsolete]
		 public static string Escape( string arg )
		 {
			  StringBuilder builder = new StringBuilder( arg.Length );
			  try
			  {
					Escape( builder, arg );
			  }
			  catch ( IOException e )
			  {
					throw new AssertionError( "IOException from using StringBuilder", e );
			  }
			  return builder.ToString();
		 }

		 /// <summary>
		 /// Joining independent lines from provided elements into one line with <seealso cref="java.lang.System.lineSeparator"/> after
		 /// each element </summary>
		 /// <param name="elements"> - lines to join </param>
		 /// <returns> joined line </returns>
		 [Obsolete]
		 public static string JoinAsLines( params string[] elements )
		 {
			  StringBuilder result = new StringBuilder();
			  foreach ( string line in elements )
			  {
					result.Append( line ).Append( Environment.NewLine );
			  }
			  return result.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void escape(Appendable output, String arg) throws java.io.IOException
		 [Obsolete]
		 public static void Escape( Appendable output, string arg )
		 {
			  int len = arg.Length;
			  for ( int i = 0; i < len; i++ )
			  {
					char ch = arg[i];
					switch ( ch )
					{
						 case '"':
							  output.append( "\\\"" );
							  break;

						 case '\'':
							  output.append( "\\\'" );
							  break;

						 case '\\':
							  output.append( "\\\\" );
							  break;

						 case '\n':
							  output.append( "\\n" );
							  break;

						 case '\t':
							  output.append( "\\t" );
							  break;

						 case '\r':
							  output.append( "\\r" );
							  break;

						 case '\b':
							  output.append( "\\b" );
							  break;

						 case '\f':
							  output.append( "\\f" );
							  break;

						 default:
							  output.append( ch );
							  break;
					}
			  }
		 }
	}

}