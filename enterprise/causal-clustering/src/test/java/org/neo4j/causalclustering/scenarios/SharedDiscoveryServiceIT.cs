using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Test = org.junit.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using CoreTopologyService = Org.Neo4j.causalclustering.discovery.CoreTopologyService;
	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using InitialDiscoveryMembersResolver = Org.Neo4j.causalclustering.discovery.InitialDiscoveryMembersResolver;
	using NoOpHostnameResolver = Org.Neo4j.causalclustering.discovery.NoOpHostnameResolver;
	using RaftCoreTopologyConnector = Org.Neo4j.causalclustering.discovery.RaftCoreTopologyConnector;
	using SharedDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.SharedDiscoveryServiceFactory;
	using TopologyServiceNoRetriesStrategy = Org.Neo4j.causalclustering.discovery.TopologyServiceNoRetriesStrategy;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

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
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

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
			  JobScheduler jobScheduler = createInitialisedScheduler();
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