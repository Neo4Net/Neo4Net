/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Server.rest
{

	using CoreGraphDatabase = Org.Neo4j.causalclustering.core.CoreGraphDatabase;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using AdvertisableService = Org.Neo4j.Server.rest.management.AdvertisableService;
	using BadInputException = Org.Neo4j.Server.rest.repr.BadInputException;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;


	/// <summary>
	/// To be deprecated by <seealso cref="org.neo4j.server.rest.causalclustering.CausalClusteringService"/>.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path(CoreDatabaseAvailabilityService.BASE_PATH) public class CoreDatabaseAvailabilityService implements org.neo4j.server.rest.management.AdvertisableService
	public class CoreDatabaseAvailabilityService : AdvertisableService
	{
		 public const string BASE_PATH = "server/core";
		 public const string IS_WRITABLE_PATH = "/writable";
		 public const string IS_AVAILABLE_PATH = "/available";
		 public const string IS_READ_ONLY_PATH = "/read-only";

		 private readonly OutputFormat _output;
		 private readonly CoreGraphDatabase _coreDatabase;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public CoreDatabaseAvailabilityService(@Context OutputFormat output, @Context GraphDatabaseService db)
		 public CoreDatabaseAvailabilityService( OutputFormat output, GraphDatabaseService db )
		 {
			  this._output = output;
			  if ( db is CoreGraphDatabase )
			  {
					this._coreDatabase = ( CoreGraphDatabase ) db;
			  }
			  else
			  {
					this._coreDatabase = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET public javax.ws.rs.core.Response discover()
		 public virtual Response Discover()
		 {
			  if ( _coreDatabase == null )
			  {
					return status( FORBIDDEN ).build();
			  }

			  return _output.ok( new CoreDatabaseAvailabilityDiscoveryRepresentation( BASE_PATH, IS_WRITABLE_PATH ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(IS_WRITABLE_PATH) public javax.ws.rs.core.Response isWritable()
		 public virtual Response Writable
		 {
			 get
			 {
				  if ( _coreDatabase == null )
				  {
						return status( FORBIDDEN ).build();
				  }
   
				  if ( _coreDatabase.Role == Role.LEADER )
				  {
						return PositiveResponse();
				  }
   
				  return NegativeResponse();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(IS_READ_ONLY_PATH) public javax.ws.rs.core.Response isReadOnly()
		 public virtual Response ReadOnly
		 {
			 get
			 {
				  if ( _coreDatabase == null )
				  {
						return status( FORBIDDEN ).build();
				  }
   
				  if ( _coreDatabase.Role == Role.FOLLOWER || _coreDatabase.Role == Role.CANDIDATE )
				  {
						return PositiveResponse();
				  }
   
				  return NegativeResponse();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(IS_AVAILABLE_PATH) public javax.ws.rs.core.Response isAvailable()
		 public virtual Response Available
		 {
			 get
			 {
				  if ( _coreDatabase == null )
				  {
						return status( FORBIDDEN ).build();
				  }
   
				  return PositiveResponse();
			 }
		 }

		 private Response NegativeResponse()
		 {
			  return PlainTextResponse( NOT_FOUND, "false" );
		 }

		 private Response PositiveResponse()
		 {
			  return PlainTextResponse( OK, "true" );
		 }

		 private Response PlainTextResponse( Response.Status status, string entityBody )
		 {
			  return status( status ).type( TEXT_PLAIN_TYPE ).entity( entityBody ).build();
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return "core";
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