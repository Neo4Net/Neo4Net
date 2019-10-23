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
namespace Neo4Net.Kernel.ha.cluster
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterMemberEvents = Neo4Net.cluster.member.ClusterMemberEvents;
	using ClusterMemberListener = Neo4Net.cluster.member.ClusterMemberListener;
	using Election = Neo4Net.cluster.protocol.election.Election;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.cluster.HighAvailabilityMemberStateMachineTest.mockAddClusterMemberListener;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;

	/*
	 * These tests reproduce state transitions which are illegal. The general requirement for them is that they
	 * set the instance to PENDING state and ask for an election, in the hopes that the result will come with
	 * proper ordering and therefore cause a proper state transition chain to MASTER or SLAVE.
	 */
	public class HAStateMachineIllegalTransitionsTest
	{
		 private readonly InstanceId _me = new InstanceId( 1 );
		 private ClusterMemberListener _memberListener;
		 private HighAvailabilityMemberStateMachine _stateMachine;
		 private Election _election;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  HighAvailabilityMemberContext context = new SimpleHighAvailabilityMemberContext( _me, false );

			  ClusterMemberEvents events = mock( typeof( ClusterMemberEvents ) );
			  HighAvailabilityMemberStateMachineTest.ClusterMemberListenerContainer memberListenerContainer = mockAddClusterMemberListener( events );

			  _election = mock( typeof( Election ) );

			  _stateMachine = BuildMockedStateMachine( context, events, _election );
			  _stateMachine.init();
			  _memberListener = memberListenerContainer.Get();
			  HighAvailabilityMemberStateMachineTest.HAStateChangeListener probe = new HighAvailabilityMemberStateMachineTest.HAStateChangeListener();
			  _stateMachine.addHighAvailabilityMemberListener( probe );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleMasterAvailableWhenInPending()
		 public virtual void ShouldProperlyHandleMasterAvailableWhenInPending()
		 {
			  /*
			   * If the instance is in PENDING state, masterIsAvailable for itself should leave it to PENDING
			   * and ask for elections
			   */

			  // sanity check of starting state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );

			  // when
			  // It receives available master without having gone through TO_MASTER
			  _memberListener.memberIsAvailable( MASTER, _me, URI.create( "ha://whatever" ), StoreId.DEFAULT );

			  // then
			  AssertPendingStateAndElectionsAsked();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleSlaveAvailableWhenInPending()
		 public virtual void ShouldProperlyHandleSlaveAvailableWhenInPending()
		 {
			  /*
			   * If the instance is in PENDING state, slaveIsAvailable for itself should set it to PENDING
			   * and ask for elections
			   */
			  // sanity check of starting state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );

			  // when
			  // It receives available SLAVE without having gone through TO_SLAVE
			  _memberListener.memberIsAvailable( SLAVE, _me, URI.create( "ha://whatever" ), StoreId.DEFAULT );

			  // then
			  AssertPendingStateAndElectionsAsked();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleNonElectedMasterBecomingAvailableWhenInToSlave()
		 public virtual void ShouldProperlyHandleNonElectedMasterBecomingAvailableWhenInToSlave()
		 {
			  /*
			   * If the instance is in TO_SLAVE and a masterIsAvailable comes that does not refer to the elected master,
			   * the instance should go to PENDING and ask for elections
			   */
			  // Given
			  InstanceId other = new InstanceId( 2 );
			  InstanceId rogueMaster = new InstanceId( 3 );

			  // sanity check of starting state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );

			  // when
			  // It becomes available master without having gone through TO_MASTER
			  _memberListener.memberIsAvailable( MASTER, other, URI.create( "ha://whatever" ), StoreId.DEFAULT );

			  // sanity check it is TO_SLAVE
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToSlave ) );

			  // when
			  // it receives that another master became master but which is different than the currently elected one
			  _memberListener.memberIsAvailable( MASTER, rogueMaster, URI.create( "ha://fromNowhere" ), StoreId.DEFAULT );

			  // then
			  AssertPendingStateAndElectionsAsked();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleConflictingMasterAvailableMessage()
		 public virtual void ShouldProperlyHandleConflictingMasterAvailableMessage()
		 {
			  /*
			   * If the instance is currently in TO_MASTER and a masterIsAvailable comes for another instance, then
			   * this instance should transition to PENDING and ask for an election.
			   */
			  // Given
			  InstanceId rogue = new InstanceId( 2 );
			  // sanity check of starting state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );

			  // when
			  // It receives available master without having gone through TO_MASTER
			  _memberListener.coordinatorIsElected( _me );

			  // then
			  // sanity check it transitioned to TO_MASTER
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToMaster ) );

			  // when
			  // it receives a masterIsAvailable for another instance
			  _memberListener.memberIsAvailable( MASTER, rogue, URI.create( "ha://someUri" ), StoreId.DEFAULT );

			  // then
			  AssertPendingStateAndElectionsAsked();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleConflictingSlaveIsAvailableMessageWhenInToMaster()
		 public virtual void ShouldProperlyHandleConflictingSlaveIsAvailableMessageWhenInToMaster()
		 {
			  /*
			   * If the instance is in TO_MASTER state, slaveIsAvailable for itself should set it to PENDING
			   * and ask for elections
			   */
			  // Given
			  // sanity check of starting state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );

			  // when
			  // It receives available master without having gone through TO_MASTER
			  _memberListener.coordinatorIsElected( _me );

			  // then
			  // sanity check it transitioned to TO_MASTER
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToMaster ) );

			  // when
			  // it receives a masterIsAvailable for another instance
			  _memberListener.memberIsAvailable( SLAVE, _me, URI.create( "ha://someUri" ), StoreId.DEFAULT );

			  // then
			  AssertPendingStateAndElectionsAsked();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleConflictingSlaveIsAvailableWhenInMaster()
		 public virtual void ShouldProperlyHandleConflictingSlaveIsAvailableWhenInMaster()
		 {
			  /*
			   * If the instance is in MASTER state, slaveIsAvailable for itself should set it to PENDING
			   * and ask for elections
			   */
			  // sanity check of starting state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );

			  // when
			  // It receives available master without having gone through TO_MASTER
			  _memberListener.coordinatorIsElected( _me );

			  // then
			  // sanity check it transitioned to TO_MASTER
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToMaster ) );

			  // when
			  // it receives a masterIsAvailable for itself, completing the transition
			  _memberListener.memberIsAvailable( MASTER, _me, URI.create( "ha://someUri" ), StoreId.DEFAULT );

			  // then
			  // it should move to MASTER
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Master ) );

			  // when
			  // it receives a slaveIsAvailable for itself while in the MASTER state
			  _memberListener.memberIsAvailable( SLAVE, _me, URI.create( "ha://someUri" ), StoreId.DEFAULT );

			  // then
			  AssertPendingStateAndElectionsAsked();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleMasterIsAvailableWhenInMasterState()
		 public virtual void ShouldProperlyHandleMasterIsAvailableWhenInMasterState()
		 {
			  /*
			   * If the instance is in MASTER state and a masterIsAvailable is received for another instance, then
			   * this instance should got to PENDING and ask for elections
			   */
			  // Given
			  InstanceId rogue = new InstanceId( 2 );
			  // sanity check of starting state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );

			  // when
			  // It receives available master without having gone through TO_MASTER
			  _memberListener.coordinatorIsElected( _me );

			  // then
			  // sanity check it transitioned to TO_MASTER
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToMaster ) );

			  // when
			  // it receives a masterIsAvailable for itself, completing the transition
			  _memberListener.memberIsAvailable( MASTER, _me, URI.create( "ha://someUri" ), StoreId.DEFAULT );

			  // then
			  // it should move to MASTER
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Master ) );

			  // when
			  // it receives a slaveIsAvailable for itself while in the MASTER state
			  _memberListener.memberIsAvailable( MASTER, rogue, URI.create( "ha://someUri" ), StoreId.DEFAULT );

			  // then
			  AssertPendingStateAndElectionsAsked();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyHandleMasterIsAvailableWhenInSlaveState()
		 public virtual void ShouldProperlyHandleMasterIsAvailableWhenInSlaveState()
		 {
			  /*
			   * If the instance is in SLAVE state and receives masterIsAvailable for an instance different than the
			   * current master, it should revert to PENDING and ask for elections
			   */
			  // Given
			  InstanceId master = new InstanceId( 2 );
			  InstanceId rogueMaster = new InstanceId( 3 );
			  // sanity check of starting state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );

			  // when
			  // a master is elected normally
			  _memberListener.coordinatorIsElected( master );
			  _memberListener.memberIsAvailable( MASTER, master, URI.create( "ha://someUri" ), StoreId.DEFAULT );

			  // then
			  // sanity check it transitioned to TO_SLAVE
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.ToSlave ) );

			  // when
			  _memberListener.memberIsAvailable( SLAVE, _me, URI.create( "ha://myUri" ), StoreId.DEFAULT );

			  // then
			  // we should be in SLAVE state
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Slave ) );

			  // when
			  // it receives a masterIsAvailable for an unelected master while in the slave state
			  _memberListener.memberIsAvailable( MASTER, rogueMaster, URI.create( "ha://someOtherUri" ), StoreId.DEFAULT );

			  // then
			  AssertPendingStateAndElectionsAsked();
		 }

		 private void AssertPendingStateAndElectionsAsked()
		 {
			  // it should remain in pending
			  assertThat( _stateMachine.CurrentState, equalTo( HighAvailabilityMemberState.Pending ) );
			  // and it should ask for elections
			  verify( _election ).performRoleElections();
		 }

		 private HighAvailabilityMemberStateMachine BuildMockedStateMachine( HighAvailabilityMemberContext context, ClusterMemberEvents events, Election election )
		 {
			  return ( new HighAvailabilityMemberStateMachineTest.StateMachineBuilder() ).WithContext(context).withEvents(events).withElection(election).build();
		 }
	}

}