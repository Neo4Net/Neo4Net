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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using Neo4Net.Helpers.Collections;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.helpers.DataCreator.createLabelledNodesWithProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.scenarios.ReadReplicaToReadReplicaCatchupIT.checkDataHasReplicatedToReadReplicas;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;

	public class ReadReplicaHierarchicalCatchupIT
	{
		 private IDictionary<int, string> _serverGroups = new Dictionary<int, string>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _serverGroups[0] = "NORTH";
			  _serverGroups[1] = "NORTH";
			  _serverGroups[2] = "NORTH";

			  _serverGroups[3] = "EAST";
			  _serverGroups[5] = "EAST";

			  _serverGroups[4] = "WEST";
			  _serverGroups[6] = "WEST";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.causalclustering.ClusterRule clusterRule = new org.Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.cluster_topology_refresh, "5s").withSharedCoreParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.multi_dc_license, "true").withSharedReadReplicaParam(org.Neo4Net.causalclustering.core.CausalClusteringSettings.multi_dc_license, "true").withDiscoveryServiceType(EnterpriseDiscoveryServiceType.HAZELCAST);
		 public ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(CausalClusteringSettings.cluster_topology_refresh, "5s").withSharedCoreParam(CausalClusteringSettings.multi_dc_license, "true").withSharedReadReplicaParam(CausalClusteringSettings.multi_dc_license, "true").withDiscoveryServiceType(EnterpriseDiscoveryServiceType.Hazelcast);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCatchupThroughHierarchy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCatchupThroughHierarchy()
		 {
			  ClusterRule = ClusterRule.withInstanceReadReplicaParam( CausalClusteringSettings.server_groups, id => _serverGroups[id] ).withInstanceCoreParam( CausalClusteringSettings.server_groups, id => _serverGroups[id] );

			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  int numberOfNodesToCreate = 100;

			  cluster.CoreTx((db, tx) =>
			  {
				Db.schema().constraintFor(label("Foo")).assertPropertyIsUnique("foobar").create();
				tx.success();
			  });

			  // 0, 1, 2 are core instances
			  createLabelledNodesWithProperty( cluster, numberOfNodesToCreate, label( "Foo" ), () => Pair.of("foobar", string.Format("baz_bat{0}", System.Guid.randomUUID())) );

			  // 3, 4 are other DCs
			  ReadReplica east3 = cluster.AddReadReplicaWithId( 3 );
			  east3.Start();
			  ReadReplica west4 = cluster.AddReadReplicaWithId( 4 );
			  west4.Start();

			  checkDataHasReplicatedToReadReplicas( cluster, numberOfNodesToCreate );

			  foreach ( CoreClusterMember coreClusterMember in cluster.CoreMembers() )
			  {
					coreClusterMember.DisableCatchupServer();
			  }

			  // 5, 6 are other DCs
			  ReadReplica east5 = cluster.AddReadReplicaWithId( 5 );
			  east5.UpstreamDatabaseSelectionStrategy = "connect-randomly-within-server-group";
			  east5.Start();
			  ReadReplica west6 = cluster.AddReadReplicaWithId( 6 );
			  west6.UpstreamDatabaseSelectionStrategy = "connect-randomly-within-server-group";
			  west6.Start();

			  checkDataHasReplicatedToReadReplicas( cluster, numberOfNodesToCreate );

		 }
	}

}