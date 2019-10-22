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
namespace Neo4Net.ha.correctness
{
	using Test = org.junit.Test;

	using InstanceId = Neo4Net.cluster.InstanceId;
	using Neo4Net.cluster.com.message;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ha.correctness.ClusterInstance.newClusterInstance;

	public class TestProver
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aClusterSnapshotShouldEqualItsOrigin() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AClusterSnapshotShouldEqualItsOrigin()
		 {
			  // Given
			  ClusterConfiguration config = new ClusterConfiguration( "default", NullLogProvider.Instance, "cluster://localhost:5001", "cluster://localhost:5002", "cluster://localhost:5003" );

			  ClusterState state = new ClusterState( new IList<ClusterInstance> { newClusterInstance( new InstanceId( 1 ), new URI( "cluster://localhost:5001" ), new Monitors(), config, 10, NullLogProvider.Instance ), newClusterInstance(new InstanceId(2), new URI("cluster://localhost:5002"), new Monitors(), config, 10, NullLogProvider.Instance), newClusterInstance(new InstanceId(3), new URI("cluster://localhost:5003"), new Monitors(), config, 10, NullLogProvider.Instance) }, emptySet() );

			  // When
			  ClusterState snapshot = state.Snapshot();

			  // Then
			  assertEquals( state, snapshot );
			  assertEquals( state.GetHashCode(), snapshot.GetHashCode() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoStatesWithSameSetupAndPendingMessagesShouldBeEqual() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TwoStatesWithSameSetupAndPendingMessagesShouldBeEqual()
		 {
			  // Given
			  ClusterConfiguration config = new ClusterConfiguration( "default", NullLogProvider.Instance, "cluster://localhost:5001", "cluster://localhost:5002", "cluster://localhost:5003" );

			  ClusterState state = new ClusterState( new IList<ClusterInstance> { newClusterInstance( new InstanceId( 1 ), new URI( "cluster://localhost:5001" ), new Monitors(), config, 10, NullLogProvider.Instance ), newClusterInstance(new InstanceId(2), new URI("cluster://localhost:5002"), new Monitors(), config, 10, NullLogProvider.Instance), newClusterInstance(new InstanceId(3), new URI("cluster://localhost:5003"), new Monitors(), config, 10, NullLogProvider.Instance) }, emptySet() );

			  // When
			  ClusterState firstState = state.PerformAction(new MessageDeliveryAction(Message.to(ClusterMessage.join, new URI("cluster://localhost:5002"), new object[]
			  {
				  "defaultcluster", new URI[]{ new URI( "cluster://localhost:5003" ) }
			  }).setHeader( Message.HEADER_CONVERSATION_ID, "-1" ).setHeader( Message.HEADER_FROM, "cluster://localhost:5002" )));
			  ClusterState secondState = state.PerformAction(new MessageDeliveryAction(Message.to(ClusterMessage.join, new URI("cluster://localhost:5002"), new object[]
			  {
				  "defaultcluster", new URI[]{ new URI( "cluster://localhost:5003" ) }
			  }).setHeader( Message.HEADER_CONVERSATION_ID, "-1" ).setHeader( Message.HEADER_FROM, "cluster://localhost:5002" )));

			  // Then
			  assertEquals( firstState, secondState );
			  assertEquals( firstState.GetHashCode(), secondState.GetHashCode() );
		 }

	}

}