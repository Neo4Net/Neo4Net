﻿using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.routing.load_balancing.procedure
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;


	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using CoreTopologyService = Neo4Net.causalclustering.discovery.CoreTopologyService;
	using ReadReplicaTopology = Neo4Net.causalclustering.discovery.ReadReplicaTopology;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.discovery.TestTopology.addressesForCore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.NullLogProvider.getInstance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class GetServersProcedureV1RoutingTest
	public class GetServersProcedureV1RoutingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<Object> data()
		 public static ICollection<object> Data()
		 {
			  return Arrays.asList( 1, 2 );
		 } //the write endpoints are always index 0

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public int serverClass;
		 public int ServerClass;

		 private ClusterId _clusterId = new ClusterId( System.Guid.randomUUID() );
		 private Config _config = Config.defaults();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEndpointsInDifferentOrders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnEndpointsInDifferentOrders()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.discovery.CoreTopologyService coreTopologyService = mock(org.Neo4Net.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService coreTopologyService = mock( typeof( CoreTopologyService ) );

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenReturn( member( 0 ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  coreMembers[member( 0 )] = addressesForCore( 0, false );
			  coreMembers[member( 1 )] = addressesForCore( 1, false );
			  coreMembers[member( 2 )] = addressesForCore( 2, false );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.discovery.CoreTopology clusterTopology = new org.Neo4Net.causalclustering.discovery.CoreTopology(clusterId, false, coreMembers);
			  CoreTopology clusterTopology = new CoreTopology( _clusterId, false, coreMembers );
			  when( coreTopologyService.LocalCoreServers() ).thenReturn(clusterTopology);
			  when( coreTopologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(emptyMap()));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LegacyGetServersProcedure proc = new LegacyGetServersProcedure(coreTopologyService, leaderLocator, config, getInstance());
			  LegacyGetServersProcedure proc = new LegacyGetServersProcedure( coreTopologyService, leaderLocator, _config, Instance );

			  // when
			  object[] endpoints = GetEndpoints( proc );

			  //then
			  object[] endpointsInDifferentOrder = GetEndpoints( proc );
			  for ( int i = 0; i < 100; i++ )
			  {
					if ( Arrays.deepEquals( endpointsInDifferentOrder, endpoints ) )
					{
						 endpointsInDifferentOrder = GetEndpoints( proc );
					}
					else
					{
						 //Different order of servers, no need to retry.
						 break;
					}
			  }
			  assertFalse( Arrays.deepEquals( endpoints, endpointsInDifferentOrder ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Object[] getEndpoints(LegacyGetServersProcedure proc) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 private object[] GetEndpoints( LegacyGetServersProcedure proc )
		 {
			  IList<object[]> results = new IList<object[]> { proc.Apply( null, new object[0], null ) };
			  object[] rows = results[0];
			  IList<IDictionary<string, object[]>> servers = ( IList<IDictionary<string, object[]>> ) rows[1];
			  IDictionary<string, object[]> endpoints = servers[ServerClass];
			  return endpoints["addresses"];
		 }
	}

}