﻿/*
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
namespace Org.Neo4j.Server.rest.management
{

	using ServerRootRepresentation = Org.Neo4j.Server.rest.management.repr.ServerRootRepresentation;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/") public class RootService
	public class RootService
	{
		 private readonly NeoServer _neoServer;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public RootService(@Context NeoServer neoServer)
		 public RootService( NeoServer neoServer )
		 {
			  this._neoServer = neoServer;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET public javax.ws.rs.core.Response getServiceDefinition(@Context UriInfo uriInfo, @Context OutputFormat output)
		 public virtual Response GetServiceDefinition( UriInfo uriInfo, OutputFormat output )
		 {
			  ServerRootRepresentation representation = new ServerRootRepresentation( uriInfo.BaseUri, _neoServer.Services );

			  return output.ok( representation );
		 }
	}

}