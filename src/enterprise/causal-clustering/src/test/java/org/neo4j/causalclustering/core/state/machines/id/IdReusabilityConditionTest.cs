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
namespace Neo4Net.causalclustering.core.state.machines.id
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using ExposedRaftState = Neo4Net.causalclustering.core.consensus.state.ExposedRaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IdReusabilityConditionTest
	{
		 private RaftMachine _raftMachine = mock( typeof( RaftMachine ) );
		 private ExposedRaftState _state = mock( typeof( ExposedRaftState ) );
		 private MemberId _myself;
		 private CommandIndexTracker _commandIndexTracker = mock( typeof( CommandIndexTracker ) );
		 private IdReusabilityCondition _idReusabilityCondition;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  when( _raftMachine.state() ).thenReturn(_state);
			  _myself = new MemberId( System.Guid.randomUUID() );
			  _idReusabilityCondition = new IdReusabilityCondition( _commandIndexTracker, _raftMachine, _myself );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseAsDefault()
		 public virtual void ShouldReturnFalseAsDefault()
		 {
			  assertFalse( _idReusabilityCondition.AsBoolean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNeverReuseWhenNotLeader()
		 public virtual void ShouldNeverReuseWhenNotLeader()
		 {
			  MemberId someoneElse = new MemberId( System.Guid.randomUUID() );

			  _idReusabilityCondition.onLeaderSwitch( new LeaderInfo( someoneElse, 1 ) );
			  assertFalse( _idReusabilityCondition.AsBoolean );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnTrueWithPendingTransactions()
		 public virtual void ShouldNotReturnTrueWithPendingTransactions()
		 {
			  assertFalse( _idReusabilityCondition.AsBoolean );

			  when( _commandIndexTracker.AppliedCommandIndex ).thenReturn( 2L ); // gap-free
			  when( _state.lastLogIndexBeforeWeBecameLeader() ).thenReturn(5L);

			  _idReusabilityCondition.onLeaderSwitch( new LeaderInfo( _myself, 1 ) );

			  assertFalse( _idReusabilityCondition.AsBoolean );
			  assertFalse( _idReusabilityCondition.AsBoolean );
			  assertFalse( _idReusabilityCondition.AsBoolean );

			  verify( _commandIndexTracker, times( 3 ) ).AppliedCommandIndex;
			  verify( _state ).lastLogIndexBeforeWeBecameLeader();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyReturnTrueWhenOldTransactionsBeenApplied()
		 public virtual void ShouldOnlyReturnTrueWhenOldTransactionsBeenApplied()
		 {
			  assertFalse( _idReusabilityCondition.AsBoolean );

			  when( _commandIndexTracker.AppliedCommandIndex ).thenReturn( 2L, 5L, 6L ); // gap-free
			  when( _state.lastLogIndexBeforeWeBecameLeader() ).thenReturn(5L);

			  _idReusabilityCondition.onLeaderSwitch( new LeaderInfo( _myself, 1 ) );

			  assertFalse( _idReusabilityCondition.AsBoolean );
			  assertFalse( _idReusabilityCondition.AsBoolean );
			  assertTrue( _idReusabilityCondition.AsBoolean );

			  verify( _commandIndexTracker, times( 3 ) ).AppliedCommandIndex;
			  verify( _state ).lastLogIndexBeforeWeBecameLeader();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReuseIfReelection()
		 public virtual void ShouldNotReuseIfReelection()
		 {
			  assertFalse( _idReusabilityCondition.AsBoolean );

			  when( _commandIndexTracker.AppliedCommandIndex ).thenReturn( 2L, 5L, 6L ); // gap-free
			  when( _state.lastLogIndexBeforeWeBecameLeader() ).thenReturn(5L);

			  _idReusabilityCondition.onLeaderSwitch( new LeaderInfo( _myself, 1 ) );

			  assertFalse( _idReusabilityCondition.AsBoolean );
			  assertFalse( _idReusabilityCondition.AsBoolean );
			  assertTrue( _idReusabilityCondition.AsBoolean );

			  MemberId someoneElse = new MemberId( System.Guid.randomUUID() );
			  _idReusabilityCondition.onLeaderSwitch( new LeaderInfo( someoneElse, 1 ) );

			  assertFalse( _idReusabilityCondition.AsBoolean );
		 }
	}

}