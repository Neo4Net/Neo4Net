using System;

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
namespace Org.Neo4j.Server.rest.management.console
{
	using StringUtils = org.apache.commons.lang3.StringUtils;

	using SyntaxException = Org.Neo4j.Cypher.SyntaxException;
	using ExecutionEngine = Org.Neo4j.Cypher.@internal.javacompat.ExecutionEngine;
	using Result = Org.Neo4j.Graphdb.Result;
	using Org.Neo4j.Helpers.Collection;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using CypherExecutor = Org.Neo4j.Server.database.CypherExecutor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.emptyMap;

	public class CypherSession : ScriptSession
	{
		 private readonly CypherExecutor _cypherExecutor;
		 private readonly Log _log;
		 private readonly HttpServletRequest _request;

		 public CypherSession( CypherExecutor cypherExecutor, LogProvider logProvider, HttpServletRequest request )
		 {
			  this._cypherExecutor = cypherExecutor;
			  this._log = logProvider.getLog( this.GetType() );
			  this._request = request;
		 }

		 public override Pair<string, string> Evaluate( string script )
		 {
			  if ( StringUtils.EMPTY.Equals( script.Trim() ) )
			  {
					return Pair.of( StringUtils.EMPTY, null );
			  }

			  string resultString;
			  try
			  {
					TransactionalContext tc = _cypherExecutor.createTransactionContext( script, emptyMap(), _request );
					ExecutionEngine engine = _cypherExecutor.ExecutionEngine;
					Result result = engine.ExecuteQuery( script, emptyMap(), tc );
					resultString = result.ResultAsString();
			  }
			  catch ( SyntaxException error )
			  {
					resultString = error.Message;
			  }
			  catch ( Exception exception )
			  {
					_log.error( "Unknown error executing cypher query", exception );
					resultString = "Error: " + exception.GetType().Name + " - " + exception.Message;
			  }
			  return Pair.of( resultString, null );
		 }
	}

}