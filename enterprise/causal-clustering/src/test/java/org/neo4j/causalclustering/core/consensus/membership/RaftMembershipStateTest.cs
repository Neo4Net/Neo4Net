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
namespace Org.Neo4j.causalclustering.core.consensus.membership
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Test = org.junit.Test;

	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using BoundedNetworkWritableChannel = Org.Neo4j.causalclustering.messaging.BoundedNetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Org.Neo4j.causalclustering.messaging.NetworkReadableClosableChannelNetty4;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.collection.IsCollectionWithSize.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class RaftMembershipStateTest
	{
		 private RaftMembershipState _state = new RaftMembershipState();

		 private ISet<MemberId> _membersA = asSet( member( 0 ), member( 1 ), member( 2 ) );
		 private ISet<MemberId> _membersB = asSet( member( 0 ), member( 1 ), member( 2 ), member( 3 ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveCorrectInitialState()
		 public virtual void ShouldHaveCorrectInitialState()
		 {
			  assertThat( _state.Latest, hasSize( 0 ) );
			  assertFalse( _state.uncommittedMemberChangeInLog() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateLatestOnAppend()
		 public virtual void ShouldUpdateLatestOnAppend()
		 {
			  // when
			  _state.append( 0, _membersA );

			  // then
			  assertEquals( _state.Latest, _membersA );

			  // when
			  _state.append( 1, _membersB );

			  // then
			  assertEquals( _state.Latest, _membersB );
			  assertEquals( 1, _state.Ordinal );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepLatestOnCommit()
		 public virtual void ShouldKeepLatestOnCommit()
		 {
			  // given
			  _state.append( 0, _membersA );
			  _state.append( 1, _membersB );

			  // when
			  _state.commit( 0 );

			  // then
			  assertEquals( _state.Latest, _membersB );
			  assertTrue( _state.uncommittedMemberChangeInLog() );
			  assertEquals( 1, _state.Ordinal );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLowerUncommittedFlagOnCommit()
		 public virtual void ShouldLowerUncommittedFlagOnCommit()
		 {
			  // given
			  _state.append( 0, _membersA );
			  assertTrue( _state.uncommittedMemberChangeInLog() );

			  // when
			  _state.commit( 0 );

			  // then
			  assertFalse( _state.uncommittedMemberChangeInLog() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRevertToCommittedStateOnTruncation()
		 public virtual void ShouldRevertToCommittedStateOnTruncation()
		 {
			  // given
			  _state.append( 0, _membersA );
			  _state.commit( 0 );
			  _state.append( 1, _membersB );
			  assertEquals( _state.Latest, _membersB );

			  // when
			  _state.truncate( 1 );

			  // then
			  assertEquals( _state.Latest, _membersA );
			  assertEquals( 3, _state.Ordinal );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTruncateEarlierThanIndicated()
		 public virtual void ShouldNotTruncateEarlierThanIndicated()
		 {
			  // given
			  _state.append( 0, _membersA );
			  _state.append( 1, _membersB );
			  assertEquals( _state.Latest, _membersB );

			  // when
			  _state.truncate( 2 );

			  // then
			  assertEquals( _state.Latest, _membersB );
			  assertEquals( 1, _state.Ordinal );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalCorrectly()
		 {
			  // given
			  RaftMembershipState.Marshal marshal = new RaftMembershipState.Marshal();
			  _state = new RaftMembershipState( 5, new MembershipEntry( 7, _membersA ), new MembershipEntry( 8, _membersB ) );

			  // when
			  ByteBuf buffer = Unpooled.buffer( 1_000 );
			  marshal.MarshalConflict( _state, new BoundedNetworkWritableChannel( buffer ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftMembershipState recovered = marshal.unmarshal(new org.neo4j.causalclustering.messaging.NetworkReadableClosableChannelNetty4(buffer));
			  RaftMembershipState recovered = marshal.unmarshal( new NetworkReadableClosableChannelNetty4( buffer ) );

			  // then
			  assertEquals( _state, recovered );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRefuseToAppendToTheSameIndexTwice()
		 public virtual void ShouldRefuseToAppendToTheSameIndexTwice()
		 {
			  // given
			  _state.append( 0, _membersA );
			  _state.append( 1, _membersB );

			  // when
			  bool reAppendA = _state.append( 0, _membersA );
			  bool reAppendB = _state.append( 1, _membersB );

			  // then
			  assertFalse( reAppendA );
			  assertFalse( reAppendB );
			  assertEquals( _membersA, _state.committed().members() );
			  assertEquals( _membersB, _state.Latest );
		 }
	}

}