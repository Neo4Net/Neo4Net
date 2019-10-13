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
namespace Neo4Net.Server.rest.repr.formats
{

	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.assertSupportedPropertyValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.readJson;

	public class JsonFormat : RepresentationFormat
	{
		 public JsonFormat() : base(MediaType.APPLICATION_JSON_TYPE)
		 {
		 }

		 protected internal override ListWriter SerializeList( string type )
		 {
			  return new ListWrappingWriter( new List<object>() );
		 }

		 protected internal override string Complete( ListWriter serializer )
		 {
			  return JsonHelper.createJsonFrom( ( ( ListWrappingWriter ) serializer ).Data );
		 }

		 protected internal override MappingWriter SerializeMapping( string type )
		 {
			  return new MapWrappingWriter( new LinkedHashMap<string, object>() );
		 }

		 protected internal override string Complete( MappingWriter serializer )
		 {
			  return JsonHelper.createJsonFrom( ( ( MapWrappingWriter ) serializer ).Data );
		 }

		 protected internal override string SerializeValue( string type, object value )
		 {
			  return JsonHelper.createJsonFrom( value );
		 }

		 private bool Empty( string input )
		 {
			  return string.ReferenceEquals( input, null ) || "".Equals( input.Trim() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, Object> readMap(String input, String... requiredKeys) throws org.neo4j.server.rest.repr.BadInputException
		 public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
		 {
			  if ( Empty( input ) )
			  {
					return DefaultFormat.validateKeys( Collections.emptyMap(), requiredKeys );
			  }
			  try
			  {
					return DefaultFormat.validateKeys( JsonHelper.jsonToMap( StripByteOrderMark( input ) ), requiredKeys );
			  }
			  catch ( Exception ex )
			  {
					throw new BadInputException( ex );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public java.util.List<Object> readList(String input) throws org.neo4j.server.rest.repr.BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override IList<object> ReadList( string input )
		 {
			  try
			  {
					return ( IList<object> ) JsonHelper.readJson( input );
			  }
			  catch ( Exception ex ) when ( ex is System.InvalidCastException || ex is JsonParseException )
			  {
					throw new BadInputException( ex );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object readValue(String input) throws org.neo4j.server.rest.repr.BadInputException
		 public override object ReadValue( string input )
		 {
			  if ( Empty( input ) )
			  {
					return Collections.emptyMap();
			  }
			  try
			  {
					return assertSupportedPropertyValue( readJson( StripByteOrderMark( input ) ) );
			  }
			  catch ( JsonParseException ex )
			  {
					throw new BadInputException( ex );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URI readUri(String input) throws org.neo4j.server.rest.repr.BadInputException
		 public override URI ReadUri( string input )
		 {
			  try
			  {
					return new URI( ReadValue( input ).ToString() );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new BadInputException( e );
			  }
		 }

		 private string StripByteOrderMark( string @string )
		 {
			  if ( !string.ReferenceEquals( @string, null ) && @string.Length > 0 && @string[0] == ( char )0xfeff )
			  {
					return @string.Substring( 1 );
			  }
			  return @string;
		 }
	}

}