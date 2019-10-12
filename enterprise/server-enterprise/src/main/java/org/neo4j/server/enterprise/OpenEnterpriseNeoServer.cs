using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Modifications Copyright (c) 2018-2019 "GraphFoundation" [https://graphfoundation.org]
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
 /*
   * Note: Check for changes to how the server is registered here: https://github.com/neo4j/neo4j/blob/ba4e188d51e027a7e7870d511044ea940cf0455e/community/server/src/main/java/org/neo4j/server/rest/management/VersionAndEditionService.java
 */
namespace Org.Neo4j.Server.enterprise
{
	using ThreadPool = org.eclipse.jetty.util.thread.ThreadPool;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using Dependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using HaSettings = Org.Neo4j.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using UnsatisfiedDependencyException = Org.Neo4j.Kernel.impl.util.UnsatisfiedDependencyException;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using ServerThreadView = Org.Neo4j.metrics.source.server.ServerThreadView;
	using ServerThreadViewSetter = Org.Neo4j.metrics.source.server.ServerThreadViewSetter;
	using EnterpriseGraphFactory = Org.Neo4j.Server.database.EnterpriseGraphFactory;
	using GraphFactory = Org.Neo4j.Server.database.GraphFactory;
	using EnterpriseAuthorizationModule = Org.Neo4j.Server.enterprise.modules.EnterpriseAuthorizationModule;
	using JMXManagementModule = Org.Neo4j.Server.enterprise.modules.JMXManagementModule;
	using AuthorizationModule = Org.Neo4j.Server.modules.AuthorizationModule;
	using DBMSModule = Org.Neo4j.Server.modules.DBMSModule;
	using ServerModule = Org.Neo4j.Server.modules.ServerModule;
	using DatabaseRoleInfoServerModule = Org.Neo4j.Server.rest.DatabaseRoleInfoServerModule;
	using EnterpriseDiscoverableURIs = Org.Neo4j.Server.rest.EnterpriseDiscoverableURIs;
	using MasterInfoService = Org.Neo4j.Server.rest.MasterInfoService;
	using DiscoverableURIs = Org.Neo4j.Server.rest.discovery.DiscoverableURIs;
	using AdvertisableService = Org.Neo4j.Server.rest.management.AdvertisableService;
	using Jetty9WebServer = Org.Neo4j.Server.web.Jetty9WebServer;
	using WebServer = Org.Neo4j.Server.web.WebServer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.configuration.ServerSettings.jmx_module_enabled;

	public class OpenEnterpriseNeoServer : CommunityNeoServer
	{
		 public OpenEnterpriseNeoServer( Config config, Dependencies dependencies ) : base( config, new EnterpriseGraphFactory(), dependencies )
		 {
		 }

		 public OpenEnterpriseNeoServer( Config config, GraphFactory graphFactory, Dependencies dependencies ) : base( config, graphFactory, dependencies )
		 {
		 }

		 protected internal override WebServer CreateWebServer()
		 {
			  Jetty9WebServer webServer = ( Jetty9WebServer ) base.CreateWebServer();
			  webServer.JettyCreatedCallback = jetty =>
			  {
				ThreadPool threadPool = jetty.ThreadPool;
				Debug.Assert( threadPool != null );
				try
				{
					 ServerThreadViewSetter setter = DatabaseConflict.Graph.DependencyResolver.resolveDependency( typeof( ServerThreadViewSetter ) );
					 setter.set( new ServerThreadViewAnonymousInnerClass( this, threadPool ) );
				}
				catch ( UnsatisfiedDependencyException )
				{
					 // nevermind, metrics are likely not enabled
				}
			  };
			  return webServer;
		 }

		 private class ServerThreadViewAnonymousInnerClass : ServerThreadView
		 {
			 private readonly OpenEnterpriseNeoServer _outerInstance;

			 private ThreadPool _threadPool;

			 public ServerThreadViewAnonymousInnerClass( OpenEnterpriseNeoServer outerInstance, ThreadPool threadPool )
			 {
				 this.outerInstance = outerInstance;
				 this._threadPool = threadPool;
			 }

			 public override int allThreads()
			 {
				  return _threadPool.Threads;
			 }

			 public override int idleThreads()
			 {
				  return _threadPool.IdleThreads;
			 }
		 }

		 protected internal override AuthorizationModule CreateAuthorizationModule()
		 {
			  return new EnterpriseAuthorizationModule( WebServerConflict, AuthManagerSupplier, UserLogProvider, Config, UriWhitelist );
		 }

		 protected internal override DBMSModule CreateDBMSModule()
		 {
			  // ConnectorPortRegister isn't available until runtime, so defer loading until then
			  System.Func<DiscoverableURIs> discoverableURIs = () => EnterpriseDiscoverableURIs.enterpriseDiscoverableURIs(Config, DependencyResolver.resolveDependency(typeof(ConnectorPortRegister)));
			  return new DBMSModule( WebServerConflict, Config, discoverableURIs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override protected Iterable<org.neo4j.server.modules.ServerModule> createServerModules()
		 protected internal override IEnumerable<ServerModule> CreateServerModules()
		 {
			  IList<ServerModule> modules = new List<ServerModule>();
			  modules.Add( new DatabaseRoleInfoServerModule( WebServerConflict, Config, UserLogProvider ) );
			  if ( Config.get( jmx_module_enabled ) )
			  {
					modules.Add( new JMXManagementModule( this ) );
			  }
			  base.CreateServerModules().forEach(modules.add);
			  return modules;
		 }

		 public override IEnumerable<AdvertisableService> Services
		 {
			 get
			 {
				  if ( Database.Graph is HighlyAvailableGraphDatabase )
				  {
						return Iterables.append( new MasterInfoService( null, null ), base.Services );
				  }
				  else
				  {
						return base.Services;
				  }
			 }
		 }

		 protected internal override Pattern[] UriWhitelist
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final java.util.List<java.util.regex.Pattern> uriWhitelist = new java.util.ArrayList<>(java.util.Arrays.asList(super.getUriWhitelist()));
				  IList<Pattern> uriWhitelist = new List<Pattern>( Arrays.asList( base.UriWhitelist ) );
   
				  if ( !Config.get( HaSettings.ha_status_auth_enabled ) )
				  {
						uriWhitelist.Add( Pattern.compile( "/db/manage/server/ha.*" ) );
				  }
   
				  if ( !Config.get( CausalClusteringSettings.status_auth_enabled ) )
				  {
						uriWhitelist.Add( Pattern.compile( "/db/manage/server/core.*" ) );
						uriWhitelist.Add( Pattern.compile( "/db/manage/server/read-replica.*" ) );
						uriWhitelist.Add( Pattern.compile( "/db/manage/server/causalclustering.*" ) );
				  }
   
				  return uriWhitelist.ToArray();
			 }
		 }
	}

}