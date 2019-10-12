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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context
{
	using Test = org.junit.Test;

	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using HeartbeatContext = Neo4Net.cluster.protocol.heartbeat.HeartbeatContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.ClusterProtocolAtomicbroadcastTestUtil.ids;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.ClusterProtocolAtomicbroadcastTestUtil.members;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class AtomicBroadcastContextImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHasQuorumWhenTwoMachinesAliveInAClusterWithThreeMachines()
		 public virtual void ShouldHasQuorumWhenTwoMachinesAliveInAClusterWithThreeMachines()
		 {
			  //Given
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  CommonContextState commonState = mock( typeof( CommonContextState ) );
			  ClusterConfiguration configuration = mock( typeof( ClusterConfiguration ) );

			  when( heartbeatContext.Alive ).thenReturn( ids( 2 ) );
			  when( commonState.Configuration() ).thenReturn(configuration);
			  when( configuration.Members ).thenReturn( members( 3 ) );

			  AtomicBroadcastContextImpl context = new AtomicBroadcastContextImpl( null, commonState, null, null, null, heartbeatContext ); // we do not care about other args
			  //When
			  bool hasQuorum = context.HasQuorum();
			  //Then
			  assertTrue( hasQuorum );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHasNoQuorumWhenOneMachineAliveInAClusterWithThreeMachines()
		 public virtual void ShouldHasNoQuorumWhenOneMachineAliveInAClusterWithThreeMachines()
		 {
			  //Given
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  CommonContextState commonState = mock( typeof( CommonContextState ) );
			  ClusterConfiguration configuration = mock( typeof( ClusterConfiguration ) );

			  when( heartbeatContext.Alive ).thenReturn( ids( 1 ) );
			  when( commonState.Configuration() ).thenReturn(configuration);
			  when( configuration.Members ).thenReturn( members( 3 ) );

			  AtomicBroadcastContextImpl context = new AtomicBroadcastContextImpl( null, commonState, null, null, null, heartbeatContext ); // we do not care about other args
			  //When
			  bool hasQuorum = context.HasQuorum();
			  //Then
			  assertFalse( hasQuorum );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHasQuorumWhenOneMachineAliveInAClusterWithOneMachine()
		 public virtual void ShouldHasQuorumWhenOneMachineAliveInAClusterWithOneMachine()
		 {
			  //Given
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  CommonContextState commonState = mock( typeof( CommonContextState ) );
			  ClusterConfiguration configuration = mock( typeof( ClusterConfiguration ) );

			  when( heartbeatContext.Alive ).thenReturn( ids( 1 ) );
			  when( commonState.Configuration() ).thenReturn(configuration);
			  when( configuration.Members ).thenReturn( members( 1 ) );

			  AtomicBroadcastContextImpl context = new AtomicBroadcastContextImpl( null, commonState, null, null, null, heartbeatContext ); // we do not care about other args
			  //When
			  bool hasQuorum = context.HasQuorum();
			  //Then
			  assertTrue( hasQuorum );
		 }
	}

}