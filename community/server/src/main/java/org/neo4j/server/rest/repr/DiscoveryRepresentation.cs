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
	using DiscoverableURIs = Org.Neo4j.Server.rest.discovery.DiscoverableURIs;

	public class DiscoveryRepresentation : MappingRepresentation
	{
		 private const string DISCOVERY_REPRESENTATION_TYPE = "discovery";
		 private readonly DiscoverableURIs _uris;

		 /// <param name="uris"> URIs that we want to make publicly discoverable. </param>
		 public DiscoveryRepresentation( DiscoverableURIs uris ) : base( DISCOVERY_REPRESENTATION_TYPE )
		 {
			  this._uris = uris;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  _uris.forEach((key, uri) =>
			  {
				if ( uri.Absolute )
				{
					 serializer.putAbsoluteUri( key, uri );
				}
				else
				{
					 serializer.PutRelativeUri( key, uri.ToString() );
				}
			  });
		 }
	}

}