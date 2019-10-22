/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.database
{

	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using DiscoveryServiceFactory = Neo4Net.causalclustering.discovery.DiscoveryServiceFactory;
	using EnterpriseDiscoveryServiceFactorySelector = Neo4Net.causalclustering.discovery.EnterpriseDiscoveryServiceFactorySelector;
	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseGraphDatabase = Neo4Net.Kernel.enterprise.EnterpriseGraphDatabase;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;

	using static Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory.Dependencies;

	public class EnterpriseGraphFactory : GraphFactory
	{
		 public override GraphDatabaseFacade NewGraphDatabase( Config config, Dependencies dependencies )
		 {
			  EnterpriseEditionSettings.Mode mode = config.Get( EnterpriseEditionSettings.mode );
			  File storeDir = config.Get( GraphDatabaseSettings.databases_root_path );
			  DiscoveryServiceFactory discoveryServiceFactory = ( new EnterpriseDiscoveryServiceFactorySelector() ).select(config);

			  switch ( mode )
			  {
			  case EnterpriseEditionSettings.Mode.HA:
					return new HighlyAvailableGraphDatabase( storeDir, config, dependencies );
			  case EnterpriseEditionSettings.Mode.ARBITER:
					// Should never reach here because this mode is handled separately by the scripts.
					throw new System.ArgumentException( "The server cannot be started in ARBITER mode." );
			  case EnterpriseEditionSettings.Mode.CORE:
					return new CoreGraphDatabase( storeDir, config, dependencies, discoveryServiceFactory );
			  case EnterpriseEditionSettings.Mode.READ_REPLICA:
					return new ReadReplicaGraphDatabase( storeDir, config, dependencies, discoveryServiceFactory );
			  default:
					return new EnterpriseGraphDatabase( storeDir, config, dependencies );
			  }
		 }
	}

}