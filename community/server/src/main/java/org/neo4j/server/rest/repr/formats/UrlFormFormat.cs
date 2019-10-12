using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.repr.formats
{


	public class UrlFormFormat : RepresentationFormat
	{
		 public UrlFormFormat() : base(MediaType.APPLICATION_FORM_URLENCODED_TYPE)
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected String serializeValue(final String type, final Object value)
		 protected internal override string SerializeValue( string type, object value )
		 {
			  throw new Exception( "Not implemented!" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.ListWriter serializeList(final String type)
		 protected internal override ListWriter SerializeList( string type )
		 {
			  throw new Exception( "Not implemented!" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.MappingWriter serializeMapping(final String type)
		 protected internal override MappingWriter SerializeMapping( string type )
		 {
			  throw new Exception( "Not implemented!" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected String complete(final org.neo4j.server.rest.repr.ListWriter serializer)
		 protected internal override string Complete( ListWriter serializer )
		 {
			  throw new Exception( "Not implemented!" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected String complete(final org.neo4j.server.rest.repr.MappingWriter serializer)
		 protected internal override string Complete( MappingWriter serializer )
		 {
			  throw new Exception( "Not implemented!" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Object readValue(final String input)
		 public override object ReadValue( string input )
		 {
			  throw new Exception( "Not implemented!" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, Object> readMap(final String input, String... requiredKeys) throws org.neo4j.server.rest.repr.BadInputException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
		 {
			  Dictionary<string, object> result = new Dictionary<string, object>();
			  if ( input.Length == 0 )
			  {
					return result;
			  }

			  foreach ( string pair in input.Split( "&", true ) )
			  {
					string[] fields = pair.Split( "=", true );
					string key;
					string value;

					try
					{
						 string charset = StandardCharsets.UTF_8.name();
						 key = EnsureThatKeyDoesNotHavePhPStyleParenthesesAtTheEnd( URLDecoder.decode( fields[0], charset ) );
						 value = URLDecoder.decode( fields[1], charset );
					}
					catch ( UnsupportedEncodingException e )
					{
						 throw new BadInputException( e );
					}

					object old = result[key];
					if ( old == null )
					{
						 result[key] = value;
					}
					else
					{
						 IList<object> list;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (old instanceof java.util.List<?>)
						 if ( old is IList<object> )
						 {
							  list = ( IList<object> ) old;
						 }
						 else
						 {
							  list = new List<object>();
							  result[key] = list;
							  list.Add( old );
						 }
						 list.Add( value );
					}
			  }

			  return DefaultFormat.validateKeys( result, requiredKeys );
		 }

		 private string EnsureThatKeyDoesNotHavePhPStyleParenthesesAtTheEnd( string key )
		 {
			  if ( key.EndsWith( "[]", StringComparison.Ordinal ) )
			  {
					return key.Substring( 0, key.Length - 2 );
			  }
			  return key;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.List<Object> readList(final String input)
		 public override IList<object> ReadList( string input )
		 {
			  throw new Exception( "Not implemented!" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.net.URI readUri(final String input)
		 public override URI ReadUri( string input )
		 {
			  throw new Exception( "Not implemented!" );
		 }
	}

}