using System;
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
namespace Org.Neo4j.Server.rest.web
{

	using CypherException = Org.Neo4j.Cypher.CypherException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Result = Org.Neo4j.Graphdb.Result;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using CypherExecutor = Org.Neo4j.Server.database.CypherExecutor;
	using BadInputException = Org.Neo4j.Server.rest.repr.BadInputException;
	using CypherResultRepresentation = Org.Neo4j.Server.rest.repr.CypherResultRepresentation;
	using InputFormat = Org.Neo4j.Server.rest.repr.InputFormat;
	using InvalidArgumentsException = Org.Neo4j.Server.rest.repr.InvalidArgumentsException;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;
	using CommitOnSuccessfulStatusCodeRepresentationWriteHandler = Org.Neo4j.Server.rest.transactional.CommitOnSuccessfulStatusCodeRepresentationWriteHandler;
	using UsageData = Org.Neo4j.Udc.UsageData;
	using VirtualValue = Org.Neo4j.Values.VirtualValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.udc.UsageDataKeys.Features_Fields.http_cypher_endpoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.udc.UsageDataKeys.features;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/cypher") public class CypherService
	public class CypherService
	{

		 private const string PARAMS_KEY = "params";
		 private const string QUERY_KEY = "query";

		 private const string INCLUDE_STATS_PARAM = "includeStats";
		 private const string INCLUDE_PLAN_PARAM = "includePlan";
		 private const string PROFILE_PARAM = "profile";

		 private readonly GraphDatabaseService _database;
		 private readonly CypherExecutor _cypherExecutor;
		 private readonly UsageData _usage;
		 private readonly OutputFormat _output;
		 private readonly InputFormat _input;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public CypherService(@Context GraphDatabaseService database, @Context CypherExecutor cypherExecutor, @Context InputFormat input, @Context OutputFormat output, @Context UsageData usage)
		 public CypherService( GraphDatabaseService database, CypherExecutor cypherExecutor, InputFormat input, OutputFormat output, UsageData usage )
		 {
			  this._database = database;
			  this._cypherExecutor = cypherExecutor;
			  this._input = input;
			  this._output = output;
			  this._usage = usage;
		 }

		 public virtual OutputFormat OutputFormat
		 {
			 get
			 {
				  return _output;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @SuppressWarnings({"unchecked", "ParameterCanBeLocal"}) public javax.ws.rs.core.Response cypher(String body, @Context HttpServletRequest request, @QueryParam(INCLUDE_STATS_PARAM) boolean includeStats, @QueryParam(INCLUDE_PLAN_PARAM) boolean includePlan, @QueryParam(PROFILE_PARAM) boolean profile) throws org.neo4j.server.rest.repr.BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual Response Cypher( string body, HttpServletRequest request, bool includeStats, bool includePlan, bool profile )
		 {

			  _usage.get( features ).flag( http_cypher_endpoint );
			  IDictionary<string, object> command = _input.readMap( body );

			  if ( !command.ContainsKey( QUERY_KEY ) )
			  {
					return _output.badRequest( new InvalidArgumentsException( "You have to provide the 'query' parameter." ) );
			  }

			  string query = ( string ) command[QUERY_KEY];
			  IDictionary<string, object> paramsMap;
			  try
			  {
					paramsMap = ( IDictionary<string, object> )( command.ContainsKey( PARAMS_KEY ) && command[PARAMS_KEY] != null ? command[PARAMS_KEY] : new Dictionary<string, object>() );
			  }
			  catch ( System.InvalidCastException )
			  {
					return _output.badRequest( new System.ArgumentException( "Parameters must be a JSON map" ) );
			  }

			  try
			  {
					QueryExecutionEngine executionEngine = _cypherExecutor.ExecutionEngine;
					bool periodicCommitQuery = executionEngine.IsPeriodicCommit( query );
					CommitOnSuccessfulStatusCodeRepresentationWriteHandler handler = ( CommitOnSuccessfulStatusCodeRepresentationWriteHandler ) _output.RepresentationWriteHandler;
					if ( periodicCommitQuery )
					{
						 handler.CloseTransaction();
					}

					MapValue @params = ValueUtils.asMapValue( paramsMap );
					TransactionalContext tc = _cypherExecutor.createTransactionContext( query, @params, request );

					Result result;
					if ( profile )
					{
						 result = executionEngine.ProfileQuery( query, @params, tc );
						 includePlan = true;
					}
					else
					{
						 result = executionEngine.ExecuteQuery( query, @params, tc );
						 includePlan = result.QueryExecutionType.requestedExecutionPlanDescription();
					}

					if ( periodicCommitQuery )
					{
						 handler.Transaction = _database.beginTx();
					}

					return _output.ok( new CypherResultRepresentation( result, includeStats, includePlan ) );
			  }
			  catch ( Exception e )
			  {
					if ( e.InnerException is CypherException )
					{
						 return _output.badRequest( e.InnerException );
					}
					else
					{
						 return _output.badRequest( e );
					}
			  }
		 }
	}

}