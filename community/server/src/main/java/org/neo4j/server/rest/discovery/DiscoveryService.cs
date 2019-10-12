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
namespace Org.Neo4j.Server.rest.discovery
{

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using DiscoveryRepresentation = Org.Neo4j.Server.rest.repr.DiscoveryRepresentation;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;

	/// <summary>
	/// Used to discover the rest of the server URIs through a HTTP GET request to
	/// the server root (/).
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/") public class DiscoveryService
	public class DiscoveryService
	{
		 private readonly Config _config;
		 private readonly OutputFormat _outputFormat;
		 private readonly DiscoverableURIs _uris;

		 // Your IDE might tell you to make this less visible than public. Don't. JAX-RS demands is to be public.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public DiscoveryService(@Context Config config, @Context OutputFormat outputFormat, @Context DiscoverableURIs uris)
		 public DiscoveryService( Config config, OutputFormat outputFormat, DiscoverableURIs uris )
		 {
			  this._config = config;
			  this._outputFormat = outputFormat;
			  this._uris = uris;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Produces(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response getDiscoveryDocument(@Context UriInfo uriInfo)
		 public virtual Response GetDiscoveryDocument( UriInfo uriInfo )
		 {
			  return _outputFormat.ok( new DiscoveryRepresentation( ( new DiscoverableURIs.Builder( _uris ) ).overrideAbsolutesFromRequest( uriInfo.BaseUri ).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Produces(javax.ws.rs.core.MediaType.WILDCARD) public javax.ws.rs.core.Response redirectToBrowser()
		 public virtual Response RedirectToBrowser()
		 {
			  return _outputFormat.seeOther( _config.get( ServerSettings.browser_path ) );
		 }
	}

}