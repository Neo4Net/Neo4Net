using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.ClusterProtocolAtomicbroadcastTestUtil.ids;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.ClusterProtocolAtomicbroadcastTestUtil.members;

	public class HeartbeatContextImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAndAliveBothNotifyHeartbeatListenerInDelayedDirectExecutor()
		 public virtual void ShouldFailAndAliveBothNotifyHeartbeatListenerInDelayedDirectExecutor()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId failedMachine = new InstanceId( 2 );
			  InstanceId goodMachine = new InstanceId( 3 );

			  Timeouts timeouts = mock( typeof( Timeouts ) );

			  CommonContextState commonState = mock( typeof( CommonContextState ) );
			  ClusterConfiguration configuration = mock( typeof( ClusterConfiguration ) );
			  when( commonState.Configuration() ).thenReturn(configuration);
			  when( configuration.Members ).thenReturn( members( 3 ) );
			  when( configuration.MemberIds ).thenReturn( ids( 3 ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Runnable> runnables = new java.util.ArrayList<>();
			  IList<ThreadStart> runnables = new List<ThreadStart>();
			  HeartbeatContext context = new HeartbeatContextImpl( me, commonState, NullLogProvider.Instance, timeouts, new DelayedDirectExecutorAnonymousInnerClass( this, NullLogProvider.Instance, runnables ) );
			  context.AddHeartbeatListener( mock( typeof( HeartbeatListener ) ) );

			  context.Suspicions( goodMachine, new HashSet<InstanceId>( singletonList( failedMachine ) ) );
			  context.Suspect( failedMachine ); // fail
			  context.Alive( failedMachine ); // alive

			  // Then
			  assertEquals( 2, runnables.Count ); // fail + alive
		 }

		 private class DelayedDirectExecutorAnonymousInnerClass : DelayedDirectExecutor
		 {
			 private readonly HeartbeatContextImplTest _outerInstance;

			 private IList<ThreadStart> _runnables;

			 public DelayedDirectExecutorAnonymousInnerClass( HeartbeatContextImplTest outerInstance, NullLogProvider getInstance, IList<ThreadStart> runnables ) : base( getInstance )
			 {
				 this.outerInstance = outerInstance;
				 this._runnables = runnables;
			 }

			 public override void execute( ThreadStart command )
			 {
				 lock ( this )
				 {
					  _runnables.Add( command );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAllInstancesIfAllOtherInstancesAreSuspected()
		 public virtual void ShouldFailAllInstancesIfAllOtherInstancesAreSuspected()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId member2 = new InstanceId( 2 );
			  InstanceId member3 = new InstanceId( 3 );

			  Timeouts timeouts = mock( typeof( Timeouts ) );

			  CommonContextState commonState = mock( typeof( CommonContextState ) );
			  ClusterConfiguration configuration = mock( typeof( ClusterConfiguration ) );
			  when( commonState.Configuration() ).thenReturn(configuration);
			  when( configuration.Members ).thenReturn( members( 3 ) );
			  when( configuration.MemberIds ).thenReturn( ids( 3 ) );

			  DelayedDirectExecutor executor = new DelayedDirectExecutor( NullLogProvider.Instance );
			  HeartbeatContext context = new HeartbeatContextImpl( me, commonState, NullLogProvider.Instance, timeouts, executor );

			  IList<InstanceId> failed = new List<InstanceId>( 2 );
			  HeartbeatListener listener = new HeartbeatListenerAnonymousInnerClass( this, failed );

			  context.AddHeartbeatListener( listener );

			  // when
			  // just one suspicion comes, no extra failing action should be taken
			  context.Suspect( member2 );
			  executor.Drain();

			  // then
			  assertEquals( 0, failed.Count );

			  // when
			  // the other instance is suspected, all instances must be marked as failed
			  context.Suspect( member3 );
			  executor.Drain();

			  // then
			  assertEquals( 2, failed.Count );
			  assertTrue( failed.Contains( member2 ) );
			  assertTrue( failed.Contains( member3 ) );

			  // when
			  // one of them comes alive again, only that instance should be marked as alive
			  context.Alive( member2 );
			  executor.Drain();

			  // then
			  assertEquals( 1, failed.Count );
			  assertTrue( failed.Contains( member3 ) );
		 }

		 private class HeartbeatListenerAnonymousInnerClass : HeartbeatListener
		 {
			 private readonly HeartbeatContextImplTest _outerInstance;

			 private IList<InstanceId> _failed;

			 public HeartbeatListenerAnonymousInnerClass( HeartbeatContextImplTest outerInstance, IList<InstanceId> failed )
			 {
				 this.outerInstance = outerInstance;
				 this._failed = failed;
			 }

			 public void failed( InstanceId server )
			 {
				  _failed.Add( server );
			 }

			 public void alive( InstanceId server )
			 {
				  _failed.Remove( server );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void majorityOfNonSuspectedInstancesShouldBeEnoughToMarkAnInstanceAsFailed()
		 public virtual void MajorityOfNonSuspectedInstancesShouldBeEnoughToMarkAnInstanceAsFailed()
		 {
			  // Given
			  InstanceId me = new InstanceId( 1 );
			  InstanceId member2 = new InstanceId( 2 );
			  InstanceId member3 = new InstanceId( 3 );
			  InstanceId member4 = new InstanceId( 4 );
			  InstanceId member5 = new InstanceId( 5 );

			  Timeouts timeouts = mock( typeof( Timeouts ) );

			  CommonContextState commonState = mock( typeof( CommonContextState ) );
			  ClusterConfiguration configuration = mock( typeof( ClusterConfiguration ) );
			  when( commonState.Configuration() ).thenReturn(configuration);
			  when( configuration.Members ).thenReturn( members( 5 ) );
			  when( configuration.MemberIds ).thenReturn( ids( 5 ) );

			  DelayedDirectExecutor executor = new DelayedDirectExecutor( NullLogProvider.Instance );
			  HeartbeatContext context = new HeartbeatContextImpl( me, commonState, NullLogProvider.Instance, timeouts, executor );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.cluster.InstanceId> failed = new java.util.ArrayList<>(4);
			  IList<InstanceId> failed = new List<InstanceId>( 4 );
			  HeartbeatListener listener = new HeartbeatListenerAnonymousInnerClass2( this, failed );

			  context.AddHeartbeatListener( listener );

			  // when
			  // just two suspicions come, no extra failing action should be taken since this is not majority
			  context.Suspect( member2 );
			  context.Suspect( member3 );
			  executor.Drain();

			  // then
			  assertEquals( 0, failed.Count );

			  // when
			  // the another instance suspects them, therefore have a majority of non suspected, then 2 and 3 must fail
			  ISet<InstanceId> suspicionsFrom5 = new HashSet<InstanceId>();
			  suspicionsFrom5.Add( member2 );
			  suspicionsFrom5.Add( member3 );
			  context.Suspicions( member5, suspicionsFrom5 );
			  executor.Drain();

			  // then
			  assertEquals( 2, failed.Count );
			  assertTrue( failed.Contains( member2 ) );
			  assertTrue( failed.Contains( member3 ) );

			  // when
			  // an instance sends a heartbeat, it should be set as alive
			  context.Alive( member2 );
			  executor.Drain();

			  // then
			  assertEquals( 1, failed.Count );
			  assertTrue( failed.Contains( member3 ) );
		 }

		 private class HeartbeatListenerAnonymousInnerClass2 : HeartbeatListener
		 {
			 private readonly HeartbeatContextImplTest _outerInstance;

			 private IList<InstanceId> _failed;

			 public HeartbeatListenerAnonymousInnerClass2( HeartbeatContextImplTest outerInstance, IList<InstanceId> failed )
			 {
				 this.outerInstance = outerInstance;
				 this._failed = failed;
			 }

			 public void failed( InstanceId server )
			 {
				  _failed.Add( server );
			 }

			 public void alive( InstanceId server )
			 {
				  _failed.Remove( server );
			 }
		 }
	}

}