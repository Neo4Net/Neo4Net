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

	public abstract class MappingRepresentation : Representation
	{
		 internal MappingRepresentation( RepresentationType type ) : base( type )
		 {
		 }

		 public MappingRepresentation( string type ) : base( type )
		 {
		 }

		 internal override string Serialize( RepresentationFormat format, URI baseUri, ExtensionInjector extensions )
		 {
			  MappingWriter writer = format.SerializeMapping( Type );
			  Serializer.InjectExtensions( writer, this, baseUri, extensions );
			  Serialize( new MappingSerializer( writer, baseUri, extensions ) );
			  writer.Done();
			  return format.Complete( writer );
		 }

		 protected internal abstract void Serialize( MappingSerializer serializer );

		 internal override void AddTo( ListSerializer serializer )
		 {
			  serializer.AddMapping( this );
		 }

		 internal override void PutTo( MappingSerializer serializer, string key )
		 {
			  serializer.PutMapping( key, this );
		 }
	}

}