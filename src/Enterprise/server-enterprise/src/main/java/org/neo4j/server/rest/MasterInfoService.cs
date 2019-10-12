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
namespace Neo4Net.Server.rest
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using AdvertisableService = Neo4Net.Server.rest.management.AdvertisableService;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path(MasterInfoService.BASE_PATH) public class MasterInfoService implements org.neo4j.server.rest.management.AdvertisableService
	public class MasterInfoService : AdvertisableService
	{
		 public const string BASE_PATH = "server/ha";
		 public const string IS_MASTER_PATH = "/master";
		 public const string IS_SLAVE_PATH = "/slave";
		 public const string IS_AVAILABLE_PATH = "/available";

		 private readonly OutputFormat _output;
		 private readonly HighlyAvailableGraphDatabase _haDb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public MasterInfoService(@Context OutputFormat output, @Context GraphDatabaseService db)
		 public MasterInfoService( OutputFormat output, GraphDatabaseService db )
		 {
			  this._output = output;
			  if ( db is HighlyAvailableGraphDatabase )
			  {
					this._haDb = ( HighlyAvailableGraphDatabase ) db;
			  }
			  else
			  {
					this._haDb = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET public javax.ws.rs.core.Response discover()
		 public virtual Response Discover()
		 {
			  if ( _haDb == null )
			  {
					return status( FORBIDDEN ).build();
			  }

			  string isMasterUri = IS_MASTER_PATH;
			  string isSlaveUri = IS_SLAVE_PATH;

			  HaDiscoveryRepresentation dr = new HaDiscoveryRepresentation( BASE_PATH, isMasterUri, isSlaveUri );
			  return _output.ok( dr );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(IS_MASTER_PATH) public javax.ws.rs.core.Response isMaster()
		 public virtual Response Master
		 {
			 get
			 {
				  if ( _haDb == null )
				  {
						return status( FORBIDDEN ).build();
				  }
   
				  string role = _haDb.role().ToLower();
				  if ( role.Equals( "master" ) )
				  {
						return PositiveResponse();
				  }
   
				  if ( role.Equals( "slave" ) )
				  {
						return NegativeResponse();
				  }
   
				  return UnknownResponse();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(IS_SLAVE_PATH) public javax.ws.rs.core.Response isSlave()
		 public virtual Response Slave
		 {
			 get
			 {
				  if ( _haDb == null )
				  {
						return status( FORBIDDEN ).build();
				  }
   
				  string role = _haDb.role().ToLower();
				  if ( role.Equals( "slave" ) )
				  {
						return PositiveResponse();
				  }
   
				  if ( role.Equals( "master" ) )
				  {
						return NegativeResponse();
				  }
   
				  return UnknownResponse();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(IS_AVAILABLE_PATH) public javax.ws.rs.core.Response isAvailable()
		 public virtual Response Available
		 {
			 get
			 {
				  if ( _haDb == null )
				  {
						return status( FORBIDDEN ).build();
				  }
   
				  string role = _haDb.role().ToLower();
				  if ( "slave".Equals( role ) || "master".Equals( role ) )
				  {
						return PlainTextResponse( OK, role );
				  }
   
				  return UnknownResponse();
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

		 private Response UnknownResponse()
		 {
			  return PlainTextResponse( NOT_FOUND, "UNKNOWN" );
		 }

		 private Response PlainTextResponse( Response.Status status, string entityBody )
		 {
			  return status( status ).type( TEXT_PLAIN_TYPE ).entity( entityBody ).build();
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return "ha";
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