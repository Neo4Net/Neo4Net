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
namespace Neo4Net.Server.modules
{

	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using DefaultPluginManager = Neo4Net.Server.plugins.DefaultPluginManager;
	using PluginManager = Neo4Net.Server.plugins.PluginManager;
	using BatchOperationService = Neo4Net.Server.rest.web.BatchOperationService;
	using CollectUserAgentFilter = Neo4Net.Server.rest.web.CollectUserAgentFilter;
	using CorsFilter = Neo4Net.Server.rest.web.CorsFilter;
	using CypherService = Neo4Net.Server.rest.web.CypherService;
	using DatabaseMetadataService = Neo4Net.Server.rest.web.DatabaseMetadataService;
	using ExtensionService = Neo4Net.Server.rest.web.ExtensionService;
	using RestfulGraphDatabase = Neo4Net.Server.rest.web.RestfulGraphDatabase;
	using TransactionalService = Neo4Net.Server.rest.web.TransactionalService;
	using WebServer = Neo4Net.Server.web.WebServer;
	using UsageData = Neo4Net.Udc.UsageData;
	using UsageDataKeys = Neo4Net.Udc.UsageDataKeys;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.configuration.ServerSettings.http_access_control_allow_origin;

	/// <summary>
	/// Mounts the database REST API.
	/// </summary>
	public class RESTApiModule : ServerModule
	{
		 private readonly Config _config;
		 private readonly WebServer _webServer;
		 private readonly System.Func<UsageData> _userDataSupplier;
		 private readonly LogProvider _logProvider;

		 private PluginManager _plugins;

		 public RESTApiModule( WebServer webServer, Config config, System.Func<UsageData> userDataSupplier, LogProvider logProvider )
		 {
			  this._webServer = webServer;
			  this._config = config;
			  this._userDataSupplier = userDataSupplier;
			  this._logProvider = logProvider;
		 }

		 public override void Start()
		 {
			  URI restApiUri = restApiUri();

			  _webServer.addFilter( new CollectUserAgentFilter( ClientNames() ), "/*" );
			  _webServer.addFilter( new CorsFilter( _logProvider, _config.get( http_access_control_allow_origin ) ), "/*" );
			  _webServer.addJAXRSClasses( ClassNames, restApiUri.ToString(), null );
			  LoadPlugins();
		 }

		 private RecentK<string> ClientNames()
		 {
			  return _userDataSupplier.get().get(UsageDataKeys.clientNames);
		 }

		 private IList<string> ClassNames
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return new IList<string> { typeof( RestfulGraphDatabase ).FullName, typeof( TransactionalService ).FullName, typeof( CypherService ).FullName, typeof( DatabaseMetadataService ).FullName, typeof( ExtensionService ).FullName, typeof( BatchOperationService ).FullName };
			 }
		 }

		 public override void Stop()
		 {
			  _webServer.removeJAXRSClasses( ClassNames, RestApiUri().ToString() );
		 }

		 private URI RestApiUri()
		 {
			  return _config.get( ServerSettings.rest_api_path );
		 }

		 private void LoadPlugins()
		 {
			  _plugins = new DefaultPluginManager( _logProvider );
		 }

		 public virtual PluginManager Plugins
		 {
			 get
			 {
				  return _plugins;
			 }
		 }
	}

}