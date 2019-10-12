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
namespace Org.Neo4j.Server.rest.management.repr
{

	using MappingRepresentation = Org.Neo4j.Server.rest.repr.MappingRepresentation;
	using MappingSerializer = Org.Neo4j.Server.rest.repr.MappingSerializer;

	public class ServerRootRepresentation : MappingRepresentation
	{
		 private Dictionary<string, string> _services = new Dictionary<string, string>();

		 public ServerRootRepresentation( URI baseUri, IEnumerable<AdvertisableService> advertisableServices ) : base( "services" )
		 {
			  foreach ( AdvertisableService svc in advertisableServices )
			  {
					_services[svc.Name] = baseUri.ToString() + svc.ServerPath;
			  }
		 }

		 public virtual IDictionary<string, IDictionary<string, string>> Serialize()
		 {
			  Dictionary<string, IDictionary<string, string>> result = new Dictionary<string, IDictionary<string, string>>();
			  result["services"] = _services;
			  return result;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  MappingRepresentation apa = new MappingRepresentationAnonymousInnerClass( this, serializer );

			  serializer.PutMapping( "services", apa );
		 }

		 private class MappingRepresentationAnonymousInnerClass : MappingRepresentation
		 {
			 private readonly ServerRootRepresentation _outerInstance;

			 private MappingSerializer _serializer;

			 public MappingRepresentationAnonymousInnerClass( ServerRootRepresentation outerInstance, MappingSerializer serializer ) : base( "services" )
			 {
				 this.outerInstance = outerInstance;
				 this._serializer = serializer;
			 }


			 protected internal override void serialize( MappingSerializer serializer )
			 {
				  foreach ( KeyValuePair<string, string> entry in _outerInstance.services.SetOfKeyValuePairs() )
				  {
						serializer.PutString( entry.Key, entry.Value );
				  }
			 }
		 }
	}

}