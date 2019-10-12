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
namespace Neo4Net.causalclustering.scenarios
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using IpFamily = Neo4Net.causalclustering.discovery.IpFamily;
	using DataCreator = Neo4Net.causalclustering.helpers.DataCreator;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.Cluster.dataMatchesEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.helpers.DataCreator.countNodes;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class BaseClusterIpFamilyIT
	public abstract class BaseClusterIpFamilyIT
	{
		 protected internal BaseClusterIpFamilyIT( DiscoveryServiceType discoveryServiceType, IpFamily ipFamily, bool useWildcard )
		 {
			  ClusterRule.withDiscoveryServiceType( discoveryServiceType );
			  ClusterRule.withIpFamily( ipFamily ).useWildcard( useWildcard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3).withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.disable_middleware_logging, "false").withSharedReadReplicaParam(org.neo4j.causalclustering.core.CausalClusteringSettings.disable_middleware_logging, "false").withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.middleware_logging_level, "0").withSharedReadReplicaParam(org.neo4j.causalclustering.core.CausalClusteringSettings.middleware_logging_level, "0");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3).withSharedCoreParam(CausalClusteringSettings.disable_middleware_logging, "false").withSharedReadReplicaParam(CausalClusteringSettings.disable_middleware_logging, "false").withSharedCoreParam(CausalClusteringSettings.middleware_logging_level, "0").withSharedReadReplicaParam(CausalClusteringSettings.middleware_logging_level, "0");

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetupClusterWithIPv6() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetupClusterWithIPv6()
		 {
			  // given
			  int numberOfNodes = 10;

			  // when
			  CoreClusterMember leader = DataCreator.createEmptyNodes( _cluster, numberOfNodes );

			  // then
			  assertEquals( numberOfNodes, countNodes( leader ) );
			  dataMatchesEventually( leader, _cluster.coreMembers() );
			  dataMatchesEventually( leader, _cluster.readReplicas() );
		 }
	}

}