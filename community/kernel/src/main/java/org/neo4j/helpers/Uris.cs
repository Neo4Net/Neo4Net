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
namespace Org.Neo4j.Helpers
{

	/// <summary>
	/// Functions for working with URIs
	/// </summary>
	[Obsolete]
	public sealed class Uris
	{
		 /// <summary>
		 /// Extract a named parameter from the query of a URI. If a parameter is set but no value defined,
		 /// then "true" is returned
		 /// </summary>
		 /// <param name="name"> of the parameter </param>
		 /// <returns> value of named parameter or null if missing </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<java.net.URI, String> parameter(final String name)
		 [Obsolete]
		 public static System.Func<URI, string> Parameter( string name )
		 {
			  return uri =>
			  {
				if ( uri == null )
				{
					 return null;
				}

				string query = uri.Query;
				if ( !string.ReferenceEquals( query, null ) )
				{
					 foreach ( string param in query.Split( "&", true ) )
					 {
						  string[] keyValue = param.Split( "=", true );

						  if ( keyValue[0].Equals( name, StringComparison.OrdinalIgnoreCase ) )
						  {
								if ( keyValue.Length == 2 )
								{
									 return keyValue[1];
								}
								else
								{
									 return "true";
								}
						  }
					 }
				}
				return null;
			  };
		 }

		 private Uris()
		 {
		 }
	}

}