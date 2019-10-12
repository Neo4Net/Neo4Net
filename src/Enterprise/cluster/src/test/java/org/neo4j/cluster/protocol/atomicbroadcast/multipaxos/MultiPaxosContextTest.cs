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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{
	using Test = org.junit.Test;


	using MultiPaxosContext = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ElectionCredentialsProvider = Neo4Net.cluster.protocol.election.ElectionCredentialsProvider;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class MultiPaxosContextTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotConsiderInstanceJoiningWithSameIdAndIpAProblem() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotConsiderInstanceJoiningWithSameIdAndIpAProblem()
		 {
			  // Given

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext ctx = new MultiPaxosContext( new InstanceId( 1 ), Collections.emptyList(), mock(typeof(ClusterConfiguration)), mock(typeof(Executor)), NullLogProvider.Instance, new ObjectStreamFactory(), new ObjectStreamFactory(), mock(typeof(AcceptorInstanceStore)), mock(typeof(Timeouts)), mock(typeof(ElectionCredentialsProvider)), config );

			  InstanceId joiningId = new InstanceId( 12 );
			  string joiningUri = "http://127.0.0.1:900";

			  // When
			  ctx.ClusterContext.instanceIsJoining( joiningId, new URI( joiningUri ) );

			  // Then
			  assertFalse( ctx.ClusterContext.isInstanceJoiningFromDifferentUri( joiningId, new URI( joiningUri ) ) );
			  assertTrue( ctx.ClusterContext.isInstanceJoiningFromDifferentUri( joiningId, new URI( "http://127.0.0.1:80" ) ) );
			  assertFalse( ctx.ClusterContext.isInstanceJoiningFromDifferentUri( new InstanceId( 13 ), new URI( joiningUri ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeepClone()
		 public virtual void ShouldDeepClone()
		 {
			  // Given
			  ObjectStreamFactory objStream = new ObjectStreamFactory();
			  AcceptorInstanceStore acceptorInstances = mock( typeof( AcceptorInstanceStore ) );
			  Executor executor = mock( typeof( Executor ) );
			  Timeouts timeouts = mock( typeof( Timeouts ) );
			  ClusterConfiguration clusterConfig = new ClusterConfiguration( "myCluster", NullLogProvider.Instance );
			  ElectionCredentialsProvider electionCredentials = mock( typeof( ElectionCredentialsProvider ) );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext ctx = new MultiPaxosContext( new InstanceId( 1 ), Collections.emptyList(), clusterConfig, executor, NullLogProvider.Instance, objStream, objStream, acceptorInstances, timeouts, electionCredentials, config );

			  // When
			  MultiPaxosContext snapshot = ctx.Snapshot( NullLogProvider.Instance, timeouts, executor, acceptorInstances, objStream, objStream, electionCredentials );

			  // Then
			  assertEquals( ctx, snapshot );
		 }
	}

}