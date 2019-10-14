using System.Collections.Generic;

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
namespace Neo4Net.Kernel.ha.cluster.member
{
	using Test = org.junit.Test;


	using InstanceId = Neo4Net.cluster.InstanceId;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ClusterMembersTest
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterMembersTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_clusterMembers = new ClusterMembers( _observedClusterMembers, _stateMachine );
		}

		 private readonly ObservedClusterMembers _observedClusterMembers = mock( typeof( ObservedClusterMembers ) );
		 private readonly HighAvailabilityMemberStateMachine _stateMachine = mock( typeof( HighAvailabilityMemberStateMachine ) );
		 private ClusterMembers _clusterMembers;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void currentInstanceStateUpdated()
		 public virtual void CurrentInstanceStateUpdated()
		 {
			  ClusterMember currentInstance = CreateClusterMember( 1, HighAvailabilityModeSwitcher.UNKNOWN );

			  when( _observedClusterMembers.AliveMembers ).thenReturn( Collections.singletonList( currentInstance ) );
			  when( _observedClusterMembers.CurrentMember ).thenReturn( currentInstance );
			  when( _stateMachine.CurrentState ).thenReturn( HighAvailabilityMemberState.MASTER );

			  ClusterMember self = _clusterMembers.CurrentMember;
			  assertEquals( HighAvailabilityModeSwitcher.MASTER, self.HARole );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aliveMembersWithValidCurrentInstanceState()
		 public virtual void AliveMembersWithValidCurrentInstanceState()
		 {
			  ClusterMember currentInstance = CreateClusterMember( 1, HighAvailabilityModeSwitcher.UNKNOWN );
			  ClusterMember otherInstance = CreateClusterMember( 2, HighAvailabilityModeSwitcher.SLAVE );
			  IList<ClusterMember> members = Arrays.asList( currentInstance, otherInstance );

			  when( _observedClusterMembers.AliveMembers ).thenReturn( members );
			  when( _observedClusterMembers.CurrentMember ).thenReturn( currentInstance );
			  when( _stateMachine.CurrentState ).thenReturn( HighAvailabilityMemberState.MASTER );

			  IEnumerable<ClusterMember> currentMembers = _clusterMembers.AliveMembers;

			  assertEquals( "Only active members should be available", 2, Iterables.count( currentMembers ) );
			  assertEquals( 1, CountInstancesWithRole( currentMembers, HighAvailabilityModeSwitcher.MASTER ) );
			  assertEquals( 1, CountInstancesWithRole( currentMembers, HighAvailabilityModeSwitcher.SLAVE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void observedStateDoesNotKnowCurrentInstance()
		 public virtual void ObservedStateDoesNotKnowCurrentInstance()
		 {
			  ClusterMember currentInstance = CreateClusterMember( 1, HighAvailabilityModeSwitcher.SLAVE );
			  ClusterMember otherInstance = CreateClusterMember( 2, HighAvailabilityModeSwitcher.MASTER );
			  IList<ClusterMember> members = Arrays.asList( currentInstance, otherInstance );

			  when( _observedClusterMembers.Members ).thenReturn( members );
			  when( _observedClusterMembers.CurrentMember ).thenReturn( null );
			  when( _stateMachine.CurrentState ).thenReturn( HighAvailabilityMemberState.SLAVE );

			  assertNull( _clusterMembers.CurrentMember );
			  assertEquals( members, _clusterMembers.Members );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incorrectlyObservedCurrentInstanceStateUpdated()
		 public virtual void IncorrectlyObservedCurrentInstanceStateUpdated()
		 {
			  ClusterMember currentInstance = CreateClusterMember( 1, HighAvailabilityModeSwitcher.SLAVE );
			  ClusterMember otherInstance = CreateClusterMember( 2, HighAvailabilityModeSwitcher.MASTER );
			  IList<ClusterMember> members = Arrays.asList( currentInstance, otherInstance );

			  when( _observedClusterMembers.Members ).thenReturn( members );
			  when( _observedClusterMembers.CurrentMember ).thenReturn( currentInstance );
			  when( _stateMachine.CurrentState ).thenReturn( HighAvailabilityMemberState.MASTER );

			  IEnumerable<ClusterMember> currentMembers = _clusterMembers.Members;

			  assertEquals( "All members should be available", 2, Iterables.count( currentMembers ) );
			  assertEquals( 2, CountInstancesWithRole( currentMembers, HighAvailabilityModeSwitcher.MASTER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void currentMemberHasCorrectRoleWhenInPendingState()
		 public virtual void CurrentMemberHasCorrectRoleWhenInPendingState()
		 {
			  ClusterMember member = CreateClusterMember( 1, HighAvailabilityModeSwitcher.MASTER );

			  when( _observedClusterMembers.CurrentMember ).thenReturn( member );
			  when( _stateMachine.CurrentState ).thenReturn( HighAvailabilityMemberState.PENDING );

			  assertEquals( HighAvailabilityModeSwitcher.UNKNOWN, _clusterMembers.CurrentMemberRole );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void currentMemberHasCorrectRoleWhenInToSlaveState()
		 public virtual void CurrentMemberHasCorrectRoleWhenInToSlaveState()
		 {
			  ClusterMember member = CreateClusterMember( 1, HighAvailabilityModeSwitcher.MASTER );

			  when( _observedClusterMembers.CurrentMember ).thenReturn( member );
			  when( _stateMachine.CurrentState ).thenReturn( HighAvailabilityMemberState.TO_SLAVE );

			  assertEquals( HighAvailabilityModeSwitcher.UNKNOWN, _clusterMembers.CurrentMemberRole );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void currentMemberHasCorrectRoleWhenInToMasterState()
		 public virtual void CurrentMemberHasCorrectRoleWhenInToMasterState()
		 {
			  ClusterMember member = CreateClusterMember( 1, HighAvailabilityModeSwitcher.MASTER );

			  when( _observedClusterMembers.CurrentMember ).thenReturn( member );
			  when( _stateMachine.CurrentState ).thenReturn( HighAvailabilityMemberState.TO_MASTER );

			  assertEquals( HighAvailabilityModeSwitcher.UNKNOWN, _clusterMembers.CurrentMemberRole );
		 }

		 private static int CountInstancesWithRole( IEnumerable<ClusterMember> currentMembers, string role )
		 {
			  int counter = 0;
			  foreach ( ClusterMember clusterMember in currentMembers )
			  {
					if ( role.Equals( clusterMember.HARole ) )
					{
						 counter++;
					}
			  }
			  return counter;
		 }

		 private static ClusterMember CreateClusterMember( int id, string role )
		 {
			  ClusterMember member = new ClusterMember( new InstanceId( id ) );
			  return member.AvailableAs( role, null, StoreId.DEFAULT );
		 }
	}

}