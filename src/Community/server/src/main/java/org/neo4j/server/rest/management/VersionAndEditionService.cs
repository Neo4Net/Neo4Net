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
namespace Neo4Net.Server.rest.management
{

	using KernelData = Neo4Net.Kernel.@internal.KernelData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.createJsonFrom;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path(VersionAndEditionService.SERVER_PATH) public class VersionAndEditionService implements AdvertisableService
	public class VersionAndEditionService : AdvertisableService
	{
		 private NeoServer _neoServer;
		 public const string SERVER_PATH = "server/version";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public VersionAndEditionService(@Context NeoServer neoServer)
		 public VersionAndEditionService( NeoServer neoServer )
		 {
			  this._neoServer = neoServer;
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return "version";
			 }
		 }

		 public virtual string ServerPath
		 {
			 get
			 {
				  return SERVER_PATH;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Produces(APPLICATION_JSON) public javax.ws.rs.core.Response getVersionAndEditionData()
		 public virtual Response VersionAndEditionData
		 {
			 get
			 {
				  return Response.ok( createJsonFrom( map( "version", NeoDatabaseVersion( _neoServer ), "edition", NeoServerEdition( _neoServer ) ) ), APPLICATION_JSON ).build();
			 }
		 }

		 private string NeoDatabaseVersion( NeoServer neoServer )
		 {
			  return neoServer.Database.Graph.DependencyResolver.resolveDependency( typeof( KernelData ) ).version().ReleaseVersion;
		 }

		 private string NeoServerEdition( NeoServer neoServer )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  string serverClassName = neoServer.GetType().FullName.ToLower();

			  if ( serverClassName.Contains( "enterpriseneoserver" ) || serverClassName.Contains( "commercialneoserver" ) )
			  {
					return "enterprise";
			  }
			  else if ( serverClassName.Contains( "communityneoserver" ) )
			  {
					return "community";
			  }
			  else
			  {
	//            return "unknown";
					throw new System.InvalidOperationException( "The Neo Server running is of unknown type. Valid types are Community " + "and Enterprise." );
			  }
		 }
	}

}