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
namespace Neo4Net.Kernel.ha.cluster
{
	using Test = org.junit.Test;


	using OnlineBackupKernelExtension = Neo4Net.backup.OnlineBackupKernelExtension;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using MemberIsAvailable = Neo4Net.cluster.member.paxos.MemberIsAvailable;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.SLAVE;

	public class HANewSnapshotFunctionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void normalClusterCreationShouldBePassedUnchanged()
		 public virtual void NormalClusterCreationShouldBePassedUnchanged()
		 {
			  // GIVEN
			  // This is what the end result should look like
			  IList<MemberIsAvailable> events = new LinkedList<MemberIsAvailable>();
			  events.Add( RoleForId( MASTER, 1 ) );
			  events.Add( RoleForId( SLAVE, 2 ) );
			  events.Add( RoleForId( SLAVE, 3 ) );

			  // WHEN events start getting added
			  IEnumerable<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
			  foreach ( MemberIsAvailable @event in events )
			  {
					result = ( new HANewSnapshotFunction() ).Apply(result, @event);
			  }

			  // THEN the result is the expected one
			  EventsMatch( result, events );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void duplicateSlaveEventsShouldBeFilteredOut()
		 public virtual void DuplicateSlaveEventsShouldBeFilteredOut()
		 {
			  // GIVEN
			  // This is the list of events
			  IList<MemberIsAvailable> events = new LinkedList<MemberIsAvailable>();
			  events.Add( RoleForId( MASTER, 1 ) );
			  events.Add( RoleForId( SLAVE, 2 ) );
			  events.Add( RoleForId( SLAVE, 3 ) );
			  events.Add( RoleForId( SLAVE, 2 ) );
			  events.Add( RoleForId( SLAVE, 3 ) );
			  // This is what it should look like
			  IList<MemberIsAvailable> expected = new LinkedList<MemberIsAvailable>();
			  expected.Add( RoleForId( MASTER, 1 ) );
			  expected.Add( RoleForId( SLAVE, 2 ) );
			  expected.Add( RoleForId( SLAVE, 3 ) );

			  // WHEN events start getting added
			  IEnumerable<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
			  foreach ( MemberIsAvailable @event in events )
			  {
					result = ( new HANewSnapshotFunction() ).Apply(result, @event);
			  }

			  // THEN the result should be the same as the one above
			  EventsMatch( result, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void instanceBeingMasterReappearsAsSlaveShouldBeTreatedAsSlave()
		 public virtual void InstanceBeingMasterReappearsAsSlaveShouldBeTreatedAsSlave()
		 {
			  // GIVEN these events
			  IList<MemberIsAvailable> events = new LinkedList<MemberIsAvailable>();
			  events.Add( RoleForId( MASTER, 1 ) );
			  events.Add( RoleForId( SLAVE, 2 ) );
			  events.Add( RoleForId( SLAVE, 1 ) );
			  events.Add( RoleForId( SLAVE, 3 ) );
			  // and this expected outcome
			  IList<MemberIsAvailable> expected = new LinkedList<MemberIsAvailable>();
			  expected.Add( RoleForId( SLAVE, 2 ) );
			  expected.Add( RoleForId( SLAVE, 1 ) );
			  expected.Add( RoleForId( SLAVE, 3 ) );

			  // WHEN events start getting added
			  IEnumerable<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
			  foreach ( MemberIsAvailable @event in events )
			  {
					result = ( new HANewSnapshotFunction() ).Apply(result, @event);
			  }

			  // THEN the result should be the expected one
			  EventsMatch( result, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void instanceBeingSlaveReappearsAsMasterShouldBeTreatedAsMaster()
		 public virtual void InstanceBeingSlaveReappearsAsMasterShouldBeTreatedAsMaster()
		 {
			  // GIVEN these events
			  IList<MemberIsAvailable> events = new LinkedList<MemberIsAvailable>();
			  events.Add( RoleForId( SLAVE, 2 ) );
			  events.Add( RoleForId( SLAVE, 1 ) );
			  events.Add( RoleForId( MASTER, 1 ) );
			  events.Add( RoleForId( SLAVE, 3 ) );
			  // and this expected outcome
			  IList<MemberIsAvailable> expected = new LinkedList<MemberIsAvailable>();
			  expected.Add( RoleForId( SLAVE, 2 ) );
			  expected.Add( RoleForId( MASTER, 1 ) );
			  expected.Add( RoleForId( SLAVE, 3 ) );

			  // WHEN events start getting added
			  IEnumerable<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
			  foreach ( MemberIsAvailable @event in events )
			  {
					result = ( new HANewSnapshotFunction() ).Apply(result, @event);
			  }

			  // THEN the result should be the expected one
			  EventsMatch( result, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void instanceBeingMasterReplacedByAnotherInstanceShouldNotRemainMaster()
		 public virtual void InstanceBeingMasterReplacedByAnotherInstanceShouldNotRemainMaster()
		 {
			  // GIVEN these events
			  IList<MemberIsAvailable> events = new LinkedList<MemberIsAvailable>();
			  events.Add( RoleForId( MASTER, 1 ) );
			  events.Add( RoleForId( MASTER, 2 ) );
			  events.Add( RoleForId( SLAVE, 3 ) );
			  // and this expected outcome
			  IList<MemberIsAvailable> expected = new LinkedList<MemberIsAvailable>();
			  expected.Add( RoleForId( MASTER, 2 ) );
			  expected.Add( RoleForId( SLAVE, 3 ) );

			  // WHEN events start getting added
			  IEnumerable<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
			  foreach ( MemberIsAvailable @event in events )
			  {
					result = ( new HANewSnapshotFunction() ).Apply(result, @event);
			  }

			  // THEN the result should be the expected one
			  EventsMatch( result, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void instanceBeingBackupReplacedByAnotherInstanceShouldNotRemainBackup()
		 public virtual void InstanceBeingBackupReplacedByAnotherInstanceShouldNotRemainBackup()
		 {
			  // GIVEN these events
			  IList<MemberIsAvailable> events = new LinkedList<MemberIsAvailable>();
			  events.Add( RoleForId( OnlineBackupKernelExtension.BACKUP, 1 ) );
			  events.Add( RoleForId( MASTER, 2 ) );
			  events.Add( RoleForId( OnlineBackupKernelExtension.BACKUP, 2 ) );
			  events.Add( RoleForId( SLAVE, 3 ) );
			  // and this expected outcome
			  IList<MemberIsAvailable> expected = new LinkedList<MemberIsAvailable>();
			  expected.Add( RoleForId( MASTER, 2 ) );
			  expected.Add( RoleForId( OnlineBackupKernelExtension.BACKUP, 2 ) );
			  expected.Add( RoleForId( SLAVE, 3 ) );

			  // WHEN events start getting added
			  IEnumerable<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
			  foreach ( MemberIsAvailable @event in events )
			  {
					result = ( new HANewSnapshotFunction() ).Apply(result, @event);
			  }

			  // THEN the result should be the expected one
			  EventsMatch( result, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void instanceBeingBackupRepeatedlyShouldRemainBackupOnceOnly()
		 public virtual void InstanceBeingBackupRepeatedlyShouldRemainBackupOnceOnly()
		 {
			  // GIVEN these events
			  IList<MemberIsAvailable> events = new LinkedList<MemberIsAvailable>();
			  events.Add( RoleForId( OnlineBackupKernelExtension.BACKUP, 1 ) );
			  events.Add( RoleForId( OnlineBackupKernelExtension.BACKUP, 1 ) );
			  events.Add( RoleForId( OnlineBackupKernelExtension.BACKUP, 1 ) );
			  events.Add( RoleForId( OnlineBackupKernelExtension.BACKUP, 1 ) );
			  // and this expected outcome
			  IList<MemberIsAvailable> expected = new LinkedList<MemberIsAvailable>();
			  expected.Add( RoleForId( OnlineBackupKernelExtension.BACKUP, 1 ) );

			  // WHEN events start getting added
			  IEnumerable<MemberIsAvailable> result = new LinkedList<MemberIsAvailable>();
			  foreach ( MemberIsAvailable @event in events )
			  {
					result = ( new HANewSnapshotFunction() ).Apply(result, @event);
			  }

			  // THEN the result should be the expected one
			  EventsMatch( result, expected );
		 }

		 private MemberIsAvailable RoleForId( string role, int id )
		 {
			  return new MemberIsAvailable( role, new InstanceId( id ), URI.create( "cluster://" + id ), URI.create( "ha://" + id ), StoreId.DEFAULT );
		 }

		 private void EventsMatch( IEnumerable<MemberIsAvailable> result, IList<MemberIsAvailable> expected )
		 {
			  IEnumerator<MemberIsAvailable> iter = result.GetEnumerator();
			  foreach ( MemberIsAvailable anExpected in expected )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( anExpected, iter.next() );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iter.hasNext() );
		 }
	}

}