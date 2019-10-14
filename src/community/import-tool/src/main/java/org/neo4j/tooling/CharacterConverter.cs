using System;

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
namespace Neo4Net.Tooling
{

	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration;

	/// <summary>
	/// Converts a string expression into a character to be used as delimiter, array delimiter, or quote character. Can be
	/// normal characters as well as for example: '\t', '\123', and "TAB".
	/// </summary>
	internal class CharacterConverter : System.Func<string, char>
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<char> apply(String value) throws RuntimeException
		 public override char? Apply( string value )
		 {
			  // Parse "raw" ASCII character style characters:
			  // - \123 --> character with id 123
			  // - \t   --> tab character
			  if ( value.StartsWith( "\\", StringComparison.Ordinal ) && value.Length > 1 )
			  {
					string raw = value.Substring( 1 );
					try
					{
						 return ( char ) int.Parse( raw );
					}
					catch ( System.FormatException )
					{
						 if ( raw.Equals( "t" ) )
						 {
							  return Configuration.TABS.delimiter();
						 }
					}
			  }
			  // hard coded TAB --> tab character
			  else if ( value.Equals( "TAB" ) )
			  {
					return Configuration.TABS.delimiter();
			  }
			  else if ( value.Length == 1 )
			  {
					return value[0];
			  }

			  throw new System.ArgumentException( "Unsupported character '" + value + "'" );
		 }
	}

}