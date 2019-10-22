/*
 * Copyright (c) 2002-2018 "Neo Technology,"
 * Network Engine for Objects in Lund AB [http://neotechnology.com]
 *
 * Modifications Copyright (c) 2018-2019 "GraphFoundation" [https://graphfoundation.org]
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
namespace Neo4Net.Server.enterprise
{

	using OpenEnterpriseCoreGraphDatabase = Neo4Net.causalclustering.core.OpenEnterpriseCoreGraphDatabase;
	using SecureDiscoveryServiceFactory = Neo4Net.causalclustering.discovery.SecureDiscoveryServiceFactory;
	using OpenEnterpriseReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.OpenEnterpriseReadReplicaGraphDatabase;
	using Dependencies = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using Mode = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings.Mode;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using EnterpriseGraphFactory = Neo4Net.Server.database.EnterpriseGraphFactory;

	public class OpenEnterpriseGraphFactory : EnterpriseGraphFactory
	{
		 public virtual GraphDatabaseFacade NewGraphDatabase( Config config, Dependencies dependencies )
		 {
			  EnterpriseEditionSettings.Mode mode = ( EnterpriseEditionSettings.Mode ) config.Get( EnterpriseEditionSettings.mode );
			  File storeDir = ( File ) config.Get( GraphDatabaseSettings.databases_root_path );
			  SecureDiscoveryServiceFactory discoveryServiceFactory = ( SecureDiscoveryServiceFactory )( new OpenEnterpriseDiscoveryServiceFactorySelector() ).Select((Config) config);
			  switch ( mode )
			  {
			  case EnterpriseEditionSettings.Mode.CORE:
					return new OpenEnterpriseCoreGraphDatabase( storeDir, config, dependencies, discoveryServiceFactory );
			  case EnterpriseEditionSettings.Mode.READ_REPLICA:
					return new OpenEnterpriseReadReplicaGraphDatabase( storeDir, config, dependencies, discoveryServiceFactory );
			  case EnterpriseEditionSettings.Mode.SINGLE:
					return new OpenEnterpriseGraphDatabaseFacade( storeDir, config, dependencies );
			  default:
					return base.NewGraphDatabase( config, dependencies );
			  }
		 }
	}

}