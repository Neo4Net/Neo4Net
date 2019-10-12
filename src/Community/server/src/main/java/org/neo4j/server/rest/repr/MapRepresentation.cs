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

	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class MapRepresentation : MappingRepresentation
	{

		 private readonly System.Collections.IDictionary _value;

		 public MapRepresentation( System.Collections.IDictionary value ) : base( RepresentationType.Map )
		 {
			  this._value = value;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  foreach ( object key in _value.Keys )
			  {
					object val = _value[key];
					string keyString = key == null ? "null" : key.ToString();
					if ( val is Number )
					{
						 serializer.PutNumber( keyString, ( Number ) val );
					}
					else if ( val is bool? )
					{
						 serializer.PutBoolean( keyString, ( bool? ) val.Value );
					}
					else if ( val is string )
					{
						 serializer.PutString( keyString, ( string ) val );
					}
					else if ( val is Path )
					{
						 PathRepresentation<Path> representation = new PathRepresentation<Path>( ( Path ) val );
						 serializer.PutMapping( keyString, representation );
					}
					else if ( val is System.Collections.IEnumerable )
					{
						 serializer.PutList( keyString, ObjectToRepresentationConverter.GetListRepresentation( ( System.Collections.IEnumerable ) val ) );
					}
					else if ( val is System.Collections.IDictionary )
					{
						 serializer.PutMapping( keyString, ObjectToRepresentationConverter.GetMapRepresentation( ( System.Collections.IDictionary ) val ) );
					}
					else if ( val == null )
					{
						 serializer.PutString( keyString, null );
					}
					else if ( val.GetType().IsArray )
					{
						 object[] objects = ToArray( val );

						 serializer.PutList( keyString, ObjectToRepresentationConverter.GetListRepresentation( asList( objects ) ) );
					}
					else if ( val is Node || val is Relationship )
					{
						 Representation representation = ObjectToRepresentationConverter.GetSingleRepresentation( val );
						 serializer.PutMapping( keyString, ( MappingRepresentation ) representation );
					}
					else
					{
						 throw new System.ArgumentException( "Unsupported value type: " + val.GetType() );
					}
			  }
		 }

		 private object[] ToArray( object val )
		 {
			  int length = getLength( val );

			  object[] objects = new object[length];

			  for ( int i = 0; i < length; i++ )
			  {
					objects[i] = get( val, i );
			  }

			  return objects;
		 }
	}

}