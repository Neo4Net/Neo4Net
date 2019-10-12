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
namespace Org.Neo4j.cluster.protocol.heartbeat
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using ObjectInputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using AcceptorInstanceStore = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.AcceptorInstanceStore;
	using MultiPaxosContext = Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context.MultiPaxosContext;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterContext = Org.Neo4j.cluster.protocol.cluster.ClusterContext;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionRole = Org.Neo4j.cluster.protocol.election.ElectionRole;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	/// <summary>
	/// Tests basic sanity and various scenarios for HeartbeatContext.
	/// All tests are performed from the perspective of instance running at 5001.
	/// </summary>
	public class HeartbeatContextTest
	{
		 private static InstanceId[] _instanceIds = new InstanceId[]
		 {
			 new InstanceId( 1 ),
			 new InstanceId( 2 ),
			 new InstanceId( 3 )
		 };

		 private static string[] _initialHosts = new string[]{ "cluster://localhost:5001", "cluster://localhost:5002", "cluster://localhost:5003" };

		 private HeartbeatContext _toTest;
		 private ClusterContext _context;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  for ( int i = 0; i < _instanceIds.Length; i++ )
			  {
					members[_instanceIds[i]] = URI.create( _initialHosts[i] );
			  }
			  ClusterConfiguration config = new ClusterConfiguration( "clusterName", NullLogProvider.Instance, _initialHosts );
			  config.Members = members;

			  _context = mock( typeof( ClusterContext ) );

			  Config configuration = mock( typeof( Config ) );
			  when( configuration.Get( ClusterSettings.max_acceptors ) ).thenReturn( 10 );

			  when( _context.Configuration ).thenReturn( config );
			  when( _context.MyId ).thenReturn( _instanceIds[0] );

			  MultiPaxosContext context = new MultiPaxosContext( _instanceIds[0], Iterables.iterable( new ElectionRole( "coordinator" ) ), config, Mockito.mock( typeof( Executor ) ), NullLogProvider.Instance, Mockito.mock( typeof( ObjectInputStreamFactory ) ), Mockito.mock( typeof( ObjectOutputStreamFactory ) ), Mockito.mock( typeof( AcceptorInstanceStore ) ), Mockito.mock( typeof( Timeouts ) ), mock( typeof( ElectionCredentialsProvider ) ), configuration );

			  _toTest = context.HeartbeatContext;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSaneInitialState()
		 public virtual void TestSaneInitialState()
		 {

			  // In config, not suspected yet
			  assertFalse( _toTest.alive( _instanceIds[0] ) );
			  // Not in config
			  assertFalse( _toTest.alive( new InstanceId( 4 ) ) );

			  // By default, instances start off as alive
			  assertEquals( _instanceIds.Length, Iterables.count( _toTest.Alive ) );
			  assertEquals( 0, _toTest.Failed.Count );

			  foreach ( InstanceId initialHost in _instanceIds )
			  {
					assertFalse( _toTest.isFailedBasedOnSuspicions( initialHost ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSuspicions()
		 public virtual void TestSuspicions()
		 {
			  InstanceId suspect = _instanceIds[1];
			  _toTest.suspect( suspect );
			  assertEquals( Collections.singleton( suspect ), _toTest.getSuspicionsFor( _context.MyId ) );
			  assertEquals( Collections.singletonList( _context.MyId ), _toTest.getSuspicionsOf( suspect ) );
			  // Being suspected by just one (us) is not enough
			  assertFalse( _toTest.isFailedBasedOnSuspicions( suspect ) );
			  assertTrue( _toTest.alive( suspect ) ); // This resets the suspicion above

			  // If we suspect an instance twice in a row, it shouldn't change its status in any way.
			  _toTest.suspect( suspect );
			  _toTest.suspect( suspect );
			  assertEquals( Collections.singleton( suspect ), _toTest.getSuspicionsFor( _context.MyId ) );
			  assertEquals( Collections.singletonList( _context.MyId ), _toTest.getSuspicionsOf( suspect ) );
			  assertFalse( _toTest.isFailedBasedOnSuspicions( suspect ) );
			  assertTrue( _toTest.alive( suspect ) );

			  // The other one sends suspicions too
			  InstanceId newSuspiciousBastard = _instanceIds[2];
			  _toTest.suspicions( newSuspiciousBastard, Collections.singleton( suspect ) );
			  _toTest.suspect( suspect );
			  // Now two instances suspect it, it should be reported failed
			  assertEquals( Collections.singleton( suspect ), _toTest.getSuspicionsFor( _context.MyId ) );
			  assertEquals( Collections.singleton( suspect ), _toTest.getSuspicionsFor( newSuspiciousBastard ) );
			  IList<InstanceId> suspiciousBastards = new List<InstanceId>( 2 );
			  suspiciousBastards.Add( _context.MyId );
			  suspiciousBastards.Add( newSuspiciousBastard );
			  assertEquals( suspiciousBastards, _toTest.getSuspicionsOf( suspect ) );
			  assertTrue( _toTest.isFailedBasedOnSuspicions( suspect ) );
			  assertTrue( _toTest.alive( suspect ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailedInstanceReportingSuspicions()
		 public virtual void TestFailedInstanceReportingSuspicions()
		 {
			  InstanceId suspect = _instanceIds[1];
			  InstanceId newSuspiciousBastard = _instanceIds[2];
			  _toTest.suspicions( newSuspiciousBastard, Collections.singleton( suspect ) );
			  _toTest.suspect( suspect );

			  // Just make sure
			  assertTrue( _toTest.isFailedBasedOnSuspicions( suspect ) );

			  // Suspicions of a failed instance should be ignored
			  _toTest.suspicions( suspect, Collections.singleton( newSuspiciousBastard ) );
			  assertTrue( "Suspicions should have been ignored", _toTest.getSuspicionsOf( newSuspiciousBastard ).Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailedInstanceBecomingAlive()
		 public virtual void TestFailedInstanceBecomingAlive()
		 {
			  InstanceId suspect = _instanceIds[1];
			  InstanceId newSuspiciousBastard = _instanceIds[2];
			  _toTest.suspicions( newSuspiciousBastard, Collections.singleton( suspect ) );
			  _toTest.suspect( suspect );

			  // Just make sure
			  assertTrue( _toTest.isFailedBasedOnSuspicions( suspect ) );

			  // Ok, here it is. We received a heartbeat, so it is alive.
			  _toTest.alive( suspect );
			  // It must no longer be failed
			  assertFalse( _toTest.isFailedBasedOnSuspicions( suspect ) );

			  // Simulate us stopping receiving heartbeats again
			  _toTest.suspect( suspect );
			  assertTrue( _toTest.isFailedBasedOnSuspicions( suspect ) );

			  // Assume the other guy started receiving heartbeats first
			  _toTest.suspicions( newSuspiciousBastard, Collections.emptySet() );
			  assertFalse( _toTest.isFailedBasedOnSuspicions( suspect ) );
		 }

		 /// <summary>
		 /// Tests the following scenario:
		 /// Instance A (the one this test simulates) sees instance C down. B agrees.
		 /// Instance A sees instance B down.
		 /// Instance C starts responding again.
		 /// Instance A should now consider C alive.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testOneInstanceComesAliveAfterAllOtherFail()
		 public virtual void TestOneInstanceComesAliveAfterAllOtherFail()
		 {
			  InstanceId instanceB = _instanceIds[1];
			  InstanceId instanceC = _instanceIds[2];

			  // Both A and B consider C down
			  _toTest.suspect( instanceC );
			  _toTest.suspicions( instanceB, Collections.singleton( instanceC ) );
			  assertTrue( _toTest.isFailedBasedOnSuspicions( instanceC ) );

			  // A sees B as down
			  _toTest.suspect( instanceB );
			  assertTrue( _toTest.isFailedBasedOnSuspicions( instanceB ) );

			  // C starts responding again
			  assertTrue( _toTest.alive( instanceC ) );

			  assertFalse( _toTest.isFailedBasedOnSuspicions( instanceC ) );
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsultSuspicionsOnlyFromCurrentClusterMembers()
		 public virtual void ShouldConsultSuspicionsOnlyFromCurrentClusterMembers()
		 {
			  // Given
			  InstanceId notInCluster = new InstanceId( -1 ); // backup, for example
			  _toTest.suspicions( notInCluster, Iterables.asSet( Iterables.iterable( _instanceIds[1] ) ) );

			  // When
			  IList<InstanceId> suspicions = _toTest.getSuspicionsOf( _instanceIds[1] );

			  // Then
			  assertThat( suspicions.Count, CoreMatchers.equalTo( 0 ) );

		 }
	}

}