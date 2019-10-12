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
namespace Org.Neo4j.causalclustering.readreplica
{

	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using SecureDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.SecureDiscoveryServiceFactory;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using Dependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;

	public class OpenEnterpriseReadReplicaGraphDatabase : ReadReplicaGraphDatabase
	{
		 /// <param name="storeDir"> </param>
		 /// <param name="config"> </param>
		 /// <param name="dependencies"> </param>
		 public OpenEnterpriseReadReplicaGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies, SecureDiscoveryServiceFactory discoveryServiceFactory ) : this( storeDir, config, dependencies, discoveryServiceFactory, new MemberId( System.Guid.randomUUID() ) )
		 {
		 }

		 /// <param name="storeDir"> </param>
		 /// <param name="config"> </param>
		 /// <param name="dependencies"> </param>
		 /// <param name="discoveryServiceFactory"> </param>
		 /// <param name="memberId"> </param>
		 public OpenEnterpriseReadReplicaGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies, DiscoveryServiceFactory discoveryServiceFactory, MemberId memberId )
		 {
			  System.Func<PlatformModule, AbstractEditionModule> factory = platformModule => new OpenEnterpriseReadReplicaEditionModule( platformModule, discoveryServiceFactory, memberId );

			  ( new GraphDatabaseFacadeFactory( DatabaseInfo.READ_REPLICA, factory ) ).initFacade( storeDir, config, dependencies, this );
		 }
	}


}