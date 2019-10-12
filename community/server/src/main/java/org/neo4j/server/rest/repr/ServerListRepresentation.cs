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

	public class ServerListRepresentation : ListRepresentation
	{
		 public ServerListRepresentation<T1>( RepresentationType type, IEnumerable<T1> content ) where T1 : Representation : base( type, content )
		 {
		 }

		 protected internal override void Serialize( ListSerializer serializer )
		 {
			  foreach ( object val in Content )
			  {
					if ( val is Number )
					{
						 serializer.AddNumber( ( Number ) val );
					}
					else if ( val is string )
					{
						 serializer.AddString( ( string ) val );
					}
					else if ( val is System.Collections.IEnumerable )
					{
						 serializer.AddList( ObjectToRepresentationConverter.GetListRepresentation( ( System.Collections.IEnumerable ) val ) );
					}
					else if ( val is System.Collections.IDictionary )
					{
						 serializer.AddMapping( ObjectToRepresentationConverter.GetMapRepresentation( ( System.Collections.IDictionary ) val ) );
					}
					else if ( val is MappingRepresentation )
					{
						 serializer.AddMapping( ( MappingRepresentation ) val );
					}
					else if ( val is Representation )
					{
						 ( ( Representation ) val ).AddTo( serializer );
					}
					//default
					else
					{
						 serializer.AddString( val.ToString() );
					}
			  }
		 }
	}

}