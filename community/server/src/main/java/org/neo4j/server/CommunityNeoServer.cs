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
namespace Org.Neo4j.Server
{

	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using CommunityGraphFactory = Org.Neo4j.Server.database.CommunityGraphFactory;
	using GraphFactory = Org.Neo4j.Server.database.GraphFactory;
	using AuthorizationModule = Org.Neo4j.Server.modules.AuthorizationModule;
	using ConsoleModule = Org.Neo4j.Server.modules.ConsoleModule;
	using DBMSModule = Org.Neo4j.Server.modules.DBMSModule;
	using ManagementApiModule = Org.Neo4j.Server.modules.ManagementApiModule;
	using Neo4jBrowserModule = Org.Neo4j.Server.modules.Neo4jBrowserModule;
	using RESTApiModule = Org.Neo4j.Server.modules.RESTApiModule;
	using SecurityRulesModule = Org.Neo4j.Server.modules.SecurityRulesModule;
	using ServerModule = Org.Neo4j.Server.modules.ServerModule;
	using ThirdPartyJAXRSModule = Org.Neo4j.Server.modules.ThirdPartyJAXRSModule;
	using DiscoverableURIs = Org.Neo4j.Server.rest.discovery.DiscoverableURIs;
	using AdvertisableService = Org.Neo4j.Server.rest.management.AdvertisableService;
	using JmxService = Org.Neo4j.Server.rest.management.JmxService;
	using ConsoleService = Org.Neo4j.Server.rest.management.console.ConsoleService;
	using Jetty9WebServer = Org.Neo4j.Server.web.Jetty9WebServer;
	using WebServer = Org.Neo4j.Server.web.WebServer;
	using UsageData = Org.Neo4j.Udc.UsageData;

	using static Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
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