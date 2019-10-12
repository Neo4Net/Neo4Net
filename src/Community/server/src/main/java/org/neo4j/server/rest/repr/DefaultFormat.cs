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
namespace Neo4Net.Server.rest.repr
{

	/// <summary>
	/// This class decorates another RepresentationFormat (called inner here), and
	/// tries to use inner to parse stuff. If it fails, it will throw an appropriate
	/// exception, and not just blow up with an exception that leads to HTTP STATUS
	/// 500
	/// </summary>
	public class DefaultFormat : RepresentationFormat
	{
		 private readonly RepresentationFormat _inner;
		 private readonly ICollection<MediaType> _supported;
		 private readonly MediaType[] _requested;

		 public DefaultFormat( RepresentationFormat inner, ICollection<MediaType> supported, params MediaType[] requested ) : base( MediaType.APPLICATION_JSON_TYPE )
		 {

			  this._inner = inner;
			  this._supported = supported;
			  this._requested = requested;
		 }

		 protected internal override string SerializeValue( string type, object value )
		 {
			  return _inner.serializeValue( type, value );
		 }

		 protected internal override ListWriter SerializeList( string type )
		 {
			  return _inner.serializeList( type );
		 }

		 protected internal override MappingWriter SerializeMapping( string type )
		 {
			  return _inner.serializeMapping( type );
		 }

		 protected internal override string Complete( ListWriter serializer )
		 {
			  return _inner.complete( serializer );
		 }

		 protected internal override string Complete( MappingWriter serializer )
		 {
			  return _inner.complete( serializer );
		 }

		 public override object ReadValue( string input )
		 {
			  try
			  {
					return _inner.readValue( input );
			  }
			  catch ( BadInputException )
			  {
					throw NewMediaTypeNotSupportedException();
			  }
		 }

		 private MediaTypeNotSupportedException NewMediaTypeNotSupportedException()
		 {
			  return new MediaTypeNotSupportedException( Response.Status.UNSUPPORTED_MEDIA_TYPE, _supported, _requested );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, Object> readMap(String input, String... requiredKeys) throws BadInputException
		 public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
		 {
			  IDictionary<string, object> result;
			  try
			  {
					result = _inner.readMap( input );
			  }
			  catch ( BadInputException )
			  {
					throw NewMediaTypeNotSupportedException();
			  }
			  return ValidateKeys( result, requiredKeys );
		 }

		 public override IList<object> ReadList( string input )
		 {
			  try
			  {
					return _inner.readList( input );
			  }
			  catch ( BadInputException )
			  {
					throw NewMediaTypeNotSupportedException();
			  }
		 }

		 public override URI ReadUri( string input )
		 {
			  try
			  {
					return _inner.readUri( input );
			  }
			  catch ( BadInputException )
			  {
					throw NewMediaTypeNotSupportedException();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T> java.util.Map<String, T> validateKeys(java.util.Map<String, T> map, String... requiredKeys) throws BadInputException
		 public static IDictionary<string, T> ValidateKeys<T>( IDictionary<string, T> map, params string[] requiredKeys )
		 {
			  ISet<string> missing = null;
			  foreach ( string key in requiredKeys )
			  {
					if ( !map.ContainsKey( key ) )
					{
						 if ( missing == null )
						 {
							  missing = new HashSet<string>();
						 }
						 missing.Add( key );
					}
			  }
			  if ( missing != null )
			  {
					if ( missing.Count == 1 )
					{
						 throw new InvalidArgumentsException( "Missing required key: \"" + missing.GetEnumerator().next() + "\"" );
					}
					else
					{
						 throw new InvalidArgumentsException( "Missing required keys: " + missing );
					}
			  }
			  return map;
		 }
	}

}