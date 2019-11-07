using System;
using System.Collections.Generic;

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
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using CoreTopologyService = Neo4Net.causalclustering.discovery.CoreTopologyService;
	using DiscoveryServiceFactory = Neo4Net.causalclustering.discovery.DiscoveryServiceFactory;
	using InitialDiscoveryMembersResolver = Neo4Net.causalclustering.discovery.InitialDiscoveryMembersResolver;
	using NoOpHostnameResolver = Neo4Net.causalclustering.discovery.NoOpHostnameResolver;
	using RaftCoreTopologyConnector = Neo4Net.causalclustering.discovery.RaftCoreTopologyConnector;
	using SharedDiscoveryServiceFactory = Neo4Net.causalclustering.discovery.SharedDiscoveryServiceFactory;
	using TopologyServiceNoRetriesStrategy = Neo4Net.causalclustering.discovery.TopologyServiceNoRetriesStrategy;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeastOnce;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertEventually;

	public class SharedDiscoveryServiceIT
	{
		 private const long TIMEOUT_MS = 15_000;
		 private const long RUN_TIME_MS = 1000;

		 private NullLogProvider _logProvider = NullLogProvider.Instance;
		 private NullLogProvider _userLogProvider = NullLogProvider.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TIMEOUT_MS) public void shouldDiscoverCompleteTargetSetWithoutDeadlocks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDiscoverCompleteTargetSetWithoutDeadlocks()
		 {
			  // given
			  ExecutorService es = Executors.newCachedThreadPool();

			  long endTimeMillis = DateTimeHelper.CurrentUnixTimeMillis() + RUN_TIME_MS;
			  while ( endTimeMillis > DateTimeHelper.CurrentUnixTimeMillis() )
			  {
					ISet<MemberId> members = new HashSet<MemberId>();
					for ( int i = 0; i < 3; i++ )
					{
						 members.Add( new MemberId( System.Guid.randomUUID() ) );
					}

					DiscoveryServiceFactory sharedService = new SharedDiscoveryServiceFactory();

					IList<Callable<Void>> discoveryJobs = new List<Callable<Void>>();
					foreach ( MemberId member in members )
					{
						 discoveryJobs.Add( CreateDiscoveryJob( member, sharedService, members ) );
					}

					IList<Future<Void>> results = es.invokeAll( discoveryJobs );
					foreach ( Future<Void> result in results )
					{
						 result.get( TIMEOUT_MS, MILLISECONDS );
					}
			  }
		 }

		 private Callable<Void> CreateDiscoveryJob( MemberId member, DiscoveryServiceFactory discoveryServiceFactory, ISet<MemberId> expectedTargetSet )
		 {
			  IJobScheduler jobScheduler = createInitializedScheduler();
			  Config config = config();
			  InitialDiscoveryMembersResolver remoteMemberResolver = new InitialDiscoveryMembersResolver( new NoOpHostnameResolver(), config );

			  CoreTopologyService topologyService = discoveryServiceFactory.CoreTopologyService( config, member, jobScheduler, _logProvider, _userLogProvider, remoteMemberResolver, new TopologyServiceNoRetriesStrategy(), new Monitors() );
			  return SharedClientStarter( topologyService, expectedTargetSet );
		 }

		 private Config Config()
		 {
			  return Config.defaults( stringMap( CausalClusteringSettings.raft_advertised_address.name(), "127.0.0.1:7000", CausalClusteringSettings.transaction_advertised_address.name(), "127.0.0.1:7001", (new BoltConnector("bolt")).enabled.name(), "true", (new BoltConnector("bolt")).advertised_address.name(), "127.0.0.1:7002" ) );
		 }

		 private Callable<Void> SharedClientStarter( CoreTopologyService topologyService, ISet<MemberId> expectedTargetSet )
		 {
			  return () =>
			  {
				try
				{
					 RaftMachine raftMock = mock( typeof( RaftMachine ) );
					 RaftCoreTopologyConnector tc = new RaftCoreTopologyConnector( topologyService, raftMock, CausalClusteringSettings.database.DefaultValue );
					 topologyService.init();
					 topologyService.start();
					 tc.start();

					 assertEventually("should discover complete target set", () =>
					 {
						  ArgumentCaptor<ISet<MemberId>> targetMembers = ArgumentCaptor.forClass( ( Type<ISet<MemberId>> ) expectedTargetSet.GetType() );
						  verify( raftMock, atLeastOnce() ).TargetMembershipSet = targetMembers.capture();
						  return targetMembers.Value;
					 }, equalTo( expectedTargetSet ), TIMEOUT_MS, MILLISECONDS);
				}
				catch ( Exception throwable )
				{
					 fail( throwable.Message );
				}
				return null;
			  };
		 }
	}

}