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

	public sealed class ServerExtensionRepresentation : MappingRepresentation
	{
		 private readonly IDictionary<string, EntityExtensionRepresentation> _extended;

		 public ServerExtensionRepresentation( string name, IList<ExtensionPointRepresentation> methods ) : base( RepresentationType.ServerPluginDescription )
		 {
			  this._extended = new Dictionary<string, EntityExtensionRepresentation>();
			  foreach ( ExtensionPointRepresentation extension in methods )
			  {
					EntityExtensionRepresentation entity = _extended.computeIfAbsent( extension.ExtendedEntity, k => new EntityExtensionRepresentation() );
					entity.Add( extension );
			  }
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  foreach ( KeyValuePair<string, EntityExtensionRepresentation> entity in _extended.SetOfKeyValuePairs() )
			  {
					serializer.PutMapping( entity.Key, entity.Value );
			  }
		 }

		 private class EntityExtensionRepresentation : MappingRepresentation
		 {
			  internal readonly IList<ExtensionPointRepresentation> Extensions;

			  internal EntityExtensionRepresentation() : base("entity-extensions")
			  {
					this.Extensions = new List<ExtensionPointRepresentation>();
			  }

			  internal virtual void Add( ExtensionPointRepresentation extension )
			  {
					Extensions.Add( extension );
			  }

			  protected internal override void Serialize( MappingSerializer serializer )
			  {
					foreach ( ExtensionPointRepresentation extension in Extensions )
					{
						 serializer.PutMapping( extension.Name, extension );
					}
			  }
		 }
	}

}