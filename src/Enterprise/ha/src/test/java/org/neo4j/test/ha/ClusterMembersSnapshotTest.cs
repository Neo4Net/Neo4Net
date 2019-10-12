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
namespace Neo4Net.Test.ha
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Test = org.junit.Test;


	using InstanceId = Neo4Net.cluster.InstanceId;
	using MemberIsAvailable = Neo4Net.cluster.member.paxos.MemberIsAvailable;
	using PaxosClusterMemberEvents = Neo4Net.cluster.member.paxos.PaxosClusterMemberEvents;
	using ClusterMembersSnapshot = Neo4Net.cluster.member.paxos.PaxosClusterMemberEvents.ClusterMembersSnapshot;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using HANewSnapshotFunction = Neo4Net.Kernel.ha.cluster.HANewSnapshotFunction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.StoreId.DEFAULT;

	public class ClusterMembersSnapshotTest
	{
		 private const string URI = "http://me";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotListPrunesSameMemberOnIdenticalAvailabilityEvents() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SnapshotListPrunesSameMemberOnIdenticalAvailabilityEvents()
		 {
			  // GIVEN
			  // -- a snapshot containing one member with a role
			  PaxosClusterMemberEvents.ClusterMembersSnapshot snapshot = new PaxosClusterMemberEvents.ClusterMembersSnapshot(new PaxosClusterMemberEvents.UniqueRoleFilter()
			 );
			  URI clusterUri = new URI( URI );
			  InstanceId instanceId = new InstanceId( 1 );
			  MemberIsAvailable memberIsAvailable = new MemberIsAvailable( MASTER, instanceId, clusterUri, new URI( URI + "?something" ), DEFAULT );
			  snapshot.AvailableMember( memberIsAvailable );

			  // WHEN
			  // -- the same member and role gets added to the snapshot
			  snapshot.AvailableMember( memberIsAvailable );

			  // THEN
			  // -- getting the snapshot list should only reveal the last one
			  assertEquals( 1, Iterables.count( snapshot.GetCurrentAvailable( instanceId ) ) );
			  assertThat( snapshot.GetCurrentAvailable( instanceId ), hasItem( memberIsAvailable( memberIsAvailable ) ) );
			  assertEquals( 1, Iterables.count( snapshot.CurrentAvailableMembers ) );
			  assertThat( snapshot.CurrentAvailableMembers, hasItems( memberIsAvailable( memberIsAvailable ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotListShouldContainOnlyOneEventForARoleWithTheSameIdWhenSwitchingFromMasterToSlave() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SnapshotListShouldContainOnlyOneEventForARoleWithTheSameIdWhenSwitchingFromMasterToSlave()
		 {
			  // GIVEN
			  // -- a snapshot containing one member with a role
			  PaxosClusterMemberEvents.ClusterMembersSnapshot snapshot = new PaxosClusterMemberEvents.ClusterMembersSnapshot( new HANewSnapshotFunction() );
			  URI clusterUri = new URI( URI );
			  InstanceId instanceId = new InstanceId( 1 );
			  MemberIsAvailable event1 = new MemberIsAvailable( MASTER, instanceId, clusterUri, new URI( URI + "?something" ), DEFAULT );
			  snapshot.AvailableMember( event1 );

			  // WHEN
			  // -- the same member, although different role, gets added to the snapshot
			  MemberIsAvailable event2 = new MemberIsAvailable( SLAVE, instanceId, clusterUri, new URI( URI + "?something" ), DEFAULT );
			  snapshot.AvailableMember( event2 );

			  // THEN
			  // -- getting the snapshot list should reveal both
			  assertEquals( 1, Iterables.count( snapshot.GetCurrentAvailable( instanceId ) ) );
			  assertThat( snapshot.GetCurrentAvailable( instanceId ), hasItems( MemberIsAvailable( event2 ) ) );
			  assertEquals( 1, Iterables.count( snapshot.CurrentAvailableMembers ) );
			  assertThat( snapshot.CurrentAvailableMembers, hasItems( MemberIsAvailable( event2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotListShouldContainOnlyOneEventForARoleWithTheSameIdWhenSwitchingFromSlaveToMaster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SnapshotListShouldContainOnlyOneEventForARoleWithTheSameIdWhenSwitchingFromSlaveToMaster()
		 {
			  // GIVEN
			  // -- a snapshot containing one member with a role
			  PaxosClusterMemberEvents.ClusterMembersSnapshot snapshot = new PaxosClusterMemberEvents.ClusterMembersSnapshot( new HANewSnapshotFunction() );
			  URI clusterUri = new URI( URI );
			  InstanceId instanceId = new InstanceId( 1 );
			  MemberIsAvailable event1 = new MemberIsAvailable( SLAVE, instanceId, clusterUri, new URI( URI + "?something" ), DEFAULT );
			  snapshot.AvailableMember( event1 );

			  // WHEN
			  // -- the same member, although different role, gets added to the snapshot
			  MemberIsAvailable event2 = new MemberIsAvailable( MASTER, instanceId, clusterUri, new URI( URI + "?something" ), DEFAULT );
			  snapshot.AvailableMember( event2 );

			  // THEN
			  // -- getting the snapshot list should reveal both
			  assertEquals( 1, Iterables.count( snapshot.GetCurrentAvailable( instanceId ) ) );
			  assertThat( snapshot.GetCurrentAvailable( instanceId ), hasItems( MemberIsAvailable( event2 ) ) );
			  assertEquals( 1, Iterables.count( snapshot.CurrentAvailableMembers ) );
			  assertThat( snapshot.CurrentAvailableMembers, hasItems( MemberIsAvailable( event2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotListPrunesOtherMemberWithSameMasterRole() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SnapshotListPrunesOtherMemberWithSameMasterRole()
		 {
			  // GIVEN
			  // -- a snapshot containing one member with a role
			  PaxosClusterMemberEvents.ClusterMembersSnapshot snapshot = new PaxosClusterMemberEvents.ClusterMembersSnapshot( new HANewSnapshotFunction() );
			  URI clusterUri = new URI( URI );
			  InstanceId instanceId = new InstanceId( 1 );
			  MemberIsAvailable @event = new MemberIsAvailable( MASTER, instanceId, clusterUri, new URI( URI + "?something1" ), DEFAULT );
			  snapshot.AvailableMember( @event );

			  // WHEN
			  // -- another member, but with same role, gets added to the snapshot
			  URI otherClusterUri = new URI( URI );
			  InstanceId otherInstanceId = new InstanceId( 2 );
			  MemberIsAvailable otherEvent = new MemberIsAvailable( MASTER, otherInstanceId, otherClusterUri, new URI( URI + "?something2" ), DEFAULT );
			  snapshot.AvailableMember( otherEvent );

			  // THEN
			  // -- getting the snapshot list should only reveal the last member added, as it had the same role
			  assertEquals( 1, Iterables.count( snapshot.GetCurrentAvailable( otherInstanceId ) ) );
			  assertThat( snapshot.GetCurrentAvailable( otherInstanceId ), hasItems( MemberIsAvailable( otherEvent ) ) );
			  assertEquals( 1, Iterables.count( snapshot.CurrentAvailableMembers ) );
			  assertThat( snapshot.CurrentAvailableMembers, hasItems( MemberIsAvailable( otherEvent ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotListDoesNotPruneOtherMemberWithSlaveRole() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SnapshotListDoesNotPruneOtherMemberWithSlaveRole()
		 {
			  // GIVEN
			  // -- a snapshot containing one member with a role
			  PaxosClusterMemberEvents.ClusterMembersSnapshot snapshot = new PaxosClusterMemberEvents.ClusterMembersSnapshot( new HANewSnapshotFunction() );
			  URI clusterUri = new URI( URI );
			  InstanceId instanceId = new InstanceId( 1 );
			  MemberIsAvailable @event = new MemberIsAvailable( SLAVE, instanceId, clusterUri, new URI( URI + "?something1" ), DEFAULT );
			  snapshot.AvailableMember( @event );

			  // WHEN
			  // -- another member, but with same role, gets added to the snapshot
			  URI otherClusterUri = new URI( URI );
			  InstanceId otherInstanceId = new InstanceId( 2 );
			  MemberIsAvailable otherEvent = new MemberIsAvailable( SLAVE, otherInstanceId, otherClusterUri, new URI( URI + "?something2" ), DEFAULT );
			  snapshot.AvailableMember( otherEvent );

			  // THEN
			  assertEquals( 2, Iterables.count( snapshot.CurrentAvailableMembers ) );
			  assertThat( snapshot.CurrentAvailableMembers, hasItems( MemberIsAvailable( @event ), MemberIsAvailable( otherEvent ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.hamcrest.Matcher<org.neo4j.cluster.member.paxos.MemberIsAvailable> memberIsAvailable(final org.neo4j.cluster.member.paxos.MemberIsAvailable expected)
		 private static Matcher<MemberIsAvailable> MemberIsAvailable( MemberIsAvailable expected )
		 {
			  return new BaseMatcherAnonymousInnerClass( expected );
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<MemberIsAvailable>
		 {
			 private MemberIsAvailable _expected;

			 public BaseMatcherAnonymousInnerClass( MemberIsAvailable expected )
			 {
				 this._expected = expected;
			 }

			 public override bool matches( object item )
			 {
				  MemberIsAvailable input = ( MemberIsAvailable ) item;
				  return Objects.Equals( input.ClusterUri, _expected.ClusterUri ) && Objects.Equals( input.Role, _expected.Role ) && Objects.Equals( input.RoleUri, _expected.RoleUri );
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }
	}

}