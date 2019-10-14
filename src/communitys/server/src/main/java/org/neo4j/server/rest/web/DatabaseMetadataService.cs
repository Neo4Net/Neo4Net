using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Server.rest.web
{

	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Database = Neo4Net.Server.database.Database;
	using RepresentationWriteHandler = Neo4Net.Server.rest.repr.RepresentationWriteHandler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/relationship/types") public class DatabaseMetadataService
	public class DatabaseMetadataService
	{
		 private readonly Database _database;
		 private RepresentationWriteHandler _representationWriteHandler = RepresentationWriteHandler.DO_NOTHING;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public DatabaseMetadataService(@Context Database database)
		 public DatabaseMetadataService( Database database )
		 {
			  this._database = database;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Produces(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response getRelationshipTypes(@QueryParam("in_use") @DefaultValue("true") boolean inUse)
		 public virtual Response GetRelationshipTypes( bool inUse )
		 {
			  try
			  {
					GraphDatabaseAPI db = _database.Graph;
					IEnumerable<RelationshipType> relationshipTypes = inUse ? Db.AllRelationshipTypesInUse : Db.AllRelationshipTypes;
					return Response.ok().type(MediaType.APPLICATION_JSON).entity(GenerateJsonRepresentation(relationshipTypes)).build();
			  }
			  finally
			  {
					_representationWriteHandler.onRepresentationFinal();
			  }
		 }

		 private string GenerateJsonRepresentation( IEnumerable<RelationshipType> relationshipTypes )
		 {
			  StringBuilder sb = new StringBuilder();
			  sb.Append( "[" );
			  foreach ( RelationshipType rt in relationshipTypes )
			  {
					sb.Append( "\"" );
					sb.Append( rt.Name() );
					sb.Append( "\"," );
			  }
			  sb.Append( "]" );
			  return sb.ToString().replaceAll(",]", "]");
		 }

		 public virtual RepresentationWriteHandler RepresentationWriteHandler
		 {
			 set
			 {
				  this._representationWriteHandler = value;
			 }
		 }
	}

}