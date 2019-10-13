using System;
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
namespace Neo4Net.Server.rest.management.repr
{

	using MappingRepresentation = Neo4Net.Server.rest.repr.MappingRepresentation;
	using MappingSerializer = Neo4Net.Server.rest.repr.MappingSerializer;

	public class ServiceDefinitionRepresentation : MappingRepresentation
	{
		 private readonly Dictionary<string, string> _uris;
		 private readonly Dictionary<string, string> _templates;
		 private readonly string _basePath;

		 public ServiceDefinitionRepresentation( string basePath ) : base( "service-definition" )
		 {
			  this._basePath = basePath;
			  _uris = new Dictionary<string, string>();
			  _templates = new Dictionary<string, string>();
		 }

		 public virtual void ResourceUri( string name, string subPath )
		 {
			  _uris[name] = Relative( subPath );
		 }

		 public virtual void ResourceTemplate( string name, string subPath )
		 {
			  _templates[name] = Relative( subPath );
		 }

		 private string Relative( string subPath )
		 {
			  if ( _basePath.EndsWith( "/", StringComparison.Ordinal ) )
			  {
					if ( subPath.StartsWith( "/", StringComparison.Ordinal ) )
					{
						 return _basePath + subPath.Substring( 1 );
					}
			  }
			  else if ( !subPath.StartsWith( "/", StringComparison.Ordinal ) )
			  {
					return _basePath + "/" + subPath;
			  }
			  return _basePath + subPath;
		 }

		 public override void Serialize( MappingSerializer serializer )
		 {
			  serializer.PutMapping( "resources", new MappingRepresentationAnonymousInnerClass( this ) );
		 }

		 private class MappingRepresentationAnonymousInnerClass : MappingRepresentation
		 {
			 private readonly ServiceDefinitionRepresentation _outerInstance;

			 public MappingRepresentationAnonymousInnerClass( ServiceDefinitionRepresentation outerInstance ) : base( "resources" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void serialize( MappingSerializer resourceSerializer )
			 {
				  foreach ( KeyValuePair<string, string> entry in _outerInstance.uris.SetOfKeyValuePairs() )
				  {
						resourceSerializer.PutRelativeUri( entry.Key, entry.Value );
				  }

				  foreach ( KeyValuePair<string, string> entry in _outerInstance.templates.SetOfKeyValuePairs() )
				  {
						resourceSerializer.PutRelativeUriTemplate( entry.Key, entry.Value );
				  }
			 }
		 }
	}

}