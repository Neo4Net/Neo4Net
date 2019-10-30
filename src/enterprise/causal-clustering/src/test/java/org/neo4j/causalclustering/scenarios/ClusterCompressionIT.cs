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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using Neo4Net.causalclustering.protocol;
	using Protocol_ModifierProtocols = Neo4Net.causalclustering.protocol.Protocol_ModifierProtocols;
	using Neo4Net.Collections.Helpers;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.compression_implementations;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.discovery.Cluster.dataMatchesEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.helpers.DataCreator.countNodes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.helpers.DataCreator.createLabelledNodesWithProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ClusterCompressionIT
	public class ClusterCompressionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol modifierProtocol;
		 public Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol ModifierProtocol;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<org.Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol> params()
		 public static ICollection<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol> Params()
		 {
			  return Arrays.asList( Protocol_ModifierProtocols.values() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.causalclustering.ClusterRule clusterRule = new org.Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3).withTimeout(1000, SECONDS);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3).withTimeout(1000, SECONDS);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateWithCompression() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateWithCompression()
		 {
			  // given
			  ClusterRule.withSharedCoreParam( compression_implementations, ModifierProtocol.implementation() ).withSharedReadReplicaParam(compression_implementations, ModifierProtocol.implementation());

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();

				// when
			  int numberOfNodes = 10;
			  CoreClusterMember leader = createLabelledNodesWithProperty( cluster, numberOfNodes, label( "Foo" ), () => Pair.of("foobar", format("baz_bat%s", System.Guid.randomUUID())) );

			  // then
			  assertEquals( numberOfNodes, countNodes( leader ) );
			  dataMatchesEventually( leader, cluster.CoreMembers() );
		 }
	}

}