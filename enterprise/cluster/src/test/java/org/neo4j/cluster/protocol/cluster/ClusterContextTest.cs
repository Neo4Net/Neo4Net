using System.Threading;

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
namespace Org.Neo4j.cluster.protocol.cluster
{
	using Test = org.junit.Test;

	using ObjectInputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using AcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using MultiPaxosContext = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ElectionContext = Org.Neo4j.cluster.protocol.election.ElectionContext;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionRole = Org.Neo4j.cluster.protocol.election.ElectionRole;
	using HeartbeatContext = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatContext;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ClusterContextTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionVersionIsUpdatedOnElectionFromSelfAndProperlyIgnoredIfOld()
		 public virtual void TestElectionVersionIsUpdatedOnElectionFromSelfAndProperlyIgnoredIfOld()
		 {
			  const string coordinatorRole = "coordinator";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId me = new org.neo4j.cluster.InstanceId(1);
			  InstanceId me = new InstanceId( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId winner = new org.neo4j.cluster.InstanceId(2);
			  InstanceId winner = new InstanceId( 2 );
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  when( heartbeatContext.Failed ).thenReturn( Collections.emptySet() );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext multiPaxosContext = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( coordinatorRole ) ), mock( typeof( ClusterConfiguration ) ), ThreadStart.run, NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );
			  ClusterContext context = multiPaxosContext.ClusterContext;
			  ElectionContext electionContext = multiPaxosContext.ElectionContext;

			  ClusterListener listener = mock( typeof( ClusterListener ) );
			  context.AddClusterListener( listener );

			  electionContext.ForgetElection( coordinatorRole );
			  long expectedVersion = electionContext.NewConfigurationStateChange().Version;
			  context.Elected( coordinatorRole, winner, me, expectedVersion );
			  assertEquals( 1, expectedVersion );
			  verify( listener, times( 1 ) ).elected( coordinatorRole, winner, null );

			  electionContext.ForgetElection( coordinatorRole );
			  expectedVersion = electionContext.NewConfigurationStateChange().Version;
			  context.Elected( coordinatorRole, winner, me, expectedVersion );
			  assertEquals( 2, expectedVersion );
			  verify( listener, times( 2 ) ).elected( coordinatorRole, winner, null );

			  context.Elected( coordinatorRole, winner, me, expectedVersion - 1 );
			  verifyNoMoreInteractions( listener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionVersionIsUpdatedOnElectionFromOtherAndIgnoredIfOld()
		 public virtual void TestElectionVersionIsUpdatedOnElectionFromOtherAndIgnoredIfOld()
		 {
			  const string coordinatorRole = "coordinator";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId me = new org.neo4j.cluster.InstanceId(1);
			  InstanceId me = new InstanceId( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId winner = new org.neo4j.cluster.InstanceId(2);
			  InstanceId winner = new InstanceId( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId elector = new org.neo4j.cluster.InstanceId(2);
			  InstanceId elector = new InstanceId( 2 );
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  when( heartbeatContext.Failed ).thenReturn( Collections.emptySet() );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext multiPaxosContext = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( coordinatorRole ) ), mock( typeof( ClusterConfiguration ) ), ThreadStart.run, NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );
			  ClusterContext context = multiPaxosContext.ClusterContext;

			  ClusterListener listener = mock( typeof( ClusterListener ) );
			  context.AddClusterListener( listener );

			  context.Elected( coordinatorRole, winner, elector, 2 );
			  verify( listener, times( 1 ) ).elected( coordinatorRole, winner, null );

			  context.Elected( coordinatorRole, winner, elector, 3 );
			  verify( listener, times( 2 ) ).elected( coordinatorRole, winner, null );

			  context.Elected( coordinatorRole, winner, elector, 2 );
			  verifyNoMoreInteractions( listener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionVersionIsResetWhenElectorChangesFromMeToOther()
		 public virtual void TestElectionVersionIsResetWhenElectorChangesFromMeToOther()
		 {
			  const string coordinatorRole = "coordinator";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId me = new org.neo4j.cluster.InstanceId(1);
			  InstanceId me = new InstanceId( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId winner = new org.neo4j.cluster.InstanceId(2);
			  InstanceId winner = new InstanceId( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId elector = new org.neo4j.cluster.InstanceId(2);
			  InstanceId elector = new InstanceId( 2 );
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  when( heartbeatContext.Failed ).thenReturn( Collections.emptySet() );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext multiPaxosContext = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( coordinatorRole ) ), mock( typeof( ClusterConfiguration ) ), ThreadStart.run, NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );
			  ClusterContext context = multiPaxosContext.ClusterContext;
			  ElectionContext electionContext = multiPaxosContext.ElectionContext;

			  ClusterListener listener = mock( typeof( ClusterListener ) );
			  context.LastElectorVersion = 5;
			  context.LastElector = me;
			  context.AddClusterListener( listener );

			  long expectedVersion = electionContext.NewConfigurationStateChange().Version;
			  context.Elected( coordinatorRole, winner, me, expectedVersion );
			  verify( listener, times( 1 ) ).elected( coordinatorRole, winner, null );

			  context.Elected( coordinatorRole, winner, elector, 2 );
			  verify( listener, times( 2 ) ).elected( coordinatorRole, winner, null );

			  context.Elected( coordinatorRole, winner, elector, 3 );
			  verify( listener, times( 3 ) ).elected( coordinatorRole, winner, null );

			  context.Elected( coordinatorRole, winner, elector, 2 );
			  verifyNoMoreInteractions( listener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testElectionVersionIsResetWhenElectorChangesFromOtherToMe()
		 public virtual void TestElectionVersionIsResetWhenElectorChangesFromOtherToMe()
		 {
			  const string coordinatorRole = "coordinator";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId me = new org.neo4j.cluster.InstanceId(1);
			  InstanceId me = new InstanceId( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId winner = new org.neo4j.cluster.InstanceId(2);
			  InstanceId winner = new InstanceId( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId elector = new org.neo4j.cluster.InstanceId(2);
			  InstanceId elector = new InstanceId( 2 );
			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  when( heartbeatContext.Failed ).thenReturn( Collections.emptySet() );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  MultiPaxosContext multiPaxosContext = new MultiPaxosContext( me, Iterables.iterable( new ElectionRole( coordinatorRole ) ), mock( typeof( ClusterConfiguration ) ), ThreadStart.run, NullLogProvider.Instance, mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), config );
			  ClusterContext context = multiPaxosContext.ClusterContext;
			  ElectionContext electionContext = multiPaxosContext.ElectionContext;

			  ClusterListener listener = mock( typeof( ClusterListener ) );
			  context.LastElectorVersion = 5;
			  context.LastElector = elector;
			  context.AddClusterListener( listener );

			  context.Elected( coordinatorRole, winner, elector, 6 );
			  verify( listener, times( 1 ) ).elected( coordinatorRole, winner, null );

			  electionContext.ForgetElection( coordinatorRole );
			  long expectedVersion = electionContext.NewConfigurationStateChange().Version;
			  context.Elected( coordinatorRole, winner, me, expectedVersion );
			  verify( listener, times( 2 ) ).elected( coordinatorRole, winner, null );
		 }
	}

}