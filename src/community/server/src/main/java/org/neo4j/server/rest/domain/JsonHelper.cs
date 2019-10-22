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
namespace Neo4Net.Server.rest.domain
{
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;
	using JsonLocation = org.codehaus.jackson.JsonLocation;
	using JsonNode = org.codehaus.jackson.JsonNode;
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;


	using PropertyValueException = Neo4Net.Server.rest.web.PropertyValueException;

	public class JsonHelper
	{
		 internal static readonly ObjectMapper ObjectMapper = new ObjectMapper();

		 private JsonHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.codehaus.jackson.JsonNode jsonNode(String json) throws JsonParseException
		 public static JsonNode JsonNode( string json )
		 {
			  try
			  {
					return ObjectMapper.readTree( json );
			  }
			  catch ( IOException e )
			  {
					throw new JsonParseException( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static java.util.Map<String, Object> jsonToMap(String json) throws JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static IDictionary<string, object> JsonToMap( string json )
		 {
			  return ( IDictionary<string, object> ) ReadJson( json );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static java.util.List<java.util.Map<String, Object>> jsonToList(String json) throws JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static IList<IDictionary<string, object>> JsonToList( string json )
		 {
			  return ( IList<IDictionary<string, object>> ) ReadJson( json );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static Object readJson(String json) throws JsonParseException
		 public static object ReadJson( string json )
		 {
			  try
			  {
					return ObjectMapper.readValue( json, typeof( object ) );
			  }
			  catch ( org.codehaus.jackson.JsonParseException e )
			  {
					string message = e.Message.Split( "\\r?\\n" )[0];
					JsonLocation location = e.Location;
					throw new JsonParseException( string.Format( "{0} [line: {1:D}, column: {2:D}]", message, location.LineNr, location.ColumnNr ), e );
			  }
			  catch ( IOException e )
			  {
					throw new JsonParseException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static Object assertSupportedPropertyValue(Object jsonObject) throws org.Neo4Net.server.rest.web.PropertyValueException
		 public static object AssertSupportedPropertyValue( object jsonObject )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (jsonObject instanceof java.util.Collection<?>)
			  if ( jsonObject is ICollection<object> )
			  {
					return jsonObject;
			  }
			  if ( jsonObject == null )
			  {
					throw new PropertyValueException( "null value not supported" );
			  }
			  if ( !( jsonObject is string || jsonObject is Number || jsonObject is bool? ) )
			  {
					throw new PropertyValueException( "Unsupported value type " + jsonObject.GetType() + "." + " Supported value types are all java primitives (byte, char, short, int, " + "long, float, double) and String, as well as arrays of all those types" );
			  }
			  return jsonObject;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String createJsonFrom(Object data) throws JsonBuildRuntimeException
		 public static string CreateJsonFrom( object data )
		 {
			  try
			  {
					StringWriter writer = new StringWriter();
					try
					{
						 JsonGenerator generator = ObjectMapper.JsonFactory.createJsonGenerator( writer ).useDefaultPrettyPrinter();
						 WriteValue( generator, data );
					}
					finally
					{
						 writer.close();
					}
					return writer.Buffer.ToString();
			  }
			  catch ( IOException e )
			  {
					throw new JsonBuildRuntimeException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeValue(org.codehaus.jackson.JsonGenerator jgen, Object value) throws java.io.IOException
		 public static void WriteValue( JsonGenerator jgen, object value )
		 {
			  ObjectMapper.writeValue( jgen, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String prettyPrint(Object item) throws java.io.IOException
		 public static string PrettyPrint( object item )
		 {
			  return ObjectMapper.writer().withDefaultPrettyPrinter().writeValueAsString(item);
		 }
	}

}