/*
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
namespace Org.Neo4j.causalclustering.readreplica
{

	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using Dependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;

	public class ReadReplicaGraphDatabase : GraphDatabaseFacade
	{
		 protected internal ReadReplicaGraphDatabase()
		 {
		 }

		 public ReadReplicaGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies, DiscoveryServiceFactory discoveryServiceFactory ) : this( storeDir, config, dependencies, discoveryServiceFactory, new MemberId( System.Guid.randomUUID() ) )
		 {
		 }

		 public ReadReplicaGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies, DiscoveryServiceFactory discoveryServiceFactory, MemberId memberId )
		 {
			  System.Func<PlatformModule, AbstractEditionModule> factory = platformModule => new EnterpriseReadReplicaEditionModule( platformModule, discoveryServiceFactory, memberId );
			  ( new GraphDatabaseFacadeFactory( DatabaseInfo.READ_REPLICA, factory ) ).initFacade( storeDir, config, dependencies, this );
		 }
	}

}