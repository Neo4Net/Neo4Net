/*
 * Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */

namespace Org.Neo4j.causalclustering.core
{

	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using SecureHazelcastDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.SecureHazelcastDiscoveryServiceFactory;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using Dependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;

	public class OpenEnterpriseCoreGraphDatabase : CoreGraphDatabase
	{
		 /// <param name="storeDir"> </param>
		 /// <param name="config"> </param>
		 /// <param name="dependencies"> </param>
		 public OpenEnterpriseCoreGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies ) : this( storeDir, config, dependencies, new SecureHazelcastDiscoveryServiceFactory() )
		 {
		 }

		 /// <param name="storeDir"> </param>
		 /// <param name="config"> </param>
		 /// <param name="dependencies"> </param>
		 /// <param name="discoveryServiceFactory"> </param>
		 public OpenEnterpriseCoreGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies, DiscoveryServiceFactory discoveryServiceFactory )
		 {
			  System.Func<PlatformModule, AbstractEditionModule> factory = platformModule => new OpenEnterpriseCoreEditionModule( platformModule, discoveryServiceFactory );

			  ( new GraphDatabaseFacadeFactory( DatabaseInfo.CORE, factory ) ).initFacade( storeDir, config, dependencies, this );
		 }
	}

}