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
namespace Neo4Net.Server
{

	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using CommunityGraphFactory = Neo4Net.Server.database.CommunityGraphFactory;
	using GraphFactory = Neo4Net.Server.database.GraphFactory;
	using AuthorizationModule = Neo4Net.Server.modules.AuthorizationModule;
	using ConsoleModule = Neo4Net.Server.modules.ConsoleModule;
	using DBMSModule = Neo4Net.Server.modules.DBMSModule;
	using ManagementApiModule = Neo4Net.Server.modules.ManagementApiModule;
	using Neo4jBrowserModule = Neo4Net.Server.modules.Neo4jBrowserModule;
	using RESTApiModule = Neo4Net.Server.modules.RESTApiModule;
	using SecurityRulesModule = Neo4Net.Server.modules.SecurityRulesModule;
	using ServerModule = Neo4Net.Server.modules.ServerModule;
	using ThirdPartyJAXRSModule = Neo4Net.Server.modules.ThirdPartyJAXRSModule;
	using DiscoverableURIs = Neo4Net.Server.rest.discovery.DiscoverableURIs;
	using AdvertisableService = Neo4Net.Server.rest.management.AdvertisableService;
	using JmxService = Neo4Net.Server.rest.management.JmxService;
	using ConsoleService = Neo4Net.Server.rest.management.console.ConsoleService;
	using Jetty9WebServer = Neo4Net.Server.web.Jetty9WebServer;
	using WebServer = Neo4Net.Server.web.WebServer;
	using UsageData = Neo4Net.Udc.UsageData;

	using static Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.CommunityDiscoverableURIs.communityDiscoverableURIs;

	public class CommunityNeoServer : AbstractNeoServer
	{
		 public CommunityNeoServer( Config config, Dependencies dependencies ) : this( config, new CommunityGraphFactory(), dependencies )
		 {
		 }

		 public CommunityNeoServer( Config config, GraphFactory graphFactory, Dependencies dependencies ) : base( config, graphFactory, dependencies )
		 {
		 }

		 protected internal override IEnumerable<ServerModule> CreateServerModules()
		 {
			  return Arrays.asList( CreateDBMSModule(), new RESTApiModule(WebServerConflict, Config, DependencyResolver.provideDependency(typeof(UsageData)), UserLogProvider), new ManagementApiModule(WebServerConflict, Config), new ThirdPartyJAXRSModule(WebServerConflict, Config, UserLogProvider, this), new ConsoleModule(WebServerConflict, Config), new Neo4jBrowserModule(WebServerConflict), CreateAuthorizationModule(), new SecurityRulesModule(WebServerConflict, Config, UserLogProvider) );
		 }

		 protected internal override WebServer CreateWebServer()
		 {
			  NetworkConnectionTracker connectionTracker = DependencyResolver.resolveDependency( typeof( NetworkConnectionTracker ) );
			  return new Jetty9WebServer( UserLogProvider, Config, connectionTracker );
		 }

		 public override IEnumerable<AdvertisableService> Services
		 {
			 get
			 {
				  IList<AdvertisableService> toReturn = new List<AdvertisableService>( 3 );
				  toReturn.Add( new ConsoleService( null, null, UserLogProvider, null ) );
				  toReturn.Add( new JmxService( null, null ) );
   
				  return toReturn;
			 }
		 }

		 protected internal virtual DBMSModule CreateDBMSModule()
		 {
			  // ConnectorPortRegister isn't available until runtime, so defer loading until then
			  System.Func<DiscoverableURIs> discoverableURIs = () => communityDiscoverableURIs(Config, DependencyResolver.resolveDependency(typeof(ConnectorPortRegister)));
			  return new DBMSModule( WebServerConflict, Config, discoverableURIs );
		 }

		 protected internal virtual AuthorizationModule CreateAuthorizationModule()
		 {
			  return new AuthorizationModule( WebServerConflict, AuthManagerSupplier, UserLogProvider, Config, UriWhitelist );
		 }
	}

}