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
namespace Neo4Net.Kernel.ha.cluster
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using InstanceId = Neo4Net.cluster.InstanceId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.ILLEGAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.PENDING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.SLAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.TO_MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.TO_SLAVE;

	/// <summary>
	/// This is the full specification for state switching in HA according to incoming cluster
	/// messages. 5 states times 3 possible messages makes for 15 methods - each comes with 2 or 3 cases for the
	/// message and context contents.
	/// 
	/// No behaviour is examined here - interactions with context and actions taken are not tested. See other tests for that.
	/// </summary>
	public class HighAvailabilityMemberStateTest
	{
		 public static readonly URI SampleUri = URI.create( "ha://foo" );
		 private InstanceId _myId;
		 private HighAvailabilityMemberContext _context;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _myId = new InstanceId( 1 );
			  _context = mock( typeof( HighAvailabilityMemberContext ) );
			  when( _context.MyId ).thenReturn( _myId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPendingMasterIsElected()
		 public virtual void TestPendingMasterIsElected()
		 {
			  // CASE 1: Got MasterIsElected for me - should switch to TO_MASTER
			  HighAvailabilityMemberState newState = PENDING.masterIsElected( _context, _myId );
			  assertEquals( TO_MASTER, newState );

			  // CASE 2: Got MasterIsElected for someone else - should remain to PENDING
			  HighAvailabilityMemberState newStateCase2 = PENDING.masterIsElected( _context, new InstanceId( 2 ) );
			  assertEquals( PENDING, newStateCase2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPendingMasterIsAvailable()
		 public virtual void TestPendingMasterIsAvailable()
		 {
			  // CASE 1: Got MasterIsAvailable for me - should not happen
			  HighAvailabilityMemberState illegal = PENDING.masterIsAvailable( _context, _myId, SampleUri );
			  assertEquals( ILLEGAL, illegal );

			  // CASE 2: Got MasterIsAvailable for someone else - should transition to TO_SLAVE
			  // TODO test correct info is passed through to context
			  HighAvailabilityMemberState newState = PENDING.masterIsAvailable( _context, new InstanceId( 2 ), SampleUri );
			  assertEquals( TO_SLAVE, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPendingSlaveIsAvailable()
		 public virtual void TestPendingSlaveIsAvailable()
		 {
			  // CASE 1: Got SlaveIsAvailable for me - should not happen, that's what TO_SLAVE exists for
			  HighAvailabilityMemberState illegal = PENDING.slaveIsAvailable( _context, _myId, SampleUri );
			  assertEquals( ILLEGAL, illegal );

			  // CASE 2: Got SlaveIsAvailable for someone else - it's ok, remain in PENDING
			  HighAvailabilityMemberState newState = PENDING.slaveIsAvailable( _context, new InstanceId( 2 ), SampleUri );
			  assertEquals( PENDING, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testToMasterMasterIsElected()
		 public virtual void TestToMasterMasterIsElected()
		 {
			  // CASE 1: Got MasterIsElected for me - it's ok, continue in TO_MASTER
			  HighAvailabilityMemberState newState = TO_MASTER.masterIsElected( _context, _myId );
			  assertEquals( TO_MASTER, newState );

			  // CASE 2: Got MasterIsElected for someone else - switch to PENDING
			  HighAvailabilityMemberState newStateCase2 = TO_MASTER.masterIsElected( _context, new InstanceId( 2 ) );
			  assertEquals( PENDING, newStateCase2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testToMasterMasterIsAvailable()
		 public virtual void TestToMasterMasterIsAvailable()
		 {
			  // CASE 1: Got MasterIsAvailable for me - it's ok, that means we completed switching and should to to MASTER
			  HighAvailabilityMemberState newState = TO_MASTER.masterIsAvailable( _context, _myId, SampleUri );
			  assertEquals( MASTER, newState );

			  // CASE 2: Got MasterIsAvailable for someone else - should not happen, should have received a MasterIsElected
			  HighAvailabilityMemberState illegal = TO_MASTER.masterIsAvailable( _context, new InstanceId( 2 ), SampleUri );
			  assertEquals( ILLEGAL, illegal );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testToMasterSlaveIsAvailable()
		 public virtual void TestToMasterSlaveIsAvailable()
		 {
			  // CASE 1: Got SlaveIsAvailable for me - not ok, i'm currently switching to master
			  HighAvailabilityMemberState illegal = TO_MASTER.slaveIsAvailable( _context, _myId, SampleUri );
			  assertEquals( ILLEGAL, illegal );

			  // CASE 2: Got SlaveIsAvailable for someone else - don't really care
			  HighAvailabilityMemberState newState = TO_MASTER.slaveIsAvailable( _context, new InstanceId( 2 ), SampleUri );
			  assertEquals( TO_MASTER, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMasterMasterIsElected()
		 public virtual void TestMasterMasterIsElected()
		 {
			  // CASE 1: Got MasterIsElected for me. Should remain master.
			  HighAvailabilityMemberState newState = MASTER.masterIsElected( _context, _myId );
			  assertEquals( MASTER, newState );

			  // CASE 2: Got MasterIsElected for someone else. Should switch to pending.
			  HighAvailabilityMemberState newStateCase2 = MASTER.masterIsElected( _context, new InstanceId( 2 ) );
			  assertEquals( PENDING, newStateCase2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMasterMasterIsAvailable()
		 public virtual void TestMasterMasterIsAvailable()
		 {
			  // CASE 1: Got MasterIsAvailable for someone else - should fail.
			  HighAvailabilityMemberState illegal = MASTER.masterIsAvailable( _context, new InstanceId( 2 ), SampleUri );
			  assertEquals( ILLEGAL, illegal );

			  // CASE 2: Got MasterIsAvailable for us - it's ok, should pass
			  HighAvailabilityMemberState newState = MASTER.masterIsAvailable( _context, _myId, SampleUri );
			  assertEquals( MASTER, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMasterSlaveIsAvailable()
		 public virtual void TestMasterSlaveIsAvailable()
		 {
			  // CASE 1: Got SlaveIsAvailable for me - should fail.
			  HighAvailabilityMemberState illegal = MASTER.slaveIsAvailable( _context, _myId, SampleUri );
			  assertEquals( ILLEGAL, illegal );

			  // CASE 2: Got SlaveIsAvailable for someone else - who cares? Should succeed.
			  HighAvailabilityMemberState newState = MASTER.slaveIsAvailable( _context, new InstanceId( 2 ), SampleUri );
			  assertEquals( MASTER, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testToSlaveMasterIsElected()
		 public virtual void TestToSlaveMasterIsElected()
		 {
			  // CASE 1: Got MasterIsElected for me - should switch to TO_MASTER
			  HighAvailabilityMemberState newState = TO_SLAVE.masterIsElected( _context, _myId );
			  assertEquals( TO_MASTER, newState );

			  // CASE 2: Got MasterIsElected for someone else - should switch to PENDING
			  HighAvailabilityMemberState newStateCase2 = TO_SLAVE.masterIsElected( _context, new InstanceId( 2 ) );
			  assertEquals( PENDING, newStateCase2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testToSlaveMasterIsAvailable()
		 public virtual void TestToSlaveMasterIsAvailable()
		 {
			  // CASE 1: Got MasterIsAvailable for me - should fail, i am currently trying to become slave
			  HighAvailabilityMemberState illegal = TO_SLAVE.masterIsAvailable( _context, _myId, SampleUri );
			  assertEquals( ILLEGAL, illegal );

			  // CASE 2: Got MasterIsAvailable for someone else who is already the master - should continue switching
			  InstanceId currentMaster = new InstanceId( 2 );
			  when( _context.ElectedMasterId ).thenReturn( currentMaster );
			  HighAvailabilityMemberState newState = TO_SLAVE.masterIsAvailable( _context, currentMaster, SampleUri );
			  assertEquals( TO_SLAVE, newState );

			  // CASE 3: Got MasterIsAvailable for someone else who is not the master - should fail
			  InstanceId instanceId = new InstanceId( 3 );
			  HighAvailabilityMemberState moreIllegal = TO_SLAVE.masterIsAvailable( _context, instanceId, SampleUri );
			  assertEquals( ILLEGAL, moreIllegal );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testToSlaveSlaveIsAvailable()
		 public virtual void TestToSlaveSlaveIsAvailable()
		 {
			  // CASE 1: It is me that that is available as slave - cool, go to SLAVE
			  HighAvailabilityMemberState newState = TO_SLAVE.slaveIsAvailable( _context, _myId, SampleUri );
			  assertEquals( SLAVE, newState );

			  // CASE 2: It is someone else that completed the switch - ignore, continue
			  HighAvailabilityMemberState newStateCase2 = TO_SLAVE.slaveIsAvailable( _context, new InstanceId( 2 ), SampleUri );

			  assertEquals( TO_SLAVE, newStateCase2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSlaveMasterIsElected()
		 public virtual void TestSlaveMasterIsElected()
		 {
			  // CASE 1: It is me that got elected master - should switch to TO_MASTER
			  HighAvailabilityMemberState newState = SLAVE.masterIsElected( _context, _myId );
			  assertEquals( TO_MASTER, newState );

			  InstanceId masterInstanceId = new InstanceId( 2 );
			  when( _context.ElectedMasterId ).thenReturn( masterInstanceId );
			  // CASE 2: It is someone else that got elected master - should switch to PENDING
			  HighAvailabilityMemberState newStateCase2 = SLAVE.masterIsElected( _context, new InstanceId( 3 ) );
			  assertEquals( PENDING, newStateCase2 );

			  // CASE 3: It is the current master that got elected again - ignore
			  HighAvailabilityMemberState newStateCase3 = SLAVE.masterIsElected( _context, masterInstanceId );
			  assertEquals( SLAVE, newStateCase3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSlaveMasterIsAvailable()
		 public virtual void TestSlaveMasterIsAvailable()
		 {
			  // CASE 1: It is me who is available as master - i don't think so
			  HighAvailabilityMemberState illegal = SLAVE.masterIsAvailable( _context, _myId, SampleUri );
			  assertEquals( ILLEGAL, illegal );

			  // CASE 2: It is someone else that is available as master and is not the master now - missed the election, fail
			  InstanceId masterInstanceId = new InstanceId( 2 );
			  when( _context.ElectedMasterId ).thenReturn( masterInstanceId );
			  HighAvailabilityMemberState moreIllegal = SLAVE.masterIsAvailable( _context, new InstanceId( 3 ), SampleUri );
			  assertEquals( ILLEGAL, moreIllegal );

			  // CASE 3: It is the same master as now - it's ok, stay calm and carry on
			  HighAvailabilityMemberState newState = SLAVE.masterIsAvailable( _context, masterInstanceId, SampleUri );
			  assertEquals( SLAVE, newState );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSlaveSlaveIsAvailable()
		 public virtual void TestSlaveSlaveIsAvailable()
		 {
			  // CASE 1 and only - always remain in SLAVE
			  assertEquals( SLAVE, SLAVE.slaveIsAvailable( mock( typeof( HighAvailabilityMemberContext ) ), mock( typeof( InstanceId ) ), SampleUri ) );
		 }
	}

}