﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Server.database
{

	using CoreGraphDatabase = Org.Neo4j.causalclustering.core.CoreGraphDatabase;
	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using EnterpriseDiscoveryServiceFactorySelector = Org.Neo4j.causalclustering.discovery.EnterpriseDiscoveryServiceFactorySelector;
	using ReadReplicaGraphDatabase = Org.Neo4j.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using EnterpriseGraphDatabase = Org.Neo4j.Kernel.enterprise.EnterpriseGraphDatabase;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;

	using static Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;

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