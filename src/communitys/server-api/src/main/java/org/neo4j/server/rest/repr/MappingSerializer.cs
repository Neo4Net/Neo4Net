using System;

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

	public class MappingSerializer : Serializer
	{
		 internal readonly MappingWriter Writer;

		 internal MappingSerializer( MappingWriter writer, URI baseUri, ExtensionInjector extensions ) : base( baseUri, extensions )
		 {
			  this.Writer = writer;
		 }

		 /// @deprecated please use <seealso cref="putAbsoluteUri(string, URI)"/> 
		 [Obsolete("please use <seealso cref=\"putAbsoluteUri(string, URI)\"/>")]
		 internal virtual void PutAbsoluteUri( string key, string path )
		 {
			  Writer.writeValue( RepresentationType.Uri, key, path );
		 }

		 internal virtual void PutAbsoluteUri( string key, URI path )
		 {
			  Writer.writeValue( RepresentationType.Uri, key, path.toASCIIString() );
		 }

		 public virtual void PutRelativeUri( string key, string path )
		 {
			  Writer.writeValue( RepresentationType.Uri, key, RelativeUri( path ) );
		 }

		 public virtual void PutRelativeUriTemplate( string key, string template )
		 {
			  Writer.writeValue( RepresentationType.Template, key, RelativeTemplate( template ) );
		 }

		 public virtual void PutString( string key, string value )
		 {
			  Writer.writeString( key, value );
		 }

		 internal virtual void PutBoolean( string key, bool value )
		 {
			  Writer.writeBoolean( key, value );
		 }

		 public virtual void PutMapping( string key, MappingRepresentation value )
		 {
			  Serialize( Writer.newMapping( value.Type, key ), value );
		 }

		 public virtual void PutList( string key, ListRepresentation value )
		 {
			  Serialize( Writer.newList( value.Type, key ), value );
		 }

		 internal void PutNumber( string key, Number value )
		 {
			  if ( value is double? || value is float? )
			  {
					Writer.writeFloatingPointNumber( RepresentationType.ValueOf( value.GetType() ), key, value.doubleValue() );
			  }
			  else
			  {
					CheckThatItIsBuiltInType( value );
					Writer.writeInteger( RepresentationType.ValueOf( value.GetType() ), key, value.longValue() );
			  }
		 }
	}

}