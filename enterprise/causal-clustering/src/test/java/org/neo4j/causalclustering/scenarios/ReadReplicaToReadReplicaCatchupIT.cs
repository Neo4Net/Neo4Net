﻿using System;

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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Org.Neo4j.causalclustering.discovery.ReadReplica;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using UpstreamDatabaseSelectionStrategy = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseSelectionStrategy;
	using Org.Neo4j.Function;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Service = Org.Neo4j.Helpers.Service;
	using Org.Neo4j.Helpers.Collection;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.helpers.DataCreator.createLabelledNodesWithProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.scenarios.ReadReplicaToReadReplicaCatchupIT.SpecificReplicaStrategy.upstreamFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class ReadReplicaToReadReplicaCatchupIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.cluster_topology_refresh, "5s").withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.multi_dc_license, "true").withSharedReadReplicaParam(org.neo4j.causalclustering.core.CausalClusteringSettings.multi_dc_license, "true").withDiscoveryServiceType(EnterpriseDiscoveryServiceType.HAZELCAST);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(CausalClusteringSettings.cluster_topology_refresh, "5s").withSharedCoreParam(CausalClusteringSettings.multi_dc_license, "true").withSharedReadReplicaParam(CausalClusteringSettings.multi_dc_license, "true").withDiscoveryServiceType(EnterpriseDiscoveryServiceType.Hazelcast);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEventuallyPullTransactionAcrossReadReplicas() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEventuallyPullTransactionAcrossReadReplicas()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  int numberOfNodesToCreate = 100;

			  cluster.CoreTx((db, tx) =>
			  {
				Db.schema().constraintFor(label("Foo")).assertPropertyIsUnique("foobar").create();
				tx.success();
			  });

			  createLabelledNodesWithProperty( cluster, numberOfNodesToCreate, label( "Foo" ), () => Pair.of("foobar", string.Format("baz_bat{0}", System.Guid.randomUUID())) );

			  ReadReplica firstReadReplica = cluster.AddReadReplicaWithIdAndMonitors( 101, new Monitors() );

			  firstReadReplica.Start();

			  CheckDataHasReplicatedToReadReplicas( cluster, numberOfNodesToCreate );

			  foreach ( CoreClusterMember coreClusterMember in cluster.CoreMembers() )
			  {
					coreClusterMember.DisableCatchupServer();
			  }

			  // when
			  upstreamFactory.Current = firstReadReplica;
			  ReadReplica secondReadReplica = cluster.AddReadReplicaWithId( 202 );
			  secondReadReplica.UpstreamDatabaseSelectionStrategy = "specific";

			  secondReadReplica.Start();

			  // then

			  CheckDataHasReplicatedToReadReplicas( cluster, numberOfNodesToCreate );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCatchUpFromCoresWhenPreferredReadReplicasAreUnavailable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCatchUpFromCoresWhenPreferredReadReplicasAreUnavailable()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  int numberOfNodes = 1;
			  int firstReadReplicaLocalMemberId = 101;

			  cluster.CoreTx((db, tx) =>
			  {
				Db.schema().constraintFor(label("Foo")).assertPropertyIsUnique("foobar").create();
				tx.success();
			  });

			  createLabelledNodesWithProperty( cluster, numberOfNodes, label( "Foo" ), () => Pair.of("foobar", string.Format("baz_bat{0}", System.Guid.randomUUID())) );

			  ReadReplica firstReadReplica = cluster.AddReadReplicaWithIdAndMonitors( firstReadReplicaLocalMemberId, new Monitors() );

			  firstReadReplica.Start();

			  CheckDataHasReplicatedToReadReplicas( cluster, numberOfNodes );

			  upstreamFactory.Current = firstReadReplica;

			  ReadReplica secondReadReplica = cluster.AddReadReplicaWithId( 202 );
			  secondReadReplica.UpstreamDatabaseSelectionStrategy = "specific";

			  secondReadReplica.Start();

			  CheckDataHasReplicatedToReadReplicas( cluster, numberOfNodes );

			  firstReadReplica.Shutdown();
			  upstreamFactory.reset();

			  cluster.RemoveReadReplicaWithMemberId( firstReadReplicaLocalMemberId );

			  // when
			  // More transactions into core
			  createLabelledNodesWithProperty( cluster, numberOfNodes, label( "Foo" ), () => Pair.of("foobar", string.Format("baz_bat{0}", System.Guid.randomUUID())) );

			  // then
			  // reached second read replica from cores
			  CheckDataHasReplicatedToReadReplicas( cluster, numberOfNodes * 2 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void checkDataHasReplicatedToReadReplicas(org.neo4j.causalclustering.discovery.Cluster<?> cluster, long numberOfNodes) throws Exception
		 internal static void CheckDataHasReplicatedToReadReplicas<T1>( Cluster<T1> cluster, long numberOfNodes )
		 {
			  foreach ( ReadReplica server in cluster.ReadReplicas() )
			  {
					GraphDatabaseService readReplica = server.database();
					using ( Transaction tx = readReplica.BeginTx() )
					{
						 ThrowingSupplier<long, Exception> nodeCount = () => count(readReplica.AllNodes);
						 assertEventually( "node to appear on read replica", nodeCount, @is( numberOfNodes ), 1, MINUTES );

						 foreach ( Node node in readReplica.AllNodes )
						 {
							  assertThat( node.GetProperty( "foobar" ).ToString(), startsWith("baz_bat") );
						 }

						 tx.Success();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public static class SpecificReplicaStrategy extends org.neo4j.causalclustering.upstream.UpstreamDatabaseSelectionStrategy
		 public class SpecificReplicaStrategy : UpstreamDatabaseSelectionStrategy
		 {
			  // This because we need a stable point for config to inject into Service loader loaded classes
			  internal static readonly UpstreamFactory UpstreamFactory = new UpstreamFactory();

			  public SpecificReplicaStrategy() : base("specific")
			  {
			  }

			  public override Optional<MemberId> UpstreamDatabase()
			  {
					ReadReplica current = UpstreamFactory.current();
					if ( current == null )
					{
						 return null;
					}
					else
					{
						 return current.MemberId();
					}
			  }
		 }

		 private class UpstreamFactory
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal ReadReplica CurrentConflict;

			  public virtual ReadReplica Current
			  {
				  set
				  {
						this.CurrentConflict = value;
				  }
			  }

			  public virtual ReadReplica Current()
			  {
					return CurrentConflict;
			  }

			  internal virtual void Reset()
			  {
					CurrentConflict = null;
			  }
		 }
	}

}