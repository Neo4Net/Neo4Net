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
namespace Neo4Net.causalclustering.core.consensus
{

	using Test = org.junit.Test;

	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;
	using MembershipEntry = Neo4Net.causalclustering.core.consensus.membership.MembershipEntry;
	using RaftLogShipper = Neo4Net.causalclustering.core.consensus.shipping.RaftLogShipper;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using RaftCoreState = Neo4Net.causalclustering.core.state.snapshot.RaftCoreState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ReplicatedInteger.valueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLogHelper.readLogEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

	public class CatchUpTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void happyClusterPropagatesUpdates() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HappyClusterPropagatesUpdates()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId leader = member(0);
			  MemberId leader = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId[] allMembers = {leader, member(1), member(2)};
			  MemberId[] allMembers = new MemberId[] { leader, member( 1 ), member( 2 ) };

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftTestFixture fixture = new RaftTestFixture(net, 3, allMembers);
			  RaftTestFixture fixture = new RaftTestFixture( net, 3, allMembers );
			  fixture.Bootstrap( allMembers );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId leaderMember = fixture.members().withId(leader).member();
			  MemberId leaderMember = fixture.Members().withId(leader).member();

			  // when
			  fixture.Members().withId(leader).timerService().invoke(RaftMachine.Timeouts.Election);
			  net.ProcessMessages();
			  fixture.Members().withId(leader).raftInstance().handle(new RaftMessages_NewEntry_Request(leaderMember, valueOf(42)));
			  net.ProcessMessages();

			  // then
			  foreach ( MemberId aMember in allMembers )
			  {
					assertThat( fixture.MessageLog(), IntegerValues(fixture.Members().withId(aMember).raftLog()), hasItems(42) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newMemberWithNoLogShouldCatchUpFromPeers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NewMemberWithNoLogShouldCatchUpFromPeers()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId leaderId = member(0);
			  MemberId leaderId = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId sleepyId = member(2);
			  MemberId sleepyId = member( 2 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId[] awakeMembers = {leaderId, member(1)};
			  MemberId[] awakeMembers = new MemberId[] { leaderId, member( 1 ) };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId[] allMembers = {leaderId, member(1), sleepyId};
			  MemberId[] allMembers = new MemberId[] { leaderId, member( 1 ), sleepyId };

			  RaftTestFixture fixture = new RaftTestFixture( net, 3, allMembers );
			  fixture.Bootstrap( allMembers );
			  fixture.Members().withId(leaderId).raftInstance().installCoreState(new RaftCoreState(new MembershipEntry(0, new HashSet<MemberId>(Arrays.asList(allMembers)))));

			  fixture.Members().withId(leaderId).timerService().invoke(RaftMachine.Timeouts.Election);
			  net.ProcessMessages();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId leader = fixture.members().withId(leaderId).member();
			  MemberId leader = fixture.Members().withId(leaderId).member();

			  net.Disconnect( sleepyId );

			  // when
			  fixture.Members().withId(leaderId).raftInstance().handle(new RaftMessages_NewEntry_Request(leader, valueOf(10)));
			  fixture.Members().withId(leaderId).raftInstance().handle(new RaftMessages_NewEntry_Request(leader, valueOf(20)));
			  fixture.Members().withId(leaderId).raftInstance().handle(new RaftMessages_NewEntry_Request(leader, valueOf(30)));
			  fixture.Members().withId(leaderId).raftInstance().handle(new RaftMessages_NewEntry_Request(leader, valueOf(40)));
			  net.ProcessMessages();

			  // then
			  foreach ( MemberId awakeMember in awakeMembers )
			  {
					assertThat( IntegerValues( fixture.Members().withId(awakeMember).raftLog() ), hasItems(10, 20, 30, 40) );
			  }

			  assertThat( IntegerValues( fixture.Members().withId(sleepyId).raftLog() ), empty() );

			  // when
			  net.Reconnect( sleepyId );
			  fixture.Members().invokeTimeout(RaftLogShipper.Timeouts.RESEND);
			  net.ProcessMessages();

			  // then
			  assertThat( fixture.MessageLog(), IntegerValues(fixture.Members().withId(sleepyId).raftLog()), hasItems(10, 20, 30, 40) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<int> integerValues(org.neo4j.causalclustering.core.consensus.log.ReadableRaftLog log) throws java.io.IOException
		 private IList<int> IntegerValues( ReadableRaftLog log )
		 {
			  IList<int> actual = new List<int>();
			  for ( long logIndex = 0; logIndex <= log.AppendIndex(); logIndex++ )
			  {
					ReplicatedContent content = readLogEntry( log, logIndex ).content();
					if ( content is ReplicatedInteger )
					{
						 ReplicatedInteger integer = ( ReplicatedInteger ) content;
						 actual.Add( integer.Get() );
					}
			  }
			  return actual;
		 }
	}

}