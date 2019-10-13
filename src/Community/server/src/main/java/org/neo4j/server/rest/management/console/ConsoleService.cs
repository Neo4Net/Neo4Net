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
namespace Neo4Net.Server.rest.management.console
{

	using Neo4Net.Helpers.Collections;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using CypherExecutor = Neo4Net.Server.database.CypherExecutor;
	using Database = Neo4Net.Server.database.Database;
	using ConsoleServiceRepresentation = Neo4Net.Server.rest.management.repr.ConsoleServiceRepresentation;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using InputFormat = Neo4Net.Server.rest.repr.InputFormat;
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using RepresentationType = Neo4Net.Server.rest.repr.RepresentationType;
	using ValueRepresentation = Neo4Net.Server.rest.repr.ValueRepresentation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path(ConsoleService.SERVICE_PATH) public class ConsoleService implements org.neo4j.server.rest.management.AdvertisableService
	public class ConsoleService : AdvertisableService
	{
		 public const string SERVICE_PATH = "server/console";
		 private const string SERVICE_NAME = "console";

		 private readonly ConsoleSessionFactory _sessionFactory;
		 private readonly Database _database;
		 private readonly LogProvider _logProvider;
		 private readonly OutputFormat _output;
		 private readonly Log _log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ConsoleService(@Context Config config, @Context Database database, @Context LogProvider logProvider, @Context HttpServletRequest req, @Context OutputFormat output, @Context CypherExecutor cypherExecutor)
		 public ConsoleService( Config config, Database database, LogProvider logProvider, HttpServletRequest req, OutputFormat output, CypherExecutor cypherExecutor ) : this( new SessionFactoryImpl( req, config.get( ServerSettings.console_module_engines ), cypherExecutor ), database, logProvider, output )
		 {
		 }

		 public ConsoleService( ConsoleSessionFactory sessionFactory, Database database, LogProvider logProvider, OutputFormat output )
		 {
			  this._sessionFactory = sessionFactory;
			  this._database = database;
			  this._logProvider = logProvider;
			  this._output = output;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return SERVICE_NAME;
			 }
		 }

		 public virtual string ServerPath
		 {
			 get
			 {
				  return SERVICE_PATH;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET public javax.ws.rs.core.Response getServiceDefinition()
		 public virtual Response ServiceDefinition
		 {
			 get
			 {
				  ConsoleServiceRepresentation result = new ConsoleServiceRepresentation( SERVICE_PATH, _sessionFactory.supportedEngines() );
   
				  return _output.ok( result );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST public javax.ws.rs.core.Response exec(@Context InputFormat input, String data)
		 public virtual Response Exec( InputFormat input, string data )
		 {
			  IDictionary<string, object> args;
			  try
			  {
					args = input.readMap( data );
			  }
			  catch ( BadInputException e )
			  {
					return _output.badRequest( e );
			  }

			  if ( !args.ContainsKey( "command" ) )
			  {
					return Response.status( Response.Status.BAD_REQUEST ).entity( "Expected command argument not present." ).build();
			  }

			  ScriptSession scriptSession;
			  try
			  {
					scriptSession = GetSession( args );
			  }
			  catch ( System.ArgumentException e )
			  {
					return _output.badRequest( e );
			  }

			  _log.debug( scriptSession.ToString() );
			  try
			  {
					Pair<string, string> result = scriptSession.Evaluate( ( string ) args["command"] );
					IList<Representation> list = new IList<Representation> { ValueRepresentation.@string( result.First() ), ValueRepresentation.@string(result.Other()) };

					return _output.ok( new ListRepresentation( RepresentationType.STRING, list ) );
			  }
			  catch ( Exception e )
			  {
					IList<Representation> list = new IList<Representation> { ValueRepresentation.@string( e.GetType() + " : " + e.Message + "\n" ), ValueRepresentation.@string(null) };
					return _output.ok( new ListRepresentation( RepresentationType.STRING, list ) );
			  }
		 }

		 private ScriptSession GetSession( IDictionary<string, object> args )
		 {
			  return _sessionFactory.createSession( ( string ) args["engine"], _database, _logProvider );
		 }
	}

}