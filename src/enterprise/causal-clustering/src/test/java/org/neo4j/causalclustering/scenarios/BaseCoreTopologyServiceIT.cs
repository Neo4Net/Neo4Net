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
namespace Neo4Net.causalclustering.scenarios
{
	using Test = org.junit.Test;

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using CoreTopologyService = Neo4Net.causalclustering.discovery.CoreTopologyService;
	using InitialDiscoveryMembersResolver = Neo4Net.causalclustering.discovery.InitialDiscoveryMembersResolver;
	using NoOpHostnameResolver = Neo4Net.causalclustering.discovery.NoOpHostnameResolver;
	using TopologyServiceNoRetriesStrategy = Neo4Net.causalclustering.discovery.TopologyServiceNoRetriesStrategy;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.initial_discovery_members;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;

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

			  IJobScheduler jobScheduler = createInitializedScheduler();
			  InitialDiscoveryMembersResolver initialDiscoveryMemberResolver = new InitialDiscoveryMembersResolver( new NoOpHostnameResolver(), config );

			  CoreTopologyService service = _discoveryServiceType.createFactory().coreTopologyService(config, new MemberId(System.Guid.randomUUID()), jobScheduler, NullLogProvider.Instance, NullLogProvider.Instance, initialDiscoveryMemberResolver, new TopologyServiceNoRetriesStrategy(), new Monitors());
			  service.init();
			  service.start();
			  service.stop();
			  service.shutdown();
		 }

	}

}