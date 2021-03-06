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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Test = org.junit.Test;

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using CoreTopologyService = Org.Neo4j.causalclustering.discovery.CoreTopologyService;
	using InitialDiscoveryMembersResolver = Org.Neo4j.causalclustering.discovery.InitialDiscoveryMembersResolver;
	using NoOpHostnameResolver = Org.Neo4j.causalclustering.discovery.NoOpHostnameResolver;
	using TopologyServiceNoRetriesStrategy = Org.Neo4j.causalclustering.discovery.TopologyServiceNoRetriesStrategy;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.initial_discovery_members;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	public abstract class BaseCoreTopologyServiceIT
	{
		 private readonly DiscoveryServiceType _discoveryServiceType;

		 protected internal BaseCoreTopologyServiceIT( DiscoveryServiceType discoveryServiceType )
		 {
			  this._discoveryServiceType = discoveryServiceType;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 120_000) public void shouldBeAbleToStartAndStopWithoutSuccessfulJoin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToStartAndStopWithoutSuccessfulJoin()
		 {
			  // Random members that does not exists, discovery will never succeed
			  string initialHosts = "localhost:" + PortAuthority.allocatePort() + ",localhost:" + PortAuthority.allocatePort();
			  Config config = Config.defaults();
			  config.augment( initial_discovery_members, initialHosts );
			  config.Augment( CausalClusteringSettings.discovery_listen_address, "localhost:" + PortAuthority.allocatePort() );

			  JobScheduler jobScheduler = createInitialisedScheduler();
			  InitialDiscoveryMembersResolver initialDiscoveryMemberResolver = new InitialDiscoveryMembersResolver( new NoOpHostnameResolver(), config );

			  CoreTopologyService service = _discoveryServiceType.createFactory().coreTopologyService(config, new MemberId(System.Guid.randomUUID()), jobScheduler, NullLogProvider.Instance, NullLogProvider.Instance, initialDiscoveryMemberResolver, new TopologyServiceNoRetriesStrategy(), new Monitors());
			  service.init();
			  service.start();
			  service.stop();
			  service.shutdown();
		 }

	}

}