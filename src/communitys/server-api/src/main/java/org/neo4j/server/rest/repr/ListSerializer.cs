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
namespace Neo4Net.Server.rest.repr
{

	public class ListSerializer : Serializer
	{
		 internal readonly ListWriter Writer;

		 internal ListSerializer( ListWriter writer, URI baseUri, ExtensionInjector extensions ) : base( baseUri, extensions )
		 {
			  this.Writer = writer;
		 }

		 public virtual void AddUri( string path )
		 {
			  Writer.writeValue( RepresentationType.Uri, RelativeUri( path ) );
		 }

		 public virtual void AddUriTemplate( string template )
		 {
			  Writer.writeValue( RepresentationType.Template, RelativeTemplate( template ) );
		 }

		 public virtual void AddString( string value )
		 {
			  Writer.writeString( value );
		 }

		 public virtual void AddMapping( MappingRepresentation value )
		 {
			  Serialize( Writer.newMapping( value.Type ), value );
		 }

		 public virtual void AddList( ListRepresentation value )
		 {
			  Serialize( Writer.newList( value.Type ), value );
		 }

		 public void AddNumber( Number value )
		 {
			  if ( value is double? || value is float? )
			  {
					Writer.writeFloatingPointNumber( RepresentationType.ValueOf( value.GetType() ), value.doubleValue() );
			  }
			  else
			  {
					CheckThatItIsBuiltInType( value );
					Writer.writeInteger( RepresentationType.ValueOf( value.GetType() ), value.longValue() );
			  }
		 }
	}

}