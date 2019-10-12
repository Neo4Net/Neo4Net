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
namespace Org.Neo4j.causalclustering.discovery
{
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public class SharedDiscoveryServiceFactory : DiscoveryServiceFactory
	{

		 private readonly SharedDiscoveryService _discoveryService = new SharedDiscoveryService();

		 public override CoreTopologyService CoreTopologyService( Config config, MemberId myself, JobScheduler jobScheduler, LogProvider logProvider, LogProvider userLogProvider, RemoteMembersResolver remoteMembersResolver, TopologyServiceRetryStrategy topologyServiceRetryStrategy, Monitors monitors )
		 {
			  return new SharedDiscoveryCoreClient( _discoveryService, myself, logProvider, config );
		 }

		 public override TopologyService ReadReplicaTopologyService( Config config, LogProvider logProvider, JobScheduler jobScheduler, MemberId myself, RemoteMembersResolver remoteMembersResolver, TopologyServiceRetryStrategy topologyServiceRetryStrategy )
		 {
			  return new SharedDiscoveryReadReplicaClient( _discoveryService, config, myself, logProvider );
		 }

	}

}