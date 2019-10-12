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
namespace Org.Neo4j.Server.rest.repr
{

	using ListWrappingWriter = Org.Neo4j.Server.rest.repr.formats.ListWrappingWriter;
	using MapWrappingWriter = Org.Neo4j.Server.rest.repr.formats.MapWrappingWriter;

	public class RepresentationTestAccess
	{
		 private static readonly URI _baseUri = URI.create( "http://neo4j.org/" );

		 private RepresentationTestAccess()
		 {
		 }

		 public static object Serialize( Representation repr )
		 {
			  if ( repr is ValueRepresentation )
			  {
					return Serialize( ( ValueRepresentation ) repr );
			  }
			  else if ( repr is MappingRepresentation )
			  {
					return Serialize( ( MappingRepresentation ) repr );
			  }
			  else if ( repr is ListRepresentation )
			  {
					return Serialize( ( ListRepresentation ) repr );
			  }
			  else
			  {
					throw new System.ArgumentException( repr.GetType().ToString() );
			  }
		 }

		 public static string Serialize( ValueRepresentation repr )
		 {
			  return Serialize( _baseUri, repr );
		 }

		 public static string Serialize( URI baseUri, ValueRepresentation repr )
		 {
			  return repr.Serialize( new StringFormat(), baseUri, null );
		 }

		 public static IDictionary<string, object> Serialize( MappingRepresentation repr )
		 {
			  return Serialize( _baseUri, repr );
		 }

		 public static IDictionary<string, object> Serialize( URI baseUri, MappingRepresentation repr )
		 {
			  IDictionary<string, object> result = new Dictionary<string, object>();
			  repr.Serialize( new MappingSerializer( new MapWrappingWriter( result ), baseUri, null ) );
			  return result;
		 }

		 public static IList<object> Serialize( ListRepresentation repr )
		 {
			  return Serialize( _baseUri, repr );
		 }

		 public static IList<object> Serialize( URI baseUri, ListRepresentation repr )
		 {
			  IList<object> result = new List<object>();
			  repr.Serialize( new ListSerializer( new ListWrappingWriter( result ), baseUri, null ) );
			  return result;
		 }

		 public static long NodeUriToId( string nodeUri )
		 {
			  int lastSlash = nodeUri.LastIndexOf( '/' );
			  if ( lastSlash == -1 )
			  {
					throw new System.ArgumentException( "'" + nodeUri + "' isn't a node URI" );
			  }
			  return long.Parse( nodeUri.Substring( lastSlash + 1 ) );
		 }

		 private class StringFormat : RepresentationFormat
		 {
			  internal StringFormat() : base(MediaType.WILDCARD_TYPE)
			  {
			  }

			  protected internal override string SerializeValue( string type, object value )
			  {
					return value.ToString();
			  }

			  protected internal override string Complete( ListWriter serializer )
			  {
					throw new System.NotSupportedException( "StringFormat.complete(ListWriter)" );
			  }

			  protected internal override string Complete( MappingWriter serializer )
			  {
					throw new System.NotSupportedException( "StringFormat.complete(MappingWriter)" );
			  }

			  protected internal override ListWriter SerializeList( string type )
			  {
					throw new System.NotSupportedException( "StringFormat.serializeList()" );
			  }

			  protected internal override MappingWriter SerializeMapping( string type )
			  {
					throw new System.NotSupportedException( "StringFormat.serializeMapping()" );
			  }

			  public override IList<object> ReadList( string input )
			  {
					throw new System.NotSupportedException( "StringFormat.readList()" );
			  }

			  public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
			  {
					throw new System.NotSupportedException( "StringFormat.readMap()" );
			  }

			  public override object ReadValue( string input )
			  {
					throw new System.NotSupportedException( "StringFormat.readValue()" );
			  }

			  public override URI ReadUri( string input )
			  {
					throw new System.NotSupportedException( "StringFormat.readUri()" );
			  }
		 }
	}

}