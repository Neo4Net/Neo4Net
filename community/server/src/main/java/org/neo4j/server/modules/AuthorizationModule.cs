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
namespace Org.Neo4j.Server.modules
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using AuthorizationDisabledFilter = Org.Neo4j.Server.rest.dbms.AuthorizationDisabledFilter;
	using AuthorizationEnabledFilter = Org.Neo4j.Server.rest.dbms.AuthorizationEnabledFilter;
	using WebServer = Org.Neo4j.Server.web.WebServer;

	public class AuthorizationModule : ServerModule
	{
		 private readonly WebServer _webServer;
		 private readonly Config _config;
		 private readonly System.Func<AuthManager> _authManagerSupplier;
		 private readonly LogProvider _logProvider;
		 private readonly Pattern[] _uriWhitelist;

		 public AuthorizationModule( WebServer webServer, System.Func<AuthManager> authManager, LogProvider logProvider, Config config, Pattern[] uriWhitelist )
		 {
			  this._webServer = webServer;
			  this._config = config;
			  this._authManagerSupplier = authManager;
			  this._logProvider = logProvider;
			  this._uriWhitelist = uriWhitelist;
		 }

		 public override void Start()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.servlet.Filter authorizationFilter;
			  Filter authorizationFilter;

			  if ( _config.get( GraphDatabaseSettings.auth_enabled ) )
			  {
					authorizationFilter = new AuthorizationEnabledFilter( _authManagerSupplier, _logProvider, _uriWhitelist );
			  }
			  else
			  {
					authorizationFilter = CreateAuthorizationDisabledFilter();
			  }

			  _webServer.addFilter( authorizationFilter, "/*" );
		 }

		 public override void Stop()
		 {
		 }

		 protected internal virtual AuthorizationDisabledFilter CreateAuthorizationDisabledFilter()
		 {
			  return new AuthorizationDisabledFilter();
		 }
	}

}