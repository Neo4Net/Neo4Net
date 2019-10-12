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
namespace Org.Neo4j.Server.rest
{
	using Gson = com.google.gson.Gson;
	using GsonBuilder = com.google.gson.GsonBuilder;
	using JsonElement = com.google.gson.JsonElement;
	using JsonParser = com.google.gson.JsonParser;
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;
	using ObjectWriter = org.codehaus.jackson.map.ObjectWriter;

	/*
	 * Naive implementation of a JSON prettifier.
	 */
	public class JSONPrettifier
	{
		 private static readonly Gson _gson = new GsonBuilder().setPrettyPrinting().create();
		 private static readonly JsonParser _jsonParser = new JsonParser();
		 private static readonly ObjectMapper _mapper = new ObjectMapper();
		 private static readonly ObjectWriter _writer = _mapper.writerWithDefaultPrettyPrinter();

		 private JSONPrettifier()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static String parse(final String json)
		 public static string Parse( string json )
		 {
			  if ( string.ReferenceEquals( json, null ) )
			  {
					return "";
			  }

			  string result = json;

			  try
			  {
					if ( json.Contains( "\"exception\"" ) )
					{
						 // the gson renderer is much better for stacktraces
						 result = GsonPrettyPrint( json );
					}
					else
					{
						 result = JacksonPrettyPrint( json );
					}
			  }
			  catch ( Exception )
			  {
					/*
					* Enable the output to see where exceptions happen.
					* We need to be able to tell the rest docs tools to expect
					* a json parsing error from here, then we can simply throw an exception instead.
					* (we have tests sending in broken json to test the response)
					*/
					// System.out.println( "***************************************" );
					// System.out.println( json );
					// System.out.println( "***************************************" );
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static String gsonPrettyPrint(final String json)
		 private static string GsonPrettyPrint( string json )
		 {
			  JsonElement element = _jsonParser.parse( json );
			  return _gson.toJson( element );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String jacksonPrettyPrint(final String json) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private static string JacksonPrettyPrint( string json )
		 {
			  object myObject = _mapper.readValue( json, typeof( object ) );
			  return _writer.writeValueAsString( myObject );
		 }
	}

}