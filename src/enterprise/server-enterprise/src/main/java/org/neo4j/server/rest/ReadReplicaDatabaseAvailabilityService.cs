/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.rest
{

	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using AdvertisableService = Neo4Net.Server.rest.management.AdvertisableService;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;


	/// <summary>
	/// To be deprecated by <seealso cref="Neo4Net.server.rest.causalclustering.CausalClusteringService"/>.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path(ReadReplicaDatabaseAvailabilityService.BASE_PATH) public class ReadReplicaDatabaseAvailabilityService implements Neo4Net.server.rest.management.AdvertisableService
	public class ReadReplicaDatabaseAvailabilityService : AdvertisableService
	{
		 internal const string BASE_PATH = "server/read-replica";
		 private const string IS_AVAILABLE_PATH = "/available";

		 private readonly ReadReplicaGraphDatabase _readReplica;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ReadReplicaDatabaseAvailabilityService(@Context OutputFormat output, @Context IGraphDatabaseService db)
		 public ReadReplicaDatabaseAvailabilityService( OutputFormat output, IGraphDatabaseService db )
		 {
			  if ( db is ReadReplicaGraphDatabase )
			  {
					this._readReplica = ( ReadReplicaGraphDatabase ) db;
			  }
			  else
			  {
					this._readReplica = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(IS_AVAILABLE_PATH) public javax.ws.rs.core.Response isAvailable()
		 public virtual Response Available
		 {
			 get
			 {
				  if ( _readReplica == null )
				  {
						return status( FORBIDDEN ).build();
				  }
   
				  return PositiveResponse();
			 }
		 }

		 private Response PositiveResponse()
		 {
			  return PlainTextResponse( OK, "true" );
		 }

		 private Response PlainTextResponse( Response.Status status, string IEntityBody )
		 {
			  return status( status ).type( TEXT_PLAIN_TYPE ).entity( IEntityBody ).build();
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return "read-replica";
			 }
		 }

		 public virtual string ServerPath
		 {
			 get
			 {
				  return BASE_PATH;
			 }
		 }
	}

}