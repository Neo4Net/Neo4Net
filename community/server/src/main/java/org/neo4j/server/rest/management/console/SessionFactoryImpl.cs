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
namespace Org.Neo4j.Server.rest.management.console
{

	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using CypherExecutor = Org.Neo4j.Server.database.CypherExecutor;
	using Database = Org.Neo4j.Server.database.Database;

	public class SessionFactoryImpl : ConsoleSessionFactory
	{
		 private static readonly ICollection<ConsoleSessionCreator> _creators = Iterables.asCollection( ServiceLoader.load( typeof( ConsoleSessionCreator ) ) );

		 private readonly HttpSession _httpSession;
		 private readonly CypherExecutor _cypherExecutor;
		 private readonly IDictionary<string, ConsoleSessionCreator> _engineCreators = new Dictionary<string, ConsoleSessionCreator>();
		 private readonly HttpServletRequest _request;

		 public SessionFactoryImpl( HttpServletRequest request, IList<string> supportedEngines, CypherExecutor cypherExecutor )
		 {
			  this._request = request;
			  this._httpSession = request.getSession( true );
			  this._cypherExecutor = cypherExecutor;

			  EnableEngines( supportedEngines );
		 }

		 public override ScriptSession CreateSession( string engineName, Database database, LogProvider logProvider )
		 {
			  engineName = engineName.ToLower();
			  if ( _engineCreators.ContainsKey( engineName ) )
			  {
					return GetOrInstantiateSession( database, engineName + "-console-session", _engineCreators[engineName], logProvider );
			  }

			  throw new System.ArgumentException( "Unknown console engine '" + engineName + "'." );
		 }

		 public override IEnumerable<string> SupportedEngines()
		 {
			  return _engineCreators.Keys;
		 }

		 private ScriptSession GetOrInstantiateSession( Database database, string key, ConsoleSessionCreator creator, LogProvider logProvider )
		 {
			  object session = _httpSession.getAttribute( key );
			  if ( session == null )
			  {
					session = creator.NewSession( database, _cypherExecutor, _request, logProvider );
					_httpSession.setAttribute( key, session );
			  }
			  return ( ScriptSession ) session;
		 }

		 private void EnableEngines( IList<string> supportedEngines )
		 {
			  foreach ( ConsoleSessionCreator creator in _creators )
			  {
					foreach ( string engineName in supportedEngines )
					{
						 if ( creator.Name().Equals(engineName, StringComparison.OrdinalIgnoreCase) )
						 {
							  _engineCreators[engineName.ToLower()] = creator;
						 }
					}
			  }
		 }

	}

}