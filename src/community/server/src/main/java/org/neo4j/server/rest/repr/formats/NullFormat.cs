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



	public class NullFormat : RepresentationFormat
	{
		 private readonly ICollection<MediaType> _supported;
		 private readonly MediaType[] _requested;

		 public NullFormat( ICollection<MediaType> supported, params MediaType[] requested ) : base( null )
		 {
			  this._supported = supported;
			  this._requested = requested;
		 }

		 public override object ReadValue( string input )
		 {
			  if ( Empty( input ) )
			  {
					return null;
			  }
			  throw new MediaTypeNotSupportedException( Response.Status.UNSUPPORTED_MEDIA_TYPE, _supported, _requested );
		 }

		 public override URI ReadUri( string input )
		 {
			  if ( Empty( input ) )
			  {
					return null;
			  }
			  throw new MediaTypeNotSupportedException( Response.Status.UNSUPPORTED_MEDIA_TYPE, _supported, _requested );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, Object> readMap(String input, String... requiredKeys) throws org.neo4j.server.rest.repr.BadInputException
		 public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
		 {
			  if ( Empty( input ) )
			  {
					if ( requiredKeys.Length != 0 )
					{
						 string missingKeys = Arrays.ToString( requiredKeys );
						 throw new InvalidArgumentsException( "Missing required keys: " + missingKeys );
					}
					return Collections.emptyMap();
			  }
			  throw new MediaTypeNotSupportedException( Response.Status.UNSUPPORTED_MEDIA_TYPE, _supported, _requested );
		 }

		 public override IList<object> ReadList( string input )
		 {
			  if ( Empty( input ) )
			  {
					return Collections.emptyList();
			  }
			  throw new MediaTypeNotSupportedException( Response.Status.UNSUPPORTED_MEDIA_TYPE, _supported, _requested );
		 }

		 private bool Empty( string input )
		 {
			  return string.ReferenceEquals( input, null ) || "".Equals( input.Trim() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected String serializeValue(final String type, final Object value)
		 protected internal override string SerializeValue( string type, object value )
		 {
			  throw new MediaTypeNotSupportedException( Response.Status.NOT_ACCEPTABLE, _supported, _requested );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.ListWriter serializeList(final String type)
		 protected internal override ListWriter SerializeList( string type )
		 {
			  throw new MediaTypeNotSupportedException( Response.Status.NOT_ACCEPTABLE, _supported, _requested );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.server.rest.repr.MappingWriter serializeMapping(final String type)
		 protected internal override MappingWriter SerializeMapping( string type )
		 {
			  throw new MediaTypeNotSupportedException( Response.Status.NOT_ACCEPTABLE, _supported, _requested );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected String complete(final org.neo4j.server.rest.repr.ListWriter serializer)
		 protected internal override string Complete( ListWriter serializer )
		 {
			  throw new MediaTypeNotSupportedException( Response.Status.NOT_ACCEPTABLE, _supported, _requested );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected String complete(final org.neo4j.server.rest.repr.MappingWriter serializer)
		 protected internal override string Complete( MappingWriter serializer )
		 {
			  throw new MediaTypeNotSupportedException( Response.Status.NOT_ACCEPTABLE, _supported, _requested );
		 }
	}

}